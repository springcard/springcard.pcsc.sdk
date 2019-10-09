/**h* SpringProxAPI/CalypsoSam
 *
 * NAME
 *   SpringProxAPI :: Working with Calypso SAMs
 *
 * NOTES
 *   Most parameters of the SPROX_CalypsoSam_xxx functions directly map to Calypso
 *   SAM internal functions.
 *   Please refer to Calypso official documentation for any details.
 * 
 **/
#include "sprox_calypso_i.h"

/**f* SpringProxAPI/SPROX_CalypsoSam_Select 
 *
 * NAME
 *   SPROX_CalypsoSam_Select
 *
 * DESCRIPTION
 *   Select the smartcard slot where the Calypso SAM is to be found
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_Select(BYTE slot);
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
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_Select) (SPROX_PARAM  BYTE slot)
{
  SPROX_CALYPSO_GET_CTX();

  ctx->sam_slot = slot;
  sprox_calypso_trace("SPROX_CalypsoSam_Select(%d)", slot);

  return 0;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_Connect 
 *
 * NAME
 *   SPROX_CalypsoSam_Connect
 *
 * DESCRIPTION
 *   Connect to a Calypso SAM in the currently select slot
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_Connect(BYTE atr_buffer[],
 *                                  WORD *atr_length);
 *
 * INPUTS
 *   WORD *atr_length         : max size for atr_buffer
 *
 * OUTPUTS
 *   BYTE  atr_buffer[]       : ATR of the found SAM (if one)
 *   WORD *atr_length         : size of SAM's ATR
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SEE ALSO
 *   SPROX_CalypsoSam_Select
 *   SPROX_CalypsoSam_Disconnect
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_Connect) (SPROX_PARAM  BYTE atr_buffer[], WORD *atr_length)
{
  SWORD rc;  
  SPROX_CALYPSO_GET_CTX();

  rc = SPROX_API_CALL(Card_PowerUp_Auto) (SPROX_PARAM_P  ctx->sam_slot, atr_buffer, atr_length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_Connect:SPROX_Card_PowerUp_Auto:rc=%d", rc);
  } else
  {
    SPROX_API_CALL(Bi_SamFastSpeed) (SPROX_PARAM_P  ctx->sam_slot);
  }     

  return rc;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_Disconnect 
 *
 * NAME
 *   SPROX_CalypsoSam_Disconnect
 *
 * DESCRIPTION
 *   Disconnect from the currently active Calypso SAM
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_Disconnect(void);
 *
 * RETURNS
 *   CALYPSO_SUCCESS            : success
 *   Other code on any error
 *
 * SEE ALSO
 *   SPROX_CalypsoSam_Connect
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_Disconnect) (SPROX_PARAM_V)
{
  SWORD rc;
  SPROX_CALYPSO_GET_CTX();

  rc = SPROX_API_CALL(Card_PowerDown) (SPROX_PARAM_P  ctx->sam_slot);
  if (rc)
    sprox_calypso_trace("SPROX_CalypsoSam_Disconnect:SPROX_Card_PowerDown:rc=%d", rc);
  return rc;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_Exchange 
 *
 * NAME
 *   SPROX_CalypsoSam_Exchange
 *
 * DESCRIPTION
 *   Exchange an APDU with the currently active Calypso SAM
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_Exchange(BYTE send_buffer[],
 *                                   WORD send_length,
 *                                   BYTE recv_buffer[],
 *                                   WORD *recv_length);
 *
 * INPUTS
 *   BYTE  send_buffer[]      : APDU to be sent to the SAM
 *   WORD  send_length        : size of the APDU to be sent
 *   WORD *recv_length        : max size for recv_buffer
 * 
 * OUTPUTS
 *   BYTE  recv_buffer[]      : SAM's answer
 *   WORD *recv_length        : size of SAM's answer
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SIDE EFFECTS
 *   Result of SPROX_CalypsoSam_SW is updated with last status word
 *
 * SEE ALSO
 *   SPROX_CalypsoSam_Connect
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_Exchange) (SPROX_PARAM  const BYTE send_buffer[],
                                                         WORD send_length,
                                                         BYTE recv_buffer[],
                                                         WORD *recv_length)
{
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  ctx->sam_sw = 0xFFFF;

  {
    WORD i;
    sprox_calypso_trace("SPROX_CalypsoSam_Exchange: ");
    sprox_calypso_trace("SAM < ");
    for (i=0; i<send_length; i++)
      sprox_calypso_trace("_%02X ", send_buffer[i]);
    sprox_calypso_trace("\n");
  }

  rc = SPROX_API_CALL(Card_Exchange) (SPROX_PARAM_P  ctx->sam_slot, send_buffer, send_length, recv_buffer, recv_length);

  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_Exchange:SPROX_Card_Exchange:rc=%d", rc);
    return rc;
  }
  
  {   
    WORD i;
    sprox_calypso_trace("SPROX_CalypsoSam_Exchange: ");
    sprox_calypso_trace("SAM > ");
    for (i=0; i<*recv_length; i++)
      sprox_calypso_trace("_%02X ", recv_buffer[i]);
    sprox_calypso_trace("\n");
  }

  if (*recv_length >= 2)
  {
    ctx->sam_sw = recv_buffer[*recv_length - 2] * 0x0100 + recv_buffer[*recv_length - 1];    
  } else
  {
    ctx->sam_sw = 0xFFFF;
  }

  return rc;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_SelectDiversifier
 *
 * NAME
 *   SPROX_CalypsoSam_SelectDiversifier
 *
 * DESCRIPTION
 *   Feed the SAM with current card's UID
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_SelectDiversifier(BYTE card_uid[8]);
 *
 * INPUTS
 *   BYTE  card_uid[8]        : serial number of the Calypso card, as returned by
 *                              SPROX_Calypso_Connect
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SEE ALSO
 *   SPROX_CalypsoSam_GetChallenge
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_SelectDiversifier) (SPROX_PARAM  const BYTE card_uid[8])
{
  BYTE    buffer[64];
  WORD    length = sizeof(buffer);
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  {
    sprox_calypso_trace("SAM : Select Diversifier\n");
  }

  buffer[0] = 0x94;
  buffer[1] = 0x14;
  buffer[2] = 0x00;
  buffer[3] = 0x00;
  buffer[4] = 0x08;

  memcpy(&buffer[5], card_uid, 8);
  rc = SPROX_API_CALL(CalypsoSam_Exchange) (SPROX_PARAM_P  buffer, 5 + 8, buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_SelectDiversifier_Atr:SPROX_CalypsoSam_Exchange:rc=%d", rc);
    return rc;
  }
  if (length < 2)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_SelectDiversifier_Atr:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;
  }

  ctx->sam_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
  if (ctx->sam_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_SelectDiversifier_Atr:SW=%04X", ctx->sam_sw);

    if (ctx->sam_sw == 0x6985)
      return CALYPSO_SAM_LOCKED;
    if (ctx->sam_sw == 0x6700)
      return CALYPSO_ERR_LENGTH;
      
    return CALYPSO_ERR_STATUS;
  }

  return rc;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_GetChallenge 
 *
 * NAME
 *   SPROX_CalypsoSam_GetChallenge
 *
 * DESCRIPTION
 *   Ask SAM to create a new nounce in order to open a secure session with a card
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_GetChallenge(BYTE sam_chal[4]);
 *
 * OUTPUTS
 *   BYTE  sam_chal[4]        : SAM's nounce, to be forwarded to the card in
 *                              SPROX_Calypso_OpenSecureSession
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SEE ALSO
 *   SPROX_CalypsoSam_SelectDiversifier_Atr
 *   SPROX_Calypso_OpenSecureSession
 *   SPROX_CalypsoSam_DigestInit
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_GetChallenge) (SPROX_PARAM  BYTE sam_chal[4])
{
  BYTE    buffer[64];
  WORD    length = sizeof(buffer);
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  {
    sprox_calypso_trace("SAM : Get Challenge\n");
  }

  buffer[0] = 0x94;
  buffer[1] = 0x84;
  buffer[2] = 0x00;
  buffer[3] = 0x00;
  buffer[4] = 0x04;

  rc = SPROX_API_CALL(CalypsoSam_Exchange) (SPROX_PARAM_P  buffer, 5, buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_GetChallenge:SPROX_CalypsoSam_Exchange:rc=%d", rc);
    return rc;
  }

  if (length < 2)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_GetChallenge:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;
  }

  ctx->sam_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];    
  if (ctx->sam_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_GetChallenge:SW=%04X", ctx->sam_sw);
    return CALYPSO_ERR_STATUS;
  }

  if (length != 6)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_GetChallenge:bad length for challenge (%d)", length-2);
    return CALYPSO_ERR_DATA;
  }

  memcpy(sam_chal, buffer, 4);
  return rc;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_DigestInit 
 *
 * NAME
 *   SPROX_CalypsoSam_DigestInit
 *
 * DESCRIPTION
 *   Activates a session in the SAM, and initializes its digest
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_DigestInit(BYTE kif,
 *                                     BYTE kvc,
 *                                     BYTE card_resp_buffer[],
 *                                     WORD card_resp_length);
 *
 * INPUTS
 *   BYTE  kif                : key identifier
 *   BYTE  kvc                : key version (as returned by the card in answer to
 *                              SPROX_Calypso_OpenSecureSession with P1's bit 7 set)
 *   BYTE  card_resp_buffer[] : card's answer to SPROX_Calypso_OpenSecureSession
 *   WORD  card_resp_length   : size of card's answer 
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SEE ALSO
 *   SPROX_CalypsoSam_DigestInit_Old
 *   SPROX_CalypsoSam_GetChallenge
 *   SPROX_Calypso_OpenSecureSession
 *   SPROX_CalypsoSam_DigestClose
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_DigestInit) (SPROX_PARAM  BYTE kif,
                                                           BYTE kvc,
                                                           const BYTE card_resp_buffer[],
                                                           WORD card_resp_length)
{
  BYTE    buffer[256];
  WORD    length = sizeof(buffer);
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  {
    sprox_calypso_trace("SAM : Digest init\n");
  }

  buffer[0] = 0x94;
  buffer[1] = 0x8A;
  buffer[2] = 0x00;
  buffer[3] = 0xFF;
  buffer[4] = (BYTE) (2+card_resp_length);
  buffer[5] = kif;
  buffer[6] = kvc;

  memcpy(&buffer[7], card_resp_buffer, card_resp_length);
  rc = SPROX_API_CALL(CalypsoSam_Exchange) (SPROX_PARAM_P  buffer, (WORD) (7 + card_resp_length), buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestInit:SPROX_CalypsoSam_Exchange:rc=%d", rc);
    return rc;
  }
  
  if (length < 2)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestInit:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;
  }  

  ctx->sam_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
  if (ctx->sam_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestInit:SW=%04X", ctx->sam_sw);
    
    if (ctx->sam_sw == 0x6600)
      return CALYPSO_ERR_PARAM;
    if (ctx->sam_sw == 0x6700)
      return CALYPSO_ERR_LENGTH;
    if (ctx->sam_sw == 0x6900)
      return CALYPSO_ERR_COUNTER;
    if (ctx->sam_sw == 0x6985)
      return CALYPSO_SAM_LOCKED;
    if (ctx->sam_sw == 0x6A00)
      return CALYPSO_ERR_PARAM;
    if (ctx->sam_sw == 0x6A83)
      return CALYPSO_NO_SUCH_KEY;
   
    return CALYPSO_ERR_STATUS;
  }

  return rc;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_DigestInit_Old
 *
 * NAME
 *   SPROX_CalypsoSam_DigestInit_Old
 *
 * DESCRIPTION
 *   Activates a session in the SAM, and initializes its digest, using absolute
 *   record number to select the key
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_DigestInit_Old(BYTE key_record,
 *                                         BYTE card_resp_buffer[],
 *                                         WORD card_resp_length);
 *
 * INPUTS
 *   BYTE  key_record         : key record number
 *   BYTE  card_resp_buffer[] : card's answer to SPROX_Calypso_OpenSecureSession
 *   WORD  card_resp_length   : size of card's answer 
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SEE ALSO
 *   SPROX_CalypsoSam_DigestInit
 *   SPROX_CalypsoSam_GetChallenge
 *   SPROX_Calypso_OpenSecureSession
 *   SPROX_CalypsoSam_DigestClose
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_DigestInit_Old) (SPROX_PARAM   BYTE key_record,
                                                                const BYTE card_resp_buffer[],
                                                                WORD card_resp_length)
{
  BYTE    buffer[256];
  WORD    length = sizeof(buffer);
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  {
    sprox_calypso_trace("SAM : Digest init\n");
  }

  buffer[0] = 0x94;
  buffer[1] = 0x8A;
  buffer[2] = 0x00;
  buffer[3] = key_record;
  buffer[4] = (BYTE) card_resp_length;

  memcpy(&buffer[5], card_resp_buffer, card_resp_length);
  rc = SPROX_API_CALL(CalypsoSam_Exchange) (SPROX_PARAM_P  buffer, (WORD) (5 + card_resp_length), buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestInit_Old:SPROX_CalypsoSam_Exchange:rc=%d", rc);
    return rc;
  }
  
  if (length < 2)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestInit_Old:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;
  }  

  ctx->sam_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
  if (ctx->sam_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestInit_Old:SW=%04X", ctx->sam_sw);
    
    if (ctx->sam_sw == 0x6600)
      return CALYPSO_ERR_PARAM;
    if (ctx->sam_sw == 0x6700)
      return CALYPSO_ERR_LENGTH;
    if (ctx->sam_sw == 0x6900)
      return CALYPSO_ERR_COUNTER;
    if (ctx->sam_sw == 0x6985)
      return CALYPSO_SAM_LOCKED;
    if (ctx->sam_sw == 0x6A00)
      return CALYPSO_ERR_PARAM;
    if (ctx->sam_sw == 0x6A83)
      return CALYPSO_NO_SUCH_KEY;
   
    return CALYPSO_ERR_STATUS;
  }

  return rc;
}

SPROX_CALYPSO_FUNC_EX(CalypsoSam_DigestUpdate) (SPROX_PARAM  const BYTE *card_buffer,
                                                             WORD card_buflen)
{
  BYTE    buffer[128];
  WORD    length = sizeof(buffer);
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  {
    sprox_calypso_trace("SAM : Digest update\n");
  }

  buffer[0] = 0x94;
  buffer[1] = 0x8C;
  buffer[2] = 0x00;
  buffer[3] = 0x00;
  buffer[4] = (BYTE) card_buflen;
  memcpy(&buffer[5], card_buffer, card_buflen);

  rc = SPROX_API_CALL(CalypsoSam_Exchange) (SPROX_PARAM_P  buffer, (WORD) (5 + card_buflen), buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestUpdate:SPROX_CalypsoSam_Exchange:rc=%d", rc);
    return rc;
  }

  if (length < 2)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestUpdate:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;
  }  

  ctx->sam_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
  if (ctx->sam_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestUpdate:SW=%04X", ctx->sam_sw);
    
    if (ctx->sam_sw == 0x6700)
      return CALYPSO_ERR_LENGTH;
    if (ctx->sam_sw == 0x6985)
      return CALYPSO_NO_SESSION;

    return CALYPSO_ERR_STATUS;
  }

  return rc;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_DigestClose 
 *
 * NAME
 *   SPROX_CalypsoSam_DigestClose
 *
 * DESCRIPTION
 *   Ask the SAM to generate the certificate to send to the card to close a session
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_DigestClose(BYTE sam_mac[4]);
 *
 * OUTPUTS
 *   BYTE  sam_mac[]          : MAC computed by the SAM, to be forwarded to the card
 *                              through SPROX_Calypso_CloseSecureSession
 *
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   Other code on any error
 *
 * SEE ALSO
 *   SPROX_Calypso_CloseSecureSession
 *   SPROX_CalypsoSam_DigestOpen
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_DigestClose) (SPROX_PARAM  BYTE sam_mac[4])
{
  BYTE    buffer[64];
  WORD    length = sizeof(buffer);
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  {
    sprox_calypso_trace("Digest close...\n");
  }

  buffer[0] = 0x94;
  buffer[1] = 0x8E;
  buffer[2] = 0x00;
  buffer[3] = 0x00;
  buffer[4] = 0x04;

  rc = SPROX_API_CALL(CalypsoSam_Exchange) (SPROX_PARAM_P  buffer, 5, buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestClose:SPROX_CalypsoSam_Exchange:rc=%d", rc);
    return rc;
  }

  if (length < 2)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestClose:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;
  }  

  ctx->sam_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
  if (ctx->sam_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestClose:SW=%04X", ctx->sam_sw);
    
    if (ctx->sam_sw == 0x6985)
      return CALYPSO_NO_SESSION;
    if (ctx->sam_sw == 0x6700)
      return CALYPSO_ERR_LENGTH;

    return CALYPSO_ERR_STATUS;
  }

  if (length < 6)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestClose:bad length for MAC (%d)", length-2);    
    return CALYPSO_ERR_DATA;
  }

  memcpy(sam_mac, buffer, 4);
  return rc;
}

/**f* SpringProxAPI/SPROX_CalypsoSam_DigestAuthenticate 
 *
 * NAME
 *   SPROX_CalypsoSam_DigestAuthenticate
 *
 * DESCRIPTION
 *   Verify the MAC returned by the card after closing the secure session
 *
 * SYNOPSYS
 *   SWORD SPROX_CalypsoSam_DigestAuthenticate(BYTE card_mac[4]);
 *
 * INPUTS
 *   BYTE  card_mac[4]        : the message authentication code provided by the Calypso card
 *                              in answer to SPROX_Calypso_CloseSecureSession
 * 
 * RETURNS
 *   CALYPSO_SUCCESS          : success
 *   CALYPSO_WRONG_MAC        : bad signature (invalid card_mac)
 *   Other code on any error
 *
 * NOTES
 *   Error CALYPSO_WRONG_MAC denotes a fatal security error.
 *   As this comes after a successfull call to SPROX_Calypso_CloseSecureSession, the
 *   card did accepted our cryptogram, but replied with an invalid one.
 *   This must mean that the card has been forged !!!
 *
 * SEE ALSO
 *   SPROX_Calypso_CloseSecureSession
 *   SPROX_CalypsoSam_DigestClose
 *
 **/
