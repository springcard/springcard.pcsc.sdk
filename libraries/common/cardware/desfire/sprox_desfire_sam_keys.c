#ifdef SPROX_DESFIRE_WITH_SAM
/**h* DesfireAPI/SamKeys
 *
 * NAME
 *   DesfireAPI :: Using the Desfire card together with a NXP SAM AV2 smartcard
 *
 * COPYRIGHT
 *   (c) 2014 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of key management using the SAM.
 *
 **/
#include "sprox_desfire_i.h"

/**f* DesfireAPI/SAM_ChangeKeyEx
 *
 * NAME
 *   SAM_ChangeKeyEx
 *
 * DESCRIPTION
 *   Perform authentication over the Desfire card or application, using the SAM.
 *   The underlying SAM command is SAM_ChangeKeyPICC (80 C4 ...)
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_ChangeKeyEx(SCARDHANDLE hCard,
 *                                      BYTE bKeyNumberCard,
 *                                      BYTE bSamParamP1,
 *                                      BYTE bSamParamP2,
 *                                      BYTE bOldKeyNumberSam,
 *                                      BYTE bOldKeyVersion,
 *                                      BYTE bNewKeyNumberSam,
 *                                      BYTE bNewKeyVersion,
 *                                      const BYTE pbDivInp[],
 *                                      BYTE bDivInpLength);
 *
 * INPUTS
 *   BYTE bKeyNumberCard         : number of the key (KeyNo) within the Desfire card.
 *   BYTE bSamParamP1            : P1 parameter to the SAM_ChangeKeyPICC command.
 *   BYTE bSamParamP2            : P2 parameter to the SAM_ChangeKeyPICC command.
 *   BOOL fUseOldKey             : Flag indicating if current key is needed
 *   BYTE bOldKeyNumberSam       : Key Entry number of current key (within the SAM).
 *   BYTE bOldKeyVersion         : version of the current key (within the SAM).
 *   BYTE bNewKeyNumberSam       : Key Entry number of new key (within the SAM).
 *   BYTE bNewKeyVersion         : version of the new key (within the SAM).
 *   const BYTE pbDivInp[]       : optionnal diversification input. Set to NULL for no diversification.
 *   BYTE bDivInpLength          : length of the diversification input (must be 0 if pbDivInp is NULL).
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   P1 xxxxxxxx
 *             +-- 0 : the ChangeKey key of the targeting application is 00 to 0D, and the key to
 *                     be changed is not the master key.
 *                 1 : the ChangeKey key of the targeting application is 0E, or the master key
 *                     itself is changed. In this case only the new key is involved, parameters
 *                     bOldKeyNumberSam and bOldKeyVersion are ignored.
 *            +--- 0 : no diversification for new key
 *                 1 : new key diversified by pbDivInp
 *           +---- 0 : no diversification for current key
 *                 1 : current key diversified by pbDivInp
 *          +----- if new key is 3DES2K : 0 : diversify new key using two encryption rounds
 *                                        1 : diversify new key using one encryption rounds
 *                 if new key is 3DES3K, AES : RFU, must be 0
 *         +------ if current key is 3DES2K : 0 : diversify current key using two encryption rounds
 *                                            1 : diversify current key using one encryption rounds
 *                 if current key 3DES3K, AES : RFU, must be 0
 *        +------- 0 : use AV1 method for key diversification (both current and new)
 *                 1 : use AV2 method for key diversification (both current and new)
 *      ++-------- RFU, must be 00
 *
 *   P2 xxxxxxxx
 *           +++-- number of the key (KeyNo) within the Desfire card (must be the same as bKeyNumberCard)
 *          +----- 0 : for every key but the Desfire card master key
 *                 1 : to change the Desfire card master key
 *      ++++------ RFU, must be 0000
 *
 *   Valid values for bDivInpLength are :
 *   - 8 when AV1 method is used with a DES, 3DES2K or 3DES3K key
 *   - 16 when AV1 method is used with an AES key
 *   - any length between 1 and 31 when AV2 method is used
 *
 *   The command will not succeed if the two involved key entries are not of the same key type within
 *   the SAM.
 *
 *   Please refer to the documentation of NXP SAM AV2 for more details (P5DF081 data sheet, § 11.7.3)
 *
 * SEE ALSO
 *   SAM_SelectApplication
 *   SAM_Authenticate
 *   SAM_AuthenticateEx
 *   SAM_ChangeKey1
 *   SAM_ChangeKey2
 *
 **/
