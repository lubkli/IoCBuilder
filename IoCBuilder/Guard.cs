using System;
using System.Globalization;
using System.Reflection;
using IoCBuilder.Exceptions;

namespace IoCBuilder
{
    internal static class Guard
    {
        public static void TypeIsAssignableFromType(Type assignee, Type providedType, Type classBeingBuilt)
        {
            if (!assignee.IsAssignableFrom(providedType))
                throw new IncompatibleTypesException(string.Format(CultureInfo.CurrentCulture,
                                                                   Resources.ResourceManager.GetString("TypeNotCompatible"), assignee, providedType, classBeingBuilt));
        }

        public static void ValidateMethodParameters(MethodBase methodInfo, object[] parameters, Type typeBeingBuilt)
        {
            ParameterInfo[] paramInfos = methodInfo.GetParameters();

            for (int i = 0; i < paramInfos.Length; i++)
                if (parameters[i] != null)
                    Guard.TypeIsAssignableFromType(paramInfos[i].ParameterType, parameters[i].GetType(), typeBeingBuilt);
        }

        public static void ArgumentNotNull(object argumentValue,
                                           string argumentName)
        {
            if (argumentValue == null) throw new ArgumentNullException(argumentName);
        }

        public static void ArgumentNotNullOrEmpty(string argumentValue,
                                                  string argumentName)
        {
            if (argumentValue == null) throw new ArgumentNullException(argumentName);
            if (argumentValue.Length == 0) throw new ArgumentException("The provided string argument must not be empty", argumentName);
        }
    }
}