/**h* MifPlusAPI/Level3
 *
 * NAME
 *   MifPlusAPI :: Implementation of Level 3 Read and Write
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Mifare Plus S and X Level 3 Read and Write functions.
 *
 * HISTORY
 *   23/02/2011 JDA : initial release
 *   28/03/2011 JDA : changed default communication modes to work 'out of
 *                    the box' with Mifare Plus S (and not only X)
 *
 **/
#include "sprox_mifplus_i.h"

/**f* MifPlusAPI/ReadEx
 *
 * NAME
 *   ReadEx
 *
 * DESCRIPTION
 *   Read data from a Mifare Plus card running at Level 3
 *
 * SYNOPSIS
 *
 *   [[sprox_milplus.dll]]
 *   SWORD SPROX_MifPlus_ReadEx(WORD address,
 *                              BYTE cmd_code,
 *                              BYTE count,
 *                              BYTE data[]);
 *
 *   [[sprox_milplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_ReadEx(SPROX_INSTANCE rInst,
 *                               BYTE cmd_code,
 *                               WORD address,
 *                               BYTE count,
 *                               BYTE data[]);
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_ReadEx(SCARDHANDLE hCard,
 *                             BYTE cmd_code,
 *                             WORD address,
 *                             BYTE count,
 *                             BYTE data[]);
 *
 * INPUTS
 *   BYTE cmd_code     : read command code. Valid values are
 *                       - Mifare Plus X : 0x30 to 0x37
 *                       - Mifare Plus S : 0x33 only (MFP_CMD_READ_PLAIN_MACED)
 *                       not every value may be used, depending on the access rights of the sector
 *   WORD address      : address of first block
 *   BYTE count        : number of blocks (max 15)
 *   BYTE data[]       : buffer to receive the data (size must be 16 * count)
 *
 * SEE ALSO
 *   Read
 *   ReadM
 *
 **/
SPROX_API_FUNC(MifPlus_ReadEx) (SPROX_PARAM  BYTE cmd_code, WORD address, BYTE count, BYTE data[])
{
  BYTE buffer[256];
  WORD i, j;
	WORD datalen, slen, rlen;
	SPROX_RC rc;
	BOOL c_mac = FALSE, r_mac = FALSE, r_cipher = FALSE;
	SPROX_MIFPLUS_GET_CTX();

  /* We are limited to 15 blocks by the size of all our buffers */
	if (count > 15)
	  return MFP_TOO_MANY_BLOCKS;

  switch (cmd_code)
	{
    case MFP_CMD_READ_PLAIN_UNMACED : break;
    case MFP_CMD_READ_PLAIN : c_mac = TRUE; break;
    case MFP_CMD_READ_PLAIN_UNMACED_R_MACED : r_mac = TRUE; break;
    case MFP_CMD_READ_PLAIN_MACED : c_mac = TRUE; r_mac = TRUE; break;
    case MFP_CMD_READ_UNMACED : r_cipher = TRUE; break;
		case MFP_CMD_READ : c_mac = TRUE; r_cipher = TRUE; break;
		case MFP_CMD_READ_UNMACED_R_MACED : r_mac = TRUE; r_cipher = TRUE; break;
		case MFP_CMD_READ_MACED : c_mac = TRUE; r_mac = TRUE; r_cipher = TRUE; break;

    default : return MFP_INVALID_CMD_CODE;
	}

  buffer[0] = cmd_code;
	buffer[1] = (BYTE) (address);
  buffer[2] = (BYTE) (address >> 8);
	buffer[3] = count;

	slen = 4;

	if (c_mac)
	{
	  /* MAC the command */
		/* --------------- */

	  BYTE mac_buffer[10];

		mac_buffer[0] = cmd_code;
		mac_buffer[1] = (BYTE)  ctx->read_counter;
		mac_buffer[2] = (BYTE) (ctx->read_counter >> 8);
		memcpy(&mac_buffer[3], ctx->transaction_id, 4);
		mac_buffer[7] = (BYTE)  address;
		mac_buffer[8] = (BYTE) (address >> 8);
		mac_buffer[9] = count;

	  MifPlus_ComputeCmac(SPROX_PARAM_P  mac_buffer, 10, &buffer[4]);

    slen += 8;
	}
	  
	/* Command/response Xchange */
	/* ------------------------ */

  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, slen, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

	ctx->read_counter++;

	/* Check status and format of response */
	/* ----------------------------------- */

	datalen = 16 * count;

	if (r_mac)
	{
    /* Response is MACed */
		/* ----------------- */

	  BYTE mac_buffer[256];
		BYTE calc_mac[8];

		rc = MifPlus_Result(buffer, rlen, (WORD) (datalen + 8));
		if (rc != MFP_SUCCESS)
			goto done;

		mac_buffer[0] = MFP_ERR_SUCCESS;
		mac_buffer[1] = (BYTE)  ctx->read_counter;
		mac_buffer[2] = (BYTE) (ctx->read_counter >> 8);
		memcpy(&mac_buffer[3], ctx->transaction_id, 4);
		mac_buffer[7] = (BYTE)  address;
		mac_buffer[8] = (BYTE) (address >> 8);
		mac_buffer[9] = count;
		memcpy(&mac_buffer[10], &buffer[1], datalen);

	  MifPlus_ComputeCmac(SPROX_PARAM_P  mac_buffer, 10 + datalen, calc_mac);

	  if (memcmp(calc_mac, &buffer[1 + datalen], 8))
		{
	    rc = MFP_WRONG_CARD_MAC;
			goto done;
		}

	} else
	{
    /* Response is not MACed */
		/* --------------------- */

		rc = MifPlus_Result(buffer, rlen, datalen);
		if (rc != MFP_SUCCESS)
			goto done;
	}

	if (r_cipher)
	{
	  /* Decipher the data */
		/* ----------------- */

	  BYTE cbc_vector[16];

	  MifPlus_GetCbcVector_Response(SPROX_PARAM_P  cbc_vector);

    for (i=0; i<datalen; i+=16)
		{
	    AES_Decrypt(&ctx->main_cipher, &buffer[1+i]);
	    for (j=0; j<16; j++)
	      buffer[1+i+j] ^= cbc_vector[j];
			memcpy(cbc_vector, &buffer[1+i], 16);
		}
	}

	if (data != NULL)
	  memcpy(data, &buffer[1], datalen);

done:  
  return rc;
}

