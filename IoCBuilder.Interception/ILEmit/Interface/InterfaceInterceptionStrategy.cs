using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Class performs interception over interface
    /// </summary>
    public class InterfaceInterceptionStrategy : BuilderStrategy
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
            ICreationPolicy creationPolicy = context.Policies.Get<ICreationPolicy>(buildKey);
            IInterfaceInterceptionPolicy interceptionPolicy = context.Policies.Get<IInterfaceInterceptionPolicy>(buildKey);
            Type typeToBuild;

            if (creationPolicy != null &&
                creationPolicy.SupportsReflection &&
                interceptionPolicy != null &&
                TryGetTypeFromBuildKey(buildKey, out typeToBuild))
            {
                ConstructorInfo ctor = creationPolicy.GetConstructor(context, buildKey);
                object[] ctorParams = creationPolicy.GetParameters(context, ctor);
                Type originalType;

                if (!TryGetTypeFromBuildKey(context.OriginalBuildKey, out originalType))
                    originalType = typeToBuild;

                string nameToBuild;
                TryGetNameFromBuildKey(buildKey, out nameToBuild);

                buildKey = InterceptInterface(context, typeToBuild, originalType, nameToBuild, interceptionPolicy, ctor, ctorParams);
            }

            return base.BuildUp(context, buildKey, existing);
        }

        private static object InterceptInterface(IBuilderContext context,
                                       Type typeToBuild,
                                       Type originalType,
                                       string nameToBuild,
                                       IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers,
                                       ConstructorInfo ctor,
                                       object[] ctorParams)
        {
            // Create a wrapper class which implements the interface
            typeToBuild = InterfaceInterceptor.WrapInterface(originalType);

            // Create an instance of the concrete class using the policy data
            object wrappedObject = ctor.Invoke(ctorParams);

            // Create the proxy that's used by the wrapper
            ILEmitProxy proxy = new ILEmitProxy(handlers);

            // Create a new policy which calls the proper constructor
            ConstructorInfo newConstructor = typeToBuild.GetConstructor(new Type[] { typeof(ILEmitProxy), originalType });
            ConstructorCreationPolicy newPolicy =
                new ConstructorCreationPolicy(newConstructor,
                                              new ValueParameter<ILEmitProxy>(proxy),
                                              new ValueParameter(originalType, wrappedObject));

            BuildKey buildKey = new BuildKey(typeToBuild, nameToBuild);

            context.Policies.Set<ICreationPolicy>(newPolicy, buildKey);

            // Return the wrapped type for building in BuildKey
            return buildKey;
        }
    }
}