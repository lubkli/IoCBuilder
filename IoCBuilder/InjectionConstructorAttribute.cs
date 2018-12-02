using System;

namespace IoCBuilder
{
    /// <summary>
    /// Attribute to specify which constructor on an object will be used for dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class InjectionConstructorAttribute : Attribute
    {
    }
}