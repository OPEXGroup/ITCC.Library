// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Utils
{
    /// <summary>
    ///     Should be used as the only constructor argument in
    ///     <see cref="Attributes.ApiRequestBodyTypeAttribute"/> and <see cref="Attributes.ApiResponseBodyTypeAttribute"/>
    ///     to indicate empty bodies
    /// </summary>
    [ApiView(ApiHttpMethod.Default)]
    public abstract class Empty
    {

    }
}
