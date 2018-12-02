using System;
using System.Collections.Generic;
using IoCBuilder.Location;
using IoCBuilder.Policies;
using IoCBuilder.Strategies.Trace;

namespace IoCBuilder
{
    /// <summary>
    /// An implementation helper class for <see cref="IBuilder{TStageEnum}"/>.
    /// </summary>
    /// <typeparam name="TStageEnum">The build stage enumeration.</typeparam>
    public class BuilderBase<TStageEnum> : IBuilder<TStageEnum>
    {
        private IPolicyList policies = new PolicyList();
        private StrategyList<TStageEnum> strategies = new StrategyList<TStageEnum>();
        private Dictionary<object, object> lockObjects = new Dictionary<object, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderBase{T}"/> class.
        /// </summary>
        public BuilderBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderBase{T}"/> class using the
        /// provided configurator.
        /// </summary>
        /// <param name="configurator">The configurator that will configure the builder.</param>
        public BuilderBase(IBuilderConfigurator<TStageEnum> configurator)
        {
            configurator.ApplyConfiguration(this);
        }

        /// <summary>
        /// See <see cref="IBuilder{TStageEnum}.Policies"/> for more information.
        /// </summary>
        public IPolicyList Policies
        {
            get { return policies; }
        }

        /// <summary>
        /// See <see cref="IBuilder{TStageEnum}.Strategies"/> for more information.
        /// </summary>
        public StrategyList<TStageEnum> Strategies
        {
            get { return strategies; }
        }

        /// <summary>
        /// See <see cref="IBuilder{TStageEnum}.BuildUp{T}"/> for more information.
        /// </summary>
        public TTypeToBuild BuildUp<TTypeToBuild>(IReadWriteLocator locator,
                                                             string idToBuild, object existing, params IPolicyList[] transientPolicies)
        {
            return (TTypeToBuild)BuildUp(locator, typeof(TTypeToBuild), idToBuild, existing, transientPolicies);
        }

        /// <summary>
        /// See <see cref="IBuilder{TStageEnum}.BuildUp"/> for more information.
        /// </summary>
        public virtual object BuildUp(IReadWriteLocator locator, Type typeToBuild,
                                             string idToBuild, object existing, params IPolicyList[] transientPolicies)
        {
            if (locator != null)
            {
                lock (GetLock(locator))
                {
                    return DoBuildUp(locator, typeToBuild, idToBuild, existing, transientPolicies);
                }
            }
            else
            {
                return DoBuildUp(locator, typeToBuild, idToBuild, existing, transientPolicies);
            }
        }

        private object DoBuildUp(IReadWriteLocator locator, Type typeToBuild, string idToBuild, object existing,
            IPolicyList[] transientPolicies)
        {
            IBuilderStrategyChain chain = strategies.MakeStrategyChain();
            ThrowIfNoStrategiesInChain(chain);

            BuildKey buildKey = new BuildKey(typeToBuild, idToBuild);

            IBuilderContext context = MakeContext(chain, locator, buildKey, transientPolicies);
            IBuilderTracePolicy trace = context.Policies.Get<IBuilderTracePolicy>(null);

            if (trace != null)
                trace.Trace(Resources.ResourceManager.GetString("BuildUpStarting"), typeToBuild, idToBuild ?? "(null)");

            object result = chain.Head.BuildUp(context, buildKey, existing);

            if (trace != null)
                trace.Trace(Resources.ResourceManager.GetString("BuildUpFinished"), typeToBuild, idToBuild ?? "(null)");

            return result;
        }

        private IBuilderContext MakeContext(IBuilderStrategyChain chain, IReadWriteLocator locator, object buildKey, params IPolicyList[] transientPolicies)
        {
            IPolicyList policies = new PolicyList(this.policies);

            foreach (IPolicyList policyList in transientPolicies)
                policies.AddPolicies(policyList);

            return new BuilderContext(chain, locator, policies, buildKey);
        }

        private static void ThrowIfNoStrategiesInChain(IBuilderStrategyChain chain)
        {
            if (chain.Head == null)
                throw new InvalidOperationException(Resources.ResourceManager.GetString("BuilderHasNoStrategies"));
        }

        /// <summary>
        /// See <see cref="IBuilder{TStageEnum}.TearDown{T}"/> for more information.
        /// </summary>
        public TItem TearDown<TItem>(IReadWriteLocator locator, TItem item)
        {
            if (typeof(TItem).IsValueType == false && item == null)
                throw new ArgumentNullException("item");

            if (locator != null)
            {
                lock (GetLock(locator))
                {
                    return DoTearDown<TItem>(locator, item);
                }
            }
            else
            {
                return DoTearDown<TItem>(locator, item);
            }
        }

        private TItem DoTearDown<TItem>(IReadWriteLocator locator, TItem item)
        {
            IBuilderStrategyChain chain = strategies.MakeReverseStrategyChain();
            ThrowIfNoStrategiesInChain(chain);

            Type type = item.GetType();
            IBuilderContext context = MakeContext(chain, locator, null);
            IBuilderTracePolicy trace = context.Policies.Get<IBuilderTracePolicy>(null);

            if (trace != null)
                trace.Trace(Resources.ResourceManager.GetString("TearDownStarting"), type);

            TItem result = (TItem)chain.Head.TearDown(context, item);

            if (trace != null)
                trace.Trace(Resources.ResourceManager.GetString("TearDownFinished"), type);

            return result;
        }

        private object GetLock(object locator)
        {
            lock (lockObjects)
            {
                if (lockObjects.ContainsKey(locator))
                    return lockObjects[locator];

                object newLock = new object();
                lockObjects[locator] = newLock;
                return newLock;
            }
        }
    }
}