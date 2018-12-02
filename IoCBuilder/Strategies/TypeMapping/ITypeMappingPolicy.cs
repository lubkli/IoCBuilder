namespace IoCBuilder.Strategies.TypeMapping
{
    /// <summary>
    /// Represents a policy for <see cref="TypeMappingStrategy"/>.
    /// </summary>
    public interface ITypeMappingPolicy : IBuilderPolicy
    {
        /// <summary>
        /// Maps one Type/ID pair to another.
        /// </summary>
        /// <param name="buildKey">The incoming Type/ID pair.</param>
        /// <returns>The new Type/ID pair.</returns>
        object Map(object buildKey);
    }
}