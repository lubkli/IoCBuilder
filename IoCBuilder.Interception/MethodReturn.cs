using System;
using System.Reflection;

namespace IoCBuilder.Interception
{
    internal class MethodReturn : IMethodReturn
    {
        private Exception exception;
        private readonly OutputParameterCollection outputs;
        private object returnValue;

        public MethodReturn(object[] arguments,
                            ParameterInfo[] parameters,
                            object returnValue)
        {
            this.returnValue = returnValue;

            exception = null;
            outputs = new OutputParameterCollection(arguments, parameters);
        }

        public MethodReturn(Exception exception,
                            ParameterInfo[] parameters)
            : this(new object[0], parameters, null)
        {
            this.exception = exception;
        }

        public Exception Exception
        {
            get { return exception; }
            set { exception = value; }
        }

        public IParameterCollection Outputs
        {
            get { return outputs; }
        }

        public object ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }
    }
}