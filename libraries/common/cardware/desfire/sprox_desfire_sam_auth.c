#ifdef SPROX_DESFIRE_WITH_SAM

/**h* DesfireAPI/SamAuth
 *
 * NAME
 *   DesfireAPI :: Using the Desfire card together with a NXP SAM AV2 smartcard
 *
 * COPYRIGHT
 *   (c) 2014 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of authentication using the SAM.
 *
 **/
#include "sprox_desfire_i.h"

/**f* DesfireAPI/SAM_SelectApplication
 *
 * NAME
 *   SAM_SelectApplication
 *
 * DESCRIPTION
 *   Selects one specific application within the SAM
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_SelectApplication(SCARDHANDLE hCard,
 *                                            DWORD aid);
 *
 * INPUTS
 *   DWORD aid                   : Application IDentifier
 *
 * RETURNS
 *   DF_OPERATION_OK    : application selected
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   SelectApplication
 *   SAM_AuthenticateEx
 *   SAM_Authenticate
 *
 **/
SPROX_API_FUNC(Desfire_SAM_SelectApplication) (SPROX_PARAM  DWORD aid)
{
  return 0;

}

/**f* DesfireAPI/SAM_AuthenticateEx
 *
 * NAME
 *   SAM_AuthenticateEx
 *
 * DESCRIPTION
 *   Perform authentication over the Desfire card or application, using the SAM.
 *   The underlying SAM command is SAM_AuthenticatePICC (80 0A ...)
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_AuthenticateEx(SCARDHANDLE hCard,
 *                                         BYTE bAuthMethod,
 *                                         BYTE bKeyNumberCard,
 *                                         BYTE bSamParamP1,
 *                                         BYTE bSamParamP2,
 *                                         BYTE bKeyNumberSam,
 *                                         BYTE bKeyVersion,
 *                                         const BYTE pbDivInp[],
 *                                         BYTE bDivInpLength);
 *
 * INPUTS
 *   BYTE bAuthMethod            : authentication method. Correct values are 0A (Authenticate)
 *                                 1A (Authenticate ISO) and AA (Authenticate AES)
 *   BYTE bKeyNumberCard         : number of the key (KeyNo) within the Desfire card.
 *   BYTE bSamParamP1            : P1 parameter to the SAM_AuthenticatePICC command.
 *   BYTE bSamParamP2            : P2 parameter to the SAM_AuthenticatePICC command.
 *   BYTE bKeyNumberSam          : number of the key (KeyNo) within the SAM.
 *                                 this could be either the Desfire Key Number within the application
 *                                 if SAM_SelectApplication has been issued before, or the absolute
 *                                 Key Entry number of no SAM_SelectApplication has been issued before.
 *   BYTE bKeyVersion            : version of the key (within the SAM).
 *   const BYTE pbDivInp[]       : optionnal diversification input. Set to NULL for no diversification.
 *   BYTE bDivInpLength          : length of the diversification input (must be 0 if pbDivInp is NULL).
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   P1 xxxxxxxx
 *             +-- 0 : no diversification
 *                 1 : key diversified by pbDivInp
 *            +--- 0 : key selection by Key Entry number
 *                 1 : key selection by Desfire Key Number (after SAM_SelectApplication)
 *           +---- RFU, must be 0
 *          +----- 3DES2K : 0 : diversify using two encryption rounds
 *                          1 : diversify using one encryption rounds
 *                 3DES3K, AES : RFU, must be 0
 *         +------ 0 : use AV1 method for key diversification
 *                 1 : use AV2 method for key diversification
 *      +++------- RFU, must be 000
 *
 *   P2 xxxxxxxx
 *      ++++++++-- RFU, must be 00000000
 *
 *   Valid values for bDivInpLength are :
 *   - 8 when AV1 method is used with a DES, 3DES2K or 3DES3K key
 *   - 16 when AV1 method is used with an AES key
 *   - any length between 1 and 31 when AV2 method is used
 *
 *   Please refer to the documentation of NXP SAM AV2 for more details (P5DF081 data sheet, § 11.7.1)
 *
 * SEE ALSO
 *   SAM_SelectApplication
 *   SAM_Authenticate
 *   SAM_AuthenticateIso
 *   SAM_AuthenticateAes
 *   SAM_ChangeKeyEx
 *   SAM_ChangeKey1
 *   SAM_ChangeKey2
 *
 **/
