using System;

namespace ITCC.HTTP.API.Enums
{
    /// <summary>
    ///     Used in <see cref="Attributes.ApiViewAttribute"/> to indicate proper usage
    /// </summary>
    [Flags]
    public enum ApiHttpMethod
    {
        /// <summary>
        ///     View is used only in responses for non-get methods
        /// </summary>
        Default = 0,
        /// <summary>
        ///     HTTP GET
        /// </summary>
        Get = 1 << 0,
        /// <summary>
        ///     HTTP POST
        /// </summary>
        Post = 1 << 1,
        /// <summary>
        ///     HTTP PUT
        /// </summary>
        Put = 1 << 2,
        /// <summary>
        ///     This SHOULD NOT be used. Use guids instead to remove entities
        /// </summary>
        Delete = 1 << 3,
        /// <summary>
        ///     HTTP PATCH
        /// </summary>
        Patch = 1 << 4
    }
}
