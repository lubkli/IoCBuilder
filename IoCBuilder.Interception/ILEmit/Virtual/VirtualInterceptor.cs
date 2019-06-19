using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Interceptor based on IL Emitting
    /// </summary>
    public class VirtualInterceptor : ILEmitInterceptor
    {
        private static readonly VirtualInterceptor instance = new VirtualInterceptor();

        private static void GenerateConstructor(TypeBuilder typeBuilder,
                                        ConstructorInfo constructor,
                                        FieldInfo fieldProxy)
        {
            // Get constructor parameters
            List<Type> parameterTypes = new List<Type>();

            parameterTypes.Add(typeof(ILEmitProxy));
            foreach (ParameterInfo parameterInfo in constructor.GetParameters())
                parameterTypes.Add(parameterInfo.ParameterType);

            // Define constructor
            ConstructorBuilder wrappedConstructor = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.HasThis,
                parameterTypes.ToArray());

            ILGenerator il = wrappedConstructor.GetILGenerator();

            // Call base constructor
            il.Emit(OpCodes.Ldarg_0);

            for (int i = 0; i < constructor.GetParameters().Length; i++)
                il.Emit(OpCodes.Ldarg_S, i + 2);

            il.Emit(OpCodes.Call, constructor);

            // Store proxy so it can be used in the overriden methods
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldProxy);

            // Return
            il.Emit(OpCodes.Ret);
        }

        private static void GenerateOverloadedMethod(TypeBuilder typeBuilder,
                                             MethodInfo methodToIntercept,
                                             MethodInfo anonymousDelegate,
                                             FieldInfo fieldProxy)
        {
            // Get method parameters
            ParameterInfo[] parameters = methodToIntercept.GetParameters();
            List<Type> parameterTypes = new List<Type>();
            List<Type> parameterRealTypes = new List<Type>();

            foreach (ParameterInfo parameter in parameters)
            {
                parameterTypes.Add(parameter.ParameterType);

                if (parameter.IsOut || parameter.ParameterType.IsByRef)
                    parameterRealTypes.Add(parameter.ParameterType.GetElementType());
                else
                    parameterRealTypes.Add(parameter.ParameterType);
            }

            // Define overriden method
            MethodBuilder method =
                typeBuilder.DefineMethod(methodToIntercept.Name,
                                         MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                         methodToIntercept.ReturnType,
                                         parameterTypes.ToArray());

            Type[] genericParameterTypes = SetupGenericMethodArguments(methodToIntercept, method);

            ILGenerator il = method.GetILGenerator();

            // Locals for reflection method info and parameter array, for calls to proxy.Invoke()
            il.DeclareLocal(typeof(MethodInfo));
            il.DeclareLocal(typeof(object[]));

            // Local for the return value
            if (methodToIntercept.ReturnType != typeof(void))
                il.DeclareLocal(methodToIntercept.ReturnType);

            // Initialize default values for out parameters
            for (int idx = 0; idx < parameters.Length; ++idx)
            {
                if (parameters[idx].IsOut && !parameters[idx].IsIn)
                {
                    il.Emit(OpCodes.Ldarg_S, idx + 1);
                    il.Emit(OpCodes.Initobj, parameterRealTypes[idx]);
                }
            }

            // Call to MethodInfo.GetCurrentMethod() and cast to MethodInfo
            il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Static | BindingFlags.Public));
            il.Emit(OpCodes.Castclass, typeof(MethodInfo));
            il.Emit(OpCodes.Stloc_0);

            // Create an array equal to the size of the # of parameters
            il.Emit(OpCodes.Ldc_I4_S, parameters.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc_1);

            // Populate the array
            for (int idx = 0; idx < parameters.Length; ++idx)
            {
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldc_I4_S, idx);
                il.Emit(OpCodes.Ldarg_S, idx + 1);

                if (parameters[idx].IsOut || parameters[idx].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldobj, parameterRealTypes[idx]);

                if (parameterRealTypes[idx].IsValueType || parameterRealTypes[idx].IsGenericParameter)
                    il.Emit(OpCodes.Box, parameterRealTypes[idx]);

                il.Emit(OpCodes.Stelem_Ref);
            }

            // Parameter 0 (this argument) for the call to Invoke
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldProxy);

            // Parameter 1 (target) for the call to Invoke
            il.Emit(OpCodes.Ldarg_0);

            // Parameter 2 (method) for the call to Invoke
            il.Emit(OpCodes.Ldloc_0);
            //il.Emit(OpCodes.Callvirt, typeof(MethodInfo).GetMethod("GetBaseDefinition"));
            il.Emit(OpCodes.Callvirt, typeof(MethodInfo).GetMethod("get_DeclaringType"));
            il.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("get_BaseType"));

            // Nachtam parametr
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, typeof(MethodInfo).GetMethod("get_Name"));
            il.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string) }));

            // Parameter 3 (parameter array) for the call to Invoke
            il.Emit(OpCodes.Ldloc_1);

            // Parameter 4 (anonymous delegate) for the call to Invoke
            il.Emit(OpCodes.Ldarg_0);

            if (genericParameterTypes.Length > 0)
                il.Emit(OpCodes.Ldftn, anonymousDelegate.MakeGenericMethod(genericParameterTypes));
            else
                il.Emit(OpCodes.Ldftn, anonymousDelegate);

            il.Emit(OpCodes.Newobj, typeof(ILEmitProxy.InvokeDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));

            // Call Invoke
            il.Emit(OpCodes.Callvirt, typeof(ILEmitProxy).GetMethod("Invoke"));

            // Retrieve the return value from Invoke
            if (methodToIntercept.ReturnType == typeof(void))
                il.Emit(OpCodes.Pop);
            else
            {
                // Cast or unbox, dependening on whether it's a class or value type
                if (methodToIntercept.ReturnType.IsClass)
                    il.Emit(OpCodes.Castclass, methodToIntercept.ReturnType);
                else
                    il.Emit(OpCodes.Unbox_Any, methodToIntercept.ReturnType);

                // Store the value into the temporary
                il.Emit(OpCodes.Stloc_2);

                // Load the return value
                il.Emit(OpCodes.Ldloc_2);
            }

            // Set out/ref values before returning
            for (int idx = 0; idx < parameters.Length; ++idx)
            {
                if (parameters[idx].IsOut || parameters[idx].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_S, idx + 1);
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldc_I4_S, idx);
                    il.Emit(OpCodes.Ldelem_Ref);

                    if (parameterRealTypes[idx].IsValueType)
                        il.Emit(OpCodes.Unbox_Any, parameterRealTypes[idx]);
                    else
                        il.Emit(OpCodes.Castclass, parameterRealTypes[idx]);

                    il.Emit(OpCodes.Stobj, parameterRealTypes[idx]);
                }
            }

            il.Emit(OpCodes.Ret);
        }

        private static MethodBuilder GenerateOverloadedMethodDelegate(MethodInfo methodToIntercept,
                                                              TypeBuilder typeBuilder)
        {            
            // Define the method
            MethodBuilder method =
                typeBuilder.DefineMethod(methodToIntercept.Name + "{Delegate}",
                                         MethodAttributes.Private | MethodAttributes.HideBySig,
                                         typeof(object),
                                         new Type[] { typeof(object[]) });

            SetupGenericMethodArguments(methodToIntercept, method);

            ILGenerator il = method.GetILGenerator();

            // Local for return value
            il.DeclareLocal(typeof(object));

            // Local for each out/ref parameter
            ParameterInfo[] parameters = methodToIntercept.GetParameters();

            foreach (ParameterInfo parameter in parameters)
                if (parameter.IsOut || parameter.ParameterType.IsByRef)
                    il.DeclareLocal(parameter.ParameterType.GetElementType());

            // Initialize out parameters to default values
            int localIndex = 1;

            for (int idx = 0; idx < parameters.Length; ++idx)
            {
                if (parameters[idx].ParameterType.IsByRef && !parameters[idx].IsOut)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4_S, idx);
                    il.Emit(OpCodes.Ldelem_Ref);

                    if (parameters[idx].ParameterType.GetElementType().IsValueType)
                        il.Emit(OpCodes.Unbox_Any, parameters[idx].ParameterType.GetElementType());
                    else
                        il.Emit(OpCodes.Castclass, parameters[idx].ParameterType.GetElementType());

                    il.Emit(OpCodes.Stloc_S, localIndex++);
                }
            }

            // Load target (this)
            il.Emit(OpCodes.Ldarg_0);

            // Push call values onto stack
            localIndex = 1;

            for (int idx = 0; idx < parameters.Length; ++idx)
                if (parameters[idx].IsOut || parameters[idx].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, localIndex++);
                else
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4_S, idx);
                    il.Emit(OpCodes.Ldelem_Ref);

                    if (parameters[idx].ParameterType.IsValueType || parameters[idx].ParameterType.IsGenericParameter)
                        il.Emit(OpCodes.Unbox_Any, parameters[idx].ParameterType);
                }

            // Call base method
            il.Emit(OpCodes.Call, methodToIntercept/*.GetBaseDefinition()*/);

            // Stash return value
            if (methodToIntercept.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldnull);
            }
            else if (methodToIntercept.ReturnType.IsValueType)
                il.Emit(OpCodes.Box, methodToIntercept.ReturnType);

            il.Emit(OpCodes.Stloc_0);

            // Copy out/ref parameter values back into passed-in parameters array
            localIndex = 1;

            for (int idx = 0; idx < parameters.Length; ++idx)
                if (parameters[idx].IsOut || parameters[idx].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4_S, idx);
                    il.Emit(OpCodes.Ldloc_S, localIndex++);

                    if (parameters[idx].ParameterType.GetElementType().IsValueType)
                        il.Emit(OpCodes.Box, parameters[idx].ParameterType.GetElementType());

                    il.Emit(OpCodes.Stelem_Ref);
                }

            // Return
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return method;
        }

        /// <summary>
        /// Generates new type as wrapper for base
        /// </summary>
        /// <param name="classToWrap">Original type.</param>
        /// <param name="module">Defines and represents a module in a dynamic assembly.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns></returns>
        protected override Type GenerateWrapperType(Type classToWrap,
                                                    ModuleBuilder module,
                                                    string moduleName)
        {
            // Define overriding type
            TypeBuilder typeBuilder = module.DefineType(MakeTypeName(moduleName, classToWrap), TypeAttributes.Public);
            typeBuilder.SetParent(SetupGenericClassArguments(classToWrap, typeBuilder));

            // Declare a field for the proxy
            FieldBuilder fieldProxy = typeBuilder.DefineField("proxy", typeof(ILEmitProxy), FieldAttributes.Private);

            // Create overrides (and delegates) for all virtual methods
            foreach (MethodInfo method in classToWrap.GetMethods())
                if (method.IsVirtual && !method.IsFinal)
                    GenerateOverloadedMethod(typeBuilder,
                                             method,
                                             GenerateOverloadedMethodDelegate(method, typeBuilder),
                                             fieldProxy);

            // Generate overrides for all constructors
            foreach (ConstructorInfo constructor in classToWrap.GetConstructors())
                GenerateConstructor(typeBuilder, constructor, fieldProxy);

            return typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Creates wrapper on top of original type.
        /// </summary>
        /// <param name="classToWrap">Original type.</param>
        /// <returns>New type.</returns>
        public static Type WrapClass(Type classToWrap)
        {
            return instance.Wrap(classToWrap);
        }
    }
}