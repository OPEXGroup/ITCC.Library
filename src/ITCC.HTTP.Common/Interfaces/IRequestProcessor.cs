// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Net.Http;

namespace ITCC.HTTP.Common.Interfaces
{
    public interface IRequestProcessor
    {
        bool AuthorizationRequired { get; }
        HttpMethod Method { get; }
        string SubUri { get; }
    }
}
