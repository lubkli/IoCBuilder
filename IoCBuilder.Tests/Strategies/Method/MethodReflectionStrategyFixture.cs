using NUnit.Framework;
using System;
using IoCBuilder.Exceptions;
using IoCBuilder.Location;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Method;
using IoCBuilder.Strategies.Singleton;

namespace IoCBuilder
{
    [TestFixture]
    public class MethodReflectionStrategyFixture
    {
        // Invalid attribute combination

        [Test]
        public void SpecifyingCreateNewAndDependencyThrows()
        {
            MockBuilderContext context = CreateContext();

            Assert.Throws<InvalidAttributeException>(() => context.HeadOfChain.BuildUp(context, new BuildKey<MockInvalidDualAttributes>(null), null));
        }

        // Attribute Inheritance

        [Test]
        public void CanInheritDependencyAttribute()
        {
            MockBuilderContext context = CreateContext();

            MockDependingObjectDerived depending = (MockDependingObjectDerived)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingObjectDerived>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsNotNull(depending.InjectedObject);
        }

        [Test]
        public void CanInheritCreateNewAttribute()
        {
            MockBuilderContext context = CreateContext();

            MockRequiresNewObjectDerived depending = (MockRequiresNewObjectDerived)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObjectDerived>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsNotNull(depending.Foo);
        }

        // Non creatable stuff

        [Test]
        public void ThrowsIfConcreteTypeToCreateCannotBeCreated()
        {
            MockBuilderContext context = CreateContext();
            Assert.Throws<ArgumentException>(() => context.HeadOfChain.BuildUp(context, new BuildKey<MockDependsOnInterface>(null), null));
        }

        // Mode 1

        [Test]
        public void CreateNewAttributeAlwaysCreatesNewObject()
        {
            MockBuilderContext context;

            context = CreateContext();
            MockRequiresNewObject depending1 = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>(null), null);

            context = CreateContext();
            MockRequiresNewObject depending2 = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>(null), null);

