// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Core
{
    internal class RequestProcessorSelectionResult<TAccount>
    {
        public RequestProcessor<TAccount> RequestProcessor { get; set; }
        public bool MethodMatches { get; set; }
        public bool IsRedirect { get; set; }
    }
}
