using NUnit.Framework;
using System;
using System.Reflection;
using TestMethod = NUnit.Framework.TestAttribute;

namespace IoCBuilder.Interception
{
    public class MethodInvocationTest
    {
        [TestMethod]
        public void ShouldBeAbleToChangeInputs()
        {
            MethodBase methodBase = typeof(InvocationTarget).GetMethod("FirstTarget");
            InvocationTarget target = new InvocationTarget();
            IMethodInvocation invocation = new MethodInvocation(target, methodBase, new object[] { 1, "two" });

            Assert.AreEqual(1, invocation.Inputs["one"]);
            invocation.Inputs["one"] = 42;
            Assert.AreEqual(42, invocation.Inputs["one"]);
        }

        internal class InvocationTarget : MarshalByRefObject
        {
            public string FirstTarget(int one,
                                      string two)
            {
                return "Boo!";
            }
        }
    }
}