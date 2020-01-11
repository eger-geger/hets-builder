using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static BoilerplateBuilders.ToString.Writers;
using static BoilerplateBuilders.ToString.Formatters;
using static BoilerplateBuilders.ToString.CollectionFormatOptions;

namespace BoilerplateBuilders.ToString
{
    public class CollectionFormatterFactory
    {
        public string ItemSeparator { get; private set; }

        public (string, string) ItemIndexPrefixAndSuffix { get; private set; }

        public (string, string) ItemValuePrefixAndSuffix { get; private set; }

        public (string, string) ItemPrefixAndSuffix { get; private set; }
        
        public string ItemIndexValueSeparator { get; private set; }
        
        public CollectionFormatOptions Options { get; private set; }

        /// <summary>
        /// Builds formatting function converting sequence of arbitrary objects to string. 
        /// </summary>
        public Func<IEnumerable, string> CreateToStringFunction()
        {
            var keyValuePairsFormatter = Collect<(int, object)>(
                AppendSequenceKeyAndValue<int, object>(),
                Write(ItemSeparator)
            );

            var seqFormatter = Wrap<IEnumerable, IEnumerable<(int, object)>>(keyValuePairsFormatter,
                seq => seq.Cast<object>().Select((value, index) => (index, value)));

            return Formatters.ToString(Enclose(seqFormatter, ("[", "]")));
        }

        private Formatter<(TK key, TV value)> AppendSequenceKeyAndValue<TK, TV>() =>
            FormatItem<(TK key, TV value)>(
                kv => kv.key?.ToString(),
                kv => kv.value?.ToString()
            );

        private Formatter<T> FormatItem<T>(Func<T, string> getName, Func<T, string> getValue)
        {
            var formatIndex = Options.HasFlag(IncludeItemIndex)
                ? Enclose(Lift(getName), ItemIndexPrefixAndSuffix)
                : Empty<T>();

            var formatValue = Enclose(
                Lift<T>(o => getValue(o)?.ToString()),
                ItemValuePrefixAndSuffix
            );

            var formatSeparator = Options.HasFlag(IncludeItemIndex)
                ? Lift<T>(Write(ItemIndexValueSeparator))
                : Empty<T>();

            var lineBreak = Options.HasFlag(ItemLineBreak)
                ? Lift<T>(NewLine)
                : Empty<T>();
            
            var formatIndexAndValue = Add(
                lineBreak,
                Enclose(
                    Sum(
                        formatIndex,
                        formatSeparator,
                        formatValue
                    ),
                    ItemPrefixAndSuffix
                )
            );

            return When(
                o => getValue(o) != null,
                formatIndexAndValue,
                Options.HasFlag(IncludeNullValues)
                    ? formatIndexAndValue
                    : Empty<T>()
            );
        }
    }
}