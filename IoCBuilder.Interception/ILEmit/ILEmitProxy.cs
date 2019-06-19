using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// IL Emitor used by interception
    /// </summary>
    public class ILEmitProxy
    {
        /// <summary>
        /// Calling of interception handlers
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public delegate object InvokeDelegate(object[] arguments);

        private readonly Dictionary<MethodBase, HandlerPipeline> handlers;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlers">Interception handlers</param>
        public ILEmitProxy(IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers)
        {
            this.handlers = new Dictionary<MethodBase, HandlerPipeline>();

            foreach (KeyValuePair<MethodBase, List<IInterceptionHandler>> kvp in handlers)
                this.handlers.Add(kvp.Key, new HandlerPipeline(kvp.Value));
        }

        private HandlerPipeline GetPipeline(MethodInfo method,
                                    object target)
        {
            // Non-generic method
            if (handlers.ContainsKey(method))
                return handlers[method];

            // Generic method
            if (method.IsGenericMethod && handlers.ContainsKey(method.GetGenericMethodDefinition()))
                return handlers[method.GetGenericMethodDefinition()];

            // Non-generic method on generic type with generic base type (virtual method interception)
            if (target.GetType().IsGenericType && target.GetType().BaseType.IsGenericType)
            {
                Type genericTarget = target.GetType().BaseType.GetGenericTypeDefinition();
                MethodInfo methodToLookup = genericTarget.GetMethod(method.Name);

                if (handlers.ContainsKey(methodToLookup))
                    return handlers[methodToLookup];
            }

            // Method in base class has same DeclaringType, but different ReflectedType
            // therefore handlers.ContainsKey doesn't work
            foreach (var key in handlers.Keys)
            {
                if (AreMethodsEqualForDeclaringType(key, method))
                    return handlers[key];
            }

            // Empty pipeline as a last resort
            return new HandlerPipeline();
        }

        private static bool AreMethodsEqualForDeclaringType(MethodBase first, MethodInfo second)
        {
            first = first.ReflectedType == first.DeclaringType ? first : first.DeclaringType.GetMethod(first.Name, first.GetParameters().Select(p => p.ParameterType).ToArray());
            second = second.ReflectedType == second.DeclaringType ? second : second.DeclaringType.GetMethod(second.Name, second.GetParameters().Select(p => p.ParameterType).ToArray());
            return first == second;
        }

        /// <summary>
        /// Invoked during interception
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="method">Intercepted method</param>
        /// <param name="arguments">Passed args</param>
        /// <param name="delegate">Interception handler</param>
        /// <returns></returns>
        public object Invoke(object target,
                             MethodInfo method,
                             object[] arguments,
                             InvokeDelegate @delegate)
        {
            HandlerPipeline pipeline = GetPipeline(method, target);
            MethodInvocation invocation = new MethodInvocation(target, method, arguments);

            IMethodReturn result =
                pipeline.Invoke(invocation,
                                delegate
                                {
                                    try
                                    {
                                        object returnValue = @delegate(arguments);
                                        return new MethodReturn(invocation.Arguments, method.GetParameters(), returnValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        return new MethodReturn(ex, method.GetParameters());
                                    }
                                });

            if (result.Exception != null)
            {
                FieldInfo remoteStackTraceString = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
                remoteStackTraceString.SetValue(result.Exception, result.Exception.StackTrace + Environment.NewLine);
                throw result.Exception;
            }

            return result.ReturnValue;
        }
    }
}