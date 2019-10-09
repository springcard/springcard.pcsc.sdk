/**h* CalypsoAPI/calypso_card_pin.c
 *
 * NAME
 *   SpringCard Calypso API :: Card command set (verify pin / change pin)
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 12/01/2010 : first public release
 *
 **/
#include "../calypso_api_i.h"
#include "calypso_card_commands_i.h"

/*
 **********************************************************************************************************************
 *
 * PIN
 *
 **********************************************************************************************************************
 */

CALYPSO_RC CalypsoCardVerifyPin__Ex(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T datasize, BYTE *remaining)
{
  CALYPSO_RC rc;
  SIZE_T recv_len = 2;
  SIZE_T send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (datasize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_INTERNAL_ERROR;

  if (remaining != NULL) *remaining = 0;

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "VerifyPin");

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_VERIFY_PIN;
  ctx->Card.Buffer[send_len++] = 0x00;
  ctx->Card.Buffer[send_len++] = 0x00;

  if ((data != NULL) && (datasize))
  {
    ctx->Card.Buffer[send_len++] = (BYTE) datasize;
    memcpy(&ctx->Card.Buffer[send_len], data, datasize);
    send_len += datasize;
  } else
    ctx->Card.Buffer[send_len++] = 0x00;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : if (remaining != NULL) *remaining = 3; break;

    case 0x63C0 : if (remaining != NULL) *remaining = 1; rc = CALYPSO_CARD_ACCESS_DENIED; break;
    case 0x63C1 : if (remaining != NULL) *remaining = 1; rc = CALYPSO_CARD_ACCESS_DENIED; break;
    case 0x63C2 : if (remaining != NULL) *remaining = 2; rc = CALYPSO_CARD_ACCESS_DENIED; break;
    case 0x6900 :                                        rc = CALYPSO_CARD_ACCESS_DENIED; break;

    case 0x6983 : rc = CALYPSO_CARD_PIN_BLOCKED; break;

    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;

    case 0x6D00 : rc = CALYPSO_CARD_FILE_NOT_FOUND; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("VerifyPin");
}

CALYPSO_RC CalypsoCardChangePin__Ex(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T datasize)
{
  CALYPSO_RC rc;
  SIZE_T recv_len = 2;
  SIZE_T send_len = 0;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (data == NULL) return CALYPSO_ERR_INTERNAL_ERROR;
  if (datasize > CALYPSO_MAX_DATA_SZ) return CALYPSO_ERR_INTERNAL_ERROR;

#ifdef CALYPSO_BENCHMARK
  ctx->benchmark.nb_write++;
#endif

  CalypsoTraceStr(TR_TRACE|TR_CARD, "ChangePin");

  ctx->Card.Buffer[send_len++] = ctx->Card.CLA;
  ctx->Card.Buffer[send_len++] = CALYPSO_INS_CHANGE_PIN;
  ctx->Card.Buffer[send_len++] = 0x00;
  if (ctx->CardApplication.Revision < 3)
    ctx->Card.Buffer[send_len++] = 0x04;
  else
    ctx->Card.Buffer[send_len++] = 0xFF;
  ctx->Card.Buffer[send_len++] = (BYTE) datasize;
  memcpy(&ctx->Card.Buffer[send_len], data, datasize);
  send_len += datasize;

  rc = CalypsoCardTransmit(ctx, ctx->Card.Buffer, send_len, ctx->Card.Buffer, &recv_len);
  if (rc) goto done;

  rc = CalypsoCardSetSW(ctx, recv_len);
  if (rc) goto done;

  switch (ctx->Card.SW)
  {
    case 0x9000 : break;

    case 0x6700 : rc = CALYPSO_ERR_SW_WRONG_P3; break;
    case 0x6900 : rc = CALYPSO_CARD_PIN_BLOCKED; break;
    case 0x6982 : rc = CALYPSO_CARD_NOT_IN_SESSION; break;
    case 0x6985 : rc = CALYPSO_CARD_ACCESS_FORBIDDEN; break;
    case 0x6A87 :
    case 0x6B00 : rc = CALYPSO_ERR_SW_WRONG_P1P2; break;

    default     : rc = CALYPSO_ERR_STATUS_WORD;
  }

done:
  RETURN("ChangePin");
}


