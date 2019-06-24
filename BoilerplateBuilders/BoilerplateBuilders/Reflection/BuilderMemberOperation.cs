using System;

namespace BoilerplateBuilders.Reflection
{
    /// <summary>
    /// Information about operation executed on field or property by builder.
    /// </summary>
    /// <typeparam name="TFunction">Type of function applied to member value.</typeparam>
    public class BuilderMemberOperation<TFunction> where TFunction : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="function"></param>
        /// <param name="builderOperationSource"></param>
        public BuilderMemberOperation(BuilderMember member, TFunction function, BuilderOperationSource builderOperationSource)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Function = function ?? throw new ArgumentNullException(nameof(function));
            BuilderOperationSource = builderOperationSource;
        }

        /// <summary>
        /// Information about field or property the function is applied to. 
        /// </summary>        
        public BuilderMember Member { get; }
        
        /// <summary>
        /// Actual function applied.
        /// </summary>
        public TFunction Function { get; }
        
        /// <summary>
        /// Indicates why this particular instance was created.
        /// </summary>
        public BuilderOperationSource BuilderOperationSource { get; }
    }
}