SPROX_API_FUNC(Desfire_SAM_AuthenticateEx) (SPROX_PARAM  BYTE bAuthMethod, BYTE bKeyNumberCard, BYTE bSamParamP1, BYTE bSamParamP2, BYTE bKeyNumberSam, BYTE bKeyVersion, const BYTE pbDivInp[], BYTE bDivInpLength)
{
  SPROX_RC status;
  DWORD i=0, offset=0;
  BYTE capdu[DF_MAX_INFO_FRAME_SIZE], rapdu[DF_MAX_INFO_FRAME_SIZE];
  DWORD capdu_len = 0;
	DWORD rapdu_len = DF_MAX_INFO_FRAME_SIZE;
  SCARDHANDLE hSam;

  SPROX_DESFIRE_GET_CTX();

  hSam=ctx->sam_context.hSam;

  /* Each new Authenticate must invalidate the current authentication status. */
  Desfire_CleanupAuthentication(SPROX_PARAM_PV);


  /* 1. Ask Ek(RndB) to Desfire */
  /* -------------------------- */

  ctx->xfer_buffer[INF + 0] = bAuthMethod;
  ctx->xfer_buffer[INF + 1] = bKeyNumberCard;
  ctx->xfer_length = 2;

#ifdef _DEBUG_SAM_AUTH
  printf("AUTH-Card Capdu 1=");
  for (i=0; i<ctx->xfer_length; i++)
    printf("%2.2x", ctx->xfer_buffer[INF + i]);
  printf("\n");
#endif


  /* Send the command string to the PICC and get its response (1st frame exchange).
     The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_ADDITIONAL_FRAME);
  if (status != DF_OPERATION_OK)
    return status;

#ifdef _DEBUG_SAM_AUTH
  printf("AUTH-Card Rapdu 1=");
  for (i=0; i<ctx->xfer_length; i++)
    printf("%2.2x", ctx->xfer_buffer[INF+i]);
  printf("\n");
#endif


  /* Check the number of bytes received */
  if (bAuthMethod == DF_AUTHENTICATE)
  {
    if (ctx->xfer_length != 9)
    {
      /* Error: block with inappropriate number of bytes received from the card. */
      return DFCARD_WRONG_LENGTH;
    }

  } else
  if (bAuthMethod == DF_AUTHENTICATE_AES)
  {
    if (ctx->xfer_length != 17)
    {
      /* Error: block with inappropriate number of bytes received from the card. */
      return DFCARD_WRONG_LENGTH;
    }
  } else
  {
    if ((ctx->xfer_length != 9) && (ctx->xfer_length != 17))
    {
      /* Error: block with inappropriate number of bytes received from the card. */
      return DFCARD_WRONG_LENGTH;
    }
  }



  /* 2. Send it to the SAM */
  /* --------------------- */
  capdu_len = 0;
  capdu[capdu_len++] = 0x80;
  capdu[capdu_len++] = 0x0A;
  capdu[capdu_len++] = bSamParamP1;
  capdu[capdu_len++] = bSamParamP2;
  capdu[capdu_len++] = (BYTE) ((ctx->xfer_length -1) + 2 + bDivInpLength);
  capdu[capdu_len++] = bKeyNumberSam;
  capdu[capdu_len++] = bKeyVersion;
  for (i=1; i< ctx->xfer_length; i++)
    capdu[capdu_len++] = ctx->xfer_buffer[INF+i];
  for (i=0; i< bDivInpLength ; i++)
    capdu[capdu_len++] = pbDivInp[i];
  capdu[capdu_len++] = 0x00;

#ifdef _DEBUG_SAM_AUTH
  printf("AUTH-SAM Capdu 1=");
  for (i = 0; i< capdu_len; i++)
    printf("%2.2x", capdu[i]);
  printf("\n");
#endif

  rapdu_len = DF_MAX_INFO_FRAME_SIZE;


  status = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
  if (status != SCARD_S_SUCCESS)
    return status;

#ifdef _DEBUG_SAM_AUTH
  printf("AUTH-SAM Rapdu 1=");
  for (i = 0; i< rapdu_len; i++)
    printf("%2.2x", rapdu[i]);
  printf("\n");
#endif

  if ((rapdu[rapdu_len-2] != 0x90) || (rapdu[rapdu_len - 1] != 0xAF))
    return SCARD_W_CARD_NOT_AUTHENTICATED;


  /* 3. Send SAM answer to DESFire */
  /* ----------------------------- */
  ctx->xfer_buffer[INF + 0] = DF_ADDITIONAL_FRAME;
  offset=INF+1;
  for (i=0; i< rapdu_len - 2; i++)
    ctx->xfer_buffer[offset++] = rapdu[i];
  ctx->xfer_length = rapdu_len - 2 + 1;

#ifdef _DEBUG_SAM_AUTH
  printf("AUTH-Card Capdu 2=");
  for (i = 0; i< ctx->xfer_length; i++)
    printf("%2.2x", ctx->xfer_buffer[INF + i]);
  printf("\n");
#endif


  /* Send the 2nd frame to the PICC and get its response. */
  SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_OPERATION_OK);

