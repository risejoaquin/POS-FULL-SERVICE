with open('PosBuilder/ViewModels/WizardViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('private string _businessType = "Retail";', 'private string _businessType = "Abarrotes / Minimarket";')

with open('PosBuilder/ViewModels/WizardViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
print("ViewModel updated")
