/**h* CalypsoAPI/Calypso_Dec_FCI.c
 *
 * NAME
 *   SpringCard Calypso API :: FCI handling
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

BOOL TLVLoop(const BYTE buffer[], BYTE *offset, WORD *tag, BYTE *length, const BYTE *value[]);
CALYPSO_RC CalypsoParseApplicationFciTpl(CALYPSO_CTX_ST *ctx, const BYTE tpl[], SIZE_T tplsize);
CALYPSO_RC CalypsoParseApplicationFciProp(CALYPSO_CTX_ST *ctx, const BYTE prop[], SIZE_T propsize);
CALYPSO_RC CalypsoParseApplicationFciDisc(CALYPSO_CTX_ST *ctx, const BYTE disc[], SIZE_T discsize);

/**f* CSB6_Calypso/CalypsoParseFci
 *
 * NAME
 *   CalypsoParseFci
 *
 * DESCRIPTION
 *   Parse the FCI
 *
 * INPUTS
 *   CALYPSO_CTX_ST  p_ctx          : library context
 *   BYTE               fci[]          : the FCI
 *   BYTE              fcisize        : size of the FCI
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_RC CalypsoParseFci(CALYPSO_CTX_ST *ctx, const BYTE fci[], SIZE_T fcisize)
{
  BYTE offset, length;
  const BYTE *value;
  WORD  tag;
  CALYPSO_RC rc = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (fci == NULL)
  {
    if (ctx->CardApplication.FciLen)
    {
      fci     = ctx->CardApplication.Fci;
      fcisize = ctx->CardApplication.FciLen;
    } else
      return CALYPSO_ERR_INVALID_PARAM;
  }  

  /* Now parse the FCI (T,L,V structure) */
  offset = 0;
  while ((offset < fcisize) && TLVLoop(fci, &offset, &tag, &length, &value) && (!rc))
  {
    if (tag == 0x6F)
    {
      /* This is the FCI template */
      rc = CalypsoParseApplicationFciTpl(ctx, value, length);
    }
  }

  return rc;
}

/**f* CSB6_Calypso/CalypsoParseIcc
 *
 * NAME
 *   CalypsoParseIcc
 *
 * DESCRIPTION
 *   Parse the ICC
 *
 * INPUTS
 *   CALYPSO_CTX_ST  p_ctx          : library context
 *   BYTE               icc[]          : the ICC
 *   BYTE              iccsize        : size of the ICC
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_RC CalypsoParseIcc(CALYPSO_CTX_ST *ctx, const BYTE icc[], SIZE_T iccsize)
{
  CALYPSO_RC rc = CALYPSO_CARD_ICC_NOT_SUPPORTED;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (icc == NULL) return CALYPSO_ERR_INVALID_PARAM;

  /* Now parse the FCI (T,L,V structure) */
  if (iccsize == 29)
  {
    if (ctx->CardApplication.Revision < 1)
	 ctx->CardApplication.Revision = 1;
    if ((icc[4] == 'F') && (icc[5] == 'R'))
    {
	 memset(&ctx->CardApplication.UID[0], 0, 4);
	 memcpy(&ctx->CardApplication.UID[4], &icc[0], 4);
    } else
    {
	 memcpy(ctx->CardApplication.UID, &icc[12], 8);
    }
    rc = 0;    
  }  

  return rc;
}

CALYPSO_RC CalypsoParseApplicationFciTpl(CALYPSO_CTX_ST *ctx, const BYTE tpl[], SIZE_T tplsize)
{
  BYTE offset, length;
  WORD tag;
  const BYTE *value;
  CALYPSO_RC rc = 0;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < tplsize) && TLVLoop(tpl, &offset, &tag, &length, &value) && (!rc))
  {
    if (tag == 0x84)
    {
      /* DFName */
      if (length <= 16)
      {
        memcpy(ctx->CardApplication.DFName, value, length);
        ctx->CardApplication.DFNameLen = length;
      }

    } else
    if (tag == 0xA5)
    {
      /* This is the FCI Proprietary template */
      rc = CalypsoParseApplicationFciProp(ctx, value, length);
    }
  }

  return rc;
}

