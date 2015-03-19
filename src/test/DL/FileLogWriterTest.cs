using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Codentia.Common.Logging;
using Codentia.Common.Logging.DL;
using NUnit.Framework;

namespace Codentia.Common.Logging.DL.Test
{
    /// <summary>
    /// Unit testing framework for FileLogWriter
    /// </summary>
    [TestFixture]
    public class FileLogWriterTest
    {
        /// <summary>
        /// Prepare for testing activity
        /// </summary>
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            if (Directory.Exists(@"c:\log\test"))
            {
                Directory.Delete(@"c:\log\test", true);
            }
        }

        /// <summary>
        /// Scenario: Object constructed and disposed
        /// Expected: Process completes without error
        /// </summary>
        [Test]
        public void _001_ConstructAndDispose()
        {
            // without opening writer
            FileLogWriter writer = new FileLogWriter();
            writer.Dispose();

            // with opening writer
            writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
            writer.Open();
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: LogWriter constructed and opened without setting LogTarget
        /// Expected: Exception(Cannot start FileLogWriter without setting LogTarget)
        /// </summary>
        [Test]
        public void _001b_ConstructAndOpen_NoLogTarget()
        {
            FileLogWriter writer = new FileLogWriter();
            Assert.That(delegate { writer.Open(); }, Throws.InstanceOf<Exception>().With.Message.EqualTo("Cannot start FileLogWriter without setting LogTarget"));           
        }

        /// <summary>
        /// Scenario: Single message written - object not opened first
        /// Expected: Object opens and then writes message
        /// </summary>
        [Test]
        public void _002_SingleMessage_NotOpen()
        {
            LogMessage msg = new LogMessage(LogMessageType.Information, "Test002", "This is a test message");
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
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
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
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
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
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
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
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
            FileLogWriter writer = new FileLogWriter();
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
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
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
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
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
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
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
            FileLogWriter writer = new FileLogWriter();

            Assert.That(delegate { writer.LogTarget = null; }, Throws.InstanceOf<Exception>().With.Message.EqualTo("Invalid LogTarget specified"));
            Assert.That(delegate { writer.LogTarget = string.Empty; }, Throws.InstanceOf<Exception>().With.Message.EqualTo("Invalid LogTarget specified"));
            
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
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = "SystemLog2.txt";
            Assert.That(writer.LogTarget, Is.EqualTo("SystemLog2.txt"));
        }

        /// <summary>
        /// Scenario: Writer created and disposed without being open
        /// Expected: Executes without error
        /// </summary>
        [Test]
        public void _012_Dispose_NotOpen()
        {
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
            writer.Dispose();

            writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl.txt";
            writer.Open();
            writer.Dispose();
        }

        /// <summary>
        /// Scenario: Create a log file and cause it to roll-over (cleanup)
        /// Expected: File renamed, new file created
        /// </summary>
        [Test]
        public void _013_CleanUp_SingleRollOver()
        {
            // create a writer
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl_cleanup13.txt";
            writer.Open();

            // put some data in the log
            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));

            writer.CleanUp(0, 5, null);

            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.Close();
            writer.Dispose();

            // now test output
            Assert.That(File.Exists(@"c:\log\test\loggingdl_cleanup13.txt"), Is.True);
            Assert.That(File.Exists(@"c:\log\test\loggingdl_cleanup13.txt_1"), Is.True);
            Assert.That(File.Exists(@"c:\log\test\loggingdl_cleanup13.txt_2"), Is.False);
        }

        /// <summary>
        /// Scenario: Create several log files by use of roll-over. Ensure that maximum file count does not exceed given limit.
        /// Expected: Files are deleted once they are beyond the roll-over limit.
        /// </summary>
        [Test]
        public void _014_CleanUp_RollOverLimit()
        {
            // create a writer
            FileLogWriter writer = new FileLogWriter();
            writer.LogTarget = @"c:\log\test\loggingdl_cleanup14.txt";
            writer.Open();

            // put some data in the log
            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.CleanUp(0, 3, null);

            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.CleanUp(0, 3, null);

            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.CleanUp(0, 3, null);

            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.CleanUp(0, 3, null);

            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.CleanUp(0, 3, null);

            writer.Write(new LogMessage(LogMessageType.Information, "Test", "This is a test message."));
            writer.Close();
            writer.Dispose();

            // now test output
            Assert.That(File.Exists(@"c:\log\test\loggingdl_cleanup14.txt"), Is.True);
            Assert.That(File.Exists(@"c:\log\test\loggingdl_cleanup14.txt_1"), Is.True);
            Assert.That(File.Exists(@"c:\log\test\loggingdl_cleanup14.txt_2"), Is.True);
            Assert.That(File.Exists(@"c:\log\test\loggingdl_cleanup14.txt_3"), Is.True);
            Assert.That(File.Exists(@"c:\log\test\loggingdl_cleanup14.txt_4"), Is.False);
        }

        /// <summary>
        /// Scenario: Open a file as a log target which cannot be accessed.
        /// Expected: Retry logic called, exception stating that streams cannot be opened is thrown.
        /// </summary>
        [Test]
        public void _015_InaccessibleFile()
        {
            FileLogWriter flw = new FileLogWriter();
            flw.LogTarget = @"c:\log\test\fileWithContention.txt";

            FileStream fs = new FileStream(flw.LogTarget, FileMode.OpenOrCreate);

            Assert.That(delegate { flw.Open(); }, Throws.Exception.With.InnerException.TypeOf<System.IO.IOException>().And.Message.EqualTo("Unable to open output streams"));
            
            fs.Close();
            fs.Dispose();
        }

        /// <summary>
        /// Scenario: CleanUp is called on a FileLogWriter when LogTarget does not exist
        /// Expected: No error occurs, no changes are made to file system.
        /// </summary>
        [Test]
        public void _016_CleanUp_LogTarget_DoesNotExist()
        {
            FileLogWriter flw = new FileLogWriter();

            flw.LogTarget = @"c:\log\test\ThisFileIsNotThere.txt";
            flw.CleanUp(1, 1, null);
        }

        /// <summary>
        /// Scenario: LogTarget is set, including the ~APP~ tag
        /// Expected: ~APP~ replaced with application base directory
        /// </summary>
        [Test]
        public void _017_LogTarget_WithAPPTag()
        {
            FileLogWriter flw = new FileLogWriter();

            flw.LogTarget = @"_APP_\Test.txt";
            DirectoryInfo di = new DirectoryInfo(".");
            Assert.That(flw.LogTarget, Is.EqualTo(string.Format(@"{0}\Test.txt", di.FullName)));
        }
    }
}
