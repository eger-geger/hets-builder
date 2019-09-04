using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BoilerplateBuilders;
using BoilerplateBuildersTests.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace BoilerplateBuildersTests
{
    [SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class HashCodeBuilderTests
    {
        private const int HashStep = 15;
        
        private static HashCodeBuilder<Account> CreateBuilder()
        {
            return new HashCodeBuilder<Account>(step: HashStep)
                .AppendPublicFields()
                .AppendPublicProperties()
                .UseElementWiseHashCodeFunctionForCollections();
        }
        
        private static IEnumerable<ITestCaseData> ComputeHashCodeCases
        {
            get
            {
                yield return new TestCaseData(new Account()).Returns(0);
                
                yield return new TestCaseData(new Account(1)).Returns(HashStep);
                
                yield return new TestCaseData(new Account(name: "Jack"))
                    .Returns("Jack".GetHashCode() * HashStep);
                
                yield return new TestCaseData(new Account(35, "John"))
                    .Returns(35 * HashStep ^ "John".GetHashCode() * HashStep);
                
                yield return new TestCaseData(new Account(phones: new []{"12-34-45", "76-99-44"}))
                    .Returns(("12-34-45".GetHashCode() * HashStep ^ "76-99-44".GetHashCode() * HashStep) * HashStep);
            }
        }

        [TestCaseSource(nameof(ComputeHashCodeCases))]
        public int ShouldComputeHashCode(Account account)
        {
            return CreateBuilder().Build().Invoke(account);
        }

        private static IEnumerable<ITestCaseData> SameHashCodeCases
        {
            get
            {
                yield return new TestCaseData(
                    new Account(),
                    new Account()
                );
                
                yield return new TestCaseData(
                    new Account(14),
                    new Account(14)
                );
                
                yield return new TestCaseData(
                    new Account(phones: new []{ "12", "13", "14" }),
                    new Account(phones: new []{ "14", "13", "12" })
                );
            }
        }

        [TestCaseSource(nameof(SameHashCodeCases))]
        public void ComputedHashCodesShouldBeEqual(Account a, Account b)
        {
            var builder = CreateBuilder(); 
            
            Assert.That(
                builder.Build().Invoke(a), 
                Is.EqualTo(builder.Build().Invoke(b))
            );
        }

        [Test]
        public void ShouldUseGivenFunctionToComputeHashCodeForAllMembersOfDerivedType()
        {
            var getHashCode = 
                CreateBuilder()
                .Use<object>(any => (any?.GetHashCode() ?? 0) * 2)
                .Build();
            
            var account = new Account(10, "Jim");
            
            Assert.That(getHashCode(account), Is.EqualTo(
                10 * HashStep * 2 
                ^ "Jim".GetHashCode() * 2 * HashStep    
            ));
        }

        [Test]
        public void ShouldOverrideFunctionComputingHashCodeForGivenMember()
        {
            var getHashCode = 
                CreateBuilder()
                .Append(ac => ac.Phones, phones => phones.Length)
                .Build();
            
            var account = new Account(5, "Jack", new []{"45-67-33", "33-55-88"});
            
            Assert.That(getHashCode(account), Is.EqualTo(
                5 * HashStep 
                ^ "Jack".GetHashCode() * HashStep 
                ^ 2 * HashStep
            ));
        }
    }
}