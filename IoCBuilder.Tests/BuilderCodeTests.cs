using NUnit.Framework;
using System;
using System.Reflection;
using IoCBuilder.Lifetime;
using IoCBuilder.Location;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Parameters;
using IoCBuilder.Strategies.Property;
using IoCBuilder.Strategies.Singleton;
using IoCBuilder.Strategies.TypeMapping;

namespace IoCBuilder
{
    [TestFixture]
    public class BuilderCodeTests
    {
        [Test]
        public void CanCreateInstances()
        {
            Builder builder = new Builder();
            Locator locator = CreateLocator();
            MockBuilderContext context = new MockBuilderContext();
            ConstructorInfo constructor = typeof(SimpleObject).GetConstructor(new Type[] { typeof(int) });
            ConstructorCreationPolicy policy = new ConstructorCreationPolicy(constructor, new ValueParameter<int>(12));
            builder.Policies.Set<ICreationPolicy>(policy, new BuildKey<SimpleObject>(null));
            policy.Create(context, typeof(SimpleObject));

            SimpleObject m1 = builder.BuildUp<SimpleObject>(locator, null, null);
            SimpleObject m2 = builder.BuildUp<SimpleObject>(locator, null, null);

            Assert.IsNotNull(m1);
            Assert.IsNotNull(m2);
            Assert.AreEqual(12, m1.IntParam);
            Assert.AreEqual(12, m2.IntParam);
            Assert.IsTrue(m1 != m2);
        }

        [Test]
        public void CanCreateSingleton()
        {
            Builder builder = new Builder();
            Locator locator = CreateLocator();

            ConstructorCreationPolicy policy = new ConstructorCreationPolicy();
            policy.AddParameter(new ValueParameter<int>(12));
            builder.Policies.Set<ICreationPolicy>(policy, new BuildKey<SimpleObject>(null));
            builder.Policies.Set<ISingletonPolicy>(new SingletonPolicy(true), new BuildKey<SimpleObject>(null));

            SimpleObject m1 = builder.BuildUp<SimpleObject>(locator, null, null);
            SimpleObject m2 = builder.BuildUp<SimpleObject>(locator, null, null);

            Assert.AreSame(m1, m2);
        }

        [Test]
        public void CreateComplexObject()
        {
            Builder builder = new Builder();
            Locator locator = CreateLocator();

            ConstructorCreationPolicy policy = new ConstructorCreationPolicy();
            policy.AddParameter(new ValueParameter<int>(12));
            builder.Policies.Set<ICreationPolicy>(policy, new BuildKey<SimpleObject>(null));

            ConstructorCreationPolicy policy2 = new ConstructorCreationPolicy();
            policy2.AddParameter(new CreationParameter(typeof(SimpleObject)));
            builder.Policies.Set<ICreationPolicy>(policy2, new BuildKey<ComplexObject>(null));

            builder.Policies.Set<ISingletonPolicy>(new SingletonPolicy(true), new BuildKey<SimpleObject>(null));
            builder.Policies.Set<ISingletonPolicy>(new SingletonPolicy(true), new BuildKey<ComplexObject>(null));

            ComplexObject cm = builder.BuildUp<ComplexObject>(locator, null, null);
            SimpleObject m = builder.BuildUp<SimpleObject>(locator, null, null);

            Assert.AreSame(m, cm.SimpleObject);
            Assert.IsNotNull(cm);
            Assert.IsNotNull(cm.SimpleObject);
            Assert.AreEqual(12, cm.SimpleObject.IntParam);
        }

