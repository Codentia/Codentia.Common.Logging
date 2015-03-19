using System;
using System.Configuration;
using NUnit.Framework;

namespace Codentia.Common.Logging.DL.Test
{
    /// <summary>
    /// Unit testing framework for EmailLogWriter
    /// </summary>
    [TestFixture]
    public class EmailLogWriterTest
    {
        /// <summary>
        /// Scenario: Object constructed and disposed
        /// Expected: Process completes without error
        /// </summary>
        [Test]
        public void _001_ConstructAndDispose()
        {
            // without opening writer
            EmailLogWriter writer = new EmailLogWriter();
            writer.Dispose();

            // with opening writer
            writer = new EmailLogWriter();
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
            EmailLogWriter writer = new EmailLogWriter();
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
            EmailLogWriter writer = new EmailLogWriter();
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
            EmailLogWriter writer = new EmailLogWriter();
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
            EmailLogWriter writer = new EmailLogWriter();
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
            EmailLogWriter writer = new EmailLogWriter();
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
            EmailLogWriter writer = new EmailLogWriter();
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
            EmailLogWriter writer = new EmailLogWriter();
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
            EmailLogWriter writer = new EmailLogWriter();
            writer.Open();
            writer.Open();
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: WriteMessage called when LogTarget is invalid
        /// Expected: Exception(Invalid LogTarget specified)
        /// </summary>
        [Test]
        public void _010_Write_InvalidLogTarget()
        {
            LogMessage msg = new LogMessage(LogMessageType.Information, "Test002", "This is a test message");
            EmailLogWriter writer = new EmailLogWriter();

            Assert.That(delegate { writer.LogTarget = null; }, Throws.InstanceOf<ArgumentException>().With.Message.EqualTo("LogTarget is not specified"));
            Assert.That(delegate { writer.LogTarget = string.Empty; }, Throws.InstanceOf<ArgumentException>().With.Message.EqualTo("LogTarget is not specified"));
            Assert.That(delegate { writer.LogTarget = "wibble"; }, Throws.InstanceOf<ArgumentException>().With.Message.EqualTo("LogTarget: wibble is not a valid email address"));

            writer.Close();
            writer.Dispose(); 
        }

        /// <summary>
        /// Scenario: LogTarget property updated and checked
        /// Expected: Value matches value input
        /// </summary>
        [Test]
        public void _011_LogTarget_Valid()
        {
            EmailLogWriter writer = new EmailLogWriter();
            writer.LogTarget = "matt@mattchedit.com";
            Assert.That(writer.LogTarget, Is.EqualTo("matt@mattchedit.com"));
        }

        /// <summary>
        /// Scenario: Writer created and disposed without being open
        /// Expected: Executes without error
        /// </summary>
        [Test]
        public void _012_Dispose()
        {
            EmailLogWriter writer = new EmailLogWriter();
            writer.Dispose();
            writer = new EmailLogWriter();
            writer.Open();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Call the CleanUp method from ILogWriter.
        /// Expected: Exception(xxxLogWriter does not support CleanUp.)
        /// </summary>
        [Test]
        public void _013_CleanUp()
        {
            EmailLogWriter writer = new EmailLogWriter();
            Assert.That(delegate { writer.CleanUp(1, 1, null); }, Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo("EmailLogWriter does not support CleanUp."));
        }

        /// <summary>
        /// Scenario: Call a configuration exception handler
        /// Expected: Writes to log file instead
        /// </summary>
        [Test]
        public void _014_WithFailureInWrite()
        {
            Configuration c = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            c.SectionGroups.Remove("system.net");
            c.Save(ConfigurationSaveMode.Modified, true);

            // c.SectionGroups[0].Sections[0];
            ConfigurationManager.RefreshSection("system.net");
                       
            EmailLogWriter writer = new EmailLogWriter();
            writer.Write(new LogMessage(LogMessageType.Information, "test", "test"));
        }
    }
}
