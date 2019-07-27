using System;
using System.Linq.Expressions;
using System.Reflection;
using BoilerplateBuilders.Utils;
using static BoilerplateBuilders.Reflection.AccessorFactory;

namespace BoilerplateBuilders.Reflection
{
    /// <summary>
    /// Pointer to object field or property selected by function builder.
    /// </summary>
    public class SelectedMember
    {
        /// <summary>
        /// Explicitly initializes all properties of a new <see cref="SelectedMember"/> instance.
        /// </summary>
        /// <param name="memberType">Type of field or property value.</param>
        /// <param name="memberName">Field or property name.</param>
        /// <param name="getter">Function retrieving member value from an instance of an object it is defined in.</param>
        private SelectedMember(Type memberType, string memberName, Func<object, object> getter)
        {
            MemberType = memberType ?? throw new ArgumentNullException(nameof(memberType));
            MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
        }

        /// <summary>
        /// Type of field or property value.
        /// </summary>
        public Type MemberType { get; }

        /// <summary>
        /// Selected field or property name.
        /// </summary>
        public string MemberName { get; }
        
        /// <summary>
        /// Function retrieving member value from an instance of an object it is defined in.
        /// </summary>
        public Func<object, object> Getter { get; }

        private bool Equals(SelectedMember other)
        {
            return MemberType == other.MemberType && string.Equals(MemberName, other.MemberName);
        }
        
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SelectedMember) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (MemberType.GetHashCode() * 397) ^ MemberName.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{MemberName}: {MemberType}";
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
        
        /// <summary>
        /// Creates instance of <see cref="SelectedMember"/> from member accessor expression.
        /// </summary>
        /// <param name="expression">Expression for field or property accessor.</param>
        /// <typeparam name="TTarget">Type of object field or property is defined in.</typeparam>
        /// <typeparam name="TMember">Type of field or property being accessed.</typeparam>
        /// <returns>
        /// <see cref="SelectedMember"/> instance pointing to accessed field or property.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="expression"/> is not field or property accessor expression.
        /// </exception>
        public static SelectedMember Create<TTarget, TMember>(Expression<Func<TTarget, TMember>> expression)
        {
            return IsSupportedMemberExpression(expression, out var memberInfo)
                ? new SelectedMember(
                    typeof(TMember),
                    memberInfo.Name,
                    expression.Compile().ToGeneric<TTarget, TMember, object, object>()
                )
                : throw new ArgumentException(
                    "Invalid expression. Only property or field accessors are supported.",
                    nameof(expression)
                );
        }

        /// <summary>
        /// Initializes <see cref="SelectedMember"/> from reflected property information.
        /// </summary>
        /// <param name="propertyInfo">Reflected property selected by builder.</param>
        /// <returns>
        /// <see cref="SelectedMember"/> instance describing given property.
        /// </returns>
        public static SelectedMember Create(PropertyInfo propertyInfo)
        {
            var getter = CreatePropertyOrFieldGetter(
                propertyInfo.DeclaringType,
                propertyInfo.PropertyType, 
                propertyInfo.Name
            );

            return new SelectedMember(propertyInfo.PropertyType, propertyInfo.Name, getter);
        }
        
        /// <summary>
        /// Initializes <see cref="SelectedMember"/> from reflected field information.
        /// </summary>
        /// <param name="fieldInfo">Reflected field selected by builder.</param>
        /// <returns>
        /// <see cref="SelectedMember"/> instance describing given field.
        /// </returns>
        public static SelectedMember Create(FieldInfo fieldInfo)
        {
            var getter = CreatePropertyOrFieldGetter(
                fieldInfo.DeclaringType,
                fieldInfo.FieldType, 
                fieldInfo.Name
            );

            return new SelectedMember(fieldInfo.FieldType, fieldInfo.Name, getter);
        }
    }
}