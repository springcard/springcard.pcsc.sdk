/**h* DesfireAPI/Read
 *
 * NAME
 *   DesfireAPI :: Core of the reading functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of the various DESFIRE read functions.
 *
 **/
#include "sprox_desfire_i.h"


static SPROX_RC DecipherAfterRead(SPROX_PARAM  BYTE recv_buffer[], DWORD *recv_length, DWORD item_count, DWORD byte_count);
static SPROX_RC DecipherAfterReadIso(SPROX_PARAM  BYTE recv_buffer[], DWORD *recv_length, DWORD item_count, DWORD byte_count);

/* DesfireAPI/ReadDataEx
 *
 * NAME
 *   ReadDataEx
 *
 * DESCRIPTION
 *   Allows to read data from a Standard Data File, a Backup Data File, a Cyclic File or a Linear Record File
 *
 * INPUTS
 *   BYTE read_command : command to send, DF_READ_DATA or DF_READ_RECORDS
 *   BYTE file_id      : ID of the file
 *   BYTE comm_mode    : communication mode
 *   DWORD from_offset : starting position for the read operation
 *   DWORD item_count  : maximum data length to read. Set to 0 to read whole file
 *   DWORD item_size   : size if the item to read
 *   BYTE data[]       :  buffer to receive the data
 *   DWORD *done_size  : actual data length read
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been read
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   ReadData
 *   ReadData2
 *   ReadRecords
 *   ReadRecords2
 *
 **/
