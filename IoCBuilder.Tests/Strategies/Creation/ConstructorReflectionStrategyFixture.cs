using NUnit.Framework;
using System;
using IoCBuilder.Exceptions;
using IoCBuilder.Location;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Singleton;

namespace IoCBuilder
{
    // These "modes" describe the classed of behavior provided by DI.
    // 1. I need a new X. Don'typeToBuild reuse any existing ones.
    // 2. I need the unnamed X. Create it if it doesn'typeToBuild exist, else return the existing one.
    // 3. I need the X named Y. Create it if it doesn'typeToBuild exist, else return the existing one.
    // 4. I want the unnamed X. Return null if it doesn'typeToBuild exist.
    // 5. I want the X named Y. Return null if it doesn'typeToBuild exist.

    [TestFixture]
    public class ConstructorReflectionStrategyFixture
    {
        // Value type creation

        [Test]
        public void CanCreateValueTypesWithConstructorInjectionStrategyInPlace()
        {
            MockBuilderContext context = CreateContext();

            Assert.AreEqual(0, context.HeadOfChain.BuildUp(context, new BuildKey<int>(null), null));
        }

        // Invalid attribute combination

        [Test]
        public void SpecifyingMultipleConstructorsThrows()
        {
            MockBuilderContext context = CreateContext();

            Assert.Throws<InvalidAttributeException>(() => context.HeadOfChain.BuildUp(context, new BuildKey<MockInvalidDualConstructorAttributes>(null), null));
        }

        [Test]
        public void SpecifyingCreateNewAndDependencyThrows()
        {
            MockBuilderContext context = CreateContext();

            Assert.Throws<InvalidAttributeException>(() => context.HeadOfChain.BuildUp(context, new BuildKey<MockInvalidDualParameterAttributes>(null), null));
        }

        // Default behavior

        [Test]
        public void DefaultBehaviorIsMode2ForUndecoratedParameter()
        {
            MockBuilderContext context = CreateContext();

            MockUndecoratedObject obj1 = (MockUndecoratedObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockUndecoratedObject>(null), null);
            MockUndecoratedObject obj2 = (MockUndecoratedObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockUndecoratedObject>(null), null);

            Assert.AreSame(obj1.Foo, obj2.Foo);
        }

        [Test]
        public void WhenSingleConstructorIsPresentDecorationIsntRequired()
        {
            MockBuilderContext context = CreateContext();

            MockUndecoratedConstructorObject obj1 = (MockUndecoratedConstructorObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockUndecoratedConstructorObject>(null), null);
            MockUndecoratedConstructorObject obj2 = (MockUndecoratedConstructorObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockUndecoratedConstructorObject>(null), null);

            Assert.IsNotNull(obj1.Foo);
            Assert.AreSame(obj1.Foo, obj2.Foo);
        }

        // Mode 1

        [Test]
        public void CreateNewAttributeAlwaysCreatesNewObject()
        {
            MockBuilderContext context = CreateContext();

            MockRequiresNewObject depending1 = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>("Foo"), null);
            MockRequiresNewObject depending2 = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>("Bar"), null);

