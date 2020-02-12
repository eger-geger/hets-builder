using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using BoilerplateBuilders.Reflection;
using BoilerplateBuilders.ToString;
using BoilerplateBuilders.Utils;

namespace BoilerplateBuilders
{
    /// <summary>
    /// Builds <see cref="object.ToString"/> function.
    /// </summary>
    /// <typeparam name="TTarget">Type of object function is being built for.</typeparam>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class ToStringBuilder<TTarget> : AbstractBuilder<TTarget, ToStringBuilder<TTarget>, Func<object, string>>
    {
        private ObjectFormatterBuilder _objectFormatterBuilder = ObjectFormatterBuilder.CreateDefault();

        private readonly CollectionFormatterBuilder _collectionFormatterBuilder = CollectionFormatterBuilder.CreateDefault();

        private readonly DictionaryFormatterBuilder _dictionaryFormatterBuilder = DictionaryFormatterBuilder.CreateDefault();

        /// <summary>
        /// Includes referenced field or property into list of members available to <see cref="object.ToString"/>
        /// function being built.
        /// If field or property was added previously than overrides function used for converting member value to string.
        /// </summary>
        /// <param name="fieldOrPropertyGetter">Expression pointing to field or property to include.</param>
        /// <param name="toString">Function returning string representation of chosen member value.</param>
        /// <typeparam name="TMember">Type of selected field or property.</typeparam>
        /// <returns>Updated builder instance.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="fieldOrPropertyGetter"/> is not field or property accessor expression.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="toString"/> is null.</exception>
        public ToStringBuilder<TTarget> Append<TMember>(
            Expression<Func<TTarget, TMember>> fieldOrPropertyGetter,
            Func<TMember, string> toString
        )
        {
            return AppendExplicit(fieldOrPropertyGetter, toString?.ToGeneric<TMember, string, object, string>());
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
        /// <returns>Updated builder instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="toString"/> is null.</exception>
        public ToStringBuilder<TTarget> Use<T>(Func<T, string> toString)
        {
            return OverrideContextForType(typeof(T), toString?.ToGeneric<T, string, object, string>());
        }

        public ToStringBuilder<TTarget> UseCollectionFormatter(
            Func<CollectionFormatterBuilder, CollectionFormatterBuilder> setup = null
        )
        {
            return Use<ICollection>(SetupOrDefault(_collectionFormatterBuilder, setup).BuildToString());
        }

        public ToStringBuilder<TTarget> UseDictionaryFormatter(
            Func<DictionaryFormatterBuilder, DictionaryFormatterBuilder> setup = null
        )
        {
            return Use(SetupOrDefault(_dictionaryFormatterBuilder, setup).BuildToString());
        }

        public ToStringBuilder<TTarget> UseDictionaryFormatter<TKey, TValue>(
            Func<DictionaryFormatterBuilder, DictionaryFormatterBuilder> setup = null
        )
        {
            return Use(SetupOrDefault(_dictionaryFormatterBuilder, setup).BuildToString<TKey, TValue>());
        }
        
        /// <summary>
        /// Overrides current <see cref="ObjectFormatterFactory"/> with given value or sets it to <see cref="DefaultFormatterFactory"/>
        /// when <paramref name="formatterFactory"/> is null.
        /// </summary>
        /// <param name="formatterFactory">New format.</param>
        /// <returns>Updated builder instance.</returns>
        public ToStringBuilder<TTarget> UseObjectFormatter(Func<ObjectFormatterBuilder, ObjectFormatterBuilder> setup)
        {
            _objectFormatterBuilder = SetupOrDefault(_objectFormatterBuilder, setup);
            return this;
        }

        private static T SetupOrDefault<T>(T defaultFormatterBuilder, Func<T, T> setup) where T : class
        {
            return setup?.Invoke(defaultFormatterBuilder) ?? defaultFormatterBuilder;
        }
        
        /// <summary>
        /// Generates final <see cref="object.ToString"/> function.
        /// </summary>
        /// <returns>Function returning string representation of <typeparamref name="TTarget"/> object.</returns>
        public Func<TTarget, string> BuildToString()
        {
            return _objectFormatterBuilder.BuildToString(GetMemberContexts()).ToSpecific<object, TTarget, string>();
        }

        /// <summary>
        /// Determines a function to use for converting member value to string when one was not explicitly given.
        /// </summary>
        /// <param name="member">Member which value need to be converted to string.</param>
        /// <returns>Function returning string representation of a member value.</returns>
        protected override Func<object, string> GetImplicitContext(SelectedMember member)
        {
            return o => o?.ToString();
        }
    }
}