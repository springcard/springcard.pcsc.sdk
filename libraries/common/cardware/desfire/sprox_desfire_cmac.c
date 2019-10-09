/**h* DesfireAPI/CMAC
 *
 * NAME
 *   DesfireAPI :: CMAC module
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of the DESFIRE CMAC scheme (EV1).
 *
 **/
#include "sprox_desfire_i.h"

SPROX_RC Desfire_XferCmacSend(SPROX_PARAM  BOOL append)
{
  BYTE cmac[8];
#ifdef SPROX_DESFIRE_WITH_SAM
  LONG rc;
  DWORD dwCmacLength = sizeof(cmac);
#endif
  SPROX_DESFIRE_GET_CTX();

  if (!(ctx->session_type & KEY_ISO_MODE))
  {
    return DF_OPERATION_OK;
  }

  if (ctx->xfer_buffer[0] == DF_ADDITIONAL_FRAME)
  {
    if (ctx->xfer_length == 1)
    {
      return DF_OPERATION_OK;
    } else
    {
      return DFCARD_UNEXPECTED_CHAINING;
    }
  }

#ifdef SPROX_DESFIRE_WITH_SAM
  if (ctx->sam_session_active)
  {
    rc = SAM_GenerateMAC(ctx->sam_context.hSam, ctx->xfer_buffer, ctx->xfer_length, cmac, &dwCmacLength);
    if ((rc != DF_OPERATION_OK) || (dwCmacLength != 8))
      return rc;
  } else
#endif
  {
    Desfire_ComputeCmac(SPROX_PARAM_P  ctx->xfer_buffer, ctx->xfer_length, FALSE, cmac);
  }

  if (append)
  {
    memcpy(&ctx->xfer_buffer[ctx->xfer_length], cmac, 8);
    ctx->xfer_length += 8;
  }

  return DF_OPERATION_OK;
}


SPROX_RC Desfire_XferCmacRecv(SPROX_PARAM_V)
{
  SPROX_DESFIRE_GET_CTX();

  if (ctx->xfer_buffer[0] == DF_ADDITIONAL_FRAME)
  {
    return DFCARD_UNEXPECTED_CHAINING;
  }
  return Desfire_VerifyCmacRecv(SPROX_PARAM_P  ctx->xfer_buffer, &ctx->xfer_length);
}


void Desfire_InitCmac(SPROX_PARAM_V)
{
  BYTE bMSB;
  BYTE block_size, rb_xor_value;
  BYTE abSavedInitVktr[16];
  DWORD t, i;
  SPROX_DESFIRE_GET_CTX_V();

  rb_xor_value = (ctx->session_type == KEY_ISO_AES) ? 0x87 : 0x1B;
  block_size   = (ctx->session_type == KEY_ISO_AES) ?   16 :    8;

  // Save the InitVector:
  memcpy(abSavedInitVktr, ctx->init_vector, 16);

  // Generate the padding bytes for O-MAC by enciphering a zero block
  // with the actual session key:
  memset(ctx->cmac_subkey_1, 0, sizeof(ctx->cmac_subkey_1));

  t = block_size;
  Desfire_CipherSend(SPROX_PARAM_P  ctx->cmac_subkey_1, &t, t);

  // If the MSBit of the generated cipher == 1 -> K1 = (cipher << 1) ^ Rb ...
  // store MSB:
  bMSB = ctx->cmac_subkey_1[0];

  // Shift the complete cipher for 1 bit ==> K1:
  for (i=0; i<(DWORD) (block_size-1); i++)
  {
    ctx->cmac_subkey_1[i] <<= 1;
    // add the carry over bit:
    ctx->cmac_subkey_1[i] |= ((ctx->cmac_subkey_1[i+1] & 0x80) ? 0x01:0x00);
  }
  ctx->cmac_subkey_1[block_size-1] <<= 1;
  if (bMSB & 0x80)
  {
    // XOR with Rb:
    ctx->cmac_subkey_1[block_size-1] ^= rb_xor_value;
  }

  // store MSB:
  bMSB = ctx->cmac_subkey_1[0];

  // Shift K1 ==> K2:
  for (i=0; i<(DWORD) (block_size-1); i++)
  {
    ctx->cmac_subkey_2[i]  = ctx->cmac_subkey_1[i] << 1;
    ctx->cmac_subkey_2[i] |= ((ctx->cmac_subkey_1[i+1] & 0x80) ? 0x01:0x00);
  }
  ctx->cmac_subkey_2[block_size-1] = ctx->cmac_subkey_1[block_size-1] << 1;

  if (bMSB & 0x80)
  {
    // XOR with Rb:
    ctx->cmac_subkey_2[block_size - 1] ^= rb_xor_value;
  }

  // We have to restore the InitVector:
  memcpy(ctx->init_vector, abSavedInitVktr, 16);
}

