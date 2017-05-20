using System;
using System.Runtime.Serialization;

namespace Team7ADProjectMVC.Exceptions
{
    [Serializable]
    internal class ChangeCollectionPointException : Exception
    {
        public ChangeCollectionPointException()
        {
        }

        public ChangeCollectionPointException(string message) : base(message)
        {
        }

        public ChangeCollectionPointException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ChangeCollectionPointException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}