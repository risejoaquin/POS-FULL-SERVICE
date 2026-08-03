using System.Windows.Controls;
using System.Windows.Media;
using PosBuilder.ViewModels;

namespace PosBuilder.Views
{
    public partial class Step3Security : UserControl
    {
        public Step3Security()
        {
            InitializeComponent();
        }

        private void JwtSecretBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == false && DataContext is WizardViewModel vm)
            {
                vm.JwtSecret = JwtSecretBox.Password;
            }
            UpdateStrength(JwtSecretBox.Password);
        }

        private void ToggleJwtVisibility_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == true)
            {
                JwtSecretVisibleBox.Text = JwtSecretBox.Password;
                JwtSecretVisibleBox.Visibility = System.Windows.Visibility.Visible;
                JwtSecretBox.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                JwtSecretBox.Password = JwtSecretVisibleBox.Text;
                JwtSecretVisibleBox.Visibility = System.Windows.Visibility.Collapsed;
                JwtSecretBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void JwtSecretVisibleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == true && DataContext is WizardViewModel vm)
            {
                vm.JwtSecret = JwtSecretVisibleBox.Text;
                UpdateStrength(JwtSecretVisibleBox.Text);
            }
        }
        
        private void UpdateStrength(string text)
        {
            int len = text.Length;
            if (len == 0)
            {
                StrengthMeter.Value = 0;
                StrengthLabel.Text = "";
            }
            else if (len < 6)
            {
                StrengthMeter.Value = 33;
                StrengthMeter.Foreground = Brushes.Red;
                StrengthLabel.Text = "Débil";
                StrengthLabel.Foreground = Brushes.Red;
            }
            else if (len < 10)
            {
                StrengthMeter.Value = 66;
                StrengthMeter.Foreground = Brushes.Orange;
                StrengthLabel.Text = "Media";
                StrengthLabel.Foreground = Brushes.Orange;
            }
            else
            {
                StrengthMeter.Value = 100;
                StrengthMeter.Foreground = Brushes.Green;
                StrengthLabel.Text = "Fuerte";
                StrengthLabel.Foreground = Brushes.Green;
            }
        }

    }
}