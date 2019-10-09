/**h* CalypsoAPI/calypso_atr_to_xml.c
 *
 * NAME
 *   SpringCard Calypso API :: export ATR info to XML
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 05/02/2010 : first public release
 *
 **/
#include "../calypso_api_i.h"


static void  CalypsoOutputAtr_TA(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize);
static void  CalypsoOutputAtr_TB(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize);
static void  CalypsoOutputAtr_TC(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize);
static void  CalypsoOutputAtr_TD(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE *offset, const BYTE atr[], SIZE_T atrsize);

static CALYPSO_RC CalypsoOutputCardAtr_HistBytes(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T size);

/**f* CSB6_Calypso/CalypsoOutputCardAtr
 *
 * NAME
 *   CalypsoOutputCardAtr
 *
 * DESCRIPTION
 *   Output the ATR of the card
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
CALYPSO_PROC CalypsoOutputCardAtr(CALYPSO_CTX_ST *ctx, const BYTE atr[], SIZE_T atrsize)
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

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_RAW))
    ParserOut_Hex(ctx, strRaw, atr, atrsize);

  /* Now explain the ATR */
  Y1 = atr[1] / 0x10;
  K  = atr[1] % 0x10;

  offset = 2;

  if (Y1 & 0x01)
    CalypsoOutputAtr_TA(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x02)
    CalypsoOutputAtr_TB(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x04)
    CalypsoOutputAtr_TC(ctx, 1, 0, &offset, atr, atrsize);

  if (Y1 & 0x08)
    CalypsoOutputAtr_TD(ctx, 1, &offset, atr, atrsize);

  if (K)
  {
    /* Explain the historical bytes */
    CalypsoOutputCardAtr_HistBytes(ctx, &atr[offset], K);
  }
 
  return 0;
}

static void CalypsoOutputAtr_TA(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize)
{
	UNUSED_PARAMETER(ctx);
	UNUSED_PARAMETER(counter);
	UNUSED_PARAMETER(T);
	UNUSED_PARAMETER(atr);
	UNUSED_PARAMETER(atrsize);
	
  *offset = *offset + 1;
}

static void CalypsoOutputAtr_TB(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize)
{
	UNUSED_PARAMETER(ctx);
	UNUSED_PARAMETER(counter);
	UNUSED_PARAMETER(T);
	UNUSED_PARAMETER(atr);
	UNUSED_PARAMETER(atrsize);

  *offset = *offset + 1;
}

static void CalypsoOutputAtr_TC(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE T, BYTE *offset, const BYTE atr[], SIZE_T atrsize)
{
	UNUSED_PARAMETER(ctx);
	UNUSED_PARAMETER(counter);
	UNUSED_PARAMETER(T);
	UNUSED_PARAMETER(atr);
	UNUSED_PARAMETER(atrsize);

  *offset = *offset + 1;
}

static void CalypsoOutputAtr_TD(CALYPSO_CTX_ST *ctx, BYTE counter, BYTE *offset, const BYTE atr[], SIZE_T atrsize)
{
  BYTE value, Y, T;
 
  value = atr[*offset];
  *offset = *offset + 1;

  Y = value / 0x10;
  T = value % 0x10;

  counter++;

  if (Y & 0x01)
    CalypsoOutputAtr_TA(ctx, counter, T, offset, atr, atrsize);

  if (Y & 0x02)
    CalypsoOutputAtr_TB(ctx, counter, T, offset, atr, atrsize);

  if (Y & 0x04)
    CalypsoOutputAtr_TC(ctx, counter, T, offset, atr, atrsize);

  if (Y & 0x08)
    CalypsoOutputAtr_TD(ctx, counter, offset, atr, atrsize);
}


static CALYPSO_RC CalypsoOutputCardAtr_HistBytes(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T size)
{
  if ((size >= 15) && (data[0] == 0x80) && (data[1] == 0x5A) &&  (data[12] == 0x82))
  {
	  ParserOut_Hex8(ctx, strPlatform, data[2]);
	  ParserOut_Hex8(ctx, strType, data[3]);
	  ParserOut_Hex8(ctx, strSubType, data[4]);
	  ParserOut_Hex8(ctx, strSoftIssuer, data[5]);
	  ParserOut_Hex8(ctx, strSoftVer, data[6]);
	  ParserOut_Hex8(ctx, strSoftRev, data[7]);
	  ParserOut_Hex(ctx, strUid, &data[8], 4);
	  ParserOut_Hex(ctx, strStatus, &data[13], 2);
	}

  return 0;
}

