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
            var builder = new DefaultFormatBuilder();
            var defaultEnclosure = ValueTuple.Create<string, string>(null, null);
            
            Assert.That(builder.Density, Is.EqualTo((FormatDensity) 0));
            Assert.That(builder.MemberEnclosure, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.MembersSeparator, Is.EqualTo(null));
            Assert.That(builder.ClassBodyEnclosure, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.MemberNameEnclosure, Is.EqualTo(defaultEnclosure));
            Assert.That(builder.MemberValueEnclosure, Is.EqualTo(defaultEnclosure));
        }

        [Test]
        public void ShouldSetMultipleDensityFlags()
        {
            var builder = new DefaultFormatBuilder()
                .SetDensityFlag(IncludeClassName)
                .SetDensityFlag(IncludeMemberName)
                .SetDensityFlag(MemberOnNewLine);
            
            Assert.That(builder.Density, Is.EqualTo(
                IncludeClassName 
                | IncludeMemberName
                | MemberOnNewLine
            ));
        }

        [Test]
        public void ShouldUnsetDensityFlags()
        {
            var builder = new DefaultFormatBuilder()
                .SetDensityFlag(IncludeClassName)
                .SetDensityFlag(IncludeMemberName)
                .SetDensityFlag(MemberOnNewLine)
                .UnsetDensityFlag(
                    IncludeClassName
                    | IncludeMemberName
                    | MemberOnNewLine
                    | IncludeNullValues
                );
            
            Assert.That(builder.Density, Is.EqualTo((FormatDensity) 0));
        }
        
        [Test]
        public void ShouldUpdateMemberEnclosure()
        {
            var builder = new DefaultFormatBuilder()
                .EncloseMemberWith("<", ">");
            
            Assert.That(builder.MemberEnclosure, Is.EqualTo(("<", ">")));
        }

        [Test]
        public void ShouldUpdateMemberValueEnclosure()
        {
            var builder = new DefaultFormatBuilder()
                .EncloseMemberValueWith(null, "]");
            
            Assert.That(
                builder.MemberValueEnclosure, 
                Is.EqualTo(ValueTuple.Create<string, string>(null, "]"))
            );
        }
    }
}