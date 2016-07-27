using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Enums;
using ITCC.Logging;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal abstract class BaseFileRequest
    {
        #region public
        public abstract FileType FileType { get; }

        public abstract string FileName { get; protected set; }

        public abstract RequestRange Range { get; protected set; }

        public abstract bool BuildRequest(string fileName, HttpRequest request);

        public virtual Task<HttpResponse> BuildResponse()
        {
            return Task.FromResult(BuildRangeResponse(FileName));
        }
        #endregion

        #region protected

        protected HttpResponse BuildRangeResponse(string fileName)
        {
            HttpResponse response;
            if (!File.Exists(fileName))
            {
                LogMessage(LogLevel.Debug, $"File {fileName} was requested but not found");
                response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
                return response;
            }
            if (Range == null)
            {
                response = ResponseFactory.CreateResponse(HttpStatusCode.OK, null);
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite);
                response.Body = fileStream;
                response.ContentType = DetermineContentType(fileName);
                return response;
            }
            var fileInfo = new FileInfo(fileName);
            long startPosition = 0;
            long endPosition = fileInfo.Length - 1;
            if (Range.RangeEnd != null)
            {
                var rangeEnd = Range.RangeEnd.Value;
                if (rangeEnd < 0)
                {
                    if (fileInfo.Length < -rangeEnd)
                    {
                        response = ResponseFactory.CreateResponse(HttpStatusCode.RequestedRangeNotSatisfiable, null,
                            new Dictionary<string, string>
                            {
                                {"Content-Range", $"bytes 0-{fileInfo.Length - 1}"}
                            });
                        return response;
                    }
                    startPosition = fileInfo.Length + rangeEnd;
                    endPosition = fileInfo.Length - 1;
                }
                if (rangeEnd > 0)
                {
                    if (fileInfo.Length < rangeEnd)
                    {
                        response = ResponseFactory.CreateResponse(HttpStatusCode.RequestedRangeNotSatisfiable, null,
                            new Dictionary<string, string>
                            {
                                {"Content-Range", $"bytes 0-{fileInfo.Length - 1}"}
                            });
                        return response;
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
                    buffer = reader.ReadBytes((int)length);
                }
            }
            response = ResponseFactory.CreateResponse(HttpStatusCode.PartialContent, null);
            response.AddHeader("Content-Range", $"bytes {startPosition}-{endPosition}");
            response.Body = new MemoryStream(buffer);

            response.ContentType = DetermineContentType(fileName);
            return response;
        }

        /// <summary>
        ///     This will return true if request is correct (not if range is found)
        /// </summary>
        /// <param name="request">Request to parse</param>
        /// <returns>False if 400 response is required</returns>
        protected bool ParseRange(HttpRequest request)
        {
            if (!request.Headers.Contains("Range"))
            {
                return true;
            }

            long rangeStart;
            long rangeEnd;

            var rangeValue = request.Headers["Range"];
            if (! rangeValue.StartsWith("bytes="))
                return false;
            rangeValue = rangeValue.Replace("bytes=", "");

            if (rangeValue.EndsWith("-"))
            {
                LogMessage(LogLevel.Trace, "Start range requested");
                if (!long.TryParse(rangeValue.TrimEnd('-'), out rangeStart))
                    return false;
                if (rangeStart < 0)
                    return false;
                Range = new RequestRange {RangeStart = rangeStart, RangeEnd = null};
                return true;
            }
            if (rangeValue.StartsWith("-"))
            {
                LogMessage(LogLevel.Trace, "End range requested");
                // rangeEnd will be negative. This is correct
                if (!long.TryParse(rangeValue, out rangeEnd))
                    return false;
                Range = new RequestRange { RangeStart = null, RangeEnd = rangeEnd };
                return true;
            }
            LogMessage(LogLevel.Trace, "Middle range requested");
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
            LogMessage(LogLevel.Trace, $"Range built: {rangeStart}-{rangeEnd}");
            Range = new RequestRange {RangeStart = rangeStart, RangeEnd = rangeEnd };
            return true;
        }

        protected bool ParseIntParam(HttpRequest request, string paramName, Action<int> callbackAction)
        {
            if (request.QueryString[paramName] == null)
                return true;

            int value;
            if (! int.TryParse(request.QueryString[paramName], out value))
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
            int width;
            int height;
            if (!int.TryParse(parts[0], out width))
                return InvalidResolutionTuple();
            if (width < 0)
                return InvalidResolutionTuple();
            if (!int.TryParse(parts[1], out height))
                return InvalidResolutionTuple();
            if (height < 0)
                return InvalidResolutionTuple();
            return new Tuple<int, int>(width, height);
        }

        protected static string DetermineContentType(string filename)
        {
            var extension = IoHelper.GetExtension(filename);
            if (extension == null)
                return "x-application/unknown";

            return MimeTypes.GetTypeByExtenstion(extension);
        }

        protected static Tuple<int, int> InvalidResolutionTuple() => new Tuple<int, int>(-1, -1);

        protected double GetDiagonal(Tuple<int, int> size) => Math.Sqrt(size.Item1*size.Item1 + size.Item2*size.Item2);

        #endregion

        #region log

        protected void LogMessage(LogLevel level, string message) => Logger.LogEntry($"{FileType.ToString().ToUpper()} RQST", level, message);

        protected void LogException(LogLevel level, Exception exception) => Logger.LogException($"{FileType.ToString().ToUpper()} RQST", level, exception);

        #endregion
    }
}
