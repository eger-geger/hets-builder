using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BoilerplateBuilders.Utils
{
    /// <summary>
    /// HashSet which preserves order of added elements. 
    /// </summary>
    /// <typeparam name="T">Type of set elements.</typeparam>
    internal class OrderedHashSet<T> : ISet<T>
    {
        private readonly LinkedList<T> _list;
        private readonly HashSet<T> _set;

        public OrderedHashSet() : this(Enumerable.Empty<T>())
        {
        }

        public OrderedHashSet(IEnumerable<T> source)
        {
            _set = new HashSet<T>(_list = new LinkedList<T>(source));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            if (item != null && _set.Add(item)) _list.AddLast(item);
        }

        public void Clear()
        {
            _set.Clear();
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _set.Remove(item) && _list.Remove(item);
        }

        public int Count => _set.Count;

        public bool IsReadOnly => false;

        bool ISet<T>.Add(T item)
        {
            if (!_set.Add(item)) return false;
            
            _list.AddLast(item);
            
            return true;

        }

        public void ExceptWith(IEnumerable<T> other)
        {
            DelegateSetUpdate(s => s.ExceptWith(other));
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            DelegateSetUpdate(s => s.IntersectWith(other));
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            DelegateSetUpdate(s => s.SymmetricExceptWith(other));
        }

        public void UnionWith(IEnumerable<T> other)
        {
            DelegateSetUpdate(s => s.UnionWith(other));
        }

        private void DelegateSetUpdate(Action<ISet<T>> update)
        {
            var copy = new HashSet<T>(_set);
            
            update(_set);

            foreach (var item in copy.Except(_set)) _list.Remove(item);
            foreach (var item in _set.Except(copy)) _list.AddLast(item);
        }
    }
}