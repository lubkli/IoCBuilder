using System;
using System.Reflection;

namespace IoCBuilder.Strategies
{
    /// <summary>
    /// Represents the member inforamation for a <see cref="PropertyInfo"/>.
    /// </summary>
    public class PropertyMemberInfo : IMemberInfo<PropertyInfo>
    {
        private readonly PropertyInfo prop;

        /// <summary>
        /// Initialize a new instance of the <see cref="PropertyMemberInfo"/> class with a <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="prop">The <see cref="PropertyInfo"/> to initialize the class.</param>
        public PropertyMemberInfo(PropertyInfo prop)
        {
            this.prop = prop;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo MemberInfo
        {
            get { return prop; }
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name
        {
            get { return prop.Name; }
        }

        /// <summary>
        /// Gets the custom attributes for the property.
        /// </summary>
        /// <param name="attributeType">The <see cref="Type"/> of the attributes to get from the property.</param>
        /// <param name="inherit">true to get inherited attrubutes; otherwise, false.</param>
        /// <returns>An array of the custom attributes.</returns>
        public object[] GetCustomAttributes(Type attributeType,
                                            bool inherit)
        {
            return prop.GetCustomAttributes(attributeType, inherit);
        }

        /// <summary>
        /// The parameters for the property.
        /// </summary>
        /// <returns>An array of <see cref="ParameterInfo"/> objects.</returns>
        public ParameterInfo[] GetParameters()
        {
            return new ParameterInfo[] { new CustomPropertyParameterInfo(prop) };
        }

        private class CustomPropertyParameterInfo : ParameterInfo
        {
            private readonly PropertyInfo prop;

            public CustomPropertyParameterInfo(PropertyInfo prop)
            {
                this.prop = prop;
            }

            public override Type ParameterType
            {
                get { return prop.PropertyType; }
            }

            public override object[] GetCustomAttributes(Type attributeType,
                                                         bool inherit)
            {
                return prop.GetCustomAttributes(attributeType, inherit);
            }
        }
    }
}