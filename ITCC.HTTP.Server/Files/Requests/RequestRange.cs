// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Files.Requests
{
    internal class RequestRange
    {
        /// <summary>
        ///     Inclusive
        /// </summary>
        public long? RangeStart { get; set; }

        /// <summary>
        ///     Exclusive
        /// </summary>
        public long? RangeEnd { get; set; }
    }
}