SPROX_API_FUNC(Desfire_ReadDataEx) (SPROX_PARAM  BYTE read_command, BYTE file_id, BYTE comm_mode, DWORD from_offset, DWORD item_count, DWORD item_size, BYTE data[], DWORD *done_size)
{
  SPROX_RC status;
  DWORD    recv_length = 0;
  DWORD    byte_count, buffer_size;
  DWORD    temp;
#if 0
  BOOL     cheating;
#endif
  BYTE    *recv_buffer;
  SPROX_DESFIRE_GET_CTX();

  /* We have to calculate the number of bytes as this function works at byte granularity.
     Cast the DWORD to double in the multiplication to detect an overflow in the result.
     If an overflow occurs, continue with the maximum number provided by a DWORD. The PICC
     will refuse the read request in this case with a BOUNDARY_ERROR. */
  byte_count = item_count * item_size;

#if 0
  if ((byte_count == 51) && (ctx->session_type & KEY_ISO_MODE))
  {
    /* Is there's a bug in the card when we request 51 bytes ? */
    item_count++;
    byte_count+=item_size;
    cheating = TRUE;
  }
#endif

  if (byte_count > DF_MAX_FILE_DATA_SIZE)
  {
    /* Must overflow */
    byte_count  = 0xFFFFFFFF;
    buffer_size = DF_MAX_FILE_DATA_SIZE + 32;
  } else
  if (byte_count == 0)
  {
    /* Actual size is unknown */
    buffer_size = DF_MAX_FILE_DATA_SIZE + 32;
  } else
  {
    /* Actual size is specified */
    buffer_size = byte_count + 32;
  }

  recv_buffer = malloc(buffer_size);

  if (recv_buffer == NULL)
    return DFCARD_OUT_OF_MEMORY;


  ctx->xfer_length = 0;

  ctx->xfer_buffer[ctx->xfer_length++] = read_command;
  /* we do not have to take the item_size into account for the
     generation of the command header as the PICC resolves the meaning of this
     parameter due to the specific command code */
  ctx->xfer_buffer[ctx->xfer_length++] = file_id;

  temp = from_offset;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (temp & 0x000000FF); temp >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (temp & 0x000000FF); temp >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (temp & 0x000000FF);

  temp = item_count;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (temp & 0x000000FF); temp >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (temp & 0x000000FF); temp >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (temp & 0x000000FF);

  recv_buffer[recv_length++] = DF_OPERATION_OK;

  for (;;)
  {
    status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | FAST_CHAINING_ALLOWED | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);

    if (status != DF_OPERATION_OK)
      goto done;

    memcpy(&recv_buffer[recv_length], &ctx->xfer_buffer[INF + 1], ctx->xfer_length - 1);
    recv_length += (ctx->xfer_length - 1);

    if (ctx->xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
      break;

    ctx->xfer_length = 1;
  }

  /* correct termination of this loop only is possible if we received
     a frame with the status code sgbOPERATION_OK
     this means that this was the last frame sent by the PICC
     the additional frame status code which was prepared at the end of the loop
     is not sent but discarded */

  if ((ctx->session_type & KEY_ISO_MODE) && (comm_mode != DF_COMM_MODE_ENCIPHERED))
	{
	  /* there must be a 8-byte CMAC at the end of the frame */
		if ((recv_length < 9) || ((item_size > 1) && (((recv_length-9) % item_size) != 0) ))
			return DFCARD_WRONG_LENGTH;
	}

  /* decide upon the communications mode which cryptographic operation
     is to be applied on the received data */

  if ((comm_mode == DF_COMM_MODE_PLAIN) || (comm_mode == DF_COMM_MODE_PLAIN2))
  {
    /* Plain communication */
    status = Desfire_VerifyCmacRecv(SPROX_PARAM_P  recv_buffer, &recv_length);
  } else
  if (comm_mode == DF_COMM_MODE_MACED)
  {
    /* MACed communication */
    if (ctx->session_type & KEY_ISO_MODE)
    {
      status = Desfire_VerifyCmacRecv(SPROX_PARAM_P  recv_buffer, &recv_length);
    } else
    {
      status = Desfire_VerifyMacRecv(SPROX_PARAM_P  recv_buffer, &recv_length);
    }
  } else
  if (comm_mode == DF_COMM_MODE_ENCIPHERED)
  {
    /* Enciphered communication */
    if (ctx->session_type & KEY_ISO_MODE)
    {
#ifdef SPROX_DESFIRE_WITH_SAM
      if (ctx->sam_session_active)
      {
        BYTE *pbOut = malloc(buffer_size-1);
        DWORD dwOutLength = buffer_size-1;

        if (pbOut == NULL)
          return DF_OUT_OF_EEPROM_ERROR;

        status = SAM_DecipherData(ctx->sam_context.hSam, recv_buffer[0], byte_count, &recv_buffer[1], recv_length-1, FALSE, pbOut, &dwOutLength);
        if (status == DF_OPERATION_OK)
        {
          memcpy(&recv_buffer[1], pbOut, dwOutLength);
          recv_length = dwOutLength+1;
        }

        free(pbOut);

      } else
#endif
      {
        status = DecipherAfterReadIso(SPROX_PARAM_P  recv_buffer, &recv_length, item_count, byte_count);
      }

    } else
    {
#ifdef SPROX_DESFIRE_WITH_SAM
      if (ctx->sam_session_active)
      {
        BOOL fSkipData = TRUE;
        BYTE *pbOut = malloc(buffer_size-1);
        DWORD dwOutLength = buffer_size-1;

        if (pbOut == NULL)
          return DF_OUT_OF_EEPROM_ERROR;

        status = SAM_DecipherData(ctx->sam_context.hSam, recv_buffer[0], byte_count, &recv_buffer[1], recv_length-1, fSkipData, pbOut, &dwOutLength);
        if (status == DF_OPERATION_OK)
        {
          memcpy(&recv_buffer[1], pbOut, dwOutLength);
          recv_length = dwOutLength+1;
        }

        free(pbOut);
      } else
#endif
      {
        status = DecipherAfterRead(SPROX_PARAM_P  recv_buffer, &recv_length, item_count, byte_count);
      }
    }
  }

  if (status != DF_OPERATION_OK)
    goto done;

  /* Remove status byte from actual length */
  if (recv_length > 0) recv_length--;

  if (item_count == 0)
  {
    /* if no length parameter has been passed,
       eNBytes is set with the number of received bytes
       this is valid for both, record files and data files */
    byte_count = recv_length;
  } else
  if (recv_length != byte_count)
  {
    /* if a length parameter has been passed we check, whether we got exactly that much data. */
    status = DFCARD_WRONG_LENGTH;
    goto done;
  }

#if 0
  if (cheating)
    byte_count-=item_size;
#endif

  /* return the number of bytes if a pointer was passed */
  if (done_size != NULL)
    *done_size = byte_count;

  /* copy data */
  if (data != NULL)
    memcpy(data, &recv_buffer[1], byte_count);

done:
  free(recv_buffer);
  return status;
}


