/**h* CalypsoAPI/Calypso_Dec_ATR.c
 *
 * NAME
 *   SpringCard Calypso API :: Card ATR parser
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

static void  CalypsoParseAtr_TA(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize);
static void  CalypsoParseAtr_TB(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize);
static void  CalypsoParseAtr_TC(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize);
static void  CalypsoParseAtr_TD(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE *offset, const BYTE atr[], SIZE_T atrsize);

static CALYPSO_RC CalypsoParseCardAtr_HistBytes(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T size);
#ifndef CALYPSO_NO_SAM
static CALYPSO_RC CalypsoParseSamAtr_HistBytes(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T size);
#endif

/**f* CSB6_Calypso/CalypsoParseCardAtr
 *
 * NAME
 *   CalypsoParseCardAtr
 *
 * DESCRIPTION
 *   Parse the ATR of the card
 *
 * INPUTS
 *   P_CALYPSO_CTX_ST  ctx          : library context
 *   BYTE              atr[]          : the ATR
 *   BYTE              atrsize        : size of the atr
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_PROC CalypsoParseCardAtr(CALYPSO_CTX_ST *ctx, const BYTE atr[], SIZE_T atrsize)
{
  BYTE offset = 0;
  BYTE Y1;
  BYTE K;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;


  if (atr == NULL)
  {
    if (ctx->Card.AtrLen)
    {
      atr     = ctx->Card.Atr;
      atrsize = ctx->Card.AtrLen;
    } else
      return CALYPSO_ERR_INVALID_PARAM;
  }

  CalypsoTraceStr(TR_TRACE|TR_CARD, "ParseCardAtr");
  CalypsoTraceHex(TR_TRACE|TR_CARD, strAtr, atr, atrsize);

  /* Now explain the ATR */
  Y1 = atr[1] / 0x10;
  K  = atr[1] % 0x10;

  offset = 2;

  if (Y1 & 0x01)
    CalypsoParseAtr_TA(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x02)
    CalypsoParseAtr_TB(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x04)
    CalypsoParseAtr_TC(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x08)
    CalypsoParseAtr_TD(ctx, 1, &offset, atr, atrsize);

  if (K)
  {
    /* Explain the historical bytes */
    CalypsoParseCardAtr_HistBytes(ctx, &atr[offset], K);
  }
 
  return 0;
}

#ifndef CALYPSO_NO_SAM
/**f* CSB6_Calypso/CalypsoParseSamAtr
 *
 * NAME
 *   CalypsoParseSamAtr
 *
 * DESCRIPTION
 *   Parse the ATR of the SAM
 *
 * INPUTS
 *   P_CALYPSO_CTX_ST  ctx          : library context
 *   BYTE              atr[]          : the ATR
 *   BYTE              atrsize        : size of the atr
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_PROC CalypsoParseSamAtr(CALYPSO_CTX_ST *ctx, const BYTE atr[], SIZE_T atrsize)
{
  BYTE offset = 0;
  BYTE Y1;
  BYTE K;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;


  if (atr == NULL)
  {
    if (ctx->Sam.AtrLen)
    {
      atr     = ctx->Sam.Atr;
      atrsize = ctx->Sam.AtrLen;
    } else
      return CALYPSO_ERR_INVALID_PARAM;
  }

  CalypsoTraceStr(TR_TRACE|TR_SAM, "ParseSamAtr");
  CalypsoTraceHex(TR_TRACE|TR_SAM, strAtr, atr, atrsize);

  /* Now explain the ATR */
  Y1 = atr[1] / 0x10;
  K  = atr[1] % 0x10;

  offset = 2;

  if (Y1 & 0x01)
    CalypsoParseAtr_TA(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x02)
    CalypsoParseAtr_TB(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x04)
    CalypsoParseAtr_TC(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x08)
    CalypsoParseAtr_TD(ctx, 1, &offset, atr, atrsize);

  if (K)
  {
    /* Explain the historical bytes */
    CalypsoParseSamAtr_HistBytes(ctx, &atr[offset], K);
  }
 
  return 0;
}
#endif

static void CalypsoParseAtr_TA(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize)
{
	UNUSED_PARAMETER(ctx);
	UNUSED_PARAMETER(counter);
	UNUSED_PARAMETER(T);
	UNUSED_PARAMETER(atr);
	UNUSED_PARAMETER(atrsize);

  *offset = *offset + 1;
}

