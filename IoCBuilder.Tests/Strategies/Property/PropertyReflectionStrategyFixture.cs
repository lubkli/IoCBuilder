using NUnit.Framework;
using System;
using IoCBuilder.Exceptions;
using IoCBuilder.Location;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Parameters;
using IoCBuilder.Strategies.Property;
using IoCBuilder.Strategies.Singleton;
using IoCBuilder;

namespace IoCBuilder
{
    // These "modes" describe the classed of behavior provided by DI.
    // 1. I need a new X. Don'typeToBuild reuse any existing ones.
    // 2. I need the unnamed X. Create it if it doesn'typeToBuild exist, else return the existing one.
    // 3. I need the X named Y. Create it if it doesn'typeToBuild exist, else return the existing one.
    // 4. I want the unnamed X. Return null if it doesn'typeToBuild exist.
    // 5. I want the X named Y. Return null if it doesn'typeToBuild exist.

    [TestFixture]
    public class PropertyReflectionStrategyFixture
    {
        // Invalid attribute combination

        [Test]
        public void SpecifyingCreateNewAndDependencyThrows()
        {
            MockBuilderContext context = CreateContext();

            Assert.Throws<InvalidAttributeException>(() => context.HeadOfChain.BuildUp(context, new BuildKey<MockInvalidDualAttributes>(null), null));
        }

        // Existing policy

        [Test]
        public void PropertyReflectionWillNotOverwriteAPreExistingPolicyForAProperty()
        {
            MockBuilderContext context = CreateContext();
            PropertySetterPolicy policy = new PropertySetterPolicy();
            policy.Properties.Add("Foo", new NamedPropertySetterInfo("Foo", new ValueParameter(typeof(object), 12)));
            context.Policies.Set<IPropertySetterPolicy>(policy, new BuildKey<MockRequiresNewObject>(null));

            MockRequiresNewObject obj = (MockRequiresNewObject)context.HeadOfChain.BuildUp(context, new BuildKey<MockRequiresNewObject>(null), null);

            Assert.AreEqual(12, obj.Foo);
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
            result.InnerChain.Add(new PropertyReflectionStrategy());
            result.InnerChain.Add(new CreationStrategy());
            result.InnerChain.Add(new PropertySetterStrategy());
            result.Policies.SetDefault<ISingletonPolicy>(new SingletonPolicy(true));
            result.Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());
            return result;
        }

        #region Mock Classes

        public class SearchUpMockObject
        {
            private int value;

            [Dependency(SearchMode = SearchMode.Up, NotPresentBehavior = NotPresentBehavior.Throw)]
            public int Value
            {
                get { return value; }
                set { this.value = value; }
            }
        }

        public class SearchLocalMockObject
        {
            private int value;

            [Dependency(SearchMode = SearchMode.Local, NotPresentBehavior = NotPresentBehavior.Throw)]
            public int Value
            {
                get { return value; }
                set { this.value = value; }
            }
        }

        public class ThrowingMockObject
        {
            [Dependency(NotPresentBehavior = NotPresentBehavior.Throw)]
            public object InjectedObject
            {
                set { }
            }
        }

        public class NamedThrowingMockObject
        {
            [Dependency(Name = "Foo", NotPresentBehavior = NotPresentBehavior.Throw)]
            public object InjectedObject
            {
                set { }
            }
        }

        public class MockInvalidDualAttributes
        {
            private int value;

            [CreateNew]
            [Dependency]
            public int Value
            {
                get { return value; }
                set { this.value = value; }
            }
        }

        private interface ISomeInterface
        {
        }

        private class MockDependsOnInterface
        {
            private ISomeInterface value;

            [Dependency]
            public ISomeInterface Value
            {
                get { return value; }
                set { this.value = value; }
            }
        }

        public class MockDependingObject
        {
            private object injectedObject;

            [Dependency]
            public virtual object InjectedObject
            {
                get { return injectedObject; }
                set { injectedObject = value; }
            }
        }

        public class MockDependingObjectDerived : MockDependingObject
        {
            public override object InjectedObject
            {
                get
                {
                    return base.InjectedObject;
                }
                set
                {
                    base.InjectedObject = value;
                }
            }
        }

        public class MockOptionalDependingObject
        {
            private object injectedObject;

            [Dependency(NotPresentBehavior = NotPresentBehavior.ReturnNull)]
            public object InjectedObject
            {
                get { return injectedObject; }
                set { injectedObject = value; }
            }
        }

        public class MockOptionalDependingObjectWithName
        {
            private object injectedObject;

            [Dependency(Name = "Foo", NotPresentBehavior = NotPresentBehavior.ReturnNull)]
            public object InjectedObject
            {
                get { return injectedObject; }
                set { injectedObject = value; }
            }
        }

        public class MockDependingNamedObject
        {
            private object injectedObject;

            [Dependency(Name = "Foo")]
            public object InjectedObject
            {
                get { return injectedObject; }
                set { injectedObject = value; }
            }
        }

        public class MockDependsOnIFoo
        {
            private IFoo foo;

            [Dependency(CreateType = typeof(Foo))]
            public IFoo Foo
            {
                get { return foo; }
                set { foo = value; }
            }
        }

        public class MockDependsOnNamedIFoo
        {
            private IFoo foo;

            [Dependency(Name = "Foo", CreateType = typeof(Foo))]
            public IFoo Foo
            {
                get { return foo; }
                set { foo = value; }
            }
        }

        public class MockRequiresNewObject
        {
            private object foo;

            [CreateNew]
            public virtual object Foo
            {
                get { return foo; }
                set { foo = value; }
            }
        }

        public class MockRequiresNewObjectDerived : MockRequiresNewObject
        {
            public override object Foo
            {
                get
                {
                    return base.Foo;
                }
                set
                {
                    base.Foo = value;
                }
            }
        }

        public interface IFoo { }

        public class Foo : IFoo { }

        #endregion Mock Classes
    }
}