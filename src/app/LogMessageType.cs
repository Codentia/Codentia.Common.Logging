namespace Codentia.Common.Logging
{
    /// <summary>
    /// Types of Message which can be logged.
    /// </summary>
    public enum LogMessageType
    {
        /// <summary>
        /// Lowest level message - informational
        /// </summary>
        Information = 1,
        
        /// <summary>
        /// Warning level - an error or exception requiring attention but which is not fatal to normal operations
        /// </summary>
        NonFatalError = 2,
        
        /// <summary>
        /// Highest level message - a critical or system-breaking error
        /// </summary>
        FatalError = 3,

        /// <summary>
        /// Information message denoting a request hit
        /// </summary>
        UrlRequest = 4
    }
}