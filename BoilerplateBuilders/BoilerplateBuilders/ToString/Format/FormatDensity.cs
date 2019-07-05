using System;

namespace BoilerplateBuilders.ToString.Format
{
    /// <summary>
    /// Determine how compact string representation should be.
    /// </summary>
    [Flags]
    public enum FormatDensity
    {
        /// <summary>
        /// Place every formatted member on new line.
        /// </summary>
        MemberOnNewLine = 0x1,
        
        /// <summary>
        /// Include member name into output.
        /// </summary>
        IncludeMemberName = 0x2,
        
        /// <summary>
        /// Include formatted object class name into output.
        /// </summary>
        IncludeClassName = 0x4,
        
        /// <summary>
        /// Include members with <code>null</code> values into output.
        /// </summary>
        IncludeNullValues = 0X8
    }
}