using System;
using System.IdentityModel.Selectors;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CERTENROLLLib;
using ITCC.Logging;

namespace ITCC.HTTP.Security
{
    public static class CertificateController
    {
        public static bool RealCertificateValidationCallBack(
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

        public static bool MockCertificateValidationCallBack(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        ///     Created Self-signed X509 certificate
        /// </summary>
        /// <param name="subjectName">Server name/address</param>
        /// <returns>Certificate instance</returns>
        private static X509Certificate2 CreateSelfSignedCertificate(string subjectName)
        {
            var domainName = new CX500DistinguishedName();
            domainName.Encode("CN=" + subjectName);
            LogMessage(LogLevel.Debug, $"Domain name: {subjectName}");

            var privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft Base Cryptographic Provider v1.0",
                MachineContext = true,
                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_SIGNATURE,
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG
            };
            privateKey.Create();

            var hashobj = new CObjectId();
            hashobj.InitializeFromAlgorithmName(
                ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                AlgorithmFlags.AlgorithmFlagsNone,
                "SHA512"
                );

            // add extended key usage if you want - look at MSDN for a list of possible OIDs
            var objectId = new CObjectId();
            objectId.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server
            var oidlist = new CObjectIds {objectId};
            var enhancedKeyUsage = new CX509ExtensionEnhancedKeyUsage();
            enhancedKeyUsage.InitializeEncode(oidlist);

            var cert = new CX509CertificateRequestCertificate();
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
            cert.Subject = domainName;
            cert.Issuer = domainName;
            cert.NotBefore = DateTime.Now;
            cert.NotAfter = DateTime.Now.AddYears(5);
            cert.HashAlgorithm = hashobj;
            cert.X509Extensions.Add((CX509Extension) enhancedKeyUsage); // add the EKU
            cert.Encode();

            var enroll = new CX509Enrollment();
            enroll.InitializeFromRequest(cert);
            enroll.CertificateFriendlyName = subjectName;
            var csr = enroll.CreateRequest();
            enroll.InstallResponse(
                InstallResponseRestrictionFlags.AllowUntrustedCertificate,
                csr,
                EncodingType.XCN_CRYPT_STRING_BASE64,
                ""
                );

            var base64Encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);

            return new X509Certificate2(
                Convert.FromBase64String(base64Encoded),
                "",
                X509KeyStorageFlags.Exportable
                );
        }

        public static X509Certificate2 GetCertificate(string subjectName, bool allowSelfSigned)
        {
            try
            {
                var certStore = new X509Store(StoreLocation.LocalMachine);
                certStore.Open(OpenFlags.ReadWrite);
                var certificates = certStore.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
                if (certificates.Count != 0)
                {
                    LogMessage(LogLevel.Debug, $"Found certificate for subject {subjectName}");
                    return certificates[0];
                }
                if (!allowSelfSigned)
                    return null;
                LogMessage(LogLevel.Debug, "Creating self-signed certificate");
                return CreateSelfSignedCertificate(subjectName);
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                try
                {
                    if (!allowSelfSigned)
                        return null;
                    LogMessage(LogLevel.Debug, "Creating self-signed certificate");
                    return CreateSelfSignedCertificate(subjectName);
                }
                catch (Exception iex)
                {
                    LogException(LogLevel.Warning, iex);
                    return null;
                }
            }
        }

        private static void LogMessage(LogLevel level, string message)
        {
            Logger.LogEntry("CERTCONTROL", level, message);
        }

        private static void LogException(LogLevel level, Exception ex)
        {
            Logger.LogException("CERTCONTROL", level, ex);
        }
    }
}