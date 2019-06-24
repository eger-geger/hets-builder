using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Utils;

namespace BoilerplateBuilders.Reflection.Equality
{
    /// <summary>
    /// Contains methods for creating functions testing equality of two objects.
    /// </summary>
    public static class EqualityFunctionFactory
    {
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
        public static EqualityDelegate CreateOrderedSequenceComparer(Type collectionType)
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
        /// for equality using <see cref="CollectionExtensions.SequenceEqualIgnoreOrder{T}"/>.
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
        public static EqualityDelegate CreateUnorderedSequenceComparer(Type collectionType)
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
        public static EqualityDelegate CreateSetComparer(Type collectionType)
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

        private static EqualityDelegate CompileComparerFunction(
            Expression expression,
            params ParameterExpression[] parameters
        )
        {
            var func = Expression.Lambda(expression, parameters).Compile();
            return (x, y) => (bool) func.DynamicInvoke(x, y);
        }
    }
}