using System;
using System.Linq.Expressions;

namespace BoilerplateBuilders.Reflection
{
    /// <summary>
    /// Contains methods creating property or field accessors.
    /// </summary>
    public static class AccessorFactory
    {
        /// <summary>
        /// Creates function accessing object public property of field value. 
        /// </summary>
        /// <param name="holderType">Type of object which property or field should be returned.</param>
        /// <param name="memberType">Type of field or property defined in <paramref name="holderType"/>.</param>
        /// <param name="memberName">Name of field or property defined in <paramref name="holderType"/>.</param>
        /// <returns>
        /// Function accepting instance ot <paramref name="holderType"/> and
        /// returning value of <paramref name="memberType"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="holderType"/> or <paramref name="memberType"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Invalid member name or type.
        /// </exception>
        public static Func<object, object> CreatePropertyOrFieldGetter(
            Type holderType, 
            Type memberType, 
            string memberName
        )
        {
            if(holderType is null)
                throw new ArgumentNullException(nameof(holderType));
            
            if(memberType is null)
                throw new ArgumentNullException(nameof(memberType));
            
            if(String.IsNullOrWhiteSpace(memberName))
                throw new ArgumentException("Null or empty.", nameof(memberName));
            
            var holderParam = Expression.Parameter(holderType);

            Delegate getter;
            
            try
            {
                getter = Expression.Lambda(
                    typeof(Func<,>).MakeGenericType(holderType, memberType),
                    Expression.PropertyOrField(holderParam, memberName),
                    holderParam
                ).Compile();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid field or property.", ex);
            }

            return source => getter.DynamicInvoke(source);
        }
    }
}