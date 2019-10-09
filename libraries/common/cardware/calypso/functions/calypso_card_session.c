/**h* CalypsoAPI/calypso_card_session.c
 *
 * NAME
 *   SpringCard Calypso API :: Card command set (security and transactions)
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release
 *   JDA 04/01/2010 : implemented GetChallenge
 *
 **/
#include "../calypso_api_i.h"
#include "calypso_card_commands_i.h"

/*
 **********************************************************************************************************************
 *
 * SECURITY AND TRANSACTIONS
 *
 **********************************************************************************************************************
 */

/**f* SpringProxINS/CalypsoCardOpenSecureSessionEx
 *
 * NAME
 *   CalypsoCardOpenSecureSessionEx
 *
 * DESCRIPTION
 *   Open a secure session on the card, with user-defined parameters
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx        : library context
 *   BYTE           apdu_p1     : the P1 parameter
 *   BYTE           apdu_p2     : the P2 parameter
 *   BYTE           sam_chal[4] : the challenge provided by the Calypso SAM
 *                                     (see CalypsoSamSelectDiversifier)
 *   BYTE           resp[]      : buffer to receive the response
 *   BYTE           *respsize   : input  = size of the response buffer
 *                                    output = actual length of the response
 *                                             (EXCLUDING the status word that must be 9000)
 * 
 * RETURNS
 *   CALYPSO_RC                 : 0 or an error code
 *
 * NOTES
 *   This function is only a command sent to the card.
 *   The response is not interpreted by this function. It is up to the caller to
 *   1. check that the returned content is valid
 *   2. feed the SAM with card's challenge
 *   3. use CalypsoSamDigestUpdate to feed the SAM with future exchanges
 *   For an automated implementation, use CalypsoStartTransaction instead.
 *
 **/
CALYPSO_PROC CalypsoCardOpenSecureSessionEx(CALYPSO_CTX_ST *ctx, BYTE apdu_p1, BYTE apdu_p2, const BYTE sam_chal[4], BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (sam_chal == NULL) return CALYPSO_ERR_INVALID_PARAM;

  CalypsoTraceStr(TR_TRACE|TR_CARD, "OpenSecureSession");
  CalypsoTraceValH(TR_TRACE|TR_CARD, "P1=", apdu_p1, 2);
  CalypsoTraceValH(TR_TRACE|TR_CARD, "P2=", apdu_p2, 2);
  CalypsoTraceHex(TR_TRACE|TR_CARD, "S'Chal=", sam_chal, 4);

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_OPEN_SESSION;
  ctx->Card.Buffer[send_len++] = apdu_p1;
  ctx->Card.Buffer[send_len++] = apdu_p2;
  ctx->Card.Buffer[send_len++] = 4;
  memcpy(&ctx->Card.Buffer[send_len], sam_chal, 4);
  send_len += 4;
  
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

    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6400 : rc = CALYPSO_CARD_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6A81 : rc = CALYPSO_CARD_NO_SUCH_KEY; break;
    case 0x6A82 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;
    case 0x6A83 : rc = CALYPSO_CARD_NO_SUCH_KEY; break;

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

  if ((resp != NULL) && (respsize != NULL) && (*respsize != 0) && (*respsize < recv_len-2))
  {
    *respsize = recv_len-2;
    rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
    goto done;
  }
  if (resp != NULL)
    memcpy(resp, ctx->Card.Buffer, recv_len-2);
  if (respsize != NULL)
    *respsize = recv_len-2;

done:
  RETURN("OpenSecureSessionEx");
}


