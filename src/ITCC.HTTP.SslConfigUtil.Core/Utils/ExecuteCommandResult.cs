// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using ITCC.HTTP.SslConfigUtil.Core.Enums;

namespace ITCC.HTTP.SslConfigUtil.Core.Utils
{
    internal struct ExecuteCommandResult
    {
        public ExecuteCommandStatus Status;
        public int ExitCode;
        public string Output;
    }
}