/**h* DesfireAPI/Authentication
 *
 * NAME
 *   DesfireAPI :: Authentification functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of the various DESFIRE authentification functions.
 *
 **/
#include "sprox_desfire_i.h" 

//#define _DEBUG_AUTH

/**f* DesfireAPI/Authenticate
 *
 * NAME
 *   Authenticate
 *
 * DESCRIPTION
 *   Perform authentication using the specified DES or 3DES key on the currently
 *   selected DESFIRE application.
 *   This is the legacy function, available even on DESFIRE EV0.
 *   The generated session key is afterwards used for non-ISO ciphering or macing.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_Authenticate(BYTE bKeyNumber,
 *                                    const BYTE pbAccessKey[16]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_Authenticate(SPROX_INSTANCE rInst,
 *                                     BYTE bKeyNumber,
 *                                     const BYTE pbAccessKey[16]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_Authenticate(SCARDHANDLE hCard,
 *                                   BYTE bKeyNumber,
 *                                   const BYTE pbAccessKey[16]);
 *
 * INPUTS
 *   BYTE bKeyNumber             : number of the key (KeyNo)
 *   const BYTE pbAccessKey[16]  : 16-byte Access Key (DES/3DES2K keys)
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
 *   * If the 2nd half of the key string is equal to the 1st half, the key is
 *   handled as a single DES key by the DESFIRE card.
 *   * If the 2nd half of the key string is NOT equal to the 1st half, the key
 *   is handled as a 3DES key.
 *
 * SEE ALSO
 *   AuthenticateIso24
 *   AuthenticateIso
 *   AuthenticateAes
 *   ChangeKeySettings
 *   GetKeySettings
 *   ChangeKey
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_Authenticate) (SPROX_PARAM  BYTE bKeyNumber, const BYTE pbAccessKey[16])
{
  DWORD t;
  SPROX_RC status;
  BYTE    abRndB[8], abRndA[8];
  SPROX_DESFIRE_GET_CTX();

  if (pbAccessKey == NULL)
    return DFCARD_LIB_CALL_ERROR;

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("Authentication(%02X, ", bKeyNumber);
    for (i=0; i<16; i++)
      printf("%02X", pbAccessKey[i]);
    printf(")\n");
  }
#endif

  /* Each new Authenticate must invalidate the current authentication status. */
  Desfire_CleanupAuthentication(SPROX_PARAM_PV);

  /* Check whether a TripleDES key was passed (both key halves are different). */
  if (memcmp(pbAccessKey, pbAccessKey + 8, 8))
  {
    /* If the two key halves are not identical, we are doing TripleDES. */
    /* We have to remember that TripleDES is in effect, because the manner of building
       the session key is different in this case. */
    Desfire_InitCrypto3Des(SPROX_PARAM_P  pbAccessKey, pbAccessKey+8, NULL);
    ctx->session_type = KEY_LEGACY_3DES;
  } else
  {
    Desfire_InitCrypto3Des(SPROX_PARAM_P  pbAccessKey, NULL, NULL);
    ctx->session_type = KEY_LEGACY_DES;
  }

  /* Create the command string consisting of the command byte and the parameter byte. */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_AUTHENTICATE;
  ctx->xfer_buffer[ctx->xfer_length++] = bKeyNumber;

  /* Send the command string to the PICC and get its response (1st frame exchange).
     The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_ADDITIONAL_FRAME);
  if (status != DF_OPERATION_OK)
    return status;

  /* Check the number of bytes received, we expect 9 bytes. */
  if (ctx->xfer_length != 9)
  {
    /* Error: block with inappropriate number of bytes received from the card. */
    return DFCARD_WRONG_LENGTH;
  }

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("Ek(RND_B) = ");
    for (i=1; i<9; i++)
      printf("%02X", ctx->xfer_buffer[i]);
    printf("\n");
  }
#endif

  /* OK, we received the 8 bytes Ek( RndB ) from the PICC.
     Decipher Ek( RndB ) to get RndB in ctx->xfer_buffer.
     Note that the status code has already been extracted from the queue. */
  t = 8;
  Desfire_CipherRecv(SPROX_PARAM_P  &ctx->xfer_buffer[1], &t);

  /* Store this RndB (is needed later on for generating the session key). */
  memcpy(abRndB, &ctx->xfer_buffer[1], 8);

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("RND_B = ");
    for (i=0; i<8; i++)
      printf("%02X", abRndB[i]);
    printf("\n");
  }
