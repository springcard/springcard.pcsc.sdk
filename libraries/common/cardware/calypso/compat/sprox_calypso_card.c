/**h* SpringProxAPI/CalypsoCard
 *
 * NAME
 *   SpringProxAPI :: Working with Calypso cards (application level)
 *
 * NOTES
 *   Most parameters of the SPROX_Calypso_xxx functions directly map to Calypso
 *   card internal functions.
 *   Please refer to Calypso official documentation for any details.
 * 
 **/
#include "sprox_calypso_i.h"


static const BYTE select_mf_pdu[] = { 0x94, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00 };
static const BYTE select_app_pdu[] = { 0x94, 0xA4, 0x04, 0x00, 0x08, '1', 'T', 'I', 'C', '.', 'I', 'C', 'A', 0x00};

static BOOL TLVLoop(BYTE buffer[], WORD *offset, WORD *tag, WORD *length, BYTE *value[]);
static BOOL CalypsoDecodeSelAppResp(BYTE Rsp[], WORD RspLen, BYTE uid[8]);

/**f* SpringProxAPI/SPROX_Calypso_Select 
 *
 * NAME
 *   SPROX_Calypso_Select
 *
 * DESCRIPTION
 *   Select the smartcard slot for Calypso operation
 *
 * SYNOPSYS
 *   SWORD SPROX_Calypso_Select(BYTE slot);
 *
 * INPUTS
 *   BYTE slot                : smarcard slot number
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * NOTES
 *   Slot # 0 to 8 map to contact slots :
 *   0 = main slot (ISO 7816 size)
 *   1 = first SAM slot (micro SIM size)
 *   2 = second SAM slot
 *   ...
 *   For contactless operation, choose either
 *   CALYPSO_LEGACYRF_SLOT = Legacy Calypso protocol (Innovatron or type B')
 *   CALYPSO_ISO14443_SLOT = ISO-14443 protocol (type B)
 *
 * SEE ALSO
 *   CALYPSO_ISO14443_SLOT
 *   CALYPSO_LEGACYRF_SLOT
 *
 **/
SPROX_CALYPSO_FUNC_EX(Calypso_Select) (SPROX_PARAM  BYTE slot)
{
  SPROX_CALYPSO_GET_CTX();
  
  ctx->card_slot = slot;
  sprox_calypso_trace("SPROX_Calypso_Select(%d)", slot);

  return CALYPSO_SUCCESS;
}

/**f* SpringProxAPI/SPROX_Calypso_Connect 
 *
 * NAME
 *   SPROX_Calypso_Connect
 *
 * DESCRIPTION
 *   Connect to a Calypso card in the currently select slot
 *
 * SYNOPSYS
 *   SWORD SPROX_Calypso_Connect(BYTE card_uid[8]);
 *
 * OUTPUTS
 *   BYTE uid[8]              : UID of the found card (if one)
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error 
 *
 * NOTES
 *   This function tries to connect using the slot selected by
 *   SPROX_Calypso_Select.
 *   Contactless Calypso card may answer both to the CALYPSO_ISO14443_SLOT
 *   slot and to the CALYPSO_LEGACYRF_SLOT.
 *   Upper layer software must cope with this and ignore the second answer
 *   in necessary.
 *
 * SEE ALSO
 *   SPROX_Calypso_Select
 *   SPROX_Calypso_Disconnect
 *
 **/
