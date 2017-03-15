// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     Marks static property of a static class as an API request processor. 
    ///     Must ONLY be used with properties implementing <see cref="Common.Interfaces.IRequestProcessor"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ApiRequestProcessorAttribute : Attribute
    {
    }
}
