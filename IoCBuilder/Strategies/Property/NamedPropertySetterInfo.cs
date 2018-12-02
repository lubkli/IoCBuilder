using System;
using System.Globalization;
using System.Reflection;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Property
{
    /// <summary>
    /// Encapsulates a call to a named property.
    /// </summary>
    public class NamedPropertySetterInfo : IPropertySetterInfo
    {
        private readonly string propertyName;
        private readonly IParameter value;

        /// <summary>
        /// Initialize a new instance of the <see cref="NamedPropertySetterInfo"/> class with the property name and the parameter value.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The paramer value.</param>
        public NamedPropertySetterInfo(string propertyName,
                                       IParameter value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }

        /// <summary>
        /// Name of the property
        /// </summary>
        public string PropertyName
        {
            get
            {
                return propertyName;
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
        /// Sets the value on the property.
        /// </summary>
        /// <param name="context">The current <see cref="IBuilderContext"/>.</param>
        /// <param name="instance">The instance to use to execute the method.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        // FxCop suppression: validation is done by Guard class.
        public void Set(IBuilderContext context,
                        object instance,
                        object buildKey)
        {
            PropertyInfo property = instance.GetType().GetProperty(propertyName);

            if (!property.CanWrite)
                throw new ArgumentException(String.Format(
                            CultureInfo.CurrentCulture,
                    Resources.ResourceManager.GetString("CannotInjectReadOnlyProperty"),
                            instance, property.Name));

            object o = value.GetValue(context);

            if (o != null)
                Guard.TypeIsAssignableFromType(property.PropertyType, o.GetType(), instance.GetType());

            property.SetValue(instance, o, null);
        }
    }
}