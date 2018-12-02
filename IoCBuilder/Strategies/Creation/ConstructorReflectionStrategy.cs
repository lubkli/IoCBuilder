using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Exceptions;
using IoCBuilder.Policies;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Creation
{
    /// <summary>
    /// Strategy that performs injection of constructor policies.
    /// </summary>
    public class ConstructorReflectionStrategy : ReflectionStrategy<ConstructorInfo>
    {
        /// <summary>
        /// Adds <paramref name="parameters"/> to the appropriate policy.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The key for the operation.</param>
        /// <param name="member">The member that's being reflected over.</param>
        /// <param name="parameters">The parameters used to satisfy the constructor.</param>
        protected override void AddParametersToPolicy(IBuilderContext context,
                                                      object buildKey,
                                                      IMemberInfo<ConstructorInfo> member,
                                                      IEnumerable<IParameter> parameters)
        {
            ConstructorCreationPolicy policy = new ConstructorCreationPolicy(member.MemberInfo, parameters);
            context.Policies.Set<ICreationPolicy>(policy, buildKey);
        }

        /// <summary>
        /// Retrieves the list of constructors to iterate looking for injection attributes.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        /// <param name="existing">The instance of creating object.</param>
        /// <returns>
        /// An enumerable wrapper around the <see cref="IMemberInfo{ConstructorInfo}"/> interfaces that
        /// represent the members to be inspected for reflection.
        /// </returns>
        protected override IEnumerable<IMemberInfo<ConstructorInfo>> GetMembers(IBuilderContext context,
                                                                                object buildKey,
                                                                                object existing)
        {
            IPolicyList containingPolicyList;
            ICreationPolicy existingPolicy = context.Policies.GetNoDefault<ICreationPolicy>(buildKey, false, out containingPolicyList);

            if (existing == null && existingPolicy == null)
            {
                Type typeToBuild = GetTypeFromBuildKey(buildKey);
                ConstructorInfo injectionCtor = null;
                ConstructorInfo[] ctors = typeToBuild.GetConstructors();

                if (ctors.Length == 1)
                    injectionCtor = ctors[0];
                else
                    foreach (ConstructorInfo ctor in ctors)
                        if (Attribute.IsDefined(ctor, typeof(InjectionConstructorAttribute)))
                        {
                            if (injectionCtor != null)
                                throw new InvalidAttributeException(typeToBuild, ".ctor");

                            injectionCtor = ctor;
                        }

                if (injectionCtor != null)
                    yield return new MethodMemberInfo<ConstructorInfo>(injectionCtor);
            }
        }

        /// <summary>
        /// Determine whether a constructor should be processed.
        /// </summary>
        /// <param name="member">The member to determine processing.</param>
        /// <returns>Always returns true.</returns>
        protected override bool MemberRequiresProcessing(IMemberInfo<ConstructorInfo> member)
        {
            return true;
        }
    }
}