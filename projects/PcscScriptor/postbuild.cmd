@echo off
pushd %0\..
echo %1
xcopy data\* %1 /Y /E /S
