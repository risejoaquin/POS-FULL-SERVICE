filepath = './PosBuilder/App.xaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

colors_xaml = """
        <SolidColorBrush x:Key="PrimaryBrush" Color="#0F172A" />
        <SolidColorBrush x:Key="SecondaryBrush" Color="#3B82F6" />
        <SolidColorBrush x:Key="AccentBrush" Color="#6366F1" />
        <SolidColorBrush x:Key="SuccessBrush" Color="#10B981" />
        <SolidColorBrush x:Key="ErrorBrush" Color="#EF4444" />
        <SolidColorBrush x:Key="WarningBrush" Color="#F59E0B" />
        <SolidColorBrush x:Key="BackgroundBrush" Color="#F8FAFC" />
        <SolidColorBrush x:Key="SurfaceBrush" Color="#FFFFFF" />
        <SolidColorBrush x:Key="TextPrimaryBrush" Color="#0F172A" />
        <SolidColorBrush x:Key="TextSecondaryBrush" Color="#64748B" />
        <SolidColorBrush x:Key="BorderBrush" Color="#E2E8F0" />
"""

if "TextPrimaryBrush" not in content:
    content = content.replace("<Application.Resources>", "<Application.Resources>\n" + colors_xaml)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
