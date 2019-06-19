using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Class performs interception using virtual members overriding.
    /// </summary>
    public class VirtualInterceptionStrategy : BuilderStrategy
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
            IVirtualInterceptionPolicy interceptionPolicy = context.Policies.Get<IVirtualInterceptionPolicy>(buildKey);
            Type typeToBuild;

            if (creationPolicy != null &&
                creationPolicy.SupportsReflection &&
                interceptionPolicy != null &&
                TryGetTypeFromBuildKey(buildKey, out typeToBuild))
            {
                ConstructorInfo ctor = creationPolicy.GetConstructor(context, buildKey);
                object[] ctorParams = creationPolicy.GetParameters(context, ctor);

                string nameToBuild;
                TryGetNameFromBuildKey(buildKey, out nameToBuild);

                buildKey = InterceptClass(context, typeToBuild, nameToBuild, interceptionPolicy, ctorParams);
            }

            return base.BuildUp(context, buildKey, existing);
        }

        private static object InterceptClass(IBuilderContext context,
                                   Type typeToBuild,
                                   string nameToBuild,
                                   IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers,
                                   object[] originalParameters)
        {
            // Create a wrapper class which derives from the intercepted class
            typeToBuild = VirtualInterceptor.WrapClass(typeToBuild);

            // Create the proxy that's used by the wrapper
            ILEmitProxy proxy = new ILEmitProxy(handlers);

            // Create a new policy which calls the proper constructor
            List<Type> newParameterTypes = new List<Type>();
            List<IParameter> newIParameters = new List<IParameter>();

            newParameterTypes.Add(typeof(ILEmitProxy));
            newIParameters.Add(new ValueParameter<ILEmitProxy>(proxy));

            foreach (object obj in originalParameters)
            {
                newParameterTypes.Add(obj.GetType());
                newIParameters.Add(new ValueParameter(obj.GetType(), obj));
            }

            ConstructorInfo newConstructor = typeToBuild.GetConstructor(newParameterTypes.ToArray());
            ConstructorCreationPolicy newPolicy = new ConstructorCreationPolicy(newConstructor, newIParameters.ToArray());

            BuildKey buildKey = new BuildKey(typeToBuild, nameToBuild);

            context.Policies.Set<ICreationPolicy>(newPolicy, buildKey);

            // Return the wrapped type for building in BuildKey
            return buildKey;
        }
    }
}