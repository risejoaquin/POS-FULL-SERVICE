with open('PosBuilder/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

update_step_view_old = """        private void UpdateStepView()
        {
            if (_viewModel.CurrentStepIndex >= 0 && _viewModel.CurrentStepIndex < _steps.Length)
            {
                StepContentControl.Content = _steps[_viewModel.CurrentStepIndex];
            }"""

update_step_view_new = """        private void UpdateStepView()
        {
            if (_viewModel.CurrentStepIndex >= 0 && _viewModel.CurrentStepIndex < _steps.Length)
            {
                StepContentControl.Content = _steps[_viewModel.CurrentStepIndex];
                
                var categories = new string[] { "Comercio y API", "Motor y conexión", "JWT & tokens", "Identidad visual", "Cuentas iniciales", "Funcionalidades", "Generar POS" };
                var titles = new string[] { "Entorno y Comercio", "Base de Datos", "Seguridad JWT", "Branding", "Usuarios Iniciales", "Módulos del Sistema", "Resumen y Generación" };
                
                _viewModel.CurrentStepSubTitle = $"PASO {_viewModel.CurrentStepIndex + 1} DE 7";
                _viewModel.CurrentStepCategory = categories[_viewModel.CurrentStepIndex];
                _viewModel.CurrentStepTitle = titles[_viewModel.CurrentStepIndex];
            }"""

content = content.replace(update_step_view_old, update_step_view_new)

# Also update the sidebar logic to match the UI. In the image:
# Green circle with checkmark for done, Blue for current, dark gray for next.
sidebar_logic_old = """            foreach (var item in StepIndicators)
            {
                if (item.Index < _viewModel.CurrentStepIndex)
                {
                    item.Icon = "✔";
                    item.Color = Brushes.Green;
                }
                else if (item.Index == _viewModel.CurrentStepIndex)
                {
                    item.Icon = "●";
                    item.Color = Brushes.Blue;
                }
                else
                {
                    item.Icon = "○";
                    item.Color = Brushes.Gray;
                }
            }"""

sidebar_logic_new = """            foreach (var item in StepIndicators)
            {
                if (item.Index < _viewModel.CurrentStepIndex)
                {
                    item.Icon = "✔";
                    item.Color = (Brush)new BrushConverter().ConvertFrom("#10B981");
                }
                else if (item.Index == _viewModel.CurrentStepIndex)
                {
                    item.Icon = (item.Index + 1).ToString();
                    item.Color = (Brush)new BrushConverter().ConvertFrom("#3B82F6");
                }
                else
                {
                    item.Icon = (item.Index + 1).ToString();
                    item.Color = (Brush)new BrushConverter().ConvertFrom("#334155");
                }
            }"""
            
content = content.replace(sidebar_logic_old, sidebar_logic_new)

with open('PosBuilder/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)

print("MainWindow.xaml.cs patched.")
