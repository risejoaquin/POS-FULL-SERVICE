with open('PosBuilder/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

import re

# Add file copy logic
copy_logic = """
            string outputDir = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Output");
            
            if (!System.IO.Directory.Exists(outputDir))
                System.IO.Directory.CreateDirectory(outputDir);

            if (!string.IsNullOrWhiteSpace(config.LogoPath) && System.IO.File.Exists(config.LogoPath))
            {
                try
                {
                    string ext = System.IO.Path.GetExtension(config.LogoPath);
                    string destPath = System.IO.Path.Combine(outputDir, "logo" + ext);
                    System.IO.File.Copy(config.LogoPath, destPath, true);
                    config.LogoPath = "logo" + ext; // Set to relative path
                }
                catch (Exception) { }
            }
"""

content = content.replace('            string outputDir = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Output");', copy_logic)

with open('PosBuilder/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)

print("MainWindow patched")
