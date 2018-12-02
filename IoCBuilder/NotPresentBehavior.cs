namespace IoCBuilder
{
    /// <summary>
    /// Enumeration describing how to handle when a dependency is not present.
    /// </summary>
    public enum NotPresentBehavior
    {
        /// <summary>
        /// Create the object
        /// </summary>
        CreateNew,

        /// <summary>
        /// Return null
        /// </summary>
        ReturnNull,

        /// <summary>
        /// Throw a DependencyMissingException
        /// </summary>
        Throw,
    }
}