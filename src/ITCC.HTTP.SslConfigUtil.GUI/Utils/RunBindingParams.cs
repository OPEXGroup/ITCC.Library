// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Security;
using ITCC.HTTP.SslConfigUtil.GUI.Enums;

namespace ITCC.HTTP.SslConfigUtil.GUI.Utils
{
    internal class RunBindingParams
    {
        public CertificateMode Mode { get; set; }
        public string AssemplyPath { get; set; }
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Thumbprint { get; set; }
        public string SubjectName { get; set; }
        public string CertificatePath { get; set; }
        public SecureString Password { get; set; }
        public bool UnsafeBinding = false;
    }
}