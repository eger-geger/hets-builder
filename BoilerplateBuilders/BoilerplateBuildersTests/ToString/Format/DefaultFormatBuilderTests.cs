using System;
using System.Collections.Generic;
using BoilerplateBuilders.ToString.Format;
using NUnit.Framework;
using static BoilerplateBuilders.ToString.Format.FormatDensity;

namespace BoilerplateBuildersTests.ToString.Format
{
    public class DefaultFormatBuilderTests
    {
        [Test]
        public void EmptyBuilder()
        {
            var builder = new DefaultToStringFactory();
            var defaultEnclosure = ValueTuple.Create<string, string>(null, null);
            
            Assert.That(builder.Density, Is.EqualTo((FormatDensity) 0));
            Assert.That(builder.MemberPrefixAndSuffix, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.MemberSeparator, Is.EqualTo(null));
            Assert.That(builder.BodyPrefixAndSuffix, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.MemberNamePrefixAndSuffix, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.MemberValuePrefixAndSuffix, Is.EqualTo(defaultEnclosure));
        }

        [Test]
        public void ShouldSetMultipleDensityFlags()
        {
            var builder = new DefaultToStringFactory()
                .SetDensityFlag(IncludeClassName)
                .SetDensityFlag(IncludeItemName)
                .SetDensityFlag(ItemOnNewLine);
            
            Assert.That(builder.Density, Is.EqualTo(
                IncludeClassName 
                | IncludeItemName
                | ItemOnNewLine
            ));
        }

        [Test]
        public void ShouldUnsetDensityFlags()
        {
            var builder = new DefaultToStringFactory()
                .SetDensityFlag(IncludeClassName)
                .SetDensityFlag(IncludeItemName)
                .SetDensityFlag(ItemOnNewLine)
                .UnsetDensityFlag(
                    IncludeClassName
                    | IncludeItemName
                    | ItemOnNewLine
                    | IncludeNullValues
                );
            
            Assert.That(builder.Density, Is.EqualTo((FormatDensity) 0));
        }
        
        [Test]
        public void ShouldUpdateMemberEnclosure()
        {
            var builder = new DefaultToStringFactory()
                .EncloseMemberWith("<", ">");
            
            Assert.That(builder.MemberPrefixAndSuffix, Is.EqualTo(("<", ">")));
        }

        [Test]
        public void ShouldUpdateMemberValueEnclosure()
        {
            var builder = new DefaultToStringFactory()
                .EncloseMemberValueWith(null, "]");
            
            Assert.That(
                builder.MemberValuePrefixAndSuffix, 
                Is.EqualTo(ValueTuple.Create<string, string>(null, "]"))
            );
        }
    }
}