using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using ITCC.HTTP.SslConfigUtil.Core.Enums;
using ITCC.Logging.Core;

namespace ITCC.HTTP.SslConfigUtil.Core
{
    public static class Binder
    {
        private static string SetCertArgs(string appGuid, string certThumbprint, IPAddress ipAddress, ushort port)
            => $"http add sslcert ipport={ipAddress}:{port} certhash={certThumbprint} appid={{{appGuid}}}";

        private static string CheckFreeArgs(IPAddress ipAddress, ushort port)
            => $"http show sslcert ipport={ipAddress}:{port}";

        public static BindingResult Bind(string assemblyPath, string subjectName, string ipAddress, string port,
            bool createSelfSignedCert)
        {
            Logger.LogEntry("Binder", LogLevel.Info, "Got new certificate bind request." +
                     "\nRequestParames:" +
                     $"\n\tAssembly file: '{assemblyPath}'," +
                     $"\n\tCertificate CN: '{subjectName}'" +
                     $"\n\tIp address: '{ipAddress}'" +
                     $"\n\tPort: '{port}'.");

            ushort portValue;
            if (!ushort.TryParse(port, out portValue) || portValue == 0)
            {
                Logger.LogEntry("Binder", LogLevel.Error, $"Port value '{port}' is invalid. Port must be a number between 1 and 65535");
                return BindingResult.InvalidPortValue;
            }
            Logger.LogEntry("Binder", LogLevel.Debug, $"Port: {portValue}");

            IPAddress address;
            if (!IPAddress.TryParse(ipAddress, out address))
            {
                Logger.LogEntry("Binder", LogLevel.Error, $"IpAddress value '{ipAddress}' is invalid.");
                return BindingResult.InvalidIpAddress;
            }
            Logger.LogEntry("Binder", LogLevel.Debug, $"Ip address: {address}");

            var getAssemblyGuidResult = AssemblyLoader.GetAssemblyGuid(assemblyPath);
            if (getAssemblyGuidResult.Status != LoadAssemblyStatus.Ok)
            {
                Logger.LogEntry("Binder", LogLevel.Error, $"Binding aborted because of assebmly loading failed: {getAssemblyGuidResult.Status}");
                return BindingResult.AssemblyLoadFailed;
            }
            Logger.LogEntry("Binder", LogLevel.Debug, $"AssemblyGuid: {getAssemblyGuidResult.Guid}");

            subjectName = subjectName.ToLower();
            if (!subjectName.ValidateSubjectname())
            {
                Logger.LogEntry("Binder", LogLevel.Error, $"SubjectName {subjectName} is not valid");
                return BindingResult.InvalidSubjectnameFormat;
            }

            X509Certificate2 certificate;

            var certificateLookupResult = CertificateController.FindBySubjectName(subjectName, out certificate);
            switch (certificateLookupResult)
            {
                case FindCertificateStatus.Found:
                    Logger.LogEntry("Binder", LogLevel.Debug, "Certificate found");
                    break;
                case FindCertificateStatus.NotFound:
                    Logger.LogEntry("Binder", LogLevel.Debug, "Certificate not found");
                    if (!createSelfSignedCert)
                    {
                        Logger.LogEntry("Binder", LogLevel.Debug, "Self-signed certificate creation is not allowed");
                        return BindingResult.SslCertificateNotFound;
                    }

                    if (!CertificateController.GenerateCertificate(subjectName, out certificate))
                        return BindingResult.SslCertificateGenerationFailed;

                    Logger.LogEntry("Binder", LogLevel.Debug, certificate.GetInfo());
                    break;
                case FindCertificateStatus.Error:
                    return BindingResult.Unspecified;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var checkBindingResult = CheckBinding(address, portValue);
            switch (checkBindingResult.Status)
            {
                case CheckBindingStatus.Empty:
                    break;
                case CheckBindingStatus.Binded:
                    Logger.LogEntry("Binder", LogLevel.Warning,
                        $"{address}:{portValue} is already in use. Binding info:\n{checkBindingResult.BindedAppliction}");
                    return BindingResult.PortIsAlreadyAssigned;
                case CheckBindingStatus.Error:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Logger.LogEntry("Binder",LogLevel.Debug, $"{address}:{portValue} is ready to bind");

            var bindResult = Bind(getAssemblyGuidResult.Guid, certificate.Thumbprint, address, portValue);
            return !bindResult ? BindingResult.Unspecified : BindingResult.Ok;
        }

        private static CheckBindingResult CheckBinding(IPAddress ipAddress, ushort port)
        {
            var executionResult = ExecuteCommand(CheckFreeArgs(ipAddress, port));
            switch (executionResult.Status)
            {
                case ExecuteCommandStatus.Ok:
                    return ParseCheckBind(executionResult.Output);
                case ExecuteCommandStatus.Error:
                    return new CheckBindingResult { Status = CheckBindingStatus.Error};
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private static bool Bind(string guid, string thumbprint, IPAddress ipAddress, ushort port)
        {
            var executionResult = ExecuteCommand(SetCertArgs(guid, thumbprint, ipAddress, port));
            if (executionResult.Status == ExecuteCommandStatus.Error)
                return false;

            return executionResult.Output.Contains("SSL Certificate successfully added") ? true : false;
        }
        private static CheckBindingResult ParseCheckBind(string raw)
        {
            if (raw.Contains("The system cannot find the file specified."))
                return new CheckBindingResult {Status = CheckBindingStatus.Empty };

            var lines = raw.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Application ID"))
                {
                    return new CheckBindingResult
                    {
                        Status = CheckBindingStatus.Binded,
                        BindedAppliction = line.Split(':')[1].Trim()
                    };
                }
            }

            //return new CheckBindingResult { Status = CheckBindingStatus.Error };
            throw new NotImplementedException();
        }
        private static ExecuteCommandResult ExecuteCommand(string agrs)
        {
            Console.WriteLine(agrs);
            try
            {
                var process = new Process
                {
                    StartInfo =
                {
                    FileName = "netsh.exe",
                    Arguments = agrs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
                };
                process.Start();

                return new ExecuteCommandResult
                {
                    Status = ExecuteCommandStatus.Ok,
                    Output = process.StandardOutput.ReadToEnd()
                };
            }
            catch (Exception exception)
            {
                Logger.LogException("Binder", LogLevel.Error, exception);
                return new ExecuteCommandResult { Status = ExecuteCommandStatus.Error };
            }


        }

        private static bool ValidateSubjectname(this string subjectName) => SubjectNamePattern.IsMatch(subjectName);
        private static readonly Regex SubjectNamePattern = new Regex(@"(?=^.{1,254}$)(^(?:(?!\d|-)[a-zA-Z0-9\-]{1,63}(?<!-)\.?)+(?:[a-zA-Z]{2,})$)", RegexOptions.Compiled);
    }
}