/**f* SpringProxINS/CalypsoCardOpenSecureSession1
 *
 * NAME
 *   CalypsoCardOpenSecureSession1
 *
 * DESCRIPTION
 *   Open a secure session, Revision 1 implementation
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx         : library context
 *   BYTE           resp[]       : buffer to receive the response
 *   SIZE_T     *respsize    : input  = size of the response buffer
 *                                 output = actual length of the response
 *                                          (EXCLUDING the status word that must be 9000)
 *   BYTE           key_no       : identifier of the cryptographic key
 *   BYTE           sfi          : identifier of the file to read (0 for current file)
 *   BYTE           rec_no       : identifier of the record to read (0 for none)
 *   const BYTE     sam_chal[4]  : challenge returned by the SAM (see CalypsoSamGetChallenge)
 *   BYTE           card_chal[4] : buffer to receive card's challenge
 *   BOOL           *ratified    : tells whether last card session was ratified or not
 *   BYTE           data[]       : buffer to receive record data (if some)
 *   SIZE_T     *datasize    : input  = size of the record data buffer
 *                                 output = actual length of record data
 * 
 * RETURNS
 *   CALYPSO_RC                  : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardOpenSecureSession1(CALYPSO_CTX_ST *ctx, BYTE resp[], SIZE_T *respsize, BYTE key_no, BYTE sfi, BYTE rec_no, const BYTE sam_chal[4], BYTE card_chal[4], BOOL *ratified, BYTE data[], SIZE_T *datasize)
{  
  BYTE apdu_p1, apdu_p2, rec_len;
  BYTE *rec_ptr;
  CALYPSO_RC rc;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if ((resp == NULL) || (respsize == NULL))
  {
  	rc = CALYPSO_ERR_INVALID_PARAM;
  	goto done;
  } 
  
  /* Command revision 2 version */
  apdu_p1 = (rec_no << 3) | key_no; /* Record number, key number, no KVC required */
  apdu_p2 = (sfi    << 3);          /* SFI */
  
  rc = CalypsoCardOpenSecureSessionEx(ctx, apdu_p1, apdu_p2, sam_chal, resp, respsize);
  if (rc) goto done;
  
  if (*respsize < 4)
  {
    rc = CALYPSO_ERR_RESPONSE_SIZE;
    goto done;
  }
  
  if (card_chal != NULL)
    memcpy(card_chal, &resp[0], 4);
  
  if (sfi || rec_no)
    rec_len = CALYPSO_MIN_RECORD_SIZE;
  else
    rec_len = 0;

  if (*respsize == (SIZE_T) (4+rec_len))
  {
    /* Ratified, KVC not returned */
    if (ratified != NULL)  *ratified = TRUE;
    rec_ptr = &resp[4];
  } else
  if (*respsize == (SIZE_T) (6+rec_len))
  {
    /* Not ratified, KVC not returned, 2 ratification bytes */
    if (ratified != NULL)  *ratified = FALSE;
    rec_ptr = &resp[6];
  } else
  if (*respsize == (SIZE_T) (8+rec_len))
  {
    /* Not ratified, KVC not returned, 4 ratification bytes */
    if (ratified != NULL)  *ratified = FALSE;
    rec_ptr = &resp[8];
  } else
  {
    rc = CALYPSO_CARD_DATA_MALFORMED;
    goto done;
  }
  
  if (datasize != NULL)
  {
    if ((*datasize != 0) && (*datasize < rec_len))
    {
      *datasize = rec_len;
      rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
      goto done;
    }
    *datasize = rec_len;
  }
  
  if ((data != NULL) && (rec_len))
    memcpy(data, rec_ptr, rec_len);
  
done:
  RETURN("OpenSecureSession1");  
}

