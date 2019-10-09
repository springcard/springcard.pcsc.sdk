/**h* CalypsoAPI/calypso_card_read.c
 *
 * NAME
 *   SpringCard Calypso API :: Card command set (files and application selection + reading)
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

/*
 **********************************************************************************************************************
 *
 * FILES AND APPLICATIONS SELECTION
 *
 **********************************************************************************************************************
 */

CALYPSO_PROC CalypsoCardSelectFileEx(CALYPSO_CTX_ST *ctx, BYTE apdu_p1, BYTE apdu_p2, const BYTE file_path[], SIZE_T file_path_len, BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;
 
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (file_path_len > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_INVALID_PARAM;
  
#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_select++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "SelectFile");
  CalypsoTraceValH(TR_TRACE|TR_CARD, "CLA=", ctx->Card.CLA, 2);
  CalypsoTraceValH(TR_TRACE|TR_CARD, "P1=", apdu_p1, 2);
  CalypsoTraceValH(TR_TRACE|TR_CARD, "P2=", apdu_p2, 2);
  CalypsoTraceHex(TR_TRACE|TR_CARD, "Data=", file_path, file_path_len);

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_SELECT;
  ctx->Card.Buffer[send_len++] = apdu_p1;
  ctx->Card.Buffer[send_len++] = apdu_p2;
  ctx->Card.Buffer[send_len++] = (BYTE) file_path_len;
  memcpy(&ctx->Card.Buffer[send_len], file_path, file_path_len);
  send_len += file_path_len;
  
  if (ctx->Card.T1)
    ctx->Card.Buffer[send_len++] = 0x00; /* New 2017/10/11 : ensure Le is present */

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : break;
    case 0x6283 : ctx->CardApplication.Invalidated = TRUE; break; /* Invalidated - let's accept it anyway, to be able to revalidate */

    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;

    default     : if ((ctx->Card.SW & 0xFF00) == 0x6100)
                    break;
                  rc = CALYPSO_ERR_STATUS_WORD;
  }
  if (rc) goto done;

  if (recv_len == 2)
  {
    /* Get Response */
    recv_len = sizeof(ctx->Card.Buffer);
    rc = CalypsoCardGetResponse(ctx, &recv_len);
    if (rc) goto done;
  }

  CalypsoTraceHex(TR_TRACE|TR_CARD, "Resp=", ctx->Card.Buffer, recv_len);

  recv_len -= 2;
  
  if ((resp != NULL) && (respsize != NULL) && (*respsize != 0) && (*respsize < recv_len))
  {
    rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
    goto done;
  }

  if (resp != NULL)
    memcpy(resp, ctx->Card.Buffer, recv_len);

  if (respsize != NULL)
    *respsize = recv_len;

done:
  if (rc != CALYPSO_SUCCESS)
    if (respsize != NULL)
      *respsize = 0;
    
  RETURN("SelectFileEx");
}

/**f* SpringProxINS/CalypsoCardSelectApplication
 *
 * NAME
 *   CalypsoCardSelectApplication
 *
 * DESCRIPTION
 *   Select the Calypso application
 *
 * WARNING
 *   This function is not supported by Rev.1 cards
 *
 * INPUTS
 *   CALYPSO_CTX_ST  *ctx     : library context
 *   const BYTE      aid[]    : AID of the application to select, possibly right truncated
 *   SIZE_T      aidsize  : length of the AID
 *   BYTE            fci[]    : buffer to receive the FCI of the selected application
 *   SIZE_T      *fcisize : input  = size of the FCI buffer
 *                              output = actual length of the FCI
 *
 * RETURNS
 *   CALYPSO_RC               : 0 or an error code
 *
 * NOTES
 *   If aid is NULL, the function will be performed using "1TIC.ICA" as default AID.
 *   On success, the FCI is parsed, you may access its content using
 *   - CalypsoCardRevision
 *   - CalypsoCardDFName
 *   - CalypsoCardSerialNumber
 *   - CalypsoCardMaxSessionUpdates
 *
 **/
CALYPSO_PROC CalypsoCardSelectApplication(CALYPSO_CTX_ST *ctx, const BYTE aid[], SIZE_T aidsize, BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (aid == NULL)
  {
    aid = (const BYTE *) "1TIC.ICA";
    aidsize = 8;
  }
  
  CalypsoTraceStr(TR_TRACE|TR_CARD, "SelectApplication");
  CalypsoTraceHex(TR_TRACE|TR_CARD, "AID=", aid, aidsize);
  
  if ((resp == NULL) && (respsize == NULL))
  {
    resp      = ctx->LastCardSelectResp;
    respsize  = &ctx->LastCardSelectRespLen;
    *respsize = sizeof(ctx->LastCardSelectResp);
  }
  
  rc = CalypsoCardSelectFileEx(ctx, 0x04, 0x00, aid, aidsize, resp, respsize);

  if (rc == CALYPSO_SUCCESS)
  { 
    ctx->CardApplication.FciLen = *respsize;
    if (ctx->CardApplication.FciLen > sizeof(ctx->CardApplication.Fci))
      ctx->CardApplication.FciLen = sizeof(ctx->CardApplication.Fci);
    memcpy(ctx->CardApplication.Fci, resp, ctx->CardApplication.FciLen);
      
    CalypsoTraceHex(TR_TRACE|TR_CARD, "FCI=", ctx->CardApplication.Fci, ctx->CardApplication.FciLen);
  }

  RETURN("SelectApplication");
}

