using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Policies;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Reflects type and set policies for later usage by strategies.
    /// </summary>
    public static class InterceptionReflector
    {
        /// <summary>
        /// Reflects type and set policies for later usage by strategies.
        /// </summary>
        /// <typeparam name="TBeingBuilt"></typeparam>
        /// <param name="policies"><see cref="IPolicyList"/> which is extended by reflection.</param>
        /// <param name="factory"><see cref="IFactory"/> used for object creation.</param>
        /// <param name="nameBeingBuild">Name part of <see cref="BuildKey"/></param>
        public static void Reflect<TBeingBuilt>(IPolicyList policies,
                                                IFactory factory,
                                                string nameBeingBuild = null)
        {
            Reflect(typeof(TBeingBuilt), typeof(TBeingBuilt), nameBeingBuild, policies, factory);
        }

        /// <summary>
        /// Reflects type and set policies for later usage by strategies.
        /// </summary>
        /// <typeparam name="TRequested"></typeparam>
        /// <typeparam name="TBeingBuilt"></typeparam>
        /// <param name="policies"><see cref="IPolicyList"/> which is extended by reflection.</param>
        /// <param name="factory"><see cref="IFactory"/> used for object creation.</param>
        /// <param name="nameBeingBuild">Name part of <see cref="BuildKey"/></param>
        public static void Reflect<TRequested, TBeingBuilt>(IPolicyList policies,
                                                            IFactory factory,
                                                            string nameBeingBuild = null)
            where TBeingBuilt : TRequested
        {
            Reflect(typeof(TRequested), typeof(TBeingBuilt), nameBeingBuild, policies, factory);
        }

        /// <summary>
        /// Reflects type and set policies for later usage by strategies.
        /// </summary>
        /// <param name="typeRequested">Requested <see cref="Type"/>.</param>
        /// <param name="typeBeingBuilt"><see cref="Type"/> being built.</param>
        /// <param name="nameBeingBuild">Name part of <see cref="BuildKey"/></param>
        /// <param name="policyList"><see cref="IPolicyList"/> which is extended by reflection.</param>
        /// <param name="factory"><see cref="IFactory"/> used for object creation.</param>
        public static void Reflect(Type typeRequested,
                                   Type typeBeingBuilt,
                                   string nameBeingBuild,
                                   IPolicyList policyList,
                                   IFactory factory)
        {
            if (typeRequested.IsGenericType && typeBeingBuilt.IsGenericType)
            {
                typeRequested = typeRequested.GetGenericTypeDefinition();
                typeBeingBuilt = typeBeingBuilt.GetGenericTypeDefinition();
            }

            Dictionary<Type, InterceptionPolicy> typePolicies = new Dictionary<Type, InterceptionPolicy>();

            foreach (MethodInfo method in typeBeingBuilt.GetMethods())
                ReflectOnMethod(typeRequested, typeBeingBuilt, typePolicies, method, factory);

            foreach (KeyValuePair<Type, InterceptionPolicy> kvp in typePolicies)
                policyList.Set(kvp.Key, kvp.Value, new BuildKey(typeBeingBuilt, nameBeingBuild));
        }

        private static void ReflectOnMethod(Type typeRequested,
                                    Type typeBeingBuilt,
                                    IDictionary<Type, InterceptionPolicy> typePolicies,
                                    MethodBase method,
                                    IFactory factory)
        {
            Dictionary<KeyValuePair<Type, Type>, List<IInterceptionHandler>> methodHandlers = new Dictionary<KeyValuePair<Type, Type>, List<IInterceptionHandler>>();
            MethodBase methodForPolicy = method;

            foreach (InterceptAttribute attr in method.GetCustomAttributes(typeof(InterceptAttribute), true))
            {
                KeyValuePair<Type, Type> key = new KeyValuePair<Type, Type>(attr.PolicyInterfaceType, attr.PolicyConcreteType);

                if (!methodHandlers.ContainsKey(key))
                {
                    if (!typePolicies.ContainsKey(attr.PolicyInterfaceType))
                        attr.ValidateInterceptionForType(typeRequested, typeBeingBuilt);

                    attr.ValidateInterceptionForMethod(method);

                    methodForPolicy = attr.GetMethodBaseForPolicy(typeRequested, method);
                    if (methodForPolicy == null)
                        return;

                    methodHandlers[key] = new List<IInterceptionHandler>();
                }

                methodHandlers[key].Add((IInterceptionHandler)factory.Get(attr.HandlerType));
            }

            foreach (KeyValuePair<Type, Type> key in methodHandlers.Keys)
            {
                if (!typePolicies.ContainsKey(key.Key))
                    typePolicies[key.Key] = (InterceptionPolicy)factory.Get(key.Value);

                typePolicies[key.Key].Add(methodForPolicy, methodHandlers[key]);
            }
        }
    }
}