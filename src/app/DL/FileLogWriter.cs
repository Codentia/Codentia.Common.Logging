using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Codentia.Common.Logging;

namespace Codentia.Common.Logging.DL
{
    /// <summary>
    /// FileLogWriter implements ILogWriter and provides a file-based data store for logging.
    /// </summary>
    public class FileLogWriter : ILogWriter
    {
        private static object _fileLock = new object();
        
        private bool _isOpen = false;
        private FileStream _outputStream = null;
        private StreamWriter _outputWriter = null;
        private string _logTarget = null;

        #region ILogWriter Members

        /// <summary>
        /// Gets or sets the LogTarget (file path + name) 
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
                    throw new Exception("Invalid LogTarget specified");
                }

                if (value.Contains("_APP_"))
                {                    
                    // value = value.Replace("~APP~", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    value = value.Replace("_APP_", Environment.CurrentDirectory);
                }

                // ensure path exists here
                string[] parts = value.Split(@"\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string pathSoFar = string.Empty;

                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Contains("."))
                    {
                        break;
                    }
                    else
                    {
                        pathSoFar = string.Format("{0}{1}{2}", pathSoFar, pathSoFar.Length > 0 ? @"\" : string.Empty, parts[i]);

                        if (!Directory.Exists(pathSoFar))
                        {
                            Directory.CreateDirectory(pathSoFar);
                        }
                    }
                }

                _logTarget = value;
            }
        }

        /// <summary>
        /// Write a given message to the log.
        /// </summary>
        /// <param name="message">Message to be written</param>
        public void Write(LogMessage message)
        {
            lock (_fileLock)
            {
                if (!_isOpen)
                {
                    Open();
                }

                _outputWriter.WriteLine(string.Format("{0} - {1} [{2}] {3}", message.Timestamp.ToString("yyyy/MM/dd HH:mm:ss"), message.Type.ToString(), message.Source, message.Message));
            }
        }

        /// <summary>
        /// Write a set of messages to the log, one after the other (in incremental order)
        /// </summary>
        /// <param name="messages">Array of messages to be written, in order</param>
        public void Write(LogMessage[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                Write(messages[i]);
            }
        }

        /// <summary>
        /// Open this LogWriter
        /// </summary>
        public void Open()
        {
            if (_logTarget == null)
            {
                throw new Exception("Cannot start FileLogWriter without setting LogTarget");
            }

            if (!_isOpen)
            {
                OpenWithRetry();
            }
        }

        /// <summary>
        /// Close this LogWriter
        /// </summary>
        public void Close()
        {
            if (_isOpen)
            {
                _outputWriter.Flush();
                _outputWriter.Close();
                _outputStream.Close();

                _isOpen = false;
            }
        }

        /// <summary>
        /// Perform clean-up of files. Roll over when maximum size is exceeded, delete when number of roll-overs is exceeded.
        /// </summary>
        /// <param name="rollOverSizeKB">File size at which to consider roll-over</param>
        /// <param name="rollOverFileLimit">Number of roll-over files to be kept (newest first)</param>
        /// <param name="retentionDates">This is not used</param>
        public void CleanUp(int rollOverSizeKB, int rollOverFileLimit, Dictionary<LogMessageType, DateTime> retentionDates)
        {
            lock (_fileLock)
            {
                Close();

                // is roll-over required?
                if (File.Exists(this.LogTarget))
                {
                    FileInfo fi = new FileInfo(this.LogTarget);
                    if ((Convert.ToDecimal(fi.Length) / 1024.0m) > Convert.ToDecimal(rollOverSizeKB))
                    {
                        // we need to roll-over
                        // check if we need to purge other files
                        string path = string.Format("{0}{1}", Path.GetDirectoryName(this.LogTarget), @"\");
                        string filePattern = this.LogTarget.Substring(this.LogTarget.LastIndexOf(@"\") + 1, this.LogTarget.Length - (this.LogTarget.LastIndexOf(@"\") + 1));
                        string[] files = Directory.GetFiles(path, string.Format("{0}_*", filePattern));

                        // too many files do or will exist
                        while (files.Length >= rollOverFileLimit)
                        {
                            int maxFile = 0;

                            // delete any out of range files
                            for (int i = 0; i < files.Length; i++)
                            {
                                string[] current = files[i].Split("_".ToCharArray(), StringSplitOptions.None);
                                int file = Convert.ToInt32(current[current.Length - 1]);

                                if (file > maxFile)
                                {
                                    maxFile = file;
                                }
                            }

                            // delete the highest found
                            if (maxFile > 0)
                            {
                                File.Delete(string.Format("{0}{1}_{2}", path, filePattern, maxFile));
                            }

                            files = Directory.GetFiles(path, string.Format("{0}_*", filePattern));
                        }

                        // now shuffle file names
                        files = Directory.GetFiles(path, string.Format("{0}_*", filePattern));

                        List<string> fileList = new List<string>();
                        fileList.AddRange(files);

                        while (File.Exists(string.Format("{0}{1}_1", path, filePattern)))
                        {
                            int maxFile = 0;
                            int maxFileIndex = 0;

                            // delete any out of range files
                            for (int j = 0; j < fileList.Count; j++)
                            {
                                string[] current = fileList[j].Split("_".ToCharArray(), StringSplitOptions.None);
                                int file = Convert.ToInt32(current[current.Length - 1]);

                                if (file > maxFile)
                                {
                                    maxFile = file;
                                    maxFileIndex = j;
                                }
                            }

                            if (maxFile > 0)
                            {
                                File.Move(string.Format("{0}{1}_{2}", path, filePattern, maxFile), string.Format("{0}{1}_{2}", path, filePattern, maxFile + 1));
                                fileList.RemoveAt(maxFileIndex);
                            }
                        }

                        File.Move(this.LogTarget, string.Format("{0}{1}_{2}", path, filePattern, 1));
                    }
                }

                Open();
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose this LogWriter
        /// </summary>
        public void Dispose()
        {
            if (_isOpen)
            {
                Close();
            }

            if (_outputStream != null)
            {
                _outputWriter.Dispose();
                _outputStream.Dispose();

                _outputWriter = null;
                _outputStream = null;
            }
        }

        #endregion

        private void OpenWithRetry()
        {
            OpenWithRetry(0);
        }

        private void OpenWithRetry(int counter)
        {
            try
            {
                _outputStream = new FileStream(_logTarget, FileMode.OpenOrCreate);
                _outputWriter = new StreamWriter(_outputStream);
                _outputWriter.BaseStream.Seek(0, SeekOrigin.End);

                _isOpen = true;
            }
            catch (Exception ex)
            {
                if (counter < 5)
                {
                    System.Threading.Thread.Sleep(10);
                    counter++;
                    OpenWithRetry(counter);
                }
                else
                {
                    throw new Exception("Unable to open output streams", ex);
                }
            }
        }        
    }
}
