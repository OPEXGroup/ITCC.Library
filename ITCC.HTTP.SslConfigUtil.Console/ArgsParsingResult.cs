// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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