// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.API.Attributes;

namespace ITCC.HTTP.API.Samples.Testing.Enums
{
    public enum FirstEnum
    {
        [EnumValueInfo("Value is zero")]
        Zero,
        [EnumValueInfo("Value is one")]
        One,
        [EnumValueInfo("Value is two")]
        Two
    }
}
