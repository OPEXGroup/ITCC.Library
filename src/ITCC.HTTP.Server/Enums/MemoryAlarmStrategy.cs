// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Enums
{
    /// <summary>
    ///     Used to determine memory pressure warning intervals
    /// </summary>
    public enum MemoryAlarmStrategy
    {
        /// <summary>
        ///     1 minute, 1 minute, 1 minute, 1 minute, 1 minute
        /// </summary>
        Constant,
        /// <summary>
        ///     1 minute, 2 minutes, 3 minutes, 4 minutes, 5 minutes
        /// </summary>
        Linear,
        /// <summary>
        ///     1 minute, 2 minutes, 4 minutes, 8 minutes, 16 minutes
        /// </summary>
        Geometric,
        /// <summary>
        ///     1 minute, 1 minute, 2 minutes, 3 minutes, 5 minutes
        /// </summary>
        Fibonacci
    }
}
