using System;
using System.Linq.Expressions;
using System.Reflection;
using BoilerplateBuilders.Utils;
using static BoilerplateBuilders.Reflection.AccessorFactory;

namespace BoilerplateBuilders.Reflection
{
    public class BuilderMember
    {
        protected BuilderMember(BuilderMember other)
        {
            if(other is null)
                throw new ArgumentNullException(nameof(other));

            MemberType = other.MemberType;
            MemberName = other.MemberName;
            DefaultValue = other.DefaultValue;
            Getter = other.Getter;
        }
        
        private BuilderMember(
            Type memberType, 
            string memberName, 
            Func<object, object> getter, 
            object defaultValue
        )
        {
            MemberType = memberType ?? throw new ArgumentNullException(nameof(memberType));
            MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
            DefaultValue = defaultValue;
        }

        public Type MemberType { get; }

        public string MemberName { get; }

        public Func<object, object> Getter { get; }
        
        public object DefaultValue { get; }

        private bool Equals(BuilderMember other)
        {
            return MemberType == other.MemberType && string.Equals(MemberName, other.MemberName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BuilderMember) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MemberType.GetHashCode() * 397) ^ MemberName.GetHashCode();
            }
        }

        public static bool operator ==(BuilderMember left, BuilderMember right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BuilderMember left, BuilderMember right)
        {
            return !Equals(left, right);
        }

        private static bool IsSupportedMemberExpression(LambdaExpression exp, out MemberInfo memberInfo)
        {
            if (exp.Body is MemberExpression memberExp && (memberExp.Member is PropertyInfo || memberExp.Member is FieldInfo))
            {
                memberInfo = memberExp.Member;
            }
            else
            {
                memberInfo = null;
            }

            return memberInfo != null;
        }
        
        public static BuilderMember Create<TTarget, TMember>(Expression<Func<TTarget, TMember>> expression)
        {
            return IsSupportedMemberExpression(expression, out var memberInfo)
                ? new BuilderMember(
                    typeof(TMember),
                    memberInfo.Name,
                    expression.Compile().ToGeneric<TTarget, TMember, object>(),
                    default(TMember)
                )
                : throw new ArgumentException(
                    "Invalid expression. Only property or field accessors are supported.",
                    nameof(expression)
                );
        }

        public static BuilderMember Create(PropertyInfo propertyInfo)
        {
            var getter = CreatePropertyOrFieldGetter(
                propertyInfo.DeclaringType,
                propertyInfo.PropertyType, 
                propertyInfo.Name
            );

            var defaultValue = propertyInfo.PropertyType.GetDefaultValue();
            
            return new BuilderMember(propertyInfo.PropertyType, propertyInfo.Name, getter, defaultValue);
        }

        public static BuilderMember Create(FieldInfo fieldInfo)
        {
            var getter = CreatePropertyOrFieldGetter(
                fieldInfo.DeclaringType,
                fieldInfo.FieldType, 
                fieldInfo.Name
            );

            var defaultValue = fieldInfo.FieldType.GetDefaultValue();
            
            return new BuilderMember(fieldInfo.FieldType, fieldInfo.Name, getter, defaultValue);
        }
    }
}