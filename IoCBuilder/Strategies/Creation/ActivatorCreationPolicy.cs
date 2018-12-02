using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Creation
{
    /// <summary>
    /// Represents a creation policy based on <see cref="Activator.CreateInstance(Type, object[])"/>
    /// </summary>
    public class ActivatorCreationPolicy : ICreationPolicy
    {
        private readonly List<IParameter> parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatorCreationPolicy"/> class with an array of <see cref="IParameter"/> instances.
        /// </summary>
        /// <param name="parameters">An array of <see cref="IParameter"/> instances.</param>
        public ActivatorCreationPolicy(params IParameter[] parameters)
            : this((IEnumerable<IParameter>)parameters) { }

        /// <summary>
        /// Initalize a new instance of the <see cref="ActivatorCreationPolicy"/> class with a collection of <see cref="IParameter"/> instances.
        /// </summary>
        /// <param name="parameters">A collection of <see cref="IParameter"/> instances.</param>
        public ActivatorCreationPolicy(IEnumerable<IParameter> parameters)
        {
            this.parameters = new List<IParameter>();

            if (parameters != null)
                this.parameters.AddRange(parameters);
        }

        /// <summary>
        /// Determines if the policy supports reflection.
        /// </summary>
        public bool SupportsReflection
        {
            get { return false; }
        }

        /// <summary>
        /// Create the object for the given <paramref name="context"/> and <paramref name="buildKey"/>.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        /// <returns>The created object.</returns>
        public object Create(IBuilderContext context,
                             object buildKey)
        {
            Guard.ArgumentNotNull(context, "context");

            return Activator.CreateInstance(BuilderStrategy.GetTypeFromBuildKey(buildKey),
                                            GetParameters(context, null));
        }

        /// <summary>
        /// Gets the constructor to be used to create the object.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        /// <returns>The constructor to use; returns null if no suitable constructor can be found.</returns>
        /// <exception cref="NotImplementedException">
        /// Does not support getting a constructor.
        /// </exception>
        public ConstructorInfo GetConstructor(IBuilderContext context,
                                              object buildKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parameter values to be passed to the constructor.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="constructor">The constructor that will be used.</param>
        /// <returns>An array of parameters to pass to the constructor.</returns>
        public object[] GetParameters(IBuilderContext context,
                                      ConstructorInfo constructor)
        {
            object[] result = new object[parameters.Count];

            for (int idx = 0; idx < parameters.Count; idx++)
                result[idx] = parameters[idx].GetValue(context);

            return result;
        }
    }
}