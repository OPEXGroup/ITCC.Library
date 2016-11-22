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