/**f* SpringProxINS/CalypsoCardOpenSecureSession2
 *
 * NAME
 *   CalypsoCardOpenSecureSession2
 *
 * DESCRIPTION
 *   Open a secure session, Revision 2 implementation
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx         : library context
 *   BYTE           resp[]       : buffer to receive the response
 *   SIZE_T     *respsize    : input  = size of the response buffer
 *                                 output = actual length of the response
 *                                          (EXCLUDING the status word that must be 9000)
 *   BYTE           key_no       : identifier of the cryptographic key
 *   BYTE           sfi          : identifier of the file to read (0 for current file)
 *   BYTE           rec_no       : identifier of the record to read (0 for none)
 *   const BYTE     sam_chal[4]  : challenge returned by the SAM (see CalypsoSamGetChallenge)
 *   BYTE           card_chal[4] : buffer to receive card's challenge
 *   BOOL           *ratified    : tells whether last card session was ratified or not
 *   BYTE           data[]       : buffer to receive record data (if some)
 *   SIZE_T     *datasize    : input  = size of the record data buffer
 *                                 output = actual length of record data
 *   BYTE           *kvc         : KVC specified by the card according to key_no
 * 
 * RETURNS
 *   CALYPSO_RC                  : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardOpenSecureSession2(CALYPSO_CTX_ST *ctx, BYTE resp[], SIZE_T *respsize, BYTE key_no, BYTE sfi, BYTE rec_no, const BYTE sam_chal[4], BYTE card_chal[4], BOOL *ratified, BYTE data[], SIZE_T *datasize, BYTE *kvc)
{  
  BYTE apdu_p1, apdu_p2, rec_len;
  BYTE *rec_ptr;
  CALYPSO_RC rc;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if ((resp == NULL) || (respsize == NULL))
  {
  	rc = CALYPSO_ERR_INVALID_PARAM;
  	goto done;
  }

  /* Command revision 2 version */
  apdu_p1 = (rec_no << 3) | key_no | 0x80; /* Record number, key number, and KVC required */
  apdu_p2 = (sfi    << 3);                 /* SFI */
  
  rc = CalypsoCardOpenSecureSessionEx(ctx, apdu_p1, apdu_p2, sam_chal, resp, respsize);
  if (rc) goto done;
  
  if (*respsize < 4)
  {
    rc = CALYPSO_ERR_RESPONSE_SIZE;
    goto done;
  }
  
  if (sfi || rec_no)
    rec_len = CALYPSO_MIN_RECORD_SIZE;
  else
    rec_len = 0;

  if (*respsize == (SIZE_T) (4+rec_len))
  {
    /* Ratified, no KVC (hope KVC has been retrieved earlier...) */
    if (ratified != NULL)  *ratified = TRUE;    
    if (card_chal != NULL) memcpy(card_chal, &resp[0], 4);
    rec_ptr = &resp[4];
  } else
  if (*respsize == (SIZE_T) (5+rec_len))
  {
    /* Ratified, KVC returned */
    if (ratified != NULL)  *ratified = TRUE;
    if (kvc != NULL)       *kvc = resp[0];
    if (card_chal != NULL) memcpy(card_chal, &resp[1], 4);
    rec_ptr = &resp[5];
  } else
  if (*respsize == (SIZE_T) (7+rec_len))
  {
    /* Not ratified, KVC returned, 2 ratification bytes */
    if (ratified != NULL)  *ratified = FALSE;
    if (kvc != NULL)       *kvc = resp[0];
    if (card_chal != NULL) memcpy(card_chal, &resp[1], 4);
    rec_ptr = &resp[7];
  } else
  if (*respsize == (SIZE_T) (9+rec_len))
  {
    /* Not ratified, KVC returned, 4 ratification bytes */
    if (ratified != NULL)  *ratified = FALSE;
    if (kvc != NULL)       *kvc = resp[0];
    if (card_chal != NULL) memcpy(card_chal, &resp[1], 4);
    rec_ptr = &resp[9];
  } else
  {
    rc = CALYPSO_CARD_DATA_MALFORMED;
    goto done;
  }
  
  if (datasize != NULL)
  {
    if ((*datasize != 0) && (*datasize < rec_len))
    {
      *datasize = rec_len;
      rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
      goto done;
    }
    *datasize = rec_len;
  }
  
  if ((data != NULL) && (rec_len))
    memcpy(data, rec_ptr, rec_len);
  
done:
  RETURN("OpenSecureSession2");  
}