#ifdef _DEBUG_SAM_AUTH
  printf("AUTH-Card Rapdu 2=");
  for (i = 0; i< ctx->xfer_length; i++)
    printf("%2.2x", ctx->xfer_buffer[i]);
  printf("\n");
#endif



  /* 4. Send DESFire answer to SAM */
  /* ----------------------------- */
  capdu_len = 0;
  capdu[capdu_len++] = 0x80;
  capdu[capdu_len++] = 0x0A;
  capdu[capdu_len++] = 0x00;
  capdu[capdu_len++] = 0x00;
  capdu[capdu_len++] = (BYTE) (ctx->xfer_length -1);
  for (i=1; i< ctx->xfer_length; i++)
    capdu[capdu_len++] = ctx->xfer_buffer[i];

#ifdef _DEBUG_SAM_AUTH
  printf("AUTH-SAM Capdu 2=");
  for (i = 0; i< capdu_len; i++)
    printf("%2.2x", capdu[i]);
  printf("\n");
#endif

  rapdu_len = DF_MAX_INFO_FRAME_SIZE;
  status = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
  if (status != SCARD_S_SUCCESS)
    return status;

#ifdef _DEBUG_SAM_AUTH
  printf("AUTH-SAM Rapdu 2=");
  for (i = 0; i< rapdu_len; i++)
    printf("%2.2x", rapdu[i]);
  printf("\n");
#endif

  if ((rapdu[rapdu_len-2] != 0x90) || (rapdu[rapdu_len - 1] != 0x00))
    return SCARD_W_CARD_NOT_AUTHENTICATED;

  ctx->sam_session_active = TRUE;

  switch (bAuthMethod)
  {
    case DF_AUTHENTICATE_AES :
      ctx->session_type = KEY_ISO_AES;
      break;

    case DF_AUTHENTICATE_ISO :
      ctx->session_type = KEY_ISO_MODE;
      break;

    default                  :
      ctx->session_type = KEY_LEGACY_3DES;
      break;
  }

  return SCARD_S_SUCCESS;

}


/**f* DesfireAPI/SAM_Authenticate
 *
 * NAME
 *   SAM_Authenticate
 *
 * DESCRIPTION
 *   Perform DES/3DES authentication over the Desfire card or application, using the SAM.
 *   The underlying SAM command is SAM_AuthenticatePICC (80 0A ...).
 *   This function is an helper to compute the valid parameters to SAM_AuthenticateEx
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_Authenticate(SCARDHANDLE hCard,
 *                                       BYTE bKeyNumberCard,
 *                                       BOOL fApplicationKeyNo,
 *                                       BYTE bKeyNumberSam,
 *                                       BYTE bKeyVersion,
 *                                       const BYTE pbDivInp[],
 *                                       BYTE bDivInpLength,
 *                                       BOOL fDivAv2Mode,
 *                                       BOOL fDivTwoRounds);
 *
 * INPUTS
 *   BYTE bKeyNumberCard         : number of the key (KeyNo) within the Desfire card.
 *   BOOL fApplicationKeyNo      : set to TRUE if SAM_SelectApplication has been issued before and
 *                                 bKeyNumberSam is the Desfire Key Number within the currently selected
 *                                 application, or FALSE if bKeyNumberSam is the absolute Key Entry number.
 *   BYTE bKeyNumberSam          : number of the key (KeyNo) within the SAM.
 *                                 This could be either the Desfire Key Number when fApplicationKeyNo is
 *                                 TRUE, or the absolute Key Entry number when fApplicationKeyNo is FALSE.
 *   BYTE bKeyVersion            : version of the key (within the SAM).
 *   const BYTE pbDivInp[]       : optionnal diversification input. Set to NULL for no diversification.
 *   BYTE bDivInpLength          : length of the diversification input (must be 0 if pbDivInp is NULL).
 *   BOOL fDivAv2Mode            : set to TRUE to use AV2 method for key diversification (FALSE stands
 *                                 for AV1 method).
 *   BOOL fDivTwoRounds          : set to TRUE to use two encryption rounds instead of one if key type
 *                                 is 3DES2K. Must be FALSE for any other key type.
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   SAM_SelectApplication
 *   SAM_AuthenticateIso
 *   SAM_AuthenticateAes
 *   SAM_AuthenticateEx
 *   SAM_ChangeKeyEx
 *   SAM_ChangeKey1
 *   SAM_ChangeKey2
 *
 **/
