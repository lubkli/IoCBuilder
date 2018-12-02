using System;
using System.Collections.Generic;

namespace IoCBuilder.Policies
{
    /// <summary>
    /// A custom collection wrapper over <see cref="IBuilderPolicy"/> objects.
    /// </summary>
    public interface IPolicyList
    {
        /// <summary>
        /// Retruns read-only dictionary of policies.
        /// </summary>
        IReadOnlyDictionary<PolicyKey, IBuilderPolicy> Policies { get; }

        /// <summary>
		/// Returns the number of policies in the list.
		/// </summary>
        int Count { get; }

        /// <summary>
		/// Adds a bundle of policies into the policy list. Any policies in this list will override
		/// policies that are already in the policy list.
		/// </summary>
		/// <param name="policiesToCopy">The policies to be copied.</param>
        void AddPolicies(IPolicyList policiesToCopy);

        /// <summary>
		/// Removes an individual policy.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The type the policy was registered as.</typeparam>
		/// <param name="buildKey">The build key the policy applies to.</param>
        void Clear<TPolicyInterface>(object buildKey);

        /// <summary>
		/// Removes an individual policy.
		/// </summary>
		/// <param name="policyInterface">The type the policy was registered as.</param>
		/// <param name="buildKey">The build key the policy applies to.</param>
        void Clear(Type policyInterface,
                   object buildKey);

        /// <summary>
		/// Removes all policies from the list.
		/// </summary>
        void ClearAll();

        /// <summary>
		/// Removes a default policy.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The type the policy was registered as.</typeparam>
        void ClearDefault<TPolicyInterface>();

        /// <summary>
		/// Removes a default policy.
		/// </summary>
		/// <param name="policyInterface">The type the policy was registered as.</param>
        void ClearDefault(Type policyInterface);

        /// <summary>
        /// Gets an individual policy.
        /// </summary>
        /// <typeparam name="TPolicyInterface">The interface the policy is registered under.</typeparam>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        TPolicyInterface Get<TPolicyInterface>(object buildKey)
            where TPolicyInterface : IBuilderPolicy;

        /// <summary>
		/// Gets an individual policy.
		/// </summary>
		/// <param name="policyInterface">The interface the policy is registered under.</param>
		/// <param name="buildKey">The build key the policy applies to.</param>
		/// <returns>The policy in the list, if present; returns null otherwise.</returns>
        IBuilderPolicy Get(Type policyInterface,
                           object buildKey);

        /// <summary>
        /// Gets an individual policy.
        /// </summary>
        /// <typeparam name="TPolicyInterface">The type the policy was registered as.</typeparam>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <param name="localOnly">true if the policy searches local only; otherwise false to seach up the parent chain.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        TPolicyInterface Get<TPolicyInterface>(object buildKey,
                                               bool localOnly)
            where TPolicyInterface : IBuilderPolicy;

        /// <summary>
        /// Gets an individual policy.
        /// </summary>
        /// <param name="policyInterface">The type the policy was registered as.</param>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <param name="localOnly">true if the policy searches local only; otherwise false to seach up the parent chain.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        IBuilderPolicy Get(Type policyInterface,
                           object buildKey,
                           bool localOnly);

        /// <summary>
        ///Get the non default policy.
        /// </summary>
        /// <typeparam name="TPolicyInterface">The type the policy was registered as.</typeparam>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <param name="localOnly">Get the non default policy.</param>
        /// <param name="containingPolicyList">The policy list in the chain that the searched for policy was found in, null if the policy was not found.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        TPolicyInterface GetNoDefault<TPolicyInterface>(object buildKey,
                                                        bool localOnly,
                                                        out IPolicyList containingPolicyList)
            where TPolicyInterface : IBuilderPolicy;

        /// <summary>
        ///Get the non default policy.
        /// </summary>
        /// <param name="policyInterface">The type the policy was registered as.</param>
        /// <param name="buildKey">The build key the policy applies to.</param>
        /// <param name="localOnly">Get the non default policy.</param>
        /// <param name="containingPolicyList">The policy list in the chain that the searched for policy was found in, null if the policy was not found.</param>
        /// <returns>The policy in the list, if present; returns null otherwise.</returns>
        IBuilderPolicy GetNoDefault(Type policyInterface,
                                    object buildKey,
                                    bool localOnly,
                                    out IPolicyList containingPolicyList);

        /// <summary>
		/// Sets an individual policy.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The interface to register the policy under.</typeparam>
		/// <param name="policy">The policy to be registered.</param>
		/// <param name="buildKey">The build key the policy applies to.</param>
        void Set<TPolicyInterface>(TPolicyInterface policy,
                                   object buildKey)
            where TPolicyInterface : IBuilderPolicy;

        /// <summary>
		/// Sets an individual policy.
		/// </summary>
		/// <param name="policyInterface">The interface to register the policy under.</param>
		/// <param name="policy">The policy to be registered.</param>
		/// <param name="buildKey">The build key the policy applies to.</param>
        void Set(Type policyInterface,
                 IBuilderPolicy policy,
                 object buildKey);

        /// <summary>
		/// Sets a default policy. When checking for a policy, if no specific individual policy
		/// is available, the default will be used.
		/// </summary>
		/// <typeparam name="TPolicyInterface">The interface to register the policy under.</typeparam>
		/// <param name="policy">The default policy to be registered.</param>
        void SetDefault<TPolicyInterface>(TPolicyInterface policy)
            where TPolicyInterface : IBuilderPolicy;

        /// <summary>
		/// Sets a default policy. When checking for a policy, if no specific individual policy
		/// is available, the default will be used.
		/// </summary>
		/// <param name="policyInterface">The interface to register the policy under.</param>
		/// <param name="policy">The default policy to be registered.</param>
        void SetDefault(Type policyInterface,
                        IBuilderPolicy policy);
    }
}