using System;
using System.Collections;
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
        public EqualityBuilder<TTarget> Use<T>(Func<T, T, bool> comparer)
        {
            return OverrideContextForType(typeof(T), comparer.ToGeneric<T, object>());
        }

        /// <summary>
        /// Instructs builder to compare collection members element-wise within resulting function. 
        /// </summary>
        /// <returns>Updated builder instance.</returns>
        public EqualityBuilder<TTarget> CompareCollectionsElementWise()
        {
            return OverrideContextForType(typeof(ICollection), SequenceEqualElementWise);
        }

        /// <summary>
        /// Instructs builder to compare collection members element-wise ignoring order within resulting function.
        /// </summary>
        /// <returns>Updated builder instance</returns>
        public EqualityBuilder<TTarget> CompareCollectionsIgnoringOrder()
        {
            return OverrideContextForType(typeof(ICollection), SequenceEqualIgnoreOrder);
        }
        
        /// <summary>
        /// Returns default comparison function (<see cref="object.Equals(object)"/>).
        /// </summary>
        protected override EqualityFunc GetImplicitContext(SelectedMember member)
        {
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