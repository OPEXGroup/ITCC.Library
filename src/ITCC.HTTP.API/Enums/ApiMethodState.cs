// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using ITCC.HTTP.API.Attributes;

namespace ITCC.HTTP.API.Enums
{
    /// <summary>
    ///     
    /// </summary>
    [Flags]
    public enum ApiMethodState
    {
        /// <summary>
        ///     Default method value. This value is meant when no additional info is provided
        /// </summary>
        [EnumValueInfo("Unspecified")]
        Unspecified = 1 << 0,
        /// <summary>
        ///     Some of method intended features do not work at the moment. Additional info should be provided
        /// </summary>
        [EnumValueInfo("Works partially")]
        WorksPartially = 1 << 1,
        /// <summary>
        ///     Method call will match the description
        /// </summary>
        [EnumValueInfo("Works fine")]
        WorksFine = 1 << 2,
        /// <summary>
        ///     Method works with bugs. Known bugs should be listed in additional description
        /// </summary>
        [EnumValueInfo("Works with bugs")]
        WorksWithBugs = 1 << 3,
        /// <summary>
        ///     Method has not been implemented yet
        /// </summary>
        [EnumValueInfo("Not implemented")]
        NotImplemented = 1 << 4,
    }
}
