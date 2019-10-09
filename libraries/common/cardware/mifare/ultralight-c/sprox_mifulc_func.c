/**h* MifUlCAPI/Functions
 *
 * NAME
 *   MifUlCAPI :: Card functions functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of the various Mifare UltraLight C functions.
 *
 **/
#include "sprox_mifulc_i.h"

typedef struct
{
  union
  {
    DES_CTX_ST  des;
    TDES_CTX_ST tdes;
  } cipher;

  BYTE init_vector[8];

} AUTH_CTX_ST;

SPROX_API_FUNC(MifUlC_AuthenticateStep1) (SPROX_PARAM  AUTH_CTX_ST *ctx, BYTE rnd_picc[8]);
SPROX_API_FUNC(MifUlC_AuthenticateStep2) (SPROX_PARAM  AUTH_CTX_ST *ctx, const BYTE rnd_picc[8], const BYTE rnd_pcd[8]);

/**f* MifUlCAPI/Authenticate
 *
 * NAME
 *   Authenticate
 *
 * DESCRIPTION
 *   Perform authentication using the specified 3DES key on the currently selected
 *   Mifare UltraLight C card.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifulc.dll]]
 *   SWORD SPROX_MifUlC_Authenticate(const BYTE key_value[16]);
 *
 *   [[sprox_mifulc_ex.dll]]
 *   SWORD SPROXx_MifUlC_Authenticate(SPROX_INSTANCE rInst,
 *                                     const BYTE key_value[16]);
 *
 *   [[pcsc_mifulcdll]]
 *   LONG  SCardMifUlC_Authenticate(SCARDHANDLE hCard,
 *                                   const BYTE key_value[16]);
 *
 * INPUTS
 *   const BYTE key_value[16] : 16-byte Access Key (3DES2K key)
 *
 * RETURNS
 *   SUCCESS : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 **/
SPROX_API_FUNC(MifUlC_Authenticate) (SPROX_PARAM  const BYTE key_value[16])
{
  SPROX_RC rc;
  AUTH_CTX_ST ctx;
	BYTE rnd_picc[8], rnd_pcd[8];
	
#ifndef _USE_PCSC
  rc = SPROX_API_CALL(A_SelectAgain) (SPROX_PARAM_P  NULL, 0);
  if (rc)
    goto done;
#endif

  TDES_Init(&ctx.cipher.tdes, &key_value[0], &key_value[8], &key_value[0]);
  
  memset(ctx.init_vector, 0, 8);
  
	rc = SPROX_API_CALL(MifUlC_AuthenticateStep1) (SPROX_PARAM_P  &ctx, rnd_picc);
  if (rc)
    goto done;
    
  GetRandomBytes(SPROX_PARAM_P  rnd_pcd, 8);
  
	rc = SPROX_API_CALL(MifUlC_AuthenticateStep2) (SPROX_PARAM_P  &ctx, rnd_picc, rnd_pcd);
	
  if (rc)
    goto done;

done:
  return rc;
}

/*
 * MifUlC_AuthenticateStep1
 * ------------------------
 */
SPROX_API_FUNC(MifUlC_AuthenticateStep1) (SPROX_PARAM  AUTH_CTX_ST *ctx, BYTE rnd_picc[8])
{
  SPROX_RC rc = SUCCESS;
  BYTE buffer[64];
  BYTE rnd_picc_e[8];
  WORD rlen;

  BYTE i;
 
  /* Send the command to the card */
  /* ---------------------------- */
  
  buffer[0] = 0x1A;
  buffer[1] = 0x00;

  rlen = sizeof(buffer);
#ifndef _USE_PCSC
  rc = MifUlC_Command(SPROX_PARAM_P  buffer, 2, buffer, &rlen);
#else
  rc = MifUlC_CommandEncapsulated(SPROX_PARAM_P  buffer, 2, buffer, sizeof(buffer), &rlen);
#endif

  if (rc)
    goto done;

  /* Check that answer is acceptable */
  /* ------------------------------- */

  if (rlen != 9)
  {
    rc = -12; // MI_BYTECOUNTERR;
    goto done;
  }
  if (buffer[0] != 0xAF)
  {
    rc = -6; // MI_CODEERR;
    goto done;
  }

  /* Retrieve card's RndB */
  /* -------------------- */

  memcpy(rnd_picc_e, &buffer[1], 8);

  TDES_Decrypt2(&ctx->cipher.tdes, rnd_picc, rnd_picc_e);
  for (i=0; i<8; i++)
    rnd_picc[i] ^= ctx->init_vector[i];

  /* Remember the received cryptogram: this is the next init vector */
  memcpy(ctx->init_vector, rnd_picc_e, 8);

done:
  return rc;
}

