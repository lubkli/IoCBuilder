using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Interface for business class representing method's invocation artifacts.
    /// </summary>
    public interface IMethodInvocation
    {
        /// <summary>
        /// Collection of arguments.
        /// </summary>
        IParameterCollection Arguments { get; }

        /// <summary>
        /// Collection of parameters.
        /// </summary>
        IParameterCollection Inputs { get; }

        /// <summary>
        /// Method to invoke.
        /// </summary>
        MethodBase MethodBase { get; }

        /// <summary>
        /// Target object.
        /// </summary>
        object Target { get; }
    }
}