#ifndef __PCSC_DESFIRE_H__
#define __PCSC_DESFIRE_H__

#include <winscard.h>

#ifdef WIN32
  #ifndef SPROX_DESFIRE_LIB
    #define SPROX_DESFIRE_LIB __declspec( dllimport )
  #endif
	#ifndef SPROX_DESFIRE_API
	  #define SPROX_DESFIRE_API __cdecl
	#endif
#else
	#define SPROX_DESFIRE_LIB
	#define SPROX_DESFIRE_API
#endif

#ifdef __linux__
  typedef char TCHAR;
  #define T(a) a
#endif

#include "desfire_card.h"

#ifdef __cplusplus
extern  "C"
{
#endif

/*
 * Library entry points
 * --------------------
 */
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_AttachLibrary(SCARDHANDLE hCard);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_DetachLibrary(SCARDHANDLE hCard);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoWrapping(SCARDHANDLE hCard, BYTE mode);

SPROX_DESFIRE_LIB const TCHAR *SPROX_DESFIRE_API SCardDesfire_GetLibraryVersion(void);

/*
 * Library helpers
 * ---------------
 */

SPROX_DESFIRE_LIB const TCHAR *SPROX_DESFIRE_API SCardDesfire_GetErrorMessage(LONG status);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ExplainDataFileSettings(const BYTE additionnal_settings_array[],
                                                                              DWORD *eFileSize);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ExplainValueFileSettings(const BYTE additionnal_settings_array[],
                                                                               LONG  *lLowerLimit,
                                                                               LONG  *lUpperLimit,
                                                                               DWORD *eLimitedCredit,
                                                                               BYTE  *bLimitedCreditEnabled);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ExplainRecordFileSettings(const BYTE additionnal_settings_array[],
                                                                                DWORD *eRecordSize,
                                                                                DWORD *eMaxNRecords,
                                                                                DWORD *eCurrNRecords);

/*
 * Desfire EV0 functions
 * ---------------------
 */

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_Authenticate(SCARDHANDLE hCard,
                                                                   BYTE bKeyNumber,
                                                                   const BYTE pbAccessKey[16]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ChangeKeySettings(SCARDHANDLE hCard,
                                                                           BYTE key_settings);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetKeySettings(SCARDHANDLE hCard,
                                                                        BYTE *key_settings,
                                                                        BYTE *key_count);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ChangeKey(SCARDHANDLE hCard,
                                                                BYTE key_number,
                                                                const BYTE new_key[16],
                                                                const BYTE old_key[16]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetKeyVersion(SCARDHANDLE hCard,
                                                                       BYTE bKeyNumber,
                                                                       BYTE *pbKeyVersion);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_FormatPICC(SCARDHANDLE hCard);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateApplication(SCARDHANDLE hCard,
                                                                        DWORD aid,
                                                                        BYTE key_setting_1,
                                                                        BYTE key_setting_2);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_DeleteApplication(SCARDHANDLE hCard,
                                                                           DWORD aid);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetApplicationIDs(SCARDHANDLE hCard,
                                                                           BYTE aid_max_count,
                                                                           DWORD aid_list[],
                                                                           BYTE *aid_count);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SelectApplication(SCARDHANDLE hCard,
                                                                           DWORD aid);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetVersion(SCARDHANDLE hCard,
                                                                    DF_VERSION_INFO *pVersionInfo);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetFileIDs(SCARDHANDLE hCard,
                                                                    BYTE fid_max_count,
                                                                    BYTE fid_list[],
                                                                    BYTE *fid_count);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetFileSettings(SCARDHANDLE hCard,
                                                                         BYTE file_id,
                                                                         BYTE *file_type,
                                                                         BYTE *comm_mode,
                                                                         WORD *access_rights,
                                                                         DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ChangeFileSettings(SCARDHANDLE hCard,
                                                                            BYTE file_id,
                                                                            BYTE comm_mode,
                                                                            WORD access_rights);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateStdDataFile(SCARDHANDLE hCard,
                                                                           BYTE file_id,
                                                                           BYTE comm_mode,
                                                                           WORD access_rights,
                                                                           DWORD file_size);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateBackupDataFile(SCARDHANDLE hCard,
                                                                              BYTE file_id,
                                                                              BYTE comm_mode,
                                                                              WORD access_rights,
                                                                              DWORD file_size);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateValueFile(SCARDHANDLE hCard,
                                                                         BYTE file_id,
                                                                         BYTE comm_mode,
                                                                         WORD access_rights,
                                                                         LONG   lower_limit,
                                                                         LONG   upper_limit,
                                                                         LONG   initial_value,
                                                                         BYTE limited_credit_enabled);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateLinearRecordFile(SCARDHANDLE hCard,
                                                                                BYTE file_id,
                                                                                BYTE comm_mode,
                                                                                WORD access_rights,
                                                                                DWORD record_size,
                                                                                DWORD max_records);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateCyclicRecordFile(SCARDHANDLE hCard,
                                                                                BYTE file_id,
                                                                                BYTE comm_mode,
                                                                                WORD access_rights,
                                                                                DWORD record_size,
                                                                                DWORD max_records);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_DeleteFile(SCARDHANDLE hCard,
                                                                    BYTE file_id);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ReadData(SCARDHANDLE hCard,
                                                                  BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  DWORD from_offset,
                                                                  DWORD max_count,
                                                                  BYTE data[],
                                                                  DWORD *done_count);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ReadData2(SCARDHANDLE hCard,
                                                                   BYTE file_id,
                                                                   DWORD from_offset,
                                                                   DWORD max_count,
                                                                   BYTE data[],
                                                                   DWORD *done_count);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_WriteData(SCARDHANDLE hCard,
                                                                   BYTE file_id,
                                                                   BYTE comm_mode,
                                                                   DWORD from_offset,
                                                                   DWORD size,
                                                                   const BYTE data[]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_WriteData2(SCARDHANDLE hCard,
                                                                    BYTE file_id,
                                                                    DWORD from_offset,
                                                                    DWORD size,
                                                                    const BYTE data[]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetValue(SCARDHANDLE hCard,
                                                                  BYTE file_id,
                                                                  BYTE comm_mode,
                                                                  LONG   *value);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetValue2(SCARDHANDLE hCard,
                                                                   BYTE file_id,
                                                                   LONG   *value);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ReadRecords(SCARDHANDLE hCard,
                                                                     BYTE file_id,
                                                                     BYTE comm_mode,
                                                                     DWORD from_record,
                                                                     DWORD max_record_count,
                                                                     DWORD record_size,
                                                                     BYTE data[],
                                                                     DWORD *record_count);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ReadRecords2(SCARDHANDLE hCard,
                                                                      BYTE file_id,
                                                                      DWORD from_record,
                                                                      DWORD max_record_count,
                                                                      DWORD record_size,
                                                                      BYTE data[],
                                                                      DWORD *record_count);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_WriteRecord(SCARDHANDLE hCard,
                                                                     BYTE file_id,
                                                                     BYTE comm_mode,
                                                                     DWORD from_offset,
                                                                     DWORD size,
                                                                     const BYTE data[]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_WriteRecord2(SCARDHANDLE hCard,
                                                                      BYTE file_id,
                                                                      DWORD from_offset,
                                                                      DWORD size,
                                                                      const BYTE data[]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_LimitedCredit(SCARDHANDLE hCard,
                                                                       BYTE file_id,
                                                                       BYTE comm_mode,
                                                                       LONG   amount);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_LimitedCredit2(SCARDHANDLE hCard,
                                                                        BYTE file_id,
                                                                        LONG   amount);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_Credit(SCARDHANDLE hCard,
                                                                BYTE file_id,
                                                                BYTE comm_mode,
                                                                LONG   amount);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_Credit2(SCARDHANDLE hCard,
                                                                 BYTE file_id,
                                                                 LONG   amount);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_Debit(SCARDHANDLE hCard,
                                                               BYTE file_id,
                                                               BYTE comm_mode,
                                                               LONG   amount);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_Debit2(SCARDHANDLE hCard,
                                                                BYTE file_id,
                                                                LONG   amount);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ClearRecordFile(SCARDHANDLE hCard,
                                                                         BYTE file_id);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CommitTransaction(SCARDHANDLE hCard);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_AbortTransaction(SCARDHANDLE hCard);

/*
 * Desfire EV1 functions
 * ---------------------
 */

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_AuthenticateIso(SCARDHANDLE hCard,
                                                                      BYTE bKeyNumber,
                                                                        const BYTE pbAccessKey[16]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_AuthenticateIso24(SCARDHANDLE hCard,
                                                                        BYTE bKeyNumber,
                                                                        const BYTE pbAccessKey[24]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_AuthenticateAes(SCARDHANDLE hCard,
                                                                      BYTE bKeyNumber,
                                                                      const BYTE pbAccessKey[16]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ChangeKey24(SCARDHANDLE hCard,
                                                                  BYTE key_number,
                                                                  const BYTE new_key[24],
                                                                  const BYTE old_key[24]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_ChangeKeyAes(SCARDHANDLE hCard,
                                                                   BYTE key_number,
                                                                   BYTE key_version,
                                                                   const BYTE new_key[16],
                                                                   const BYTE old_key[16]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetFreeMemory(SCARDHANDLE hCard,
                                                                    DWORD *pdwFreeBytes);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SetConfiguration(SCARDHANDLE hCard,
                                                                       BYTE option,
                                                                       const BYTE data[],
                                                                       BYTE length);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetCardUID(SCARDHANDLE hCard,
                                                                 BYTE uid[7]);

/*
 * Desfire ISO-related functions
 * -----------------------------
 */
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateIsoApplication(SCARDHANDLE hCard,
                                                                           DWORD       aid,
                                                                           BYTE        key_settings,
                                                                           BYTE        keys_count,
                                                                           WORD        iso_df_id,
                                                                           const BYTE  iso_df_name[16],
                                                                           BYTE        iso_df_namelen);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetIsoApplications(SCARDHANDLE hCard,
                                                                         BYTE app_max_count,
                                                                         DF_ISO_APPLICATION_ST app_list[],
                                                                         BYTE *app_count);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_GetIsoFileIDs(SCARDHANDLE hCard,
                                                                    BYTE fid_max_count,
                                                                    WORD fid_list[],
                                                                    BYTE *fid_count);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateIsoStdDataFile(SCARDHANDLE hCard,
                                                                           BYTE file_id,
                                                                           WORD iso_ef_id,
                                                                           BYTE comm_mode,
                                                                           WORD access_rights,
                                                                           DWORD file_size);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateIsoBackupDataFile(SCARDHANDLE hCard,
                                                                              BYTE file_id,
                                                                              WORD iso_ef_id,
                                                                              BYTE comm_mode,
                                                                              WORD access_rights,
                                                                              DWORD file_size);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateIsoLinearRecordFile(SCARDHANDLE hCard,
                                                                                BYTE file_id,
                                                                                WORD iso_ef_id,
                                                                                BYTE comm_mode,
                                                                                WORD access_rights,
                                                                                DWORD record_size,
                                                                                DWORD max_records);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_CreateIsoCyclicRecordFile(SCARDHANDLE hCard,
                                                                                BYTE file_id,
                                                                                WORD iso_ef_id,
                                                                                BYTE comm_mode,
                                                                                WORD access_rights,
                                                                                DWORD record_size,
                                                                                DWORD max_records);
/*
 * Desfire ISO functions
 * ---------------------
 */
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoApdu(SCARDHANDLE hCard,
                                                              BYTE INS,
                                                              BYTE P1,
                                                              BYTE P2,
																															BYTE Lc,
																															const BYTE data_in[],
																															BYTE Le,
																															BYTE data_out[],
																															WORD *data_out_len,
																															WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoSelectApplet(SCARDHANDLE hCard,
                                                                      WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoSelectDF(SCARDHANDLE hCard,
                                                                  WORD fid,
                                                                  BYTE fci[],
																																	WORD fci_max_length,
																																	WORD *fci_length,
																																	WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoSelectDFName(SCARDHANDLE hCard,
                                                                      const BYTE df_name[],
                                                                      BYTE df_name_len,
                                                                      BYTE fci[],
																																		  WORD fci_max_length,
																																		  WORD *fci_length,
																																			WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoSelectEF(SCARDHANDLE hCard,
                                                                  WORD fid,
                                                                  WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoReadBinary(SCARDHANDLE hCard,
                                                                    WORD offset,
																																		BYTE data[],
                                                                    BYTE want_length,
																																		WORD *length,
																																		WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoUpdateBinary(SCARDHANDLE hCard,
                                                                      WORD offset,
																																			const BYTE data[],
                                                                      BYTE length,
																																			WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoReadRecord(SCARDHANDLE hCard,
                                                                    BYTE number,
                                                                    BOOL read_all,
																																		BYTE data[],
                                                                    WORD max_length,
																																		WORD *length,
																																		WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoAppendRecord(SCARDHANDLE hCard,
                                                                      const BYTE data[],
                                                                      BYTE length,
																																			WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoGetChallenge(SCARDHANDLE hCard,
                                                                      BYTE chal_size,
																																			BYTE card_chal_1[],
																																			WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoExternalAuthenticate(SCARDHANDLE hCard,
                                                                              BYTE key_algorithm,
                                                                              BYTE key_reference,
																																							BYTE chal_size,
																																				      const BYTE card_chal_1[],
																																							const BYTE host_chal_1[],
																																				      const BYTE key_value[],
																																			        WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoInternalAuthenticate(SCARDHANDLE hCard,
                                                                              BYTE key_algorithm,
                                                                              BYTE key_reference,
																																							BYTE chal_size,
																																				      const BYTE host_chal_2[],
																																							BYTE card_chal_2[],
																																				      const BYTE key_value[],
																																			        WORD *SW);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_IsoMutualAuthenticate(SCARDHANDLE hCard,
                                                                            BYTE key_algorithm,
                                                                            BYTE key_reference,
																																		        const BYTE key_value[],
																																			      WORD *SW);



SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SetSessionKey(SCARDHANDLE hCard,
                                                                    const BYTE pbSessionKey[16]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SetSessionKey24(SCARDHANDLE hCard,
                                                                      const BYTE pbSessionKey[24]);
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SetSessionKeyAes(SCARDHANDLE hCard,
                                                                       const BYTE pbSessionKey[16]);
#ifdef SPROX_DESFIRE_WITH_SAM
/*
 * Functions to use the Desfire card together with a NXP SAM AV2 smartcard
 * -----------------------------------------------------------------------
 */

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_AttachSAM(SCARDHANDLE hCard,
                                                                SCARDHANDLE hSam);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_DetachSAM(SCARDHANDLE hCard,
                                                                SCARDHANDLE hSam,
                                                                DWORD dwDisposition);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_Unlock(SCARDHANDLE hSam,
                                                                  BYTE bKeyNumberSam,
                                                                  BYTE bKeyVersion,
                                                                  const BYTE pbKeyValue[16]);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_SelectApplication(SCARDHANDLE hCard,
                                                                            DWORD aid);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_AuthenticateEx(SCARDHANDLE hCard,
                                                                         BYTE bAuthMethod,
                                                                         BYTE bKeyNumberCard,
                                                                         BYTE bSamParamP1,
                                                                         BYTE bSamParamP2,
                                                                         BYTE bKeyNumberSam,
                                                                         BYTE bKeyVersion,
                                                                         const BYTE pbDivInp[],
                                                                         BYTE bDivInpLength);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_Authenticate(SCARDHANDLE hCard,
                                                                       BYTE bKeyNumberCard,
                                                                       BOOL fApplicationKeyNo,
                                                                       BYTE bKeyNumberSam,
                                                                       BYTE bKeyVersion,
                                                                       const BYTE pbDivInp[],
                                                                       BYTE bDivInpLength,
                                                                       BOOL fDivAv2Mode,
                                                                       BOOL fDivTwoRounds);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_AuthenticateIso(SCARDHANDLE hCard,
                                                                       BYTE bKeyNumberCard,
                                                                       BOOL fApplicationKeyNo,
                                                                       BYTE bKeyNumberSam,
                                                                       BYTE bKeyVersion,
                                                                       const BYTE pbDivInp[],
                                                                       BYTE bDivInpLength,
                                                                       BOOL fDivAv2Mode,
                                                                       BOOL fDivTwoRounds);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_AuthenticateAes(SCARDHANDLE hCard,
                                                                       BYTE bKeyNumberCard,
                                                                       BOOL fApplicationKeyNo,
                                                                       BYTE bKeyNumberSam,
                                                                       BYTE bKeyVersion,
                                                                       const BYTE pbDivInp[],
                                                                       BYTE bDivInpLength,
                                                                       BOOL fDivAv2Mode,
                                                                       BOOL fDivTwoRounds);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_ChangeKeyEx(SCARDHANDLE hCard,
                                                                      BYTE bKeyNumberCard,
                                                                      BYTE bSamParamP1,
                                                                      BYTE bSamParamP2,
                                                                      BYTE bOldKeyNumberSam,
                                                                      BYTE bOldKeyVersion,
                                                                      BYTE bNewKeyNumberSam,
                                                                      BYTE bNewKeyVersion,
                                                                      const BYTE pbDivInp[],
                                                                      BYTE bDivInpLength);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_ChangeKey1(SCARDHANDLE hCard,
                                                                     BYTE bKeyNumberCard,
                                                                     BOOL fIsCardMasterKey,
                                                                     BYTE bNewKeyNumberSam,
                                                                     BYTE bNewKeyVersion,
                                                                     const BYTE pbDivInp[],
                                                                     BYTE bDivInpLength,
                                                                     BOOL fDivAv2Mode,
                                                                     BOOL fNewDivEnable,
                                                                     BOOL fNewDivTwoRounds);

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_SAM_ChangeKey2(SCARDHANDLE hCard,
                                                                     BYTE bKeyNumberCard,
                                                                     BYTE bOldKeyNumberSam,
                                                                     BYTE bOldKeyVersion,
                                                                     BYTE bNewKeyNumberSam,
                                                                     BYTE bNewKeyVersion,
                                                                     const BYTE pbDivInp[],
                                                                     BYTE bDivInpLength,
                                                                     BOOL fDivAv2Mode,
                                                                     BOOL fOldDivEnable,
                                                                     BOOL fOldDivTwoRounds,
                                                                     BOOL fNewDivEnable,
                                                                     BOOL fNewDivTwoRounds);
#endif

#ifdef __cplusplus
}
#endif


#endif
