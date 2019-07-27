using System;
using System.Collections.Generic;
using System.Linq;
using BoilerplateBuilders.Utils;
using Operation = BoilerplateBuilders.Reflection.MemberContext<System.Func<object, int>>;

namespace BoilerplateBuilders.Reflection.HashCode
{
    /// <summary>
    /// Computes hashcode for an object by combining results of configured objects' members hash codes.
    /// </summary>
    /// <typeparam name="TTarget">Type of object to compute hashcode for.</typeparam>
    public class HashCodeFunction<TTarget>
    {
        private readonly int _seed, _step;
        
        private readonly ISet<Operation> _operations;

        internal HashCodeFunction(IEnumerable<Operation> operations, int seed = 0, int step = 397)
        {
            _seed = seed;
            _step = step;
            
            _operations = new OrderedHashSet<Operation>(
                operations ?? throw new ArgumentNullException(nameof(operations))
            );
        }
        
        /// <summary>
        /// Computes hashcode for an object by combining results of configured members hash codes.
        /// </summary>
        /// <param name="target">Type of member to compute hashcode for.</param>
        /// <returns>Computed hashcode</returns>
        public int GetHashCode(TTarget target)
        {
            int ComputeHashCode(Operation op) =>
                op.Context(op.Member.Getter(target));
            
            return _operations
                .Select(ComputeHashCode)
                .GetSequenceHashCode(_seed, _step);
        }
        
        private bool Equals(HashCodeFunction<TTarget> other)
        {
            return 
                _seed == other._seed 
                   && _step == other._step 
                   && _operations.SetEquals(other._operations);
        }
        
        /// <summary>
        /// Compares two instances of <see cref="HashCodeFunction{TTarget}"/> for equality.
        /// Two hashcode functions are considered equal if they both accept object of the type,
        /// consist of the same set of a member functions and have same initialization values. 
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>
        /// Flag indicating whether <paramref name="obj"/> equals to current instance.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HashCodeFunction<TTarget>) obj);
        }

        /// <summary>
        /// Computes builder hashcode by combining hashes of its' member functions.
        /// </summary>
        public override int GetHashCode()
        {
            return _operations.GetSequenceHashCode(_seed, _step);
        }
    }
}