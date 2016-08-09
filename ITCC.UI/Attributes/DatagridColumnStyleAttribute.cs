using System;
using System.Windows.Controls;

namespace ITCC.UI.Attributes
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
