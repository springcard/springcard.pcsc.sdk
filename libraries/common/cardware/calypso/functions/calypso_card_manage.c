/**h* CalypsoAPI/calypso_card_manage.c
 *
 * NAME
 *   SpringCard Calypso API :: Card command set (invalidate / rehabilitate / verify pin / change pin)
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release
 *   JDA 04/01/2010 : implemented invalidate / rehabilitate
 *
 **/
#include "../calypso_api_i.h"
#include "calypso_card_commands_i.h"

/*
 **********************************************************************************************************************
 *
 * INVALIDATE / REHABILITATE
 *
 **********************************************************************************************************************
 */

/**f* SpringProxINS/CalypsoCardInvalidate
 *
 * NAME
 *   CalypsoCardInvalidate
 *
 * DESCRIPTION
 *   Invalidate the currently selected card application
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx : library context
 *
 * RETURNS
 *   CALYPSO_RC          : 0 or an error code
 *
 * SEE ALSO
 *   CalypsoCardRehabilitate
 *
 **/
CALYPSO_PROC CalypsoCardInvalidate(CALYPSO_CTX_ST *ctx)
{
  CALYPSO_RC rc;
  SIZE_T recv_len = 2;
  SIZE_T send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "Invalidate");

  if (ctx->CardApplication.SessionActive && (ctx->CardApplication.SessionCurMods >= ctx->CardApplication.SessionMaxMods))
  {
    CalypsoTraceStr(TR_ERROR|TR_CARD, "Invalidate: Transaction limit");
  }

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_INVALIDATE;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x00;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;

    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("Invalidate");
}

/**f* SpringProxINS/CalypsoCardRehabilitate
 *
 * NAME
 *   CalypsoCardRehabilitate
 *
 * DESCRIPTION
 *   Rehabilitate the currently selected card application
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx : library context
 *
 * RETURNS
 *   CALYPSO_RC          : 0 or an error code
 *
 * SEE ALSO
 *   CalypsoCardInvalidate
 *
 **/
CALYPSO_PROC CalypsoCardRehabilitate(CALYPSO_CTX_ST *ctx)
{
  CALYPSO_RC rc;
  SIZE_T recv_len = 2;
  SIZE_T send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "Rehabilitate");

  if (ctx->CardApplication.SessionActive && (ctx->CardApplication.SessionCurMods >= ctx->CardApplication.SessionMaxMods))
  {
    CalypsoTraceStr(TR_ERROR|TR_CARD, "Rehabilitate: Transaction limit");
  }

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_REHABILITATE;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x00;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;

    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("Rehabilitate");
}
