using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BoilerplateBuilders;
using BoilerplateBuildersTests.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace BoilerplateBuildersTests
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class EqualityBuilderTests
    {
        private static IEnumerable<ITestCaseData> BuildComparerFunctionTestCases
        {
            get
            {
                var phones = new []{"12-44-55", "67-88-03"};
                
                yield return new TestCaseData(
                    new Account(), 
                    new Account()    
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(5), 
                    new Account(5)    
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(5), 
                    new Account(3)    
                ).Returns(false);
                
                yield return new TestCaseData(
                    new Account(id: 5, name: "Jack"), 
                    new Account(id: 5, name: "Jack")    
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(id: 5, name: "Jack"), 
                    new Account(id: 5, name: "Bill")    
                ).Returns(false);
                
                yield return new TestCaseData(
                    new Account(id: 5, name: "Jack", phones: phones), 
                    new Account(id: 5, name: "Jack", phones: phones)    
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(id: 5, name: "Jack", phones: new []{"44-12-55"}), 
                    new Account(id: 5, name: "Jack", phones: new []{"44-12-55"})    
                ).Returns(false);
            }
        }

        [TestCaseSource(nameof(BuildComparerFunctionTestCases))]        
        public bool ShouldBuildEqualityComparerFunction(Account a, Account b)
        {
            return new EqualityBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .Build()
                .Invoke(a, b); 
        }

        private static IEnumerable<ITestCaseData> OverrideFunctionForMemberTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new Account(phones: new []{"12-55-16"}),
                    new Account(phones: new []{"12-55-16"})
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(phones: new []{"12-55-16"}),
                    new Account(phones: new []{"32-56-12"})
                ).Returns(false);
                
                yield return new TestCaseData(
                    new Account(phones: new []{"12-55-16"}),
                    new Account()
                ).Returns(false);
                
                yield return new TestCaseData(
                    new Account(id: 7, name: "Jack", phones: new []{"12-55-16"}),
                    new Account(id: 7, name: "Jack", phones: new []{"12-55-16"})
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(id: 8, name: "Jack", phones: new []{"12-55-16"}),
                    new Account(id: 7, name: "Jack", phones: new []{"12-55-16"})
                ).Returns(false);
                
                yield return new TestCaseData(
                    new Account(phones: new []{"12-55-16", "44-11-12"}),
                    new Account(phones: new []{"12-55-16"})
                ).Returns(true);
            }
        }

        [TestCaseSource(nameof(OverrideFunctionForMemberTestCases))]
        public bool ShouldOverrideComparisonFunctionForMember(Account a, Account b)
        {
            var equals = new EqualityBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .Append(ac => ac.Phones, (ap, bp) => ap.FirstOrDefault() == bp.FirstOrDefault())
                .Build();

            return equals(a, b);
        }

        private static IEnumerable<ITestCaseData> CompareCollectionElementWiseTestCases
        {
            get
            {
                yield return new TestCaseData(    
                    new Account(),
                    new Account()
                ).Returns(true);
                
                yield return new TestCaseData(    
                    new Account(id: 5, name: "Jim", phones: new []{"12-77-15", "14-44-22"}),
                    new Account(id: 5, name: "Jim", phones: new []{"12-77-15", "14-44-22"})
                ).Returns(true);
                
                yield return new TestCaseData(    
                    new Account(id: 5, name: "Jim", phones: new []{"12-77-15", "14-44-22"}),
                    new Account(id: 5, name: "Bob", phones: new []{"12-77-15", "14-44-22"})
                ).Returns(false);
                
                yield return new TestCaseData(    
                    new Account(phones: new []{"12-77-15", "14-44-22"}),
                    new Account(phones: new []{"12-77-15", "14-44-22"})
                ).Returns(true);
                
                yield return new TestCaseData(    
                    new Account(phones: new []{"14-44-22", "12-77-15"}),
                    new Account(phones: new []{"12-77-15", "14-44-22"})
                ).Returns(false);
                
                yield return new TestCaseData(    
                    new Account(phones: new []{"12-77-15", "14-44-22"}),
                    new Account(phones: new []{"12-77-15"})
                ).Returns(false);
                
                yield return new TestCaseData(    
                    new Account(phones: new []{"12-77-15", "14-44-22"}),
                    new Account()
                ).Returns(false);
            }
        }

        [TestCaseSource(nameof(CompareCollectionElementWiseTestCases))]
        public bool ShouldBuildFunctionComparingCollectionsElementWise(Account a, Account b)
        {
            var equals = new EqualityBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .CompareCollectionsElementWise()
                .Build();

            return equals(a, b);
        }

        private static IEnumerable<ITestCaseData> CompareCollectionsIgnoreOrder
        {
            get
            {
                yield return new TestCaseData(
                    new Account(),
                    new Account()
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(id: 5, name: "Jack", phones: new []{"67-19-33", "42-18-55"}),
                    new Account(id: 5, name: "Jack", phones: new []{"42-18-55", "67-19-33"})
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(id: 5, name: "Jack", phones: new []{"67-19-33", "42-18-55"}),
                    new Account(id: 3, name: "Jack", phones: new []{"42-18-55", "67-19-33"})
                ).Returns(false);
                
                yield return new TestCaseData(
                    new Account(phones: new []{"67-19-33", "42-18-55"}),
                    new Account(phones: new []{"42-18-55", "67-19-33"})
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(phones: new []{"42-18-55", "67-19-33"}),
                    new Account(phones: new []{"42-18-55", "67-19-33"})
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(phones: new []{"42-18-55", "67-19-33"}),
                    new Account(phones: new []{"42-18-55"})
                ).Returns(false);
                
                yield return new TestCaseData(
                    new Account(phones: new []{"42-18-55", "67-19-33"}),
                    new Account()
                ).Returns(false);
            }
        }

        [TestCaseSource(nameof(CompareCollectionsIgnoreOrder))]
        public bool ShouldBuildFunctionComparingCollectionsElementWiseIgnoringOrder(Account a, Account b)
        {
            var equals = new EqualityBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .CompareCollectionsIgnoringOrder()
                .Build();

            return equals(a, b);
        }

        private static IEnumerable<ITestCaseData> OverrideFunctionForTypeTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new Account(),
                    new Account()
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(1),
                    new Account(5)
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(id: 1, name: "Jack", phones: new []{"12-44-55"}),
                    new Account(id: 5, name: "Jack", phones: new []{"55-18-02"})
                ).Returns(true);
                
                yield return new TestCaseData(
                    new Account(1),
                    new Account(13)
                ).Returns(false);
                
                yield return new TestCaseData(
                    new Account(id: 1, name: "Jack", phones: new []{"12-44-55"}),
                    new Account(id: 5, name: "Jack", phones: new []{"55-18-02", "33-12-55"})
                ).Returns(false);
            }
        }

        [TestCaseSource(nameof(OverrideFunctionForTypeTestCases))]
        public bool ShouldUseProvidedComparisonFunctionForHierarchy(Account a, Account b)
        {
            var equals = new EqualityBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .Use<int>((ai, bi) => Math.Abs(ai - bi) < 10)
                .Use<string[]>((ar, br) => ar.Length == br.Length)
                .Build();

            return equals(a, b);
        }
    }
}