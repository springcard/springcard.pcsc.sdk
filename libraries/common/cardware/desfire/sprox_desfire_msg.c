/**h* DesfireAPI/Messages
 *
 * NAME
 *   DesfireAPI :: Error messages and misc. translation functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Error message function to retrieve the error message corresponding to the status code
 *
 **/

#include "sprox_desfire_i.h"

/**f* DesfireAPI/GetErrorMessage
 *
 * NAME
 *   GetErrorMessage
 *
 * DESCRIPTION
 *   Retrieves the error message corresponding to the status code
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   const TCHAR* SPROX_Desfire_GetErrorMessage(SWORD status);
 *
 *   [[sprox_desfire_ex.dll]]
 *   const TCHAR* SPROXx_Desfire_GetErrorMessage(SWORD status);
 *
 *   [[pcsc_desfire.dll]]
 *   const TCHAR*  SCardDesfire_GetErrorMessage(LONG status);
 *
 * INPUTS
 *   SPROX_RC status  : value of error code
 *
 * RETURNS
 *   const TCHAR *    : error message corresponding to the status code
 *
 **/
SPROX_API_FUNC_T(const TCHAR *, Desfire_GetErrorMessage) (SPROX_RC status)
{
  switch (status)
  {
    case DF_OPERATION_OK:
      return _T("Function was executed without failure");

    case DFCARD_ERROR:
      return _T("Desfire : unknown error");
    case DFCARD_ERROR - DF_NO_CHANGES:
      return _T("Desfire : no changes done to backup file, no need to commit/abort");
    case DFCARD_ERROR - DF_OUT_OF_EEPROM_ERROR:
      return _T("Desfire : insufficient NV-memory to complete command");
    case DFCARD_ERROR - DF_ILLEGAL_COMMAND_CODE:
      return _T("Desfire : command code not supported");
    case DFCARD_ERROR - DF_INTEGRITY_ERROR:
      return _T("Desfire : CRC or MAC does not match, or invalid padding bytes");
    case DFCARD_ERROR - DF_NO_SUCH_KEY:
      return _T("Desfire : invalid key number specified");
    case DFCARD_ERROR - DF_LENGTH_ERROR:
      return _T("Desfire : length of command string invalid");
    case DFCARD_ERROR - DF_PERMISSION_DENIED:
      return _T("Desfire : current configuration or status does not allow the requested command");
    case DFCARD_ERROR - DF_PARAMETER_ERROR:
      return _T("Desfire : value of the parameter(s) invalid");
    case DFCARD_ERROR - DF_APPLICATION_NOT_FOUND:
      return _T("Desfire : requested application not present on the card");
    case DFCARD_ERROR - DF_APPL_INTEGRITY_ERROR:
      return _T("Desfire : unrecoverable error within application, application will be disabled");
    case DFCARD_ERROR - DF_AUTHENTICATION_CORRECT:
      return _T("Desfire : successfull authentication");
    case DFCARD_ERROR - DF_AUTHENTICATION_ERROR:
      return _T("Desfire : current authentication status does not allow the requested command");
    case DFCARD_ERROR - DF_ADDITIONAL_FRAME:
      return _T("Desfire : additionnal data frame is expected to be sent");
    case DFCARD_ERROR - DF_BOUNDARY_ERROR:
      return _T("Desfire : attempt to read or write data out of the file's or record's limits");
    case DFCARD_ERROR - DF_CARD_INTEGRITY_ERROR:
      return _T("Desfire : unrecoverable error within the card, the card will be disabled");
    case DFCARD_ERROR - DF_COMMAND_ABORTED:
      return _T("Desfire : the current command has been aborted");
    case DFCARD_ERROR - DF_CARD_DISABLED_ERROR:
      return _T("Desfire : card was disabled by an unrecoverable error");
    case DFCARD_ERROR - DF_COUNT_ERROR:
      return _T("Desfire : maximum number of 28 applications has been reached");
    case DFCARD_ERROR - DF_DUPLICATE_ERROR:
      return _T("Desfire : the specified file or application already exists");
    case DFCARD_ERROR - DF_FILE_NOT_FOUND:
      return _T("Desfire : the specified file does not exists");
    case DFCARD_ERROR - DF_FILE_INTEGRITY_ERROR:
      return _T("Desfire : unrecoverable error within file, file will be disabled");
    case DFCARD_ERROR - DF_EEPROM_ERROR:
      return _T("Desfire : could not complete NV-memory write operation, due to power loss, aborting");

    case DFCARD_LIB_CALL_ERROR:
      return _T("Desfire : invalid parameters in function call");
    case DFCARD_OUT_OF_MEMORY:
      return _T("Desfire : not enough memory");
    case DFCARD_OVERFLOW:
      return _T("Desfire : supplied buffer is too short");
    case DFCARD_WRONG_KEY:
      return _T("Desfire : card authentication denied");
    case DFCARD_WRONG_MAC:
      return _T("Desfire : wrong MAC in card's frame");
    case DFCARD_WRONG_CRC:
      return _T("Desfire : wrong CRC in card's fame");
    case DFCARD_WRONG_LENGTH:
      return _T("Desfire : length of card's frame is invalid");
    case DFCARD_WRONG_PADDING:
      return _T("Desfire : wrong padding in card's frame");
    case DFCARD_WRONG_FILE_TYPE:
      return _T("Desfire : wrong file type");
    case DFCARD_WRONG_RECORD_SIZE:
      return _T("Desfire : wrong record size");

    case DFCARD_PCSC_BAD_RESP_LEN:
      return _T("Desfire : card's response is too short");
    case DFCARD_PCSC_BAD_RESP_SW:
      return _T("Desfire : card's status word is invalid");

  }

  return _T("Not a Desfire error code");
}


