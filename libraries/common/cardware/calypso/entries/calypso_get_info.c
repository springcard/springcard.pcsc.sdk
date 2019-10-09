/**h* CalypsoAPI/Calypso_Info.c
 *
 * NAME
 *   SpringCard Calypso API :: Access to card's information
 *
 * DESCRIPTION
 *   Retrieve information from the current context after a card has been
 *   successfully activated using CalypsoCardActivate
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

CALYPSO_LIB void CALYPSO_API CalypsoGetVersion(char *version, SIZE_T versionsize)
{
  const char s[] = "SpringCard 'VOJJ' library for Calypso v" CALYPSO_API_VERSION;
  SIZE_T i;

  if (version != NULL)
  {
    for (i=0; i<sizeof(s); i++)
    {
      if (i >= (versionsize - 1))
      {
        version[i] = '\0';
        break;
      }
      version[i] = s[i];
    }
  }
}

/**f* CSB6_Calypso/CalypsoCardActivate
 *
 * NAME
 *   CalypsoCardActivate
 *
 * DESCRIPTION
 *   Activate a Calypso card. Check that the ATR indicates that the card is actually
 *   Calypso, or uses the Select Application APDU command to enter the Calypso
 *   ticketing application. In both cases, parses the available data, and populate
 *   the card related fields in the context.
 *
 * INPUTS
 *   P_CSB6_CALYPSO_CTX  p_ctx          : library context
 *   BYTE               aid[]          : optional : name of the ticketing application
 *   DWORD              aidsize        : size of the aid
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 * NOTES
 *   If AID is not provided (NULL), the default AID "1TIC.ICA" is used.
 *
 **/
CALYPSO_PROC CalypsoCardActivate(CALYPSO_CTX_ST *ctx, const BYTE aid[], SIZE_T aidsize)
{
  CALYPSO_RC rc;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;  

  memset(&ctx->CardApplication, 0, sizeof(ctx->CardApplication));

  /* Select the Ticketing application. May fail (old cards don't have it) */
  rc = CalypsoCardSelectApplication(ctx, aid, aidsize, NULL, NULL);
  CalypsoTraceRC(TR_TRACE|TR_CARD, "- CardSelectApplication ->", rc);
  if (rc & CALYPSO_ERR_FATAL_) return rc;
  
  if (rc == 0)
  {
    /* Parse the FCI returned by the Ticketing application */
    rc = CalypsoParseFci(ctx, NULL, 0);
    CalypsoTraceRC(TR_TRACE|TR_CARD, "- ParseFci ->", rc);
    if (rc) return rc;
  } else
  {
    /* Parse the ATR instead */
#ifdef CALYPSO_HOST
    if (!ctx->Card.AtrLen)
    {
      /* Get ATR as returned by the reader */
      CALYPSO_RC atr_rc = CalypsoCardGetAtr(ctx, NULL, NULL);
      CalypsoTraceRC(TR_TRACE|TR_CARD, "- CardGetAtr ->", atr_rc);
      if (atr_rc & CALYPSO_ERR_FATAL_) return rc;
    }
#endif

    rc = CalypsoParseCardAtr(ctx, NULL, 0);
    CalypsoTraceRC(TR_TRACE|TR_CARD, "- ParseCardAtr ->", rc);

#ifdef CALYPSO_HOST
    if ((rc == CALYPSO_ERR_ATR_INVALID) || (rc == CALYPSO_ERR_STATUS_WORD))
    {
      /* Ask the reader the real ATR (if reader is compliant with this specific command...) */
      SIZE_T recv_len, send_len;

      send_len = 0;
      ctx->Card.Buffer[send_len++] = 0xFF; /* Embedded APDU interpreter CLA */
      ctx->Card.Buffer[send_len++] = 0xCA; /* Get Data */
      ctx->Card.Buffer[send_len++] = 0xFA; /* ATR */
      ctx->Card.Buffer[send_len++] = 0x01; /* Actual Calypso ATR, no a computed one */
      ctx->Card.Buffer[send_len++] = 0x00;

      recv_len = sizeof(ctx->Card.Buffer);
      if ((CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len) == CALYPSO_SUCCESS)
       && (recv_len > 2) && (ctx->Card.Buffer[recv_len-2] == 0x90) && (ctx->Card.Buffer[recv_len-1] == 0x00))
      {
        /* Everything is OK */
        ctx->Card.AtrLen = recv_len-2;
        if (ctx->Card.AtrLen > sizeof(ctx->Card.Atr))
          ctx->Card.AtrLen = sizeof(ctx->Card.Atr);
        memcpy(ctx->Card.Atr, ctx->Card.Buffer, ctx->Card.AtrLen);

        CalypsoTraceHex(TR_TRACE|TR_CARD, "- New ATR =", ctx->Card.Atr, ctx->Card.AtrLen);

        rc = CalypsoParseCardAtr(ctx, NULL, 0);
        CalypsoTraceRC(TR_TRACE|TR_CARD, "- ParseCardAtr ->", rc);
      }
    }
#endif

    if (!ctx->CardApplication.Revision)
    {
      /* At least try to select the Transport DF */
      SIZE_T recv_len;
      recv_len = sizeof(ctx->Card.Buffer);
      rc = CalypsoCardSelectDF(ctx, CALYPSO_DF_TICKETING, ctx->Card.Buffer, &recv_len);
      if (rc & CALYPSO_ERR_FATAL_) return rc;
      if (rc == 0)
      {
        /* Success */

        CalypsoTraceHex(TR_TRACE|TR_CARD, "- Transport DF ", ctx->Card.Buffer, recv_len);
        ctx->CardApplication.Revision = 1;
      }
    }

  }

  if (!ctx->CardApplication.Revision)
  {
    CalypsoTraceStr(TR_TRACE|TR_CARD, "Unsupported Card");
    rc = CALYPSO_CARD_NOT_SUPPORTED;
  }

  return rc;
}

