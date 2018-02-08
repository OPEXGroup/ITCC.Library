// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace ITCC.WPF.Behaviors
{
    public class SecurePasswordBindingBehavior : Behavior<PasswordBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PasswordChanged += OnPasswordBoxValueChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PasswordChanged -= OnPasswordBoxValueChanged;
        }

        public SecureString Password
        {
            get => (SecureString)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            nameof(Password),
            typeof(SecureString),
            typeof(SecurePasswordBindingBehavior), 
            new PropertyMetadata(null));


        private void OnPasswordBoxValueChanged(object sender, RoutedEventArgs e)
        {
            var binding = BindingOperations.GetBindingExpression(this, PasswordProperty);
            if (binding == null)
                return;

            var property = binding.DataItem.GetType().GetProperty(binding.ParentBinding.Path.Path);
            if (property != null)
                property.SetValue(binding.DataItem, AssociatedObject.SecurePassword, null);
        }
    }
}
