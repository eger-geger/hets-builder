//using System;
//using System.Collections.Generic;
//using System.Linq;
//using BoilerplateBuilders.Getter;
//using BoilerplateBuilders.Utils;
//
//namespace BoilerplateBuilders
//{
//    public class EqualityComparerBuilder<T> : AbstractBuilder<T, EqualityComparerBuilder<T>>, IEqualityComparer<T>
//    {
//        private const int HashConst = 397;
//
//        private readonly IDictionary<BasicMember, Func<object, object, bool>> _equalityFunctions = 
//            new Dictionary<BasicMember, Func<object, object, bool>>();
//        
//        public bool Equals(T x, T y)
//        {
//            if (ReferenceEquals(x, y)) return true;
//            if (x == null || y == null) return false;
//            
//            return Members
//                .Select(getter => (getter.Apply(x), getter.Apply(y), getter))
//                .All(xy => ObjectsEquals(xy.Item1, xy.Item2, xy.Item3));
//        }
//        
//        private bool ObjectsEquals(object x, object y, BasicMember getter)
//        {
//            if (ReferenceEquals(x, y)) return true;
//            if (x is null || y is null) return false;
//
//            if (!_equalityFunctions.TryGetValue(getter, out var compare))
//            {
//                compare = CreateComparerFunc(getter);
//                _equalityFunctions[getter] = compare;
//            }
//
//            return compare(x, y);
//        }
//
//        private Func<object, object, bool> CreateComparerFunc(BasicMember getter)
//        {
//            if (getter.MemberType.IsSet())
//                return CreateSetComparingFunc(getter.MemberType);
//
//            if (getter.MemberType.IsEnumerable())
//                return CreateSequenceComparingFunc(getter.MemberType);
//
//            return Equals;
//        }
//
//        private static Func<object, object, bool> CreateSetComparingFunc(Type type)
//        {
//            throw new NotImplementedException();
//        }
//
//        private static Func<object, object, bool> CreateSequenceComparingFunc(Type type)
//        {
//            throw new NotImplementedException();
//        }
//        
//        private static bool SequencesEquals<T>(IEnumerable<T> x, IEnumerable<T> y)
//        {
//            return x.Zip(y, ValueTuple.Create).All(xy => Equals(xy.Item1, xy.Item2));
//        }
//
//        private static bool SequencesEqualsIgnoreOrder<T>(IEnumerable<T> x, IEnumerable<T> y)
//        {
//            var ys = y.ToList();
//
//            return x.All(ys.Remove) && ys.Count == 0;
//        }
//        
//        private static bool SetEquals<T>(ISet<T> x, IEnumerable<T> y)
//        {
//            return x.SetEquals(y);
//        }
//        
//        public int GetHashCode(T target)
//        {
//            unchecked
//            {
//                return Members.Aggregate(default(int), (hashCode, getter) => 
//                    hashCode ^ (getter.Apply(target)?.GetHashCode() ?? 0) * HashConst 
//                );
//            }
//        }
//        
//    }
//}