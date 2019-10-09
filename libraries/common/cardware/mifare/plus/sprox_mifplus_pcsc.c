/**h* MifPlusAPI/PCSC
 *
 * NAME
 *   MifPlusAPI :: PC/SC functions
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Mifare Plus communication over PC/SC API.
 *
 * HISTORY
 *   23/02/2011 JDA : initial release
 *   28/03/2011 JDA : changed ATR-related rules to accept Mifare Plus S (and not only S)
 *
 **/
#include "sprox_mifplus_i.h"

#ifdef _USE_PCSC

#ifdef WIN32
  #pragma comment(lib,"winscard.lib")
#endif

#define DEBUG_PCSC

SPROX_API_FUNC(MifPlus_CommandEx) (SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len)
{
  LONG    status;
  WORD    sw;

  BYTE    recv_buffer[256+2];
  DWORD   recv_length = sizeof(recv_buffer);

#ifdef DEBUG_PCSC
  DWORD i;
#endif
  
  SPROX_MIFPLUS_GET_CTX();

  if (rsp_got_len != NULL)
    *rsp_got_len = 0;

#ifdef DEBUG_PCSC
  printf("CSB<:");
  for (i=0; i<cmd_len; i++)
  {
    if ( (i == 4) || (i == 5))
      printf(" ");

    printf("%02X", cmd_buffer[i]);
  }
  printf("\n");
#endif
  
  status = SCardTransmit(hCard,
                         SCARD_PCI_T1,
                         cmd_buffer,
                         cmd_len,
                         NULL,
                         recv_buffer,
                         &recv_length);
                         
  
  if (status != SCARD_S_SUCCESS)
  {
#ifdef DEBUG_PCSC
    printf("CSB>: %ld (%08lX)\n", status, status);
#endif
    return status;
  }

#ifdef DEBUG_PCSC
  printf("CSB>:");
  for (i=0; i<recv_length; i++)
  {
    if ( (i > 0) && (i == (recv_length-2)) )
      printf(" ");
    printf("%02X", recv_buffer[i]);
  }
  printf("\n");
#endif

 
  /* Check status word */
  /* ----------------- */
 
  if (recv_length < 2)
    return SCARD_E_UNEXPECTED;

  sw  = recv_buffer[recv_length-2]; sw <<= 8;
  sw |= recv_buffer[recv_length-1];

  if (sw == 0x9000)
  {
    recv_length -= 2;

		if (rsp_buffer == NULL)
		{
      if (rsp_got_len != NULL)
        *rsp_got_len = (WORD) recv_length;
		} else
    if (recv_length > rsp_max_len)
    {
      status = SCARD_E_INSUFFICIENT_BUFFER;
    } else
    {
      memcpy(rsp_buffer, recv_buffer, recv_length);
      if (rsp_got_len != NULL)
        *rsp_got_len = (WORD) recv_length;
    }
  } else
  if ((sw & 0xFF00) == 0x6F00)
  {
    status = 0 - (signed char) (sw & 0x00FF);
  } else
  {
    status = SCARD_E_CARD_UNSUPPORTED;
  }

  return status;
}


