using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using TestMethod = NUnit.Framework.TestAttribute;

namespace IoCBuilder.Interception
{
    public class InterfaceInterceptorTest
    {
        private static TInterface WrapAndCreateType<TInterface, TConcrete>(IEnumerable<KeyValuePair<MethodBase, List<IInterceptionHandler>>> handlers)
            where TConcrete : TInterface
        {
            Type wrappedType = InterfaceInterceptor.WrapInterface(typeof(TInterface));
            ILEmitProxy proxy = new ILEmitProxy(handlers);
            object target = Activator.CreateInstance(typeof(TConcrete));
            List<object> wrappedCtorArgs = new List<object>();
            wrappedCtorArgs.Add(proxy);
            wrappedCtorArgs.Add(target);
            return (TInterface)Activator.CreateInstance(wrappedType, wrappedCtorArgs.ToArray());
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

        public class FindMethod
        {
            [TestMethod]
            public void GenericClassGenericMethod()
            {
                MethodInfo result = InterfaceInterceptor.FindMethod("Insert", new object[] { typeof(int), "T" }, typeof(IList<>).GetMethods());

                Assert.AreSame(typeof(IList<>).GetMethod("Insert"), result);
            }

            [TestMethod]
            public void GenericClassGenericMethodWithExtraGenerics()
            {
                MethodInfo result = InterfaceInterceptor.FindMethod("Bar", new object[] { "T", "T2" }, typeof(IFoo<>).GetMethods());

                Assert.AreSame(typeof(IFoo<>).GetMethod("Bar"), result);
            }

            [TestMethod]
            public void GenericClassNonGenericMethod()
            {
                MethodInfo result = InterfaceInterceptor.FindMethod("RemoveAt", new Type[] { typeof(int) }, typeof(IList<>).GetMethods());

                Assert.AreSame(typeof(IList<>).GetMethod("RemoveAt"), result);
            }

            [TestMethod]
            public void MethodNotFound()
            {
                MethodInfo result = InterfaceInterceptor.FindMethod("ThisMethodDoesNotExist", new Type[0], typeof(Object).GetMethods());

                Assert.Null(result);
            }

            [TestMethod]
            public void NonGenericClass()
            {
                MethodInfo result = InterfaceInterceptor.FindMethod("ToString", new Type[0], typeof(Object).GetMethods());

                Assert.AreSame(typeof(Object).GetMethod("ToString"), result);
            }

            [TestMethod]
            public void Overloads()
            {
                MethodInfo result1 = InterfaceInterceptor.FindMethod("Overload", new Type[0], typeof(IFoo<>).GetMethods());
                MethodInfo result2 = InterfaceInterceptor.FindMethod("Overload", new Type[] { typeof(int) }, typeof(IFoo<>).GetMethods());
                MethodInfo result3 = InterfaceInterceptor.FindMethod("Overload", new Type[] { typeof(double) }, typeof(IFoo<>).GetMethods());

                Assert.NotNull(result1);
                Assert.NotNull(result2);
                Assert.Null(result3);
                Assert.AreNotSame(result1, result2);
            }

            [TestMethod]
            public void ParameterTypesNotMatching()
            {
                MethodInfo result = InterfaceInterceptor.FindMethod("ToString", new Type[] { typeof(int) }, typeof(Object).GetMethods());

                Assert.Null(result);
            }

            public class Foo<T> : IFoo<T>
            {
                public void Bar<T2>(T data,
                                    T2 data2)
                { }

                public void Baz()
                {
                }

                public void Overload()
                {
                }

                public void Overload(int x)
                {
                }
            }

            public interface IFoo<T>
            {
                void Bar<T2>(T data,
                             T2 data2);

                void Overload();

                void Overload(int x);
            }
        }

        public class GenericClasses
        {
            [TestMethod]
            public void GenericMethod()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(IGenericSpy<>).GetMethod("GenericMethod");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                IGenericSpy<int> result = WrapAndCreateType<IGenericSpy<int>, GenericSpy<int>>(dictionary);
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
                MethodBase method = typeof(IGenericSpy<>).GetMethod("MethodWhichTakesGenericData");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                IGenericSpy<int> result = WrapAndCreateType<IGenericSpy<int>, GenericSpy<int>>(dictionary);
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
                MethodBase method = typeof(IGenericSpy<>).GetMethod("GenericReturn");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                IGenericSpy<int> result = WrapAndCreateType<IGenericSpy<int>, GenericSpy<int>>(dictionary);
                int value = result.GenericReturn(256.9);

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                string e1 = "In method with data " + (256.9d).ToString();
                Assert.AreEqual(e1, Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
                Assert.AreEqual(default(int), value);
            }

            [TestMethod]
            public void WhereClauseOnInterface()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(IGenericSpyWithWhereClause<>).GetMethod("Method");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                IGenericSpyWithWhereClause<Foo> result = WrapAndCreateType<IGenericSpyWithWhereClause<Foo>, GenericSpyWithWhereClause<Foo>>(dictionary);
                result.Method(new Foo());

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In method with data " + typeof(Foo).FullName, Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            public class Foo : IFoo { }

            public class GenericSpy<T> : IGenericSpy<T>
            {
                public void GenericMethod<T1>(T data,
                                              T1 data1)
                {
                    Recorder.Records.Add("In method with data " + data + " and " + data1);
                }

                public T GenericReturn<T1>(T1 data)
                {
                    Recorder.Records.Add("In method with data " + data);
                    return default(T);
                }

                public void MethodWhichTakesGenericData(T data)
                {
                    Recorder.Records.Add("In method with data " + data);
                }
            }

            public class GenericSpyWithWhereClause<T> : IGenericSpyWithWhereClause<T>
                where T : class, IFoo
            {
                public void Method(T data)
                {
                    Recorder.Records.Add("In method with data " + data);
                }
            }

            public interface IFoo { }

            public interface IGenericSpy<T>
            {
                void GenericMethod<T1>(T data,
                                       T1 data1);

                T GenericReturn<T1>(T1 data);

                void MethodWhichTakesGenericData(T data);
            }

            public interface IGenericSpyWithWhereClause<T>
                where T : class, IFoo
            {
                void Method(T data);
            }
        }

        public class GenericMethods
        {
            [TestMethod]
            public void GenericMethod()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(INonGenericSpy).GetMethod("GenericMethod");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                INonGenericSpy result = WrapAndCreateType<INonGenericSpy, NonGenericSpy>(dictionary);
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
                MethodBase method = typeof(INonGenericSpy).GetMethod("GenericReturn");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                INonGenericSpy result = WrapAndCreateType<INonGenericSpy, NonGenericSpy>(dictionary);
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
                MethodBase method = typeof(INonGenericSpy).GetMethod("WhereMethod");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                INonGenericSpy result = WrapAndCreateType<INonGenericSpy, NonGenericSpy>(dictionary);
                result.WhereMethod(new Foo());

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In method with data " + typeof(Foo).FullName, Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            public class Foo : IFoo { }

            public interface IFoo { }

            public interface INonGenericSpy
            {
                void GenericMethod<T>(T data);

                T GenericReturn<T, T1>(T1 data);

                void WhereMethod<T>(T data)
                    where T : class, IFoo;
            }

            public class NonGenericSpy : INonGenericSpy
            {
                public void GenericMethod<T>(T data)
                {
                    Recorder.Records.Add("In method with data " + data);
                }

                public T GenericReturn<T, T1>(T1 data)
                {
                    Recorder.Records.Add("In method with data " + data);
                    return default(T);
                }

                public void WhereMethod<T>(T data)
                    where T : class, IFoo
                {
                    Recorder.Records.Add("In method with data " + data);
                }
            }
        }

        public class InParameters
        {
            [TestMethod]
            public void InReferenceParameter()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyIn).GetMethod("InReferenceParameter");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyIn result = WrapAndCreateType<ISpyIn, SpyIn>(dictionary);
                result.InReferenceParameter("Hello");

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In Method: Hello", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            [TestMethod]
            public void InValueParameter()
            {
                Recorder.Records.Clear();
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyIn).GetMethod("InValueParameter");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyIn result = WrapAndCreateType<ISpyIn, SpyIn>(dictionary);
                result.InValueParameter(42);

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In Method: 42", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            public interface ISpyIn
            {
                void InReferenceParameter(string s);

                void InValueParameter(int x);
            }

            private sealed class SpyIn : ISpyIn
            {
                void ISpyIn.InReferenceParameter(string s)
                {
                    Recorder.Records.Add("In Method: " + s);
                }

                public void InValueParameter(int x)
                {
                    Recorder.Records.Add("In Method: " + x);
                }
            }
        }

