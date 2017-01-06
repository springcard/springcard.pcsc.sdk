@echo off
pushd %0\..
set BITS=32
if ."%ProgramW6432%"==."%ProgramFiles%" set BITS=64
pcsc_no_minidriver%BITS%.exe -q -a 3B8F800146666D010112020207FF0302001304BF -n "NFC Smartphone" -y
popd
pause