/**f* SpringProxINS/CalypsoCardOpenSecureSession3
 *
 * NAME
 *   CalypsoCardOpenSecureSession3
 *
 * DESCRIPTION
 *   Open a secure session, Revision 3 implementation
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx         : library context
 *   BYTE           resp[]       : buffer to receive the response
 *   SIZE_T     *respsize    : input  = size of the response buffer
 *                                 output = actual length of the response
 *                                          (EXCLUDING the status word that must be 9000)
 *   BYTE           key_no       : identifier of the cryptographic key
 *   BYTE           sfi          : identifier of the file to read (0 for current file)
 *   BYTE           rec_no       : identifier of the record to read (0 for none)
 *   const BYTE     sam_chal[4]  : challenge returned by the SAM (see CalypsoSamGetChallenge)
 *   BYTE           card_chal[4] : buffer to receive card's challenge
 *   BOOL           *ratified    : tells whether last card session was ratified or not
 *   BYTE           data[]       : buffer to receive record data (if some)
 *   SIZE_T     *datasize    : input  = size of the record data buffer
 *                                 output = actual length of record data
 *   BYTE           *kvc         : KVC specified by the card according to key_no
 *   BYTE           *kif         : KIF specified by the card according to key_no
 * 
 * RETURNS
 *   CALYPSO_RC                  : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardOpenSecureSession3(CALYPSO_CTX_ST *ctx, BYTE resp[], SIZE_T *respsize, BYTE key_no, BYTE sfi, BYTE rec_no, const BYTE sam_chal[4], BYTE card_chal[4], BOOL *ratified, BYTE data[], SIZE_T *datasize, BYTE *kvc, BYTE *kif)
{  
  BYTE apdu_p1, apdu_p2;
  CALYPSO_RC rc;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if ((resp == NULL) || (respsize == NULL))
  {
  	rc = CALYPSO_ERR_INVALID_PARAM;
  	goto done;
  }
  
  /* Command revision 3 version */
  apdu_p1 = (rec_no << 3) | key_no; /* Record number and key number */
  apdu_p2 = (sfi    << 3) | 0x01;   /* SFI and revision 3 mode bit  */
  
  rc = CalypsoCardOpenSecureSessionEx(ctx, apdu_p1, apdu_p2, sam_chal, resp, respsize);
  if (rc) goto done;
  
  if (*respsize < 8)
  {
    rc = CALYPSO_ERR_RESPONSE_SIZE;
    goto done;
  }
  
  if (card_chal != NULL)
    memcpy(card_chal, &resp[0], 4);
  
  if (ratified != NULL)
    *ratified = resp[4];  
  if (kif != NULL)
    *kif = resp[5];
  if (kvc != NULL)
    *kvc = resp[6];
    
  if (*respsize != (SIZE_T) (8+resp[7]))
  {
    rc = CALYPSO_CARD_DATA_MALFORMED;
    goto done;
  }
  
  if (datasize != NULL)
  {
    if ((*datasize != 0) && (*datasize < resp[7]))
    {
      *datasize = resp[7];
      rc = CALYPSO_ERR_BUFFER_TOO_SHORT;
      goto done;
    }
    *datasize = resp[7];
  }
  
  if ((data != NULL) && (resp[7]))
    memcpy(data, &resp[8], resp[7]);
  
done:
  RETURN("OpenSecureSession3");  
}


/**f* SpringProxINS/CalypsoCardCloseSecureSession 
 *
 * NAME
 *   CalypsoCardCloseSecureSession
 *
 * DESCRIPTION
 *   Close a secure session
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx        : library context
 *   BOOL           ratify_now  : if set, the session is immediately ratified
 *   BYTE           sam_sign[4] : the signature provided by the Calypso SAM
 *   BYTE           resp[]      : buffer to receive the response
 *   BYTE           *respsize   : input  = size of the response buffer
 *                                output = actual length of the response
 *                                         (EXCLUDING the status word that must be 9000)
 * 
 * RETURNS
 *   CALYPSO_RC                 : 0 or an error code
 *
 * NOTES
 *   This function is only a command sent to the card.
 *   The response is not interpreted by this function. It is up to the caller to
 *   1. check that the returned content is valid
 *   2. ask the SAM to verify card's signature
 *   For an automated implementation, use CalypsoCommitTransaction instead.
 *
 **/
