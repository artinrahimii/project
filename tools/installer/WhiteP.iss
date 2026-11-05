#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif
#ifndef Publisher
  #define Publisher "Artin Rahimi"
#endif
#ifndef PublishDir
  #error PublishDir must point to dotnet publish output.
#endif
#ifndef LicensesDir
  #error LicensesDir must point to generated dependency license inventory.
#endif
#ifndef OutputDir
  #define OutputDir "artifacts/releases"
#endif

[Setup]
AppId={{0BFC15C6-2FC5-4FF3-B6C2-9E88AB4F6C22}
AppName=White-P
AppVersion={#AppVersion}
AppVerName=White-P {#AppVersion}
AppPublisher={#Publisher}
AppPublisherURL=https://github.com/artinrahimii/project
AppSupportURL=https://github.com/artinrahimii/project/issues
AppUpdatesURL=https://github.com/artinrahimii/project/releases
AppCopyright=Copyright (C) 2026 {#Publisher}
DefaultDirName={localappdata}\Programs\White-P
DefaultGroupName=White-P
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
OutputDir={#OutputDir}
OutputBaseFilename=WhiteP-Setup-{#AppVersion}-win-x64
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupLogging=yes
UninstallDisplayName=White-P
UninstallDisplayIcon={app}\WhiteP.Host.Windows.exe
VersionInfoCompany={#Publisher}
VersionInfoDescription=White-P Windows Host Installer
VersionInfoProductName=White-P
VersionInfoVersion={#AppVersion}.0
VersionInfoProductVersion={#AppVersion}.0
VersionInfoTextVersion={#AppVersion}
VersionInfoProductTextVersion={#AppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#LicensesDir}\*"; DestDir: "{app}\licenses"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\White-P"; Filename: "{app}\WhiteP.Host.Windows.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\White-P"; Filename: "{app}\WhiteP.Host.Windows.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\WhiteP.Host.Windows.exe"; Description: "Launch White-P"; Flags: postinstall nowait skipifsilent
