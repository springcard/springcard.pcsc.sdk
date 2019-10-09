@echo off
pushd %0\..\
@mkdir %1\conf
copy conf\*.* %1\conf /Y
