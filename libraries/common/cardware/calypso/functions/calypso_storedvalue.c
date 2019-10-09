/**h* CalypsoAPI/calypso_storedvalue.c
 *
 * NAME
 *   SpringCard Calypso API :: Card and SAM command set (stored value commands)
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 04/01/2009 : first public release
 *
 **/
#include "../calypso_api_i.h"
#include "calypso_card_commands_i.h"
#include "calypso_sam_commands_i.h"

#if (defined(CALYPSO_STOREDVALUE))

#define CALYPSO_SV_RELOAD_TYPE           0x07
#define CALYPSO_SV_RELOAD_GET_LE_NO_UID  0x21
#define CALYPSO_SV_RELOAD_GET_LE____UID  0x29
#define CALYPSO_SV_RELOAD_SAM_LC         0x36
#define CALYPSO_SV_RELOAD_SAM_LE         0x0B
#define CALYPSO_SV_RELOAD_CARD_LC        0x17

#define CALYPSO_SV_DEBIT_TYPE            0x09
#define CALYPSO_SV_DEBIT_GET_LE_NO_UID   0x1E
#define CALYPSO_SV_DEBIT_GET_LE____UID   0x26
#define CALYPSO_SV_DEBIT_SAM_LC          0x30
#define CALYPSO_SV_DEBIT_SAM_LE          0x0B
#define CALYPSO_SV_DEBIT_CARD_LC         0x14

