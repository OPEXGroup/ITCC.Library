// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using ITCC.HTTP.API.Attributes;

namespace ITCC.HTTP.Api.Documentation.Testing.Enums
{
    [Flags]
    public enum SecondEnum
    {
        [EnumValueInfo("No value")]
        None = 0,
        [EnumValueInfo("Single value")]
        Single = 1 << 0,
        [EnumValueInfo("Multiple values")]
        Multiple = 1 << 1
    }
}
