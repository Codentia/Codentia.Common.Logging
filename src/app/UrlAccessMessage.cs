using System;
using System.Text;
using System.Web;

namespace Codentia.Common.Logging
{
    /// <summary>
    /// Data Structure to encapsulate a url access message which has been queued for logging
    /// </summary>
    [Serializable]
    public class UrlAccessMessage : LogMessage
    {
        private string _languages;
        private string _hostAddress;
        private string _url;
        private string _browser;
        private int _browserMajorVersion = 0;
        private double _browserMinorVersion = 0.0d;
        private string _referralUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlAccessMessage"/> class.
        /// </summary>
        /// <param name="request">Request object to be logged</param>
        public UrlAccessMessage(HttpRequest request)
            : base(LogMessageType.UrlRequest, "HttpRequest", string.Empty)
        {
            if (request != null)
            {
                _languages = ProcessLanguageArray(request.UserLanguages);

                _hostAddress = request.UserHostAddress;
                _url = request.Url.ToString();

                _browser = request.Browser == null ? null : request.Browser.Browser;

                _referralUrl = string.Empty;
                
                try
                {
                    _browserMajorVersion = request.Browser == null || request.Browser.Version == null || request.Browser.Version.StartsWith(".") ? 0 : request.Browser.MajorVersion;
                    _browserMinorVersion = request.Browser == null || request.Browser.Version == null ? 0.0d : request.Browser.MinorVersion;
                    _referralUrl = request.UrlReferrer != null ? request.UrlReferrer.ToString() : string.Empty;
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }

        /// <summary>
        /// Gets the Message of the message
        /// </summary>
        public new string Message
        {
            get
            {
                return string.Format("IP={0}, Url={1}, Referrer={2}, Languages={3}, Browser={4}, Version={5}.{6}", _hostAddress, _url, _referralUrl, _languages, _browser, _browserMajorVersion, _browserMinorVersion);
            }
        }

        /// <summary>
        /// Gets the set of languages supported by the request
        /// </summary>
        public string Languages
        {
            get
            {
                return _languages;
            }
        }

        /// <summary>
        /// Gets the IP Address of the request
        /// </summary>
        public string HostAddress
        {
            get
            {
                return string.IsNullOrEmpty(_hostAddress) ? string.Empty : _hostAddress;
            }
        }

        /// <summary>
        /// Gets the Url requested
        /// </summary>
        public string Url
        {
            get
            {
                return _url;
            }
        }

        /// <summary>
        /// Gets the referring Url (if any)
        /// </summary>
        public string ReferreralUrl
        {
            get
            {
                return _referralUrl;
            }
        }

        /// <summary>
        ///  Gets the browser name
        /// </summary>
        public string Browser
        {
            get
            {
                return string.IsNullOrEmpty(_browser) ? "Unknown" : _browser;
            }
        }

        /// <summary>
        /// Gets the browser major version
        /// </summary>
        public int BrowserMajorVersion
        {
            get
            {
                return _browserMajorVersion;
            }
        }

        /// <summary>
        /// Gets the browser minor version
        /// </summary>
        public string BrowserMinorVersion
        {
            get
            {
                return _browserMinorVersion.ToString();
            }
        }

        /// <summary>
        /// Process Language Array
        /// </summary>
        /// <param name="languages">language array</param>
        /// <returns>string of array</returns>
        internal static string ProcessLanguageArray(string[] languages)
        {
            StringBuilder output = new StringBuilder();

            if (languages != null)
            {
                for (int i = 0; i < languages.Length; i++)
                {
                    if (i > 0)
                    {
                        output.Append(";");
                    }

                    output.Append(languages[i]);
                }
            }

            return output.ToString();
        }
    }
}