namespace BoilerplateBuilders
{
    /// <summary>
    /// Determines how collections should be compared for equality.
    /// </summary>
    public enum SequenceComparisonMode
    {
        /// <summary>
        /// Sequences are considered equal if they contain same number of same elements in same order.
        /// </summary>
        SameOrder,
        
        /// <summary>
        /// Sequences are considered equal if they contain same number of same elements in arbitrary order.
        /// </summary>
        IgnoreOrder
    }
}