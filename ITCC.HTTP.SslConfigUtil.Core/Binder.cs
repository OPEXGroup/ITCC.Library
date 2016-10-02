using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using ITCC.HTTP.SslConfigUtil.Core.Enums;
using ITCC.HTTP.SslConfigUtil.Core.Utils;
using ITCC.HTTP.SslConfigUtil.Core.Views;
using ITCC.Logging.Core;

namespace ITCC.HTTP.SslConfigUtil.Core
{
    public static class Binder
    {
        public static BindingResult Bind(string assemblyPath, string ipAddressString, string portString,
            CertificateThumbprintBindingParams bindingParams, bool unsafeBinding = false)
        {
            Debug.WriteLine("Bind with existing cert started");
            var basicParams = ParseBaseParams(assemblyPath, ipAddressString, portString);
            if (basicParams.Status != ParseBaseParamsStatus.Ok)
                return new BindingResult
                {
                    Status = BindingStatus.InvalidParams,
                    Reason = EnumHelper.DisplayName(basicParams.Status)
                };

            X509Certificate2 certificate;
            var certificateLookupResult = CertificateController.FindByThumbprint(bindingParams.Thumbprint, out certificate);
            if (certificateLookupResult != FindCertificateStatus.Found)
                return new BindingResult
                {
                    Status = BindingStatus.SslCertificateNotFound
                };

            return Bind(basicParams.AssemblyGuid, basicParams.IpAddress, basicParams.Port, certificate, unsafeBinding);
        }
        public static BindingResult Bind(string assemblyPath, string ipAddressString, string portString,
            CertificateFileBindingParams bindingParams, bool unsafeBinding = false)
        {
            Debug.WriteLine("Bind with cert file started");
            var basicParams = ParseBaseParams(assemblyPath, ipAddressString, portString);
            if (basicParams.Status != ParseBaseParamsStatus.Ok)
                return new BindingResult
                {
                    Status = BindingStatus.InvalidParams,
                    Reason = EnumHelper.DisplayName(basicParams.Status)
                };

            X509Certificate2 certificate;
            var openCertResult = CertificateController.InstallFromFile(bindingParams.Filepath, bindingParams.Password,
                out certificate);
            if (openCertResult != OpenCertificateStatus.Ok)
                return new BindingResult
                {
                    Status = BindingStatus.SslCertFromFileError,
                    Reason = EnumHelper.DisplayName(openCertResult)
                };

            return Bind(basicParams.AssemblyGuid, basicParams.IpAddress, basicParams.Port, certificate, unsafeBinding);
        }
        public static BindingResult Bind(string assemblyPath, string ipAddressString, string portString,
            CertificateSubjectnameParams bindingParams, bool unsafeBinding = false)
        {
            Debug.WriteLine("Bind with generation started");
            var basicParams = ParseBaseParams(assemblyPath, ipAddressString, portString);
            if (basicParams.Status != ParseBaseParamsStatus.Ok)
                return new BindingResult
                {
                    Status = BindingStatus.InvalidParams,
                    Reason = EnumHelper.DisplayName(basicParams.Status)
                };

            var subjectName = bindingParams.SubjectName.ToLower();
            if (!subjectName.ValidateSubjectname())
            {
                Logger.LogEntry("Binder", LogLevel.Error, $"SubjectName {subjectName} is not valid");
                return new BindingResult
                {
                    Status = BindingStatus.InvalidSubjectnameFormat
                };
            }

            X509Certificate2 certificate;
            var certificateLookupResult = CertificateController.FindBySubjectName(subjectName, out certificate);
            switch (certificateLookupResult)
            {
                case FindCertificateStatus.Found:
                    break;
                case FindCertificateStatus.NotFound:
                    if (!CertificateController.GenerateCertificate(subjectName, out certificate))
                        return new BindingResult
                        {
                            Status = BindingStatus.SslCertificateGenerationFailed
                        };
                    break;
                case FindCertificateStatus.Error:
                    return new BindingResult
                    {
                        Status = BindingStatus.UnknownError
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Bind(basicParams.AssemblyGuid, basicParams.IpAddress, basicParams.Port, certificate, unsafeBinding);
        }
        public static UnbindResult Unbind(string assemblyPath, string ipAddressString, string portString, bool unsafeUnbinding = false)
        {
            var baseParams = ParseBaseParams(assemblyPath, ipAddressString, portString);
            return baseParams.Status != ParseBaseParamsStatus.Ok 
                ? new UnbindResult { Status = UnbindStatus.InvalidParams } 
                : new UnbindResult { Status = Unbind(baseParams.AssemblyGuid, baseParams.IpAddress, baseParams.Port, unsafeUnbinding) };
        }
        public static IEnumerable<CertificateView> GetCertificateList() => CertificateController.GetCertificates();

        #region private
        private static BindingResult Bind(string assemblyGuid, IPAddress ipAddress, ushort port,
        X509Certificate2 certificate, bool unsafeBinding)
        {
            var checkBindingResult = CheckBinding(ipAddress, port);
            switch (checkBindingResult.Status)
            {
                case CheckBindingStatus.Empty:
                    break;
                case CheckBindingStatus.Binded:
                    if (assemblyGuid != checkBindingResult.BindedAppliction && !unsafeBinding)
                    {
                        Logger.LogEntry("Binder", LogLevel.Warning, $"{ipAddress}:{port} is already in use.");
                        return new BindingResult
                        {
                            Status = BindingStatus.PortIsAlreadyAssigned
                        };
                    }

                    if (Unbind(assemblyGuid, ipAddress, port, unsafeBinding) != UnbindStatus.Ok)
                        return new BindingResult
                        {
                            Status = BindingStatus.UnbindingError
                        };

                    break;
                case CheckBindingStatus.Error:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Logger.LogEntry("Binder", LogLevel.Debug, $"{ipAddress}:{port} is ready to bind");

            var bindResult = TryBind(assemblyGuid, certificate.Thumbprint, ipAddress, port);
            return !bindResult ? new BindingResult { Status = BindingStatus.UnknownError } : new BindingResult { Status = BindingStatus.Ok };
        }
        private static UnbindStatus Unbind(string assemblyGuid, IPAddress ipAddress, ushort port,
            bool unsafeUnbinding)
        {
            var checkBinding = CheckBinding(ipAddress, port);
            switch (checkBinding.Status)
            {
                case CheckBindingStatus.Empty:
                    return UnbindStatus.Empty;
                case CheckBindingStatus.Binded:
                    var sameApp = assemblyGuid == checkBinding.BindedAppliction;
                    if (!sameApp && !unsafeUnbinding)
                        return UnbindStatus.DifferentApplicationGuid;

                    return TryUnbind(ipAddress, port) ? UnbindStatus.Ok : UnbindStatus.Error;
                case CheckBindingStatus.Error:
                    return UnbindStatus.Error;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ParseParamsResult ParseBaseParams(string assemblyPath, string ipAddressString, string portString)
        {
            Logger.LogEntry("Binder", LogLevel.Info, "Got new certificate bind request." +
                        "\nRequestParames:" +
                        $"\n\tAssembly file: '{assemblyPath}'," +
                        $"\n\tIp address: '{ipAddressString}'" +
                        $"\n\tPort: '{portString}'.");
            ushort port;
            if (!ushort.TryParse(portString, out port) || port == 0)
            {
                Logger.LogEntry("Binder", LogLevel.Error, $"Port value '{portString}' is invalid. Port must be a number between 1 and 65535");
                return new ParseParamsResult { Status = ParseBaseParamsStatus.InvalidPortValue };
            }
            Logger.LogEntry("Binder", LogLevel.Debug, $"Port: {port}");

            IPAddress ipAddress;
            if (!IPAddress.TryParse(ipAddressString, out ipAddress))
            {
                Logger.LogEntry("Binder", LogLevel.Error, $"IpAddress value '{ipAddressString}' is invalid.");
                return new ParseParamsResult { Status = ParseBaseParamsStatus.InvalidIpAddress };
            }
            Logger.LogEntry("Binder", LogLevel.Debug, $"Ip address: {ipAddress}");

            var getAssemblyGuidResult = AssemblyLoader.GetAssemblyGuid(assemblyPath);
            if (getAssemblyGuidResult.Status != LoadAssemblyStatus.Ok)
            {
                Logger.LogEntry("Binder", LogLevel.Error, $"Binding aborted because of assebmly loading failed: {getAssemblyGuidResult.Status}");
                return new ParseParamsResult { Status = ParseBaseParamsStatus.AssemblyLoadFailed };
            }
            Logger.LogEntry("Binder", LogLevel.Debug, $"AssemblyGuid: {getAssemblyGuidResult.Guid}");

            return new ParseParamsResult
            {
                Status = ParseBaseParamsStatus.Ok,
                AssemblyGuid = getAssemblyGuidResult.Guid,
                IpAddress = ipAddress,
                Port = port
            };
        }
        private static CheckBindingResult CheckBinding(IPAddress ipAddress, ushort port)
        {
            var executionResult = ExecuteCommand(CheckBindingArgs(ipAddress, port));
            switch (executionResult.Status)
            {
                case ExecuteCommandStatus.Ok:
                    return ParseCheckBind(executionResult);
                case ExecuteCommandStatus.Error:
                    return new CheckBindingResult { Status = CheckBindingStatus.Error };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private static CheckBindingResult ParseCheckBind(ExecuteCommandResult executionResult)
        {
            if (executionResult.ExitCode == 1)
                return new CheckBindingResult { Status = CheckBindingStatus.Empty };

            var matches = GuidPattern.Matches(executionResult.Output);
            if (matches.Count > 0)
            {
                return new CheckBindingResult
                {
                    Status = CheckBindingStatus.Binded,
                    BindedAppliction = matches[0].Value
                };
            }

            return new CheckBindingResult { Status = CheckBindingStatus.Error };
        }
        private static ExecuteCommandResult ExecuteCommand(string agrs)
        {
            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = "netsh.exe",
                        Arguments = agrs,
                        UseShellExecute = false,
                        StandardOutputEncoding = Encoding.UTF8,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return new ExecuteCommandResult
                {
                    Status = ExecuteCommandStatus.Ok,
                    ExitCode = process.ExitCode,
                    Output = process.StandardOutput.ReadToEnd()
                };
            }
            catch (Exception exception)
            {
                Logger.LogException("Binder", LogLevel.Error, exception);
                return new ExecuteCommandResult
                {
                    Status = ExecuteCommandStatus.Error,
                    ExitCode = -1
                };
            }
        }
        private static bool TryBind(string guid, string thumbprint, IPAddress ipAddress, ushort port)
        {
            Logger.LogEntry("Binder", LogLevel.Debug, "TryBind started");
            var executionResult = ExecuteCommand(AddBindingArgs(guid, thumbprint, ipAddress, port));
            Logger.LogEntry("Binder", LogLevel.Debug, $"Command execurted, status={executionResult.ExitCode} output:\n{executionResult.Output}");
            if (executionResult.Status == ExecuteCommandStatus.Error)
                return false;

            return executionResult.ExitCode == 0;
        }
        private static bool TryUnbind(IPAddress ipAddress, ushort port)
        {
            var executionResult = ExecuteCommand(RemoveBindingArgs(ipAddress, port));
            if (executionResult.Status == ExecuteCommandStatus.Error)
                return false;

            return executionResult.ExitCode == 0;
        }

        private static bool ValidateSubjectname(this string subjectName) => SubjectNamePattern.IsMatch(subjectName);
        private static readonly Regex GuidPattern = new Regex(@"[0-9A-F]{8}-([0-9A-F]{4}-){3}[0-9A-F]{12}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SubjectNamePattern = new Regex(@"(?=^.{1,254}$)(^(?:(?!\d|-)[a-zA-Z0-9\-]{1,63}(?<!-)\.?)+(?:[a-zA-Z]{2,})$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string AddBindingArgs(string appGuid, string certThumbprint, IPAddress ipAddress, ushort port)
            => $"http add sslcert ipport={ipAddress}:{port} certhash={certThumbprint} appid={{{appGuid}}}";
        private static string CheckBindingArgs(IPAddress ipAddress, ushort port)
            => $"http show sslcert ipport={ipAddress}:{port}";
        private static string RemoveBindingArgs(IPAddress ipAddress, ushort port)
            => $"http delete sslcert ipport={ipAddress}:{port}";

        #endregion
    }
}