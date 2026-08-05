using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PosCore.ViewModels;
using PosCore.Models;
using System.Linq;

namespace PosCore.Views
{
    public partial class MainWindow : Window
    {
        private StringBuilder _inputBuffer = new StringBuilder();
        private DateTime _lastKeystroke = DateTime.Now;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Focus Search box
            if (e.Key == Key.F1)
            {
                SearchBox.Focus();
                e.Handled = true;
                return;
            }
            
            // Barcode logic (simple fallback if they scan while focus is elsewhere)
            TimeSpan timeSinceLastKey = DateTime.Now - _lastKeystroke;
            if (timeSinceLastKey.TotalMilliseconds > 50)
            {
                _inputBuffer.Clear();
            }

            _lastKeystroke = DateTime.Now;

            if (e.Key == Key.Enter)
            {
                if (_inputBuffer.Length > 0 && !SearchBox.IsFocused)
                {
                    if (DataContext is MainViewModel vm)
                    {
                        vm.SearchQuery = _inputBuffer.ToString();
                        vm.ProcessBarcode();
                    }
                    _inputBuffer.Clear();
                }
            }
            else
            {
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                    _inputBuffer.Append((char)('0' + (e.Key - Key.D0)));
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                    _inputBuffer.Append((char)('0' + (e.Key - Key.NumPad0)));
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                    _inputBuffer.Append(e.Key.ToString());
            }
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.ProcessBarcode();
                }
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }
    }
}