void Desfire_ComputeCmac(SPROX_PARAM  const BYTE data[], DWORD length, BOOL move_status, BYTE cmac[])
{
  BYTE  *buffer;
  DWORD  i, actual_length, block_size;
  SPROX_DESFIRE_GET_CTX_V();

  if (!(ctx->session_type & KEY_ISO_MODE))
  {
    printf("INVALID FUNCTION CALL %s %d\n", __FILE__, __LINE__);
  }

    // Adapt the crypto mode if the sessionkey is done in CBC_Send_Decrypt:
//    enCryptoMethod = (m_SessionKeyCryptoMethod == CRM_3DES_DF4 ? CRM_3DES_ISO:m_SessionKeyCryptoMethod);

  block_size = (ctx->session_type == KEY_ISO_AES) ? 16 : 8;

  // First we enlarge eNumOfBytes to a multiple of the cipher block size for allocating
  // memory of the intermediate buffer. Zero padding will be done by the DF8Encrypt function.
  // If we are ISO-authenticated, we have to do the spetial padding for the O-MAC:
  actual_length = length;
  while (actual_length % block_size)
    actual_length++;

  buffer = malloc(actual_length);
  memset(buffer, 0, actual_length);

  if (move_status)
  {
    memcpy(buffer, &data[1], length-1);
    buffer[length-1] = data[0];
  } else
  {
    memcpy(buffer, data, length);
  }

  /* Do the ISO padding and/or XORing */

  if (length % block_size)
  {
    /* Block incomplete -> padding */
    buffer[length++] = 0x80;

    /* XOR the last eight bytes with CMAC_SubKey2 */
    length = actual_length - block_size;
    for (i=0; i<block_size; i++)
    {
      buffer[length + i] ^= ctx->cmac_subkey_2[i];
    }
  } else
  {
    /* Block complete -> no padding */

    /* XOR the last eight bytes with CMAC_SubKey1 */
    length = actual_length - block_size;
    for (i=0; i<block_size; i++)
    {
      buffer[length + i] ^= ctx->cmac_subkey_1[i];
    }
  }

  Desfire_CipherSend(SPROX_PARAM_P  buffer, &actual_length, actual_length);

  // Save the current init vector, which is the last cipher block of the cryptogram:
  memcpy(ctx->init_vector, &buffer[actual_length - block_size], block_size);

  if (cmac != NULL)
  {
    // The mac is the first half of the init vector:
    memcpy(cmac, ctx->init_vector, 8);
  }

  free(buffer);
}

SPROX_RC Desfire_VerifyCmacRecv(SPROX_PARAM  BYTE buffer[], DWORD *length)
{
  DWORD l;
  BYTE cmac[8];
  SPROX_DESFIRE_GET_CTX();

  if (!(ctx->session_type & KEY_ISO_MODE))
    return DF_OPERATION_OK;

  l = *length;

  if (l < 9)
    return DFCARD_WRONG_LENGTH;


#ifdef SPROX_DESFIRE_WITH_SAM
  if (ctx->sam_session_active)
  {
    BOOL fKeepStatus = TRUE;
    LONG rc = SAM_VerifyMAC(ctx->sam_context.hSam, buffer[0], &buffer[1], l-1, 8, fKeepStatus);
    l -= 8;
    *length = l;
    return rc;

  } else
 #endif
  {
    l -= 8;
    Desfire_ComputeCmac(SPROX_PARAM_P  buffer, l, TRUE, cmac);
    if (memcmp(&buffer[l], cmac, 8))
      return DFCARD_WRONG_MAC;

    *length = l;
    return DF_OPERATION_OK;
  }

}
