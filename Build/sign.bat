@echo off
set KDRIVE=K:
if not exist %1 echo Nonexistent %1
if not exist %KDRIVE% set KDRIVE=\\TSCLIENT\K
if not exist %KDRIVE% echo %KDRIVE% not found.
set /p pw="Passphrase: "
"c:\Program Files\Microsoft SDKs\Windows\v7.1\Bin\signtool.exe" sign /f %KDRIVE%\cert\godaddy\abnaki.pfx /p %pw%  /tr http://tsa.starfieldtech.com /td SHA256 %1
