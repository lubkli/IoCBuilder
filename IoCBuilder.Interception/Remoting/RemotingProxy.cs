
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IoCBuilder.Interception
{
    public class RemotingProxy : DispatchProxy
    {
        public object Wrapped { get; set; }
        public Action<MethodInfo, object[]> Start { get; set; }
        public Action<MethodInfo, object[], object> End { get; set; }


        public RemotingProxy(object target,
                             Type typeToProxy,
                             IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers)
        {

        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            Start?.Invoke(targetMethod, args);
            object result = targetMethod.Invoke(Wrapped, args);
            End?.Invoke(targetMethod, args, result);
            return result;
        }

        internal object GetTransparentProxy()
        {
            return this;
        }
    }


    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        IEcho toWrap = new EchoImpl();
    //        IEcho decorator = DispatchProxy.Create<IEcho, GenericDecorator>();
    //        ((GenericDecorator)decorator).Wrapped = toWrap;
    //        ((GenericDecorator)decorator).Start = (tm, a) => Console.WriteLine($"{tm.Name}({string.Join(',', a)}) is started");
    //        ((GenericDecorator)decorator).End = (tm, a, r) => Console.WriteLine($"{tm.Name}({string.Join(',', a)}) is ended with result {r}");
    //        string result = decorator.Echo("Hello");
    //    }

    //    class EchoImpl : IEcho
    //    {
    //        public string Echo(string message) => message;
    //    }

    //    interface IEcho
    //    {
    //        string Echo(string message);
    //    }
    //}
}

#if NET40
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Provides functionality for proxies.
    /// </summary>
    public class RemotingProxy : RealProxy, IRemotingTypeInfo
    {
        private readonly Dictionary<MethodBase, HandlerPipeline> handlers;
        private readonly object target;
        private readonly Type typeOfTarget;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="typeToProxy"></param>
        /// <param name="handlers"></param>
        public RemotingProxy(object target,
                             Type typeToProxy,
                             IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers)
            : base(typeToProxy)
        {
            this.target = target;
            this.handlers = new Dictionary<MethodBase, HandlerPipeline>();

            foreach (KeyValuePair<MethodBase, List<IInterceptionHandler>> kvp in handlers)
                this.handlers.Add(kvp.Key, new HandlerPipeline(kvp.Value));

            typeOfTarget = target.GetType();
        }

        /// <summary>
        /// Target object
        /// </summary>
        public object Target
        {
            get { return target; }
        }

        string IRemotingTypeInfo.TypeName
        {
            get { return typeOfTarget.FullName; }
            set { throw new NotImplementedException(); }
        }

        bool IRemotingTypeInfo.CanCastTo(Type fromType,
                                         object obj)
        {
            return fromType.IsAssignableFrom(obj.GetType());
        }

        /// <summary>
        /// Execute pipe line
        /// </summary>
        /// <param name="msg">Message with <see cref="IMethodCallMessage"/>.</param>
        /// <returns>Returns <see cref="ReturnMessage"/>.</returns>
        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMessage = (IMethodCallMessage)msg;
            HandlerPipeline pipeline;

            if (handlers.ContainsKey(callMessage.MethodBase))
                pipeline = handlers[callMessage.MethodBase];
            else
                pipeline = new HandlerPipeline();

            MethodInvocation invocation = new MethodInvocation(target, callMessage.MethodBase, callMessage.Args);

            IMethodReturn result =
                pipeline.Invoke(invocation,
                                delegate
                                {
                                    try
                                    {
                                        object returnValue = callMessage.MethodBase.Invoke(target, invocation.Arguments);
                                        return new MethodReturn(invocation.Arguments, callMessage.MethodBase.GetParameters(), returnValue);
                                    }
                                    catch (TargetInvocationException ex)
                                    {
                                        return new MethodReturn(ex.InnerException, callMessage.MethodBase.GetParameters());
                                    }
                                });

            if (result.Exception == null)
                return new ReturnMessage(result.ReturnValue, invocation.Arguments, invocation.Arguments.Length,
                                         callMessage.LogicalCallContext, callMessage);

            return new ReturnMessage(result.Exception, callMessage);
        }
    }
}
#endif