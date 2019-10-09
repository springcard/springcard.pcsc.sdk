#ifndef __DESFIRE_CARD_H__
#define __DESFIRE_CARD_H__

#define APPLICATION_ID_SIZE          3
#define MAX_INFO_FRAME_SIZE          60

/*
 * DESFire commands (codes sent by the PCD)
 * ----------------------------------------
 */

#define DF_AUTHENTICATE                     0x0A
#define DF_CREDIT                           0x0C
#define DF_AUTHENTICATE_ISO                 0x1A
#define DF_LIMITED_CREDIT                   0x1C
#define DF_WRITE_RECORD                     0x3B
#define DF_WRITE_DATA                       0x3D
#define DF_GET_KEY_SETTINGS                 0x45
#define DF_GET_CARD_UID                     0x51
#define DF_CHANGE_KEY_SETTINGS              0x54
#define DF_SELECT_APPLICATION               0x5A
#define DF_SET_CONFIGURATION                0x5C
#define DF_CHANGE_FILE_SETTINGS             0x5F
#define DF_GET_VERSION                      0x60
#define DF_GET_ISO_FILE_IDS                 0x61
#define DF_GET_KEY_VERSION                  0x64
#define DF_GET_APPLICATION_IDS              0x6A
#define DF_GET_VALUE                        0x6C
#define DF_GET_DF_NAMES                     0x6D
#define DF_GET_FREE_MEMORY                  0x6E
#define DF_GET_FILE_IDS                     0x6F
#define DF_ABORT_TRANSACTION                0xA7
#define DF_AUTHENTICATE_AES                 0xAA
#define DF_READ_RECORDS                     0xBB
#define DF_READ_DATA                        0xBD
#define DF_CREATE_CYCLIC_RECORD_FILE        0xC0
#define DF_CREATE_LINEAR_RECORD_FILE        0xC1
#define DF_CHANGE_KEY                       0xC4
#define DF_COMMIT_TRANSACTION               0xC7
#define DF_CREATE_APPLICATION               0xCA
#define DF_CREATE_BACKUP_DATA_FILE          0xCB
#define DF_CREATE_VALUE_FILE                0xCC
#define DF_CREATE_STD_DATA_FILE             0xCD
#define DF_DELETE_APPLICATION               0xDA
#define DF_DEBIT                            0xDC
#define DF_DELETE_FILE                      0xDF
#define DF_CLEAR_RECORD_FILE                0xEB
#define DF_GET_FILE_SETTINGS                0xF5
#define DF_FORMAT_PICC                      0xFC


/*
 * DESFire status (codes returned by the PICC)
 * -------------------------------------------
 */

/**d* DesfireAPI/DF_OPERATION_OK
 *
 * NAME
 *   DF_OPERATION_OK
 *
 * DESCRIPTION
 *   Function was executed without failure
 *
 **/
#define DF_OPERATION_OK                     0x00

/**d* DesfireAPI/DF_NO_CHANGES
 *
 * NAME
 *   DF_NO_CHANGES
 *
 * DESCRIPTION
 *   Desfire error : no changes done to backup file, no need to commit/abort
 *
 **/
#define DF_NO_CHANGES                       0x0C

/**d* DesfireAPI/DF_OUT_OF_EEPROM_ERROR
 *
 * NAME
 *   DF_OUT_OF_EEPROM_ERROR
 *
 * DESCRIPTION
 *   Desfire error : insufficient NV-memory to complete command
 *
 **/
#define DF_OUT_OF_EEPROM_ERROR              0x0E

/**d* DesfireAPI/DF_ILLEGAL_COMMAND_CODE
 *
 * NAME
 *   DF_ILLEGAL_COMMAND_CODE
 *
 * DESCRIPTION
 *   Desfire error : command code not supported
 *
 **/
#define DF_ILLEGAL_COMMAND_CODE             0x1C

/**d* DesfireAPI/DF_INTEGRITY_ERROR
 *
 * NAME
 *   DF_INTEGRITY_ERROR
 *
 * DESCRIPTION
 *   Desfire error : CRC or MAC does not match, or invalid padding bytes
 *
 **/
