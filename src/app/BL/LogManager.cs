using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Web;
using Codentia.Common.Logging.DL;

namespace Codentia.Common.Logging.BL
{
    /// <summary>
    /// Singleton class responsible for managing logging. From specification:
    ///  <para></para>        
    ///     * Creates and manages message queue
    ///     * Exposes methods for logging messages
    ///     * Exception
    ///     * Message by elements (e.g. source, etc)
    ///     * Source can be passed in as an object or a string
    ///     * Will flush messages out upon disposal
    ///     * Responsible for log-roll-over and creation of folders if absent
    ///  <para></para>
    /// <seealso cref="LogMessage"/>
    /// </summary>
    public sealed class LogManager : MarshalByRefObject, IDisposable
    {
        private static LogManager _instance;
        private static object _instanceLock = new object();
        private static object _queueLock = new object();
        private static object _writerLock = new object();

        private static int _portNumber = LogManager.RemotingPort;

        private Thread _writerThread;
        private Thread _cleanupThread;
        private bool _running;

        private Dictionary<LogMessageType, LogTarget[]> _mappings = new Dictionary<LogMessageType, LogTarget[]>();
        private Dictionary<LogMessageType, string[]> _mappingEmail = new Dictionary<LogMessageType, string[]>();
        private Dictionary<LogMessageType, string> _mappingFilename = new Dictionary<LogMessageType, string>();
        private Dictionary<LogMessageType, string> _mappingDatabaseName = new Dictionary<LogMessageType, string>();

        private List<LogMessage> _messageQueue;

        private bool _autoCleanUpDatabase = false;
        private int _databaseRetentionDays = 0;

        private bool _autoCleanUpFile = false;
        private int _fileRetentionSizeKB = 0;
        private int _fileRetentionCount = 0;

