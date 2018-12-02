using System.Collections.Generic;

namespace IoCBuilder.Strategies.Property
{
    /// <summary>
    /// Represents a policy for <see cref="PropertySetterStrategy"/>. The properties are
    /// indexed by the name of the property.
    /// </summary>
    public interface IPropertySetterPolicy : IBuilderPolicy
    {
        /// <summary>
        /// Gets a collection of properties to be called on the object instance.
        /// </summary>
        Dictionary<string, IPropertySetterInfo> Properties { get; }
    }
}