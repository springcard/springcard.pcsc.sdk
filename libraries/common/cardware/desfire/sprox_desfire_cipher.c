/**h* DesfireAPI/Ciphering
 *
 * NAME
 *   DesfireAPI :: Ciphering module
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Algorithm independant implementation of the DESFIRE ciphering schemes.
 *
 **/
#include "sprox_desfire_i.h"

//#define _DEBUG_CIPHER

void Desfire_InitCrypto3Des(SPROX_PARAM  const BYTE des_key1[8], const BYTE des_key2[8], const BYTE des_key3[8])
{
  SPROX_DESFIRE_GET_CTX_V();

  if (des_key1 == NULL) return;
  if (des_key2 == NULL) des_key2 = des_key1;
  if (des_key3 == NULL) des_key3 = des_key1;

  TDES_Init(&ctx->cipher_context.tdes, des_key1, des_key2, des_key3);
}

void Desfire_InitCryptoAes(SPROX_PARAM  const BYTE aes_key[16])
{
  SPROX_DESFIRE_GET_CTX_V();

  if (aes_key == NULL) return;

  AES_Init(&ctx->cipher_context.aes, aes_key);
}

void Desfire_CipherRecv(SPROX_PARAM  BYTE data[], DWORD *length)
{
  BYTE buffer[16];
  WORD  block_count;
  DWORD i, j;
  SPROX_DESFIRE_GET_CTX_V();

  if ((data == NULL) || (length == NULL))
    return;

#ifdef _DEBUG_CIPHER
  {
    DWORD i;
    printf("CipherRecv(");
    for (i=0; i<*length; i++)
      printf("%02X", data[i]);
    printf(")\n");
  }
#endif

  switch (ctx->session_type)
  {
    case KEY_LEGACY_DES :
      {
        block_count = (WORD) (*length / 8);

#ifdef _DEBUG_CIPHER
        printf("KEY_LEGACY_DES, %d blocks\n", block_count);
#endif

        memset(buffer, 0x00, 8);      /* B  <- 00...00   */
        for (i = 0; i < block_count; i++)
        {
          memcpy(ctx->init_vector, buffer, 8); /* IV <- B         */
          memcpy(buffer, &data[8 * i], 8);     /* B  <- P         */

#ifdef _DEBUG_CIPHER
          {
            printf("block %d/%d, IV=", i, block_count);
            for (j=0; j<8; j++)
              printf("%02X", ctx->init_vector[j]);
            printf(", C=");
            for (j=0; j<8; j++)
              printf("%02X", buffer[j]);
          }
#endif

          TDES_Decrypt2(&ctx->cipher_context.tdes, &data[8 * i], buffer); /* P  <- iDES(B)   */

#ifdef _DEBUG_CIPHER
          {
            DWORD j;
            printf(" -> ");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
          }
#endif

          for (j = 0; j < 8; j++)
            data[8 * i + j] ^= ctx->init_vector[j];  /* P  <- P XOR IV  */

#ifdef _DEBUG_CIPHER
          {
            printf(", P=");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
            printf("\n");
          }
#endif
        }
      }
      break;

    case KEY_LEGACY_3DES :
      {
        block_count = (WORD) (*length / 8);

#ifdef _DEBUG_CIPHER
        printf("KEY_LEGACY_3DES, %d blocks\n", block_count);
#endif

        memset(buffer, 0x00, 8);      /* B  <- 00...00   */
        for (i = 0; i < block_count; i++)
        {
          memcpy(ctx->init_vector, buffer, 8);   /* IV <- B         */
          memcpy(buffer, &data[8 * i], 8);  /* B  <- P         */

#ifdef _DEBUG_CIPHER
          {
            printf("block %d/%d, IV=", i, block_count);
            for (j=0; j<8; j++)
              printf("%02X", ctx->init_vector[j]);
            printf(", C=");
            for (j=0; j<8; j++)
              printf("%02X", buffer[j]);
          }
#endif

          TDES_Decrypt2(&ctx->cipher_context.tdes, &data[8 * i], buffer); /* P  <- iDES(B)   */

#ifdef _DEBUG_CIPHER
          {
            printf(" -> ");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
          }
#endif

          for (j = 0; j < 8; j++)
            data[8 * i + j] ^= ctx->init_vector[j];  /* P  <- P XOR IV  */

#ifdef _DEBUG_CIPHER
          {
            printf(", P=");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
            printf("\n");
          }
#endif
        }
      }
      break;

    case KEY_ISO_DES    :
    case KEY_ISO_3DES2K :
    case KEY_ISO_3DES3K :
      {
        block_count = (WORD) (*length / 8);

        for (i = 0; i < block_count; i++)
        {
          memcpy(buffer, &data[8 * i], 8);  /* B  <- P         */
          TDES_Decrypt2(&ctx->cipher_context.tdes, &data[8 * i], buffer); /* P  <- iDES(B)   */
          for (j = 0; j < 8; j++)
            data[8 * i + j] ^= ctx->init_vector[j];  /* P  <- P XOR IV  */
          memcpy(ctx->init_vector, buffer, 8); /* IV <- B         */
        }

      }
      break;

    case KEY_ISO_AES :
      {
        block_count = (WORD) (*length / 16);

        for (i = 0; i < block_count; i++)
        {
          memcpy(buffer, &data[16 * i], 16);  /* B  <- P         */
          AES_Decrypt2(&ctx->cipher_context.aes, &data[16 * i], buffer); /* P  <- iDES(B)   */
          for (j = 0; j < 16; j++)
            data[16 * i + j] ^= ctx->init_vector[j];  /* P  <- P XOR IV  */
          memcpy(ctx->init_vector, buffer, 16); /* IV <- B         */
        }

      }
      break;
  }

#ifdef _DEBUG_CIPHER
  {
    DWORD i;
    printf("        ==> ");
    for (i=0; i<*length; i++)
      printf("%02X", data[i]);
    printf("\n");
  }
#endif
}