/**f* MifPlusAPI/ReadM
 *
 * NAME
 *   ReadM
 *
 * DESCRIPTION
 *   Read multiple blocks from a Mifare Plus X card running at Level 3,
 *   using Plain+MACed communication mode (0x33, MFP_CMD_READ_PLAIN_MACED)
 *
 * NOTES
 *   To use other communication modes (Mifare Plus X only), use ReadEx instead,
 *   specifying explicitely the command code.
 *
 * SYNOPSIS
 *
 *   [[sprox_milplus.dll]]
 *   SWORD SPROX_MifPlus_ReadM(WORD address,
 *                             BYTE count,
 *                             BYTE data[]);
 *
 *   [[sprox_milplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_ReadM(SPROX_INSTANCE rInst,
 *                              WORD address,
 *                              BYTE count,
 *                              BYTE data[]);
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_ReadM(SCARDHANDLE hCard,
 *                            WORD address,
 *                            BYTE count,
 *                            BYTE data[]);
 *
 * INPUTS
 *   WORD address      : address of first block
 *   BYTE count        : number of blocks (max 15)
 *   BYTE data[]       : buffer to receive the data (size must be 16 * count)
 *
 * SEE ALSO
 *   Read
 *   ReadEx
 *
 **/
SPROX_API_FUNC(MifPlus_ReadM) (SPROX_PARAM  WORD address, BYTE count, BYTE data[])
{
  return SPROX_API_CALL(MifPlus_ReadEx) (SPROX_PARAM_P  MFP_CMD_READ_PLAIN_MACED, address, count, data);
}

/**f* MifPlusAPI/Read
 *
 * NAME
 *   Read
 *
 * DESCRIPTION
 *   Read a single block from a Mifare Plus X card running at Level 3,
 *   using Plain+MACed communication mode (0x33, MFP_CMD_READ_PLAIN_MACED)
 *
 * NOTES
 *   To use other communication modes (Mifare Plus X only), use ReadEx instead,
 *   specifying explicitely the command code.
 *
 * SYNOPSIS
 *
 *   [[sprox_milplus.dll]]
 *   SWORD SPROX_MifPlus_Read(WORD address,
 *                            BYTE data[16]);
 *
 *   [[sprox_milplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_Read(SPROX_INSTANCE rInst,
 *                             WORD address,
 *                             BYTE data[16]);
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_Read(SCARDHANDLE hCard,
 *                           WORD address,
 *                           BYTE data[16]);
 *
 * INPUTS
 *   WORD address      : address of block
 *   BYTE data[]       : buffer to receive the data (size must be 16)
 *
 * SEE ALSO
 *   ReadM
 *   ReadEx
 *
 **/
