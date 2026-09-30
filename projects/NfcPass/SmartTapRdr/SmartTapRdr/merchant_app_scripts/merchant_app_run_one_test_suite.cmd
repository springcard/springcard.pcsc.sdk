@echo off
REM usage : projects-internal\dotnet\SmartTapRdr\run_merchant_app_test.cmd <test_suite_id>
set test_suite=%1

REM mode is pcsc, export or springcore
if [%2] == [] (
    set mode=pcsc
)
if NOT [%2] == []  (
    set mode=%2
)
echo mode: %mode%

echo ------------------------- TEST SUITE %test_suite% -------------------------

REM Determine number of test cases in test suite
find /C "testSuiteId" <  D:\dev\springcard.pcsc.sdk\projects-internal\dotnet\SmartTapRdr\conf\%test_suite%\test_suite_%test_suite%.json > num_of_test.txt
set /p max_test=< num_of_test.txt
echo Number of test = %max_test%
rm num_of_test.txt


REM Memo: in Android name the test suite test_suite_n where n is the test suite number

FOR /L %%G IN (1,1,%max_test%) DO (
    @echo on
    call merchant_app_run_one_test_case.cmd %test_suite% %%G %mode%
    @echo off
)

REM Once finished, export the result and store it on the computer
echo Retiving result file
sleep 1
adb shell am broadcast -a com.google.commerce.tapandpay.merchantapp.SAVE_TEST_SUITE_RESULTS_TO_FILE --es test_suite_name 'test_suite_%test_suite%' --es file_name 'results_test_suite_%test_suite%.json'
adb pull /sdcard/Documents/testcasesfiles/results_test_suite_%test_suite%.json D:\dev\springcard.pcsc.sdk\_output\

echo Finished TEST SUITE %test_suite%