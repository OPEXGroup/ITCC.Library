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

            return await base.BuildResponse();
        }

        #endregion

        #region public
        public int? Width { get; private set; }

        public int? Height { get; private set; }

        public int? Diagonal { get; private set; }
        #endregion
    }
}
