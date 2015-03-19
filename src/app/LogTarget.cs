namespace Codentia.Common.Logging
{
    /// <summary>
    /// Targets available for logging system
    /// </summary>
    public enum LogTarget
    {
        /// <summary>
        /// Log to SQL Database
        /// </summary>
        Database,

        /// <summary>
        /// Log to file on disk
        /// </summary>
        File,
        
        /// <summary>
        /// Send a copy of the message by email
        /// </summary>
        Email,
        
        /// <summary>
        /// Log to Console.Out (primarily for Unit Test usage)
        /// </summary>
        Console
    }
}