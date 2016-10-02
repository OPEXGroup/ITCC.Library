using System.Collections.Generic;

namespace ITCC.HTTP.SslConfigUtil.Console
{
    internal struct ArgsParsingResult
    {
        public bool IsSucceed;
        public Dictionary<string, string> ParamDictionary;
        public string FailReason;
    }
}