SPROX_CALYPSO_FUNC_EX(Calypso_Connect) (SPROX_PARAM  BYTE uid[8])
{
  SWORD   rc;  
  SPROX_CALYPSO_GET_CTX();

  ctx->in_session = FALSE;

  if (ctx->card_slot == CALYPSO_ISO14443_SLOT)
  {
    BYTE    atq[11];
    WORD    length;    
    BYTE    buffer[128];

    SPROX_API_CALL(SetConfig) (SPROX_PARAM_P  CFG_MODE_ISO_14443_A);
    rc = SPROX_API_CALL(SetConfig) (SPROX_PARAM_P CFG_MODE_ISO_14443_B);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_SetConfig(B):rc=%d", rc);
      return rc;
    }

    rc = SPROX_API_CALL(B_SelectAny) (SPROX_PARAM_P  0x00, atq);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_B_SelectAny:rc=%d", rc);
      return rc;
    }
      
    rc = SPROX_API_CALL(TclB_Attrib) (SPROX_PARAM_P  atq, 0xFF);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_TclB_Attrib:rc=%d", rc);
      SPROX_API_CALL(TclB_Halt) (SPROX_PARAM_P  atq);
      return rc;      
    }

    length = sizeof(buffer);
    rc = SPROX_API_CALL(Tcl_Exchange) (SPROX_PARAM_P  0xFF, select_app_pdu, sizeof(select_app_pdu), buffer, &length);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_Tcl_Exchange:rc=%d", rc);
      SPROX_API_CALL(Tcl_Deselect) (SPROX_PARAM_P  0xFF);
      return rc;
    }
    
    if (length < 2)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SelectApplication:card answer is too short (%d)", length);
      return CALYPSO_ERR_PROTO;      
    }

    ctx->card_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
    length -= 2;

    if (ctx->card_sw != 0x9000)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SelectApplication:SW=%04X", ctx->card_sw);
      SPROX_API_CALL(Tcl_Deselect) (SPROX_PARAM_P  0xFF);
      return CALYPSO_ERR_STATUS;
    }
    
    if (!CalypsoDecodeSelAppResp(buffer, length, uid))
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:UID not found in FCI");
      SPROX_API_CALL(Tcl_Deselect) (SPROX_PARAM_P  0xFF);
      return CALYPSO_ERR_DATA;      
    }
         
  } else
  if (ctx->card_slot == CALYPSO_LEGACYRF_SLOT)
  {
    BYTE    length;
    BYTE    buffer[64];
    BYTE    picc_uid[4];

    rc = SPROX_API_CALL(ConfigureForCalypso) (SPROX_PARAM_PV);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_ConfigureForCalypso:rc=%d", rc);
      return rc;
    }

    length = sizeof(buffer);
    rc = SPROX_API_CALL(Calypso_APGEN) (SPROX_PARAM_P  0x00, TRUE, TRUE, 0xFF, buffer, &length, picc_uid);
    if (rc == MI_NOTAGERR) 
    {
      length = sizeof(buffer);
      rc = SPROX_API_CALL(Calypso_APGEN) (SPROX_PARAM_P  0x00, TRUE, TRUE, 0xFF, buffer, &length, picc_uid);
    }
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_Calypso_APGEN:rc=%d", rc);
      return rc;
    }

    length = sizeof(buffer);
    rc = SPROX_API_CALL(Calypso_COM_RA) (SPROX_PARAM_P  ctx->pcd_addr, ctx->picc_addr, picc_uid, select_mf_pdu, sizeof(select_mf_pdu), buffer, &length);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_Calypso_COM_RA:rc=%d", rc);
      return rc;
    }

    if (length < 6)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_Calypso_COM_RA(Select MF):card answer is too short (%d)", length);
      return CALYPSO_ERR_PROTO;      
    }

    if (uid != NULL)
    {
      memset(&uid[0], 0, 4);
      memcpy(&uid[4], picc_uid, 4);
    }

  } else
  {
    WORD    length;
    BYTE    buffer[64];
    
    length = sizeof(buffer);
    rc = SPROX_API_CALL(Card_PowerUp_Auto) (SPROX_PARAM_P  ctx->card_slot, buffer, &length);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_Card_PowerUp_Auto:rc=%d", rc);
      return rc;
    }
    
    length = sizeof(buffer);
    rc = SPROX_API_CALL(Card_Exchange) (SPROX_PARAM_P  ctx->card_slot, select_app_pdu, sizeof(select_app_pdu), buffer, &length);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SPROX_Card_Exchange:rc=%d", rc);
      return rc;
    }
    
    if (length < 2)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SelectApplication:card answer is too short (%d)", length);
      return CALYPSO_ERR_PROTO;      
    }

    ctx->card_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
    length -= 2;

    if (ctx->card_sw != 0x9000)
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:SelectApplication:SW=%04X", ctx->card_sw);
      return CALYPSO_ERR_STATUS;
    }
    
    if (!CalypsoDecodeSelAppResp(buffer, length, uid))
    {
      sprox_calypso_trace("SPROX_Calypso_Connect:UID not found in FCI");
      return CALYPSO_ERR_DATA;      
    }
    
  }
  
  return rc;     
}

