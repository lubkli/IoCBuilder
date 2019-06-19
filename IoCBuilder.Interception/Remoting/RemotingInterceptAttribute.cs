using System;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Class used for marking members for interception and handler association.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RemotingInterceptAttribute : InterceptAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlerType">Type of handler</param>
        public RemotingInterceptAttribute(Type handlerType)
            : base(handlerType) { }

        /// <summary>
        /// Returns builder policy.
        /// </summary>
        public override Type PolicyConcreteType
        {
            get { return typeof(RemotingInterceptionPolicy); }
        }

        /// <summary>
        /// Returns type of interception policy.
        /// </summary>
        public override Type PolicyInterfaceType
        {
            get { return typeof(IRemotingInterceptionPolicy); }
        }

        /// <summary>
        /// Validate type classifiers.
        /// </summary>
        /// <param name="typeRequested">Requested type.</param>
        /// <param name="typeBeingBuilt">Type to be build.</param>
        public override void ValidateInterceptionForType(Type typeRequested,
                                                         Type typeBeingBuilt)
        {
            if (!typeof(MarshalByRefObject).IsAssignableFrom(typeBeingBuilt))
                throw new InvalidOperationException("Type " + typeBeingBuilt.FullName + " must derive from MarshalByRefObject to be intercepted.");
        }
    }
}