using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static BoilerplateBuilders.ToString.ObjectFormatOptions;
using static BoilerplateBuilders.ToString.Formatters;
using static BoilerplateBuilders.ToString.Writers;
using ToStringMember = BoilerplateBuilders.Reflection.MemberContext<System.Func<object, string>>;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    ///     Builds formatting function according to provided settings.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ObjectFormatterFactory : IFormatterFactory
    {
        /// <summary>
        ///     Determines what should be printed into formatted output and how
        ///     densely it should be formatted.
        /// </summary>
        public ObjectFormatOptions Options { get; private set; }

        /// <summary>
        ///     Tuple of symbols placed before first and after last member in output.
        /// </summary>
        public (string, string) BodyPrefixAndSuffix { get; private set; }

        /// <summary>
        ///     Pair of symbols placed before and after every member within output.
        /// </summary>
        public (string, string) MemberPrefixAndSuffix { get; private set; }

        /// <summary>
        ///     Prefix and suffix appended to every member name when it is printed to output.
        /// </summary>
        public (string, string) MemberNamePrefixAndSuffix { get; private set; }

        /// <summary>
        ///     Prefix and suffix appended to every member value when it is printed to output.
        /// </summary>
        public (string, string) MemberValuePrefixAndSuffix { get; private set; }

        /// <summary>
        ///     Symbol placed between two subsequent members within output.
        /// </summary>
        public string MemberSeparator { get; private set; }

        /// <summary>
        ///     Symbol placed between member name and value when both included to output.
        /// </summary>
        public string MemberNameValueSeparator { get; private set; }

        /// <summary>
        ///     Builds <see cref="object.ToString" /> function from sequence of formatting operations.
        /// </summary>
        /// <param name="members">Sequence of formatting member operations.</param>
        /// <returns>Function converting object to string representation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="members" /> is null.
        /// </exception>
        public Func<object, string> ObjectFormatter(IEnumerable<ToStringMember> members)
        {
            if (members is null)
                throw new ArgumentNullException(nameof(members));

            var formatMembers = JoinWithSeparator(members.Select(FormatObjectMember));

            var formatBody = Enclose(formatMembers, BodyPrefixAndSuffix);

            var formatClassName = Options.HasFlag(IncludeClassName)
                ? Lift<object>(o => o?.GetType().Name)
                : Empty<object>();

            var formatObject =
                Add(formatClassName, When(
                    o => o != null,
                    formatBody,
                    Lift<object>(Write("null"))
                ));

            return Formatters.ToString(formatObject);
        }

        /// <summary>
        /// Builds formatting function converting sequence of arbitrary objects to string. 
        /// </summary>
        public Func<IEnumerable, string> EnumerableFormatter()
        {
            var keyValuePairsFormatter = Collect<(int, object)>(
                AppendSequenceKeyAndValue<int, object>(),
                Write(MemberSeparator)
            );

            var seqFormatter = Wrap<IEnumerable, IEnumerable<(int, object)>>(keyValuePairsFormatter,
                seq => seq.Cast<object>().Select((value, index) => (index, value)));

            return Formatters.ToString(Enclose(seqFormatter, ("[", "]")));
        }

        /// <summary>
        ///     Deactivates density flag and returns updated format instance.
        /// </summary>
        /// <param name="options">Density flag(s) to negate.</param>
        /// <returns>Updated <see cref="ObjectFormatterFactory" /> instance.</returns>
        public ObjectFormatterFactory RemoveFlags(ObjectFormatOptions options)
        {
            Options &= Options ^ options;
            return this;
        }

        /// <summary>
        ///     Sets density flag and returns updated format instance.
        /// </summary>
        /// <param name="options">Density flag(s) to set.</param>
        /// <returns>Updated <see cref="ObjectFormatterFactory" /> instance.</returns>
        public ObjectFormatterFactory AddFlags(ObjectFormatOptions options)
        {
            Options |= options;
            return this;
        }

        /// <summary>
        ///     Instructs to enclose every formatted member into pair of <paramref name="prefix" />
        ///     and <paramref name="suffix" /> symbols.
        /// </summary>
        /// <param name="prefix">Placed before formatted member name and value.</param>
        /// <param name="suffix">Placed after formatted member name and value.</param>
        /// <returns>Updated <see cref="ObjectFormatterFactory" /> instance.</returns>
        public ObjectFormatterFactory ObjectMemberPrefixAndSuffix(string prefix, string suffix)
        {
            MemberPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        ///     Instructs to enclose every formatted member value into pair of <paramref name="prefix" />
        ///     and <paramref name="suffix" /> symbols.
        /// </summary>
        /// <param name="prefix">Placed before formatted member value.</param>
        /// <param name="suffix">Placed after formatted member value.</param>
        /// <returns>Updated <see cref="ObjectFormatterFactory" /> instance.</returns>
        public ObjectFormatterFactory ObjectMemberValuePrefixAndSuffix(string prefix, string suffix)
        {
            MemberValuePrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        ///     Instructs to enclose every formatted member name into pair of <paramref name="prefix" />
        ///     and <paramref name="suffix" /> symbols.
        /// </summary>
        /// <param name="prefix">Placed before formatted member name.</param>
        /// <param name="suffix">Placed after formatted member name.</param>
        /// <returns>Updated <see cref="ObjectFormatterFactory" /> instance.</returns>
        public ObjectFormatterFactory ObjectMemberNamePrefixAndSuffix(string prefix, string suffix)
        {
            MemberNamePrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        ///     Instructs to place <paramref name="separator" /> symbol between
        ///     subsequent formatted members.
        /// </summary>
        /// <param name="separator">Placed between subsequent members.</param>
        /// <returns>Updated <see cref="ObjectFormatterFactory" /> instance.</returns>
        public ObjectFormatterFactory JoinMembersWith(string separator)
        {
            MemberSeparator = separator;
            return this;
        }

        /// <summary>
        ///     Sets <see cref="MemberNameValueSeparator" /> and returns updated factory.
        /// </summary>
        /// <param name="separator">Character placed between member name and value.</param>
        /// <returns>Updated factory.</returns>
        public ObjectFormatterFactory JoinMemberNameAndValueWith(string separator)
        {
            MemberNameValueSeparator = separator;
            return this;
        }

        /// <summary>
        ///     Instructs to enclose formatted list of members into pair of <paramref name="opening" />
        ///     and <paramref name="closing" /> symbols.
        /// </summary>
        /// <param name="opening">Placed before first formatted member.</param>
        /// <param name="closing">Placed after lats formatted member.</param>
        /// <returns>Updated <see cref="ObjectFormatterFactory" /> instance.</returns>
        public ObjectFormatterFactory ObjectBodyPrefixAndSuffix(string opening, string closing)
        {
            BodyPrefixAndSuffix = (opening, closing);
            return this;
        }

        private Formatter<object> JoinWithSeparator(IEnumerable<Formatter<object>> formatters)
        {
            var separator = Lift<object>(Write(MemberSeparator));

            return formatters.Aggregate((first, second) =>
                Sum(first, separator, second)
            );
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
                Add(PrependNewLineToMember<object>(), FormatMemberValue<object>()),
                Write(MemberSeparator)
            );

            var setFormatter = Wrap<IEnumerable, IEnumerable<object>>(valuesFormatter, seq => seq.Cast<object>());

            return Formatters.ToString(Enclose(setFormatter, ("{", "}")));
        }

        private Formatter<(TK key, TV value)> AppendSequenceKeyAndValue<TK, TV>() =>
            FormatNameAndValue<(TK key, TV value)>(
                kv => ToString(kv.key),
                kv => ToString(kv.value)
            );

        private Formatter<T> FormatNameAndValue<T>(Func<T, string> getName, Func<T, string> getValue)
        {
            var formatName = FormatName(getName, IncludeMemberName);
            var formatValue = FormatMemberValue(getValue);

            var formatNameValueSeparator = Options.HasFlag(IncludeMemberName)
                ? Lift<T>(Write(MemberNameValueSeparator))
                : Empty<T>();

            var formatNameAndValue = Add(
                PrependNewLineToMember<T>(),
                Enclose(
                    Sum(
                        formatName,
                        formatNameValueSeparator,
                        formatValue
                    ),
                    MemberPrefixAndSuffix
                )
            );

            return When(
                o => getValue(o) != null,
                formatNameAndValue,
                Options.HasFlag(IncludeNullValues)
                    ? formatNameAndValue
                    : Empty<T>()
            );
        }

        private Formatter<object> FormatObjectMember(ToStringMember op) =>
            Wrap(FormatNameAndValue(
                _ => op.Member.MemberName,
                op.Context
            ), op.Member.Getter);

        private Formatter<T> PrependNewLineToMember<T>()
        {
            return Options.HasFlag(MemberOnNewLine)
                ? Lift<T>(NewLine)
                : Empty<T>();
        }

        private Formatter<T> FormatName<T>(Func<T, string> getName, ObjectFormatOptions nameOption)
        {
            return Options.HasFlag(nameOption)
                ? Enclose(Lift(getName), MemberNamePrefixAndSuffix)
                : Empty<T>();
        }

        private Formatter<T> FormatMemberValue<T>(Func<T, string> toString = null)
        {
            return Enclose(UnlessNull(Lift(toString ?? ToString)), MemberValuePrefixAndSuffix);
        }

        private static string ToString<T>(T value) => value?.ToString();
    }
}