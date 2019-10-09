/**h* MifPlusAPI/Cipher
 *
 * NAME
 *   MifPlusAPI :: Cipher module
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of Mifare Plus ciphering scheme.
 *
 **/
#include "sprox_mifplus_i.h"

void MifPlus_GetCbcVector_Command(SPROX_PARAM  BYTE vector[16])
{
  SPROX_MIFPLUS_GET_CTX_V();

	if (vector == NULL) return;

	memset(vector, 0, 16);

	if ((ctx->level == 3) && (ctx->in_session))
	{
	  memcpy(&vector[0], ctx->transaction_id, 4);
		vector[4] = (BYTE)  ctx->read_counter;
		vector[5] = (BYTE) (ctx->read_counter >> 8);
		vector[6] = (BYTE)  ctx->write_counter;
		vector[7] = (BYTE) (ctx->write_counter >> 8);
		memcpy(&vector[8],  &vector[4], 4);
		memcpy(&vector[12], &vector[4], 4);
	}
}

void MifPlus_GetCbcVector_Response(SPROX_PARAM  BYTE vector[16])
{
  SPROX_MIFPLUS_GET_CTX_V();

	if (vector == NULL) return;

	memset(vector, 0, 16);

	if ((ctx->level == 3) && (ctx->in_session))
	{
		vector[0] = (BYTE)  ctx->read_counter;
		vector[1] = (BYTE) (ctx->read_counter >> 8);
		vector[2] = (BYTE)  ctx->write_counter;
		vector[3] = (BYTE) (ctx->write_counter >> 8);
		memcpy(&vector[4], &vector[0], 4);
		memcpy(&vector[8], &vector[0], 4);
	  memcpy(&vector[12], ctx->transaction_id, 4);
	}
}

void MifPlus_ComputeKey(SPROX_PARAM  const BYTE rnd_pcd[16], const BYTE rnd_picc[16], BYTE key_constant, BYTE key_value[16])
{
  BYTE buffer[16];
	SPROX_MIFPLUS_GET_CTX_V();

	if (key_constant == MFP_CONSTANT_KEY_MAC)
	{
	  /* Special format for Kmac */
		buffer[0]  = rnd_pcd[7];
		buffer[1]  = rnd_pcd[8];
		buffer[2]  = rnd_pcd[9];
		buffer[3]  = rnd_pcd[10];
		buffer[4]  = rnd_pcd[11];

		buffer[5]  = rnd_picc[7];
		buffer[6]  = rnd_picc[8];
		buffer[7]  = rnd_picc[9];
		buffer[8]  = rnd_picc[10];
		buffer[9]  = rnd_picc[11];

		buffer[10] = rnd_pcd[0] ^ rnd_picc[0];
		buffer[11] = rnd_pcd[1] ^ rnd_picc[1];
		buffer[12] = rnd_pcd[2] ^ rnd_picc[2];
		buffer[13] = rnd_pcd[3] ^ rnd_picc[3];
		buffer[14] = rnd_pcd[4] ^ rnd_picc[4];

		buffer[15] = key_constant;
	} else
	{
		buffer[0]  = rnd_pcd[11];
		buffer[1]  = rnd_pcd[12];
		buffer[2]  = rnd_pcd[13];
		buffer[3]  = rnd_pcd[14];
		buffer[4]  = rnd_pcd[15];

		buffer[5]  = rnd_picc[11];
		buffer[6]  = rnd_picc[12];
		buffer[7]  = rnd_picc[13];
		buffer[8]  = rnd_picc[14];
		buffer[9]  = rnd_picc[15];

		buffer[10] = rnd_pcd[4] ^ rnd_picc[4];
		buffer[11] = rnd_pcd[5] ^ rnd_picc[5];
		buffer[12] = rnd_pcd[6] ^ rnd_picc[6];
		buffer[13] = rnd_pcd[7] ^ rnd_picc[7];
		buffer[14] = rnd_pcd[8] ^ rnd_picc[8];

		buffer[15] = key_constant;
	}

	AES_Encrypt(&ctx->main_cipher, buffer);

  memcpy(key_value, buffer, 16);
}
