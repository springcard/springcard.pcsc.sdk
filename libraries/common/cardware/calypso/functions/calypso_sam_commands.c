/**h* CalypsoAPI/Calypso_SAM_Commands.c
 *
 * NAME
 *   SpringCard Calypso API :: SAM command set
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release
 *
 **/
#include "../calypso_api_i.h"
#include "calypso_sam_commands_i.h"

#ifndef CALYPSO_NO_SAM
#ifdef SPROX_LIB_INSIDE
BOOL calypso_sam_dirty = FALSE;
#endif

#ifdef CALYPSO_TRACE
  #define RETURN(n)   if (rc) CalypsoTraceRC(TR_DEBUG|TR_TRACE|TR_SAM, n " err.", rc); return rc
#else
  #define RETURN(n)   return rc
#endif

CALYPSO_RC CalypsoSamSetSW(CALYPSO_CTX_ST *ctx, SIZE_T recv_len)
{
  if (recv_len < 2) return CALYPSO_ERR_RESPONSE_MISSING;

  ctx->Sam.SW   = ctx->Sam.Buffer[recv_len-2];
  ctx->Sam.SW <<= 8; 
  ctx->Sam.SW  |= ctx->Sam.Buffer[recv_len-1];

  CalypsoTraceValH(TR_DEBUG|TR_CARD, "SAM's SW=", ctx->Sam.SW, 4);
  return CALYPSO_SUCCESS;
}

CALYPSO_PROC CalypsoSamGetSW(CALYPSO_CTX_ST *ctx, BYTE sw[2])
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (sw == NULL)  return CALYPSO_ERR_INVALID_PARAM;

  sw[0] = (BYTE) ((ctx->Sam.SW >> 8) & 0x00FF);
  sw[1] = (BYTE) ( ctx->Sam.SW       & 0x00FF);

  return CALYPSO_SUCCESS;
}

CALYPSO_RC CalypsoSamGetResponse(CALYPSO_CTX_ST *ctx, SIZE_T *recv_len)
{
  CALYPSO_RC rc;
  BYTE apdu[5];
  SIZE_T old_recv_len;
  BYTE data_len = 0x00;
  BYTE cla;
  BOOL first_time = TRUE;

  if (ctx == NULL) return CALYPSO_ERR_INTERNAL_ERROR;
  if (recv_len == NULL) return CALYPSO_ERR_INTERNAL_ERROR;

  old_recv_len = *recv_len;

  cla = 0x00;

  if ((ctx->Sam.SW & 0xFF00) == 0x6100)
  {
    data_len = (BYTE) (ctx->Sam.SW & 0x00FF);
  }

again:

  apdu[0] = cla;
  apdu[1] = CALYPSO_INS_GET_RESPONSE;
  apdu[2] = 0x00;
  apdu[3] = 0x00;
  apdu[4] = data_len;

  rc = CalypsoSamTransmit(ctx, apdu, 5, ctx->Sam.Buffer, recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, *recv_len);
  if (rc) goto done;

  if ((ctx->Sam.SW & 0xFF00) == 0x6C00)
  {
    /* New Get Response with specified Le */
    data_len = (BYTE) (ctx->Sam.SW & 0x00FF);
    *recv_len = old_recv_len;
    goto again;
  }
  
  switch (ctx->Sam.SW)
  {
    case 0x9000 : break;
    case 0x6E00 : /* CLA invalid */
                  if (first_time && (cla != ctx->Sam.CLA))
                  {
                    first_time = FALSE;
                    cla = ctx->Sam.CLA;
                    *recv_len = old_recv_len;
                    goto again;
                  }
                  rc = CALYPSO_ERR_STATUS_WORD;
                  break;
    case 0x6983 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("GetResponse");
}


/**f* SpringProxINS/CalypsoSamSelectDiversifier
 *
 * NAME
 *   CalypsoSamSelectDiversifier
 *
 * DESCRIPTION
 *   Feed the SAM with the UID of the card currently being worked on
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx        : library context
 *   const BYTE     card_uid[8] : the UID of the card
 *
 * RETURNS
 *   CALYPSO_RC                 : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoSamSelectDiversifier(CALYPSO_CTX_ST *ctx, const BYTE card_uid[8])
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (card_uid == NULL) return CALYPSO_ERR_INVALID_PARAM;

again:
  CalypsoTraceStr(TR_TRACE|TR_SAM, "SelectDiversifier");
  CalypsoTraceHex(TR_TRACE|TR_SAM, "C'UID=", card_uid, 8);
  CalypsoTraceValH(TR_TRACE|TR_SAM, "CLA=", ctx->Sam.CLA, 2);

  send_len = 0;
  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x14;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 8;
  memcpy(&ctx->Sam.Buffer[send_len], card_uid, 8);
  send_len += 8;

  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Sam.SW == 0x9000)
  {
    if (recv_len > 2)
      rc = CALYPSO_ERR_RESPONSE_SIZE;
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_IS_LOCKED; break;
      case 0x6E00 : if (ctx->Sam.CLA != 0x94)
                    {
                      ctx->Sam.CLA = 0x94;
                      goto again;
                    }
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:
  RETURN("SelectDiversifier");
}

/**f* SpringProxINS/CalypsoSamGetChallenge
 *
 * NAME
 *   CalypsoSamGetChallenge
 *
 * DESCRIPTION
 *   Retrieve the challenge from the SAM, to be used in CalypsoCardOpenSecureSession1,
 *   CalypsoCardOpenSecureSession2 or CalypsoCardOpenSecureSession3
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx        : library context
 *   BYTE           sam_chal[4] : the UID of the card
 *
 * RETURNS
 *   CALYPSO_RC                 : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoSamGetChallenge(CALYPSO_CTX_ST *ctx, BYTE sam_chal[4])
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_TRACE|TR_SAM, "GetChallenge");

  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x84;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x04;

  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Sam.SW == 0x9000)
  {
    if (recv_len != 6)
    {
      rc = CALYPSO_ERR_RESPONSE_SIZE;
    } else
    {
      /* OK ! */
      CalypsoTraceHex(TR_TRACE|TR_SAM, "S'Chal=", ctx->Sam.Buffer, 4);

      if (sam_chal != NULL)
        memcpy(sam_chal, ctx->Sam.Buffer, 4);
    }
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_IS_LOCKED; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:
  RETURN("GetChallenge");
}

