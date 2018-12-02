using NUnit.Framework;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder
{
    [TestFixture]
    public class LookupParameterFixture
    {
        [Test]
        public void ConstructorPolicyCanUseLookupToFindAnObject()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            ctx.InnerLocator.Add("foo", 11);

            LookupParameter param = new LookupParameter("foo");

            Assert.AreEqual(11, param.GetValue(ctx));
            Assert.AreSame(typeof(int), param.GetParameterType(ctx));
        }
    }
}