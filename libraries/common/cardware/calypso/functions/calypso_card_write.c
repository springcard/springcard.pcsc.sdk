/**h* CalypsoAPI/calypso_card_write.c
 *
 * NAME
 *   SpringCard Calypso API :: Card command set (write / append / update / increase / decrease)
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release
 *   JDA 15/11/2009 : implemented increase / decrease
 *
 **/
#include "../calypso_api_i.h"
#include "calypso_card_commands_i.h"

/*
 **********************************************************************************************************************
 *
 * WRITE / APPEND / UPDATE
 *
 **********************************************************************************************************************
 */

/**f* SpringProxINS/CalypsoCardAppendRecord
 *
 * NAME
 *   CalypsoCardAppendRecord
 *
 * DESCRIPTION
 *   Add one record into the current EF (must be a cyclic file)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx     : library context
 *   BYTE           sfi      : identifier of the file (0 for current file)
 *   const BYTE     data[]   : data to be written in the record
 *   BYTE           datasize : size of the record
 *
 * RETURNS
 *   CALYPSO_RC              : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardAppendRecord(CALYPSO_CTX_ST *ctx, BYTE sfi, const BYTE data[], BYTE datasize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len = 2;
  SIZE_T send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if ((data == NULL) && (datasize > 0)) return CALYPSO_ERR_INVALID_PARAM;
#if (CALYPSO_MAX_DATA_SZ < 256)  
  if (datasize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_COMMAND_OVERFLOW;
#endif

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "AppendRecord");

  if (ctx->CardApplication.SessionActive && (ctx->CardApplication.SessionCurMods >= ctx->CardApplication.SessionMaxMods))
  {
    CalypsoTraceStr(TR_ERROR|TR_CARD, "AppendRecord: Transaction limit");
  }

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_APPEND_RECORD;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = sfi * 8;
  ctx->Card.Buffer[send_len++] = datasize;
  if (data != NULL)
    memcpy(&ctx->Card.Buffer[send_len], data, datasize);
  send_len += datasize;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;

    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6981 : rc = CALYPSO_CARD_WRONG_FILE_TYPE; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6986 : rc = CALYPSO_CARD_FILE_NOT_SELECTED; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("AppendRecord");
}

/**f* SpringProxINS/CalypsoCardUpdateBinary
 *
 * NAME
 *   CalypsoCardUpdateBinary
 *
 * DESCRIPTION
 *   Write bytes into an EF (must be a binary EF)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx   : library context
 *   BYTE           sfi    : identifier of the file (0 for current file)
 *   WORD           offset : address of first byte
 *   const BYTE     data[] : data to be written
 *   BYTE           length : number of bytes
 *
 * RETURNS
 *   CALYPSO_RC            : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardUpdateBinary(CALYPSO_CTX_ST *ctx, BYTE sfi, WORD offset, const BYTE data[], BYTE length)
{
  CALYPSO_RC rc;
  SIZE_T recv_len = 2;
  SIZE_T send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if ((data == NULL) && (length > 0)) return CALYPSO_ERR_INVALID_PARAM;
#if (CALYPSO_MAX_DATA_SZ < 256)
  if (length > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_COMMAND_OVERFLOW;
#endif

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "UpdateBinary");

  if (ctx->CardApplication.SessionActive && (ctx->CardApplication.SessionCurMods >= ctx->CardApplication.SessionMaxMods))
  {
    CalypsoTraceStr(TR_ERROR|TR_CARD, "UpdateBinary: Transaction limit");
  }

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_UPDATE_BINARY;
  if ((sfi != 0x00) && (offset < 0x0100))
  {
    ctx->Card.Buffer[send_len++] = 0x80 | sfi;
  } else
  if ((sfi == 0x00) && (offset < 0x8000))
  {
    ctx->Card.Buffer[send_len++] = offset / 0x0100;
  } else
  {
    rc = CALYPSO_ERR_INVALID_PARAM;
    goto done;
  }
  ctx->Card.Buffer[send_len++] = offset % 0x0100;
  ctx->Card.Buffer[send_len++] = length;
  if (data != NULL)
    memcpy(&ctx->Card.Buffer[send_len], data, length);
  send_len += length;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;

    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6981 : rc = CALYPSO_CARD_WRONG_FILE_TYPE; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6986 : rc = CALYPSO_CARD_FILE_NOT_SELECTED; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6A83 : rc = CALYPSO_CARD_FILE_OVERFLOW; break;
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("UpdateBinary");
}


/**f* SpringProxINS/CalypsoCardUpdateRecord
 *
 * NAME
 *   CalypsoCardUpdateRecord
 *
 * DESCRIPTION
 *   Update a record in the current EF
 *
 * WARNING
 *   On a cyclic file, it is only possible to replace the most recent record (rec_no = 1)
 *   On a linear file, there's no limitation
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx     : library context
 *   BYTE           sfi      : identifier of the file (0 for current file)
 *   BYTE           rec_no   : identifier of the record
 *   const BYTE     data[]   : data to be written in the recoed
 *   BYTE           datasize : size of the record
 *
 * RETURNS
 *   CALYPSO_RC              : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardUpdateRecord(CALYPSO_CTX_ST *ctx, BYTE sfi, BYTE rec_no, const BYTE data[], BYTE datasize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len = 2;
  SIZE_T send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if ((data == NULL) && (datasize > 0)) return CALYPSO_ERR_INVALID_PARAM;
#if (CALYPSO_MAX_DATA_SZ < 256)
  if (datasize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_COMMAND_OVERFLOW;
#endif

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "UpdateRecord");

  if (ctx->CardApplication.SessionActive && (ctx->CardApplication.SessionCurMods >= ctx->CardApplication.SessionMaxMods))
  {
    CalypsoTraceStr(TR_ERROR|TR_CARD, "UpdateRecord: Transaction limit");
  }

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_UPDATE_RECORD;
  ctx->Card.Buffer[send_len++] = rec_no;
  ctx->Card.Buffer[send_len++] = (sfi * 8) + 4;
  ctx->Card.Buffer[send_len++] = (BYTE) datasize;
  if (data != NULL)
    memcpy(&ctx->Card.Buffer[send_len], data, datasize);
  send_len += datasize;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;

    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6981 : rc = CALYPSO_CARD_WRONG_FILE_TYPE; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6986 : rc = CALYPSO_CARD_FILE_NOT_SELECTED; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6A83 : rc = CALYPSO_CARD_FILE_OVERFLOW; break;
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("UpdateRecord");
}

/**f* SpringProxINS/CalypsoCardWriteRecord
 *
 * NAME
 *   CalypsoCardWriteRecord
 *
 * DESCRIPTION
 *   Write over a record in the current EF
 *
 * WARNING
 *   On a cyclic file, it is only possible to overwrite the most recent record (rec_no = 1)
 *   On a linear file, there's no limitation
 *
 * INPUTS
 *   CALYPSO_CTX_ST  *ctx     : library context
 *   BYTE            sfi      : identifier of the file (0 for current file)
 *   BYTE            rec_no   : identifier of the record
 *   const BYTE      data[]   : data to be overwritten over the record
 *   BYTE            datasize : size of the record
 *
 * RETURNS
 *   CALYPSO_RC               : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardWriteRecord(CALYPSO_CTX_ST *ctx, BYTE sfi, BYTE rec_no, const BYTE data[], BYTE datasize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len = 2;
  SIZE_T send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if ((data == NULL) && (datasize > 0)) return CALYPSO_ERR_INVALID_PARAM;
#if (CALYPSO_MAX_DATA_SZ < 256)
  if (datasize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_COMMAND_OVERFLOW;
#endif

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "WriteRecord");

  if (ctx->CardApplication.SessionActive && (ctx->CardApplication.SessionCurMods >= ctx->CardApplication.SessionMaxMods))
  {
    CalypsoTraceStr(TR_ERROR|TR_CARD, "WriteRecord: Transaction limit");
  }

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_WRITE_RECORD;
  ctx->Card.Buffer[send_len++] = rec_no;
  ctx->Card.Buffer[send_len++] = (sfi * 8) + 4;
  ctx->Card.Buffer[send_len++] = (BYTE) datasize;
  if (data != NULL)
    memcpy(&ctx->Card.Buffer[send_len], data, datasize);
  send_len += datasize;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;

    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6981 : rc = CALYPSO_CARD_WRONG_FILE_TYPE; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6986 : rc = CALYPSO_CARD_FILE_NOT_SELECTED; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6A83 : rc = CALYPSO_CARD_FILE_OVERFLOW; break;
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("WriteRecord");
}



/*
 **********************************************************************************************************************
 *
 * COUNTERS
 *
 **********************************************************************************************************************
 */


