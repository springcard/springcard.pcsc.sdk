
@echo off
echo ------------------------- ALL TESTS -------------------------

REM mode is pcsc, export or springcore
if [%1] == [] (
set mode=pcsc
)
if NOT [%1] == []  (
    set mode=%1
)
echo mode: %mode%

FOR /L %%G IN (0,1,18) DO (
    @echo on
    call merchant_app_run_one_test_suite.cmd %%G %mode%
    @echo off
)