SPROX_API_FUNC(MifUlC_AuthenticateStep2) (SPROX_PARAM  AUTH_CTX_ST *ctx, const BYTE rnd_picc[8], const BYTE rnd_pcd[8])
{
  SPROX_RC rc = SUCCESS;
  BYTE rnd_pcd_e[8];
  BYTE rnd_picc_e[8];

  BYTE buffer[24];
  WORD rlen;

  BYTE i;

  /* Encrypt RnA */
  /* ----------- */
  for (i=0; i<8; i++)
    rnd_pcd_e[i] = rnd_pcd[i] ^ ctx->init_vector[i];   /* P  <- P XOR IV  */
  TDES_Encrypt(&ctx->cipher.tdes, rnd_pcd_e);  /* IV <- 3DES(P)   */

  /* Remember the computed cryptogram: this is the next init vector */
  memcpy(ctx->init_vector, rnd_pcd_e, 8);               /* P  <- IV        */
   
	/* Compute RnB', rotating RndB to the right */
  /* ---------------------------------------- */
  for (i=0; i<7; i++)
    rnd_picc_e[i] = rnd_picc[i+1];
  rnd_picc_e[7] = rnd_picc[0];

	/* Encrypt RnB' */
  /* ------------ */
  for (i=0; i<8; i++)
    rnd_picc_e[i] ^= ctx->init_vector[i];               /* P  <- P XOR IV  */
  TDES_Encrypt(&ctx->cipher.tdes, rnd_picc_e);  /* IV <- 3DES(P)   */

  /* Remember the computed cryptogram: this is the next init vector */
  memcpy(ctx->init_vector, rnd_picc_e, 8);              /* P  <- IV        */

  /* Build the command buffer */
  /* ------------------------ */

  buffer[0] = 0xAF;
  memcpy(&buffer[1], rnd_pcd_e, 8);
  memcpy(&buffer[9], rnd_picc_e, 8);

  /* Do the exchange */
  /* --------------- */
  rlen = sizeof(buffer);
#ifndef _USE_PCSC
  rc = MifUlC_Command(SPROX_PARAM_P  buffer, 17, buffer, &rlen);
#else
  rc = MifUlC_CommandEncapsulated(SPROX_PARAM_P  buffer, 17, buffer, sizeof(buffer), &rlen);
#endif

  if (rc)
  {
    if (rc == -22) // MI_ACCESSERR)
      rc = -4; // MI_AUTHERR;
    goto done;
  }

  /* Check that answer is acceptable */
  /* ------------------------------- */

  if (buffer[0] != 0x00)
  {
    rc = -4; // MI_AUTHERR;
    goto done;
  }
  if (rlen != 9)
  {
    rc = -12; // MI_BYTECOUNTERR;
    goto done;
  }

  memcpy(rnd_pcd_e, &buffer[1], 8);

  TDES_Decrypt2(&ctx->cipher.tdes, rnd_pcd_e, rnd_pcd_e);
  for (i=0; i<8; i++)
    rnd_pcd_e[i] ^= ctx->init_vector[i];

  /* Compare RndA' with RndA */
  /* ----------------------- */

  if (memcmp(&rnd_pcd_e[0], &rnd_pcd[1], 7) || (rnd_pcd_e[7] != rnd_pcd[0]))
  {
    rc = -10; // MI_NOTAUTHERR;
    goto done;
  }



done:
  return rc;
}

/**f* MifUlCAPI/ChangeKey
 *
 * NAME
 *   ChangeKey
 *
 * DESCRIPTION
 *   Write the new 3DES key into a Mifare UltraLight C card.

 *
 * SYNOPSIS
 *
 *   [[sprox_mifulc.dll]]
 *   SWORD SPROX_MifUlC_ChangeKey(const BYTE key_value[16]);
 *
 *   [[sprox_mifulc_ex.dll]]
 *   SWORD SPROXx_MifUlC_ChangeKey(SPROX_INSTANCE rInst,
 *                                 const BYTE key_value[16]);
 *
 *   [[pcsc_mifulcdll]]
 *   LONG  SCardMifUlC_ChangeKey(SCARDHANDLE hCard,
 *                               const BYTE key_value[16]);
 *
 * INPUTS
 *   const BYTE key_value[16] : new 16-byte Access Key (3DES2K key)
 *
 * RETURNS
 *   SUCCESS : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 **/
SPROX_API_FUNC(MifUlC_ChangeKey) (SPROX_PARAM  const BYTE key_value[16])
{
  SPROX_RC rc;
  BYTE i, j;
  BYTE addr;
  BYTE data[16];
  
  for (i=0; i<16; i+=8)
  {
    for (j=0; j<8; j++)
      data[i+j] = key_value[i+7-j];
  }
  
  for (i=0; i<4; i++)
  {
    addr = 0x2C + i;

    rc = SPROX_API_CALL(MifUlC_Write4) (SPROX_PARAM_P  addr, &data[4 * i]);
    if (rc)
      break;
  }
  
  return rc;
}

