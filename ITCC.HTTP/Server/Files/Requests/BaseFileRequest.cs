using System;
using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal abstract class BaseFileRequest
    {
        #region public
        public abstract FileType FileType { get; }

        public abstract string FileName { get; protected set; }

        public abstract RequestRange Range { get; protected set; }

        public abstract bool Build(string fileName, HttpRequest request);
        #endregion

        #region protected

        /// <summary>
        ///     This will return true if request is correct (not if range is found)
        /// </summary>
        /// <param name="request">Request to parse</param>
        /// <returns>False if 400 response is required</returns>
        protected bool ParseRange(HttpRequest request)
        {
            if (!request.Headers.Contains("Range"))
            {
                Range = new RequestRange {RangeStart = null, RangeEnd = null};
                return true;
            }

            long rangeStart;
            long rangeEnd;

            var rangeValue = request.Headers["Range"];
            if (! rangeValue.StartsWith("bytes="))
                return false;

            if (rangeValue.EndsWith("-"))
            {
                if (!long.TryParse(rangeValue.TrimEnd('-'), out rangeStart))
                    return false;
                if (rangeStart < 0)
                    return false;
                Range = new RequestRange {RangeStart = rangeStart, RangeEnd = null};
            }
            if (rangeValue.StartsWith("-"))
            {
                // rangeEnd will be negative. This is correct
                if (!long.TryParse(rangeValue, out rangeEnd))
                    return false;
                Range = new RequestRange { RangeStart = null, RangeEnd = rangeEnd };
            }
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
        #endregion
    }
}
