#ifndef __SPROX_DESFIRE_H__
#define __SPROX_DESFIRE_H__

#include "products/springprox/springprox.h"
#include "desfire_card.h"

#ifdef WIN32
  #include <windows.h>
  #include <tchar.h>

  /* Shall we provide UNICODE functions ? */
  #ifdef UNICODE
    #define DesFire_GetErrorMessageW DesFire_GetErrorMessage
  #else
    #define DesFire_GetErrorMessageA DesFire_GetErrorMessage
  #endif

  #ifndef SPROX_DESFIRE_LIB
    #define SPROX_DESFIRE_LIB __declspec( dllimport )
  #endif

  #ifndef SPROX_DESFIRE_API
    #ifndef UNDER_CE
      /* Under Desktop Windows we use the cdecl calling convention */
      #define SPROX_DESFIRE_API __cdecl
    #else
      /* Under Windows CE we use the stdcall calling convention */
      #define SPROX_DESFIRE_API __stdcall
    #endif
  #endif
#else
  #define SPROX_DESFIRE_LIB
  #define SPROX_DESFIRE_API
#endif

#include "sprox_desfire_old.h"

#ifdef __cplusplus
extern  "C"
{
#endif

/*
 * Library entry points
 * --------------------
 */

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_AttachDesfireLibrary(void);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_DetachDesfireLibrary(void);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_SelectCid(BYTE cid);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoWrapping(BYTE mode);

/*
 * Library helpers
 * ---------------
 */

SPROX_DESFIRE_LIB const TCHAR *SPROX_DESFIRE_API SPROX_Desfire_GetErrorMessage(SWORD status);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ExplainDataFileSettings(const BYTE additionnal_settings_array[],
                                                                                DWORD *eFileSize);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ExplainValueFileSettings(const BYTE additionnal_settings_array[],
                                                                                 LONG *lLowerLimit,
                                                                                 LONG *lUpperLimit,
                                                                                 DWORD *eLimitedCredit,
                                                                                 BYTE *bLimitedCreditEnabled);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ExplainRecordFileSettings(const BYTE additionnal_settings_array[],
                                                                                  DWORD *eRecordSize,
                                                                                  DWORD *eMaxNRecords,
                                                                                  DWORD *eCurrNRecords);

/*
 * Desfire EV0 functions
 * ---------------------
 */

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_Authenticate(BYTE bKeyNumber,
                                                                     const BYTE pbAccessKey[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ChangeKeySettings(BYTE key_settings);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetKeySettings(BYTE *key_settings,
                                                                       BYTE *key_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ChangeKey(BYTE key_number,
                                                                  const BYTE new_key[16],
                                                                  const BYTE old_key[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetKeyVersion(BYTE bKeyNumber,
                                                                      BYTE *pbKeyVersion);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_FormatPICC(void);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateApplication(DWORD aid,
                                                                          BYTE key_settings,
                                                                          BYTE keys_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_DeleteApplication(DWORD aid);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetApplicationIDs(BYTE aid_max_count,
                                                                          DWORD aid_list[],
                                                                          BYTE *aid_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_SelectApplication(DWORD aid);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetVersion(DF_VERSION_INFO *pVersionInfo);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetFileIDs(BYTE fid_max_count,
                                                                   BYTE fid_list[],
                                                                   BYTE *fid_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetFileSettings(BYTE file_id,
                                                                        BYTE *file_type,
                                                                        BYTE *comm_mode,
                                                                        WORD *access_rights,
                                                                        DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ChangeFileSettings(BYTE file_id,
                                                                           BYTE comm_mode,
                                                                           WORD access_rights);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateStdDataFile(BYTE file_id,
                                                                          BYTE comm_mode,
                                                                          WORD access_rights,
                                                                          DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateBackupDataFile(BYTE file_id,
                                                                             BYTE comm_mode,
                                                                             WORD access_rights,
                                                                             DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateValueFile(BYTE file_id,
                                                                        BYTE comm_mode,
                                                                        WORD access_rights,
                                                                        LONG lower_limit,
                                                                        LONG upper_limit,
                                                                        LONG initial_value,
                                                                        BYTE limited_credit_enabled);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateLinearRecordFile(BYTE file_id,
                                                                               BYTE comm_mode,
                                                                               WORD access_rights,
                                                                               DWORD record_size,
                                                                               DWORD max_records);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateCyclicRecordFile(BYTE file_id,
                                                                               BYTE comm_mode,
                                                                               WORD access_rights,
                                                                               DWORD record_size,
                                                                               DWORD max_records);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_DeleteFile(BYTE file_id);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ReadData(BYTE file_id,
                                                                 BYTE comm_mode,
                                                                 DWORD from_offset,
                                                                 DWORD max_count,
                                                                 BYTE data[],
                                                                 DWORD *done_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ReadData2(BYTE file_id,
                                                                  DWORD from_offset,
                                                                  DWORD max_count,
                                                                  BYTE data[],
                                                                  DWORD *done_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_WriteData(BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  DWORD from_offset,
                                                                  DWORD size,
                                                                  const BYTE data[]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_WriteData2(BYTE file_id,
                                                                   DWORD from_offset,
                                                                   DWORD size,
                                                                   const BYTE data[]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetValue(BYTE file_id,
                                                                 BYTE comm_mode,
                                                                 LONG *value);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetValue2(BYTE file_id,
                                                                  LONG *value);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ReadRecords(BYTE file_id,
                                                                    BYTE comm_mode,
                                                                    DWORD from_record,
                                                                    DWORD max_record_count,
                                                                    DWORD record_size,
                                                                    BYTE data[],
                                                                    DWORD *record_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ReadRecords2(BYTE file_id,
                                                                     DWORD from_record,
                                                                     DWORD max_record_count,
                                                                     DWORD record_size,
                                                                     BYTE data[],
                                                                     DWORD *record_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_WriteRecord(BYTE file_id,
                                                                    BYTE comm_mode,
                                                                    DWORD from_offset,
                                                                    DWORD size,
                                                                    const BYTE data[]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_WriteRecord2(BYTE file_id,
                                                                     DWORD from_offset,
                                                                     DWORD size,
                                                                     const BYTE data[]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_LimitedCredit(BYTE file_id,
                                                                      BYTE comm_mode,
                                                                      LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_LimitedCredit2(BYTE file_id,
                                                                       LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_Credit(BYTE file_id,
                                                               BYTE comm_mode,
                                                               LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_Credit2(BYTE file_id,
                                                                LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_Debit(BYTE file_id,
                                                              BYTE comm_mode,
                                                              LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_Debit2(BYTE file_id,
                                                               LONG amount);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ClearRecordFile(BYTE file_id);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CommitTransaction(void);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_AbortTransaction(void);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_AbortTransaction(void);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_DES_SetKey(const BYTE key[8]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_DES_Encrypt(BYTE inoutbuf[8]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_DES_Decrypt(BYTE inoutbuf[8]);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_TDES_SetKey(const BYTE key1[8],
                                                                    const BYTE key2[8],
                                                                    const BYTE key3[8]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_TDES_Encrypt(BYTE inoutbuf[8]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_TDES_Decrypt(BYTE inoutbuf[8]);

/*
 * Desfire EV1 functions
 * ---------------------
 */

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_AuthenticateIso(BYTE bKeyNumber,
                                                                        const BYTE pbAccessKey[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_AuthenticateIso24(BYTE bKeyNumber,
                                                                          const BYTE pbAccessKey[24]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_AuthenticateAes(BYTE bKeyNumber,
                                                                        const BYTE pbAccessKey[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ChangeKey24(BYTE key_number,
                                                                    const BYTE new_key[24],
                                                                    const BYTE old_key[24]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_ChangeKeyAes(BYTE key_number,
                                                                     BYTE key_version,
                                                                     const BYTE new_key[16],
                                                                     const BYTE old_key[16]);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetFreeMemory(DWORD *pdwFreeBytes);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_SetConfiguration(BYTE option,
                                                                         const BYTE data[],
                                                                         BYTE length);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetCardUID(BYTE uid[7]);

/*
 * Desfire ISO-related functions
 * -----------------------------
 */
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateIsoApplication(DWORD       aid,
                                                                             BYTE        key_settings,
                                                                             BYTE        keys_count,
                                                                             WORD        iso_df_id,
                                                                             const BYTE  iso_df_name[16],
                                                                             BYTE        iso_df_namelen);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetIsoApplications(BYTE app_max_count,
                                                                           DF_ISO_APPLICATION_ST app_list[],
                                                                           BYTE *app_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_GetIsoFileIDs(BYTE fid_max_count,
                                                                      WORD fid_list[],
                                                                      BYTE *fid_count);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateIsoStdDataFile(BYTE file_id,
                                                                             WORD iso_ef_id,
                                                                             BYTE comm_mode,
                                                                             WORD access_rights,
                                                                             DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateIsoBackupDataFile(BYTE file_id,
                                                                                WORD iso_ef_id,
                                                                                BYTE comm_mode,
                                                                                WORD access_rights,
                                                                                DWORD file_size);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateIsoLinearRecordFile(BYTE file_id,
                                                                                  WORD iso_ef_id,
                                                                                  BYTE comm_mode,
                                                                                  WORD access_rights,
                                                                                  DWORD record_size,
                                                                                  DWORD max_records);
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_CreateIsoCyclicRecordFile(BYTE file_id,
                                                                                  WORD iso_ef_id,
                                                                                  BYTE comm_mode,
                                                                                  WORD access_rights,
                                                                                  DWORD record_size,
                                                                                  DWORD max_records);
/*
 * Desfire ISO functions
 * ---------------------
 */
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoApdu(BYTE INS,
                                                                BYTE P1,
                                                                BYTE P2,
																																BYTE Lc,
																																const BYTE data_in[],
																																BYTE Le,
																																BYTE data_out[],
																																WORD *data_out_len,
																																WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoSelectApplet(WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoSelectDF(WORD fid,
                                                                    BYTE fci[],
																																		WORD fci_max_length,
																																		WORD *fci_length,
																																		WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoSelectDFName(const BYTE df_name[],
                                                                        BYTE df_name_len,
                                                                        BYTE fci[],
																																		    WORD fci_max_length,
																																		    WORD *fci_length,
																																				WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoSelectEF(WORD fid,
                                                                    WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoReadBinary(WORD offset,
																																			BYTE data[],
                                                                      BYTE want_length,
																																			WORD *length,
																																			WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoUpdateBinary(WORD offset,
																																			  const BYTE data[],
                                                                        BYTE length,
																																			  WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoReadRecord(BYTE number,
                                                                      BOOL read_all,
																																			BYTE data[],
                                                                      WORD max_length,
																																			WORD *length,
																																			WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoAppendRecord(const BYTE data[],
                                                                        BYTE length,
																																			  WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoGetChallenge(BYTE chal_size,
																																				BYTE card_chal_1[],
																																			  WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoExternalAuthenticate(BYTE key_algorithm,
                                                                                BYTE key_reference,
																																								BYTE chal_size,
																																				        const BYTE card_chal_1[],
																																								const BYTE host_chal_1[],
																																				        const BYTE key_value[],
																																			          WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoInternalAuthenticate(BYTE key_algorithm,
                                                                                BYTE key_reference,
																																								BYTE chal_size,
																																				        const BYTE host_chal_2[],
																																								BYTE card_chal_2[],
																																				        const BYTE key_value[],
																																			          WORD *SW);

SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_Desfire_IsoMutualAuthenticate(BYTE key_algorithm,
                                                                              BYTE key_reference,
																																		          const BYTE key_value[],
																																			        WORD *SW);

#ifdef __cplusplus
}
#endif


#endif
