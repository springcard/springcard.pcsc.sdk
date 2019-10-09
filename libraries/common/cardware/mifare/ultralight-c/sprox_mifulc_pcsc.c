/**h* MifUlCAPI/PCSC
 *
 * NAME
 *   MifUlCAPI :: PC/SC functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   MIFULC communication over PC/SC API.
 *
 **/
#include "sprox_mifulc_i.h"

#ifdef _USE_PCSC

#ifdef WIN32
  #pragma comment(lib,"winscard.lib")
#endif


//#define DEBUG_PCSC

SPROX_RC MifUlC_Command(SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len)
{
  LONG    status;
  WORD    sw;

  BYTE    recv_buffer[128];
  DWORD   recv_length = sizeof(recv_buffer);

#ifdef DEBUG_PCSC
  DWORD i;
#endif
  
  SPROX_MIFULC_GET_CTX();

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


SPROX_RC MifUlC_CommandEncapsulated(SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len)
{
  LONG    status;
  WORD    sw;

  BYTE    send_buffer[128];
  DWORD   send_length;
  BYTE    recv_buffer[128];
  DWORD   recv_length = sizeof(recv_buffer);

#ifdef DEBUG_PCSC
  DWORD i;
#endif
  
  SPROX_MIFULC_GET_CTX();

  if (rsp_got_len != NULL)
    *rsp_got_len = 0;

  if (cmd_len > 64)
    return SCARD_E_INSUFFICIENT_BUFFER;

  send_buffer[0] = 0xFF; /* CLA */
  send_buffer[1] = 0xFE; /* INS */
  send_buffer[2] = 0x01; /* P1 */
  send_buffer[3] = 0x08; /* P2 */
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

SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_SuspendTracking(SCARDHANDLE hCard)
{
  BYTE buffer[5] = { 0xFF, 0xFB, 0x01, 0x00, 0x00};
  SPROX_MIFULC_GET_CTX();
  return MifUlC_Command(hCard, buffer, sizeof(buffer), NULL, 0, NULL);
}

SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_ResumeTracking(SCARDHANDLE hCard)
{
  BYTE buffer[5] = { 0xFF, 0xFB, 0x00, 0x00, 0x00};
  SPROX_MIFULC_GET_CTX();
  return MifUlC_Command(hCard, buffer, sizeof(buffer), NULL, 0, NULL);
}

SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_WakeUp(SCARDHANDLE hCard)
{
  BYTE buffer[5] = {  0xFF, 0xFE, 0x01, 0x08, 0x00}; /* Command Encapsulated empty */
  SPROX_MIFULC_GET_CTX();
  return MifUlC_Command(hCard, buffer, sizeof(buffer), NULL, 0, NULL);
}

#define MAX_INSTANCES 16

typedef struct
{
  BOOL                  bInited;
  SCARDHANDLE           hCard;

} INSTANCE_ST;

INSTANCE_ST instances[MAX_INSTANCES];

/**f* MifUlCAPI/[PCSC]AttachLibrary
 *
 * NAME
 *   [PCSC]AttachLibrary
 *
 * DESCRIPTION
 *   Associates the Desfire DLL with a smartcard connected in PC/SC.
 *   Call this function immediately after SCardConnect to be able to
 *   use pcsc_mifulc.dll functions.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifulc.dll]]
 *   Not applicable.
 *
 *   [[sprox_mifulc_ex.dll]]
 *   Not applicable. See [Legacy]AttachLibrary
 *
 *   [[pcsc_mifulc.dll]]
 *   LONG  SCardMifUlC_AttachLibrary (SCARDHANDLE hCard);
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
SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_AttachLibrary(SCARDHANDLE hCard)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if ((instances[i].bInited) && (instances[i].hCard == hCard))
      goto take_this_one;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (!instances[i].bInited)
      goto take_this_one;
  
  return SCARD_E_SERVER_TOO_BUSY;
  
take_this_one:

  instances[i].hCard   = hCard;
  instances[i].bInited = TRUE;
 
  return SCARD_S_SUCCESS;
}

/**f* MifUlCAPI/[PCSC]DetachLibrary
 *
 * NAME
 *   [PCSC]DetachLibrary
 *
 * DESCRIPTION
 *   Remove the attachement between the Desfire DLL and a PC/SC smartcard.
 *   Call this function immediately before SCardDisconnect.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifulc.dll]]
 *   Not applicable.
 *
 *   [[sprox_mifulc_ex.dll]]
 *   Not applicable. See [Legacy]DetachLibrary.
 *
 *   [[pcsc_mifulc.dll]]
 *   LONG  SCardMifUlC_DetachLibrary (SCARDHANDLE hCard);
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
SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_DetachLibrary(SCARDHANDLE hCard)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
  {
    if (instances[i].hCard == hCard)
    {
      instances[i].hCard   = 0;
      instances[i].bInited = FALSE;
      return SCARD_S_SUCCESS;
    }
  }

  return SCARD_E_INVALID_HANDLE;  
}

#endif
