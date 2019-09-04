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
            var builder = new DefaultFormatProviderBuilder();
            var defaultEnclosure = ValueTuple.Create<string, string>(null, null);
            
            Assert.That(builder.Density, Is.EqualTo((FormatDensity) 0));
            Assert.That(builder.ItemQuotes, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.ItemSeparator, Is.EqualTo(null));
            Assert.That(builder.AllMembersQuotes, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.ItemNameQuotes, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.ItemValueQuotes, Is.EqualTo(defaultEnclosure));
        }

        [Test]
        public void ShouldSetMultipleDensityFlags()
        {
            var builder = new DefaultFormatProviderBuilder()
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
            var builder = new DefaultFormatProviderBuilder()
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
            var builder = new DefaultFormatProviderBuilder()
                .EncloseMemberWith("<", ">");
            
            Assert.That(builder.ItemQuotes, Is.EqualTo(("<", ">")));
        }

        [Test]
        public void ShouldUpdateMemberValueEnclosure()
        {
            var builder = new DefaultFormatProviderBuilder()
                .EncloseMemberValueWith(null, "]");
            
            Assert.That(
                builder.ItemValueQuotes, 
                Is.EqualTo(ValueTuple.Create<string, string>(null, "]"))
            );
        }
    }
}