using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ITCC.Logging;

namespace ITCC.UI.Utils
{
    public class DataGridHelper : FrameworkElement
    {
        public static void HandleAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e,
            string lastColumnName = null)
        {
            var displayName = PropertyHelper.GetPropertyDisplayName(e.PropertyDescriptor);

            if (!string.IsNullOrEmpty(displayName))
            {
                Logger.LogEntry("GENERATOR", LogLevel.Trace, $"Generating column {displayName}");

                e.Column.Header = displayName;
                if (displayName == lastColumnName)
                {
                    e.Column.Width = new DataGridLength(420, DataGridLengthUnitType.Star);

                    var col = e.Column as DataGridTextColumn;

                    var style = new Style(typeof(TextBlock));
                    style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                    style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));

                    if (col != null)
                        col.ElementStyle = style;
                }
            }

            var headerTooltip = PropertyHelper.GetPropertyHeaderTooltip(e.PropertyDescriptor);
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
        }
    }
}