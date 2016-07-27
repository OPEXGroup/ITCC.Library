using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal class VideoRequest : BaseFileRequest
    {
        #region BaseFileRequest
        public override FileType FileType => FileType.Video;

        public override string FileName { get; protected set; }

        public override RequestRange Range { get; protected set; }

        public override bool Build(string fileName, HttpRequest request)
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
        #endregion

        #region public
        public int? Width { get; private set; }

        public int? Height { get; private set; }

        public int? Diagonal { get; private set; }
        #endregion
    }
}
