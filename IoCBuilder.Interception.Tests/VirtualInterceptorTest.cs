using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using IoCBuilder.Interception.Tests.Stubs;
using TestMethod = NUnit.Framework.TestAttribute;

namespace IoCBuilder.Interception
{
    public class VirtualInterceptorTest
    {
        private static T WrapAndCreateType<T>(IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers,
                                      params object[] ctorArgs)
        {
            Type wrappedType = VirtualInterceptor.WrapClass(typeof(T));
            ILEmitProxy proxy = new ILEmitProxy(handlers);
            List<object> wrappedCtorArgs = new List<object>();
            wrappedCtorArgs.Add(proxy);
            wrappedCtorArgs.AddRange(ctorArgs);
            return (T)Activator.CreateInstance(wrappedType, wrappedCtorArgs.ToArray());
        }

        public struct ComplexValueType
        {
            public byte Byte;
            public char Char;
            public decimal Decimal;
            public double Double;
            public float Float;
            public int Int;
            public long Long;
            public short Short;
            public string String;
            public uint UInt;
            public ulong ULong;
            public ushort UShort;
        }

        public class Errors
        {
            [TestMethod]
            public void CannotInterceptNonPublicClass()
            {
                Assert.Throws<TypeLoadException>(
                    delegate
                    {
                        VirtualInterceptor.WrapClass(typeof(PrivateClass));
                    });
            }

            [TestMethod]
            public void CannotInterceptSealedClass()
            {
                Assert.Throws<TypeLoadException>(
                    delegate
                    {
                        VirtualInterceptor.WrapClass(typeof(SealedClass));
                    });
            }

            public sealed class SealedClass { }

            private class PrivateClass
            { }
        }

        public class GenericClasses
        {
            [TestMethod]
            public void GenericMethod()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(GenericSpy<>).GetMethod("GenericMethod");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                GenericSpy<int> result = WrapAndCreateType<GenericSpy<int>>(dictionary);
                result.GenericMethod(46, "2");

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In method with data 46 and 2", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            [TestMethod]
            public void NonGenericMethod()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(GenericSpy<>).GetMethod("MethodWhichTakesGenericData");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                GenericSpy<int> result = WrapAndCreateType<GenericSpy<int>>(dictionary);
                result.MethodWhichTakesGenericData(24);

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In method with data 24", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            [TestMethod]
            public void ReturnsDataOfGenericType()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(GenericSpy<>).GetMethod("GenericReturn");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                GenericSpy<int> result = WrapAndCreateType<GenericSpy<int>>(dictionary);
                int value = result.GenericReturn(256.9);

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                string e1 = "In method with data " + (256.9d).ToString();
                Assert.AreEqual(e1, Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
                Assert.AreEqual(default(int), value);
            }

            [TestMethod]
            public void WhereClauseOnClass()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(GenericSpyWithWhereClause<>).GetMethod("Method");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                GenericSpyWithWhereClause<Foo> result = WrapAndCreateType<GenericSpyWithWhereClause<Foo>>(dictionary);
                result.Method(new Foo());

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In method with data " + typeof(Foo).FullName, Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            public class Foo : IFoo { }

            public class GenericSpy<T>
            {
                public virtual void GenericMethod<T1>(T data,
                                                      T1 data1)
                {
                    Recorder.Records.Add("In method with data " + data + " and " + data1);
                }

                public virtual T GenericReturn<T1>(T1 data)
                {
                    Recorder.Records.Add("In method with data " + data);
                    return default(T);
                }

                public virtual void MethodWhichTakesGenericData(T data)
                {
                    Recorder.Records.Add("In method with data " + data);
                }
            }

            public class GenericSpyWithWhereClause<T>
                where T : class, IFoo
            {
                public virtual void Method(T data)
                {
                    Recorder.Records.Add("In method with data " + data);
                }
            }

            public interface IFoo { }
        }

        public class GenericMethods
        {
            [TestMethod]
            public void GenericMethod()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(NonGenericSpy).GetMethod("GenericMethod");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                NonGenericSpy result = WrapAndCreateType<NonGenericSpy>(dictionary);
                result.GenericMethod(21);

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In method with data 21", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            [TestMethod]
            public void ReturnsDataOfGenericType()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(NonGenericSpy).GetMethod("GenericReturn");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                NonGenericSpy result = WrapAndCreateType<NonGenericSpy>(dictionary);
                int value = result.GenericReturn<int, double>(256.9);

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                string e1 = "In method with data " + (256.9d).ToString();
                Assert.AreEqual(e1, Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
                Assert.AreEqual(default(int), value);
            }

