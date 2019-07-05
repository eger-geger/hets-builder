using System;

namespace BoilerplateBuilders.Utils
{
    public static class FunctionExtensions
    {
        public static Func<TArg, TRet> ToSpecific<TArg, TRet>(this Func<object, TRet> f) =>
            arg => f(arg); 
        
        public static Func<object, TGenericReturn> ToGeneric<TArg, TReturn, TGenericReturn>(
            this Func<TArg, TReturn> fn
        ) where TReturn : TGenericReturn
            => o => (TGenericReturn) fn((TArg) o);
        
        public static Func<object, object, bool> ToGeneric<TArg>(
            this Func<TArg, TArg, bool> fn
        ) => (a, b) => fn((TArg) a, (TArg) b);

        public static Func<(TArg1, TArg2, TArg3), TResult> ToTupled<TArg1, TArg2, TArg3, TResult>(
            Func<TArg1, TArg2, TArg3, TResult> fn
        ) => tuple => fn(tuple.Item1, tuple.Item2, tuple.Item3);
    }
}