SPROX_API_FUNC(Desfire_SAM_ChangeKeyEx) (SPROX_PARAM  BYTE bKeyNumberCard, BYTE bSamParamP1,  BYTE bSamParamP2,
                                         BYTE bOldKeyNumberSam, BYTE bOldKeyVersion, BYTE bNewKeyNumberSam, BYTE bNewKeyVersion,
                                         const BYTE pbDivInp[], BYTE bDivInpLength)
{
  LONG rc;
  DWORD i, offset;
  BYTE capdu[DF_MAX_INFO_FRAME_SIZE], rapdu[DF_MAX_INFO_FRAME_SIZE];
  DWORD capdu_len = 0;
	DWORD rapdu_len = DF_MAX_INFO_FRAME_SIZE;
  SCARDHANDLE hSam;
  SPROX_DESFIRE_GET_CTX();

  hSam=ctx->sam_context.hSam;

  /* 1. Send SAM Change Key PICC command */
  /* ----------------------------------- */
  capdu[capdu_len++] = 0x80;
  capdu[capdu_len++] = 0xC4;
  capdu[capdu_len++] = bSamParamP1;
  capdu[capdu_len++] = bSamParamP2;
  capdu[capdu_len++] = 0x04 + bDivInpLength;
  capdu[capdu_len++] = bOldKeyNumberSam;
  capdu[capdu_len++] = bOldKeyVersion;
  capdu[capdu_len++] = bNewKeyNumberSam;
  capdu[capdu_len++] = bNewKeyVersion;
  for (i=0; i<bDivInpLength; i++)
    capdu[capdu_len ++ ] = pbDivInp[i];
  capdu[capdu_len++] = 0x00;

#ifdef _DEBUG_SAM_KEYS
  printf("KEYS-SAM Capdu 1=");
  for (i = 0; i< capdu_len; i++)
    printf("%2.2x", capdu[i]);
  printf("\n");
#endif

  rapdu_len = DF_MAX_INFO_FRAME_SIZE;

  rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
  if (rc != SCARD_S_SUCCESS)
    return rc;

#ifdef _DEBUG_SAM_KEYS
  printf("KEYS-SAM Rapdu 1=");
  for (i = 0; i< rapdu_len; i++)
    printf("%2.2x", rapdu[i]);
  printf("\n");
#endif

  if ((rapdu[rapdu_len-2] != 0x90) || (rapdu[rapdu_len - 1] != 0x00))
    return SCARD_W_CARD_NOT_AUTHENTICATED;

  /* Send SAM Answer to DESFire Card */
  /* ------------------------------- */
  ctx->xfer_buffer[INF + 0] = 0xC4;
  ctx->xfer_buffer[INF + 1] = bKeyNumberCard;
  offset=INF+2;
  for (i=0; i< rapdu_len - 2; i++)
    ctx->xfer_buffer[offset++] = rapdu[i];
  ctx->xfer_length = rapdu_len - 2 + 2;

#ifdef _DEBUG_SAM_KEYS
  printf("KEYS-Carte Capdu 1=");
  for (i = 0; i< ctx->xfer_length; i++)
    printf("%2.2x", ctx->xfer_buffer[INF + i]);
  printf("\n");
#endif

  /* Send the 2nd frame to the PICC and get its response. */
  rc = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_OPERATION_OK);
  if (rc != DF_OPERATION_OK)
    return rc;

#ifdef _DEBUG_SAM_KEYS
  printf("KEYS-Card Rapdu 2=");
  for (i = 0; i< ctx->xfer_length; i++)
    printf("%2.2x", ctx->xfer_buffer[i]);
  printf("\n");
