// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.Client.Utils
{
    /// <summary>
    ///     Dummy response success class
    /// </summary>
    [ApiView(ApiHttpMethod.Default)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class None
    {
    }
}
