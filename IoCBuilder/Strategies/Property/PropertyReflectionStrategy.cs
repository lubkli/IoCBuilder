using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Property
{
    /// <summary>
    /// Strategy that performs injection of method policies.
    /// </summary>
    /// <remarks>
    /// This strategy processes any injection attribute that inherits from <see cref="ParameterAttribute"/>,
    /// thus providing a generic strategy that easily allows for extension through new injection attributes and
    /// their corresponding <see cref="IParameter"/> implementations (if necessary) to retrieve values.
    /// </remarks>
    public class PropertyReflectionStrategy : ReflectionStrategy<PropertyInfo>
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
                                                      IMemberInfo<PropertyInfo> member,
                                                      IEnumerable<IParameter> parameters)
        {
            IPropertySetterPolicy result = context.Policies.Get<IPropertySetterPolicy>(buildKey);

            if (result == null)
            {
                result = new PropertySetterPolicy();
                context.Policies.Set(result, buildKey);
            }

            foreach (IParameter parameter in parameters)
            {
                //TODO: Solve somehow if two same properties can be set more-times.

                // Just now it works this way: If any concrete policiy exists for some property, reflexion is not applied.
                if (!result.Properties.ContainsKey(member.MemberInfo.Name))
                    result.Properties.Add(member.MemberInfo.Name, new ReflectionPropertySetterInfo(member.MemberInfo, parameter));
                //else
                //    Trace.TraceWarning("Property " + member.MemberInfo.Name + "will not be set more than one times.");
            }
        }

        /// <summary>
        /// Retrieves the list of properties to iterate looking for injection attributes.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The key for the object being reflected over.</param>
        /// <param name="existing">The instance of the object being reflected over.</param>
        /// <returns>
        /// An enumerable wrapper around the <see cref="IMemberInfo{PropertyInfo}"/> interfaces that
        /// represent the members to be inspected for reflection.
        /// </returns>
        protected override IEnumerable<IMemberInfo<PropertyInfo>> GetMembers(IBuilderContext context,
                                                                             object buildKey,
                                                                             object existing)
        {
            foreach (PropertyInfo propInfo in GetTypeFromBuildKey(buildKey).GetProperties())
                yield return new PropertyMemberInfo(propInfo);
        }

        /// <summary>
        /// Determine whether a member should be processed.
        /// </summary>
        /// <param name="member">The member to determine processing.</param>
        /// <returns>true if the member needs processing; otherwise, false.</returns>
        protected override bool MemberRequiresProcessing(IMemberInfo<PropertyInfo> member)
        {
            return (member.GetCustomAttributes(typeof(ParameterAttribute), true).Length > 0);
        }
    }
}