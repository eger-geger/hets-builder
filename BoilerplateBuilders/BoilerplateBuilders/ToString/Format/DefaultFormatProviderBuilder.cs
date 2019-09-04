using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Operation = BoilerplateBuilders.Reflection.MemberContext<System.Func<object, string>>;
using static BoilerplateBuilders.ToString.Format.FormatDensity;
using static BoilerplateBuilders.ToString.Format.FormatCombinators;

namespace BoilerplateBuilders.ToString.Format
{
    /// <summary>
    /// Builds formatting function according to settings.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class DefaultFormatProviderBuilder : IFormatProvider
    {
        /// <summary>
        /// Determines what should be printed into formatted output and how
        /// densely it should be formatted. 
        /// </summary>
        public FormatDensity Density { get; private set; }

        /// <summary>
        /// Tuple of symbols placed before first and after last member in output.
        /// </summary>
        public (string, string) AllMembersQuotes { get; private set; }

        /// <summary>
        /// Pair of symbols placed before and after every member within output.
        /// </summary>
        public (string, string) ItemQuotes { get; private set; }

        /// <summary>
        /// Pair of symbols placed before and after every member name when it is printed to output.
        /// </summary>
        public (string, string) ItemNameQuotes { get; private set; }

        /// <summary>
        /// Pair of values placed before and after every member value within output.
        /// </summary>
        public (string, string) ItemValueQuotes { get; private set; }

        /// <summary>
        /// Symbol placed between two subsequent members within output.
        /// </summary>
        public string ItemSeparator { get; private set; }

        /// <summary>
        /// Deactivates density flag and returns updated format instance. 
        /// </summary>
        /// <param name="flags">Density flag(s) to negate.</param>
        /// <returns>Updated <see cref="DefaultFormatProviderBuilder"/> instance.</returns>
        public DefaultFormatProviderBuilder UnsetDensityFlag(FormatDensity flags)
        {
            Density &= Density ^ flags;
            return this;
        }

        /// <summary>
        /// Sets density flag and returns updated format instance.
        /// </summary>
        /// <param name="flags">Density flag(s) to set.</param>
        /// <returns>Updated <see cref="DefaultFormatProviderBuilder"/> instance.</returns>
        public DefaultFormatProviderBuilder SetDensityFlag(FormatDensity flags)
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
        /// <returns>Updated <see cref="DefaultFormatProviderBuilder"/> instance.</returns>
        public DefaultFormatProviderBuilder EncloseMemberWith(string opening, string closing)
        {
            ItemQuotes = (opening, closing);
            return this;
        }

        /// <summary>
        /// Instructs to enclose every formatted member value into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols.
        /// </summary>
        /// <param name="opening">Placed before formatted member value.</param>
        /// <param name="closing">Placed after formatted member value.</param>
        /// <returns>Updated <see cref="DefaultFormatProviderBuilder"/> instance.</returns>
        public DefaultFormatProviderBuilder EncloseMemberValueWith(string opening, string closing)
        {
            ItemValueQuotes = (opening, closing);
            return this;
        }

        /// <summary>
        /// Instructs to enclose every formatted member name into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols. 
        /// </summary>
        /// <param name="opening">Placed before formatted member name.</param>
        /// <param name="closing">Placed after formatted member name.</param>
        /// <returns>Updated <see cref="DefaultFormatProviderBuilder"/> instance.</returns>
        public DefaultFormatProviderBuilder EncloseMemberNameWith(string opening, string closing)
        {
            ItemNameQuotes = (opening, closing);
            return this;
        }

        /// <summary>
        /// Instructs to place <paramref name="separator"/> symbol between
        /// subsequent formatted members.
        /// </summary>
        /// <param name="separator">Placed between subsequent members.</param>
        /// <returns>Updated <see cref="DefaultFormatProviderBuilder"/> instance.</returns>
        public DefaultFormatProviderBuilder SeparateMembersWith(string separator)
        {
            ItemSeparator = separator;
            return this;
        }

        /// <summary>
        /// Instructs to enclose formatted list of members into pair of <paramref name="opening"/>
        /// and <paramref name="closing"/> symbols.
        /// </summary>
        /// <param name="opening">Placed before first formatted member.</param>
        /// <param name="closing">Placed after lats formatted member.</param>
        /// <returns>Updated <see cref="DefaultFormatProviderBuilder"/> instance.</returns>
        public DefaultFormatProviderBuilder EncloseMemberListWith(string opening, string closing)
        {
            AllMembersQuotes = (opening, closing);
            return this;
        }

        /// <summary>
        /// Builds <see cref="object.ToString"/> function from sequence of formatting operations.
        /// </summary>
        /// <param name="operations">Sequence of formatting member operations.</param>
        /// <returns>Function converting object to string representation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="operations"/> is null.
        /// </exception>
        public Func<object, string> BuildObjectFormatter(IEnumerable<Operation> operations)
        {
            if (operations is null)
                throw new ArgumentNullException(nameof(operations));

            var formatMembers = Enclose(
                AllMembersQuotes,
                operations
                    .Select(AppendObjectMember)
                    .Aggregate(Join<object>(Append(ItemSeparator)))
            );

            var formatObject = AppendClassName() +
                               When(
                                   o => o != null,
                                   formatMembers,
                                   ToFormatter<object>(Append("null"))
                               );

            return MakeToString(formatObject);
        }

        public Func<IEnumerable, string> BuildCollectionFormatter()
        {
            var keyValuePairsFormatter = Enumerate<(int, object)>(
                AppendSequenceKeyAndValue<int, object>(),
                Append(ItemSeparator)
            );

            var seqFormatter = Map<IEnumerable, IEnumerable<(int, object)>>(
                seq => seq.Cast<object>().Select((value, index) => (index, value)),
                keyValuePairsFormatter
            );

            return MakeToString(Enclose(("[", "]"), seqFormatter));
        }

        public Func<IDictionary<K, V>, string> BuildDictionaryFormatter<K, V>()
        {
            var keyValuePairsFormatter = Enumerate<(K, V)>(
                AppendSequenceKeyAndValue<K, V>(),
                Append(ItemSeparator)
            );
            
            var seqFormatter = Map<IDictionary<K, V>, IEnumerable<(K, V)>>(
                dict => dict.Select(kv => (kv.Key, kv.Value)),
                keyValuePairsFormatter
            );
            
            return MakeToString(Enclose(("{", "}"), seqFormatter));
        }

        public Func<IEnumerable, string> BuildSetFormatter()
        {
            var valuesFormatter = Enumerate(
                AppendNewLineBeforeItem<object>() + AppendItemValue<object>(),
                Append(ItemSeparator)
            );

            var setFormatter = Map<IEnumerable, IEnumerable<object>>(
                seq => seq.Cast<object>(),
                valuesFormatter
            );
            
            return MakeToString(Enclose(("{", "}"), setFormatter));
        } 
        
        private Formatter<(K key, V value)> AppendSequenceKeyAndValue<K, V>()
        {
            var appendNameAndValue = Enclose(
                ItemQuotes,
                AppendItemName<(K key, V value)>(kv => kv.key?.ToString()) +
                AppendItemValue<(K key, V value)>(kv => kv.value?.ToString())
            );

            return AppendNewLineBeforeItem<(K, V)>() + appendNameAndValue;
        }

        private Formatter<object> AppendClassName()
        {
            return When(
                Density.HasFlag(IncludeClassName),
                Append<object>(o => o?.GetType().Name)
            );
        }

        private Formatter<object> AppendObjectMember(Operation op)
        {
            var appendNameAndValue = Enclose(
                ItemQuotes,
                AppendItemName<object>(_ => op.Member.MemberName) + AppendItemValue(op.Context)
            );

            var appendMember = When(
                o => o != null,
                AppendNewLineBeforeItem<object>() + appendNameAndValue,
                When(
                    Density.HasFlag(IncludeNullValues),
                    AppendNewLineBeforeItem<object>() + appendNameAndValue
                )
            );

            return Map(op.Member.Getter, appendMember);
        }

        private Formatter<T> AppendNewLineBeforeItem<T>()
        {
            return When<T>(
                Density.HasFlag(ItemOnNewLine),
                (_, sb) => sb.AppendLine()
            );
        }

        private Formatter<T> AppendItemName<T>(Func<T, string> getName)
        {
            return When(
                Density.HasFlag(IncludeItemName),
                Enclose(ItemNameQuotes, Append(getName))
            );
        }

        private Formatter<T> AppendItemValue<T>(Func<T, string> toString = null)
        {
            toString = toString ?? (o => o?.ToString());
            
            return Enclose(ItemValueQuotes, Append<T>(v => v == null ? "null" : toString(v)));
        }
    }
}