#define DF_INTEGRITY_ERROR                  0x1E

/**d* DesfireAPI/DF_NO_SUCH_KEY
 *
 * NAME
 *   DF_NO_SUCH_KEY
 *
 * DESCRIPTION
 *   Desfire error : invalid key number specified
 *
 **/
#define DF_NO_SUCH_KEY                      0x40

/**d* DesfireAPI/DF_LENGTH_ERROR
 *
 * NAME
 *   DF_LENGTH_ERROR
 *
 * DESCRIPTION
 *   Desfire error : length of command string invalid
 *
 **/
#define DF_LENGTH_ERROR                     0x7E

/**d* DesfireAPI/DF_PERMISSION_DENIED
 *
 * NAME
 *   DF_PERMISSION_DENIED
 *
 * DESCRIPTION
 *   Desfire error : current configuration or status does not allow the requested command
 *
 **/
#define DF_PERMISSION_DENIED                0x9D

/**d* DesfireAPI/DF_PARAMETER_ERROR
 *
 * NAME
 *   DF_PARAMETER_ERROR
 *
 * DESCRIPTION
 *   Desfire error : value of the parameter(s) invalid
 *
 **/
#define DF_PARAMETER_ERROR                  0x9E

/**d* DesfireAPI/DF_APPLICATION_NOT_FOUND
 *
 * NAME
 *   DF_APPLICATION_NOT_FOUND
 *
 * DESCRIPTION
 *   Desfire error : requested application not present on the card
 *
 **/
#define DF_APPLICATION_NOT_FOUND            0xA0

/**d* DesfireAPI/DF_APPL_INTEGRITY_ERROR
 *
 * NAME
 *   DF_APPL_INTEGRITY_ERROR
 *
 * DESCRIPTION
 *   Desfire error : unrecoverable error within application, application will be disabled
 *
 **/
#define DF_APPL_INTEGRITY_ERROR             0xA1

/**d* DesfireAPI/DF_AUTHENTICATION_CORRECT
 *
 * NAME
 *   DF_AUTHENTICATION_CORRECT
 *
 * DESCRIPTION
 *   Desfire success : successfull authentication
 *
 **/
#define DF_AUTHENTICATION_CORRECT           0xAC

/**d* DesfireAPI/DF_AUTHENTICATION_ERROR
 *
 * NAME
 *   DF_AUTHENTICATION_ERROR
 *
 * DESCRIPTION
 *   Desfire error : current authentication status does not allow the requested command
 *
 **/
#define DF_AUTHENTICATION_ERROR             0xAE

/**d* DesfireAPI/DF_ADDITIONAL_FRAME
 *
 * NAME
 *   DF_ADDITIONAL_FRAME
 *
 * DESCRIPTION
 *   Desfire error : additionnal data frame is expected to be sent
 *
 **/
#define DF_ADDITIONAL_FRAME                 0xAF

/**d* DesfireAPI/DF_BOUNDARY_ERROR
 *
 * NAME
 *   DF_BOUNDARY_ERROR
 *
 * DESCRIPTION
 *   Desfire error : attempt to read or write data out of the file's or record's limits
 *
 **/
#define DF_BOUNDARY_ERROR                   0xBE

/**d* DesfireAPI/DF_CARD_INTEGRITY_ERROR
 *
 * NAME
 *   DF_CARD_INTEGRITY_ERROR
 *
 * DESCRIPTION
 *   Desfire error : unrecoverable error within the card, the card will be disabled
 *
 **/
#define DF_CARD_INTEGRITY_ERROR             0xC1

/**d* DesfireAPI/DF_COMMAND_ABORTED
 *
 * NAME
 *   DF_COMMAND_ABORTED
 *
 * DESCRIPTION
 *   Desfire error : the current command has been aborted
 *
 **/
#define DF_COMMAND_ABORTED                  0xCA

/**d* DesfireAPI/DF_CARD_DISABLED_ERROR
 *
 * NAME
 *   DF_CARD_DISABLED_ERROR
 *
 * DESCRIPTION
 *   Desfire error : card was disabled by an unrecoverable error
 *
 **/
