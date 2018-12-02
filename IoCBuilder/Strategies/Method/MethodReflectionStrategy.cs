using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Method
{
    /// <summary>
    /// Strategy that performs injection of method policies.
    /// </summary>
    public class MethodReflectionStrategy : ReflectionStrategy<MethodInfo>
    {
        /// <summary>
        /// Adds <paramref name="parameters"/> to the appropriate policy.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The key for the object being reflected over.</param>
        /// <param name="member">The member that's being reflected over.</param>
        /// <param name="parameters">The parameters used to satisfy the method.</param>
        protected override void AddParametersToPolicy(IBuilderContext context,
                                                      object buildKey,
                                                      IMemberInfo<MethodInfo> member,
                                                      IEnumerable<IParameter> parameters)
        {
            IMethodCallPolicy result = context.Policies.Get<IMethodCallPolicy>(buildKey);

            if (result == null)
            {
                result = new MethodCallPolicy();
                context.Policies.Set(result, buildKey);
            }

            result.Methods.Add(new ReflectionMethodCallInfo(member.MemberInfo, parameters));
        }

        /// <summary>
        /// Retrieves the list of methods to iterate looking for injection attributes.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The key for the object being reflected over.</param>
        /// <param name="existing">The instance of the object being reflected over.</param>
        /// <returns>
        /// An enumerable wrapper around the <see cref="IMemberInfo{MethodInfo}"/> interfaces that
        /// represent the members to be inspected for reflection.
        /// </returns>
        protected override IEnumerable<IMemberInfo<MethodInfo>> GetMembers(IBuilderContext context,
                                                                           object buildKey,
                                                                           object existing)
        {
            foreach (MethodInfo method in GetTypeFromBuildKey(buildKey).GetMethods())
                yield return new MethodMemberInfo<MethodInfo>(method);
        }

        /// <summary>
        /// Determine whether a member should be processed.
        /// </summary>
        /// <param name="member">The member to determine processing.</param>
        /// <returns>true if the member needs processing; otherwise, false.</returns>
        protected override bool MemberRequiresProcessing(IMemberInfo<MethodInfo> member)
        {
            return (member.GetCustomAttributes(typeof(InjectionMethodAttribute), true).Length > 0);
        }
    }
}