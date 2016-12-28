// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Windows.Controls;

namespace ITCC.WPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DatagridColumnStyleAttribute : Attribute
    {
        public DatagridColumnStyleAttribute(bool wrappedText = false, double columnPreferredWidth = -1, DataGridLengthUnitType columnWidthUnitType = DataGridLengthUnitType.Auto)
        {
            WrappedText = wrappedText;
            ColumnPreferredWidth = columnPreferredWidth;
            ColumnWidthUnitType = columnWidthUnitType;
        }

        public readonly bool WrappedText;
        public readonly double ColumnPreferredWidth;
        public readonly DataGridLengthUnitType ColumnWidthUnitType;
    }
}
