/**h* CalypsoAPI/calypso_transaction.c
 *
 * NAME
 *   SpringCard Calypso API :: Implementation of the Card+SAM transaction
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release
 *   JDA 15/08/2009 : implemented Revision 3
 *
 **/
#include "../calypso_api_i.h"

#ifndef CALYPSO_NO_TRANS

/**f* SpringProxINS/CalypsoStartTransaction
 *
 * NAME
 *   CalypsoStartTransaction
 *
 * DESCRIPTION
 *   Start a transaction between card and SAM (light implemenation)
 *   No data will be returned by the card, SAM's KIF will be deduce from card's key_no
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx       : library context
 *   BOOL           *ratified  : on output: was the previous session ratified, or not ?
 *   BYTE           key_no     : key number (card side)
 *
 * RETURNS
 *   CALYPSO_RC                : 0 or an error code
 *
 * NOTES
 *   key_no should be either CALYPSO_KEY_ISSUER, CALYPSO_KEY_LOAD or CALYPSO_KEY_DEBIT
 *
 *   See CalypsoStartTransactionEx for implementation details
 *
 * SEE ALSO
 *   CalypsoStartTransactionEx
 *   CalypsoCommitTransaction
 *   CalypsoCancelTransaction
 *
 **/
CALYPSO_PROC CalypsoStartTransaction(CALYPSO_CTX_ST *ctx, BOOL *ratified, BYTE key_no)
{
  BYTE kif;

  switch (key_no)
  {
    case CALYPSO_KEY_ISSUER : kif = CALYPSO_KIF_ISSUER; break;
    case CALYPSO_KEY_LOAD   : kif = CALYPSO_KIF_LOAD; break;
    case CALYPSO_KEY_DEBIT  : kif = CALYPSO_KIF_DEBIT; break;
    default                 : if (ctx->CardApplication.Revision >= 3)
                                kif = 0xFF;
                              else
                                return CALYPSO_KIF_IS_UNKNOWN_FOR_KEY;
  }

  return CalypsoStartTransactionEx(ctx, ratified, key_no, kif, 0, 0, NULL, NULL);  
}

/**f* SpringProxINS/CalypsoStartTransactionEx
 *
 * NAME
 *   CalypsoStartTransactionEx
 *
 * DESCRIPTION
 *   Start a transaction between card and SAM (full implementation)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx       : library context
 *   BOOL           *ratified  : on output: was the previous session ratified, or not ?
 *   BYTE           key_no     : key number (card side)
 *   BYTE           kif        : key identifier (KIF - SAM side)
 *   BYTE           sfi        : short identifier of the file to read (0 if none)
 *   BYTE           rec_no     : record number of the record to read (0 if none)
 *   BYTE           data[]     : buffer to receive record's data
 *   SIZE_T     *datasize  : in  : size of data
 *                               out : actual length of data returned by the card
 *
 * RETURNS
 *   CALYPSO_RC                : 0 or an error code
 *
 * NOTES
 *    This function performs he following sequence (with the appropriate parameters)
 *    - CalypsoSamSelectDiversifier
 *    - CalypsoCardOpenSecureSession1, CalypsoCardOpenSecureSession2 or CalypsoCardOpenSecureSession3
 *      (depending on the revision of the card)
 *    - CalypsoSamDigestInit
 *
 *    Afterwards, every APDU (in/out) exchanged with the card will be forwarded to the SAM
 *    (using CalypsoSamDigestUpdate) until either CalypsoCommitTransaction or CalypsoCancelTransaction
 *    is called
 *
 * SEE ALSO
 *   CalypsoStartTransaction
 *   CalypsoCommitTransaction
 *   CalypsoCancelTransaction
 *
 **/
