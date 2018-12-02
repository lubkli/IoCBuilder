using System.Collections.Generic;

namespace IoCBuilder.Strategies.Method
{
    /// <summary>
    /// Represents a policy for <see cref="MethodCallStrategy"/>.
    /// </summary>
    public interface IMethodCallPolicy : IBuilderPolicy
    {
        /// <summary>
        /// Gets a collection of methods to be called on the object instance.
        /// </summary>
        /// <value>
        /// A collection of methods to be called on the object instance.
        /// </value>
        List<IMethodCallInfo> Methods { get; }
    }
}