#define DF_CARD_DISABLED_ERROR              0xCD

/**d* DesfireAPI/DF_COUNT_ERROR
 *
 * NAME
 *   DF_COUNT_ERROR
 *
 * DESCRIPTION
 *   Desfire error : maximum number of 28 applications has been reached
 *
 **/
#define DF_COUNT_ERROR                      0xCE

/**d* DesfireAPI/DF_DUPLICATE_ERROR
 *
 * NAME
 *   DF_DUPLICATE_ERROR
 *
 * DESCRIPTION
 *   Desfire error : the specified file or application already exists
 *
 **/
#define DF_DUPLICATE_ERROR                  0xDE

/**d* DesfireAPI/DF_FILE_NOT_FOUND
 *
 * NAME
 *   DF_FILE_NOT_FOUND
 *
 * DESCRIPTION
 *   Desfire error : the specified file does not exists
 *
 **/
#define DF_FILE_NOT_FOUND                   0xF0

/**d* DesfireAPI/DF_FILE_INTEGRITY_ERROR
 *
 * NAME
 *   DF_FILE_INTEGRITY_ERROR
 *
 * DESCRIPTION
 *   Desfire error : unrecoverable error within file, file will be disabled
 *
 **/
#define DF_FILE_INTEGRITY_ERROR             0xF1

/**d* DesfireAPI/DF_EEPROM_ERROR
 *
 * NAME
 *   DF_EEPROM_ERROR
 *
 * DESCRIPTION
 *   Desfire error : could not complete NV-memory write operation, due to power loss, aborting
 *
 **/
#define DF_EEPROM_ERROR                     0xEE


/*
 * DESFire API status/error codes
 * ------------------------------
 */

/**d* DesfireAPI/DFCARD_ERROR
 *
 * NAME
 *   DFCARD_ERROR
 *
 * DESCRIPTION
 *   Desfire error : unknown error
 *
 **/
#define DFCARD_ERROR                        -1000

/**d* DesfireAPI/DFCARD_LIB_CALL_ERROR
 *
 * NAME
 *   DFCARD_LIB_CALL_ERROR
 *
 * DESCRIPTION
 *   Desfire error : invalid parameters in function call
 *
 **/
#define DFCARD_LIB_CALL_ERROR               DFCARD_ERROR+1

/**d* DesfireAPI/DFCARD_OUT_OF_MEMORY
 *
 * NAME
 *   DFCARD_OUT_OF_MEMORY
 *
 * DESCRIPTION
 *   Desfire error : not enough memory
 *
 **/
#define DFCARD_OUT_OF_MEMORY                DFCARD_ERROR+2

/**d* DesfireAPI/DFCARD_OVERFLOW
 *
 * NAME
 *   DFCARD_OVERFLOW
 *
 * DESCRIPTION
 *   Desfire error : supplied buffer is too short
 *
 **/
#define DFCARD_OVERFLOW                     DFCARD_ERROR+3

/**d* DesfireAPI/DFCARD_WRONG_KEY
 *
 * NAME
 *   DFCARD_WRONG_KEY
 *
 * DESCRIPTION
 *   Desfire error : wrong DES/3DES key
 *
 **/
#define DFCARD_WRONG_KEY                    DFCARD_ERROR+4

/**d* DesfireAPI/DFCARD_WRONG_MAC
 *
 * NAME
 *   DFCARD_WRONG_MAC
 *
 * DESCRIPTION
 *   Desfire error : wrong MAC in incoming data
 *
 **/
#define DFCARD_WRONG_MAC                    DFCARD_ERROR+5

/**d* DesfireAPI/DFCARD_WRONG_CRC
 *
 * NAME
 *   DFCARD_WRONG_CRC
 *
 * DESCRIPTION
 *   Desfire error : wrong CRC in incoming data
 *
 **/
#define DFCARD_WRONG_CRC                    DFCARD_ERROR+6

