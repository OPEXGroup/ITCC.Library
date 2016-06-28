using System;

namespace ITCC.UI.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataGridIgnoreAttribute : Attribute
    {
        public DataGridIgnoreAttribute(bool ignoreColumn = true)
        {
            IgnoreColumn = ignoreColumn;
        }

        public readonly bool IgnoreColumn;
    }
}
