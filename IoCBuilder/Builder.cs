using IoCBuilder.Strategies.BuilderAware;
using IoCBuilder.Strategies.Creation;
using IoCBuilder.Strategies.Method;
using IoCBuilder.Strategies.Property;
using IoCBuilder.Strategies.Singleton;
using IoCBuilder.Strategies.TypeMapping;

namespace IoCBuilder
{
    /// <summary>
    /// An implementation of <see cref="IBuilder{TStageEnum}"/> which uses <see cref="BuilderStage"/>
    /// as the stages of the build process. It contains all the default strategies shipped
    /// with ObjectBuilder.
    /// </summary>
    public class Builder : BuilderBase<BuilderStage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> class.
        /// </summary>
        public Builder()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> class using the provided
        /// configurator.
        /// </summary>
        /// <param name="configurator">The configurator that will configure the builder.</param>
        public Builder(IBuilderConfigurator<BuilderStage> configurator)
        {
            Strategies.AddNew<TypeMappingStrategy>(BuilderStage.TypeMapping);

            Strategies.AddNew<SingletonStrategy>(BuilderStage.Lifetime);

            Strategies.AddNew<ConstructorReflectionStrategy>(BuilderStage.PreCreation);
            Strategies.AddNew<PropertyReflectionStrategy>(BuilderStage.PreCreation);
            Strategies.AddNew<MethodReflectionStrategy>(BuilderStage.PreCreation);

            Strategies.AddNew<CreationStrategy>(BuilderStage.Creation);

            Strategies.AddNew<PropertySetterStrategy>(BuilderStage.Initialization);
            Strategies.AddNew<MethodCallStrategy>(BuilderStage.Initialization);

            Strategies.AddNew<BuilderAwareStrategy>(BuilderStage.PostInitialization);

            Policies.SetDefault<ICreationPolicy>(new DefaultCreationPolicy());

            if (configurator != null)
                configurator.ApplyConfiguration(this);
        }
    }
}