SPROX_API_FUNC(Desfire_ExplainDataFileSettings) (const BYTE additionnal_settings_array[], DWORD *eFileSize)
{
  DF_ADDITIONAL_FILE_SETTINGS *p = (DF_ADDITIONAL_FILE_SETTINGS *) additionnal_settings_array;

  if (p == NULL)
    return DFCARD_LIB_CALL_ERROR;

  if (eFileSize != NULL)
    *eFileSize = p->stDataFileSettings.eFileSize;

  return DF_OPERATION_OK;
}

SPROX_API_FUNC(Desfire_ExplainValueFileSettings) (const BYTE additionnal_settings_array[], LONG *lLowerLimit, LONG *lUpperLimit, DWORD *eLimitedCredit, BYTE *bLimitedCreditEnabled)
{
  DF_ADDITIONAL_FILE_SETTINGS *p = (DF_ADDITIONAL_FILE_SETTINGS *) additionnal_settings_array;

  if (p == NULL)
    return DFCARD_LIB_CALL_ERROR;

  if (lLowerLimit != NULL)
    *lLowerLimit = p->stValueFileSettings.lLowerLimit;
  if (lUpperLimit != NULL)
    *lUpperLimit = p->stValueFileSettings.lUpperLimit;
  if (eLimitedCredit != NULL)
    *eLimitedCredit = p->stValueFileSettings.eLimitedCredit;
  if (bLimitedCreditEnabled != NULL)
    *bLimitedCreditEnabled = p->stValueFileSettings.bLimitedCreditEnabled;

  return DF_OPERATION_OK;
}

SPROX_API_FUNC(Desfire_ExplainRecordFileSettings) (const BYTE additionnal_settings_array[], DWORD *eRecordSize, DWORD *eMaxNRecords, DWORD *eCurrNRecords)
{
  DF_ADDITIONAL_FILE_SETTINGS *p = (DF_ADDITIONAL_FILE_SETTINGS *) additionnal_settings_array;

  if (p == NULL)
    return DFCARD_LIB_CALL_ERROR;

  if (eRecordSize != NULL)
    *eRecordSize = p->stRecordFileSettings.eRecordSize;
  if (eMaxNRecords != NULL)
    *eMaxNRecords = p->stRecordFileSettings.eMaxNRecords;
  if (eCurrNRecords != NULL)
    *eCurrNRecords = p->stRecordFileSettings.eCurrNRecords;

  return DF_OPERATION_OK;
}