CALYPSO_RC CalypsoParseApplicationFciProp(CALYPSO_CTX_ST *ctx, const BYTE prop[], SIZE_T propsize)
{
  BYTE offset, length;
  WORD tag;
  const BYTE *value;
  CALYPSO_RC rc = 0;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < propsize) && TLVLoop(prop, &offset, &tag, &length, &value) && (!rc))
  {
    if (tag == 0xBF0C)
    {
      /* This is the FCI Issuer Discreationary template */
      rc = CalypsoParseApplicationFciDisc(ctx, value, length);
    }
  }

  return rc;
}

/*
 * Decodage du FCI (reponse de la carte a l'APDU "Select Application"
 * On y retrouve notamment l'UID complet (8 octets)
 */
CALYPSO_RC CalypsoParseApplicationFciDisc(CALYPSO_CTX_ST *ctx, const BYTE disc[], SIZE_T discsize)
{
  BYTE offset, length;
  WORD tag;
  const BYTE *value;
  CALYPSO_RC rc = 0;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < discsize) && TLVLoop(disc, &offset, &tag, &length, &value) && (!rc))
  {
    if (tag == 0xC7)
    {
      /* This is at least revision 2 ! */
      if (ctx->CardApplication.Revision < 2) ctx->CardApplication.Revision = 2;

      /* Full serial number */
      if (length == sizeof(ctx->CardApplication.UID))
      {
        memcpy(ctx->CardApplication.UID, value, sizeof(ctx->CardApplication.UID));
      } else
        rc = CALYPSO_CARD_DATA_MALFORMED;

    } else
    if (tag == 0x53)
    {
      if (length == sizeof(ctx->CardApplication.StartupInfo))
      {
        /* Store the startup information */
        memcpy(ctx->CardApplication.StartupInfo, value, sizeof(ctx->CardApplication.StartupInfo));

        /* Store the decoded information table */
        ctx->CardApplication.SessionMaxMods = value[0];
        ctx->CardApplication.Platform       = value[1];
        ctx->CardApplication.Type           = value[2];
        ctx->CardApplication.SubType        = value[3];
        ctx->CardApplication.SoftIssuer     = value[4];
        ctx->CardApplication.SoftVersion    = value[5];
        ctx->CardApplication.SoftRevision   = value[6];

        /* Recognize the actual revision of the specifications */  
        CalypsoRecognizeRevision(ctx);

      } else
        rc = CALYPSO_CARD_DATA_MALFORMED;
    }
  }

  return rc;
}

BOOL TLVLoop(const BYTE buffer[], BYTE *offset, WORD *tag, BYTE *length, const BYTE *value[])
{
  WORD t;
  BYTE o;
  WORD l;

  if (buffer == NULL)
    return FALSE;

  if (offset != NULL)
    o = *offset;
  else
    o = 0;

  if ((buffer[o] == 0x00) || (buffer[o] == 0xFF))
    return FALSE;

  /* Read the tag */
  if ((buffer[o] & 0x1F) != 0x1F)
  {
    /* Short tag */
    t = buffer[o++];
  } else
  {
    /* Long tag */
    t = buffer[o++];
    t <<= 8;
    t |= buffer[o++];
  }

  if (tag != NULL)
    *tag = t;

  /* Read the length */
  if (buffer[o] & 0x80)
  {
    /* Multi-byte lenght */
    switch (buffer[o++] & 0x7F)
    {
      case 0x01:
        l = buffer[o++];
        break;
      case 0x02:
        l = buffer[o++];
        l <<= 8;
        l += buffer[o++];
        break;
      default:
        return FALSE;
    }
  } else
  {
    /* Length on a single byte */
    l = buffer[o++];
  }

  if (l >= 0x0100)
    return FALSE;              /* Overflow */

  if (length != NULL)
    *length = (BYTE) l;

  /* Get a pointer on data */
  if (value != NULL)
    *value = (const BYTE *) &buffer[o];

  /* Jump to the end of data */
  o += l;

  if (offset != NULL)
    *offset = (BYTE) o;

  return TRUE;
}




