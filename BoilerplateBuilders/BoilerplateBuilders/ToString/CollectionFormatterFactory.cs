using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoilerplateBuilders.ToString.Primitives;
using static BoilerplateBuilders.ToString.Primitives.Writers;
using static BoilerplateBuilders.ToString.Primitives.Formatters;
using static BoilerplateBuilders.ToString.CollectionFormatOptions;
using IndexAndValue = System.ValueTuple<int, object>;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    /// Builds <see cref="object.ToString"/> function accepting <see cref="IEnumerable{T}"/>.
    /// </summary>
    public class CollectionFormatterFactory
    {
        /// <summary>
        /// Separate subsequent index-value pairs (or subsequent values when index not included into output) within output. 
        /// </summary>
        public string ItemSeparator { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after all formatted sequence items.
        /// </summary>
        public (string, string) SequencePrefixAndSuffix { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after index of each sequence item when one is included into output.
        /// </summary>
        public (string, string) IndexPrefixAndSuffix { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after each sequence value.
        /// </summary>
        public (string, string) ValuePrefixAndSuffix { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after each pair of index and value (or just value when index not included into output).
        /// </summary>
        public (string, string) ItemPrefixAndSuffix { get; private set; }

        /// <summary>
        /// Placed between index and value within every such pair. Used only when index included into output.
        /// </summary>
        public string IndexValueSeparator { get; private set; }

        /// <summary>
        /// Controls overall structure of formatted output.
        /// </summary>
        public CollectionFormatOptions Options { get; private set; }

        /// <summary>
        /// Sets <see cref="SequencePrefixAndSuffix"/> and returns updated factory instance.
        /// </summary>
        /// <param name="prefix">Placed before formatted sequence items.</param>
        /// <param name="suffix">Placed after formatted sequence items.</param>
        public CollectionFormatterFactory SetSequencePrefixAndSuffix(string prefix, string suffix)
        {
            SequencePrefixAndSuffix = (prefix, suffix);
            return this;
        }
        
        /// <summary>
        /// Sets <see cref="IndexPrefixAndSuffix"/> and returns updated factory instance.
        /// </summary>
        /// <param name="prefix">Placed before every formatted sequence item index.</param>
        /// <param name="suffix">Placed after every formatted sequence item index.</param>
        public CollectionFormatterFactory SetIndexPrefixAndSuffix(string prefix, string suffix)
        {
            IndexPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Sets <see cref="ValuePrefixAndSuffix"/> and returns updated factory instance.
        /// </summary>
        /// <param name="prefix">Placed before every formatted sequence value.</param>
        /// <param name="suffix">Placed after every formatted sequence value.</param>
        public CollectionFormatterFactory SetValuePrefixAndSuffix(string prefix, string suffix)
        {
            ValuePrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Sets <see cref="ItemPrefixAndSuffix"/> and returns updated factory instance.
        /// </summary>
        /// <param name="prefix">Placed before every formatted index-value pair of sequence items.</param>
        /// <param name="suffix">Placed after every formatted index-value pair of sequence items.</param>
        public CollectionFormatterFactory SetItemPrefixAndSuffix(string prefix, string suffix)
        {
            ItemPrefixAndSuffix = (prefix, suffix);
            return this;
        }
    
        /// <summary>
        /// Sets <see cref="ItemSeparator"/> and returns updated factory instance.
        /// </summary>
        /// <param name="separator">
        /// Separate subsequent index-value pairs (or subsequent values when index not included into output) within output.
        /// </param>
        public CollectionFormatterFactory SetItemSeparator(string separator)
        {
            ItemSeparator = separator;
            return this;
        }
    
        /// <summary>
        /// Sets <see cref="IndexValueSeparator"/> and returns updated factory instance.
        /// </summary>
        /// <param name="separator">
        /// Placed between index and value within every such pair.
        /// Applies only when <see cref="CollectionFormatOptions.IncludeItemIndex"/> added to <see cref="Options"/>.
        /// </param>
        public CollectionFormatterFactory SetIndexValueSeparator(string separator)
        {
            IndexValueSeparator = separator;
            return this;
        }
        
        /// <summary>
        /// Sets <see cref="Options"/> and returns updated factory instance.
        /// </summary>
        /// <param name="options">Determines overall structure of formatted output.</param>
        public CollectionFormatterFactory SetOptions(CollectionFormatOptions options)
        {
            Options |= options;
            return this;
        }

        /// <summary>
        /// Builds formatting function converting sequence of arbitrary objects to string. 
        /// </summary>
        public Func<IEnumerable, string> CreateToStringFunction()
        {
            var seqFormatter = Wrap<IEnumerable, IEnumerable<IndexAndValue>>(
                Collect(FormatCollectionItem(), Write(ItemSeparator)),
                ToIndexValueSequence
            );

            seqFormatter = Options.HasFlag(IncludeLineBreak)
                ? Add(seqFormatter, Lift<IEnumerable>(WriteNewLine))
                : seqFormatter;
            
            return Formatters.MakeToString(Enclose(seqFormatter, SequencePrefixAndSuffix));
        }

        private static IEnumerable<IndexAndValue> ToIndexValueSequence(IEnumerable seq) =>
            seq.Cast<object>().Select((v, i) => (i, v));
        
        private Formatter<IndexAndValue> FormatCollectionItem()
        {
            var formatIndex = Options.HasFlag(IncludeItemIndex)
                ? Enclose(Lift<IndexAndValue>(IndexToString), IndexPrefixAndSuffix)
                : Empty<IndexAndValue>();

            var formatValue = Enclose(
                Lift<IndexAndValue>(ValueToString),
                ValuePrefixAndSuffix
            );

            var formatSeparator = Options.HasFlag(IncludeItemIndex)
                ? Lift<IndexAndValue>(Write(IndexValueSeparator))
                : Empty<IndexAndValue>();

            var lineBreak = Options.HasFlag(IncludeLineBreak)
                ? Lift<IndexAndValue>(WriteNewLine)
                : Empty<IndexAndValue>();

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
                o => o.Item2 != null,
                formatIndexAndValue,
                Options.HasFlag(IncludeNullValues)
                    ? formatIndexAndValue
                    : Empty<IndexAndValue>()
            );
        }

        private static string IndexToString(IndexAndValue iv) => iv.Item1.ToString("D");

        private static string ValueToString(IndexAndValue iv) => iv.Item2?.ToString();
    }
}