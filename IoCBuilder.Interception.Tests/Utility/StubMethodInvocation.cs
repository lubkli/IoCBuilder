using System.Reflection;

namespace IoCBuilder.Interception
{
    public class StubMethodInvocation : IMethodInvocation
    {
        private IParameterCollection arguments;
        private IParameterCollection inputs;
        private MethodBase methodBase;
        private object target;

        public IParameterCollection Arguments
        {
            get { return arguments; }
            set { arguments = value; }
        }

        public IParameterCollection Inputs
        {
            get { return inputs; }
            set { inputs = value; }
        }

        public MethodBase MethodBase
        {
            get { return methodBase; }
            set { methodBase = value; }
        }

        public object Target
        {
            get { return target; }
            set { target = value; }
        }
    }
}