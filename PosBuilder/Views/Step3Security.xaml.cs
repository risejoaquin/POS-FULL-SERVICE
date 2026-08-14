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

        private void ProvisionKeyBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == false && DataContext is WizardViewModel vm)
            {
                vm.ProvisionKey = ProvisionKeyBox.Password;
            }
            UpdateStrength(ProvisionKeyBox.Password);
        }

        private void ToggleJwtVisibility_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == true)
            {
                ProvisionKeyVisibleBox.Text = ProvisionKeyBox.Password;
                ProvisionKeyVisibleBox.Visibility = System.Windows.Visibility.Visible;
                ProvisionKeyBox.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ProvisionKeyBox.Password = ProvisionKeyVisibleBox.Text;
                ProvisionKeyVisibleBox.Visibility = System.Windows.Visibility.Collapsed;
                ProvisionKeyBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ProvisionKeyVisibleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == true && DataContext is WizardViewModel vm)
            {
                vm.ProvisionKey = ProvisionKeyVisibleBox.Text;
                UpdateStrength(ProvisionKeyVisibleBox.Text);
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