CALYPSO_PROC CalypsoSamDigestInitCompat(CALYPSO_CTX_ST *ctx, BYTE kno, const BYTE cardresp[], SIZE_T cardrespsize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (cardresp == NULL) return CALYPSO_ERR_INVALID_PARAM;
  if (cardrespsize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_INTERNAL_OVERFLOW;

  CalypsoTraceStr(TR_TRACE|TR_SAM, "DigestInitCompat");
  CalypsoTraceValH(TR_TRACE|TR_SAM, "KNO=", kno, 2);
  CalypsoTraceHex(TR_TRACE|TR_SAM, "C'Resp=", cardresp, cardrespsize);

  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x8A;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = kno;
  ctx->Sam.Buffer[send_len++] = (BYTE) (cardrespsize);
  memcpy(&ctx->Sam.Buffer[send_len], cardresp, cardrespsize);
  send_len += cardrespsize;
  
#ifdef SPROX_LIB_INSIDE
  if (calypso_sam_dirty)
  {
    CalypsoSamTransmitDirty(ctx, ctx->Sam.Buffer, send_len);  
    return CALYPSO_SUCCESS;
  }
#endif
  
  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Sam.SW == 0x9000)
  {
    if (recv_len != 2)
      rc = CALYPSO_ERR_RESPONSE_SIZE;
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6600 : rc = CALYPSO_SAM_KEY_NOT_USABLE; break;
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6A00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;
      case 0x6900 : rc = CALYPSO_SAM_COUNTER_LIMIT; break;
      case 0x6A83 : rc = CALYPSO_SAM_NO_SUCH_KEY; break;
      case 0x6985 : rc = CALYPSO_SAM_IS_LOCKED; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:  
  RETURN("DigestInitCompat");
}

/**f* SpringProxINS/CalypsoSamDigestInit
 *
 * NAME
 *   CalypsoSamDigestInit
 *
 * DESCRIPTION
 *   SAM-side counterpart of CalypsoCardOpenSecureSession1, CalypsoCardOpenSecureSession2
 *   or CalypsoCardOpenSecureSession3
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx         : library context
 *   BYTE           kif          : key identifier
 *   BYTE           kvc          : key version and counter
 *   const BYTE     cardresp[]   : card's answer to the open secure session command
 *                                 (including its challenge)
 *   SIZE_T     cardrespsize : length of cardresp
 *
 * RETURNS
 *   CALYPSO_RC                  : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoSamDigestInit(CALYPSO_CTX_ST *ctx, BYTE kif, BYTE kvc, const BYTE cardresp[], SIZE_T cardrespsize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (cardresp == NULL) return CALYPSO_ERR_INVALID_PARAM;
  if (cardrespsize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_INTERNAL_OVERFLOW;

  CalypsoTraceStr(TR_TRACE|TR_SAM, "DigestInit");
  CalypsoTraceValH(TR_TRACE|TR_SAM, "KIF=", kif, 2);
  CalypsoTraceValH(TR_TRACE|TR_SAM, "KVC=", kvc, 2);
  CalypsoTraceHex(TR_TRACE|TR_SAM, "C'Resp=", cardresp, cardrespsize);

  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x8A;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0xFF;
  ctx->Sam.Buffer[send_len++] = (BYTE) (2 + cardrespsize);
  ctx->Sam.Buffer[send_len++] = kif;
  ctx->Sam.Buffer[send_len++] = kvc;
  memcpy(&ctx->Sam.Buffer[send_len], cardresp, cardrespsize);
  send_len += cardrespsize;
  
#ifdef SPROX_LIB_INSIDE
  if (calypso_sam_dirty)
  {
    CalypsoSamTransmitDirty(ctx, ctx->Sam.Buffer, send_len);  
    return CALYPSO_SUCCESS;
  }
#endif
  
  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Sam.SW == 0x9000)
  {
    if (recv_len != 2)
      rc = CALYPSO_ERR_RESPONSE_SIZE;
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6600 : rc = CALYPSO_SAM_KEY_NOT_USABLE; break;
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6A00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;
      case 0x6900 : rc = CALYPSO_SAM_COUNTER_LIMIT; break;
      case 0x6A83 : rc = CALYPSO_SAM_NO_SUCH_KEY; break;
      case 0x6985 : rc = CALYPSO_SAM_IS_LOCKED; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:  
  RETURN("DigestInit");
}

/**f* SpringProxINS/CalypsoSamDigestUpdate
 *
 * NAME
 *   CalypsoSamDigestUpdate
 *
 * DESCRIPTION
 *   Forward a card's APDU (in/out) to the SAM
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx         : library context
 *   const BYTE     cardapdu[]   : APDU
 *   SIZE_T     cardapdusize : length of the APDU
 *
 * RETURNS
 *   CALYPSO_RC                  : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoSamDigestUpdate(CALYPSO_CTX_ST *ctx, const BYTE cardapdu[], SIZE_T cardapdusize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (cardapdu == NULL) return CALYPSO_ERR_INVALID_PARAM;
  if (cardapdusize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_INTERNAL_OVERFLOW;

  CalypsoTraceStr(TR_TRACE|TR_SAM, "DigestUpdate");
  CalypsoTraceHex(TR_TRACE|TR_SAM, "C'APDU=", cardapdu, cardapdusize);

  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x8C;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = (BYTE) cardapdusize;
  memcpy(&ctx->Sam.Buffer[send_len], cardapdu, cardapdusize);
  send_len += cardapdusize;
 
#ifdef SPROX_LIB_INSIDE
  if (calypso_sam_dirty)
  { 
    CalypsoSamTransmitDirty(ctx, ctx->Sam.Buffer, send_len);
    return CALYPSO_SUCCESS;
  }
#endif
  
  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;  
  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Sam.SW == 0x9000)
  {
    if (recv_len != 2)
      rc = CALYPSO_ERR_RESPONSE_SIZE;
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_NOT_IN_SESSION; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:
  RETURN("DigestUpdate");
}

/**f* SpringProxINS/CalypsoSamDigestClose
 *
 * NAME
 *   CalypsoSamDigestClose
 *
 * DESCRIPTION
 *   SAM-side counterpart of CalypsoCardCloseSecureSession
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx        : library context
 *   BYTE           sam_sign[4] : signature computed by the SAM
 *                                (to be forwarded to the card)
 *
 * RETURNS
 *   CALYPSO_RC                 : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoSamDigestClose(CALYPSO_CTX_ST *ctx, BYTE sam_sign[4])
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_TRACE|TR_SAM, "DigestClose");

  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x8E;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x04;

  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Sam.SW == 0x9000)
  {
    if (recv_len != 6)
    {
      rc = CALYPSO_ERR_RESPONSE_SIZE;
    } else
    {
      /* OK ! */
      CalypsoTraceHex(TR_TRACE|TR_SAM, "S'Sign=", ctx->Sam.Buffer, 4);

      if (sam_sign != NULL)
        memcpy(sam_sign, ctx->Sam.Buffer, 4);
    }
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_NOT_IN_SESSION; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:
  RETURN("DigestClose");
}

