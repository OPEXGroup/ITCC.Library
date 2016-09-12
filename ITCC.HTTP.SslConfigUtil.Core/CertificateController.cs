using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Security;
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
using static ITCC.HTTP.SslConfigUtil.Core.AssemblyLoader;

namespace ITCC.HTTP.SslConfigUtil.Core
{
    public static class CertificateController
    {

        private static string SetCertCommand(string appGuid, string certThumbprint, IPAddress ipAddress, ushort port)
            => $"netsh http add sslcert ipport = {ipAddress}:{port} certhash = {certThumbprint} appid = {{ {appGuid} }}";

        private static string CheckFreeCommand(IPAddress ipAddress, ushort port)
            => $"netsh http show sslcert ipport={ipAddress}:{port}";

        public static BindingResult Bind(string assemblyPath, string subjectName, string ipAddress, string port,
            bool createSelfSignedCert)
        {
            LogDebug("Got new certificate bind request." +
                     "\nRequestParames:" +
                     $"\n\tAssembly file: '{assemblyPath}'," +
                     $"\n\tCertificate CN: '{subjectName}'" +
                     $"\n\tIp address: '{ipAddress}'" +
                     $"\n\tPort: '{port}'.");

            ushort p;
            if (!ushort.TryParse(port, out p) || p == 0)
            {
                LogError($"Port value '{port}' is invalid. Port must be a number between 1 and 65535");
                return BindingResult.InvalidPortValue;
            }
            LogDebug($"Port: {p}");

            IPAddress address;
            if (!IPAddress.TryParse(ipAddress, out address))
            {
                LogError($"IpAddress value '{ipAddress}' is invalid.");
                return BindingResult.InvalidIpAddress;
            }
            LogDebug($"Ip address: {address}");

            var getAssemblyGuidResult = GetAssemblyGuid(assemblyPath);
            if (getAssemblyGuidResult.Status != LoadAssemblyStatus.Ok)
            {
                LogError($"Binding aborted because of assebmly loading failed: {getAssemblyGuidResult.Status}");
                return BindingResult.AssemblyLoadFailed;
            }
            LogDebug($"AssemblyGuid: {getAssemblyGuidResult.Guid}");

            //Validate SubjectName

            X509Certificate2 certificate;

            var certificateLookupResult = FindBySubjectName(subjectName, out certificate);
            switch (certificateLookupResult)
            {
                case FindCertificateStatus.Found:
                    LogDebug("Certificate found");
                    break;
                case FindCertificateStatus.NotFound:
                    LogDebug("Certificate not found");
                    if (!createSelfSignedCert)
                    {
                        LogDebug("Self-signed certificate creation is not allowed");
                        return BindingResult.SslCertificateNotFound;
                    }

                    if (!GenerateCertificate(subjectName, out certificate))
                        return BindingResult.SslCertificateGenerationFailed;

                    LogDebug(certificate.GetInfo());
                    File.WriteAllBytes(@"D:\Self-signed cert test.cer", certificate.Export(X509ContentType.Cert));

                    break;
                case FindCertificateStatus.Error:
                    return BindingResult.Unspecified;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //Check current binding

            //Bind

            return BindingResult.Ok;
        }





        private static bool Install(string path, SecureString password, out X509Certificate2 cert)
        {
            LogDebug("Certificate installation started");
            var certificate = OpenFromFile(path, password);

            if (certificate == null)
            {
                LogError("Unable to import certificate from file");
                cert = null;
                return false;
            }

            LogDebug("Certificate opened");

            if (!certificate.IsValid())
            {
                LogError("Certificate is not valid");
                cert = null;
                return false;
            }

            LogDebug($"Certificate information:\n{certificate.GetInfo()}");

            if (!certificate.Verify())
            {
                LogDebug("Trying to add new Trusted CA");
                if (!AddRootCa(certificate))
                {
                    LogError("Unable to add new Root CA");
                    cert = null;
                    return false;
                }
            }
            LogDebug("Certificate verified");

            try
            {
                var personalCertStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                personalCertStore.Open(OpenFlags.ReadWrite);

                var existingCertificates = personalCertStore.Certificates.Find(X509FindType.FindByThumbprint,
                    certificate.Thumbprint, true);
                if (existingCertificates.Count > 0)
                {
                    LogDebug("Certificate is already installed");
                    cert = certificate;
                    return true;
                }
                personalCertStore.Add(certificate);
                LogDebug("Certificate successfully installed");
                cert = certificate;
                return true;

            }
            catch (CryptographicException cryptographicException)
            {
                LogException(cryptographicException);
                cert = null;
                return false;
            }
        }

        private static FindCertificateStatus FindBySubjectName(string subjectName, out X509Certificate2 certificate)
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

        private static bool GenerateCertificate(string subjectName, out X509Certificate2 cert)
        {
            try
            {
                LogDebug($"Generating self-signed certificate with subjectname '{subjectName}'");
                cert = GenerateSelfSignedCertificate(subjectName);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Self-signed certificate generation failed");
                cert = null;
                LogException(ex);
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

        private static string GetInfo(this X509Certificate2 certificate)
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


        public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, int keyStrength = 2048)
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

            certificateGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DataEncipherment));
            //certificateGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));


            var exUsage = new ExtendedKeyUsage(new List<DerObjectIdentifier>
            {
                new DerObjectIdentifier("1.3.6.1.5.5.7.3.1")
            });

            //TODO: certificate extended key usage repair

            //certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, ExtendedKeyUsage.GetInstance(new DerObjectIdentifier("1.3.6.1.5.5.7.3.1")));
            //certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, ExtendedKeyUsage.GetInstance(new DerObjectIdentifier("1.3.6.1.5.5.7.3.2")));

            // selfsign certificate
            var certificate = certificateGenerator.Generate(signatureFactory);

            // correcponding private key
            var info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);

            // merge into X509Certificate2
            var x509 = new X509Certificate2(certificate.GetEncoded());

            var seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());
            if (seq.Count != 9)
                throw new PemException("malformed sequence in RSA private key");

            var rsa = RsaPrivateKeyStructure.GetInstance(seq);
            var rsaparams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);

            x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);

            return x509;
        }

        private static void LogError(string message) => Logger.LogEntry("CertController", LogLevel.Error, message);
        private static void LogDebug(string message) => Logger.LogEntry("CertController", LogLevel.Debug, message);
        private static void LogException(Exception exception) => Logger.LogException("CertController", LogLevel.Debug, exception);
    }
}