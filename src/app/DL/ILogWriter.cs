using System;
using System.Collections.Generic;

namespace Codentia.Common.Logging.DL
{
    /// <summary>
    /// This interface defines the required structure for a class which wishes to write data to MIT logs
    /// </summary>
    public interface ILogWriter : IDisposable
    {
        /// <summary>
        /// Gets or sets the target parameter for the LogWriter
        /// </summary>
        string LogTarget
        {
            get;
            set;
        }

        /// <summary>
        /// Write a single message to the log
        /// </summary>
        /// <param name="message">Message to be written</param>
        void Write(LogMessage message);

        /// <summary>
        /// Write a set of messages to the log
        /// </summary>
        /// <param name="messages">Messages to be written</param>
        void Write(LogMessage[] messages);

        /// <summary>
        /// Prepare the LogWriter for use
        /// </summary>
        void Open();

        /// <summary>
        /// Finish all pending operations and prepare the LogWriter for disposal
        /// </summary>
        void Close();

        /// <summary>
        /// Perform Clean-Up operations
        /// </summary>
        /// <param name="rollOverSizeKB">Maximum size of data store before roll-over required</param>
        /// <param name="rollOverFileLimit">Maximum number of (old) rolled-over stores to be kept</param>
        /// <param name="retentionDates">Dictionary LogMessageType/DateTime detailing date from which to keep data by type</param>
        void CleanUp(int rollOverSizeKB, int rollOverFileLimit, Dictionary<LogMessageType, DateTime> retentionDates);
    }
}
