// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using ITCC.HTTP.API.Attributes;

namespace ITCC.HTTP.API.Enums
{
    /// <summary>
    ///     Used in <see cref="ApiViewAttribute"/> to indicate proper usage
    /// </summary>
    [Flags]
    public enum ApiHttpMethod
    {
        /// <summary>
        ///     Default value
        /// </summary>
        [EnumValueInfo(@"Default value")]
        Default = 0,
        /// <summary>
        ///     HTTP GET
        /// </summary>
        [EnumValueInfo(@"HTTP GET")]
        Get = 1 << 0,
        /// <summary>
        ///     HTTP POST
        /// </summary>
        [EnumValueInfo(@"HTTP POST")]
        Post = 1 << 1,
        /// <summary>
        ///     HTTP PUT
        /// </summary>
        [EnumValueInfo(@"HTTP PUT")]
        Put = 1 << 2,
        /// <summary>
        ///     HTTP DELETE. This SHOULD NOT be used. Use guids instead to remove entities
        /// </summary>
        [EnumValueInfo(@"HTTP DELETE")]
        Delete = 1 << 3,
        /// <summary>
        ///     HTTP PATCH
        /// </summary>
        [EnumValueInfo(@"HTTP PATCH")]
        Patch = 1 << 4
    }
}