/**f* SpringProxAPI/SPROX_Calypso_Disconnect 
 *
 * NAME
 *   SPROX_Calypso_Disconnect
 *
 * DESCRIPTION
 *   Disconnect from the currently active Calypso card
 *
 * SYNOPSYS
 *   SWORD SPROX_Calypso_Disconnect(void);
 *
 * RETURNS
 *   CALYPSO_SUCCESS           : success
 *   Other code on any error
 *
 * SEE ALSO
 *   SPROX_Calypso_Connect
 *
 **/
SPROX_CALYPSO_FUNC_EX(Calypso_Disconnect) (SPROX_PARAM_V)
{
  SWORD rc;
  SPROX_CALYPSO_GET_CTX();

  ctx->in_session = FALSE;
  
  if (ctx->card_slot == CALYPSO_ISO14443_SLOT)
  {
    rc = SPROX_API_CALL(Tcl_Deselect) (SPROX_PARAM_P  0xFF);    
    if (rc)
      sprox_calypso_trace("SPROX_Calypso_Disconnect:SPROX_Tcl_Deselect:rc=%d", rc);
    
  } else  
  if (ctx->card_slot == CALYPSO_LEGACYRF_SLOT)
  {
    rc = SPROX_API_CALL(Calypso_DISC) (SPROX_PARAM_P  ctx->pcd_addr, ctx->picc_addr);
    if (rc)
      sprox_calypso_trace("SPROX_Calypso_Disconnect:SPROX_Calypso_DISC:rc=%d", rc);
    
  } else
  {
    rc = SPROX_API_CALL(Card_PowerDown) (SPROX_PARAM_P  ctx->card_slot);
    if (rc)
      sprox_calypso_trace("SPROX_Calypso_Disconnect:SPROX_Card_PowerDown:rc=%d", rc);
    
  }
  
  return rc;
}

/**f* SpringProxAPI/SPROX_Calypso_Exchange 
 *
 * NAME
 *   SPROX_Calypso_Exchange
 *
 * DESCRIPTION
 *   Exchange an APDU with the currently active Calypso card
 *
 * SYNOPSYS
 *   SWORD SPROX_Calypso_Exchange(BYTE send_buffer[],
 *                                WORD send_length,
 *                                BYTE recv_buffer[],
 *                                WORD *recv_length);
 *
 * INPUTS
 *   BYTE  send_buffer[]      : APDU to be sent to the card
 *   WORD  send_length        : size of the APDU to be sent
 *   WORD *recv_length        : max size for recv_buffer
 * 
 * OUTPUTS
 *   BYTE  recv_buffer[]      : card's answer
 *   WORD *recv_length        : size of card's answer
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SIDE EFFECTS
 *   This function implicitly forward exchanged data with the SAM if a
 *   a secure session is active. See SPROX_Calypso_OpenSecureSession
 *   Result of SPROX_Calypso_SW is updated with last status word
 *
 * SEE ALSO
 *   SPROX_Calypso_Connect
 *
 **/
