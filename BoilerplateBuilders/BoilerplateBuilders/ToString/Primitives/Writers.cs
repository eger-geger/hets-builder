using System.Linq;
using System.Text;

namespace BoilerplateBuilders.ToString.Primitives
{
    /// <summary>
    /// Function which writes something to given <paramref name="output"/>.  
    /// </summary>
    /// <param name="output">Modified builder instance.</param>
    public delegate void Writer(StringBuilder output);
    
    /// <summary>
    /// <see cref="Writer"/> function combinators.
    /// </summary>
    public static class Writers
    {
        /// <summary>
        /// Creates a function writing empty string to output.
        /// </summary>
        public static Writer Empty => _ => { };

        /// <summary>
        /// Combines two writers by invoking them sequentially.
        /// </summary>
        /// <param name="first">Writer applied first.</param>
        /// <param name="second">Writer applied second.</param>
        public static Writer Add(Writer first, Writer second) => IsEmpty(second) ? first : first + second;

        /// <summary>
        /// Combines multiple writers by invoking them sequentially.
        /// </summary>
        /// <param name="writers">Writers to add.</param>
        public static Writer Sum(params Writer[] writers) => writers.Aggregate(Add);
        
        /// <summary>
        /// Determines if a writer is empty (<see cref="Empty"/>).
        /// </summary>
        /// <param name="wr">Writer function to check.</param>
        public static bool IsEmpty(Writer wr) => Empty == wr || wr == null;
        
        /// <summary>
        /// Creates function writing a line break to output.
        /// </summary>
        public static Writer WriteLineBreak => sb => sb.AppendLine();
        
        /// <summary>
        /// Creates function writing a whitespace to output.
        /// </summary>
        public static Writer WriteWhitespace => sb => sb.Append(" ");
        
        /// <summary>
        /// Creates a function writing given string to output unless it is null or empty. 
        /// </summary>
        /// <param name="s">String to write to output.</param>
        public static Writer Write(string s) => string.IsNullOrEmpty(s) ? Empty : sb => sb.Append(s);
        
        /// <summary>
        /// Executes writer.
        /// </summary>
        /// <param name="writer">Writer to execute.</param>
        /// <returns>Writer output or empty string when writer was null.</returns>
        public static string ToString(Writer writer)
        {
            var sb = new StringBuilder();
            writer?.Invoke(sb);
            return sb.ToString();
        }
    }
}