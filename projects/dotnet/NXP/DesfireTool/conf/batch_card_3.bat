@echo off
@SET DEBUG_LEVEL=5
@SET DESFIRE_TOOL=..\DesfireTool.exe %*

@echo -- Select Master Card application
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% SelectApplication --application-id=000000
if errorlevel 1 ( goto :the_end )

@echo -- Authenticate using Master key
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep Authenticate --key-id=00 --key-value=00000000000000000000000000000000
if errorlevel 1 ( goto :the_end )

@echo -- Format this PICC
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% FormatPICC
if errorlevel 1 ( goto :the_end )

@echo -- Create an application (6 keys, AES)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateApplication --application-id=414553 --key-settings=0F87
if errorlevel 1 ( goto :the_end )

@echo -- Select application 414553
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep SelectApplication --application-id=414553
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 1 (StdDataFile, MAC)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateStdDataFile --file-id=01 --comm-mode=01 --access-rights=1000 --size=28
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 2 (Value File, MAC)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateValueFile --file-id=02 --comm-mode=01 --access-rights=2000 --lower-limit=0 --upper-limit=500 --initial-value=250 --value-flags=00
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 3 (Cyclic Record, MAC)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateCyclicRecordFile --file-id=03 --comm-mode=01 --access-rights=3000 --size=25 --count=2 
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 4 (StdDataFile, Fully Enciphered)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateStdDataFile --file-id=04 --comm-mode=03 --access-rights=4000 --size=38
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 5 (Value File, Fully Enciphered)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateValueFile --file-id=05 --comm-mode=03 --access-rights=5000 --lower-limit=0 --upper-limit=400000000 --initial-value=305419896 --value-flags=00
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 6 (Cyclic Record, Fully Enciphered)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateCyclicRecordFile --file-id=06 --comm-mode=03 --access-rights=6000 --size=36 --count=2 
if errorlevel 1 ( goto :the_end )

@echo -- Authenticate with Application's master key
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep AuthenticateAES --key-id=00 --key-value=00000000000000000000000000000000
if errorlevel 1 ( goto :the_end )

@echo -- Write some data to the file 1
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep WriteData --file-id=01 --comm-mode=01 --offset=0 --data=5374644461746146696C65204445534669726520544445532D4D4143
if errorlevel 1 ( goto :the_end )

@echo -- Write some data to the file 3
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep WriteRecord --file-id=03 --comm-mode=01 --number=0 --data=4379636C6963205265636F726420446573666972652D4D4143
if errorlevel 1 ( goto :the_end )

@echo -- Commit those data to the file 3
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CommitTransaction 
if errorlevel 1 ( goto :the_end )

@echo -- Write some data to the file 4
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep WriteData --file-id=04 --comm-mode=03 --offset=0 --data=5374644461746146696C65204445534669726520544445532D46756C6C204369706865726564
if errorlevel 1 ( goto :the_end )

@echo -- Write some data to the file 6
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep WriteRecord --file-id=06 --comm-mode=03 --number=0 --data=4379636C6963205265636F726420446573666972652D46756C6C79204369706865726564
if errorlevel 1 ( goto :the_end )

@echo -- Commit those data to the file 6
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CommitTransaction 
if errorlevel 1 ( goto :the_end )


@echo -- Changes Keys 1 to 6
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKeyAES --key-id=01 --old-key-value=00000000000000000000000000000000 --key-value=101112131415161718191A1B1C1D1E1F --key-version=00
if errorlevel 1 ( goto :the_end )
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKeyAES --key-id=02 --old-key-value=00000000000000000000000000000000 --key-value=202122232425262728292A2B2C2D2E2F --key-version=00
if errorlevel 1 ( goto :the_end )
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKeyAES --key-id=03 --old-key-value=00000000000000000000000000000000 --key-value=303132333435363738393A3B3C3D3E3F --key-version=00
if errorlevel 1 ( goto :the_end )
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKeyAES --key-id=04 --old-key-value=00000000000000000000000000000000 --key-value=404142434445464748494A4B4C4D4E4F --key-version=00
if errorlevel 1 ( goto :the_end )
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKeyAES --key-id=05 --old-key-value=00000000000000000000000000000000 --key-value=505152535455565758595A5B5C5D5E5F --key-version=00
if errorlevel 1 ( goto :the_end )
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKeyAES --key-id=06 --old-key-value=00000000000000000000000000000000 --key-value=606162636465666768696A6B6C6D6E6F --key-version=00
if errorlevel 1 ( goto :the_end )

@echo ___ Success  ___
:the_end
@echo ___ Finished  ___