using System.Reflection;

namespace IoCBuilder.Interception
{
    internal class OutputParameterCollection : ParameterCollection
    {
        public OutputParameterCollection(object[] arguments,
                                         ParameterInfo[] parameters)
            : base(arguments,
                   parameters,
                   delegate (ParameterInfo info)
                   {
                       return info.IsOut;
                   })
        { }
    }
}