SPROX_CALYPSO_FUNC_EX(Calypso_Exchange) (SPROX_PARAM  const BYTE send_buffer[], WORD send_length, BYTE recv_buffer[], WORD *recv_length)
{
  SWORD  rc;
  BYTE   send_buffer_copy[64];
  BYTE   sam_buffer[256];
  WORD   sam_buflen;

  SPROX_CALYPSO_GET_CTX();
  
  if (send_buffer == NULL)
    return CALYPSO_ERR_FUNCTION_CALL;
  if (recv_buffer == NULL)
    return CALYPSO_ERR_FUNCTION_CALL;
  if (recv_length == NULL)
    return CALYPSO_ERR_FUNCTION_CALL;
  if (send_length > 64)
    return CALYPSO_ERR_FUNCTION_CALL;
  if (*recv_length < 2)
    return CALYPSO_ERR_FUNCTION_CALL;
    
  {
    WORD i;
    sprox_calypso_trace("SPROX_Calypso_Exchange: ");    
    sprox_calypso_trace("Card< ");
    for (i=0; i<send_length; i++)
      sprox_calypso_trace("_%02X ", send_buffer[i]);
    sprox_calypso_trace("\n");
  }
  
  /* Remember send_buffer (in case send and recv buffer are the same pointer...) */
  memcpy(send_buffer_copy, send_buffer, send_length);

  /* Perform the actual exchange */
  if (ctx->card_slot == CALYPSO_ISO14443_SLOT)
  {
    rc = SPROX_API_CALL(Tcl_Exchange) (SPROX_PARAM_P  0xFF, send_buffer, send_length, recv_buffer, recv_length);
    if (rc)
      sprox_calypso_trace("SPROX_Calypso_Exchange:SPROX_Tcl_Exchange:rc=%d", rc);
        
  } else
  if (ctx->card_slot == CALYPSO_LEGACYRF_SLOT)
  {
    BYTE    l = (BYTE) * recv_length;

    rc = SPROX_API_CALL(Calypso_COM_R) (SPROX_PARAM_P  ctx->pcd_addr, ctx->picc_addr, send_buffer, (BYTE) send_length, recv_buffer, &l);
    *recv_length = l;
    if (rc)
      sprox_calypso_trace("SPROX_Calypso_Exchange:SPROX_Calypso_COM_R:rc=%d", rc);

  } else
  {
    rc = SPROX_API_CALL(Card_Exchange) (SPROX_PARAM_P  ctx->card_slot, send_buffer, send_length, recv_buffer, recv_length);
    if (rc)
      sprox_calypso_trace("SPROX_Calypso_Exchange:SPROX_Card_Exchange:rc=%d", rc);
  }
 
  if (rc)
    return rc;


  {
    WORD i;
    sprox_calypso_trace("SPROX_Calypso_Exchange: ");
    sprox_calypso_trace("Card> ");
    for (i=0; i<*recv_length; i++)
      sprox_calypso_trace("_%02X ", recv_buffer[i]);
    sprox_calypso_trace("\n");
  }
   
  /* Retrive status word */ 
  if (*recv_length >= 2)
  {
    ctx->card_sw = recv_buffer[*recv_length - 2] * 0x0100 + recv_buffer[*recv_length - 1];    
  } else
  {
    ctx->card_sw = 0xFFFF;
  }

  if (ctx->in_session)
  {
    /* Forward IN and OUT APDUs to the SAM */
    sam_buflen = send_length;
    memcpy(&sam_buffer[0], send_buffer_copy, send_length);
    if (send_length <= 5)
    {
      memcpy(&sam_buffer[sam_buflen], recv_buffer, *recv_length-2);
      sam_buflen += *recv_length-2;
    }

    rc = SPROX_API_CALL(CalypsoSam_DigestUpdate) (SPROX_PARAM_P  sam_buffer, sam_buflen);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Exchange:SPROX_CalypsoSam_DigestUpdate:rc=%d", rc);
      return rc;
    }

    if (send_length > 5)
    {
      memcpy(&sam_buffer[0], recv_buffer, *recv_length);
      sam_buflen = *recv_length;
    } else
    {
      memcpy(&sam_buffer[0], &recv_buffer[*recv_length-2], 2);
      sam_buflen = 2;
    }

    rc = SPROX_API_CALL(CalypsoSam_DigestUpdate) (SPROX_PARAM_P  sam_buffer, sam_buflen);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_Exchange:SPROX_CalypsoSam_DigestUpdate:rc=%d", rc);
      return rc;
    }
  }

  return rc;
}

/**f* SpringProxAPI/SPROX_Calypso_OpenSecureSession 
 *
 * NAME
 *   SPROX_Calypso_OpenSecureSession
 *
 * DESCRIPTION
 *   Open a secure session on the currently active card
 *
 * SYNOPSYS
 *   SWORD SPROX_Calypso_OpenSecureSession(BYTE p1,
 *                                         BYTE p2,
 *                                         BYTE sam_chal[4],
 *                                         BYTE card_resp[],
 *                                         WORD *card_resp_size);
 *
 * INPUTS
 *   BYTE  p1                 : the P1 parameter for open secure session command,
 *                              as defined in Calypso card's specification
 *   BYTE  p2                 : the P2 parameter for open secure session command,
 *                              as defined in Calypso card's specification
 *   BYTE  sam_chal[4]        : the challenge provided by the Calypso SAM in answer
 *                              to SPROX_CalypsoSam_GetChallenge
 *   WORD *card_resp_length   : max size for card_resp_buffer
 * 
 * OUTPUTS
 *   BYTE  card_resp_buffer[] : card's answer (without the status word)
 *   WORD *card_resp_length   : size of card's answer
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SIDE EFFECTS
 *   Once a secure session has been opened, any exchange between card and application
 *   (through SPROX_Calypso_Exchange) are implicitly forwarded to the SAM
 *
 * SEE ALSO
 *   SPROX_CalypsoSam_DigestInit
 *   SPROX_Calypso_CloseSecureSession
 *   SPROX_Calypso_Exchange
 *
 **/
