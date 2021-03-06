using System;
using System.Collections.Generic;
using System.Text;
using BoilerplateBuilders.ToString;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static BoilerplateBuilders.ToString.CollectionFormatOptions;

namespace BoilerplateBuildersTests.ToString
{
    public class CollectionFormatTests
    {
        private static IEnumerable<ITestCaseData> SequenceFormatterTestCases
        {
            get
            {
                yield return new TestCaseData(None).Returns("[<'John'>,<''>]");
                yield return new TestCaseData(IncludeIndex).Returns("[<'0':'John'>,<'1':''>]");
                yield return new TestCaseData(IncludeNullValues).Returns("[<'John'>,<''>,<''>]");
                yield return new TestCaseData(ItemPerLine | IncludeIndex | IncludeNullValues)
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
        public string ShouldApplyFormatOptions(CollectionFormatOptions options)
        {
            var format = new CollectionFormat()
                .AddOptions(options)
                .SetCollectionPrefixAndSuffix("[", "]")
                .SetIndexValuePairPrefixAndSuffix("<", ">")
                .SetIndexPrefixAndSuffix("'", "'")
                .SetValuePrefixAndSuffix("'", "'")
                .SetIndexValueSeparator(":")
                .SetIndexValuePairSeparator(",");

            return format.Compile()(new []{"John", "", null});     
        }

        [Test]
        public void ShouldCreateFormatWithDefaultSetting()
        {
            ValueTuple<string, string> defaultPrefixAndSuffix = (null, null);
            
            var format = new CollectionFormat();

            var toString = format.Compile();
            
            Assert.That(format.IndexValuePairSeparator, Is.Null);
            Assert.That(format.IndexValueSeparator, Is.Null);
            Assert.That(format.IndexValuePairPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.ValuePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.IndexPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.CollectionPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.Options, Is.EqualTo(None));
            
            Assert.That(toString(new object[]{"John", 12, "Mercedes"}), Is.EqualTo("John12Mercedes"));
        }

        [Test]
        public void ShouldCreateDefaultFormat()
        {
            ValueTuple<string, string> defaultPrefixAndSuffix = (null, null);
            
            var format = CollectionFormat.CreateDefault();

            var toString = format.Compile();
            
            Assert.That(toString, Is.Not.Null);
            
            Assert.That(format.IndexValueSeparator, Is.Null);
            Assert.That(format.IndexValuePairSeparator, Is.EqualTo(", "));
            Assert.That(format.ValuePrefixAndSuffix, Is.EqualTo(("'", "'")));
            Assert.That(format.IndexPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.IndexValuePairPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.CollectionPrefixAndSuffix, Is.EqualTo(("[", "]")));
            Assert.That(format.Options, Is.EqualTo(IncludeNullValues));
            
            Assert.That(
                toString(new object[]{"John", "007", null, "Bond"}), 
                Is.EqualTo("['John', '007', '', 'Bond']")
            );
        }

        [Test]
        public void ShouldAlwaysCreateNewInstanceOfDefaultFormat()
        {
            var firstFormat = CollectionFormat.CreateDefault()
                .AddOptions(IncludeIndex)
                .SetIndexValuePairPrefixAndSuffix("<", ">");

            var firstToString = firstFormat.Compile();

            var secondFormat = CollectionFormat.CreateDefault();

            var secondToString = secondFormat.Compile();
            
            Assert.That(secondFormat, Is.Not.SameAs(firstFormat));
            Assert.That(secondToString, Is.Not.SameAs(firstToString));
            
            Assert.That(
                secondToString(new object[]{"John", "007", null, "Bond"}), 
                Is.EqualTo("['John', '007', '', 'Bond']")
            );
        }
    }
}