namespace IoCBuilder.Strategies.BuilderAware
{
    /// <summary>
    /// Implemented on a class when it wants to receive notifications
    /// about the build process.
    /// </summary>
    public interface IBuilderAware
    {
        /// <summary>
        /// Called by the <see cref="BuilderAwareStrategy"/> when the object is being built up.
        /// </summary>
        /// <param name="buildKey">The build key of the object that was just built up.</param>
        void OnBuiltUp(object buildKey);

        /// <summary>
        /// Called by the <see cref="BuilderAwareStrategy"/> when the object is being torn down.
        /// </summary>
        void OnTearingDown();
    }
}