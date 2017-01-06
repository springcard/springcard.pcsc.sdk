#ifndef __SPROX_DESFIRE_OLD_H__
#define __SPROX_DESFIRE_OLD_H__

#ifdef __cplusplus
extern  "C"
{
#endif

/*
 * DesFire exported functions
 * --------------------------
 */
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Authenticate(BYTE bKeyNumber,
                                                                   const BYTE * pbAccessKey);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ChangeKeySettings(BYTE key_settings);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetKeySettings(BYTE * key_settings,
                                                                     BYTE * key_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ChangeKey(BYTE key_number,
                                                                const BYTE * new_key,
                                                                const BYTE * old_key);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetKeyVersion(BYTE bKeyNumber,
                                                                    BYTE * pbKeyVersion);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_FormatPICC(void);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateApplication(DWORD aid,
                                                                        BYTE key_settings,
                                                                        BYTE keys_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_DeleteApplication(DWORD aid);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetApplicationIDs(BYTE aid_max_count,
                                                                        DWORD * aid_list,
                                                                        BYTE * aid_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_SelectApplication(DWORD aid);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetVersion(DF_VERSION_INFO * pVersionInfo);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetFileIDs(BYTE fid_max_count,
                                                                 BYTE * fid_list,
                                                                 BYTE * fid_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetFileSettings(BYTE file_id,
                                                                      BYTE * file_type,
                                                                      BYTE * comm_mode,
                                                                      WORD * access_rights,
                                                                      DF_ADDITIONAL_FILE_SETTINGS * additionnal_settings);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ChangeFileSettings(BYTE file_id,
                                                                         BYTE comm_mode,
                                                                         WORD access_rights);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateStdDataFile(BYTE file_id,
                                                                        BYTE comm_mode,
                                                                        WORD access_rights,
                                                                        DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateBackupDataFile(BYTE file_id,
                                                                           BYTE comm_mode,
                                                                           WORD access_rights,
                                                                           DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateValueFile(BYTE file_id,
                                                                      BYTE comm_mode,
                                                                      WORD access_rights,
                                                                      LONG lower_limit,
                                                                      LONG upper_limit,
                                                                      LONG initial_value,
                                                                      BYTE limited_credit_enabled);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateLinearRecordFile(BYTE file_id,
                                                                             BYTE comm_mode,
                                                                             WORD access_rights,
                                                                             DWORD record_size,
                                                                             DWORD max_records);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CreateCyclicRecordFile(BYTE file_id,
                                                                             BYTE comm_mode,
                                                                             WORD access_rights,
                                                                             DWORD record_size,
                                                                             DWORD max_records);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_DeleteFile(BYTE file_id);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ReadData(BYTE file_id,
                                                               BYTE comm_mode,
                                                               DWORD from_offset,
                                                               DWORD max_count,
                                                               BYTE * data,
                                                               DWORD * done_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ReadData2(BYTE file_id,
                                                                DWORD from_offset,
                                                                DWORD max_count,
                                                                BYTE * data,
                                                                DWORD * done_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_WriteData(BYTE file_id,
                                                                BYTE comm_mode,
                                                                DWORD from_offset,
                                                                DWORD size,
                                                                BYTE * data);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_WriteData2(BYTE file_id,
                                                                 DWORD from_offset,
                                                                 DWORD size,
                                                                 BYTE * data);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetValue(BYTE file_id,
                                                               BYTE comm_mode,
                                                               LONG * value);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_GetValue2(BYTE file_id,
                                                                LONG * value);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ReadRecords(BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  DWORD from_record,
                                                                  DWORD max_record_count,
                                                                  DWORD record_size,
                                                                  BYTE * data,
                                                                  DWORD * record_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ReadRecords2(BYTE file_id,
                                                                   DWORD from_record,
                                                                   DWORD max_record_count,
                                                                   DWORD record_size,
                                                                   BYTE * data,
                                                                   DWORD * record_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_WriteRecord(BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  DWORD from_offset,
                                                                  DWORD size,
                                                                  BYTE * data);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_WriteRecord2(BYTE file_id,
                                                                   DWORD from_offset,
                                                                   DWORD size,
                                                                   BYTE * data);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_LimitedCredit(BYTE file_id,
                                                                    BYTE comm_mode,
                                                                    LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_LimitedCredit2(BYTE file_id,
                                                                     LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Credit(BYTE file_id,
                                                             BYTE comm_mode,
                                                             LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Credit2(BYTE file_id,
                                                              LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Debit(BYTE file_id,
                                                            BYTE comm_mode,
                                                            LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_Debit2(BYTE file_id,
                                                             LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ClearRecordFile(BYTE file_id);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_CommitTransaction(void);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_AbortTransaction(void);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_SelectCid(BYTE cid);

/*
 * DesFire helpers
 * ---------------
 */
SPROX_DESFIRE_LIB const TCHAR *SPROX_DESFIRE_API DesFire_GetErrorMessage(SWORD status);


/*
 * New for dummy .NET languages
 * ----------------------------
 * Caller must provide sizeof(additionnal_settings_array) > 13 !!!
 */
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ExplainDataFileSettings(BYTE * additionnal_settings_array,
                                                                              DWORD * eFileSize);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ExplainValueFileSettings(BYTE * additionnal_settings_array,
                                                                               LONG * lLowerLimit,
                                                                               LONG * lUpperLimit,
                                                                               DWORD * eLimitedCredit,
                                                                               BYTE * bLimitedCreditEnabled);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API DesFireCard_ExplainRecordFileSettings(BYTE * additionnal_settings_array,
                                                                                DWORD * eRecordSize,
                                                                                DWORD * eMaxNRecords,
                                                                                DWORD * eCurrNRecords);

#ifdef __cplusplus
}
#endif


#endif
