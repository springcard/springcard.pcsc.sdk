@echo off
setlocal enabledelayedexpansion

REM This batch load all test suites inside the merchant app on the phone

dir d:\dev\springcard.pcsc.sdk\ * /s/b | findstr test_suite_[0-9].json > list_test.txt
dir d:\dev\springcard.pcsc.sdk\ * /s/b | findstr test_suite_[0-9][0-9].json >> list_test.txt

adb shell mkdir -p /sdcard/Documents/testcasesfiles
for /F %%A in (list_test.txt) do (
    set fullpath=%%A
    echo !fullpath!
    adb push !fullpath! /sdcard/Documents/testcasesfiles/
    set id=%%~nA
    echo !id!
    adb shell am broadcast -a com.google.commerce.tapandpay.merchantapp.LOAD_TEST_CASES_FROM_FILE --es load_test_cases_file_name '!id!.json' --es test_suite_name '!id!'
) 

rm list_test.txt