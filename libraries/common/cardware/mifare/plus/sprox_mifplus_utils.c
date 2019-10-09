/**h* MifPlusAPI/Utils
 *
 * NAME
 *   MifPlusAPI :: Utilities
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Utilities to control the card and the reader
 *
 **/
#include "sprox_mifplus_i.h"


SPROX_RC MifPlus_Result(BYTE rsp_buffer[], WORD rsp_got_len, WORD rsp_expect_len)
{
	if (rsp_got_len == 0)
		return MFP_EMPTY_CARD_ANSWER;

	if (rsp_buffer[0] != MFP_ERR_SUCCESS)
		return MFP_ERROR-rsp_buffer[0];

	if (rsp_got_len != (rsp_expect_len + 1))
		return MFP_WRONG_CARD_LENGTH;

	return MFP_SUCCESS;
}

/**f* MifPlusAPI/GetCardLevel
 *
 * NAME
 *   GetCardLevel
 *
 * DESCRIPTION
 *   Return the security Level of the connected card
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_GetCardLevel(BYTE *level);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_GetCardLevel(SPROX_INSTANCE rInst,
 *                                     BYTE *level)
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_GetCardLevel(SCARDHANDLE hCard,
 *                                   BYTE *level)
 *
 **/
SPROX_API_FUNC(MifPlus_GetCardLevel) (SPROX_PARAM  BYTE *level)
{
  SPROX_MIFPLUS_GET_CTX();

	if (level != NULL)
	  *level = ctx->level;

  return MFP_SUCCESS;
}

/**f* MifPlusAPI/EnterTcl
 *
 * NAME
 *   EnterTcl
 *
 * DESCRIPTION
 *   Set the card to ISO 14443-4 level (T=CL)
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_EnterTcl(void);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_EnterTcl(SPROX_INSTANCE rInst)
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_EnterTcl(SCARDHANDLE hCard)
 *
 * SEE ALSO
 *   LeaveTcl
 *
 **/
SPROX_API_FUNC(MifPlus_EnterTcl) (SPROX_PARAM_V)
{
  SPROX_RC rc;
  SPROX_MIFPLUS_GET_CTX();

	if (ctx->tcl)
	  return MFP_SUCCESS;

#ifdef _USE_PCSC
  rc = SCardMifPlus_SlotControl(hCard, 0x20, 0x01);
#else
  rc = SPROX_API_CALL(TclA_GetAts) (SPROX_PARAM_P  ctx->cid, NULL, NULL);
#endif

	if (rc == MFP_SUCCESS)
	  ctx->tcl = TRUE;

	return rc;
}

/**f* MifPlusAPI/LeaveTcl
 *
 * NAME
 *   LeaveTcl
 *
 * DESCRIPTION
 *   Set the card to ISO 14443-3 level (raw communication)
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_LeaveTcl(void);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_LeaveTcl(SPROX_INSTANCE rInst)
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_LeaveTcl(SCARDHANDLE hCard)
 *
 * SEE ALSO
 *   EnterTcl
 *
 **/
SPROX_API_FUNC(MifPlus_LeaveTcl) (SPROX_PARAM_V)
{
  SPROX_RC rc;
  SPROX_MIFPLUS_GET_CTX();

	if (!ctx->tcl)
	  return MFP_SUCCESS;

#ifdef _USE_PCSC
  rc = SCardMifPlus_SlotControl(hCard, 0x20, 0x00);
#else
  rc = SPROX_API_CALL(Tcl_Deselect) (SPROX_PARAM_P  ctx->cid);
#endif
	
	if (rc == MFP_SUCCESS)
	  ctx->tcl = FALSE;

  /* The card is now halted, we must wake-it up */
#ifdef _USE_PCSC
  
#else
  rc = SPROX_API_CALL(A_SelectAgain) (SPROX_PARAM_P  NULL, 0);
#endif

	return rc;
}

