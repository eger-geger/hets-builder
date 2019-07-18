using System;

namespace BoilerplateBuilders.Reflection
{
    /// <summary>
    /// Information about operation executed on field or property as part of a function being built.
    /// </summary>
    /// <typeparam name="TFunction">Type of function applied to member value.</typeparam>
    public class MemberFunction<TFunction> where TFunction : class
    {
        /// <summary>
        /// Initializes new member operation by specifying both member and operation.
        /// </summary>
        /// <param name="member">Pointer to field or property to which value function will be applied.</param>
        /// <param name="function">Function applied to object member value as part of final function being built.</param>
        /// <param name="memberFunctionSource">Type of action triggered creation of current member operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="member"/> or <paramref name="function"/> is null.</exception>
        public MemberFunction(SelectedMember member, TFunction function, MemberFunctionSource memberFunctionSource)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Function = function ?? throw new ArgumentNullException(nameof(function));
            MemberFunctionSource = memberFunctionSource;
        }

        /// <summary>
        /// Pointer to field or property to which value <see cref="Function"/> will be applied.
        /// </summary>        
        public SelectedMember Member { get; }
        
        /// <summary>
        /// Function applied to <see cref="Member"/> value as part of final function being built.
        /// </summary>
        public TFunction Function { get; }
        
        /// <summary>
        /// Type of action triggered creation of current member operation.
        /// </summary>
        public MemberFunctionSource MemberFunctionSource { get; }
    }
}