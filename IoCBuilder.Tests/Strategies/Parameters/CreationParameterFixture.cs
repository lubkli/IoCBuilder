using NUnit.Framework;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder
{
    [TestFixture]
    public class CreationParameterFixture
    {
        [Test]
        public void CreationParameterUsesStrategyChainToCreateObjects()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            NullStrategy strategy = new NullStrategy();
            ctx.InnerChain.Add(strategy);

            CreationParameter param = new CreationParameter(typeof(object));
            param.GetValue(ctx);

            Assert.IsTrue(strategy.WasCalled);
            Assert.AreEqual(typeof(object), BuilderStrategy.GetTypeFromBuildKey(strategy.BuildKey));
        }

        [Test]
        public void CreationParameterCanCreateObjectsOfAGivenID()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            NullStrategy strategy = new NullStrategy();
            ctx.InnerChain.Add(strategy);

            CreationParameter param = new CreationParameter(typeof(object), "foo");
            param.GetValue(ctx);

            Assert.AreEqual("foo", BuilderStrategy.GetNameFromBuildKey(strategy.BuildKey));
        }

        private class NullStrategy : BuilderStrategy
        {
            public bool WasCalled = false;
            public object BuildKey = null;

            public override object BuildUp(IBuilderContext context, object buildKey, object existing)
            {
                WasCalled = true;
                BuildKey = buildKey;

                return null;
            }
        }
    }
}