using IoCBuilder.Location;
using IoCBuilder.Policies;

namespace IoCBuilder
{
    /// <summary>
    /// An implementation of <see cref="IBuilderContext"/>.
    /// </summary>
    public class BuilderContext : IBuilderContext
    {
        private IBuilderStrategyChain chain;
        private IReadWriteLocator locator;
        private IPolicyList policies;
        private object originalBuildKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderContext"/> class.
        /// </summary>
        protected BuilderContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderContext"/> class using
        /// the provided chain, locator, and policies.
        /// </summary>
        /// <param name="chain">The strategy chain.</param>
        /// <param name="locator">The locator.</param>
        /// <param name="policies">The permanent policies from the builder.</param>
        /// <param name="originalBuildKey"></param>
        public BuilderContext(IBuilderStrategyChain chain, IReadWriteLocator locator, IPolicyList policies, object originalBuildKey)
        {
            this.chain = chain;
            this.locator = locator;
            this.policies = new PolicyList(policies);
            this.originalBuildKey = originalBuildKey;
        }

        /// <summary>
        /// See <see cref="IBuilderContext.HeadOfChain"/> for more information.
        /// </summary>
        public IBuilderStrategy HeadOfChain
        {
            get { return chain.Head; }
        }

        /// <summary>
        /// See <see cref="IBuilderContext.Locator"/> for more information.
        /// </summary>
        public IReadWriteLocator Locator
        {
            get { return locator; }
        }

        /// <summary>
        /// Sets the locator.
        /// </summary>
        protected void SetLocator(IReadWriteLocator locator)
        {
            this.locator = locator;
        }

        /// <summary>
        /// Gets the original build key for the build operation.
        /// </summary>
        public object OriginalBuildKey
        {
            get { return originalBuildKey; }
        }

        /// <summary>
        /// See <see cref="IBuilderContext.Policies"/> for more information.
        /// </summary>
        public IPolicyList Policies
        {
            get { return policies; }
        }

        /// <summary>
        /// Sets the policies.
        /// </summary>
        protected void SetPolicies(IPolicyList policies)
        {
            this.policies = policies;
        }

        /// <summary>
        /// The strategy chain.
        /// </summary>
        protected IBuilderStrategyChain StrategyChain
        {
            get { return chain; }
            set { chain = value; }
        }

        /// <summary>
        /// See <see cref="IBuilderContext.GetNextInChain"/> for more information.
        /// </summary>
        public IBuilderStrategy GetNextInChain(IBuilderStrategy currentStrategy)
        {
            return chain.GetNext(currentStrategy);
        }
    }
}