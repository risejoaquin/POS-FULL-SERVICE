with open('PosCore/Services/LicenseService.cs', 'r') as f:
    c = f.read()

c = c.replace("""        // Si nunca se ha validado, no permitir
        // Fallback temporal para ambientes donde el server aún no tiene el controlador de licencias
        Log.Warning("No se pudo contactar al servidor de licencias. Permitiendo acceso fallback local.");
        return true;""", """        MessageBox.Show("No se puede iniciar el sistema. No hay una licencia válida registrada y no se ha podido contactar con el servidor.", "Error de Licencia", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;""")

with open('PosCore/Services/LicenseService.cs', 'w') as f:
    f.write(c)