SPROX_CALYPSO_FUNC_EX(Calypso_OpenSecureSession) (SPROX_PARAM  BYTE p1, BYTE p2, const BYTE sam_chal[4], BYTE card_resp_buffer[], WORD *card_resp_length)
{
  BYTE    buffer[64];
  BYTE    l = 0;
  WORD    length = sizeof(buffer);
  SWORD   rc;

  SPROX_CALYPSO_GET_CTX();

  {
    sprox_calypso_trace("Card: Open secure session\n");
  }

  ctx->in_session = FALSE;

  buffer[0] = 0x94;
  buffer[1] = 0x8A;
  buffer[2] = p1;
  buffer[3] = p2;
  buffer[4] = 0x04;

  memcpy(&buffer[5], sam_chal, 4);

  rc = SPROX_API_CALL(Calypso_Exchange) (SPROX_PARAM_P  buffer, 5 + 4, buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:SPROX_Calypso_Exchange(94 8A %02X %02X 04):rc=%d", p1, p2, rc);
    return rc;
  }

  while ((length == 2) && (buffer[length - 2] == 0x6C))
  {
    /* GetResponse is mandatory */
    buffer[0] = 0x00;
    buffer[1] = 0xC0;
    buffer[2] = 0x00;
    buffer[3] = 0x00;
    buffer[4] = l;

    length = sizeof(buffer);
    rc = SPROX_API_CALL(Calypso_Exchange) (SPROX_PARAM_P  buffer, 5, buffer, &length);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:SPROX_Calypso_Exchange(GET RESP):rc=%d", rc);
      return rc;
    }

    l = buffer[length - 1];
  }

  if (length < 2)
  {
    sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;      
  }

  ctx->card_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];  
  if (ctx->card_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:SW=%04X", ctx->card_sw);
    
    if (ctx->card_sw == 0x6B00)
      return CALYPSO_ERR_PARAM;
    if (ctx->card_sw == 0x6700)
      return CALYPSO_ERR_LENGTH;
    if (ctx->card_sw == 0x6400)
      return CALYPSO_IN_SESSION;
    if (ctx->card_sw == 0x6985)
      return CALYPSO_ERR_DENIED;
    if (ctx->card_sw == 0x6A81)
      return CALYPSO_NO_SUCH_KEY;
    if (ctx->card_sw == 0x6A82)
      return CALYPSO_ERR_PARAM;
    if (ctx->card_sw == 0x6A83)
      return CALYPSO_ERR_PARAM;
      
    return CALYPSO_ERR_STATUS;
  }

  if (length < 6)
  {
    sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:bad length for challenge (%d)", length-2);
    return CALYPSO_ERR_DATA;
  } 
 
  
  switch (length - 2)
  {
    case  4 :
    case  5 :
    case 33 :    
    case 34 : sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:OK, last session ratified (%d)", length-2);
              break;
    
    case  6 :
    case  7 :
    case 35 :
    case 36 : sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:OK, last session NOT ratified (%d)", length-2);
              break;
    
    default : sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:OK, unhandled length (%d)", length-2);
              break;
  }


  if (card_resp_length != NULL)
    *card_resp_length = length - 2;
  if (card_resp_buffer != NULL)
    memcpy(card_resp_buffer, buffer, length - 2);
  ctx->in_session = TRUE;
  return rc;
}

