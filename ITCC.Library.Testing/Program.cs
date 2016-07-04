using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Geocoding;
using Geocoding.Google;
using ITCC.Geocoding;
using ITCC.Geocoding.Enums;
using ITCC.Geocoding.Yandex;
using ITCC.Geocoding.Yandex.Enums;
using ITCC.HTTP.Client;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Server;
using ITCC.Logging;
using ITCC.Logging.Loggers;
using ITCC.Logging.Utils;
using Newtonsoft.Json;

namespace ITCC.Library.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            if (!InitializeLoggers())
                return;

            Logger.LogEntry("MAIN", LogLevel.Info, "Started");

            StartServer();

            Console.ReadLine();
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
        }

        private static void StopServer()
        {
            StaticServer<object>.Stop();
        }
    }
}
