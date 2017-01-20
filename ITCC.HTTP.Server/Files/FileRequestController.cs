// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using ITCC.HTTP.Common;
using ITCC.HTTP.Server.Auth;
using ITCC.HTTP.Server.Common;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Enums;
using ITCC.HTTP.Server.Files.Preprocess;
using ITCC.HTTP.Server.Files.Requests;
using ITCC.HTTP.Server.Interfaces;
using ITCC.HTTP.Server.Service;
using ITCC.HTTP.Server.Utils;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Files
{
    internal class FileRequestController<TAccount> : IServiceController, IDisposable
        where TAccount : class
    {
        #region IServiceRequestProcessor
        public bool RequestIsSuitable(HttpListenerRequest request)
        {
            if (request == null || !FilesEnabled)
            {
                return false;
            }
            return request.Url.LocalPath.Trim('/').StartsWith(FilesBaseUri);
        }

        public Task HandleRequestAsync(HttpListenerContext context) => HandleFileRequestAsync(context);

        public string Name => "Files";
        #endregion

        #region IDisposable

        public void Dispose()
        {
            _preprocessTimer.Stop();
            _preprocessTimer.Dispose();
        }

        #endregion

        #region config

        public bool Start(FileRequestControllerConfiguration<TAccount> configuration, ServerStatistics<TAccount> statistics)
        {
            FilesEnabled = configuration.FilesEnabled;
            FilesBaseUri = configuration.FilesBaseUri;
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

            if (FilesPreprocessingEnabled && FilesEnabled)
            {
                FilePreprocessController.Start(configuration.FilesPreprocessorThreads,
                    configuration.FilesCompressionEnabled);
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

        public void Stop()
        {
            if (FilesPreprocessingEnabled && FilesEnabled)
            {
                _preprocessTimer?.Stop();
                FilePreprocessController.Stop();
            }
            _started = false;
        }

        public bool FilesEnabled { get; private set; }
        public string FilesBaseUri { get; private set; }
        public string FilesLocation { get; private set; }
        public string FaviconPath { get; private set; }
        public bool FilesNeedAuthorization { get; private set; }
        public List<FileSection> FileSections { get; private set; }
        public bool FilesPreprocessingEnabled { get; private set; }
        public double ExistingFilesPreprocessingFrequency { get; private set; }

        private Delegates.FilesAuthorizer<TAccount> _filesAuthorizer;
        private ServerStatistics<TAccount> _statistics;
        private Timer _preprocessTimer;
        private bool _started;

        #endregion

        #region public
        public async Task HandleFileRequestAsync(HttpListenerContext context)
        {
            if (!_started)
            {
                ResponseFactory.BuildResponse(context, HttpStatusCode.NotImplemented, null);
                return;
            }

            var request = context.Request;
            try
            {
                var filename = ExtractFileName(request.Url.LocalPath);
                var section = ExtractFileSection(request.Url.LocalPath);
                if (filename == null || section == null || filename.Contains("_CHANGED_"))
                {
                    ResponseFactory.BuildResponse(context, HttpStatusCode.BadRequest, null);
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
                            await HandleFileGetRequestAsync(context, filePath).ConfigureAwait(false);
                            return;
                        }
                        if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Head)
                        {
                            await HandleFileHeadRequestAsync(context, filePath).ConfigureAwait(false);
                            return;
                        }
                        if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Post)
                        {
                            await HandleFilePostRequestAsync(context, section, filePath).ConfigureAwait(false);
                            return;
                        }
                        if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Delete)
                        {
                            HandleFileDeleteRequest(context, filePath);
                            return;
                        }
                        ResponseFactory.BuildResponse(context,  HttpStatusCode.MethodNotAllowed, null);
                        return;
                    default:
                        ResponseFactory.BuildResponse(context, authResult);
                        return;
                }
            }
            catch (Exception exception)
            {
                LogException(LogLevel.Warning, exception);
                ResponseFactory.BuildResponse(context, HttpStatusCode.InternalServerError, null);
            }
        }

        public async Task HandleFaviconAsync(HttpListenerContext context)
        {
            var response = context.Response;
            LogTrace($"Favicon requested, path: {FaviconPath}");
            if (string.IsNullOrEmpty(FaviconPath) || !File.Exists(FaviconPath))
            {
                ResponseFactory.BuildResponse(context, HttpStatusCode.NotFound, null);
                return;
            }
            ResponseFactory.BuildResponse(context, HttpStatusCode.OK, null);
            response.ContentType = "image/x-icon";
            using (var fileStream = new FileStream(FaviconPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite))
            {
                await fileStream.CopyToAsync(response.OutputStream).ConfigureAwait(false);
            }
        }

        public bool FileExists(string sectionName, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;
            var section = FileSections.FirstOrDefault(s => s.Name == sectionName);
            if (section == null)
            {
                LogDebug($"Section {sectionName} not found");
                return false;
            }

            var filePath = FilesLocation + Path.DirectorySeparatorChar + section.Folder + Path.DirectorySeparatorChar + filename;
            return File.Exists(filePath);
        }

        public Stream GetFileStream(string sectionName, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;
            var section = FileSections.FirstOrDefault(s => s.Name == sectionName);
            if (section == null)
            {
                LogDebug($"Section {sectionName} not found");
                return null;
            }

            var filePath = FilesLocation + Path.DirectorySeparatorChar + section.Folder + Path.DirectorySeparatorChar + filename;
            return File.Exists(filePath) ? new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) : null;
        }

        public async Task<string> GetFileStringAsync(string sectionName, string filename)
        {
            var stream = GetFileStream(sectionName, filename);
            if (stream == null)
                return null;

            using (stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<FileOperationStatus> AddFileAsync(string sectionName, string filename, Stream content)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return FileOperationStatus.BadParameters;
            var section = FileSections.FirstOrDefault(s => s.Name == sectionName);
            if (section == null)
            {
                LogDebug($"Section {sectionName} not found");
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
                    await file.FlushAsync();
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

        public FileOperationStatus DeleteFile(string sectionName, string filename)
        {
            var section = FileSections.FirstOrDefault(s => s.Name == sectionName);
            if (section == null)
            {
                LogDebug($"Section {sectionName} not found");
                return FileOperationStatus.NotFound;
            }

            if (filename.Contains("_CHANGED_"))
                return FileOperationStatus.BadParameters;
            var filePath = FilesLocation + Path.DirectorySeparatorChar + section.Folder + Path.DirectorySeparatorChar + filename;

            if (!File.Exists(filePath))
            {
                LogDebug($"File {filePath} does not exist and cannot be deleted");
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

        private async Task HandleFileGetRequestAsync(HttpListenerContext context, string filePath)
        {
            if (FilesPreprocessingEnabled && FilePreprocessController.FileInProgress(filePath))
            {
                LogDebug($"File {filePath} was requested but is in progress");
                ResponseFactory.BuildResponse(context, HttpStatusCode.ServiceUnavailable, null);
                return;
            }

            var fileRequest = FileRequestFactory.BuildRequest(filePath, context.Request);
            if (fileRequest == null) 
            {
                LogDebug($"Failed to build file request for {filePath}");
                ResponseFactory.BuildResponse(context, HttpStatusCode.BadRequest, null);
                return;
            }
            await fileRequest.BuildResponse(context);
        }

        private Task HandleFileHeadRequestAsync(HttpListenerContext context, string filePath)
        {
            if (FilesPreprocessingEnabled && FilePreprocessController.FileInProgress(filePath))
            {
                LogDebug($"File {filePath} was requested but is in progress");
                ResponseFactory.BuildResponse(context, HttpStatusCode.ServiceUnavailable, null);
                return Task.FromResult(0);
            }

            if (!File.Exists(filePath))
            {
                LogDebug($"File {filePath} was requested but was not found");
                ResponseFactory.BuildResponse(context, HttpStatusCode.NotFound, null);
                return Task.FromResult(0);
            }

            var response = context.Response;
            var fileInfo = new FileInfo(filePath);

            response.ContentLength64 = fileInfo.Length;
            response.StatusCode = (int) HttpStatusCode.OK;
            response.ContentType = MimeTypes.GetTypeByExtenstion(Path.GetExtension(filePath).Substring(1));

            return Task.FromResult(0);
        }

        private async Task HandleFilePostRequestAsync(HttpListenerContext context, FileSection section, string filePath)
        {
            if (File.Exists(filePath))
            {
                LogDebug($"File {filePath} already exists");
                ResponseFactory.BuildResponse(context, HttpStatusCode.Conflict, null);
                return;
            }
            var fileContent = context.Request.InputStream;
            var contentLength = context.Request.ContentLength64;
            if (section.MaxFileSize > 0 && contentLength > section.MaxFileSize)
            {
                LogDebug($"Trying to create file of size {contentLength} in section {section.Name} with max size of {section.MaxFileSize}");
                ResponseFactory.BuildResponse(context, HttpStatusCode.RequestEntityTooLarge, null);
                return;
            }
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                await fileContent.CopyToAsync(file).ConfigureAwait(false);
                await file.FlushAsync();
            }
            GC.Collect(1);
            LogTrace($"Total memory: {GC.GetTotalMemory(true)}");
            LogDebug($"File {filePath} created");
            if (FilesPreprocessingEnabled)
            {
                var code = FilePreprocessController.EnqueueFile(filePath) ? HttpStatusCode.Accepted : HttpStatusCode.Created;
                ResponseFactory.BuildResponse(context, code, null);
            }
            else
                ResponseFactory.BuildResponse(context, HttpStatusCode.Created, null);
        }

        private void HandleFileDeleteRequest(HttpListenerContext context, string filePath)
        {
            if (!File.Exists(filePath))
            {
                LogDebug($"File {filePath} does not exist and cannot be deleted");
                ResponseFactory.BuildResponse(context, HttpStatusCode.NotFound, null);
                return;
            }

            var compressed = IoHelper.LoadAllChanged(filePath);

            try
            {
                File.Delete(filePath);
                compressed.ForEach(File.Delete);
                ResponseFactory.BuildResponse(context, HttpStatusCode.OK, null);
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                ResponseFactory.BuildResponse(context, HttpStatusCode.InternalServerError, null);
            }
        }

        private string ExtractFileName(string localPath)
        {
            if (localPath == null)
                return null;
            try
            {
                var result = localPath.Trim('/');
                var slashIndex = result.LastIndexOf("/", StringComparison.Ordinal);
                result = result.Remove(0, slashIndex + 1);
                LogDebug($"File requested: {result}");
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private FileSection ExtractFileSection(string localPath)
        {
            if (localPath == null)
                return null;
            try
            {
                var sectionName = localPath.Trim('/');
                var slashIndex = sectionName.IndexOf("/", StringComparison.Ordinal);
                var lastSlashIndex = sectionName.LastIndexOf("/", StringComparison.Ordinal);
                sectionName = sectionName.Substring(slashIndex + 1, lastSlashIndex - slashIndex - 1);
                LogDebug($"Section requested: {sectionName}");
                return FileSections.FirstOrDefault(s => s.Folder == sectionName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void PreprocessExistingFiles()
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

        [Conditional("TRACE")]
        private static void LogTrace(string message) => Logger.LogTrace("HTTP FILES", message);

        [Conditional("DEBUG")]
        private static void LogDebug(string message) => Logger.LogDebug("HTTP FILES", message);

        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("HTTP FILES", level, message);

        private static void LogException(LogLevel level, Exception ex) => Logger.LogException("HTTP FILES", level, ex);

        #endregion

        
    }
}