CALYPSO_PROC CalypsoCardStoredValueGetDecode(CALYPSO_CTX_ST *ctx, const BYTE get_sv_resp[], SIZE_T get_sv_respsize, signed long *balance, CALYPSO_STOREDVALUE_ST *target)
{
  CALYPSO_STOREDVALUE_ST l_target;
  SIZE_T r = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
 
  if (get_sv_resp == NULL)
  {
    get_sv_resp     = ctx->CardStoredValue.GetData;
    get_sv_respsize = ctx->CardStoredValue.GetDataSize;
  }

  memset(&l_target, 0, sizeof(CALYPSO_STOREDVALUE_ST));

  if (get_sv_respsize == CALYPSO_SV_RELOAD_GET_LE_NO_UID)
  {
    /* Reload */
    l_target._IsReload = TRUE;
  } else
  if (get_sv_respsize == CALYPSO_SV_RELOAD_GET_LE____UID)
  {
    /* Reload with serial number */
    l_target._IsReload = TRUE;
    l_target._HasSerialNumber = TRUE;
  } else
  if (get_sv_respsize == CALYPSO_SV_DEBIT_GET_LE_NO_UID)
  {
    /* Debit */
    l_target._IsDebit = TRUE;
  } else
  if (get_sv_respsize == CALYPSO_SV_DEBIT_GET_LE____UID)
  {
    /* Debit with serial number */
    l_target._IsDebit = TRUE;
    l_target._HasSerialNumber = TRUE;
  } else
  {
    return CALYPSO_ERR_INVALID_PARAM;
  }

  /* KVC */
  l_target.CurrentKVC = get_sv_resp[r++];
  /* skip SV TNum */
  r += 2;
  /* skip PrevSignatureLo */
  r += 3;
  /* skip challenge */
  r += 2;
  /* Balance */
  {
    DWORD t;

    /* Sign extension */
    if (get_sv_resp[r] & 0x80) t = 0x000000FF; else t = 0x00000000; t <<= 8;

    /* Actual data */
    t |= get_sv_resp[r++]; t <<= 8;
    t |= get_sv_resp[r++]; t <<= 8;
    t |= get_sv_resp[r++];

    l_target.CurrentBalance = (signed long) t;
  }

  if (l_target._IsReload)
  {
    l_target.ReloadOrDebit.Date  = get_sv_resp[r++]; l_target.ReloadOrDebit.Date <<= 8;
    l_target.ReloadOrDebit.Date |= get_sv_resp[r++]; 

    l_target.Reload.Free1 = get_sv_resp[r++];

    l_target.ReloadOrDebit.KVC |= get_sv_resp[r++]; 

    l_target.Reload.Free2 = get_sv_resp[r++];

    /* Load balance */
    {
      DWORD t;

      /* Sign extension */
      if (get_sv_resp[r] & 0x80) t = 0x000000FF; else t = 0x00000000; t <<= 8;    

      /* Actual data */
      t |= get_sv_resp[r++]; t <<= 8;
      t |= get_sv_resp[r++]; t <<= 8;
      t |= get_sv_resp[r++];

      l_target.ReloadOrDebit.Balance = (signed long) t;
    }

    /* Load Amount */
    {
      DWORD t;

      /* Sign extension */
      if (get_sv_resp[r] & 0x80) t = 0x000000FF; else t = 0x00000000; t <<= 8;    

      /* Actual data */
      t |= get_sv_resp[r++]; t <<= 8;
      t |= get_sv_resp[r++]; t <<= 8;
      t |= get_sv_resp[r++];

      l_target.ReloadOrDebit.Amount = (signed long) t;
    }

    l_target.ReloadOrDebit.Time  = get_sv_resp[r++]; l_target.ReloadOrDebit.Date <<= 8;
    l_target.ReloadOrDebit.Time |= get_sv_resp[r++]; 

    /* SamId */
    memcpy(l_target.ReloadOrDebit.SamId, &get_sv_resp[r], 4);
    r += 4;

    /* skip SamTransNum */
    r += 3;

    /* skip SvTransNum */
    r += 2;

  } else
  if (l_target._IsDebit)
  {
    /* Debit Amount */
    {
      DWORD t;

      /* Sign extension */
      if (get_sv_resp[r] & 0x80) t = 0x0000FFFF; else t = 0x00000000; t <<= 16;

      /* Actual data */
      t |= get_sv_resp[r++]; t <<= 8;
      t |= get_sv_resp[r++];

      l_target.ReloadOrDebit.Amount = (signed long) t;
    }

    l_target.ReloadOrDebit.Date  = get_sv_resp[r++]; l_target.ReloadOrDebit.Date <<= 8;
    l_target.ReloadOrDebit.Date |= get_sv_resp[r++]; 

    l_target.ReloadOrDebit.Time  = get_sv_resp[r++]; l_target.ReloadOrDebit.Date <<= 8;
    l_target.ReloadOrDebit.Time |= get_sv_resp[r++]; 

    l_target.ReloadOrDebit.KVC |= get_sv_resp[r++]; 

    /* SamId */
    memcpy(l_target.ReloadOrDebit.SamId, &get_sv_resp[r], 4);
    r += 4;

    /* skip SamTransNum */
    r += 3;

    /* Debit balance */
    {
      DWORD t;

      /* Sign extension */
      if (get_sv_resp[r] & 0x80) t = 0x000000FF; else t = 0x00000000; t <<= 8;    

      /* Actual data */
      t |= get_sv_resp[r++]; t <<= 8;
      t |= get_sv_resp[r++]; t <<= 8;
      t |= get_sv_resp[r++];

      l_target.ReloadOrDebit.Balance = (signed long) t;
    }

    /* skip SvTransNum */
    r += 2;
  }

  if (l_target._HasSerialNumber)
  {
    memcpy(l_target.SerialNumber, &get_sv_resp[r], 8);
    r += 8;
  }

  if (balance != NULL)
    *balance = l_target.CurrentBalance;

  if (target != NULL)
    memcpy(target, &l_target, sizeof(CALYPSO_STOREDVALUE_ST));

  return 0;
}

CALYPSO_PROC CalypsoCardStoredValueGetEx(CALYPSO_CTX_ST *ctx, BYTE type, BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len;
  BYTE l_e = 0x00;
  BOOL first_time = TRUE;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVGet");

  switch (type)
  {
    case CALYPSO_SV_RELOAD_TYPE : l_e = CALYPSO_SV_RELOAD_GET_LE_NO_UID; break;
    case CALYPSO_SV_DEBIT_TYPE  : l_e = CALYPSO_SV_DEBIT_GET_LE_NO_UID; break;
  }

again:

  send_len = 0;
  ctx->Card.Buffer[send_len++] = (ctx->Card.CLA == 0x94) ? 0xFA : ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_SV_GET;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = type;
  ctx->Card.Buffer[send_len++] = l_e;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  if (first_time && ((ctx->Card.SW & 0xFF00) == 0x6C00))
  {
    /* Le value incorrect, retry with the specified Le */
    first_time = FALSE;
    l_e = (BYTE) (ctx->Card.SW & 0x00FF);
    goto again;
  }

  if ((ctx->Card.SW & 0xFF00) == 0x6100)
  {
    /* Get Response */
    recv_len = sizeof(ctx->Card.Buffer);
    rc = CalypsoCardGetResponse(ctx, &recv_len);
    if (rc) goto done;
  }

  switch (ctx->Card.SW)
  {
    case 0x9000 : break;

    case 0x6985 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6A81 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;
    case 0x6A86 : rc = CALYPSO_ERR_SW_WRONG_P3; break;

    case 0x6D00 : 
    case 0x6E00 : rc = CALYPSO_CARD_NOT_SUPPORTED; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }
  if (rc) goto done;

  CalypsoTraceHex(TR_TRACE|TR_TRANS, "Resp=", ctx->Card.Buffer, recv_len);

  recv_len -= 2;

  memcpy(ctx->CardStoredValue.GetData, ctx->Card.Buffer, recv_len);
  ctx->CardStoredValue.GetDataSize = recv_len;
  
  if ((resp != NULL) && (respsize != NULL) && (*respsize != 0) && (*respsize < recv_len))
  {
    *respsize = recv_len;
    rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
    goto done;
  }

  if (resp != NULL)
    memcpy(resp, ctx->Card.Buffer, recv_len);

  if (respsize != NULL)
    *respsize = recv_len;

done:
  RETURN("SVGet");
}

