using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Operation = BoilerplateBuilders.Reflection.MemberFunction<System.Func<object, string>>;
using Formatter = System.Action<object, System.Text.StringBuilder>;
using static BoilerplateBuilders.ToString.Format.FormatDensity;
using static BoilerplateBuilders.ToString.Format.FormatCombinators;

namespace BoilerplateBuilders.ToString.Format
{
    /// <summary>
    /// Builds formatting function according to settings.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class DefaultFormatBuilder : IToStringFormat
    {
        /// <summary>
        /// Determines what should be printed into formatted output and how
        /// densely it should be formatted. 
        /// </summary>
        public FormatDensity Density { get; private set; }
        
        /// <summary>
        /// Tuple of symbols placed before first and after last member in output.
        /// </summary>
        public (string, string) ClassBodyEnclosure { get; private set; }

        /// <summary>
        /// Pair of symbols placed before and after every member within output.
        /// </summary>
        public (string, string) MemberEnclosure { get; private set; }
        
        /// <summary>
        /// Pair of symbols placed before and after member name when it is printed to output.
        /// </summary>
        public (string, string) MemberNameEnclosure { get; private set; }
        
        /// <summary>
        /// Pair of values placed before and after member value within output.
        /// </summary>
        public (string, string) MemberValueEnclosure { get; private set; }
        
        /// <summary>
        /// Symbol placed between two subsequent members within output. 
        /// </summary>
        public string MembersSeparator { get; private set; }
            
        /// <summary>
        /// Deactivates density flag and returns updated format instance. 
        /// </summary>
        /// <param name="flags">Density flag(s) to negate.</param>
        /// <returns>Updated <see cref="DefaultFormatBuilder"/> instance.</returns>
        public DefaultFormatBuilder UnsetDensityFlag(FormatDensity flags)
        {
            Density &= Density ^ flags;
            return this;
        }
        
        /// <summary>
        /// Sets density flag and returns updated format instance.
        /// </summary>
        /// <param name="flags">Density flag(s) to set.</param>
        /// <returns>Updated <see cref="DefaultFormatBuilder"/> instance.</returns>
        public DefaultFormatBuilder SetDensityFlag(FormatDensity flags)
        {
            Density |= flags;
            return this;
        }
        
        /// <summary>
        /// Instructs to enclose every formatted member into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols. 
        /// </summary>
        /// <param name="opening">Placed before formatted member name and value.</param>
        /// <param name="closing">Placed after formatted member name and value.</param>
        /// <returns>Updated <see cref="DefaultFormatBuilder"/> instance.</returns>
        public DefaultFormatBuilder EncloseMemberWith(string opening, string closing)
        {
            MemberEnclosure = (opening, closing);
            return this;
        }
        
        /// <summary>
        /// Instructs to enclose every formatted member value into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols.
        /// </summary>
        /// <param name="opening">Placed before formatted member value.</param>
        /// <param name="closing">Placed after formatted member value.</param>
        /// <returns>Updated <see cref="DefaultFormatBuilder"/> instance.</returns>
        public DefaultFormatBuilder EncloseMemberValueWith(string opening, string closing)
        {
            MemberValueEnclosure = (opening, closing);
            return this;
        }
        
        /// <summary>
        /// Instructs to enclose every formatted member name into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols. 
        /// </summary>
        /// <param name="opening">Placed before formatted member name.</param>
        /// <param name="closing">Placed after formatted member name.</param>
        /// <returns>Updated <see cref="DefaultFormatBuilder"/> instance.</returns>
        public DefaultFormatBuilder EncloseMemberNameWith(string opening, string closing)
        {
            MemberNameEnclosure = (opening, closing);
            return this;
        }
        
        /// <summary>
        /// Instructs to place <paramref name="separator"/> symbol between
        /// subsequent formatted members.
        /// </summary>
        /// <param name="separator">Placed between subsequent members.</param>
        /// <returns>Updated <see cref="DefaultFormatBuilder"/> instance.</returns>
        public DefaultFormatBuilder SeparateMembersWith(string separator)
        {
            MembersSeparator = separator;
            return this;
        }

        /// <summary>
        /// Instructs to enclose formatted list of members into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols.
        /// </summary>
        /// <param name="opening">Placed before first formatted member.</param>
        /// <param name="closing">Placed after lats formatted member.</param>
        /// <returns>Updated <see cref="DefaultFormatBuilder"/> instance.</returns>
        public DefaultFormatBuilder EncloseMemberListWith(string opening, string closing)
        {
            ClassBodyEnclosure = (opening, closing);
            return this;
        }
        
        /// <summary>
        /// Converts set of members operations to function returning string representation of an object
        /// according to specified settings.
        /// </summary>
        /// <param name="operations">Set of formatting member operations.</param>
        /// <returns>Function converting object to string representation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="operations"/> is null.
        /// </exception>
        public Func<object, string> Build(IEnumerable<Operation> operations)
        {
            if (operations is null)
                throw new ArgumentNullException(nameof(operations));

            var membersFormatter = operations
                .Select(AppendMember)
                .Aggregate(Join(AppendUnlessNullOrEmpty(MembersSeparator)));

            var bodyFormatter = Enclose(ClassBodyEnclosure, membersFormatter);
            
            var objectFormatter = AppendClassName() + When(o => o != null, bodyFormatter); 

            return MakeToString(objectFormatter);
        }

        private Formatter AppendClassName()
        {
            return When(
                Density.HasFlag(IncludeClassName),
                Append(o => o.GetType().Name)
            );
        }

        private Formatter AppendMember(Operation op)
        {
            var appendNewLine = When(
                Density.HasFlag(MemberOnNewLine), 
                (_, sb) => sb.AppendLine()
            );

            var appendMemberNameAndValue = Enclose(
                MemberEnclosure, 
                AppendMemberName(op.Member.MemberName) + AppendMemberValue(op.Function)
            );

            var appendMember = When(
                o => o != null || Density.HasFlag(IncludeNullValues),
                appendNewLine + appendMemberNameAndValue
            ); 
            
            return Map(op.Member.Getter, appendMember);
        }

        private Formatter AppendMemberName(string memberName)
        {
            return When(
                Density.HasFlag(IncludeMemberName),
                Enclose(MemberNameEnclosure, Append(memberName))
            );
        }

        private Formatter AppendMemberValue(Func<object, string> toString)
        {
            return Enclose(MemberValueEnclosure, Append(v => v == null ? "null" : toString(v)));
        }
    }
}