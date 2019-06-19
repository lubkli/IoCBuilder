using NUnit.Framework;
using System;
using System.Reflection;
using IoCBuilder.Policies;
using TestMethod = NUnit.Framework.TestAttribute;

namespace IoCBuilder.Interception
{
    public class InterceptionReflectorTest
    {
        public class MixedInterception
        {
            [TestMethod]
            public void CanMixInterceptionTypes()
            {
                PolicyList policies = new PolicyList();

                InterceptionReflector.Reflect<IFoo, FooBar>(policies, new StubObjectFactory());

                BuildKey buildKey = new BuildKey(typeof(FooBar));

                Assert.AreEqual(1, policies.Get<IVirtualInterceptionPolicy>(buildKey).Count);
                Assert.AreEqual(1, policies.Get<IInterfaceInterceptionPolicy>(buildKey).Count);
                Assert.AreEqual(1, policies.Get<IRemotingInterceptionPolicy>(buildKey).Count);
            }

            public class FooBar : MarshalByRefObject, IFoo
            {
                [RemotingIntercept(typeof(RecordingHandler))]
                [VirtualIntercept(typeof(RecordingHandler))]
                [InterfaceIntercept(typeof(RecordingHandler))]
                public virtual void Foo() { }
            }

            public interface IFoo
            {
                void Foo();
            }
        }

        public class NoInterceptionType
        {
            [TestMethod]
            public void ClassWithNoDecorations()
            {
                PolicyList policies = new PolicyList();

                InterceptionReflector.Reflect<object>(policies, new StubObjectFactory());

                Assert.AreEqual(0, policies.Count);
            }
        }

        public class ViaInterface
        {
            [TestMethod]
            public void CanAddHandlersInInheritedClass()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(IOneMethod).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<IOneMethod, DerivedWithAddedIntercepts>(policies, new StubObjectFactory());
                IInterfaceInterceptionPolicy policy = policies.Get<IInterfaceInterceptionPolicy>(new BuildKey(typeof(DerivedWithAddedIntercepts)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(2, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][1]);
            }

