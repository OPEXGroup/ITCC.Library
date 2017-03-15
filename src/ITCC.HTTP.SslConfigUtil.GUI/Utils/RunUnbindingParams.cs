// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.SslConfigUtil.GUI.Utils
{
    internal class RunUnbindingParams
    {
        public string ApplicationPath { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public bool Unsafe = false;
    }
}