        [Test]
        public void CanCreateNamedInstance()
        {
            Builder builder = new Builder();
            Locator locator = CreateLocator();

            ConstructorCreationPolicy policy1 = new ConstructorCreationPolicy();
            policy1.AddParameter(new ValueParameter<int>(12));
            builder.Policies.Set<ICreationPolicy>(policy1, new BuildKey<SimpleObject>("Object1"));

            ConstructorCreationPolicy policy2 = new ConstructorCreationPolicy();
            policy2.AddParameter(new ValueParameter<int>(32));
            builder.Policies.Set<ICreationPolicy>(policy2, new BuildKey<SimpleObject>("Object2"));

            SimpleObject m1 = builder.BuildUp<SimpleObject>(locator, "Object1", null);
            SimpleObject m2 = builder.BuildUp<SimpleObject>(locator, "Object2", null);

            Assert.IsNotNull(m1);
            Assert.IsNotNull(m2);
            Assert.AreEqual(12, m1.IntParam);
            Assert.AreEqual(32, m2.IntParam);
            Assert.IsTrue(m1 != m2);
        }

        [Test]
        public void RefParamsCanAskForSpecificallyNamedObjects()
        {
            Builder builder = new Builder();
            Locator locator = CreateLocator();

            ConstructorCreationPolicy policy1 = new ConstructorCreationPolicy();
            policy1.AddParameter(new ValueParameter<int>(12));
            builder.Policies.Set<ICreationPolicy>(policy1, new BuildKey<SimpleObject>("Object1"));
            builder.Policies.Set<ISingletonPolicy>(new SingletonPolicy(true), new BuildKey<SimpleObject>("Object1"));

            ConstructorCreationPolicy policy2 = new ConstructorCreationPolicy();
            policy2.AddParameter(new ValueParameter<int>(32));
            builder.Policies.Set<ICreationPolicy>(policy2, new BuildKey<SimpleObject>("Object2"));
            builder.Policies.Set<ISingletonPolicy>(new SingletonPolicy(true), new BuildKey<SimpleObject>("Object2"));

            ConstructorCreationPolicy policy3 = new ConstructorCreationPolicy();
            policy3.AddParameter(new CreationParameter(typeof(SimpleObject), "Object2"));
            builder.Policies.Set<ICreationPolicy>(policy3, new BuildKey<ComplexObject>(null));
            builder.Policies.Set<ISingletonPolicy>(new SingletonPolicy(true), new BuildKey<ComplexObject>(null));

            ComplexObject cm = builder.BuildUp<ComplexObject>(locator, null, null);
            SimpleObject sm = builder.BuildUp<SimpleObject>(locator, "Object2", null);

            Assert.IsNotNull(cm);
            Assert.IsNotNull(cm.SimpleObject);
            Assert.AreEqual(32, cm.SimpleObject.IntParam);
            Assert.AreSame(sm, cm.SimpleObject);
        }

        [Test]
        public void CanInjectValuesIntoProperties()
        {
            Builder builder = new Builder();
            Locator locator = CreateLocator();

            PropertySetterPolicy policy = new PropertySetterPolicy();
            policy.Properties.Add("StringProperty", new NamedPropertySetterInfo("StringProperty", new ValueParameter<string>("Bar is here")));
            builder.Policies.Set<IPropertySetterPolicy>(policy, new BuildKey<SimpleObject>(null));

            SimpleObject sm = builder.BuildUp<SimpleObject>(locator, null, null);

            Assert.IsNotNull(sm);
            Assert.AreEqual("Bar is here", sm.StringProperty);
        }

        [Test]
        public void CanInjectMultiplePropertiesIncludingCreatedObjects()
        {
            Builder builder = new Builder();
            Locator locator = CreateLocator();

            ConstructorCreationPolicy policy = new ConstructorCreationPolicy();
            policy.AddParameter(new ValueParameter<int>(15));
            builder.Policies.Set<ICreationPolicy>(policy, new BuildKey<SimpleObject>(null));

            PropertySetterPolicy policy1 = new PropertySetterPolicy();
            policy1.Properties.Add("StringProperty", new NamedPropertySetterInfo("StringProperty", new ValueParameter<string>("Bar is here")));
            policy1.Properties.Add("SimpleObject", new NamedPropertySetterInfo("SimpleObject", new CreationParameter(typeof(SimpleObject))));
            builder.Policies.Set<IPropertySetterPolicy>(policy1, new BuildKey<ComplexObject>(null));

            ComplexObject co = builder.BuildUp<ComplexObject>(locator, null, null);

            Assert.IsNotNull(co);
            Assert.IsNotNull(co.SimpleObject);
            Assert.AreEqual("Bar is here", co.StringProperty);
            Assert.AreEqual(15, co.SimpleObject.IntParam);
        }

