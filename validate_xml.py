import xml.etree.ElementTree as ET
try:
    ET.parse('./PosCore/Views/InventoryWindow.xaml')
    print("XML is valid")
except Exception as e:
    print(f"Error: {e}")