void Desfire_CipherSend(SPROX_PARAM  BYTE data[], DWORD *length, DWORD max_length)
{
  DWORD actual_length;
  DWORD block_size;
  DWORD block_count;
  DWORD i, j;
  SPROX_DESFIRE_GET_CTX_V();

  if ((data == NULL) || (length == NULL))
    return;

#ifdef _DEBUG_CIPHER
  {
    DWORD i;
    printf("CipherSend(");
    for (i=0; i<*length; i++)
      printf("%02X", data[i]);
    printf(")\n");
  }
#endif

  actual_length = *length;

  /* Step 1 : padding */
  block_size = (ctx->session_type == KEY_ISO_AES) ? 16 : 8;
  while (actual_length % block_size)
  {
    if (actual_length >= max_length)
      return;
    data[actual_length++] = 0x00;
  }

  block_count = (actual_length / block_size);

  switch (ctx->session_type)
  {
    case KEY_LEGACY_DES :
      {
#ifdef _DEBUG_CIPHER
        printf("KEY_LEGACY_DES, %d blocks\n", block_count);
#endif

        /* IV <- 0 */
        memset(ctx->init_vector, 0, sizeof(ctx->init_vector));
        for (i = 0; i < block_count; i++)
        {
#ifdef _DEBUG_CIPHER
          {
            printf("block %d/%d, IV=", i, block_count);
            for (j=0; j<8; j++)
              printf("%02X", ctx->init_vector[j]);
            printf(", P=");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
          }
#endif

          for (j = 0; j < 8; j++)
            data[8 * i + j] ^= ctx->init_vector[j];  /* P  <- P XOR IV  */

#ifdef _DEBUG_CIPHER
          {
            printf(" -> ");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
          }
#endif

          /* Legacy mode : PICC always encrypts, PCD always decrypts */
          TDES_Decrypt2(&ctx->cipher_context.tdes, ctx->init_vector, &data[8 * i]);  /* IV <- i3DES(P)   */

          memcpy(&data[8 * i], ctx->init_vector, 8); /* P  <- IV        */

#ifdef _DEBUG_CIPHER
          {
            printf(", C=");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
            printf("\n");
          }
#endif
        }
      }
      break;

    case KEY_LEGACY_3DES :
      {
#ifdef _DEBUG_CIPHER
        printf("KEY_LEGACY_3DES, %d blocks\n", block_count);
#endif

        /* IV <- 0 */
        memset(ctx->init_vector, 0, sizeof(ctx->init_vector));
        for (i = 0; i < block_count; i++)
        {
#ifdef _DEBUG_CIPHER
          {
            printf("block %d/%d, IV=", i, block_count);
            for (j=0; j<8; j++)
              printf("%02X", ctx->init_vector[j]);
            printf(", P=");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
          }
#endif

          for (j = 0; j < 8; j++)
            data[8 * i + j] ^= ctx->init_vector[j];  /* P  <- P XOR IV  */

#ifdef _DEBUG_CIPHER
          {
            printf(" -> ");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
          }
#endif

          /* Legacy mode : PICC always encrypts, PCD always decrypts */
          TDES_Decrypt2(&ctx->cipher_context.tdes, ctx->init_vector, &data[8 * i]);  /* IV <- i3DES(P)   */

          memcpy(&data[8 * i], ctx->init_vector, 8); /* P  <- IV        */

#ifdef _DEBUG_CIPHER
          {
            printf(", C=");
            for (j=0; j<8; j++)
              printf("%02X", data[8 * i + j]);
            printf("\n");
          }
#endif
        }
      }
      break;

    case KEY_ISO_DES    :
    case KEY_ISO_3DES2K :
    case KEY_ISO_3DES3K :
      {
        /* Keep last IV */
        for (i = 0; i < block_count; i++)
        {
          for (j = 0; j < 8; j++)
            data[8 * i + j] ^= ctx->init_vector[j];  /* P  <- P XOR IV  */
          /* ISO mode : sending means encrypting */
          TDES_Encrypt2(&ctx->cipher_context.tdes, ctx->init_vector, &data[8 * i]);  /* IV <- 3DES(P)   */
          memcpy(&data[8 * i], ctx->init_vector, 8); /* P  <- IV        */
        }
      }
      break;

    case KEY_ISO_AES :
      {
        /* Keep last IV */
        for (i = 0; i < block_count; i++)
        {
          for (j = 0; j < 16; j++)
            data[16 * i + j] ^= ctx->init_vector[j];  /* P  <- P XOR IV  */
          /* ISO mode : sending means encrypting */
          AES_Encrypt2(&ctx->cipher_context.aes, ctx->init_vector, &data[16 * i]);  /* IV <- 3DES(P)   */
          memcpy(&data[16 * i], ctx->init_vector, 16); /* P  <- IV        */
        }
      }
      break;
  }

  *length = actual_length;

#ifdef _DEBUG_CIPHER
  {
    DWORD i;
    printf("        ==> ");
    for (i=0; i<*length; i++)
      printf("%02X", data[i]);
    printf("\n");
  }
#endif
}

