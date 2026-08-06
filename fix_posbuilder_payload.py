with open('./PosBuilder/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("EmpPassword = config.EmployeePassword\n                    };", "EmpPassword = config.EmployeePassword,\n                        ExtraUsers = config.ExtraUsers\n                    };")

with open('./PosBuilder/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
