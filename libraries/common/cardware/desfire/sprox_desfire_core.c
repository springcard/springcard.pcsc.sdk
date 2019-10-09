/* DesfireAPI/Core
 *
 * NAME
 *   DesfireAPI ::  Core functions of the DesfireAPI
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of the DESFIRE communication methods.
 *
 **/
#include "sprox_desfire_i.h"
/* DesfireAPI/Command
 *
 * NAME
 *   Command
 *
 * DESCRIPTION
 *   Performs a T=CL block exchange according to ISO 14443-A-4
 *   "low" level command<->response transfert
 *
 * INPUTS
 *   BOOL fast          : if true, DESFire optimized loop is used
 *   WORD accept_len    : length of accepted response
 *   BYTE accept_status : flag to set which status are accepted WANTS_OPERATION_OK or WANTS_ADDITIONAL_FRAME
 *
 * RETURNS
 *   DF_OPERATION_OK    : T=CL block exchange succeeded
 *   Other code if internal or communication error has occured.
 *
 **/
SPROX_API_FUNC(Desfire_Command) (SPROX_PARAM  WORD accept_len, BYTE flags)
{
#ifdef DEBUG_CORE
  DWORD i;
#endif
  SPROX_RC status;

#ifdef SPROX_DESFIRE_WITH_SAM
  BYTE pbSamCmac[8];
  DWORD dwSamCmacLength = sizeof(pbSamCmac);
#endif

  SPROX_DESFIRE_GET_CTX();



  if (ctx->xfer_length > DF_MAX_INFO_FRAME_SIZE)
  {
#ifdef DEBUG_CORE
    printf("INTERNAL OVERFLOW (%ld>%ld)\n", ctx->xfer_length, DF_MAX_INFO_FRAME_SIZE);
#endif
    return DFCARD_LIB_CALL_ERROR;
  }

  if (ctx->session_type & KEY_ISO_MODE)
    if (flags & COMPUTE_COMMAND_CMAC)
        Desfire_XferCmacSend(SPROX_PARAM_P  (flags & APPEND_COMMAND_CMAC) ? TRUE : FALSE);

#ifdef DEBUG_CORE
  printf("DSF(%03d)<", ctx->xfer_length);
  for (i=0; i<ctx->xfer_length; i++)
  {
    if (i == 1)
      printf(" ");
    printf("%02X", ctx->xfer_buffer[i]);
  }
  printf("\n");
#endif

  status = SPROX_API_CALL(Desfire_Exchange) (SPROX_PARAM_PV);

  if (status != DF_OPERATION_OK)
  {
#ifdef DEBUG_CORE
    printf("DSF>: %ld (%08lX)\n", status, status);
#endif
    return status;
  }

#ifdef DEBUG_CORE
  printf("DSF(%03d)>:", ctx->xfer_length);
  for (i=0; i<ctx->xfer_length; i++)
  {
    if (i == 1)
      printf(" ");
    printf("%02X", ctx->xfer_buffer[i]);
  }
  printf("\n");
#endif

  /* Check whether the PICC's response consists of at least one byte.
     If the response is empty, we can not even determine the PICC's status.
     Therefore an empty response is always a length error. */
  if ((ctx->xfer_length < 1) || (ctx->xfer_length > MAX_INFO_FRAME_SIZE))
  {
    /* Error: block with inappropriate number of bytes received from the PICC. */
    return DFCARD_WRONG_LENGTH;
  }

  /*  Check the status byte received from the PICC.
     Depending on which of the flags (sgbWANTS_OPERATION_OK and sgbWANTS_ADDITIONAL_FRAME)
     are set in the bExpectedStatusFlags parameter, we accept either the status byte
     sgbOPERATION_OK or the status byte sgbDF_ADDITIONAL_FRAME or both of them. */
  if (!(((ctx->xfer_buffer[0] == DF_OPERATION_OK) && (flags & WANTS_OPERATION_OK)) ||
        ((ctx->xfer_buffer[0] == DF_ADDITIONAL_FRAME) && (flags & WANTS_ADDITIONAL_FRAME))))
  {
    /* Error: error or unexpected status received from the PICC. */
    return (DFCARD_ERROR - ctx->xfer_buffer[0]);
  }

  if (ctx->session_type & KEY_ISO_MODE)
  {
    if (flags & (CHECK_RESPONSE_CMAC|LOOSE_RESPONSE_CMAC))
    {
      /* Go into CMAC stuff */
      /* If authenticated with SAM, Desfire_XferCmacRecv uses the SAM*/
      if (Desfire_XferCmacRecv(SPROX_PARAM_PV) != DF_OPERATION_OK)
      {
        if (flags & LOOSE_RESPONSE_CMAC)
        {
#ifdef SPROX_DESFIRE_WITH_SAM
          if (!ctx->sam_session_active)
#endif
          {
            memcpy(ctx->init_vector, &ctx->xfer_buffer[ctx->xfer_length], 8);
          }

        } else
        {
          return DFCARD_WRONG_MAC;
        }


      }
    }
  }

  /* Finally check the length of the response. */
  if ((accept_len != 0) && (accept_len != ctx->xfer_length))
  {
    /* Error: block with inappropriate number of bytes received from the PICC. */
    return DFCARD_WRONG_LENGTH;
  }

  return DF_OPERATION_OK;
}

