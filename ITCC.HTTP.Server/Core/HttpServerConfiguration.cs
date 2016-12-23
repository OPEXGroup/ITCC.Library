using System;
using System.Collections.Generic;
using System.Linq;
using ITCC.HTTP.Common.Enums;
using ITCC.HTTP.Server.Common;
using ITCC.HTTP.Server.Enums;
using ITCC.HTTP.Server.Files;
using ITCC.HTTP.SslConfigUtil.Core.Enums;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Core
{
    public class HttpServerConfiguration<TAccount>
        where TAccount : class
    {
        /// <summary>
        ///     Used for certificate search
        /// </summary>
        public string SubjectName { get; set; }
        /// <summary>
        ///     TCP port to listen
        /// </summary>
        public ushort Port { get; set; }
        /// <summary>
        ///     Http/https
        /// </summary>
        public Protocol Protocol { get; set; }

        /// <summary>
        ///     Used for https servers
        /// </summary>
        public BindType CertificateBindType { get; set; } = BindType.SubjectName;

        /// <summary>
        ///     Used for https servers
        /// </summary>
        public bool AllowGeneratedCertificates { get; set; } = false;

        /// <summary>
        ///     For Protocol = Https and CertificateBindType = FromFile
        /// </summary>
        public string CertificateFilename { get; set; }

        /// <summary>
        ///     For Protocol = Https and CertificateBindType = CertificateThumbprint
        /// </summary>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        ///     Does server support file streaming?
        /// </summary>
        public bool FilesEnabled { get; set; }
        /// <summary>
        ///     File location on disk
        /// </summary>
        public string FilesLocation { get; set; }
        /// <summary>
        ///     All files can be found at SubjectName:Port/FilesBaseUri/Filename
        /// </summary>
        public string FilesBaseUri { get; set; }
        /// <summary>
        ///     If true, every file request will be checked with Authorizer
        /// </summary>
        public bool FilesNeedAuthorization { get; set; }
        /// <summary>
        ///     If true, then files will be preprocessed (image sizes variants, video transcoding)
        /// </summary>
        public bool FilesPreprocessingEnabled { get; set; } = true;
        /// <summary>
        ///     If true, zipped copies of all files will be created. Works only with FilesPreprocessingEnabled
        /// </summary>
        public bool FilesCompressionEnabled { get; set; } = false;
        /// <summary>
        ///     Number of threads used for files preprocessing. All CPU cores will be used for negative values
        /// </summary>
        public int FilesPreprocessorThreads { get; set; } = -1;
        /// <summary>
        ///     How often do we preprocess existing files. Negative means never
        /// </summary>
        public double ExistingFilesPreprocessingFrequency { get; set; } = 60; 
        /// <summary>
        ///     File sections for separate access grants
        /// </summary>
        public List<FileSection> FileSections { get; set; } = new List<FileSection>(); 
        /// <summary>
        ///     Method used to check authorization tokens for files requests
        /// </summary>
        public Delegates.FilesAuthorizer<TAccount> FilesAuthorizer { get; set; }

        /// <summary>
        ///     Method used to receive authorization tokens
        /// </summary>
        public Delegates.Authentificator Authentificator { get; set; }
        /// <summary>
        ///     Method used to check authorization tokens
        /// </summary>
        public Delegates.Authorizer<TAccount> Authorizer { get; set; }

        /// <summary>
        ///     Used to process response bodies. Defaults to application/json; charset=utf-8
        /// </summary>
        public List<BodyEncoder> BodyEncoders { get; set; } = new List<BodyEncoder> {new BodyEncoder()};

        /// <summary>
        ///     For objects of these types simple ToString() will be invoked
        /// </summary>
        public List<Type> NonSerializableTypes { get; set; } = new List<Type>();

        /// <summary>
        ///     If true, server will write response bodies into trace logs
        /// </summary>
        public bool LogResponseBodies { get; set; } = true;

        /// <summary>
        ///     Max response body size to write to trace logs. Negative values mean no limit
        /// </summary>
        public int ResponseBodyLogLimit { get; set; } = -1;

        /// <summary>
        ///     Max request body size to write to trace logs. Negative values mean no limit
        /// </summary>
        public int RequestBodyLogLimit { get; set; } = -1;

        /// <summary>
        ///     Message body parts patterns that must NEVER be logger. Item1 will be replaced with Item2 (Regex support)
        /// </summary>
        public List<Tuple<string, string>> LogBodyReplacePatterns { get; set; } = new List<Tuple<string, string>>();

        /// <summary>
        ///     Headers which should NEVER be logged
        /// </summary>
        public List<string> LogProhibitedHeaders { get; set; } = new List<string>();

        /// <summary>
        ///     Query params which should NEVER be logged
        /// </summary>
        public List<string> LogProhibitedQueryParams { get; set; } = new List<string>();

        /// <summary>
        ///     If true, then unauthorized /statistics requests are enabled
        /// </summary>
        public bool StatisticsEnabled { get; set; }

        /// <summary>
        ///     Method used to grand or deny access to /statistics uri
        /// </summary>
        public Delegates.StatisticsAuthorizer StatisticsAuthorizer { get; set; }

        /// <summary>
        ///     Favicon requests (Just for fun)
        /// </summary>
        public string FaviconPath { get; set; }

        /// <summary>
        ///     Used for Server: header
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        ///     Warning will be given if request handle workes for more milliseconds. Negatime values mean infinity
        /// </summary>
        public double RequestMaxServeTime { get; set; } = -1;

        /// <summary>
        ///     Memory size in megabytes after which warnings will be thrown. Negative values mean never
        /// </summary>
        public int CriticalMemoryValue { get; set; } = -1;

        /// <summary>
        ///     Memory pressure alarm interval startegy
        /// </summary>
        public MemoryAlarmStrategy MemoryAlarmStrategy { get; set; } = MemoryAlarmStrategy.Fibonacci;

        public bool IsEnough()
        {
            if (SubjectName == null)
            {
                LogMessage(LogLevel.Warning, "You must specify server subject");
                return false;
            }

            if (BodyEncoders == null || BodyEncoders.Count == 0 || BodyEncoders.Count(be => be.IsDefault) > 1)
            {
                LogMessage(LogLevel.Warning, "Bad BodyEncoders passed to Start()");
                return false;
            }

            if (NonSerializableTypes == null)
            {
                LogMessage(LogLevel.Warning, "NonSerializableTypes cannot be null");
                return false;
            }

            if (FilesNeedAuthorization && FilesAuthorizer == null)
            {
                LogMessage(LogLevel.Warning, "No files authorizer passed to Start()");
                return false;
            }

            if (FilesEnabled && FileSections.Count == 0)
            {
                LogMessage(LogLevel.Warning, "No file sections passed to Start()");
                return false;
            }

            if (FilesEnabled && FilesBaseUri == null)
            {
                LogMessage(LogLevel.Warning, "No files base uri passed to Start()");
                return false;
            }

            if (FilesEnabled && FilesLocation == null)
            {
                LogMessage(LogLevel.Warning, "No file location passed to Start()");
                return false;
            }

            if (FilesPreprocessingEnabled && FilesPreprocessorThreads == 0)
            {
                LogMessage(LogLevel.Warning, "Cannot use zero threads for files preprocessing");
                return false;
            }

            if (!Enum.IsDefined(typeof(MemoryAlarmStrategy), MemoryAlarmStrategy))
            {
                LogMessage(LogLevel.Warning, "Invalid memeory alarm strategy");
                return false;
            }

            return true;
        }

        internal IEnumerable<string> GetReservedUris()
        {
            if (!IsEnough())
                throw new InvalidOperationException("Configuration is inconsistent");

            var result = new List<string>();
            if (Authentificator != null)
                result.Add("login");
            if (FilesEnabled)
                result.Add(FilesBaseUri.Trim('/'));
            if (StatisticsEnabled)
                result.Add("statistics");
            result.Add("ping");

            return result;
        } 

        private void LogMessage(LogLevel level, string message) => Logger.LogEntry("SERVERCONFIG", level, message);
    }
}
