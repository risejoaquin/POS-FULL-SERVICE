import re

with open('./PosCore/ViewModels/ReturnsViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

props = """    [ObservableProperty]
    private ObservableCollection<Order> _recentOrders = new();

    [ObservableProperty]
    private string _notificationMessage = string.Empty;
"""

content = content.replace("    [ObservableProperty]\n    private ObservableCollection<Order> _recentOrders = new();", props)

with open('./PosCore/ViewModels/ReturnsViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