            [TestMethod]
            public void WhereClauseOnMethod()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(NonGenericSpy).GetMethod("WhereMethod");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                NonGenericSpy result = WrapAndCreateType<NonGenericSpy>(dictionary);
                result.WhereMethod(new Foo());

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In method with data " + typeof(Foo).FullName, Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            public class Foo : IFoo { }

            public interface IFoo { }

            public class NonGenericSpy
            {
                public virtual void GenericMethod<T>(T data)
                {
                    Recorder.Records.Add("In method with data " + data);
                }

                public virtual T GenericReturn<T, T1>(T1 data)
                {
                    Recorder.Records.Add("In method with data " + data);
                    return default(T);
                }

                public virtual void WhereMethod<T>(T data)
                    where T : class, IFoo
                {
                    Recorder.Records.Add("In method with data " + data);
                }
            }
        }

        public class InParameters
        {
            [TestMethod]
            public void OneParameter()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyIn).GetMethod("OneParameter");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyIn result = WrapAndCreateType<SpyIn>(dictionary);
                int retValue = result.OneParameter(21);

                Assert.AreEqual(21 * 2, retValue);
                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In Method", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            [TestMethod]
            public void TwentyParameters()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyIn).GetMethod("TwentyParameters");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyIn result = WrapAndCreateType<SpyIn>(dictionary);
                int retValue = result.TwentyParameters(12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10);

