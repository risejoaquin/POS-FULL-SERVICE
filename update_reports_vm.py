import re

filepath = './PosCore/ViewModels/ReportsViewModel.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

new_commands = """    [RelayCommand]
    private async Task FilterToday()
    {
        StartDate = DateTime.Now.Date;
        EndDate = DateTime.Now.Date;
        await LoadData();
    }

    [RelayCommand]
    private async Task FilterLastWeek()
    {
        StartDate = DateTime.Now.Date.AddDays(-7);
        EndDate = DateTime.Now.Date;
        await LoadData();
    }

    [RelayCommand]
    private async Task FilterThisMonth()
    {
        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        EndDate = DateTime.Now.Date;
        await LoadData();
    }

"""

if "FilterToday" not in content:
    # Insert before LoadData method
    content = content.replace("    [RelayCommand]\n    private async Task LoadData()", new_commands + "    [RelayCommand]\n    private async Task LoadData()")

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Commands added.")
else:
    print("Commands already exist.")
