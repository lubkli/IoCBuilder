using NUnit.Framework;
using System;
using IoCBuilder.Exceptions;
using IoCBuilder.Strategies.TypeMapping;

namespace IoCBuilder
{
    [TestFixture]
    public class TypeMappingStrategyFixture
    {
        [Test]
        public void CanMapInterfacesToConcreteTypes()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            TypeMappingStrategy strategy = new TypeMappingStrategy();
            ctx.Policies.Set<ITypeMappingPolicy>(new TypeMappingPolicy(typeof(SalesFoo), null), new BuildKey<IFoo>("sales"));
            ctx.Policies.Set<ITypeMappingPolicy>(new TypeMappingPolicy(typeof(Foo), null), new BuildKey<IFoo>("marketing"));
            ctx.InnerChain.Add(strategy);

            MockStrategy mock = new MockStrategy();
            ctx.InnerChain.Add(mock);

            strategy.BuildUp<IFoo>(ctx, null, "sales");

            Assert.IsTrue(mock.WasRun);
            Assert.AreEqual(typeof(SalesFoo), mock.IncomingType);

            mock.WasRun = false;
            mock.IncomingType = null;

            strategy.BuildUp<IFoo>(ctx, null, "marketing");

            Assert.IsTrue(mock.WasRun);
            Assert.AreEqual(typeof(Foo), mock.IncomingType);
        }

        [Test]
        public void IncompatibleTypes()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            TypeMappingStrategy strategy = new TypeMappingStrategy();
            ctx.Policies.Set<ITypeMappingPolicy>(new TypeMappingPolicy(typeof(object), null), new BuildKey<IFoo>("sales"));
            ctx.InnerChain.Add(strategy);

            Assert.Throws<IncompatibleTypesException>(() => strategy.BuildUp<IFoo>(ctx, null, "sales"));
        }

        private class MockStrategy : BuilderStrategy
        {
            public bool WasRun = false;
            public Type IncomingType = null;

            public override object BuildUp(IBuilderContext context, object buildKey, object existing)
            {
                WasRun = true;
                IncomingType = BuilderStrategy.GetTypeFromBuildKey(buildKey);
                return null;
            }
        }

        private interface IFoo
        {
        }

        private class Foo : IFoo
        {
        }

        private class SalesFoo : IFoo
        {
        }
    }
}