using System;
using System.Collections.Generic;
using BoilerplateBuilders.ToString.Primitives;
using static BoilerplateBuilders.ToString.Primitives.Formatters;
using static BoilerplateBuilders.ToString.Primitives.ToStringFunctions;
using static BoilerplateBuilders.ToString.Primitives.Writers;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    /// Builds <see cref="object.ToString"/> functions for dictionaries.
    /// </summary>
    public class DictionaryFormatterFactory
    {
        /// <summary>
        /// Placed between dictionary key and value.
        /// </summary>
        public string KeyValueSeparator { get; private set; }
        
        /// <summary>
        /// Separate subsequent key-value pairs within output. 
        /// </summary>
        public string KeyValuePairSeparator { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after each dictionary key included into output.
        /// </summary>
        public (string, string) KeyPrefixAndSuffix { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after each dictionary value.
        /// </summary>
        public (string, string) ValuePrefixAndSuffix { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after each dictionary key-value pair.
        /// </summary>
        public (string, string) KeyValuePairPrefixAndSuffix { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after all formatted sequence items.
        /// </summary>
        public (string, string) DictionaryPrefixAndSuffix { get; private set; }

        /// <summary>
        /// Controls overall structure of formatted dictionary output.
        /// </summary>
        public DictionaryFormatOptions Options { get; private set; }

        /// <summary>
        /// Sets <see cref="KeyValuePairSeparator"/> and returns updated <see cref="DictionaryFormatterFactory"/>.
        /// </summary>
        public DictionaryFormatterFactory SetKeyValuePairSeparator(string separator)
        {
            KeyValuePairSeparator = separator;
            return this;
        }
        
        /// <summary>
        /// Sets <see cref="KeyValueSeparator"/> and returns updated <see cref="DictionaryFormatterFactory"/>.
        /// </summary>
        public DictionaryFormatterFactory SetKeyValueSeparator(string separator)
        {
            KeyValueSeparator = separator;
            return this;
        }

        /// <summary>
        /// Sets <see cref="DictionaryPrefixAndSuffix"/> and returns updated <see cref="DictionaryFormatterFactory"/>.
        /// </summary>
        public DictionaryFormatterFactory SetDictionaryPrefixAndSuffix(string prefix, string suffix)
        {
            DictionaryPrefixAndSuffix = (prefix, suffix);
            return this;
        }
        
        /// <summary>
        /// Sets <see cref="KeyPrefixAndSuffix"/> and returns updated <see cref="DictionaryFormatterFactory"/>.
        /// </summary>
        public DictionaryFormatterFactory SetKeyPrefixAndSuffix(string prefix, string suffix)
        {
            KeyPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Sets <see cref="ValuePrefixAndSuffix"/> and returns updated <see cref="DictionaryFormatterFactory"/>.
        /// </summary>
        public DictionaryFormatterFactory SetValuePrefixAndSuffix(string prefix, string suffix)
        {
            ValuePrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Sets <see cref="KeyValuePairPrefixAndSuffix"/> and returns updated <see cref="DictionaryFormatterFactory"/>.
        /// </summary>
        public DictionaryFormatterFactory SetKeyValuePrefixAndSuffix(string prefix, string suffix)
        {
            KeyValuePairPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        /// <summary>
        /// Extends current formatting options with provided options and returns updated <see cref="DictionaryFormatterFactory"/>.
        /// </summary>
        /// <param name="options">Formatting options added.</param>
        public DictionaryFormatterFactory AddOptions(DictionaryFormatOptions options)
        {
            Options |= options;
            return this;
        }

        /// <summary>
        /// Removes given formatting options from current if present and returns updated <see cref="DictionaryFormatterFactory"/>.
        /// </summary>
        /// <param name="options">Formatted options removed.</param>
        public DictionaryFormatterFactory RemoveOptions(DictionaryFormatOptions options)
        {
            Options &= Options ^ options;
            return this;
        }

        /// <summary>
        /// Creates <see cref="object.ToString"/> function tailored for <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of dictionary key.</typeparam>
        /// <typeparam name="TValue">Type of dictionary value.</typeparam>
        public Func<IDictionary<TKey, TValue>, string> CreateToString<TKey, TValue>()
        {
            var formatter = Wrap<IDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>>(
                Collect(MakeKeyValuePairFormatter<TKey, TValue>(), Write(KeyValuePairSeparator)),
                dictionary => dictionary
            );

            return MakeToString(UnlessNull(Enclose(formatter, DictionaryPrefixAndSuffix)));
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

            return Options.HasFlag(DictionaryFormatOptions.IncludeLineBreak)
                ? Add(Lift<KeyValuePair<TKey, TValue>>(WriteNewLine), enclosedKvpFormatter)
                : enclosedKvpFormatter;
        }
    }
}