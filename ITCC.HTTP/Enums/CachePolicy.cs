using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.Enums
{
    /// <summary>
    ///     Request processor cache policy
    /// </summary>
    public enum CachePolicy
    {
        /// <summary>
        ///     Caching is prohibited for this request processor
        /// </summary>
        NoCache,
        /// <summary>
        ///     Per-account cache. Default behaviour if server cache is enabled
        /// </summary>
        PrivateCache,
        /// <summary>
        ///     Per-method cache
        /// </summary>
        PublicCache
    }
}
