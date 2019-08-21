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
        public static bool SequenceEqualElementWise(object a, object b)
        {
            if(!(a is IEnumerable ae))
                throw new ArgumentException("Not an enumerable", nameof(a));
                
            if(!(b is IEnumerable be))
                throw new ArgumentException("Not an enumerable", nameof(b));

            return ReferenceEquals(a, b) || ae.Cast<object>().SequenceEqual(be.Cast<object>());
        }

        public static bool SequenceEqualIgnoreOrder(object a, object b)
        {
            if(!(a is ICollection ac))
                throw new ArgumentException("Not a collection.", nameof(a));
            
            if(!(b is ICollection bc))
                throw new ArgumentException("Not a collection.", nameof(b));

            return ReferenceEquals(ac, bc) || ac.SequenceEqualIgnoreOrder(bc);
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