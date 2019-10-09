/**h* CalypsoAPI/Calypso_Commands.c
 *
 * NAME
 *   SpringCard Calypso API :: Card command set
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
#include "calypso_card_commands_i.h"

CALYPSO_RC CalypsoCardSetSW(CALYPSO_CTX_ST *ctx, SIZE_T recv_len)
{
  if (recv_len < 2) return CALYPSO_ERR_RESPONSE_MISSING;

  ctx->Card.SW   = ctx->Card.Buffer[recv_len-2];
  ctx->Card.SW <<= 8; 
  ctx->Card.SW  |= ctx->Card.Buffer[recv_len-1];

  CalypsoTraceValH(TR_DEBUG|TR_CARD, "Card's SW=", ctx->Card.SW, 4);
  return CALYPSO_SUCCESS;
}

CALYPSO_PROC CalypsoCardGetSW(CALYPSO_CTX_ST *ctx, BYTE sw[2])
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (sw == NULL)  return CALYPSO_ERR_INVALID_PARAM;

  sw[0] = (BYTE) ((ctx->Card.SW >> 8) & 0x00FF);
  sw[1] = (BYTE) ( ctx->Card.SW       & 0x00FF);

  return CALYPSO_SUCCESS;
}

CALYPSO_RC CalypsoCardGetResponse(CALYPSO_CTX_ST *ctx, SIZE_T *recv_len)
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

  if ((ctx->Card.SW & 0xFF00) == 0x6100)
  {
    data_len = (BYTE) (ctx->Card.SW & 0x00FF);
  }

again:

  apdu[0] = cla;
  apdu[1] = CALYPSO_INS_GET_RESPONSE;
  apdu[2] = 0x00;
  apdu[3] = 0x00;
  apdu[4] = data_len;

  rc = CalypsoCardTransmit(ctx, apdu, 5, ctx->Card.Buffer, recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, *recv_len);
  if (rc) goto done;

  if ((ctx->Card.SW & 0xFF00) == 0x6C00)
  {
    /* New Get Response with specified Le */
    data_len = (BYTE) (ctx->Card.SW & 0x00FF);
    *recv_len = old_recv_len;
    goto again;
  }
  
  switch (ctx->Card.SW)
  {
    case 0x9000 : break;
    case 0x6D00 : break; /* No data */

    case 0x6E00 : /* CLA invalid */
                  if (first_time && (cla != ctx->Card.CLA))
                  {
                    first_time = FALSE;
                    cla = ctx->Card.CLA;
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


