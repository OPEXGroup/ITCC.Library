using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using ITCC.HTTP.Common;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Server.Files.Preprocess;
using ITCC.HTTP.Server.Files.Requests;
using ITCC.HTTP.Utils;
using ITCC.Logging;

namespace ITCC.HTTP.Server.Files
{
    internal static class FileRequestController<TAccount>
        where TAccount : class
    {
        #region config

        public static bool Start(FileRequestControllerConfiguration<TAccount> configuration, ServerStatistics<TAccount> statistics)
        {
            FilesLocation = configuration.FilesLocation;
            FaviconPath = configuration.FaviconPath;
            FilesNeedAuthorization = configuration.FilesNeedAuthorization;
            FileSections = configuration.FileSections;
            FilesPreprocessingEnabled = configuration.FilesPreprocessingEnabled;
            ExistingFilesPreprocessingFrequency = configuration.ExistingFilesPreprocessingFrequency;
            _filesAuthorizer = configuration.FilesAuthorizer;
            _statistics = statistics;

            if (!IoHelper.HasWriteAccessToDirectory(FilesLocation))
            {
                LogMessage(LogLevel.Warning, $"Cannot use file folder {FilesLocation} : no write access");
                return false;
            }

            if (FilesPreprocessingEnabled)
            {
                FilePreprocessController.Start(configuration.FilesPreprocessorThreads);
                if (ExistingFilesPreprocessingFrequency > 0)
                {
                    PreprocessExistingFiles();
                    _preprocessTimer = new Timer(1000 * ExistingFilesPreprocessingFrequency);
                    _preprocessTimer.Elapsed += (sender, args) => PreprocessExistingFiles();
                    _preprocessTimer.Start();
                }
            }

            _started = true;
            return true;
        }

        public static void Stop()
        {
            if (FilesPreprocessingEnabled)
            {
                _preprocessTimer?.Stop();
                FilePreprocessController.Stop();
            }
            _started = false;
        }

        public static string FilesLocation { get; private set; }
        public static string FaviconPath { get; private set; }
        public static bool FilesNeedAuthorization { get; private set; }
        public static List<FileSection> FileSections { get; private set; }
        public static bool FilesPreprocessingEnabled { get; private set; }
        public static double ExistingFilesPreprocessingFrequency { get; private set; }

        private static Delegates.FilesAuthorizer<TAccount> _filesAuthorizer;
        private static ServerStatistics<TAccount> _statistics;
        private static Timer _preprocessTimer;
        private static bool _started;

        #endregion

        #region public
        public static async Task HandleFileRequest(HttpListenerContext context)
        {
            if (!_started)
            {
                ResponseFactory.BuildResponse(context.Response, HttpStatusCode.NotImplemented, null);
                return;
            }

            var request = context.Request;
            try
            {
                var filename = ExtractFileName(request.Url.LocalPath);
                var section = ExtractFileSection(request.Url.LocalPath);
                if (filename == null || section == null || filename.Contains("_CHANGED_"))
                {
                    ResponseFactory.BuildResponse(context.Response, HttpStatusCode.BadRequest, null);
                    return;
                }
                var filePath = FilesLocation + Path.DirectorySeparatorChar + section.Folder + Path.DirectorySeparatorChar + filename;

                AuthorizationResult<TAccount> authResult;
                if (FilesNeedAuthorization)
                {
                    authResult = await _filesAuthorizer.Invoke(request, section, filename).ConfigureAwait(false);
                }
                else
                {
                    authResult = new AuthorizationResult<TAccount>(null, AuthorizationStatus.NotRequired);
                }
                _statistics?.AddAuthResult(authResult);
                switch (authResult.Status)
                {
                    case AuthorizationStatus.NotRequired:
                    case AuthorizationStatus.Ok:
                        if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Get)
                        {
                            await HandleFileGetRequest(context, filePath);
                            return;
                        }
                        if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Post)
                        {
                            await HandleFilePostRequest(context, section, filePath).ConfigureAwait(false);
                            return;
                        }
                        if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Delete)
                        {
                            HandleFileDeleteRequest(context, filePath);
                            return;
                        }
                        ResponseFactory.BuildResponse(context.Response,  HttpStatusCode.MethodNotAllowed, null);
                        return;
                    default:
                        ResponseFactory.BuildResponse(context.Response, authResult);
                        return;
                }
            }
            catch (Exception exception)
            {
                LogException(LogLevel.Warning, exception);
                ResponseFactory.BuildResponse(context.Response, HttpStatusCode.InternalServerError, null);
            }
        }

        public static async Task HandleFavicon(HttpListenerContext context)
        {
            var response = context.Response;
            LogMessage(LogLevel.Trace, $"Favicon requested, path: {FaviconPath}");
            if (string.IsNullOrEmpty(FaviconPath) || !File.Exists(FaviconPath))
            {
                ResponseFactory.BuildResponse(response, HttpStatusCode.NotFound, null);
                return;
            }
            ResponseFactory.BuildResponse(response, HttpStatusCode.OK, null);
            response.ContentType = "image/x-icon";
            using (var fileStream = new FileStream(FaviconPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite))
            {
                await fileStream.CopyToAsync(response.OutputStream);
            }
        }

        public static async Task<FileOperationStatus> AddFile(string sectionName, string filename, Stream content)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return FileOperationStatus.BadParameters;
            var section = FileSections.FirstOrDefault(s => s.Name == sectionName);
            if (section == null)
            {
                LogMessage(LogLevel.Debug, $"Section {sectionName} not found");
                return FileOperationStatus.NotFound;
            }
            if (content == null || !content.CanRead)
                return FileOperationStatus.BadParameters;

            if (filename.Contains("_CHANGED_"))
                return FileOperationStatus.BadParameters;
            var filePath = FilesLocation + Path.DirectorySeparatorChar + section.Folder + Path.DirectorySeparatorChar + filename;

            if (File.Exists(filePath))
                return FileOperationStatus.Conflict;

            try
            {
                using (var file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    await content.CopyToAsync(file).ConfigureAwait(false);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                return FileOperationStatus.Error;
            }

            if (FilesPreprocessingEnabled)
            {
                return FilePreprocessController.EnqueueFile(filePath)
                    ? FileOperationStatus.JobQueued
                    : FileOperationStatus.Ok;
            }
            return FileOperationStatus.Ok;
        }

        public static FileOperationStatus DeleteFile(string sectionName, string filename)
        {
            var section = FileSections.FirstOrDefault(s => s.Name == sectionName);
            if (section == null)
            {
                LogMessage(LogLevel.Debug, $"Section {sectionName} not found");
                return FileOperationStatus.NotFound;
            }

            if (filename.Contains("_CHANGED_"))
                return FileOperationStatus.BadParameters;
            var filePath = FilesLocation + Path.DirectorySeparatorChar + section.Folder + Path.DirectorySeparatorChar + filename;

            if (!File.Exists(filePath))
            {
                LogMessage(LogLevel.Debug, $"File {filePath} does not exist and cannot be deleted");
                return FileOperationStatus.NotFound;
            }

            var compressed = IoHelper.LoadAllChanged(filePath);

            try
            {
                File.Delete(filePath);
                compressed.ForEach(File.Delete);
                return FileOperationStatus.Ok;
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                return FileOperationStatus.Error;
            }
        }

        #endregion

        #region private

        private static async Task HandleFileGetRequest(HttpListenerContext context, string filePath)
        {
            var response = context.Response;
            if (FilesPreprocessingEnabled && FilePreprocessController.FileInProgress(filePath))
            {
                LogMessage(LogLevel.Debug, $"File {filePath} was requested but is in progress");
                ResponseFactory.BuildResponse(response, HttpStatusCode.ServiceUnavailable, null);
                return;
            }

            var fileRequest = FileRequestFactory.BuildRequest(filePath, context.Request);
            if (fileRequest == null) 
            {
                LogMessage(LogLevel.Debug, $"Failed to build file request for {filePath}");
                ResponseFactory.BuildResponse(response, HttpStatusCode.BadRequest, null);
                return;
            }
            await fileRequest.BuildResponse(context);
        }

        private static async Task HandleFilePostRequest(HttpListenerContext context, FileSection section, string filePath)
        {
            var response = context.Response;
            if (File.Exists(filePath))
            {
                LogMessage(LogLevel.Debug, $"File {filePath} already exists");
                ResponseFactory.BuildResponse(response, HttpStatusCode.Conflict, null);
                return;
            }
            var fileContent = context.Request.InputStream;
            if (section.MaxFileSize > 0 && fileContent.Length > section.MaxFileSize)
            {
                LogMessage(LogLevel.Debug, $"Trying to create file of size {fileContent.Length} in section {section.Name} with max size of {section.MaxFileSize}");
                ResponseFactory.BuildResponse(response, HttpStatusCode.RequestEntityTooLarge, null);
                return;
            }
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                await fileContent.CopyToAsync(file).ConfigureAwait(false);
                file.Flush();
                file.Close();
            }
            GC.Collect();
            LogMessage(LogLevel.Trace, $"Total memory: {GC.GetTotalMemory(true)}");
            LogMessage(LogLevel.Debug, $"File {filePath} created");
            if (FilesPreprocessingEnabled)
            {
                var code = FilePreprocessController.EnqueueFile(filePath) ? HttpStatusCode.Accepted : HttpStatusCode.Created;
                ResponseFactory.BuildResponse(context.Response, code, null);
            }
            else
                ResponseFactory.BuildResponse(context.Response, HttpStatusCode.Created, null);
        }

        private static void HandleFileDeleteRequest(HttpListenerContext context, string filePath)
        {
            var response = context.Response;
            if (!File.Exists(filePath))
            {
                LogMessage(LogLevel.Debug, $"File {filePath} does not exist and cannot be deleted");
                ResponseFactory.BuildResponse(response, HttpStatusCode.NotFound, null);
                return;
            }

            var compressed = IoHelper.LoadAllChanged(filePath);

            try
            {
                File.Delete(filePath);
                compressed.ForEach(File.Delete);
                ResponseFactory.BuildResponse(response, HttpStatusCode.OK, null);
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                ResponseFactory.BuildResponse(response, HttpStatusCode.InternalServerError, null);
            }
        }

        private static string ExtractFileName(string localPath)
        {
            if (localPath == null)
                return null;
            try
            {
                var result = localPath.Trim('/');
                var slashIndex = result.LastIndexOf("/", StringComparison.Ordinal);
                result = result.Remove(0, slashIndex + 1);
                LogMessage(LogLevel.Debug, $"File requested: {result}");
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static FileSection ExtractFileSection(string localPath)
        {
            if (localPath == null)
                return null;
            try
            {
                var sectionName = localPath.Trim('/');
                var slashIndex = sectionName.IndexOf("/", StringComparison.Ordinal);
                var lastSlashIndex = sectionName.LastIndexOf("/", StringComparison.Ordinal);
                sectionName = sectionName.Substring(slashIndex + 1, lastSlashIndex - slashIndex - 1);
                LogMessage(LogLevel.Debug, $"Section requested: {sectionName}");
                return FileSections.FirstOrDefault(s => s.Folder == sectionName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void PreprocessExistingFiles()
        {
            foreach (var section in FileSections)
            {
                var directory = Path.Combine(FilesLocation, section.Folder);
                foreach (var file in Directory.GetFiles(directory).Where(f => !f.Contains(Constants.ChangedString)))
                {
                    if (IoHelper.LoadAllChanged(file).Any())
                        continue;
                    FilePreprocessController.EnqueueFile(file);
                }
            }
        }

        #endregion

        #region log
        private static void LogMessage(LogLevel level, string message)
        {
            Logger.LogEntry("HTTP FILES", level, message);
        }

        private static void LogException(LogLevel level, Exception ex)
        {
            Logger.LogException("HTTP FILES", level, ex);
        }
        #endregion
    }
}
