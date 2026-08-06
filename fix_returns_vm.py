import re

with open('./PosCore/ViewModels/ReturnsViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

props = """    [ObservableProperty]
    private string _notificationMessage = string.Empty;
"""

content = content.replace("    [ObservableProperty]\n    private Order _selectedOrder;", props + "    [ObservableProperty]\n    private Order _selectedOrder;")

content = content.replace(
    "foreach (var o in orders)\n        {\n            RecentOrders.Add(o);\n        }",
    "foreach (var o in orders)\n        {\n            RecentOrders.Add(o);\n        }\n\n        NotificationMessage = \"Lista actualizada: \" + DateTime.Now.ToString(\"HH:mm:ss\");\n        await Task.Delay(3000);\n        if (NotificationMessage.StartsWith(\"Lista actualizada\")) NotificationMessage = string.Empty;"
)

with open('./PosCore/ViewModels/ReturnsViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