SPROX_API_FUNC(MifPlus_CommandEncapsulated) (SPROX_PARAM  BYTE _P1, BYTE _P2, const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len)
{
  LONG    status;
  WORD    sw;

  BYTE    send_buffer[256+5+1];
  DWORD   send_length;
  BYTE    recv_buffer[252+2];
  DWORD   recv_length = sizeof(recv_buffer);

#ifdef DEBUG_PCSC
  DWORD i;
#endif
  
  SPROX_MIFPLUS_GET_CTX();

  if (rsp_got_len != NULL)
    *rsp_got_len = 0;

  if (cmd_len > 64)
    return SCARD_E_INSUFFICIENT_BUFFER;

  send_buffer[0] = 0xFF; /* CLA */
  send_buffer[1] = 0xFE; /* INS */
  send_buffer[2] = _P1; /* P1 */
  send_buffer[3] = _P2; /* P2 */
  send_buffer[4] = (BYTE) cmd_len; /* Lc */
  memcpy(&send_buffer[5], cmd_buffer, cmd_len);

  send_length = 5 + cmd_len;

#ifdef DEBUG_PCSC
  printf("CSB<:");
  for (i=0; i<send_length; i++)
  {
    if ( (i == 4) || (i == 5))
      printf(" ");

    printf("%02X", send_buffer[i]);
  }
  printf("\n");
#endif
  
  status = SCardTransmit(hCard,
                         SCARD_PCI_T1,
                         send_buffer,
                         send_length,
                         NULL,
                         recv_buffer,
                         &recv_length);
                         
  
  if (status != SCARD_S_SUCCESS)
  {
#ifdef DEBUG_PCSC
    printf("CSB>: %ld (%08lX)\n", status, status);
#endif
    return status;
  }

#ifdef DEBUG_PCSC
  printf("CSB>:");
  for (i=0; i<recv_length; i++)
  {
    if ( (i > 0) && (i == (recv_length-2)) )
      printf(" ");
    printf("%02X", recv_buffer[i]);
  }
  printf("\n");
#endif

 
  /* Check status word */
  /* ----------------- */
 
  if (recv_length < 2)
    return SCARD_E_UNEXPECTED;

  sw  = recv_buffer[recv_length-2]; sw <<= 8;
  sw |= recv_buffer[recv_length-1];
  
  if (sw == 0x9000)
  {
    recv_length -= 2;

    if (recv_length > rsp_max_len)
    {
      status = SCARD_E_INSUFFICIENT_BUFFER;
    } else
    {
      if (rsp_buffer != NULL)
        memcpy(rsp_buffer, recv_buffer, recv_length);

      if (rsp_got_len != NULL)
        *rsp_got_len = (WORD) recv_length;
    }
  } else
  if ((sw & 0xFF00) == 0x6F00)
  {
    status = 0 - (signed char) (sw & 0x00FF);
  } else
  {
    status = SCARD_E_CARD_UNSUPPORTED;
  }

  return status;
}

SPROX_API_FUNC(MifPlus_Command) (SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len)
{
	SPROX_MIFPLUS_GET_CTX();

#ifdef _WITH_SELFTEST
	if (in_selftest)
		return MifPlus_CommandHook(SPROX_PARAM_P  cmd_buffer, cmd_len, rsp_buffer, rsp_max_len, rsp_got_len);
#endif

	if (ctx->tcl)
		return SPROX_API_CALL(MifPlus_CommandEncapsulated) (SPROX_PARAM_P  0x00, 0x00, cmd_buffer, cmd_len, rsp_buffer, rsp_max_len, rsp_got_len);
	else
		return SPROX_API_CALL(MifPlus_CommandEncapsulated) (SPROX_PARAM_P  0x01, 0x07, cmd_buffer, cmd_len, rsp_buffer, rsp_max_len, rsp_got_len);
}

SPROX_API_FUNC(MifPlus_GetData) (SCARDHANDLE hCard, BYTE _P1, BYTE _P2, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len)
{
  BYTE cmd_buffer[5] = { 0xFF, 0xCA, 0xCC, 0xCC, 0x00};
  SPROX_MIFPLUS_GET_CTX();

	cmd_buffer[2] = _P1;
	cmd_buffer[3] = _P2;

  return SPROX_API_CALL(MifPlus_CommandEx) (hCard, cmd_buffer, sizeof(cmd_buffer), rsp_buffer, rsp_max_len, rsp_got_len);
}

