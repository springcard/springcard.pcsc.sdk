/**h* DesfireAPI/Keys
 *
 * NAME
 *   DesfireAPI :: Key management functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of management functions to change keys or
 *   key settings withing a DESFIRE application.
 *
 **/
#include "sprox_desfire_i.h"

/**f* DesfireAPI/ChangeKeySettings
 *
 * NAME
 *   ChangeKeySettings
 *
 * DESCRIPTION
 *   Changes the key settings of the currently selected application
 *   (or of card's master key if root application is selected)
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ChangeKeySettings (BYTE key_settings);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ChangeKeySettings(SPROX_INSTANCE rInst,
 *                                          BYTE key_settings);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ChangeKeySettings  (SCARDHANDLE hCard,
 *                                          BYTE key_settings);
 *
 * INPUTS
 *   BYTE key_settings  : new key settings (see chapter 4.3.2 of datasheet of mifare
 *                        DesFire MF3ICD40 for more information)
 *
 * RETURNS
 *   DF_OPERATION_OK    : change succeeded
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   Authenticate
 *   GetKeySettings
 *   ChangeKey
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_ChangeKeySettings) (SPROX_PARAM  BYTE key_settings)
{
  SPROX_DESFIRE_GET_CTX();

  ctx->xfer_length = 0;

  /* Create the info block containing the command code */
  ctx->xfer_buffer[ctx->xfer_length++] = DF_CHANGE_KEY_SETTINGS;
  ctx->xfer_buffer[ctx->xfer_length++] = key_settings;

#ifdef SPROX_DESFIRE_WITH_SAM
  if (ctx->sam_session_active)
  {
    DWORD i;
    BYTE pbOut[30];
    DWORD dwOutLength = sizeof(pbOut);

    if (ctx->session_type & KEY_ISO_MODE)
    {
      SAM_EncipherData(ctx->sam_context.hSam, ctx->xfer_buffer, ctx->xfer_length, 1, pbOut, &dwOutLength);
    } else
    {
      /* Non-ISO authenticated: don't send command */
      SAM_EncipherData(ctx->sam_context.hSam, &ctx->xfer_buffer[1], ctx->xfer_length-1, 0, pbOut, &dwOutLength);
    }

    ctx->xfer_length = 0;
    ctx->xfer_buffer[ctx->xfer_length++] = DF_CHANGE_KEY_SETTINGS;
    for (i=0; i<dwOutLength; i++)
      ctx->xfer_buffer[ctx->xfer_length++] = pbOut[i];

  } else
#endif
  {
    /* Append the CRC value corresponding to the key_settings byte */
    /* Then 'Encrypt' the bNewKeySettings byte and its CRC bytes.  */
    Desfire_XferAppendCrc(SPROX_PARAM_P  1);
    Desfire_XferCipherSend(SPROX_PARAM_P  1);
  }

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);


}

