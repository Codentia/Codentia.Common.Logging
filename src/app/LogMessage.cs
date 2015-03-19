using System;

namespace Codentia.Common.Logging
{
    /// <summary>
    /// Data Structure to encapsulate a message which has been queued for logging
    /// </summary>
    [Serializable]
    public class LogMessage
    {
        private LogMessageType _type;
        private string _source;
        private string _message;
        private DateTime _timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="type">The type of message</param>
        /// <param name="source">The source of message</param>
        /// <param name="message">The content of message.</param>
        public LogMessage(LogMessageType type, string source, string message)
        {
            _type = type;
            _source = source;
            _message = message;
            _timestamp = DateTime.Now;
        }

        /// <summary>
        /// Gets the Type of the message
        /// </summary>
        public LogMessageType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the Source of the message
        /// </summary>
        public string Source
        {
            get
            {
                return _source;
            }
        }

        /// <summary>
        /// Gets the Message of the message
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Gets the Timestamp of the message
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }
        }
    }
}