/**f* SpringProxINS/CalypsoSamDigestAuthenticate
 *
 * NAME
 *   CalypsoSamDigestAuthenticate
 *
 * DESCRIPTION
 *   Verify card's signature after CalypsoCardCloseSecureSession
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx         : library context
 *   const BYTE     card_sign[4] : signature returned by the card
 *
 * RETURNS
 *   CALYPSO_RC                  : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoSamDigestAuthenticate(CALYPSO_CTX_ST *ctx, const BYTE card_sign[4])
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (card_sign == NULL) return CALYPSO_ERR_INVALID_PARAM;

  CalypsoTraceStr(TR_TRACE|TR_SAM, "DigestAuthenticate");
  CalypsoTraceHex(TR_TRACE|TR_SAM, "C'Sign=", card_sign, 4);

  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x82;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x4;
  memcpy(&ctx->Sam.Buffer[send_len], card_sign, 4);
  send_len += 4;

  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Sam.SW == 0x9000)
  {
    if (recv_len != 2)
      rc = CALYPSO_ERR_RESPONSE_SIZE;
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_NOT_IN_SESSION; break;
      case 0x6988 : rc = CALYPSO_SAM_DENIED_CARD_SIGN; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:
  RETURN("DigestAuthenticate");
}

/**f* SpringProxINS/CalypsoSamGiveRandom 
 *
 * NAME
 *   CalypsoSamGiveRandom
 *
 * DESCRIPTION
 *   Feed the SAM with a challenge generated by the card
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx         : library context
 *   const BYTE     challenge[8] : the challenge returned by the card
 * 
 * RETURNS
 *   CALYPSO_RC                  : 0 or an error code
 *
 * SEE ALSO
 *   CalypsoCardGetChallenge
 *
 **/