CALYPSO_PROC CalypsoCardStoredValueGetForDebit(CALYPSO_CTX_ST *ctx, BYTE resp[], SIZE_T *respsize)
{
  return CalypsoCardStoredValueGetEx(ctx, CALYPSO_SV_DEBIT_TYPE, resp, respsize);
}

CALYPSO_PROC CalypsoCardStoredValueGetForReload(CALYPSO_CTX_ST *ctx, BYTE resp[], SIZE_T *respsize)
{
  return CalypsoCardStoredValueGetEx(ctx, CALYPSO_SV_RELOAD_TYPE, resp, respsize);
}

CALYPSO_PROC CalypsoCardStoredValueDebit(CALYPSO_CTX_ST *ctx, const BYTE get_sv_resp[], SIZE_T get_sv_respsize, signed long amount, WORD pdate, WORD ptime, BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, card_send_len, sam_send_len;
  BYTE i;

  BYTE kvc;
  BYTE sam_chal[3];
  BYTE sam_tnum[3];
  BYTE sam_sign[5];

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (get_sv_resp == NULL)
  {
    if (get_sv_respsize != 0)
      return CALYPSO_ERR_INVALID_PARAM;

    get_sv_resp     = ctx->CardStoredValue.GetData;
    get_sv_respsize = ctx->CardStoredValue.GetDataSize;
  }
  if ((get_sv_respsize != CALYPSO_SV_DEBIT_GET_LE_NO_UID)
   && (get_sv_respsize != CALYPSO_SV_DEBIT_GET_LE____UID)) return CALYPSO_ERR_INVALID_PARAM;

  CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVDebit");

  /* Retrieve the data provided by the card */
  /* -------------------------------------- */

  kvc = get_sv_resp[0];

  /* Prepare the card command (to be send later) */
  /* ------------------------------------------- */

  memset(ctx->Card.Buffer, 0, sizeof(ctx->Card.Buffer));

  card_send_len = 0;
  ctx->Card.Buffer[card_send_len++] = (ctx->Card.CLA == 0x94) ? 0xFA : ctx->Card.CLA;
  ctx->Card.Buffer[card_send_len++] = CALYPSO_INS_SV_DEBIT;
  ctx->Card.Buffer[card_send_len++] = 0x00; /* P1 : Dummy chal */
  ctx->Card.Buffer[card_send_len++] = 0x00; /* P2 : Dummy chal */
  ctx->Card.Buffer[card_send_len++] = CALYPSO_SV_DEBIT_CARD_LC;
  ctx->Card.Buffer[card_send_len++] = 0x00; /* P3 : Dummy chal */

  /* Amount */
  {
    WORD t = (WORD) amount;
    ctx->Card.Buffer[card_send_len++] = (BYTE) (t >> 8);
    ctx->Card.Buffer[card_send_len++] = (BYTE)  t;
  }
  /* Date */
  {
    WORD t = pdate;
    ctx->Card.Buffer[card_send_len++] = (BYTE) (t >> 8);
    ctx->Card.Buffer[card_send_len++] = (BYTE)  t;
  }
  /* Time */
  {
    WORD t = ptime;
    ctx->Card.Buffer[card_send_len++] = (BYTE) (t >> 8);
    ctx->Card.Buffer[card_send_len++] = (BYTE)  t;
  }
  ctx->Card.Buffer[card_send_len++] = kvc; /* KVC */

  /* Generate the SAM command */
  /* ------------------------ */

  sam_send_len = 0;
  /* Header */
  ctx->Sam.Buffer[sam_send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_INS_SAM_SV_DEBIT;
  ctx->Sam.Buffer[sam_send_len++] = 0x00;
  ctx->Sam.Buffer[sam_send_len++] = 0xFF;
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_SV_DEBIT_SAM_LC;

  /* Rebuild last card C-APDU in the incoming data */
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_INS_SV_GET;              /* INS */
  ctx->Sam.Buffer[sam_send_len++] = 0x00;                            /* P1 */
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_SV_DEBIT_TYPE;           /* P2 */
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_SV_DEBIT_GET_LE_NO_UID;  /* Le */

  /* Rebuild last card R-APDU in the incoming data */
  memcpy(&ctx->Sam.Buffer[sam_send_len], get_sv_resp, CALYPSO_SV_DEBIT_GET_LE_NO_UID);
  sam_send_len += CALYPSO_SV_DEBIT_GET_LE_NO_UID;
  ctx->Sam.Buffer[sam_send_len++] = 0x90;                            /* SW1 */
  ctx->Sam.Buffer[sam_send_len++] = 0x00;                            /* SW2 */

  /* Prepare next card C-APDU in the incoming data */
  memcpy(&ctx->Sam.Buffer[sam_send_len], &ctx->Card.Buffer[1], card_send_len-1);
  sam_send_len += card_send_len-1;

  /* Check that there's no mistake... */
  if (sam_send_len != (SIZE_T) (ctx->Sam.Buffer[4] + 5))
  {
    /* Oups */
    CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVDebit format error in SAM command");
    rc = CALYPSO_ERR_INTERNAL_ERROR;
    goto done;
  }

  /* Send the command to the SAM and check its answer */
  /* ------------------------------------------------ */

  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, sam_send_len, ctx->Sam.Buffer, &recv_len);
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
    if (recv_len != (2+ CALYPSO_SV_DEBIT_SAM_LE))
    {
      CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVDebit length error in SAM response");
      rc = CALYPSO_ERR_RESPONSE_SIZE;
      goto done;
    }
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_IS_LOCKED; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
    goto done;
  }

  /* Decode the data */
  memcpy(sam_chal, &ctx->Sam.Buffer[0], 3);
  memcpy(sam_tnum, &ctx->Sam.Buffer[3], 3);
  memcpy(sam_sign, &ctx->Sam.Buffer[6], 5);

  /* Complete the card command */
  /* ------------------------- */

  /* Put challenge in P1, P2 and first byte of data */
  ctx->Card.Buffer[2] = sam_chal[0]; /* P1 : Actual chal */
  ctx->Card.Buffer[3] = sam_chal[1]; /* P3 : Actual chal */
  ctx->Card.Buffer[5] = sam_chal[2]; /*      Actual chal */

  /* Append SAM's ID to the data */
  for (i=0; i<4; i++)
  {
    ctx->Card.Buffer[card_send_len++] = ctx->SamApplication.UID[i];
  }

  /* Append SAM's transaction number to the data */
  for (i=0; i<3; i++)
  {
    ctx->Card.Buffer[card_send_len++] = sam_tnum[i];
  }

  /* Append SAM's signature to the data */
  for (i=0; i<5; i++)
  {
    ctx->Card.Buffer[card_send_len++] = sam_sign[i];
  }

  /* Check that there's no mistake... */
  if (card_send_len != (SIZE_T) (ctx->Card.Buffer[4] + 5))
  {
    /* Oups */
    CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVDebit format error in card command");
    rc = CALYPSO_ERR_INTERNAL_ERROR;
    goto done;
  }
  
  /* Send the command to the card and check its answer */
  /* ------------------------------------------------- */

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, card_send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;
  
  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : 
    case 0x6200 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;
    
    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6900 : rc = CALYPSO_CARD_FILE_OVERFLOW; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6988 : rc = CALYPSO_CARD_DENIED_SAM_SIGN; break;
    case 0x6D00 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }
  if (rc) goto done;

  recv_len -= 2;
  
  if ((resp != NULL) && (respsize != NULL) && (*respsize != 0) && (*respsize < recv_len))
  {
    *respsize = recv_len;
    rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
    goto done;
  }

  if (resp != NULL)
    memcpy(resp, ctx->Card.Buffer, recv_len);

  if (respsize != NULL)
    *respsize = recv_len;

