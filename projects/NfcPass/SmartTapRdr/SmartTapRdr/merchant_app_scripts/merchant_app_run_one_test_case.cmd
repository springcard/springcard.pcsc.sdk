@echo off
set test_suite=%1
set test_case=%2
REM mode is pcsc, export or springcore
if [%3] == [] (
    set mode=pcsc
)
if NOT [%3] == []  (
    set mode=%3
)
echo mode: %mode%


REM Wake-up phone if necessary
adb shell dumpsys display | grep "mScreenState" > screenState.txt
set /p screenState=<screenState.txt
rm screenState.txt
if "%screenState%" == "  mScreenState=OFF" (
    adb shell input keyevent KEYCODE_WAKEUP
    adb shell input keyevent 82 REM Unlock
    sleep 1
)

REM Unlock screen if necessary
adb shell dumpsys display | grep "mHoldingDisplaySuspendBlocker" > lockState.txt
set /p lockState=<lockState.txt
rm lockState.txt
if "%lockState%" == "  mHoldingDisplaySuspendBlocker=false" (
    REM adb shell input keyevent 82 REM Unlock
    adb shell input touchscreen swipe 400 880 400 380 REM Swipe to unlock
    sleep 1
)

echo ------------------------- TEST %test_suite%.%test_case% -------------------------
if %mode% == pcsc (
    @echo on
    adb shell am broadcast -a com.google.commerce.tapandpay.merchantapp.SET_ACTIVE --es test_suite_name 'test_suite_%test_suite%' --es test_case_name_prefix '%test_suite%.%test_case%' & D:\dev\springcard.pcsc.sdk\_output\SmartTapRdr\SmartTapReadCli.exe -v6  -c  D:\dev\springcard.pcsc.sdk\projects-internal\dotnet\SmartTapRdr\conf\%test_suite%\terminal_configs\%test_suite%.%test_case%.json
    @echo off
)
if %mode% == export (
    @echo on
    D:\dev\springcard.pcsc.sdk\_output\SmartTapRdr\SmartTapReadCli.exe -v6  -c  D:\dev\springcard.pcsc.sdk\projects-internal\dotnet\SmartTapRdr\conf\%test_suite%\terminal_configs\%test_suite%.%test_case%.json -e D:\dev\springcard.springcore.firmware\_output\springcore_google_smart_tap\%test_suite%.%test_case%.multiconf
    @echo off
)
if %mode% == springcore (
    @echo on
    adb shell am broadcast -a com.google.commerce.tapandpay.merchantapp.SET_ACTIVE --es test_suite_name 'test_suite_%test_suite%' --es test_case_name_prefix '%test_suite%.%test_case%' 
    D:\dev\springcard.springcore.firmware\binaries\tools\SpringCoreConfig.exe D:\dev\springcard.springcore.firmware\_output\springcore_google_smart_tap\%test_suite%.%test_case%.multiconf
    REM sleep 4
    sleep 6
    D:\dev\springcard.springcore.firmware\binaries\tools\SpringCoreControl.exe A3
    sleep 3
    D:\dev\springcard.springcore.firmware\binaries\tools\SpringCoreControl.exe A0
    @echo off
)