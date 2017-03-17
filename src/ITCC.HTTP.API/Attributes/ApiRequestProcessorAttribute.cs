// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     Marks static property of a static class as an API request processor. 
    ///     Must ONLY be used with properties implementing <see cref="Common.Interfaces.IRequestProcessor"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ApiRequestProcessorAttribute : Attribute
    {
        public ApiRequestProcessorAttribute(string description, string subUri, ApiHttpMethod method, bool authRequired, string remarks = null)
        {
            Description = description;
            SubUri = subUri;
            Method = method;
            AuthRequired = authRequired;
            Remarks = remarks;
        }

        public string Description { get; }
        public string SubUri { get; }
        public ApiHttpMethod Method { get; }
        public bool AuthRequired { get; }
        public string Remarks { get; }
    }
}
