﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Encoders;
using ITCC.HTTP.Server.Enums;
using ITCC.HTTP.Server.Files;
using ITCC.HTTP.Server.Interfaces;
using ITCC.HTTP.Server.Testing.Utils;
using ITCC.HTTP.SslConfigUtil.Core.Enums;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Testing
{
    internal static class ServerController
    {
        public static bool Start()
        {
            var config = GetConfig();

            var startResult = StaticServer<AccountMock>.Start(config);
            Logger.LogEntry("SERV CONTROL", startResult == ServerStartStatus.Ok ? LogLevel.Info : LogLevel.Error, $"Start result: {startResult}");

            StaticServer<AccountMock>.AddRequestProcessor(new RequestProcessor<AccountMock>
            {
                AuthorizationRequired = false,
                Handler = (account, request) =>
                {
                    Logger.LogEntry("CONTENT", LogLevel.Info, $"{request.ContentType} (== null: {request.ContentType == null})");
                    return Task.FromResult(new HandlerResult(HttpStatusCode.OK, null));
                },
                SubUri = "content",
                Method = HttpMethod.Get
            });

            return startResult == ServerStartStatus.Ok;
        }

        public static void Stop() => StaticServer<AccountMock>.Stop();

        private static HttpServerConfiguration<AccountMock> GetConfig() => new HttpServerConfiguration<AccountMock>
        {
            Port = 8888,
            Protocol = Configuration.Protocol,
            AllowGeneratedCertificates = true,
            CertificateBindType = BindType.SubjectName,
            LogBodyReplacePatterns = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("(\"Token\":\")([\\w\\d]+)(\")", "$1REMOVED_FROM_LOG$3")
            },
            LogProhibitedQueryParams = new List<string> {"password"},
            LogProhibitedHeaders = new List<string> {"Authorization"},
            ServerName = "ITCC Test",
            StatisticsEnabled = true,
            SubjectName = "localhost",
            FilesEnabled = true,
            FilesNeedAuthorization = false,
            FilesBaseUri = "files",
            FileSections = new List<FileSection>
            {
                new FileSection
                {
                    Folder = "Test",
                    MaxFileSize = -1,
                    Name = "Test"
                },
                new FileSection
                {
                    Folder = "Test2",
                    MaxFileSize = -1,
                    Name = "Test2"
                }
            },
            FilesLocation = Configuration.FilesLocation,
            FilesPreprocessingEnabled = true,
            FilesCompressionEnabled = true,
            FilesPreprocessorThreads = -1,
            BodyEncoders = new List<IBodyEncoder>
            {
                new PlainTextBodyEncoder(),
                new JsonBodyEncoder(isDefault: true),
                new XmlBodyEncoder()
            },
            CriticalMemoryValue = 1024,
            RequestTracingEnabled = true,
            DebugLogsEnabled = true,
            MaxConcurrentRequests = 24,
            MaxRequestQueue = 1024,
            ConfigurationViewEnabled = true
        };
    }
}
