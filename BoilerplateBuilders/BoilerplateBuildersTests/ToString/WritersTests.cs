using System;
using BoilerplateBuilders.ToString;
using NUnit.Framework;
using static BoilerplateBuilders.ToString.Writers;

namespace BoilerplateBuildersTests.ToString
{
    public class WritersTests
    {
        [Test]
        public void EmptyWriterShouldWriteEmptyString()
        {
            Assert.That(Writers.ToString(Empty), Is.Empty);
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