void CipherSend_AES(BYTE data[], DWORD *length, DWORD max_length, BYTE Key[16], BYTE IV[16])
{
  AES_CTX_ST ctx;

  DWORD actual_length;
  DWORD block_size;
  DWORD block_count;
  DWORD i, j;

  if ((data == NULL) || (length == NULL))
    return;

  AES_Init(&ctx , Key);

  actual_length = *length;

  /* Padding  */
  block_size = 16 ;
  while (actual_length % block_size)
  {
    if (actual_length >= max_length)
      return;
    data[actual_length++] = 0x00;
  }

  block_count = (actual_length / block_size);

  for (i = 0; i < block_count; i++)
  {
    for (j = 0; j < 16; j++)
      data[16 * i + j] ^= IV[j];  /* P  <- P XOR IV */

    /* ISO mode : sending means encrypting  */
    AES_Encrypt2(&ctx, IV, &data[16 * i]); /* IV <- 3DES(P) */
    memcpy(&data[16 * i], IV, 16);         /* P  <- IV      */
  }

  *length = actual_length;

}


void Desfire_XferCipherSend(SPROX_PARAM  DWORD start_offset)
{
  DWORD block_size;
  DWORD actual_length;
  SPROX_DESFIRE_GET_CTX_V();

  if (start_offset > ctx->xfer_length)
    return;

  block_size = (ctx->session_type == KEY_ISO_AES) ? 16 : 8;

  /* Padd until we are aligned on block boundary */
  actual_length = ctx->xfer_length - start_offset;
  while (actual_length % block_size)
  {
    if (ctx->xfer_length >= sizeof(ctx->xfer_buffer))
      return;

    ctx->xfer_buffer[ctx->xfer_length++] = 0x00;
    actual_length++;
  }

  /* Cipher the data */
  Desfire_CipherSend(SPROX_PARAM_P  &ctx->xfer_buffer[start_offset], &actual_length, actual_length);
}




/**f* DesfireAPI/DES_SetKey
 *
 * NAME
 *   DES_SetKey
 *
 * DESCRIPTION
 *   Init a DES context with a 64-bit key
 *   Parity bits are not checked
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_DES_SetKey(const BYTE key[8]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable.
 *
 *   [[pcsc_desfire.dll]]
 *   Not applicable.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   Desfire_DES_Encrypt
 *   Desfire_DES_Decrypt
 *
 **/
