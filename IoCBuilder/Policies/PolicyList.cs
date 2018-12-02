using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;

namespace IoCBuilder.Policies
{
    /// <summary>
    /// A custom collection wrapper over <see cref="IBuilderPolicy"/> objects.
    /// </summary>
    public class PolicyList : IPolicyList
    {
        private readonly IPolicyList innerPolicyList;
        private readonly object lockObject = new object();
        private readonly Dictionary<PolicyKey, IBuilderPolicy> policies = new Dictionary<PolicyKey, IBuilderPolicy>();

        /// <summary>
        /// Retruns read-only dictionary of policies.
        /// </summary>
        public IReadOnlyDictionary<PolicyKey, IBuilderPolicy> Policies
        {
            get
            {
                return new ReadOnlyDictionary<PolicyKey, IBuilderPolicy>(policies);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyList"/> class.
        /// </summary>
        public PolicyList()
            : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyList"/> class using the
		/// provided (optional) policies to copy.
        /// </summary>
        /// <param name="innerPolicyList"></param>
        public PolicyList(IPolicyList innerPolicyList)
        {
            this.innerPolicyList = innerPolicyList ?? new NullPolicyList();
        }

        /// <summary>
		/// Adds a bundle of policies into the policy list. Any policies in this list will override
		/// policies that are already in the policy list.
		/// </summary>
		/// <param name="policiesToCopy">The policies to be copied.</param>
        public void AddPolicies(IPolicyList policiesToCopy)
        {
            lock (lockObject)
            {
                if (policiesToCopy != null)
                {
                    foreach (KeyValuePair<PolicyKey, IBuilderPolicy> kvp in policiesToCopy.Policies)
                        policies[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
		/// Returns the number of policies in the list.
		/// </summary>
        public int Count
        {
            get
            {
                lock (lockObject)
                    return policies.Count;
            }
        }

        /// <summary>
		/// Removes an individual policy.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The type the policy was registered as.</typeparam>
		/// <param name="buildKey">The build key the policy applies to.</param>
        public void Clear<TPolicyInterface>(object buildKey)
        {
            Clear(typeof(TPolicyInterface), buildKey);
        }

        /// <summary>
		/// Removes an individual policy.
		/// </summary>
		/// <param name="policyInterface">The type the policy was registered as.</param>
		/// <param name="buildKey">The build key the policy applies to.</param>
        public void Clear(Type policyInterface,
                          object buildKey)
        {
            lock (lockObject)
                policies.Remove(new PolicyKey(policyInterface, buildKey));
        }

        /// <summary>
		/// Removes all policies from the list.
		/// </summary>
        public void ClearAll()
        {
            lock (lockObject)
                policies.Clear();
        }

        /// <summary>
		/// Removes a default policy.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The type the policy was registered as.</typeparam>
        public void ClearDefault<TPolicyInterface>()
        {
            Clear(typeof(TPolicyInterface), null);
        }

        /// <summary>
		/// Removes a default policy.
		/// </summary>
		/// <param name="policyInterface">The type the policy was registered as.</param>
        public void ClearDefault(Type policyInterface)
        {
            Clear(policyInterface, null);
        }

        /// <summary>
		/// Gets an individual policy.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The interface the policy is registered under.</typeparam>
		/// <param name="buildKey">The build key the policy applies to.</param>
		/// <returns>The policy in the list, if present; returns null otherwise.</returns>
        public TPolicyInterface Get<TPolicyInterface>(object buildKey)
            where TPolicyInterface : IBuilderPolicy
        {
            return (TPolicyInterface)Get(typeof(TPolicyInterface), buildKey, false);
        }

        /// <summary>
		/// Gets an individual policy.
		/// </summary>
		/// <param name="policyInterface">The interface the policy is registered under.</param>
		/// <param name="buildKey">The build key the policy applies to.</param>
		/// <returns>The policy in the list, if present; returns null otherwise.</returns>
        public IBuilderPolicy Get(Type policyInterface,
                                  object buildKey)
        {
            return Get(policyInterface, buildKey, false);
        }

        /// <summary>
        /// Gets an individual policy.
        /// </summary>
        /// <typeparam name="TPolicyInterface">The type the policy was registered as.</typeparam>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <param name="localOnly">true if the policy searches local only; otherwise false to seach up the parent chain.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        public TPolicyInterface Get<TPolicyInterface>(object buildKey,
                                                      bool localOnly)
            where TPolicyInterface : IBuilderPolicy
        {
            return (TPolicyInterface)Get(typeof(TPolicyInterface), buildKey, localOnly);
        }

        /// <summary>
        /// Gets an individual policy.
        /// </summary>
        /// <param name="policyInterface">The type the policy was registered as.</param>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <param name="localOnly">true if the policy searches local only; otherwise false to seach up the parent chain.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        public IBuilderPolicy Get(Type policyInterface,
                                  object buildKey,
                                  bool localOnly)
        {
            Type buildType;
            IPolicyList containingPolicyList;

            BuilderStrategy.TryGetTypeFromBuildKey(buildKey, out buildType);

            //if (!BuilderStrategy.TryGetTypeFromBuildKey(buildKey, out buildType) || !buildType.IsGenericType)
            //    return
            //        GetNoDefault(policyInterface, buildKey, localOnly) ??
            //        GetNoDefault(policyInterface, null, localOnly);

            //return
            //    GetNoDefault(policyInterface, buildKey, localOnly) ??
            //    GetNoDefault(policyInterface, buildType.GetGenericTypeDefinition(), localOnly) ??
            //    GetNoDefault(policyInterface, null, localOnly);

            return GetPolicyForKey(policyInterface, buildKey, localOnly, out containingPolicyList) ??
                GetPolicyForOpenGenericKey(policyInterface, buildKey, buildType, localOnly, out containingPolicyList) ??
                GetPolicyForType(policyInterface, buildType, localOnly, out containingPolicyList) ??
                GetPolicyForOpenGenericType(policyInterface, buildType, localOnly, out containingPolicyList) ??
                GetDefaultForPolicy(policyInterface, localOnly, out containingPolicyList);
        }

        private IBuilderPolicy GetPolicyForKey(Type policyInterface, object buildKey, bool localOnly, out IPolicyList containingPolicyList)
        {
            if (buildKey != null)
            {
                return GetNoDefault(policyInterface, buildKey, localOnly, out containingPolicyList);
            }
            containingPolicyList = null;
            return null;
        }

        private IBuilderPolicy GetPolicyForOpenGenericKey(Type policyInterface, object buildKey, Type buildType, bool localOnly, out IPolicyList containingPolicyList)
        {
            if (buildType != null && buildType.GetTypeInfo().IsGenericType)
            {
                return GetNoDefault(policyInterface, ReplaceType(buildKey, buildType.GetGenericTypeDefinition()),
                    localOnly, out containingPolicyList);
            }
            containingPolicyList = null;
            return null;
        }

        private IBuilderPolicy GetPolicyForType(Type policyInterface, Type buildType, bool localOnly, out IPolicyList containingPolicyList)
        {
            if (buildType != null)
            {
                return this.GetNoDefault(policyInterface, buildType, localOnly, out containingPolicyList);
            }
            containingPolicyList = null;
            return null;
        }

        private IBuilderPolicy GetPolicyForOpenGenericType(Type policyInterface, Type buildType, bool localOnly, out IPolicyList containingPolicyList)
        {
            if (buildType != null && buildType.GetTypeInfo().IsGenericType)
            {
                return GetNoDefault(policyInterface, buildType.GetGenericTypeDefinition(), localOnly, out containingPolicyList);
            }
            containingPolicyList = null;
            return null;
        }

        private IBuilderPolicy GetDefaultForPolicy(Type policyInterface, bool localOnly, out IPolicyList containingPolicyList)
        {
            return GetNoDefault(policyInterface, null, localOnly, out containingPolicyList);
        }

        /// <summary>
        ///Get the non default policy.
        /// </summary>
        /// <typeparam name="TPolicyInterface">The type the policy was registered as.</typeparam>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <param name="localOnly">Get the non default policy.</param>
        /// <param name="containingPolicyList">The policy list in the chain that the searched for policy was found in, null if the policy was not found.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        public TPolicyInterface GetNoDefault<TPolicyInterface>(object buildKey,
                                                               bool localOnly,
                                                               out IPolicyList containingPolicyList)
            where TPolicyInterface : IBuilderPolicy
        {
            return (TPolicyInterface)GetNoDefault(typeof(TPolicyInterface), buildKey, localOnly, out containingPolicyList);
        }

        /// <summary>
        ///Get the non default policy.
        /// </summary>
        /// <param name="policyInterface">The type the policy was registered as.</param>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <param name="localOnly">Get the non default policy.</param>
        /// <param name="containingPolicyList">The policy list in the chain that the searched for policy was found in, null if the policy was not found.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        public IBuilderPolicy GetNoDefault(Type policyInterface,
                                           object buildKey,
                                           bool localOnly,
                                           out IPolicyList containingPolicyList)
        {
            containingPolicyList = null;

            lock (lockObject)
            {
                IBuilderPolicy policy;
                if (policies.TryGetValue(new PolicyKey(policyInterface, buildKey), out policy))
                {
                    containingPolicyList = this;
                    return policy;
                }
            }

            if (localOnly)
                return null;

            return innerPolicyList.GetNoDefault(policyInterface, buildKey, localOnly, out containingPolicyList);
        }

        /// <summary>
		/// Sets an individual policy.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The interface to register the policy under.</typeparam>
		/// <param name="policy">The policy to be registered.</param>
		/// <param name="buildKey">The build key the policy applies to.</param>
        public void Set<TPolicyInterface>(TPolicyInterface policy,
                                          object buildKey)
            where TPolicyInterface : IBuilderPolicy
        {
            Set(typeof(TPolicyInterface), policy, buildKey);
        }

        /// <summary>
		/// Sets an individual policy.
		/// </summary>
		/// <param name="policyInterface">The interface to register the policy under.</param>
		/// <param name="policy">The policy to be registered.</param>
		/// <param name="buildKey">The build key the policy applies to.</param>
        public void Set(Type policyInterface,
                        IBuilderPolicy policy,
                        object buildKey)
        {
            lock (lockObject)
                policies[new PolicyKey(policyInterface, buildKey)] = policy;
        }

        /// <summary>
		/// Sets a default policy. When checking for a policy, if no specific individual policy
		/// is available, the default will be used.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The interface to register the policy under.</typeparam>
		/// <param name="policy">The default policy to be registered.</param>
        public void SetDefault<TPolicyInterface>(TPolicyInterface policy)
            where TPolicyInterface : IBuilderPolicy
        {
            Set(typeof(TPolicyInterface), policy, null);
        }

        /// <summary>
		/// Sets a default policy. When checking for a policy, if no specific individual policy
		/// is available, the default will be used.
		/// </summary>
		/// <param name="policyInterface">The interface to register the policy under.</param>
		/// <param name="policy">The default policy to be registered.</param>
        public void SetDefault(Type policyInterface,
                               IBuilderPolicy policy)
        {
            Set(policyInterface, policy, null);
        }

        private static object ReplaceType(object buildKey, Type newType)
        {
            var typeKey = buildKey as Type;
            if (typeKey != null)
            {
                return newType;
            }

            var originalKey = buildKey as BuildKey;
            if (originalKey != null)
            {
                return new BuildKey(newType, originalKey.Name);
            }

            throw new ArgumentException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.ResourceManager.GetString("CannotExtractTypeFromBuildKey"),
                    buildKey),
                "buildKey");
        }

