using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.ToString;
using BoilerplateBuilders.ToString.Primitives;
using BoilerplateBuilders.Utils;

namespace BoilerplateBuilders
{
    /// <summary>
    /// Builds <see cref="object.ToString"/> function accepting objects of given type (<typeparamref name="TTarget"/>).
    /// </summary>
    /// <typeparam name="TTarget">Type of object function is being built for.</typeparam>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class ToStringBuilder<TTarget> : AbstractBuilder<TTarget, ToStringBuilder<TTarget>, Func<object, string>>
    {
        private ObjectFormat _objectFormat = ObjectFormat.CreateDefault();

        /// <summary>
        /// Includes referenced field or property into list of members used by <see cref="object.ToString"/>
        /// function being built.
        /// If field or property was added previously (implicitly or explicitly) than overrides function used for
        /// converting that member`s value to string.
        /// </summary>
        /// <param name="getter">Expression pointing to field or property.</param>
        /// <param name="toString">Function returning string representation of chosen member value.</param>
        /// <typeparam name="TMember">Type of selected field or property.</typeparam>
        /// <returns>Updated <see cref="ToStringBuilder{TTarget}"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="getter"/> is not field or property accessor expression.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="toString"/> is null.</exception>
        public ToStringBuilder<TTarget> Append<TMember>(
            Expression<Func<TTarget, TMember>> getter,
            Func<TMember, string> toString
        )
        {
            return AppendExplicit(getter, toString?.ToGeneric<TMember, string, object, string>());
        }

        /// <summary>
        /// Instructs builder to use a given function for all selected (previously or later)
        /// members which values are assignable to type <typeparamref name="T"/>.
        /// Members added with explicit <see cref="object.ToString"/> function (e.g.: <see cref="Append{TMember}"/>)
        /// are not affected by this call.
        /// </summary>
        /// <param name="toString">
        /// Function returning string representation of an object of type <typeparamref name="T"/>.
        /// </param>
        /// <typeparam name="T">Type of object converted to string.</typeparam>
        /// <returns>Updated <see cref="ToStringBuilder{TTarget}"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="toString"/> is null.</exception>
        public ToStringBuilder<TTarget> Use<T>(Func<T, string> toString)
        {
            return OverrideContextForType(typeof(T), toString?.ToGeneric<T, string, object, string>());
        }

        /// <summary>
        /// Use <see cref="CollectionFormat"/> for converting members of <see cref="ICollection"/> type
        /// to string. Only affects members which declared types are assignable to <see cref="ICollection"/>. 
        /// </summary>
        /// <param name="setup">
        /// Function accepting default <see cref="CollectionFormat"/> and returning modified version or brand new
        /// instance to be used by <see cref="object.ToString"/> function being built. When it returns null than
        /// default <see cref="CollectionFormat"/> (the one passed to it) will be used.
        /// <see cref="CollectionFormat.CreateDefault"/> will be used when function is omitted (or null passed).
        /// </param>
        /// <returns>Updated <see cref="ToStringBuilder{TTarget}"/> instance.</returns>
        public ToStringBuilder<TTarget> UseCollectionFormat(
            Func<CollectionFormat, CollectionFormat> setup = null
        )
        {
            return Use<ICollection>(SetupFormat(CollectionFormat.CreateDefault(), setup).Compile());
        }

        /// <summary>
        /// Use <see cref="DictionaryFormat"/> for converting members of <see cref="IDictionary"/> type
        /// to string. Only affects members which declared types are assignable to <see cref="IDictionary"/>. 
        /// </summary>
        /// <param name="setup">
        /// Function accepting default <see cref="DictionaryFormat"/> and returning modified version or brand new
        /// instance to be used by <see cref="object.ToString"/> function being built. When it returns null than
        /// default <see cref="DictionaryFormat"/> (the one passed to it) will be used.
        /// <see cref="DictionaryFormat.CreateDefault"/> will be used when function is omitted (or null passed).
        /// </param>
        /// <returns>Updated <see cref="ToStringBuilder{TTarget}"/> instance.</returns>
        public ToStringBuilder<TTarget> UseDictionaryFormat(
            Func<DictionaryFormat, DictionaryFormat> setup = null
        )
        {
            return Use(SetupFormat(DictionaryFormat.CreateDefault(), setup).Compile());
        }

        /// <summary>
        /// Use <see cref="DictionaryFormat"/> for converting members of <see cref="IDictionary{TKey,TValue}"/> type
        /// to string. Only affects members which declared types are assignable to <see cref="IDictionary{TKey,TValue}"/> with matching
        /// <typeparamref name="TKey"/> and <typeparamref name="TValue"/>. 
        /// </summary>
        /// <param name="setup">
        /// Function accepting default <see cref="DictionaryFormat"/> and returning modified version or brand new
        /// instance to be used by <see cref="object.ToString"/> function being built. When it returns null than
        /// default <see cref="DictionaryFormat"/> passed to it will be used. <see cref="DictionaryFormat.CreateDefault"/>
        /// will be used when function is omitted (or null passed). 
        /// </param>
        /// <typeparam name="TKey">Type of keys of affected dictionaries.</typeparam>
        /// <typeparam name="TValue">Type of values of affected dictionaries.</typeparam>
        /// <returns>Updated <see cref="ToStringBuilder{TTarget}"/> instance.</returns>
        public ToStringBuilder<TTarget> UseDictionaryFormat<TKey, TValue>(
            Func<DictionaryFormat, DictionaryFormat> setup = null
        )
        {
            return Use(SetupFormat(DictionaryFormat.CreateDefault(), setup).Compile<TKey, TValue>());
        }
        
        /// <summary>
        /// Overrides current <see cref="ObjectFormat"/> controlling how selected object members will rendered to string.
        /// </summary>
        /// <param name="setup">
        /// Function accepting current <see cref="ObjectFormat"/>. It is expected to return updated or brand new instance
        /// to be used by <see cref="object.ToString"/> function being built. When it returns null the previously
        /// used <see cref="ObjectFormat"/> (the one passed to setup function) will be used. 
        /// </param>
        /// <returns>Updated <see cref="ToStringBuilder{TTarget}"/> instance.</returns>
        public ToStringBuilder<TTarget> UseObjectFormat(Func<ObjectFormat, ObjectFormat> setup)
        {
            _objectFormat = SetupFormat(_objectFormat, setup);
            return this;
        }

        private static T SetupFormat<T>(T format, Func<T, T> setup) where T : class
        {
            return setup?.Invoke(format) ?? format;
        }
        
        /// <summary>
        /// Generates final <see cref="object.ToString"/> function.
        /// </summary>
        /// <returns>Function returning string representation of <typeparamref name="TTarget"/> object.</returns>
        public Func<TTarget, string> Build()
        {
            return _objectFormat.Compile(GetMemberContexts()).ToSpecific<object, TTarget, string>();
        }

        /// <summary>
        /// Determines a function to use for converting member value to string when one was not explicitly given.
        /// </summary>
        /// <param name="member">Member which value need to be converted to string.</param>
        /// <returns>Function returning string representation of a member value.</returns>
        protected override Func<object, string> GetImplicitContext(SelectedMember member) => ToStringFunctions.ToString;
    }
}