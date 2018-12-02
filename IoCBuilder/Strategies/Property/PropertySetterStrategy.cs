namespace IoCBuilder.Strategies.Property
{
    /// <summary>
    /// Implementation of <see cref="IBuilderStrategy"/> which sets property values.
    /// </summary>
    /// <remarks>
    /// This strategy looks for policies in the context registered under the interface type
    /// <see cref="IPropertySetterPolicy"/>, and sets the property values. If no policy is
    /// found, the no property values are set.
    /// </remarks>
    public class PropertySetterStrategy : BuilderStrategy
    {
        /// <summary>
        /// Called during the chain of responsibility for a build operation.  Looks for the <see cref="IPropertySetterPolicy"/> and
        /// sets the value for the property if found.
        /// </summary>
        /// <param name="context">The context for the operation.</param>
        /// <param name="buildKey">The key for the operation.</param>
        /// <param name="existing">The instance for the operation.</param>
        public override object BuildUp(IBuilderContext context,
                                       object buildKey,
                                       object existing)
        {
            IPropertySetterPolicy policy = context.Policies.Get<IPropertySetterPolicy>(buildKey);

            if (existing != null)
            {
                if (policy == null)
                {
                    string id = null;
                    BuilderStrategy.TryGetNameFromBuildKey(buildKey, out id);
                    BuildKey newKey = new BuildKey(existing.GetType(), id);

                    policy = context.Policies.Get<IPropertySetterPolicy>(newKey);
                }

                if (policy != null)
                {
                    foreach (IPropertySetterInfo property in policy.Properties.Values)
                    {
                        TraceBuildUp(context, buildKey, Resources.ResourceManager.GetString("CallingProperty"), property.PropertyName, property.PropertyValue);
                        property.Set(context, existing, buildKey);
                    }
                }
            }

            return base.BuildUp(context, buildKey, existing);
        }
    }
}