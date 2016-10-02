using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using ITCC.HTTP.SslConfigUtil.Core.Enums;
using ITCC.Logging.Core;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System.IO;
using ITCC.HTTP.SslConfigUtil.Core.Views;

namespace ITCC.HTTP.SslConfigUtil.Core
{
    internal static class CertificateController
    {
        internal static IEnumerable<CertificateView> GetCertificates()
        {
            var personalCertStote = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            personalCertStote.Open(OpenFlags.ReadOnly);
            return personalCertStote.Certificates.Cast<X509Certificate2>().Where(certificate => certificate.Verify() && certificate.IsValid()).Select(CertificateView.FromCert).ToList();
        }
        internal static FindCertificateStatus FindBySubjectName(string subjectName, out X509Certificate2 certificate)
        {
            try
            {
                var personalCertStote = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                personalCertStote.Open(OpenFlags.ReadWrite);
                var result = personalCertStote.Certificates.Find(X509FindType.FindBySubjectName, subjectName, true);
                var succeed = result.Count > 0;
                certificate = succeed ? result[0] : null;
                return succeed ? FindCertificateStatus.Found : FindCertificateStatus.NotFound;

            }
            catch (CryptographicException cryptographicException)
            {
                LogException(cryptographicException);
                certificate = null;
                return FindCertificateStatus.Error;
            }
        }
        internal static FindCertificateStatus FindByThumbprint(string thumbtrint, out X509Certificate2 certificate)
        {
            try
            {
                var personalCertStote = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                personalCertStote.Open(OpenFlags.ReadWrite);
                var result = personalCertStote.Certificates.Find(X509FindType.FindByThumbprint, thumbtrint, true);
                var succeed = result.Count > 0;
                certificate = succeed ? result[0] : null;
                return succeed ? FindCertificateStatus.Found : FindCertificateStatus.NotFound;

            }
            catch (CryptographicException cryptographicException)
            {
                LogException(cryptographicException);
                certificate = null;
                return FindCertificateStatus.Error;
            }
        }
        internal static bool GenerateCertificate(string subjectName, out X509Certificate2 cert)
        {
            try
            {
                LogDebug($"Generating self-signed certificate with subjectname '{subjectName}'");
                cert = GenerateSelfSignedCertificate(subjectName);
                LogDebug("Certificate issued");

                var filaname = $"{subjectName}_self-signed.pfx";
                var password = ToSecureString(Guid.NewGuid().ToString());
                using (var fs = new FileStream(filaname, FileMode.CreateNew))
                {
                    var buffer = cert.Export(X509ContentType.Pfx, password);
                    fs.Write(buffer, 0, buffer.Length);
                }

                var installResult = InstallSelfSigned(filaname, password, out cert);

                File.Delete(filaname);

                return installResult;
            }
            catch (Exception ex)
            {
                LogError($"Self-signed certificate generation failed");
                cert = null;
                LogException(ex);
                return false;
            }
        }
        internal static string GetInfo(this X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException();

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{"Issuer",-15} {certificate.Issuer}");
            stringBuilder.AppendLine($"{"SubjectName",-15} {certificate.SubjectName.Name}");
            stringBuilder.AppendLine($"{"Key algorithm",-15} {certificate.SignatureAlgorithm.FriendlyName}");
            stringBuilder.AppendLine($"{"Expiration",-15} {certificate.GetExpirationDateString()}");
            stringBuilder.AppendLine($"{"Thumbprint",-15} {certificate.Thumbprint}");

            return stringBuilder.ToString();
        }
        internal static OpenCertificateStatus InstallFromFile(string path, SecureString password, out X509Certificate2 cert)
        {
            LogDebug("Certificate installation started");
            cert = null;
            if (!File.Exists(path))
                return OpenCertificateStatus.NotFound;

            cert = OpenFromFile(path, password);
            if (cert == null)
                return OpenCertificateStatus.InvalidPassword;

            return VerifyAndInstall(cert) ? OpenCertificateStatus.Ok : OpenCertificateStatus.InvalidCertificate;
        }

        #region private

