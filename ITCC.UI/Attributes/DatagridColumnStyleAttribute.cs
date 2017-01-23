// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;

namespace ITCC.UI.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DatagridColumnStyleAttribute : Attribute
    {
        public DatagridColumnStyleAttribute(bool wrappedText = false, double columnPreferredWidth = -1)
        {
            WrappedText = wrappedText;
            ColumnPreferredWidth = columnPreferredWidth;
        }

        public readonly bool WrappedText;
        public readonly double ColumnPreferredWidth;
    }
}
