// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
