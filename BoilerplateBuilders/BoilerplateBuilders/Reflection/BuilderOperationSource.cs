namespace BoilerplateBuilders.Reflection
{
    /// <summary>
    /// Indicates how/why builder operation was created. 
    /// </summary>
    public enum BuilderOperationSource
    {
        /// <summary>
        /// Operation was chosen implicitly by builder based on member information and it's settings. 
        /// </summary>
        Implicit,
        
        /// <summary>
        /// Operation was specified explicitly for current member.
        /// </summary>
        ExplicitMember,
        
        /// <summary>
        /// Operation was specified explicitly for specific type/interface (and all subtypes/implementations).
        /// </summary>
        ExplicitType
    }
}