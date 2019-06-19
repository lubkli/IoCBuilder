using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using IoCBuilder.Strategies.TypeMapping;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace IoCBuilder.Interception
{
    [TestClass]
    public class InterceptionFactoryFixture
    {
        [TestMethod]
        public void InterfaceInterceptionTest()
        {
            Factory factory = new Factory();
            factory.RegisterStrategy<InterceptionReflectionStrategy>(BuilderStage.Reflection);

            factory.RegisterStrategy<InterfaceInterceptionStrategy>(BuilderStage.PreCreation);
            factory.RegisterStrategy<VirtualInterceptionStrategy>(BuilderStage.PreCreation);

            factory.RegisterStrategy<RemotingInterceptionStrategy>(BuilderStage.PostInitialization);

            TypeMappingPolicy policy = new TypeMappingPolicy(typeof(InterfaceClass), string.Empty);
            factory.SetPolicy<ITypeMappingPolicy>(policy, typeof(INotify));

            Recorder.Records.Clear();
            INotify n = factory.Get<INotify>();
            n.BoolProperty = true;
            n.Notify("#");

            Assert.AreEqual(6, Recorder.Records.Count());
        }

        [TestMethod]
        public void VirtualInterceptionTest()
        {
            Factory factory = new Factory();
            factory.RegisterStrategy<InterceptionReflectionStrategy>(BuilderStage.Reflection);

            factory.RegisterStrategy<InterfaceInterceptionStrategy>(BuilderStage.PreCreation);
            factory.RegisterStrategy<VirtualInterceptionStrategy>(BuilderStage.PreCreation);

            factory.RegisterStrategy<RemotingInterceptionStrategy>(BuilderStage.PostInitialization);

            Recorder.Records.Clear();

            VirtualClass n = factory.Get<VirtualClass>();
            n.BoolProperty = true;
            n.Notify("#");

            Assert.AreEqual(6, Recorder.Records.Count());
        }

        [TestMethod]
        public void VirtualInterceptionWithInjectionTest()
        {
            Factory factory = new Factory();

            factory.RegisterStrategy<InterceptionReflectionStrategy>(BuilderStage.Reflection);

            factory.RegisterStrategy<InterfaceInterceptionStrategy>(BuilderStage.Interception);
            factory.RegisterStrategy<VirtualInterceptionStrategy>(BuilderStage.Interception);

            factory.RegisterStrategy<RemotingInterceptionStrategy>(BuilderStage.PostInitialization);

            Recorder.Records.Clear();

            DependencyClass n = factory.Get<DependencyClass>();
            n.Dependet.Notify("#");
            n.Notify("!");

            Assert.AreEqual(6, Recorder.Records.Count());
        }

        public interface INotify
        {
            bool BoolProperty { get; set; }

            void Notify(string name);
        }

        public class InterfaceClass : INotify
        {
            private bool _boolProperty;

            public bool BoolProperty
            {
                get
                {
                    return _boolProperty;
                }
                [InterfaceIntercept(typeof(MyRecordingHandler))]
                set
                {
                    Recorder.Records.Add("Inside Property Setter");
                    _boolProperty = value;
                }
            }

            [InterfaceIntercept(typeof(MyRecordingHandler))]
            public void Notify(string name)
            {
                Recorder.Records.Add("Inside Method");
            }
        }

        public class BaseVirtualClass
        {
            private bool _boolProperty;

            public virtual bool BoolProperty
            {
                get
                {
                    return _boolProperty;
                }
                [VirtualIntercept(typeof(MyRecordingHandler))]
                set
                {
                    Recorder.Records.Add("Inside Property Setter");
                    _boolProperty = value;
                }
            }

            [VirtualIntercept(typeof(MyRecordingHandler))]
            public virtual void Notify(string name)
            {
                Recorder.Records.Add("Inside Method");
            }
        }

        public class VirtualClass : BaseVirtualClass
        {
        }

        public class DependentClass
        {
            [VirtualIntercept(typeof(MyRecordingHandler))]
            public virtual void Notify(string name)
            {
                Recorder.Records.Add("Inside Method");
            }
        }

        public class DependencyClass
        {
            public DependencyClass([Dependency]DependentClass Dependet)
            {
            }

            [Dependency]
            public DependentClass Dependet
            {
                get;
                set;
            }

            [VirtualIntercept(typeof(MyRecordingHandler))]
            public virtual void Notify(string name)
            {
                Recorder.Records.Add("Inside Method");
            }
        }

        public class MyRecordingHandler : IInterceptionHandler
        {
            public MyRecordingHandler()
            {
            }

            public IMethodReturn Invoke(IMethodInvocation call, GetNextHandlerDelegate getNext)
            {
                Recorder.Records.Add("Before Method");
                IMethodReturn result = getNext().Invoke(call, getNext);
                Recorder.Records.Add("After Method");
                return result;
            }
        }
    }
}