using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Class for invoking methods.
    /// </summary>
    public class MethodInvocation : IMethodInvocation
    {
        private readonly ParameterCollection allParams;
        private readonly object[] arguments;
        private readonly InputParameterCollection inputParams;
        private readonly MethodBase methodBase;
        private readonly object target;

        /// <summary>
        /// Invoke method.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="methodBase">Method to invoke.</param>
        /// <param name="arguments">Method's parameters.</param>
        public MethodInvocation(object target,
                                MethodBase methodBase,
                                object[] arguments)
        {
            this.target = target;
            this.methodBase = methodBase;
            this.arguments = arguments;

            ParameterInfo[] paramInfos = methodBase.GetParameters();
            inputParams = new InputParameterCollection(arguments, paramInfos);
            allParams = new ParameterCollection(arguments, paramInfos);
        }

        IParameterCollection IMethodInvocation.Arguments
        {
            get { return allParams; }
        }

        /// <summary>
        /// Method's parameters.
        /// </summary>
        public object[] Arguments
        {
            get { return arguments; }
        }

        /// <summary>
        /// Collection of input parameters and it's infos.
        /// </summary>
        public IParameterCollection Inputs
        {
            get { return inputParams; }
        }

        /// <summary>
        /// Method to invoke.
        /// </summary>
        public MethodBase MethodBase
        {
            get { return methodBase; }
        }

        /// <summary>
        /// Target object.
        /// </summary>
        public object Target
        {
            get { return target; }
        }
    }
}