// TODO : doc
CALYPSO_PROC CalypsoCardDecrease(CALYPSO_CTX_ST *ctx, BYTE sfi, BYTE rec_no, DWORD decval, DWORD *newval)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 8;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "Decrease");

  if (ctx->CardApplication.SessionActive && (ctx->CardApplication.SessionCurMods >= ctx->CardApplication.SessionMaxMods))
  {
    CalypsoTraceStr(TR_ERROR|TR_CARD, "Decrease: Transaction limit");
  }

  ctx->Card.Buffer[0] = ctx->Card.CLA;
  ctx->Card.Buffer[1] = CALYPSO_INS_DECREASE;
  ctx->Card.Buffer[2] = rec_no;
  ctx->Card.Buffer[3] = (8 * sfi);
  ctx->Card.Buffer[4] = 3;

  ctx->Card.Buffer[7] = (BYTE) (decval & 0x0000FF); decval >>= 8;
  ctx->Card.Buffer[6] = (BYTE) (decval & 0x0000FF); decval >>= 8;
  ctx->Card.Buffer[5] = (BYTE) (decval & 0x0000FF);
  
  if (ctx->Card.T1)   
    ctx->Card.Buffer[send_len++] = 0x00; /* New 2017/10/11 : ensure Le is present */  

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;
  
  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;  
  
  if ((ctx->Card.SW & 0xFF00) == 0x6100)
  {
    /* Get Response */
    recv_len = sizeof(ctx->Card.Buffer);
    rc = CalypsoCardGetResponse(ctx, &recv_len);
    if (rc) goto done;  
  }

  switch (ctx->Card.SW)
  {
    case 0x9000 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;   

    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6981 : rc = CALYPSO_CARD_WRONG_FILE_TYPE; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6986 : rc = CALYPSO_CARD_FILE_NOT_SELECTED; break;
    case 0x6A80 : rc = CALYPSO_CARD_COUNTER_LIMIT; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }
  if (rc) goto done;
   
  if (recv_len < 5)  
  {
    rc = CALYPSO_CARD_DATA_MALFORMED;
    goto done;
  }
    
  if (newval != NULL)
  {
    DWORD t;
    t  = 0;
    t += ctx->Card.Buffer[0]; t <<= 8;
    t += ctx->Card.Buffer[1]; t <<= 8;
    t += ctx->Card.Buffer[2];
    *newval = t;
  }

