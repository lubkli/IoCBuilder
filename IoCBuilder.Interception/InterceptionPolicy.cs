using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Represents an abstract class of policies for interception strategies.
    /// </summary>
    public abstract class InterceptionPolicy : IInterceptionPolicy
    {
        private readonly Dictionary<MethodBase, List<IInterceptionHandler>> handlers = new Dictionary<MethodBase, List<IInterceptionHandler>>();

        /// <summary>
        /// Count of policies
        /// </summary>
        public int Count
        {
            get { return handlers.Count; }
        }

        /// <summary>
        /// Indexer returning list of <see cref="IInterceptionHandler"/> for <see cref="MethodBase"/>
        /// </summary>
        /// <param name="method">The <see cref="MethodBase"/> for which the list of <see cref="IInterceptionHandler"/> is required</param>
        /// <returns>List of <see cref="IInterceptionHandler"/>.</returns>
        public IList<IInterceptionHandler> this[MethodBase method]
        {
            get { return handlers[method].AsReadOnly(); }
        }

        /// <summary>
        /// Gets <see cref="IEnumerable{T}"/> of registered <see cref="MethodBase"/>.
        /// </summary>
        public IEnumerable<MethodBase> Methods
        {
            get { return handlers.Keys; }
        }

        /// <summary>
        /// Register <see cref="IEnumerable{T}"/> of <see cref="IInterceptionHandler"/> for <see cref="MethodBase"/>.
        /// </summary>
        /// <param name="method"><see cref="MethodBase"/> of registration for.</param>
        /// <param name="methodHandlers"> <see cref="IEnumerable{T}"/> of <see cref="IInterceptionHandler"/></param>
        public void Add(MethodBase method,
                        IEnumerable<IInterceptionHandler> methodHandlers)
        {
            handlers[method] = new List<IInterceptionHandler>(methodHandlers);
        }

        /// <summary>
        /// Register <see cref="IEnumerable{T}"/> of <see cref="IInterceptionHandler"/> for <see cref="MethodBase"/>.
        /// </summary>
        /// <param name="method"><see cref="MethodBase"/> of registration for.</param>
        /// <param name="methodHandlers">Array of <see cref="IInterceptionHandler"/></param>
        public void Add(MethodBase method,
                        params IInterceptionHandler[] methodHandlers)
        {
            handlers[method] = new List<IInterceptionHandler>(methodHandlers);
        }

        /// <summary>
        /// Returns enumerator of <see cref="MethodBase"/> and list of <see cref="IInterceptionHandler"/> pairs.
        /// </summary>
        /// <returns>Enumerator of <see cref="KeyValuePair{TKey, TValue}"/>.</returns>
        public IEnumerator<KeyValuePair<MethodBase, List<IInterceptionHandler>>> GetEnumerator()
        {
            return handlers.GetEnumerator();
        }

        /// <summary>
        /// Returns enumerator of <see cref="MethodBase"/> and list of <see cref="IInterceptionHandler"/> pairs.
        /// </summary>
        /// <returns><see cref="IEnumerator"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}