done:
  if (rc) CalypsoTraceRC(TR_DEBUG|TR_TRACE|TR_TRANS, "SVDebit err.", rc);
  return rc;
}

CALYPSO_PROC CalypsoCardStoredValueReload(CALYPSO_CTX_ST *ctx, const BYTE get_sv_resp[], SIZE_T get_sv_respsize, signed long amount, WORD pdate, WORD ptime, BYTE pfree[2], BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, card_send_len, sam_send_len;
  BYTE i;

  BYTE kvc;
  BYTE sam_chal[3];
  BYTE sam_tnum[3];
  BYTE sam_sign[5];

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (get_sv_resp == NULL)
  {
    if (get_sv_respsize != 0)
      return CALYPSO_ERR_INVALID_PARAM;

    get_sv_resp     = ctx->CardStoredValue.GetData;
    get_sv_respsize = ctx->CardStoredValue.GetDataSize;
  }
  if ((get_sv_respsize != CALYPSO_SV_RELOAD_GET_LE_NO_UID)
   && (get_sv_respsize != CALYPSO_SV_RELOAD_GET_LE____UID)) return CALYPSO_ERR_INVALID_PARAM;

  CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVReload");

  /* Retrieve the data provided by the card */
  /* -------------------------------------- */

  kvc = get_sv_resp[0];

  /* Prepare the card command (to be send later) */
  /* ------------------------------------------- */

  memset(ctx->Card.Buffer, 0, sizeof(ctx->Card.Buffer));

  card_send_len = 0;
  ctx->Card.Buffer[card_send_len++] = (ctx->Card.CLA == 0x94) ? 0xFA : ctx->Card.CLA;
  ctx->Card.Buffer[card_send_len++] = CALYPSO_INS_SV_RELOAD;
  ctx->Card.Buffer[card_send_len++] = 0x00; /* P1 : Dummy chal */
  ctx->Card.Buffer[card_send_len++] = 0x00; /* P2 : Dummy chal */
  ctx->Card.Buffer[card_send_len++] = CALYPSO_SV_RELOAD_CARD_LC;
  ctx->Card.Buffer[card_send_len++] = 0x00; /*      Dummy chal */

  /* Date */
  {
    WORD t = pdate;
    ctx->Card.Buffer[card_send_len++] = (BYTE) (t >> 8);
    ctx->Card.Buffer[card_send_len++] = (BYTE)  t;
  }

  /* Free 1 */
  if (pfree != NULL)
    ctx->Card.Buffer[card_send_len++] = pfree[0];
  else
    ctx->Card.Buffer[card_send_len++] = 0x00;

  /* KVC */
  ctx->Card.Buffer[card_send_len++] = kvc;

  /* Free 2 */
  if (pfree != NULL)
    ctx->Card.Buffer[card_send_len++] = pfree[1];
  else
    ctx->Card.Buffer[card_send_len++] = 0x00;

  /* Amount */
  {
    DWORD t = (DWORD) amount;
    ctx->Card.Buffer[card_send_len++] = (BYTE) (t >> 16);
    ctx->Card.Buffer[card_send_len++] = (BYTE) (t >> 8);
    ctx->Card.Buffer[card_send_len++] = (BYTE)  t;
  }

  /* Time */
  {
    WORD t = ptime;
    ctx->Card.Buffer[card_send_len++] = (BYTE) (t >> 8);
    ctx->Card.Buffer[card_send_len++] = (BYTE)  t;
  }


  /* Generate the SAM command */
  /* ------------------------ */

  sam_send_len = 0;
  /* Header */
  ctx->Sam.Buffer[sam_send_len++] = ctx->Sam.CLA;
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_INS_SAM_SV_RELOAD;
  ctx->Sam.Buffer[sam_send_len++] = 0x00;
  ctx->Sam.Buffer[sam_send_len++] = 0xFF;
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_SV_RELOAD_SAM_LC;

  /* Rebuild last card C-APDU in the incoming data */
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_INS_SV_GET;              /* INS */
  ctx->Sam.Buffer[sam_send_len++] = 0x00;                            /* P1 */
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_SV_RELOAD_TYPE;          /* P2 */
  ctx->Sam.Buffer[sam_send_len++] = CALYPSO_SV_RELOAD_GET_LE_NO_UID; /* Le */

  /* Rebuild last card R-APDU in the incoming data */
  memcpy(&ctx->Sam.Buffer[sam_send_len], get_sv_resp, CALYPSO_SV_RELOAD_GET_LE_NO_UID);
  sam_send_len += CALYPSO_SV_RELOAD_GET_LE_NO_UID;
  ctx->Sam.Buffer[sam_send_len++] = 0x90;                            /* SW1 */
  ctx->Sam.Buffer[sam_send_len++] = 0x00;                            /* SW2 */

  /* Prepare next card C-APDU in the incoming data */
  memcpy(&ctx->Sam.Buffer[sam_send_len], &ctx->Card.Buffer[1], card_send_len-1);
  sam_send_len += card_send_len-1;

  /* Check that there's no mistake... */
  if (sam_send_len != (SIZE_T) (ctx->Sam.Buffer[4] + 5))
  {
    /* Oups */
    CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVReload format error in SAM command");
    rc = CALYPSO_ERR_INTERNAL_ERROR;
    goto done;
  }

  /* Send the command to the SAM and check its answer */
  /* ------------------------------------------------ */

  recv_len = sizeof(ctx->Sam.Buffer);
  rc = CalypsoSamTransmit(ctx, ctx->Sam.Buffer, sam_send_len, ctx->Sam.Buffer, &recv_len);
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
    if (recv_len != (2+ CALYPSO_SV_RELOAD_SAM_LE))
    {
      CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVReload length error in SAM response");
      rc = CALYPSO_ERR_RESPONSE_SIZE;
      goto done;
    }
  } else
  {
    switch (ctx->Sam.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      case 0x6985 : rc = CALYPSO_SAM_IS_LOCKED; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
    goto done;
  }

  /* Decode the data */
  memcpy(sam_chal, &ctx->Sam.Buffer[0], 3);
  memcpy(sam_tnum, &ctx->Sam.Buffer[3], 3);
  memcpy(sam_sign, &ctx->Sam.Buffer[6], 5);

  /* Complete the card command */
  /* ------------------------- */

  /* Put challenge in P1, P2 and first byte of data */
  ctx->Card.Buffer[2] = sam_chal[0]; /* P1 : Actual chal */
  ctx->Card.Buffer[3] = sam_chal[1]; /* P3 : Actual chal */
  ctx->Card.Buffer[5] = sam_chal[2]; /*      Actual chal */

  /* Append SAM's ID to the data */
  for (i=0; i<4; i++)
  {
    ctx->Card.Buffer[card_send_len++] = ctx->SamApplication.UID[i];
  }

  /* Append SAM's transaction number to the data */
  for (i=0; i<3; i++)
  {
    ctx->Card.Buffer[card_send_len++] = sam_tnum[i];
  }

  /* Append SAM's signature to the data */
  for (i=0; i<5; i++)
  {
    ctx->Card.Buffer[card_send_len++] = sam_sign[i];
  }

  /* Check that there's no mistake... */
  if (card_send_len != (SIZE_T) (ctx->Card.Buffer[4] + 5))
  {
    /* Oups */
    CalypsoTraceStr(TR_TRACE|TR_TRANS, "SVReload format error in card command");
    rc = CALYPSO_ERR_INTERNAL_ERROR;
    goto done;
  }
  
  /* Send the command to the card and check its answer */
  /* ------------------------------------------------- */

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, card_send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;
  
  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : 
    case 0x6200 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;
    
    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6900 : rc = CALYPSO_CARD_FILE_OVERFLOW; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6988 : rc = CALYPSO_CARD_DENIED_SAM_SIGN; break;
    case 0x6D00 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }
  if (rc) goto done;
  
  recv_len -= 2;
  
  if ((resp != NULL) && (respsize != NULL) && (*respsize != 0) && (*respsize < recv_len))
  {
    *respsize = recv_len;
    rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
    goto done;
  }

  if (resp != NULL)
    memcpy(resp, ctx->Card.Buffer, recv_len);

  if (respsize != NULL)
    *respsize = recv_len;


done:
  if (rc) CalypsoTraceRC(TR_DEBUG|TR_TRACE|TR_TRANS, "SVReload err.", rc);
  return rc;
}

#endif
