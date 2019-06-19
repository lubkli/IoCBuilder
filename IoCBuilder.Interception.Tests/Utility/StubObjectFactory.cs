using System;

namespace IoCBuilder.Interception
{
    public class StubObjectFactory : IFactory
    {
        public object Get(Type typeToBuild)
        {
            return Activator.CreateInstance(typeToBuild);
        }

        public TToBuild Get<TToBuild>()
        {
            return (TToBuild)Get(typeof(TToBuild));
        }

        public object Inject(object @object)
        {
            throw new NotImplementedException();
        }
    }
}