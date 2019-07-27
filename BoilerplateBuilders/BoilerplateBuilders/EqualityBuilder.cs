using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.Reflection.Equality;
using BoilerplateBuilders.Utils;
using static BoilerplateBuilders.Reflection.Equality.EqualityFunctionFactory;
using EqualityFunc = System.Func<object, object, bool>;

namespace BoilerplateBuilders
{
    /// <summary>
    /// Builds function comparing two object of <typeparamref name="TTarget"/> type for equality.
    /// </summary>
    /// <typeparam name="TTarget">Type of object being compared.</typeparam>
    public class EqualityBuilder<TTarget> : AbstractBuilder<TTarget, EqualityBuilder<TTarget>, EqualityFunc>
    {
        /// <summary>
        /// Determines how collections should be compared by default.
        /// </summary>
        public SequenceComparisonMode SequenceMode { get; private set; } = SequenceComparisonMode.SameOrder;
        
        /// <summary>
        /// Sets <see cref="SequenceMode"/> which defines how collections should be compared by default.
        /// </summary>
        /// <returns>Current equality builder instance.</returns>
        public EqualityBuilder<TTarget> WithSequenceComparisonMode(SequenceComparisonMode mode)
        {
            SequenceMode = mode;
            return this;
        }

        /// <summary>
        /// Adds field or property to use for object comparison using given function to compare it's values.
        /// Overrides any previous or default value. 
        /// </summary>
        /// <param name="expression">Points to field or property being used for object comparison.</param>
        /// <param name="comparisonFunc">Function to use for comparing selected field or property values.</param>
        /// <typeparam name="TMember">Type of chosen field or property value.</typeparam>
        /// <returns>Current equality builder instance.</returns>
        public EqualityBuilder<TTarget> Append<TMember>(
            Expression<Func<TTarget, TMember>> expression,
            Func<TMember, TMember, bool> comparisonFunc
        )
        {
            return AppendExplicit(expression, comparisonFunc.ToGeneric<TMember, object>());
        }

        /// <summary>
        /// Sets equality comparison function for all object assignable to given type.
        /// Overrides any previous or default value.
        /// </summary>
        /// <param name="comparer">Function comparing objects or given type for equality.</param>
        /// <typeparam name="T">Type of compared objects.</typeparam>
        /// <returns></returns>
        /// <remarks>
        /// Setting <typeparamref name="T"/> equal to <see cref="object"/> would override comparison function for all members.
        /// </remarks>
        public EqualityBuilder<TTarget> CompareWith<T>(Func<T, T, bool> comparer)
        {
            return OverrideContextForType(typeof(T), comparer.ToGeneric<T, object>());
        }

        /// <summary>
        /// Chooses function to compare objects based on type and
        /// <see cref="SequenceMode"/> setting.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="ISet{T}.SetEquals"/> for <see cref="ISet{T}"/> objects.
        /// Returns function comparing sequences (<see cref="IEnumerable{T}"/>) element-wise.
        /// Uses objects' equality function for all other types.
        /// </remarks>
        [SuppressMessage("ReSharper", "InvertIf")]
        protected override EqualityFunc GetImplicitContext(SelectedMember member)
        {
            if (member.MemberType.IsAssignableToSet())
            {
                return CreateSetComparer(member.MemberType);
            }
            
            if (member.MemberType.IsAssignableToEnumerable())
            {
                switch (SequenceMode)
                {
                    case SequenceComparisonMode.SameOrder:
                        return CreateOrderedSequenceComparer(member.MemberType);
                    case SequenceComparisonMode.IgnoreOrder:
                        return CreateUnorderedSequenceComparer(member.MemberType);
                    default:
                        return CreateOrderedSequenceComparer(member.MemberType);
                }
            }
            
            return Equals;
        }
        
        /// <summary>
        /// Builds and returns final immutable equality comparer function.
        /// </summary>
        public Func<TTarget, TTarget, bool> Build()
        {
            return new EqualityFunction<TTarget>(GetMemberContexts()).Equals;
        }
    }
}