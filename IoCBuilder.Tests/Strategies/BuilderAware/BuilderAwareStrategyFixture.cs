using NUnit.Framework;
using IoCBuilder.Strategies.BuilderAware;

namespace IoCBuilder
{
    [TestFixture]
    public class BuilderAwareStrategyFixture
    {
        [Test]
        public void BuildIgnoresClassWithoutInterface()
        {
            BuilderAwareStrategy strategy = new BuilderAwareStrategy();
            MockBuilderContext context = new MockBuilderContext();
            IgnorantObject obj = new IgnorantObject();

            context.InnerChain.Add(strategy);

            context.HeadOfChain.BuildUp(context, new BuildKey<IgnorantObject>(null), obj);

            Assert.IsFalse(obj.OnAssembledCalled);
            Assert.IsFalse(obj.OnDisassemblingCalled);
        }

        [Test]
        public void UnbuildIgnoresClassWithoutInterface()
        {
            BuilderAwareStrategy strategy = new BuilderAwareStrategy();
            MockBuilderContext context = new MockBuilderContext();
            IgnorantObject obj = new IgnorantObject();

            context.InnerChain.Add(strategy);

            context.HeadOfChain.TearDown(context, obj);

            Assert.IsFalse(obj.OnAssembledCalled);
            Assert.IsFalse(obj.OnDisassemblingCalled);
        }

        [Test]
        public void BuildCallsClassWithInterface()
        {
            BuilderAwareStrategy strategy = new BuilderAwareStrategy();
            MockBuilderContext context = new MockBuilderContext();
            AwareObject obj = new AwareObject();

            context.InnerChain.Add(strategy);

            context.HeadOfChain.BuildUp(context, new BuildKey<AwareObject>("foo"), obj);

            Assert.IsTrue(obj.OnAssembledCalled);
            Assert.IsFalse(obj.OnDisassemblingCalled);
            Assert.AreEqual("foo", obj.AssembledID);
        }

        [Test]
        public void UnbuildCallsClassWithInterface()
        {
            BuilderAwareStrategy strategy = new BuilderAwareStrategy();
            MockBuilderContext context = new MockBuilderContext();
            AwareObject obj = new AwareObject();

            context.InnerChain.Add(strategy);

            context.HeadOfChain.TearDown(context, obj);

            Assert.IsFalse(obj.OnAssembledCalled);
            Assert.IsTrue(obj.OnDisassemblingCalled);
        }

        [Test]
        public void BuildChecksConcreteTypeAndNotRequestedType()
        {
            BuilderAwareStrategy strategy = new BuilderAwareStrategy();
            MockBuilderContext context = new MockBuilderContext();
            AwareObject obj = new AwareObject();

            context.InnerChain.Add(strategy);

            context.HeadOfChain.BuildUp(context, new BuildKey<IgnorantObject>(null), obj);

            Assert.IsTrue(obj.OnAssembledCalled);
            Assert.IsFalse(obj.OnDisassemblingCalled);
        }

        private class IgnorantObject
        {
            public bool OnAssembledCalled = false;
            public bool OnDisassemblingCalled = false;
            public string AssembledID = null;

            public void OnBuiltUp(object buildKey)
            {
                OnAssembledCalled = true;
                AssembledID = BuilderStrategy.GetNameFromBuildKey(buildKey);
            }

            public void OnTearingDown()
            {
                OnDisassemblingCalled = true;
            }
        }

        private class AwareObject : IgnorantObject, IBuilderAware
        {
        }
    }
}