using System;
using System.Collections.Generic;
using System.Net.Mail;
using Codentia.Common.Helper;

namespace Codentia.Common.Logging.DL
{
    /// <summary>
    /// ILogWriter implementation which outputs to email messages
    /// </summary>
    public class EmailLogWriter : ILogWriter
    {
        private bool _isOpen;
        private SmtpClient _smtpClient;
        private string _logTarget = "alerts@code.je";

        #region ILogWriter Members

        /// <summary>
        /// Gets or sets the LogTarget (email address)
        /// </summary>
        public string LogTarget
        {
            get
            {
                return _logTarget;
            }

            set
            {
                ParameterCheckHelper.CheckIsValidEmailAddress(value, "LogTarget");

                _logTarget = value;
            }
        }

        /// <summary>
        /// Write a single message
        /// </summary>
        /// <param name="message">Message to write</param>
        public void Write(LogMessage message)
        {
            if (!_isOpen)
            {
                Open();
            }

            try
            {
                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress(_logTarget));
                msg.Subject = string.Format("Log {0} from {1}", message.Type.ToString(), message.Source);
                msg.Body = string.Format("{0} - {1} [{2}] {3}", message.Timestamp.ToString("yyyy/MM/dd HH:mm:ss"), message.Type.ToString(), message.Source, message.Message);

                _smtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                // all we can do is write out to the console and bail
                ConsoleLogWriter clw = new ConsoleLogWriter();
                clw.Write(new LogMessage(LogMessageType.FatalError, "EmailLogWriter", string.Format("Failed to write message: {0}", ex.Message)));
                clw.Dispose();
                clw.Close();
            }
        }

        /// <summary>
        /// Write a set of messages
        /// </summary>
        /// <param name="messages">Array of messages to write</param>
        public void Write(LogMessage[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                Write(messages[i]);
            }
        }

        /// <summary>
        /// Open Smtp connection / initialise smtp client
        /// </summary>
        public void Open()
        {
            if (!_isOpen)
            {
                _smtpClient = new SmtpClient();
                _isOpen = true;
            }
        }

        /// <summary>
        /// Close down
        /// </summary>
        public void Close()
        {
            if (_isOpen)
            {
                _isOpen = false;
            }
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="rollOverSizeKB">The parameter is not used.</param>
        /// <param name="rollOverFileLimit">The parameter is not used.</param>
        /// <param name="retentionDates">The parameter is not used.</param>
        public void CleanUp(int rollOverSizeKB, int rollOverFileLimit, Dictionary<LogMessageType, DateTime> retentionDates)
        {
            throw new System.NotImplementedException("EmailLogWriter does not support CleanUp.");
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// This is not used
        /// </summary>
        public void Dispose()
        {
            if (_isOpen)
            {
                Close();
            }

            _smtpClient = null;
        }

        #endregion
    }
}