/**f* SpringProxAPI/SPROX_Calypso_CloseSecureSession 
 *
 * NAME
 *   SPROX_Calypso_CloseSecureSession
 *
 * DESCRIPTION
 *   Close a secure session with the currently active card
 *
 * SYNOPSYS
 *   SWORD SPROX_Calypso_CloseSecureSession(BYTE sam_mac[4],
 *                                          BYTE card_mac[4]);
 *
 * INPUTS
 *   BYTE  sam_mac[4]         : the message authentication code provided by the Calypso SAM
 *                              in answer to SPROX_CalypsoSam_DigestClose
 * 
 * OUTPUTS
 *   BYTE  card_mac[4]        : card's answer (without the status word)
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   CALYPSO_WRONG_MAC        : bad signature (invalid sam_mac)
 *   Other code on any error
 *
 * NOTES
 *   Error CALYPSO_WRONG_MAC denotes a fatal security error.
 *   Most of the time, this is caused by SAM and card not sharing the same key (p1 and p2
 *   parameters for SPROX_Calypso_OpenSecureSession not matching kif parameter for
 *   SPROX_CalypsoSam_DigestInit, or bad kvc provided to SPROX_CalypsoSam_DigestInit,
 *   or why not card and SAM not coming from the same network).
 *   This can also be caused by an erronous exchange betwen application, card, and SAM
 *   (for example an exchange with the card not correctly forwared to the SAM).
 *   Whatever the reason, transaction is cancelled by the card (and must be cancelled
 *   by application as well).
 *
 *   On success, card_mac shall be passed to SPROX_CalypsoSam_DigestAuthenticate, to check
 *   that the card really knows the key, and isn't cheating.
 *
 * SEE ALSO
 *   SPROX_Calypso_OpenSecureSession
 *   SPROX_Calypso_Exchange
 *   SPROX_CalypsoSam_DigestAuthenticate
 *
 **/
SPROX_CALYPSO_FUNC_EX(Calypso_CloseSecureSession) (SPROX_PARAM  const BYTE sam_mac[4], BYTE card_mac[4])
{
  BYTE    buffer[64];
  BYTE    l = 0;
  WORD    length = sizeof(buffer);
  SWORD   rc;

  SPROX_CALYPSO_GET_CTX();

  ctx->in_session = FALSE;

  buffer[0] = 0x94;
  buffer[1] = 0x8E;
  buffer[2] = 0x00;
  buffer[3] = 0x00;
  buffer[4] = 0x04;

  memcpy(&buffer[5], sam_mac, 4);

  rc = SPROX_API_CALL(Calypso_Exchange) (SPROX_PARAM_P  buffer, 5 + 4, buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_Calypso_CloseSecureSession:SPROX_Calypso_Exchange(94 8E 00 00 04):rc=%d", rc);
    return rc;
  }

  while ((length == 2) && (buffer[length - 2] == 0x6C))
  {
    /* GetResponse is mandatory */
    buffer[0] = 0x00;
    buffer[1] = 0xC0;
    buffer[2] = 0x00;
    buffer[3] = 0x00;
    buffer[4] = l;

    length = sizeof(buffer);
    rc = SPROX_API_CALL(Calypso_Exchange) (SPROX_PARAM_P  buffer, 5, buffer, &length);
    if (rc)
    {
      sprox_calypso_trace("SPROX_Calypso_CloseSecureSession:SPROX_Calypso_Exchange(GET RESP):rc=%d", rc);
      return rc;
    }

    l = buffer[length - 1];
  }
  
  if (length < 2)
  {
    sprox_calypso_trace("SPROX_Calypso_CloseSecureSession:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;      
  }  

  ctx->card_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
  if (ctx->card_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_Calypso_CloseSecureSession:SW=%04X", ctx->card_sw);

    if (ctx->card_sw == 0x6B00)
      return CALYPSO_ERR_PARAM;
    if (ctx->card_sw == 0x6700)
      return CALYPSO_ERR_LENGTH;
    if (ctx->card_sw == 0x6982)
      return CALYPSO_WRONG_MAC;
    if (ctx->card_sw == 0x6985)
      return CALYPSO_NO_SESSION;

    return CALYPSO_ERR_STATUS;
  }

  if (length < 6)
  {
    sprox_calypso_trace("SPROX_Calypso_OpenSecureSession:bad length for MAC (%d)", length-2);
    return CALYPSO_ERR_DATA;
  }

  memcpy(card_mac, buffer, 4);

#if 0
  /* Send another frame to "ratify" the whole transaction sequence */
  length = sizeof(buffer);
  rc = SPROX_API_CALL(Calypso_Exchange) (SPROX_PARAM_P  select_mf_pdu, sizeof(select_mf_pdu), buffer, &length);
#endif
  return rc;
}

SPROX_CALYPSO_FUNC_EX2(WORD, Calypso_SW) (SPROX_PARAM_V)
{
  SPROX_CALYPSO_GET_CTX(); 
  return ctx->card_sw;
}

static BOOL CalypsoDecodeFCIDisc(BYTE FciDisc[], WORD FciDiscLen, BYTE uid[8])
{
  WORD offset, length;
  WORD tag;
  BYTE *value;
  BOOL rc = FALSE;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < FciDiscLen) && TLVLoop(FciDisc, &offset, &tag, &length, &value))
  {
    if (tag == 0xC7)
    {
      /* Full serial number */
      if (length <= 8)
      {
        if (uid != NULL)
        {
          memset(uid, 0, 8);
          memcpy(uid, &value[8-length], length);
        }
        rc = TRUE;
      }
    } else
    if (tag == 0x53)
    {
      /* Discretionary Data */
    }
  }
  
  return rc;
}

