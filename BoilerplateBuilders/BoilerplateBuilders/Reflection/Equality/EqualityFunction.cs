using System;
using System.Collections.Generic;
using System.Linq;
using BoilerplateBuilders.Utils;
using Operation = BoilerplateBuilders.Reflection.MemberContext<System.Func<object, object, bool>>;

namespace BoilerplateBuilders.Reflection.Equality
{
    /// <summary>
    /// Function comparing two objects for equality.
    /// </summary>
    /// <typeparam name="TTarget">Type of compared objects.</typeparam>
    public class EqualityFunction<TTarget>
    {
        private readonly ISet<Operation> _operations;

        internal EqualityFunction(IEnumerable<Operation> members)
        {
            if (members is null)
                throw new ArgumentNullException(nameof(members));
            
            _operations = new OrderedHashSet<Operation>(members);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        public bool Equals(TTarget a, TTarget b)
        {
            bool MemberValuesEqual(Operation op) =>
                MemberValuesEquals(op.Member.Getter(a), op.Member.Getter(b), op);
            
            return _operations.All(MemberValuesEqual);
        }
        
        private static bool MemberValuesEquals(object x, object y, Operation op)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return op.Context(x, y);
        }

        private bool Equals(EqualityFunction<TTarget> other)
        {
            return _operations.SetEquals(other._operations);
        }

        /// <summary>
        /// Compares two <see cref="EqualityFunction{TTarget}"/> objects for equality.
        /// Two <see cref="EqualityFunction{TTarget}"/> are considered equal if they accept same
        /// type and consist of same set of equality operations.
        /// </summary>
        /// <param name="obj">Object to compare current with.</param>
        /// <returns>
        /// Flag determining whether <paramref name="obj"/> is equal to current instance.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EqualityFunction<TTarget>) obj);
        }
                
        /// <summary>
        /// Computes <see cref="EqualityFunction{TTarget}"/> hashcode by combining hashes of its'
        /// equality operations. 
        /// </summary>
        public override int GetHashCode()
        {
            return _operations.GetHashCodeElementWise();
        }
    }
}