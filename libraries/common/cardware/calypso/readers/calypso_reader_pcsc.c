/**h* CalypsoAPI/calypso_reader_pcsc.c
 *
 * NAME
 *   calypso_reader_pcsc.c
 *
 * DESCRIPTION
 *   PC/SC reader implementation
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

#ifdef WIN32
#pragma comment (lib, "winscard.lib")
#endif

CALYPSO_RC CalypsoPcscTransmit(PCSC_CTX_ST *pcsc_ctx, const BYTE send_apdu[], SIZE_T send_apdu_len, BYTE recv_apdu[], SIZE_T *recv_apdu_len)
{
	const SCARD_IO_REQUEST *pioPci;
	BYTE  l_recv_apdu[2];
	DWORD l_recv_apdu_len = 2;

	if (pcsc_ctx == NULL) return CALYPSO_ERR_INTERNAL_ERROR;

	if ((recv_apdu != NULL) && (recv_apdu_len != NULL))
	{
		/* Normal */
		l_recv_apdu_len = (DWORD) (* recv_apdu_len);
	}
	else if ((recv_apdu == NULL) && (recv_apdu_len == NULL))
	{
		/* Local */
		recv_apdu = l_recv_apdu;
	}
	else
		return CALYPSO_ERR_INVALID_PARAM;

	switch (pcsc_ctx->dwProtocol)
	{
		case SCARD_PROTOCOL_T0 :
			pioPci = SCARD_PCI_T0;
		break;
		case SCARD_PROTOCOL_T1 :
			pioPci = SCARD_PCI_T1;
		break;
		default                :
			CalypsoTraceStr((BYTE) (pcsc_ctx->bTrace | TR_ERROR), "SCardTransmit invalid proto");
			pioPci = NULL;
	}

	CalypsoTraceHex((BYTE) (pcsc_ctx->bTrace | TR_TRANSMIT), "DN", send_apdu, send_apdu_len);

	pcsc_ctx->lLastResult = SCardTransmit(pcsc_ctx->hCard, pioPci, send_apdu, (DWORD) send_apdu_len, NULL, recv_apdu, &l_recv_apdu_len);

	if (pcsc_ctx->lLastResult != SCARD_S_SUCCESS)
	{
		CalypsoTraceValD((BYTE) (pcsc_ctx->bTrace | TR_ERROR), "SCardTransmit T=", pcsc_ctx->dwProtocol-1, 1);
		CalypsoTraceValH((BYTE) (pcsc_ctx->bTrace | TR_ERROR), "SCardTransmit error ", pcsc_ctx->lLastResult, 8);
		return CALYPSO_ERR_DIALOG_;
	}

	CalypsoTraceHex((BYTE) (pcsc_ctx->bTrace | TR_TRANSMIT), "UP", recv_apdu, l_recv_apdu_len);

	if (l_recv_apdu_len == 0)
	{
		CalypsoTraceStr((BYTE) (pcsc_ctx->bTrace | TR_TRANSMIT), "Removed");
		return CALYPSO_ERR_REMOVED_;
	}
	
	if (recv_apdu_len != NULL)
		*recv_apdu_len = l_recv_apdu_len;

	return 0;
}

CALYPSO_RC CalypsoPcscGetAtr(PCSC_CTX_ST *pcsc_ctx, BYTE atr[], SIZE_T *atrlen)
{
  DWORD dwState;
  DWORD l_atr_len;
  
  char szReaderName[255];
  DWORD dwReaderNameLen = sizeof(szReaderName);

  if ((pcsc_ctx == NULL) || (atr == NULL) || (atrlen == NULL)) return CALYPSO_ERR_INTERNAL_ERROR;  
  
  l_atr_len = (DWORD) *atrlen;

  pcsc_ctx->lLastResult = SCardStatus(pcsc_ctx->hCard,
                                      szReaderName,
                                      &dwReaderNameLen,
                                      &dwState,
                                      &pcsc_ctx->dwProtocol,
                                      atr,
                                      &l_atr_len);

  if (pcsc_ctx->lLastResult != SCARD_S_SUCCESS)
  {
    *atrlen = 0;
    CalypsoTraceValH((BYTE) (pcsc_ctx->bTrace | TR_ERROR), "SCardStatus error ", pcsc_ctx->lLastResult, 8);
    return CALYPSO_ERR_REMOVED_;
  }
  
  *atrlen = l_atr_len;

  CalypsoTraceHex((BYTE) (pcsc_ctx->bTrace | TR_TRANSMIT), "ATR=", atr, *atrlen);
  CalypsoTraceValD((BYTE) (pcsc_ctx->bTrace | TR_TRANSMIT), "Protocol T=", pcsc_ctx->dwProtocol-1, 0);
  return 0;
}

/**f* CSB6_Calypso/CalypsoCardBindPcsc
 *
 * NAME
 *   CalypsoCardBindPcsc
 *
 * DESCRIPTION
 *   Bind a PC/SC smartcard handle to the Calypso card to be used by the library
 *
 * INPUTS
 *   CALYPSO_CTX_ST  ctx   : library context
 *   SCARDHANDLE     hCard : handle to the smartcard, returned by SCardConnect
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 **/
CALYPSO_PROC CalypsoCardBindPcsc(CALYPSO_CTX_ST *ctx, SCARDHANDLE hCard)
{
  CALYPSO_RC rc;
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  memset(&ctx->Card.Pcsc, 0, sizeof(ctx->Card.Pcsc));

  ctx->Card.Type = CALYPSO_TYPE_PCSC;

  ctx->Card.Pcsc.hCard  = hCard;  
  ctx->Card.Pcsc.bTrace = TR_CARD;

  ctx->Card.AtrLen = sizeof(ctx->Card.Atr);
  rc = CalypsoPcscGetAtr(&ctx->Card.Pcsc, ctx->Card.Atr, &ctx->Card.AtrLen);
  if (rc)
    rc |= CALYPSO_ERR_CARD_;

  return rc;
}

/**f* CSB6_Calypso/CalypsoSamBindPcsc
 *
 * NAME
 *   CalypsoSamBindPcsc
 *
 * DESCRIPTION
 *   Bind a PC/SC smartcard handle to the Calypso SAM to be used by the library
 *
 * INPUTS
 *   CALYPSO_CTX_ST ctx   : library context
 *   SCARDHANDLE    hCard : handle to the smartcard, returned by SCardConnect
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 **/
CALYPSO_PROC CalypsoSamBindPcsc(CALYPSO_CTX_ST *ctx, SCARDHANDLE hCard)
{
  CALYPSO_RC rc;
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  memset(&ctx->Sam.Pcsc, 0, sizeof(ctx->Sam.Pcsc));

  ctx->Sam.Type = CALYPSO_TYPE_PCSC;

  ctx->Sam.Pcsc.hCard  = hCard;
  ctx->Sam.Pcsc.bTrace = TR_SAM;

  ctx->Sam.CLA    = 0x80;

  ctx->Sam.AtrLen = sizeof(ctx->Sam.Atr);
  rc = CalypsoPcscGetAtr(&ctx->Sam.Pcsc, ctx->Sam.Atr, &ctx->Sam.AtrLen);
  if (rc)
    rc |= CALYPSO_ERR_CARD_;

  return rc;
}
