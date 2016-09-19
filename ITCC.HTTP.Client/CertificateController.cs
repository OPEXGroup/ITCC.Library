using System;
using System.IdentityModel.Selectors;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ITCC.HTTP.Client
{
    internal static class CertificateController
    {
        internal static bool RealCertificateValidationCallBack(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            var certificate2 = certificate as X509Certificate2 ?? new X509Certificate2(certificate);

            try
            {
                X509CertificateValidator.PeerOrChainTrust.Validate(certificate2);
                //X509CertificateValidator.ChainTrust.Validate(certificate2);
            }
            catch
            {
                return false;
            }

            var request = sender as WebRequest;
            var requestHostname = request?.RequestUri.Host ?? (string) sender;

            var certHostname = certificate2.GetNameInfo(X509NameType.DnsName, false);
            return requestHostname.Equals(certHostname, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool MockCertificateValidationCallBack(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors) => true;
    }
}