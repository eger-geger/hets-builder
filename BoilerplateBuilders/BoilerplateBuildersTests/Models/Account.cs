using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BoilerplateBuildersTests.Models
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Account
    {
        public readonly int Id;

        public Account(int id = 0, string name = null, params string[] phones)
        {
            Id = id;
            Name = name;
            Phones = phones;
        }

        public string Name { get; }

        public string[] Phones { get; }
        
        public Dictionary<string, object> Extra { get; set; }
        
        public override string ToString()
        {
            return new StringBuilder()
                .Append("Account(")
                .AppendFormat("{0}: {1}", nameof(Id), Id)
                .Append(", ")
                .AppendFormat("{0}: {1}", nameof(Name), Name ?? "NULL")
                .Append(", ")
                .AppendFormat("{0}: [", nameof(Phones))
                .AppendJoin(", ", Phones ?? new string[0])
                .Append("]")
                .Append(")")
                .ToString();
        }
    }
}