            [TestMethod]
            public void InterceptionIsInherited()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(IOneMethod).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<IOneMethod, DerivedOneMethod>(policies, new StubObjectFactory());
                IInterfaceInterceptionPolicy policy = policies.Get<IInterfaceInterceptionPolicy>(new BuildKey(typeof(DerivedOneMethod)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void NonPublicInterfaceNotCompatible()
            {
                PolicyList policies = new PolicyList();

                Assert.Throws<InvalidOperationException>(
                    delegate
                    {
                        InterceptionReflector.Reflect<INonPublicInterface, NonPublicInterface>(policies, new StubObjectFactory());
                    });
            }

            [TestMethod]
            public void OneInterceptedMethod()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(IOneMethod).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<IOneMethod, OneMethod>(policies, new StubObjectFactory());
                IInterfaceInterceptionPolicy policy = policies.Get<IInterfaceInterceptionPolicy>(new BuildKey(typeof(OneMethod)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void ReflectShouldHappenOnGenericBaseClass()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(IGeneric<>).GetMethod("DoSomething");

                InterceptionReflector.Reflect<IGeneric<int>, Generic<int>>(policies, new StubObjectFactory());
                IInterfaceInterceptionPolicy policy = policies.Get<IInterfaceInterceptionPolicy>(new BuildKey(typeof(Generic<>)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void RequestingToBuildInterface1WillNotInterceptedInterface2Methods()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(IInterface1).GetMethod("InterceptedMethod1");

                InterceptionReflector.Reflect<IInterface1, TwoInterfaceClass>(policies, new StubObjectFactory());
                IInterfaceInterceptionPolicy policy = policies.Get<IInterfaceInterceptionPolicy>(new BuildKey(typeof(TwoInterfaceClass)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void RequestingToBuildNonInterfaceMeansMethodsNotIntercepted()
            {
                PolicyList policies = new PolicyList();

                InterceptionReflector.Reflect<OneMethod>(policies, new StubObjectFactory());

                Assert.AreEqual(0, policies.Count);
            }

            [TestMethod]
            public void TwoAttributesOnOneMethod()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(IOneMethod).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<IOneMethod, OneMethodTwoAttributes>(policies, new StubObjectFactory());
                IInterfaceInterceptionPolicy policy = policies.Get<IInterfaceInterceptionPolicy>(new BuildKey(typeof(OneMethodTwoAttributes)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(2, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][1]);
            }

            [TestMethod]
            public void TwoInterceptedMethods()
            {
                PolicyList policies = new PolicyList();
                MethodBase method1 = typeof(ITwoMethods).GetMethod("InterceptedMethod1");
                MethodBase method2 = typeof(ITwoMethods).GetMethod("InterceptedMethod2");

                InterceptionReflector.Reflect<ITwoMethods, TwoMethods>(policies, new StubObjectFactory());
                IInterfaceInterceptionPolicy policy = policies.Get<IInterfaceInterceptionPolicy>(new BuildKey(typeof(TwoMethods)));

                Assert.AreEqual(2, policy.Count);
                Assert.AreEqual(1, policy[method1].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method1][0]);
                Assert.AreEqual(1, policy[method2].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method2][0]);
            }

            public interface IGeneric<T>
            {
                void DoSomething(T data);
            }

            public interface IInterface1
            {
                void InterceptedMethod1();
            }

            public interface IInterface2
            {
                void InterceptedMethod2();
            }

            public interface IOneMethod
            {
                void InterceptedMethod();
            }

            public interface ITwoMethods
            {
                void InterceptedMethod1();

                void InterceptedMethod2();
            }

            internal class DerivedOneMethod : OneMethod { }

            internal class DerivedWithAddedIntercepts : OneMethod
            {
                [InterfaceIntercept(typeof(RecordingHandler))]
                public override void InterceptedMethod() { }
            }

            internal class Generic<T> : IGeneric<T>
            {
                [InterfaceIntercept(typeof(RecordingHandler))]
                public void DoSomething(T data) { }
            }

            internal interface INonPublicInterface
            {
                void InterceptedMethod();
            }

            internal class NonPublicInterface : INonPublicInterface
            {
                [InterfaceIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod() { }
            }

            internal class OneMethod : IOneMethod
            {
                [InterfaceIntercept(typeof(RecordingHandler))]
                public virtual void InterceptedMethod() { }

                public void NonInterceptedMethod()
                {
                }
            }

            internal class OneMethodTwoAttributes : IOneMethod
            {
                [InterfaceIntercept(typeof(RecordingHandler))]
                [InterfaceIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod() { }
            }

            internal class TwoInterfaceClass : IInterface1, IInterface2
            {
                [InterfaceIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod1() { }

                [InterfaceIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod2() { }
            }

            internal class TwoMethods : ITwoMethods
            {
                [InterfaceIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod1() { }

                [InterfaceIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod2() { }
            }
        }

        public class ViaRemoting
        {
            [TestMethod]
            public void CanAddHandlersInInheritedClass()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(DerivedWithAddedIntercepts).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<DerivedWithAddedIntercepts>(policies, new StubObjectFactory());
                IRemotingInterceptionPolicy policy = policies.Get<IRemotingInterceptionPolicy>(new BuildKey(typeof(DerivedWithAddedIntercepts)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(2, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][1]);
            }

            [TestMethod]
            public void InterceptionIsInherited()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(DerivedOneMethod).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<DerivedOneMethod>(policies, new StubObjectFactory());
                IRemotingInterceptionPolicy policy = policies.Get<IRemotingInterceptionPolicy>(new BuildKey(typeof(DerivedOneMethod)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void NonMBROTypeIncompatibleWithRemoting()
            {
                PolicyList policies = new PolicyList();

                Assert.Throws<InvalidOperationException>(
                    delegate
                    {
                        InterceptionReflector.Reflect<NonMBRO>(policies, new StubObjectFactory());
                    });
            }

            [TestMethod]
            public void OneInterceptedMethod()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(OneMethod).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<OneMethod>(policies, new StubObjectFactory());
                IRemotingInterceptionPolicy policy = policies.Get<IRemotingInterceptionPolicy>(new BuildKey(typeof(OneMethod)));

                Assert.NotNull(policy);
                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void TwoAttributesOnOneMethod()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(OneMethodTwoAttributes).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<OneMethodTwoAttributes>(policies, new StubObjectFactory());
                IRemotingInterceptionPolicy policy = policies.Get<IRemotingInterceptionPolicy>(new BuildKey(typeof(OneMethodTwoAttributes)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(2, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][1]);
            }

            [TestMethod]
            public void TwoInterceptedMethods()
            {
                PolicyList policies = new PolicyList();
                MethodBase method1 = typeof(TwoMethods).GetMethod("InterceptedMethod1");
                MethodBase method2 = typeof(TwoMethods).GetMethod("InterceptedMethod2");

                InterceptionReflector.Reflect<TwoMethods>(policies, new StubObjectFactory());
                IRemotingInterceptionPolicy policy = policies.Get<IRemotingInterceptionPolicy>(new BuildKey(typeof(TwoMethods)));

                Assert.AreEqual(2, policy.Count);
                Assert.AreEqual(1, policy[method1].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method1][0]);
                Assert.AreEqual(1, policy[method2].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method2][0]);
            }

            internal class DerivedOneMethod : OneMethod { }

            internal class DerivedWithAddedIntercepts : OneMethod
            {
                [RemotingIntercept(typeof(RecordingHandler))]
                public override void InterceptedMethod() { }
            }

            internal class NonMBRO
            {
                [RemotingIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod() { }
            }

            internal class OneMethod : MarshalByRefObject
            {
                [RemotingIntercept(typeof(RecordingHandler))]
                public virtual void InterceptedMethod() { }

                public void NonInterceptedMethod()
                {
                }
            }

            internal class OneMethodTwoAttributes : MarshalByRefObject
            {
                [RemotingIntercept(typeof(RecordingHandler))]
                [RemotingIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod() { }
            }

            internal class TwoMethods : MarshalByRefObject
            {
                [RemotingIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod1() { }

                [RemotingIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod2() { }
            }
        }

        public class ViaVirtualMethod
        {
            [TestMethod]
            public void CanAddHandlersInInheritedClass()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(DerivedWithAddedIntercepts).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<DerivedWithAddedIntercepts>(policies, new StubObjectFactory());
                IVirtualInterceptionPolicy policy = policies.Get<IVirtualInterceptionPolicy>(new BuildKey(typeof(DerivedWithAddedIntercepts)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(2, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][1]);
            }

            [TestMethod]
            public void InterceptionIsInherited()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(DerivedOneMethod).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<DerivedOneMethod>(policies, new StubObjectFactory());
                IVirtualInterceptionPolicy policy = policies.Get<IVirtualInterceptionPolicy>(new BuildKey(typeof(DerivedOneMethod)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void NonPublicTypesNotCompatible()
            {
                PolicyList policies = new PolicyList();

                Assert.Throws<InvalidOperationException>(
                    delegate
                    {
                        InterceptionReflector.Reflect<InternalClass>(policies, new StubObjectFactory());
                    });
            }

            [TestMethod]
            public void NonVirtualMethodNotCompatible()
            {
                PolicyList policies = new PolicyList();

                Assert.Throws<InvalidOperationException>(
                    delegate
                    {
                        InterceptionReflector.Reflect<NonVirtualMethod>(policies, new StubObjectFactory());
                    });
            }

            [TestMethod]
            public void OneInterceptedMethod()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(OneMethod).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<OneMethod>(policies, new StubObjectFactory());
                IVirtualInterceptionPolicy policy = policies.Get<IVirtualInterceptionPolicy>(new BuildKey(typeof(OneMethod)));

                Assert.NotNull(policy);
                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void ReflectShouldHappenOnGenericBaseClass()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(Generic<>).GetMethod("DoSomething");

                InterceptionReflector.Reflect<Generic<int>>(policies, new StubObjectFactory());
                IVirtualInterceptionPolicy policy = policies.Get<IVirtualInterceptionPolicy>(new BuildKey(typeof(Generic<>)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(1, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
            }

            [TestMethod]
            public void SealedTypeNotCompatible()
            {
                PolicyList policies = new PolicyList();

                Assert.Throws<InvalidOperationException>(
                    delegate
                    {
                        InterceptionReflector.Reflect<SealedClass>(policies, new StubObjectFactory());
                    });
            }

            [TestMethod]
            public void TwoAttributesOnOneMethod()
            {
                PolicyList policies = new PolicyList();
                MethodBase method = typeof(OneMethodTwoAttributes).GetMethod("InterceptedMethod");

                InterceptionReflector.Reflect<OneMethodTwoAttributes>(policies, new StubObjectFactory());
                IVirtualInterceptionPolicy policy = policies.Get<IVirtualInterceptionPolicy>(new BuildKey(typeof(OneMethodTwoAttributes)));

                Assert.AreEqual(1, policy.Count);
                Assert.AreEqual(2, policy[method].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][0]);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method][1]);
            }

            [TestMethod]
            public void TwoInterceptedMethods()
            {
                PolicyList policies = new PolicyList();
                MethodBase method1 = typeof(TwoMethods).GetMethod("InterceptedMethod1");
                MethodBase method2 = typeof(TwoMethods).GetMethod("InterceptedMethod2");

                InterceptionReflector.Reflect<TwoMethods>(policies, new StubObjectFactory());
                IVirtualInterceptionPolicy policy = policies.Get<IVirtualInterceptionPolicy>(new BuildKey(typeof(TwoMethods)));

                Assert.AreEqual(2, policy.Count);
                Assert.AreEqual(1, policy[method1].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method1][0]);
                Assert.AreEqual(1, policy[method2].Count);
                Assert.IsAssignableFrom<RecordingHandler>(policy[method2][0]);
            }

            [TestMethod]
            public void VirtualSealedMethodNotCompatible()
            {
                PolicyList policies = new PolicyList();

                Assert.Throws<InvalidOperationException>(
                    delegate
                    {
                        InterceptionReflector.Reflect<VirtualSealed>(policies, new StubObjectFactory());
                    });
            }

            public class DerivedOneMethod : OneMethod { }

            public class DerivedWithAddedIntercepts : OneMethod
            {
                [VirtualIntercept(typeof(RecordingHandler))]
                public override void InterceptedMethod() { }
            }

            public class Generic<T>
            {
                [VirtualIntercept(typeof(RecordingHandler))]
                public virtual void DoSomething(T data) { }
            }

            public class NonVirtualMethod
            {
                [VirtualIntercept(typeof(RecordingHandler))]
                public void InterceptedMethod() { }
            }

            public class OneMethod
            {
                [VirtualIntercept(typeof(RecordingHandler))]
                public virtual void InterceptedMethod() { }

                public void NonInterceptedMethod()
                {
                }
            }

            public class OneMethodTwoAttributes
            {
                [VirtualIntercept(typeof(RecordingHandler))]
                [VirtualIntercept(typeof(RecordingHandler))]
                public virtual void InterceptedMethod() { }
            }

            public sealed class SealedClass : SealedClassBase { }

            public class SealedClassBase
            {
                [VirtualIntercept(typeof(RecordingHandler))]
                public virtual void InterceptedMethod() { }
            }

            public class TwoMethods
            {
                [VirtualIntercept(typeof(RecordingHandler))]
                public virtual void InterceptedMethod1() { }

                [VirtualIntercept(typeof(RecordingHandler))]
                public virtual void InterceptedMethod2() { }
            }

            public class VirtualSealed : OneMethod
            {
                public override sealed void InterceptedMethod()
                {
                }
            }

            internal class InternalClass
            {
                [VirtualIntercept(typeof(RecordingHandler))]
                public virtual void InterceptedMethod() { }
            }
        }
    }
}