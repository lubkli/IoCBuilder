using System;
using System.Collections.Generic;
using System.Globalization;
using IoCBuilder.Strategies.Parameters;
using IoCBuilder.Strategies.Trace;

namespace IoCBuilder
{
    /// <summary>
    /// An implementation of <see cref="IBuilderStrategy"/>.
    /// </summary>
    public abstract class BuilderStrategy : IBuilderStrategy
    {
        /// <summary>
        /// Generic version of BuildUp, to aid unit-testing.
        /// </summary>
        public TItem BuildUp<TItem>(IBuilderContext context, TItem existing, string idToBuild)
        {
            return (TItem)BuildUp(context, new BuildKey<TItem>(idToBuild), existing);
        }

        /// <summary>
        /// See <see cref="IBuilderStrategy.BuildUp"/> for more information.
        /// </summary>
        public virtual object BuildUp(IBuilderContext context, object buildKey, object existing)
        {
            IBuilderStrategy next = context.GetNextInChain(this);

            if (next != null)
                return next.BuildUp(context, buildKey, existing);
            else
                return existing;
        }

        /// <summary>
        /// See <see cref="IBuilderStrategy.TearDown"/> for more information.
        /// </summary>
        public virtual object TearDown(IBuilderContext context, object item)
        {
            IBuilderStrategy next = context.GetNextInChain(this);

            if (next != null)
                return next.TearDown(context, item);
            else
                return item;
        }

        /// <summary>
        /// Creates a trace list of parameter types from a list of <see cref="IParameter"/> objects.
        /// </summary>
        /// <param name="parameters">The parameters</param>
        /// <returns>The type list in string form</returns>
        protected string ParametersToTypeList(params object[] parameters)
        {
            List<string> types = new List<string>();

            foreach (object parameter in parameters)
            {
                if (parameter == null)
                    types.Add(string.Empty);
                else
                    types.Add(parameter.GetType().Name);
            }

            return string.Join(", ", types.ToArray());
        }

        /// <summary>
        /// Traces debugging information, if there is an appropriate policy.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The build key being built.</param>
        /// <param name="format">The format of the message.</param>
        /// <param name="args">The message arguments.</param>
        protected void TraceBuildUp(IBuilderContext context, object buildKey, string format, params object[] args)
        {
            IBuilderTracePolicy policy = context.Policies.Get<IBuilderTracePolicy>(null);

            if (policy != null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);

                policy.Trace(Resources.ResourceManager.GetString("BuilderStrategyTraceBuildUp"), GetType().Name, buildKey.ToString(), message);
            }
        }

        /// <summary>
        /// Traces debugging information, if there is an appropriate policy.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="item">Item being torn down.</param>
        /// <param name="format">The format of the message.</param>
        /// <param name="args">The message arguments.</param>
        protected void TraceTearDown(IBuilderContext context, object item, string format, params object[] args)
        {
            IBuilderTracePolicy policy = context.Policies.Get<IBuilderTracePolicy>(null);

            if (policy != null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                policy.Trace(Resources.ResourceManager.GetString("BuilderStrategyTraceTearDown"), GetType().Name, item.GetType().Name, message);
            }
        }

        /// <summary>
        /// Determines if tracing is enabled
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <returns>Returns true if tracing is enabled; false otherwise.</returns>
        protected bool TraceEnabled(IBuilderContext context)
        {
            return context.Policies.Get<IBuilderTracePolicy>(null) != null;
        }

        /// <summary>
        /// Extracts type from build key.
        /// </summary>
        /// <param name="buildKey">Build key containing type to build.</param>
        /// <param name="type">When this method returns, contains type to build.</param>
        /// <returns>true if buildKey was converted successfully; otherwise, false.</returns>
        public static bool TryGetTypeFromBuildKey(object buildKey,
                                          out Type type)
        {
            type = buildKey as Type;

            if (type == null)
            {
                ITypeBasedBuildKey typeBasedBuildKey = buildKey as ITypeBasedBuildKey;
                if (typeBasedBuildKey != null)
                    type = typeBasedBuildKey.Type;
            }

            return type != null;
        }

        /// <summary>
        /// Retuns type to build from build key.
        /// </summary>
        /// <param name="buildKey">Build key containing type to build.</param>
        /// <returns>Type to build extracted from build key.</returns>
        public static Type GetTypeFromBuildKey(object buildKey)
        {
            Type result;

            if (!TryGetTypeFromBuildKey(buildKey, out result))
                throw new ArgumentException("Cannot extract type from build key " + buildKey, "buildKey");

            return result;
        }

        /// <summary>
        /// Extracts name from build key.
        /// </summary>
        /// <param name="buildKey">Build key containing name.</param>
        /// <param name="name">When this method returns, contains name.</param>
        /// <returns>true if buildKey was converted successfully; otherwise, false.</returns>
        public static bool TryGetNameFromBuildKey(object buildKey,
                                          out string name)
        {
            name = buildKey as string;

            if (name == null)
            {
                INameBasedBuildKey typeBasedBuildKey = buildKey as INameBasedBuildKey;
                if (typeBasedBuildKey != null)
                {
                    name = typeBasedBuildKey.Name;
                    return true;
                }
            }

            return name != null;
        }

        /// <summary>
        /// Retuns name from build key.
        /// </summary>
        /// <param name="buildKey">Build key containing name.</param>
        /// <returns>Name extracted from build key.</returns>
        public static string GetNameFromBuildKey(object buildKey)
        {
            string result;

            if (!TryGetNameFromBuildKey(buildKey, out result))
                throw new ArgumentException("Cannot extract name from build key " + buildKey, "buildKey");

            return result;
        }
    }
}