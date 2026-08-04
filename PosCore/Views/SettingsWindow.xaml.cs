using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PosCore.Models;

namespace PosCore.Views
{
    public partial class SettingsWindow : Window
    {
        private ShortcutManager _manager;
        public List<ShortcutConfig> Shortcuts { get; set; }
        
        public SettingsWindow(ShortcutManager manager)
        {
            _manager = manager;
            Shortcuts = new List<ShortcutConfig>();
            InitializeComponent();
            
            // Try loading default on first open based on selection or manager
            LoadToGrid(_manager.CurrentShortcuts);
        }

        private void LoadToGrid(List<ShortcutConfig> list)
        {
            if (Shortcuts == null) Shortcuts = new List<ShortcutConfig>();
            Shortcuts.Clear();
            if (list != null)
            {
                foreach(var s in list) 
                {
                    Shortcuts.Add(new ShortcutConfig { 
                        Name = s.Name, 
                        Description = s.Description, 
                        Action = s.Action, 
                        Color = s.Color, 
                        Icon = s.Icon 
                    });
                }
            }
            if (GridShortcuts != null)
            {
                GridShortcuts.ItemsSource = null;
                GridShortcuts.ItemsSource = Shortcuts;
            }
        }

        private void CmbIndustry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbIndustry.SelectedItem is ComboBoxItem item)
            {
                string? industry = item.Content?.ToString();
                if (industry != null)
                {
                    var tempManager = new ShortcutManager();
                    tempManager.LoadShortcuts(industry);
                    LoadToGrid(tempManager.CurrentShortcuts);
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _manager.SaveUserShortcuts(Shortcuts);
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
