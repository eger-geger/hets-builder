using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BoilerplateBuilders.Reflection
{
    public class BuilderMemberFactory
    {
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        
        public static IEnumerable<BuilderMember> CollectProperties(
            Type type, BindingFlags bindingFlags = PublicInstance
        )
        {
            if(type is null)
                throw new ArgumentNullException(nameof(type));
            
            return type
                .GetProperties(bindingFlags)
                .Select(BuilderMember.Create);
        }

        public static IEnumerable<BuilderMember> CollectFields(
            Type type, BindingFlags bindingFlags = PublicInstance
        )
        {
            if(type is null)
                throw new ArgumentNullException(nameof(type));

            return type
                .GetFields(bindingFlags)
                .Select(BuilderMember.Create);
        }
        
        public static IEnumerable<BuilderMember> CollectFieldsAndPropertiesMarkedWith<TAttribute>(
            Type type,
            BindingFlags bindingFlags = PublicInstance
        )
        {
            bool hasAttribute(MemberInfo memberInfo) =>
                memberInfo.IsDefined(typeof(TAttribute), inherit: true);
            
            var fields = 
                type.GetFields(bindingFlags)
                    .Where(hasAttribute)
                    .Select(BuilderMember.Create);

            var properties =
                type.GetProperties(bindingFlags)
                    .Where(hasAttribute)
                    .Select(BuilderMember.Create);

            return fields.Union(properties);
        }
        
    }
}