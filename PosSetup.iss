[Setup]
AppName=Super POS Express
AppVersion=1.0.1
DefaultDirName={pf}\Super POS Express
DefaultGroupName=Super POS Express
OutputDir=.\Output
OutputBaseFilename=SuperPOSExpress_Setup
Compression=lzma
SolidCompression=yes

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "PosCore\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Add SQlite dependencies if needed, handled by .NET publish

[Icons]
Name: "{group}\Super POS Express"; Filename: "{app}\PosCore.exe"
Name: "{commondesktop}\Super POS Express"; Filename: "{app}\PosCore.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\PosCore.exe"; Description: "{cm:LaunchProgram,Super POS Express}"; Flags: nowait postinstall skipifsilent
