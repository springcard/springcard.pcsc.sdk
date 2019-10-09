/**h* CalypsoAPI/Calypso_Dec_FInfo.c
 *
 * NAME
 *   SpringCard Calypso API :: File information handling
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

CALYPSO_RC CalypsoParseSelectResp(CALYPSO_CTX_ST *ctx, const BYTE resp[], SIZE_T respsize, FILE_INFO_ST *file_info)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (resp == NULL)
  {
    if (ctx->LastCardSelectRespLen)
    {
      resp     = ctx->LastCardSelectResp;
      respsize = ctx->LastCardSelectRespLen;
    } else
      return CALYPSO_ERR_INVALID_PARAM;
  }

  if (respsize < 19) return CALYPSO_CARD_FILE_INFO_INVALID;
  if ((resp[0] != 0x85) || (resp[1] != 0x17)) return CALYPSO_CARD_FILE_INFO_INVALID;

  if (file_info != NULL)
  {
    file_info->ShortId = resp[2];    
    file_info->Type    = resp[3];
    file_info->SubType = resp[4];
    file_info->RecSize = resp[5];
    file_info->NumRec  = resp[6];

    file_info->AC      = resp[7];  file_info->AC <<= 8;
    file_info->AC     |= resp[8];  file_info->AC <<= 8;
    file_info->AC     |= resp[9];  file_info->AC <<= 8;
    file_info->AC     |= resp[10];

    file_info->NKey    = resp[11];  file_info->NKey <<= 8;
    file_info->NKey   |= resp[12];  file_info->NKey <<= 8;
    file_info->NKey   |= resp[13];  file_info->NKey <<= 8;
    file_info->NKey   |= resp[14];

    file_info->Status  = resp[15];

    file_info->KVC[0]  = resp[16];
    file_info->KVC[1]  = resp[17];
    file_info->KVC[2]  = resp[18];
  }

  return 0;
}
