#include "../calypso_api_i.h"

#ifdef CALYPSO_TRACE
BYTE calypso_trace_level;
#endif

#ifdef CALYPSO_BENCHMARK
  static DWORD benchmark_t;
  #define CARD_T0() { benchmark_t = timer_milliseconds(); }
  #define CARD_T1() { benchmark_t = timer_milliseconds() - benchmark_t; ctx->benchmark.card_dialog += benchmark_t; }
  #define SAM_T0()  { benchmark_t = timer_milliseconds(); }
  #define SAM_T1()  { benchmark_t = timer_milliseconds() - benchmark_t; ctx->benchmark.sam_dialog += benchmark_t; }
#else  
  #define CARD_T0()
  #define CARD_T1()
  #define SAM_T0()
  #define SAM_T1()
#endif

CALYPSO_RC CalypsoCardSelectInnovatron(CALYPSO_CTX_ST *ctx, const BYTE atr[], SIZE_T atrlen)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  ctx->Card.Proto = PICC_TYPE_INNOVATRON;
  ctx->Card.CLA = 0x94;
  
  if (atr != NULL)
  {
    ctx->Card.AtrLen = atrlen;
    memcpy(ctx->Card.Atr, atr, atrlen);  
  }
  
  CalypsoTraceHex(TR_TRACE|TR_CARD, "Select Innovatron ", atr, atrlen);

  return MI_OK;
}

CALYPSO_RC CalypsoCardSelectTcl(CALYPSO_CTX_ST *ctx, BYTE cid, BYTE cla)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  ctx->Card.Proto = PICC_TYPE_14443_A|PICC_TYPE_14443_B;
  ctx->Card.Cid = cid;
  ctx->Card.CLA = cla;
  ctx->Card.T1  = TRUE;
  
  CalypsoTraceValH(TR_TRACE|TR_CARD, "Select T=CL CID=", cid, 2);

  return MI_OK;  
}

CALYPSO_PROC CalypsoCardTransmit(P_CALYPSO_CTX ctx, const BYTE in_buffer[], SIZE_T in_length, BYTE out_buffer[], SIZE_T *out_length)
{
  CALYPSO_RC rc;
  SIZE_T l = 0;
  SIZE_T z;
  
#ifndef CALYPSO_NO_SAM
  static BYTE in_buffer_copy[CALYPSO_MAX_APDU_SZ];
#endif
      
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (out_length == NULL) out_length = &l;
  z = *out_length;
  
  /* Remember in_buffer (as in most cases in_ and out_ buffers are equals...) */
#ifndef CALYPSO_NO_SAM
  memcpy(in_buffer_copy, in_buffer, in_length);
#endif
    
  CalypsoTraceHex(TR_TRANSMIT, "C<", in_buffer, in_length);

  CARD_T0();
  
  switch (ctx->Card.Proto)
  {
#if (MICORE_WITH_INNOVATRON) 
    case PICC_TYPE_INNOVATRON :
      rc = Innovatron_Exchange(NULL, in_buffer, in_length, out_buffer, &z);
      if ((rc == MI_QUIT) && (sprox_lib_config.compliance == SPROX_COMPLIANCE_RCTIF4))
      {
        /* Emergency de-activation of the card has occured (after SW=6581 or SW=6400) */
        ctx->Card.Proto = 0;
        /* Return code OK to allow further processing of the SW */
        rc = MI_OK;
      }
      break;
#endif
    case (PICC_TYPE_14443_A|PICC_TYPE_14443_B) :
    case PICC_TYPE_14443_B :
    case PICC_TYPE_14443_A :
      rc = Tcl_Exchange(ctx->Card.Cid, in_buffer, in_length, out_buffer, &z);
      break;

    default : rc = CALYPSO_ERR_NOT_BINDED;    
  }
  
  *out_length = z;

  CARD_T1();

  if (rc)
  {
    /* Card must have been powered down by lower layer */
    ctx->Card.Proto = 0;
    CalypsoTraceRC(TR_TRANSMIT, "C> ", rc);
    goto done;
  }

  CalypsoTraceHex(TR_TRANSMIT, "C>", out_buffer, *out_length);

#ifndef CALYPSO_NO_SAM
#if 1
  if (ctx->CardApplication.SessionActive)
  {
    /* Forward IN and OUT Card APDUs to the SAM */
    /* ---------------------------------------- */

    /* IN APDU */    
    rc = CalypsoSamDigestUpdate(ctx, in_buffer_copy, in_length);
    if (rc) goto done;

    rc = CalypsoSamDigestUpdate(ctx, out_buffer, *out_length);
    if (rc) goto done;
  }
#else
  if (ctx->CardApplication.SessionActive)
  {
    /* Forward IN and OUT Card APDUs to the SAM */
    /* ---------------------------------------- */
    
    WORD sam_buflen;

    /* First buffer is already the sent command */
    sam_buflen = in_length;
    if (in_length <= 5)
    {
      /* Short command : append response into first buffer */
      memcpy(&sam_buffer[sam_buflen], out_buffer, *out_length-2);
      sam_buflen += *out_length-2;
    }

    rc = CalypsoSamDigestUpdate(ctx, sam_buffer, sam_buflen);
    if (rc) goto done;

    if (in_length > 5)
    {
      /* Long command : second buffer for full response */
      memcpy(&sam_buffer[0], out_buffer, *out_length);
      sam_buflen = *out_length;
    } else
    {
      /* Short command : second buffer for status word only */
      memcpy(&sam_buffer[0], &out_buffer[*out_length-2], 2);
      sam_buflen = 2;
    }

    rc = CalypsoSamDigestUpdate(ctx, sam_buffer, sam_buflen);
    if (rc) goto done;
  }
#endif
#endif

done:  

  if (rc < MI_OK)
  {
    switch (rc)
    {
      case MI_NOTAGERR         :
      case MI_INTERFACEERR     : 
      case MI_ACCESSTIMEOUT    : rc = CALYPSO_ERR_FATAL_|CALYPSO_ERR_REMOVED_;
                                 break;
      default                  : rc = CALYPSO_ERR_FATAL_|CALYPSO_ERR_DIALOG_;
                                 break;
    }    
  }

  return rc;
}

