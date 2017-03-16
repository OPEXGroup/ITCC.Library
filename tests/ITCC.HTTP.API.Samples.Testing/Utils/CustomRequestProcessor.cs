// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.Common.Interfaces;
using System.Net.Http;

namespace ITCC.HTTP.API.Samples.Testing.Utils
{
    public class CustomRequestProcessor : IRequestProcessor
    {
        public bool AuthorizationRequired { get; }
        public HttpMethod Method { get; }
        public string SubUri { get; }
    }
}
