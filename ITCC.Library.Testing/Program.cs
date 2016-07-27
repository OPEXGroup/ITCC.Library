using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ITCC.HTTP.Client;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Server;
using ITCC.Library.Testing.Networking;
using ITCC.Logging;
using ITCC.Logging.Loggers;
using Newtonsoft.Json;

namespace ITCC.Library.Testing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            Thread.CurrentThread.Name = "MAIN";
            if (!InitializeLoggers())
                return;

            Logger.LogEntry("MAIN", LogLevel.Info, "Started");

            StartServer();

            StaticClient.ServerAddress = "http://localhost:8888";

            Console.ReadLine();
            StopServer();
            Logger.LogEntry("MAIN", LogLevel.Info, "Finished");
        }

        private static bool InitializeLoggers()
        {
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger(), true);

            return true;
        }

        private static void StartServer()
        {
            StaticServer<object>.Start(new HttpServerConfiguration<object>
            {
                BodyEncoding = Encoding.UTF8,
                BodySerializer = JsonConvert.SerializeObject,
                Port = 8888,
                Protocol = Protocol.Http,
                ServerName = "ITCC Test",
                StatisticsEnabled = true,
                SubjectName = "localhost",
                AutoGzipCompression = true,
                FilesEnabled = true,
                FilesNeedAuthorization = false,
                FilesBaseUri = "files",
                FileSections = new List<FileSection>
                {
                    new FileSection
                    {
                        Folder = "Pictures",
                        MaxFileSize = -1,
                        Name = "Pictures"
                    }
                },
                FilesLocation = @"C:\Users\vladimir.tyrin",
                FilesPreprocessingEnabled = true,
                FilesPreprocessorThreads = 1
            });

            StaticServer<object>.AddRequestProcessor(new RequestProcessor<object>
            {
                AuthorizationRequired = false,
                Handler = async (o, request) =>
                {
                    int delay;
                    var delayString = request.QueryString["value"];
                    if (delayString != null)
                    {
                        try
                        {
                            delay = Convert.ToInt32(delayString);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException("HANDLER", LogLevel.Debug, ex);
                            return new HandlerResult(HttpStatusCode.BadRequest, "Bad delay value!");
                        }
                    }
                    else
                    {
                        delay = 5;
                    }
                    Logger.LogEntry("HANDLER", LogLevel.Trace, $"Will sleep for {delay} seconds, then respond to {request.RemoteEndPoint}");
                    await Task.Delay(delay * 1000);
                    return new HandlerResult(HttpStatusCode.OK, "Hello ^_^");
                },
                Method = HttpMethod.Get,
                SubUri = "delay"
            });

            StaticServer<object>.AddRequestProcessor(new RequestProcessor<object>
            {
                AuthorizationRequired = false,
                Method = HttpMethod.Get,
                SubUri = "bigdata",
                Handler = (account, request) =>
                {
                    var builder = new StringBuilder(64 * 1024 * 1024);
                    for (var i = 0; i < 1024; ++i)
                    {
                        var str = string.Empty;
                        for (var j = 0; j < 1024; ++j)
                            str += "12345678901234567890123456789012";
                        builder.Append(str);
                    }
                    return Task.FromResult(new HandlerResult(HttpStatusCode.OK, builder.ToString()));
                }
            });

            StaticServer<object>.AddStaticRedirect("test", "delay");
        }

        private static void StopServer()
        {
            StaticServer<object>.Stop();
        }
    }
}
