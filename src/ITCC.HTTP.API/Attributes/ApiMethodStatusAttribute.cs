// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     Used to describe current method status
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ApiMethodStatusAttribute : Attribute
    {
        public ApiMethodStatusAttribute(ApiMethodState state, string comment = null)
        {
            State = state;
            Comment = comment;
        }

        public ApiMethodState State { get; }
        public string Comment { get; }
    }
}
