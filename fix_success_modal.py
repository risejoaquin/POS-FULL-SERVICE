import re

# Update SuccessModal.xaml
with open('./PosBuilder/Views/SuccessModal.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('Volver al Resumen', 'Cerrar Generador')
content = content.replace('Nueva Configuración', 'Abrir Carpeta Creada')

with open('./PosBuilder/Views/SuccessModal.xaml', 'w', encoding='utf-8') as f:
    f.write(content)

# Update SuccessModal.xaml.cs
with open('./PosBuilder/Views/SuccessModal.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()
    
content = content.replace('this.Close();', 'Application.Current.Shutdown();')

with open('./PosBuilder/Views/SuccessModal.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
