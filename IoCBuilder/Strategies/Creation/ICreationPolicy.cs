using System.Reflection;

namespace IoCBuilder.Strategies.Creation
{
    /// <summary>
    /// Represents a policy for <see cref="CreationStrategy"/>.
    /// </summary>
    public interface ICreationPolicy : IBuilderPolicy
    {
        /// <summary>
        /// Determines if the policy supports reflection.
        /// </summary>
        bool SupportsReflection { get; }

        /// <summary>
        /// Create the object for the given <paramref name="context"/> and <paramref name="buildKey"/>.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        /// <returns></returns>
        object Create(IBuilderContext context,
                      object buildKey);

        /// <summary>
        /// Gets the constructor to be used to create the object.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        /// <returns></returns>
        ConstructorInfo GetConstructor(IBuilderContext context,
                                       object buildKey);

        /// <summary>
        /// Gets the parameter values to be passed to the constructor.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="constructor">The constructor that will be used.</param>
        /// <returns>An array of parameters to pass to the constructor.</returns>
        object[] GetParameters(IBuilderContext context,
                               ConstructorInfo constructor);
    }
}