/**f* SpringProxINS/CalypsoCardSelectDF
 *
 * NAME
 *   CalypsoCardSelectDF
 *
 * DESCRIPTION
 *   Select a DF
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   WORD           file_id   : identifier of the file
 *   BYTE           resp[]    : buffer to receive the response to select
 *   SIZE_T     *respsize : input  = size of the response buffer
 *                              output = actual length of the response
 *
 * RETURNS
 *   CALYPSO_RC               : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardSelectDFEx(CALYPSO_CTX_ST *ctx, BYTE param1, WORD file_id, BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  BYTE file_path[2];

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  
  file_path[0]  = (BYTE) (file_id >> 8);
  file_path[1]  = (BYTE) (file_id);

  CalypsoTraceStr(TR_TRACE|TR_CARD, "SelectDF");
  CalypsoTraceHex(TR_TRACE|TR_CARD, "ID=", file_path, 2);
  
  if ((resp == NULL) && (respsize == NULL))
  {
    resp      = ctx->LastCardSelectResp;
    respsize  = &ctx->LastCardSelectRespLen;
    *respsize = sizeof(ctx->LastCardSelectResp);
  }
  
  rc = CalypsoCardSelectFileEx(ctx, param1, 0x00, file_path, 2, resp, respsize);
   
  return rc;
}

CALYPSO_PROC CalypsoCardSelectDF(CALYPSO_CTX_ST *ctx, WORD file_id, BYTE resp[], SIZE_T *respsize)
{
  return CalypsoCardSelectDFEx(ctx, 0x09, file_id, resp, respsize);
}

/**f* SpringProxINS/CalypsoCardSelectEF
 *
 * NAME
 *   CalypsoCardSelectEF
 *
 * DESCRIPTION
 *   Select an EF under current DF
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   WORD           file_id   : identifier of the file
 *   BYTE           resp[]    : buffer to receive the response to select
 *   SIZE_T     *respsize : input  = size of the response buffer
 *                              output = actual length of the response
 *
 * RETURNS
 *   CALYPSO_RC               : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardSelectEF(CALYPSO_CTX_ST *ctx, WORD file_id, BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  BYTE file_path[2];

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  file_path[0]  = (BYTE) (file_id >> 8);
  file_path[1]  = (BYTE) (file_id);

  if ((resp == NULL) && (respsize == NULL))
  {
    resp      = ctx->LastCardSelectResp;
    respsize  = &ctx->LastCardSelectRespLen;
    *respsize = sizeof(ctx->LastCardSelectResp);
  }

  rc = CalypsoCardSelectFileEx(ctx, 0x02, 0x00, file_path, 2, resp, respsize);

  return rc;
}

/**f* SpringProxINS/CalypsoCardSelectFile
 *
 * NAME
 *   CalypsoCardSelectFile
 *
 * DESCRIPTION
 *   Select an EF or a DF (or the MF)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   DWORD          file_id   : identifier of the file (see below)
 *   BYTE           resp[]    : buffer to receive the response to select
 *   SIZE_T     *respsize : input  = size of the response buffer
 *                              output = actual length of the response
 *
 * RETURNS
 *   CALYPSO_RC               : 0 or an error code
 *
 * NOTES
 *   file_id is a DWORD (4 bytes) value constructed as follow :
 *   - 0x0000XXXX : selects EF with ID XXXX under current DF
 *   - 0xYYYYXXXX : selects EF with ID XXXX under DF with ID YYYY
 *   - 0x00000000 : selects the Master File
 *
 **/
CALYPSO_PROC CalypsoCardSelectFile(CALYPSO_CTX_ST *ctx, DWORD file_id, BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  BYTE file_path[4];
  BYTE file_path_len = 0;
  BYTE apdu_p1;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  if (file_id & 0xFFFF0000)
  {
    file_path[0]  = (BYTE) (file_id >> 24);
    file_path[1]  = (BYTE) (file_id >> 16);
    file_path[2]  = (BYTE) (file_id >> 8);
    file_path[3]  = (BYTE) (file_id);
    file_path_len = 4;
    apdu_p1 = 0x08;

  } else
  if (file_id & 0x0000FFFF)
  {
    file_path[0]  = (BYTE) (file_id >> 8);
    file_path[1]  = (BYTE) (file_id);
    file_path_len = 2;
    apdu_p1 = 0x02;
  } else
  {
    file_path[0]  = 0x3F;
    file_path[1]  = 0x00;
    file_path_len = 2;
    apdu_p1 = 0x00;
  }

  if ((resp == NULL) && (respsize == NULL))
  {
    resp      = ctx->LastCardSelectResp;
    respsize  = &ctx->LastCardSelectRespLen;
    *respsize = sizeof(ctx->LastCardSelectResp);
  }

  rc = CalypsoCardSelectFileEx(ctx, apdu_p1, 0x00, file_path, file_path_len, resp, respsize);

  return rc;
}

