using System;

namespace IoCBuilder.Strategies.TypeMapping
{
    /// <summary>
    /// Implementation of <see cref="ITypeMappingPolicy"/> which does simple type/ID
    /// mapping.
    /// </summary>
    public class TypeMappingPolicy : ITypeMappingPolicy
    {
        private BuildKey pair;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMappingPolicy"/> class using
        /// the provided type and ID.
        /// </summary>
        /// <param name="type">The new type to be returned during Map.</param>
        /// <param name="id">The new ID to be returned during Map.</param>
        public TypeMappingPolicy(Type type, string id)
        {
            pair = new BuildKey(type, id);
        }

        /// <summary>
        /// See <see cref="ITypeMappingPolicy.Map"/> for more information.
        /// </summary>
        public object Map(object buildKey)
        {
            return pair;
        }
    }
}