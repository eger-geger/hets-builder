using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.ToString;
using BoilerplateBuildersTests.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static BoilerplateBuilders.ToString.FormatDensity;

namespace BoilerplateBuildersTests.ToString
{
    public class DefaultFormatterFactoryTests
    {
        private static IEnumerable<MemberContext<Func<object, string>>> AccountFormattingMembers
        {
            get
            {
                yield return new MemberContext<Func<object, string>>(
                    SelectedMember.Create<Account, int>(ac => ac.Id),
                    id => id.ToString(),
                    ContextSource.Implicit
                );

                yield return new MemberContext<Func<object, string>>(
                    SelectedMember.Create<Account, string>(ac => ac.Name),
                    name => name?.ToString(),
                    ContextSource.Implicit
                );

                yield return new MemberContext<Func<object, string>>(
                    SelectedMember.Create<Account, string[]>(ac => ac.Phones),
                    phones => string.Join(", ", (string[]) phones),
                    ContextSource.Implicit
                );
            }
        }
        
        [Test]
        public void EmptyBuilder()
        {
            var factory = new DefaultFormatterFactory();
            var defaultPrefixAndSuffix = ValueTuple.Create<string, string>(null, null);

            Assert.That(factory.Density, Is.EqualTo((FormatDensity) 0));
            Assert.That(factory.MemberPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.MemberSeparator, Is.EqualTo(null));
            Assert.That(factory.BodyPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.MemberNamePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.MemberValuePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
        }

        [Test]
        public void ShouldSetMultipleDensityFlags()
        {
            var factory = new DefaultFormatterFactory()
                .SetDensityFlag(IncludeClassName)
                .SetDensityFlag(IncludeMemberName)
                .SetDensityFlag(MemberOnNewLine);

            Assert.That(factory.Density, Is.EqualTo(
                IncludeClassName
                | IncludeMemberName
                | MemberOnNewLine
            ));
        }

        [Test]
        public void ShouldUnsetDensityFlags()
        {
            var factory = new DefaultFormatterFactory()
                .SetDensityFlag(IncludeClassName)
                .SetDensityFlag(IncludeMemberName)
                .SetDensityFlag(MemberOnNewLine)
                .UnsetDensityFlag(
                    IncludeClassName
                    | IncludeMemberName
                    | MemberOnNewLine
                    | IncludeNullValues
                );

            Assert.That(factory.Density, Is.EqualTo((FormatDensity) 0));
        }

        [Test]
        public void ShouldUpdateMemberPrefixAndSuffix()
        {
            var factory = new DefaultFormatterFactory()
                .EncloseMemberWith("<", ">");

            Assert.That(factory.MemberPrefixAndSuffix, Is.EqualTo(("<", ">")));
        }

        [Test]
        public void ShouldUpdateMemberValuePrefixAndSuffix()
        {
            var factory = new DefaultFormatterFactory()
                .EncloseMemberValueWith(null, "]");

            Assert.That(
                factory.MemberValuePrefixAndSuffix,
                Is.EqualTo(ValueTuple.Create<string, string>(null, "]"))
            );
        }

        [Test]
        public void ShouldUpdateMemberNamePrefixAndSuffix()
        {
            var factory = new DefaultFormatterFactory()
                .EncloseMemberNameWith("'", "'");

            Assert.That(factory.MemberNamePrefixAndSuffix, Is.EqualTo(ValueTuple.Create("'", "'")));
        }

        [Test]
        public void ShouldUpdateMemberSeparator()
        {
            var factory = new DefaultFormatterFactory()
                .JoinMembersWith(",");

            Assert.That(factory.MemberSeparator, Is.EqualTo(","));
        }

        [Test]
        public void ShouldUpdateBodyPrefixAndSuffix()
        {
            var factory = new DefaultFormatterFactory()
                .EncloseBodyWith("{", "}");

            Assert.That(factory.BodyPrefixAndSuffix, Is.EqualTo(ValueTuple.Create("{", "}")));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static IEnumerable<ITestCaseData> ObjectFormatterTestCases
        {
            get
            {
                yield return new TestCaseData(IncludeClassName | IncludeMemberName | MemberOnNewLine)
                    .Returns(
                        new StringBuilder()
                            .Append("Account")
                            .Append("<")
                            .AppendLine()
                            .AppendLine("'Id':\"19\",")
                            .AppendLine("'Name':\"John\",")
                            .Append("'Phones':\"12-33-19, 66-18-23\"")
                            .Append(">")
                            .ToString()
                    );
                
                yield return new TestCaseData(IncludeClassName | IncludeMemberName)
                    .Returns(
                        new StringBuilder()
                            .Append("Account")
                            .Append("<")
                            .Append("'Id':\"19\",")
                            .Append("'Name':\"John\",")
                            .Append("'Phones':\"12-33-19, 66-18-23\"")
                            .Append(">")
                            .ToString()
                    );
                
                yield return new TestCaseData(IncludeMemberName)
                    .Returns(
                        new StringBuilder()
                            .Append("<")
                            .Append("'Id':\"19\",")
                            .Append("'Name':\"John\",")
                            .Append("'Phones':\"12-33-19, 66-18-23\"")
                            .Append(">")
                            .ToString()
                    );
                
                yield return new TestCaseData(Dense)
                    .Returns(
                        new StringBuilder()
                            .Append("<")
                            .Append("\"19\",")
                            .Append("\"John\",")
                            .Append("\"12-33-19, 66-18-23\"")
                            .Append(">")
                            .ToString()
                    );
            }
        }

        [TestCaseSource(nameof(ObjectFormatterTestCases))]
        public string ShouldFormatObjectAccordingToDensity(FormatDensity density)
        {
            var account = new Account(19, "John", new[] {"12-33-19", "66-18-23"});

            var factory = new DefaultFormatterFactory()
                .SetDensityFlag(density)
                .EncloseBodyWith("<", ">")
                .EncloseMemberNameWith("'", "'")
                .EncloseMemberValueWith("\"", "\"")
                .JoinMemberNameAndValueWith(":")
                .JoinMembersWith(",");

            return factory.ObjectFormatter(AccountFormattingMembers)(account);
        }

        [Test]
        public void ObjectFormatterFactoryMethodShouldThrowArgumentNullExceptionWhenSequenceOfFormattingMembersIsNull()
        {
            var factory = new DefaultFormatterFactory();
            
            Assert.Throws<ArgumentNullException>(()=> factory.ObjectFormatter(null));
        }

        [Test]
        public void ObjectFormatterShouldIncludeNullValuesWhenCorrespondingDensityFlagWasSet()
        {
            var factory = new DefaultFormatterFactory()
                .SetDensityFlag(IncludeNullValues)
                .SetDensityFlag(IncludeMemberName)
                .EncloseBodyWith("(", ")")
                .JoinMemberNameAndValueWith(": ")
                .JoinMembersWith(", ");

            var toString = factory.ObjectFormatter(AccountFormattingMembers);
            
            Assert.That(toString(new Account(15)), Is.EqualTo(
                "(Id: 15, Name: null, Phones: null)"
            ));
        }

        private static IEnumerable<ITestCaseData> SequenceFormatterTestCases
        {
            get
            {
                yield return new TestCaseData(Dense).Returns("['John','']");
                yield return new TestCaseData(IncludeMemberName).Returns("['0':'John','1':'']");
                yield return new TestCaseData(IncludeNullValues).Returns("['John','','']");
                yield return new TestCaseData(MemberOnNewLine | IncludeMemberName | IncludeNullValues)
                    .Returns(
                        new StringBuilder()
                            .AppendLine("[")
                            .AppendLine("'0':'John',")
                            .AppendLine("'1':'',")
                            .Append("'2':''")
                            .Append("]")
                            .ToString()
                    );
            }
        }
        
        [TestCaseSource(nameof(SequenceFormatterTestCases))]
        public string ShouldFormatSequenceAccordingToDensity(FormatDensity density)
        {
            var factory = new DefaultFormatterFactory()
                .SetDensityFlag(density)
                .EncloseBodyWith("<", ">")
                .EncloseMemberNameWith("'", "'")
                .EncloseMemberValueWith("'", "'")
                .JoinMemberNameAndValueWith(":")
                .JoinMembersWith(",");

            return factory.EnumerableFormatter()(new []{"John", "", null});     
        }
    }
}