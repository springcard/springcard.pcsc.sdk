@echo off
pushd %0\..
set BITS=32
if ."%ProgramW6432%"==."%ProgramFiles%" set BITS=64
pcsc_no_minidriver%BITS%.exe -q -a 3B878001C105xxxxxxxxxxxx -n "NXP Mifare Plus T=CL" -y
pcsc_no_minidriver%BITS%.exe -q -a 3B8180018080 -n "NXP Desfire" -y
pcsc_no_minidriver%BITS%.exe -q -a 3B6F0000805Axxxxxxxxxxxxxxxxxxxx829000 -n "Calypso Rev1 (Innovatron protocol)" -y
popd
pause