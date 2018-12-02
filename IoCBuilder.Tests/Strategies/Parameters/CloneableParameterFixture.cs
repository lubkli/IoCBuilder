using NUnit.Framework;
using System;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder
{
    [TestFixture]
    public class CloneableParameterFixture
    {
        // Suggestion: make this do a serialization deep copy in the absence of ICloneable
        // Or:         create a separate parameter type for serialization-based deep copy

        [Test]
        public void CloneParametersReturnsNewObjectOfCorrectType()
        {
            MockBuilderContext ctx = new MockBuilderContext();
            ctx.InnerLocator.Add("foo", new CloneableObject());

            CloneParameter cloneParam = new CloneParameter(new LookupParameter("foo"));

            object result1 = cloneParam.GetValue(ctx);
            object result2 = cloneParam.GetValue(ctx);

            Assert.IsTrue(result1 is CloneableObject);
            Assert.IsTrue(result2 is CloneableObject);
            Assert.IsFalse(result1 == result2);
        }

        [Test]
        public void CloneParametersCopiesValues()
        {
            CloneableObject obj = new CloneableObject();
            obj.Value = new object();

            CloneParameter cloneParam = new CloneParameter(new ValueParameter<CloneableObject>(obj));
            CloneableObject result = (CloneableObject)cloneParam.GetValue(null);

            Assert.AreSame(obj.Value, result.Value);
            Assert.AreSame(typeof(CloneableObject), cloneParam.GetParameterType(null));
        }
    }

    internal class CloneableObject : ICloneable
    {
        public object Value;

        public object Clone()
        {
            CloneableObject result = new CloneableObject();
            result.Value = Value;
            return result;
        }
    }
}