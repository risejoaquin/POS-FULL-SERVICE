with open('PosBuilder/ViewModels/WizardViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

import re

# Add using PosBuilder.Models; and System.Collections.ObjectModel;
if 'using PosBuilder.Models;' not in content:
    content = 'using PosBuilder.Models;\nusing System.Collections.ObjectModel;\n' + content

# Add ExtraUsers and AddUserCommand
extra_code = """
        [ObservableProperty]
        private string _employeePassword = "";

        public ObservableCollection<UserModel> ExtraUsers { get; set; } = new ObservableCollection<UserModel>();

        [RelayCommand]
        public void AddUser()
        {
            ExtraUsers.Add(new UserModel { Username = "Nuevo Usuario", Role = "Empleado" });
        }

        [RelayCommand]
        public void RemoveUser(UserModel user)
        {
            if (user != null && ExtraUsers.Contains(user))
            {
                ExtraUsers.Remove(user);
            }
        }
"""
content = content.replace('        [ObservableProperty]\n        private string _employeePassword = "";', extra_code)

with open('PosBuilder/ViewModels/WizardViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)

print("WizardViewModel updated")
