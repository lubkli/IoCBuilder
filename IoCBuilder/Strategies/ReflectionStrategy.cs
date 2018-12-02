using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Exceptions;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies
{
    /// <summary>
    /// Base generic strategy for all injection attribute processors.
    /// </summary>
    public abstract class ReflectionStrategy<TMemberInfo> : BuilderStrategy
    {
        /// <summary>
        /// Adds <paramref name="parameters"/> to the appropriate policy.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The key for the operation.</param>
        /// <param name="member">The member that's being reflected over.</param>
        /// <param name="parameters">The parameters used to satisfy the member call.</param>
        protected abstract void AddParametersToPolicy(IBuilderContext context,
                                                      object buildKey,
                                                      IMemberInfo<TMemberInfo> member,
                                                      IEnumerable<IParameter> parameters);

        /// <summary>
        /// Called during the chain of responsibility for a build operation. The
        /// PreBuildUp method is called when the chain is being executed in the
        /// forward direction.
        /// </summary>
        /// <param name="context">Context of the build operation.</param>
        /// <param name="buildKey">The key for the operation.</param>
        /// <param name="existing">The instance for the operation.</param>
        // FxCop suppression: Validation is done by Guard class
        public override object BuildUp(IBuilderContext context,
                                       object buildKey,
                                       object existing)
        {
            Type type;
            TryGetTypeFromBuildKey(buildKey, out type);

            foreach (IMemberInfo<TMemberInfo> member in GetMembers(context, buildKey, existing))
            {
                if (MemberRequiresProcessing(member))
                {
                    IEnumerable<IParameter> parameters =
                        GenerateIParametersFromParameterInfos(member.GetParameters(), type, member.Name);

                    TraceBuildUp(context, buildKey, Resources.ResourceManager.GetString("ReflectedMember"), member.Name);

                    AddParametersToPolicy(context, buildKey, member, parameters);
                }
            }

            return base.BuildUp(context, buildKey, existing);
        }

        private static IEnumerable<IParameter> GenerateIParametersFromParameterInfos(IEnumerable<ParameterInfo> parameterInfos,
                                                                             Type type,
                                                                             string memberName)
        {
            List<IParameter> result = new List<IParameter>();

            foreach (ParameterInfo parameterInfo in parameterInfos)
            {
                ParameterAttribute attribute = GetInjectionAttribute(parameterInfo, type, memberName);
                result.Add(attribute.CreateParameter(parameterInfo.ParameterType));
            }

            return result;
        }

        private static ParameterAttribute GetInjectionAttribute(ICustomAttributeProvider parameterInfo,
                                                        Type type,
                                                        string memberName)
        {
            ParameterAttribute[] attributes = (ParameterAttribute[])parameterInfo.GetCustomAttributes(typeof(ParameterAttribute), true);

            switch (attributes.Length)
            {
                case 0:
                    return new DependencyAttribute();

                case 1:
                    return attributes[0];

                default:
                    throw new InvalidAttributeException(type, memberName);
            }
        }

        /// <summary>
        /// Retrieves the list of members to iterate looking for
        /// injection attributes, such as properties and constructor
        /// parameters.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The key for the operation.</param>
        /// <param name="existing">The instance for the operation.</param>
        /// <returns>
        /// An enumerable wrapper around the <see cref="IMemberInfo{TMemberInfo}"/> interfaces that
        /// represent the members to be inspected for reflection.
        /// </returns>
        protected abstract IEnumerable<IMemberInfo<TMemberInfo>> GetMembers(IBuilderContext context,
                                                                            object buildKey,
                                                                            object existing);

        /// <summary>
        /// Determine whether a member should be processed.
        /// </summary>
        /// <param name="member">The member to determine processing.</param>
        /// <returns>true if the member needs processing; otherwise, false.</returns>
        protected abstract bool MemberRequiresProcessing(IMemberInfo<TMemberInfo> member);
    }
}