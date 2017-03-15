// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     Says that class or struct represents public API view. 
    ///     All API views MUST have this attributes and all other classes MUST NOT
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ApiViewAttribute : Attribute
    {
        /// <summary>
        ///     Creates new <see cref="ApiViewAttribute"/> instance
        /// </summary>
        /// <param name="httpMethod">
        ///     Target HTTP method(s). One view SHOULD NOT be used for both reads (GET) and writes (PUT,POST).
        /// </param>
        public ApiViewAttribute(ApiHttpMethod httpMethod)
        {
            HttpMethod = httpMethod;
        }

        public ApiHttpMethod HttpMethod { get; }
    }
}