/**h* MifPlusAPI/EV1
 *
 * NAME
 *   MifPlusAPI :: EV1
 *
 * COPYRIGHT
 *   (c) 2019 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   New features for Mifare Plus EV1
 *
 **/
#include "sprox_mifplus_i.h"


SPROX_API_FUNC(MifPlus_GetVersion) (SPROX_PARAM  BYTE version[], WORD max_size, WORD *got_size)
{
	BYTE buffer[256];
	BYTE response[256];
	WORD response_len = 0;
	WORD slen, rlen;
	SPROX_RC rc;
	SPROX_MIFPLUS_GET_CTX();
	
	buffer[0] = MFP_CMD_GET_VERSION;
	slen = 1;
	
	if (ctx->in_session)
	{
		BYTE mac_buffer[7];
		
		mac_buffer[0] = MFP_CMD_GET_VERSION;
		mac_buffer[1] = (BYTE)  ctx->read_counter;
		mac_buffer[2] = (BYTE) (ctx->read_counter >> 8);
		memcpy(&mac_buffer[3], ctx->transaction_id, 4);
		
		MifPlus_ComputeCmac(SPROX_PARAM_P  mac_buffer, sizeof(mac_buffer), &buffer[1]);
		slen += 8;
	}
	
	/* Part 1 */
	
	rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, slen, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
		goto done;

	ctx->read_counter++;
	
    rc = MifPlus_Result(buffer, rlen, 7);
	if (rc != (MFP_ERROR - MFP_CONTINUE))
		goto done;
	
	memcpy(&response[response_len], &buffer[1], rlen - 1);
	response_len += (rlen - 1);
	
	/* Part 2 */
	
	buffer[0] = MFP_CONTINUE;
	slen = 1;
	
	rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, slen, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
		goto done;
	
    rc = MifPlus_Result(buffer, rlen, 7);
	if (rc != (MFP_ERROR - MFP_CONTINUE))
		goto done;

	memcpy(&response[response_len], &buffer[1], rlen - 1);
	response_len += (rlen - 1);

	/* Part 3 */
	
	buffer[0] = MFP_CONTINUE;
	slen = 1;
	
	rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, slen, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
		goto done;
	
	if (rlen == 0)
	{
		rc = MFP_EMPTY_CARD_ANSWER;
		goto done;
	}

	if (buffer[0] != MFP_ERR_SUCCESS)
	{
		rc = MFP_ERROR-buffer[0];
		goto done;
	}
	
	memcpy(&response[response_len], &buffer[1], rlen - 1);
	response_len += (rlen - 1);
	
	if (ctx->in_session)
	{
		/* Check MAC in response */	
		BYTE mac_buffer[256];
		BYTE calc_mac[8];
		
		response_len -= 8;

		mac_buffer[0] = MFP_ERR_SUCCESS;
		mac_buffer[1] = (BYTE)  ctx->read_counter;
		mac_buffer[2] = (BYTE) (ctx->read_counter >> 8);
		memcpy(&mac_buffer[3], ctx->transaction_id, 4);
		memcpy(&mac_buffer[7], response, response_len);

		MifPlus_ComputeCmac(SPROX_PARAM_P  mac_buffer, 7 + response_len, calc_mac);

		if (memcmp(calc_mac, &response[response_len], 8))
		{
			rc = MFP_WRONG_CARD_MAC;
			goto done;
		}
	}	
	
	response_len -= 1;
	
	if (got_size != NULL)
		*got_size = response_len;
	
	if (version != NULL)
	{
		if (max_size < response_len)
		{
			response_len = max_size;
			rc = MFP_ERR_RESPONSE_OVERFLOW;
		}
		
		memcpy(version, response, response_len);
	}
	
	
done:
	return rc;
}

SPROX_API_FUNC(MifPlus_GetSignature) (SPROX_PARAM  BYTE signature[], WORD max_size, WORD *got_size)
{
	BYTE buffer[256];
	WORD i, j;
	WORD slen, rlen;
	SPROX_RC rc;
	BOOL c_mac = FALSE, r_mac = FALSE, r_cipher = FALSE;
	SPROX_MIFPLUS_GET_CTX();
	
	buffer[0] = MFP_CMD_READ_SIGNATURE;
	buffer[1] = 0x00;
	slen = 2;
	
	if (ctx->in_session)
	{
		/* Add MAC to command */
		BYTE mac_buffer[8];
				
		mac_buffer[0] = MFP_CMD_READ_SIGNATURE;
		mac_buffer[1] = (BYTE)  ctx->read_counter;
		mac_buffer[2] = (BYTE) (ctx->read_counter >> 8);
		memcpy(&mac_buffer[3], ctx->transaction_id, 4);
		mac_buffer[7] = 0x00;
		
		MifPlus_ComputeCmac(SPROX_PARAM_P  mac_buffer, sizeof(mac_buffer), &buffer[slen]);
		slen += 8;
	}
	
	rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, slen, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
		goto done;

	ctx->read_counter++;
	
	if (ctx->in_session)
	{
		/* Check MAC in response */	
		BYTE mac_buffer[256];
		BYTE calc_mac[8];

		rc = MifPlus_Result(buffer, rlen, (WORD) (64 + 8));
		if (rc != MFP_SUCCESS)
			goto done;

		mac_buffer[0] = MFP_ERR_SUCCESS;
		mac_buffer[1] = (BYTE)  ctx->read_counter;
		mac_buffer[2] = (BYTE) (ctx->read_counter >> 8);
		memcpy(&mac_buffer[3], ctx->transaction_id, 4);
		memcpy(&mac_buffer[7], &buffer[1], 64);

		MifPlus_ComputeCmac(SPROX_PARAM_P  mac_buffer, 7 + 64, calc_mac);

		if (memcmp(calc_mac, &buffer[1 + 64], 8))
		{
			rc = MFP_WRONG_CARD_MAC;
			goto done;
		}
	}
	
	if (ctx->in_session)
	{		
		/* Decipher the response */
		BYTE cbc_vector[16];
		
		MifPlus_GetCbcVector_Response(SPROX_PARAM_P  cbc_vector);
		
		for (i=0; i<64; i+=16)
		{
			AES_Decrypt(&ctx->main_cipher, &buffer[1+i]);
			for (j=0; j<16; j++)
				buffer[1+i+j] ^= cbc_vector[j];
			memcpy(cbc_vector, &buffer[1+i], 16);
		}
	}
	
	rlen -= 1;
		
	if (got_size != NULL)
		*got_size = rlen;
		
	if (signature != NULL)
	{
		if (max_size < rlen)
		{
			rlen = max_size;
			rc = MFP_ERR_RESPONSE_OVERFLOW;
		}
		
		memcpy(signature, &buffer[1], rlen);
	}
	
done:
	return rc;
}
