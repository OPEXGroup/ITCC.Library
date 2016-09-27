using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ITCC.UI.Utils
{
    public static class DataGridExtensions
    {
        #region public

        public static readonly DependencyProperty AlwaysScrollToEndProperty =
            DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), typeof(DataGridExtensions),
                new PropertyMetadata(false, AlwaysScrollToEndChanged));

        public static bool GetAlwaysScrollToEnd(this DataGrid scroll)
        {
            if (scroll == null)
                throw new ArgumentNullException(nameof(scroll));

            return (bool)scroll.GetValue(AlwaysScrollToEndProperty);
        }

        public static void SetAlwaysScrollToEnd(this DataGrid scroll, bool alwaysScrollToEnd)
        {
            if (scroll == null)
                throw new ArgumentNullException(nameof(scroll));

            scroll.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
        }

        #endregion

        #region private

        private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                throw new InvalidOperationException(
                    "The attached AlwaysScrollToEnd property can only be applied to DataGrid instances.");

            if (dataGrid.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(dataGrid, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll == null)
                        return;
                    var alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;
                    if (alwaysScrollToEnd)
                    {
                        scroll.ScrollToEnd();
                        scroll.ScrollChanged += ScrollChanged;
                    }
                    else
                    {
                        scroll.ScrollChanged -= ScrollChanged;
                    }
                }
            }
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scroll = sender as ScrollViewer;
            if (scroll == null)
                throw new InvalidOperationException(
                    "The attached AlwaysScrollToEnd property can only be applied to DataGrid instances.");

            if (Math.Abs(e.ExtentHeightChange) < Tolerance)
                _autoScroll = Math.Abs(scroll.VerticalOffset - scroll.ScrollableHeight) < Tolerance;

            if (_autoScroll && Math.Abs(e.ExtentHeightChange) > Tolerance)
                scroll.ScrollToVerticalOffset(scroll.ExtentHeight);
        }

        private const double Tolerance = 0.01;
        private static bool _autoScroll;

        #endregion
    }
}
