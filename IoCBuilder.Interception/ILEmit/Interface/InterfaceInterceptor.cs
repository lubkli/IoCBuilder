using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Interceptor based on IL Emitting
    /// </summary>
    public class InterfaceInterceptor : ILEmitInterceptor
    {
        private static readonly InterfaceInterceptor instance = new InterfaceInterceptor();

        /// <summary>
        /// Find method for interception.
        /// </summary>
        /// <param name="methodName">Method name.</param>
        /// <param name="methodArgumentTypes">Types of method's arguments.</param>
        /// <param name="methodsToSearch">List of method to search.</param>
        /// <returns></returns>
        public static MethodInfo FindMethod(string methodName,
                                            object[] methodArgumentTypes,
                                            IEnumerable<MethodInfo> methodsToSearch)
        {
            if (methodsToSearch == null)
                return null;

            foreach (MethodInfo method in methodsToSearch)
            {
                if (method.Name != methodName)
                    continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != methodArgumentTypes.Length)
                    continue;

                int idx = 0;
                for (; idx < parameters.Length; idx++)
                {
                    Type methodArgumentType = methodArgumentTypes[idx] as Type;

                    if (methodArgumentType != null)
                    {
                        if (methodArgumentType != parameters[idx].ParameterType)
                            break;
                    }
                    else
                    {
                        if ((string)methodArgumentTypes[idx] != parameters[idx].ParameterType.Name)
                            break;
                    }
                }

                if (idx == parameters.Length)
                    return method;
            }

            return null;
        }

        private static void GenerateConstructor(TypeBuilder typeBuilder,
                                        Type interfaceToWrap,
                                        FieldInfo fieldProxy,
                                        FieldInfo fieldTarget,
                                        IEnumerable<KeyValuePair<FieldBuilder, MethodInfo>> overloadedMethods)
        {
            ConstructorBuilder constructor =
                typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                              CallingConventions.HasThis,
                                              new Type[] { typeof(ILEmitProxy), interfaceToWrap });

            ConstructorInfo defaultBaseConstructor = typeof(Object).GetConstructor(new Type[0]);

            ILGenerator il = constructor.GetILGenerator();

            // Locals
            il.DeclareLocal(typeof(MethodInfo[]));
            il.DeclareLocal(typeof(object[]));

            // Call base constructor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, defaultBaseConstructor);

            // Stash proxy into field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldProxy);

            // Stash target into field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, fieldTarget);

            MethodInfo getTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            MethodInfo getMethodsMethod = typeof(Type).GetMethod("GetMethods", new Type[0]);
            MethodInfo findMethodMethod = typeof(InterfaceInterceptor).GetMethod("FindMethod", BindingFlags.Public | BindingFlags.Static);
            MethodInfo makeByRefTypeMethod = typeof(Type).GetMethod("MakeByRefType");

            // Get all the methods on the generic version of the interface
            il.Emit(OpCodes.Ldtoken, interfaceToWrap);
            il.Emit(OpCodes.Call, getTypeFromHandleMethod);
            il.Emit(OpCodes.Call, getMethodsMethod);
            il.Emit(OpCodes.Stloc_0);

            // Find all the methods
            foreach (KeyValuePair<FieldBuilder, MethodInfo> kvp in overloadedMethods)
            {
                // Create array of parameter types
                ParameterInfo[] parameters = kvp.Value.GetParameters();
                il.Emit(OpCodes.Ldc_I4_S, parameters.Length);
                il.Emit(OpCodes.Newarr, typeof(object));
                il.Emit(OpCodes.Stloc_1);

                for (int idx = 0; idx < parameters.Length; idx++)
                {
                    ParameterInfo parameter = parameters[idx];
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldc_I4_S, idx);

                    if (parameter.ParameterType.IsGenericParameter)
                    {
                        il.Emit(OpCodes.Ldstr, parameter.ParameterType.Name);
                    }
                    else if (parameter.ParameterType.IsByRef)
                    {
                        il.Emit(OpCodes.Ldtoken, parameter.ParameterType.GetElementType());
                        il.Emit(OpCodes.Call, getTypeFromHandleMethod);
                        il.Emit(OpCodes.Callvirt, makeByRefTypeMethod);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldtoken, parameter.ParameterType);
                        il.Emit(OpCodes.Call, getTypeFromHandleMethod);
                    }

                    il.Emit(OpCodes.Stelem_Ref);
                }

                // Call helper method
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, kvp.Value.Name);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Call, findMethodMethod);
                il.Emit(OpCodes.Stfld, kvp.Key);
            }

            // Return
            il.Emit(OpCodes.Ret);
        }

        private static void GenerateOverloadedMethod(TypeBuilder typeBuilder,
                                             MethodInfo methodToIntercept,
                                             MethodInfo anonymousDelegate,
                                             FieldInfo fieldProxy,
                                             FieldInfo fieldTarget,
                                             FieldInfo fieldMethodInfo)
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

            // Create overriden method
            MethodBuilder method =
                typeBuilder.DefineMethod(methodToIntercept.Name,
                                         MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final,
                                         methodToIntercept.ReturnType,
                                         parameterTypes.ToArray());

            Type[] genericParameterTypes = SetupGenericMethodArguments(methodToIntercept, method);

            ILGenerator il = method.GetILGenerator();

            // Locals
            il.DeclareLocal(typeof(object[]));

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

            // Create an array equal to the size of the # of parameters
            il.Emit(OpCodes.Ldc_I4_S, parameters.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc_0);

            // Populate the array
            for (int idx = 0; idx < parameters.Length; ++idx)
            {
                il.Emit(OpCodes.Ldloc_0);
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
            il.Emit(OpCodes.Ldfld, fieldTarget);

            // Parameter 2 (method) for the call to Invoke
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldMethodInfo);

            // Parameter 3 (parameter array) for the call to Invoke
            il.Emit(OpCodes.Ldloc_0);

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
                il.Emit(OpCodes.Stloc_1);

                // (Seemingly unnecessary?) branch
                Label end = il.DefineLabel();
                il.Emit(OpCodes.Br_S, end);
                il.MarkLabel(end);

                // Load the return value
                il.Emit(OpCodes.Ldloc_1);
            }

            // Set out/ref values before returning
            for (int idx = 0; idx < parameters.Length; ++idx)
            {
                if (parameters[idx].IsOut || parameters[idx].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_S, idx + 1);
                    il.Emit(OpCodes.Ldloc_0);
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
                                                              TypeBuilder typeBuilder,
                                                              FieldInfo fieldTarget)
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

            // Load target
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldTarget);

            // Push call values onto stack
            localIndex = 1;

            for (int idx = 0; idx < parameters.Length; ++idx)
            {
                if (parameters[idx].IsOut || parameters[idx].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, localIndex++);
                else
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4_S, idx);
                    il.Emit(OpCodes.Ldelem_Ref);

                    if (parameters[idx].ParameterType.IsValueType || parameters[idx].ParameterType.IsGenericParameter)
                        il.Emit(OpCodes.Unbox_Any, parameters[idx].ParameterType);
                    else
                        il.Emit(OpCodes.Castclass, parameters[idx].ParameterType);
                }
            }

            // Call intercepted method
            il.Emit(OpCodes.Callvirt, methodToIntercept);

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
        /// <param name="interfaceToWrap">Original type.</param>
        /// <param name="module">Defines and represents a module in a dynamic assembly.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns></returns>
        protected override Type GenerateWrapperType(Type interfaceToWrap,
                                                    ModuleBuilder module,
                                                    string moduleName)
        {
            // Define implementing type
            TypeBuilder typeBuilder = module.DefineType(MakeTypeName(moduleName, interfaceToWrap), TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(SetupGenericClassArguments(interfaceToWrap, typeBuilder));

            // Declare fields for the proxy and the target
            FieldBuilder fieldProxy = typeBuilder.DefineField("proxy",
                                                              typeof(ILEmitProxy),
                                                              FieldAttributes.Private);
            FieldBuilder fieldTarget = typeBuilder.DefineField("target",
                                                               interfaceToWrap,
                                                               FieldAttributes.Private);

            // Create overrides (and delegates) for all the interface methods
            Dictionary<FieldBuilder, MethodInfo> overloadedMethods = new Dictionary<FieldBuilder, MethodInfo>();

            foreach (MethodInfo method in interfaceToWrap.GetMethods())
            {
                // Cache a copy of the MethodInfo for each method
                FieldBuilder fieldMethodInfo = typeBuilder.DefineField("methodInfo" + overloadedMethods.Count,
                                                                       typeof(MethodInfo),
                                                                       FieldAttributes.Private);

                GenerateOverloadedMethod(typeBuilder,
                                         method,
                                         GenerateOverloadedMethodDelegate(method, typeBuilder, fieldTarget),
                                         fieldProxy,
                                         fieldTarget,
                                         fieldMethodInfo);

                overloadedMethods[fieldMethodInfo] = method;
            }

            // Create a single constructor which takes the proxy and target, and gets
            // all the necessary MethodInfo objects for the class
            GenerateConstructor(typeBuilder, interfaceToWrap, fieldProxy, fieldTarget, overloadedMethods);

            return typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Creates wrapper on top of original type.
        /// </summary>
        /// <param name="interfaceToWrap">Original type.</param>
        /// <returns>New type.</returns>
        public static Type WrapInterface(Type interfaceToWrap)
        {
            return instance.Wrap(interfaceToWrap);
        }
    }
}