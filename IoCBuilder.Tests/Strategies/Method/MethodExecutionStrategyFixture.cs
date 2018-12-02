using NUnit.Framework;
using System;
using IoCBuilder.Exceptions;
using IoCBuilder.Strategies.Method;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder
{
    [TestFixture]
    public class MethodCallStrategyFixture
    {
        #region Success Cases

        [Test]
        public void StrategyCallsParameterlessMethod()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            policy.Methods.Add(/*"ParameterlessMethod", */new NamedMethodCallInfo("ParameterlessMethod"));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj);

            Assert.IsTrue(obj.ParameterlessWasCalled);
        }

        [Test]
        public void StrategyDoesWorkBasedOnConcreteTypeInsteadOfPassedType()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            policy.Methods.Add(/*"ParameterlessMethod", */new NamedMethodCallInfo("ParameterlessMethod"));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), obj);

            Assert.IsTrue(obj.ParameterlessWasCalled);
        }

        [Test]
        public void StrategyCallsMethodWithDirectValues()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            policy.Methods.Add(/*"IntMethod", */new NamedMethodCallInfo("IntMethod", new ValueParameter<int>(32)));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj);

            Assert.AreEqual(32, obj.IntValue);
        }

        [Test]
        public void StrategyCallsMethodsUsingIParameterValues()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            policy.Methods.Add(/*"IntMethod", */new NamedMethodCallInfo("IntMethod", new ValueParameter<int>(32)));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj);

            Assert.AreEqual(32, obj.IntValue);
        }

        [Test]
        public void CanCallMultiParameterMethods()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            policy.Methods.Add(/*"MultiParamMethod", */new NamedMethodCallInfo("MultiParamMethod", new ValueParameter<double>(1.0), new ValueParameter<string>("foo")));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj);

            Assert.AreEqual(1.0, obj.MultiDouble);
            Assert.AreEqual("foo", obj.MultiString);
        }

        [Test]
        public void StrategyCallsMultipleMethodsAndCallsThemInOrder()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            policy.Methods.Add(/*"ParameterlessMethod", */new NamedMethodCallInfo("ParameterlessMethod"));
            policy.Methods.Add(/*"IntMethod", */new NamedMethodCallInfo("IntMethod", new ValueParameter<int>(32)));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj);

            Assert.AreEqual(1, obj.CallOrderParameterless);
            Assert.AreEqual(2, obj.CallOrderInt);
        }

        #endregion Success Cases

        #region Failure Cases

        [Test]
        public void StrategyWithNoObjectDoesntThrow()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            ctx.InnerChain.Add(strategy);

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<object>(null), null);
        }

        [Test]
        public void StrategyDoesNothingWithNoPolicy()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj);

            Assert.IsFalse(obj.ParameterlessWasCalled);
        }

        [Test]
        public void SettingPolicyForMissingMethodDoesntThrow()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            policy.Methods.Add(/*"NonExistantMethod", */new NamedMethodCallInfo("NonExistantMethod"));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj);
        }

        [Test]
        public void SettingPolicyForWrongParameterCountDoesntThrow()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            policy.Methods.Add(/*"ParameterlessMethod",*/ new NamedMethodCallInfo("ParameterlessMethod", new ValueParameter<int>(123)));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj);
        }

        [Test]
        public void IncompatibleTypesThrows()
        {
            MethodCallStrategy strategy = new MethodCallStrategy();
            MockBuilderContext ctx = new MockBuilderContext();
            MockObject obj = new MockObject();
            ctx.InnerChain.Add(strategy);

            MethodCallPolicy policy = new MethodCallPolicy();
            //MethodInfo mi = typeof(MockObject).GetMethod("IntMethod");
            policy.Methods.Add(/*"IntMethod", */new NamedMethodCallInfo("IntMethod", new ValueParameter<string>(String.Empty)));
            ctx.Policies.Set<IMethodCallPolicy>(policy, new BuildKey<MockObject>(null));

            Assert.Throws<IncompatibleTypesException>(() => ctx.HeadOfChain.BuildUp(ctx, new BuildKey<MockObject>(null), obj));
        }

        #endregion Failure Cases

        // ---------------------------------------------------------------------
        // TODO: Test List
        // ---------------------------------------------------------------------
        // Call method with non-void return values, and do something with the value
        // Testing with ref & out parameters
        // Statics

        #region Support Classes

        public interface IFoo
        {
        }

        public interface IBar
        {
        }

        public class FooBar : IFoo, IBar
        {
        }

        public class MockObject
        {
            private int currentOrder = 0;

            public bool ParameterlessWasCalled = false;
            public bool AmbiguousWasCalled = false;
            public int IntValue = 0;
            public int CallOrderParameterless = 0;
            public int CallOrderInt = 0;
            public double MultiDouble = 0.0;
            public string MultiString = null;

            public void ParameterlessMethod()
            {
                CallOrderParameterless = ++currentOrder;
                ParameterlessWasCalled = true;
            }

            public void IntMethod(int intValue)
            {
                CallOrderInt = ++currentOrder;
                IntValue = intValue;
            }

            public void AmbiguousMethod(IFoo foo)
            {
                AmbiguousWasCalled = true;
            }

            public void AmbiguousMethod(IBar bar)
            {
                AmbiguousWasCalled = true;
            }

            public void MultiParamMethod(double d, string s)
            {
                MultiDouble = d;
                MultiString = s;
            }
        }

        #endregion Support Classes
    }
}