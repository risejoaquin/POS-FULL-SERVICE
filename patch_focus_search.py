import re

with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

if 'FocusSearchCommand' not in content:
    new_cmd = """
    [RelayCommand]
    private void FocusSearch()
    {
        // We will send a message or rely on an event to focus the search box.
        // For simplicity, we can define an event here.
        OnFocusSearchRequested?.Invoke();
    }
    public event Action OnFocusSearchRequested;
"""
    # Insert it before CheckoutCommand
    content = content.replace('[RelayCommand]\n    private async Task Checkout()', new_cmd + '    [RelayCommand]\n    private async Task Checkout()')

with open('./PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
