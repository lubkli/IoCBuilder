using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoCBuilder.Lifetime;
using IoCBuilder.Location;
using IoCBuilder.Strategies.Singleton;
using IoCBuilder.Strategies.TypeMapping;

namespace IoCBuilder
{
    /// <summary>
    /// An implementation of <see cref="IFactory"/>. This is class for the simplest usage of IoC/DI.
    /// Factory is facade for builder, which is used for detailed object creation and manipulation.
    /// </summary>
    public class Factory : IFactory
    {
        private Builder builder;
        private Locator locator;
        private ILifetimeContainer lifetime;

        /// <summary>
        /// Initialize a new instance of the <see cref="Factory"/> class.
        /// </summary>
        public Factory()
            : this(new Builder(), new Locator(), new LifetimeContainer())
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="Factory"/> class.
        /// </summary>
        /// <param name="builder">The builder, which is using as hard worker.</param>
        /// <param name="locator">The memory of builder used for object location. Builder remembers objects in the locator. Uses weak reference.</param>
        /// <param name="lifetime">The instance lifetime container, which prevents object disposing. Uses reference.</param>
        public Factory(Builder builder, Locator locator, ILifetimeContainer lifetime)
        {
            this.builder = builder;
            this.locator = locator;
            this.lifetime = lifetime;

            if (!locator.Contains(typeof(ILifetimeContainer)))
                locator.Add(typeof(ILifetimeContainer), lifetime);

            if (!locator.Contains(typeof(IFactory)))
                locator.Add(typeof(IFactory), this);
        }

        /// <summary>
        /// Returns object of required type.
        /// </summary>
        /// <param name="typeToBuild">Required type.</param>
        /// <returns>Created or retrieved and injected object.</returns>
        public object Get(Type typeToBuild)
        {
            return builder.BuildUp(locator, typeToBuild, null, null);
        }

        /// <summary>
        /// Returns object of required type.
        /// </summary>
        /// <typeparam name="TToBuild">Required type.</typeparam>
        /// <returns>Created or retrieved and injected object.</returns>
        public TToBuild Get<TToBuild>()
        {
            return (TToBuild)Get(typeof(TToBuild));
        }

        /// <summary>
        /// Performs dependency injection on <see cref="object"/>
        /// </summary>
        /// <param name="object">Object to be injected.</param>
        /// <returns>Injected object.</returns>
        public object Inject(object @object)
        {
            Guard.ArgumentNotNull(@object, "object");

