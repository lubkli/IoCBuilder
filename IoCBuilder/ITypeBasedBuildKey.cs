using System;

namespace IoCBuilder
{
    /// <summary>
    /// Represents a type part of build key.
    /// </summary>
    public interface ITypeBasedBuildKey
    {
        /// <summary>
        /// Return the <see cref="Type"/> stored in the build key.
        /// </summary>
        Type Type { get; }
    }
}