done:
  RETURN("Decrease");
}

// TODO : doc
CALYPSO_PROC CalypsoCardIncrease(CALYPSO_CTX_ST *ctx, BYTE sfi, BYTE rec_no, DWORD incval, DWORD *newval)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 8;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "Increase");

  if (ctx->CardApplication.SessionActive && (ctx->CardApplication.SessionCurMods >= ctx->CardApplication.SessionMaxMods))
  {
    CalypsoTraceStr(TR_ERROR|TR_CARD, "Increase: Transaction limit");
  }

  ctx->Card.Buffer[0] = ctx->Card.CLA;
  ctx->Card.Buffer[1] = CALYPSO_INS_INCREASE;
  ctx->Card.Buffer[2] = rec_no;
  ctx->Card.Buffer[3] = (8 * sfi);
  ctx->Card.Buffer[4] = 3;

  ctx->Card.Buffer[7] = (BYTE) (incval & 0x0000FF); incval >>= 8;
  ctx->Card.Buffer[6] = (BYTE) (incval & 0x0000FF); incval >>= 8;
  ctx->Card.Buffer[5] = (BYTE) (incval & 0x0000FF);

  if (ctx->Card.T1)   
    ctx->Card.Buffer[send_len++] = 0x00; /* New 2017/10/11 : ensure Le is present */  
  
  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;
  
  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;  

  if ((ctx->Card.SW & 0xFF00) == 0x6100)
  {
    /* Get Response */
    recv_len = sizeof(ctx->Card.Buffer);
    rc = CalypsoCardGetResponse(ctx, &recv_len);
    if (rc) goto done;  
  }
  
  switch (ctx->Card.SW)
  {
    case 0x9000 : if (ctx->CardApplication.SessionActive) ctx->CardApplication.SessionCurMods++; break;   

    case 0x6400 : rc = CALYPSO_CARD_TRANSACTION_LIMIT; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6981 : rc = CALYPSO_CARD_WRONG_FILE_TYPE; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6986 : rc = CALYPSO_CARD_FILE_NOT_SELECTED; break;
    case 0x6A80 : rc = CALYPSO_CARD_COUNTER_LIMIT; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }
  if (rc) goto done;
   
  if (recv_len < 5)  
  {
    rc = CALYPSO_CARD_DATA_MALFORMED;
    goto done;
  }
    
  if (newval != NULL)
  {
    DWORD t;
    t  = 0;
    t += ctx->Card.Buffer[0]; t <<= 8;
    t += ctx->Card.Buffer[1]; t <<= 8;
    t += ctx->Card.Buffer[2];
    *newval = t;
  }

done:
  RETURN("Increase");
}

