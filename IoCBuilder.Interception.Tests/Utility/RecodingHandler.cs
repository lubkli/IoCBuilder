namespace IoCBuilder.Interception
{
    public class RecordingHandler : IInterceptionHandler
    {
        private readonly string message;

        public RecordingHandler()
        {
            message = "";
        }

        public RecordingHandler(string message)
        {
            this.message = string.Format(" ({0})", message);
        }

        public IMethodReturn Invoke(IMethodInvocation call,
                                    GetNextHandlerDelegate getNext)
        {
            Recorder.Records.Add("Before Method" + message);
            IMethodReturn result = getNext().Invoke(call, getNext);
            Recorder.Records.Add("After Method" + message);
            return result;
        }
    }
}