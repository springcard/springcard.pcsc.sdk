/**h* CalypsoAPI/calypso_reader_legacy.c
 *
 * NAME
 *   calypso_reader_legacy.c
 *
 * DESCRIPTION
 *   Legacy reader (CSB4, K531, SpringProx ...) implementation
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 28/12/2009 : first public release
 *
 **/
#include "../calypso_api_i.h"

#ifdef CALYPSO_LEGACY

  #ifndef SPROX_API_REENTRANT

    #define SPROX_API_CALL(a)            SPROX_ ## a
    #define SPROX_PARAM
    #define SPROX_PARAM_V                void
    #define SPROX_PARAM_P
    #define SPROX_PARAM_PV
     
  #else

    #define SPROX_API_CALL(a)            SPROXx_ ## a
    #define SPROX_PARAM                  SPROX_INSTANCE rInst, 
    #define SPROX_PARAM_V                SPROX_INSTANCE rInst
    #define SPROX_PARAM_P                legacy_ctx->rInst,
    #define SPROX_PARAM_PV               legacy_ctx->rInst
    
  #endif

CALYPSO_RC CalypsoLegacyTransmit(LEGACY_CTX_ST *legacy_ctx, const BYTE send_apdu[], SIZE_T send_apdu_len, BYTE recv_apdu[], SIZE_T *recv_apdu_len)
{
  BYTE l_recv_apdu[2];
  WORD l_recv_apdu_len, l_send_apdu_len;

  if (legacy_ctx == NULL) return CALYPSO_ERR_INTERNAL_ERROR;
  if (send_apdu == NULL) return CALYPSO_ERR_INTERNAL_ERROR;
  if (send_apdu_len > CALYPSO_MAX_APDU_SZ) return CALYPSO_ERR_INTERNAL_ERROR;

  l_send_apdu_len = (WORD) send_apdu_len;

  if ((recv_apdu != NULL) && (recv_apdu_len != NULL))
  {
    /* Normal */
    if (*recv_apdu_len >= 0x00010000)
      l_recv_apdu_len = 0xFFFF;
    else
      l_recv_apdu_len = (WORD) *recv_apdu_len;
  } else
  if ((recv_apdu == NULL) && (recv_apdu_len == NULL))
  {
     /* Local */
     recv_apdu       = l_recv_apdu;
     l_recv_apdu_len = sizeof(l_recv_apdu);
  } else
    return CALYPSO_ERR_INVALID_PARAM;

  CalypsoTraceHex((BYTE) (legacy_ctx->bTrace | TR_TRANSMIT), "DN", send_apdu, send_apdu_len);

  switch (legacy_ctx->Proto)
  {
    case 0 :
      legacy_ctx->swLastResult = SPROX_API_CALL(Card_Exchange) (SPROX_PARAM_P  legacy_ctx->CidOrSlot, send_apdu, l_send_apdu_len, recv_apdu, &l_recv_apdu_len);
      break;

    case PROTO_14443_A    : 
    case PROTO_14443_B    : 
    case PROTO_INNOVATRON :
      legacy_ctx->swLastResult = SPROX_API_CALL(Tcl_Exchange) (SPROX_PARAM_P  0xFF, send_apdu, l_send_apdu_len, recv_apdu, &l_recv_apdu_len);
      break;

    default :
      return CALYPSO_ERR_NOT_BINDED;
  }

	if (legacy_ctx->swLastResult != MI_OK)
  {
    CalypsoTraceValH((BYTE) (legacy_ctx->bTrace | TR_ERROR), "CardExchange proto ", legacy_ctx->Proto, 4);
    CalypsoTraceValD((BYTE) (legacy_ctx->bTrace | TR_ERROR), "CardExchange error ", legacy_ctx->swLastResult, 0);
		return CALYPSO_ERR_DIALOG_;
  }

  CalypsoTraceHex((BYTE) (legacy_ctx->bTrace | TR_TRANSMIT), "UP", recv_apdu, l_recv_apdu_len);

  if (recv_apdu_len != NULL)
    *recv_apdu_len = l_recv_apdu_len;

  if (l_recv_apdu_len == 0)
  {
    return CALYPSO_ERR_REMOVED_;
  }
  return 0;
}

CALYPSO_PROC CalypsoCardBindLegacyEx(P_CALYPSO_CTX ctx, WORD proto, const BYTE info[], WORD infolen)
{
  LEGACY_CTX_ST *legacy_ctx;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  legacy_ctx = &ctx->Card.Legacy;
  memset(legacy_ctx, 0, sizeof(LEGACY_CTX_ST));

#ifdef SPROX_API_REENTRANT
  legacy_ctx->rInst = ctx->rInst;
#endif
  
  legacy_ctx->bTrace = TR_CARD;
  legacy_ctx->Proto  = proto;

  switch (legacy_ctx->Proto)
  {
    case PROTO_14443_A :
      legacy_ctx->swLastResult = SPROX_API_CALL(TclA_GetAts) (SPROX_PARAM_P  0xFF, NULL, NULL);
      break;

    case PROTO_14443_B :
      legacy_ctx->swLastResult = SPROX_API_CALL(TclB_Attrib) (SPROX_PARAM_P  NULL, 0xFF);
      break;

    case PROTO_INNOVATRON :
      if ((info != NULL) && (infolen > 6))
      {
        /* Info shall be card's REPGEN - let's extract the ATR */
        ctx->Card.AtrLen = infolen-6;
        if (ctx->Card.AtrLen > sizeof(ctx->Card.Atr))
          ctx->Card.AtrLen = sizeof(ctx->Card.Atr);
        memcpy(ctx->Card.Atr, &info[6], ctx->Card.AtrLen);
      }
      legacy_ctx->swLastResult = SPROX_API_CALL(Bi_Attrib) (SPROX_PARAM_P  NULL);
      break;

    default :
      return CALYPSO_ERR_INVALID_PARAM;
  }

  if (legacy_ctx->swLastResult != MI_OK)
  {
    CalypsoTraceValH((BYTE) (legacy_ctx->bTrace | TR_ERROR), "CardConnect proto ", legacy_ctx->Proto, 4);
    CalypsoTraceValD((BYTE) (legacy_ctx->bTrace | TR_ERROR), "CardConnect error ", legacy_ctx->swLastResult, 0);
		return CALYPSO_ERR_CARD_|CALYPSO_ERR_DIALOG_;
  }

  ctx->Card.Type = CALYPSO_TYPE_LEGACY;  
  return 0;
}

CALYPSO_PROC CalypsoSamBindLegacy(P_CALYPSO_CTX ctx, BYTE slot)
{
  LEGACY_CTX_ST *legacy_ctx;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  legacy_ctx = &ctx->Sam.Legacy;
  memset(legacy_ctx, 0, sizeof(LEGACY_CTX_ST));

  ctx->Sam.Legacy.bTrace = TR_SAM;
  ctx->Sam.Legacy.CidOrSlot = slot;

#ifdef SPROX_API_REENTRANT
  legacy_ctx->rInst = ctx->rInst;
#endif
  
  ctx->Sam.Type = CALYPSO_TYPE_LEGACY;
  return 0;
}

#ifdef SPROX_API_REENTRANT
CALYPSO_PROC CalypsoAttachLegacyInstance(P_CALYPSO_CTX ctx, SPROX_INSTANCE rInst)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  ctx->rInst = rInst;
  ctx->Card.Legacy.rInst = rInst;
  ctx->Sam.Legacy.rInst = rInst;
  
  return 0;
}
#endif

#endif
