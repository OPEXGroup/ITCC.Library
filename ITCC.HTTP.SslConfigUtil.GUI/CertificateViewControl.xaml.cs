using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ITCC.HTTP.SslConfigUtil.GUI
{
    /// <summary>
    /// Interaction logic for CertificateViewControl.xaml
    /// </summary>
    public partial class CertificateViewControl : UserControl
    {
        #region IssuedTo
        public string IssuedTo
        {
            get { return (string)GetValue(IssuedToProperty); }
            set { SetValue(IssuedToProperty, value); }
        }
        public static readonly DependencyProperty IssuedToProperty = DependencyProperty.Register(
            "IssuedTo",
            typeof(string),
            typeof(CertificateViewControl),
            new PropertyMetadata(null, OnIssuedToChanged));

        public static string GetIssuedTo(UIElement element) => (string)element.GetValue(IssuedToProperty);
        public static void SetIssuedTo(UIElement element, string value) => element.SetValue(IssuedToProperty, value);
        private static void OnIssuedToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CertificateViewControl;
            if (control == null)
                return;

            control.IssuedToTextBlock.Text = (string) e.NewValue;
        }
        #endregion

        #region IssuedBy
        public string IssuedBy
        {
            get { return (string)GetValue(IssuedByProperty); }
            set { SetValue(IssuedByProperty, value); }
        }
        public static readonly DependencyProperty IssuedByProperty = DependencyProperty.Register(
            "IssuedBy",
            typeof(string),
            typeof(CertificateViewControl),
            new PropertyMetadata(null, OnIssuedByChanged));

        public static string GetIssuedBy(UIElement element) => (string)element.GetValue(IssuedByProperty);
        public static void SetIssuedBy(UIElement element, string value) => element.SetValue(IssuedByProperty, value);
        private static void OnIssuedByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CertificateViewControl;
            if (control == null)
                return;

            control.IssuedByTextBox.Text = (string)e.NewValue;

            control.Background = control.IssuedTo == control.IssuedBy ? Brushes.LightGoldenrodYellow : Brushes.PaleGreen;
        }
        #endregion

        #region ValidUntil
        public DateTime ValidUntil
        {
            get { return (DateTime)GetValue(ValidUntilProperty); }
            set { SetValue(ValidUntilProperty, value); }
        }
        public static readonly DependencyProperty ValidUntilProperty = DependencyProperty.Register(
            "ValidUntil",
            typeof(DateTime),
            typeof(CertificateViewControl),
            new PropertyMetadata(new DateTime(), OnValidUntilChanged));

        public static DateTime GetValidUntil(UIElement element) => (DateTime)element.GetValue(ValidUntilProperty);
        public static void SetValidUntil(UIElement element, DateTime value) => element.SetValue(ValidUntilProperty, value);
        private static void OnValidUntilChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CertificateViewControl;
            if (control == null)
                return;

            control.ValidUntolTextBlock.Text = ((DateTime) e.NewValue).ToShortDateString();
        }

        #endregion

        public CertificateViewControl()
        {
            InitializeComponent();
        }
    }
}