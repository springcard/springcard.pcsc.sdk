#ifndef __SPROX_DESFIRE_EX_H__
#define __SPROX_DESFIRE_EX_H__

#include "products/springprox/springprox_ex.h"
#include "sprox_desfire.h"

#ifdef __cplusplus
extern  "C"
{
#endif

/*
 * Library entry points
 * --------------------
 */

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_AttachDesfireLibrary(SPROX_INSTANCE rInst);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_DetachDesfireLibrary(SPROX_INSTANCE rInst);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_SelectCid(SPROX_INSTANCE rInst,
                                                                   BYTE cid);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_IsoWrapping(SPROX_INSTANCE rInst,
                                                                     BYTE mode);


/*
 * Library helpers
 * ---------------
 */

SPROX_DESFIRE_LIB const TCHAR *SPROX_DESFIRE_API SPROXx_Desfire_GetErrorMessage(SWORD status);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ExplainDataFileSettings(const BYTE additionnal_settings_array[],
                                                                                 DWORD *eFileSize);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ExplainValueFileSettings(const BYTE additionnal_settings_array[],
                                                                                  SDWORD *lLowerLimit,
                                                                                  SDWORD *lUpperLimit,
                                                                                  DWORD *eLimitedCredit,
                                                                                  BYTE *bLimitedCreditEnabled);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ExplainRecordFileSettings(const BYTE additionnal_settings_array[],
                                                                                   DWORD *eRecordSize,
                                                                                   DWORD *eMaxNRecords,
                                                                                   DWORD *eCurrNRecords);

/*
 * Desfire EV0 functions
 * ---------------------
 */
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_Authenticate(SPROX_INSTANCE rInst,
                                                                      BYTE bKeyNumber,
                                                                      const BYTE pbAccessKey[16]);
                                                                   
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ChangeKeySettings(SPROX_INSTANCE rInst,
                                                                           BYTE key_settings);
                                                                        
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetKeySettings(SPROX_INSTANCE rInst,
                                                                        BYTE *key_settings,
                                                                        BYTE *key_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ChangeKey(SPROX_INSTANCE rInst,
                                                                   BYTE key_number,
                                                                   const BYTE new_key[16],
                                                                   const BYTE old_key[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetKeyVersion(SPROX_INSTANCE rInst,
                                                                       BYTE bKeyNumber,
                                                                       BYTE *pbKeyVersion);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_FormatPICC(SPROX_INSTANCE rInst);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateApplication(SPROX_INSTANCE rInst,
                                                                           DWORD aid,
                                                                           BYTE key_settings,
                                                                           BYTE keys_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_DeleteApplication(SPROX_INSTANCE rInst,
                                                                           DWORD aid);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetApplicationIDs(SPROX_INSTANCE rInst,
                                                                           BYTE aid_max_count,
                                                                           DWORD aid_list[],
                                                                           BYTE *aid_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_SelectApplication(SPROX_INSTANCE rInst,
                                                                           DWORD aid);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetVersion(SPROX_INSTANCE rInst,
                                                                    DF_VERSION_INFO *pVersionInfo);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetFileIDs(SPROX_INSTANCE rInst,
                                                                    BYTE fid_max_count,
                                                                    BYTE fid_list[],
                                                                    BYTE *fid_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetFileSettings(SPROX_INSTANCE rInst,
                                                                         BYTE file_id,
                                                                         BYTE *file_type,
                                                                         BYTE *comm_mode,
                                                                         WORD *access_rights,
                                                                         DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ChangeFileSettings(SPROX_INSTANCE rInst,
                                                                            BYTE file_id,
                                                                            BYTE comm_mode,
                                                                            WORD access_rights);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateStdDataFile(SPROX_INSTANCE rInst,
                                                                           BYTE file_id,
                                                                           BYTE comm_mode,
                                                                           WORD access_rights,
                                                                           DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateBackupDataFile(SPROX_INSTANCE rInst,
                                                                              BYTE file_id,
                                                                              BYTE comm_mode,
                                                                              WORD access_rights,
                                                                              DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateValueFile(SPROX_INSTANCE rInst,
                                                                         BYTE file_id,
                                                                         BYTE comm_mode,
                                                                         WORD access_rights,
                                                                         SDWORD lower_limit,
                                                                         SDWORD upper_limit,
                                                                         SDWORD initial_value,
                                                                         BYTE limited_credit_enabled);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateLinearRecordFile(SPROX_INSTANCE rInst,
                                                                                BYTE file_id,
                                                                                BYTE comm_mode,
                                                                                WORD access_rights,
                                                                                DWORD record_size,
                                                                                DWORD max_records);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateCyclicRecordFile(SPROX_INSTANCE rInst,
                                                                                BYTE file_id,
                                                                                BYTE comm_mode,
                                                                                WORD access_rights,
                                                                                DWORD record_size,
                                                                                DWORD max_records);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_DeleteFile(SPROX_INSTANCE rInst,
                                                                    BYTE file_id);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ReadData(SPROX_INSTANCE rInst,
                                                                  BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  DWORD from_offset,
                                                                  DWORD max_count,
                                                                  BYTE data[],
                                                                  DWORD *done_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ReadData2(SPROX_INSTANCE rInst,
                                                                   BYTE file_id,
                                                                   DWORD from_offset,
                                                                   DWORD max_count,
                                                                   BYTE data[],
                                                                   DWORD *done_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_WriteData(SPROX_INSTANCE rInst,
                                                                   BYTE file_id,
                                                                   BYTE comm_mode,
                                                                   DWORD from_offset,
                                                                   DWORD size,
                                                                   const BYTE data[]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_WriteData2(SPROX_INSTANCE rInst,
                                                                    BYTE file_id,
                                                                    DWORD from_offset,
                                                                    DWORD size,
                                                                    const BYTE data[]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetValue(SPROX_INSTANCE rInst,
                                                                  BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  SDWORD *value);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetValue2(SPROX_INSTANCE rInst,
                                                                   BYTE file_id,
                                                                   SDWORD *value);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ReadRecords(SPROX_INSTANCE rInst,
                                                                     BYTE file_id,
                                                                     BYTE comm_mode,
                                                                     DWORD from_record,
                                                                     DWORD max_record_count,
                                                                     DWORD record_size,
                                                                     BYTE data[],
                                                                     DWORD *record_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ReadRecords2(SPROX_INSTANCE rInst,
                                                                      BYTE file_id,
                                                                      DWORD from_record,
                                                                      DWORD max_record_count,
                                                                      DWORD record_size,
                                                                      BYTE data[],
                                                                      DWORD *record_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_WriteRecord(SPROX_INSTANCE rInst,
                                                                     BYTE file_id,
                                                                     BYTE comm_mode,
                                                                     DWORD from_offset,
                                                                     DWORD size,
                                                                     const BYTE data[]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_WriteRecord2(SPROX_INSTANCE rInst,
                                                                      BYTE file_id,
                                                                      DWORD from_offset,
                                                                      DWORD size,
                                                                      const BYTE data[]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_LimitedCredit(SPROX_INSTANCE rInst,
                                                                       BYTE file_id,
                                                                       BYTE comm_mode,
                                                                       SDWORD amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_LimitedCredit2(SPROX_INSTANCE rInst,
                                                                        BYTE file_id,
                                                                        SDWORD amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_Credit(SPROX_INSTANCE rInst,
                                                                BYTE file_id,
                                                                BYTE comm_mode,
                                                                SDWORD amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_Credit2(SPROX_INSTANCE rInst,
                                                                 BYTE file_id,
                                                                 SDWORD amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_Debit(SPROX_INSTANCE rInst,
                                                               BYTE file_id,
                                                               BYTE comm_mode,
                                                               SDWORD amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_Debit2(SPROX_INSTANCE rInst,
                                                                BYTE file_id,
                                                                SDWORD amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ClearRecordFile(SPROX_INSTANCE rInst,
                                                                         BYTE file_id);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CommitTransaction(SPROX_INSTANCE rInst);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_AbortTransaction(SPROX_INSTANCE rInst);

/*
 * Desfire EV1 functions
 * ---------------------
 */

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_AuthenticateIso(SPROX_INSTANCE rInst,
                                                                         BYTE bKeyNumber,
                                                                         const BYTE pbAccessKey[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_AuthenticateIso24(SPROX_INSTANCE rInst,
                                                                           BYTE bKeyNumber,
                                                                           const BYTE pbAccessKey[24]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_AuthenticateAes(SPROX_INSTANCE rInst,
                                                                         BYTE bKeyNumber,
                                                                         const BYTE pbAccessKey[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ChangeKey24(SPROX_INSTANCE rInst,
                                                                     BYTE key_number,
                                                                     const BYTE new_key[24],
                                                                     const BYTE old_key[24]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_ChangeKeyAes(SPROX_INSTANCE rInst,
                                                                      BYTE key_number,
                                                                      BYTE key_version,
                                                                      const BYTE new_key[16],
                                                                      const BYTE old_key[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetFreeMemory(SPROX_INSTANCE rInst,
                                                                       DWORD *pdwFreeBytes);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_SetConfiguration(SPROX_INSTANCE rInst,
                                                                          BYTE option,
                                                                          const BYTE data[],
                                                                          BYTE length);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetCardUID(SPROX_INSTANCE rInst,
                                                                    BYTE uid[7]);

/*
 * Desfire ISO-related functions
 * -----------------------------
 */
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateIsoApplication(SPROX_INSTANCE rInst,
                                                                             DWORD       aid,
                                                                             BYTE        key_settings,
                                                                             BYTE        keys_count,
                                                                             WORD        iso_df_id,
                                                                             const BYTE  iso_df_name[16],
                                                                             BYTE        iso_df_namelen);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetIsoDFNames(SPROX_INSTANCE rInst,
                                                                       BOOL       *follow_up,
                                                                       DWORD      *aid,
                                                                       WORD       *iso_df_id,
                                                                       BYTE        iso_df_name[16+1],
                                                                       BYTE       *iso_df_namelen);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_GetIsoFileIDs(SPROX_INSTANCE rInst,
                                                                       BYTE fid_max_count,
                                                                       WORD fid_list[],
                                                                       BYTE *fid_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateIsoStdDataFile(SPROX_INSTANCE rInst,
                                                                              BYTE file_id,
                                                                              WORD iso_ef_id,
                                                                              BYTE comm_mode,
                                                                              WORD access_rights,
                                                                              DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateIsoBackupDataFile(SPROX_INSTANCE rInst,
                                                                                 BYTE file_id,
                                                                                 WORD iso_ef_id,
                                                                                 BYTE comm_mode,
                                                                                 WORD access_rights,
                                                                                 DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateIsoValueFile(SPROX_INSTANCE rInst,
                                                                            BYTE file_id,
                                                                            WORD iso_ef_id,
                                                                            BYTE comm_mode,
                                                                            WORD access_rights,
                                                                            SDWORD lower_limit,
                                                                            SDWORD upper_limit,
                                                                            SDWORD initial_value,
                                                                            BYTE limited_credit_enabled);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateIsoLinearRecordFile(SPROX_INSTANCE rInst,
                                                                                   BYTE file_id,
                                                                                   WORD iso_ef_id,
                                                                                   BYTE comm_mode,
                                                                                   WORD access_rights,
                                                                                   DWORD record_size,
                                                                                   DWORD max_records);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_Desfire_CreateIsoCyclicRecordFile(SPROX_INSTANCE rInst,
                                                                                   BYTE file_id,
                                                                                   WORD iso_ef_id,
                                                                                   BYTE comm_mode,
                                                                                   WORD access_rights,
                                                                                   DWORD record_size,
                                                                                   DWORD max_records);

#ifdef __cplusplus
}
#endif

#endif
