using System;
using System.Collections.Generic;
using System.Web;
using Codentia.Common.Data;
using Codentia.Test.Helper;
using NUnit.Framework;

namespace Codentia.Common.Logging.DL.Test
{
    /// <summary>
    /// Unit testing framework for DatabaseLogWriter
    /// </summary>
    [TestFixture]
    public class DatabaseLogWriterTest
    {
        private string _logTargetName = "logging_sql";

        /// <summary>
        /// Prepare for testing
        /// </summary>
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {            
        }

        /// <summary>
        /// Scenario: Object constructed and disposed
        /// Expected: Process completes without error
        /// </summary>
        [Test]
        public void _001_ConstructAndDispose()
        {
            // without opening writer
            DatabaseLogWriter writer = new DatabaseLogWriter();
            writer.Dispose();

            // with opening writer
            writer = new DatabaseLogWriter();
            writer.Open();
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Single message written - object not opened first
        /// Expected: Object opens and then writes message
        /// </summary>
        [Test]
        public void _002_SingleMessage_NotOpen()
        {
            LogMessage msg = new LogMessage(LogMessageType.Information, "Test002", "This is a test message");
            DatabaseLogWriter writer = new DatabaseLogWriter();
            writer.LogTarget = _logTargetName;
            writer.Write(msg);

            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Message written, object is open
        /// Expected: Writes message
        /// </summary>
        [Test]
        public void _003_SingleMessage_Open()
        {
            LogMessage msg = new LogMessage(LogMessageType.Information, "Test003", "This is a test message");
            DatabaseLogWriter writer = new DatabaseLogWriter();
            writer.LogTarget = _logTargetName;
            writer.Open();
            writer.Write(msg);

            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: set of messages written - object not opened first
        /// Expected: Object opens and then writes messages
        /// </summary>
        [Test]
        public void _004_MultiMessage_NotOpen()
        {
            DatabaseLogWriter writer = new DatabaseLogWriter();
            LogMessage[] msgs = new LogMessage[]
            {
                new LogMessage(LogMessageType.Information, "Test004", "This is a test message"),
                new LogMessage(LogMessageType.Information, "Test004", "This is another test message")
            };

            writer.LogTarget = _logTargetName;
            writer.Write(msgs);

            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Messages written when object is open
        /// Expected: Completes without error
        /// </summary>
        [Test]
        public void _005_MultiMessage_Open()
        {
            DatabaseLogWriter writer = new DatabaseLogWriter();
            LogMessage[] msgs = new LogMessage[]
            {
                new LogMessage(LogMessageType.Information, "Test004", "This is a test message"),
                new LogMessage(LogMessageType.Information, "Test004", "This is another test message")
            };

            writer.Open();
            writer.LogTarget = _logTargetName;
            writer.Write(msgs);
            
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Objected closed when not open
        /// Expected: No effect
        /// </summary>
        [Test]
        public void _006_Close_NotOpen()
        {
            DatabaseLogWriter writer = new DatabaseLogWriter();
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Object closed when open
        /// Expected: Completes without error
        /// </summary>
        [Test]
        public void _007_Close_Open()
        {
            DatabaseLogWriter writer = new DatabaseLogWriter();
            writer.Open();
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Object opened when not open
        /// Expected: Completes without error
        /// </summary>
        [Test]
        public void _008_Open_NotOpen()
        {
            DatabaseLogWriter writer = new DatabaseLogWriter();
            writer.Open();
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Objected opened when already open
        /// Expected: No effect
        /// </summary>
        [Test]
        public void _009_Open_Open()
        {
            DatabaseLogWriter writer = new DatabaseLogWriter();
            writer.Open();
            writer.Open();
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Test all that methods that trap for empty LogTarget
        /// Expected: System.NotImplementedException(LogTarget cannot be null or empty string)
        /// </summary>
        [Test]
        public void _010_Write_InvalidLogTarget()
        {
            LogMessage msg = new LogMessage(LogMessageType.Information, "Test002", "This is a test message");
            DatabaseLogWriter writer = new DatabaseLogWriter();

            Assert.That(delegate { writer.LogTarget = null; }, Throws.InstanceOf<Exception>().With.Message.EqualTo("Cannot set LogTarget as null or empty"));
            Assert.That(delegate { writer.LogTarget = string.Empty; }, Throws.InstanceOf<Exception>().With.Message.EqualTo("Cannot set LogTarget as null or empty"));
            Assert.That(delegate { writer.Write(msg); }, Throws.InstanceOf<Exception>().With.Message.EqualTo("LogTarget has not been set"));                  

            DateTime writeDateTime = DateTime.Now;
            Dictionary<LogMessageType, DateTime> cleanUpParams = new Dictionary<LogMessageType, DateTime>();
            cleanUpParams[LogMessageType.FatalError] = writeDateTime.AddMilliseconds(500);
            cleanUpParams[LogMessageType.Information] = writeDateTime.AddMilliseconds(500);
            cleanUpParams[LogMessageType.NonFatalError] = writeDateTime.AddMilliseconds(500);
            cleanUpParams[LogMessageType.UrlRequest] = writeDateTime.AddMilliseconds(500);

            Assert.That(delegate { writer.CleanUp(0, 0, cleanUpParams); }, Throws.InstanceOf<Exception>().With.Message.EqualTo("LogTarget has not been set"));                  

            writer.Close();
            writer.Dispose();

            DatabaseLogWriter writerMulti = new DatabaseLogWriter();
            LogMessage[] msgs = new LogMessage[]
            {
                new LogMessage(LogMessageType.Information, "Test004", "This is a test message"),
                new LogMessage(LogMessageType.Information, "Test004", "This is another test message")
            };

            Assert.That(delegate { writerMulti.Write(msgs); }, Throws.InstanceOf<Exception>().With.Message.EqualTo("LogTarget has not been set"));                  

            writerMulti.Close();
            writerMulti.Dispose();
        }

        /// <summary>
        /// Scenario: Attempt to write a UrlAccessMessage
        /// Expected: Message written with no error
        /// </summary>
        [Test]
        public void _011_UrlAccessMessage()
        {
            HttpContext x = HttpHelper.CreateHttpContext(string.Empty);
            UrlAccessMessage msg = new UrlAccessMessage(x.Request);
            DatabaseLogWriter writer = new DatabaseLogWriter();

            writer.LogTarget = _logTargetName;
            writer.Write((LogMessage)msg);

            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Run the cleanup method, giving a purge date (the date at which data will be kept from)
        /// Expected: Only data added after that date/time is kept
        /// </summary>
        [Test]
        public void _012_CleanUp()
        {
            // populate log
            DatabaseLogWriter writer = new DatabaseLogWriter();
            writer.LogTarget = _logTargetName;
            Assert.That(writer.LogTarget, Is.EqualTo(_logTargetName));
            writer.Write(new LogMessage(LogMessageType.Information, "Test", "Test message"));
            writer.Write(new LogMessage(LogMessageType.FatalError, "Test", "Test message"));
            writer.Write(new LogMessage(LogMessageType.NonFatalError, "Test", "Test message"));
            HttpContext x = HttpHelper.CreateHttpContext(string.Empty);
            UrlAccessMessage msg = new UrlAccessMessage(x.Request);
            writer.Write((LogMessage)msg);

            // record the time, pause for a second and write again
            DateTime writeDateTime = DateTime.Now;
            System.Threading.Thread.Sleep(1000);

            writer.Write(new LogMessage(LogMessageType.Information, "Test", "Test message"));
            writer.Write(new LogMessage(LogMessageType.FatalError, "Test", "Test message"));
            writer.Write(new LogMessage(LogMessageType.NonFatalError, "Test", "Test message"));
            msg = new UrlAccessMessage(x.Request);
            writer.Write((LogMessage)msg);

            writer.Close();
            writer.Dispose();

            writer = new DatabaseLogWriter();

            // now perform cleanup
            Dictionary<LogMessageType, DateTime> cleanUpParams = new Dictionary<LogMessageType, DateTime>();
            cleanUpParams[LogMessageType.FatalError] = writeDateTime.AddMilliseconds(500);
            cleanUpParams[LogMessageType.Information] = writeDateTime.AddMilliseconds(500);
            cleanUpParams[LogMessageType.NonFatalError] = writeDateTime.AddMilliseconds(500);
            cleanUpParams[LogMessageType.UrlRequest] = writeDateTime.AddMilliseconds(500);

            writer.LogTarget = _logTargetName;
            writer.CleanUp(0, 0, cleanUpParams);

            // ensure one record of each type exists
            Assert.That(DbInterface.ExecuteQueryScalar<int>("SELECT COUNT(*) FROM SystemLog WHERE LogMessageType = 1"), Is.EqualTo(1));
            Assert.That(DbInterface.ExecuteQueryScalar<int>("SELECT COUNT(*) FROM SystemLog WHERE LogMessageType = 2"), Is.EqualTo(1));
            Assert.That(DbInterface.ExecuteQueryScalar<int>("SELECT COUNT(*) FROM SystemLog WHERE LogMessageType = 3"), Is.EqualTo(1));
            Assert.That(DbInterface.ExecuteQueryScalar<int>("SELECT COUNT(*) FROM SystemAccessLog"), Is.EqualTo(1));

            writer.Close();
            writer.Dispose();
        }
    }
}
