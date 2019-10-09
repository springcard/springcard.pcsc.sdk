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
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep FormatPICC
if errorlevel 1 ( goto :the_end )

@echo -- Create an application (6 keys, 3K3DES)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateApplication --application-id=334B54 --key-settings=0F47
if errorlevel 1 ( goto :the_end )

@echo -- Select application 334B54
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep SelectApplication --application-id=334B54
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 3 (Cyclic Record, MAC)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateCyclicRecordFile --file-id=03 --comm-mode=01 --access-rights=3000 --size=25 --count=2 
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 4 (StdDataFile, Fully Enciphered)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateStdDataFile --file-id=04 --comm-mode=03 --access-rights=4000 --size=38
if errorlevel 1 ( goto :the_end )

@echo -- Create a new data file 5 (Cyclic Record, Fully Enciphered)
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep CreateStdDataFile --file-id=06 --comm-mode=03 --access-rights=6000 --size=36
if errorlevel 1 ( goto :the_end )

@echo -- Authenticate with Application's master key
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep AuthenticateISO --key-id=00 --key-value=000000000000000000000000000000000000000000000000
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
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep WriteData --file-id=06 --comm-mode=03 --offset=0 --data=4379636C6963205265636F726420446573666972652D46756C6C79204369706865726564
if errorlevel 1 ( goto :the_end )

@echo -- Changes Keys 3, 4 and 6
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKey --key-id=03 --old-key-value=000000000000000000000000000000000000000000000000 --key-value=242322212019181716151413121110090807060504030201 
if errorlevel 1 ( goto :the_end )
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKey --key-id=04 --old-key-value=000000000000000000000000000000000000000000000000 --key-value=010203040506070809101112131415161718192021222324
if errorlevel 1 ( goto :the_end )
@%DESFIRE_TOOL% --verbose=%DEBUG_LEVEL% --keep ChangeKey --key-id=06 --old-key-value=000000000000000000000000000000000000000000000000 --key-value=11992288337766445500aabbccddeeff0f1e2d3c4b5a6978
if errorlevel 1 ( goto :the_end )

@echo ___ Success  ___
:the_end
@echo ___ Finished  ___