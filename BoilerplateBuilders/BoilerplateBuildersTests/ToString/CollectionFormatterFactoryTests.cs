using System;
using System.Collections.Generic;
using System.Text;
using BoilerplateBuilders.ToString;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static BoilerplateBuilders.ToString.CollectionFormatOptions;

namespace BoilerplateBuildersTests.ToString
{
    public class CollectionFormatterFactoryTests
    {
        private static IEnumerable<ITestCaseData> SequenceFormatterTestCases
        {
            get
            {
                yield return new TestCaseData(None).Returns("[<'John'>,<''>]");
                yield return new TestCaseData(IncludeItemIndex).Returns("[<'0':'John'>,<'1':''>]");
                yield return new TestCaseData(IncludeNullValues).Returns("[<'John'>,<''>,<''>]");
                yield return new TestCaseData(IncludeLineBreak | IncludeItemIndex | IncludeNullValues)
                    .Returns(
                        new StringBuilder()
                            .AppendLine("[")
                            .AppendLine("<'0':'John'>,")
                            .AppendLine("<'1':''>,")
                            .AppendLine("<'2':''>")
                            .Append("]")
                            .ToString()
                    );
            }
        }
        
        [TestCaseSource(nameof(SequenceFormatterTestCases))]
        public string ShouldFormatSequenceAccordingToDensity(CollectionFormatOptions options)
        {
            var factory = new CollectionFormatterFactory()
                .SetOptions(options)
                .SetSequencePrefixAndSuffix("[", "]")
                .SetItemPrefixAndSuffix("<", ">")
                .SetIndexPrefixAndSuffix("'", "'")
                .SetValuePrefixAndSuffix("'", "'")
                .SetIndexValueSeparator(":")
                .SetItemSeparator(",");

            return factory.CreateToStringFunction()(new []{"John", "", null});     
        }

        [Test]
        public void ShouldCreateFactoryWithDefaultSetting()
        {
            ValueTuple<string, string> defaultPrefixAndSuffix = (null, null);
            
            var factory = new CollectionFormatterFactory();

            var toString = factory.CreateToStringFunction();
            
            Assert.That(factory.ItemSeparator, Is.Null);
            Assert.That(factory.IndexValueSeparator, Is.Null);
            Assert.That(factory.ItemPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.ValuePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.IndexPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.SequencePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.Options, Is.EqualTo(None));
            
            Assert.That(toString(new object[]{"John", 12, "Mercedes"}), Is.EqualTo("John12Mercedes"));
        }
    }
}