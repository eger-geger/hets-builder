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
            var factory = new CollectionFormatterFactory();

            var toString = factory.CreateToStringFunction();
            
            Assert.That(factory.ItemSeparator, Is.Null);
            Assert.That(factory.IndexValueSeparator, Is.Null);
            Assert.That(factory.ItemPrefixAndSuffix.Item1, Is.Null);
            Assert.That(factory.ItemPrefixAndSuffix.Item2, Is.Null);
            Assert.That(factory.ValuePrefixAndSuffix.Item1, Is.Null);
            Assert.That(factory.ValuePrefixAndSuffix.Item2, Is.Null);
            Assert.That(factory.IndexPrefixAndSuffix.Item1, Is.Null);
            Assert.That(factory.IndexPrefixAndSuffix.Item2, Is.Null);
            Assert.That(factory.SequencePrefixAndSuffix.Item1, Is.Null);
            Assert.That(factory.SequencePrefixAndSuffix.Item2, Is.Null);
            Assert.That(factory.Options, Is.EqualTo(None));
            
            Assert.That(toString(new object[]{"John", 12, "Mercedes"}), Is.EqualTo("John12Mercedes"));
        }
    }
}