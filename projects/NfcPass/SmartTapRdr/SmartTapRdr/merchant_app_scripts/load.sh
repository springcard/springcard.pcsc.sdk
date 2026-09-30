# cf https://developers.google.com/pay/smart-tap/guides/testing-and-certification/test-the-implementation#load-test-cases

# This file is: load.sh
# Usage: ./load.sh test_cases_file suite_name
# Example ./load.sh tests.json "My first test suite"
# Loads all test cases from JSON array into a test suite with given name

echo "am broadcast -a com.google.commerce.tapandpay.merchantapp.LOAD_TEST_CASES --es load_test_cases '"''"$(cat $1)"''"' --es test_suite_name '$2'" > cmd.sh

adb push cmd.sh sdcard
adb shell sh /data/local/tmp/cmd.sh
adb shell rm /data/local/tmp/cmd.sh

rm cmd.sh