static void CalypsoParseAtr_TB(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize)
{
	UNUSED_PARAMETER(ctx);
	UNUSED_PARAMETER(counter);
	UNUSED_PARAMETER(T);
	UNUSED_PARAMETER(atr);
	UNUSED_PARAMETER(atrsize);

  *offset = *offset + 1;
}

static void CalypsoParseAtr_TC(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize)
{
	UNUSED_PARAMETER(ctx);
	UNUSED_PARAMETER(counter);
	UNUSED_PARAMETER(T);
	UNUSED_PARAMETER(atr);
	UNUSED_PARAMETER(atrsize);

  *offset = *offset + 1;
}

static void CalypsoParseAtr_TD(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE *offset, const BYTE atr[], SIZE_T atrsize)
{
  BYTE value, Y, T;
 
  value = atr[*offset];
  *offset = *offset + 1;

  Y = value / 0x10;
  T = value % 0x10;

  counter++;

  if (Y & 0x01)
    CalypsoParseAtr_TA(ctx, counter, T, offset, atr, atrsize);

  if (Y & 0x02)
    CalypsoParseAtr_TB(ctx, counter, T, offset, atr, atrsize);

  if (Y & 0x04)
    CalypsoParseAtr_TC(ctx, counter, T, offset, atr, atrsize);

  if (Y & 0x08)
    CalypsoParseAtr_TD(ctx, counter, offset, atr, atrsize);
}


static CALYPSO_RC CalypsoParseCardAtr_HistBytes(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T size)
{
  BYTE i;

  if (size < 15)
    return CALYPSO_ERR_ATR_INVALID;

  if ((data[0] != 0x80) || (data[1] != 0x5A))
  {
    CalypsoTraceHex(TR_TRACE|TR_CARD, "Not a Calypso ATR ", data, 2);
    return CALYPSO_ERR_ATR_INVALID;
  }
  
  /* Define the MaxSessionModif to its default */
  if (!ctx->CardApplication.SessionMaxMods)
  {
    ctx->CardApplication.SessionMaxMods = CALYPSO_MIN_SESSION_MODIFS;
  }

  /* Retrieve the information table */
  ctx->CardApplication.Platform     = data[2];
  ctx->CardApplication.Type         = data[3];
  ctx->CardApplication.SubType      = data[4];
  ctx->CardApplication.SoftIssuer   = data[5];
  ctx->CardApplication.SoftVersion  = data[6];
  ctx->CardApplication.SoftRevision = data[7];

  /* Recognize the actual revision of the specifications */  
  CalypsoRecognizeRevision(ctx);

  /* Do we already know the UID ? */  
  for (i=0; i<8; i++)
    if (ctx->CardApplication.UID[i] != 0x00) break;
  
  if (i >= 8)
  {
    /* Put the serial number in the context */
    memcpy(&ctx->CardApplication.UID[4], &data[8], 4);
  }

  CalypsoTraceHex(TR_TRACE|TR_CARD, strUid, ctx->CardApplication.UID, sizeof(ctx->CardApplication.UID));
  CalypsoTraceHex(TR_TRACE|TR_CARD, strStatus, &data[12], 3);

  return 0;
}

#ifndef CALYPSO_NO_SAM
static CALYPSO_RC CalypsoParseSamAtr_HistBytes(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T size)
{
  if (size < 15)
    return CALYPSO_ERR_ATR_INVALID;

  if ((data[0] != 0x80) || (data[1] != 0x5A))
  {
    CalypsoTraceHex(TR_TRACE|TR_SAM, "Not a Calypso ATR ", data, 2);
    return CALYPSO_ERR_ATR_INVALID;
  }

  if (data[3] != 0x80)
  {
    CalypsoTraceHex(TR_TRACE|TR_SAM, "Not a Calypso SAM's ATR ", data, size);
    return CALYPSO_ERR_ATR_INVALID;
  }

  /* The UID is there */  
  memcpy(&ctx->SamApplication.UID[0], &data[8], 4);

  CalypsoTraceHex(TR_TRACE|TR_SAM, strUid, ctx->SamApplication.UID, sizeof(ctx->SamApplication.UID));
  CalypsoTraceHex(TR_TRACE|TR_SAM, strStatus, &data[12], 3);

  return 0;
}
#endif