CALYPSO_RC CalypsoCardSerialNumber(CALYPSO_CTX_ST *ctx, BYTE card_uid[8])
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (!ctx->CardApplication.Revision) return CALYPSO_CARD_NOT_SUPPORTED;

  if (card_uid != NULL)
    memcpy(card_uid, ctx->CardApplication.UID, 8);

  return 0;
}

CALYPSO_PROC CalypsoCardStartupInfo(P_CALYPSO_CTX ctx, BYTE info[7])
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (!ctx->CardApplication.Revision) return CALYPSO_CARD_NOT_SUPPORTED;

  if (info != NULL)
    memcpy(info, ctx->CardApplication.StartupInfo, 7);

  return 0;
}

CALYPSO_RC CalypsoCardRevision(CALYPSO_CTX_ST *ctx)
{
  if (ctx == NULL) return 0;
  return ctx->CardApplication.Revision;  
}

CALYPSO_RC CalypsoCardDFName(CALYPSO_CTX_ST *ctx, BYTE name[], SIZE_T *namesize)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (!ctx->CardApplication.Revision) return CALYPSO_CARD_NOT_SUPPORTED;

  if ((namesize != NULL) && (*namesize != 0) && (*namesize < ctx->CardApplication.DFNameLen))
  {
    *namesize = ctx->CardApplication.DFNameLen;
    return CALYPSO_ERR_BUFFER_TOO_SHORT;
  }

  if (namesize != NULL)
    *namesize = ctx->CardApplication.DFNameLen;
  if (name != NULL)
    memcpy(name, ctx->CardApplication.DFName, ctx->CardApplication.DFNameLen);

  return 0;
}

CALYPSO_RC CalypsoCardSessionModifs(CALYPSO_CTX_ST *ctx)
{
  if (ctx == NULL) return 0;
  return ctx->CardApplication.SessionCurMods;  
}

CALYPSO_RC CalypsoCardSessionModifsMax(CALYPSO_CTX_ST *ctx)
{
  if (ctx == NULL) return 0;
  return ctx->CardApplication.SessionMaxMods;  
}


