using System.Windows;
using PosApplication.Interfaces.Local;

namespace PosCore.Views
{
    public partial class ManagerOverrideWindow : Window
    {
        private readonly ILocalAuthService _authService;
        public bool IsAuthorized { get; private set; } = false;
        public string AuthorizedBy { get; private set; } = string.Empty;

        public ManagerOverrideWindow(string actionDescription, ILocalAuthService authService)
        {
            InitializeComponent();
            ActionDescText.Text = $"Acción: {actionDescription}";
            _authService = authService;
            PinBox.Focus();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void BtnAuthorize_Click(object sender, RoutedEventArgs e)
        {
            var pin = PinBox.Password;
            if (string.IsNullOrWhiteSpace(pin))
            {
                MessageBox.Show("Ingrese un PIN válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var isAuthorized = await _authService.ValidateManagerOverrideAsync(string.Empty, pin);
                if (isAuthorized)
                {
                    IsAuthorized = true;
                    AuthorizedBy = pin == "admin" ? "Admin (Default)" : "Manager Override";
                    DialogResult = true;
                    Close();
                    return;
                }

                MessageBox.Show("PIN incorrecto o no tiene permisos de Administrador.", "Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                PinBox.Clear();
                PinBox.Focus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al validar autorización: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PinBox.Clear();
                PinBox.Focus();
            }
        }
    }
}
