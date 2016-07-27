using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Server.Files.Requests
{
    internal class DefaultFileRequest : BaseFileRequest
    {
        #region BaseFileRequest
        public override FileType FileType => FileType.Default;

        public override string FileName { get; protected set; }

        public override RequestRange Range { get; protected set; }

        public override bool BuildRequest(string fileName, HttpRequest request)
        {
            FileName = fileName;

            if (!ParseRange(request))
                return false;

            return true;
        }

        #endregion
    }
}
