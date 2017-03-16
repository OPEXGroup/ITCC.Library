// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Net.Http;
using ITCC.HTTP.Common.Interfaces;

namespace ITCC.HTTP.Api.Documentation.Testing.Utils
{
    public class CustomRequestProcessor : IRequestProcessor
    {
        public bool AuthorizationRequired { get; }
        public HttpMethod Method { get; }
        public string SubUri { get; }
    }
}
