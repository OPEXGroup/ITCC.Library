// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Net;
using ITCC.HTTP.Server.Enums;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal static class FileRequestFactory
    {
        #region public
        public static BaseFileRequest BuildRequest(string fileName, HttpListenerRequest request)
        {
            var type = FileTypeSelector.GetFileTypeByName(fileName);
            BaseFileRequest result;
            switch (type)
            {
                case FileType.Default:
                    result = new DefaultFileRequest();
                    break;
                case FileType.Image:
                    result = new ImageRequest();
                    break;
                case FileType.Video:
                    result = new VideoRequest();
                    break;
                default:
                    return null;
            }

            if (!result.BuildRequest(fileName, request))
                return null;
            return result;
        }
        #endregion
    }
}
