using System;

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