SPROX_API_FUNC(MifPlus_Read) (SPROX_PARAM  WORD address, BYTE data[])
{
  return SPROX_API_CALL(MifPlus_ReadEx) (SPROX_PARAM_P  MFP_CMD_READ_PLAIN_MACED, address, 1, data);
}

/**f* MifPlusAPI/WriteEx
 *
 * NAME
 *   WriteEx
 *
 * DESCRIPTION
 *   Write data to a Mifare Plus card running at Level 3
 *
 * SYNOPSIS
 *
 *   [[sprox_milplus.dll]]
 *   SWORD SPROX_MifPlus_WriteEx(WORD address,
 *                               BYTE cmd_code,
 *                               BYTE count,
 *                               const BYTE data[]);
 *
 *   [[sprox_milplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_WriteEx(SPROX_INSTANCE rInst,
 *                                BYTE cmd_code,
 *                                WORD address,
 *                                BYTE count,
 *                                const BYTE data[]);
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_WriteEx(SCARDHANDLE hCard,
 *                              BYTE cmd_code,
 *                              WORD address,
 *                              BYTE count,
 *                              const BYTE data[]);
 *
 * INPUTS
 *   BYTE cmd_code     : write command code. Valid values are
 *                       - Mifare Plus X : 0xA0 to 0xA3
 *                       - Mifare Plus S : 0xA1 or 0xA3
 *                       not every value may be used, depending on the access rights of the sector
 *   WORD address      : address of first block
 *   BYTE count        : number of blocks (max 15)
 *   const BYTE data[] : data to be writen (size must be 16 * count)
 *
 * SEE ALSO
 *   Write
 *   WriteM
 *
 **/
SPROX_API_FUNC(MifPlus_WriteEx) (SPROX_PARAM  BYTE cmd_code, WORD address, BYTE count, const BYTE data[16])
{
  BYTE buffer[256];
	WORD i, j;
	WORD datalen, slen, rlen;
	BOOL c_mac = FALSE, r_mac = FALSE, c_cipher = FALSE;
	SPROX_RC rc;
	SPROX_MIFPLUS_GET_CTX();

  /* We are limited to 15 blocks by the size of all our buffers */
	if (count > 15)
	  return MFP_TOO_MANY_BLOCKS;

  switch (cmd_code)
	{
    case MFP_CMD_WRITE_PLAIN : c_mac = TRUE; break;
    case MFP_CMD_WRITE : c_mac = TRUE; c_cipher = TRUE; break;
    case MFP_CMD_WRITE_PLAIN_MACED : c_mac = TRUE; r_mac = TRUE; break;
    case MFP_CMD_WRITE_MACED : c_mac = TRUE; c_cipher = TRUE; r_mac = TRUE; break;
    default : return MFP_INVALID_CMD_CODE;
	}

	datalen = 16 * count;

	buffer[0] = cmd_code;
	buffer[1] = (BYTE)  address;
	buffer[2] = (BYTE) (address >> 8);
	memcpy(&buffer[3], data, datalen);

	slen = 3 + datalen;

	if (c_cipher)
	{
	  /* Cipher the data */
		/* --------------- */

	  BYTE cbc_vector[16];

	  MifPlus_GetCbcVector_Command(SPROX_PARAM_P  cbc_vector);

    for (i=0; i<datalen; i+=16)
		{
	    for (j=0; j<16; j++)
	      buffer[3+i+j] ^= cbc_vector[j];
	    AES_Encrypt(&ctx->main_cipher, &buffer[3+i]);
		}
	}

	if (c_mac)
	{
	  /* MAC the command */
		/* --------------- */

	  BYTE mac_buffer[256];

		mac_buffer[0] = cmd_code;
		mac_buffer[1] = (BYTE)  ctx->write_counter;
		mac_buffer[2] = (BYTE) (ctx->write_counter >> 8);
		memcpy(&mac_buffer[3], ctx->transaction_id, 4);
		mac_buffer[7] = (BYTE)  address;
		mac_buffer[8] = (BYTE) (address >> 8);
		memcpy(&mac_buffer[9], &buffer[3], datalen);

	  MifPlus_ComputeCmac(SPROX_PARAM_P  mac_buffer, 9 + datalen, &buffer[3 + datalen]);

		slen += 8;
	}

	/* Command/response Xchange */
	/* ------------------------ */

  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, slen, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

	ctx->write_counter++;

	/* Check status and format of response */
	/* ----------------------------------- */

	if (r_mac)
	{
    /* Response is MACed */
		/* ----------------- */

		BYTE mac_buffer[7];
		BYTE calc_mac[8];

    rc = MifPlus_Result(buffer, rlen, 8);
	  if (rc != MFP_SUCCESS)
      goto done;

	  mac_buffer[0] = MFP_ERR_SUCCESS;
	  mac_buffer[1] = (BYTE)  ctx->write_counter;
	  mac_buffer[2] = (BYTE) (ctx->write_counter >> 8);
	  memcpy(&mac_buffer[3], ctx->transaction_id, 4);

	  MifPlus_ComputeCmac(SPROX_PARAM_P  mac_buffer, 7, calc_mac);

	  if (memcmp(calc_mac, &buffer[1], 8))
		{
	    rc = MFP_WRONG_CARD_MAC;
			goto done;
		}

	} else
	{
	  /* Response is not MACed */
		/* --------------------- */

    rc = MifPlus_Result(buffer, rlen, 0);
	  if (rc != MFP_SUCCESS)
      goto done;
	}

done:
  return rc;
}

