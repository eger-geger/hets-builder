using System;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    /// Controls overall structure of formatted output.
    /// </summary>
    [Flags]
    public enum DictionaryFormatOptions
    {
        /// <summary>
        /// Most basic output.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Output line break after each formatted key-value pair.
        /// </summary>
        ItemPerLine = 1
    }
}