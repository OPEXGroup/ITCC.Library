using System;

namespace ITCC.WPF.Attributes
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
