using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.Utils;
using static BoilerplateBuilders.Reflection.ContextSource;
using static BoilerplateBuilders.Reflection.MemberFactory;

namespace BoilerplateBuilders
{
    /// <summary>
    /// Provides common builder skeleton and behavior for inspecting objects members, selecting members
    /// and associating selected member with <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TTarget">Type of object being explored by builder.</typeparam>
    /// <typeparam name="TBuilder">Type of a builder object itself.</typeparam>
    /// <typeparam name="TContext">Type of contextual information associated with every selected member.</typeparam>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class AbstractBuilder<TTarget, TBuilder, TContext>
        where TBuilder : AbstractBuilder<TTarget, TBuilder, TContext>
    {
        private readonly ISet<MemberContext<TContext>> _memberContexts;

        private readonly IDictionary<Type, TContext> _explicitTypeContexts;

        /// <summary>
        /// Default constructor initializing private members.
        /// </summary>
        protected AbstractBuilder()
        {
            _memberContexts = new OrderedHashSet<MemberContext<TContext>>();
            _explicitTypeContexts = new Dictionary<Type, TContext>();
        }

        /// <summary>
        /// Includes referenced field or property into members available to object being built.
        /// </summary>
        /// <param name="fieldOrPropertyGetter">Expression pointing to field or property to include.</param>
        /// <typeparam name="TMember">Type of selected field or property.</typeparam>
        /// <returns>Updated builder instance.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="fieldOrPropertyGetter"/> is not field or property accessor expression.
        /// </exception>
        public TBuilder Append<TMember>(Expression<Func<TTarget, TMember>> fieldOrPropertyGetter)
        {
            _memberContexts.Add(CreateImplicitMemberContext(SelectedMember.Create(fieldOrPropertyGetter)));
            return this as TBuilder;
        }
        
        /// <summary>
        /// Appends current builder with all public fields and properties from
        /// <typeparamref name="TTarget"/> type  marked with <typeparamref name="TAttribute"/>
        /// attribute with default context selected by builder (<see cref="GetImplicitContext"/>).
        /// </summary>
        /// <typeparam name="TAttribute">Type of marker attribute.</typeparam>
        /// <returns>Updated builder instance.</returns>
        public TBuilder AppendPublicFieldsAndPropertiesMarkedWith<TAttribute>() where TAttribute : Attribute
        {
            _memberContexts
                .UnionWith(
                    SelectFieldsAndPropertiesMarkedWith<TAttribute>(typeof(TTarget))
                        .Select(CreateImplicitMemberContext)
                );
            
            return this as TBuilder;
        }
        
        /// <summary>
        /// Appends current builder with all public properties from <typeparamref name="TTarget"/>
        /// with default context (<see cref="GetImplicitContext"/>).
        /// </summary>
        /// <returns>Updated builder instance.</returns>
        public TBuilder AppendPublicProperties()
        {
            _memberContexts.UnionWith(SelectProperties(typeof(TTarget)).Select(CreateImplicitMemberContext));
            return this as TBuilder;
        }

        /// <summary>
        /// Appends current builder with all public fields of <typeparamref name="TTarget"/>
        /// with default context (<see cref="GetImplicitContext"/>).
        /// </summary>
        /// <returns>Updated builder instance.</returns>
        public TBuilder AppendPublicFields()
        {
            _memberContexts.UnionWith(SelectFields(typeof(TTarget)).Select(CreateImplicitMemberContext));
            return this as TBuilder;
        }
        
        /// <summary>
        /// Appends member and context to builder. 
        /// </summary>
        /// <param name="expression">Expression pointing to field or property to include.</param>
        /// <param name="context">Context associated with selected member.</param>
        /// <typeparam name="TMember">Type of selected field or property.</typeparam>
        /// <returns>Updated builder instance.</returns>
        protected TBuilder AppendExplicit<TMember>(Expression<Func<TTarget, TMember>> expression, TContext context)
        {
            _memberContexts.Add(new MemberContext<TContext>(
                SelectedMember.Create(expression),
                context,
                ExplicitMember
            ));
            
            return this as TBuilder;
        }
        
        /// <summary>
        /// Instructs builder to use given context for all selected members assignable to <paramref name="type"/>.
        /// It affects only members associated with context implicitly (from <see cref="GetImplicitContext"/>).
        /// Any members added with <see cref="AppendExplicit{TMember}"/> are not affected by this setting. 
        /// </summary>
        /// <param name="type">Head of type hierarchy to override context for.</param>
        /// <param name="context">Context object to use instead of one selected by default.</param>
        /// <returns>Updated builder instance.</returns>
        protected TBuilder OverrideContextForType(Type type, TContext context)
        {
            _explicitTypeContexts[type] = context;
            return this as TBuilder;
        }
        
        private MemberContext<TContext> CreateImplicitMemberContext(SelectedMember member)
        {
            return new MemberContext<TContext>(member, GetImplicitContext(member), Implicit);
        }
        
        private bool HasTypeExplicitContext(Type type, out TContext context)
        {
            context = _explicitTypeContexts
                .Where(kv => kv.Key.IsAssignableFrom(type))
                .OrderBy(kv => kv.Key.GetAllBaseTypesAndInterfaces().Count())
                .LastOrDefault()
                .Value;

            return context != null;
        }
        
        /// <summary>
        /// Builds sequence of effective member contexts for all members added so far.
        /// </summary>
        protected IEnumerable<MemberContext<TContext>> GetMemberContexts()
        {
            return _memberContexts.Select(GetEffectiveContext);
        }

        private MemberContext<TContext> GetEffectiveContext(MemberContext<TContext> memberContext)
        {
            return
                memberContext.Source != ExplicitMember
                && HasTypeExplicitContext(memberContext.Member.MemberType, out var context)
                    ? new MemberContext<TContext>(memberContext.Member, context, ExplicitType)
                    : memberContext;
        }
        
        /// <summary>
        /// Creates instance of a context for selected member when one was no specified explicitly.
        /// </summary>
        /// <param name="member">Selected property or field of <typeparamref name="TTarget"/>.</param>
        /// <returns>Context to be associated with given member.</returns>
        protected abstract TContext GetImplicitContext(SelectedMember member);
    }
}
