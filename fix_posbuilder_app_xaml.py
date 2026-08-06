import os
import re

filepath = './PosBuilder/App.xaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add a dictionary with colors
colors = """        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="/PosCore;component/Themes/Colors.xaml" />
                <!-- Or just define them inline if reference fails, wait, PosBuilder doesn't reference PosCore -->
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
"""

# Wait, PosBuilder doesn't reference PosCore. Let's just inline the colors in PosBuilder App.xaml.
