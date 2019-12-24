using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static BoilerplateBuilders.ToString.FormatDensity;
using static BoilerplateBuilders.ToString.Formatters;
using static BoilerplateBuilders.ToString.Writers;
using ToStringMember = BoilerplateBuilders.Reflection.MemberContext<System.Func<object, string>>;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    /// Builds formatting function according to provided settings.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class DefaultToStringFactory : IToStringFactory
    {
        /// <summary>
        /// Determines what should be printed into formatted output and how
        /// densely it should be formatted. 
        /// </summary>
        public FormatDensity Density { get; private set; }

        /// <summary>
        /// Tuple of symbols placed before first and after last member in output.
        /// </summary>
        public (string, string) BodyPrefixAndSuffix { get; private set; }

        /// <summary>
        /// Pair of symbols placed before and after every member within output.
        /// </summary>
        public (string, string) MemberPrefixAndSuffix { get; private set; }

        /// <summary>
        /// Prefix and suffix appended to every member name when it is printed to output.
        /// </summary>
        public (string, string) MemberNamePrefixAndSuffix { get; private set; }

        /// <summary>
        /// Prefix and suffix appended to every member value when it is printed to output.
        /// </summary>
        public (string, string) MemberValuePrefixAndSuffix { get; private set; }

        /// <summary>
        /// Symbol placed between two subsequent members within output.
        /// </summary>
        public string MemberSeparator { get; private set; }

        /// <summary>
        /// Deactivates density flag and returns updated format instance. 
        /// </summary>
        /// <param name="flags">Density flag(s) to negate.</param>
        /// <returns>Updated <see cref="DefaultToStringFactory"/> instance.</returns>
        public DefaultToStringFactory UnsetDensityFlag(FormatDensity flags)
        {
            Density &= Density ^ flags;
            return this;
        }

        /// <summary>
        /// Sets density flag and returns updated format instance.
        /// </summary>
        /// <param name="flags">Density flag(s) to set.</param>
        /// <returns>Updated <see cref="DefaultToStringFactory"/> instance.</returns>
        public DefaultToStringFactory SetDensityFlag(FormatDensity flags)
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
        /// <returns>Updated <see cref="DefaultToStringFactory"/> instance.</returns>
        public DefaultToStringFactory EncloseMemberWith(string opening, string closing)
        {
            MemberPrefixAndSuffix = (opening, closing);
            return this;
        }

        /// <summary>
        /// Instructs to enclose every formatted member value into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols.
        /// </summary>
        /// <param name="opening">Placed before formatted member value.</param>
        /// <param name="closing">Placed after formatted member value.</param>
        /// <returns>Updated <see cref="DefaultToStringFactory"/> instance.</returns>
        public DefaultToStringFactory EncloseMemberValueWith(string opening, string closing)
        {
            MemberValuePrefixAndSuffix = (opening, closing);
            return this;
        }

        /// <summary>
        /// Instructs to enclose every formatted member name into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols. 
        /// </summary>
        /// <param name="opening">Placed before formatted member name.</param>
        /// <param name="closing">Placed after formatted member name.</param>
        /// <returns>Updated <see cref="DefaultToStringFactory"/> instance.</returns>
        public DefaultToStringFactory EncloseMemberNameWith(string opening, string closing)
        {
            MemberNamePrefixAndSuffix = (opening, closing);
            return this;
        }

        /// <summary>
        /// Instructs to place <paramref name="separator"/> symbol between
        /// subsequent formatted members.
        /// </summary>
        /// <param name="separator">Placed between subsequent members.</param>
        /// <returns>Updated <see cref="DefaultToStringFactory"/> instance.</returns>
        public DefaultToStringFactory JoinMembersWith(string separator)
        {
            MemberSeparator = separator;
            return this;
        }

        /// <summary>
        /// Instructs to enclose formatted list of members into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols.
        /// </summary>
        /// <param name="opening">Placed before first formatted member.</param>
        /// <param name="closing">Placed after lats formatted member.</param>
        /// <returns>Updated <see cref="DefaultToStringFactory"/> instance.</returns>
        public DefaultToStringFactory EncloseBodyWith(string opening, string closing)
        {
            BodyPrefixAndSuffix = (opening, closing);
            return this;
        }

        /// <summary>
        /// Builds <see cref="object.ToString"/> function from sequence of formatting operations.
        /// </summary>
        /// <param name="members">Sequence of formatting member operations.</param>
        /// <returns>Function converting object to string representation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="members"/> is null.
        /// </exception>
        public Func<object, string> ObjectToString(IEnumerable<ToStringMember> members)
        {
            if (members is null)
                throw new ArgumentNullException(nameof(members));

            var formatMembers = Enclose(
                members
                    .Select(FormatMember)
                    .Aggregate(Joiner(Lift<object>(Write(MemberSeparator)))),
                BodyPrefixAndSuffix
            );

            var formatClassName = Density.HasFlag(IncludeClassName)
                ? Lift<object>(o => o?.GetType().Name)
                : Empty<object>();

            var formatObject =
                formatClassName + When(
                    o => o != null,
                    formatMembers,
                    Lift<object>(Write("null"))
                );

            return Formatters.ToString(formatObject);
        }

        public Func<IEnumerable, string> EnumerableToString()
        {
            var keyValuePairsFormatter = Collect<(int, object)>(
                AppendSequenceKeyAndValue<int, object>(),
                Write(MemberSeparator)
            );

            var seqFormatter = Wrap<IEnumerable, IEnumerable<(int, object)>>(keyValuePairsFormatter,
                seq => seq.Cast<object>().Select((value, index) => (index, value)));

            return Formatters.ToString(Enclose(seqFormatter, ("[", "]")));
        }

        public Func<IDictionary<K, V>, string> BuildDictionaryFormatter<K, V>()
        {
            var formatPairs = Collect<(K, V)>(
                AppendSequenceKeyAndValue<K, V>(),
                Write(MemberSeparator)
            );

            var formatSeq = Wrap<IDictionary<K, V>, IEnumerable<(K, V)>>(formatPairs,
                dict => dict.Select(kv => (kv.Key, kv.Value)));

            return Formatters.ToString(Enclose(formatSeq, ("{", "}")));
        }

        public Func<IEnumerable, string> BuildSetFormatter()
        {
            var valuesFormatter = Collect(
                PrependNewLineToMember<object>() + FormatMemberValue<object>(),
                Write(MemberSeparator)
            );

            var setFormatter = Wrap<IEnumerable, IEnumerable<object>>(valuesFormatter, seq => seq.Cast<object>());

            return Formatters.ToString(Enclose(setFormatter, ("{", "}")));
        }

        private Formatter<(K key, V value)> AppendSequenceKeyAndValue<K, V>()
        {
            var formatName = FormatMemberName<(K key, V value)>(kv => ToString(kv.key));
            var formatValue = FormatMemberValue<(K key, V value)>(kv => ToString(kv.value)); 
            var formatPair = Enclose(formatName + formatValue, MemberPrefixAndSuffix);
            return PrependNewLineToMember<(K key, V value)>() + formatPair;
        }

        private Formatter<object> FormatMember(ToStringMember op)
        {
            var nameAndValueFmt = Enclose(
                FormatMemberName<object>(_ => op.Member.MemberName) + FormatMemberValue(op.Context),
                MemberPrefixAndSuffix
            );

            var memberFmt = When(
                o => o != null,
                PrependNewLineToMember<object>() + nameAndValueFmt,
                Density.HasFlag(IncludeNullValues)
                    ? PrependNewLineToMember<object>() + nameAndValueFmt
                    : Empty<object>()
            );

            return Wrap(memberFmt, op.Member.Getter);
        }

        private Formatter<T> PrependNewLineToMember<T>()
        {
            return Density.HasFlag(ItemOnNewLine)
                ? Lift<T>(NewLine)
                : Empty<T>();
        }

        private Formatter<T> FormatMemberName<T>(Func<T, string> getName)
        {
            return Density.HasFlag(IncludeItemName)
                ? Enclose(Lift(getName), MemberNamePrefixAndSuffix)
                : Empty<T>();
        }

        private Formatter<T> FormatMemberValue<T>(Func<T, string> toString = null) =>
            Enclose(UnlessNull(Lift(toString ?? ToString)), MemberValuePrefixAndSuffix);

        private static string ToString<T>(T value) => value?.ToString();
    }
}