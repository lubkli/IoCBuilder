using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Creation
{
    /// <summary>
    /// A creation policy where the constructor to choose is derived from the parameters
    /// provided by the user.
    /// </summary>
    public class ConstructorCreationPolicy : ICreationPolicy
    {
        private ConstructorInfo constructor;
        private readonly List<IParameter> parameters;

        /// <summary>
        /// Initalize a new instance of the <see cref="ConstructorCreationPolicy"/> class with an empty constructor and no parameters.
        /// </summary>
        public ConstructorCreationPolicy()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initalize a new instance of the <see cref="ConstructorCreationPolicy"/> class with a <see cref="ConstructorInfo"/> and an array of <see cref="IParameter"/> instances.
        /// </summary>
        /// <param name="constructor">The <see cref="ConstructorInfo"/> to use to create the object.</param>
        /// <param name="parameters">An array of <see cref="IParameter"/> instances.</param>
        public ConstructorCreationPolicy(ConstructorInfo constructor,
                                         params IParameter[] parameters)
            : this(constructor, (IEnumerable<IParameter>)parameters) { }

        /// <summary>
        /// Initalize a new instance of the <see cref="ConstructorCreationPolicy"/> class with a <see cref="ConstructorInfo"/> and a collection of <see cref="IParameter"/> instances.
        /// </summary>
        /// <param name="constructor">The <see cref="ConstructorInfo"/> to use to create the object.</param>
        /// <param name="parameters">A collection of <see cref="IParameter"/> instances.</param>
        public ConstructorCreationPolicy(ConstructorInfo constructor,
                                         IEnumerable<IParameter> parameters)
        {
            // I will allow null ctor and params for later adding for backward compatibility (of unit test, but not for other code probably)
            //Guard.ArgumentNotNull(constructor, "constructor");

            this.constructor = constructor;
            this.parameters = new List<IParameter>();

            if (parameters != null)
                this.parameters.AddRange(parameters);
        }

        /// <summary>
        /// Adds a parameter to the list of parameters used to find the constructor.
        /// </summary>
        /// <param name="parameter">The parameter to add.</param>
        public void AddParameter(IParameter parameter)
        {
            parameters.Add(parameter);
        }

        private ConstructorInfo SelectConstructor(IBuilderContext context, Type type, string id)
        {
            if (constructor != null)
                return constructor;

            List<Type> types = new List<Type>();

            foreach (IParameter parm in parameters)
                types.Add(parm.GetParameterType(context));

            return type.GetConstructor(types.ToArray());
        }

        /// <summary>
        /// Determines if the policy supports reflection.
        /// </summary>
        public bool SupportsReflection
        {
            get { return true; }
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

            return GetConstructor(context, buildKey).Invoke(GetParameters(context, constructor));
        }

        /// <summary>
        /// Gets the constructor to be used to create the object.
        /// </summary>
        /// <param name="context">The builder context.</param>
        /// <param name="buildKey">The key for the object being built.</param>
        /// <returns>The constructor to use; returns null if no suitable constructor can be found.</returns>
        public ConstructorInfo GetConstructor(IBuilderContext context,
                                              object buildKey)
        {
            if (constructor == null)
            {
                constructor = SelectConstructor(context, BuilderStrategy.GetTypeFromBuildKey(buildKey), BuilderStrategy.GetNameFromBuildKey(buildKey));
            }

            return constructor;
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