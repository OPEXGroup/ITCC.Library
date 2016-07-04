namespace ITCC.HTTP.Server
{
    internal class RequestProcessorSelectionResult<TAccount>
    {
        public RequestProcessor<TAccount> RequestProcessor { get; set; }
        public bool MethodMatches { get; set; }
        public bool IsRedirect { get; set; }
    }
}