            Assert.IsNotNull(depending1);
            Assert.IsNotNull(depending2);
            Assert.IsNotNull(depending1.Foo);
            Assert.IsNotNull(depending2.Foo);
            Assert.IsFalse(depending1.Foo == depending2.Foo);
        }

        [Test]
        public void NamedAndUnnamedObjectsInLocatorDontGetUsedForCreateNew()
        {
            MockBuilderContext context;
            object unnamed = new object();
            object named = new object();

            context = CreateContext();
            context.Locator.Add(new BuildKey<object>(null), unnamed);
            context.Locator.Add(new BuildKey<object>("Foo"), named);
            MockRequiresNewObject depending1 = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>(null), null);

            context = CreateContext();
            context.Locator.Add(new BuildKey<object>(null), unnamed);
            context.Locator.Add(new BuildKey<object>("Foo"), named);
            MockRequiresNewObject depending2 = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>(null), null);

            Assert.IsFalse(depending1.Foo == unnamed);
            Assert.IsFalse(depending1.Foo == unnamed);
            Assert.IsFalse(depending2.Foo == named);
            Assert.IsFalse(depending2.Foo == named);
        }

        // Mode 2

        [Test]
        public void CanInjectExistingUnnamedObjectIntoProperty()
        {
            // Mode 2, with an existing object
            MockBuilderContext context = CreateContext();
            object dependent = new object();
            context.InnerLocator.Add(new BuildKey<object>(null), dependent);

            object depending = context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingObject>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsTrue(depending is MockDependingObject);
            Assert.AreSame(dependent, ((MockDependingObject)depending).InjectedObject);
        }

        [Test]
        public void InjectionCreatingNewUnnamedObjectWillOnlyCreateOnce()
        {
            // Mode 2, both flavors
            MockBuilderContext context;

            context = CreateContext();
            MockDependingObject depending1 = (MockDependingObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingObject>(null), null);

            context = CreateContext(context.Locator);
            MockDependingObject depending2 = (MockDependingObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingObject>(null), null);

            Assert.AreSame(depending1.InjectedObject, depending2.InjectedObject);
        }

        [Test]
        public void InjectionCreatesNewObjectIfNotExisting()
        {
            // Mode 2, no existing object
            MockBuilderContext context = CreateContext();

            object depending = context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingObject>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsTrue(depending is MockDependingObject);
            Assert.IsNotNull(((MockDependingObject)depending).InjectedObject);
        }

        [Test]
        public void CanInjectNewInstanceWithExplicitTypeIfNotExisting()
        {
            // Mode 2, explicit type
            MockBuilderContext context = CreateContext();

            MockDependsOnIFoo depending = (MockDependsOnIFoo)context.HeadOfChain.BuildUp(
                context, new BuildKey<MockDependsOnIFoo>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsNotNull(depending.Foo);
        }

        // Mode 3

        [Test]
        public void CanInjectExistingNamedObjectIntoProperty()
        {
            // Mode 3, with an existing object
            MockBuilderContext context = CreateContext();
            object dependent = new object();
            context.InnerLocator.Add(new BuildKey<object>("Foo"), dependent);

            object depending = context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingNamedObject>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsTrue(depending is MockDependingNamedObject);
            Assert.AreSame(dependent, ((MockDependingNamedObject)depending).InjectedObject);
        }

        [Test]
        public void InjectionCreatingNewNamedObjectWillOnlyCreateOnce()
        {
            // Mode 3, both flavors
            MockBuilderContext context;

            context = CreateContext();
            MockDependingNamedObject depending1 = (MockDependingNamedObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingNamedObject>(null), null);

            context = CreateContext(context.Locator);
            MockDependingNamedObject depending2 = (MockDependingNamedObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingNamedObject>(null), null);

            Assert.AreSame(depending1.InjectedObject, depending2.InjectedObject);
        }

        [Test]
        public void InjectionCreatesNewNamedObjectIfNotExisting()
        {
            // Mode 3, no existing object
            MockBuilderContext context = CreateContext();

            MockDependingNamedObject depending = (MockDependingNamedObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingNamedObject>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsNotNull(depending.InjectedObject);
        }

        [Test]
        public void CanInjectNewNamedInstanceWithExplicitTypeIfNotExisting()
        {
            // Mode 3, explicit type
            MockBuilderContext context = CreateContext();

            MockDependsOnNamedIFoo depending = (MockDependsOnNamedIFoo)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependsOnNamedIFoo>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsNotNull(depending.Foo);
        }

        // Mode 2 & 3 together

        [Test]
        public void NamedAndUnnamedObjectsDontCollide()
        {
            MockBuilderContext context = CreateContext();
            object dependent = new object();
            context.InnerLocator.Add(new BuildKey<object>(null), dependent);

            MockDependingNamedObject depending = (MockDependingNamedObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingNamedObject>(null), null);

            Assert.IsFalse(Object.ReferenceEquals(dependent, depending.InjectedObject));
        }

        // Mode 4

        [Test]
        public void PropertyIsNullIfUnnamedNotExists()
        {
            // Mode 4, no object provided
            MockBuilderContext context = CreateContext();

            MockOptionalDependingObject depending = (MockOptionalDependingObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockOptionalDependingObject>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsNull(depending.InjectedObject);
        }

        [Test]
        public void CanInjectExistingUnnamedObjectIntoOptionalDependentProperty()
        {
            // Mode 4, with an existing object
            MockBuilderContext context = CreateContext();
            object dependent = new object();
            context.InnerLocator.Add(new BuildKey<object>(null), dependent);

            object depending = context.HeadOfChain.BuildUp(context, new BuildKey<MockOptionalDependingObject>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsTrue(depending is MockOptionalDependingObject);
            Assert.AreSame(dependent, ((MockOptionalDependingObject)depending).InjectedObject);
        }

        // Mode 5

        [Test]
        public void PropertyIsNullIfNamedNotExists()
        {
            // Mode 5, no object provided
            MockBuilderContext context = CreateContext();

            MockOptionalDependingObjectWithName depending = (MockOptionalDependingObjectWithName)context.HeadOfChain.BuildUp(context, new BuildKey<MockOptionalDependingObjectWithName>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsNull(depending.InjectedObject);
        }

        [Test]
        public void CanInjectExistingNamedObjectIntoOptionalDependentProperty()
        {
            // Mode 5, with an existing object
            MockBuilderContext context = CreateContext();
            object dependent = new object();
            context.InnerLocator.Add(new BuildKey<object>("Foo"), dependent);

            object depending = context.HeadOfChain.BuildUp(context, new BuildKey<MockOptionalDependingObjectWithName>(null), null);

            Assert.IsNotNull(depending);
            Assert.IsTrue(depending is MockOptionalDependingObjectWithName);
            Assert.AreSame(dependent, ((MockOptionalDependingObjectWithName)depending).InjectedObject);
        }

        // NotPresentBehavior.Throw Tests

        [Test]
        public void StrategyThrowsIfObjectNotPresent()
        {
            MockBuilderContext context = CreateContext();

            Assert.Throws<DependencyMissingException>(() => context.HeadOfChain.BuildUp(context, new BuildKey<ThrowingMockObject>(null), null));
        }

        [Test]
        public void StrategyThrowsIfNamedObjectNotPresent()
        {
            MockBuilderContext context = CreateContext();

            Assert.Throws<DependencyMissingException>(() => context.HeadOfChain.BuildUp(context, new BuildKey<NamedThrowingMockObject>(null), null));
        }

        // SearchMode Tests

        [Test]
        public void CanSearchDependencyUp()
        {
            Locator parent = new Locator();
            parent.Add(new BuildKey<int>(null), 25);
            Locator child = new Locator(parent);
            MockBuilderContext context = CreateContext(child);

            context.HeadOfChain.BuildUp(context, new BuildKey<SearchUpMockObject>(null), null);
        }

        [Test]
        public void LocalSearchFailsIfDependencyIsOnlyUpstream()
        {
            Locator parent = new Locator();
            parent.Add(new BuildKey<int>(null), 25);
            Locator child = new Locator(parent);
            MockBuilderContext context = CreateContext(child);

            Assert.Throws<DependencyMissingException>(() => context.HeadOfChain.BuildUp(context, new BuildKey<SearchLocalMockObject>(null), null));
        }

        [Test]
        public void LocalSearchGetsLocalIfDependencyIsAlsoUpstream()
        {
            Locator parent = new Locator();
            parent.Add(new BuildKey<int>(null), 25);
            Locator child = new Locator(parent);
            child.Add(new BuildKey<int>(null), 15);
            MockBuilderContext context = CreateContext(child);

            SearchLocalMockObject obj = (SearchLocalMockObject)context.HeadOfChain.BuildUp(context, new BuildKey<SearchLocalMockObject>(null), null);

            Assert.AreEqual(15, obj.Value);
        }

        // Helpers

        private MockBuilderContext CreateContext()
        {
            return CreateContext(new Locator());
        }

        private MockBuilderContext CreateContext(IReadWriteLocator locator)
        {
            MockBuilderContext result = new MockBuilderContext(locator);
            result.InnerChain.Add(new SingletonStrategy());
            result.InnerChain.Add(new MethodReflectionStrategy());
            result.InnerChain.Add(new CreationStrategy());
            result.InnerChain.Add(new MethodCallStrategy());
            result.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            result.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));
            return result;
        }

        #region Mock Classes

        public class SearchUpMockObject
        {
            public int Value;

            [InjectionMethod]
            public void SetValue([Dependency(SearchMode = SearchMode.Up, NotPresentBehavior = NotPresentBehavior.Throw)] int value)
            {
                Value = value;
            }
        }

        public class SearchLocalMockObject
        {
            public int Value;

            [InjectionMethod]
            public void SetValue([Dependency(SearchMode = SearchMode.Local, NotPresentBehavior = NotPresentBehavior.Throw)] int value)
            {
                Value = value;
            }
        }

        public class ThrowingMockObject
        {
            [InjectionMethod]
            public void SetValue([Dependency(NotPresentBehavior = NotPresentBehavior.Throw)] object obj)
            {
            }
        }

        public class NamedThrowingMockObject
        {
            [InjectionMethod]
            public void SetValue([Dependency(Name = "Foo", NotPresentBehavior = NotPresentBehavior.Throw)] object obj)
            {
            }
        }

        public class MockInvalidDualAttributes
        {
            [InjectionMethod]
            public void SetValue([CreateNew][Dependency] int dummy)
            {
            }
        }

        public class MockDependsOnInterface
        {
            [InjectionMethod]
            public void DoSomething(IFoo foo)
            {
            }
        }

        public class MockDependingObject
        {
            public object InjectedObject;

            [InjectionMethod]
            public virtual void DoSomething([Dependency] object injectedObject)
            {
                InjectedObject = injectedObject;
            }
        }

        public class MockDependingObjectDerived : MockDependingObject
        {
            public override void DoSomething(object injectedObject)
            {
                base.DoSomething(injectedObject);
            }
        }

        public class MockOptionalDependingObject
        {
            public object InjectedObject;

            [InjectionMethod]
            public void SetObject([Dependency(NotPresentBehavior = NotPresentBehavior.ReturnNull)] object foo)
            {
                InjectedObject = foo;
            }
        }

        public class MockOptionalDependingObjectWithName
        {
            public object InjectedObject;

            [InjectionMethod]
            public void SetObject([Dependency(Name = "Foo", NotPresentBehavior = NotPresentBehavior.ReturnNull)] object foo)
            {
                InjectedObject = foo;
            }
        }

        public class MockDependingNamedObject
        {
            public object InjectedObject;

            [InjectionMethod]
            public void SetObject([Dependency(Name = "Foo")] object foo)
            {
                InjectedObject = foo;
            }
        }

        public class MockDependsOnIFoo
        {
            public IFoo Foo;

            [InjectionMethod]
            public void SetFoo([Dependency(CreateType = typeof(Foo))] IFoo foo)
            {
                Foo = foo;
            }
        }

        public class MockDependsOnNamedIFoo
        {
            public IFoo Foo;

            [InjectionMethod]
            public void SetFoo([Dependency(Name = "Foo", CreateType = typeof(Foo))] IFoo foo)
            {
                Foo = foo;
            }
        }

        public class MockRequiresNewObject
        {
            public object Foo;

            [InjectionMethod]
            public virtual void SetFoo([CreateNew] object foo)
            {
                Foo = foo;
            }
        }

        public class MockRequiresNewObjectDerived : MockRequiresNewObject
        {
            public override void SetFoo(object foo)
            {
                base.SetFoo(foo);
            }
        }

        public interface IFoo { }

        public class Foo : IFoo { }

        #endregion Mock Classes
    }
}