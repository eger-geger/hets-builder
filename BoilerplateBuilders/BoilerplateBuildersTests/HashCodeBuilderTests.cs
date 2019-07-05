using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BoilerplateBuilders;
using BoilerplateBuildersTests.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace BoilerplateBuildersTests
{
    [SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
    public class HashCodeBuilderTests
    {
        private const int HashStep = 15;
        
        private readonly HashCodeBuilder<Account> _builder = 
            new HashCodeBuilder<Account>(step: HashStep)
                .AppendPublicFields()
                .AppendPublicProperties();

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
            return _builder.Build().Invoke(account);
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
            Assert.That(
                _builder.Build().Invoke(a), 
                Is.EqualTo(_builder.Build().Invoke(b))
            );
        }
    }
}