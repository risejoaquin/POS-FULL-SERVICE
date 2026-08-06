with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
"""        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var lowerQuery = SearchQuery.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(lowerQuery) || p.Barcode.ToLower().Contains(lowerQuery));
        }""",
"""        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var lowerQuery = SearchQuery.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(lowerQuery) || p.Barcode.ToLower().Contains(lowerQuery));
        }
        
        if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "Todas")
        {
            query = query.Where(p => p.Category == SelectedCategory);
        }"""
)

with open('./PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
