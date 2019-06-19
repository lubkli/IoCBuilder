using System;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Class performs interception using Remote Proxy.
    /// </summary>
    public class RemotingInterceptionStrategy : BuilderStrategy
    {
        /// <summary>
        /// Override of <see cref="IBuilderStrategy.BuildUp"/>. Search prepared policy
        /// from <see cref="InterceptionReflectionStrategy"/> and when succeed provides interception.
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

                IRemotingInterceptionPolicy policy = context.Policies.Get<IRemotingInterceptionPolicy>(buildKey);

                if (existing != null && policy != null)
                    existing = RemotingInterceptor.Wrap(existing, originalType, policy);
            }

            return base.BuildUp(context, buildKey, existing);
        }
    }
}