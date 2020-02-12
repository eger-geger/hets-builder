namespace BoilerplateBuilders.Reflection
{
    /// <summary>
    /// Indicates how/why <see cref="MemberContext{TContext}"/> was created. 
    /// </summary>
    public enum ContextSource
    {
        /// <summary>
        /// Was chosen implicitly by builder based on member information and it's settings. 
        /// </summary>
        Implicit,
        
        /// <summary>
        /// Was provided explicitly for specific member.
        /// </summary>
        ExplicitMember,
        
        /// <summary>
        /// Was provided explicitly for specific type/interface (and all subtypes/implementations).
        /// </summary>
        ExplicitType
    }
}