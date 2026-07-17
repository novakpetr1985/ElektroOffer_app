#ifndef SourceDir
  #error SourceDir must be supplied by the build script.
#endif
#ifndef OutputDir
  #define OutputDir "..\..\artifacts\installer"
#endif
#ifndef AppVersion
  #define AppVersion "1.12.0"
#endif

[Setup]
AppId={{3F463E75-9A89-49D4-B2D1-08C22771D6B7}
AppName=ElektroOffer
AppVersion={#AppVersion}
AppPublisher=ElektroOffer
DefaultDirName={localappdata}\Programs\ElektroOffer
DefaultGroupName=ElektroOffer
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=ElektroOffer-Setup-{#AppVersion}-x64
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
UninstallDisplayIcon={app}\ElektroOffer_app.exe
CloseApplications=yes
RestartApplications=no
SetupLogging=yes

[Languages]
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\ElektroOffer"; Filename: "{app}\ElektroOffer_app.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\ElektroOffer"; Filename: "{app}\ElektroOffer_app.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Vytvořit zástupce na ploše"; GroupDescription: "Další možnosti:"; Flags: unchecked

[Run]
Filename: "{app}\ElektroOffer_app.exe"; Description: "Spustit ElektroOffer"; Flags: nowait postinstall skipifsilent