CALYPSO_PROC CalypsoSamGiveRandom(CALYPSO_CTX_ST *ctx, const BYTE challenge[8])
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (challenge == NULL) return CALYPSO_ERR_INVALID_PARAM;

again:
  CalypsoTraceStr(TR_TRACE|TR_SAM, "GiveRandom");
  CalypsoTraceHex(TR_TRACE|TR_SAM, "Chal=", challenge, 8);
  CalypsoTraceValH(TR_TRACE|TR_SAM, "CLA=", ctx->Sam.CLA, 2);

  send_len = 0;
  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x86;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 0x00;
  ctx->Sam.Buffer[send_len++] = 8;
  memcpy(&ctx->Sam.Buffer[send_len], challenge, 8);
  send_len += 8;

  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Sam.SW == 0x9000)
  {
    if (recv_len > 2)
      rc = CALYPSO_ERR_RESPONSE_SIZE;
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_IS_LOCKED; break;
      case 0x6E00 : if (ctx->Sam.CLA != 0x94)
                    {
                      ctx->Sam.CLA = 0x94;
                      goto again;
                    }
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:
  RETURN("GiveRandom");
}

/**f* SpringProxINS/CalypsoSamCipherCardDataEx 
 *
 * NAME
 *   CalypsoSamCipherCardDataEx
 *
 * DESCRIPTION
 *   Ask the SAM to cipher data to be transmitted to the card
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx         : library context
 *   BYTE           apdu_p1
 *   BYTE           apdu_p2
 *   const BYTE     plain[]
 *   SIZE_T     plainsize
 *   BYTE           cipher[]
 *   SIZE_T     *ciphersize
 * 
 * RETURNS
 *   CALYPSO_RC                  : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoSamCipherCardDataEx(P_CALYPSO_CTX ctx, BYTE apdu_p1, BYTE apdu_p2, const BYTE plain[], SIZE_T plainsize, BYTE cipher[], SIZE_T *ciphersize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (plain == NULL) return CALYPSO_ERR_INTERNAL_ERROR;
  if (plainsize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_INTERNAL_ERROR;

  CalypsoTraceStr(TR_TRACE|TR_SAM, "CipherCardData");
  CalypsoTraceValH(TR_TRACE|TR_SAM, "P1=", apdu_p1, 2);
  CalypsoTraceValH(TR_TRACE|TR_SAM, "P2=", apdu_p2, 2);
  CalypsoTraceHex(TR_TRACE|TR_SAM, "Data=", plain, plainsize);

  ctx->Sam.Buffer[send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[send_len++] = 0x12;
  ctx->Sam.Buffer[send_len++] = apdu_p1;
  ctx->Sam.Buffer[send_len++] = apdu_p2;
  ctx->Sam.Buffer[send_len++] = (BYTE) plainsize; // JDA
  memcpy(&ctx->Sam.Buffer[send_len], plain, plainsize);
  send_len += plainsize;

  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, send_len, ctx->Sam.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoSamSetSW(ctx, recv_len);
  if (rc) goto done;

  if ((recv_len == 2) && ((ctx->Sam.SW == 0x9000) || (ctx->Sam.SW & 0xFF00) == 0x6100))
  {
    /* Get Response */
    recv_len = sizeof(ctx->Sam.Buffer);
    rc = CalypsoSamGetResponse(ctx, &recv_len);
    if (rc) goto done;
  }

  if (ctx->Sam.SW == 0x9000)
  {
    /* OK ! */
    CalypsoTraceHex(TR_TRACE|TR_SAM, "Resp=", ctx->Sam.Buffer, recv_len-2);

    if (cipher != NULL)
      memcpy(cipher, ctx->Sam.Buffer, recv_len-2);

    if (ciphersize != NULL)
      *ciphersize = recv_len-2;
     
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_NOT_IN_SESSION; break;
      case 0x6988 : rc = CALYPSO_SAM_DENIED_CARD_SIGN; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:
  RETURN("CipherCardData");
}

#endif
