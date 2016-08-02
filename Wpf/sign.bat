@echo off
set /p pw="Passphrase: "
"c:\Program Files\Microsoft SDKs\Windows\v7.1\Bin\signtool.exe" sign /f k:\cert\godaddy\abnaki.pfx /p %pw%  /tr http://tsa.starfieldtech.com /td SHA256 %1
