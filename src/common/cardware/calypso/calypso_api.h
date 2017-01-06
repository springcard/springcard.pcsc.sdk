/**h* CalypsoAPI/Calypso_API.h
 *
 * NAME
 *   SpringCard Calypso API header
 *
 * DESCRIPTION
 *   A software library to work easily with Calypso e-ticketing cards
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * PORTABILITY
 *   Windows i86 (MSVC++ 6)
 *   Renesas H8S/2212 (GCC/KPIT)
 *
 **/
#ifndef __CALYPSO_API_H__
#define __CALYPSO_API_H__

#ifdef WIN32
  #ifndef CALYPSO_LIB
    #define CALYPSO_LIB __declspec( dllimport )
  #endif
  #ifndef UNDER_CE
    /* Under Desktop Windows we use the cdecl calling convention */
    #define CALYPSO_API __cdecl
  #else
  	/* Under Windows CE we use the stdcall calling convention */
    #define CALYPSO_API __stdcall
  #endif
#endif

#ifndef CALYPSO_LIB
  #define CALYPSO_LIB
#endif
#ifndef CALYPSO_API
  #define CALYPSO_API
#endif

#define CALYPSO_PROC CALYPSO_LIB CALYPSO_RC CALYPSO_API

#include "calypso_types.h"
#include "calypso_errors.h"
#include "calypso_card.h"

#ifndef P_CALYPSO_CTX
  #define P_CALYPSO_CTX   void *
#endif

CALYPSO_LIB DWORD CALYPSO_API CalypsoBench(BOOL reset);

CALYPSO_LIB void CALYPSO_API CalypsoGetVersion(char *version, CALYPSO_SZ versionsize);

CALYPSO_LIB void CALYPSO_API CalypsoSetTraceLevel(BYTE level);
CALYPSO_LIB void CALYPSO_API CalypsoSetTraceFile(const char *filename);

CALYPSO_PROC CalypsoGetLastError(P_CALYPSO_CTX ctx);


/* Context manipulation */
/* -------------------- */
CALYPSO_LIB P_CALYPSO_CTX CALYPSO_API CalypsoCreateContext(void);
CALYPSO_LIB void CALYPSO_API CalypsoDestroyContext(P_CALYPSO_CTX ctx);
CALYPSO_PROC CalypsoCleanupContext(P_CALYPSO_CTX ctx);

/* Access to the card and the SAM */
/* ------------------------------ */

#ifdef SPROX_API_REENTRANT
CALYPSO_PROC CalypsoAttachLegacyInstance(P_CALYPSO_CTX ctx, SPROX_INSTANCE rInst);
#endif
CALYPSO_PROC CalypsoCardBindLegacyEx(P_CALYPSO_CTX ctx, WORD proto, const BYTE info[], WORD infolen);
CALYPSO_PROC CalypsoSamBindLegacy(P_CALYPSO_CTX ctx, BYTE slot);

#if (defined(SCARD_S_SUCCESS) && !defined(UNDER_CE))
/* PC/SC enabled... */
CALYPSO_PROC CalypsoCardBindPcsc(P_CALYPSO_CTX ctx, SCARDHANDLE hCard);
CALYPSO_PROC CalypsoSamBindPcsc(P_CALYPSO_CTX ctx, SCARDHANDLE hCard);
#endif

CALYPSO_PROC CalypsoCardSelectInnovatron(P_CALYPSO_CTX ctx, const BYTE atr[], CALYPSO_SZ atrlen);
CALYPSO_PROC CalypsoCardSelectTcl(P_CALYPSO_CTX ctx, BYTE cid);
CALYPSO_PROC CalypsoCardTransmit(P_CALYPSO_CTX ctx, const BYTE in_buffer[], CALYPSO_SZ in_length, BYTE out_buffer[], CALYPSO_SZ *out_length);
CALYPSO_PROC CalypsoSamTransmit(P_CALYPSO_CTX ctx, const BYTE in_buffer[], CALYPSO_SZ in_length, BYTE out_buffer[], CALYPSO_SZ *out_length);
CALYPSO_PROC CalypsoSamTransmitDirty(P_CALYPSO_CTX ctx, const BYTE in_buffer[], CALYPSO_SZ in_length);


/* Card manipulation */
/* ----------------- */

CALYPSO_PROC CalypsoCardGetSW(P_CALYPSO_CTX ctx, BYTE sw[2]);