#ifndef CALYPSO_NO_SAM
CALYPSO_RC CalypsoSamTransmitDirty(CALYPSO_CTX_ST *ctx, const BYTE in_buffer[], WORD in_length)
{  
#if (SPROX_INS_LIB_WITH_GEMCORE)
  SAM_T0();
  Sc7816_ExchangeOverlapped(ctx->Sam.Slot, in_buffer, in_length);  
  SAM_T1();
  return MI_OK;
#else
  return CALYPSO_ERR_NOT_IMPLEMENTED;
#endif
}

CALYPSO_RC CalypsoSamTransmit(CALYPSO_CTX_ST *ctx, const BYTE in_buffer[], WORD in_length, BYTE out_buffer[], WORD *out_length)
{
#if (SPROX_INS_LIB_WITH_GEMCORE)
  CALYPSO_RC rc;
  BYTE  gcr_rc = GCR_OK;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if (!ctx->Sam.Active) return CALYPSO_ERR_FATAL_|CALYPSO_ERR_SAM_|CALYPSO_ERR_REMOVED_;
      
  CalypsoTraceHex(TR_TRANSMIT, "S<", in_buffer, in_length);

  SAM_T0();

  if (!Sc7816_Exchange(ctx->Sam.Slot, in_buffer, in_length, out_buffer, out_length))
    gcr_rc = Sc7816_GetLastError();
  
  SAM_T1();
  
  if (gcr_rc != GCR_OK)
  {
    CalypsoTraceRC(TR_TRANSMIT, "S> GCR ", gcr_rc);
  }
  
  switch (gcr_rc)
  {
    case GCR_OK                    :
    case GCR_CARD_ERR_NOT_9000     : rc = MI_OK;
                                     break;

 //rc = CALYPSO_ERR_SAM_MUTE;

    case GCR_RESYNCH_OK            :
    case GCR_ERR_PPS_INVALID       :      
    case GCR_CARD_POWERED_UP       :     
    case GCR_ERR_REPORTED_LRC      :
    case GCR_ERR_READER_LRC        :
    case GCR_ERR_READER_MUTE       :
    case GCR_ERR_SEQUENCE_NUMBER   :
    case GCR_ERR_RESYNCH           :
    case GCR_ERR_SERIOUS_ERROR     :
    case GCR_ERR_NACK              :
    case GCR_ERR_PARITY            :
    case GCR_ERR_BUFFER_OVERFLOW   :
    case GCR_ERR_INVALID_PARAMETER :
    case GCR_ERR_INVALID_RESPONSE  :
    case GCR_ERR_NO_SLOT           :
    case GCR_ERR_BAD_CONFIG        :
    case GCR_ERR_DRIVER_UNKNOWN    :
    case GCR_ERR_COMMAND_DENIED    :
    case GCR_ERR_COMMAND_ARGS      :
    case GCR_ERR_COMMAND_UNKNOWN   :
    case GCR_ERR_RESP_OVERFLOW     :
    case GCR_CARD_ERR_ATR_TS       :
    case GCR_ERR_SEND_OVERFLOW     :
    case GCR_ERR_COMMAND_PARAMS    :
    case GCR_CARD_ERR_ATR_TCK      :
    case GCR_CARD_ERR_ATR_INVALID  : rc = CALYPSO_ERR_FATAL_|CALYPSO_ERR_SAM_; break;
      
    case GCR_CARD_ERR_MALFUNCTION  :
    case GCR_CARD_ERR_DIALOG       :
    case GCR_CARD_ERR_PARITY       :
    case GCR_CARD_ERR_PROTO        :
    case GCR_CARD_ERR_CHAINING     :
    case GCR_CARD_ERR_PROC_BYTE    :
    case GCR_CARD_ERR_INTERRUPTED  :
    case GCR_CARD_ERR_CHAINING_2   : rc = CALYPSO_ERR_FATAL_|CALYPSO_ERR_SAM_|CALYPSO_ERR_DIALOG_; break;
    

    case GCR_CARD_POWERED_DOWN     :
    case GCR_CARD_ERR_SHORTCIRCUIT : 
    case GCR_CARD_REMOVED          :
    case GCR_CARD_ABSENT           : rc = CALYPSO_ERR_FATAL_|CALYPSO_ERR_SAM_|CALYPSO_ERR_REMOVED_; break;
    
    default                        : rc = CALYPSO_ERR_FATAL_|CALYPSO_ERR_SAM_; break;
  }

  if (rc == MI_OK)
  {
    CalypsoTraceHex(TR_TRANSMIT, "S>", out_buffer, *out_length);
  } else
  {
    CalypsoTraceRC(TR_TRANSMIT, "S> ", rc);
  }

  return rc;
#else
  return CALYPSO_ERR_NOT_IMPLEMENTED;
#endif
}
#endif