/* Calypso TN001 and TN009 */
void CalypsoRecognizeRevision(CALYPSO_CTX_ST *ctx)
{
  BYTE rev = 3; /* Be optimistic */
  
  if ((ctx->CardApplication.Type == 0x01) || (ctx->CardApplication.Type == 0x81))
  {
    /* CD97 group */
    if (ctx->CardApplication.SoftVersion < 0x04)
    {
      rev = 1;
    } else
    {
      rev = 2;
    }
  } else
  if (ctx->CardApplication.Type == 0x02)
  {
    /* Modeus */
    rev  = 1;
  } else    
  if (ctx->CardApplication.Type == 0x03)
  {
    /* GTML group */
    switch (ctx->CardApplication.SubType)
    {
      case 0x06 :
      case 0x04 :
      case 0x05 :
      case 0x14 :
      case 0x15 :
      case 0x81 :
      case 0x82 :
      case 0x83 :
      case 0x84 :
      case 0xA1 :
      case 0xA2 :
      case 0xA3 :
      case 0xA4 : rev = 2; break;      
      default   : rev = 1;
    }
  } else
  if (ctx->CardApplication.Type == 0x04)
  {
    /* CT2000 */
    rev  = 1;
  } else    
  if ((ctx->CardApplication.Type >= 0x06) && (ctx->CardApplication.Type <= 0x1F))
  {
    /* Revision 2 */
    rev  = 2;
  } else
  if ((ctx->CardApplication.Type >= 0x20) && (ctx->CardApplication.Type <= 0x7F))
  {
    /* Revision 3 */
    if (ctx->CardApplication.Type & 0x01)
      ctx->CardApplication.WithPin = TRUE;
    if (ctx->CardApplication.Type & 0x02)
      ctx->CardApplication.WithStoredValue = TRUE;
    if (ctx->CardApplication.Type & 0x01)
      ctx->CardApplication.NeedRatificationFrame = TRUE;
  } else
  {
    /* RFU... */
  }
   
  ctx->CardApplication.Revision = rev;
  switch (rev)
  {
    case 1  :
    case 2  : ctx->Card.CLA = 0x94;
              break;
      
    case 3  : ctx->Card.CLA = 0x00;
              break;      
             
    default : /* Future Calypso revision ??? */
              ctx->Card.CLA = 0x00;
              break;      
  }

#ifndef CALYPSO_MINIMAL_STRINGS
  CalypsoTraceValD(TR_TRACE|TR_CARD, strMaxMods, ctx->CardApplication.SessionMaxMods, 0);
  CalypsoTraceValH(TR_TRACE|TR_CARD, strPlatform, ctx->CardApplication.Platform, 2);
  CalypsoTraceValH(TR_TRACE|TR_CARD, strType, ctx->CardApplication.Type, 2);
  CalypsoTraceValH(TR_TRACE|TR_CARD, strSubType, ctx->CardApplication.SubType, 2);
  CalypsoTraceValH(TR_TRACE|TR_CARD, strSoftIssuer, ctx->CardApplication.SoftIssuer, 2);
  CalypsoTraceValH(TR_TRACE|TR_CARD, strSoftVer, ctx->CardApplication.SoftVersion, 2);
  CalypsoTraceValH(TR_TRACE|TR_CARD, strSoftRev, ctx->CardApplication.SoftRevision, 2);
#endif

  CalypsoTraceValD(TR_TRACE|TR_CARD, strRevision, ctx->CardApplication.Revision, 0);      

#ifndef CALYPSO_MINIMAL_STRINGS
  if (ctx->CardApplication.WithPin)
    CalypsoTraceStr(TR_TRACE|TR_CARD, strWithPin);
  if (ctx->CardApplication.WithStoredValue)
    CalypsoTraceStr(TR_TRACE|TR_CARD, strWithStoredValue);
  if (ctx->CardApplication.NeedRatificationFrame)
    CalypsoTraceStr(TR_TRACE|TR_CARD, strNeedRatificationFrame);
#endif
}
