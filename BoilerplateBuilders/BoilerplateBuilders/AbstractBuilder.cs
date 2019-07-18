using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.Utils;
using static BoilerplateBuilders.Reflection.MemberFactory;

namespace BoilerplateBuilders
{
    /// <summary>
    /// Provides common builder skeleton and behavior like
    /// exploring objects, adding and storing builder member operations.
    /// </summary>
    /// <typeparam name="TTarget">Type of object builder will be used with.</typeparam>
    /// <typeparam name="TBuilder">Type of a builder object itself.</typeparam>
    /// <typeparam name="TFunction">
    /// Most generic signature of a function being built
    /// (e.g. <code>Func&lt;object, object, bool&gt;</code> for equality).
    /// </typeparam>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class AbstractBuilder<TTarget, TBuilder, TFunction>
        where TBuilder : AbstractBuilder<TTarget, TBuilder, TFunction>
        where TFunction : class
    {
        private readonly ISet<MemberFunction<TFunction>> _operations;

        private readonly IDictionary<Type, TFunction> _explicitTypeFunctions;

        /// <summary>
        /// Default constructor initializing private members.
        /// </summary>
        protected AbstractBuilder()
        {
            _operations = new SortedSet<MemberFunction<TFunction>>();
            _explicitTypeFunctions = new Dictionary<Type, TFunction>();
        }

        /// <summary>
        /// Includes referenced field or property into list of members available to function being build.
        /// </summary>
        /// <param name="fieldOrPropertyGetter">Expression pointing to field or property to include.</param>
        /// <typeparam name="TMember">Type of selected field or property.</typeparam>
        /// <returns>Updated builder instance.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="fieldOrPropertyGetter"/> is not field or property accessor expression.
        /// </exception>
        public TBuilder Append<TMember>(Expression<Func<TTarget, TMember>> fieldOrPropertyGetter)
        {
            _operations.Add(CreateImplicitOperation(SelectedMember.Create(fieldOrPropertyGetter)));
            return this as TBuilder;
        }
        
        /// <summary>
        /// Appends current builder with all public fields and properties from
        /// <typeparamref name="TTarget"/> type  marked with <typeparamref name="TAttribute"/>
        /// attribute  with default operations selected by builder (<see cref="GetDefaultFunction"/>).
        /// </summary>
        /// <typeparam name="TAttribute">Type of required attribute.</typeparam>
        /// <returns>Updated builder instance.</returns>
        public TBuilder AppendPublicFieldsAndPropertiesMarkedWith<TAttribute>() where TAttribute : Attribute
        {
            _operations
                .UnionWith(
                    SelectFieldsAndPropertiesMarkedWith<TAttribute>(typeof(TTarget))
                        .Select(CreateImplicitOperation)
                );
            
            return this as TBuilder;
        }
        
        /// <summary>
        /// Appends current builder with all public properties from <typeparamref name="TTarget"/>
        /// with default operations selected by builder (<see cref="GetDefaultFunction"/>).
        /// </summary>
        /// <returns>Updated builder instance.</returns>
        public TBuilder AppendPublicProperties()
        {
            _operations.UnionWith(SelectProperties(typeof(TTarget)).Select(CreateImplicitOperation));
            return this as TBuilder;
        }

        /// <summary>
        /// Appends current builder with all public fields of <typeparamref name="TTarget"/>
        /// with default operations selected by builder (<see cref="GetDefaultFunction"/>).
        /// </summary>
        /// <returns>Updated builder instance.</returns>
        public TBuilder AppendPublicFields()
        {
            _operations.UnionWith(SelectFields(typeof(TTarget)).Select(CreateImplicitOperation));
            return this as TBuilder;
        }
        
        /// <summary>
        /// Appends builder with selected <typeparamref name="TTarget"/>
        /// field or property and specified generic builder function (
        /// e.g.: <code>Func&lt;object, object, bool&gt;</code> for equality). 
        /// </summary>
        /// <param name="expression">Field or property selector.</param>
        /// <param name="function">Function used by builder operation on selected member.</param>
        /// <typeparam name="TMember">type of selected field or property.</typeparam>
        /// <returns>Updated builder instance.</returns>
        protected TBuilder AppendExplicit<TMember>(Expression<Func<TTarget, TMember>> expression, TFunction function)
        {
            _operations.Add(new MemberFunction<TFunction>(
                SelectedMember.Create(expression),
                function,
                MemberFunctionSource.ExplicitMember
            ));
            
            return this as TBuilder;
        }
        
        /// <summary>
        /// Instructs builder to use <paramref name="function"/> builder function
        /// for all selected members of <paramref name="type"/> type or type derived from it.
        /// It affects only members added with implicit functions chosen by builder.
        /// Any members added with <see cref="AppendExplicit{TMember}"/>
        /// are not affected by this call. 
        /// </summary>
        /// <param name="type">Type to override builder function for.</param>
        /// <param name="function">Custom builder function.</param>
        /// <returns>Updated builder instance.</returns>
        protected TBuilder OverrideFunction(Type type, TFunction function)
        {
            _explicitTypeFunctions[type] = function ?? throw new ArgumentNullException(nameof(function));
            return this as TBuilder;
        }
        
        private MemberFunction<TFunction> CreateImplicitOperation(SelectedMember member)
        {
            return CreateOperation(member, MemberFunctionSource.Implicit);
        }
        
        private MemberFunction<TFunction> CreateOperation(SelectedMember member, MemberFunctionSource source)
        {
            return new MemberFunction<TFunction>(member, GetDefaultFunction(member), source);
        }

        private bool HasTypeExplicitComparer(Type type, out TFunction function)
        {
            function = _explicitTypeFunctions
                .Where(kv => kv.Key.IsAssignableFrom(type))
                .OrderBy(kv => kv.Key.GetAllBaseTypesAndInterfaces().Count())
                .LastOrDefault()
                .Value;

            return function != null;
        }
        
        /// <summary>
        /// Returns final set of member operations accounting for implicit (default) and explicit operations.
        /// </summary>
        protected IEnumerable<MemberFunction<TFunction>> BuildOperations()
        {
            foreach (var func in _operations)
            {
                if (func.MemberFunctionSource == MemberFunctionSource.ExplicitMember)
                    yield return func;
                else if (HasTypeExplicitComparer(func.Member.MemberType, out var function))
                    yield return new MemberFunction<TFunction>(func.Member, function, MemberFunctionSource.ExplicitType);
                else
                    yield return func;
            }
        }
        
        /// <summary>
        /// Chooses generic builder function best suited for given member.
        /// </summary>
        /// <param name="member">Selected property or field of <typeparamref name="TTarget"/>.</param>
        /// <returns>Generic builder function.</returns>
        protected abstract TFunction GetDefaultFunction(SelectedMember member);
    }
}
