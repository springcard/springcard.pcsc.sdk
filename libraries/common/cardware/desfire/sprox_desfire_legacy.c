/**h* DesfireAPI/Legacy
 *
 * NAME
 *   DesfireAPI :: Specific entry points for Legacy re-entrant and non-reentrant DLL
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * SEE ALSO
 *   LegacyEX
 *
 **/

#include "sprox_desfire_i.h"

#ifndef _USE_PCSC

#ifndef SPROX_API_REENTRANT 
	SPROX_DESFIRE_CTX_ST desfire_ctx = 
	{
	  0xFF, // BYTE  tcl_cid;
	  DF_ISO_WRAPPING_OFF, // iso_wrapping;
	
	  0, // current_aid;
	
	  KEY_EMPTY, // session_type;
	  { 0 }, // session_key[24];
	  0, // session_key_id;
	
	  FALSE, // xfer_fast;
	  0, // xfer_length;
	  { 0 }, // xfer_buffer[64];
	
	  { 0 }, // init_vector[16];
	
	  { 0 }, // cmac_subkey_1[16];
	  { 0 }, // cmac_subkey_2[16];
	
	  { 0 }, // cipher_context;
	
#ifdef SPROX_DESFIRE_WITH_SAM  
    FALSE, // sam_session_active;
    NULL,  // *sam_context;
#endif
  
	};
#endif

/**f* DesfireAPI/[Legacy]SelectCid
 *
 * NAME
 *   [Legacy]SelectCid
 *
 * DESCRIPTION
 *   Selects the logical number of the addressed DesFire card
 *   CID is used to distinguish several Desfire cards simultaneously selected by a single reader
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_SelectCid (BYTE cid);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_SelectCid(SPROX_INSTANCE rInst, BYTE cid);
 *
 *   [[pcsc_desfire.dll]]
 *   Not applicable.
 * 
 * INPUTS
 *   BYTE cid        : logical number of the addressed DesFire card
 *
 * RETURNS
 *   DF_OPERATION_OK : CID selected
 *
 **/
SPROX_API_FUNC(Desfire_SelectCid) (SPROX_PARAM  BYTE cid)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_CleanupAuthentication(SPROX_PARAM_PV);
  ctx->tcl_cid = cid;

  return DF_OPERATION_OK;
}


/**f* DesfireAPI/[Legacy]IsoWrapping
 *
 * NAME
 *   [Legacy]IsoWrapping
 *
 * DESCRIPTION
 *   Select the wrapping mode of Desfire legacy commands into ISO 7816-4 APDUs.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoWrapping (BYTE mode);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoWrapping(SPROX_INSTANCE rInst, BYTE mode);
 *
 *   [[pcsc_desfire.dll]]
 *   Not applicable. See [PC/SC]IsoWrapping.
 *
 * INPUTS
 *   BYTE mode             : ISO wrapping mode (DF_ISO_WRAPPING_CARD or DF_ISO_WRAPPING_READER)
 *
 * RETURNS
 *   DF_OPERATION_OK       : ISO wrapping mode set
 *   DFCARD_LIB_CALL_ERROR : ISO wrapping mode not set, mode not available
 *
 * NOTES
 *   DF_ISO_WRAPPING_CARD uses the card's wrapping method (see chapter 3.13 
 *   of datasheet of mifare DesFire MF3ICD40 for more information).
 *
 *   DF_ISO_WRAPPING_OFF disables the wrapping (native legacy command are used).
 *   This is the default mode.
 *
 **/

SPROX_API_FUNC(Desfire_IsoWrapping) (SPROX_PARAM  BYTE mode)
{
  SPROX_DESFIRE_GET_CTX();
  
  switch (mode)
  {
    case DF_ISO_WRAPPING_OFF    : break;
    case DF_ISO_WRAPPING_CARD   : break;

    case DF_ISO_WRAPPING_READER :
    default                     : return DFCARD_LIB_CALL_ERROR;    
  }

  ctx->iso_wrapping = mode;
  return DF_OPERATION_OK;
}

SPROX_API_FUNC(Desfire_Exchange) (SPROX_PARAM_V)
{
  SPROX_RC status;
  BYTE     send_buffer[256];
  WORD     send_length;
  BYTE     recv_buffer[256];
  WORD     recv_length = sizeof(recv_buffer);
  SPROX_DESFIRE_GET_CTX();
  
  if (ctx->xfer_length > 65535)
    return DFCARD_WRONG_LENGTH;
    
  if (ctx->iso_wrapping == DF_ISO_WRAPPING_CARD)
  {
    DWORD l = sizeof(send_buffer);

    if (!BuildIsoApdu(ctx->xfer_buffer, ctx->xfer_length, send_buffer, &l, FALSE))
      return DFCARD_LIB_CALL_ERROR;
      
    send_length = (WORD) l;
		ctx->xfer_fast = FALSE;
    
  } else
  {
    memcpy(send_buffer, ctx->xfer_buffer, ctx->xfer_length);
    send_length = (WORD) ctx->xfer_length;
  }

  if (ctx->xfer_fast)
  {
    status = SPROX_API_CALL(TclA_ExchangeDF) (SPROX_PARAM_P  ctx->tcl_cid, send_buffer, send_length, recv_buffer, &recv_length);
  } else
  {
    status = SPROX_API_CALL(TclA_Exchange) (SPROX_PARAM_P  5, ctx->tcl_cid, 0xFF, send_buffer, send_length, recv_buffer, &recv_length);
  }

	if (status != MI_OK)
	  return status;
 
  /* Check status word */
  /* ----------------- */
 
  if (ctx->iso_wrapping == DF_ISO_WRAPPING_CARD)
  {
    if ((recv_length < 2) || (recv_length > (MAX_INFO_FRAME_SIZE + 1)))
      return DFCARD_WRONG_LENGTH;

    /* SW1 must be 91YY */    
    if (recv_buffer[recv_length-2] != 0x91)
      return DFCARD_PCSC_BAD_RESP_SW;
     
    /* SW2 is the card status */
    ctx->xfer_buffer[0] = recv_buffer[recv_length-1];

    /* Retrieve card's data */
    memcpy(&ctx->xfer_buffer[1], recv_buffer, recv_length - 2);
    ctx->xfer_length = recv_length - 1;
    
  } else
  {
    if ((recv_length < 1) || (recv_length > MAX_INFO_FRAME_SIZE))
      return DFCARD_WRONG_LENGTH;

    memcpy(ctx->xfer_buffer, recv_buffer, recv_length);
    ctx->xfer_length = recv_length;
  }

  return status;
}


#endif
