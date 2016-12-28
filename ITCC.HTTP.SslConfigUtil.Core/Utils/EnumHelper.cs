// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using ITCC.HTTP.SslConfigUtil.Core.Enums;

namespace ITCC.HTTP.SslConfigUtil.Core.Utils
{
    public static class EnumHelper
    {
        internal static readonly Dictionary<LoadAssemblyStatus, string> LoadAssemblyDescriptionDictionary =
            new Dictionary<LoadAssemblyStatus, string>
            {
                {LoadAssemblyStatus.Ok, "Assembly loaded."},
                {LoadAssemblyStatus.AccessDenied, "Access denied."},
                {LoadAssemblyStatus.BadAssemblyFormat, "File must be a .NET assembly"},
                {LoadAssemblyStatus.DirectoryNotFound, "Directory not found."},
                {LoadAssemblyStatus.FileNotFound, "File not found."},
                {LoadAssemblyStatus.IsNullOrWhiteSpace, "File path is null or white space."},
                {LoadAssemblyStatus.PathTooLong, "File path is too long (260 max)."},
                {LoadAssemblyStatus.UnknownError, "Unknown error occured."}
            };
        internal static string DisplayName(LoadAssemblyStatus status) => LoadAssemblyDescriptionDictionary[status];

        private static readonly Dictionary<BindingStatus, string> BindingDescriptionDescriptionDictionary =
            new Dictionary<BindingStatus, string>
            {
                {BindingStatus.Ok, "Binding successfully created."},
                {BindingStatus.InvalidParams, "Input paramerers are invalid."},
                {BindingStatus.SslCertFromFileError, "Loading certificate from file failed."},
                {BindingStatus.UnbindingError, "Unable to remove existing binding."},
                {BindingStatus.UnknownError, "Unknown error."},
                {BindingStatus.InvalidSubjectnameFormat, "SubjectName hs incorrect format."},
                {BindingStatus.PortIsAlreadyAssigned, "IP:port is already assigned to another application. Use 'UnsafeBinding' option to disable assembly guid validation."},
                {BindingStatus.SslCertificateExpired, "SSL certificate expired."},
                {BindingStatus.SslCertificateGenerationFailed, "Generation of self-signed certificate failed."},
                {BindingStatus.SslCertificateNotFound, "Successfully binded."},
                {BindingStatus.Unspecified, "Unspecified status."},
            };
        public static string DisplayName(BindingStatus status) => BindingDescriptionDescriptionDictionary[status];

        private static readonly Dictionary<UnbindStatus, string> UnbindStatusDescriptionDictionary =
            new Dictionary<UnbindStatus, string>
            {
                {UnbindStatus.Ok, "Binding successfully removed."},
                {UnbindStatus.DifferentApplicationGuid, "Binding used by anothed application. Use 'UnsafeBinding' option to disable assembly guid validation."},
                {UnbindStatus.InvalidParams, "Input paramerers are invalid."},
                {UnbindStatus.Empty, "Binding with entered ip:port is not exist."},
                {UnbindStatus.Error, "Unknown error."},
            };
        public static string DisplayName(UnbindStatus status) => UnbindStatusDescriptionDictionary[status];

        private static readonly Dictionary<ParseBaseParamsStatus, string> ParseBaseParamsStatusDescriptionDictionary =
           new Dictionary<ParseBaseParamsStatus, string>
           {
                {ParseBaseParamsStatus.Ok, "Params are valid."},
                {ParseBaseParamsStatus.AssemblyLoadFailed, "Unable to load assembly."},
                {ParseBaseParamsStatus.InvalidIpAddress, "Ip address has incorrect format."},
                {ParseBaseParamsStatus.InvalidPortValue, "Invalid port value."},
           };
        internal static string DisplayName(ParseBaseParamsStatus status) => ParseBaseParamsStatusDescriptionDictionary[status];

        private static readonly Dictionary<OpenCertificateStatus, string> OpenCertificateStatusDescriptionDictionary =
          new Dictionary<OpenCertificateStatus, string>
          {
                {OpenCertificateStatus.Ok, "Certificate successfully opened."},
                {OpenCertificateStatus.InvalidCertificate, "Certificate is not valid."},
                {OpenCertificateStatus.InvalidPassword, "Entered certificate's password is not correct."},
                {OpenCertificateStatus.NotFound, "Certificate file not found."},
                {OpenCertificateStatus.Error, "Unknown error."},
          };
        internal static string DisplayName(OpenCertificateStatus status) => OpenCertificateStatusDescriptionDictionary[status];
    }
}