/**f* SpringProxINS/CalypsoCardVerifyPinPlainEx
 *
 * NAME
 *   CalypsoCardVerifyPinPlainEx
 *
 * DESCRIPTION
 *   Verify card's PIN, returning the remaining number of tries
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx       : library context
 *   const BYTE     pin[4]     : the pin code
 *   BYTE           *remaining : the number of tries remaining
 *
 * RETURNS
 *   CALYPSO_RC                : 0 or an error code
 *
 * SEE ALSO
 *   CalypsoCardVerifyPinPlain
 *   CalypsoCardVerifyPinCipherEx
 *   CalypsoCardVerifyPinCipher
 *
 **/
CALYPSO_PROC CalypsoCardVerifyPinPlainEx(CALYPSO_CTX_ST *ctx, const BYTE pin[4], BYTE *remaining)
{
  if (pin != NULL)
    memcpy(ctx->CardApplication.CurrentPin, pin, 4);

  return CalypsoCardVerifyPin__Ex(ctx, pin, 4, remaining);
}

/**f* SpringProxINS/CalypsoCardVerifyPinPlain
 *
 * NAME
 *   CalypsoCardVerifyPinPlain
 *
 * DESCRIPTION
 *   Verify card's PIN
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx   : library context
 *   const BYTE     pin[4] : the pin code
 *
 * RETURNS
 *   CALYPSO_RC            : 0 or an error code
 *
 * SEE ALSO
 *   CalypsoCardVerifyPinPlainEx
 *   CalypsoCardVerifyPinCipherEx
 *   CalypsoCardVerifyPinCipher
 *
 **/
CALYPSO_PROC CalypsoCardVerifyPinPlain(CALYPSO_CTX_ST *ctx, const BYTE pin[4])
{
  if (pin != NULL)
    memcpy(ctx->CardApplication.CurrentPin, pin, 4);

  return CalypsoCardVerifyPin__Ex(ctx, pin, 4, NULL);
}

/**f* SpringProxINS/CalypsoCardVerifyPinCipherEx
 *
 * NAME
 *   CalypsoCardVerifyPinCipherEx
 *
 * DESCRIPTION
 *   Verify card's PIN, returning the remaining number of tries (ciphered mode)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx       : library context
 *   const BYTE     pin[4]     : the pin code
 *   BYTE           *remaining : the number of tries remaining
 *
 * RETURNS
 *   CALYPSO_RC                : 0 or an error code
 *
 * NOTES
 *   The pin has to be ciphered by the SAM using card's DEBIT key. Therefore, this function
 *   can only be called after a previous call to CalypsoStartTransaction (or CalypsoStartTransactionEx)
 *   having key_no = CALYPSO_KEY_DEBIT
 *
 * SEE ALSO
 *   CalypsoCardVerifyPinPlain
 *   CalypsoCardVerifyPinPlainEx
 *   CalypsoCardVerifyPinCipher
 *
 **/
CALYPSO_PROC CalypsoCardVerifyPinCipherEx(CALYPSO_CTX_ST *ctx, const BYTE pin[4], BYTE *remaining)
{
  CALYPSO_RC rc;
  BYTE challenge[8];
  BYTE pin_buffer[6];
  BYTE datagram[CALYPSO_MAX_DATA_SZ];
  SIZE_T datagramsize = sizeof(datagram);

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  if (pin == NULL)
    return CalypsoCardVerifyPin__Ex(ctx, NULL, 0, remaining);

  memcpy(ctx->CardApplication.CurrentPin, pin, 4);

  // TODO
  // OpenSecureSession avec la cle 3 (validation) pour avoir KIF et KVC
  // (ou retrouver KIF / KVC si session deja ouverte)
  // et passer KIF et KVC au SAM dans CalypsoSamCipherCardDataEx (avec P2=FF)

  rc = CalypsoCardGetChallenge(ctx, challenge);
  if (rc) return rc;

  rc = CalypsoSamGiveRandom(ctx, challenge);
  if (rc) return rc; 

  /* Incoming data = KIF, KVC, pin */
  pin_buffer[0] = ctx->CardApplication.CurrentKif;
  pin_buffer[1] = ctx->CardApplication.CurrentKvc;
  memcpy(&pin_buffer[2], pin, 4);

  CalypsoTraceHex(TR_TRACE|TR_TRANS, "Verify pin cipher", pin_buffer, sizeof(pin_buffer));

  rc = CalypsoSamCipherCardDataEx(ctx, 0x80, 0xFF, pin_buffer, sizeof(pin_buffer), datagram, &datagramsize);
  if (rc) return rc;

  return CalypsoCardVerifyPin__Ex(ctx, datagram, datagramsize, remaining);
}

