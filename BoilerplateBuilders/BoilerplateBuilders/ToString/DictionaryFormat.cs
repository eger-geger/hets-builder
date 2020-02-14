using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoilerplateBuilders.ToString.Primitives;
using static BoilerplateBuilders.ToString.Primitives.Formatters;
using static BoilerplateBuilders.ToString.Primitives.ToStringFunctions;
using static BoilerplateBuilders.ToString.Primitives.Writers;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    /// Builds <see cref="object.ToString"/> functions for dictionaries.
    /// </summary>
    public class DictionaryFormat
    {
        /// <summary>
        /// Placed between dictionary key and value.
        /// </summary>
        public string KeyValueSeparator { get; set; }

        /// <summary>
        /// Separate subsequent key-value pairs within output. 
        /// </summary>
        public string KeyValuePairSeparator { get; set; }

        /// <summary>
        /// Prefix and suffix placed before/after each dictionary key included into output.
        /// </summary>
        public (string, string) KeyPrefixAndSuffix { get; set; }

        /// <summary>
        /// Prefix and suffix placed before/after each dictionary value.
        /// </summary>
        public (string, string) ValuePrefixAndSuffix { get; set; }

        /// <summary>
        /// Prefix and suffix placed before/after each dictionary key-value pair.
        /// </summary>
        public (string, string) KeyValuePairPrefixAndSuffix { get; set; }

        /// <summary>
        /// Prefix and suffix placed before/after all formatted sequence items.
        /// </summary>
        public (string, string) DictionaryPrefixAndSuffix { get; set; }

        /// <summary>
        /// Controls overall structure of formatted dictionary output.
        /// </summary>
        public DictionaryFormatOptions Options { get; set; }

        /// <summary>
        /// Creates formatter factory with default settings.
        /// </summary>
        public static DictionaryFormat CreateDefault()
        {
            return new DictionaryFormat()
                .SetKeyValueSeparator(":")
                .SetKeyValuePairSeparator(", ")
                .SetValuePrefixAndSuffix("'", "'")
                .SetDictionaryPrefixAndSuffix("{", "}");
        }
        
        /// <summary>
        /// Sets <see cref="KeyValuePairSeparator"/> and returns updated <see cref="DictionaryFormat"/>.
        /// </summary>
        public DictionaryFormat SetKeyValuePairSeparator(string separator)
        {
            KeyValuePairSeparator = separator;
            return this;
        }
        
        /// <summary>
        /// Sets <see cref="KeyValueSeparator"/> and returns updated <see cref="DictionaryFormat"/>.
        /// </summary>
        public DictionaryFormat SetKeyValueSeparator(string separator)
        {
            KeyValueSeparator = separator;
            return this;
        }

        /// <summary>
        /// Sets <see cref="DictionaryPrefixAndSuffix"/> and returns updated <see cref="DictionaryFormat"/>.
        /// </summary>
        public DictionaryFormat SetDictionaryPrefixAndSuffix(string prefix, string suffix)
        {
            DictionaryPrefixAndSuffix = (prefix, suffix);
            return this;
        }
        
        /// <summary>
        /// Sets <see cref="KeyPrefixAndSuffix"/> and returns updated <see cref="DictionaryFormat"/>.
        /// </summary>
        public DictionaryFormat SetKeyPrefixAndSuffix(string prefix, string suffix)
        {
            KeyPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Sets <see cref="ValuePrefixAndSuffix"/> and returns updated <see cref="DictionaryFormat"/>.
        /// </summary>
        public DictionaryFormat SetValuePrefixAndSuffix(string prefix, string suffix)
        {
            ValuePrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Sets <see cref="KeyValuePairPrefixAndSuffix"/> and returns updated <see cref="DictionaryFormat"/>.
        /// </summary>
        public DictionaryFormat SetKeyValuePrefixAndSuffix(string prefix, string suffix)
        {
            KeyValuePairPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Extends current formatting options with provided options and returns updated <see cref="DictionaryFormat"/>.
        /// </summary>
        /// <param name="options">Formatting options added.</param>
        public DictionaryFormat AddOptions(DictionaryFormatOptions options)
        {
            Options |= options;
            return this;
        }

        /// <summary>
        /// Removes given formatting options from current if present and returns updated <see cref="DictionaryFormat"/>.
        /// </summary>
        /// <param name="options">Formatted options removed.</param>
        public DictionaryFormat RemoveOptions(DictionaryFormatOptions options)
        {
            Options &= Options ^ options;
            return this;
        }

        /// <summary>
        /// Creates <see cref="object.ToString"/> function tailored for generic dictionary <see cref="IDictionary"/>.
        /// </summary>
        public Func<IDictionary, string> Compile()
        {
            var formatter = Wrap<IDictionary, IEnumerable<KeyValuePair<object, object>>>(
                MakeDictionaryFormatter<object, object>(),
                ToKeyValuePairSequence
            );

            return MakeToString(formatter);
        }

        private static IEnumerable<KeyValuePair<object, object>> ToKeyValuePairSequence(IDictionary dictionary)
        {
            return dictionary
                .Cast<DictionaryEntry>()
                .Select(entry => new KeyValuePair<object, object>(entry.Key, entry.Value));
        }
        
        /// <summary>
        /// Creates <see cref="object.ToString"/> function tailored for <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of dictionary key.</typeparam>
        /// <typeparam name="TValue">Type of dictionary value.</typeparam>
        public Func<IDictionary<TKey, TValue>, string> Compile<TKey, TValue>()
        {
            return MakeToString(MakeDictionaryFormatter<TKey, TValue>());
        }

        private Formatter<IEnumerable<KeyValuePair<TKey, TValue>>> MakeDictionaryFormatter<TKey, TValue>()
        {
            var formatter = Collect(MakeKeyValuePairFormatter<TKey, TValue>(), Write(KeyValuePairSeparator));

            var formatterWithLineBreak = Options.HasFlag(DictionaryFormatOptions.ItemPerLine)
                ? Add(formatter, Lift<IEnumerable<KeyValuePair<TKey, TValue>>>(WriteLineBreak))
                : formatter;

            return UnlessNull(Enclose(formatterWithLineBreak, DictionaryPrefixAndSuffix));
        }
        
        private Formatter<KeyValuePair<TKey, TValue>> MakeKeyValuePairFormatter<TKey, TValue>()
        {
            var keyFormatter = Wrap<KeyValuePair<TKey, TValue>, TKey>(
                Enclose(Lift<TKey>(ToString<TKey>), KeyPrefixAndSuffix),
                keyValuePair => keyValuePair.Key
            );

            var valueFormatter = Wrap<KeyValuePair<TKey, TValue>, TValue>(
                Enclose(UnlessNull(Lift<TValue>(ToString<TValue>)), ValuePrefixAndSuffix),
                keyValuePair => keyValuePair.Value
            );

            var keyValuePairFormatter = Sum(
                keyFormatter,
                Lift<KeyValuePair<TKey, TValue>>(Write(KeyValueSeparator)),
                valueFormatter
            );

            var enclosedKvpFormatter = Enclose(keyValuePairFormatter, KeyValuePairPrefixAndSuffix);

            return Options.HasFlag(DictionaryFormatOptions.ItemPerLine)
                ? Add(Lift<KeyValuePair<TKey, TValue>>(WriteLineBreak), enclosedKvpFormatter)
                : enclosedKvpFormatter;
        }
    }
}