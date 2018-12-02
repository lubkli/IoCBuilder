using NUnit.Framework;
using IoCBuilder.Policies;
using IoCBuilder.Strategies.Creation;

namespace IoCBuilder
{
    [TestFixture]
    public class BuilderContextFixture
    {
        [Test]
        public void TestSettingAndRetrievePolicy()
        {
            IPolicyList policies = new PolicyList();
            MockCreationPolicy policy = new MockCreationPolicy();

            policies.Set<IBuilderPolicy>(policy, new BuildKey<object>(null));
            BuildKey buildKey = new BuildKey<object>(null);
            BuilderContext context = new BuilderContext(null, null, policies, buildKey);

            IBuilderPolicy outPolicy = context.Policies.Get<IBuilderPolicy>(buildKey);

            Assert.IsNotNull(outPolicy);
            Assert.AreSame(policy, outPolicy);
        }

        [Test]
        public void TestNoPoliciesReturnsNull()
        {
            IPolicyList policies = new PolicyList();
            BuildKey buildKey = new BuildKey<object>(null);
            BuilderContext context = new BuilderContext(null, null, policies, buildKey);

            Assert.IsNull(context.Policies.Get<IBuilderPolicy>(buildKey));
        }

        [Test]
        public void PassingNullPoliciesObjectDoesntThrowWhenAskingForPolicy()
        {
            BuildKey buildKey = new BuildKey<object>(null);
            BuilderContext context = new BuilderContext(null, null, null, buildKey);

            Assert.IsNull(context.Policies.Get<IBuilderPolicy>(buildKey));
        }

        [Test]
        public void CanSetPoliciesUsingTheContext()
        {
            BuildKey buildKey = new BuildKey<object>("foo");
            BuilderContext context = new BuilderContext(null, null, null, buildKey);
            MockCreationPolicy policy = new MockCreationPolicy();

            context.Policies.Set<IBuilderPolicy>(policy, buildKey);

            Assert.AreSame(policy, context.Policies.Get<IBuilderPolicy>(new BuildKey<object>("foo")));
        }

        [Test]
        public void SettingPolicyViaContextDoesNotAffectPoliciesPassedToContextConstructor()
        {
            IPolicyList policies = new PolicyList();
            MockCreationPolicy policy1 = new MockCreationPolicy();

            policies.Set<IBuilderPolicy>(policy1, new BuildKey<object>(null));
            BuildKey buildKey = new BuildKey<string>(null);
            BuilderContext context = new BuilderContext(null, null, policies, buildKey);

            MockCreationPolicy policy2 = new MockCreationPolicy();
            context.Policies.Set<IBuilderPolicy>(policy2, buildKey);

            Assert.AreEqual(1, policies.Count);
        }

        private class MockCreationPolicy : ConstructorCreationPolicy
        {
            public MockCreationPolicy()
                : base(null, null)
            {
            }
        }
    }
}