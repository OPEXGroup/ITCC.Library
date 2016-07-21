using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ITCC.Logging;
using ITCC.UI.Attributes;

namespace ITCC.UI.Utils
{
    public class DataGridHelper : FrameworkElement
    {
        public static void HandleAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var dataGridIgnore = PropertyHelper.GetPropertyAttribute<DataGridIgnoreAttribute>(e.PropertyDescriptor);
            if (dataGridIgnore != null && dataGridIgnore.IgnoreColumn)
            {
                e.Cancel = true;
                return;
            }

            var displayName = PropertyHelper.GetPropertyDisplayName(e.PropertyDescriptor);

            if (!string.IsNullOrEmpty(displayName))
            {
                Logger.LogEntry("GENERATOR", LogLevel.Trace, $"Generating column {displayName}");

                e.Column.Header = displayName;
            }

            var headerTooltip = PropertyHelper.GetPropertyAttribute<HeaderTooltipAttribute>(e.PropertyDescriptor);
            if (headerTooltip != null)
            {
                Logger.LogEntry("GENERATOR", LogLevel.Trace, $"Adding tooltip {headerTooltip.TooltipContent}");


                var trigger = new Trigger
                {
                    Property = IsMouseOverProperty,
                    Value = true
                };
                var setter = new Setter
                {
                    Property = ToolTipProperty,
                    Value = headerTooltip.TooltipContent
                };
                trigger.Setters.Add(setter);

                var style = new Style(typeof(DataGridColumnHeader), (Style)Application.Current.TryFindResource("DataGridColumnHeaderStyle"));
                style.Triggers.Add(trigger);
                e.Column.HeaderStyle = style;
            }

            var styleAttribute = PropertyHelper.GetPropertyAttribute<DatagridColumnStyleAttribute>(e.PropertyDescriptor);
            if (styleAttribute != null)
            {
                var col = e.Column as DataGridTextColumn;
                Style style = null;
                
                if (styleAttribute.WrappedText)
                {
                    style = new Style(typeof (TextBlock), col?.ElementStyle);
                    style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                    style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Top));
                }

                if (styleAttribute.ColumnPreferredWidth > 0)
                {
                    e.Column.Width = new DataGridLength(styleAttribute.ColumnPreferredWidth, DataGridLengthUnitType.Star);
                }

                if (col != null && style != null)
                    col.ElementStyle = style;
            }
        }
    }
}