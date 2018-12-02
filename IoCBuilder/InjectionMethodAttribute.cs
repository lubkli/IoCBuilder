using System;

namespace IoCBuilder
{
    /// <summary>
    /// Attribute to specify that dependency injection should call a method during the
    /// build-up of an object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InjectionMethodAttribute : Attribute
    {
    }
}