/*
 **********************************************************************************************************************
 *
 * READ
 *
 **********************************************************************************************************************
 */

/**f* SpringProxINS/CalypsoCardReadBinary
 *
 * NAME
 *   CalypsoCardReadBinary
 *
 * DESCRIPTION
 *   Read bytes from an EF (must be a binary EF)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   BYTE           sfi       : identifier of the file (0 for current file)
 *   WORD           offset    : address of first byte
 *   BYTE           ask_size  : size to be read ('Le' parameter in APDU - may be 0)
 *   BYTE           data[]    : buffer to receive the data
 *   SIZE_T     *datasize : input  = size of the data buffer
 *                              output = actual length of the data
 *
 * RETURNS
 *   CALYPSO_RC            : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardReadBinary(P_CALYPSO_CTX ctx, BYTE sfi, WORD offset, BYTE ask_size, BYTE data[], SIZE_T *datasize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len;
  SIZE_T old_datasize = 0;
  BOOL first_time = TRUE;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_read++;
#endif

  if (datasize != NULL)
  {
    old_datasize = *datasize;
    *datasize = 0;
  }

  CalypsoTraceStr(TR_TRACE|TR_CARD, "ReadBinary");

again:

  send_len = 0;
  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_READ_BINARY;
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
  ctx->Card.Buffer[send_len++] = ask_size;

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  if (first_time && ((ctx->Card.SW & 0xFF00) == 0x6C00))
  {
    /* Le value incorrect, retry with the specified Le */
    first_time = FALSE;
    ask_size = (BYTE) (ctx->Card.SW & 0x00FF);
    goto again;
  }

  switch (ctx->Card.SW)
  {
    case 0x9000 : break;

    case 0x6981 : rc = CALYPSO_CARD_WRONG_FILE_TYPE; break;
    case 0x6982 : rc = CALYPSO_CARD_ACCESS_DENIED; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6986 : rc = CALYPSO_CARD_FILE_NOT_SELECTED; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6A83 : rc = CALYPSO_CARD_FILE_OVERFLOW; break;
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

  if (rc) goto done;
  
  if ((data != NULL) && (datasize != NULL) && (old_datasize != 0) && (old_datasize < recv_len-2))
  {
    *datasize = recv_len-2;
    rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
    goto done;
  }
  if (data != NULL)
    memcpy(data, ctx->Card.Buffer, recv_len-2);
  if (datasize != NULL)
    *datasize = recv_len-2;

done:
  RETURN("ReadBinary");
}

/**f* SpringProxINS/CalypsoCardReadRecord
 *
 * NAME
 *   CalypsoCardReadRecord
 *
 * DESCRIPTION
 *   Read one record from the current EF (either cyclic or linear)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   BYTE           sfi       : identifier of the file (0 for current file)
 *   BYTE           rec_no    : identifier of the record
 *   BYTE           rec_size  : expected size of the record ('Le' parameter in APDU - may be 0)
 *   BYTE           data[]    : buffer to receive the data
 *   SIZE_T     *datasize : input  = size of the data buffer
 *                              output = actual length of the data
 *
 * RETURNS
 *   CALYPSO_RC               : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardReadRecord(CALYPSO_CTX_ST *ctx, BYTE sfi, BYTE rec_no, BYTE rec_size, BYTE data[], SIZE_T *datasize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len;
  SIZE_T old_datasize = 0;
  BOOL first_time = TRUE;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_read++;
#endif

  if (datasize != NULL)
  {
    old_datasize = *datasize;
    *datasize = 0;
  }

  CalypsoTraceStr(TR_TRACE|TR_CARD, "ReadRecord");

again:

  send_len = 0;
  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_READ_RECORD;
  ctx->Card.Buffer[send_len++] = rec_no;
  ctx->Card.Buffer[send_len++] = (sfi * 8) + 4;
  ctx->Card.Buffer[send_len++] = rec_size;

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  if (first_time && ((ctx->Card.SW & 0xFF00) == 0x6C00))
  {
    /* Le value incorrect, retry with the specified Le */
    first_time = FALSE;
    rec_size = (BYTE) (ctx->Card.SW & 0x00FF);
    goto again;
  }

  switch (ctx->Card.SW)
  {
    case 0x9000 : break;

    case 0x6981 : rc = CALYPSO_CARD_WRONG_FILE_TYPE; break;
    case 0x6982 : rc = CALYPSO_CARD_ACCESS_DENIED; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6986 : rc = CALYPSO_CARD_FILE_NOT_SELECTED; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6A83 : rc = CALYPSO_CARD_FILE_OVERFLOW; break;
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

  if (rc) goto done;
  
  if ((data != NULL) && (datasize != NULL) && (old_datasize != 0) && (old_datasize < recv_len-2))
  {
    *datasize = recv_len-2;
    rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
    goto done;
  }
  if (data != NULL)
    memcpy(data, ctx->Card.Buffer, recv_len-2);
  if (datasize != NULL)
    *datasize = recv_len-2;

done:
  RETURN("ReadRecord");
}
