// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Security;
using ITCC.HTTP.SslConfigUtil.Core.Enums;

namespace ITCC.HTTP.SslConfigUtil.Core
{
    public abstract class BindingParams
    {
        public BindType Type { get; set; }
    }
    public class CertificateThumbprintBindingParams : BindingParams
    {
        public string Thumbprint { get; set; }

        public CertificateThumbprintBindingParams(string thumbprint)
        {
            Thumbprint = thumbprint;
        }
    }
    public class CertificateFileBindingParams : BindingParams
    {
        public string Filepath { get; set; }
        public SecureString Password { get; set; }

        public CertificateFileBindingParams(string filepath, SecureString password)
        {
            Filepath = filepath;
            Password = password;
        }
    }
    public class CertificateSubjectnameParams : BindingParams
    {
        public string SubjectName { get; set; }
        public bool AllowGenerated { get; set; }

        public CertificateSubjectnameParams(string subjectName, bool allowGenerated = true)
        {
            SubjectName = subjectName;
            AllowGenerated = allowGenerated;
        }
    }
}