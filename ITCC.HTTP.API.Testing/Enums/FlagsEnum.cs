// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace ITCC.HTTP.API.Testing.Enums
{
    [Flags]
    public enum FlagsEnum
    {
        Zero,
        First = 1,
        Second = 2,
        Third = 4
    }
}