/**f* SpringProxINS/CalypsoCardVerifyPinCipher
 *
 * NAME
 *   CalypsoCardVerifyPinCipher
 *
 * DESCRIPTION
 *   Verify card's PIN (ciphered mode)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx   : library context
 *   const BYTE     pin[4] : the pin code
 *
 * RETURNS
 *   CALYPSO_RC            : 0 or an error code
 *
 * NOTES
 *   The pin has to be ciphered by the SAM using card's DEBIT key. Therefore, this function
 *   can only be called after a previous call to CalypsoStartTransaction (or CalypsoStartTransactionEx)
 *   having key_no = CALYPSO_KEY_DEBIT
 *
 * SEE ALSO
 *   CalypsoCardVerifyPinPlain
 *   CalypsoCardVerifyPinPlainEx
 *   CalypsoCardVerifyPinCipherEx
 *
 **/
CALYPSO_PROC CalypsoCardVerifyPinCipher(CALYPSO_CTX_ST *ctx, const BYTE pin[4])
{
  return CalypsoCardVerifyPinCipherEx(ctx, pin, NULL);
}


/**f* SpringProxINS/CalypsoCardChangePinPlain
 *
 * NAME
 *   CalypsoCardChangePinPlain
 *
 * DESCRIPTION
 *   Change card's PIN
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx       : library context
 *   const BYTE     new_pin[4] : the new pin code
 *
 * RETURNS
 *   CALYPSO_RC                : 0 or an error code
 *
 * NOTES
 *   This function must follow a successfull call to CalypsoCardVerifyPinPlain
 *   (or CalypsoCardVerifyPinPlainEx or CalypsoCardVerifyPinCipher or CalypsoCardVerifyPinCipherEx)
 *
 * SEE ALSO
 *   CalypsoCardChangePinCipher
 *
 **/
CALYPSO_PROC CalypsoCardChangePinPlain(CALYPSO_CTX_ST *ctx, const BYTE new_pin[4])
{
  return CalypsoCardChangePin__Ex(ctx, new_pin, 4);
}

/**f* SpringProxINS/CalypsoCardChangePinCipher
 *
 * NAME
 *   CalypsoCardChangePinCipher
 *
 * DESCRIPTION
 *   Change card's PIN (ciphered mode)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx       : library context
 *   const BYTE     new_pin[4] : the new pin code
 *
 * RETURNS
 *   CALYPSO_RC                : 0 or an error code
 *
 * NOTES
 *   The pin has to be ciphered by the SAM using card's ISSUER key. Therefore, this function
 *   can only be called after a previous call to CalypsoStartTransaction (or CalypsoStartTransactionEx)
 *   having key_no = CALYPSO_KEY_ISSUER
 *   This function should also follow a successfull call to CalypsoCardVerifyPinPlain
 *   (or CalypsoCardVerifyPinPlainEx or CalypsoCardVerifyPinCipher or CalypsoCardVerifyPinCipherEx) 
 *
 * SEE ALSO
 *   CalypsoCardChangePinPlain
 *
 **/
CALYPSO_PROC CalypsoCardChangePinCipher(CALYPSO_CTX_ST *ctx, const BYTE new_pin[4])
{
  CALYPSO_RC rc;
  BYTE challenge[8];
  BYTE pin_buffer[10];
  BYTE datagram[CALYPSO_MAX_DATA_SZ];
  SIZE_T datagramsize = sizeof(datagram);

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (new_pin == NULL) return CALYPSO_ERR_INVALID_PARAM;

  // TODO
  // OpenSecureSession avec la cle 3 (validation) pour avoir KIF et KVC
  // (ou retrouver KIF / KVC si session deja ouverte)
  // et passer KIF et KVC au SAM dans CalypsoSamCipherCardDataEx (avec P2=FF)

  rc = CalypsoCardGetChallenge(ctx, challenge);
  if (rc) return rc;

  rc = CalypsoSamGiveRandom(ctx, challenge);
  if (rc) return rc; 

  /* Incoming data = KIF, KVC, current pin, new pin */
  pin_buffer[0] = ctx->CardApplication.CurrentKif;
  pin_buffer[1] = ctx->CardApplication.CurrentKvc;
  memcpy(&pin_buffer[2], ctx->CardApplication.CurrentPin, 4);
  memcpy(&pin_buffer[6], new_pin, 4);

  CalypsoTraceHex(TR_TRACE|TR_TRANS, "Change pin cipher", pin_buffer, sizeof(pin_buffer));

  rc = CalypsoSamCipherCardDataEx(ctx, 0x40, 0xFF, pin_buffer, sizeof(pin_buffer), datagram, &datagramsize);
  if (rc) return rc;

  return CalypsoCardChangePin__Ex(ctx, datagram, datagramsize);
}
