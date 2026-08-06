def fix_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Remove all the bad `-e` stuff and trailing ResourceDictionary tags
    content = content.replace('-e', '')
    content = content.replace('<Style TargetType="Button" BasedOn="{StaticResource ModernButton}" />\n</ResourceDictionary>', '')
    content = content.replace('\n    <Style TargetType="DataGrid" BasedOn="{StaticResource ModernDataGrid}" />\n</ResourceDictionary>', '')
    
    # We will manually clean up
    lines = content.split('\n')
    cleaned_lines = []
    for line in lines:
        if line.strip() == '</ResourceDictionary>':
            continue
        if '<Style TargetType="Button" BasedOn="{StaticResource ModernButton}" />' in line:
            continue
        if '<Style TargetType="DataGrid" BasedOn="{StaticResource ModernDataGrid}" />' in line:
            continue
        cleaned_lines.append(line)
        
    final_content = '\n'.join(cleaned_lines)
    
    if 'ButtonStyles.xaml' in filepath:
        final_content += '\n    <Style TargetType="Button" BasedOn="{StaticResource ModernButton}" />\n</ResourceDictionary>'
    elif 'GridStyles.xaml' in filepath:
        final_content += '\n    <Style TargetType="DataGrid" BasedOn="{StaticResource ModernDataGrid}" />\n</ResourceDictionary>'
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(final_content)

fix_file('./PosCore/Themes/ButtonStyles.xaml')
fix_file('./PosCore/Themes/GridStyles.xaml')
