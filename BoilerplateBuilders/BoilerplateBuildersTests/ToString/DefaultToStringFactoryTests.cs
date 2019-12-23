using System;
using System.Collections.Generic;
using System.Text;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.ToString;
using BoilerplateBuildersTests.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static BoilerplateBuilders.ToString.FormatDensity;

namespace BoilerplateBuildersTests.ToString
{
    public class DefaultToStringFactoryTests
    {
        [Test]
        public void EmptyBuilder()
        {
            var factory = new DefaultToStringFactory();
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
            var factory = new DefaultToStringFactory()
                .SetDensityFlag(IncludeClassName)
                .SetDensityFlag(IncludeItemName)
                .SetDensityFlag(ItemOnNewLine);

            Assert.That(factory.Density, Is.EqualTo(
                IncludeClassName
                | IncludeItemName
                | ItemOnNewLine
            ));
        }

        [Test]
        public void ShouldUnsetDensityFlags()
        {
            var factory = new DefaultToStringFactory()
                .SetDensityFlag(IncludeClassName)
                .SetDensityFlag(IncludeItemName)
                .SetDensityFlag(ItemOnNewLine)
                .UnsetDensityFlag(
                    IncludeClassName
                    | IncludeItemName
                    | ItemOnNewLine
                    | IncludeNullValues
                );

            Assert.That(factory.Density, Is.EqualTo((FormatDensity) 0));
        }

        [Test]
        public void ShouldUpdateMemberPrefixAndSuffix()
        {
            var factory = new DefaultToStringFactory()
                .EncloseMemberWith("<", ">");

            Assert.That(factory.MemberPrefixAndSuffix, Is.EqualTo(("<", ">")));
        }

        [Test]
        public void ShouldUpdateMemberValuePrefixAndSuffix()
        {
            var factory = new DefaultToStringFactory()
                .EncloseMemberValueWith(null, "]");

            Assert.That(
                factory.MemberValuePrefixAndSuffix,
                Is.EqualTo(ValueTuple.Create<string, string>(null, "]"))
            );
        }

        [Test]
        public void ShouldUpdateMemberNamePrefixAndSuffix()
        {
            var factory = new DefaultToStringFactory()
                .EncloseMemberNameWith("'", "'");

            Assert.That(factory.MemberNamePrefixAndSuffix, Is.EqualTo(ValueTuple.Create("'", "'")));
        }

        [Test]
        public void ShouldUpdateMemberSeparator()
        {
            var factory = new DefaultToStringFactory()
                .JoinMembersWith(",");

            Assert.That(factory.MemberSeparator, Is.EqualTo(","));
        }

        [Test]
        public void ShouldUpdateBodyPrefixAndSuffix()
        {
            var factory = new DefaultToStringFactory()
                .EncloseBodyWith("{", "}");

            Assert.That(factory.BodyPrefixAndSuffix, Is.EqualTo(ValueTuple.Create("{", "}")));
        }

        private static IEnumerable<ITestCaseData> ObjectToStringTestCases
        {
            get
            {
                yield return new TestCaseData(IncludeClassName | IncludeItemName | ItemOnNewLine)
                    .Returns(
                        new StringBuilder()
                            .Append("Account")
                            .AppendLine()
                            .AppendLine("'id'")
                            .ToString()
                    );
            }
        }

        [TestCaseSource(nameof(ObjectToStringTestCases))]
        public string ShouldCreateProperToStringFunction(FormatDensity density)
        {
            var account = new Account(19, "John", new[] {"12-33-19", "66-18-23"});

            var members = new[]
            {
                new MemberContext<Func<object, string>>(
                    SelectedMember.Create<Account, int>(ac => ac.Id),
                    id => id.ToString(),
                    ContextSource.Implicit
                ),
                new MemberContext<Func<object, string>>(
                    SelectedMember.Create<Account, string>(ac => ac.Name),
                    name => name?.ToString(),
                    ContextSource.Implicit
                ),
                new MemberContext<Func<object, string>>(
                    SelectedMember.Create<Account, string[]>(ac => ac.Phones),
                    phones => string.Join(", ", (string[]) phones),
                    ContextSource.Implicit
                )
            };

            var factory = new DefaultToStringFactory()
                .SetDensityFlag(density)
                .EncloseBodyWith("<", ">")
                .EncloseMemberNameWith("'", "'")
                .EncloseMemberValueWith("\"", "\"")
                .JoinMembersWith(",");

            return factory.ObjectToString(members)(account);
        }
    }
}