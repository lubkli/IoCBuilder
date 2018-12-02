using System.Collections.Generic;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Method
{
    /// <summary>
    /// Encapsulates a method call.
    /// </summary>
    public interface IMethodCallInfo
    {
        /// <summary>
        /// Name of the method
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// Parameters of the method
        /// </summary>
        List<IParameter> Parameters { get; }

        /// <summary>
        /// ExecuteBuildUp the method to be called.
        /// </summary>
        /// <param name="context">The current <see cref="IBuilderContext"/>.</param>
        /// <param name="instance">The instance to use to execute the method.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        void Execute(IBuilderContext context,
                     object instance,
                     object buildKey);
    }
}