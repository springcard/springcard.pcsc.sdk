@echo off
pushd %0\..
copy scripts\* %1 /Y
copy scripts\* ..\..\_output /Y
rem call i:\builder\tools\signtool.cmd ..\..\_output\win32\pcscdiag2.exe
