using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public class ObjectFormatterFactoryTests
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
            var factory = new ObjectFormatterBuilder();
            var toStringFunction = factory.BuildToString(AccountFormattingMembers);
            var defaultPrefixAndSuffix = ValueTuple.Create<string, string>(null, null);

            Assert.That(factory.Options, Is.EqualTo((ObjectFormatOptions) 0));
            Assert.That(factory.MemberPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.MemberSeparator, Is.EqualTo(null));
            Assert.That(factory.ObjectPrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.MemberNamePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            Assert.That(factory.MemberValuePrefixAndSuffix, Is.EqualTo(defaultPrefixAndSuffix));
            
            Assert.That(toStringFunction(new Account(10, "James", "777")), Is.EqualTo("10James777"));
        }

        [Test]
        public void ShouldSetMultipleDensityFlags()
        {
            var factory = new ObjectFormatterBuilder()
                .AddFlags(IncludeClassName)
                .AddFlags(IncludeMemberName)
                .AddFlags(MemberPerLine);

            Assert.That(factory.Options, Is.EqualTo(
                IncludeClassName
                | IncludeMemberName
                | MemberPerLine
            ));
        }

        [Test]
        public void ShouldUnsetDensityFlags()
        {
            var factory = new ObjectFormatterBuilder()
                .AddFlags(IncludeClassName)
                .AddFlags(IncludeMemberName)
                .AddFlags(MemberPerLine)
                .RemoveFlags(
                    IncludeClassName
                    | IncludeMemberName
                    | MemberPerLine
                    | IncludeNullValues
                );

            Assert.That(factory.Options, Is.EqualTo((ObjectFormatOptions) 0));
        }

        [Test]
        public void ShouldUpdateMemberPrefixAndSuffix()
        {
            var factory = new ObjectFormatterBuilder()
                .ObjectMemberPrefixAndSuffix("<", ">");

            Assert.That(factory.MemberPrefixAndSuffix, Is.EqualTo(("<", ">")));
        }

        [Test]
        public void ShouldUpdateMemberValuePrefixAndSuffix()
        {
            var factory = new ObjectFormatterBuilder()
                .ObjectMemberValuePrefixAndSuffix(null, "]");

            Assert.That(
                factory.MemberValuePrefixAndSuffix,
                Is.EqualTo(ValueTuple.Create<string, string>(null, "]"))
            );
        }

        [Test]
        public void ShouldUpdateMemberNamePrefixAndSuffix()
        {
            var factory = new ObjectFormatterBuilder()
                .ObjectMemberNamePrefixAndSuffix("'", "'");

            Assert.That(factory.MemberNamePrefixAndSuffix, Is.EqualTo(ValueTuple.Create("'", "'")));
        }

        [Test]
        public void ShouldUpdateMemberSeparator()
        {
            var factory = new ObjectFormatterBuilder()
                .JoinMembersWith(",");

            Assert.That(factory.MemberSeparator, Is.EqualTo(","));
        }

        [Test]
        public void ShouldUpdateBodyPrefixAndSuffix()
        {
            var factory = new ObjectFormatterBuilder()
                .ObjectBodyPrefixAndSuffix("{", "}");

            Assert.That(factory.ObjectPrefixAndSuffix, Is.EqualTo(ValueTuple.Create("{", "}")));
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

            var factory = new ObjectFormatterBuilder()
                .AddFlags(options)
                .ObjectBodyPrefixAndSuffix("<", ">")
                .ObjectMemberNamePrefixAndSuffix("'", "'")
                .ObjectMemberValuePrefixAndSuffix("\"", "\"")
                .JoinMemberNameAndValueWith(":")
                .JoinMembersWith(",");

            return factory.BuildToString(AccountFormattingMembers)(account);
        }

        [Test]
        public void ObjectFormatterFactoryMethodShouldThrowArgumentNullExceptionWhenSequenceOfFormattingMembersIsNull()
        {
            var factory = new ObjectFormatterBuilder();
            
            Assert.Throws<ArgumentNullException>(()=> factory.BuildToString(null));
        }

        [Test]
        public void ObjectFormatterShouldIncludeNullValuesWhenCorrespondingDensityFlagWasSet()
        {
            var factory = new ObjectFormatterBuilder()
                .AddFlags(IncludeNullValues)
                .AddFlags(IncludeMemberName)
                .ObjectBodyPrefixAndSuffix("(", ")")
                .JoinMemberNameAndValueWith(": ")
                .JoinMembersWith(", ");

            var toString = factory.BuildToString(AccountFormattingMembers);
            
            Assert.That(toString(new Account(15, null, null)), Is.EqualTo(
                "(Id: 15, Name: null, Phones: null)"
            ));
        }

        [Test]
        public void ObjectFormatterShouldUseDefaultToStringFunctionWhenOneWasNotProvidedExplicitly()
        {
            //TODO test that default ToString is being used
        }
    }
}