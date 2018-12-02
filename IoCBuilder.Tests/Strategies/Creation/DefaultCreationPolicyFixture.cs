using NUnit.Framework;
using IoCBuilder.Strategies.Creation;

namespace IoCBuilder
{
    [TestFixture]
    public class DefaultCreationPolicyFixture
    {
        [Test]
        public void CanReturnObjectWithEmptyConstructor()
        {
            MockBuilderContext ctx = CreateContext();

            object result = ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), null);

            Assert.IsNotNull(result);
        }

        [Test]
        public void CanCreateObjectWithParameterizedConstructor()
        {
            MockBuilderContext ctx = CreateContext();

            CtorObject result = (CtorObject)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<CtorObject>(null), null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Foo);
        }

        [Test]
        public void DependencyChainIsFollowed()
        {
            MockBuilderContext ctx = CreateContext();

            CascadingCtorObject result = (CascadingCtorObject)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<CascadingCtorObject>(null), null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InnerCtorObject);
            Assert.IsNotNull(result.InnerCtorObject.Foo);
        }

        [Test]
        public void PassingAnExistingObjectReturnsThatObject()
        {
            MockBuilderContext ctx = CreateContext();

            object existing = new object();
            object result = ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), existing);

            Assert.AreSame(existing, result);
        }

        [Test]
        public void PassingAnExistingObjectRunsLaterStrategies()
        {
            MockBuilderContext ctx = CreateContext();
            MockStrategy mockStrategy = new MockStrategy();
            ctx.InnerChain.Add(mockStrategy);

            object existing = new object();
            object result = ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), existing);

            Assert.IsTrue(mockStrategy.WasRun);
        }

        [Test]
        public void MultiParameterCtorWorks()
        {
            MockBuilderContext ctx = CreateContext();

            MultiParamCtorObject result = (MultiParamCtorObject)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MultiParamCtorObject>(null), null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.O1);
            Assert.IsNotNull(result.O2);
        }

        [Test]
        public void PureValueTypeCanBeConstructed()
        {
            MockBuilderContext ctx = CreateContext();

            Assert.AreEqual(0, ctx.HeadOfChain.BuildUp(ctx, new BuildKey<int>(null), null));
        }

        [Test]
        public void ConstructorWithValueTypeWorks()
        {
            MockBuilderContext ctx = CreateContext();

            CtorValueTypeObject result = (CtorValueTypeObject)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<CtorValueTypeObject>(null), null);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.IntValue);
        }

        private MockBuilderContext CreateContext()
        {
            MockBuilderContext result = new MockBuilderContext();
            result.InnerChain.Add(new CreationStrategy());
            result.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            return result;
        }

        private class CtorObject
        {
            public object Foo;

            public CtorObject(object foo)
            {
                Foo = foo;
            }
        }

        private class CascadingCtorObject
        {
            public CtorObject InnerCtorObject;

            public CascadingCtorObject(CtorObject ctorObject)
            {
                InnerCtorObject = ctorObject;
            }
        }

        private class MultiParamCtorObject
        {
            public object O1;
            public object O2;

            public MultiParamCtorObject(object o1, object o2)
            {
                O1 = o1;
                O2 = o2;
            }
        }

        private class CtorValueTypeObject
        {
            public int IntValue;

            public CtorValueTypeObject(int i)
            {
                IntValue = i;
            }
        }

        private struct CtorValueType
        {
            public object ObjectValue;

            public CtorValueType(object o)
            {
                ObjectValue = o;
            }
        }

        private class MockStrategy : BuilderStrategy
        {
            public bool WasRun = false;

            public override object BuildUp(IBuilderContext context, object buildKey, object existing)
            {
                WasRun = true;
                return existing;
            }
        }
    }
}