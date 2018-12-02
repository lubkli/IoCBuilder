using NUnit.Framework;
using System;
using IoCBuilder.Exceptions;

namespace IoCBuilder
{
    [TestFixture]
    public class FactoryFixture
    {
        public class ExistingObjects
        {
            [Test]
            public void CanInjectExistingObject()
            {
                ExistingObject obj = new ExistingObject();
                Factory container = new Factory();
                container.RegisterSingletonInstance("Hello world");

                container.Inject(obj);

                Assert.AreEqual("Hello world", obj.Name);
            }

            [Test]
            public void NullThrows()
            {
                IFactory container = new Factory();

                Assert.Throws<ArgumentNullException>(delegate
                {
                    container.Inject(null);
                });
            }

            // Helpers

            private class ExistingObject
            {
                private string name;

                [Dependency]
                public string Name
                {
                    get { return name; }
                    set { name = value; }
                }
            }
        }

        public class Singletons
        {
            [Test]
            public void CanRegisterSingletonInstance()
            {
                Factory container = new Factory();
                MyObject obj = new MyObject();
                container.RegisterSingletonInstance<IMyObject>(obj);

                IMyObject result = container.Get<IMyObject>();

                Assert.AreSame(result, obj);
            }

            [Test]
            public void CanRegisterTypesToBeConsideredCached()
            {
                Factory container = new Factory();
                container.CacheInstancesOf<MyObject>();

                MyObject result1 = container.Get<MyObject>();
                MyObject result2 = container.Get<MyObject>();

                Assert.NotNull(result1);
                Assert.NotNull(result2);
                Assert.AreSame(result1, result2);
            }

            [Test]
            public void ObjectsAreNotSingletonByDefault()
            {
                Factory container = new Factory();

                object obj1 = container.Get<object>();
                object obj2 = container.Get<object>();

                Assert.AreNotSame(obj1, obj2);
            }
        }

        public class TypeMapping
        {
            [Test]
            public void CanRegisterTypeMapping()
            {
                Factory container = new Factory();
                container.RegisterTypeMapping<IMyObject, MyObject>();

                IMyObject result = container.Get<IMyObject>();

                Assert.NotNull(result);
                Assert.IsAssignableFrom<MyObject>(result);
            }

            [Test]
            public void CanTypeMapFromGenericToGeneric()
            {
                Factory container = new Factory();
                container.RegisterTypeMapping(typeof(IFoo<int>), typeof(Foo<int>));

                IFoo<int> result = container.Get<IFoo<int>>();

                Assert.IsAssignableFrom<Foo<int>>(result);
            }

            //TODO: Learn Creation Strategy new thing :-)
            // PolicyList knows it in this version.
            // This should not throw this exception, but should create
            // mapped type. This is real hell of generics, but future
            // version should be able to do it.

            [Test]
            public void CanTypeMapFromGenericToOpenGeneric()
            {
                Factory container = new Factory();
                container.RegisterTypeMapping(typeof(IFoo<>), typeof(Foo<>));
                Assert.Throws<IncompatibleTypesException>(() =>
                {
                    IFoo<int> result = container.Get<IFoo<int>>();

                    Assert.IsAssignableFrom<Foo<int>>(result);
                });
            }

            public class Foo<T> : IFoo<T> { }

            public interface IFoo<T> { }
        }

        private interface IMyObject
        { }

        private class MyObject : IMyObject
        { }
    }
}