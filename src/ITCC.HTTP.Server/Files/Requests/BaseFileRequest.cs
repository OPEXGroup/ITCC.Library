// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ITCC.HTTP.Common;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Enums;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal abstract class BaseFileRequest
    {
        #region const

        public const int LargeFileSizeThreshold = 8*1024*1024;
        public const int SmallFileBufferSize = 128*1024;
        public const int LargeFileBufferSize = 2*1024*1024;
        #endregion

        #region public

        public abstract FileType FileType { get; }
        public abstract string FileName { get; protected set; }
        public abstract RequestRange Range { get; protected set; }
        public abstract bool BuildRequest(string fileName, HttpListenerRequest request);

        public virtual Task BuildResponse(HttpListenerContext context)
            => BuildRangeResponse(context, FileName);

        #endregion

        #region protected

        protected async Task BuildRangeResponse(HttpListenerContext context, string fileName)
        {
            var response = context.Response;
            if (!File.Exists(fileName))
            {
                LogDebug($"File {fileName} was requested but not found");
                ResponseFactory.BuildResponse(context, HttpStatusCode.NotFound, null);
                return;
            }
            if (Range == null)
            {

                response.StatusCode = (int) HttpStatusCode.OK;
                response.ContentType = DetermineContentType(fileName);
                var fileLength = new FileInfo(fileName).Length;
                response.ContentLength64 = fileLength;
                ResponseFactory.SerializeResponse(response, null);
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    try
                    {
                        var bufferSize = fileLength >= LargeFileSizeThreshold
                            ? LargeFileBufferSize
                            : SmallFileBufferSize;
                        await fileStream.CopyToAsync(response.OutputStream, bufferSize);
                    }
                    catch (HttpListenerException ex)
                    {
                        LogMessage(LogLevel.Debug, $"Error: {ex.Message}");
                    }
                }
                return;
            }
            var fileInfo = new FileInfo(fileName);
            long startPosition = 0;
            var endPosition = fileInfo.Length - 1;
            if (Range.RangeEnd != null)
            {
                var rangeEnd = Range.RangeEnd.Value;
                if (rangeEnd < 0)
                {
                    if (fileInfo.Length < -rangeEnd)
                    {
                        response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                        response.AddHeader("Content-Range", $"bytes 0-{fileInfo.Length - 1}");
                        ResponseFactory.SerializeResponse(response, null);
                        return;
                    }
                    startPosition = fileInfo.Length + rangeEnd;
                    endPosition = fileInfo.Length - 1;
                }
                if (rangeEnd > 0)
                {
                    if (fileInfo.Length < rangeEnd)
                    {
                        response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                        response.AddHeader("Content-Range", $"bytes 0-{fileInfo.Length - 1}");
                        ResponseFactory.SerializeResponse(response, null);
                        return;
                    }
                    endPosition = rangeEnd;
                }
            }
            if (Range.RangeStart != null)
            {
                startPosition = Range.RangeStart.Value;
            }

            byte[] buffer;
            var length = endPosition - startPosition + 1;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);
                    buffer = reader.ReadBytes((int) length);
                }
            }

            response.StatusCode = (int) HttpStatusCode.PartialContent;
            response.AddHeader("Content-Range", $"bytes {startPosition}-{endPosition}");
            response.ContentType = DetermineContentType(fileName);
            response.ContentLength64 = buffer.Length;
            ResponseFactory.SerializeResponse(response, null);
            try
            {
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (HttpListenerException ex)
            {
                LogMessage(LogLevel.Debug, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        ///     This will return true if request is correct (not if range is found)
        /// </summary>
        /// <param name="request">Request to parse</param>
        /// <returns>False if 400 response is required</returns>
        protected bool ParseRange(HttpListenerRequest request)
        {
            if (!request.Headers.AllKeys.Contains("Range"))
            {
                return true;
            }

            long rangeStart;
            long rangeEnd;

            var rangeValue = request.Headers["Range"];
            if (! rangeValue.StartsWith("bytes="))
                return false;
            rangeValue = rangeValue.Replace("bytes=", "");
            if (rangeValue == "0-")
                return true;

            if (rangeValue.EndsWith("-"))
            {
                LogTrace("Start range requested");
                if (!long.TryParse(rangeValue.TrimEnd('-'), out rangeStart))
                    return false;
                if (rangeStart < 0)
                    return false;
                Range = new RequestRange {RangeStart = rangeStart, RangeEnd = null};
                return true;
            }
            if (rangeValue.StartsWith("-"))
            {
                LogTrace("End range requested");
                // rangeEnd will be negative. This is correct
                if (!long.TryParse(rangeValue, out rangeEnd))
                    return false;
                Range = new RequestRange { RangeStart = null, RangeEnd = rangeEnd };
                return true;
            }
            LogTrace("Middle range requested");
            var parts = rangeValue.Split('-');
            if (parts.Length != 2)
                return false;
            if (!long.TryParse(parts[0], out rangeStart))
                return false;
            if (rangeStart < 0)
                return false;
            if (!long.TryParse(parts[1], out rangeEnd))
                return false;
            // Here rangeEnd MUST be greater than start
            if (rangeEnd <= rangeStart)
                return false;
            LogTrace($"Range built: {rangeStart}-{rangeEnd}");
            Range = new RequestRange {RangeStart = rangeStart, RangeEnd = rangeEnd };
            return true;
        }

        protected bool ParseIntParam(HttpListenerRequest request, string paramName, Action<int> callbackAction)
        {
            if (request.QueryString[paramName] == null)
                return true;

            if (!int.TryParse(request.QueryString[paramName], out int value))
                return false;
            try
            {
                callbackAction.Invoke(value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected Tuple<int, int> GetFileResolution(string fileName)
        {
            if (FileType != FileType.Image && FileType != FileType.Video)
                return InvalidResolutionTuple();

            var changedIndex = fileName.LastIndexOf(Constants.ChangedString, StringComparison.Ordinal);
            var endPart = fileName.Remove(0, changedIndex + Constants.ChangedString.Length);
            var targetPart = Path.GetFileNameWithoutExtension(endPart);
            
            var parts = targetPart.Split('x');
            if (parts.Length != 2)
                return InvalidResolutionTuple();
            if (!int.TryParse(parts[0], out int width))
                return InvalidResolutionTuple();
            if (width < 0)
                return InvalidResolutionTuple();
            if (!int.TryParse(parts[1], out int height))
                return InvalidResolutionTuple();
            if (height < 0)
                return InvalidResolutionTuple();
            return new Tuple<int, int>(width, height);
        }

        protected static string DetermineContentType(string filename)
        {
            var extension = IoHelper.GetExtension(filename);
            if (string.IsNullOrEmpty(extension))
                return "x-application/unknown";

            return MimeTypes.GetTypeByExtenstion(extension);
        }

        protected static Tuple<int, int> InvalidResolutionTuple() => new Tuple<int, int>(-1, -1);

        protected double GetDiagonal(Tuple<int, int> size) => Math.Sqrt(size.Item1*size.Item1 + size.Item2*size.Item2);

        #endregion

        #region log

        protected void LogTrace(string message) => Logger.LogTrace($"{FileType.ToString().ToUpper()} RQST", message);
        protected void LogDebug(string message) => Logger.LogDebug($"{FileType.ToString().ToUpper()} RQST", message);
        protected void LogMessage(LogLevel level, string message) => Logger.LogEntry($"{FileType.ToString().ToUpper()} RQST", level, message);
        protected void LogException(LogLevel level, Exception exception) => Logger.LogException($"{FileType.ToString().ToUpper()} RQST", level, exception);

        #endregion
    }
}
