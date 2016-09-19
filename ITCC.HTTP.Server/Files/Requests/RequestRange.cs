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
