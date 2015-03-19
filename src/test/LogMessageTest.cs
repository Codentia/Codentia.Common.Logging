using System;
using NUnit.Framework;

namespace Codentia.Common.Logging.Types.Test
{
    /// <summary>
    /// Unit testing framework for LogMessage struct
    /// </summary>
    [TestFixture]
    public class LogMessageTest
    {
        /// <summary>
        /// Scenario: LogMessage class created with valid arguments
        /// Expected: Properties reflect input values
        /// </summary>
        [Test]
        public void _001_ConstructorAndProperties()
        {
            DateTime past = DateTime.Now.AddSeconds(-1);

            LogMessage lm = new LogMessage(LogMessageType.Information, "Test1", "Test message 1");
            Assert.That(lm.Type, Is.EqualTo(LogMessageType.Information), "Type incorrect");
            Assert.That(lm.Source, Is.EqualTo("Test1"), "Source incorrect");
            Assert.That(lm.Message, Is.EqualTo("Test message 1"), "Message incorrect");            
            Assert.That(lm.Timestamp, Is.GreaterThan(past), "Timestamp incorrect");

            LogMessage lm2 = new LogMessage(LogMessageType.NonFatalError, "Test2", "Test message 2");
            Assert.That(lm2.Type, Is.EqualTo(LogMessageType.NonFatalError), "Type incorrect");
            Assert.That(lm2.Source, Is.EqualTo("Test2"), "Source incorrect");
            Assert.That(lm2.Message, Is.EqualTo("Test message 2"), "Message incorrect");                                   
            Assert.That(lm2.Timestamp, Is.GreaterThan(past), "Timestamp incorrect");

            LogMessage lm3 = new LogMessage(LogMessageType.FatalError, "Test3", "Test message 3");
            Assert.That(lm3.Type, Is.EqualTo(LogMessageType.FatalError), "Type incorrect");
            Assert.That(lm3.Source, Is.EqualTo("Test3"), "Source incorrect");
            Assert.That(lm3.Message, Is.EqualTo("Test message 3"), "Message incorrect");
            Assert.That(lm3.Timestamp, Is.GreaterThan(past), "Timestamp incorrect");
        }
    }
}
