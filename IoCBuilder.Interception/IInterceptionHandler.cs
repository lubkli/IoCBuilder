namespace IoCBuilder.Interception
{
    /// <summary>
    /// Interception method call
    /// </summary>
    /// <param name="call">Current call</param>
    /// <param name="getNext">Next one</param>
    /// <returns></returns>
    public delegate IMethodReturn InvokeHandlerDelegate(IMethodInvocation call,
                                                        GetNextHandlerDelegate getNext);

    /// <summary>
    /// Next interception method call
    /// </summary>
    /// <returns></returns>
    public delegate InvokeHandlerDelegate GetNextHandlerDelegate();

    /// <summary>
    /// Interception handler used by pipeline
    /// </summary>
    public interface IInterceptionHandler
    {
        /// <summary>
        /// Interception method
        /// </summary>
        /// <param name="call">Current call</param>
        /// <param name="getNext">Next one</param>
        /// <returns></returns>
        IMethodReturn Invoke(IMethodInvocation call,
                             GetNextHandlerDelegate getNext);
    }
}