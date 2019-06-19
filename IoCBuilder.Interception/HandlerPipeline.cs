using System.Collections.Generic;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Pipeline of interception handlers
    /// </summary>
    public class HandlerPipeline
    {
        private readonly List<IInterceptionHandler> handlers;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlers">List of interception handlers</param>
        public HandlerPipeline(IEnumerable<IInterceptionHandler> handlers)
        {
            this.handlers = new List<IInterceptionHandler>(handlers);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlers">List of interception handlers</param>
        public HandlerPipeline(params IInterceptionHandler[] handlers)
        {
            this.handlers = new List<IInterceptionHandler>(handlers);
        }

        /// <summary>
        /// Invoke each interception handler in pipeline
        /// </summary>
        /// <param name="input">Target method</param>
        /// <param name="target">Interception method</param>
        /// <returns></returns>
        public IMethodReturn Invoke(IMethodInvocation input,
                                    InvokeHandlerDelegate target)
        {
            if (handlers.Count == 0)
                return target(input, null);

            int handlerIndex = 0;

            IMethodReturn result = handlers[0].Invoke(input, delegate
                                                             {
                                                                 ++handlerIndex;

                                                                 if (handlerIndex < handlers.Count)
                                                                     return handlers[handlerIndex].Invoke;
                                                                 else
                                                                     return target;
                                                             });
            return result;
        }
    }
}