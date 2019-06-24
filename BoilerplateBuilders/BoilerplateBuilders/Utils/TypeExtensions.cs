using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BoilerplateBuilders.Utils
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns default value for a type.
        /// </summary>
        /// <param name="type">Type of default value.</param>
        /// <returns>Default value of given type (e.g.: null for reference type or 0 for int).</returns>
        public static object GetDefaultValue(this Type type)
        {
            return Expression.Lambda(
                    typeof(Func<>).MakeGenericType(type), 
                    Expression.Default(type)
                )
                .Compile()
                .DynamicInvoke();
        }

        /// <summary>
        /// Returns specific generic interface implemented by <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type which should implement <paramref name="interfaceType"/>.</param>
        /// <param name="interfaceType">Generic interface type (e.g.: <code>typeof(IEnumerable&lt;&gt;)</code>).</param>
        /// <returns>Specific interface type.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> or <paramref name="interfaceType"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="interfaceType"/> is not generic interface or
        /// <paramref name="type"/> does not implement it.
        /// </exception>
        public static Type GetImplementedGenericInterfaceType(this Type type, Type interfaceType)
        {
            return GetImplementedGenericInterfaceTypeOrNull(type, interfaceType)
                ?? throw new ArgumentException(
                    $"Type does not implement {interfaceType}.", 
                    nameof(interfaceType)
                );
        }

        /// <summary>
        /// Returns specific generic interface implemented by <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type which should implement <paramref name="interfaceType"/>.</param>
        /// <param name="interfaceType">Generic interface type (e.g.: <code>typeof(IEnumerable&lt;&gt;)</code>).</param>
        /// <returns>
        /// Specific interface type or null if  <paramref name="type"/>
        /// does not implement <paramref name="interfaceType"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> or <paramref name="interfaceType"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="interfaceType"/> is not generic interface.
        /// </exception>
        public static Type GetImplementedGenericInterfaceTypeOrNull(this Type type, Type interfaceType)
        {
            if(type is null)
                throw new ArgumentNullException(nameof(type));
            
            if(interfaceType is null)
                throw new ArgumentNullException(nameof(interfaceType));
            
            if(!interfaceType.IsInterface)
                throw new ArgumentException("Is not interface.", nameof(interfaceType));
            
            if(!interfaceType.IsGenericType)
                throw new ArgumentException("Not generic type.", nameof(interfaceType));
            
            var isInterface = IsGenericType(interfaceType);
            
            return isInterface(type)
                ? type
                : type.GetInterfaces().SingleOrDefault(isInterface);
        }
        
        public static bool IsAssignableToSet(this Type type)
        {
            return type.GetImplementedGenericInterfaceTypeOrNull(typeof(ISet<>)) != null;
        }

        public static bool IsAssignableToEnumerable(this Type type)
        {
            return type.GetImplementedGenericInterfaceTypeOrNull(typeof(IEnumerable<>)) != null;
        }
        
        private static Func<Type, bool> IsGenericType(Type genericTypeDefinition)
        {
            return type => type.IsGenericType
                           && type.GetGenericTypeDefinition() == genericTypeDefinition;
        }

        public static IEnumerable<Type> GetAllBaseTypesAndInterfaces(this Type type)
        {
            return GetAllBaseTypes(type).Union(type.GetInterfaces());
        }

        public static IEnumerable<Type> GetAllBaseTypes(this Type type)
        {
            for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                yield return baseType;
            }
        }
        
    }
}