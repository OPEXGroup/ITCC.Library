// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.Logging.Core.Interfaces
{
    /// <summary>
    ///     Interface for objects which can write logs
    /// </summary>
    public interface ILogReceiver
    {
        /// <summary>
        ///     Gets or sets logger personal loglevel
        /// </summary>
        LogLevel Level { get; set; }

        /// <summary>
        ///     Writes entry if it is interesting to logger
        /// </summary>
        /// <param name="sender">Event scope</param>
        /// <param name="args">Event params</param>
        void WriteEntry(object sender, LogEntryEventArgs args);
    }
}