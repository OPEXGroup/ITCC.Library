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

            //StartServer();

            //var requestList = new List<string>
            //{
            //    "Большой Симоновский переулок, 11"
            //};

            //try
            //{
            //    foreach (var request in requestList)
            //    {
            //        var result = await YandexGeocoder.GeocodeAsync(request, 10, LangType.ru_RU);
            //        Logger.LogEntry("TEST", LogLevel.Debug, $"Got {result.Count} result");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogException("TEST", LogLevel.Warning, ex);
            //}
            IGeocoder geocoder = new GoogleGeocoder() { };
            IEnumerable<Address> addresses = geocoder.Geocode("Большой Симоновский переулок, 11");
            Console.WriteLine("Formatted: " + addresses.First().FormattedAddress); //Formatted: 1600 Pennslyvania Avenue Northwest, Presiden'ts Park, Washington, DC 20500, USA
            Console.WriteLine("Coordinates: " + addresses.First().Coordinates.Latitude + ", " + addresses.First().Coordinates.Longitude); //Coordinates: 38.8978378, -77.0365123

            Console.ReadLine();

            //await Task.Delay(100000);
            //Logger.LogEntry("MAIN", LogLevel.Info, "Finished");
        }

        private static bool InitializeLoggers()
        {
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger(), true);

            if (!Directory.Exists("Log"))
            {
                try
                {
                    Directory.CreateDirectory("Log");
                }
                catch (Exception ex)
                {
                    Logger.LogException("CONFIG", LogLevel.Critical, ex);
                    return false;
                }
            }
            Logger.RegisterReceiver(
                new BufferedRotatingFileLogger($"Log\\TestLog", LogLevel.Trace, 10, 1024 * 1024, 10000), true);

            var emailLogger = new EmailLogger(LogLevel.Error, new EmailLoggerConfiguration
            {
                FlushLevel = LogLevel.Critical,
                Login = "yobalawson@outlook.com",
                Password = "PASSWORD",
                Receivers = new List<string> { "b0-0b@yandex.ru" },
                ReportPeriod = 60,
                Sender = "ISENGARD@itcc.company",
                SmtpHost = "smtp-mail.outlook.com",
                SmptPort = 587,
                Subject = "ISENGARD",
                SendEmptyReports = true,
                MaxQueueSize = 50
            });
            // emailLogger.Start();
            // Logger.RegisterReceiver(emailLogger);

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
