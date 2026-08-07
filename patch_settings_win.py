import re

with open('./PosCore/Views/SettingsWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Add AvailableActions
available_actions = """        public List<ShortcutConfig> Shortcuts { get; set; }
        public List<string> AvailableActions { get; set; } = new List<string>
        {
            "None",
            "OpenShift",
            "OpenReturns",
            "OpenReports",
            "OpenUsers",
            "OpenInventory",
            "SuspendOrder",
            "ResumeOrder",
            "TechSupport",
            "OpenSettings",
            "OpenDiscount"
        };"""

content = content.replace("        public List<ShortcutConfig> Shortcuts { get; set; }", available_actions)

with open('./PosCore/Views/SettingsWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)

