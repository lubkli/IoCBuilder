using System.Collections.Generic;

namespace IoCBuilder.Strategies.Property
{
    /// <summary>
    /// Implementation of <see cref="IPropertySetterPolicy"/>.
    /// </summary>
    public class PropertySetterPolicy : IPropertySetterPolicy
    {
        private readonly Dictionary<string, IPropertySetterInfo> properties = new Dictionary<string, IPropertySetterInfo>();

        /// <summary>
        /// Gets a collection of properties to be called on the object instance.
        /// </summary>
        public Dictionary<string, IPropertySetterInfo> Properties
        {
            get { return properties; }
        }
    }
}