        private LogManager()
        {
            // load mapping configuration
            NameValueCollection targetMappings = (NameValueCollection)ConfigurationManager.GetSection("Codentia.Common.Logging/TargetMapping");

            if (targetMappings != null)
            {
                for (int i = 0; i < targetMappings.AllKeys.Length; i++)
                {
                    LogMessageType current = (LogMessageType)Enum.Parse(typeof(LogMessageType), targetMappings.AllKeys[i], true);
                    string[] targets = Convert.ToString(targetMappings[current.ToString()]).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    List<LogTarget> targetList = new List<LogTarget>();
                    List<string> emailList = new List<string>();
                    string alternativeFile = null;
                    string databaseName = null;

                    for (int j = 0; j < targets.Length; j++)
                    {
                        if (targets[j].StartsWith("email", StringComparison.CurrentCultureIgnoreCase))
                        {
                            targetList.Add(LogTarget.Email);
                            emailList.Add(targets[j].Substring(targets[j].IndexOf("~") + 1));
                        }
                        else
                        {
                            if (targets[j].StartsWith("file", StringComparison.CurrentCultureIgnoreCase))
                            {
                                targetList.Add(LogTarget.File);

                                if (targets[j].Contains("~"))
                                {
                                    alternativeFile = targets[j].Substring(targets[j].IndexOf("~") + 1);
                                }
                            }
                            else
                                if (targets[j].StartsWith("database", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    targetList.Add(LogTarget.Database);

                                    if (targets[j].Contains("~"))
                                    {
                                        databaseName = targets[j].Substring(targets[j].IndexOf("~") + 1);
                                    }
                                }
                                else
                                {
                                    targetList.Add((LogTarget)Enum.Parse(typeof(LogTarget), targets[j], true));
                                }
                        }
                    }

                    _mappings.Add(current, targetList.ToArray());

                    if (targetList.Contains(LogTarget.Email))
                    {
                        if (emailList.Count > 0)
                        {
                            _mappingEmail.Add(current, emailList.ToArray());
                        }
                    }

                    if (targetList.Contains(LogTarget.File))
                    {
                        if (!string.IsNullOrEmpty(alternativeFile))
                        {
                            _mappingFilename.Add(current, alternativeFile);
                        }
                        else
                        {
                            _mappingFilename.Add(current, "SystemLog.txt");
                        }
                    }

                    if (targetList.Contains(LogTarget.Database))
                    {
                        _mappingDatabaseName.Add(current, databaseName);
                    }
                }
            }

            // load retention/clean-up configuration
            NameValueCollection databaseRetention = (NameValueCollection)ConfigurationManager.GetSection("Codentia.Common.Logging/RetentionPolicy/Database");
            NameValueCollection fileRetention = (NameValueCollection)ConfigurationManager.GetSection("Codentia.Common.Logging/RetentionPolicy/File");

            if (databaseRetention != null)
            {
                _autoCleanUpDatabase = Convert.ToBoolean(databaseRetention["AutoCleanUp"]);
                _databaseRetentionDays = Convert.ToInt32(databaseRetention["RetainDays"]);
            }

            if (fileRetention != null)
            {
                _autoCleanUpFile = Convert.ToBoolean(fileRetention["AutoCleanUp"]);
                _fileRetentionSizeKB = Convert.ToInt32(fileRetention["RollOverSizeKB"]);
                _fileRetentionCount = Convert.ToInt32(fileRetention["RollOverFileCount"]);
            }

            // now set up remaining objects
            _running = true;
            _messageQueue = new List<LogMessage>();

            if (_autoCleanUpDatabase || _autoCleanUpFile)
            {
                _cleanupThread = new Thread(new ThreadStart(CleanUp));
                _cleanupThread.Start();
            }

            _writerThread = new Thread(new ThreadStart(WriteMessages));
            _writerThread.Start();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static LogManager Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    // safety check
                    if (_instance != null)
                    {
                        try
                        {
                            _instance.CheckConnectionIsValid();
                        }
                        catch (Exception)
                        {
                            _instance = null;
                        }
                    }

                    if (_instance == null)
                    {
                        TcpChannel channel;
                        try
                        {
                            IDictionary props = (IDictionary)new Hashtable();
                            props["port"] = _portNumber;
                            props["name"] = System.AppDomain.CurrentDomain.FriendlyName;

                            // Creating a custom formatter for a TcpChannel sink chain.
                            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
                            provider.TypeFilterLevel = TypeFilterLevel.Full;
                            channel = new TcpChannel(props, null, provider);
                            ChannelServices.RegisterChannel(channel, false);
                            RemotingConfiguration.RegisterWellKnownServiceType(typeof(LogManager), "LogManager", WellKnownObjectMode.Singleton);
                            _instance = (LogManager)Activator.GetObject(typeof(LogManager), string.Format("tcp://localhost:{0}/LogManager", _portNumber));
                        }
                        catch (SocketException)
                        {
                            _instance = (LogManager)Activator.GetObject(typeof(LogManager), string.Format("tcp://localhost:{0}/LogManager", _portNumber));
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Gets the get remoting port.
        /// </summary>
        private static int RemotingPort
        {
            get
            {
                // generate a unique, predictable port number by hashing the current app domain and seeding the RNG with it
                Random r = new Random(System.AppDomain.CurrentDomain.FriendlyName.GetHashCode());
                int port = r.Next(11000, 21999);
                return port;
            }
        }

        /// <summary>
        /// Checks the connection is valid.
        /// This is a dummy method - calls to it will throw an exception on the remote end (which is used in the instance property above), if invalid.
        /// </summary>
        /// <returns>bool - true</returns>
        public bool CheckConnectionIsValid()
        {
            return true;
        }

        /// <summary>
        /// Add a message to the logging queue
        /// </summary>
        /// <param name="request">HttpRequest to log</param>
        public void AddToLog(UrlAccessMessage request)
        {
            AddMessageToQueue(request);
        }

        /// <summary>
        /// Add a message to the logging queue
        /// </summary>
        /// <param name="exception">Exception to be logged</param>
        /// <param name="source">Source of the exception</param>
        public void AddToLog(Exception exception, string source)
        {
            UrlAccessMessage httpMessage = null;
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                httpMessage = new UrlAccessMessage(HttpContext.Current.Request);
            }

            AddMessageToQueue(new LogMessage(LogMessageType.FatalError, source, string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", exception.Source, System.Environment.NewLine, exception.Message, System.Environment.NewLine, exception.StackTrace, System.Environment.NewLine, exception.TargetSite, httpMessage != null ? string.Concat(System.Environment.NewLine, httpMessage.Message) : string.Empty)));
        }

        /// <summary>
        /// Add a message to the logging queue
        /// </summary>
        /// <param name="exception">Exception to be logged</param>
        /// <param name="source">Source of the exception</param>
        public void AddToLog(WebException exception, string source)
        {
            string errorResponse = string.Empty;

            if (exception.Response != null)
            {
                StreamReader sr = new StreamReader(exception.Response.GetResponseStream());
                errorResponse = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
            }

            AddMessageToQueue(new LogMessage(LogMessageType.FatalError, source, string.Format("Response={0}, Message={1}, StackTrace={2}", errorResponse, exception.Message, System.Environment.NewLine, exception.StackTrace)));
        }

        /// <summary>
        /// Add a message to the logging queue
        /// </summary>
        /// <param name="type">Type of message</param>
        /// <param name="source">Source of message</param>
        /// <param name="message">Content of message</param>
        public void AddToLog(LogMessageType type, string source, string message)
        {
            AddMessageToQueue(new LogMessage(type, source, message));
        }

        /// <summary>
        /// Force all queued messages to be written out immediately
        /// </summary>
        public void Flush()
        {
            lock (_queueLock)
            {
                if (_messageQueue.Count > 0)
                {
                    for (int i = 0; i < _messageQueue.Count; i++)
                    {
                        WriteMessage(_messageQueue[i]);
                    }

                    _messageQueue.Clear();
                    _messageQueue.TrimExcess();
                }
            }
        }

        /// <summary>
        /// Stop processing and clean up the object immediately
        /// </summary>
        public void Dispose()
        {
            Flush();

            lock (_instanceLock)
            {
                _running = false;

                if (_writerThread != null)
                {
                    _writerThread.Join();
                }

                if (_cleanupThread != null)
                {
                    _cleanupThread.Join();
                }

                _instance = null;
            }
        }

        private void WriteMessages()
        {
            while (_running)
            {
                LogMessage[] messagesToWrite = null;

                lock (_queueLock)
                {
                    _messageQueue.TrimExcess();

                    if (_messageQueue.Count > 0)
                    {
                        if (_messageQueue.Count < 10)
                        {
                            messagesToWrite = _messageQueue.ToArray();
                            _messageQueue.Clear();
                            _messageQueue.TrimExcess();
                        }
                        else
                        {
                            messagesToWrite = new LogMessage[10];
                            _messageQueue.CopyTo(0, messagesToWrite, 0, 10);
                            _messageQueue.RemoveRange(0, 10);
                            _messageQueue.TrimExcess();
                        }
                    }
                }

                // write messages here
                if (messagesToWrite != null)
                {
                    for (int i = 0; i < messagesToWrite.Length; i++)
                    {
                        WriteMessage(messagesToWrite[i]);
                    }
                }

                Thread.Sleep(50);
            }
        }

        private void WriteMessage(LogMessage message)
        {
            lock (_writerLock)
            {
                if (_mappings.ContainsKey(message.Type))
                {
                    LogTarget[] targets = _mappings[message.Type];
                    string[] emailAddresses = null;

                    if (_mappingEmail.ContainsKey(message.Type))
                    {
                        emailAddresses = _mappingEmail[message.Type];
                    }

                    int emailPointer = 0;

                    for (int i = 0; i < targets.Length; i++)
                    {
                        switch (targets[i])
                        {
                            case LogTarget.Console:
                                ConsoleLogWriter consoleWriter = new ConsoleLogWriter();
                                consoleWriter.Write(message);
                                consoleWriter.Dispose();
                                break;
                            case LogTarget.Database:
                                DatabaseLogWriter databaseWriter = new DatabaseLogWriter();
                                databaseWriter.LogTarget = _mappingDatabaseName[message.Type];
                                databaseWriter.Write(message);
                                databaseWriter.Dispose();
                                break;
                            case LogTarget.Email:
                                EmailLogWriter emailWriter = new EmailLogWriter();
                                emailWriter.LogTarget = emailAddresses[emailPointer];
                                emailWriter.Write(message);
                                emailWriter.Dispose();
                                emailPointer++;
                                break;
                            case LogTarget.File:
                                FileLogWriter fileWriter = new FileLogWriter();
                                fileWriter.LogTarget = _mappingFilename[message.Type];
                                fileWriter.Write(message);
                                fileWriter.Dispose();
                                break;
                        }
                    }
                }
            }
        }

        private void AddMessageToQueue(LogMessage message)
        {
            lock (_queueLock)
            {
                _messageQueue.Add(message);
            }
        }

        private void CleanUp()
        {
            while (_running)
            {
                if (_autoCleanUpDatabase)
                {
                    lock (_writerLock)
                    {
                        Dictionary<LogMessageType, DateTime> retentionDates = new Dictionary<LogMessageType, DateTime>();
                        retentionDates[LogMessageType.FatalError] = DateTime.Now.AddDays(-1 * _databaseRetentionDays);
                        retentionDates[LogMessageType.NonFatalError] = DateTime.Now.AddDays(-1 * _databaseRetentionDays);
                        retentionDates[LogMessageType.Information] = DateTime.Now.AddDays(-1 * _databaseRetentionDays);
                        retentionDates[LogMessageType.UrlRequest] = DateTime.Now.AddDays(-1 * _databaseRetentionDays);

                        Dictionary<LogMessageType, string>.Enumerator enumMappings = _mappingDatabaseName.GetEnumerator();
                        while (enumMappings.MoveNext())
                        {
                            DatabaseLogWriter writer = new DatabaseLogWriter();
                            writer.LogTarget = enumMappings.Current.Value;
                            writer.CleanUp(0, 0, retentionDates);
                            writer.Close();
                            writer.Dispose();
                        }
                    }
                }

                if (_autoCleanUpFile)
                {
                    lock (_writerLock)
                    {
                        IEnumerator<LogMessageType> files = _mappingFilename.Keys.GetEnumerator();
                        while (files.MoveNext())
                        {
                            FileLogWriter writer = new FileLogWriter();
                            writer.LogTarget = _mappingFilename[files.Current];
                            writer.CleanUp(_fileRetentionSizeKB, _fileRetentionCount, null);
                            writer.Close();
                            writer.Dispose();
                        }
                    }
                }

                for (int i = 0; i < 300 && _running; i++)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
