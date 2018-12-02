using NUnit.Framework;
using System;
using IoCBuilder.Exceptions;
using IoCBuilder.Strategies.Parameters;
using IoCBuilder.Strategies.Property;

namespace IoCBuilder
{
    [TestFixture]
    public class PropertySetterStrategyFixture
    {
        [Test]
        public void ReturnsNullWhenPassedNull()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            PropertySetterStrategy strategy = new PropertySetterStrategy();
            Assert.IsNull(strategy.BuildUp<object>(ctx, null, null));
        }

        [Test]
        public void CanInjectProperties()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            PropertySetterStrategy strategy = new PropertySetterStrategy();

            PropertySetterPolicy policy1 = new PropertySetterPolicy();
            policy1.Properties.Add("Foo", new NamedPropertySetterInfo("Foo", new ValueParameter<string>("value for foo")));
            ctx.Policies.Set<IPropertySetterPolicy>(policy1, new BuildKey<MockInjectionTarget>(null));

            MockInjectionTarget target = new MockInjectionTarget();
            strategy.BuildUp<MockInjectionTarget>(ctx, target, null);

            Assert.AreEqual("value for foo", target.Foo);
        }

        [Test]
        public void InjectionIsBasedOnConcreteTypeNotRequestedType()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            PropertySetterStrategy strategy = new PropertySetterStrategy();

            PropertySetterPolicy policy1 = new PropertySetterPolicy();
            policy1.Properties.Add("Foo", new NamedPropertySetterInfo("Foo", new ValueParameter<string>("value for foo")));
            ctx.Policies.Set<IPropertySetterPolicy>(policy1, new BuildKey<MockInjectionTarget>(null));

            MockInjectionTarget target = new MockInjectionTarget();
            strategy.BuildUp<object>(ctx, target, null);

            Assert.AreEqual("value for foo", target.Foo);
        }

        [Test]
        public void ThrowsIfPropertyInjectedIsReadOnly()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            PropertySetterStrategy strategy = new PropertySetterStrategy();

            PropertySetterPolicy policy1 = new PropertySetterPolicy();
            policy1.Properties.Add("Bar", new NamedPropertySetterInfo("Bar", new ValueParameter<string>("value for foo")));
            ctx.Policies.Set<IPropertySetterPolicy>(policy1, new BuildKey<MockInjectionTarget>(null));

            MockInjectionTarget target = new MockInjectionTarget();
            Assert.Throws<ArgumentException>(() => strategy.BuildUp<object>(ctx, target, null));
        }

        [Test]
        public void IncompatibleTypesThrow()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            PropertySetterStrategy strategy = new PropertySetterStrategy();

            PropertySetterPolicy policy = new PropertySetterPolicy();
            policy.Properties.Add("Foo", new NamedPropertySetterInfo("Foo", new ValueParameter<object>(new object())));
            ctx.Policies.Set<IPropertySetterPolicy>(policy, new BuildKey<MockInjectionTarget>(null));

            MockInjectionTarget target = new MockInjectionTarget();
            Assert.Throws<IncompatibleTypesException>(() => strategy.BuildUp<MockInjectionTarget>(ctx, target, null));
        }

        // ---------------------------------------------------------------------
        //  Test List
        // ---------------------------------------------------------------------
        // - Type conversion?
        // - What if we have a mapping that doesn'typeToBuild match a property?
        // - What is the property is not public?

        private class MockInjectionTarget
        {
            private string foo = null;

            public string Foo
            {
                get { return foo; }
                set { foo = value; }
            }

            public string Bar { get { return null; } }
        }
    }
}