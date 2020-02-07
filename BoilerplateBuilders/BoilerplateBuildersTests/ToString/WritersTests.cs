using System;
using System.Collections.Generic;
using BoilerplateBuilders.ToString;
using BoilerplateBuilders.ToString.Primitives;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static BoilerplateBuilders.ToString.Primitives.Writers;

namespace BoilerplateBuildersTests.ToString
{
    public class WritersTests
    {
        [Test]
        public void EmptyWriterShouldWriteEmptyString()
        {
            Assert.That(Writers.ToString(Empty), Is.Empty);
        }

        private static IEnumerable<ITestCaseData> EmptyWritersTestCases
        {
            get
            {
                yield return new TestCaseData(Empty);
                yield return new TestCaseData(Add(Empty, Empty));
                yield return new TestCaseData(Empty + null);
            }
        }
        
        [TestCaseSource(nameof(EmptyWritersTestCases))]
        public void EmptyWriterShouldBeConsideredEmpty(Writer empty)
        {
            Assert.That(IsEmpty(empty), Is.True);
        }
        
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("Jack")]
        public void ShouldWriteGivenString(string s)
        {
            Assert.That(Writers.ToString(Write(s)), Is.EqualTo(s));            
        }

        [Test]
        public void ShouldWriteLineBreak()
        {
            Assert.That(Writers.ToString(NewLine), Is.EqualTo(Environment.NewLine));
        }

        [Test]
        public void ToStringShouldReturnEmptyStringWhenNoWriterWasGiven()
        {
            Assert.That(Writers.ToString(null), Is.Empty);
        }
    }
}