using System;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder
{
    /// <summary>
    /// Attribute applied to properties and constructor parameters, to describe when the
    /// dependency injection system should always create new instances of the requested object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class CreateNewAttribute : ParameterAttribute
    {
        /// <summary>
        /// See <see cref="ParameterAttribute.CreateParameter"/> for more information.
        /// </summary>
        public override IParameter CreateParameter(Type annotatedMemberType)
        {
            return new CreationParameter(annotatedMemberType, Guid.NewGuid().ToString());
        }
    }
}