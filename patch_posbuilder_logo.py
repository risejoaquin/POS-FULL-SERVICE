import re

with open('./PosBuilder/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Make sure it copies to PosClient/logo.ext
content = content.replace('string destPath = System.IO.Path.Combine(outputDir, "logo" + ext);', 
                          'string destPath = System.IO.Path.Combine(outputDir, "PosClient", "logo" + ext);\n                    if (!System.IO.Directory.Exists(System.IO.Path.Combine(outputDir, "PosClient"))) System.IO.Directory.CreateDirectory(System.IO.Path.Combine(outputDir, "PosClient"));')

with open('./PosBuilder/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
