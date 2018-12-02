namespace IoCBuilder.Strategies.Singleton
{
    /// <summary>
    /// Implementation of <see cref="ISingletonPolicy"/>.
    /// </summary>
    public class SingletonPolicy : ISingletonPolicy
    {
        private bool isSingleton;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isSingleton">Whether the object should be a singleton.</param>
        public SingletonPolicy(bool isSingleton)
        {
            this.isSingleton = isSingleton;
        }

        /// <summary>
        /// See <see cref="ISingletonPolicy.IsSingleton"/> for more information.
        /// </summary>
        public bool IsSingleton
        {
            get { return isSingleton; }
        }
    }
}