import re

with open('./PosBuilder/ViewModels/WizardViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('Outputout', 'Output')

with open('./PosBuilder/ViewModels/WizardViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
