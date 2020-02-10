using System;
using System.Collections.Generic;
using System.Text;
using BoilerplateBuilders.ToString;
using NUnit.Framework;

namespace BoilerplateBuildersTests.ToString
{
    public class DictionaryFormatterFactoryTests
    {
        private static readonly IDictionary<int, string> Dictionary = new Dictionary<int, string>
        {
            {0, null},
            {1, "Leroy Jenkins"},
            {7, "James Bond"}
        };

        [Test]
        public void ShouldInitializeEmptyFactory()
        {
            ValueTuple<string, string> emptyPrefixAndSuffix = (null, null);

            var factory = new DictionaryFormatterFactory();

            var toString = factory.CreateToString<int, string>();

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
        public void ShouldInitializeDefaultFactory()
        {
            ValueTuple<string, string> emptyPrefixAndSuffix = (null, null);

            var factory = DictionaryFormatterFactory.CreateDefault();
            
            var toString = factory.CreateToString<int, string>();
            
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
            var factory = DictionaryFormatterFactory
                .CreateDefault()
                .SetKeyValueSeparator("::");

            var toString = factory.CreateToString<int, string>();
            
            Assert.That(factory.KeyValueSeparator, Is.EqualTo("::"));
            Assert.That(toString(Dictionary), Is.EqualTo("{0::'null', 1::'Leroy Jenkins', 7::'James Bond'}"));
        }

        [Test]
        public void ShouldSetKeyValuePairSeparatorWithinOutput()
        {
            var factory = DictionaryFormatterFactory
                .CreateDefault()
                .SetKeyValuePairSeparator("|");

            var toString = factory.CreateToString<int, string>();
            
            Assert.That(factory.KeyValuePairSeparator, Is.EqualTo("|"));
            Assert.That(toString(Dictionary), Is.EqualTo("{0:'null'|1:'Leroy Jenkins'|7:'James Bond'}"));
        }

        [Test]
        public void ShouldSetKeyPrefixAndSuffixWithinOutput()
        {
            var factory = DictionaryFormatterFactory
                .CreateDefault()
                .SetKeyPrefixAndSuffix("[", "]");

            var toString = factory.CreateToString<int, string>();
            
            Assert.That(factory.KeyPrefixAndSuffix, Is.EqualTo(("[", "]")));
            Assert.That(toString(Dictionary), Is.EqualTo("{[0]:'null', [1]:'Leroy Jenkins', [7]:'James Bond'}"));
        }

        [Test]
        public void ShouldSetValuePrefixAndSuffixWithinOutput()
        {
            var factory = DictionaryFormatterFactory
                .CreateDefault()
                .SetValuePrefixAndSuffix("(", ")");

            var toString = factory.CreateToString<int, string>();
            
            Assert.That(factory.ValuePrefixAndSuffix, Is.EqualTo(("(", ")")));
            Assert.That(toString(Dictionary), Is.EqualTo("{0:(null), 1:(Leroy Jenkins), 7:(James Bond)}"));
        }

        [Test]
        public void ShouldSetKeyValuePairPrefixAndSuffixWithinOutput()
        {
            var factory = DictionaryFormatterFactory
                .CreateDefault()
                .SetKeyValuePrefixAndSuffix("<", ">");

            var toString = factory.CreateToString<int, string>();
            
            Assert.That(factory.KeyValuePairPrefixAndSuffix, Is.EqualTo(("<", ">")));
            Assert.That(toString(Dictionary), Is.EqualTo("{<0:'null'>, <1:'Leroy Jenkins'>, <7:'James Bond'>}"));
        }

        [Test]
        public void ShouldIncludeDictionaryPrefixAndSuffixIntoOutput()
        {
            var factory = DictionaryFormatterFactory
                .CreateDefault()
                .SetDictionaryPrefixAndSuffix("<", ">");
            
            var toString = factory.CreateToString<int, string>();
            
            Assert.That(factory.DictionaryPrefixAndSuffix, Is.EqualTo(("<", ">")));
            Assert.That(toString(Dictionary), Is.EqualTo("<0:'null', 1:'Leroy Jenkins', 7:'James Bond'>"));
        }

        [Test]
        public void ShouldProduceNewLineSeparatedOutputWithLineBreak()
        {
            var factory = DictionaryFormatterFactory
                .CreateDefault()
                .SetKeyValuePairSeparator(null)
                .AddOptions(DictionaryFormatOptions.IncludeLineBreak);
            
            var toString = factory.CreateToString<int, string>();
            
            Assert.That(factory.Options, Is.EqualTo(DictionaryFormatOptions.IncludeLineBreak));
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
            var factory = DictionaryFormatterFactory
                .CreateDefault()
                .AddOptions(DictionaryFormatOptions.IncludeLineBreak)
                .RemoveOptions(DictionaryFormatOptions.IncludeLineBreak);
            
            var toString = factory.CreateToString<int, string>();
            
            Assert.That(factory.Options, Is.EqualTo(DictionaryFormatOptions.None));
            Assert.That(toString(Dictionary), Is.EqualTo("{0:'null', 1:'Leroy Jenkins', 7:'James Bond'}"));
        }
    }
}