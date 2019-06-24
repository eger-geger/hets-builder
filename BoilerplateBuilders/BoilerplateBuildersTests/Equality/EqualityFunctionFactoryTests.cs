using System;
using System.Collections.Generic;
using System.Linq;
using BoilerplateBuilders.Reflection.Equality;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace BoilerplateBuildersTests.Equality
{
    public class EqualityFunctionFactoryTests
    {
        private static IEnumerable<ITestCaseData> CompareSequencesCases
        {
            get
            {
                yield return new TestCaseData(
                    typeof(IEnumerable<int>),
                    Enumerable.Empty<int>(),
                    Enumerable.Empty<int>()
                ).Returns(true);
                
                yield return new TestCaseData(
                    typeof(IEnumerable<int>),
                    Enumerable.Empty<int>(),
                    new []{1}
                ).Returns(false);
                
                yield return new TestCaseData(
                    typeof(IEnumerable<int>),
                    new List<int>{43},
                    new []{43}
                ).Returns(true);
                
                yield return new TestCaseData(
                    typeof(IDictionary<int, string>),
                    new Dictionary<int, string>
                    {
                        [1] = "Ivan",
                        [2] = "Thijs"
                    },
                    new Dictionary<int, string>
                    {
                        [1] = "Ivan",
                        [2] = "Thijs"
                    }
                ).Returns(true);
                
                yield return new TestCaseData(
                    typeof(ICollection<int>),
                    new []{1, 2, 4},
                    new []{1, 2, 4}
                ).Returns(true);
                
                yield return new TestCaseData(
                    typeof(ICollection<int>),
                    new []{1, 2, 4},
                    new []{4, 2, 1}
                ).Returns(false);
                
                yield return new TestCaseData(
                    typeof(IList<string>),
                    new List<string>{"Lviv"},
                    new []{"Lviv"}
                ).Returns(true);
            }
        }
        
        [TestCaseSource(nameof(CompareSequencesCases))]
        public bool ShouldCompareSequences(Type seqType, object a, object b)
        {
            return EqualityFunctionFactory.CreateOrderedSequenceComparer(seqType)(a, b);
        }

        private static IEnumerable<ITestCaseData> CompareSetsCases
        {
            get
            {
                yield return new TestCaseData(
                      typeof(ISet<int>),
                      new HashSet<int>(),
                      new HashSet<int>()
                ).Returns(true);
                
                yield return new TestCaseData(
                    typeof(ISet<int>),
                    new HashSet<int>{1, 2, 4},
                    new HashSet<int>{4, 2, 1}
                ).Returns(true);
                
                yield return new TestCaseData(
                    typeof(ISet<int>),
                    new HashSet<int>{1, 2, 4},
                    new HashSet<int>{4, 2}
                ).Returns(false);
            }
        }
        
        [TestCaseSource(nameof(CompareSetsCases))]
        public bool ShouldCompareSets(Type setType, object a, object b)
        {
            return EqualityFunctionFactory.CreateSetComparer(setType)(a, b);
        }
    }
}