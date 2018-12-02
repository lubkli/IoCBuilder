using System;

namespace IoCBuilder.Strategies.Parameters
{
    /// <summary>
    /// Represents a single parameter used for constructor and method calls, and
    /// property setting.
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// Gets the type of the parameter value.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <returns>The parameter's type.</returns>
        Type GetParameterType(IBuilderContext context);

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <returns>The parameter's value.</returns>
        object GetValue(IBuilderContext context);
    }
}