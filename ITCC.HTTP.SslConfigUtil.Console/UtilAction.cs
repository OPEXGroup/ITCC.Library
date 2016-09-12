using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.SslConfigUtil.Console
{
    internal class UtilAction<TResult>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ActionParameter[] Params { get; set; }
        public Func<TResult> Callback { get; set; }
    }
}