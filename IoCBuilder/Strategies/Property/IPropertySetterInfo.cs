namespace IoCBuilder.Strategies.Property
{
    /// <summary>
    /// Encapsulates a property setter.
    /// </summary>
    public interface IPropertySetterInfo
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Value of the property
        /// </summary>
        object PropertyValue { get; }

        /// <summary>
        /// Sets the value on the property.
        /// </summary>
        /// <param name="context">The current <see cref="IBuilderContext"/>.</param>
        /// <param name="instance">The instance to use to execute the method.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        void Set(IBuilderContext context,
                 object instance,
                 object buildKey);
    }
}