/**d* DesfireAPI/DFCARD_WRONG_LENGTH
 *
 * NAME
 *   DFCARD_WRONG_LENGTH
 *
 * DESCRIPTION
 *   Desfire error : wrong length of incoming data
 *
 **/
#define DFCARD_WRONG_LENGTH                 DFCARD_ERROR+7

/**d* DesfireAPI/DFCARD_WRONG_PADDING
 *
 * NAME
 *   DFCARD_WRONG_PADDING
 *
 * DESCRIPTION
 *   Desfire error : wrong padding in incoming data
 *
 **/
#define DFCARD_WRONG_PADDING                DFCARD_ERROR+8

/**d* DesfireAPI/DFCARD_WRONG_FILE_TYPE
 *
 * NAME
 *   DFCARD_WRONG_FILE_TYPE
 *
 * DESCRIPTION
 *   Desfire error : wrong file type
 *
 **/
#define DFCARD_WRONG_FILE_TYPE              DFCARD_ERROR+9

/**d* DesfireAPI/DFCARD_WRONG_RECORD_SIZE
 *
 * NAME
 *   DFCARD_WRONG_RECORD_SIZE
 *
 * DESCRIPTION
 *   Desfire error : wrong record size
 *
 **/
#define DFCARD_WRONG_RECORD_SIZE            DFCARD_ERROR+10

/**d* DesfireAPI/DFCARD_PCSC_BAD_RESP_LEN
 *
 * NAME
 *   DFCARD_PCSC_BAD_RESP_LEN
 *
 * DESCRIPTION
 *   Desfire error : response too short
 *
 **/
#define DFCARD_PCSC_BAD_RESP_LEN            DFCARD_ERROR+11

/**d* DesfireAPI/DFCARD_PCSC_BAD_RESP_SW
 *
 * NAME
 *   DFCARD_PCSC_BAD_RESP_SW
 *
 * DESCRIPTION
 *   Desfire error : invalid status word
 *
 **/
#define DFCARD_PCSC_BAD_RESP_SW             DFCARD_ERROR+12

/**d* DesfireAPI/DFCARD_UNEXPECTED_CHAINING
 *
 * NAME
 *   DFCARD_UNEXPECTED_CHAINING
 *
 * DESCRIPTION
 *   Desfire error : card is chaining, where it shouldn't
 *
 **/
#define DFCARD_UNEXPECTED_CHAINING          DFCARD_ERROR+13

/**d* DesfireAPI/DFCARD_FUNC_NOT_AVAILABLE
 *
 * NAME
 *   DFCARD_FUNC_NOT_AVAILABLE
 *
 * DESCRIPTION
 *   Desfire error : the function is not currently available
 *
 **/
#define DFCARD_FUNC_NOT_AVAILABLE           DFCARD_ERROR+14

/*
 * DESFire file types
 * ------------------
 */
#define	DF_STANDARD_DATA_FILE			          0x00
#define	DF_BACKUP_DATA_FILE				          0x01
#define	DF_VALUE_FILE                       0x02
#define	DF_LINEAR_RECORD_FILE			          0x03
#define	DF_CYCLIC_RECORD_FILE			          0x04

/*
 * DESFire communication mode flag combinations.
 * ---------------------------------------------
 */
#define	DF_COMM_MODE_PLAIN			            0x00
#define	DF_COMM_MODE_MACED			            0x01
#define	DF_COMM_MODE_PLAIN2			            0x02
#define	DF_COMM_MODE_ENCIPHERED		          0x03

/*
 * DESFire access rights
 * ---------------------
 */

/* Shift counts for extracting the individual AccessRight settings. */
#define DF_READ_ONLY_ACCESS_SHIFT           12
#define DF_WRITE_ONLY_ACCESS_SHIFT          8
#define DF_READ_WRITE_ACCESS_SHIFT          4

/* AccessRights masks. */
#define DF_READ_ONLY_ACCESS_MASK            0xF000
#define DF_WRITE_ONLY_ACCESS_MASK           0x0F00
#define DF_READ_WRITE_ACCESS_MASK           0x00F0
#define DF_CHANGE_RIGHTS_ACCESS_MASK        0x000F

