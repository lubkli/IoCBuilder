using System;
using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Abstract class used for marking members for interception and handler association.
    /// </summary>
    public abstract class InterceptAttribute : Attribute
    {
        private readonly Type handlerType;

        /// <summary>
        /// Create marker and associate handler.
        /// </summary>
        /// <param name="handlerType">Associated handler.</param>
        public InterceptAttribute(Type handlerType)
        {
            this.handlerType = handlerType;
        }

        /// <summary>
        /// Gets handler type.
        /// </summary>
        public Type HandlerType
        {
            get { return handlerType; }
        }

        /// <summary>
        /// Gets policy concrete type used during interception.
        /// </summary>
        public abstract Type PolicyConcreteType { get; }

        /// <summary>
        /// Gets policy interface type used during interception.
        /// </summary>
        public abstract Type PolicyInterfaceType { get; }

        /// <summary>
        /// Returns <see cref="MethodBase"/> for policy.
        /// </summary>
        /// <param name="typeRequested">Required type.</param>
        /// <param name="method"><see cref="MethodBase"/> for interception.</param>
        /// <returns><see cref="MethodBase"/> for policy.</returns>
        public virtual MethodBase GetMethodBaseForPolicy(Type typeRequested,
                                                         MethodBase method)
        {
            return method;
        }

        /// <summary>
        /// Validate <see cref="MethodBase"/> for interception.
        /// </summary>
        /// <param name="method"><see cref="MethodBase"/> for interception.</param>
        public virtual void ValidateInterceptionForMethod(MethodBase method)
        {
        }

        /// <summary>
        /// Validate <see cref="Type"/> for interception.
        /// </summary>
        /// <param name="typeRequested">Requested <see cref="Type"/></param>
        /// <param name="typeBeingBuilt"><see cref="Type"/> which is being built.</param>
        public virtual void ValidateInterceptionForType(Type typeRequested,
                                                        Type typeBeingBuilt)
        { }
    }
}