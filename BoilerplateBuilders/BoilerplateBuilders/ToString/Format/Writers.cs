using System.Text;

namespace BoilerplateBuilders.ToString.Format
{
    /// <summary>
    /// Function which writes something to given <paramref name="stringBuilder"/>.  
    /// </summary>
    /// <param name="stringBuilder">Modified builder instance.</param>
    public delegate void Writer(StringBuilder stringBuilder);
    
    public static class Writers
    {
        public static Writer Empty => _ => { };

        /// <summary>
        /// Creates formatting function ignoring formatted value and appending provided constant at current position. 
        /// </summary>
        /// <param name="s">Constant appended to <see cref="StringBuilder"/>.</param>
        /// <returns>
        /// Formatting function appending constant value when invoked or do nothing function if value is null or empty.
        /// </returns>
        public static Writer Write(string s) => string.IsNullOrEmpty(s) ? Empty : sb => sb.Append(s);

        public static Writer NewLine() => sb => sb.AppendLine();
    }
}