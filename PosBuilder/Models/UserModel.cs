using CommunityToolkit.Mvvm.ComponentModel;

namespace PosBuilder.Models
{
    public partial class UserModel : ObservableObject
    {
        [ObservableProperty]
        private string _username = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _role = "Empleado";
    }
}
