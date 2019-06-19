using System;
using System.Collections.Generic;
using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Interceptor base on Remoting
    /// </summary>
    public class RemotingInterceptor
    {
        /// <summary>
        /// Creates wrapper on top of original type.
        /// </summary>
        /// <param name="obj">Original object.</param>
        /// <param name="typeToWrap">Original type.</param>
        /// <param name="handlers">List of interception handlers.</param>
        /// <returns>New type.</returns>
        public static object Wrap(object obj,
                                  Type typeToWrap,
                                  IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers)
        {
            return new RemotingProxy(obj, typeToWrap, handlers).GetTransparentProxy();
        }

        /// <summary>
        /// Creates wrapper on top of original type.
        /// </summary>
        /// <typeparam name="T">Original type.</typeparam>
        /// <param name="obj">Original object.</param>
        /// <param name="handlers">List of interception handlers.</param>
        /// <returns>New type.</returns>
        public static T Wrap<T>(T obj,
                                IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers)
        {
            return (T)Wrap(obj, typeof(T), handlers);
        }
    }
}