            Assert.IsNotNull(depending1);
            Assert.IsNotNull(depending2);
            Assert.IsNotNull(depending1.Foo);
            Assert.IsNotNull(depending2.Foo);
            Assert.IsFalse(Object.ReferenceEquals(depending1.Foo, depending2.Foo));
        }

        [Test]
        public void NamedAndUnnamedObjectsInLocatorDontGetUsedForCreateNew()
        {
            MockBuilderContext context = CreateContext();
            object unnamed = new object();
            object named = new object();
            context.Locator.Add(new BuildKey<object>(null), unnamed);
            context.Locator.Add(new BuildKey<object>("Foo"), named);

            MockRequiresNewObject depending1 = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>(null), null);
            MockRequiresNewObject depending2 = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>(null), null);

            Assert.IsFalse(depending1.Foo == unnamed);
            Assert.IsFalse(depending2.Foo == unnamed);
            Assert.IsFalse(depending1.Foo == named);
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
            MockBuilderContext context = CreateContext();

            MockDependingObject depending1 = (MockDependingObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingObject>(null), null);
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
            MockBuilderContext context = CreateContext();

            MockDependingNamedObject depending1 = (MockDependingNamedObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockDependingNamedObject>(null), null);
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

        private MockBuilderContext CreateContext(Locator locator)
        {
            MockBuilderContext result = new MockBuilderContext(locator);
            result.InnerChain.Add(new SingletonStrategy());
            result.InnerChain.Add(new ConstructorReflectionStrategy());
            result.InnerChain.Add(new CreationStrategy());
            result.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            result.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));
            return result;
        }

        public class SearchUpMockObject
        {
            public int Value;

            public SearchUpMockObject(
                [Dependency(SearchMode = SearchMode.Up, NotPresentBehavior = NotPresentBehavior.Throw)]
                int value)
            {
                this.Value = value;
            }
        }

        public class SearchLocalMockObject
        {
            public int Value;

            public SearchLocalMockObject(
                [Dependency(SearchMode = SearchMode.Local, NotPresentBehavior = NotPresentBehavior.Throw)]
                int value
                )
            {
                this.Value = value;
            }
        }

        public class ThrowingMockObject
        {
            [InjectionConstructor]
            public ThrowingMockObject([Dependency(NotPresentBehavior = NotPresentBehavior.Throw)] object foo)
            {
            }
        }

        public class NamedThrowingMockObject
        {
            [InjectionConstructor]
            public NamedThrowingMockObject([Dependency(Name = "Foo", NotPresentBehavior = NotPresentBehavior.Throw)] object foo)
            {
            }
        }

        public class MockDependingObject
        {
            private object injectedObject;

            public MockDependingObject([Dependency] object injectedObject)
            {
                this.injectedObject = injectedObject;
            }

            public virtual object InjectedObject
            {
                get { return injectedObject; }
                set { injectedObject = value; }
            }
        }

        public class MockOptionalDependingObject
        {
            private object injectedObject;

            public MockOptionalDependingObject
                (
                    [Dependency(NotPresentBehavior = NotPresentBehavior.ReturnNull)] object injectedObject
                )
            {
                this.injectedObject = injectedObject;
            }

            public object InjectedObject
            {
                get { return injectedObject; }
                set { injectedObject = value; }
            }
        }

        public class MockOptionalDependingObjectWithName
        {
            private object injectedObject;

            public MockOptionalDependingObjectWithName
                (
                    [Dependency(Name = "Foo", NotPresentBehavior = NotPresentBehavior.ReturnNull)] object injectedObject
                )
            {
                this.injectedObject = injectedObject;
            }

            public object InjectedObject
            {
                get { return injectedObject; }
                set { injectedObject = value; }
            }
        }

        public class MockDependingNamedObject
        {
            private object injectedObject;

            public MockDependingNamedObject([Dependency(Name = "Foo")] object injectedObject)
            {
                this.injectedObject = injectedObject;
            }

            public object InjectedObject
            {
                get { return injectedObject; }
                set { injectedObject = value; }
            }
        }

        public class MockDependsOnIFoo
        {
            private IFoo foo;

            public MockDependsOnIFoo([Dependency(CreateType = typeof(Foo))] IFoo foo)
            {
                this.foo = foo;
            }

            public IFoo Foo
            {
                get { return foo; }
                set { foo = value; }
            }
        }

        public class MockDependsOnNamedIFoo
        {
            private IFoo foo;

            public MockDependsOnNamedIFoo([Dependency(Name = "Foo", CreateType = typeof(Foo))] IFoo foo)
            {
                this.foo = foo;
            }

            public IFoo Foo
            {
                get { return foo; }
                set { foo = value; }
            }
        }

        public class MockRequiresNewObject
        {
            private object foo;

            public MockRequiresNewObject([CreateNew] object foo)
            {
                this.foo = foo;
            }

            public virtual object Foo
            {
                get { return foo; }
                set { foo = value; }
            }
        }

        public interface IFoo { }

        public class Foo : IFoo { }

        private class MockInvalidDualParameterAttributes
        {
            [InjectionConstructor]
            public MockInvalidDualParameterAttributes([CreateNew][Dependency]object obj)
            {
            }
        }

        private class MockInvalidDualConstructorAttributes
        {
            [InjectionConstructor]
            public MockInvalidDualConstructorAttributes(object obj)
            {
            }

            [InjectionConstructor]
            public MockInvalidDualConstructorAttributes(int i)
            {
            }
        }

        private class MockUndecoratedObject
        {
            public object Foo;

            [InjectionConstructor]
            public MockUndecoratedObject(object foo)
            {
                Foo = foo;
            }
        }

        private class MockUndecoratedConstructorObject
        {
            public object Foo;

            public MockUndecoratedConstructorObject(object foo)
            {
                Foo = foo;
            }
        }
    }
}