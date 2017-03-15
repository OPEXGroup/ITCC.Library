// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using ITCC.HTTP.API.Attributes;

namespace ITCC.HTTP.API.Testing.Enums
{
    [Flags]
    public enum FlagsEnum
    {
        [EnumValueInfo(displayName: "Z")]
        Zero,
        [EnumValueInfo(displayName: "F")]
        First = 1,
        [EnumValueInfo(displayName: "S")]
        Second = 2,
        [EnumValueInfo(displayName: "T")]
        Third = 4
    }
}
