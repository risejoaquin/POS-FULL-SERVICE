using System.Windows;
using PosCore.ViewModels;

namespace PosCore.Views
{
    public partial class UsersWindow : Window
    {
        public UsersWindow(UsersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    
    private void NewPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is PosCore.ViewModels.UsersViewModel vm)
        {
            if (sender is System.Windows.Controls.PasswordBox pb)
            {
                vm.NewPin = pb.Password;
            }
        }
    }
}
}
