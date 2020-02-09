using System;

namespace BoilerplateBuilders.ToString
{
    [Flags]
    public enum DictionaryFormatOptions
    {
        /// <summary>
        /// Most basic output.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Output line break between sequence items.
        /// </summary>
        IncludeLineBreak = 1
    }
}