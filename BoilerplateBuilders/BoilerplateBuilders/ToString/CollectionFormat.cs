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
    public class CollectionFormat
    {
        /// <summary>
        /// Placed between index and value within every such pair. Used only when index included into output.
        /// </summary>
        public string IndexValueSeparator { get; set; }
        
        /// <summary>
        /// Separate subsequent index-value pairs (or subsequent values when index not included into output) within output. 
        /// </summary>
        public string IndexValuePairSeparator { get; set; }

        /// <summary>
        /// Prefix and suffix placed before/after index of each collection item when one is included into output.
        /// Unused unless <see cref="CollectionFormatOptions.IncludeIndex"/> added to <see cref="Options"/>.
        /// </summary>
        public (string, string) IndexPrefixAndSuffix { get; set; }

        /// <summary>
        /// Prefix and suffix placed before/after each collection value.
        /// </summary>
        public (string, string) ValuePrefixAndSuffix { get; set; }

        /// <summary>
        /// Prefix and suffix placed before/after each pair of collection index and value.
        /// </summary>
        public (string, string) IndexValuePairPrefixAndSuffix { get; set; }

        /// <summary>
        /// Prefix and suffix placed before/after all formatted collection items.
        /// </summary>
        public (string, string) CollectionPrefixAndSuffix { get; set; }
        
        /// <summary>
        /// Controls overall structure of formatted output.
        /// </summary>
        public CollectionFormatOptions Options { get; set; }

        /// <summary>
        /// Creates a collection formatter builder with default settings.
        /// </summary>
        public static CollectionFormat CreateDefault()
        {
            return new CollectionFormat()
                .SetOptions(IncludeNullValues)
                .SetIndexValuePairSeparator(", ")
                .SetValuePrefixAndSuffix("'", "'")
                .SetCollectionPrefixAndSuffix("[", "]");
        }
        
        /// <summary>
        /// Sets <see cref="CollectionPrefixAndSuffix"/> and returns updated <see cref="CollectionFormat"/>.
        /// </summary>
        /// <param name="prefix">Placed before all formatted collection values.</param>
        /// <param name="suffix">Placed after all formatted collection values.</param>
        public CollectionFormat SetCollectionPrefixAndSuffix(string prefix, string suffix)
        {
            CollectionPrefixAndSuffix = (prefix, suffix);
            return this;
        }
        
        /// <summary>
        /// Sets <see cref="IndexPrefixAndSuffix"/> and returns updated <see cref="CollectionFormat"/>.
        /// </summary>
        /// <param name="prefix">Placed before collection value index.</param>
        /// <param name="suffix">Placed after collection value index.</param>
        public CollectionFormat SetIndexPrefixAndSuffix(string prefix, string suffix)
        {
            IndexPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Sets <see cref="ValuePrefixAndSuffix"/> and returns updated <see cref="CollectionFormat"/>.
        /// </summary>
        /// <param name="prefix">Placed before every formatted collection value.</param>
        /// <param name="suffix">Placed after every formatted collection value.</param>
        public CollectionFormat SetValuePrefixAndSuffix(string prefix, string suffix)
        {
            ValuePrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Sets <see cref="IndexValuePairPrefixAndSuffix"/> and returns updated <see cref="CollectionFormat"/>.
        /// </summary>
        /// <param name="prefix">Placed before every formatted index-value pair.</param>
        /// <param name="suffix">Placed after every formatted index-value pair.</param>
        public CollectionFormat SetIndexValuePairPrefixAndSuffix(string prefix, string suffix)
        {
            IndexValuePairPrefixAndSuffix = (prefix, suffix);
            return this;
        }
    
        /// <summary>
        /// Sets <see cref="IndexValuePairSeparator"/> and returns updated <see cref="CollectionFormat"/>.
        /// </summary>
        /// <param name="separator"> Separate formatted index-value pairs within output.</param>
        public CollectionFormat SetIndexValuePairSeparator(string separator)
        {
            IndexValuePairSeparator = separator;
            return this;
        }
    
        /// <summary>
        /// Sets <see cref="IndexValueSeparator"/> and returns updated <see cref="CollectionFormat"/>.
        /// Ignored unless <see cref="CollectionFormatOptions.IncludeIndex"/> added to <see cref="Options"/>.
        /// </summary>
        /// <param name="separator">Placed between formatted value index and value.</param>
        public CollectionFormat SetIndexValueSeparator(string separator)
        {
            IndexValueSeparator = separator;
            return this;
        }
        
        /// <summary>
        /// Sets <see cref="Options"/> and returns updated <see cref="CollectionFormat"/>.
        /// </summary>
        /// <param name="options">Determines overall structure of formatted output.</param>
        public CollectionFormat SetOptions(CollectionFormatOptions options)
        {
            Options |= options;
            return this;
        }

        /// <summary>
        /// Builds formatting function converting sequence of arbitrary objects to string. 
        /// </summary>
        public Func<IEnumerable, string> Compile()
        {
            var formatter = Wrap<IEnumerable, IEnumerable<IndexAndValue>>(
                Collect(FormatCollectionItem(), Write(IndexValuePairSeparator)),
                ToIndexValueSequence
            );

            var formatterWithLineBreak = Options.HasFlag(ItemPerLine)
                ? Add(formatter, Lift<IEnumerable>(WriteLineBreak))
                : formatter;
            
            return MakeToString(Enclose(formatterWithLineBreak, CollectionPrefixAndSuffix));
        }

        private static IEnumerable<IndexAndValue> ToIndexValueSequence(IEnumerable seq) =>
            seq.Cast<object>().Select((v, i) => (i, v));
        
        private Formatter<IndexAndValue> FormatCollectionItem()
        {
            var formatIndex = Options.HasFlag(IncludeIndex)
                ? Enclose(Lift<IndexAndValue>(IndexToString), IndexPrefixAndSuffix)
                : Empty<IndexAndValue>();

            var formatValue = Enclose(
                Lift<IndexAndValue>(ValueToString),
                ValuePrefixAndSuffix
            );

            var formatSeparator = Options.HasFlag(IncludeIndex)
                ? Lift<IndexAndValue>(Write(IndexValueSeparator))
                : Empty<IndexAndValue>();

            var lineBreak = Options.HasFlag(ItemPerLine)
                ? Lift<IndexAndValue>(WriteLineBreak)
                : Empty<IndexAndValue>();

            var formatIndexAndValue = Add(
                lineBreak,
                Enclose(
                    Sum(
                        formatIndex,
                        formatSeparator,
                        formatValue
                    ),
                    IndexValuePairPrefixAndSuffix
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