CALYPSO_PROC CalypsoStartTransactionEx(CALYPSO_CTX_ST *ctx, BOOL *ratified, BYTE key_no, BYTE kif, BYTE sfi, BYTE rec_no, BYTE data[], SIZE_T *datasize)
{
  BYTE resp[64];
  SIZE_T respsize = sizeof(resp);
  BYTE sam_chal[4], card_chal[4];
  BOOL _ratified;
  BYTE kvc = 0xFF;
  CALYPSO_RC rc;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;  
  if (!ctx->CardApplication.Revision) return CALYPSO_CARD_NOT_SUPPORTED;

  CalypsoTraceStr(TR_TRACE|TR_TRANS, "StartTransaction");
  CalypsoTraceValD(TR_TRACE|TR_TRANS, "C'Rev=", ctx->CardApplication.Revision, 0);
  CalypsoTraceHex(TR_TRACE|TR_TRANS, "C'UID=", ctx->CardApplication.UID, sizeof(ctx->CardApplication.UID));
  CalypsoTraceValH(TR_TRACE|TR_TRANS, "Key=", key_no, 2);

  if (ctx->CardApplication.SessionActive)
  {
    CalypsoTraceStr(TR_ERROR|TR_TRANS, "StartTransaction: Already in transaction");
  }

  /* Feed the SAM with card's UID */
  rc = CalypsoSamSelectDiversifier(ctx, ctx->CardApplication.UID);
  if (rc) goto done;

  /* Ask the SAM to provide a challenge */
  rc = CalypsoSamGetChallenge(ctx, sam_chal);
  if (rc) goto done;

  //sfi =0; rec_no = 0;
  
  /* Ask the card to open a secure session */
  if (ctx->CardApplication.Revision >= 3)
  {
    rc = CalypsoCardOpenSecureSession3(ctx, resp, &respsize, key_no, sfi, rec_no, sam_chal, card_chal, &_ratified, data, datasize, &kvc, &kif);
  } else
  if (ctx->CardApplication.Revision == 2)
  {
    rc = CalypsoCardOpenSecureSession2(ctx, resp, &respsize, key_no, sfi, rec_no, sam_chal, card_chal, &_ratified, data, datasize, &kvc);
  } else
  if (ctx->CardApplication.Revision == 1)
  {
    rc = CalypsoCardOpenSecureSession1(ctx, resp, &respsize, key_no, sfi, rec_no, sam_chal, card_chal, &_ratified, data, datasize);
  } else
    rc = CALYPSO_ERR_INVALID_CONTEXT;
  
  if (rc) goto done;
  
  CalypsoTraceValH(TR_TRACE|TR_TRANS, "KIF=", kif, 2);
  CalypsoTraceValH(TR_TRACE|TR_TRANS, "KVC=", kvc, 2);
  CalypsoTraceValD(TR_TRACE|TR_TRANS, "Ratified=", _ratified, 0);

  if (ratified != NULL)
    *ratified = _ratified;

  /* Forward KVC and challenge to the SAM */
  if (ctx->CardApplication.Revision == 1)
  {
    BYTE kno;
    switch (kif)
    {
      case CALYPSO_KIF_ISSUER : kno = CALYPSO_KNO_ISSUER; break;
      case CALYPSO_KIF_LOAD   : kno = CALYPSO_KNO_LOAD; break;
      case CALYPSO_KIF_DEBIT  : kno = CALYPSO_KNO_DEBIT; break;
      default                 : rc = CALYPSO_KIF_IS_UNKNOWN_FOR_KEY; goto done;
    }

    rc = CalypsoSamDigestInitCompat(ctx, kno, resp, respsize);
  } else
  {
    if (kif == 0xFF)
    {
      /* KIF not returned by the card - let's use default one */
      switch (key_no)
      {
        case CALYPSO_KEY_ISSUER : kif = CALYPSO_KIF_ISSUER; break;
        case CALYPSO_KEY_LOAD   : kif = CALYPSO_KIF_LOAD; break;
        case CALYPSO_KEY_DEBIT  : kif = CALYPSO_KIF_DEBIT; break;
        default : break; /* Oups is 0xFF a valid KIF ??? */
      }
      CalypsoTraceValH(TR_TRACE|TR_TRANS, "KIF used=", kif, 2);
    }

    rc = CalypsoSamDigestInit(ctx, kif, kvc, resp, respsize);
  }
  if (rc)
  {
    /* We'd better rollback the transaction on the card side right now */
    CalypsoCardCloseSecureSession(ctx, FALSE, NULL, NULL, NULL);
    goto done;
  }

  /* Ready ! */
  ctx->CardApplication.SessionActive  = TRUE;
  ctx->CardApplication.SessionCurMods = 0;

  /* Remember KIF and KVC in case we need it later */
  ctx->CardApplication.CurrentKif = kif;
  ctx->CardApplication.CurrentKvc = kvc;

done:
  return rc;
}

