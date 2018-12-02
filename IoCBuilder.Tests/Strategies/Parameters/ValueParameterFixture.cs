using NUnit.Framework;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder
{
    [TestFixture]
    public class ValueParameterFixture
    {
        [Test]
        public void ValueParameterReturnsStoredTypeAndValue()
        {
            ValueParameter<int> x = new ValueParameter<int>(12);

            Assert.AreEqual(typeof(int), x.GetParameterType(null));
            Assert.AreEqual(12, x.GetValue(null));
        }
    }
}