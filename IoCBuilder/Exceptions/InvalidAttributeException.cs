using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace IoCBuilder.Exceptions
{
    /// <summary>
    /// Indicates that an invalid combination of dependency injection attributes were used.
    /// </summary>
    [Serializable]
    public class InvalidAttributeException : Exception
    {
        /// <summary>
        /// Initializes the exception.
        /// </summary>
        public InvalidAttributeException()
        {
        }

        /// <summary>
        /// Initializes the exception.
        /// </summary>
        /// <param name="message">Error Message</param>
        public InvalidAttributeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes the exception.
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="exception">Inner Exception</param>
        public InvalidAttributeException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Initializes the exception.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberName"></param>
        public InvalidAttributeException(Type type, string memberName)
            : base(String.Format(CultureInfo.CurrentCulture, IoCBuilder.Resources.ResourceManager.GetString("InvalidAttributeCombination"), type, memberName))
        {
        }

        /// <summary>
        /// Initializes the exception.
        /// </summary>
        protected InvalidAttributeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}