/**h* DesfireAPI/PCSC
 *
 * NAME
 *   DesfireAPI :: PC/SC functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   DESFIRE communication over PC/SC API.
 *
 **/
#include "sprox_desfire_i.h"

#include "build.h"

#ifdef _USE_PCSC

#ifdef WIN32
  #pragma comment(lib,"winscard.lib")
#endif

//#define _DEBUG_PCSC

SPROX_API_FUNC(Desfire_Exchange) (SPROX_PARAM_V)
{
  LONG    status;

  BYTE    card_status;
  WORD    resp_data_len;
  BYTE    *resp_data_ptr;

  BYTE    send_buffer[256];
  DWORD   send_length;
  BYTE    recv_buffer[256];
  DWORD   recv_length = sizeof(recv_buffer);

#ifdef _DEBUG_PCSC
  DWORD i;
#endif

  SPROX_DESFIRE_GET_CTX();

  if (ctx->xfer_length > 200)
    return SCARD_E_INSUFFICIENT_BUFFER;

  /* Build the APDU */
  /* -------------- */
  if (ctx->iso_wrapping == DF_ISO_WRAPPING_CARD)
  {
    DWORD l = sizeof(send_buffer);

    if (!BuildIsoApdu(ctx->xfer_buffer, ctx->xfer_length, send_buffer, &l, FALSE))
      return DFCARD_LIB_CALL_ERROR;

    send_length = l;

  } else
  if (ctx->iso_wrapping == DF_ISO_WRAPPING_READER)
  {
    DWORD l = sizeof(send_buffer);

    if (!BuildIsoApdu(ctx->xfer_buffer, ctx->xfer_length, send_buffer, &l, TRUE))
      return DFCARD_LIB_CALL_ERROR;

    send_length = l;

  } else
  {
    memcpy(send_buffer, ctx->xfer_buffer, ctx->xfer_length);
    send_length = ctx->xfer_length;
  }

#ifdef _DEBUG_PCSC
  printf("CSB<:");
  for (i=0; i<send_length; i++)
  {
    if ( (i == 4) || (i == 5) || (i == (send_length-1)) )
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
#ifdef _DEBUG_PCSC
    printf("CSB>: %ld (%08lX)\n", status, status);
#endif
    return status;
  }

#ifdef _DEBUG_PCSC
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

  if (ctx->iso_wrapping == DF_ISO_WRAPPING_CARD)
  {
    if (recv_length < 2)
      return DFCARD_PCSC_BAD_RESP_LEN;

    /* SW1 must be 91YY */
    if (recv_buffer[recv_length-2] != 0x91)
      return DFCARD_PCSC_BAD_RESP_SW;

    /* SW2 is the card status */
    card_status  = recv_buffer[recv_length-1];

    /* Retrieve card's data */
    resp_data_len = (WORD) (recv_length - 2);
    resp_data_ptr = &recv_buffer[0];

  } else
  if (ctx->iso_wrapping == DF_ISO_WRAPPING_READER)
  {
    /* SW must be 9000 */
    WORD sw;

    if (recv_length < 2)
      return DFCARD_PCSC_BAD_RESP_LEN;

    sw = (recv_buffer[recv_length-2] * 0x0100) | recv_buffer[recv_length-1];
#ifdef _DEBUG_PCSC
    printf("SW=%02X%02X\n", recv_buffer[recv_length-2], recv_buffer[recv_length-1]);
#endif

    switch (sw)
    {
      case 0x9000 : break;

      case 0x6F01 :
      case 0x6F3D :
      case 0x6F51 :
      case 0x6F52 :
                    return SCARD_W_REMOVED_CARD;

      case 0x6F02 :
      case 0x6F3E :
                    return SCARD_E_COMM_DATA_LOST;

      case 0x6F47 :
                    return SCARD_E_CARD_UNSUPPORTED;

      default     : return DFCARD_PCSC_BAD_RESP_SW;
    }

    if (recv_length < 3)
      return DFCARD_WRONG_LENGTH;

    /* Card status word is at the beginning of the buffer */
    card_status   = recv_buffer[0];

    /* Retrieve card's data */
    resp_data_len = (WORD) (recv_length - 3);
    resp_data_ptr = &recv_buffer[1];

  } else
  {
    if (recv_length < 1)
      return DFCARD_WRONG_LENGTH;

    card_status   = recv_buffer[0];
    resp_data_len = (WORD) (recv_length - 1);
    resp_data_ptr = &recv_buffer[1];
  }

  /* Remember received size */
  ctx->xfer_length = resp_data_len + 1;

  /* Check whether the PICC's response consists of at least one byte.
     If the response is empty, we can not even determine the PICC's status.
     Therefore an empty response is always a length error. */
  if ((ctx->xfer_length < 1) || (ctx->xfer_length > MAX_INFO_FRAME_SIZE))
  {
    /* Error: block with inappropriate number of bytes received from the PICC. */
    return DFCARD_WRONG_LENGTH;
  }

  /* Copy the buffer */
  memcpy(&ctx->xfer_buffer[1], resp_data_ptr, ctx->xfer_length);
  ctx->xfer_buffer[0] = card_status;

#ifdef _DEBUG_PCSC
  printf("DSF(%03d)>", ctx->xfer_length);
  for (i=0; i<ctx->xfer_length; i++)
  {
    if (i == 1)
      printf(" ");
    printf("%02X", ctx->xfer_buffer[i]);
  }
  printf("\n");
#endif

  return DF_OPERATION_OK;
}

