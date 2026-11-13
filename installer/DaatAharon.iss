#define AppName "דעת אהרן"
#define AppExeName "DaatAharon.App.exe"
#define AppVersion GetEnv("APP_VERSION")
#define PublishDir "..\artifacts\publish"
[Setup]
AppId={{A74B1A63-0C1D-4D2B-92F2-9B7D3F71E811}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=דעת אהרן
DefaultDirName={autopf}\DaatAharon
DefaultGroupName={#AppName}
OutputDir=..\artifacts\installer
OutputBaseFilename=DaatAharon-Setup-{#AppVersion}-x64
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#AppExeName}
SetupLogging=yes
[Languages]
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
[Tasks]
Name: "desktopicon"; Description: "יצירת קיצור דרך בשולחן העבודה"; GroupDescription: "קיצורי דרך:"; Flags: unchecked
[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "WebView2Bootstrapper.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
[Run]
Filename: "{tmp}\WebView2Bootstrapper.exe"; Parameters: "/silent /install"; StatusMsg: "מתקין רכיב דפדפן נדרש..."; Flags: waituntilterminated; Check: not IsWebView2Installed
Filename: "{app}\{#AppExeName}"; Description: "הפעלת דעת אהרן"; Flags: nowait postinstall skipifsilent
[Code]
function IsWebView2Installed: Boolean;
var Version: String;
begin
  Result := RegQueryStringValue(HKLM64, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F1E...}', 'pv', Version) and (Version <> '');
  if not Result then Result := RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F1E...}', 'pv', Version) and (Version <> '');
end;
