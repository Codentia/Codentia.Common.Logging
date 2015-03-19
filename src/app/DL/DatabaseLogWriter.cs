using System;
using System.Collections.Generic;
using System.Data;
using Codentia.Common.Data;

namespace Codentia.Common.Logging.DL
{
    /// <summary>
    /// ILogWriter implementation which outputs to database
    /// </summary>
    public class DatabaseLogWriter : ILogWriter
    {
        #region ILogWriter Members
        private string _logTarget = string.Empty;

        /// <summary>
        /// Gets or sets log Target
        /// </summary>
        public string LogTarget
        {
            get
            {
                return _logTarget;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception("Cannot set LogTarget as null or empty");
                }

                _logTarget = value;
            }
        }

        /// <summary>
        /// Write a single message
        /// </summary>
        /// <param name="message">Message to write</param>
        public void Write(LogMessage message)
        {
            if (string.IsNullOrEmpty(_logTarget))
            {
                throw new Exception("LogTarget has not been set");
            }

            if (message is UrlAccessMessage)
            {
                UrlAccessMessage urlMessage = (UrlAccessMessage)message;

                DbParameter[] parameters = new DbParameter[]
                {
                    new DbParameter("@LogStamp", DbType.DateTime,  urlMessage.Timestamp),
		            new DbParameter("@Languages", DbType.String, urlMessage.Languages),
		            new DbParameter("@HostAddress", DbType.StringFixedLength, 15, urlMessage.HostAddress),
		            new DbParameter("@RequestUrl", DbType.String, urlMessage.Url),
		            new DbParameter("@ReferralUrl", DbType.String, urlMessage.ReferreralUrl),
		            new DbParameter("@Browser", DbType.StringFixedLength, 100, urlMessage.Browser),
		            new DbParameter("@BrowserMajorVersion", DbType.Int32, urlMessage.BrowserMajorVersion),
		            new DbParameter("@BrowserMinorVersion", DbType.StringFixedLength, 10, urlMessage.BrowserMinorVersion)
                };

                DbInterface.ExecuteProcedureNoReturn(_logTarget, "dbo.SystemLog_WriteUrlAccessMessage", parameters);
            }
            else
            {
                DbParameter[] parameters = new DbParameter[]
                {
                    new DbParameter("@LogStamp", DbType.DateTime, message.Timestamp),
                    new DbParameter("@LogMessageType", DbType.Int32, Convert.ToInt32(message.Type)),
                    new DbParameter("@Source", DbType.StringFixedLength, 200, message.Source),
                    new DbParameter("@Message", DbType.String,  message.Message)
                };

                DbInterface.ExecuteProcedureNoReturn(_logTarget, "dbo.SystemLog_WriteSingleMessage", parameters);
            }
        }

        /// <summary>
        /// Write a set of messages
        /// </summary>
        /// <param name="messages">Messages to write</param>
        public void Write(LogMessage[] messages)
        {
            if (string.IsNullOrEmpty(_logTarget))
            {
                throw new Exception("LogTarget has not been set");
            }

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
        /// Perform cleanup activities
        /// </summary>
        /// <param name="rollOverSizeKB">The parameter is not used.</param>
        /// <param name="rollOverFileLimit">The parameter is not used.</param>
        /// <param name="retentionDates">Dictionary stating the date/time from which to keep data for each LogMessageType</param>
        public void CleanUp(int rollOverSizeKB, int rollOverFileLimit, Dictionary<LogMessageType, DateTime> retentionDates)
        {
            if (string.IsNullOrEmpty(_logTarget))
            {
                throw new Exception("LogTarget has not been set");
            }

            Dictionary<LogMessageType, DateTime>.Enumerator types = retentionDates.GetEnumerator();
            while (types.MoveNext())
            {
                if (types.Current.Key == LogMessageType.UrlRequest)
                {
                    DbParameter[] salParams = new DbParameter[]
                    {
                        new DbParameter("@KeepFromDate", DbType.DateTime,  types.Current.Value)
                    };

                    DbInterface.ExecuteProcedureNoReturn(_logTarget, "dbo.SystemAccessLog_CleanUp", salParams);
                }
                else
                {
                    DbParameter[] salParams = new DbParameter[]
                    {
                        new DbParameter("@LogMessageType", DbType.Int32, Convert.ToInt32(types.Current.Key)),
                        new DbParameter("@KeepFromDate", DbType.DateTime, types.Current.Value)
                    };

                    DbInterface.ExecuteProcedureNoReturn(_logTarget, "dbo.SystemLog_CleanUp", salParams);
                }
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Not supported
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }
}
