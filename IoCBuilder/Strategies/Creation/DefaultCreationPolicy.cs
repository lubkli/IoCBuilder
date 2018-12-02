using System;
using System.Reflection;

namespace IoCBuilder.Strategies.Creation
{
    /// <summary>
    /// Default creation policy which selects the first public constructor
    /// of an object, using the builder to resolve/create any parameters the
    /// constructor requires.
    /// </summary>
    public class DefaultCreationPolicy : ICreationPolicy
    {
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
            Type typeToBuild = GetTypeFromBuildKey(buildKey);
            if (typeToBuild == null)
                throw new ArgumentException("Default creation policy cannot create for unknown build key " + buildKey, "buildKey");

            ConstructorInfo[] constructors = typeToBuild.GetConstructors();
            if (constructors.Length == 0)
                return Activator.CreateInstance(typeToBuild);

            ConstructorInfo constructor = GetConstructor(typeToBuild);
            return constructor.Invoke(GetParameters(context, constructor));
        }

        /// <summary>
        /// Gets the build key from the type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of object to be built.</param>
        /// <returns>The build key for the object being built.</returns>
        protected virtual object GetBuildKeyFromType(Type type)
        {
            return new BuildKey(type);
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
            Type typeToBuild = GetTypeFromBuildKey(buildKey);
            if (typeToBuild == null)
                throw new ArgumentException("Default creation policy cannot create for unknown build key " + buildKey, "buildKey");

            return GetConstructor(typeToBuild);
        }

        private static ConstructorInfo GetConstructor(Type typeToBuild)
        {
            ConstructorInfo[] constructors = typeToBuild.GetConstructors();

            if (constructors.Length > 0)
                return constructors[0];

            return null;
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
            ParameterInfo[] parms = constructor.GetParameters();
            object[] parmsValueArray = new object[parms.Length];

            for (int i = 0; i < parms.Length; ++i)
                parmsValueArray[i] = context.HeadOfChain.BuildUp(context,
                                                                 GetBuildKeyFromType(parms[i].ParameterType),
                                                                 null);

            return parmsValueArray;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of object to build from the <paramref name="buildKey"/>.
        /// </summary>
        /// <param name="buildKey">The key of object to be built.</param>
        /// <returns>The <see cref="Type"/> of object to be built.</returns>
        protected virtual Type GetTypeFromBuildKey(object buildKey)
        {
            return BuilderStrategy.GetTypeFromBuildKey(buildKey);
        }
    }
}