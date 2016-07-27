using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Enums;
using ITCC.Logging;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal class ImageRequest : BaseFileRequest
    {
        #region BaseFileRequest
        public override FileType FileType => FileType.Image;

        public override string FileName { get; protected set; }

        public override RequestRange Range { get; protected set; }

        public override bool BuildRequest(string fileName, HttpRequest request)
        {
            FileName = fileName;

            if (!ParseRange(request))
                return false;

            if (!ParseIntParam(request, "width", w => Width = w))
                return false;

            if (!ParseIntParam(request, "height", h => Height = h))
                return false;

            if (!ParseIntParam(request, "diagonal", d => Diagonal = d))
                return false;

            return true;
        }

        public override async Task<HttpResponse> BuildResponse()
        {
            if (Diagonal == null && Width == null && Height == null)
                return await base.BuildResponse();

            var availableFiles = IoHelper.LoadAllChanged(FileName);
            if (availableFiles == null || availableFiles.Count == 0)
            {
                LogMessage(LogLevel.Debug, $"No preprocessed images found for {FileName}");
                return await base.BuildResponse();
            }

            var resolutionDict = new Dictionary<Tuple<int, int>, string>();
            foreach (var file in availableFiles)
            {
                var resolution = GetFileResolution(file);
                if (Equals(resolution, InvalidResolutionTuple()))
                    continue; 
                resolutionDict.Add(resolution, file);
            }
            if (!resolutionDict.Any())
            {
                LogMessage(LogLevel.Debug, $"No additional resolutions found for image {FileName}");
                return await base.BuildResponse();
            }

            double requestedDiagonal;
            if (Diagonal != null)
                requestedDiagonal = Diagonal.Value;
            else
            {
                var maybeWidth = Width ?? 0;
                var maybeHeight = Height ?? 0;
                requestedDiagonal = Math.Sqrt(maybeWidth*maybeWidth + maybeHeight*maybeHeight);
            }

            var minDiff = double.PositiveInfinity;
            string fileName = FileName;
            foreach (var item in resolutionDict)
            {
                var diff = Math.Abs(requestedDiagonal - GetDiagonal(item.Key));
                if (diff < minDiff)
                {
                    minDiff = diff;
                    fileName = item.Value;
                }
            }
            LogMessage(LogLevel.Debug, $"Returning content of {fileName}");
            return BuildRangeResponse(fileName);
        }

        #endregion

        #region public
        public int? Width { get; private set; }

        public int? Height { get; private set; }

        public int? Diagonal { get; private set; }
        #endregion
    }
}
