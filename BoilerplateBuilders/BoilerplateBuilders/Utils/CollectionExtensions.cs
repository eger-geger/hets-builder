using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>
    /// Additions to <see cref="System.Linq.Enumerable"/> extension methods.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Compares content of two sequences ignoring order of elements.
        /// </summary>
        /// <param name="source">Collection of a known size to compare.</param>
        /// <param name="other">Possible lazy evaluated sequence to compare.</param>
        /// <typeparam name="T">Type of collection/sequence elements.</typeparam>
        /// <returns>
        /// 'True' when collections contains exactly same elements and 'False' otherwise. 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> or <paramref name="other"/> is null.
        /// </exception>
        public static bool SequenceEqualIgnoreOrder<T>(
            this ICollection<T> source,
            IEnumerable<T> other
        )
        {
            if(source is null)
                throw new ArgumentNullException(nameof(source));
            
            if(other is null)
                throw new ArgumentNullException(nameof(other));
            
            var copy = source.ToList();
            return other.All(copy.Remove) && copy.Count == 0;
        }
        
        /// <summary>
        /// Computes sequence hashcode by combining hash codes of its' elements.
        /// </summary>
        /// <param name="sequence">Sequence to compute has code for.</param>
        /// <param name="seed">Initial hashcode value.</param>
        /// <param name="step">Value applied to every sequence element.</param>
        /// <returns>Sequence hashcode.</returns>
        public static int GetSequenceHashCode(this IEnumerable sequence, int seed = 0, int step = 397)
        {
            return sequence?.Cast<object>().Aggregate(seed, (hashCode, value) =>
                unchecked(hashCode ^ (value?.GetHashCode() * step ?? 0))
            ) ?? seed;
        }
    }
}