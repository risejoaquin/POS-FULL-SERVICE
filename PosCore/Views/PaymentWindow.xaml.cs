
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PosCore.Views
{
    public class PaymentEntry
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public partial class PaymentWindow : Window
    {
        public bool IsPaid { get; private set; } = false;
        public decimal Total { get; }
        
        public ObservableCollection<PaymentEntry> Payments { get; set; } = new();

        private string _inputBuffer = "";
        private decimal _tendered = 0m;
        private string _selectedMethod = "Efectivo";

        public PaymentWindow(decimal total)
        {
            this.KeyDown += PaymentWindow_KeyDown;
            InitializeComponent();
            Total = total;
            TotalText.Text = total.ToString("C");
            BtnExact.Content = total.ToString("C");
            UpdateState();
            SelectMethod("Efectivo", BtnEfectivo);
        }
        
        private void PaymentWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key >= System.Windows.Input.Key.D0 && e.Key <= System.Windows.Input.Key.D9)
            {
                int val = (int)e.Key - (int)System.Windows.Input.Key.D0;
                AppendNumpad(val.ToString());
            }
            else if (e.Key >= System.Windows.Input.Key.NumPad0 && e.Key <= System.Windows.Input.Key.NumPad9)
            {
                int val = (int)e.Key - (int)System.Windows.Input.Key.NumPad0;
                AppendNumpad(val.ToString());
            }
            else if (e.Key == System.Windows.Input.Key.Back)
            {
                if (_inputBuffer.Length > 0) { _inputBuffer = _inputBuffer.Substring(0, _inputBuffer.Length - 1); UpdateState(); }
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnComplete_Click(this, null!);
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                BtnCancel_Click(this, null!);
            }
        }

        private void AppendNumpad(string val)
        {
            if (val == ".")
            {
                if (!_inputBuffer.Contains(".")) _inputBuffer += ".";
            }
            else
            {
                if (_inputBuffer == "0" && val != ".") _inputBuffer = val;
                else if (_inputBuffer.Length < 10) _inputBuffer += val;
            }
            UpdateState();
        }

        private void UpdateState()
        {
            if (string.IsNullOrEmpty(_inputBuffer)) _tendered = 0;
            else decimal.TryParse(_inputBuffer, out _tendered);
            InputAmountBox.Text = _tendered.ToString("N2");
        }

        private void BtnNumpad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string num) AppendNumpad(num);
        }

        private void BtnNumpadDel_Click(object sender, RoutedEventArgs e)
        {
            if (_inputBuffer.Length > 0) { _inputBuffer = _inputBuffer.Substring(0, _inputBuffer.Length - 1); UpdateState(); }
        }

        private void BtnQuickAmount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string amountStr)
            {
                string clean = amountStr.Replace("$", "").Replace(",", "");
                _inputBuffer = clean;
                UpdateState();
            }
        }

        private void ResetMethods()
        {
            BtnEfectivo.Background = Brushes.White; BtnEfectivo.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            BtnTarjeta.Background = Brushes.White; BtnTarjeta.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            BtnTransferencia.Background = Brushes.White; BtnTransferencia.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            BtnVale.Background = Brushes.White; BtnVale.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
        }

        private void SelectMethod(string method, Button btn)
        {
            _selectedMethod = method;
            ResetMethods();
            btn.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129));
            btn.Foreground = Brushes.White;
        }

        private void BtnPayEfectivo_Click(object sender, RoutedEventArgs e) => SelectMethod("Efectivo", BtnEfectivo);
        private void BtnPayTarjeta_Click(object sender, RoutedEventArgs e) => SelectMethod("Tarjeta", BtnTarjeta);
        private void BtnPayTransferencia_Click(object sender, RoutedEventArgs e) => SelectMethod("Transferencia", BtnTransferencia);
        private void BtnPayVale_Click(object sender, RoutedEventArgs e) => SelectMethod("Vale", BtnVale);

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public decimal TipAmount { get; private set; } = 0;
        private bool _isProcessing = false;

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (_isProcessing) return;
            
            if (_tendered == 0) _tendered = Total; // Auto fill exact amount if they just clicked complete
            
            if (_tendered < Total && _selectedMethod == "Efectivo")
            {
                MessageBox.Show($"Faltan {(Total - _tendered).ToString("C")} por pagar.", "Pago Incompleto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Payments.Clear();
            Payments.Add(new PaymentEntry { Method = _selectedMethod, Amount = _tendered >= Total ? Total : _tendered });

            _isProcessing = true;
            IsPaid = true;
            DialogResult = true;
            Close();
        }
    }
}
