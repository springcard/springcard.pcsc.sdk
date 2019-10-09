/**h* MifPlusAPI/Legacy
 *
 * NAME
 *   MifPlusAPI :: Specific entry points for Legacy re-entrant and non-reentrant DLL
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * HISTORY
 *   23/02/2011 JDA : initial release
 *   28/03/2011 JDA : changed SAK-related rules to accept Mifare Plus S (and not only S)
 *
 **/
#include "sprox_mifplus_i.h"

#ifndef _USE_PCSC

#ifndef SPROX_API_REENTRANT 
	SPROX_MIFPLUS_CTX_ST mifplus_ctx = { 0 };
#endif

/**f* MifPlusAPI/[Legacy]SelectCid
 *
 * NAME
 *   [Legacy]SelectCid
 *
 * DESCRIPTION
 *   Selects the logical number of the addressed Mifare Plus card
 *   CID is used to distinguish several Mifare Plus cards simultaneously selected by a single reader
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_SelectCid (BYTE cid);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_SelectCid(SPROX_INSTANCE rInst, BYTE cid);
 *
 *   [[pcsc_mifplus.dll]]
 *   Not applicable.
 * 
 * INPUTS
 *   BYTE cid        : logical number of the addressed Mifare Plus card
 *
 **/
SPROX_API_FUNC(MifPlus_SelectCid) (SPROX_PARAM  BYTE cid)
{
  SPROX_MIFPLUS_GET_CTX();

  ctx->cid = cid;

  return MFP_SUCCESS;
}

/**f* MifPlusAPI/[Legacy]SelectCard
 *
 * NAME
 *   [Legacy]SelectCard
 *
 * DESCRIPTION
 *   Gives the Mifare Plus DLL the SAK and ATS of the Mifare Plus card.
 *   This is mandatory to detect the Level the card runs at.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_SelectCard(BYTE sak,
 *                                  const BYTE ats[],
 *                                  BYTE atslen);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_SelectCid(SPROX_INSTANCE rInst,
 *                                  BYTE sak,
 *                                  const BYTE ats[],
 *                                  BYTE atslen);
 *
 *   [[pcsc_mifplus.dll]]
 *   Not applicable.
 * 
 * INPUTS
 *   BYTE sak         : ISO 14443-3 A Select AKnowledge
 *   const BYTE ats[] : ISO 14443-4 A Answer To Select
 *   BYTE atslen      : size of ats
 *
 **/
SPROX_API_FUNC(MifPlus_SelectCard) (SPROX_PARAM  BYTE sak, const BYTE ats[], BYTE ats_len)
{
  SPROX_MIFPLUS_GET_CTX();
  
  if (ats == NULL)
    return MFP_LIB_CALL_ERROR;
    
  /* As the ATS is supplied, guess the card runs in T=CL */
  ctx->tcl = TRUE;
  
  if (sak == MFP_SAK_LEVEL_0_3)
	{
    /* Level 0 or level 3 ? call a Level 3 function to make the difference */
		SPROX_RC rc = SPROX_API_CALL(MifPlus_DeselectVirtualCard) (SPROX_PARAM_PV);
  
		switch (rc)
		{
		  case MFP_SUCCESS : ctx->level = 3; break;
		  case MFP_ERROR-MFP_ERR_CONDITION_OF_USE : ctx->level = 0; break;
		  default : return MFP_LIB_UNKNOWN_CARD_LEVEL;
		}

	} else
	if ((sak | 0x10) == MFP_SAK_LEVEL_1)
	{
	  /* Level 1 */
		ctx->level = 1;

	} else
	if ((sak | 0x10) == MFP_SAK_LEVEL_2)
	{
	  /* Level 2 */
		ctx->level = 2;

	} else
	{
	  return MFP_LIB_UNKNOWN_CARD_LEVEL;
	}
	
	return MFP_SUCCESS;
}


SPROX_API_FUNC(MifPlus_Command) (SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len)
{  
  SPROX_RC status;
  WORD rlen = rsp_max_len;
  SPROX_MIFPLUS_GET_CTX();
  
#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("MifPlus < ");
    for (i=0; i<cmd_len; i++)
      printf("%02X", cmd_buffer[i]);
    printf("\n");
  }
#endif
  
  if (ctx->tcl)
  {
    status = SPROX_API_CALL(Tcl_Exchange) (SPROX_PARAM_P  ctx->cid, cmd_buffer, cmd_len, rsp_buffer, &rlen);
  } else
  {
    rlen += 2;
    status = SPROX_API_CALL(A_Exchange) (SPROX_PARAM_P  cmd_buffer, (WORD) (cmd_len+2), rsp_buffer, &rlen, TRUE, 1024);
    if (rlen > 2) rlen -=2;
  }

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("MifPlus > ");
    for (i=0; i<rlen; i++)
      printf("%02X", rsp_buffer[i]);
    printf("\n");
  }
#endif
  
	if (status != MFP_SUCCESS)
	  return status;
	  
	if (rsp_got_len != NULL)
	  *rsp_got_len = rlen;

  return status;
}

#endif
