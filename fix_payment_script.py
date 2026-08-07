import os
import re

filepath = './PosCore/Views/PaymentWindow.xaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Remove Width="380"
new_content = re.sub(r'Width="380"', '', content)

# Add horizontal content alignment stretch to ListView
if 'HorizontalContentAlignment="Stretch"' not in new_content:
    listview_pattern = r'<ListView x:Name="PaymentsList"[^>]*>'
    listview_match = re.search(listview_pattern, new_content)
    if listview_match:
        # We need to insert ItemContainerStyle
        style_xml = """
                        <ListView.ItemContainerStyle>
                            <Style TargetType="ListViewItem">
                                <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
                            </Style>
                        </ListView.ItemContainerStyle>"""
        # Insert after the ListView tag
        new_content = new_content[:listview_match.end()] + style_xml + new_content[listview_match.end():]

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(new_content)
print(f"Fixed PaymentWindow.xaml")
