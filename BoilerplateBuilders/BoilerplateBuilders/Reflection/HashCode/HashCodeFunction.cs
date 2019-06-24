using System;
using System.Collections.Generic;
using System.Linq;
using Operation = BoilerplateBuilders.Reflection.BuilderMemberOperation<BoilerplateBuilders.Reflection.HashCode.GetHashCodeDelegate>;

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
            _operations = new SortedSet<Operation>(
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
            return _operations
                .Select(op => ComputeHashCode(op, target))
                .GetItemWiseHashCode(_seed, _step);
        }

        private static int ComputeHashCode(Operation op, TTarget target)
        {
            return op.Function(op.Member.Getter(target));
        }

        private bool Equals(HashCodeFunction<TTarget> other)
        {
            return _seed == other._seed && _step == other._step && _operations.Equals(other._operations);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HashCodeFunction<TTarget>) obj);
        }

        public override int GetHashCode()
        {
            return _operations.GetItemWiseHashCode(_seed, _step);
        }
    }
}