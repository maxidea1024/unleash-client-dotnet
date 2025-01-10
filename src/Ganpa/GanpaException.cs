namespace Ganpa
{
    using System;
    using System.Runtime.Serialization;

    /// <inheritdoc />
    [Serializable]
    public class GanpaException : Exception
    {
        /// <inheritdoc />
        public GanpaException()
        {
        }

        /// <inheritdoc />
        public GanpaException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public GanpaException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <inheritdoc />
        protected GanpaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}