/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 10:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_const.
  /// </summary>
  public partial class Desfire
  {
    
    const UInt32 SCARD_E_INSUFFICIENT_BUFFER  = 0x80100008;
    const UInt32 SCARD_W_REMOVED_CARD         = 0x80100069;
    const UInt32 SCARD_E_COMM_DATA_LOST       = 0x8010002F;
    const UInt32 SCARD_E_CARD_UNSUPPORTED     = 0x8010001C;
    
    const byte APPLICATION_ID_SIZE            = 3;
    const int MAX_INFO_FRAME_SIZE             = 60;
    
    const byte INF        = 0;

    const byte WANTS_OPERATION_OK      = 0x01;
    const byte WANTS_ADDITIONAL_FRAME  = 0x02;

    const byte FAST_CHAINING_ALLOWED   = 0x08;

    const byte APPEND_COMMAND_CMAC     = 0x10;
    const byte COMPUTE_COMMAND_CMAC    = 0x20;
    const byte CHECK_RESPONSE_CMAC     = 0x40;
    const byte LOOSE_RESPONSE_CMAC     = 0x80;

    const byte KEY_EMPTY               = 0x00;
    //const byte KEY_UNSET               0xFF;

    const byte KEY_LEGACY_DES          = 0x01;
    const byte KEY_LEGACY_3DES         = 0x02;

    const byte KEY_ISO_DES             = 0x81;
    const byte KEY_ISO_3DES2K          = 0x82;

    const byte KEY_ISO_3DES3K          = 0x83;
    const byte KEY_ISO_AES             = 0x84;

    const byte KEY_ISO_MODE            = 0x80;
  /*
   * DESFire commands (codes sent by the PCD)
   * ----------------------------------------
   */

    const byte DF_AUTHENTICATE              = 0x0A;
    const byte DF_CREDIT                    = 0x0C;
    const byte DF_AUTHENTICATE_ISO          = 0x1A;
    const byte DF_LIMITED_CREDIT            = 0x1C;
    const byte DF_WRITE_RECORD              = 0x3B;
    const byte DF_WRITE_DATA                = 0x3D;
    const byte DF_GET_KEY_SETTINGS          = 0x45;
    const byte DF_GET_CARD_UID              = 0x51;
    const byte DF_CHANGE_KEY_SETTINGS       = 0x54;
    const byte DF_SELECT_APPLICATION        = 0x5A;
    const byte DF_SET_CONFIGURATION         = 0x5C;
    const byte DF_CHANGE_FILE_SETTINGS      = 0x5F;
    const byte DF_GET_VERSION               = 0x60;
    const byte DF_GET_ISO_FILE_IDS          = 0x61;
    const byte DF_GET_KEY_VERSION           = 0x64;
    const byte DF_GET_APPLICATION_IDS       = 0x6A;
    const byte DF_GET_VALUE                 = 0x6C;
    const byte DF_GET_DF_NAMES              = 0x6D;
    const byte DF_GET_FREE_MEMORY           = 0x6E;
    const byte DF_GET_FILE_IDS              = 0x6F;
    const byte DF_ABORT_TRANSACTION         = 0xA7;
    const byte DF_AUTHENTICATE_AES          = 0xAA;
    const byte DF_READ_RECORDS              = 0xBB;
    const byte DF_READ_DATA                 = 0xBD;
    const byte DF_CREATE_CYCLIC_RECORD_FILE = 0xC0;
    const byte DF_CREATE_LINEAR_RECORD_FILE = 0xC1;
    const byte DF_CHANGE_KEY                = 0xC4;
    const byte DF_COMMIT_TRANSACTION        = 0xC7;
    const byte DF_CREATE_APPLICATION        = 0xCA;
    const byte DF_CREATE_BACKUP_DATA_FILE   = 0xCB;
    const byte DF_CREATE_VALUE_FILE         = 0xCC;
    const byte DF_CREATE_STD_DATA_FILE      = 0xCD;
    const byte DF_DELETE_APPLICATION        = 0xDA;
    const byte DF_DEBIT                     = 0xDC;
    const byte DF_DELETE_FILE               = 0xDF;
    const byte DF_CLEAR_RECORD_FILE         = 0xEB;
    const byte DF_GET_FILE_SETTINGS         = 0xF5;
    const byte DF_FORMAT_PICC               = 0xFC;

    
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
    const byte DF_OPERATION_OK                     = 0x00;
    
    /**d* DesfireAPI/DF_NO_CHANGES
     *
     * NAME
     *   DF_NO_CHANGES
     *
     * DESCRIPTION
     *   Desfire error : no changes done to backup file, no need to commit/abort
     *
     **/
     const byte DF_NO_CHANGES                       = 0x0C;
    
    /**d* DesfireAPI/DF_OUT_OF_EEPROM_ERROR
     *
     * NAME
     *   DF_OUT_OF_EEPROM_ERROR
     *
     * DESCRIPTION
     *   Desfire error : insufficient NV-memory to complete command
     *
     **/
     const byte DF_OUT_OF_EEPROM_ERROR              = 0x0E;
    
    /**d* DesfireAPI/DF_ILLEGAL_COMMAND_CODE
     *
     * NAME
     *   DF_ILLEGAL_COMMAND_CODE
     *
     * DESCRIPTION
     *   Desfire error : command code not supported
     *
     **/
     const byte DF_ILLEGAL_COMMAND_CODE             = 0x1C;
    
    /**d* DesfireAPI/DF_INTEGRITY_ERROR
     *
     * NAME
     *   DF_INTEGRITY_ERROR
     *
     * DESCRIPTION
     *   Desfire error : CRC or MAC does not match, or invalid padding bytes
     *
     **/
     const byte DF_INTEGRITY_ERROR                  = 0x1E;
    
    /**d* DesfireAPI/DF_NO_SUCH_KEY
     *
     * NAME
     *   DF_NO_SUCH_KEY
     *
     * DESCRIPTION
     *   Desfire error : invalid key number specified
     *
     **/
     const byte DF_NO_SUCH_KEY                      = 0x40;
    
    /**d* DesfireAPI/DF_LENGTH_ERROR
     *
     * NAME
     *   DF_LENGTH_ERROR
     *
     * DESCRIPTION
     *   Desfire error : length of command string invalid
     *
     **/
     const byte DF_LENGTH_ERROR                     = 0x7E;
    
    /**d* DesfireAPI/DF_PERMISSION_DENIED
     *
     * NAME
     *   DF_PERMISSION_DENIED
     *
     * DESCRIPTION
     *   Desfire error : current configuration or status does not allow the requested command
     *
     **/
     const byte DF_PERMISSION_DENIED                = 0x9D;
    
    /**d* DesfireAPI/DF_PARAMETER_ERROR
     *
     * NAME
     *   DF_PARAMETER_ERROR
     *
     * DESCRIPTION
     *   Desfire error : value of the parameter(s) invalid
     *
     **/
     const byte DF_PARAMETER_ERROR                  = 0x9E;
    
    /**d* DesfireAPI/DF_APPLICATION_NOT_FOUND
     *
     * NAME
     *   DF_APPLICATION_NOT_FOUND
     *
     * DESCRIPTION
     *   Desfire error : requested application not present on the card
     *
     **/
     const byte DF_APPLICATION_NOT_FOUND            = 0xA0;
    
    /**d* DesfireAPI/DF_APPL_INTEGRITY_ERROR
     *
     * NAME
     *   DF_APPL_INTEGRITY_ERROR
     *
     * DESCRIPTION
     *   Desfire error : unrecoverable error within application, application will be disabled
     *
     **/
     const byte DF_APPL_INTEGRITY_ERROR             = 0xA1;
    
    /**d* DesfireAPI/DF_AUTHENTICATION_CORRECT
     *
     * NAME
     *   DF_AUTHENTICATION_CORRECT
     *
     * DESCRIPTION
     *   Desfire success : successfull authentication
     *
     **/
     const byte DF_AUTHENTICATION_CORRECT           = 0xAC;
    
    /**d* DesfireAPI/DF_AUTHENTICATION_ERROR
     *
     * NAME
     *   DF_AUTHENTICATION_ERROR
     *
     * DESCRIPTION
     *   Desfire error : current authentication status does not allow the requested command
     *
     **/
     const byte DF_AUTHENTICATION_ERROR             = 0xAE;
    
    /**d* DesfireAPI/DF_ADDITIONAL_FRAME
     *
     * NAME
     *   DF_ADDITIONAL_FRAME
     *
     * DESCRIPTION
     *   Desfire error : additionnal data frame is expected to be sent
     *
     **/
     const byte DF_ADDITIONAL_FRAME                 = 0xAF;
    
    /**d* DesfireAPI/DF_BOUNDARY_ERROR
     *
     * NAME
     *   DF_BOUNDARY_ERROR
     *
     * DESCRIPTION
     *   Desfire error : attempt to read or write data out of the file's or record's limits
     *
     **/
     const byte DF_BOUNDARY_ERROR                   = 0xBE;
    
    /**d* DesfireAPI/DF_CARD_INTEGRITY_ERROR
     *
     * NAME
     *   DF_CARD_INTEGRITY_ERROR
     *
     * DESCRIPTION
     *   Desfire error : unrecoverable error within the card, the card will be disabled
     *
     **/
     const byte DF_CARD_INTEGRITY_ERROR             = 0xC1;
    
    /**d* DesfireAPI/DF_COMMAND_ABORTED
     *
     * NAME
     *   DF_COMMAND_ABORTED
     *
     * DESCRIPTION
     *   Desfire error : the current command has been aborted
     *
     **/
     const byte DF_COMMAND_ABORTED                  = 0xCA;
    
    /**d* DesfireAPI/DF_CARD_DISABLED_ERROR
     *
     * NAME
     *   DF_CARD_DISABLED_ERROR
     *
     * DESCRIPTION
     *   Desfire error : card was disabled by an unrecoverable error
     *
     **/
     const byte DF_CARD_DISABLED_ERROR              = 0xCD;
    
    /**d* DesfireAPI/DF_COUNT_ERROR
     *
     * NAME
     *   DF_COUNT_ERROR
     *
     * DESCRIPTION
     *   Desfire error : maximum number of 28 applications has been reached
     *
     **/
     const byte DF_COUNT_ERROR                      = 0xCE;
    
    /**d* DesfireAPI/DF_DUPLICATE_ERROR
     *
     * NAME
     *   DF_DUPLICATE_ERROR
     *
     * DESCRIPTION
     *   Desfire error : the specified file or application already exists
     *
     **/
     const byte DF_DUPLICATE_ERROR                  = 0xDE;
    
    /**d* DesfireAPI/DF_FILE_NOT_FOUND
     *
     * NAME
     *   DF_FILE_NOT_FOUND
     *
     * DESCRIPTION
     *   Desfire error : the specified file does not exists
     *
     **/
     const byte DF_FILE_NOT_FOUND                   = 0xF0;
    
    /**d* DesfireAPI/DF_FILE_INTEGRITY_ERROR
     *
     * NAME
     *   DF_FILE_INTEGRITY_ERROR
     *
     * DESCRIPTION
     *   Desfire error : unrecoverable error within file, file will be disabled
     *
     **/
     const byte DF_FILE_INTEGRITY_ERROR             = 0xF1;
    
    /**d* DesfireAPI/DF_EEPROM_ERROR
     *
     * NAME
     *   DF_EEPROM_ERROR
     *
     * DESCRIPTION
     *   Desfire error : could not complete NV-memory write operation, due to power loss, aborting
     *
     **/
      const byte DF_EEPROM_ERROR                     = 0xEE;


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
    const int DFCARD_ERROR                        = -1000;
    
    /**d* DesfireAPI/DFCARD_LIB_CALL_ERROR
     *
     * NAME
     *   DFCARD_LIB_CALL_ERROR
     *
     * DESCRIPTION
     *   Desfire error : invalid parameters in function call
     *
     **/
     const int DFCARD_LIB_CALL_ERROR               = DFCARD_ERROR+1;
    
    /**d* DesfireAPI/DFCARD_OUT_OF_MEMORY
     *
     * NAME
     *   DFCARD_OUT_OF_MEMORY
     *
     * DESCRIPTION
     *   Desfire error : not enough memory
     *
     **/
     const int DFCARD_OUT_OF_MEMORY                = DFCARD_ERROR+2;
    
    /**d* DesfireAPI/DFCARD_OVERFLOW
     *
     * NAME
     *   DFCARD_OVERFLOW
     *
     * DESCRIPTION
     *   Desfire error : supplied buffer is too short
     *
     **/
     const int DFCARD_OVERFLOW                     = DFCARD_ERROR+3;
    
    /**d* DesfireAPI/DFCARD_WRONG_KEY
     *
     * NAME
     *   DFCARD_WRONG_KEY
     *
     * DESCRIPTION
     *   Desfire error : wrong DES/3DES key
     *
     **/
     const int DFCARD_WRONG_KEY                    = DFCARD_ERROR+4;
    
    /**d* DesfireAPI/DFCARD_WRONG_MAC
     *
     * NAME
     *   DFCARD_WRONG_MAC
     *
     * DESCRIPTION
     *   Desfire error : wrong MAC in incoming data
     *
     **/
     const int DFCARD_WRONG_MAC                    = DFCARD_ERROR+5;
    
    /**d* DesfireAPI/DFCARD_WRONG_CRC
     *
     * NAME
     *   DFCARD_WRONG_CRC
     *
     * DESCRIPTION
     *   Desfire error : wrong CRC in incoming data
     *
     **/
     const int DFCARD_WRONG_CRC                    = DFCARD_ERROR+6;
    
    /**d* DesfireAPI/DFCARD_WRONG_LENGTH
     *
     * NAME
     *   DFCARD_WRONG_LENGTH
     *
     * DESCRIPTION
     *   Desfire error : wrong length of incoming data
     *
     **/
     const int DFCARD_WRONG_LENGTH                 = DFCARD_ERROR+7;
    
    /**d* DesfireAPI/DFCARD_WRONG_PADDING
     *
     * NAME
     *   DFCARD_WRONG_PADDING
     *
     * DESCRIPTION
     *   Desfire error : wrong padding in incoming data
     *
     **/
     const int DFCARD_WRONG_PADDING                = DFCARD_ERROR+8;
    
    /**d* DesfireAPI/DFCARD_WRONG_FILE_TYPE
     *
     * NAME
     *   DFCARD_WRONG_FILE_TYPE
     *
     * DESCRIPTION
     *   Desfire error : wrong file type
     *
     **/
     const int DFCARD_WRONG_FILE_TYPE              = DFCARD_ERROR+9;
    
    /**d* DesfireAPI/DFCARD_WRONG_RECORD_SIZE
     *
     * NAME
     *   DFCARD_WRONG_RECORD_SIZE
     *
     * DESCRIPTION
     *   Desfire error : wrong record size
     *
     **/
     const int DFCARD_WRONG_RECORD_SIZE            = DFCARD_ERROR+10;
    
    /**d* DesfireAPI/DFCARD_PCSC_BAD_RESP_LEN
     *
     * NAME
     *   DFCARD_PCSC_BAD_RESP_LEN
     *
     * DESCRIPTION
     *   Desfire error : response too short
     *
     **/
     const int DFCARD_PCSC_BAD_RESP_LEN            = DFCARD_ERROR+11;
    
    /**d* DesfireAPI/DFCARD_PCSC_BAD_RESP_SW
     *
     * NAME
     *   DFCARD_PCSC_BAD_RESP_SW
     *
     * DESCRIPTION
     *   Desfire error : invalid status word
     *
     **/
     const int DFCARD_PCSC_BAD_RESP_SW             = DFCARD_ERROR+12;
    
    /**d* DesfireAPI/DFCARD_UNEXPECTED_CHAINING
     *
     * NAME
     *   DFCARD_UNEXPECTED_CHAINING
     *
     * DESCRIPTION
     *   Desfire error : card is chaining, where it shouldn't
     *
     **/
     const int DFCARD_UNEXPECTED_CHAINING          = DFCARD_ERROR+13;
    
    /**d* DesfireAPI/DFCARD_FUNC_NOT_AVAILABLE
     *
     * NAME
     *   DFCARD_FUNC_NOT_AVAILABLE
     *
     * DESCRIPTION
     *   Desfire error : the function is not currently available
     *
     **/
     const int DFCARD_FUNC_NOT_AVAILABLE           = DFCARD_ERROR+14;

    /*
     * DESFire file types
     * ------------------
     */
    public const byte DF_STANDARD_DATA_FILE                = 0x00;
    public const byte DF_BACKUP_DATA_FILE                  = 0x01;
    public const byte DF_VALUE_FILE                        = 0x02;
    public const byte DF_LINEAR_RECORD_FILE                = 0x03;
    public const byte DF_CYCLIC_RECORD_FILE                = 0x04;

    /*
     * DESFire communication mode flag combinations.
     * ---------------------------------------------
     */
    public const byte DF_COMM_MODE_PLAIN                   = 0x00;
    public const byte DF_COMM_MODE_MACED                   = 0x01;
    public const byte DF_COMM_MODE_PLAIN2                  = 0x02;
    public const byte DF_COMM_MODE_ENCIPHERED              = 0x03;

    /*
     * DESFire access rights
     * ---------------------
     */
    
    /* Shift counts for extracting the individual AccessRight settings. */
    const byte DF_READ_ONLY_ACCESS_SHIFT           = 12;
    const byte DF_WRITE_ONLY_ACCESS_SHIFT          = 8;
    const byte DF_READ_WRITE_ACCESS_SHIFT          = 4;
    
    /* AccessRights masks. */
    const UInt16 DF_READ_ONLY_ACCESS_MASK            = 0xF000;
    const UInt16 DF_WRITE_ONLY_ACCESS_MASK           = 0x0F00;
    const UInt16 DF_READ_WRITE_ACCESS_MASK           = 0x00F0;
    const UInt16 DF_CHANGE_RIGHTS_ACCESS_MASK        = 0x000F;

    /*
     * DESFire flags for CreateApplication
     * -----------------------------------
     */
    public const byte DF_APPLSETTING1_MASTER_CHANGEABLE   = 0x01;
    public const byte DF_APPLSETTING1_FREE_LISTING        = 0x02;
    public const byte DF_APPLSETTING1_FREE_CREATE_DELETE  = 0x04;
    public const byte DF_APPLSETTING1_CONFIG_CHANGEABLE   = 0x08;
    public const byte DF_APPLSETTING1_SAME_KEY_NEEDED     = 0xE0;
    public const byte DF_APPLSETTING1_ALL_KEYS_FROZEN     = 0xF0;
    
    public const byte DF_APPLSETTING2_ISO_EF_IDS          = 0x20;
    public const byte DF_APPLSETTING2_DES_OR_3DES2K       = 0x00;
    public const byte DF_APPLSETTING2_3DES3K              = 0x40;
    public const byte DF_APPLSETTING2_AES                 = 0x80;


    /* Data item lengths for the GetFileSettings command */
    const int DF_FILE_SETTINGS_COMMON_LENGTH  = 4;
    const int DF_DATA_FILE_SETTINGS_LENGTH    = 3;
    const int DF_VALUE_FILE_SETTINGS_LENGTH   = 13;
    const int DF_RECORD_FILE_SETTINGS_LENGTH  = 9;
    
    const int DF_APPLICATION_ID_SIZE          = 3;
    
    const int DF_MAX_INFO_FRAME_SIZE          = 59;
    const int DF_MAX_FILE_DATA_SIZE           = 8192;
    
    public const int DF_ISO_WRAPPING_OFF         = 0;
    public const int DF_ISO_WRAPPING_CARD        = 1;
    public const int DF_ISO_WRAPPING_READER      = 2;
    
    const int DF_ISO_CIPHER_UNSET         = 0;
    const int DF_ISO_CIPHER_2KDES         = 2;
    const int DF_ISO_CIPHER_3KDES         = 4;
    const int DF_ISO_CIPHER_AES           = 9;
    
    const byte DF_ISO_APPLICATION_KEY      = 0x80;
    const byte DF_ISO_CARD_MASTER_KEY      = 0x00;
   
  
  }
}