SPROX_API_FUNC (Desfire_DES_SetKey) (SPROX_PARAM  const BYTE key[8])
{
  SPROX_DESFIRE_GET_CTX();

  if (key == NULL) return DF_PARAMETER_ERROR;

  DES_Init(&ctx->cipher_context.des, key);

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/DES_Encrypt
 *
 * NAME
 *   DES_Encrypt
 *
 * DESCRIPTION
 *   Standard DES encryption (E1 -> D2 -> E3)
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_DES_Encrypt(BYTE inoutbuf[8]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable.
 *
 *   [[pcsc_desfire.dll]]
 *   Not applicable.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   Desfire_DES_SetKey
 *   Desfire_DES_Decrypt
 *
 **/
SPROX_API_FUNC (Desfire_DES_Encrypt) (SPROX_PARAM  BYTE inoutbuf[8])
{
  SPROX_DESFIRE_GET_CTX();

  if (inoutbuf == NULL) return DF_PARAMETER_ERROR;

  DES_Encrypt(&ctx->cipher_context.des, inoutbuf);

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/DES_Decrypt
 * NAME
 *   DES_Decrypt
 *
 * DESCRIPTION
 *   Standard DES decryption (D3 -> E2 -> D1)
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_DES_Decrypt(BYTE inoutbuf[8]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable.
 *
 *   [[pcsc_desfire.dll]]
 *   Not applicable.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE AL
 *   Desfire_DES_SetKey
 *   Desfire_DES_Encrypt
 *
 **/
SPROX_API_FUNC (Desfire_DES_Decrypt) (SPROX_PARAM  BYTE inoutbuf[8])
{
  SPROX_DESFIRE_GET_CTX();

  if (inoutbuf == NULL) return DF_PARAMETER_ERROR;

  DES_Decrypt(&ctx->cipher_context.des, inoutbuf);

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/TDES_SetKey
 *
 * NAME
 *   TDES_SetKey
 *
 * DESCRIPTION
 *   Init a 3-DES context with 2 64-bit keys
 *   Parity bits are not checked
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_TDES_SetKey(const BYTE key1[8],
 *                                   const BYTE key2[8],
 *                                   const BYTE key3[8]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable.
 *
 *   [[pcsc_desfire.dll]]
 *   Not applicable.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   Desfire_TDES_Encrypt
 *   Desfire_TDES_Decrypt
 *
 **/
SPROX_API_FUNC (Desfire_TDES_SetKey) (SPROX_PARAM  const BYTE key1[8], const BYTE key2[8], const BYTE key3[8])
{
  SPROX_DESFIRE_GET_CTX();

  if (key1 == NULL) return DF_PARAMETER_ERROR;
  if (key2 == NULL) key2 = key1;
  if (key3 == NULL) key3 = key1;

  TDES_Init(&ctx->cipher_context.tdes, key1, key2, key3);

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/TDES_Encrypt
 *
 * NAME
 *   TDES_Encrypt
 *
 * DESCRIPTION
 *   Standard 3-DES encryption (E1 -> D2 -> E3)
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_TDES_Encrypt(BYTE inoutbuf[8]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable.
 *
 *   [[pcsc_desfire.dll]]
 *   Not applicable.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   Desfire_TDES_SetKey
 *   Desfire_TDES_Decrypt
 *
 **/
SPROX_API_FUNC (Desfire_TDES_Encrypt) (SPROX_PARAM  BYTE inoutbuf[8])
{
  SPROX_DESFIRE_GET_CTX();

  if (inoutbuf == NULL) return DF_PARAMETER_ERROR;

  TDES_Encrypt(&ctx->cipher_context.tdes, inoutbuf);

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/TDES_Decrypt
 * NAME
 *   TDES_Decrypt
 *
 * DESCRIPTION
 *   Standard 3-DES decryption (D3 -> E2 -> D1)
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_TDES_Decrypt(BYTE inoutbuf[8]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   Not applicable.
 *
 *   [[pcsc_desfire.dll]]
 *   Not applicable.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE AL
 *   Desfire_TDES_SetKey
 *   Desfire_TDES_Encrypt
 *
 **/
SPROX_API_FUNC (Desfire_TDES_Decrypt) (SPROX_PARAM  BYTE inoutbuf[8])
{
  SPROX_DESFIRE_GET_CTX();

  if (inoutbuf == NULL) return DF_PARAMETER_ERROR;

  TDES_Decrypt(&ctx->cipher_context.tdes, inoutbuf);

  /* Success. */
  return DF_OPERATION_OK;
}
