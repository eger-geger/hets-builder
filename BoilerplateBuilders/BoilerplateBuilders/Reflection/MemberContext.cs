using System;

namespace BoilerplateBuilders.Reflection
{
    /// <summary>
    /// Represents object member selected by builder and additional builder-specific context. 
    /// </summary>
    /// <typeparam name="TContext">Type of context associated with selected member.</typeparam>
    public class MemberContext<TContext>
    {
        /// <summary>
        /// Initializes new member context by providing all required properties.
        /// </summary>
        /// <param name="member">Inforation about member selected by builder.</param>
        /// <param name="context">Builder-specific context associated with selected member.</param>
        /// <param name="source">Indicates what has triggered context creation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="member"/> or <paramref name="context"/> is null.</exception>
        public MemberContext(SelectedMember member, TContext context, ContextSource source)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Context = context;
            Source = source;
        }

        /// <summary>
        /// Inforation about member selected by builder.
        /// </summary>        
        public SelectedMember Member { get; }
        
        /// <summary>
        /// Builder-specific context associated with selected member
        /// </summary>
        public TContext Context { get; }
        
        /// <summary>
        /// Indicates what has triggered current context creation.
        /// </summary>
        public ContextSource Source { get; }
    }
}