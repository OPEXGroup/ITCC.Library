using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.Server
{
    internal struct AccountProcessorPair<TAccount>
        where TAccount : class, IEquatable<TAccount>
    {
        public TAccount Account { get; set; }
        public RequestProcessor<TAccount> RequestProcessor { get; set; }
    }
}
