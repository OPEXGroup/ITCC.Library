using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ITCC.Logging.Core;

namespace ITCC.HTTP.SslConfigUtil.Core
{
    internal static class AssemblyLoader
    {
        public struct GetAssemblyGuidResult
        {
            public LoadAssemblyStatus Status;
            public string Guid;
        }
        public static GetAssemblyGuidResult GetAssemblyGuid(string assymblyFilePath)
        {
            LogDebug("Loading started.");

            var maxPathField = typeof(Path).GetField("MaxPath",
                BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
            if (maxPathField == null)
            {
                LogDebug("Unable to get MaxPathLength.");
                return new GetAssemblyGuidResult { Status = LoadAssemblyStatus.UnknownError };
            }

            var maxPathLength = (int)maxPathField.GetValue(null);

            if (string.IsNullOrWhiteSpace(assymblyFilePath))
            {
                LogDebug("Filpath is null or white space.");
                return new GetAssemblyGuidResult { Status = LoadAssemblyStatus.IsNullOrWhiteSpace };
            }
            if (assymblyFilePath.Length > maxPathLength)
            {
                LogDebug($"Filepath is too long ({assymblyFilePath.Length} > {maxPathLength}).");
                return new GetAssemblyGuidResult { Status = LoadAssemblyStatus.PathTooLong };
            }
            if (!Directory.Exists(Path.GetDirectoryName(assymblyFilePath)))
            {
                LogDebug("Directory is not exists.");
                return new GetAssemblyGuidResult { Status = LoadAssemblyStatus.DirectoryNotFound };
            }
            if (!File.Exists(assymblyFilePath))
            {
                LogDebug("Assembly not found.");
                return new GetAssemblyGuidResult { Status = LoadAssemblyStatus.FileNotFound };
            }

            try
            {
                LogDebug("Loading assembly");
                var assembly = Assembly.LoadFile(assymblyFilePath);
                var result = new GetAssemblyGuidResult
                {
                    Status = LoadAssemblyStatus.Ok,
                    Guid = ((GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value
                };
                LogDebug("Assembly loaded");
                return result;
            }
            catch (FileLoadException ex)
            {
                LogDebug(ex.Message);
                return new GetAssemblyGuidResult { Status = LoadAssemblyStatus.AccessDenied };
            }
            catch (BadImageFormatException ex)
            {
                LogDebug(ex.Message);
                return new GetAssemblyGuidResult { Status = LoadAssemblyStatus.BadAssemblyFormat };
            }
            catch (Exception ex)
            {
                LogException(ex);
                return new GetAssemblyGuidResult { Status = LoadAssemblyStatus.UnknownError };
            }
        }

        private static void LogDebug(string message) => Logger.LogEntry("AssemblyLoader", LogLevel.Debug, message);
        private static void LogException(Exception exception) => Logger.LogException("AssemblyLoader", LogLevel.Debug, exception);
    }
}