/**f* DesfireAPI/ReadData
 *
 * NAME
 *   ReadData
 *
 * DESCRIPTION
 *   Allows to read data from Standard Data File or Backup Data File
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ReadData(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD max_count,
 *                                     BYTE data[],
 *                                     DWORD *done_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ReadData(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD max_count,
 *                                     BYTE data[],
 *                                     DWORD *done_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ReadData(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD max_count,
 *                                     BYTE data[],
 *                                     DWORD *done_count);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
 *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
 *                       datasheet of mifare DesFire MF3ICD40 for more information)
 *   DWORD from_offset : starting position for the read operation
 *   DWORD max_count   : maximum data length to read. Set to 0 to read whole file
 *   BYTE data[]       : buffer to receive the data
 *   DWORD *done_count : actual data length read
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been read
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   ReadData2
 *
 **/
SPROX_API_FUNC(Desfire_ReadData) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, DWORD from_offset, DWORD max_count, BYTE data[], DWORD *done_count)
{
  return SPROX_API_CALL(Desfire_ReadDataEx) (SPROX_PARAM_P  DF_READ_DATA, file_id, comm_mode, from_offset, max_count, 1, data, done_count);
}

/**f* DesfireAPI/ReadData2
 *
 * NAME
 *   ReadData2
 *
 * DESCRIPTION
 *   Allows to read data from Standard Data File or Backup Data File
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ReadData2(BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD max_count,
 *                                     BYTE data[],
 *                                     DWORD *done_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ReadData2(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD max_count,
 *                                     BYTE data[],
 *                                     DWORD *done_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ReadData2(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD max_count,
 *                                     BYTE data[],
 *                                     DWORD *done_count);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   DWORD from_offset : starting position for the read operation in bytes
 *   DWORD max_count   : maximum data length to read. Set to 0 to read whole file
 *   BYTE data[]       : buffer to receive the data
 *   DWORD *done_count : actual data length read
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been read
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   ReadData
 *
 **/
SPROX_API_FUNC(Desfire_ReadData2) (SPROX_PARAM  BYTE file_id, DWORD from_offset, DWORD max_count, BYTE data[], DWORD *done_count)
{
  BYTE    comm_mode;
  WORD    access_rights;
  BYTE    read_only_access;
  BYTE    read_write_access;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* we have to receive the communications mode first */
  status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, NULL, &comm_mode, &access_rights, NULL);
  if (status != DF_OPERATION_OK)
    return status;

  /* Depending on the AccessRights field (settings r and r/w) we have to decide whether     */
  /* we are able to communicate in the mode indicated by comm_mode.                         */
  /* If ctx->auth_key does neither match r nor r/w and one of this settings                 */
  /* contains the value "ever" (0x0E) communication has to be done in plain mode regardless */
  /* of the mode indicated by comm_mode.                                                    */
  read_only_access  = (BYTE) ((access_rights & DF_READ_ONLY_ACCESS_MASK)  >> DF_READ_ONLY_ACCESS_SHIFT);
  read_write_access = (BYTE) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

  if ((read_only_access  != ctx->session_key_id)
   && (read_write_access != ctx->session_key_id)
   && ((read_only_access == 0x0E) || (read_write_access == 0x0E)))
  {
    comm_mode = DF_COMM_MODE_PLAIN;
  }

  /* Now execute the command */
  return SPROX_API_CALL(Desfire_ReadData) (SPROX_PARAM_P  file_id, comm_mode, from_offset, max_count, data, done_count);
}






