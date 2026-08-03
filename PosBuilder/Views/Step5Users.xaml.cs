using System.Windows.Controls;

namespace PosBuilder.Views
{
    public partial class Step5Users : UserControl
    {
        public Step5Users()
        {
            InitializeComponent();
        }
        private void AdminPassBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PosBuilder.ViewModels.WizardViewModel vm)
            {
                vm.AdminPassword = AdminPassBox.Password;
            }
            if (AdminPassBox.Password.Length > 0 && AdminPassBox.Password.Length < 6)
                AdminPassWarning.Visibility = System.Windows.Visibility.Visible;
            else
                AdminPassWarning.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void EmpPassBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PosBuilder.ViewModels.WizardViewModel vm)
            {
                vm.EmployeePassword = EmpPassBox.Password;
            }
            if (EmpPassBox.Password.Length > 0 && EmpPassBox.Password.Length < 6)
                EmpPassWarning.Visibility = System.Windows.Visibility.Visible;
            else
                EmpPassWarning.Visibility = System.Windows.Visibility.Collapsed;
        }

    }
}
