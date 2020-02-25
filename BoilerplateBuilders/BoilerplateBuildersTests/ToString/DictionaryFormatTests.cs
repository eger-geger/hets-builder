using System;
using System.Collections.Generic;
using System.Text;
using BoilerplateBuilders.ToString;
using NUnit.Framework;

namespace BoilerplateBuildersTests.ToString
{
    public class DictionaryFormatTests
    {
        private static readonly IDictionary<int, string> Dictionary = new Dictionary<int, string>
        {
            {0, null},
            {1, "Leroy Jenkins"},
            {7, "James Bond"}
        };

        [Test]
        public void ShouldInitializeEmptyFormat()
        {
            ValueTuple<string, string> emptyPrefixAndSuffix = (null, null);

            var factory = new DictionaryFormat();

            var toString = factory.Compile<int, string>();

            Assert.That(factory.KeyValueSeparator, Is.Null);
            Assert.That(factory.KeyValuePairSeparator, Is.Null);
            Assert.That(factory.KeyPrefixAndSuffix, Is.EqualTo(emptyPrefixAndSuffix));
            Assert.That(factory.ValuePrefixAndSuffix, Is.EqualTo(emptyPrefixAndSuffix));
            Assert.That(factory.KeyValuePairPrefixAndSuffix, Is.EqualTo(emptyPrefixAndSuffix));
            Assert.That(factory.DictionaryPrefixAndSuffix, Is.EqualTo(emptyPrefixAndSuffix));
            Assert.That(factory.Options, Is.EqualTo(DictionaryFormatOptions.None));

            Assert.That(toString(Dictionary), Is.EqualTo("0null1Leroy Jenkins7James Bond"));
        }

        [Test]
        public void ShouldCreateDefaultFormat()
        {
            ValueTuple<string, string> emptyPrefixAndSuffix = (null, null);

            var factory = DictionaryFormat.CreateDefault();
            
            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.KeyValueSeparator, Is.EqualTo(":"));
            Assert.That(factory.KeyValuePairSeparator, Is.EqualTo(", "));
            Assert.That(factory.KeyPrefixAndSuffix, Is.EqualTo(emptyPrefixAndSuffix));
            Assert.That(factory.ValuePrefixAndSuffix, Is.EqualTo(("'", "'")));
            Assert.That(factory.KeyValuePairPrefixAndSuffix, Is.EqualTo(emptyPrefixAndSuffix));
            Assert.That(factory.DictionaryPrefixAndSuffix, Is.EqualTo(("{", "}")));
            Assert.That(factory.Options, Is.EqualTo(DictionaryFormatOptions.None));
            Assert.That(toString(Dictionary), Is.EqualTo("{0:'null', 1:'Leroy Jenkins', 7:'James Bond'}"));
        }
        
        [Test]
        public void ShouldSetKeyValueSeparatorWithinOutput()
        {
            var factory = DictionaryFormat
                .CreateDefault()
                .SetKeyValueSeparator("::");

            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.KeyValueSeparator, Is.EqualTo("::"));
            Assert.That(toString(Dictionary), Is.EqualTo("{0::'null', 1::'Leroy Jenkins', 7::'James Bond'}"));
        }

        [Test]
        public void ShouldSetKeyValuePairSeparatorWithinOutput()
        {
            var factory = DictionaryFormat
                .CreateDefault()
                .SetKeyValuePairSeparator("|");

            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.KeyValuePairSeparator, Is.EqualTo("|"));
            Assert.That(toString(Dictionary), Is.EqualTo("{0:'null'|1:'Leroy Jenkins'|7:'James Bond'}"));
        }

        [Test]
        public void ShouldSetKeyPrefixAndSuffixWithinOutput()
        {
            var factory = DictionaryFormat
                .CreateDefault()
                .SetKeyPrefixAndSuffix("[", "]");

            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.KeyPrefixAndSuffix, Is.EqualTo(("[", "]")));
            Assert.That(toString(Dictionary), Is.EqualTo("{[0]:'null', [1]:'Leroy Jenkins', [7]:'James Bond'}"));
        }

        [Test]
        public void ShouldSetValuePrefixAndSuffixWithinOutput()
        {
            var factory = DictionaryFormat
                .CreateDefault()
                .SetValuePrefixAndSuffix("(", ")");

            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.ValuePrefixAndSuffix, Is.EqualTo(("(", ")")));
            Assert.That(toString(Dictionary), Is.EqualTo("{0:(null), 1:(Leroy Jenkins), 7:(James Bond)}"));
        }

        [Test]
        public void ShouldSetKeyValuePairPrefixAndSuffixWithinOutput()
        {
            var factory = DictionaryFormat
                .CreateDefault()
                .SetKeyValuePrefixAndSuffix("<", ">");

            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.KeyValuePairPrefixAndSuffix, Is.EqualTo(("<", ">")));
            Assert.That(toString(Dictionary), Is.EqualTo("{<0:'null'>, <1:'Leroy Jenkins'>, <7:'James Bond'>}"));
        }

        [Test]
        public void ShouldIncludeDictionaryPrefixAndSuffixIntoOutput()
        {
            var factory = DictionaryFormat
                .CreateDefault()
                .SetDictionaryPrefixAndSuffix("<", ">");
            
            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.DictionaryPrefixAndSuffix, Is.EqualTo(("<", ">")));
            Assert.That(toString(Dictionary), Is.EqualTo("<0:'null', 1:'Leroy Jenkins', 7:'James Bond'>"));
        }

        [Test]
        public void ShouldProduceNewLineSeparatedOutputWithLineBreak()
        {
            var factory = DictionaryFormat
                .CreateDefault()
                .SetKeyValuePairSeparator(null)
                .AddOptions(DictionaryFormatOptions.ItemPerLine);
            
            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.Options, Is.EqualTo(DictionaryFormatOptions.ItemPerLine));
            Assert.That(toString(Dictionary), Is.EqualTo(
                new StringBuilder()
                    .AppendLine("{")
                    .AppendLine("0:'null'")
                    .AppendLine("1:'Leroy Jenkins'")
                    .AppendLine("7:'James Bond'")
                    .Append("}")
                    .ToString()
            ));
        }

        [Test]
        public void ShouldRemovePreviouslyAddedOption()
        {
            var factory = DictionaryFormat
                .CreateDefault()
                .AddOptions(DictionaryFormatOptions.ItemPerLine)
                .RemoveOptions(DictionaryFormatOptions.ItemPerLine);
            
            var toString = factory.Compile<int, string>();
            
            Assert.That(factory.Options, Is.EqualTo(DictionaryFormatOptions.None));
            Assert.That(toString(Dictionary), Is.EqualTo("{0:'null', 1:'Leroy Jenkins', 7:'James Bond'}"));
        }
        
        [Test]
        public void ShouldAlwaysCreateNewInstanceOfDefaultFormat()
        {
            var firstFormat = DictionaryFormat.CreateDefault()
                .AddOptions(DictionaryFormatOptions.ItemPerLine)
                .SetKeyPrefixAndSuffix("[", "]")
                .SetKeyValuePrefixAndSuffix("<", ">");

            var firstToString = firstFormat.Compile<int, string>();
            
            var secondFormat = DictionaryFormat.CreateDefault();
            var secondToString = secondFormat.Compile<int, string>();

            Assert.That(secondFormat, Is.Not.SameAs(firstFormat));
            Assert.That(secondToString, Is.Not.SameAs(firstToString));
            
            Assert.That(
                secondToString(Dictionary),
                Is.EqualTo("{0:'null', 1:'Leroy Jenkins', 7:'James Bond'}")
            );
        }
    }
}