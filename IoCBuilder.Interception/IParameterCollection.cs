using System.Collections;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Exposes an enumerator, which supports a simple iteration over collection of parameters.
    /// </summary>
    public interface IParameterCollection : IEnumerable
    {
        /// <summary>
        /// Indexer returning parametr for specified name.
        /// </summary>
        /// <param name="name">Parameter's name.</param>
        /// <returns>The parameter.</returns>
        object this[string name] { get; set; }

        /// <summary>
        /// Indexer returning parametr for specified position.
        /// </summary>
        /// <param name="idx">Parameter's position.</param>
        /// <returns>The parameter.</returns>
        object this[int idx] { get; set; }
    }
}