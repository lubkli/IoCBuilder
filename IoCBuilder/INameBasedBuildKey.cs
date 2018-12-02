namespace IoCBuilder
{
    /// <summary>
    /// Represents a name part of build key.
    /// </summary>
    public interface INameBasedBuildKey
    {
        /// <summary>
        /// Returns the name stored in the build key.
        /// </summary>
        string Name { get; }
    }
}