CALYPSO_PROC CalypsoCardCloseSecureSession(CALYPSO_CTX_ST *ctx, BOOL ratify_now, const BYTE sam_sign[4], BYTE resp[], SIZE_T *respsize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_TRACE|TR_CARD, "CloseSecureSession");
  if (sam_sign != NULL)
    CalypsoTraceHex(TR_TRACE|TR_CARD, "S'Sign=", sam_sign, 4);

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_CLOSE_SESSION;
  ctx->Card.Buffer[send_len++] = ratify_now ? 0x80 : 0x00;
  ctx->Card.Buffer[send_len++] = 0x00;
  if (sam_sign == NULL)
  {
    ctx->Card.Buffer[send_len++] = 0x00;
  } else
  {
    ctx->Card.Buffer[send_len++] = 4;
    memcpy(&ctx->Card.Buffer[send_len], sam_sign, 4);
    send_len += 4;
    
    if (ctx->Card.T1)   
      ctx->Card.Buffer[send_len++] = 0x00; /* New 2017/10/11 : ensure Le is present */
  }

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : break;

    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;
    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6982 : rc = CALYPSO_CARD_DENIED_SAM_SIGN; break;
    case 0x6985 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6988 : rc = CALYPSO_CARD_DENIED_SAM_SIGN; break;

    default     : if ((ctx->Card.SW & 0xFF00) == 0x6100)
                    break;

                  rc = CALYPSO_ERR_STATUS_WORD;
  }
  if (rc) goto done;

  if ((recv_len == 2) && (sam_sign != NULL))
  {
    /* Get Response */
    recv_len = sizeof(ctx->Card.Buffer);
    rc = CalypsoCardGetResponse(ctx, &recv_len);
    if (rc) goto done;
  }

  recv_len -= 2;

  if (recv_len)
    CalypsoTraceHex(TR_TRACE|TR_CARD, "C'Sign=", ctx->Card.Buffer, recv_len);

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
  RETURN("CloseSecureSession");
}

/**f* SpringProxINS/CalypsoCardGetChallenge 
 *
 * NAME
 *   CalypsoCardGetChallenge
 *
 * DESCRIPTION
 *   Ask the card to generate an 8-byte challenge
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx          : library context
 *   BYTE           challenge[8]  : the challenge returned by the card
 * 
 * RETURNS
 *   CALYPSO_RC                   : 0 or an error code
 *
 * SEE ALSO
 *   CalypsoSamGiveRandom
 *
 **/
CALYPSO_PROC CalypsoCardGetChallenge(CALYPSO_CTX_ST *ctx, BYTE challenge[8])
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_TRACE|TR_CARD, "GetChallenge");

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_GET_CHALLENGE;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x08;

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  if (ctx->Card.SW == 0x9000)
  {
    if (recv_len != 10)
    {
      rc = CALYPSO_ERR_RESPONSE_SIZE;
    } else
    {
      /* OK ! */
      CalypsoTraceHex(TR_TRACE|TR_CARD, "C'Chal=", ctx->Card.Buffer, 8);

      if (challenge != NULL)
        memcpy(challenge, ctx->Card.Buffer, 8);
    }
  } else
  {
    switch (ctx->Card.SW)
    {
      case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
      default     : rc = CALYPSO_ERR_STATUS_WORD;
    }
  }

done:
  RETURN("GetChallenge");
}

/**f* SpringProxINS/CalypsoCardSendRatificationFrame 
 *
 * NAME
 *   CalypsoCardSendRatificationFrame
 *
 * DESCRIPTION
 *   Ask card to compute a challenge, specifying an invalid length
 *   (1 byte instead of 4). This command is used to ratify the session
 *   in a card not implementing the 'ratify on deselect' feature
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx : library context
 * 
 * RETURNS
 *   CALYPSO_RC          : 0 or an error code
 *
 **/
CALYPSO_PROC CalypsoCardSendRatificationFrame(P_CALYPSO_CTX ctx)
{
  CALYPSO_RC rc;
  SIZE_T recv_len, send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_TRACE|TR_CARD, "GetChallenge");

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_GET_CHALLENGE;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x01;

  recv_len = sizeof(ctx->Card.Buffer);
  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

done:
  RETURN("GetChallenge");
}
