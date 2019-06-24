using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.Utils;
using static BoilerplateBuilders.Reflection.BuilderMemberFactory;

namespace BoilerplateBuilders
{
    public abstract class AbstractBuilder<TTarget, TBuilder, TFunction>
        where TBuilder : AbstractBuilder<TTarget, TBuilder, TFunction>
        where TFunction : class
    {
        protected readonly ISet<BuilderMemberOperation<TFunction>> Functions;

        private readonly IDictionary<Type, TFunction> _explicitTypeFunctions;

        public TBuilder AppendPublicFieldsAndPropertiesMarkedWith<TAttribute>() where TAttribute : Attribute
        {
            Functions
                .UnionWith(
                    CollectFieldsAndPropertiesMarkedWith<TAttribute>(typeof(TTarget))
                        .Select(CreateImplicitBuilderFunction)
                );
            
            return this as TBuilder;
        }

        public TBuilder AppendPublicProperties()
        {
            Functions.UnionWith(CollectProperties(typeof(TTarget)).Select(CreateImplicitBuilderFunction));
            return this as TBuilder;
        }

        public TBuilder AppendPublicFields()
        {
            Functions.UnionWith(CollectFields(typeof(TTarget)).Select(CreateImplicitBuilderFunction));
            return this as TBuilder;
        }
        
        protected TBuilder AppendExplicitMemberFunction<TMember>(
            Expression<Func<TTarget, TMember>> expression,
            TFunction function
        )
        {
            Functions.Add(new BuilderMemberOperation<TFunction>(
                BuilderMember.Create(expression),
                function,
                BuilderOperationSource.ExplicitMember
            ));
            
            return this as TBuilder;
        }

        public TBuilder AppendExplicitTypeFunction(Type type, TFunction function)
        {
            _explicitTypeFunctions[type] = function ?? throw new ArgumentNullException(nameof(function));
            return this as TBuilder;
        }
        
        private BuilderMemberOperation<TFunction> CreateImplicitBuilderFunction(BuilderMember member)
        {
            return CreateBuilderFunction(member, BuilderOperationSource.Implicit);
        }
        
        private BuilderMemberOperation<TFunction> CreateBuilderFunction(BuilderMember member, BuilderOperationSource source)
        {
            return new BuilderMemberOperation<TFunction>(member, GetDefaultFunction(member), source);
        }
        
        public TBuilder Append<TMember>(Expression<Func<TTarget, TMember>> getter)
        {
            Functions.Add(CreateImplicitBuilderFunction(BuilderMember.Create(getter)));
            return this as TBuilder;
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
        
        protected IEnumerable<BuilderMemberOperation<TFunction>> BuildFinalFunctionSet()
        {
            foreach (var func in Functions)
            {
                if (func.BuilderOperationSource == BuilderOperationSource.ExplicitMember)
                    yield return func;
                else if (HasTypeExplicitComparer(func.Member.MemberType, out var function))
                    yield return new BuilderMemberOperation<TFunction>(func.Member, function, BuilderOperationSource.ExplicitType);
                else
                    yield return func;
            }
        }
        
        protected abstract TFunction GetDefaultFunction(BuilderMember member);
    }
}
