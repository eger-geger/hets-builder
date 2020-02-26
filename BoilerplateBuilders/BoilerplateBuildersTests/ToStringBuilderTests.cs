using System.Collections.Generic;
using BoilerplateBuilders;
using BoilerplateBuilders.ToString;
using BoilerplateBuildersTests.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BoilerplateBuildersTests
{
    public class ToStringBuilderTests
    {
        [Test]
        public void ShouldBuildToStringWithDefaultFormat()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .Build();

            var account = new Account(50, "James Bond", "777-777");

            Assert.That(toString, Is.Not.Null);
            Assert.That(toString(account),
                Is.EqualTo("Account(Id: \"50\", Name: \"James Bond\", Phones: \"System.String[]\")"));
        }

        [Test]
        public void ShouldBuildToStringWithDefaultArrayFormat()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .UseCollectionFormat()
                .Build();

            var account = new Account(50, "James Bond", "777-777", "44-07-77");

            Assert.That(toString, Is.Not.Null);
            Assert.That(toString(account),
                Is.EqualTo("Account(Id: \"50\", Name: \"James Bond\", Phones: \"['777-777', '44-07-77']\")"));
        }

        [Test]
        public void ShouldBuildToStringWithExplicitMembersOnly()
        {
            var toString = new ToStringBuilder<Account>()
                .Append(acc => acc.Id, id => id.ToString("X"))
                .Append(acc => acc.Name, name => "Agent 007")
                .Build();

            var account = new Account(50, "James Bond", "777-777", "44-07-77");

            Assert.That(toString, Is.Not.Null);
            Assert.That(toString(account), Is.EqualTo("Account(Id: \"32\", Name: \"Agent 007\")"));
        }

        [Test]
        public void ShouldBuildToStringWithExplicitMemberFormat()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .Append(acc => acc.Id, id => id.ToString("X"))
                .Build();


            Assert.That(toString, Is.Not.Null);
            Assert.That(
                toString(new Account(50, "James Bond")),
                Is.EqualTo("Account(Name: \"James Bond\", Phones: \"System.String[]\", Id: \"32\")")
            );
        }

        [Test]
        public void ShouldBuildToStringWithExplicitFormatForAllMembersOfGivenType()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .Use<int>(i => i.ToString("X"))
                .Build();

            Assert.That(toString, Is.Not.Null);

            Assert.That(
                toString(new Account(16, "James Bond", null)),
                Is.EqualTo("Account(Id: \"10\", Name: \"James Bond\")")
            );
        }

        [Test]
        public void ShouldBuildToStringFormattingDictionaryMembers()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicProperties()
                .UseDictionaryFormat()
                .Build();
            
            Assert.That(
                toString(new Account(44, "James Bond", null)
                {
                    Extra = new Dictionary<string, object>
                    {
                        {"Code", "007"},
                        {"Age", 36}
                    }
                }),
                Is.EqualTo("Account(Name: \"James Bond\", Extra: \"{Code:'007', Age:'36'}\")")
            );
        }

        [Test]
        public void ShouldBuildToStringWithCustomDictionaryFormat()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicProperties()
                .UseDictionaryFormat(fmt => fmt
                    .SetDictionaryPrefixAndSuffix("", "")
                    .SetKeyValuePairSeparator("|")
                    .SetValuePrefixAndSuffix("<", ">")
                    .SetKeyValueSeparator("")
                )
                .Build();
            
            Assert.That(
                toString(new Account(44, "James Bond", null)
                {
                    Extra = new Dictionary<string, object>
                    {
                        {"Code", "007"},
                        {"Age", 36}
                    }
                }),
                Is.EqualTo("Account(Name: \"James Bond\", Extra: \"Code<007>|Age<36>\")")
            );
        }

        [Test]
        public void ShouldBuildToStringWithSpecificTypedDictionaryFormat()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicProperties()
                .UseDictionaryFormat(fmt => fmt
                    .SetDictionaryPrefixAndSuffix("", "")
                    .SetKeyValuePairSeparator("|")
                    .SetValuePrefixAndSuffix("<", ">")
                    .SetKeyValueSeparator("")
                )
                .UseDictionaryFormat<string, object>()
                .Build();
            
            Assert.That(
                toString(new Account(44, "James Bond", null)
                {
                    Extra = new Dictionary<string, object>
                    {
                        {"Code", "007"},
                        {"Age", 36}
                    }
                }),
                Is.EqualTo("Account(Name: \"James Bond\", Extra: \"{Code:'007', Age:'36'}\")")
            );
        }

        [Test]
        public void ShouldBuildToStringWithCustomCollectionFormat()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .UseCollectionFormat(fmt => fmt
                    .AddOptions(CollectionFormatOptions.IncludeIndex)
                    .SetIndexValueSeparator(":")
                    .SetIndexPrefixAndSuffix("#", null)
                ).Build();
            
            Assert.That(
                toString(new Account(42, "James Bond", "12-66-77", "33-12-33")),
                Is.EqualTo("Account(Id: \"42\", Name: \"James Bond\", Phones: \"[#0:'12-66-77', #1:'33-12-33']\")")    
            );
        }

        [Test]
        public void ShouldBuildToStringWithCustomObjectFormat()
        {
            var toString = new ToStringBuilder<Account>()
                .AppendPublicFields()
                .AppendPublicProperties()
                .UseObjectFormat(fmt => fmt
                    .RemoveFlags(ObjectFormatOptions.IncludeMemberName)
                    .ObjectMemberValuePrefixAndSuffix("'", "'")
                ).Build();
            
            Assert.That(
                toString(new Account(42, "James Bond", "44-77-07")),
                Is.EqualTo("Account('42', 'James Bond', 'System.String[]')")
            );
        }
    }
}