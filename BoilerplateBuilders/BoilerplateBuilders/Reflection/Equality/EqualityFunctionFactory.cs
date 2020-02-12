using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Utils;
using EqualityFunc = System.Func<object, object, bool>;

namespace BoilerplateBuilders.Reflection.Equality
{
    /// <summary>
    /// Contains methods for creating functions testing equality of two objects.
    /// </summary>
    public static class EqualityFunctionFactory
    {
        /// <summary>
        /// Determines if two sequences contain same set of elements with same order.
        /// </summary>
        /// <param name="first">First compared sequence.</param>
        /// <param name="second">Second compared sequence.</param>
        /// <returns>Flag indicating whether sequences are equal.</returns>
        /// <exception cref="ArgumentException">
        /// Either <paramref name="first"/> or <paramref name="second"/> object is not a sequence
        /// (does not implement <see cref="IEnumerable"/> interface).
        /// </exception>
        public static bool SequenceEqualElementWise(object first, object second)
        {
            if(!(first is IEnumerable ae))
                throw new ArgumentException("Not an enumerable", nameof(first));
                
            if(!(second is IEnumerable be))
                throw new ArgumentException("Not an enumerable", nameof(second));

            return ReferenceEquals(first, second) || ae.Cast<object>().SequenceEqual(be.Cast<object>());
        }

        /// <summary>
        /// Determines if two given collections contains same set of elements ignoring order.
        /// </summary>
        /// <param name="first">First compared collection.</param>
        /// <param name="second">Second compared collection.</param>
        /// <returns>Flag indicating whether collections are equal.</returns>
        /// <exception cref="ArgumentException">
        /// Either <paramref name="first"/> or <paramref name="second"/> object is not a collection
        /// (does not implement <see cref="ICollection"/> interface).
        /// </exception>
        public static bool SequenceEqualIgnoreOrder(object first, object second)
        {
            if(!(first is ICollection fc))
                throw new ArgumentException("Not a collection.", nameof(first));
            
            if(!(second is ICollection sc))
                throw new ArgumentException("Not a collection.", nameof(second));

            return ReferenceEquals(fc, sc) || fc.SequenceEqualIgnoreOrder(sc);
        }
        
        
        /// <summary>
        /// Creates function comparing two sequences (instances of <see cref="IEnumerable{T}"/>)
        /// for equality using <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource},IEnumerable{TSource})"/>.
        /// </summary>
        /// <param name="collectionType">
        /// Compared sequence type implementing <see cref="IEnumerable{T}"/> or <see cref="IEnumerable{T}"/> itself.
        /// </param>
        /// <returns>Function comparing two sequences of type <paramref name="collectionType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionType"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collectionType"/> does not implement <see cref="IEnumerable{T}"/>.
        /// </exception>
        public static EqualityFunc CreateOrderedSequenceComparer(Type collectionType)
        {
            if(collectionType is null)
                throw new ArgumentNullException(nameof(collectionType));
            
            Type seqType;
            
            try
            {
                seqType = collectionType.GetImplementedGenericInterfaceType(typeof(IEnumerable<>));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(
                    $"Does not implement {typeof(IEnumerable<>)}.", 
                    nameof(collectionType), 
                    ex
                );
            }

            var paramX = Expression.Parameter(seqType);
            var paramY = Expression.Parameter(seqType);

            return CompileComparerFunction(
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.SequenceEqual),
                    seqType.GenericTypeArguments,
                    paramX, paramY
                ), 
                paramX, paramY
            );
        }
    
        /// <summary>
        /// Creates function comparing two sequences (instances of <see cref="IEnumerable{T}"/>)
        /// for equality using <see cref="CollectionExtensions.SequenceEqualIgnoreOrder"/>.
        /// </summary>
        /// <param name="collectionType">
        /// Compared sequence type implementing <see cref="IEnumerable{T}"/> or <see cref="IEnumerable{T}"/> itself.
        /// </param>
        /// <returns>Function comparing two sequences of type <paramref name="collectionType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionType"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collectionType"/> does not implement <see cref="IEnumerable{T}"/>.
        /// </exception>
        public static EqualityFunc CreateUnorderedSequenceComparer(Type collectionType)
        {
            if(collectionType is null)
                throw new ArgumentNullException(nameof(collectionType));
            
            Type seqType;
            
            try
            {
                seqType = collectionType.GetImplementedGenericInterfaceType(typeof(IEnumerable<>));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(
                    $"Does not implement {typeof(IEnumerable<>)}.", 
                    nameof(collectionType), 
                    ex
                );
            }
                

            var paramX = Expression.Parameter(seqType);
            var paramY = Expression.Parameter(seqType);

            return CompileComparerFunction(
                Expression.Call(
                    typeof(CollectionExtensions),
                    nameof(CollectionExtensions.SequenceEqualIgnoreOrder),
                    seqType.GenericTypeArguments,
                    paramX, paramY
                ), 
                paramX, paramY
            );
        }
        
        /// <summary>
        /// Creates function comparing two sets for equality using <see cref="ISet{T}.SetEquals"/>.
        /// </summary>
        /// <param name="collectionType">Concrete type of sets to compare.</param>
        /// <returns>Function comparing two sets for equality.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionType"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collectionType"/> does not implement <see cref="ISet{T}"/>.
        /// </exception>
        public static EqualityFunc CreateSetComparer(Type collectionType)
        {
            if(collectionType is null)
                throw new ArgumentNullException(nameof(collectionType));
            
            
            Type setType;

            try
            {
                setType = collectionType.GetImplementedGenericInterfaceType(typeof(ISet<>));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(
                    $"Does not implement {typeof(ISet<>)}",
                    nameof(collectionType),
                    ex
                );
            }

            var paramX = Expression.Parameter(setType);
            var paramY = Expression.Parameter(setType);

            return CompileComparerFunction(
                Expression.Call(paramX, nameof(ISet<object>.SetEquals), null, paramY), 
                paramX, paramY
            );
        }

        private static EqualityFunc CompileComparerFunction(
            Expression expression,
            params ParameterExpression[] parameters
        )
        {
            var func = Expression.Lambda(expression, parameters).Compile();
            return (x, y) => (bool) func.DynamicInvoke(x, y);
        }
    }
}