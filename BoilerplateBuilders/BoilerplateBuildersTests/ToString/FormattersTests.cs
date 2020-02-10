using System;
using BoilerplateBuilders.ToString;
using BoilerplateBuilders.ToString.Primitives;
using NUnit.Framework;
using static BoilerplateBuilders.ToString.Primitives.Writers;
using static BoilerplateBuilders.ToString.Primitives.Formatters;

namespace BoilerplateBuildersTests.ToString
{
    public class FormattersTests
    {
        private static string ToHexString(int i) => i.ToString("X");

        [Test]
        public void EmptyFormatterShouldIgnoreInput()
        {
            var toString = Formatters.MakeToString(Empty<int>());

            Assert.That(toString(50), Is.Empty);
        }

        [Test]
        public void WriterLiftedToFormatterShouldIgnoreInput()
        {
            var fmt = Lift<int>(Write("100"));

            Assert.That(Writers.ToString(fmt(50)), Is.EqualTo("100"));
        }

        [Test]
        public void ShouldApplyLiftedFunctionToInput()
        {
            var fmt = Lift<int>(ToHexString);

            Assert.That(Writers.ToString(fmt(15)), Is.EqualTo("F"));
        }

        [Test]
        public void ShouldThrowNullArgumentExceptionWhenLiftedFunctionIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => Lift<int>(null));
        }

        [Test]
        public void ToStringShouldProduceEmptyStringWhenPassedNull()
        {
            var toString = MakeToString<object>(null);

            Assert.That(toString(15), Is.Empty);
        }

        [Test]
        public void WrapShouldApplyFunctionToInputAndCallWrappedFormatter()
        {
            var strFmt = Wrap<string, int>(Lift<int>(ToHexString), s => s.Length);

            Assert.That(Writers.ToString(strFmt("John Malkovich")), Is.EqualTo("E"));
        }

        [Test]
        public void CollectShouldApplyFormattingFunctionToEverySequenceItem()
        {
            var seqFmt = Collect(Lift<int>(ToHexString), Write("|"));

            Assert.That(Writers.ToString(seqFmt(new[] {15, 20, 47})), Is.EqualTo("F|14|2F"));
        }

        [Test]
        public void CollectShouldUseWhiteSpaceToGlueFormattedItemsWhenNoGlueWasGiven()
        {
            var seqFmt = Collect(Lift<int>(ToHexString));

            Assert.That(Writers.ToString(seqFmt(new[] {15, 20, 47})), Is.EqualTo("F 14 2F"));
        }

        [Test]
        public void EncloseAddsPrefixAndSuffixToValueProducedByWrappedFormatter()
        {
            var fmt = Enclose(Lift<int>(ToHexString), ("[", "]"));

            Assert.That(Writers.ToString(fmt(15)), Is.EqualTo("[F]"));
        }

        [TestCase(1, ExpectedResult = "1")]
        [TestCase(4, ExpectedResult = "100")]
        [TestCase(8, ExpectedResult = "1000")]
        [TestCase(11, ExpectedResult = "B")]
        [TestCase(16, ExpectedResult = "10")]
        public string WhenShouldChooseFormatterBasedOnCondition(int input)
        {
            var posFmt = Lift<int>(ToHexString);
            var negFmt = Lift<int>(i => Convert.ToString(i, 2));
            var fmt = When(i => i > 8, posFmt, negFmt);

            return Writers.ToString(fmt(input));
        }

        [Test]
        public void WhenShouldUseEmptyFormatterByDefault()
        {
            var fmt = When(i => i > 8, Lift<int>(ToHexString));

            Assert.That(Writers.ToString(fmt(1)), Is.Empty);
        }

        [TestCase(null, ExpectedResult = "nothing")]
        [TestCase("John", ExpectedResult = "something")]
        public string UnlessNullShouldChooseFormatterBasedOnObjectReference(string input)
        {
            var fmt = UnlessNull(
                Lift<string>(s => "something"),
                Lift<string>(_ => "nothing")
            );

            return Writers.ToString(fmt(input));
        }

        [Test]
        public void UnlessNullShouldEmitNullString()
        {
            var fmt = UnlessNull(Lift<string>(s => s.ToString()));
            
            Assert.That(Writers.ToString(fmt(null)), Is.EqualTo("null"));
        }

        [Test]
        public void AddShouldCombineTwoFunctionsByApplyingThemInOrder()
        {
            var binFmt = Lift<int>(i => Convert.ToString(i, 2));
            var hexFmt = Lift<int>(i => i.ToString("X"));
            var sum = Add(binFmt, hexFmt);
            
            Assert.That(Writers.ToString(sum(15)), Is.EqualTo("1111F"));
        }

        [Test]
        public void AdditionShouldAllowNulls()
        {
            var hexFmt = Lift<int>(ToHexString);
            
            Assert.That(Writers.ToString(Add(hexFmt, null)(15)), Is.EqualTo("F"));
            Assert.That(Writers.ToString(Add(null, hexFmt)(15)), Is.EqualTo("F"));
        }

        [Test]
        public void AddingTwoNullFormattersShouldResultInEmptyFormatter()
        {
            var sum = Add<int>(null, null);
            
            Assert.That(sum, Is.Not.Null);
            Assert.That(Writers.ToString(sum(1)), Is.Empty);
        }

        [Test]
        public void SumShouldAddMultipleFormatter()
        {
            var binFmt = Lift<int>(i => Convert.ToString(i, 2));
            var hexFmt = Lift<int>(i => i.ToString("X"));
            var decFmt = Lift<int>(i => i.ToString());

            var sum = Sum(binFmt, decFmt, hexFmt);
            
            Assert.That(Writers.ToString(sum(15)), Is.EqualTo("111115F"));

        }
    }
}