SPROX_API_FUNC(Desfire_SAM_Authenticate) (SPROX_PARAM  BYTE bKeyNumberCard, BOOL fApplicationKeyNo, BYTE bKeyNumberSam, BYTE bKeyVersion, const BYTE pbDivInp[], BYTE bDivInpLength, BOOL fDivAv2Mode, BOOL fDivTwoRounds)
{

  BYTE bSamParamP1 = 0;
  BYTE bSamParamP2 = 0;

  if ((pbDivInp != NULL) && (bDivInpLength > 0))
    bSamParamP1 |= 0x01;

  if (fApplicationKeyNo)
    bSamParamP1 |= 0x02;

  if (fDivTwoRounds)
    bSamParamP1 |= 0x08;

  if (fDivAv2Mode)
    bSamParamP1 |= 0x10;

  return SPROX_API_CALL(Desfire_SAM_AuthenticateEx) (SPROX_PARAM_P DF_AUTHENTICATE,
                                                    bKeyNumberCard,
                                                    bSamParamP1,
                                                    bSamParamP2,
                                                    bKeyNumberSam,
                                                    bKeyVersion,
                                                    pbDivInp,
                                                    bDivInpLength);
}

/**f* DesfireAPI/SAM_AuthenticateIso
 *
 * NAME
 *   SAM_AuthenticateIso
 *
 * DESCRIPTION
 *   Perform Iso authentication over the Desfire card or application, using the SAM.
 *   The underlying SAM command is SAM_AuthenticatePICC (80 0A ...).
 *   This function is an helper to compute the valid parameters to SAM_AuthenticateEx
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_AuthenticateIso(SCARDHANDLE hCard,
 *                                       BYTE bKeyNumberCard,
 *                                       BOOL fApplicationKeyNo,
 *                                       BYTE bKeyNumberSam,
 *                                       BYTE bKeyVersion,
 *                                       const BYTE pbDivInp[],
 *                                       BYTE bDivInpLength,
 *                                       BOOL fDivAv2Mode,
 *                                       BOOL fDivTwoRounds);
 *
 * INPUTS
 *   BYTE bKeyNumberCard         : number of the key (KeyNo) within the Desfire card.
 *   BOOL fApplicationKeyNo      : set to TRUE if SAM_SelectApplication has been issued before and
 *                                 bKeyNumberSam is the Desfire Key Number within the currently selected
 *                                 application, or FALSE if bKeyNumberSam is the absolute Key Entry number.
 *   BYTE bKeyNumberSam          : number of the key (KeyNo) within the SAM.
 *                                 This could be either the Desfire Key Number when fApplicationKeyNo is
 *                                 TRUE, or the absolute Key Entry number when fApplicationKeyNo is FALSE.
 *   BYTE bKeyVersion            : version of the key (within the SAM).
 *   const BYTE pbDivInp[]       : optionnal diversification input. Set to NULL for no diversification.
 *   BYTE bDivInpLength          : length of the diversification input (must be 0 if pbDivInp is NULL).
 *   BOOL fDivAv2Mode            : set to TRUE to use AV2 method for key diversification (FALSE stands
 *                                 for AV1 method).
 *   BOOL fDivTwoRounds          : set to TRUE to use two encryption rounds instead of one if key type
 *                                 is 3DES2K. Must be FALSE for any other key type.
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   SAM_SelectApplication
 *   SAM_Authenticate
 *   SAM_AuthenticateAes
 *   SAM_AuthenticateEx
 *   SAM_ChangeKeyEx
 *   SAM_ChangeKey1
 *   SAM_ChangeKey2
 *
 **/
