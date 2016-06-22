using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.UI.Attributes
{
    public class DataGridIgnoreAttribute : Attribute
    {
        public DataGridIgnoreAttribute(bool ignoreColumn = true)
        {
            IgnoreColumn = ignoreColumn;
        }

        public readonly bool IgnoreColumn;
    }
}
