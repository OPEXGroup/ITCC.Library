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
    public class CertificateSubjectnameParams
    {
        public string SubjectName { get; set; }

        public CertificateSubjectnameParams(string subjectName)
        {
            SubjectName = subjectName;
        }
    }
}