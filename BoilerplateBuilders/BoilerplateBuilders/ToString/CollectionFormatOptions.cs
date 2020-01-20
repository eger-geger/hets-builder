using System;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    /// Flags controlling behavior of formatting function created by <see cref="CollectionFormatterFactory"/>.
    /// </summary>
    [Flags]
    public enum CollectionFormatOptions
    {
        /// <summary>
        /// Most basic output.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Output item index in addition to item value.
        /// </summary>
        IncludeItemIndex = 1,
        
        /// <summary>
        /// Output item even if it's value is null.
        /// </summary>
        IncludeNullValues = 2,
        
        /// <summary>
        /// Output line break between sequence items.
        /// </summary>
        IncludeLineBreak = 4
    }
}