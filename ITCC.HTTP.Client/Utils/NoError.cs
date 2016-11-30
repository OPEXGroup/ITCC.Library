using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.Client.Utils
{
    /// <summary>
    ///     Dummy response success class
    /// </summary>
    [ApiView(ApiHttpMethod.Default)]
    public sealed class NoError
    {
    }
}