INSTANCE_ST instances[MAX_INSTANCES];

/**f* DesfireAPI/[PCSC]AttachLibrary
 *
 * NAME
 *   [PCSC]AttachLibrary
 *
 * DESCRIPTION
 *   Associates the Desfire DLL with a smartcard connected in PC/SC.
 *   Call this function immediately after SCardConnect to be able to
 *   use pcsc_desfire.dll functions.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   Not applicable.
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable. See [Legacy]AttachLibrary
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_AttachLibrary (SCARDHANDLE hCard);
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
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_AttachLibrary(SCARDHANDLE hCard)
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

  instances[i].desfire_ctx = malloc(sizeof(SPROX_DESFIRE_CTX_ST));
  if (instances[i].desfire_ctx == NULL)
    return SCARD_E_NO_MEMORY;

  instances[i].hCard   = hCard;
  instances[i].bInited = TRUE;

  memset(instances[i].desfire_ctx, 0, sizeof(SPROX_DESFIRE_CTX_ST));

  instances[i].desfire_ctx->session_type = KEY_EMPTY;
  instances[i].desfire_ctx->iso_wrapping = DF_ISO_WRAPPING_CARD;

  return SCARD_S_SUCCESS;
}

/**f* DesfireAPI/[PCSC]DetachLibrary
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
 *   [[sprox_desfire.dll]]
 *   Not applicable.
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable. See [Legacy]DetachLibrary.
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_DetachLibrary (SCARDHANDLE hCard);
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
SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_DetachLibrary(SCARDHANDLE hCard)
{
  BYTE i;

  for (i=0; i<MAX_INSTANCES; i++)
  {
    if (instances[i].hCard == hCard)
    {
      if (instances[i].desfire_ctx != NULL)
        free(instances[i].desfire_ctx);
      instances[i].desfire_ctx = NULL;
      instances[i].hCard   = 0;
      instances[i].bInited = FALSE;
      return SCARD_S_SUCCESS;
    }
  }

  return SCARD_E_INVALID_HANDLE;
}

/**f* DesfireAPI/[PCSC]IsoWrapping
 *
 * NAME
 *   [PCSC]IsoWrapping
 *
 * DESCRIPTION
 *   Select the wrapping mode of Desfire legacy commands into ISO 7816-4 APDUs.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   Not applicable. See [Legacy]IsoWrapping.
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable. See [Legacy]IsoWrapping.
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoWrapping (SCARDHANDLE hCard, BYTE mode);
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
 *   of datasheet of mifare DesFire MF3ICD40 for more information). This is
 *   the default setting.
 *
 *   DF_ISO_WRAPPING_READER uses the reader's wrapping method (for early releases
 *   of Desfire cards that doesn't offer the card's wrapping method). This works
 *   only with SpringCard CSB6 Family products.
 *
 **/
LONG SCardDesfire_IsoWrapping(SCARDHANDLE hCard, BYTE mode)
{
  SPROX_DESFIRE_GET_CTX();

  switch (mode)
  {
    case DF_ISO_WRAPPING_CARD   : break;
    case DF_ISO_WRAPPING_READER : break;

    case DF_ISO_WRAPPING_OFF    :
    default                     : return DFCARD_LIB_CALL_ERROR;
  }

  ctx->iso_wrapping = mode;
  return DF_OPERATION_OK;
}

SPROX_DESFIRE_LIB  const TCHAR *SPROX_DESFIRE_API SCardDesfire_GetLibraryVersion()
{
  return version;
}

SPROX_DESFIRE_CTX_ST *desfire_get_ctx(SCARDHANDLE hCard)
{
  BYTE i;

  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].hCard == hCard)
      return instances[i].desfire_ctx;

  return NULL;
}

#endif
