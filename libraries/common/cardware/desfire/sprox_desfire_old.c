#include "sprox_desfire_i.h"

#ifndef _USE_PCSC
#ifndef SPROX_API_REENTRANT

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_SelectCid(BYTE cid)
{
  return SPROX_Desfire_SelectCid(cid);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Authenticate(BYTE bKeyNumber,
                                                                   const BYTE * pbAccessKey)
{
  return SPROX_Desfire_Authenticate(bKeyNumber, pbAccessKey);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ChangeKeySettings(BYTE key_settings)
{
  return SPROX_Desfire_ChangeKeySettings(key_settings);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetKeySettings(BYTE * key_settings,
                                                                     BYTE * key_count)
{
  return SPROX_Desfire_GetKeySettings(key_settings, key_count);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ChangeKey(BYTE key_number,
                                                                const BYTE * new_key,
                                                                const BYTE * old_key)
{
  return SPROX_Desfire_ChangeKey(key_number, new_key, old_key);  
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetKeyVersion(BYTE bKeyNumber,
                                                                    BYTE * pbKeyVersion)
{
  return SPROX_Desfire_GetKeyVersion(bKeyNumber, pbKeyVersion);   
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_FormatPICC(void)
{
  return SPROX_Desfire_FormatPICC();
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateApplication(DWORD aid,
                                                                        BYTE key_settings,
                                                                        BYTE keys_count)
{
  return SPROX_Desfire_CreateApplication(aid, key_settings, keys_count);
}
                                                                        
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_DeleteApplication(DWORD aid)
{
  return SPROX_Desfire_DeleteApplication(aid);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetApplicationIDs(BYTE aid_max_count,
                                                                        DWORD * aid_list,
                                                                        BYTE * aid_count)
{
  return SPROX_Desfire_GetApplicationIDs(aid_max_count, aid_list, aid_count);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_SelectApplication(DWORD aid)
{
  return SPROX_Desfire_SelectApplication(aid);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetVersion(DF_VERSION_INFO * pVersionInfo)
{
  return SPROX_Desfire_GetVersion(pVersionInfo);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetFileIDs(BYTE fid_max_count,
                                                                 BYTE * fid_list,
                                                                 BYTE * fid_count)
{
  return SPROX_Desfire_GetFileIDs(fid_max_count, fid_list, fid_count);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetFileSettings(BYTE file_id,
                                                                      BYTE * file_type,
                                                                      BYTE * comm_mode,
                                                                      WORD * access_rights,
                                                                      DF_ADDITIONAL_FILE_SETTINGS * additionnal_settings)
{
  return SPROX_Desfire_GetFileSettings(file_id, file_type, comm_mode, access_rights, additionnal_settings);  
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ChangeFileSettings(BYTE file_id,
                                                                         BYTE comm_mode,
                                                                         WORD access_rights)
{
  return SPROX_Desfire_ChangeFileSettings(file_id, comm_mode, access_rights);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateStdDataFile(BYTE file_id,
                                                                        BYTE comm_mode,
                                                                        WORD access_rights,
                                                                        DWORD file_size)
{
  return SPROX_Desfire_CreateStdDataFile(file_id, comm_mode, access_rights, file_size);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateBackupDataFile(BYTE file_id,
                                                                           BYTE comm_mode,
                                                                           WORD access_rights,
                                                                           DWORD file_size)
{
  return SPROX_Desfire_CreateBackupDataFile(file_id, comm_mode, access_rights, file_size);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateValueFile(BYTE file_id,
                                                                      BYTE comm_mode,
                                                                      WORD access_rights,
                                                                      LONG lower_limit,
                                                                      LONG upper_limit,
                                                                      LONG initial_value,
                                                                      BYTE limited_credit_enabled)
{
  return SPROX_Desfire_CreateValueFile(file_id, comm_mode, access_rights, lower_limit, upper_limit, initial_value, limited_credit_enabled);  
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateLinearRecordFile(BYTE file_id,
                                                                             BYTE comm_mode,
                                                                             WORD access_rights,
                                                                             DWORD record_size,
                                                                             DWORD max_records)
{
  return SPROX_Desfire_CreateLinearRecordFile(file_id, comm_mode, access_rights, record_size, max_records);
}                                                                             

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateCyclicRecordFile(BYTE file_id,
                                                                             BYTE comm_mode,
                                                                             WORD access_rights,
                                                                             DWORD record_size,
                                                                             DWORD max_records)
{
  return SPROX_Desfire_CreateCyclicRecordFile(file_id, comm_mode, access_rights, record_size, max_records);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_DeleteFile(BYTE file_id)
{
  return SPROX_Desfire_DeleteFile(file_id);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ReadData(BYTE file_id,
                                                               BYTE comm_mode,
                                                               DWORD from_offset,
                                                               DWORD max_count,
                                                               BYTE * data,
                                                               DWORD * done_count)
{
  return SPROX_Desfire_ReadData(file_id, comm_mode, from_offset, max_count, data, done_count);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ReadData2(BYTE file_id,
                                                                DWORD from_offset,
                                                                DWORD max_count,
                                                                BYTE * data,
                                                                DWORD * done_count)
{
  return SPROX_Desfire_ReadData2(file_id, from_offset, max_count, data, done_count);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_WriteData(BYTE file_id,
                                                                BYTE comm_mode,
                                                                DWORD from_offset,
                                                                DWORD size,
                                                                BYTE * data)
{
  return SPROX_Desfire_WriteData(file_id, comm_mode, from_offset, size, data);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_WriteData2(BYTE file_id,
                                                                 DWORD from_offset,
                                                                 DWORD size,
                                                                 BYTE * data)
{
  return SPROX_Desfire_WriteData2(file_id, from_offset, size, data);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetValue(BYTE file_id,
                                                               BYTE comm_mode,
                                                               LONG * value)
{
  return SPROX_Desfire_GetValue(file_id, comm_mode, value);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetValue2(BYTE file_id,
                                                                LONG * value)
{
  return SPROX_Desfire_GetValue2(file_id, value);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ReadRecords(BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  DWORD from_record,
                                                                  DWORD max_record_count,
                                                                  DWORD record_size,
                                                                  BYTE * data,
                                                                  DWORD * record_count)
{
  return SPROX_Desfire_ReadRecords(file_id, comm_mode, from_record, max_record_count, record_size, data, record_count);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ReadRecords2(BYTE file_id,
                                                                   DWORD from_record,
                                                                   DWORD max_record_count,
                                                                   DWORD record_size,
                                                                   BYTE * data,
                                                                   DWORD * record_count)
{
  return SPROX_Desfire_ReadRecords2(file_id, from_record, max_record_count, record_size, data, record_count);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_WriteRecord(BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  DWORD from_offset,
                                                                  DWORD size,
                                                                  BYTE * data)
{
  return SPROX_Desfire_WriteRecord(file_id, comm_mode, from_offset, size, data);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_WriteRecord2(BYTE file_id,
                                                                   DWORD from_offset,
                                                                   DWORD size,
                                                                   BYTE * data)
{
  return SPROX_Desfire_WriteRecord2(file_id, from_offset, size, data);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_LimitedCredit(BYTE file_id,
                                                                    BYTE comm_mode,
                                                                    LONG amount)
{
  return SPROX_Desfire_LimitedCredit(file_id, comm_mode, amount);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_LimitedCredit2(BYTE file_id,
                                                                     LONG amount)
{
  return SPROX_Desfire_LimitedCredit2(file_id, amount);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Credit(BYTE file_id,
                                                             BYTE comm_mode,
                                                             LONG amount)
{
  return SPROX_Desfire_Credit(file_id, comm_mode, amount);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Credit2(BYTE file_id,
                                                              LONG amount)
{
  return SPROX_Desfire_Credit2(file_id, amount);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Debit(BYTE file_id,
                                                            BYTE comm_mode,
                                                            LONG amount)
{
  return SPROX_Desfire_Debit(file_id, comm_mode, amount);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Debit2(BYTE file_id,
                                                             LONG amount)
{
  return SPROX_Desfire_Debit2(file_id, amount);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ClearRecordFile(BYTE file_id)
{
  return SPROX_Desfire_ClearRecordFile(file_id);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CommitTransaction(void)
{
  return SPROX_Desfire_CommitTransaction();
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_AbortTransaction(void)
{
  return SPROX_Desfire_AbortTransaction();
}

SPROX_DESFIRE_LIB const TCHAR *SPROX_DESFIRE_API DesFire_GetErrorMessage(SWORD status)
{
  return SPROX_Desfire_GetErrorMessage(status);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ExplainDataFileSettings(BYTE * additionnal_settings_array,
                                                                              DWORD * eFileSize)
{
  return SPROX_Desfire_ExplainDataFileSettings(additionnal_settings_array, eFileSize);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ExplainValueFileSettings(BYTE * additionnal_settings_array,
                                                                               LONG * lLowerLimit,
                                                                               LONG * lUpperLimit,
                                                                               DWORD * eLimitedCredit,
                                                                               BYTE * bLimitedCreditEnabled)
{
  return SPROX_Desfire_ExplainValueFileSettings(additionnal_settings_array, lLowerLimit, lUpperLimit, eLimitedCredit, bLimitedCreditEnabled);
}

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ExplainRecordFileSettings(BYTE * additionnal_settings_array,
                                                                                DWORD * eRecordSize,
                                                                                DWORD * eMaxNRecords,
                                                                                DWORD * eCurrNRecords)
{
  return SPROX_Desfire_ExplainRecordFileSettings(additionnal_settings_array, eRecordSize, eMaxNRecords, eCurrNRecords);
}

#endif
#endif
