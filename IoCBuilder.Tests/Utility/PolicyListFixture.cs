using NUnit.Framework;
using IoCBuilder.Policies;

namespace IoCBuilder
{
    [TestFixture]
    public class PolicyListFixture
    {
        [Test]
        public void CanAddPolicyToBagAndRetrieveIt()
        {
            IPolicyList list = new PolicyList();

            list.Set<IBuilderPolicy>(new MockPolicy(), new BuildKey<object>(null));

            Assert.IsNotNull(list.Get<IBuilderPolicy>(new BuildKey<object>(null)));
        }

        [Test]
        public void CanAddMultiplePoliciesToBagAndRetrieveThem()
        {
            IPolicyList list = new PolicyList();
            MockPolicy policy1 = new MockPolicy();
            MockPolicy policy2 = new MockPolicy();

            list.Set<IBuilderPolicy>(policy1, new BuildKey<object>("1"));
            list.Set<IBuilderPolicy>(policy2, new BuildKey<string>("2"));

            Assert.AreSame(policy1, list.Get<IBuilderPolicy>(new BuildKey<object>("1")));
            Assert.AreSame(policy2, list.Get<IBuilderPolicy>(new BuildKey<string>("2")));
        }

        [Test]
        public void SetOverwritesExistingPolicy()
        {
            IPolicyList list = new PolicyList();
            MockPolicy policy1 = new MockPolicy();
            MockPolicy policy2 = new MockPolicy();

            list.Set<IBuilderPolicy>(policy1, new BuildKey<string>("1"));
            list.Set<IBuilderPolicy>(policy2, new BuildKey<string>("1"));

            Assert.AreSame(policy2, list.Get<IBuilderPolicy>(new BuildKey<string>("1")));
        }

        [Test]
        public void CanClearPolicy()
        {
            IPolicyList list = new PolicyList();
            MockPolicy policy = new MockPolicy();

            list.Set<IBuilderPolicy>(policy, new BuildKey<string>("1"));
            list.Clear<IBuilderPolicy>(new BuildKey<string>("1"));

            Assert.IsNull(list.Get<IBuilderPolicy>(new BuildKey<string>("1")));
        }

        [Test]
        public void CanClearAllPolicies()
        {
            IPolicyList list = new PolicyList();
            list.Set<IBuilderPolicy>(new MockPolicy(), new BuildKey<object>(null));
            list.Set<IBuilderPolicy>(new MockPolicy(), new BuildKey<object>("1"));

            list.ClearAll();

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void DefaultPolicyUsedWhenSpecificPolicyIsntAvailable()
        {
            IPolicyList list = new PolicyList();
            MockPolicy defaultPolicy = new MockPolicy();

            list.SetDefault<IBuilderPolicy>(defaultPolicy);

            Assert.AreSame(defaultPolicy, list.Get<IBuilderPolicy>(new BuildKey<object>(null)));
        }

        [Test]
        public void SpecificPolicyOverridesDefaultPolicy()
        {
            IPolicyList list = new PolicyList();
            MockPolicy defaultPolicy = new MockPolicy();
            MockPolicy specificPolicy = new MockPolicy();

            list.Set<IBuilderPolicy>(specificPolicy, new BuildKey<object>(null));
            list.SetDefault<IBuilderPolicy>(defaultPolicy);

            Assert.AreSame(specificPolicy, list.Get<IBuilderPolicy>(new BuildKey<object>(null)));
        }

        [Test]
        public void CanClearDefaultPolicy()
        {
            IPolicyList list = new PolicyList();
            MockPolicy defaultPolicy = new MockPolicy();
            list.SetDefault<IBuilderPolicy>(defaultPolicy);

            list.ClearDefault<IBuilderPolicy>();

            Assert.IsNull(list.Get<IBuilderPolicy>(new BuildKey<object>(null)));
        }

        private class MockPolicy : IBuilderPolicy
        {
        }
    }
}