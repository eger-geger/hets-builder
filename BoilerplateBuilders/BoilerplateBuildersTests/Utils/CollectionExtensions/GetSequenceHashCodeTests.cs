using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace BoilerplateBuildersTests.Utils.CollectionExtensions
{
    public class GetSequenceHashCodeTests
    {
        private const int Seed = 1, Step = 19;
        
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static IEnumerable<ITestCaseData> SequenceHashCodeTestCases
        {
            get
            {
                yield return new TestCaseData(
                    Enumerable.Empty<int>()    
                ).Returns(Seed);
                
                yield return new TestCaseData(
                    new []{ 1 }    
                ).Returns(Seed ^ Step);
                
                yield return new TestCaseData(
                    new []{ 3 }    
                ).Returns(Seed ^ 3 * Step);
                
                yield return new TestCaseData(
                    new []{ 1, 5 }    
                ).Returns(Seed ^ Step ^ 5 * Step);
                
                yield return new TestCaseData(
                    new []{ 5, 1 }    
                ).Returns(Seed ^ Step ^ 5 * Step);
                
                yield return new TestCaseData(
                    arg: new []{ null, "Jack" }    
                ).Returns(Seed ^ "Jack".GetHashCode() * Step);
            }
        }

        [TestCaseSource(nameof(SequenceHashCodeTestCases))]
        public int ShouldComputeHashCode(IEnumerable seq)
        {
            return seq.GetSequenceHashCode(Seed, Step);
        }
        
    }
}