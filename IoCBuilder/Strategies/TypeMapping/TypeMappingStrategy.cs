using System;

namespace IoCBuilder.Strategies.TypeMapping
{
    /// <summary>
    /// Implementation of <see cref="IBuilderStrategy"/> which remaps type and ID.
    /// </summary>
    /// <remarks>
    /// This strategy looks for policies in the context registered under the interface type
    /// <see cref="ITypeMappingPolicy"/>. The purpose of this strategy is to allow a user to
    /// ask for some generic type (say, "the IDatabase with an ID of 'sales'") and have it
    /// mapped into the appropriate concrete type (say, "an instance of SalesDatabase").
    /// </remarks>
    public class TypeMappingStrategy : BuilderStrategy
    {
        /// <summary>
        /// Implementation of <see cref="IBuilderStrategy.BuildUp"/>.
        /// </summary>
        public override object BuildUp(IBuilderContext context, object buildKey, object existing)
        {
            object result = buildKey;
            ITypeMappingPolicy policy = context.Policies.Get<ITypeMappingPolicy>(buildKey);

            if (policy != null)
            {
                result = policy.Map(buildKey);

                Type t = BuilderStrategy.GetTypeFromBuildKey(buildKey);
                Type n = BuilderStrategy.GetTypeFromBuildKey(result);

                TraceBuildUp(context, buildKey, Resources.ResourceManager.GetString("TypeMapped"), new object[] { t, n });

                Guard.TypeIsAssignableFromType(t, n, t);
            }

            return base.BuildUp(context, result, existing);
        }
    }
}