        public class OutParameters
        {
            [TestMethod]
            public void OutComplexValueType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyOut).GetMethod("OutComplexValueType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyOut result = WrapAndCreateType<ISpyOut, SpyOut>(dictionary);
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
                MethodBase method = typeof(ISpyOut).GetMethod("OutDouble");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyOut result = WrapAndCreateType<ISpyOut, SpyOut>(dictionary);
                double outValue;
                result.OutDouble(out outValue);

                Assert.AreEqual(double.MaxValue, outValue);
            }

            [TestMethod]
            public void OutInt16()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyOut).GetMethod("OutInt16");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyOut result = WrapAndCreateType<ISpyOut, SpyOut>(dictionary);
                short outValue;
                result.OutInt16(out outValue);

                Assert.AreEqual(short.MaxValue, outValue);
            }

            [TestMethod]
            public void OutInt32()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyOut).GetMethod("OutInt32");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyOut result = WrapAndCreateType<ISpyOut, SpyOut>(dictionary);
                int outValue;
                result.OutInt32(out outValue);

                Assert.AreEqual(int.MaxValue, outValue);
            }

            [TestMethod]
            public void OutInt64()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyOut).GetMethod("OutInt64");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyOut result = WrapAndCreateType<ISpyOut, SpyOut>(dictionary);
                long outValue;
                result.OutInt64(out outValue);

                Assert.AreEqual(long.MaxValue, outValue);
            }

            [TestMethod]
            public void OutReferenceType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyOut).GetMethod("OutReferenceType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyOut result = WrapAndCreateType<ISpyOut, SpyOut>(dictionary);
                string outReference;
                result.OutReferenceType(out outReference);

                Assert.AreEqual("Hello, world!", outReference);
            }

            public interface ISpyOut
            {
                void OutChar(out char outValue);

                void OutComplexValueType(out ComplexValueType outValueType);

                void OutDouble(out double outValue);

                void OutInt16(out short outValue);

                void OutInt32(out int outValue);

                void OutInt64(out long outValue);

                void OutReferenceType(out string outReference);
            }

            private sealed class SpyOut : ISpyOut
            {
                public void OutChar(out char outValue)
                {
                    outValue = 'a';
                }

                public void OutComplexValueType(out ComplexValueType outValueType)
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

                public void OutDouble(out double outValue)
                {
                    outValue = double.MaxValue;
                }

                public void OutInt16(out short outValue)
                {
                    outValue = short.MaxValue;
                }

                public void OutInt32(out int outValue)
                {
                    outValue = int.MaxValue;
                }

                public void OutInt64(out long outValue)
                {
                    outValue = long.MaxValue;
                }

                public void OutReferenceType(out string outReference)
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
                MethodBase method = typeof(ISpyRef).GetMethod("RefClassParameter");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyRef result = WrapAndCreateType<ISpyRef, SpyRef>(dictionary);
                string refValue = "Hello, ";
                result.RefClassParameter(ref refValue);

                Assert.AreEqual("Hello, world!", refValue);
            }

            [TestMethod]
            public void RefValueType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyRef).GetMethod("RefValueType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyRef result = WrapAndCreateType<ISpyRef, SpyRef>(dictionary);
                int refValue = 21;
                result.RefValueType(ref refValue);

                Assert.AreEqual(42, refValue);
            }

            public interface ISpyRef
            {
                void RefClassParameter(ref string value);

                void RefValueType(ref int value);
            }

            private sealed class SpyRef : ISpyRef
            {
                public void RefClassParameter(ref string value)
                {
                    value += "world!";
                }

                public void RefValueType(ref int value)
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
                MethodBase method = typeof(ISpyReturn).GetMethod("Exception");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyReturn result = WrapAndCreateType<ISpyReturn, SpyReturn>(dictionary);
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
                MethodBase method = typeof(ISpyReturn).GetMethod("NoReturnValue");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyReturn result = WrapAndCreateType<ISpyReturn, SpyReturn>(dictionary);
                result.NoReturnValue();

                Assert.AreEqual(3, Recorder.Records.Count);
                Assert.AreEqual("Before Method", Recorder.Records[0]);
                Assert.AreEqual("In Method", Recorder.Records[1]);
                Assert.AreEqual("After Method", Recorder.Records[2]);
            }

            [TestMethod]
            public void ReturnsClassType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyReturn).GetMethod("ReturnsClassType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyReturn result = WrapAndCreateType<ISpyReturn, SpyReturn>(dictionary);
                object retValue = result.ReturnsClassType();

                Assert.AreSame(SpyReturn.ObjectReturn, retValue);
            }

            [TestMethod]
            public void ReturnsValueType()
            {
                RecordingHandler handler = new RecordingHandler();
                MethodBase method = typeof(ISpyReturn).GetMethod("ReturnsValueType");
                Dictionary<MethodBase, List<IInterceptionHandler>> dictionary = new Dictionary<MethodBase, List<IInterceptionHandler>>();
                List<IInterceptionHandler> handlers = new List<IInterceptionHandler>();
                handlers.Add(handler);
                dictionary.Add(method, handlers);

                ISpyReturn result = WrapAndCreateType<ISpyReturn, SpyReturn>(dictionary);
                int retValue = result.ReturnsValueType();

                Assert.AreEqual(SpyReturn.ValueReturn, retValue);
            }

            public interface ISpyReturn
            {
                void Exception();

                void NoReturnValue();

                object ReturnsClassType();

                int ReturnsValueType();
            }

            private sealed class SpyReturn : ISpyReturn
            {
                public const int ValueReturn = 42;
                public static readonly object ObjectReturn = new object();

                public void Exception()
                {
                    throw new ArgumentException();
                }

                public void NoReturnValue()
                {
                    Recorder.Records.Add("In Method");
                }

                public object ReturnsClassType()
                {
                    return ObjectReturn;
                }

                public int ReturnsValueType()
                {
                    return ValueReturn;
                }
            }
        }
    }
}