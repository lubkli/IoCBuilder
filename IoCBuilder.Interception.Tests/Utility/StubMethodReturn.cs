using System;

namespace IoCBuilder.Interception
{
    public class StubMethodReturn : IMethodReturn
    {
        private Exception exception;
        private IParameterCollection outputs;
        private object returnValue;

        public Exception Exception
        {
            get { return exception; }
            set { exception = value; }
        }

        public IParameterCollection Outputs
        {
            get { return outputs; }
            set { outputs = value; }
        }

        public object ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }
    }
}