/**f* DesfireAPI/GetKeySettings
 *
 * NAME
 *   GetKeySettings
 *
 * DESCRIPTION
 *   Gets information on the DesFire card and application master key settings. In addition it returns
 *   the maximum number of keys which can be stored within the selected application.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetKeySettings (BYTE *key_settings,
 *                                       BYTE *key_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetKeySettings(SPROX_INSTANCE rInst,
 *                                       BYTE *key_settings,
 *                                       BYTE *key_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetKeySettings  (SCARDHANDLE hCard,
 *                                       BYTE *key_settings,
 *                                       BYTE *key_count);
 *
 * INPUTS
 *   BYTE *key_settings          : master key settings (see chapter 4.3.2 of datasheet of mifare
 *                                 DesFire MF3ICD40 for more information)
 *   BYTE *key_count             : maximum number of keys
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   Authenticate
 *   ChangeKeySettings
 *   ChangeKey
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_GetKeySettings) (SPROX_PARAM  BYTE *key_settings, BYTE *key_count)
{
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code */
  ctx->xfer_buffer[INF + 0] = DF_GET_KEY_SETTINGS;
  ctx->xfer_length = 1;

  /* Communicate the info block to the card and check the operation's return status. */
  /* GetKeySettings returns 3 bytes, the status and two bytes of information.        */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  3, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

  /* Return the requested key settings bytes. */
  *key_settings = ctx->xfer_buffer[INF + 1];
  *key_count = ctx->xfer_buffer[INF + 2];

  /* Success */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/ChangeKey
 *
 * NAME
 *   ChangeKey
 *
 * DESCRIPTION
 *   Change a DES, or 3DES2K key in the selected Desfire application.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ChangeKey(BYTE key_number,
 *                                 const BYTE new_key[16],
 *                                 const BYTE old_key[16]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ChangeKey(SPROX_INSTANCE rInst,
 *                                  BYTE key_number,
 *                                  const BYTE new_key[16],
 *                                  const BYTE old_key[16]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ChangeKey(SCARDHANDLE hCard,
 *                                BYTE key_number,
 *                                const BYTE new_key[16],
 *                                const BYTE old_key[16]);
 *
 * INPUTS
 *   BYTE key_number        : number of the key (KeyNo)
 *   const BYTE new_key[16] : 16-byte New Key (DES/3DES keys)
 *   const BYTE old_key[16] : 16-byte Old Key (DES/3DES keys)
 *
 * RETURNS
 *   DF_OPERATION_OK    : change succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
 *   * If the 2nd half of the key string is equal to the 1st half, the key is
 *   handled as a single DES key by the DesFire card.
 *   * If the 2nd half of the key string is NOT equal to the 1st half, the key
 *   is handled as a 3DES key.
 *
 *   After a successful change of the key used to reach the current authentication status, this
 *   authentication is invalidated, an authentication with the new key is necessary for subsequent
 *   operations.
 *
 *   If authentication has been performed before calling ChangeKey with the old key,
 *   use NULL instead of old_key.
 *
 * SEE ALSO
 *   ChangeKey24
 *   ChangeKeyAes
 *   Authenticate
 *   ChangeKeySettings
 *   GetKeySettings
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_ChangeKey) (SPROX_PARAM  BYTE key_number, const BYTE new_key[16], const BYTE old_key[16])
{
  BYTE    b, buffer[24];
  SPROX_DESFIRE_GET_CTX();

#ifdef SPROX_DESFIRE_WITH_SAM
  if (ctx->sam_session_active)
    return DFCARD_FUNC_NOT_AVAILABLE;
#endif

  if ( (ctx->current_aid != 0x000000) && (key_number & DF_APPLSETTING2_AES) ) /* Update 27/07/2011 to check the current AID (patch submitted by Evaldas Auryla) */
    return DFCARD_LIB_CALL_ERROR;

  if ( (ctx->session_type == KEY_ISO_DES) || (ctx->session_type == KEY_ISO_3DES2K) || (ctx->session_type == KEY_ISO_3DES3K) || (ctx->session_type == KEY_ISO_AES) )
	{
	  BYTE new_key_24[24];
    BYTE old_key_24[24];

		if (new_key != NULL)
		{
      memcpy(&new_key_24[0],  &new_key[0], 16);
			memcpy(&new_key_24[16], &new_key[0], 8);
			new_key = new_key_24;
		}

		if (old_key != NULL)
		{
      memcpy(&old_key_24[0],  &old_key[0], 16);
			memcpy(&old_key_24[16], &old_key[0], 8);
			old_key = old_key_24;
		}

		return SPROX_API_CALL(Desfire_ChangeKey24) (SPROX_PARAM_P  key_number, new_key, old_key);
  }

#ifdef _DEBUG_KEYS
  {
    BYTE i;
    printf("ChangeKey(%02X, ", key_number);
    for (i=0; i<16; i++)
      printf("%02X", new_key[i]);
    if (old_key != NULL)
    {
      printf(", ");
      for (i=0; i<16; i++)
        printf("%02X", old_key[i]);
    }
    printf(")\n");
  }
#endif

  memset(buffer, 0, sizeof(buffer));

  /* Begin the info block with the command code and the number of the key to be changed. */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_CHANGE_KEY;
  ctx->xfer_buffer[ctx->xfer_length++] = key_number;

  if (old_key != NULL)
  {
    /* If a 'previous key' has been passed, format the 24 byte cryptogram according to the     */
    /* three key procedure (new key, previous key, all encrypted with the current session key) */

    /* Take the new key into the buffer. */
    memcpy(buffer, new_key, 16);

    /* XOR the previous key to the new key. */
    for (b = 0; b < 16; b++)
      buffer[b] ^= old_key[b];

    /* Append a first CRC, computed over the XORed key combination. */
    Desfire_ComputeCrc16(SPROX_PARAM_P  &buffer[0], 16, &buffer[16]);

    /* Append a second CRC, computed over the new key only. */
    Desfire_ComputeCrc16(SPROX_PARAM_P  new_key, 16, &buffer[18]);

  } else
  {
    /* If no 'previous key' has been passed, format the 24 byte cryptogram according to the */
    /* two key procedure (new key, encrypted with the current session key).                 */

    /* Take the new key into the buffer. */
    memcpy(buffer, new_key, 16);

    /* Append the CRC computed over the new key. */
    Desfire_ComputeCrc16(SPROX_PARAM_P  new_key, 16, &buffer[16]);
  }

  /* Append the 24 byte buffer to the command string. */
  memcpy(&ctx->xfer_buffer[ctx->xfer_length], buffer, 24);
  ctx->xfer_length += 24;

#ifdef _DEBUG_KEYS
  {
    BYTE i;
    printf("Plain : ");
    for (i=0; i<24; i++)
      printf("%02X", ctx->xfer_buffer[2+i]);
    printf("\n");
  }
#endif

  /* 'Encrypt' the 24 bytes */
  Desfire_XferCipherSend(SPROX_PARAM_P  2);

#ifdef _DEBUG_KEYS
  {
    BYTE i;
    printf("Cipher: ");
    for (i=0; i<24; i++)
      printf("%02X", ctx->xfer_buffer[2+i]);
    printf("\n");
  }
#endif

  /* Forget the current authentication state if we're changing the current key */
  if ((key_number&0x3F) == (ctx->session_key_id&0x3F))
    Desfire_CleanupAuthentication(SPROX_PARAM_PV);

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_OPERATION_OK);
}

