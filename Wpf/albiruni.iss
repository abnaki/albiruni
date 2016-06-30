; Inno Setup 
#define AppVer GetFileVersion('bin\Release\Albiruni.exe')

[Setup]
AppName=Albiruni
AppVersion={#AppVer}
OutputBaseFilename=Albiruni-{#AppVer}-Setup
DefaultDirName={pf}\Albiruni
DefaultGroupName=Albiruni
UninstallDisplayIcon={app}\Albiruni.exe
DisableProgramGroupPage=yes
Compression=lzma2
SolidCompression=yes
SetupIconFile=albiruni.ico
OutputDir=SetupOutput

[Files]
Source: "bin\Release\*"; Excludes: "*.pdb,*vshost*"; DestDir: "{app}"

[Icons]
Name: "{group}\Albiruni"; Filename: "{app}\Albiruni.exe"

