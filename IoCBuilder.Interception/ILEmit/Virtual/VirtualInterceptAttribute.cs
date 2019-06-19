using System;
using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Class used for marking members for interception and handler association.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VirtualInterceptAttribute : InterceptAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlerType">Type of handler</param>
        public VirtualInterceptAttribute(Type handlerType)
            : base(handlerType) { }

        /// <summary>
        /// Returns builder policy.
        /// </summary>
        public override Type PolicyConcreteType
        {
            get { return typeof(VirtualInterceptionPolicy); }
        }

        /// <summary>
        /// Returns type of interception policy.
        /// </summary>
        public override Type PolicyInterfaceType
        {
            get { return typeof(IVirtualInterceptionPolicy); }
        }

        /// <summary>
        /// Validate method classifiers.
        /// </summary>
        /// <param name="method">Validating method.</param>
        public override void ValidateInterceptionForMethod(MethodBase method)
        {
            if (!method.IsVirtual || method.IsFinal)
                throw new InvalidOperationException("Method " + method.DeclaringType.FullName + "." + method.Name + " must be virtual and not sealed to be intercepted.");
        }

        /// <summary>
        /// Validate type classifiers.
        /// </summary>
        /// <param name="typeRequested">Requested type.</param>
        /// <param name="typeBeingBuilt">Type to be build.</param>
        public override void ValidateInterceptionForType(Type typeRequested,
                                                         Type typeBeingBuilt)
        {
            if ((!typeBeingBuilt.IsPublic && !typeBeingBuilt.IsNestedPublic) || typeBeingBuilt.IsSealed)
                throw new InvalidOperationException("Type " + typeBeingBuilt.FullName + " must be public and not sealed.");
        }
    }
}