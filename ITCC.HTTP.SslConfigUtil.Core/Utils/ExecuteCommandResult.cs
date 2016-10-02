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