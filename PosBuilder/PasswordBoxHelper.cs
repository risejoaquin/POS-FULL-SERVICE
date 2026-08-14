using System.Windows;
using System.Windows.Controls;

namespace PosBuilder
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper));

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPasswordProperty, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            return dp.GetValue(BindPasswordProperty) as bool? ?? false;
        }

        public static string GetBoundPassword(DependencyObject dp)
        {
            return dp.GetValue(BoundPasswordProperty) as string ?? string.Empty;
        }

        public static void SetBoundPassword(DependencyObject dp, string value)
        {
            dp.SetValue(BoundPasswordProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox? box = d as PasswordBox;
            if (box == null || !GetBindPassword(d))
            {
                return;
            }

            // avoid recursive updating
            box.PasswordChanged -= HandlePasswordChanged;

            string newPassword = e.NewValue as string ?? string.Empty;

            if (!GetIsUpdating(box))
            {
                box.Password = newPassword;
            }

            box.PasswordChanged += HandlePasswordChanged;
        }

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox? box = dp as PasswordBox;
            if (box == null)
            {
                return;
            }

            bool wasBound = e.OldValue as bool? ?? false;
            bool needToBind = e.NewValue as bool? ?? false;

            if (wasBound)
            {
                box.PasswordChanged -= HandlePasswordChanged;
            }

            if (needToBind)
            {
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox? box = sender as PasswordBox;
            if (box == null) return;
            
            SetIsUpdating(box, true);
            SetBoundPassword(box, box.Password);
            SetIsUpdating(box, false);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return dp.GetValue(IsUpdatingProperty) as bool? ?? false;
        }
    }
}