#endif

  /* 3. Verify MAC, if present */
  /* ------------------------- */
  if (ctx->xfer_length == 1)
  {
   return DF_OPERATION_OK;
  } else
  {
    BOOL fKeepStatus = TRUE;
    return SAM_VerifyMAC(hSam, ctx->xfer_buffer[0], &ctx->xfer_buffer[1], ctx->xfer_length - 1, 8, fKeepStatus);
  }
}


/**f* DesfireAPI/SAM_ChangeKey1
 *
 * NAME
 *   SAM_ChangeKey1
 *
 * DESCRIPTION
 *   Perform authentication over the Desfire card or application, using the SAM.
 *   The underlying SAM command is SAM_ChangeKeyPICC (80 C4 ...)
 *   This function is an helper to compute the valid parameters to SAM_ChangeKeyEx when
 *   only the new key is involved (i.e. when the ChangeKey key of the targeting application
 *   is 0E, or the master key itself is changed).
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_ChangeKey1(SCARDHANDLE hCard,
 *                                     BYTE bKeyNumberCard,
 *                                     BOOL fIsCardMasterKey,
 *                                     BYTE bNewKeyNumberSam,
 *                                     BYTE bNewKeyVersion,
 *                                     const BYTE pbDivInp[],
 *                                     BYTE bDivInpLength,
 *                                     BOOL fDivAv2Mode,
 *                                     BOOL fNewDivEnable,
 *                                     BOOL fNewDivTwoRounds);
 *
 * INPUTS
 *   BYTE bKeyNumberCard         : number of the key (KeyNo) within the Desfire card.
 *   BOOL fIsCardMasterKey       : set to TRUE to change the Desfire card master key (key 00 in application
 *                                 000000).
 *   BYTE bNewKeyNumberSam       : Key Entry number of new key (within the SAM).
 *   BYTE bNewKeyVersion         : version of the new key (within the SAM).
 *   const BYTE pbDivInp[]       : optionnal diversification input. Set to NULL for no diversification.
 *   BYTE bDivInpLength          : length of the diversification input (must be 0 if pbDivInp is NULL).
 *   BOOL fDivAv2Mode            : set to TRUE to use AV2 method for key diversification (FALSE stands
 *                                 for AV1 method).
 *   BOOL fNewDivEnable          : set to TRUE to diversify the new key using pbDivInp.
 *   BOOL fNewDivTwoRounds       : set to TRUE to use two encryption rounds instead of one if key type
 *                                 for new key is 3DES2K. Must be FALSE for any other key type.
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   SAM_SelectApplication
 *   SAM_AuthenticateEx
 *   SAM_Authenticate
 *   SAM_ChangeKeyEx
 *   SAM_ChangeKey2
 *
 **/
