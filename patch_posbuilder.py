import re

with open('./PosBuilder/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the open folder output. Outputout? Maybe it was Path.Combine(outputDir, "out") ?
content = content.replace('Outputout', 'Output')

with open('./PosBuilder/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
