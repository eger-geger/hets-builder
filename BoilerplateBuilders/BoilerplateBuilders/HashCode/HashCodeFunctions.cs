using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoilerplateBuilders.Reflection;

namespace BoilerplateBuilders.HashCode
{
    /// <summary>
    /// Functions computing hashcode.
    /// </summary>
    public static class HashCodeFunctions
    {
        public static int ComputeCollectionHashCode(object value, int seed, int step)
        {
            switch (value)
            {
                case null:
                    return 0;
                case IEnumerable collection:
                    return collection.GetHashCodeElementWise(seed, step);
                default:
                    throw new ArgumentException($"{value.GetType()} is not {typeof(ICollection)}.");
            }
        }

        /// <summary>
        /// Constructs function computing hash code of an object by combining hashcode of its' members.
        /// </summary>
        /// <param name="operations">Sequence of objects' members hashcode operations.</param>
        /// <param name="seed">Initial hashcode value.</param>
        /// <param name="step">Value applied to every members' hashcode.</param>
        /// <typeparam name="TTarget">Type of object to compute hashcode for.</typeparam>
        /// <returns>Function computing hashcode for object of type <typeparamref name="TTarget"/>.</returns>
        public static Func<TTarget, int> BuildHashCodeFunction<TTarget>(
            IEnumerable<MemberContext<Func<object, int>>> operations,
            int seed,
            int step
        )
        {
            return target =>
            {
                if(target == null)
                    throw new ArgumentNullException(nameof(target));
                
                return operations
                    .Select(op => op.Context(op.Member.Getter(target)))
                    .GetHashCodeElementWise(seed, step);
            };
        }
    }
}