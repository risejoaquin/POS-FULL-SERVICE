with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
for i, line in enumerate(lines):
    if i == 124: # Line 125 is 124 in 0-based index
        new_lines.append("    private void ApplySearchFilter()\n    {\n")
        new_lines.append("        if (string.IsNullOrWhiteSpace(SearchQuery))\n")
        new_lines.append("        {\n")
        new_lines.append("            FilteredProducts = new ObservableCollection<Product>(Products);\n")
        new_lines.append("            return;\n")
        new_lines.append("        }\n")
    elif i == 126: # line 127
        new_lines.append("        var query = SearchQuery.ToLower();\n")
    else:
        new_lines.append(line)

with open('./PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

