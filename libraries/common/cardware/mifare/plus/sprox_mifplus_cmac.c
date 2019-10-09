/**h* MifPlusAPI/CMAC
 *
 * NAME
 *   MifPlusAPI :: CMAC module
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of Mifare Plus MACing scheme.
 *
 **/
#include "sprox_mifplus_i.h"

#define RB_XOR_VALUE 0x87
#define BLOCK_SIZE   16

void MifPlus_InitCmac(SPROX_PARAM  const BYTE key[16])
{
  BYTE bMSB;
  int i;
  SPROX_MIFPLUS_GET_CTX_V();

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("MAC Key = ");
    for (i=0; i<16; i++)
      printf("%02X", key[i]);
    printf("\n");
  }
#endif

	AES_Init(&ctx->cmac_cipher, key);

  // Generate the padding bytes for O-MAC by enciphering a zero block
  // with the actual session key:
  memset(ctx->cmac_subkey_1, 0, sizeof(ctx->cmac_subkey_1));

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("Step1 : ");
    for (i=0; i<16; i++)
      printf("%02X", ctx->cmac_subkey_1[i]);
    printf("\n");
  }
#endif

	AES_Encrypt(&ctx->cmac_cipher, ctx->cmac_subkey_1);

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("Step2 : ");
    for (i=0; i<16; i++)
      printf("%02X", ctx->cmac_subkey_1[i]);
    printf("\n");
  }
#endif

  // If the MSBit of the generated cipher == 1 -> K1 = (cipher << 1) ^ Rb ...
  // store MSB:
  bMSB = ctx->cmac_subkey_1[0];

  // Shift the complete cipher for 1 bit ==> K1:
  for (i=0; i<(BLOCK_SIZE-1); i++)
  {
    ctx->cmac_subkey_1[i] <<= 1;
    // add the carry over bit:
    ctx->cmac_subkey_1[i] |= ((ctx->cmac_subkey_1[i+1] & 0x80) ? 0x01:0x00);
  }
  ctx->cmac_subkey_1[BLOCK_SIZE-1] <<= 1;
  if (bMSB & 0x80)
  {
    // XOR with Rb:
    ctx->cmac_subkey_1[BLOCK_SIZE-1] ^= RB_XOR_VALUE;
  }

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("CMAC SubKey1 =");
    for (i=0; i<16; i++)
      printf("%02X", ctx->cmac_subkey_1[i]);
    printf("\n");
  }
#endif

  // store MSB:
  bMSB = ctx->cmac_subkey_1[0];

  // Shift K1 ==> K2:
  for (i=0; i<(int) (BLOCK_SIZE-1); i++)
  {
    ctx->cmac_subkey_2[i]  = ctx->cmac_subkey_1[i] << 1;
    ctx->cmac_subkey_2[i] |= ((ctx->cmac_subkey_1[i+1] & 0x80) ? 0x01:0x00);
  }
  ctx->cmac_subkey_2[BLOCK_SIZE-1] = ctx->cmac_subkey_1[BLOCK_SIZE-1] << 1;

  if (bMSB & 0x80)
  {
    // XOR with Rb:
    ctx->cmac_subkey_2[BLOCK_SIZE - 1] ^= RB_XOR_VALUE;
  }

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("CMAC SubKey2 =");
    for (i=0; i<16; i++)
      printf("%02X", ctx->cmac_subkey_2[i]);
    printf("\n");
  }
#endif
}

void MifPlus_ComputeCmac(SPROX_PARAM  const BYTE data[], DWORD length, BYTE cmac[])
{
  BYTE  *buffer;
	BYTE vector[BLOCK_SIZE];
  DWORD  i, j, actual_length;
  SPROX_MIFPLUS_GET_CTX_V();

	memset(vector, 0, BLOCK_SIZE);

#ifdef MIFPLUS_DEBUG
  {
    DWORD i;
    printf("Compute CMAC < ");
    for (i=0; i<length; i++)
      printf("%02X", data[i]);
    printf("\n");
  }
#endif

  // First we enlarge eNumOfBytes to a multiple of the cipher block size for allocating
  // memory of the intermediate buffer. Zero padding will be done by the DF8Encrypt function.
  // If we are ISO-authenticated, we have to do the spetial padding for the O-MAC:
  actual_length = length;
  while (actual_length % BLOCK_SIZE)
    actual_length++;

  buffer = malloc(actual_length);
  memset(buffer, 0, actual_length);
  memcpy(buffer, data, length);

  /* Do the ISO padding and/or XORing */
  if (length % BLOCK_SIZE)
  {
    /* Block incomplete -> padding */
    buffer[length++] = 0x80;

    /* XOR the last eight bytes with CMAC_SubKey2 */
    length = actual_length - BLOCK_SIZE;
    for (i=0; i<BLOCK_SIZE; i++)
    {
      buffer[length + i] ^= ctx->cmac_subkey_2[i];
    }
  } else
  {
    /* Block complete -> no padding */

    /* XOR the last eight bytes with CMAC_SubKey1 */
    length = actual_length - BLOCK_SIZE;
    for (i=0; i<BLOCK_SIZE; i++)
    {
      buffer[length + i] ^= ctx->cmac_subkey_1[i];
    }
  }

  for (i = 0; i < actual_length; i+=16)
  {
    for (j = 0; j < 16; j++)
      buffer[i + j] ^= vector[j];
    AES_Encrypt(&ctx->cmac_cipher, &buffer[i]);
    memcpy(vector, &buffer[i], 16);
  }

  if (cmac != NULL)
  {
		for (i=0; i<8; i++)
		  cmac[i] = vector[1 + (i * 2)];
  }

  free(buffer);

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("CMAC =");
    for (i=0; i<8; i++)
      printf("%02X", cmac[i]);
    printf("\n");
  }
#endif
}

