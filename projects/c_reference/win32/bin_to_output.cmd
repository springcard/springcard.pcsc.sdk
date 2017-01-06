@echo off

if not exist ..\..\..\..\bin goto Fin

if not exist "..\..\..\..\_output\win32" mkdir ..\..\..\..\_output\win32
copy ..\..\..\..\bin\win32\*.dll ..\..\..\..\_output\win32\
if exist ..\..\..\..\bin copy ..\..\..\..\bin\win32\*.lib ..\..\..\..\_output\win32\

if not exist "..\..\..\..\_output\win64" mkdir ..\..\..\..\_output\win64
if exist ..\..\..\..\bin copy ..\..\..\..\bin\win64\*.dll ..\..\..\..\_output\win64\
if exist ..\..\..\..\bin copy ..\..\..\..\bin\win64\*.lib ..\..\..\..\_output\win64\

:Fin
echo Nothing to do
:q