SPROX_API_FUNC(Desfire_SAM_ChangeKey1) (SPROX_PARAM  BYTE bKeyNumberCard, BOOL fIsCardMasterKey, BYTE bNewKeyNumberSam, BYTE bNewKeyVersion, const BYTE pbDivInp[], BYTE bDivInpLength, BOOL fDivAv2Mode, BOOL fNewDivEnable, BOOL fNewDivTwoRounds)
{

  BYTE bSamParamP1 = 0x01;
  BYTE bSamParamP2 = (bKeyNumberCard & 0x0F);

  if (fIsCardMasterKey)
    bSamParamP2 |= 0x10;


  if (fNewDivEnable)
    bSamParamP1 |= 0x02;

  if (fNewDivTwoRounds)
    bSamParamP1 |= 0x08;

  if (fDivAv2Mode)
    bSamParamP1 |= 0x20;



  return SCardDesfire_SAM_ChangeKeyEx(hCard,
                                       bKeyNumberCard,
                                       bSamParamP1,
                                       bSamParamP2,
                                       0x00,  //won't be checked
                                       0x00,  //won't be checked
                                       bNewKeyNumberSam,
                                       bNewKeyVersion,
                                       pbDivInp,
                                       bDivInpLength);

}

 /**f* DesfireAPI/SAM_ChangeKey2
 *
 * NAME
 *   SAM_ChangeKey2
 *
 * DESCRIPTION
 *   Perform authentication over the Desfire card or application, using the SAM.
 *   The underlying SAM command is SAM_ChangeKeyPICC (80 C4 ...)
 *   This function is an helper to compute the valid parameters to SAM_ChangeKeyEx when
 *   both the new and the current keys are involved (i.e. when the ChangeKey key of the
 *   targeting application 00 to 0D, and the key to be changed is not the master key).
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_ChangeKey2(SCARDHANDLE hCard,
 *                                     BYTE bKeyNumberCard,
 *                                     BOOL fIsCardMasterKey,
 *                                     BYTE bOldKeyNumberSam,
 *                                     BYTE bOldKeyVersion,
 *                                     BYTE bNewKeyNumberSam,
 *                                     BYTE bNewKeyVersion,
 *                                     const BYTE pbDivInp[],
 *                                     BYTE bDivInpLength,
 *                                     BOOL fDivAv2Mode,
 *                                     BOOL fOldDivEnable,
 *                                     BOOL fOldDivTwoRounds,
 *                                     BOOL fNewDivEnable,
 *                                     BOOL fNewDivTwoRounds);
 *
 * INPUTS
 *   BYTE bKeyNumberCard         : number of the key (KeyNo) within the Desfire card.
 *   BYTE bOldKeyNumberSam       : Key Entry number of current key (within the SAM).
 *   BYTE bOldKeyVersion         : version of the current key (within the SAM).
 *   BYTE bNewKeyNumberSam       : Key Entry number of new key (within the SAM).
 *   BYTE bNewKeyVersion         : version of the new key (within the SAM).
 *   const BYTE pbDivInp[]       : optionnal diversification input. Set to NULL for no diversification.
 *   BYTE bDivInpLength          : length of the diversification input (must be 0 if pbDivInp is NULL).
 *   BOOL fDivAv2Mode            : set to TRUE to use AV2 method for key diversification (FALSE stands
 *                                 for AV1 method).
 *   BOOL fOldDivEnable          : set to TRUE to diversify the current key using pbDivInp.
 *   BOOL fOldDivTwoRounds       : set to TRUE to use two encryption rounds instead of one if key type
 *                                 for current key is 3DES2K. Must be FALSE for any other key type.
 *   BOOL fNewDivEnable          : set to TRUE to diversify the new key using pbDivInp.
 *   BOOL fNewDivTwoRounds       : set to TRUE to use two encryption rounds instead of one if key type
 *                                 for new key is 3DES2K. Must be FALSE for any other key type.
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   SAM_SelectApplication
 *   SAM_AuthenticateEx
 *   SAM_Authenticate
 *   SAM_ChangeKeyEx
 *   SAM_ChangeKey1
 *
 **/
SPROX_API_FUNC(Desfire_SAM_ChangeKey2) (SPROX_PARAM  BYTE bKeyNumberCard, BYTE bOldKeyNumberSam, BYTE bOldKeyVersion, BYTE bNewKeyNumberSam, BYTE bNewKeyVersion, const BYTE pbDivInp[], BYTE bDivInpLength, BOOL fDivAv2Mode, BOOL fOldDivEnable, BOOL fOldDivTwoRounds, BOOL fNewDivEnable, BOOL fNewDivTwoRounds)
{
  BYTE bSamParamP1 = 0x00;
  BYTE bSamParamP2 = bKeyNumberCard;

  if (fOldDivEnable)
    bSamParamP1 |= 0x04;

  if (fNewDivEnable)
    bSamParamP1 |= 0x02;

  if (fNewDivTwoRounds)
    bSamParamP1 |= 0x08;

  if (fOldDivTwoRounds)
    bSamParamP1 |= 0x10;

  if (fDivAv2Mode)
    bSamParamP1 |= 0x20;



  return SCardDesfire_SAM_ChangeKeyEx(hCard,
                                       bKeyNumberCard,
                                       bSamParamP1,
                                       bSamParamP2,
                                       bOldKeyNumberSam,
                                       bOldKeyVersion,
                                       bNewKeyNumberSam,
                                       bNewKeyVersion,
                                       pbDivInp,
                                       bDivInpLength);
}

#endif

