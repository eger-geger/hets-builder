using System;
using System.Text;
using Formatter = System.Action<object, System.Text.StringBuilder>;

namespace BoilerplateBuilders.ToString.Format
{
    public static class FormatCombinators
    {
        private static Formatter None => (_1, _2) => { };

        public static Formatter Map(Func<object, object> mapper, Formatter formatter)
        {
            return (o, sb) => formatter(mapper(o), sb);
        }
        
        public static Func<object, string> MakeToString(Formatter formatter)
        {
            return o =>
            {
                var sb = new StringBuilder();
                formatter(o, sb);
                return sb.ToString();
            };
        }
        
        public static Formatter Append(string s)
        {
            return Append(_ => s);
        }

        public static Formatter Append(Func<object, string> toString)
        {
            return (o, sb) => sb.Append(toString(o));
        }
        
        public static Formatter Enclose((string opening, string closing) symbols, Formatter body)
        {
            var (opening, closing) = symbols;

            return AppendUnlessNullOrEmpty(opening)
                   + body
                   + AppendUnlessNullOrEmpty(closing);
        }
        
        public static Func<Formatter, Formatter, Formatter> Join(Formatter separator)
        {
            return (fa, fb) => fa + separator + fb;
        }

        public static Formatter When(Func<object, bool> condition, Formatter formatter)
        {
            return (o, sb) => (condition(o) ? formatter : None)(o, sb);
        }
        
        public static Formatter When(bool condition, Formatter formatter)
        {
            return condition ? formatter : None;
        }

        public static Formatter Unless(bool condition, Formatter f)
        {
            return When(!condition, f);
        }
        
        public static Formatter AppendUnlessNullOrEmpty(string s)
        {
            return Unless(string.IsNullOrEmpty(s), Append(s));
        }
    }
}