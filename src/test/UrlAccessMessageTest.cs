using System.Web;
using Codentia.Test.Helper;
using NUnit.Framework;

namespace Codentia.Common.Logging.Types.Test
{
    /// <summary>
    /// Unit testing framework for LogMessage struct
    /// </summary>
    [TestFixture]
    public class UrlAccessMessageTest
    {
        /// <summary>
        /// Scenario: LogMessage class created with valid arguments
        /// Expected: Properties reflect input values
        /// </summary>
        [Test]
        public void _001_ConstructorAndProperties_NullRequestParams()
        {
            HttpContext x = HttpHelper.CreateHttpContext(string.Empty);

            UrlAccessMessage lm = new UrlAccessMessage(x.Request);
            Assert.That(lm.Message, Is.EqualTo("IP=, Url=http://a/a.aspx, Referrer=, Languages=, Browser=, Version=0.0"));
            Assert.That(lm.Browser, Is.EqualTo("Unknown"));
            Assert.That(lm.BrowserMajorVersion, Is.EqualTo(0));
            Assert.That(lm.BrowserMinorVersion, Is.EqualTo("0"));
            Assert.That(lm.Languages, Is.EqualTo(string.Empty));
            Assert.That(lm.ReferreralUrl, Is.EqualTo(string.Empty));
            Assert.That(lm.Url, Is.EqualTo("http://a/a.aspx"));
            Assert.That(lm.HostAddress, Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// Scenario: Test the internal method ProcessLanguage array
        /// Expected: null argument should return string.Empty
        /// </summary>
        [Test]
        public void _002_ProcessLanguageArray_Null()
        {
            Assert.That(UrlAccessMessage.ProcessLanguageArray(null), Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// Scenario: Test the internal method ProcessLanguage array
        /// Expected: Single element array argument should return that element as a string
        /// </summary>
        [Test]
        public void _003_ProcessLanguageArray_OneItem()
        {
            Assert.That(UrlAccessMessage.ProcessLanguageArray(new string[] { "en" }), Is.EqualTo("en"));
        }

        /// <summary>
        /// Scenario: Test the internal method ProcessLanguage array
        /// Expected: Multiple element array argument should return a semi-colon delimited string containing all elements
        /// </summary>
        [Test]
        public void _003_ProcessLanguageArray_N_Items()
        {
            Assert.That(UrlAccessMessage.ProcessLanguageArray(new string[] { "en", "fr" }), Is.EqualTo("en;fr"));
        }
    }
}