static BOOL CalypsoDecodeFCIProp(BYTE FciProp[], WORD FciPropLen, BYTE uid[8])
{
  WORD offset, length;
  WORD tag;
  BYTE *value;
  BOOL rc = FALSE;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < FciPropLen) && TLVLoop(FciProp, &offset, &tag, &length, &value))
  {
    if (tag == 0xBF0C)
    {
      /* This is the FCI Issuer Discreationary template */
      if (CalypsoDecodeFCIDisc(value, length, uid)) rc = TRUE;
    }
  }
  
  return rc;
}

static BOOL CalypsoDecodeFCI(BYTE Fci[], WORD FciLen, BYTE uid[8])
{
  WORD offset, length;
  WORD tag;
  BYTE *value;
  BOOL rc = FALSE;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < FciLen) && TLVLoop(Fci, &offset, &tag, &length, &value))
  {
    if ((tag == 0x84) && (length <= 16))
    {
      /* DF name */
    } else
    if (tag == 0xA5)
    {
      /* This is the FCI Proprietary template */
      if (CalypsoDecodeFCIProp(value, length, uid)) rc = TRUE;
    }
  }
  
  return rc;
}

static BOOL CalypsoDecodeSelAppResp(BYTE Rsp[], WORD RspLen, BYTE uid[8])
{
  WORD offset, length;
  WORD tag;
  BYTE *value;
  BOOL rc = FALSE;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < RspLen) && TLVLoop(Rsp, &offset, &tag, &length, &value))
  {
    if (tag = 0x6F)
    {
      /* This is the FCI template */
      if (CalypsoDecodeFCI(value, length, uid)) rc = TRUE;
    }
  }
  
  return rc;
}

/*
 * TLVLoop
 * --------
 * A really (too much ?) simple ASN1 T,L,V parser
 */
static BOOL TLVLoop(BYTE buffer[], WORD *offset, WORD *tag, WORD *length, BYTE *value[])
{
  WORD t;
  WORD o, l;

  if (buffer == NULL)
    return FALSE;

  if (offset != NULL)
    o = *offset;
  else
    o = 0;

  if ((buffer[o] == 0x00) || (buffer[o] == 0xFF))
    return FALSE;

  /* Read the tag */
  if ((buffer[o] & 0x1F) != 0x1F)
  {
    /* Short tag */
    t = buffer[o++];
  } else
  {
    /* Long tag */
    t = buffer[o++];
    t <<= 8;
    t |= buffer[o++];
  }

  if (tag != NULL)
    *tag = t;

  /* Read the length */
  if (buffer[o] & 0x80)
  {
    /* Multi-byte lenght */
    switch (buffer[o++] & 0x7F)
    {
      case 0x01:
        l = buffer[o++];
        break;
      case 0x02:
        l = buffer[o++];
        l <<= 8;
        l += buffer[o++];
        break;
      default:
        return FALSE;
    }
  } else
  {
    /* Length on a single byte */
    l = buffer[o++];
  }

  if (l > 65535)
    return FALSE;              /* Overflow */

  if (length != NULL)
    *length = (WORD) l;

  /* Get a pointer on data */
  if (value != NULL)
    *value = &buffer[o];

  /* Jump to the end of data */
  o += l;

  if (offset != NULL)
    *offset = (WORD) o;

  return TRUE;
}
