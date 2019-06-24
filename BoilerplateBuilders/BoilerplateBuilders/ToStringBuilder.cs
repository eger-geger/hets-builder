using System;
using BoilerplateBuilders.Reflection;

namespace BoilerplateBuilders
{
    public class ToStringBuilder<T> : AbstractBuilder<T, ToStringBuilder<T>, Func<object, string>>
    {
        public string ToString(T target)
        {
            throw new NotImplementedException();
        }

        protected override Func<object, string> GetDefaultFunction(BuilderMember member)
        {
            return o => o.ToString();
        }
    }
}