using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace BoilerplateBuildersTests.Utils.CollectionExtensions
{
    public class SequenceEqualsIgnoreOrderTests
    {
        private static IEnumerable<ITestCaseData> EqualityValueTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new object [] {},
                    new object [] {}
                ).Returns(true);
                
                yield return new TestCaseData(
                    new object [] { 1 },
                    new object [] { 1 }
                ).Returns(true);
                
                yield return new TestCaseData(
                    new object [] { "a", "f" },
                    new object [] { "f", "a" }
                ).Returns(true);
                
                yield return new TestCaseData(
                    new object [] { "a", "f" },
                    new object [] { "f", "a", "b" }
                ).Returns(false);
                
                yield return new TestCaseData(
                    new object [] { "a", "f" },
                    new object [] { }
                ).Returns(false);
            }
        }
        
        [TestCaseSource(nameof(EqualityValueTestCases))]
        public bool ShouldReturnEqualityValue(ICollection<object> a, IEnumerable<object> b)
        {
            return a.SequenceEqualIgnoreOrder(b);
        }
    }
}