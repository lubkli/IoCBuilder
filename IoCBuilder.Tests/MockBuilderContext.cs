using IoCBuilder.Lifetime;
using IoCBuilder.Location;
using IoCBuilder.Policies;

namespace IoCBuilder
{
    internal class MockBuilderContext : BuilderContext
    {
        public IReadWriteLocator InnerLocator;
        public BuilderStrategyChain InnerChain = new BuilderStrategyChain();
        public IPolicyList InnerPolicies = new PolicyList();
        public ILifetimeContainer lifetimeContainer = new LifetimeContainer();

        public MockBuilderContext()
            : this(new Locator())
        {
        }

        public MockBuilderContext(IReadWriteLocator locator)
        {
            InnerLocator = locator;
            SetLocator(InnerLocator);
            StrategyChain = InnerChain;
            SetPolicies(InnerPolicies);

            if (!Locator.Contains(typeof(ILifetimeContainer)))
                Locator.Add(typeof(ILifetimeContainer), lifetimeContainer);
        }
    }
}