/*
 * DESFire flags for CreateApplication
 * -----------------------------------
 */
#define DF_APPLSETTING1_MASTER_CHANGEABLE   0x01
#define DF_APPLSETTING1_FREE_LISTING        0x02
#define DF_APPLSETTING1_FREE_CREATE_DELETE  0x04
#define DF_APPLSETTING1_CONFIG_CHANGEABLE   0x08
#define DF_APPLSETTING1_SAME_KEY_NEEDED     0xE0
#define DF_APPLSETTING1_ALL_KEYS_FROZEN     0xF0

#define DF_APPLSETTING2_ISO_EF_IDS          0x20
#define DF_APPLSETTING2_DES_OR_3DES2K       0x00
#define DF_APPLSETTING2_3DES3K              0x40
#define DF_APPLSETTING2_AES                 0x80

/*
 * DF_VERSION_INFO
 * ---------------
 * Structure for returning the information supplied by the GetVersion command.
 */
typedef struct
{
  BYTE    bHwVendorID;
  BYTE    bHwType;
  BYTE    bHwSubType;
  BYTE    bHwMajorVersion;
  BYTE    bHwMinorVersion;
  BYTE    bHwStorageSize;
  BYTE    bHwProtocol;
  BYTE    bSwVendorID;
  BYTE    bSwType;
  BYTE    bSwSubType;
  BYTE    bSwMajorVersion;
  BYTE    bSwMinorVersion;
  BYTE    bSwStorageSize;
  BYTE    bSwProtocol;
  BYTE    abUid[7];
  BYTE    abBatchNo[5];
  BYTE    bProductionCW;
  BYTE    bProductionYear;
} DF_VERSION_INFO;

/*
 * DF_ADDITIONAL_FILE_SETTINGS
 * ---------------------------
 * Union for returning the information supplied by the GetFileSettings command.
 * Use stDataFileSettings for Standard Data Files and Backup Data Files.
 * Use stValueFileSettings for Value Files.
 * Use stRecordFileSettings for Linear Record Files and Cyclic Record Files.
 */
typedef union
{
  struct
  {
    DWORD   eFileSize;
  } stDataFileSettings;

  struct
  {
    LONG    lLowerLimit;
    LONG    lUpperLimit;
    DWORD   eLimitedCredit;
    BYTE    bLimitedCreditEnabled;
  } stValueFileSettings;

  struct
  {
    DWORD   eRecordSize;
    DWORD   eMaxNRecords;
    DWORD   eCurrNRecords;
  } stRecordFileSettings;
} DF_ADDITIONAL_FILE_SETTINGS;

/* Data item lengths for the GetFileSettings command */
#define	DF_FILE_SETTINGS_COMMON_LENGTH	4
#define	DF_DATA_FILE_SETTINGS_LENGTH	  3
#define	DF_VALUE_FILE_SETTINGS_LENGTH	 13
#define	DF_RECORD_FILE_SETTINGS_LENGTH  9

#define	DF_APPLICATION_ID_SIZE			3

#define	DF_MAX_INFO_FRAME_SIZE			59
#define	DF_MAX_FILE_DATA_SIZE			  8192

#define DF_ISO_WRAPPING_OFF         0
#define DF_ISO_WRAPPING_CARD        1
#define DF_ISO_WRAPPING_READER      2

#define DF_ISO_CIPHER_UNSET         0
#define DF_ISO_CIPHER_2KDES         2
#define DF_ISO_CIPHER_3KDES         4
#define DF_ISO_CIPHER_AES           9

#define DF_ISO_APPLICATION_KEY      0x80
#define DF_ISO_CARD_MASTER_KEY      0x00

/*
 * DF_ISO_APPLICATION_ST
 * ---------------------
 * Structure for the GetIsoDFNames command
 */
typedef struct
{
	DWORD dwAid;
  WORD  wIsoId;
  BYTE  abIsoName[16];
  BYTE  bIsoNameLen;
} DF_ISO_APPLICATION_ST;

#endif