SPROX_API_FUNC(MifPlus_SlotControl) (SPROX_PARAM  BYTE _P1, BYTE _P2)
{
  BYTE buffer[5] = { 0xFF, 0xFB, 0xCC, 0xCC, 0x00};
  SPROX_MIFPLUS_GET_CTX();

	buffer[2] = _P1;
	buffer[3] = _P2;

  return SPROX_API_CALL(MifPlus_CommandEx) (SPROX_PARAM_P  buffer, sizeof(buffer), NULL, 0, NULL);
}



SPROX_API_FUNC(MifPlus_SuspendTracking) (SPROX_PARAM_V)
{
  SPROX_MIFPLUS_GET_CTX();
  return SPROX_API_CALL(MifPlus_SlotControl) (SPROX_PARAM_P  0x01, 0x00);
}

SPROX_API_FUNC(MifPlus_ResumeTracking) (SPROX_PARAM_V)
{
  SPROX_MIFPLUS_GET_CTX();
  return SPROX_API_CALL(MifPlus_SlotControl) (SPROX_PARAM_P  0x00, 0x00);
}

SPROX_API_FUNC(MifPlus_WakeUp) (SPROX_PARAM_V)
{
  BYTE buffer[5] = {  0xFF, 0xFE, 0x01, 0x08, 0x00}; /* Command Encapsulated empty */
  SPROX_MIFPLUS_GET_CTX();
  return SPROX_API_CALL(MifPlus_CommandEx) (SPROX_PARAM_P  buffer, sizeof(buffer), NULL, 0, NULL);
}

#define MAX_INSTANCES 16

typedef struct
{
  BOOL                  bInited;
  SCARDHANDLE           hCard;
	SPROX_MIFPLUS_CTX_ST  mifplus_ctx;

} INSTANCE_ST;

INSTANCE_ST instances[MAX_INSTANCES];


SPROX_MIFPLUS_CTX_ST *mifplus_get_ctx(SCARDHANDLE hCard)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].hCard == hCard)
      return &instances[i].mifplus_ctx;

  return NULL;  
}

/**f* MifPlusAPI/[PCSC]AttachLibrary
 *
 * NAME
 *   [PCSC]AttachLibrary
 *
 * DESCRIPTION
 *   Associates the Mifare Plus DLL with a smartcard connected in PC/SC.
 *   Call this function immediately after SCardConnect to be able to
 *   use pcsc_mifplus.dll functions.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   Not applicable.
 *
 *   [[sprox_mifplus_ex.dll]]
 *   Not applicable. See [Legacy]AttachLibrary
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_AttachLibrary (SCARDHANDLE hCard);
 *
 * INPUTS
 *   SCARDHANDLE hCard  : handle of the card
 *
 * RETURNS
 *   SCARD_S_SUCCESS    : library attached
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   [PCSC]DetachLibrary
 *
 **/
static const BYTE MifPlus_ATR_Tcl_Begin[] = MIFPLUS_ATR_TCL_BEGIN;
static const BYTE MifPlus_ATR_Raw_Begin[] = MIFPLUS_ATR_RAW_BEGIN;

