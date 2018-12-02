using NUnit.Framework;
using System;
using System.Reflection;
using IoCBuilder.Exceptions;
using IoCBuilder.Lifetime;
using IoCBuilder.Location;
using IoCBuilder.Policies;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Parameters;
using IoCBuilder.Strategies.Singleton;

namespace IoCBuilder
{
    [TestFixture]
    public class CreationStrategyFixture
    {
        [Test]
        public void CreationStrategyWithNoPoliciesFails()
        {
            MockBuilderContext ctx = CreateContext();

            Assert.Throws<ArgumentException>(() => ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), null));
        }

        [Test]
        public void CreationStrategyUsesSingletonPolicyToLocateCreatedItems()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            ctx.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));

            object obj = ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), null);

            Assert.AreEqual(1, container.Count);
            Assert.AreSame(obj, ctx.Locator.Get(new BuildKey<object>(null)));
        }

        [Test]
        public void CreationStrategyOnlyLocatesItemIfSingletonPolicySetForThatType()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            ctx.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));
            ctx.Policies.Set<ISingletonPolicy>(new SingletonPolicy(false), new BuildKey<object>(null));

            object obj = ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), null);

            Assert.AreEqual(0, container.Count);
            Assert.IsNull(ctx.Locator.Get(new BuildKey<object>(null)));
        }

        [Test]
        public void AllCreatedDependenciesArePlacedIntoLocatorAndLifetimeContainer()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            ctx.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));

            MockDependingObject obj = (MockDependingObject)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockDependingObject>(null), null);

            Assert.AreEqual(2, container.Count);
            Assert.AreSame(obj, ctx.Locator.Get(new BuildKey<MockDependingObject>(null)));
            Assert.AreSame(obj.DependentObject, ctx.Locator.Get(new BuildKey<MockDependentObject>(null)));
        }

        [Test]
        public void InjectedDependencyIsReusedWhenDependingObjectIsCreatedTwice()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            ctx.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));

            MockDependingObject obj1 = (MockDependingObject)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockDependingObject>(null), null);
            MockDependingObject obj2 = (MockDependingObject)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockDependingObject>(null), null);

            Assert.AreSame(obj1.DependentObject, obj2.DependentObject);
        }

        [Test]
        public void NamedObjectsOfSameTypeAreUnique()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            ctx.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));

            object obj1 = ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>("Foo"), null);
            object obj2 = ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>("Bar"), null);

            Assert.AreEqual(2, container.Count);
            Assert.IsFalse(object.ReferenceEquals(obj1, obj2));
        }

        [Test]
        public void CircularDependenciesCanBeResolved()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            ctx.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));

            CircularDependency1 d1 = (CircularDependency1)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<CircularDependency1>(null), null);

            Assert.IsNotNull(d1);
            Assert.IsNotNull(d1.Depends2);
            Assert.IsNotNull(d1.Depends2.Depends1);
            Assert.AreSame(d1, d1.Depends2.Depends1);
            Assert.AreEqual(2, container.Count);
        }

        [Test]
        public void CreatingAbstractTypeThrows()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            ctx.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));

            Assert.Throws<ArgumentException>(() => ctx.HeadOfChain.BuildUp(ctx, new BuildKey<AbstractClass>(null), null));
        }

        [Test]
        public void CanCreateValueTypes()
        {
            MockBuilderContext ctx = CreateContext();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());

            Assert.AreEqual(0, (int)ctx.HeadOfChain.BuildUp(ctx, new BuildKey<int>(null), null));
        }

        [Test]
        public void CannotCreateStrings()
        {
            MockBuilderContext ctx = CreateContext();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());

            Assert.Throws<ArgumentException>(() => ctx.HeadOfChain.BuildUp(ctx, new BuildKey<string>(null), null));
        }

        [Test]
        public void NotFindingAMatchingConstructorThrows()
        {
            MockBuilderContext ctx = CreateContext();
            FailingCreationPolicy policy = new FailingCreationPolicy();
            ctx.Policies.SetDefault<ICreationPolicy>(policy);

            Assert.Throws<ArgumentException>(() => ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), null));
        }

        [Test]
        public void CreationStrategyWillLocateExistingObjects()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ctx.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            ctx.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));
            object obj = new object();

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), obj);

            Assert.AreEqual(1, container.Count);
            Assert.AreSame(obj, ctx.Locator.Get(new BuildKey<object>(null)));
        }

        [Test]
        public void IncompatibleTypesThrows()
        {
            MockBuilderContext ctx = CreateContext();
            ILifetimeContainer container = ctx.Locator.Get<ILifetimeContainer>();
            ConstructorInfo ci = typeof(MockObject).GetConstructor(new Type[] { typeof(int) });
            ICreationPolicy policy = new ConstructorCreationPolicy(ci, new ValueParameter<string>(String.Empty));
            ctx.Policies.Set<ICreationPolicy>(policy, new BuildKey<MockObject>(null));

            Assert.Throws<IncompatibleTypesException>(() =>
            {
                object obj = ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), null);
            });
        }

        [Test]
        public void CreationPolicyWillRecordSingletonsUsingLocalLifetimeContainerOnly()
        {
            BuilderStrategyChain chain = new BuilderStrategyChain();
            chain.Add(new CreationStrategy());

            Locator parentLocator = new Locator();
            ILifetimeContainer container = new LifetimeContainer();
            parentLocator.Add(typeof(ILifetimeContainer), container);

            Locator childLocator = new Locator(parentLocator);

            IPolicyList policies = new PolicyList();
            policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));

            BuildKey buildKey = new BuildKey<object>(null);

            BuilderContext ctx = new BuilderContext(chain, childLocator, policies, buildKey);

            object obj = ctx.HeadOfChain.BuildUp(ctx, buildKey, null);

            Assert.IsNotNull(obj);
            Assert.IsNull(childLocator.Get(new BuildKey<object>(null)));
        }

        #region Helpers

        private class MockObject
        {
            private int foo;

            public MockObject(int foo)
            {
                this.foo = foo;
            }
        }

        internal class FailingCreationPolicy : ICreationPolicy
        {
            //public ConstructorInfo SelectConstructor(IBuilderContext context, Type type, string id)
            //{
            //    return null;
            //}

            //public object[] GetParameters(IBuilderContext context, Type type, string id, ConstructorInfo ci)
            //{
            //    return new object[] { };
            //}

            public bool SupportsReflection
            {
                get
                {
                    return false;
                }
            }

            public object Create(IBuilderContext context, object buildKey)
            {
                throw new NotImplementedException();
            }

            public ConstructorInfo GetConstructor(IBuilderContext context, object buildKey)
            {
                return null;
            }

            public object[] GetParameters(IBuilderContext context, ConstructorInfo constructor)
            {
                return new object[] { };
            }
        }

        private MockBuilderContext CreateContext()
        {
            MockBuilderContext result = new MockBuilderContext();
            result.InnerChain.Add(new SingletonStrategy());
            result.InnerChain.Add(new CreationStrategy());
            return result;
        }

        private abstract class AbstractClass
        {
            public AbstractClass()
            {
            }
        }

        private class MockDependingObject
        {
            public object DependentObject;

            public MockDependingObject(MockDependentObject obj)
            {
                DependentObject = obj;
            }
        }

        private class MockDependentObject
        {
        }

        private class CircularDependency1
        {
            private Guid ID;

            public CircularDependency2 Depends2;

            public CircularDependency1(CircularDependency2 depends2)
            {
                ID = Guid.NewGuid();
                Depends2 = depends2;
            }
        }

        private class CircularDependency2
        {
            private Guid ID;

            public CircularDependency1 Depends1;

            public CircularDependency2(CircularDependency1 depends1)
            {
                ID = Guid.NewGuid();
                Depends1 = depends1;
            }
        }

        #endregion Helpers
    }
}