        private class NullPolicyList : IPolicyList
        {
            public IReadOnlyDictionary<PolicyKey, IBuilderPolicy> Policies
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public int Count { get { return 0; } }

            public void AddPolicies(IPolicyList policiesToCopy)
            {
                throw new NotImplementedException();
            }

            public void Clear<TPolicyInterface>(object buildKey)
            {
                throw new NotImplementedException();
            }

            public void Clear(Type policyInterface,
                              object buildKey)
            {
                throw new NotImplementedException();
            }

            public void ClearAll()
            {
                throw new NotImplementedException();
            }

            public void ClearDefault<TPolicyInterface>()
            {
                throw new NotImplementedException();
            }

            public void ClearDefault(Type policyInterface)
            {
                throw new NotImplementedException();
            }

            public TPolicyInterface Get<TPolicyInterface>(object buildKey) where TPolicyInterface : IBuilderPolicy
            {
                return default(TPolicyInterface);
            }

            public IBuilderPolicy Get(Type policyInterface,
                                      object buildKey)
            {
                return null;
            }

            public TPolicyInterface Get<TPolicyInterface>(object buildKey,
                                                          bool localOnly)
                where TPolicyInterface : IBuilderPolicy
            {
                return default(TPolicyInterface);
            }

            public IBuilderPolicy Get(Type policyInterface,
                                      object buildKey,
                                      bool localOnly)
            {
                return null;
            }

            public TPolicyInterface GetNoDefault<TPolicyInterface>(object buildKey,
                                                                   bool localOnly,
                                                                   out IPolicyList containingPolicyList)
                where TPolicyInterface : IBuilderPolicy
            {
                containingPolicyList = this;
                return default(TPolicyInterface);
            }

            public IBuilderPolicy GetNoDefault(Type policyInterface,
                                               object buildKey,
                                               bool localOnly,
                                               out IPolicyList containingPolicyList)
            {
                containingPolicyList = null;
                return null;
            }

            public void Set<TPolicyInterface>(TPolicyInterface policy,
                                              object buildKey)
                where TPolicyInterface : IBuilderPolicy
            {
                throw new NotImplementedException();
            }

            public void Set(Type policyInterface,
                            IBuilderPolicy policy,
                            object buildKey)
            {
                throw new NotImplementedException();
            }

            public void SetDefault<TPolicyInterface>(TPolicyInterface policy)
                where TPolicyInterface : IBuilderPolicy
            {
                throw new NotImplementedException();
            }

            public void SetDefault(Type policyInterface,
                                   IBuilderPolicy policy)
            {
                throw new NotImplementedException();
            }
        }
    }
}