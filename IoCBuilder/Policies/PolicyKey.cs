using System;

namespace IoCBuilder.Policies
{
    /// <summary>
    /// Represents the information necessary for registration of a builder policy. A policy is
    /// registered by the interface policy type, the type the policy applies to, and the ID
    /// the policy applies to.
    /// </summary>
    public struct PolicyKey
    {
#pragma warning disable 219

        /// <summary>
        /// The build key the policy applies to.
        /// </summary>
        public readonly object BuildKey;

        /// <summary>
        /// The type the policy is or was registered as.
        /// </summary>
        public readonly Type PolicyType;

#pragma warning restore 219

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyKey"/> struct using the
        /// provided policy type, application type, and application ID.</summary>
        /// <param name="policyType">The policy interface type.</param>
        /// <param name="buildKey">The buildKey the policy applies to.</param>
        public PolicyKey(Type policyType,
                         object buildKey)
        {
            PolicyType = policyType;
            BuildKey = buildKey;
        }
    }
}