/**f* SpringProxINS/CalypsoCommitTransaction
 *
 * NAME
 *   CalypsoCommitTransaction
 *
 * DESCRIPTION
 *   Commit a transaction
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx       : library context
 *   BOOL           ratify_now : if TRUE, card will ratify its transaction immediately
 *                               if FALSE, the ratification will happen on the next APDU
 *                               received by the card (call CalypsoCardSendRatificationFrame)
 *
 * RETURNS
 *   CALYPSO_RC                : 0 or an error code
 *
 * NOTES
 *    This function performs he following sequence (with the appropriate parameters)
 *    - CalypsoSamDigestClose
 *    - CalypsoCardCloseSecureSession
 *    - CalypsoSamDigestAuthenticate
 *
 * SEE ALSO
 *   CalypsoStartTransaction
 *   CalypsoStartTransactionEx
 *   CalypsoCancelTransaction
 *
 **/
CALYPSO_PROC CalypsoCommitTransaction(CALYPSO_CTX_ST *ctx, BOOL ratify_now)
{
  BYTE resp[CALYPSO_MAX_DATA_SZ];
  SIZE_T respsize = CALYPSO_MAX_DATA_SZ;
  BYTE sam_sign[4], card_sign[4];
  DWORD rc;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_TRACE|TR_TRANS, "CommitTransaction");

  if (!ctx->CardApplication.SessionActive)
  {
    CalypsoTraceStr(TR_ERROR|TR_TRANS, "CommitTransaction: Not in transaction");
  }

  ctx->CardApplication.SessionActive = FALSE;

  /* Ask the SAM to compute the signature */
  rc = CalypsoSamDigestClose(ctx, sam_sign);
  if (rc) goto done;

  /* Ask the card to close its session */
  rc = CalypsoCardCloseSecureSession(ctx, ratify_now, sam_sign, resp, &respsize);
  if (rc) goto done;

  /* Check card's response to retrieve its signature */
  if (respsize >= 4)
  {
    memcpy(card_sign, &resp[respsize-4], 4);
  } else
  {
    rc = CALYPSO_ERR_RESPONSE_SIZE;
    goto done;
  }

  /* Forward card's signature to the SAM */
  rc = CalypsoSamDigestAuthenticate(ctx, card_sign);
  if (rc) goto done;

done:
  return rc;
}

/**f* SpringProxINS/CalypsoCancelTransaction
 *
 * NAME
 *   CalypsoCancelTransaction
 *
 * DESCRIPTION
 *   Cancel a transaction (discarding all changes)
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx       : library context
 *
 * RETURNS
 *   CALYPSO_RC                : 0 or an error code
 *
 * SEE ALSO
 *   CalypsoStartTransaction
 *   CalypsoStartTransactionEx
 *   CalypsoCommitTransaction
 *
 **/
CALYPSO_PROC CalypsoCancelTransaction(CALYPSO_CTX_ST *ctx)
{
  BYTE resp[CALYPSO_MAX_DATA_SZ];
  SIZE_T respsize = CALYPSO_MAX_DATA_SZ;
  CALYPSO_RC rc;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoTraceStr(TR_TRACE|TR_TRANS, "CancelTransaction");

  if (!ctx->CardApplication.SessionActive)
  {
    CalypsoTraceStr(TR_ERROR|TR_TRANS, "CancelTransaction: Not in transaction");
  }

  ctx->CardApplication.SessionActive = FALSE;

  /* Ask the card to close its session */
  rc = CalypsoCardCloseSecureSession(ctx, FALSE, NULL, resp, &respsize);

  return rc;
}

#endif
