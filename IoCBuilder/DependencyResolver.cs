using System;
using System.Globalization;
using IoCBuilder.Exceptions;

namespace IoCBuilder
{
    /// <summary>
    /// Dependency resolver worker object
    /// </summary>
    public class DependencyResolver
    {
        private IBuilderContext context;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The builder context in which the resolver will resolve
        /// dependencies.</param>
        public DependencyResolver(IBuilderContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.context = context;
        }

        /// <summary>
        /// Resolves a dependency.
        /// </summary>
        /// <param name="typeToResolve">The type to be resolved.</param>
        /// <param name="typeToCreate">The type to be created, if the type cannot be resolved
        /// (and notPresent is set to <see cref="NotPresentBehavior.CreateNew"/>).</param>
        /// <param name="id">The ID of the object to be resolved. Pass null for the unnamed object.</param>
        /// <param name="notPresent">Flag to describe how to behave if the dependency is not found.</param>
        /// <param name="searchMode">Flag to describe whether searches are local only, or local and up.</param>
        /// <returns>The dependent object. If the object is not found, and notPresent
        /// is set to <see cref="NotPresentBehavior.ReturnNull"/>, will return null.</returns>
        public object Resolve(Type typeToResolve, Type typeToCreate, string id, NotPresentBehavior notPresent, SearchMode searchMode)
        {
            if (typeToResolve == null)
                throw new ArgumentNullException("typeToResolve");
            if (!Enum.IsDefined(typeof(NotPresentBehavior), notPresent))
                throw new ArgumentException(Resources.ResourceManager.GetString("InvalidEnumerationValue"), "notPresent");

            if (typeToCreate == null)
                typeToCreate = typeToResolve;

            BuildKey key = new BuildKey(typeToResolve, id);

            if (context.Locator.Contains(key, searchMode))
                return context.Locator.Get(key, searchMode);

            switch (notPresent)
            {
                case NotPresentBehavior.CreateNew:
                    return context.HeadOfChain.BuildUp(context, new BuildKey(typeToCreate, id), null);

                case NotPresentBehavior.ReturnNull:
                    return null;

                default:
                    throw new DependencyMissingException(string.Format(CultureInfo.CurrentCulture, Resources.ResourceManager.GetString("DependencyMissing"), typeToResolve.ToString()));
            }
        }
    }
}