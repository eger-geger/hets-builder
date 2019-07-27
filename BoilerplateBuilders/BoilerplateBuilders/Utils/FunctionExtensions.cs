using System;

namespace BoilerplateBuilders.Utils
{
    /// <summary>
    /// Holds functions operating on functions. 
    /// </summary>
    public static class FunctionExtensions
    {
        /// <summary>
        /// Converts function signature to accept more specific argument subtype.
        /// </summary>
        /// <param name="function">Function accepting single arguments.</param>
        /// <typeparam name="TGeneric">Provided function argument type.</typeparam>
        /// <typeparam name="TArg">Resulting function argument type.</typeparam>
        /// <typeparam name="TRet">Function return type.</typeparam>
        /// <returns>Same function but with updated signature.</returns>
        public static Func<TArg, TRet> ToSpecific<TGeneric, TArg, TRet>(this Func<TGeneric, TRet> function)
            where TArg : TGeneric => arg => function(arg);
             
        /// <summary>
        /// Converts function signature to one with more generic argument and return types.
        /// Even though new function declares argument of more generic type, it is still just a copy of original function
        /// so passing argument incompatible with it will cause error <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="function">Function to change signature of.</param>
        /// <typeparam name="TArg">Original function argument type.</typeparam>
        /// <typeparam name="TReturn">Original function return type.</typeparam>
        /// <typeparam name="TGenericArg">Resulting function argument type.</typeparam>
        /// <typeparam name="TGenericReturn">Resulting function return type.</typeparam>
        /// <returns>Copy of a function with updated signature.</returns>
        public static Func<TGenericArg, TGenericReturn> ToGeneric<TArg, TReturn, TGenericArg, TGenericReturn>(
            this Func<TArg, TReturn> function) 
            where TReturn : TGenericReturn
            where TArg : TGenericArg
            => o => function((TArg) o);
        
        /// <summary>
        /// Converts function signature to one with more generic arguments and return types.
        /// Even though new function declares arguments of more generic type, it is still just a copy of original function
        /// so passing argument incompatible with it will cause error <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="function">Function to change signature of.</param>
        /// <typeparam name="TArg">Type of original function arguments.</typeparam>
        /// <typeparam name="TGenericArg">Type of resulting function arguments.</typeparam>
        /// <returns>Copy of a function with updated signature.</returns>
        public static Func<TGenericArg, TGenericArg, bool> ToGeneric<TArg, TGenericArg>(this Func<TArg, TArg, bool> function)
            where TArg : TGenericArg
            => (a, b) => function((TArg) a, (TArg) b);
    }
}