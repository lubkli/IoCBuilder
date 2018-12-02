namespace IoCBuilder.Strategies.BuilderAware
{
    /// <summary>
    /// Implementation of <see cref="IBuilderStrategy"/> which will notify an object about
    /// the completion of a <see cref="IBuilder{T}.BuildUp"/> operation, or start of a
    /// <see cref="IBuilder{T}.TearDown"/> operation.
    /// </summary>
    /// <remarks>
    /// This strategy checks the object that is passing through the builder chain to see if it
    /// implements IBuilderAware and if it does, it will call <see cref="IBuilderAware.OnBuiltUp"/>
    /// and <see cref="IBuilderAware.OnTearingDown"/>. This strategy is meant to be used from the
    /// <see cref="BuilderStage.PostInitialization"/> stage.
    /// </remarks>
    public class BuilderAwareStrategy : BuilderStrategy
    {
        /// <summary>
        /// See <see cref="IBuilderStrategy.BuildUp"/> for more information.
        /// </summary>
        public override object BuildUp(IBuilderContext context, object buildKey, object existing)
        {
            IBuilderAware awareObject = existing as IBuilderAware;

            if (awareObject != null)
            {
                TraceBuildUp(context, buildKey, Resources.ResourceManager.GetString("CallingOnBuiltUp"));
                awareObject.OnBuiltUp(buildKey);
            }

            return base.BuildUp(context, buildKey, existing);
        }

        /// <summary>
        /// See <see cref="IBuilderStrategy.TearDown"/> for more information.
        /// </summary>
        public override object TearDown(IBuilderContext context, object item)
        {
            IBuilderAware awareObject = item as IBuilderAware;

            if (awareObject != null)
            {
                TraceTearDown(context, item, Resources.ResourceManager.GetString("CallingOnTearingDown"));
                awareObject.OnTearingDown();
            }

            return base.TearDown(context, item);
        }
    }
}