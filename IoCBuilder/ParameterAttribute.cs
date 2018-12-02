using System;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder
{
    /// <summary>
    /// Attribute that represents dependency injection members, whose value at
    /// build time will be determined by the <see cref="IParameter"/> returned
    /// from the attribute <see cref="CreateParameter"/> factory method.
    /// </summary>
    public abstract class ParameterAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ParameterAttribute"/> class.
        /// </summary>
        protected ParameterAttribute() { }

        /// <summary>
        /// Creates a parameter for use with various <see cref="IBuilderPolicy"/> implementations
        /// that can process <see cref="IParameter"/>s.
        /// </summary>
        /// <param name="memberType">The type of the annotated member, such as a property or a
        /// constructor parameter.</param>
        /// <returns>The parameter instance that knows how to retrieve a value for the dependency.</returns>
        public abstract IParameter CreateParameter(Type memberType);
    }
}