SPROX_API_FUNC(MifPlus_AttachLibrary) (SCARDHANDLE hCard)
{
  BYTE i;
	BYTE atr[32];
	DWORD atrlen;
	BYTE cardinfo[32];
	WORD cardinfolen;
	LONG rc;

	rc = SCardStatus(hCard, NULL, NULL, NULL, NULL, atr, &atrlen);
	if (rc != MFP_SUCCESS)
	  return rc;
 
  for (i=0; i<MAX_INSTANCES; i++)
    if ((instances[i].bInited) && (instances[i].hCard == hCard))
      goto take_this_one;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (!instances[i].bInited)
      goto take_this_one;
  
  return SCARD_E_SERVER_TOO_BUSY;
  
take_this_one:

  instances[i].hCard   = hCard;
  memset(&instances[i].mifplus_ctx, 0, sizeof(instances[i].mifplus_ctx));
  instances[i].bInited = TRUE;

  /* For now we don't know the card's Level (we're not sure this is actually a Mifare Plus, either...) */
	instances[i].mifplus_ctx.level = 0xFF;

	if ((atrlen > sizeof(MifPlus_ATR_Tcl_Begin))
	 && !memcmp(atr, MifPlus_ATR_Tcl_Begin, sizeof(MifPlus_ATR_Tcl_Begin)))
	{
	  /* Could be a Mifare Plus T=CL, in level 0 or level 3 */
		/* -------------------------------------------------- */
		
		/* Is it actually a Mifare Plus ? */
		if ((atr[sizeof(MifPlus_ATR_Tcl_Begin)] & 0xF0) != 0x20)
		  return MFP_LIB_UNKNOWN_CARD_LEVEL;

		instances[i].mifplus_ctx.tcl   = TRUE;

		/* If the card accepts DeselectVirtualCard command, it must run at Level 3 */
		rc = SPROX_API_CALL(MifPlus_DeselectVirtualCard)(hCard);
		switch (rc)
		{
		  case MFP_SUCCESS : instances[i].mifplus_ctx.level = 3; break;
		  case MFP_ERROR-MFP_ERR_CONDITION_OF_USE : instances[i].mifplus_ctx.level = 0; break;
		  default : return MFP_LIB_UNKNOWN_CARD_LEVEL;
		}

  } else
	if ((atrlen > sizeof(MifPlus_ATR_Raw_Begin))
	 && !memcmp(atr, MifPlus_ATR_Raw_Begin, sizeof(MifPlus_ATR_Raw_Begin)))
  {
	  /* Not T=CL -> level 1 or level 2, based on SAK */		
		/* -------------------------------------------- */

		instances[i].mifplus_ctx.tcl = FALSE;

    /* Get SAK */
		rc = SCardMifPlus_GetData(hCard, 0xF0, 0x00, cardinfo, sizeof(cardinfo), &cardinfolen);
		if (rc != MFP_SUCCESS)
		{
			instances[i].bInited = FALSE;
			return rc;
		}

    /* Now take a decision */
		switch (cardinfo[2] | 0x10)
		{
      case MFP_SAK_LEVEL_1 : instances[i].mifplus_ctx.level = 1; break;
			case MFP_SAK_LEVEL_2 : instances[i].mifplus_ctx.level = 2; break;
			default : return MFP_LIB_UNKNOWN_CARD_LEVEL;
		}
	} else
	{
	  /* Unrecognized ATR ? */
		/* ------------------ */

    return MFP_LIB_UNKNOWN_CARD_LEVEL;
	}
 
  return SCARD_S_SUCCESS;
}

/**f* MifPlusAPI/[PCSC]DetachLibrary
 *
 * NAME
 *   [PCSC]DetachLibrary
 *
 * DESCRIPTION
 *   Remove the attachement between the Mifare Plus DLL and a PC/SC smartcard.
 *   Call this function immediately before SCardDisconnect.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   Not applicable.
 *
 *   [[sprox_mifplus_ex.dll]]
 *   Not applicable. See [Legacy]DetachLibrary.
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_DetachLibrary (SCARDHANDLE hCard);
 *
 * INPUTS
 *   SCARDHANDLE hCard      : handle of the card
 *
 * RETURNS
 *   SCARD_S_SUCCESS        : library detached
 *   SCARD_E_INVALID_HANDLE : invalid handle
 *
 * SEE ALSO
 *   [PCSC]AttachLibrary
 *
 **/
SPROX_API_FUNC(MifPlus_DetachLibrary) (SCARDHANDLE hCard)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
  {
    if (instances[i].hCard == hCard)
    {
			memset(&instances[i].mifplus_ctx, 0, sizeof(instances[i].mifplus_ctx));
      instances[i].hCard   = 0;
      instances[i].bInited = FALSE;
      return SCARD_S_SUCCESS;
    }
  }

  return SCARD_E_INVALID_HANDLE;  
}

#endif