#endif

  /* Now the PCD has to generate RndA. */
  GetRandomBytes(SPROX_PARAM_P  abRndA, 8);

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("RND_A = ");
    for (i=0; i<8; i++)
      printf("%02X", abRndA[i]);
    printf("\n");
  }
#endif

  /* Start the second frame with a status byte indicating to the PICC that the Authenticate
     command is continued. */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_ADDITIONAL_FRAME;

  /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
     after the status byte. */
  memcpy(&ctx->xfer_buffer[ctx->xfer_length], abRndA, 8);
  ctx->xfer_length += 8;

  memcpy(&ctx->xfer_buffer[ctx->xfer_length], &abRndB[1], 7);
  ctx->xfer_length += 7;
  ctx->xfer_buffer[ctx->xfer_length++] = abRndB[0]; /* first byte move to last byte */

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("RND_A||RND_B' = ");
    for (i=1; i<17; i++)
      printf("%02X", ctx->xfer_buffer[i]);
    printf("\n");
  }
#endif

  /* Apply the DES send operation to the 16 argument bytes before sending the second frame
     to the PICC ( do not include the status byte in the DES operation ). */
  t = 16;
  Desfire_CipherSend(SPROX_PARAM_P  &ctx->xfer_buffer[1], &t, t);

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("Dk(RND_A||RND_B') = ");
    for (i=1; i<17; i++)
      printf("%02X", ctx->xfer_buffer[i]);
    printf("\n");
  }
#endif

  /* Send the 2nd frame to the PICC and get its response. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("Ek(RND_A') = ");
    for (i=1; i<9; i++)
      printf("%02X", ctx->xfer_buffer[i]);
    printf("\n");
  }
#endif

  /* We should now have Ek( RndA' ) in our buffer.
     RndA' was made from RndA by the PICC by rotating the string one byte to the left.
     Decipher Ek( RndA' ) to get RndA' in ctx->xfer_buffer. */
  t = 8;
  Desfire_CipherRecv(SPROX_PARAM_P  &ctx->xfer_buffer[1], &t);

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("RND_A' = ");
    for (i=1; i<9; i++)
      printf("%02X", ctx->xfer_buffer[i]);
    printf("\n");
  }
#endif

  /* Now we have RndA' in our buffer.
     We have to check whether it matches our local copy of RndA.
     If one of the subsequent comparisons fails, we do not trust the PICC and therefore
     abort the authentication procedure ( no session key is generated ). */

  /* First compare the bytes 1 to 7 of RndA with the first 7 bytes in the queue. */
  if (memcmp(&ctx->xfer_buffer[INF + 1], abRndA + 1, 7))
    return DFCARD_WRONG_KEY;

  /* Then compare the leftmost byte of RndA with the last byte in the queue. */
  if (ctx->xfer_buffer[INF + 8] != abRndA[0])
    return DFCARD_WRONG_KEY;

  /* The actual authentication has succeeded.
     Finally we have to generate the session key from both random numbers RndA and RndB.
     The first half of the session key is the concatenation of RndA[0-3] + RndB[0-3]. */
  /* If the original key passed through pbAccessKey is a TripleDES key, the session
     key must also be a TripleDES key. */

  if (ctx->session_type == KEY_LEGACY_DES)
  {
    memcpy(ctx->session_key +  0, abRndA + 0, 4);
    memcpy(ctx->session_key +  4, abRndB + 0, 4);
    memcpy(ctx->session_key +  8, ctx->session_key, 8);
    memcpy(ctx->session_key + 16, ctx->session_key, 8);

    Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, NULL, NULL);

  } else
  if (ctx->session_type == KEY_LEGACY_3DES)
  {
    /* For TripleDES generate the second part of the session key.
       This is the concatenation of RndA[4-7] + RndB[4-7]. */
    memcpy(ctx->session_key +  0, abRndA + 0, 4);
    memcpy(ctx->session_key +  4, abRndB + 0, 4);
    memcpy(ctx->session_key +  8, abRndA + 4, 4);
    memcpy(ctx->session_key + 12, abRndB + 4, 4);
    memcpy(ctx->session_key + 16, ctx->session_key, 8);

    Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, ctx->session_key+8, NULL);
  }

