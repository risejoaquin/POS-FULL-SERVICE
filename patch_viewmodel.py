import re

with open('PosBuilder/ViewModels/WizardViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

props = """
        [ObservableProperty]
        private string _currentStepTitle;
        
        [ObservableProperty]
        private string _currentStepSubTitle;
"""

if "_currentStepTitle" not in content:
    content = content.replace("private int _currentStepIndex = 0;", "private int _currentStepIndex = 0;\n" + props)

with open('PosBuilder/ViewModels/WizardViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)

print("ViewModel patched.")
