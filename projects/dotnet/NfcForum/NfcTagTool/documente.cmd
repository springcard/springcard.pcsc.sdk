@echo off
pushd %0\..
mkdir .\doc
robodoc.exe --src . --doc .\doc --multidoc --html --index --toc --folds
pause
