using System;
using System.Collections.Generic;
using System.Linq;
using Operation = BoilerplateBuilders.Reflection.BuilderMemberOperation<System.Func<object, object, bool>>;

namespace BoilerplateBuilders.Reflection.Equality
{
    /// <summary>
    /// Function comparing two objects for equality.
    /// </summary>
    /// <typeparam name="TTarget">Type of compared objects.</typeparam>
    public class EqualityFunction<TTarget>
    {
        private readonly ISet<Operation> _members;

        internal EqualityFunction(IEnumerable<Operation> members)
        {
            if (members is null)
                throw new ArgumentNullException(nameof(members));
            
            _members = new SortedSet<Operation>(members);
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
        
        private static bool Compare(object x, object y, Operation op)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return op.Function(x, y);
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