        [Test]
        public void CanCreateConcreteObjectByAskingForInterface()
        {
            Builder builder = new Builder();
            builder.Policies.Set<ITypeMappingPolicy>(new TypeMappingPolicy(typeof(SimpleObject), null), new BuildKey<ISimpleObject>(null));
            Locator locator = CreateLocator();

            ISimpleObject sm = builder.BuildUp<ISimpleObject>(locator, null, null);

            Assert.IsNotNull(sm);
            Assert.IsTrue(sm is SimpleObject);
        }

        [Test]
        public void CanCreateNamedConcreteObjectByAskingForNamedInterface()
        {
            Builder builder = new Builder();
            ConstructorCreationPolicy policy = new ConstructorCreationPolicy(null, new ValueParameter<int>(12));
            builder.Policies.Set<ICreationPolicy>(policy, new BuildKey<SimpleObject>("Foo"));
            builder.Policies.Set<ITypeMappingPolicy>(new TypeMappingPolicy(typeof(SimpleObject), null), new BuildKey<ISimpleObject>(null));
            builder.Policies.Set<ITypeMappingPolicy>(new TypeMappingPolicy(typeof(SimpleObject), "Foo"), new BuildKey<ISimpleObject>("sm2"));
            Locator locator = CreateLocator();

            ISimpleObject sm1 = builder.BuildUp<ISimpleObject>(locator, null, null);
            ISimpleObject sm2 = builder.BuildUp<ISimpleObject>(locator, "sm2", null);

            Assert.IsNotNull(sm1);
            Assert.IsNotNull(sm2);
            Assert.IsTrue(sm1 is SimpleObject);
            Assert.IsTrue(sm2 is SimpleObject);
            Assert.AreEqual(0, ((SimpleObject)sm1).IntParam);
            Assert.AreEqual(12, ((SimpleObject)sm2).IntParam);
        }

        [Test]
        public void CanAddStrategiesToBuilder()
        {
            Builder builder = new Builder();
            MockStrategy strategy = new MockStrategy();
            Locator locator = CreateLocator();

            builder.Strategies.Add(strategy, BuilderStage.PostInitialization);

            builder.BuildUp(locator, typeof(object), null, null);

            Assert.IsTrue(strategy.WasCalled);
        }

        [Test]
        public void CanCreateGenericType()
        {
            Builder builder = new Builder();
            Locator locator = CreateLocator();

            GenericObject<int> result = builder.BuildUp<GenericObject<int>>(locator, null, null);

            Assert.IsNotNull(result);
        }

        private Locator CreateLocator()
        {
            Locator locator = new Locator();
            ILifetimeContainer lifetime = new LifetimeContainer();
            locator.Add(typeof(ILifetimeContainer), lifetime);
            return locator;
        }

        private class GenericObject<TValue>
        {
            public TValue TheValue;

            public GenericObject(TValue theValue)
            {
                TheValue = theValue;
            }
        }

        public interface ISimpleObject
        {
        }

        public class SimpleObject : ISimpleObject
        {
            public int IntParam;
            private string stringProperty;

            public string StringProperty
            {
                get { return stringProperty; }
                set { stringProperty = value; }
            }

            public SimpleObject(int foo)
            {
                IntParam = foo;
            }
        }

        public class ComplexObject
        {
            private SimpleObject simpleObject;
            private string stringProperty;

            public SimpleObject SimpleObject
            {
                get { return simpleObject; }
                set { simpleObject = value; }
            }

            public string StringProperty
            {
                get { return stringProperty; }
                set { stringProperty = value; }
            }

            public ComplexObject(SimpleObject monk)
            {
                SimpleObject = monk;
            }
        }

        public class MockStrategy : BuilderStrategy
        {
            public bool WasCalled = false;

            public override object BuildUp(IBuilderContext context, object buildKey, object existing)
            {
                WasCalled = true;
                return null;
            }
        }
    }
}