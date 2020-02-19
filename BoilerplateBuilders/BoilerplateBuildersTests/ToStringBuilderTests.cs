using BoilerplateBuilders;
using BoilerplateBuildersTests.Models;
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
                .UseCollectionFormatter()
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

            var account = new Account(50, "James Bond");
            
            Assert.That(toString, Is.Not.Null);
            Assert.That(toString(account), Is.EqualTo("Account(Name: \"James Bond\", Phones: \"System.String[]\", Id: \"32\")"));
        }

        [Test]
        public void ShouldBuildToStringWithExplicitFormatForAllMembersOfGivenType()
        {
            //TODO
        }

        [Test]
        public void ShouldBuildToStringFormattingDictionaryMembers()
        {
            //TODO
        }

        [Test]
        public void ShouldBuildToStringWithCustomDictionaryFormat()
        {
            //TODO
        }

        [Test]
        public void ShouldBuildToStringWithCustomCollectionFormat()
        {
            //TODO
        }

        [Test]
        public void ShouldBuildToStringWithCustomObjectFormat()
        {
            //TODO
        }
    }
}