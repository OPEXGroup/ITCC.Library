using System.Net;
using ITCC.HTTP.Server.Enums;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal class DefaultFileRequest : BaseFileRequest
    {
        #region BaseFileRequest
        public override FileType FileType => FileType.Default;

        public override string FileName { get; protected set; }

        public override RequestRange Range { get; protected set; }

        public override bool BuildRequest(string fileName, HttpListenerRequest request)
        {
            FileName = fileName;

            if (!ParseRange(request))
                return false;

            return true;
        }

        #endregion
    }
}
