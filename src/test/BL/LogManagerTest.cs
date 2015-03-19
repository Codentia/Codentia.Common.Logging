using System;
using System.Net;
using System.Web;
using Codentia.Test.Helper;
using NUnit.Framework;

namespace Codentia.Common.Logging.BL.Test
{
    /// <summary>
    /// TestFixture for LogManager Singleton class
    /// <seealso cref="LogManager"/>
    /// </summary>
    [TestFixture]
    public class LogManagerTest
    {
        /// <summary>
        /// Close down the log manager - tidy up after testing
        /// </summary>
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogManager.Instance.Dispose();
        }

        /// <summary>
        /// Scenario: Object created.
        /// Expected: Multiple instances returned are references to the same object
        /// </summary>
        [Test]
        public void _001_Singleton_Instantiation()
        {
            LogManager x1 = LogManager.Instance;
            LogManager x2 = LogManager.Instance;

            Assert.That(x1, Is.Not.Null, "Instance is null");
            Assert.That(x1, Is.SameAs(x2), "Two instances differ");
        }

        /// <summary>
        /// Scenario: Object disposed
        /// Expected: After disposal, a reference to a fresh instance is obtained, class flushes data before disposing
        /// </summary>
        [Test]
        public void _002_Dispose()
        {
            LogManager x1 = LogManager.Instance;
            LogManager.Instance.Dispose();
            LogManager x2 = LogManager.Instance;

            Assert.That(x2, Is.Not.Null, "Instance is null following disposal");
            ////Assert.That(x2, Is.Not.SameAs(x1), "Instances match after disposal - should not");
        }

        /// <summary>
        /// Scenario: Message written from an exception
        /// Expected: Manual verification required
        /// </summary>
        [Test]
        public void _004_AddToLog_Exception_StringSource()
        {
            try
            {
                Convert.ToInt32("NaN");
                Assert.Fail("No exception raised");
            }
            catch (Exception ex)
            {
                LogManager.Instance.AddToLog(ex, "004 Source");
            }
        }

        /// <summary>
        /// Scenario: Message written from details
        /// Expected: Manual verification required
        /// </summary>
        [Test]
        public void _005_AddToLog_Details_StringSource()
        {
            LogManager.Instance.AddToLog(LogMessageType.Information, "005 Source", "Information message (sql)");
            LogManager.Instance.AddToLog(LogMessageType.NonFatalError, "005 Source", "Warning message (sql)");
            LogManager.Instance.AddToLog(LogMessageType.FatalError, "005 Source", "Error message (sql)");

            System.Threading.Thread.Sleep(250);
        }

        /// <summary>
        /// Scenario: Object should safely flush all messages to disk
        /// Expected: Manual verification required
        /// </summary>
        [Test]
        public void _006_Flush()
        {
            LogManager.Instance.AddToLog(LogMessageType.Information, "006 Source", "Information message (sql)");
            LogManager.Instance.AddToLog(LogMessageType.NonFatalError, "006 Source", "Warning message (sql)");
            LogManager.Instance.AddToLog(LogMessageType.FatalError, "006 Source", "Error message (sql)");

            LogManager.Instance.Flush();
        }

        /// <summary>
        /// Scenario: Large number of information messages created
        /// Expected: Completes without error
        /// </summary>
        [Test]
        public void _007_LargeVolume_Information()
        {
            for (int i = 0; i < 998; i++)
            {
                LogManager.Instance.AddToLog(LogMessageType.Information, "007 Source", "Information message");
            }

            LogManager.Instance.Flush();
        }

        /// <summary>
        /// Scenario: Repeat the above test, but with a 5s pause before flushing (and smaller volume) to allow internal delivery methods to handle messages
        /// Expected: Completes without error
        /// </summary>
        [Test]
        public void _008_LargeVolume_Information_WithPause()
        {
            for (int i = 0; i < 75; i++)
            {
                LogManager.Instance.AddToLog(LogMessageType.Information, "008 Source", "Information message");
            }

            System.Threading.Thread.Sleep(5000);

            LogManager.Instance.Flush();
        }

        /// <summary>
        /// Scenario: Enqueue a UrlAccessMessage indirectly (via an HttpRequest object)
        /// Expected: Completes (and flushes) without error
        /// </summary>
        [Test]
        public void _009_AddToLog_HttpRequest()
        {
            HttpContext context = HttpHelper.CreateHttpContext(string.Empty);

            LogManager.Instance.AddToLog(new UrlAccessMessage(context.Request));

            LogManager.Instance.Flush();
        }

        /// <summary>
        /// Scenario: Add a WebException to the queue
        /// Expected: Completes and flushes without error
        /// </summary>
        [Test]
        public void _010_AddToLog_WebException()
        {
            // we need a web request which throws an exception, but has a response stream
            // e.g. manages to connect, then gets 401 (unauthorized) or similar
            // so try to connect to taskmanager without any credentials
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("http://wiki.mattchedit.com/");

            try
            {
                HttpWebResponse hrs = (HttpWebResponse)hwr.GetResponse();
                Assert.Fail("No exception raised");
            }
            catch (WebException ex)
            {
                LogManager.Instance.AddToLog(ex, this.ToString());
            }
        }
    }
}
