// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Net;
using ITCC.HTTP.SslConfigUtil.Core.Enums;

namespace ITCC.HTTP.SslConfigUtil.Core.Utils
{
    internal class ParseParamsResult
    {
        public ParseBaseParamsStatus Status { get; set; }
        public string Reason { get; set; }
        public string AssemblyGuid { get; set; }
        public IPAddress IpAddress { get; set; }
        public ushort Port { get; set; }
    }
}