                Assert.AreEqual(120, retValue);
                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In Method", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            [TestMethod]
            public void TwoParameters()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyIn).GetMethod("TwoParameters");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyIn result = WrapAndCreateType<SpyIn>(dictionary);
                string retValue = result.TwoParameters(42, "Hello ");

                Assert.AreEqual("Hello 42", retValue);
                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In Method", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            public class SpyIn
            {
                public virtual int OneParameter(int x)
                {
                    Recorder.Records.Add("In Method");
                    return x * 2;
                }

                public virtual int TwentyParameters(int p0,
                                                    int p1,
                                                    int p2,
                                                    int p3,
                                                    int p4,
                                                    int p5,
                                                    int p6,
                                                    int p7,
                                                    int p8,
                                                    int p9,
                                                    int p10,
                                                    int p11,
                                                    int p12,
                                                    int p13,
                                                    int p14,
                                                    int p15,
                                                    int p16,
                                                    int p17,
                                                    int p18,
                                                    int p19)
                {
                    Recorder.Records.Add("In Method");
                    return p0 * p19;
                }

                public virtual string TwoParameters(int x,
                                                    string y)
                {
                    Recorder.Records.Add("In Method");
                    return y + x;
                }
            }
        }

        public class OutParameters
        {
            [TestMethod]
            public void OutComplexValueType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyOut).GetMethod("OutComplexValueType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyOut result = WrapAndCreateType<SpyOut>(dictionary);
                ComplexValueType outValue;
                result.OutComplexValueType(out outValue);

                Assert.AreEqual(byte.MaxValue, outValue.Byte);
                Assert.AreEqual('a', outValue.Char);
                Assert.AreEqual(decimal.MaxValue, outValue.Decimal);
                Assert.AreEqual(double.MaxValue, outValue.Double);
                Assert.AreEqual(float.MaxValue, outValue.Float);
                Assert.AreEqual(int.MaxValue, outValue.Int);
                Assert.AreEqual(long.MaxValue, outValue.Long);
                Assert.AreEqual(short.MaxValue, outValue.Short);
                Assert.AreEqual("Hello, world!", outValue.String);
                Assert.AreEqual(uint.MaxValue, outValue.UInt);
                Assert.AreEqual(ulong.MaxValue, outValue.ULong);
                Assert.AreEqual(ushort.MaxValue, outValue.UShort);
            }

            [TestMethod]
            public void OutDouble()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyOut).GetMethod("OutDouble");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyOut result = WrapAndCreateType<SpyOut>(dictionary);
                double outValue;
                result.OutDouble(out outValue);

                Assert.AreEqual(double.MaxValue, outValue);
            }

            [TestMethod]
            public void OutInt16()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyOut).GetMethod("OutInt16");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyOut result = WrapAndCreateType<SpyOut>(dictionary);
                short outValue;
                result.OutInt16(out outValue);

                Assert.AreEqual(short.MaxValue, outValue);
            }

            [TestMethod]
            public void OutInt32()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyOut).GetMethod("OutInt32");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyOut result = WrapAndCreateType<SpyOut>(dictionary);
                int outValue;
                result.OutInt32(out outValue);

                Assert.AreEqual(int.MaxValue, outValue);
            }

            [TestMethod]
            public void OutInt64()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyOut).GetMethod("OutInt64");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyOut result = WrapAndCreateType<SpyOut>(dictionary);
                long outValue;
                result.OutInt64(out outValue);

                Assert.AreEqual(long.MaxValue, outValue);
            }

            [TestMethod]
            public void OutReferenceType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyOut).GetMethod("OutReferenceType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyOut result = WrapAndCreateType<SpyOut>(dictionary);
                string outReference;
                result.OutReferenceType(out outReference);

                Assert.AreEqual("Hello, world!", outReference);
            }

            public class SpyOut
            {
                public virtual void OutChar(out char outValue)
                {
                    outValue = 'a';
                }

                public virtual void OutComplexValueType(out ComplexValueType outValueType)
                {
                    outValueType = new ComplexValueType();
                    outValueType.Byte = byte.MaxValue;
                    outValueType.Char = 'a';
                    outValueType.Decimal = decimal.MaxValue;
                    outValueType.Double = double.MaxValue;
                    outValueType.Float = float.MaxValue;
                    outValueType.Int = int.MaxValue;
                    outValueType.Long = long.MaxValue;
                    outValueType.Short = short.MaxValue;
                    outValueType.String = "Hello, world!";
                    outValueType.UInt = uint.MaxValue;
                    outValueType.ULong = ulong.MaxValue;
                    outValueType.UShort = ushort.MaxValue;
                }

                public virtual void OutDouble(out double outValue)
                {
                    outValue = double.MaxValue;
                }

                public virtual void OutInt16(out short outValue)
                {
                    outValue = short.MaxValue;
                }

                public virtual void OutInt32(out int outValue)
                {
                    outValue = int.MaxValue;
                }

                public virtual void OutInt64(out long outValue)
                {
                    outValue = long.MaxValue;
                }

                public virtual void OutReferenceType(out string outReference)
                {
                    outReference = "Hello, world!";
                }
            }
        }

        public class RefParameters
        {
            [TestMethod]
            public void RefClassParameter()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyRef).GetMethod("RefClassParameter");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyRef result = WrapAndCreateType<SpyRef>(dictionary);
                string refValue = "Hello, ";
                result.RefClassParameter(ref refValue);

                Assert.AreEqual("Hello, world!", refValue);
            }

            [TestMethod]
            public void RefValueType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyRef).GetMethod("RefValueType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyRef result = WrapAndCreateType<SpyRef>(dictionary);
                int refValue = 21;
                result.RefValueType(ref refValue);

                Assert.AreEqual(42, refValue);
            }

            public class SpyRef
            {
                public virtual void RefClassParameter(ref string value)
                {
                    value += "world!";
                }

                public virtual void RefValueType(ref int value)
                {
                    value *= 2;
                }
            }
        }

        public class ReturnValues
        {
            [TestMethod]
            public void Exception()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyReturn).GetMethod("Exception");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyReturn result = WrapAndCreateType<SpyReturn>(dictionary, 42);
                Assert.Throws<ArgumentException>(delegate
                                                 {
                                                     result.Exception();
                                                 });
            }

            [TestMethod]
            public void NoReturnValue()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyReturn).GetMethod("NoReturnValue");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyReturn result = WrapAndCreateType<SpyReturn>(dictionary, 42);
                result.NoReturnValue();

                Assert.AreEqual(42, result.ConstructorValue);
                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In Method", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            [TestMethod]
            public void ReturnsClassType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyReturn).GetMethod("ReturnsClassType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyReturn result = WrapAndCreateType<SpyReturn>(dictionary, 42);
                object retValue = result.ReturnsClassType();

                Assert.AreSame(SpyReturn.ObjectReturn, retValue);
            }

            [TestMethod]
            public void ReturnsValueType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(SpyReturn).GetMethod("ReturnsValueType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                SpyReturn result = WrapAndCreateType<SpyReturn>(dictionary, 42);
                int retValue = result.ReturnsValueType();

                Assert.AreEqual(SpyReturn.ValueReturn, retValue);
            }

            public class SpyReturn
            {
                public const int ValueReturn = 42;
                public static object ObjectReturn = new object();
                public readonly int ConstructorValue;

                public SpyReturn(int x)
                {
                    ConstructorValue = x;
                }

                public virtual void Exception()
                {
                    throw new ArgumentException();
                }

                public virtual void NoReturnValue()
                {
                    Recorder.Records.Add("In Method");
                }

                public virtual object ReturnsClassType()
                {
                    return ObjectReturn;
                }

                public virtual int ReturnsValueType()
                {
                    return ValueReturn;
                }
            }
        }

        public class Inheritance
        {
            [TestMethod]
            public void SecondOverride()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ClassC).GetMethod("CanDo");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ClassC result = WrapAndCreateType<ClassC>(dictionary);
                int retValue = result.CanDo();

                Assert.AreEqual(3, retValue);
            }
        }
    }
}