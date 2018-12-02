namespace IoCBuilder.Strategies.Trace
{
    /// <summary>
    /// Represents a tracing policy for ObjectBuilder.
    /// </summary>
    public interface IBuilderTracePolicy : IBuilderPolicy
    {
        /// <summary>
        /// Trace a message.
        /// </summary>
        /// <param name="format">Message format.</param>
        /// <param name="args">Message arguments.</param>
        void Trace(string format, params object[] args);
    }
}