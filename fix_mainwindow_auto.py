with open('./PosBuilder/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("System.Diagnostics.Process.Start(clientFallbackProcess);\n                    }\n                }\n                catch (Exception ex)\n                {\n                    System.Windows.MessageBox.Show(\"Error al iniciar las aplicaciones: \" + ex.Message, \"Ejecución Automática\", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);\n                }\n\n                Close();", "System.Diagnostics.Process.Start(clientFallbackProcess);\n                    }\n                }\n                catch (Exception ex)\n                {\n                    System.Windows.MessageBox.Show(\"Error al iniciar las aplicaciones: \" + ex.Message, \"Ejecución Automática\", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);\n                }")

with open('./PosBuilder/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(content)