        private static bool InstallSelfSigned(string path, SecureString password, out X509Certificate2 cert)
        {
            LogDebug("Certificate installation started");
            try
            {
                cert = new X509Certificate2(path, password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            }
            catch (Exception ex)
            {
                cert = null;
                LogException(ex);
                return false;
            }

            return VerifyAndInstall(cert);
        }
        private static bool VerifyAndInstall(X509Certificate2 certificate)
        {
            LogDebug("Certificate opened");

            if (!certificate.IsValid())
            {
                LogError("Certificate is not valid");
                return false;
            }

            LogDebug($"Certificate information:\n{certificate.GetInfo()}");

            if (!certificate.Verify())
            {
                LogDebug("Trying to add new Trusted CA");
                if (!AddRootCa(certificate))
                {
                    LogError("Unable to add new Root CA");
                    return false;
                }
            }
            LogDebug("Certificate verified");

            try
            {
                var personalCertStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                personalCertStore.Open(OpenFlags.ReadWrite);

                if (certificate.Thumbprint == null)
                    throw new ArgumentNullException();

                var existingCertificates = personalCertStore.Certificates.Find(X509FindType.FindByThumbprint,
                    certificate.Thumbprint, true);
                if (existingCertificates.Count > 0)
                {
                    LogDebug("Certificate is already installed");
                    return true;
                }
                personalCertStore.Add(certificate);
                LogDebug("Certificate successfully installed");
                return true;

            }
            catch (CryptographicException cryptographicException)
            {
                LogException(cryptographicException);
                return false;
            }
        }
        private static X509Certificate2 OpenFromFile(string filepath, SecureString password)
        {
            var certificate = new X509Certificate2();

            try
            {
                certificate.Import(filepath);
            }
            catch (CryptographicException)
            {
                try
                {
                    LogDebug("Certificate protected with password. Trying to open...");
                    certificate.Import(filepath, password, X509KeyStorageFlags.DefaultKeySet);
                }
                catch (CryptographicException)
                {
                    LogError("Incorrect password");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }

            return certificate;
        }
        private static bool IsValid(this X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException();

            if (DateTime.Now < certificate.NotBefore)
            {
                LogError($"Certificate is not valid yet (begins from {certificate.NotBefore:G})");
                return false;
            }
            if (DateTime.Now > certificate.NotAfter)
            {
                LogError($"Certificate has expired ({certificate.NotAfter:G})");
                return false;
            }

            var serverAuthAllowed = false;
            foreach (var extension in certificate.Extensions)
            {
                var eku = extension as X509EnhancedKeyUsageExtension;
                if (eku == null)
                    continue;

                if (eku.EnhancedKeyUsages.Cast<Oid>().Any(oid => oid.FriendlyName.Equals("Server Authentication")))
                    serverAuthAllowed = true;
            }

            if (!serverAuthAllowed)
            {
                LogError("Certificate cannot be used for server authorization");
                return false;
            }

            LogDebug("Certificate is valid");
            return true;
        }
        private static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, int keyStrength = 2048)
        {
            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Issuer and Subject Name
            var x500DistinguishedName = new X509Name("CN=" + subjectName);
            certificateGenerator.SetIssuerDN(x500DistinguishedName);
            certificateGenerator.SetSubjectDN(x500DistinguishedName);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(2);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            var issuerKeyPair = subjectKeyPair;

            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512WITHRSA", issuerKeyPair.Private, random);

            certificateGenerator.AddExtension(
                X509Extensions.BasicConstraints,
                true,
                new BasicConstraints(false));
            certificateGenerator.AddExtension(
                X509Extensions.KeyUsage,
                true,
                new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
            certificateGenerator.AddExtension(
                X509Extensions.ExtendedKeyUsage,
                false,
                ExtendedKeyUsage.GetInstance(new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth, KeyPurposeID.IdKPClientAuth)));

            // selfsign certificate
            var certificate = certificateGenerator.Generate(signatureFactory);

            // correcponding private key
            var info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);
            // merge into X509Certificate2
            var x509 = new X509Certificate2(certificate.GetEncoded())
            {
                FriendlyName = $"{subjectName} self-signed"
            };

            var seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());
            if (seq.Count != 9)
                throw new PemException("malformed sequence in RSA private key");

            var rsa = RsaPrivateKeyStructure.GetInstance(seq);
            var rsaparams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);

            x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
            return x509;
        }
        private static bool AddRootCa(X509Certificate2 certificate)
        {
            var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationFlag = X509RevocationFlag.EntireChain,
                    RevocationMode = X509RevocationMode.NoCheck,
                    UrlRetrievalTimeout = new TimeSpan(0, 1, 0),
                    VerificationFlags = X509VerificationFlags.NoFlag
                }
            };

            chain.Build(certificate);
            LogDebug($"Chain length: {chain.ChainElements.Count}");
            foreach (var elem in chain.ChainElements)
            {
                var currentChainElement = elem.Certificate;
                var currentElementStatus = elem.ChainElementStatus.Select(x => x.Status);

                if (!currentElementStatus.Contains(X509ChainStatusFlags.UntrustedRoot))
                    continue;

                LogDebug($"Untrusted root CA found: {currentChainElement.IssuerName.Name}.Trying to add...");
                try
                {
                    var trustedCaCertStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    trustedCaCertStore.Open(OpenFlags.ReadWrite);

                    trustedCaCertStore.Add(currentChainElement);
                    LogDebug("Root CA Added");
                }
                catch (CryptographicException ex)
                {
                    LogException(ex);
                    return false;
                }
            }

            return true;
        }
        private static void LogError(string message) => Logger.LogEntry("CertController", LogLevel.Error, message);
        private static void LogDebug(string message) => Logger.LogEntry("CertController", LogLevel.Debug, message);
        private static void LogException(Exception exception) => Logger.LogException("CertController", LogLevel.Debug, exception);
        private static SecureString ToSecureString(string unsecurePassword)
        {
            if (string.IsNullOrEmpty(unsecurePassword))
                return null;

            var password = new SecureString();
            foreach (var c in unsecurePassword)
                password.AppendChar(c);
            return password;
        }

        #endregion

    }
}