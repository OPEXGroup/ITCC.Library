// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Security.Cryptography.X509Certificates;

namespace ITCC.HTTP.SslConfigUtil.Core.Views
{
    public class CertificateView
    {
        public string SubjectName { get; set; }
        public string Issuer { get; set; }
        public string Thumbprint { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }

        public static CertificateView FromCert(X509Certificate2 cert)
        {
            return new CertificateView
            {
                SubjectName = cert.Subject,
                Issuer = cert.Issuer,
                Thumbprint = cert.Thumbprint,
                NotBefore = cert.NotBefore,
                NotAfter = cert.NotAfter
            };
        }
    }
}