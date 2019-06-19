using System.Collections.Generic;
using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Represents a base interface of policies for interception strategies.
    /// </summary>
    public interface IInterceptionPolicy : IBuilderPolicy, IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>>
    {
        /// <summary>
        /// Count of policies
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Indexer returning list of <see cref="IInterceptionHandler"/> for <see cref="MethodBase"/>
        /// </summary>
        /// <param name="method">The <see cref="MethodBase"/> for which the list of <see cref="IInterceptionHandler"/> is required</param>
        /// <returns>List of <see cref="IInterceptionHandler"/>.</returns>
        IList<IInterceptionHandler> this[MethodBase method] { get; }

        /// <summary>
        /// Gets <see cref="IEnumerable{T}"/> of registered <see cref="MethodBase"/>.
        /// </summary>
        IEnumerable<MethodBase> Methods { get; }
    }
}