using System.Collections.Generic;

namespace IoCBuilder.Strategies.Method
{
    /// <summary>
    /// Represents a policy for <see cref="MethodCallStrategy"/>.
    /// </summary>
    public class MethodCallPolicy : IMethodCallPolicy
    {
        private readonly List<IMethodCallInfo> methods = new List<IMethodCallInfo>();

        /// <summary>
        /// Gets a collection of methods to be called on the object instance.
        /// </summary>
        /// <value>
        /// A collection of methods to be called on the object instance.
        /// </value>
        public List<IMethodCallInfo> Methods
        {
            get { return methods; }
        }
    }
}