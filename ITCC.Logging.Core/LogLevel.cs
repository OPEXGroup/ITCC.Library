namespace ITCC.Logging.Core
{
    /// <summary>
    ///     Possible log severity levels
    /// </summary>
    public enum LogLevel
    {
        None,
        /// <summary>
        ///     Application just crashes
        /// </summary>
        Critical,

        /// <summary>
        ///     Page can crash
        /// </summary>
        Error,

        /// <summary>
        ///     Thats really bad, but we can deal with it
        /// </summary>
        Warning,

        /// <summary>
        ///     Some important information
        /// </summary>
        Info,

        /// <summary>
        ///     Can be used for debug
        /// </summary>
        Debug,

        /// <summary>
        ///     Produces tons of events
        /// </summary>
        Trace
    }
}
