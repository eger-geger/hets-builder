using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        public SequenceEqualityComparisonMode SequenceMode { get; private set; } = SequenceEqualityComparisonMode.SameOrder;
        
        /// <summary>
        /// Sets <see cref="SequenceMode"/> which defines how collections should be compared by default.
        /// </summary>
        /// <returns>Current equality builder instance.</returns>
        public EqualityBuilder<TTarget> WithSequenceComparisonMode(SequenceEqualityComparisonMode mode)
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
            return AppendExplicitMemberFunction(expression, comparisonFunc.ToGeneric());
        }

        /// <summary>
        /// Sets equality comparison function for all object assignable to given type.
        /// Overrides any previous or default value.
        /// </summary>
        /// <param name="comparisonFunc">Function comparing objects or given type for equality.</param>
        /// <typeparam name="T">Type of compared objects.</typeparam>
        /// <returns></returns>
        /// <remarks>
        /// Setting <typeparamref name="T"/> equal to <see cref="object"/> would override comparison function for all members.
        /// </remarks>
        public EqualityBuilder<TTarget> WithExplicitTypeComparer<T>(Func<T, T, bool> comparisonFunc)
        {
            return AppendExplicitTypeFunction(typeof(T), comparisonFunc.ToGeneric());
        }

        protected override EqualityFunc GetDefaultFunction(BuilderMember member)
        {
            if (member.MemberType.IsAssignableToSet())
            {
                return CreateSetComparer(member.MemberType);
            }
            
            if (member.MemberType.IsAssignableToEnumerable())
            {
                switch (SequenceMode)
                {
                    case SequenceEqualityComparisonMode.SameOrder:
                        return CreateOrderedSequenceComparer(member.MemberType);
                    case SequenceEqualityComparisonMode.IgnoreOrder:
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
        public EqualityFunction<TTarget> Build()
        {
            return new EqualityFunction<TTarget>(BuildFinalFunctionSet());
        }
    }
}