using System;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Class reflects type of object for interception
    /// and sets policies for later use.
    /// </summary>
    public class InterceptionReflectionStrategy : BuilderStrategy
    {
        /// <summary>
        /// Override of <see cref="IBuilderStrategy.BuildUp"/>. Reflects type of object for interception
        /// and sets policies for later use.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The build key of object to be intercepted.</param>
        /// <param name="existing">The existing object, which is null.</param>
        /// <returns>The intercepted object.</returns>
        public override object BuildUp(IBuilderContext context,
                                       object buildKey,
                                       object existing)
        {
            Type typeToBuild;

            if (TryGetTypeFromBuildKey(buildKey, out typeToBuild))
            {
                Type originalType;

                if (!TryGetTypeFromBuildKey(context.OriginalBuildKey, out originalType))
                    originalType = typeToBuild;

                string nameToBuild;
                TryGetNameFromBuildKey(buildKey, out nameToBuild);

                IFactory factory = context.Locator.Get<IFactory>();

                if (factory != null)
                    InterceptionReflector.Reflect(originalType, typeToBuild, nameToBuild, context.Policies, factory);
            }

            return base.BuildUp(context, buildKey, existing);
        }
    }
}