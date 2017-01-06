@echo off
pushd %0\..
set BITS=32
if ."%ProgramW6432%"==."%ProgramFiles%" set BITS=64
pcsc_no_minidriver%BITS%.exe -q -a 3B8F8001804F0CA000000306xxxxxx00000000xx -n "Any memory card (PC/SC v2)" -y
popd
pause