#ifdef _DEBUG_AUTH
  {
    BYTE i;
    printf("Ksession = ");
    for (i=0; i<16; i++)
      printf("%02X", ctx->session_key[i]);
    printf("\n");
  }
#endif

  /* Authenticate succeeded, therefore we remember the number of the key which was used
     to obtain the current authentication status. */
  ctx->session_key_id   = bKeyNumber;

  /* Reset the init vector */
  Desfire_CleanupInitVector(SPROX_PARAM_PV);

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/AuthenticateIso
 *
 * NAME
 *   AuthenticateIso
 *
 * DESCRIPTION
 *   Perform authentication using the specified 3DES key on the currently
 *   selected DESFIRE application.
 *   The generated session key is afterwards used for ISO ciphering or CMACing.
 *   This function is not available on DESFIRE EV0 cards.
 *
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_AuthenticateIso(BYTE bKeyNumber,
 *                                       const BYTE pbAccessKey[16]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_AuthenticateIso(SPROX_INSTANCE rInst,
 *                                        BYTE bKeyNumber,
 *                                        const BYTE pbAccessKey[16]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_AuthenticateIso(SCARDHANDLE hCard,
 *                                      BYTE bKeyNumber,
 *                                      const BYTE pbAccessKey[16]);
 *
 * INPUTS
 *   BYTE bKeyNumber             : number of the key (KeyNo)
 *   const BYTE pbAccessKey[16]  : 16-byte Access Key (DES/3DES2K keys)
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
 *   - If the 2nd half of the key string is equal to the 1st half, the 
 *   64-bit key is handled as a single DES key by the DESFIRE card
 *   (well, actually there are only 56 significant bits).
 *   - If the 2nd half of the key string is NOT equal to the 1st half, the
 *   key is a 128 bit 3DES key
 *   (well, actually there are only 112 significant bits).
 *
 * SEE ALSO
 *   Authenticate
 *   AuthenticateIso24
 *   AuthenticateAes
 *   ChangeKeySettings
 *   GetKeySettings
 *   ChangeKey
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_AuthenticateIso) (SPROX_PARAM  BYTE bKeyNumber, const BYTE pbAccessKey[16])
{
  BYTE bKeyBuffer24[24];
  SPROX_DESFIRE_GET_CTX();

	if (pbAccessKey == NULL)
	  return DFCARD_LIB_CALL_ERROR;

	memcpy(&bKeyBuffer24[0],  &pbAccessKey[0], 16);
	memcpy(&bKeyBuffer24[16], &pbAccessKey[0], 8);

  return SPROX_API_CALL(Desfire_AuthenticateIso24) (SPROX_PARAM_P  bKeyNumber, bKeyBuffer24);
}

/**f* DesfireAPI/AuthenticateIso24
 *
 * NAME
 *   AuthenticateIso24
 *
 * DESCRIPTION
 *   Perform authentication using the specified 3DES key on the currently
 *   selected DESFIRE application.
 *   The generated session key is afterwards used for ISO ciphering or CMACing.
 *   This function is not available on DESFIRE EV0 cards.
 *
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_AuthenticateIso24(BYTE bKeyNumber,
 *                                         const BYTE pbAccessKey[24]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_AuthenticateIso24(SPROX_INSTANCE rInst,
 *                                          BYTE bKeyNumber,
 *                                          const BYTE pbAccessKey[24]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_AuthenticateIso24(SCARDHANDLE hCard,
 *                                        BYTE bKeyNumber,
 *                                        const BYTE pbAccessKey[24]);
 *
 * INPUTS
 *   BYTE bKeyNumber             : number of the key (KeyNo)
 *   const BYTE pbAccessKey[24]  : 24-byte Access Key (DES/3DES2K/3DES3K keys)
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   Both DES and 3DES keys are stored in strings consisting of 24 bytes :
 *   - If the 2nd third of the key string is equal to the 1st third, the 
 *   64-bit key is handled as a single DES key by the DESFIRE card
 *   (well, actually there are only 56 significant bits).
 *   - If the 2nd third of the key string is NOT equal to the 1st third AND
 *   the 3rd third is equal to the 1st third, the key is a 128 bit 3DES key
 *   (well, actually there are only 112 significant bits).
 *   - Overwise, the key is a 192 bit 3DES key "3DES3K mode" (well, actually
 *   (well, actually there are only 168 significant bits).
 *
 * SEE ALSO
 *   Authenticate
 *   AuthenticateIso
 *   AuthenticateAes
 *   ChangeKeySettings
 *   GetKeySettings
 *   ChangeKey
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_AuthenticateIso24) (SPROX_PARAM  BYTE bKeyNumber, const BYTE pbAccessKey[24])
{
  BYTE rnd_size;
  SPROX_RC status;
  DWORD t;
  BYTE  abRndB[16], abRndA[16];
  SPROX_DESFIRE_GET_CTX();

	if (pbAccessKey == NULL)
	  return DFCARD_LIB_CALL_ERROR;

  /* Each new Authenticate must invalidate the current authentication status. */
  Desfire_CleanupAuthentication(SPROX_PARAM_PV);

  /* Create the command string consisting of the command byte and the parameter byte. */
  ctx->xfer_buffer[INF + 0] = DF_AUTHENTICATE_ISO;
  ctx->xfer_buffer[INF + 1] = bKeyNumber;
  ctx->xfer_length = 2;

  /* Send the command string to the PICC and get its response (1st frame exchange).
     The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_ADDITIONAL_FRAME);
  if (status != DF_OPERATION_OK)
    return status;

  /* Check the number of bytes received, we expect 9 or 17 bytes. */
  if (ctx->xfer_length == 9)
  {
    /* This is a 3DES2K (or a DES) */
    rnd_size = 8;
    if (memcmp(pbAccessKey, pbAccessKey+8,  8))
    {
      ctx->session_type = KEY_ISO_3DES2K;
      Desfire_InitCrypto3Des(SPROX_PARAM_P  pbAccessKey, pbAccessKey+8, NULL);
    } else
    {
      ctx->session_type = KEY_ISO_DES;
      Desfire_InitCrypto3Des(SPROX_PARAM_P  pbAccessKey, NULL, NULL);
    }
  } else
  if (ctx->xfer_length == 17)
  {
    /* This is a 3DES3K */
    rnd_size = 16;
    ctx->session_type = KEY_ISO_3DES3K;
    Desfire_InitCrypto3Des(SPROX_PARAM_P  pbAccessKey, pbAccessKey+8, pbAccessKey+16);
  } else
  {
    /* Error: block with inappropriate number of bytes received from the card. */
    return DFCARD_WRONG_LENGTH;
  }

  /* OK, we received the cryptogram Ek( RndB ) from the PICC.
     Decipher Ek( RndB ) to get RndB in ctx->xfer_buffer.
     Note that the status code has already been extracted from the queue. */
  t = rnd_size;
  Desfire_CipherRecv(SPROX_PARAM_P  &ctx->xfer_buffer[INF + 1], &t);

  /* Store this RndB (is needed later on for generating the session key). */
  memcpy(abRndB, &ctx->xfer_buffer[1], rnd_size);

  /* Now the PCD has to generate RndA. */
  GetRandomBytes(SPROX_PARAM_P  abRndA, rnd_size);

  /* Start the second frame with a status byte indicating to the PICC that the Authenticate
     command is continued. */
  ctx->xfer_buffer[0] = DF_ADDITIONAL_FRAME;

  /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
     after the status byte. */
  memcpy(&ctx->xfer_buffer[1], abRndA, rnd_size);
  memcpy(&ctx->xfer_buffer[1 + rnd_size], &abRndB[1], rnd_size-1);
  ctx->xfer_buffer[1 + 2 * rnd_size - 1] = abRndB[0]; /* first byte move to last byte */
  ctx->xfer_length = 1 + 2 * rnd_size;

  /* Apply the DES send operation to the argument bytes before sending the second frame
     to the PICC ( do not include the status byte in the DES operation ). */
  t = 2 * rnd_size;
  Desfire_CipherSend(SPROX_PARAM_P  &ctx->xfer_buffer[1], &t, t);

  /* Send the 2nd frame to the PICC and get its response. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

  /* We should now have Ek( RndA' ) in our buffer.
     RndA' was made from RndA by the PICC by rotating the string one byte to the left.
     Decipher Ek( RndA' ) to get RndA' in ctx->xfer_buffer. */
  t = rnd_size;
  Desfire_CipherRecv(SPROX_PARAM_P  &ctx->xfer_buffer[1], &t);

  /* Now we have RndA' in our buffer.
     We have to check whether it matches our local copy of RndA.
     If one of the subsequent comparisons fails, we do not trust the PICC and therefore
     abort the authentication procedure ( no session key is generated ). */

  /* First compare the bytes 1 to bRndLen-1 of RndA with the first bRndLen-1 bytes in the queue. */
  if (memcmp(&ctx->xfer_buffer[1], &abRndA[1], rnd_size-1))
  {
    return DFCARD_WRONG_KEY;
  }

  /* Then compare the leftmost byte of RndA with the last byte in the queue. */
  if (ctx->xfer_buffer[1 + rnd_size - 1] != abRndA[0])
  {
    return DFCARD_WRONG_KEY;
  }

  /* The actual authentication has succeeded.
     Finally we have to generate the session key from both random numbers RndA and RndB. */
  if (ctx->session_type == KEY_ISO_DES)
  {
    memcpy(ctx->session_key +  0, abRndA +  0, 4);
    memcpy(ctx->session_key +  4, abRndB +  0, 4);
    memcpy(ctx->session_key +  8, ctx->session_key, 8);
    memcpy(ctx->session_key + 16, ctx->session_key, 8);

    Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, NULL, NULL);
  } else
  if (ctx->session_type == KEY_ISO_3DES2K)
  {
    memcpy(ctx->session_key +  0, abRndA +  0, 4);
    memcpy(ctx->session_key +  4, abRndB +  0, 4);
    memcpy(ctx->session_key +  8, abRndA +  4, 4);
    memcpy(ctx->session_key + 12, abRndB +  4, 4);
    memcpy(ctx->session_key + 16, ctx->session_key, 8);

    Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, ctx->session_key+8, NULL);
  } else
  if (ctx->session_type == KEY_ISO_3DES3K)
  {
    memcpy(ctx->session_key +  0, abRndA +  0, 4);
    memcpy(ctx->session_key +  4, abRndB +  0, 4);
    memcpy(ctx->session_key +  8, abRndA +  6, 4);
    memcpy(ctx->session_key + 12, abRndB +  6, 4);
    memcpy(ctx->session_key + 16, abRndA + 12, 4);
    memcpy(ctx->session_key + 20, abRndB + 12, 4);

    Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, ctx->session_key+8, ctx->session_key+16);
  }

  /* Authenticate succeeded, therefore we remember the number of the key which was used
     to obtain the current authentication status. */
  ctx->session_key_id   = bKeyNumber;

  /* Reset the init vector */
  Desfire_CleanupInitVector(SPROX_PARAM_PV);

  /* Initialize the CMAC calculator */
  Desfire_InitCmac(SPROX_PARAM_PV);

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/AuthenticateAes
 *
 * NAME
 *   AuthenticateAes
 *
 * DESCRIPTION
 *   Perform authentication using the specified AES key on the currently
 *   selected DESFIRE application.
 *   This function is not available on DESFIRE EV0 cards.
 *   The generated session key is afterwards used for ISO ciphering or CMACing.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_AuthenticateAes(BYTE bKeyNumber,
 *                                       const BYTE pbAccessKey[16]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_AuthenticateAes(SPROX_INSTANCE rInst,
 *                                        BYTE bKeyNumber,
 *                                        const BYTE pbAccessKey[16]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_AuthenticateAes(SCARDHANDLE hCard,
 *                                      BYTE bKeyNumber,
 *                                      const BYTE pbAccessKey[16]);
 *
 * INPUTS
 *   BYTE bKeyNumber             : number of the key (KeyNo)
 *   const BYTE pbAccessKey[16]  : 16-byte Access Key (AES)
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   AES keys are always 128-bit long.
 *
 * SEE ALSO
 *   Authenticate
 *   AuthenticateIso24
 *   AuthenticateIso
 *   ChangeKeySettings
 *   GetKeySettings
 *   ChangeKey
 *   GetKeyVersion
 *
 **/
SPROX_API_FUNC(Desfire_AuthenticateAes) (SPROX_PARAM  BYTE bKeyNumber, const BYTE pbAccessKey[16])
{
  SPROX_RC status;
  DWORD t;
  BYTE  abRndB[16], abRndA[16];
  SPROX_DESFIRE_GET_CTX();

	if (pbAccessKey == NULL)
	  return DFCARD_LIB_CALL_ERROR;

  /* Each new Authenticate must invalidate the current authentication status. */
  Desfire_CleanupAuthentication(SPROX_PARAM_PV);

  /* Initialize the cipher unit with the authentication key */
  ctx->session_type = KEY_ISO_AES;
  Desfire_InitCryptoAes(SPROX_PARAM_P  pbAccessKey);

  /* Create the command string consisting of the command byte and the parameter byte. */
  ctx->xfer_buffer[INF + 0] = DF_AUTHENTICATE_AES;
  ctx->xfer_buffer[INF + 1] = bKeyNumber;
  ctx->xfer_length = 2;

  /* Send the command string to the PICC and get its response (1st frame exchange).
     The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_ADDITIONAL_FRAME);
  if (status != DF_OPERATION_OK)
    return status;

  /* Check the number of bytes received, we expect 17 bytes. */
  if (ctx->xfer_length != 17)
  {
    /* Error: block with inappropriate number of bytes received from the card. */
    return DFCARD_WRONG_LENGTH;
  }

  /* OK, we received the cryptogram Ek( RndB ) from the PICC.
     Decipher Ek( RndB ) to get RndB in ctx->xfer_buffer.
     Note that the status code has already been extracted from the queue. */
  t = 16;
  Desfire_CipherRecv(SPROX_PARAM_P  &ctx->xfer_buffer[INF + 1], &t);

  /* Store this RndB (is needed later on for generating the session key). */
  memcpy(abRndB, &ctx->xfer_buffer[INF + 1], 16);

  /* Now the PCD has to generate RndA. */
  GetRandomBytes(SPROX_PARAM_P  abRndA, 16);

  /* Start the second frame with a status byte indicating to the PICC that the Authenticate
     command is continued. */
  ctx->xfer_buffer[INF + 0] = DF_ADDITIONAL_FRAME;

  /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
     after the status byte. */
  memcpy(&ctx->xfer_buffer[INF + 1], abRndA, 16);
  memcpy(&ctx->xfer_buffer[INF + 1 + 16], &abRndB[1], 15);
  ctx->xfer_buffer[INF + 1 + 31] = abRndB[0]; /* first byte move to last byte */
  ctx->xfer_length = 1 + 32;

  /* Apply the DES send operation to the argument bytes before sending the second frame
     to the PICC ( do not include the status byte in the DES operation ). */
  t = 32;
  Desfire_CipherSend(SPROX_PARAM_P  &ctx->xfer_buffer[1], &t, t);

  /* Send the 2nd frame to the PICC and get its response. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

  /* We should now have Ek( RndA' ) in our buffer.
     RndA' was made from RndA by the PICC by rotating the string one byte to the left.
     Decipher Ek( RndA' ) to get RndA' in ctx->xfer_buffer. */
  t = 16;
  Desfire_CipherRecv(SPROX_PARAM_P  &ctx->xfer_buffer[1], &t);

  /* Now we have RndA' in our buffer.
     We have to check whether it matches our local copy of RndA.
     If one of the subsequent comparisons fails, we do not trust the PICC and therefore
     abort the authentication procedure ( no session key is generated ). */

  /* First compare the bytes 1 to bRndLen-1 of RndA with the first bRndLen-1 bytes in the queue. */
  if (memcmp(&ctx->xfer_buffer[INF + 1], &abRndA[1], 15))
  {
    return DFCARD_WRONG_KEY;
  }

  /* Then compare the leftmost byte of RndA with the last byte in the queue. */
  if (ctx->xfer_buffer[INF + 1 + 15] != abRndA[0])
  {
    return DFCARD_WRONG_KEY;
  }

  /* The actual authentication has succeeded.
     Finally we have to generate the session key from both random numbers RndA and RndB. */
  memcpy(ctx->session_key +  0, abRndA +  0, 4);
  memcpy(ctx->session_key +  4, abRndB +  0, 4);
  memcpy(ctx->session_key +  8, abRndA + 12, 4);
  memcpy(ctx->session_key + 12, abRndB + 12, 4);
  memset(ctx->session_key + 16, 0, 8);

  /* Initialize the cipher unit with the session key */
  ctx->session_type = KEY_ISO_AES;
  Desfire_InitCryptoAes(SPROX_PARAM_P  ctx->session_key);

  /* Authenticate succeeded, therefore we remember the number of the key which was used
     to obtain the current authentication status. */
  ctx->session_key_id   = bKeyNumber;

  /* Reset the init vector */
  Desfire_CleanupInitVector(SPROX_PARAM_PV);

  /* Initialize the CMAC calculator */
  Desfire_InitCmac(SPROX_PARAM_PV);

  /* Success. */
  return DF_OPERATION_OK;
}

void Desfire_CleanupAuthentication(SPROX_PARAM_V)
{
  SPROX_DESFIRE_GET_CTX_V();
  ctx->session_key_id = (BYTE) -1;
  ctx->session_type = KEY_EMPTY;
  memset(ctx->init_vector, 0, sizeof(ctx->init_vector));

#ifdef SPROX_DESFIRE_WITH_SAM
  ctx->sam_session_active = FALSE;
#endif
}

void Desfire_CleanupInitVector(SPROX_PARAM_V)
{
  SPROX_DESFIRE_GET_CTX_V();
  memset(ctx->init_vector, 0, sizeof(ctx->init_vector));
}

SPROX_API_FUNC(Desfire_SetSessionKey) (SPROX_PARAM  const BYTE pbSessionKey[16])
{
  SPROX_DESFIRE_GET_CTX();

  memcpy(ctx->session_key, pbSessionKey, 16);
	
  ctx->session_type = KEY_ISO_DES;
  Desfire_InitCrypto3Des(SPROX_PARAM_P  &ctx->session_key[0], &ctx->session_key[0], &ctx->session_key[0]);

  /* Reset the init vector */
  Desfire_CleanupInitVector(SPROX_PARAM_PV);

  /* Initialize the CMAC calculator */
  Desfire_InitCmac(SPROX_PARAM_PV);

  /* Success. */
  return DF_OPERATION_OK;
}

SPROX_API_FUNC(Desfire_SetSessionKey24) (SPROX_PARAM  const BYTE pbSessionKey[24])
{
  SPROX_DESFIRE_GET_CTX();
  
  memcpy(ctx->session_key, pbSessionKey, 24);
	
  ctx->session_type = KEY_ISO_3DES2K;
  Desfire_InitCrypto3Des(SPROX_PARAM_P  &ctx->session_key[0], &ctx->session_key[8], &ctx->session_key[16]);

  /* Reset the init vector */
  Desfire_CleanupInitVector(SPROX_PARAM_PV);

  /* Initialize the CMAC calculator */
  Desfire_InitCmac(SPROX_PARAM_PV);

  /* Success. */
  return DF_OPERATION_OK;
}

SPROX_API_FUNC(Desfire_SetSessionKeyAes) (SPROX_PARAM  const BYTE pbSessionKey[16])
{
  SPROX_DESFIRE_GET_CTX();
  
  memcpy(ctx->session_key, pbSessionKey, 16);
	  
  ctx->session_type = KEY_ISO_AES;
  Desfire_InitCryptoAes(SPROX_PARAM_P  ctx->session_key);

  /* Reset the init vector */
  Desfire_CleanupInitVector(SPROX_PARAM_PV);

  /* Initialize the CMAC calculator */
  Desfire_InitCmac(SPROX_PARAM_PV);

  /* Success. */
  return DF_OPERATION_OK;
}
