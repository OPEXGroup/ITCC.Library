namespace ITCC.HTTP.Common
{
    /// <summary>
    ///     Simple container for all library constants related to HTTP
    /// </summary>
    public static class Constants
    {
        /// <summary>
        ///     Resolution multipliers for video and image compression
        /// </summary>
        public static readonly double[] ResolutionMultipliers = { 0.1, 0.25, 0.5, 0.75, 1.0 };

        /// <summary>
        ///     Files preprocessor will insert this string into changed files' names
        /// </summary>
        public const string ChangedString = "_CHANGED_";

        /// <summary>
        ///     If something needed to be replaced in log, it will be replaced with this string
        /// </summary>
        public const string RemovedLogString = "REMOVED_FROM_LOG";

        /// <summary>
        ///     Files preprocessor thread will sleep for this amount of milliseconds if no work is queued before next queue check
        /// </summary>
        public const int FilesPreprocessorThreadSleepInterval = 1000;
    }
}
