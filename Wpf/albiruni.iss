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
DisableProgramGroupPage=yes
Compression=lzma2
SolidCompression=yes
SetupIconFile=albiruni.ico
OutputDir=SetupOutput

[Files]
Source: "bin\Release\*"; Excludes: "*.pdb,*vshost*"; DestDir: "{app}"

[Icons]
Name: "{group}\Albiruni"; Filename: "{app}\Albiruni.exe"

