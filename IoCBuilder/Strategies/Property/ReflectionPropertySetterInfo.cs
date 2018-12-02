using System;
using System.Reflection;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Property
{
    /// <summary>
    /// Encapsulates a reflection property setting.
    /// </summary>
    public class ReflectionPropertySetterInfo : IPropertySetterInfo
    {
        private readonly PropertyInfo property;
        private readonly IParameter value;
        /// <summary>
        /// Initialize a new instance of the <see cref="ReflectionPropertySetterInfo"/> class with a property and paramter value.
        /// </summary>
        /// <param name="property">The property to use to set the value.</param>
        /// <param name="value">The value for the property.</param>

        public ReflectionPropertySetterInfo(PropertyInfo property,
                                            IParameter value)
        {
            this.property = property;
            this.value = value;
        }

        /// <summary>
        /// Gets the property for the setter call.
        /// </summary>
        public PropertyInfo Property
        {
            get { return property; }
        }

        /// <summary>
        /// Name of the property
        /// </summary>
        public string PropertyName
        {
            get
            {
                return property.Name;
            }
        }

        /// <summary>
        /// Value of the property
        /// </summary>
        public object PropertyValue
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Gets the value for the property.
        /// </summary>
        public IParameter Value
        {
            get { return value; }
        }

        /// <summary>
        /// Sets the value on the property.
        /// </summary>
        /// <param name="context">The current <see cref="IBuilderContext"/>.</param>
        /// <param name="instance">The instance to use to execute the method.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        public void Set(IBuilderContext context,
                        object instance,
                        object buildKey)
        {
            Property.SetValue(instance, Value.GetValue(context), null);
        }
    }
}