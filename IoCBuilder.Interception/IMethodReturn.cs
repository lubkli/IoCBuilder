using System;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Interface for business class representing method's return artifacts.
    /// </summary>
    public interface IMethodReturn
    {
        /// <summary>
        /// Return exception.
        /// </summary>
        Exception Exception { get; set; }

        /// <summary>
        /// Output parameters.
        /// </summary>
        IParameterCollection Outputs { get; }

        /// <summary>
        /// Return value.
        /// </summary>
        object ReturnValue { get; set; }
    }
}