/**f* MifPlusAPI/WriteM
 *
 * NAME
 *   WriteM
 *
 * DESCRIPTION
 *   Write multiple blocks to a Mifare Plus X card running at Level 3,
 *   using Plain+MACed communication mode (0xA3, MFP_CMD_WRITE_PLAIN_MACED)
 *
 * NOTES
 *   To use other communication modes, use WriteEx instead,
 *   specifying explicitely the command code.
 *
 * SYNOPSIS
 *
 *   [[sprox_milplus.dll]]
 *   SWORD SPROX_MifPlus_WriteM(WORD address,
 *                              BYTE count,
 *                              const BYTE data[]);
 *
 *   [[sprox_milplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_WriteM(SPROX_INSTANCE rInst,
 *                               WORD address,
 *                               BYTE count,
 *                               const BYTE data[]);
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_WriteM(SCARDHANDLE hCard,
 *                             WORD address,
 *                             BYTE count,
 *                             const BYTE data[]);
 *
 * INPUTS
 *   WORD address      : address of first block
 *   BYTE count        : number of blocks (max 15)
 *   const BYTE data[] : data to be writen (size must be 16 * count)
 *
 * SEE ALSO
 *   Write
 *   WriteEx
 *
 **/
SPROX_API_FUNC(MifPlus_WriteM) (SPROX_PARAM  WORD address, BYTE count, const BYTE data[])
{
  return SPROX_API_CALL(MifPlus_WriteEx) (SPROX_PARAM_P  MFP_CMD_WRITE_PLAIN_MACED, address, count, data);
}

/**f* MifPlusAPI/Write
 *
 * NAME
 *   Write
 *
 * DESCRIPTION
 *   Write a single block to a Mifare Plus X card running at Level 3,
 *   using Plain+MACed communication mode (0xA3, MFP_CMD_WRITE_PLAIN_MACED)
 *
 * NOTES
 *   To use other communication modes, use WriteEx instead,
 *   specifying explicitely the command code.
 *
 * SYNOPSIS
 *
 *   [[sprox_milplus.dll]]
 *   SWORD SPROX_MifPlus_Write(WORD address,
 *                             const BYTE data[16]);
 *
 *   [[sprox_milplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_Write(SPROX_INSTANCE rInst,
 *                              WORD address,
 *                              const BYTE data[16]);
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_Write(SCARDHANDLE hCard,
 *                            WORD address,
 *                            const BYTE data[16]);
 *
 * INPUTS
 *   WORD address        : address of block
 *   const BYTE data[16] : data to be writen (size must be 16)
 *
 * SEE ALSO
 *   Write
 *   WriteEx
 *
 **/
SPROX_API_FUNC(MifPlus_Write) (SPROX_PARAM  WORD address, const BYTE data[])
{
  return SPROX_API_CALL(MifPlus_WriteEx) (SPROX_PARAM_P  MFP_CMD_WRITE_PLAIN_MACED, address, 1, data);
}

