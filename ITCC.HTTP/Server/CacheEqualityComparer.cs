using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.Server
{
    internal class CacheEqualityComparer<TAccount>  : IEqualityComparer<AccountProcessorPair<TAccount>>
        where TAccount : class, IEquatable<TAccount>

    {
        public bool Equals(AccountProcessorPair<TAccount> x, AccountProcessorPair<TAccount> y)
        {
            if (x.Account == null && y.Account == null)
                return x.RequestProcessor == y.RequestProcessor;

            if ((x.Account == null && y.Account != null) || (x.Account != null && y.Account == null))
                return false;

            return x.Account != null && x.Account.Equals(y.Account) && x.RequestProcessor == y.RequestProcessor;
        }

        public int GetHashCode(AccountProcessorPair<TAccount> obj)
            => obj.GetHashCode();
    }
}
