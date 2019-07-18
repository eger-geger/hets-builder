using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BoilerplateBuilders.Reflection
{
    /// <summary>
    /// Provides functions for inspecting fields and properties of a given type.
    /// </summary>
    public class MemberFactory
    {
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        
        /// <summary>
        /// Collects all properties of a given type matching criteria. 
        /// </summary>
        /// <param name="type">Type to inspect.</param>
        /// <param name="bindingFlags">Search criteria (default: public instance properties).</param>
        /// <returns>Sequence of member info objects describing matched properties.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> is null.
        /// </exception>
        public static IEnumerable<SelectedMember> SelectProperties(Type type, BindingFlags bindingFlags = PublicInstance)
        {
            if(type is null)
                throw new ArgumentNullException(nameof(type));
            
            return type
                .GetProperties(bindingFlags)
                .Select(SelectedMember.Create);
        }

        /// <summary>
        /// Collects all type fields matching search criteria.
        /// </summary>
        /// <param name="type">Type to inspect.</param>
        /// <param name="bindingFlags">Search criteria (default: public instance properties).</param>
        /// <returns>Sequence of member info objects describing matched fields.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> is null.
        /// </exception>
        public static IEnumerable<SelectedMember> SelectFields(Type type, BindingFlags bindingFlags = PublicInstance)
        {
            if(type is null)
                throw new ArgumentNullException(nameof(type));

            return type
                .GetFields(bindingFlags)
                .Select(SelectedMember.Create);
        }
            
        /// <summary>
        /// Collects all types' fields and properties matching search criteria and marked with
        /// <typeparamref name="TAttribute"/> attribute.
        /// </summary>
        /// <param name="type">Type to inspect.</param>
        /// <param name="bindingFlags">Search criteria (default all public instance).</param>
        /// <typeparam name="TAttribute">Required attribute type.</typeparam>
        /// <returns>
        /// Sequence of member info objects describing matched fields and properties.
        /// </returns>
        public static IEnumerable<SelectedMember> SelectFieldsAndPropertiesMarkedWith<TAttribute>(
            Type type,
            BindingFlags bindingFlags = PublicInstance
        )
        {
            bool HasAttribute(MemberInfo memberInfo) =>
                memberInfo.IsDefined(typeof(TAttribute), inherit: true);
            
            var fields = 
                type.GetFields(bindingFlags)
                    .Where(HasAttribute)
                    .Select(SelectedMember.Create);

            var properties =
                type.GetProperties(bindingFlags)
                    .Where(HasAttribute)
                    .Select(SelectedMember.Create);

            return fields.Union(properties);
        }
        
    }
}