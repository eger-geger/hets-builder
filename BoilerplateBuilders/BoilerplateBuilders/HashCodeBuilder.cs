using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.Reflection.HashCode;
using BoilerplateBuilders.Utils;
using HashCodeFunc = System.Func<object, int>;

namespace BoilerplateBuilders
{
    /// <summary>
    /// Creates function computing hashcode using hash codes of configured members.
    /// </summary>
    /// <typeparam name="TTarget">Type of objects to compute hashcode for.</typeparam>
    public class HashCodeBuilder<TTarget> : AbstractBuilder<TTarget, HashCodeBuilder<TTarget>, HashCodeFunc>
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
            return AppendExplicit(expression, computeHashCode.ToGeneric<TMember, int, object, int>());
        }
        
        
        /// <summary>
        /// Instructs builder to use supplied function for computing hashcode of members' values
        /// which are assignable to given type.
        /// </summary>
        /// <param name="computeHashCode">Function computing hashcode.</param>
        /// <typeparam name="T">Type accepted by <paramref name="computeHashCode"/> function.</typeparam>
        /// <returns>Updated builder instance.</returns>
        public HashCodeBuilder<TTarget> Use<T>(Func<T, int> computeHashCode)
        {
            return OverrideContextForType(typeof(T), computeHashCode.ToGeneric<T, int, object, int>());
        }
        
        /// <summary>
        /// Selects function computing hashcode for given member based on member type.
        /// </summary>
        /// <param name="member">Field or property to compute hashcode for.</param>
        /// <returns>Function computing hashcode for value of given member.</returns>
        /// <remarks>
        /// It has special treatment for sequences (<see cref="IEnumerable"/>) by computing
        /// hashcode element-wise. It uses objects' hashcode function in other cases.
        /// Either way returned function handles null values correctly.
        /// </remarks>
        protected override HashCodeFunc GetImplicitContext(SelectedMember member)
        {
            if (member.MemberType.IsCollection())
            {
                return o => (o as IEnumerable).GetSequenceHashCode(_seed, _step);
            }

            return o => o?.GetHashCode() ?? 0;
        }
        
        /// <summary>
        /// Builds function computing hashcode of given object.
        /// </summary>
        public Func<TTarget, int> Build()
        {
            return new HashCodeFunction<TTarget>(GetMemberContexts(), _seed, _step).GetHashCode;
        }
        
    }
}