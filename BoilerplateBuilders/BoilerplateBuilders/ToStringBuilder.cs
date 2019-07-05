using System;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.ToString.Format;
using BoilerplateBuilders.Utils;

namespace BoilerplateBuilders
{
    public class ToStringBuilder<TTarget> : AbstractBuilder<TTarget, ToStringBuilder<TTarget>, Func<object, string>>
    {
        private static readonly DefaultFormatBuilder DefaultFormatBuilder =
            new DefaultFormatBuilder()
                .SetDensityFlag(FormatDensity.IncludeClassName)
                .SetDensityFlag(FormatDensity.IncludeMemberName)
                .SeparateMembersWith(", ");

        public IToStringFormat ToStringFormat { get; private set; }
        
        public ToStringBuilder<TTarget> Append<TMember>(Expression<Func<TTarget, TMember>> getter, Func<TMember, string> toString)
        {
            return AppendExplicit(getter, toString.ToGeneric<TMember, string, string>());
        }
        
        public ToStringBuilder<TTarget> Use<T>(Func<T, string> toString)
        {
            return OverrideFunction(typeof(T), toString.ToGeneric<T, string, string>());
        }

        public Func<TTarget, string> Build()
        {
            return (ToStringFormat ?? DefaultFormatBuilder)
                .Build(BuildOperations())
                .ToSpecific<TTarget, string>();
        }

        protected override Func<object, string> GetDefaultFunction(BuilderMember member)
        {
            return o => o.ToString();
        }
    }
}