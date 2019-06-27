using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.Utils;
using static BoilerplateBuilders.Reflection.BuilderMemberFactory;

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
    public abstract class AbstractBuilder<TTarget, TBuilder, TFunction>
        where TBuilder : AbstractBuilder<TTarget, TBuilder, TFunction>
        where TFunction : class
    {
        private readonly ISet<BuilderMemberOperation<TFunction>> _operations;

        private readonly IDictionary<Type, TFunction> _explicitTypeFunctions;

        /// <summary>
        /// Default constructor initializing private members.
        /// </summary>
        protected AbstractBuilder()
        {
            _operations = new SortedSet<BuilderMemberOperation<TFunction>>();
            _explicitTypeFunctions = new Dictionary<Type, TFunction>();
        }

        /// <summary>
        /// Appends builder with selected field or property with default
        /// operation selected by builder.
        /// </summary>
        /// <param name="getter"><typeparamref name="TTarget"/> field or property getter.</param>
        /// <typeparam name="TMember">Type of chosen field or property.</typeparam>
        /// <returns>Updated builder instance.</returns>
        public TBuilder Append<TMember>(Expression<Func<TTarget, TMember>> getter)
        {
            _operations.Add(CreateImplicitOperation(BuilderMember.Create(getter)));
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
                    CollectFieldsAndPropertiesMarkedWith<TAttribute>(typeof(TTarget))
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
            _operations.UnionWith(CollectProperties(typeof(TTarget)).Select(CreateImplicitOperation));
            return this as TBuilder;
        }

        /// <summary>
        /// Appends current builder with all public fields of <typeparamref name="TTarget"/>
        /// with default operations selected by builder (<see cref="GetDefaultFunction"/>).
        /// </summary>
        /// <returns>Updated builder instance.</returns>
        public TBuilder AppendPublicFields()
        {
            _operations.UnionWith(CollectFields(typeof(TTarget)).Select(CreateImplicitOperation));
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
            _operations.Add(new BuilderMemberOperation<TFunction>(
                BuilderMember.Create(expression),
                function,
                BuilderOperationSource.ExplicitMember
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
        
        private BuilderMemberOperation<TFunction> CreateImplicitOperation(BuilderMember member)
        {
            return CreateOperation(member, BuilderOperationSource.Implicit);
        }
        
        private BuilderMemberOperation<TFunction> CreateOperation(BuilderMember member, BuilderOperationSource source)
        {
            return new BuilderMemberOperation<TFunction>(member, GetDefaultFunction(member), source);
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
        protected IEnumerable<BuilderMemberOperation<TFunction>> BuildOperations()
        {
            foreach (var func in _operations)
            {
                if (func.BuilderOperationSource == BuilderOperationSource.ExplicitMember)
                    yield return func;
                else if (HasTypeExplicitComparer(func.Member.MemberType, out var function))
                    yield return new BuilderMemberOperation<TFunction>(func.Member, function, BuilderOperationSource.ExplicitType);
                else
                    yield return func;
            }
        }
        
        /// <summary>
        /// Chooses generic builder function best suited for given member.
        /// </summary>
        /// <param name="member">Selected property or field of <typeparamref name="TTarget"/>.</param>
        /// <returns>Generic builder function.</returns>
        protected abstract TFunction GetDefaultFunction(BuilderMember member);
    }
}
