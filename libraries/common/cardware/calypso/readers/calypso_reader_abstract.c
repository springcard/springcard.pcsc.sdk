/**h* CalypsoAPI/calypso_reader_abstract.c
 *
 * NAME
 *   calypso_reader_abstract.c
 *
 * DESCRIPTION
 *   Reader abstraction layer
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

/**f* CSB6_Calypso/CalypsoCardTransmit
 *
 * NAME
 *   CalypsoCardTransmit
 *
 * DESCRIPTION
 *   This is an alias to SCardTransmit with the hCard set to the Calypso card handle as specified
 *   in CalypsoCardBindPcsc.
 *   This function is used by the library, do not call it directly from your application.
 *
 * SIDE EFFECTS
 *   When a session is active, the dialog between the application and the card is forwarded to the SAM
 *   in two calls to CalypsoSamDigestUpdate
 *
 **/
CALYPSO_PROC CalypsoCardTransmit(CALYPSO_CTX_ST *ctx, const BYTE send_buffer[], SIZE_T send_length, BYTE recv_buffer[], SIZE_T *recv_length)
{
  DWORD rc;
  BYTE send_buffer_copy[CALYPSO_MAX_APDU_SZ];

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (send_buffer == NULL) return CALYPSO_ERR_INVALID_PARAM;
  if (send_length > CALYPSO_MAX_APDU_SZ) return CALYPSO_ERR_INVALID_PARAM;

  memcpy(send_buffer_copy, send_buffer, send_length);

  /* Actual card transmit */

  switch (ctx->Card.Type)
  { 	
#ifdef _USE_PCSC
    case CALYPSO_TYPE_PCSC       : rc = CalypsoPcscTransmit(&ctx->Card.Pcsc, send_buffer, send_length, recv_buffer, recv_length);
                                   break;
#endif
#ifdef CALYPSO_LEGACY
    case CALYPSO_TYPE_LEGACY     : rc = CalypsoLegacyTransmit(&ctx->Card.Legacy, send_buffer, send_length, recv_buffer, recv_length);
                                   break;
#endif

    default                      : rc = CALYPSO_ERR_NOT_BINDED;
  }

  if (rc)
  {
    rc |= CALYPSO_ERR_CARD_;
    return rc;
  }

#if (CALYPSO_WITH_SAM)
  if (ctx->CardApplication.SessionActive)
  {
    /* Currently in transaction, forward the APDUs to the SAM */
    rc = CalypsoSamDigestUpdate(ctx, send_buffer_copy, send_length);
    if (rc) return rc;

    rc = CalypsoSamDigestUpdate(ctx, recv_buffer, *recv_length);
    if (rc) return rc;
  }
#endif

  return 0;
}

#if (CALYPSO_WITH_SAM)
/**f* CSB6_Calypso/CalypsoSamTransmit
 *
 * NAME
 *   CalypsoSamTransmit
 *
 * DESCRIPTION
 *   This is an alias to SCardTransmit with the hCard set to the Calypso SAM handle as specified
 *   in CalypsoSamBindPcsc.
 *   This function is used by the library, do not call it directly from your application.
 *
 **/
CALYPSO_PROC CalypsoSamTransmit(CALYPSO_CTX_ST *ctx, const BYTE send_buffer[], SIZE_T send_length, BYTE recv_buffer[], SIZE_T *recv_length)
{
  CALYPSO_RC rc;

  switch (ctx->Sam.Type)
  {
#ifdef _USE_PCSC
    case CALYPSO_TYPE_PCSC   : rc = CalypsoPcscTransmit(&ctx->Sam.Pcsc, send_buffer, send_length, recv_buffer, recv_length);
                               break;
#endif
#ifdef CALYPSO_LEGACY
    case CALYPSO_TYPE_LEGACY : rc = CalypsoLegacyTransmit(&ctx->Sam.Legacy, send_buffer, send_length, recv_buffer, recv_length);
                               break;
#endif
    default                  : rc = CALYPSO_ERR_NOT_BINDED;
  }

  if (rc)
  {
    rc |= CALYPSO_ERR_SAM_;
    return rc;
  }

  return rc;
}
#endif

CALYPSO_PROC CalypsoCardGetAtr(CALYPSO_CTX_ST *ctx, BYTE atr[], SIZE_T *atrsize)
{
  CALYPSO_RC rc;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_DEBUG|TR_CARD, "<GetAtr>");

  if (ctx->Card.AtrLen)
  {
    rc = 0;
  } else
  {
    ctx->Card.AtrLen = sizeof(ctx->Card.Atr);

    switch (ctx->Card.Type)
    {
#ifdef _USE_PCSC
      case CALYPSO_TYPE_PCSC   : rc = CalypsoPcscGetAtr(&ctx->Card.Pcsc, ctx->Card.Atr, &ctx->Card.AtrLen);
                                 break;
#endif
#ifdef CALYPSO_LEGACY
      case CALYPSO_TYPE_LEGACY : rc = CALYPSO_ERR_INVALID_PARAM;
                                 break;
#endif
      default                  : rc = CALYPSO_ERR_NOT_BINDED;
    }

    if (rc) ctx->Card.AtrLen = 0;
  }
  
  if (rc)
  {
    rc |= CALYPSO_ERR_CARD_;
    return rc;
  }

  if (rc == 0)
  {
    if ((atrsize != NULL) && (*atrsize < ctx->Card.AtrLen))
    {
      *atrsize = ctx->Card.AtrLen;
      rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
    } else
    {
      if (atr != NULL)
        memcpy(atr, ctx->Card.Atr, ctx->Card.AtrLen);
      if (atrsize != NULL)
        *atrsize = ctx->Card.AtrLen;
    }
  }

  CalypsoTraceRC(TR_DEBUG|TR_CARD, "</GetAtr> RC=", rc);
  return rc;
}

