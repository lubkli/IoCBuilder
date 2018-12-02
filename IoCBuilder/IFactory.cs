using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoCBuilder
{
    /// <summary>
    /// Represents the main interface for an object factory.
    /// </summary>
    public interface IFactory
    {
        /// <summary>
        /// Returns object of required type.
        /// </summary>
        /// <param name="typeToBuild">Required type.</param>
        /// <returns>Created or retrieved and injected object.</returns>
        object Get(Type typeToBuild);

        /// <summary>
        /// Returns object of required type.
        /// </summary>
        /// <typeparam name="TToBuild">Required type.</typeparam>
        /// <returns>Created or retrieved and injected object.</returns>
        TToBuild Get<TToBuild>();

        /// <summary>
        /// Performs dependency injection on <see cref="object"/>
        /// </summary>
        /// <param name="object">Object to be injected.</param>
        /// <returns>Injected object.</returns>
        object Inject(object @object);
    }
}