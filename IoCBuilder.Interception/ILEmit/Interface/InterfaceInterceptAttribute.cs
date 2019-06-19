using System;
using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Class used for marking members for interception and handler association.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class InterfaceInterceptAttribute : InterceptAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlerType">Type of handler</param>
        public InterfaceInterceptAttribute(Type handlerType)
            : base(handlerType) { }

        /// <summary>
        /// Returns builder policy.
        /// </summary>
        public override Type PolicyConcreteType
        {
            get { return typeof(InterfaceInterceptionPolicy); }
        }

        /// <summary>
        /// Returns type of interception policy.
        /// </summary>
        public override Type PolicyInterfaceType
        {
            get { return typeof(IInterfaceInterceptionPolicy); }
        }

        /// <summary>
        /// Returns method for <see cref="InterceptionReflector"/>
        /// </summary>
        /// <param name="typeRequested">Requested type analyzed by reflector.</param>
        /// <param name="method">Analyzed method.</param>
        /// <returns></returns>
        public override MethodBase GetMethodBaseForPolicy(Type typeRequested,
                                                          MethodBase method)
        {
            if (!typeRequested.IsInterface)
                return null;

            ParameterInfo[] paramInfos = method.GetParameters();
            object[] paramTypes = new object[paramInfos.Length];

            for (int idx = 0; idx < paramInfos.Length; ++idx)
                if (paramInfos[idx].ParameterType.IsGenericParameter)
                    paramTypes[idx] = paramInfos[idx].ParameterType.Name;
                else
                    paramTypes[idx] = paramInfos[idx].ParameterType;

            return InterfaceInterceptor.FindMethod(method.Name, paramTypes, typeRequested.GetMethods());
        }

        /// <summary>
        /// Validate type classifiers.
        /// </summary>
        /// <param name="typeRequested">Requested type.</param>
        /// <param name="typeBeingBuilt">Type to be build.</param>
        public override void ValidateInterceptionForType(Type typeRequested,
                                                         Type typeBeingBuilt)
        {
            if (!typeRequested.IsInterface)
                return;

            if (!typeRequested.IsPublic && !typeRequested.IsNestedPublic)
                throw new InvalidOperationException("Interface " + typeRequested.FullName + " must be public to be intercepted");
        }
    }
}