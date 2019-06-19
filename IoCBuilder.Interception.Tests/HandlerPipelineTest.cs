using NUnit.Framework;
using System;
using System.Collections.Generic;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace IoCBuilder.Interception
{
    [TestClass]
    public class HandlerPipelineTest
    {
        [TestMethod]
        public void NoHandlersCallsTarget()
        {
            bool called = false;
            StubMethodInvocation invocation = new StubMethodInvocation();
            StubMethodReturn returnValue = new StubMethodReturn();
            HandlerPipeline pipeline = new HandlerPipeline();

            IMethodReturn result = pipeline.Invoke(invocation, delegate (IMethodInvocation call,
                                                                        GetNextHandlerDelegate getNext)
                                                               {
                                                                   Assert.AreSame(call, invocation);
                                                                   Assert.Null(getNext);

                                                                   called = true;
                                                                   return returnValue;
                                                               });

            Assert.True(called);
            Assert.AreSame(returnValue, result);
        }

        [TestMethod]
        public void NullHandlerLists()
        {
            Assert.Throws<ArgumentNullException>(delegate
                                                 {
                                                     new HandlerPipeline((IEnumerable<IInterceptionHandler>)null);
                                                 });

            Assert.Throws<ArgumentNullException>(delegate
                                                 {
                                                     new HandlerPipeline((IInterceptionHandler[])null);
                                                 });
        }

        [TestMethod]
        public void OneHandler()
        {
            Recorder.Records.Clear();
            RecordingHandler handler = new RecordingHandler();
            StubMethodInvocation invocation = new StubMethodInvocation();
            HandlerPipeline pipeline = new HandlerPipeline(handler);

            pipeline.Invoke(invocation, delegate
                                        {
                                            Recorder.Records.Add("method");
                                            return null;
                                        });

            Assert.AreEqual(3, Recorder.Records.Count);
            Assert.AreEqual("Before Method", Recorder.Records[0]);
            Assert.AreEqual("method", Recorder.Records[1]);
            Assert.AreEqual("After Method", Recorder.Records[2]);
        }

        [TestMethod]
        public void TwoHandlers()
        {
            Recorder.Records.Clear();
            RecordingHandler handler1 = new RecordingHandler("1");
            RecordingHandler handler2 = new RecordingHandler("2");
            StubMethodInvocation invocation = new StubMethodInvocation();
            HandlerPipeline pipeline = new HandlerPipeline(handler1, handler2);

            pipeline.Invoke(invocation, delegate
                                        {
                                            Recorder.Records.Add("method");
                                            return null;
                                        });

            Assert.AreEqual(5, Recorder.Records.Count);
            Assert.AreEqual("Before Method (1)", Recorder.Records[0]);
            Assert.AreEqual("Before Method (2)", Recorder.Records[1]);
            Assert.AreEqual("method", Recorder.Records[2]);
            Assert.AreEqual("After Method (2)", Recorder.Records[3]);
            Assert.AreEqual("After Method (1)", Recorder.Records[4]);
        }
    }
}