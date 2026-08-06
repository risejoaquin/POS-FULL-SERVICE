using System;
using System.Diagnostics;
using System.Windows;

namespace PosBuilder.Views
{
    public partial class SuccessModal : Window
    {
        private string _credentials;
        private string _outputFolder;

        public SuccessModal(string outputFolder, string credentials)
        {
            InitializeComponent();
            _credentials = credentials;
            _outputFolder = outputFolder;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = _outputFolder,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir la carpeta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
