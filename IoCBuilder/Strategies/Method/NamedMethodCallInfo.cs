using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using IoCBuilder.Exceptions;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Method
{
    /// <summary>
    /// Encapsulates a call to a named method.
    /// </summary>
    public class NamedMethodCallInfo : IMethodCallInfo
    {
        private readonly string methodName;
        private readonly List<IParameter> parameters;

        /// <summary>
        /// Name of method
        /// </summary>
        public string MethodName
        {
            get { return methodName; }
        }

        /// <summary>
        /// Parameters of the method
        /// </summary>
        public List<IParameter> Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        /// Initalize a new instance of the <see cref="NamedMethodCallInfo"/> class with an array of <see cref="IParameter"/> instances.
        /// </summary>
        /// <param name="methodName">The name of the method to execute.</param>
        /// <param name="parameters">An array of <see cref="IParameter"/> instances.</param>
        public NamedMethodCallInfo(string methodName,
                                   params IParameter[] parameters)
            : this(methodName, (IEnumerable<IParameter>)parameters) { }

        /// <summary>
        /// Initalize a new instance of the <see cref="NamedMethodCallInfo"/> class with a collection of <see cref="IParameter"/> instances.
        /// </summary>
        /// <param name="methodName">The name of the method to execute.</param>
        /// <param name="parameters">A collection of <see cref="IParameter"/> instances.</param>
        public NamedMethodCallInfo(string methodName,
                                   IEnumerable<IParameter> parameters)
        {
            this.methodName = methodName;
            this.parameters = new List<IParameter>();

            if (parameters != null)
                this.parameters.AddRange(parameters);
        }

        /// <summary>
        /// ExecuteBuildUp the method to be called.
        /// </summary>
        /// <param name="context">The current <see cref="IBuilderContext"/>.</param>
        /// <param name="instance">The instance to use to execute the method.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        public void Execute(IBuilderContext context,
                            object instance,
                            object buildKey)
        {
            List<Type> parameterTypes = new List<Type>();
            foreach (IParameter parameter in parameters)
                parameterTypes.Add(parameter.GetParameterType(context));

            List<object> parameterValues = new List<object>();
            foreach (IParameter parameter in parameters)
                parameterValues.Add(parameter.GetValue(context));

            MethodInfo method = instance.GetType().GetMethod(methodName, parameterTypes.ToArray());

            if (method != null)
            {
                method.Invoke(instance, parameterValues.ToArray());
            }
            else
            {
                method = instance.GetType().GetMethod(methodName);
                // Detection of missmatched parameters
                if (method != null)
                    Guard.ValidateMethodParameters(method, parameterTypes.ToArray(), instance.GetType());
            }
        }
    }
}