/**f* MifUlCAPI/Read
 *
 * NAME
 *   Read
 *
 * DESCRIPTION
 *   Read 16 bytes (4 pages) from a Mifare UltraLight C card.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifulc.dll]]
 *   SWORD SPROX_MifUlC_Read(BYTE address,
 *                           BYTE data[16]);
 *
 *   [[sprox_mifulc_ex.dll]]
 *   SWORD SPROXx_MifUlC_Read(SPROX_INSTANCE rInst,
 *                            BYTE address,
 *                            BYTE data[16]);
 *
 *   [[pcsc_mifulcdll]]
 *   LONG  SCardMifUlC_Read(SCARDHANDLE hCard,
 *                          BYTE address,
 *                          BYTE data[16]);
 *
 * INPUTS
 *   BYTE address    : address of the first page to be read
 *   BYTE data[16]   : buffer for card's data
 *
 * RETURNS
 *   SUCCESS : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 **/
SPROX_API_FUNC(MifUlC_Read) (SPROX_PARAM  BYTE address, BYTE data[16])
{
  SPROX_RC rc;
  WORD rlen;
  BYTE buffer[64];

  /* Read binary */
  buffer[0] = 0x30;
  buffer[1] = address;

  rlen = sizeof(buffer);
  
#ifndef _USE_PCSC
  rc = MifUlC_Command(SPROX_PARAM_P  buffer, 2, buffer, &rlen);
#else
  rc = MifUlC_CommandEncapsulated(SPROX_PARAM_P  buffer, 2, buffer, sizeof(buffer), &rlen);
#endif

  if (rc)
    goto done;

  if (rlen != 16)
  {
    rc = -12; // MI_BYTECOUNTERR;
    goto done;
  }

  if (data != NULL)
  {
    memcpy(data, buffer, rlen);
  }

done:
  return rc;
}

/**f* MifUlCAPI/Write4
 *
 * NAME
 *   Write4
 *
 * DESCRIPTION
 *   Write 4 bytes (1 page) into a Mifare UltraLight C card.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifulc.dll]]
 *   SWORD SPROX_MifUlC_Write(BYTE address,
 *                            const BYTE data[4]);
 *
 *   [[sprox_mifulc_ex.dll]]
 *   SWORD SPROXx_MifUlC_Write(SPROX_INSTANCE rInst,
 *                             BYTE address,
 *                             const BYTE data[4]);
 *
 *   [[pcsc_mifulcdll]]
 *   LONG  SCardMifUlC_Write(SCARDHANDLE hCard,
 *                           BYTE address,
 *                           const BYTE data[4]);
 *
 * INPUTS
 *   BYTE address        : address of the page to be written
 *   const BYTE data[16] : new data
 *
 * RETURNS
 *   SUCCESS : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 **/
SPROX_API_FUNC(MifUlC_Write4) (SPROX_PARAM  BYTE address, const BYTE data[4])
{
  SPROX_RC rc;
  WORD rlen;
  BYTE buffer[64];

  if (data == NULL)
    return -248; // MI_LIB_CALL_ERROR;

  /* Write binary */
  buffer[0] = 0xA2;
  buffer[1] = address;
  memcpy(&buffer[2], data, 4);

  rlen = sizeof(buffer);
#ifndef _USE_PCSC
  rc = MifUlC_Command(SPROX_PARAM_P  buffer, 6, NULL, NULL);
#else
  rc = MifUlC_CommandEncapsulated(SPROX_PARAM_P  buffer, 6, buffer, sizeof(buffer), &rlen);
#endif

  if (rc == -2) // MI_CRCERR
    rc = SUCCESS;

  return rc;
}

/**f* MifUlCAPI/Write
 *
 * NAME
 *   Write
 *
 * DESCRIPTION
 *   Write 16 bytes (4 pages) into a Mifare UltraLight C card.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifulc.dll]]
 *   SWORD SPROX_MifUlC_Write(BYTE address,
 *                            const BYTE data[16]);
 *
 *   [[sprox_mifulc_ex.dll]]
 *   SWORD SPROXx_MifUlC_Write(SPROX_INSTANCE rInst,
 *                             BYTE address,
 *                             const BYTE data[16]);
 *
 *   [[pcsc_mifulcdll]]
 *   LONG  SCardMifUlC_Write(SCARDHANDLE hCard,
 *                           BYTE address,
 *                           const BYTE data[16]);
 *
 * INPUTS
 *   BYTE address        : address of the first page to be written
 *   const BYTE data[16] : new data
 *
 * RETURNS
 *   SUCCESS : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 **/
SPROX_API_FUNC(MifUlC_Write) (SPROX_PARAM  BYTE address, const BYTE data[16])
{
  SPROX_RC rc;
  BYTE i;

  for (i=0; i<4; i++)
  {
    rc = SPROX_API_CALL(MifUlC_Write4) (SPROX_PARAM_P  address, data);
    if (rc)
      break;

    address += 1;
    if (data != NULL) data += 4;
  }

  return rc;
}
