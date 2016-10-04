using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ITCC.UI.Utils
{
    public static class ScrollViewerExtensions
    {
        #region public

        public static readonly DependencyProperty AlwaysScrollToEndProperty =
            DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), typeof(ScrollViewerExtensions),
                new PropertyMetadata(false, AlwaysScrollToEndChanged));

        public static bool GetAlwaysScrollToEnd(this ScrollViewer scroll)
        {
            if (scroll == null)
                throw new ArgumentNullException(nameof(scroll));

            return (bool)scroll.GetValue(AlwaysScrollToEndProperty);
        }

        public static void SetAlwaysScrollToEnd(this ScrollViewer scroll, bool alwaysScrollToEnd)
        {
            if (scroll == null)
                throw new ArgumentNullException(nameof(scroll));

            scroll.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
        }

        #endregion

        #region private

        private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var scroll = sender as ScrollViewer;
            if (scroll != null)
            {
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
            else
            {
                throw new InvalidOperationException(
                    "The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances.");
            }
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scroll = sender as ScrollViewer;
            if (scroll == null)
                throw new InvalidOperationException(
                    "The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances.");

            if (Math.Abs(e.ExtentHeightChange) < Tolerance)
                _autoScroll = Math.Abs(scroll.VerticalOffset - scroll.ScrollableHeight) < Tolerance;

            if (!_autoScroll || !(Math.Abs(e.ExtentHeightChange) > Tolerance))
                return;

            scroll.ScrollToVerticalOffset(scroll.ExtentHeight);
        }

        private const double Tolerance = 0.01;
        private static bool _autoScroll;

        #endregion
    }
}