CALYPSO_PROC CalypsoCardGetAtr(P_CALYPSO_CTX ctx, BYTE atr[], CALYPSO_SZ *atrsize);
CALYPSO_PROC CalypsoCardSelectApplication(P_CALYPSO_CTX ctx, const BYTE aid[], CALYPSO_SZ aidsize, BYTE fci[], CALYPSO_SZ *fcisize);
CALYPSO_PROC CalypsoCardSelectDF(P_CALYPSO_CTX ctx, WORD file_id, BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardSelectEF(P_CALYPSO_CTX ctx, WORD file_id, BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardReadBinary(P_CALYPSO_CTX ctx, BYTE sfi, WORD offset, BYTE ask_size, BYTE data[], CALYPSO_SZ *datasize);
CALYPSO_PROC CalypsoCardUpdateBinary(P_CALYPSO_CTX ctx, BYTE sfi, WORD offset, const BYTE data[], BYTE length);
CALYPSO_PROC CalypsoCardReadRecord(P_CALYPSO_CTX ctx, BYTE sfi, BYTE rec_no, BYTE rec_size, BYTE data[], CALYPSO_SZ *datasize);
CALYPSO_PROC CalypsoCardAppendRecord(P_CALYPSO_CTX ctx, BYTE sfi, const BYTE data[], BYTE datasize);
CALYPSO_PROC CalypsoCardUpdateRecord(P_CALYPSO_CTX ctx, BYTE sfi, BYTE rec_no, const BYTE data[], BYTE datasize);
CALYPSO_PROC CalypsoCardWriteRecord(P_CALYPSO_CTX ctx, BYTE sfi, BYTE rec_no, const BYTE data[], BYTE datasize);
CALYPSO_PROC CalypsoCardIncrease(P_CALYPSO_CTX ctx, BYTE sfi, BYTE rec_no, DWORD incval, DWORD *newval);
CALYPSO_PROC CalypsoCardDecrease(P_CALYPSO_CTX ctx, BYTE sfi, BYTE rec_no, DWORD decval, DWORD *newval);
CALYPSO_PROC CalypsoCardInvalidate(P_CALYPSO_CTX ctx);
CALYPSO_PROC CalypsoCardRehabilitate(P_CALYPSO_CTX ctx);
CALYPSO_PROC CalypsoCardOpenSecureSessionEx(P_CALYPSO_CTX ctx, BYTE apdu_p1, BYTE apdu_p2, const BYTE sam_chal[4], BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardOpenSecureSession1(P_CALYPSO_CTX ctx, BYTE resp[], CALYPSO_SZ *respsize, BYTE key_no, BYTE sfi, BYTE rec_no, const BYTE sam_chal[4], BYTE card_chal[4], BOOL *ratified, BYTE data[], CALYPSO_SZ *datasize);
CALYPSO_PROC CalypsoCardOpenSecureSession2(P_CALYPSO_CTX ctx, BYTE resp[], CALYPSO_SZ *respsize, BYTE key_no, BYTE sfi, BYTE rec_no, const BYTE sam_chal[4], BYTE card_chal[4], BOOL *ratified, BYTE data[], CALYPSO_SZ *datasize, BYTE *kvc);
CALYPSO_PROC CalypsoCardOpenSecureSession3(P_CALYPSO_CTX ctx, BYTE resp[], CALYPSO_SZ *respsize, BYTE key_no, BYTE sfi, BYTE rec_no, const BYTE sam_chal[4], BYTE card_chal[4], BOOL *ratified, BYTE data[], CALYPSO_SZ *datasize, BYTE *kvc, BYTE *kif);
CALYPSO_PROC CalypsoCardCloseSecureSession(P_CALYPSO_CTX ctx, BOOL ratify_now, const BYTE sam_sign[4], BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardGetChallenge(P_CALYPSO_CTX ctx, BYTE card_chal[8]);
CALYPSO_PROC CalypsoCardSendRatificationFrame(P_CALYPSO_CTX ctx);
CALYPSO_PROC CalypsoCardVerifyPinPlainEx(P_CALYPSO_CTX ctx, const BYTE pin[4], BYTE *remaining);
CALYPSO_PROC CalypsoCardVerifyPinPlain(P_CALYPSO_CTX ctx, const BYTE pin[4]);
CALYPSO_PROC CalypsoCardVerifyPinCipherEx(P_CALYPSO_CTX ctx, const BYTE pin[4], BYTE *remaining);
CALYPSO_PROC CalypsoCardVerifyPinCipher(P_CALYPSO_CTX ctx, const BYTE pin[4]);
CALYPSO_PROC CalypsoCardChangePinPlain(P_CALYPSO_CTX ctx, const BYTE new_pin[4]);
CALYPSO_PROC CalypsoCardChangePinCipher(P_CALYPSO_CTX ctx, const BYTE new_pin[4]);

/* Card related information */
/* ------------------------ */

CALYPSO_PROC CalypsoCardActivate(P_CALYPSO_CTX ctx, const BYTE aid[], CALYPSO_SZ aidsize);
CALYPSO_PROC CalypsoCardDispose(P_CALYPSO_CTX ctx);

CALYPSO_PROC CalypsoCardSerialNumber(P_CALYPSO_CTX ctx, BYTE card_uid[8]);
CALYPSO_PROC CalypsoCardStartupInfo(P_CALYPSO_CTX ctx, BYTE info[7]);
CALYPSO_PROC CalypsoCardRevision(P_CALYPSO_CTX ctx);
CALYPSO_PROC CalypsoCardDFName(P_CALYPSO_CTX ctx, BYTE name[], CALYPSO_SZ *namesize);
CALYPSO_PROC CalypsoCardSessionModifs(P_CALYPSO_CTX ctx);
CALYPSO_PROC CalypsoCardSessionModifsMax(P_CALYPSO_CTX ctx);

/* SAM manipulation */
/* ---------------- */

CALYPSO_PROC CalypsoSamSelectDiversifier(P_CALYPSO_CTX ctx, const BYTE card_uid[8]);
CALYPSO_PROC CalypsoSamGetChallenge(P_CALYPSO_CTX ctx, BYTE sam_chal[4]);
CALYPSO_PROC CalypsoSamDigestInit(P_CALYPSO_CTX ctx, BYTE kif, BYTE kvc, const BYTE card_resp_buffer[], CALYPSO_SZ card_resp_length);
CALYPSO_PROC CalypsoSamDigestInitCompat(P_CALYPSO_CTX ctx, BYTE kno, const BYTE card_resp_buffer[], CALYPSO_SZ card_resp_length);
CALYPSO_PROC CalypsoSamDigestUpdate(P_CALYPSO_CTX ctx, const BYTE card_buffer[], CALYPSO_SZ card_buflen);
CALYPSO_PROC CalypsoSamDigestClose(P_CALYPSO_CTX ctx, BYTE sam_sign[4]);
CALYPSO_PROC CalypsoSamDigestAuthenticate(P_CALYPSO_CTX ctx, const BYTE card_sign[4]);
CALYPSO_PROC CalypsoSamDispose(P_CALYPSO_CTX ctx);
CALYPSO_PROC CalypsoSamGiveRandom(P_CALYPSO_CTX ctx, const BYTE chal[8]);
CALYPSO_PROC CalypsoSamCipherCardDataEx(P_CALYPSO_CTX ctx, BYTE apdu_p1, BYTE apdu_p2, const BYTE plain[], CALYPSO_SZ plainsize, BYTE cipher[], CALYPSO_SZ *ciphersize);

CALYPSO_PROC CalypsoSamSetAutoUpdate(P_CALYPSO_CTX ctx, BOOL enable);
CALYPSO_PROC CalypsoSamSetCommSpeed(P_CALYPSO_CTX ctx, BOOL fast);

CALYPSO_PROC CalypsoSamSerialNumber(P_CALYPSO_CTX ctx, BYTE sam_uid[4]);

/* All in one transaction management */
/* --------------------------------- */

CALYPSO_PROC CalypsoStartTransaction(P_CALYPSO_CTX ctx, BOOL *ratified, BYTE key_no);
CALYPSO_PROC CalypsoStartTransactionEx(P_CALYPSO_CTX ctx, BOOL *ratified, BYTE key_no, BYTE kif, BYTE sfi, BYTE rec_no, BYTE data[], CALYPSO_SZ *datasize);
CALYPSO_PROC CalypsoCommitTransaction(P_CALYPSO_CTX ctx, BOOL ratify_now);
CALYPSO_PROC CalypsoCancelTransaction(P_CALYPSO_CTX ctx);

/* Stored value */
/* ------------ */

CALYPSO_PROC CalypsoCardStoredValueGetForDebit(P_CALYPSO_CTX ctx, BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardStoredValueGetForReload(P_CALYPSO_CTX ctx, BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardStoredValueGetDecode(P_CALYPSO_CTX ctx, const BYTE get_sv_resp[], CALYPSO_SZ get_sv_respsize, signed long *balance, CALYPSO_STOREDVALUE_ST *target);
CALYPSO_PROC CalypsoCardStoredValueDebit(P_CALYPSO_CTX ctx, const BYTE get_sv_resp[], CALYPSO_SZ get_sv_respsize, signed long amount, WORD pdate, WORD ptime, BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardStoredValueUnDebit(P_CALYPSO_CTX ctx, const BYTE get_sv_resp[], CALYPSO_SZ get_sv_respsize, signed long amount, WORD pdate, WORD ptime, BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardStoredValueReload(P_CALYPSO_CTX ctx, const BYTE get_sv_resp[], CALYPSO_SZ get_sv_respsize, signed long amount, WORD pdate, WORD ptime, BYTE pfree[2], BYTE resp[], CALYPSO_SZ *respsize);
CALYPSO_PROC CalypsoCardStoredValueCheck(P_CALYPSO_CTX ctx, const BYTE op_sv_resp[], CALYPSO_SZ op_sv_respsize);

/* Calypso parser */
/* -------------- */

CALYPSO_PROC CalypsoParseCardAtr(P_CALYPSO_CTX ctx, const BYTE atr[], CALYPSO_SZ atrsize);
CALYPSO_PROC CalypsoParseSamAtr(P_CALYPSO_CTX ctx, const BYTE atr[], CALYPSO_SZ atrsize);
CALYPSO_PROC CalypsoParseFci(P_CALYPSO_CTX ctx, const BYTE fci[], CALYPSO_SZ fcisize);
CALYPSO_PROC CalypsoParseSelectResp(P_CALYPSO_CTX ctx, const BYTE resp[], CALYPSO_SZ respsize, FILE_INFO_ST *file_info);

/* Intercode decoder - Structure output */
/* ------------------------------------ */

CALYPSO_PROC CalypsoDecodeEnvAndHolderRecord(P_CALYPSO_CTX ctx, const BYTE data[], CALYPSO_SZ datasize, CALYPSO_ENVANDHOLDER_ST *target);
CALYPSO_PROC CalypsoDecodeContractRecord(P_CALYPSO_CTX ctx, const BYTE data[], CALYPSO_SZ datasize, CALYPSO_CONTRACT_ST *target);

/* Intercode encoder */
/* ----------------- */

CALYPSO_PROC CalypsoEncodeEventRecord(P_CALYPSO_CTX ctx, BYTE data[], BYTE size, CALYPSO_EVENT_ST *values);

/* Intercode decoder - XML (or INI) output */
/* --------------------------------------- */

CALYPSO_PROC CalypsoSetOutputOptions(P_CALYPSO_CTX ctx, BYTE options);
CALYPSO_PROC CalypsoSetXmlOutput(P_CALYPSO_CTX ctx, const char *filename);
CALYPSO_PROC CalypsoSetIniOutput(P_CALYPSO_CTX ctx, const char *filename);
CALYPSO_PROC CalypsoSetXmlOutputStr(P_CALYPSO_CTX ctx, char *target, CALYPSO_SZ length);
CALYPSO_PROC CalypsoSetIniOutputStr(P_CALYPSO_CTX ctx, char *target, CALYPSO_SZ length);
CALYPSO_PROC CalypsoClearOutput(P_CALYPSO_CTX ctx);

CALYPSO_PROC CalypsoOutputCardAtr(P_CALYPSO_CTX ctx, const BYTE atr[], CALYPSO_SZ atrsize);
CALYPSO_PROC CalypsoOutputCardInfo(P_CALYPSO_CTX ctx);
CALYPSO_PROC CalypsoOutputFileInfo(P_CALYPSO_CTX ctx, FILE_INFO_ST *file_info);

CALYPSO_PROC CalypsoOutputEnvironmentRecordEx(P_CALYPSO_CTX ctx, const BYTE data[], CALYPSO_SZ datasize);
CALYPSO_PROC CalypsoOutputHolderRecordEx(P_CALYPSO_CTX ctx, const BYTE data[], CALYPSO_SZ datasize);
CALYPSO_PROC CalypsoOutputEnvAndHolderRecordEx(P_CALYPSO_CTX ctx, const BYTE data[], CALYPSO_SZ datasize, BOOL flat);
CALYPSO_PROC CalypsoOutputContractRecordEx(P_CALYPSO_CTX ctx, const BYTE data[], CALYPSO_SZ datasize);
CALYPSO_PROC CalypsoOutputEventRecordEx(P_CALYPSO_CTX ctx, const BYTE data[], CALYPSO_SZ datasize);


CALYPSO_PROC CalypsoExploreAndParse(P_CALYPSO_CTX ctx);

CALYPSO_LIB BOOL CalypsoIsRecordEmpty(const BYTE data[], CALYPSO_SZ datasize);

CALYPSO_LIB void CALYPSO_API CalypsoSetTraceLevel(BYTE level);
CALYPSO_LIB void CALYPSO_API CalypsoSetTraceFile(const char *filename);



#endif
