using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder
{
    [TestFixture]
    public class ConstructorCreationPolicyTest
    {
        [Test]
        public void CreatesObjectAndPassesValue()
        {
            MockBuilderContext context = new MockBuilderContext();
            ConstructorInfo constructor = typeof(Dummy).GetConstructor(new Type[] { typeof(int) });
            ConstructorCreationPolicy policy = new ConstructorCreationPolicy(constructor, Params(42));

            Dummy result = (Dummy)policy.Create(context, typeof(Dummy));

            Assert.NotNull(result);
            Assert.AreEqual(42, result.val);
        }

        [Test]
        public void NonMatchingParameterCount()
        {
            MockBuilderContext context = new MockBuilderContext();
            ConstructorInfo constructor = typeof(Dummy).GetConstructor(new Type[] { typeof(int) });
            ConstructorCreationPolicy policy = new ConstructorCreationPolicy(constructor);

            Assert.Throws<TargetParameterCountException>(
                delegate
                {
                    policy.Create(context, typeof(Dummy));
                });
        }

        [Test]
        public void NonMatchingParameterTypes()
        {
            MockBuilderContext context = new MockBuilderContext();
            ConstructorInfo constructor = typeof(Dummy).GetConstructor(new Type[] { typeof(int) });
            ConstructorCreationPolicy policy = new ConstructorCreationPolicy(constructor, Params("foo"));

            Assert.Throws<ArgumentException>(
                delegate
                {
                    policy.Create(context, typeof(Dummy));
                });
        }

        // Allowed, test removed
        //public void NullConstructor()
        //{
        //    Assert.Throws<ArgumentNullException>(
        //        delegate
        //        {
        //            new ConstructorCreationPolicy(null);
        //        });
        //}

        [Test]
        public void NullContext()
        {
            ConstructorInfo constructor = typeof(Dummy).GetConstructor(new Type[] { typeof(int) });
            ConstructorCreationPolicy policy = new ConstructorCreationPolicy(constructor);

            Assert.Throws<ArgumentNullException>(
                delegate
                {
                    policy.Create(null, typeof(Dummy));
                });
        }

        private static IEnumerable<IParameter> Params(params object[] parameters)
        {
            foreach (object parameter in parameters)
                yield return new ValueParameter(parameter.GetType(), parameter);
        }

        internal class Dummy
        {
            public readonly int val;

            public Dummy(int val)
            {
                this.val = val;
            }
        }
    }

    //public class ConstructorPolicyFixture
    //{
    //    [Test]
    //    public void GetConstructorReturnsTheCorrectOneWhenParamsPassedThruAddParameter()
    //    {
    //        ConstructorCreationPolicy policy = new ConstructorCreationPolicy();

    //        policy.AddParameter(new ValueParameter<int>(5));
    //        ConstructorInfo actual = policy.SelectConstructor(new MockBuilderContext(), typeof(MockObject), null);
    //        ConstructorInfo expected = typeof(MockObject).GetConstructors()[1];

    //        Assert.AreSame(expected, actual);
    //    }

    //    [Test]
    //    public void GetConstructorReturnsTheCorrectOneWhenParamsPassedThruCtor()
    //    {
    //        ConstructorCreationPolicy policy = new ConstructorCreationPolicy(new ValueParameter<int>(5));

    //        ConstructorInfo actual = policy.SelectConstructor(new MockBuilderContext(), typeof(MockObject), null);
    //        ConstructorInfo expected = typeof(MockObject).GetConstructors()[1];

    //        Assert.AreSame(expected, actual);
    //    }

    //    private class MockObject
    //    {
    //        public MockObject()
    //        {
    //        }

    //        public MockObject(int val)
    //        {
    //        }
    //    }
    //}
}