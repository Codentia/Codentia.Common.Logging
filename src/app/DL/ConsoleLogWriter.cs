using System;
using System.Collections.Generic;

namespace Codentia.Common.Logging.DL
{
    /// <summary>
    /// ILogWriter implementation which writes to Console.Out
    /// </summary>
    public class ConsoleLogWriter : ILogWriter
    {
        #region ILogWriter Members

        /// <summary>
        /// Gets or sets the LogTarget (unused)
        /// </summary>
        public string LogTarget
        {
            get
            {
                throw new System.NotImplementedException("LogTarget is not supported by ConsoleLogWriter");
            }

            set
            {
                throw new System.NotImplementedException("LogTarget is not supported by ConsoleLogWriter");
            }
        }

        /// <summary>
        /// Write a single message
        /// </summary>
        /// <param name="message">Message to write</param>
        public void Write(LogMessage message)
        {
            Console.Out.WriteLine(string.Format("{0} - {1} [{2}] {3}", message.Timestamp.ToString("yyyy/MM/dd HH:mm:ss"), message.Type.ToString(), message.Source, message.Message));
        }

        /// <summary>
        /// Write a set of messages
        /// </summary>
        /// <param name="messages">Messages to write (array)</param>
        public void Write(LogMessage[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                Write(messages[i]);
            }
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public void Open()
        {
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="rollOverSizeKB">The parameter is not used.</param>
        /// <param name="rollOverFileLimit">The parameter is not used.</param>
        /// <param name="retentionDates">The parameter is not used.</param>
        public void CleanUp(int rollOverSizeKB, int rollOverFileLimit, Dictionary<LogMessageType, DateTime> retentionDates)
        {
            throw new System.NotImplementedException("ConsoleLogWriter does not support CleanUp.");
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// This is not used
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }
}
