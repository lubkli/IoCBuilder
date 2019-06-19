using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Interceptor based on IL Emitting
    /// </summary>
    public abstract class ILEmitInterceptor
    {
        private static readonly AssemblyBuilder assemblyBuilder;
        private static readonly Dictionary<Type, Type> wrappers = new Dictionary<Type, Type>();

        static ILEmitInterceptor()
        {
            //assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("ILEmitDynamicTypes"), AssemblyBuilderAccess.RunAndSave);

            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            //if (Environment.GetEnvironmentVariable("TRACE")?.Equals("ON") ?? false)
            //    isTracing = true;
        }

        /// <summary>
        /// Generates new type as wrapper for base
        /// </summary>
        /// <param name="typeToWrap">Original type.</param>
        /// <param name="moduleBuilder">Defines and represents a module in a dynamic assembly.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns></returns>
        protected abstract Type GenerateWrapperType(Type typeToWrap,
                                                    ModuleBuilder moduleBuilder,
                                                    string moduleName);

        /// <summary>
        /// Returns new type name
        /// </summary>
        /// <param name="assemblyName">name of the assembly</param>
        /// <param name="typeToWrap">original type</param>
        /// <returns></returns>
        protected static string MakeTypeName(string assemblyName,
                                             Type typeToWrap)
        {
            return "{Wrappers}.ns" + assemblyName + "." + typeToWrap.Name;
        }

        private static GenericTypeParameterBuilder[] SetupGenericArguments(Type[] genericParameterTypes,
                                                                   DefineGenericParametersDelegate @delegate)
        {
            // Nothing to do if it's not generic
            if (genericParameterTypes.Length == 0)
                return null;

            // Extract parameter names
            string[] genericParameterNames = new string[genericParameterTypes.Length];
            for (int idx = 0; idx < genericParameterTypes.Length; idx++)
                genericParameterNames[idx] = genericParameterTypes[idx].Name;

            // Setup constraints on generic types (i.e., "where" clauses)
            GenericTypeParameterBuilder[] genericBuilders = @delegate(genericParameterNames);

            for (int idx = 0; idx < genericBuilders.Length; idx++)
            {
                genericBuilders[idx].SetGenericParameterAttributes(genericParameterTypes[idx].GenericParameterAttributes);

                //foreach (Type type in genericParameterTypes[idx].GetGenericParameterConstraints())
                //    genericBuilders[idx].SetBaseTypeConstraint(type);

                List<Type> interfaceConstraintList = null;

                foreach (Type type in genericParameterTypes[idx].GetGenericParameterConstraints())
                {
                    if (type.IsClass)
                        genericBuilders[idx].SetBaseTypeConstraint(type);
                    else
                    {
                        if (interfaceConstraintList == null)
                            interfaceConstraintList = new List<Type>();
                        interfaceConstraintList.Add(type);
                    }
                }

                if (interfaceConstraintList != null)
                    genericBuilders[idx].SetInterfaceConstraints(interfaceConstraintList.ToArray());
            }

            return genericBuilders;
        }

        /// <summary>
        /// Setup generic class arguments
        /// </summary>
        /// <param name="classToWrap">Original type.</param>
        /// <param name="typeBuilder">Defines and creates new instances of classes during run time.</param>
        /// <returns></returns>
        protected static Type SetupGenericClassArguments(Type classToWrap,
                                                         TypeBuilder typeBuilder)
        {
            GenericTypeParameterBuilder[] builders =
                SetupGenericArguments(classToWrap.GetGenericArguments(),
                                      delegate (string[] names)
                                      {
                                          return typeBuilder.DefineGenericParameters(names);
                                      });

            if (builders != null)
                return classToWrap.MakeGenericType(builders);

            return classToWrap;
        }

        /// <summary>
        /// Setup generic method arguments
        /// </summary>
        /// <param name="methodToIntercept">Provides information about methods and constructors.</param>
        /// <param name="methodBuilder">Defines and represents a method (or constructor) on a dynamic class.</param>
        /// <returns></returns>
        protected static Type[] SetupGenericMethodArguments(MethodBase methodToIntercept,
                                                            MethodBuilder methodBuilder)
        {
            Type[] arguments = methodToIntercept.GetGenericArguments();

            SetupGenericArguments(arguments,
                                  delegate (string[] names)
                                  {
                                      return methodBuilder.DefineGenericParameters(names);
                                  });

            return arguments;
        }

        /// <summary>
        /// Creates wrapper on top of original type.
        /// </summary>
        /// <param name="typeToWrap">Original type.</param>
        /// <returns>New type.</returns>
        protected Type Wrap(Type typeToWrap)
        {
            lock (wrappers)
            {
                Type actualClassToWrap = typeToWrap;

                if (typeToWrap.IsGenericType)
                    actualClassToWrap = typeToWrap.GetGenericTypeDefinition();

                if (!wrappers.ContainsKey(actualClassToWrap))
                {
                    string moduleName = Guid.NewGuid().ToString("N");
                    ModuleBuilder module = assemblyBuilder.DefineDynamicModule(moduleName + ".dll");
                    wrappers[actualClassToWrap] = GenerateWrapperType(actualClassToWrap, module, moduleName);
                }

                if (actualClassToWrap != typeToWrap)
                    return wrappers[actualClassToWrap].MakeGenericType(typeToWrap.GetGenericArguments());

                return wrappers[typeToWrap];
            }
        }

        private delegate GenericTypeParameterBuilder[] DefineGenericParametersDelegate(string[] parameterNames);
    }
}