SPROX_API_FUNC(Desfire_SAM_AuthenticateIso) (SPROX_PARAM  BYTE bKeyNumberCard, BOOL fApplicationKeyNo, BYTE bKeyNumberSam, BYTE bKeyVersion, const BYTE pbDivInp[], BYTE bDivInpLength, BOOL fDivAv2Mode, BOOL fDivTwoRounds)
{

  BYTE bSamParamP1 = 0;
  BYTE bSamParamP2 = 0;

  if ((pbDivInp != NULL) && (bDivInpLength > 0))
    bSamParamP1 |= 0x01;

  if (fApplicationKeyNo)
    bSamParamP1 |= 0x02;

  if (fDivTwoRounds)
    bSamParamP1 |= 0x08;

  if (fDivAv2Mode)
    bSamParamP1 |= 0x10;

  return SPROX_API_CALL(Desfire_SAM_AuthenticateEx) (SPROX_PARAM_P  DF_AUTHENTICATE_ISO,
                                                    bKeyNumberCard,
                                                    bSamParamP1,
                                                    bSamParamP2,
                                                    bKeyNumberSam,
                                                    bKeyVersion,
                                                    pbDivInp,
                                                    bDivInpLength);
}

/**f* DesfireAPI/SAM_AuthenticateAes
 *
 * NAME
 *   SAM_AuthenticateAes
 *
 * DESCRIPTION
 *   Perform AES authentication over the Desfire card or application, using the SAM.
 *   The underlying SAM command is SAM_AuthenticatePICC (80 0A ...).
 *   This function is an helper to compute the valid parameters to SAM_AuthenticateEx
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_AuthenticateAes(SCARDHANDLE hCard,
 *                                       BYTE bKeyNumberCard,
 *                                       BOOL fApplicationKeyNo,
 *                                       BYTE bKeyNumberSam,
 *                                       BYTE bKeyVersion,
 *                                       const BYTE pbDivInp[],
 *                                       BYTE bDivInpLength,
 *                                       BOOL fDivAv2Mode,
 *                                       BOOL fDivTwoRounds);
 *
 * INPUTS
 *   BYTE bKeyNumberCard         : number of the key (KeyNo) within the Desfire card.
 *   BOOL fApplicationKeyNo      : set to TRUE if SAM_SelectApplication has been issued before and
 *                                 bKeyNumberSam is the Desfire Key Number within the currently selected
 *                                 application, or FALSE if bKeyNumberSam is the absolute Key Entry number.
 *   BYTE bKeyNumberSam          : number of the key (KeyNo) within the SAM.
 *                                 This could be either the Desfire Key Number when fApplicationKeyNo is
 *                                 TRUE, or the absolute Key Entry number when fApplicationKeyNo is FALSE.
 *   BYTE bKeyVersion            : version of the key (within the SAM).
 *   const BYTE pbDivInp[]       : optionnal diversification input. Set to NULL for no diversification.
 *   BYTE bDivInpLength          : length of the diversification input (must be 0 if pbDivInp is NULL).
 *   BOOL fDivAv2Mode            : set to TRUE to use AV2 method for key diversification (FALSE stands
 *                                 for AV1 method).
 *   BOOL fDivTwoRounds          : set to TRUE to use two encryption rounds instead of one if key type
 *                                 is 3DES2K. Must be FALSE for any other key type.
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   SAM_SelectApplication
 *   SAM_Authenticate
 *   SAM_AuthenticateIso
 *   SAM_AuthenticateEx
 *   SAM_ChangeKeyEx
 *   SAM_ChangeKey1
 *   SAM_ChangeKey2
 *
 **/
SPROX_API_FUNC(Desfire_SAM_AuthenticateAes) (SPROX_PARAM  BYTE bKeyNumberCard, BOOL fApplicationKeyNo, BYTE bKeyNumberSam, BYTE bKeyVersion, const BYTE pbDivInp[], BYTE bDivInpLength, BOOL fDivAv2Mode, BOOL fDivTwoRounds)
{

  BYTE bSamParamP1 = 0;
  BYTE bSamParamP2 = 0;

  if ((pbDivInp != NULL) && (bDivInpLength > 0))
    bSamParamP1 |= 0x01;

  if (fApplicationKeyNo)
    bSamParamP1 |= 0x02;

  if (fDivTwoRounds)
    bSamParamP1 |= 0x08;

  if (fDivAv2Mode)
    bSamParamP1 |= 0x10;

  return SPROX_API_CALL(Desfire_SAM_AuthenticateEx) (SPROX_PARAM_P  DF_AUTHENTICATE_AES,
                                                    bKeyNumberCard,
                                                    bSamParamP1,
                                                    bSamParamP2,
                                                    bKeyNumberSam,
                                                    bKeyVersion,
                                                    pbDivInp,
                                                    bDivInpLength);
}

#endif