/**f* DesfireAPI/ChangeKey24
 *
 * NAME
 *   ChangeKey24
 *
 * DESCRIPTION
 *   Change a 3DES3K key in the selected Desfire application.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ChangeKey24(BYTE key_number,
 *                                   const BYTE new_key[24],
 *                                   const BYTE old_key[24]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ChangeKey24(SPROX_INSTANCE rInst,
 *                                    BYTE key_number,
 *                                    const BYTE new_key[24],
 *                                    const BYTE old_key[24]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ChangeKey24(SCARDHANDLE hCard,
 *                                  BYTE key_number,
 *                                  const BYTE new_key[24],
 *                                  const BYTE old_key[24]);
 *
 * INPUTS
 *   BYTE key_number        : number of the key (KeyNo)
 *   const BYTE new_key[24] : 24-byte New Key (3DES keys)
 *   const BYTE old_key[24] : 24-byte Old Key (3DES keys)
 *
 * RETURNS
 *   DF_OPERATION_OK    : change succeeded
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   ChangeKey16
 *   ChangeKeyAes
 *   Authenticate
 *   ChangeKeySettings
 *   GetKeySettings
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_ChangeKey24) (SPROX_PARAM  BYTE key_number, const BYTE new_key[24], const BYTE old_key[24])
{
  BYTE  key_size;
  BYTE  b;
  SPROX_DESFIRE_GET_CTX();

#ifdef SPROX_DESFIRE_WITH_SAM
  if (ctx->sam_session_active)
    return DFCARD_FUNC_NOT_AVAILABLE;
#endif

	if (key_number & DF_APPLSETTING2_AES)
	  return DFCARD_LIB_CALL_ERROR;

	if (key_number & DF_APPLSETTING2_3DES3K)
  {
    key_size = 24;

	} else
  {
		switch (ctx->session_type)
		{
			case KEY_ISO_AES    :
			case KEY_ISO_DES    :
			case KEY_ISO_3DES2K : key_size = 16; break;

			case KEY_ISO_3DES3K : key_size = 24; break;

			default             : return DFCARD_LIB_CALL_ERROR;
		}
	}

  memset(ctx->xfer_buffer, 0, sizeof(ctx->xfer_buffer));

  /* Begin the info block with the command code and the number of the key to be changed. */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_CHANGE_KEY;
  ctx->xfer_buffer[ctx->xfer_length++] = key_number;

  /* Take the new key into the buffer. */
  memcpy(&ctx->xfer_buffer[ctx->xfer_length], new_key, key_size);
  ctx->xfer_length += key_size;

  if (old_key != NULL)
  {
    /* If a 'previous key' has been passed, format the 40 byte cryptogram according to the     */
    /* three key procedure (new key, previous key, all encrypted with the current session key) */

    /* XOR the previous key with the new key. */
    for (b = 0; b < key_size; b++)
      ctx->xfer_buffer[ctx->xfer_length - key_size + b] ^= old_key[b];

    /* Append a first CRC, computed over the XORed key combination and the header */
    Desfire_ComputeCrc32(SPROX_PARAM_P  &ctx->xfer_buffer[0], ctx->xfer_length, &ctx->xfer_buffer[ctx->xfer_length]);
    ctx->xfer_length += 4;

    /* Append a second CRC, computed over the new key only. */
    Desfire_ComputeCrc32(SPROX_PARAM_P  new_key, key_size, &ctx->xfer_buffer[ctx->xfer_length]);
    ctx->xfer_length += 4;

  } else
  {
    /* If no 'previous key' has been passed, format the 32 byte cryptogram according to the */
    /* two key procedure (new key, encrypted with the current session key).                 */

    /* Append the CRC computed over the new key and the header */
    Desfire_ComputeCrc32(SPROX_PARAM_P  &ctx->xfer_buffer[0], ctx->xfer_length, &ctx->xfer_buffer[ctx->xfer_length]);
    ctx->xfer_length += 4;
  }

  /* 'Encrypt' the buffer */
  Desfire_XferCipherSend(SPROX_PARAM_P  2);

  /* Forget the current authentication state if we're changing the current key */
  if ((key_number&0x3F) == (ctx->session_key_id&0x3F))
    Desfire_CleanupAuthentication(SPROX_PARAM_PV);

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, LOOSE_RESPONSE_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/ChangeKeyAes
 *
 * NAME
 *   ChangeKeyAes
 *
 * DESCRIPTION
 *   Change an AES key in the selected Desfire application.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ChangeKeyAes(BYTE key_number,
 *                                    BYTE key_version,
 *                                    const BYTE new_key[16],
 *                                    const BYTE old_key[16]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ChangeKeyAes(SPROX_INSTANCE rInst,
 *                                     BYTE key_number,
 *                                     BYTE key_version,
 *                                     const BYTE new_key[16],
 *                                     const BYTE old_key[16]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ChangeKeyAes(SCARDHANDLE hCard,
 *                                   BYTE key_number,
 *                                   BYTE key_version,
 *                                   const BYTE new_key[16],
 *                                   const BYTE old_key[16]);
 *
 * INPUTS
 *   BYTE key_number        : number of the key (KeyNo)
 *   BYTE key_version       : version number to be stored together with the key
 *   const BYTE new_key[16] : 16-byte New Key (AES key)
 *   const BYTE old_key[16] : 16-byte Old Key (AES key)
 *
 * RETURNS
 *   DF_OPERATION_OK    : change succeeded
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   ChangeKey
 *   ChangeKey24
 *   Authenticate
 *   ChangeKeySettings
 *   GetKeySettings
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_ChangeKeyAes) (SPROX_PARAM  BYTE key_number, BYTE key_version, const BYTE new_key[16], const BYTE old_key[16])
{
  BYTE  b;
  SPROX_DESFIRE_GET_CTX();

#ifdef SPROX_DESFIRE_WITH_SAM
  if (ctx->sam_session_active)
    return DFCARD_FUNC_NOT_AVAILABLE;
#endif

  memset(ctx->xfer_buffer, 0, sizeof(ctx->xfer_buffer));

  /* Begin the info block with the command code and the number of the key to be changed. */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_CHANGE_KEY;
  ctx->xfer_buffer[ctx->xfer_length++] = key_number;

  /* Take the new key into the buffer. */
  memcpy(&ctx->xfer_buffer[ctx->xfer_length], new_key, 16);
  ctx->xfer_length += 16;

  if (old_key != NULL)
  {
    /* If a 'previous key' has been passed, format the 40 byte cryptogram according to the     */
    /* three key procedure (new key, previous key, all encrypted with the current session key) */

    /* XOR the previous key with the new key. */
    for (b = 0; b < 16; b++)
      ctx->xfer_buffer[ctx->xfer_length - 16 + b] ^= old_key[b];

    /* The key version goes here */
    ctx->xfer_buffer[ctx->xfer_length++] = key_version;

    /* Append a first CRC, computed over the XORed key combination and the header */
    Desfire_ComputeCrc32(SPROX_PARAM_P  &ctx->xfer_buffer[0], ctx->xfer_length, &ctx->xfer_buffer[ctx->xfer_length]);
    ctx->xfer_length += 4;

    /* Append a second CRC, computed over the new key only. */
    Desfire_ComputeCrc32(SPROX_PARAM_P  new_key, 16, &ctx->xfer_buffer[ctx->xfer_length]);
    ctx->xfer_length += 4;

  } else
  {
    /* If no 'previous key' has been passed, format the 32 byte cryptogram according to the */
    /* two key procedure (new key, encrypted with the current session key).                 */

    /* The key version goes here */
    ctx->xfer_buffer[ctx->xfer_length++] = key_version;

    /* Append the CRC computed over the new key and the header */
    Desfire_ComputeCrc32(SPROX_PARAM_P  &ctx->xfer_buffer[0], ctx->xfer_length, &ctx->xfer_buffer[ctx->xfer_length]);
    ctx->xfer_length += 4;
  }

  /* 'Encrypt' the buffer */
  Desfire_XferCipherSend(SPROX_PARAM_P  2);

  /* Forget the current authentication state if we're changing the current key */
  if ((key_number&0x3F) == (ctx->session_key_id&0x3F))
    Desfire_CleanupAuthentication(SPROX_PARAM_PV);

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, LOOSE_RESPONSE_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/GetKeyVersion
 *
 * NAME
 *   GetKeyVersion
 *
 * DESCRIPTION
 *   Reads out the current key version of any key stored on the PICC
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetKeyVersion(BYTE bKeyNumber,
 *                                     BYTE *pbKeyVersion);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetKeyVersion(SPROX_INSTANCE rInst,
 *                                      BYTE bKeyNumber,
 *                                      BYTE *pbKeyVersion);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetKeyVersion(SCARDHANDLE hCard,
 *                                    BYTE bKeyNumber,
 *                                    BYTE *pbKeyVersion);
 *
 * INPUTS
 *   BYTE bKeyNumber             : number of the key (KeyNo)
 *   BYTE *pbKeyVersion          : current version of the specified key
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   This command can be issued without valid authentication.
 *
 * SEE ALSO
 *   Authenticate
 *   GetKeySettings
 *   ChangeKeySettings
 *   ChangeKey
 *
 **/
SPROX_API_FUNC(Desfire_GetKeyVersion) (SPROX_PARAM  BYTE bKeyNumber, BYTE *pbKeyVersion)
{
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code and the key number argument. */
  ctx->xfer_buffer[INF + 0] = DF_GET_KEY_VERSION;
  ctx->xfer_buffer[INF + 1] = bKeyNumber;
  ctx->xfer_length = 2;

  /* Communicate the info block to the card and check the operation's return status.   */
  /* The returned info block must contain two bytes, the status code and the requested */
  /* key version byte.                                                                 */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  2, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

  /* Get the key version byte. */
  if (pbKeyVersion != NULL)
    *pbKeyVersion = ctx->xfer_buffer[INF + 1];

  /* Success. */
  return DF_OPERATION_OK;
}
