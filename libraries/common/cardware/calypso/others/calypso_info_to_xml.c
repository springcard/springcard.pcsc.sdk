/**h* CalypsoAPI/Calypso_Out_Info.c
 *
 * NAME
 *   SpringCard Calypso API :: Card info data output
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 16/12/2009 : first public release
 *
 **/
#include "../calypso_api_i.h"

/* Calypso TN001 and TN009 */

/**f* Calypso_API/CalypsoOutputCardInfo
 *
 * NAME
 *   CalypsoOutputCardInfo
 *
 * DESCRIPTION
 *   
 *
 * INPUTS
 *   P_CALYPSO_CTX ctx    : library context
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_PROC CalypsoOutputCardInfo(CALYPSO_CTX_ST *ctx)
{
  const char *name = "?";

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  
  if ((ctx->CardApplication.Type == 0x01) || (ctx->CardApplication.Type == 0x81))
  {
    /* CD97 group */
    if (ctx->CardApplication.SoftVersion < 0x04)
    {
      name = "CD97"; 
    } else
    {
      name = "CD97-BX (compat)";
    }
  } else
  if (ctx->CardApplication.Type == 0x02)
  {
    /* Modeus */
    name = "Modeus";     
  } else    
  if (ctx->CardApplication.Type == 0x03)
  {
    /* GTML group */
    switch (ctx->CardApplication.SubType)
    {
      case 0x06 : name = "BMS-2"; break;
      case 0x04 :
      case 0x05 :
      case 0x14 :
      case 0x15 : name = "GTML2"; break;
      case 0x81 :
      case 0x82 :
      case 0x83 :
      case 0x84 :
      case 0xA1 :
      case 0xA2 :
      case 0xA3 :
      case 0xA4 : name = "Tango"; break;      
      default   : name = "GTML"; 
    }
  } else
  if (ctx->CardApplication.Type == 0x04)
  {
    /* CT2000 */
    name = "CT2000";     
  } else    
  if ((ctx->CardApplication.Type >= 0x06) && (ctx->CardApplication.Type <= 0x1F))
  {
    /* Revision 2 */   
    switch (ctx->CardApplication.Type)
    {
      case 0x06 : name = "CDLight or TimeCOS"; break;
      case 0x07 : name = "CD97-BX"; break;
      case 0x11 :
      case 0x13 : name = "CD21 or CiTi"; break;

      default   : name = "Rev.2";
    }

  } else
  if ((ctx->CardApplication.Type >= 0x20) && (ctx->CardApplication.Type <= 0x27))
  {
    /* Revision 3, assigned values */
	switch (ctx->CardApplication.Type)
	{
	  case 0x20 : name = "Rev.3"; break;
	  case 0x21 : name = "Rev.3 +PIN"; break;
	  case 0x22 : name = "Rev.3 +SV"; break;
	  case 0x23 : name = "Rev.3 +PIN+SV"; break;
	  case 0x24 : name = "Rev.3 (RR)"; break;
	  case 0x25 : name = "Rev.3 +PIN (RR)"; break;
	  case 0x26 : name = "Rev.3 +SV (RR)"; break;
	  case 0x27 : name = "Rev.3 +PIN+SV (RR)"; break;
	  default   : name = "Rev.3 (20-27)";
	}
  } else
  if ((ctx->CardApplication.Type >= 0x28) && (ctx->CardApplication.Type <= 0x7F))
  {
    /* Revision 3, RFU values */
    name = "Rev.3 (28-7F)";
  }
   
  ParserOut_Dec(ctx,  strMaxMods, ctx->CardApplication.SessionMaxMods);
  ParserOut_Hex8(ctx, strPlatform, ctx->CardApplication.Platform);
  ParserOut_Hex8(ctx, strType, ctx->CardApplication.Type);
  ParserOut_Hex8(ctx, strSubType, ctx->CardApplication.SubType);
  ParserOut_Hex8(ctx, strSoftIssuer, ctx->CardApplication.SoftIssuer);
  ParserOut_Hex8(ctx, strSoftVer, ctx->CardApplication.SoftVersion);
  ParserOut_Hex8(ctx, strSoftRev, ctx->CardApplication.SoftRevision);    
  ParserOut_Dec(ctx,  strRevision, ctx->CardApplication.Revision);    
  ParserOut_Str(ctx,  strProduct, name);

  if (ctx->CardApplication.DFNameLen)
  {
    char buffer[sizeof(ctx->CardApplication.DFName) + 1];
    BOOL f = TRUE;
    BYTE i;

    memset(buffer, 0, sizeof(buffer));

    for (i=0; i<ctx->CardApplication.DFNameLen; i++)
    {
      if ((ctx->CardApplication.DFName[i] < ' ') || (ctx->CardApplication.DFName[i] >= 128))
      {
        f = FALSE;
        break;
      }
      buffer[i] = ctx->CardApplication.DFName[i];
    }

    if (f)
    {
      ParserOut_Str(ctx,  strDf, buffer);
    } else
    {
      ParserOut_Hex(ctx,  strDf, ctx->CardApplication.DFName, ctx->CardApplication.DFNameLen);
    }
  }

  return 0;
}




CALYPSO_PROC CalypsoOutputFileInfo(P_CALYPSO_CTX ctx, FILE_INFO_ST *file_info)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (file_info == NULL) return CALYPSO_ERR_INVALID_PARAM;

  ParserOut_Dec(ctx, "ShortId", file_info->ShortId);
  ParserOut_Dec(ctx, "Type", file_info->Type);
  switch (file_info->Type)
  {
    case 0x01 : ParserOut_Str(ctx, "Type_s", "MF"); break;
    case 0x02 : ParserOut_Str(ctx, "Type_s", "DF"); break;
    case 0x04 : ParserOut_Str(ctx, "Type_s", "EF"); break;
    default   : ParserOut_Str(ctx, "Type_s", "unknown"); break;
  }
  ParserOut_Dec(ctx, "SubType", file_info->SubType);
  switch (file_info->SubType)
  {
    case 0x00 : ParserOut_Str(ctx, "SubType_s", ""); break;
    case 0x02 : ParserOut_Str(ctx, "SubType_s", "Linear"); break;
    case 0x04 : ParserOut_Str(ctx, "SubType_s", "Cyclic"); break;
    case 0x08 : ParserOut_Str(ctx, "SubType_s", "Counter"); break;
    default   : ParserOut_Str(ctx, "SubType_s", "unknown"); break;
  }
  ParserOut_Dec(ctx, "RecSize", file_info->RecSize);
  ParserOut_Dec(ctx, "NumRec", file_info->NumRec);
  ParserOut_Hex32(ctx, "AC", file_info->AC);
  ParserOut_Hex32(ctx, "NKey", file_info->NKey);
  ParserOut_Hex8(ctx, "Status", file_info->Status);
  ParserOut_Hex8(ctx, "KVC1", file_info->KVC[0]);
  ParserOut_Hex8(ctx, "KVC2", file_info->KVC[1]);
  ParserOut_Hex8(ctx, "KVC3", file_info->KVC[2]);

  return 0;
}