static SPROX_RC DecipherAfterRead(SPROX_PARAM  BYTE recv_buffer[], DWORD *recv_length, DWORD item_count, DWORD byte_count)
{
  DWORD length;

  length = *recv_length;

  /* check whether the received data count is multiple of 8 */
  length--;
  if (length % 8)
  {
    return DF_LENGTH_ERROR;
  }

  /* at first we have to decipher the recv_buffer */
  Desfire_CipherRecv(SPROX_PARAM_P  &recv_buffer[1], &length);

  /* now there are two different paths caused by the two different padding methods */

  /* if the data length was specified in the parameter section of the command
     the recv_buffer content is padded with all zeroes or possibly none,
     after the CRC to reach multiples of 8 bytes, suitable for the DesOperation
     in the other case, where no data length was specified, the recv_buffer content is padded
     at least with one byte coded to 0x80 which is followed by all zeroes or possibly
     none, after the CRC to reach multiples of 8 bytes, suitable for the DesOperation */
  if (item_count != 0)
  {
    while (length > (byte_count + 2))
    {
      if (recv_buffer[length] != 0x00)
      {
        /* Fatal */
        return DFCARD_WRONG_PADDING;
      }
      length--;
    }

  } else
  {
    /* no length was specified, therefore the second padding rule applies
       we have to find the character 0x80 at the end of the received data
       and obtain the number of data bytes by subtracting 2 bytes from the position
       where 0x80 was found ( position + 1 = length ) */
    while (length > 0)
    {
      if (recv_buffer[length] == 0x80)
      {
        /* Size of CRC */
        length -= 2;
        break;
      }
      if (recv_buffer[length] != 0x00)
      {
        /* Wrong padding */
        return DFCARD_WRONG_PADDING;
      }

      /* Still in padding */
      length--;
    }

    byte_count = length-1;
  }

  /* this is the CRC check for both cases (specified and unspecified length) */
  if (Desfire_VerifyCrc16(SPROX_PARAM_P  &recv_buffer[1], byte_count, NULL) != DF_OPERATION_OK)
  {
    /* abortion due to integrity error -> wrong CRC */
    return DFCARD_WRONG_CRC;
  }

  *recv_length = byte_count + 1;
  return DF_OPERATION_OK;
}

static SPROX_RC DecipherAfterReadIso(SPROX_PARAM  BYTE recv_buffer[], DWORD *recv_length, DWORD item_count, DWORD byte_count)
{
  DWORD length, block_size;
  SPROX_DESFIRE_GET_CTX();

  length = *recv_length;

  if (length < 1)
    return DFCARD_WRONG_LENGTH;
  length--;

  /* check whether the received data count is multiple of block size */
  block_size = (ctx->session_type == KEY_ISO_AES) ? 16 : 8;
  if (length % block_size)
    return DF_LENGTH_ERROR;

  /* at first we have to decipher the recv_buffer */
  Desfire_CipherRecv(SPROX_PARAM_P  &recv_buffer[1], &length);

  /* now there are two different paths caused by the two different padding methods */

  /* if the data length was specified in the parameter section of the command
     the recv_buffer content is padded with all zeroes or possibly none,
     after the CRC to reach multiples of 8 bytes, suitable for the DesOperation
     in the other case, where no data length was specified, the recv_buffer content is padded
     at least with one byte coded to 0x80 which is followed by all zeroes or possibly
     none, after the CRC to reach multiples of 8 bytes, suitable for the DesOperation */
  if (item_count != 0)
  {
    while (length > (byte_count + 4))
    {
      if (recv_buffer[length] != 0x00)
      {
        /* Fatal */
        return DFCARD_WRONG_PADDING;
      }
      length--;
    }

  } else
  {
    /* no length was specified, therefore the second padding rule applies
       we have to find the character 0x80 at the end of the received data
       and obtain the number of data bytes by subtracting 2 bytes from the position
       where 0x80 was found ( position + 1 = length ) */

    while (length > 0)
    {
      if (recv_buffer[length] == 0x80)
      {
        /* Size of CRC */
        length -= 4;
        break;
      }
      if (recv_buffer[length] != 0x00)
      {
        /* Wrong padding */
        return DFCARD_WRONG_PADDING;
      }

      /* Still in padding */
      length--;
    }

    byte_count = length-1;
  }

  /* this is the CRC check for both cases (specified and unspecified length) */
  // In ISO mode the status byte is added at the end of the data block
  // for CRC32 calculation:
  if (Desfire_VerifyCrc32(SPROX_PARAM_P  &recv_buffer[0], byte_count+1, TRUE, NULL) != DF_OPERATION_OK)
  {
    /* abortion due to integrity error -> wrong CRC */
    return DFCARD_WRONG_CRC;
  }

  *recv_length = byte_count+1;
  return DF_OPERATION_OK;
}

