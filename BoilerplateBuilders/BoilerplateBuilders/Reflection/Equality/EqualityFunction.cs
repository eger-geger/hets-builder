using System;
using System.Collections.Generic;
using System.Linq;

namespace BoilerplateBuilders.Reflection.Equality
{
    /// <summary>
    /// Function comparing two objects for equality.
    /// </summary>
    /// <typeparam name="TTarget">Type of compared objects.</typeparam>
    public class EqualityFunction<TTarget>
    {
        private readonly ISet<BuilderMemberOperation<EqualityDelegate>> _members;

        internal EqualityFunction(IEnumerable<BuilderMemberOperation<EqualityDelegate>> members)
        {
            if (members is null)
                throw new ArgumentNullException(nameof(members));
            
            _members = new HashSet<BuilderMemberOperation<EqualityDelegate>>(members);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        public bool Equals(TTarget a, TTarget b)
        {
            return _members
                .Select(bf => (bf.Member.Getter(a), bf.Member.Getter(b), bf))
                .All(abm => Compare(abm.Item1, abm.Item2, abm.Item3));
        }
        
        private static bool Compare(object x, object y, BuilderMemberOperation<EqualityDelegate> member)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return member.Function(x, y);
        }

        protected bool Equals(EqualityFunction<TTarget> other)
        {
            return _members.SetEquals(other._members);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EqualityFunction<TTarget>) obj);
        }

        public override int GetHashCode()
        {
            return _members
                .Select(m => GetHashCode())
                .Aggregate(seed: 0, func: (acc, h) => acc ^ h * 397);
        }
    }
}