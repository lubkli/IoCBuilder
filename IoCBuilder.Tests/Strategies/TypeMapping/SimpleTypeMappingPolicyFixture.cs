using NUnit.Framework;
using IoCBuilder.Strategies.TypeMapping;

namespace IoCBuilder
{
    [TestFixture]
    public class SimpleTypeMappingPolicyFixture
    {
        [Test]
        public void PolicyReturnsGivenType()
        {
            TypeMappingPolicy policy = new TypeMappingPolicy(typeof(Foo), null);

            Assert.AreEqual(new BuildKey<Foo>(null), policy.Map(new BuildKey(null)));
        }

        private class Foo
        {
        }
    }
}