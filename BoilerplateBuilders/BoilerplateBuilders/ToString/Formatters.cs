using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BoilerplateBuilders.ToString.Writers;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    /// Function converting object to string according to predefined rules (format). 
    /// </summary>
    /// <param name="value">Instance or value to convert.</param>
    /// <typeparam name="T">Type of converted instance.</typeparam>
    /// <returns>Function writing string representation of object.</returns>
    public delegate Writer Formatter<in T>(T value);
    
    /// <summary>
    /// <see cref="Formatter{T}"/> function combinators.
    /// </summary>
    public static class Formatters
    {
        /// <summary>
        /// Creates formatting function which ignores input value and writes empty string.
        /// </summary>
        /// <typeparam name="T">Formatted value type.</typeparam>
        public static Formatter<T> Empty<T>() => _ => Writers.Empty;
        
        /// <summary>
        /// Creates function ignoring input value and using provided writer function instead.
        /// </summary>
        /// <param name="writer">Writer function.</param>
        /// <typeparam name="T">Formatted value type.</typeparam>
        public static Formatter<T> Lift<T>(Writer writer) => _ => writer;
        
        /// <summary>
        /// Creates formatting function applying provided function to input value.
        /// </summary>
        /// <param name="toString">Function converting formatted value to string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="toString"/> is null.</exception>
        public static Formatter<T> Lift<T>(Func<T, string> toString)
        {
            if (toString == null)
                throw new ArgumentNullException(nameof(toString));

            return t => Write(toString(t));
        }

        /// <summary>
        /// Creates formatting function from another formatter applied to result of a mapper function applied to input. 
        /// </summary>
        /// <typeparam name="TA">Type of resulting function input.</typeparam>
        /// <typeparam name="TB">Type of wrapped formatting function input.</typeparam>
        /// <param name="fmt">Formatting function applied to mapped value.</param>
        /// <param name="map">
        /// Function applied to created function input and producing value passed to wrapped formatter.
        /// </param>
        public static Formatter<TA> Wrap<TA, TB>(Formatter<TB> fmt, Func<TA, TB> map) => a => fmt(map(a));
        
        /// <summary>
        /// Creates formatting function for sequence which applies provided formatter to every sequence item
        /// and joins results with <paramref name="glue"/>.
        /// </summary>
        /// <param name="fmt">Formats individual sequence elements.</param>
        /// <param name="glue">Joins formatter results applied to subsequent items.</param>
        /// <typeparam name="T">Type of collection elements.</typeparam>
        public static Formatter<IEnumerable<T>> Collect<T>(Formatter<T> fmt, Writer glue = null)
        {
            glue = glue ?? Whitespace;

            return seq => seq
                .Select(fmt.Invoke)
                .Aggregate((a, b) => a + glue + b);
        }
    
        /// <summary>
        /// Combines formatter functions by applying them sequentially and summing resulting writer functions.
        /// </summary>
        /// <param name="formatters">Formatter functions to combine.</param>
        /// <typeparam name="T">Type of formatter functions.</typeparam>
        public static Formatter<T> Sum<T>(params Formatter<T>[] formatters) => formatters.Aggregate(Empty<T>(), Add);

        /// <summary>
        /// Combines two formatter functions by applying them sequentially and summing resulting writer functions.
        /// </summary>
        /// <param name="first">Formatter function applied first.</param>
        /// <param name="second">Formatter function applied second.</param>
        /// <typeparam name="T">Type of formatter functions.</typeparam>
        public static Formatter<T> Add<T>(Formatter<T> first, Formatter<T> second) => 
            o => Writers.Empty + first?.Invoke(o) + second?.Invoke(o);
        
        /// <summary>
        /// Converts formatting function to function returning string representation of formatted value accumulated in
        /// <see cref="StringBuilder"/> by formatting function.
        /// </summary>
        /// <param name="formatter">Formatting function.</param>
        /// <returns>Function returning string representation of formatted value.</returns>
        public static Func<T, string> ToString<T>(Formatter<T> formatter) => 
            o => Writers.ToString(formatter?.Invoke(o));

        /// <summary>
        /// Creates formatting function adding prefix and suffix to value produced by another formatting function.
        /// </summary>
        /// <param name="fmt">Wrapped formatter.</param>
        /// <param name="prefixAndSuffix">Prefix and suffix added to formatted value.</param>
        public static Formatter<T> Enclose<T>(Formatter<T> fmt, (string prefix, string suffix) prefixAndSuffix)
        {
            var (prefix, suffix) = prefixAndSuffix;
            return t => Write(prefix) + fmt(t) + Write(suffix);
        }

        /// <summary>
        /// Creates formatting function invoking one of two provided formatting functions
        /// depending on result of <paramref name="condition"/> applied to input value. 
        /// </summary>
        /// <param name="condition">
        /// Condition applied to input value determining which of two formatters to apply.
        /// </param>
        /// <param name="positive">
        /// Formatting function invoked when condition is positive.
        /// </param>
        /// <param name="negative">
        /// Formatting function invoked when condition is negative (default: <see cref="Empty{T}"/>).
        /// </param>
        public static Formatter<T> When<T>(Func<T, bool> condition, Formatter<T> positive, Formatter<T> negative = null)
        {
            return o => (condition(o) ? positive : negative ?? Empty<T>())(o);
        }

        /// <summary>
        /// Creates formatting function applying first formatter when input is not null and second otherwise.
        /// </summary>
        /// <param name="positive">Applied to input when one is not null.</param>
        /// <param name="negative">Applied to null values of input (default: emits "null").</param>
        /// <typeparam name="T">Formatter input type.</typeparam>
        public static Formatter<T> UnlessNull<T>(Formatter<T> positive, Formatter<T> negative = null) => 
            When(obj => obj != null, positive, negative ?? Lift<T>(Write("null")));
    }
}