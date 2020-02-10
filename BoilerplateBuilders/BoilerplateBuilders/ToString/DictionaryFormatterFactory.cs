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
    /// Builds <see cref="object.ToString"/> for <see cref="IDictionary"/>
    /// </summary>
    //TODO 
    public class DictionaryFormatterFactory
    {
        /// <summary>
        /// Separate subsequent key-value pairs within output. 
        /// </summary>
        public string KeyValuePairSeparator { get; private set; }

        /// <summary>
        /// Prefix and suffix placed before/after all formatted sequence items.
        /// </summary>
        public (string, string) DictionaryPrefixAndSuffix { get; private set; }

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
        public (string, string) KeyValuePrefixAndSuffix { get; private set; }

        /// <summary>
        /// Placed between dictionary key and value.
        /// </summary>
        public string KeyValueSeparator { get; private set; }

        /// <summary>
        /// Controls overall structure of formatted dictionary output.
        /// </summary>
        public DictionaryFormatOptions Options { get; private set; }

        public DictionaryFormatterFactory SetKeyValuePairSeparator(string separator)
        {
            KeyValuePairSeparator = separator;
            return this;
        }

        public DictionaryFormatterFactory SetKeyValueSeparator(string separator)
        {
            KeyValueSeparator = separator;
            return this;
        }

        public DictionaryFormatterFactory SetDictionaryPrefixAndSuffix(string prefix, string suffix)
        {
            DictionaryPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        public DictionaryFormatterFactory SetKeyPrefixAndSuffix(string prefix, string suffix)
        {
            KeyPrefixAndSuffix = (prefix, suffix);
            return this;
        }

        public DictionaryFormatterFactory SetValuePrefixAndSuffix(string prefix, string suffix)
        {
            ValuePrefixAndSuffix = (prefix, suffix);
            return this;
        }

        public DictionaryFormatterFactory SetKeyValuePrefixAndSuffix(string prefix, string suffix)
        {
            KeyValuePrefixAndSuffix = (prefix, suffix);
            return this;
        }

        public DictionaryFormatterFactory AddOptions(DictionaryFormatOptions options)
        {
            Options |= options;
            return this;
        }

        public DictionaryFormatterFactory RemoveOptions(DictionaryFormatOptions options)
        {
            Options &= Options ^ options;
            return this;
        }

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

            return Enclose(keyValuePairFormatter, KeyValuePrefixAndSuffix);
        }
    }
}