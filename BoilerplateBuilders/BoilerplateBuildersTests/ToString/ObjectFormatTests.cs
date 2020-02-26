using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.ToString;
using BoilerplateBuildersTests.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static BoilerplateBuilders.ToString.ObjectFormatOptions;
using Enumerable = System.Linq.Enumerable;

namespace BoilerplateBuildersTests.ToString
{
    public class ObjectFormatTests
    {
        private static IEnumerable<MemberContext<Func<object, string>>> AccountMembers
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
                    name => name.ToString(),
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
            var format = new ObjectFormat();
            var toString = format.Compile(AccountMembers);
            var defaultPrefixAndSuffix = ValueTuple.Create<string, string>(null, null);

            Assert.That(format.Options, Is.EqualTo((ObjectFormatOptions) 0));
            Assert.That(format.MemberPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.MemberSeparator, Is.EqualTo(null));
            Assert.That(format.ObjectPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.MemberNamePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(format.MemberValuePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));

            Assert.That(toString(new Account(10, "James", "777")), Is.EqualTo("10James777"));
        }

        [Test]
        public void ShouldSetMultipleDensityFlags()
        {
            var format = new ObjectFormat()
                .AddFlags(IncludeClassName)
                .AddFlags(IncludeMemberName)
                .AddFlags(MemberPerLine);

            Assert.That(format.Options, Is.EqualTo(
                IncludeClassName
                | IncludeMemberName
                | MemberPerLine
            ));
        }

        [Test]
        public void ShouldUnsetDensityFlags()
        {
            var format = new ObjectFormat()
                .AddFlags(IncludeClassName)
                .AddFlags(IncludeMemberName)
                .AddFlags(MemberPerLine)
                .RemoveFlags(
                    IncludeClassName
                    | IncludeMemberName
                    | MemberPerLine
                    | IncludeNullValues
                );

            Assert.That(format.Options, Is.EqualTo((ObjectFormatOptions) 0));
        }

        [Test]
        public void ShouldUpdateMemberPrefixAndSuffix()
        {
            var format = new ObjectFormat()
                .ObjectMemberPrefixAndSuffix("<", ">");

            Assert.That(format.MemberPrefixAndSuffix, Is.EqualTo(("<", ">")));
        }

        [Test]
        public void ShouldUpdateMemberValuePrefixAndSuffix()
        {
            var format = new ObjectFormat()
                .ObjectMemberValuePrefixAndSuffix(null, "]");

            Assert.That(
                format.MemberValuePrefixAndSuffix,
                Is.EqualTo(ValueTuple.Create<string, string>(null, "]"))
            );
        }

        [Test]
        public void ShouldUpdateMemberNamePrefixAndSuffix()
        {
            var format = new ObjectFormat()
                .ObjectMemberNamePrefixAndSuffix("'", "'");

            Assert.That(format.MemberNamePrefixAndSuffix, Is.EqualTo(ValueTuple.Create("'", "'")));
        }

        [Test]
        public void ShouldUpdateMemberSeparator()
        {
            var format = new ObjectFormat()
                .JoinMembersWith(",");

            Assert.That(format.MemberSeparator, Is.EqualTo(","));
        }

        [Test]
        public void ShouldUpdateBodyPrefixAndSuffix()
        {
            var format = new ObjectFormat()
                .ObjectBodyPrefixAndSuffix("{", "}");

            Assert.That(format.ObjectPrefixAndSuffix, Is.EqualTo(ValueTuple.Create("{", "}")));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static IEnumerable<ITestCaseData> ObjectFormatterTestCases
        {
            get
            {
                yield return new TestCaseData(IncludeClassName | IncludeMemberName | MemberPerLine)
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

                yield return new TestCaseData(None)
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
        public string ShouldFormatObjectAccordingToDensity(ObjectFormatOptions options)
        {
            var account = new Account(19, "John", "12-33-19", "66-18-23");

            var format = new ObjectFormat()
                .AddFlags(options)
                .ObjectBodyPrefixAndSuffix("<", ">")
                .ObjectMemberNamePrefixAndSuffix("'", "'")
                .ObjectMemberValuePrefixAndSuffix("\"", "\"")
                .JoinMemberNameAndValueWith(":")
                .JoinMembersWith(",");

            return format.Compile(AccountMembers)(account);
        }

        [Test]
        public void ObjectFormatterFactoryMethodShouldThrowArgumentNullExceptionWhenSequenceOfFormattingMembersIsNull()
        {
            var factory = new ObjectFormat();

            Assert.Throws<ArgumentNullException>(() => factory.Compile(null));
        }

        [Test]
        public void ObjectFormatterShouldIncludeNullValuesWhenCorrespondingDensityFlagWasSet()
        {
            var format = new ObjectFormat()
                .AddFlags(IncludeNullValues)
                .AddFlags(IncludeMemberName)
                .ObjectBodyPrefixAndSuffix("(", ")")
                .JoinMemberNameAndValueWith(": ")
                .JoinMembersWith(", ");

            var toString = format.Compile(AccountMembers);

            Assert.That(toString(new Account(15, null, null)), Is.EqualTo(
                "(Id: 15, Name: null, Phones: null)"
            ));
        }

        [Test]
        public void ShouldUseDefaultToStringFunctionWhenOneWasNotProvidedExplicitly()
        {
            var toString = ObjectFormat.CreateDefault()
                .Compile(AccountMembers.Select(am =>
                    new MemberContext<Func<object, string>>(am.Member, null, ContextSource.Implicit)
                ));

            Assert.That(
                toString(new Account(42, "James")),
                Is.EqualTo("Account(Id: \"42\", Name: \"James\", Phones: \"System.String[]\")")
            );
        }

        [Test]
        public void ShouldCreateDefaultFormat()
        {
            var format = ObjectFormat.CreateDefault();
            var toString = format.Compile(AccountMembers);

            Assert.That(toString, Is.Not.Null);

            Assert.That(
                toString(new Account(42, "James", null)),
                Is.EqualTo("Account(Id: \"42\", Name: \"James\")")
            );
        }

        [Test]
        public void ShouldAlwaysCreateNewInstanceOfDefaultFormat()
        {
            var firstFormat = ObjectFormat.CreateDefault()
                .AddFlags(MemberPerLine)
                .ObjectMemberNamePrefixAndSuffix("<", ">");

            var firstToString = firstFormat.Compile(AccountMembers);

            var secondFormat = ObjectFormat.CreateDefault();
            var secondToString = secondFormat.Compile(AccountMembers);

            Assert.That(firstFormat, Is.Not.SameAs(secondFormat));
            Assert.That(firstToString, Is.Not.SameAs(secondToString));
            Assert.That(secondToString(new Account(42, "James", null)),
                Is.EqualTo("Account(Id: \"42\", Name: \"James\")"));
        }
    }
}