            return builder.BuildUp(locator, @object.GetType(), null, @object);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void CacheInstancesOf<T>()
        {
            CacheInstancesOf(typeof(T));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="typeToCache"></param>
        public void CacheInstancesOf(Type typeToCache)
        {
            builder.Policies.Set<ISingletonPolicy>(new SingletonPolicy(true), new BuildKey(typeToCache));
        }

        /// <summary>
        /// Register object as singleton.
        /// </summary>
        /// <typeparam name="TTypeToRegisterAs">Type of singleton object.</typeparam>
        /// <param name="instance">Instance of singleton object.</param>
        public void RegisterSingletonInstance<TTypeToRegisterAs>(TTypeToRegisterAs instance)
        {
            RegisterSingletonInstance(typeof(TTypeToRegisterAs), instance);
        }

        /// <summary>
        /// Register object as singleton.
        /// </summary>
        /// <param name="typeToRegisterAs">Type of singleton object.</param>
        /// <param name="instance">Instance of singleton object.</param>
        public void RegisterSingletonInstance(Type typeToRegisterAs,
                                              object instance)
        {
            if (!typeToRegisterAs.IsInstanceOfType(instance))
                throw new ArgumentException("Object is not type compatible with registration type", "instance");

            locator.Add(new BuildKey(typeToRegisterAs), instance);
            lifetime.Add(instance);
        }

        /// <summary>
        /// Type/Type mapping registration is used as KeyValuePair with requested type and type,
        /// which will be build.
        /// </summary>
        /// <typeparam name="TRequested">Requested type.</typeparam>
        /// <typeparam name="TToBuild">Type, which will be build.</typeparam>
        public void RegisterTypeMapping<TRequested, TToBuild>()
        {
            RegisterTypeMapping(typeof(TRequested), typeof(TToBuild));
        }

        /// <summary>
        /// Type/Type mapping registration is used as KeyValuePair with requested type and type,
        /// which will be build.
        /// </summary>
        /// <param name="typeRequested">Requested type.</param>
        /// <param name="typeToBuild">Type, which will be build.</param>
        public void RegisterTypeMapping(Type typeRequested,
                                        Type typeToBuild)
        {
            TypeMappingPolicy policy = new TypeMappingPolicy(typeToBuild, nameof(typeRequested));
            builder.Policies.Set<ITypeMappingPolicy>(policy, new BuildKey(typeRequested));
        }

        /// <summary>
        /// Register strategy, which will be applied during object BuildUp and TearDown.
        /// </summary>
        /// <typeparam name="TStrategy">Strategy to be registered.</typeparam>
        /// <param name="stage">Builder stage when the strategy will be applied.</param>
        public void RegisterStrategy<TStrategy>(BuilderStage stage)
            where TStrategy : IBuilderStrategy, new()
        {
            builder.Strategies.AddNew<TStrategy>(stage);
        }

        /// <summary>
        /// Register strategy, which will be applied during object BuildUp and TearDown.
        /// </summary>
        /// <param name="strategy">Strategy to be registered.</param>
        /// <param name="stage">Builder stage when the strategy will be applied.</param>
        public void RegisterStrategy(IBuilderStrategy strategy,
                                     BuilderStage stage)
        {
            builder.Strategies.Add(strategy, stage);
        }

        /// <summary>
        /// Sets a default policy. When checking for a policy, if no specific individual policy
		/// is available, the default will be used.
        /// </summary>
        /// <param name="policyType">The interface to register the policy under.</param>
        /// <param name="policy">The default policy to be registered.</param>
        public void SetDefaultPolicy(Type policyType,
                                     IBuilderPolicy policy)
        {
            builder.Policies.SetDefault(policyType, policy);
        }

        /// <summary>
        /// Sets a default policy. When checking for a policy, if no specific individual policy
		/// is available, the default will be used.
        /// </summary>
        /// <typeparam name="TPolicy">The interface to register the policy under.</typeparam>
        /// <param name="policy">The default policy to be registered.</param>
        public void SetDefaultPolicy<TPolicy>(TPolicy policy)
            where TPolicy : IBuilderPolicy
        {
            builder.Policies.SetDefault(policy);
        }

        /// <summary>
        /// Sets an individual policy.
        /// </summary>
        /// <param name="policyType">The interface to register the policy under.</param>
        /// <param name="policy">The policy to be registered.</param>
        /// <param name="typeToRegisterFor">The type the policy applies to.</param>
        public void SetPolicy(Type policyType,
                              IBuilderPolicy policy,
                              Type typeToRegisterFor)
        {
            builder.Policies.Set(policyType, policy, new BuildKey(typeToRegisterFor));
        }

        /// <summary>
        /// Sets an individual policy.
        /// </summary>
        /// <typeparam name="TPolicy">The interface to register the policy under.</typeparam>
        /// <param name="policy">The policy to be registered.</param>
        /// <param name="typeToRegisterFor">The type the policy applies to.</param>
        public void SetPolicy<TPolicy>(TPolicy policy,
                                       Type typeToRegisterFor)
            where TPolicy : IBuilderPolicy
        {
            builder.Policies.Set(policy, new BuildKey(typeToRegisterFor));
        }

        /// <summary>
        /// Performs an unbuild operation.
        /// </summary>
        /// <typeparam name="TItem">Type of object to be unbuild.</typeparam>
        /// <param name="existingObject">Reference to the object.</param>
        /// <returns>Unbuild object.</returns>
        public TItem TearDown<TItem>(TItem existingObject)
        {
            return builder.TearDown<TItem>(locator, existingObject);
        }
    }
}