SPROX_CALYPSO_FUNC_EX(CalypsoSam_DigestAuthenticate) (SPROX_PARAM  const BYTE card_mac[4])
{
  BYTE    buffer[64];
  WORD    length = sizeof(buffer);
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  buffer[0] = 0x94;
  buffer[1] = 0x82;
  buffer[2] = 0x00;
  buffer[3] = 0x00;
  buffer[4] = 0x04;

  memcpy(&buffer[5], card_mac, 4);
  rc = SPROX_API_CALL(CalypsoSam_Exchange) (SPROX_PARAM_P  buffer, 5 + 4, buffer, &length);
  if (rc)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestAuthenticate:SPROX_CalypsoSam_Exchange:rc=%d", rc);
    return rc;
  }
  
  if (length < 2)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestAuthenticate:card answer is too short (%d)", length);
    return CALYPSO_ERR_PROTO;
  }   

  ctx->sam_sw = buffer[length - 2] * 0x0100 + buffer[length - 1];
  if (ctx->sam_sw != 0x9000)
  {
    sprox_calypso_trace("SPROX_CalypsoSam_DigestAuthenticate:SW=%04X", ctx->sam_sw);

    if (ctx->sam_sw == 0x6700)
      return CALYPSO_ERR_LENGTH;
    if (ctx->sam_sw == 0x6985)
      return CALYPSO_NO_SESSION;
    if (ctx->sam_sw == 0x6988)
      return CALYPSO_WRONG_MAC;
      
    return CALYPSO_ERR_STATUS;
  }

  return rc;
}

SPROX_CALYPSO_FUNC_EX2(WORD, CalypsoSam_SW) (SPROX_PARAM_V)
{
  SPROX_CALYPSO_GET_CTX();
  return ctx->sam_sw;
}
