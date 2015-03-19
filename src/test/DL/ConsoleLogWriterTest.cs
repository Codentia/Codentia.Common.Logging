using System;
using NUnit.Framework;

namespace Codentia.Common.Logging.DL.Test
{
    /// <summary>
    /// Unit testing framework for ConsoleLogWriter
    /// </summary>
    [TestFixture]
    public class ConsoleLogWriterTest
    {
        /// <summary>
        /// Scenario: Object constructed and disposed
        /// Expected: Process completes without error
        /// </summary>
        [Test]
        public void _001_ConstructAndDispose()
        {
            // without opening writer
            ConsoleLogWriter writer = new ConsoleLogWriter();
            writer.Dispose();

            // with opening writer
            writer = new ConsoleLogWriter();
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
            ConsoleLogWriter writer = new ConsoleLogWriter();
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
            ConsoleLogWriter writer = new ConsoleLogWriter();
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
            ConsoleLogWriter writer = new ConsoleLogWriter();
            LogMessage[] msgs = new LogMessage[]
            {
                new LogMessage(LogMessageType.Information, "Test004", "This is a test message"),
                new LogMessage(LogMessageType.Information, "Test004", "This is another test message")
            };

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
            ConsoleLogWriter writer = new ConsoleLogWriter();
            LogMessage[] msgs = new LogMessage[]
            {
                new LogMessage(LogMessageType.Information, "Test004", "This is a test message"),
                new LogMessage(LogMessageType.Information, "Test004", "This is another test message")
            };

            writer.Open();
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
            ConsoleLogWriter writer = new ConsoleLogWriter();
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
            ConsoleLogWriter writer = new ConsoleLogWriter();
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
            ConsoleLogWriter writer = new ConsoleLogWriter();
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
            ConsoleLogWriter writer = new ConsoleLogWriter();
            writer.Open();
            writer.Open();
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: WriteMessage called when LogTarget is invalid
        /// Expected: System.NotImplementedException(LogTarget property not supported by ConsoleLogWriter)
        /// </summary>
        [Test]
        public void _010_Write_InvalidLogTarget()
        {
            LogMessage msg = new LogMessage(LogMessageType.Information, "Test002", "This is a test message");
            ConsoleLogWriter writer = new ConsoleLogWriter();

            Assert.That(delegate { writer.LogTarget = null; }, Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo("LogTarget is not supported by ConsoleLogWriter"));
            Assert.That(delegate { Console.Out.WriteLine(writer.LogTarget); }, Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo("LogTarget is not supported by ConsoleLogWriter"));

            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Call the CleanUp method from ILogWriter.
        /// Expected: Exception(xxxLogWriter does not support CleanUp.)
        /// </summary>
        [Test]
        public void _011_CleanUp()
        {
            ConsoleLogWriter writer = new ConsoleLogWriter();
            Assert.That(delegate { writer.CleanUp(1, 1, null); }, Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo("ConsoleLogWriter does not support CleanUp."));
        }
    }
}
