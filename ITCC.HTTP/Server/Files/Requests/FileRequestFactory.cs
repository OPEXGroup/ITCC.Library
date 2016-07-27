using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Server.Files.Preprocess;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal static class FileRequestFactory
    {
        #region public
        public static BaseFileRequest BuildRequest(string fileName, HttpRequest request)
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
