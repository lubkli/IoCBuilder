using IoCBuilder.Lifetime;

namespace IoCBuilder.Strategies.Singleton
{
    /// <summary>
    /// Implementation of <see cref="IBuilderStrategy"/> which allows objects to be
    /// singletons.
    /// </summary>
    /// <remarks>
    /// This strategy looks for policies in the context registered under the interface type
    /// <see cref="ISingletonPolicy"/>. It uses the locator in the build context to rememeber
    /// singleton objects, and the lifetime container contained in the locator to ensure they
    /// are not garbage collected. Upon the second request for an object, it will short-circuit
    /// the strategy chain and return the singleton instance (and will not re-inject the
    /// object).
    /// </remarks>
    public class SingletonStrategy : BuilderStrategy
    {
        /// <summary>
        /// Implementation of <see cref="IBuilderStrategy.BuildUp"/>.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The build key of the object being built.</param>
        /// <param name="existing">The existing instance of the object.</param>
        /// <returns>The built object.</returns>
        public override object BuildUp(IBuilderContext context, object buildKey, object existing)
        {
            ILifetimeContainer lifetime = context.Locator.Get<ILifetimeContainer>(typeof(ILifetimeContainer), SearchMode.Local);

            if (context.Locator == null || lifetime == null) // TODO: Remove lifetime readout from locator (we will use it from context) and then replace prev condition for lifetime with this || context.Lifetime == null
                return base.BuildUp(context, buildKey, existing);

            if (context.Locator.Contains(buildKey, SearchMode.Local))
            {
                TraceBuildUp(context, buildKey, Resources.ResourceManager.GetString("SingletonReturned"));
                return context.Locator.Get(buildKey);
            }

            return base.BuildUp(context, buildKey, existing);
        }
    }
}