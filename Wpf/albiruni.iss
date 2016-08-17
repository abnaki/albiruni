; Inno Setup 
#define AppVer GetFileVersion('bin\Release\Albiruni.exe')
#if AppVer == ""
#error GetFileVersion() failed maybe because file does not exist right?
#endif

[Setup]
AppName=Albiruni
AppVersion={#AppVer}
AppCopyright=Copyright (C) 2016- Abnaki Light Industry LLC
LicenseFile=..\LICENSE
OutputBaseFilename=Albiruni-{#AppVer}-Setup
DefaultDirName={pf}\Albiruni
DefaultGroupName=Albiruni
UninstallDisplayIcon={app}\Albiruni.exe
ArchitecturesInstallIn64BitMode=x64 ia64
DisableProgramGroupPage=yes
Compression=lzma2
SolidCompression=yes
SetupIconFile=albiruni.ico
OutputDir=SetupOutput
; SignTool=signtool ; can't prompt for passphrase

[Files]
Source: "bin\Release\*"; Excludes: "*.pdb,*vshost*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
Source: "..\Other\OtherLicenses\*"; DestDir: "{app}\OtherLicenses"; Flags: recursesubdirs
Source: "..\Sample\*"; DestDir: "{app}\Sample"; Excludes: "*.albiruni*,*.abt"; Flags: recursesubdirs

[Icons]
Name: "{group}\Albiruni"; Filename: "{app}\Albiruni.exe"

