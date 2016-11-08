using ITCC.HTTP.Server.Testing.Enums;
using ITCC.HTTP.Common.Enums;
using ITCC.Logging.Core;
using System;
using System.IO;
using System.Configuration;
using System.Threading;

namespace ITCC.HTTP.Server.Testing.Utils
{
    internal static class Configuration
    {
        public static bool ReadAppConfig()
        {
            try
            {
                Protocol = (Protocol)Enum.Parse(typeof(Protocol), ConfigurationManager.AppSettings["Protocol"]);
                ServerPort = Convert.ToUInt16(ConfigurationManager.AppSettings["Port"]);
                AutoGzipCompression = Convert.ToBoolean(ConfigurationManager.AppSettings["AutoGzipCompression"]);

                LoggerMode =
                    (LoggerMode)Enum.Parse(typeof(LoggerMode), ConfigurationManager.AppSettings["LoggerMode"]);
                LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), ConfigurationManager.AppSettings["LogLevel"]);

                LogDirectory = Environment.CurrentDirectory + "\\Log";
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                FilesLocation = ConfigurationManager.AppSettings["FilesLocation"];
                if (!Directory.Exists(FilesLocation))
                {
                    try
                    {
                        Directory.CreateDirectory(FilesLocation);
                        Directory.CreateDirectory(FilesLocation + Path.PathSeparator + "Test");
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }

                WorkerThreads = Math.Max(Convert.ToInt32(ConfigurationManager.AppSettings["WorkerThreads"]), Environment.ProcessorCount);
                IocpThreads = Math.Max(Convert.ToInt32(ConfigurationManager.AppSettings["IocpThreads"]), Environment.ProcessorCount);
                Logger.LogEntry("CONFIGURATION", LogLevel.Info, $"Max worker threads: {WorkerThreads}; Max IOCP threads: {IocpThreads}");
                ThreadPool.SetMaxThreads(WorkerThreads, IocpThreads);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        #region http
        public static Protocol Protocol { get; private set; }
        public static ushort ServerPort { get; private set; }
        public static bool AutoGzipCompression { get; private set; }

        #endregion

        #region files
        public static string FilesLocation { get; private set; }
        #endregion

        #region log
        public static string LogDirectory { get; private set; }
        public static LoggerMode LoggerMode { get; private set; }
        public static LogLevel LogLevel { get; private set; }
        #endregion

        #region Threading
        public static int WorkerThreads { get; private set; }
        public static int IocpThreads { get; private set; }
        #endregion
    }
}
