using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.Reflection.HashCode;
using BoilerplateBuilders.Utils;

namespace BoilerplateBuilders
{
    /// <summary>
    /// Creates function computing hashcode using hash codes of configured members.
    /// </summary>
    /// <typeparam name="TTarget">Type of objects to compute hashcode for.</typeparam>
    public class HashCodeBuilder<TTarget> : AbstractBuilder<TTarget, HashCodeBuilder<TTarget>, GetHashCodeDelegate>
    {
        private readonly int _seed;

        private readonly int _step;
        
        /// <summary>
        /// Initializes new builder instance with provided or default values.
        /// </summary>
        /// <param name="seed">Initial hashcode value.</param>
        /// <param name="step">Value applied to every hashcode member.</param>
        public HashCodeBuilder(int seed = 0, int step = 397)
        {
            _seed = seed;
            _step = step;
        }

        /// <summary>
        /// Include field or property into hashcode computation
        /// using provided function for computing hashcode of its' value.
        /// </summary>
        /// <param name="expression">Points to field or property to be used for computing object hashcode.</param>
        /// <param name="computeHashCode">Function computing hashcode for chosen member value.</param>
        /// <typeparam name="TMember">Type of chosen field or property value.</typeparam>
        /// <returns>Updated builder instance.</returns>
        public HashCodeBuilder<TTarget> Append<TMember>(
            Expression<Func<TTarget, TMember>> expression,
            Func<TMember, int> computeHashCode
        )
        {
            return AppendExplicitMemberFunction(expression, new GetHashCodeDelegate(computeHashCode));
        }
        
        
        /// <summary>
        /// Instructs builder to use supplied function for computing hashcode of target object member values
        /// which are assignable to given type.
        /// </summary>
        /// <param name="computeHashCode">Computes hashcode.</param>
        /// <typeparam name="T">Type of interest.</typeparam>
        /// <returns>Updated builder instance.</returns>
        public HashCodeBuilder<TTarget> OverrideHashCodeFor<T>(Func<T, int> computeHashCode)
        {
            return AppendExplicitTypeFunction(typeof(T), new GetHashCodeDelegate(computeHashCode));
        }
        
        protected override GetHashCodeDelegate GetDefaultFunction(BuilderMember member)
        {
            if (member.MemberType.IsAssignableToEnumerable())
            {
                return o => (o as IEnumerable).GetItemWiseHashCode(_seed, _step);
            }

            return o => o?.GetHashCode() ?? 0;
        }
        
        /// <summary>
        /// Builds function computing hashcode of given object.
        /// </summary>
        public HashCodeFunction<TTarget> Build()
        {
            return new HashCodeFunction<TTarget>(BuildFinalFunctionSet(), _seed, _step);
        }
        
    }
}