import re

with open('PosBuilder/ViewModels/WizardViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

props = """
        [ObservableProperty]
        private string _currentStepCategory;
"""

if "_currentStepCategory" not in content:
    content = content.replace("private string _currentStepSubTitle;", "private string _currentStepSubTitle;\n" + props)

with open('PosBuilder/ViewModels/WizardViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)

print("ViewModel patched 2.")
