/**h* DesfireAPI/Write
 *
 * NAME
 *   DesfireAPI :: Core of the writing functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of the various DESFIRE write functions.
 *
 **/
#include "sprox_desfire_i.h"

/**f* DesfireAPI/WriteData
 *
 * NAME
 *   WriteData
 *
 * DESCRIPTION
 *   Allows to write data from Standard Data File or Backup Data File
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_WriteData(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_WriteData(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_WriteData(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
 *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
 *                       datasheet of mifare DesFire MF3ICD40 for more information)
 *   DWORD from_offset : starting position for the write operation in bytes
 *   DWORD size        : size of the buffer in bytes
 *   BYTE data[]       : buffer to write to the card
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   WriteData2
 *
 **/
SPROX_API_FUNC(Desfire_WriteData) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, DWORD from_offset, DWORD size, const BYTE data[])
{
  return SPROX_API_CALL(Desfire_WriteDataEx) (SPROX_PARAM_P  DF_WRITE_DATA, file_id, comm_mode, from_offset, size, data);
}

/**f* DesfireAPI/WriteData2
 *
 * NAME
 *   WriteData2
 *
 * DESCRIPTION
 *   Allows to write data from Standard Data File or Backup Data File
 *
  * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_WriteData2(BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_WriteData2(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_WriteData2(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   DWORD from_offset : starting position for the write operation in bytes
 *   DWORD size        : size of the buffer in bytes
 *   BYTE data[]       : buffer to write to the card
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   WriteData
 *
 **/
SPROX_API_FUNC(Desfire_WriteData2) (SPROX_PARAM  BYTE file_id, DWORD from_offset, DWORD size, const BYTE data[])
{
  BYTE    comm_mode;
  WORD    access_rights;
  BYTE    write_only_access;
  BYTE    read_write_access;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* we have to receive the communications mode first */
  status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, NULL, &comm_mode, &access_rights, NULL);
  if (status != DF_OPERATION_OK)
    return status;

  /* Depending on the access_rights field (settings w and r/w) we have to decide whether   */
  /* we are able to communicate in the mode indicated by comm_mode.                        */
  /* If ctx->auth_key does neither match w nor r/w and one of this settings                */
  /* contains the value "ever" (0xE) communication has to be done in plain mode regardless */
  /* of the mode indicated by comm_mode.                                                   */
  write_only_access = (BYTE) ((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
  read_write_access = (BYTE) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

  if ((write_only_access != ctx->session_key_id)
   && (read_write_access != ctx->session_key_id)
   && ((write_only_access == 0x0E) || (read_write_access == 0x0E)))
  {
    comm_mode = DF_COMM_MODE_PLAIN;
  }

  /* Now execute the command */
  return SPROX_API_CALL(Desfire_WriteData) (SPROX_PARAM_P  file_id, comm_mode, from_offset, size, data);
}



/* DesfireAPI/WriteDataEx
 *
 * NAME
 *   WriteDataEx
 *
 * DESCRIPTION
 *   Allows to write data from a Standard Data File, a Backup Data File, a Cyclic File or a Linear Record File
 *
 * INPUTS
 *   BYTE write_command : command to send, DF_WRITE_DATA or DF_WRITE_RECORD
 *   BYTE file_id       : ID of the file
 *   BYTE comm_mode     : communication mode
 *   DWORD from_offset  : starting position for the write operation
 *   DWORD size         : size of the buffer
 *   BYTE data[]        : buffer to write to the card
 *
 * RETURNS
 *   DF_OPERATION_OK    : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   WriteData
 *   WriteData2
 *   WriteRecord
 *   WriteRecord2
 *
 **/
SPROX_API_FUNC(Desfire_WriteDataEx) (SPROX_PARAM  BYTE write_command, BYTE file_id, BYTE comm_mode, DWORD from_offset, DWORD size, const BYTE data[])
{
  SPROX_RC status;
  DWORD    buffer_size, max_frame_length, length, next_length, done_length = 0;
  BYTE     comm_flags;
  BYTE    *buffer;
  DWORD    temp;
  SPROX_DESFIRE_GET_CTX();

  if (ctx->iso_wrapping == DF_ISO_WRAPPING_CARD)
  {
    /* 5 bytes are used by the ISO APDU header */
    max_frame_length = DF_MAX_INFO_FRAME_SIZE - 5;
  } else
  {
    max_frame_length = DF_MAX_INFO_FRAME_SIZE;
  }

  buffer_size = size+64; // TODO : confirmer la longueur

  buffer = malloc(buffer_size);
  if (buffer == NULL)
    return DFCARD_OUT_OF_MEMORY;

  memset(buffer, 0x00, buffer_size); // TODO

  length = 0;
  buffer[length++] = write_command;
  buffer[length++] = file_id;

  temp = from_offset;
  buffer[length++] = (BYTE) (temp & 0x000000FF); temp >>= 8;
  buffer[length++] = (BYTE) (temp & 0x000000FF); temp >>= 8;
  buffer[length++] = (BYTE) (temp & 0x000000FF);

  temp = size;
  buffer[length++] = (BYTE) (temp & 0x000000FF); temp >>= 8;
  buffer[length++] = (BYTE) (temp & 0x000000FF); temp >>= 8;
  buffer[length++] = (BYTE) (temp & 0x000000FF);

  memcpy(&buffer[length], data, size);
  length += size;

  /* decide upon the communications mode which cryptographic */
  /* operation is to be applied on the data                  */

  if (comm_mode == DF_COMM_MODE_ENCIPHERED)
  {
    DWORD pos_data = length - size;
    DWORD len_data = size;
#ifdef SPROX_DESFIRE_WITH_SAM
    if (ctx->sam_session_active)
    {
      BYTE pbOut[MAX_DATA_SIZE];
      DWORD dwOutLength = sizeof(pbOut);

      if (ctx->session_type & KEY_ISO_MODE)
      {
        status = SAM_EncipherData(ctx->sam_context.hSam, buffer, length, (BYTE) pos_data, pbOut, &dwOutLength);
        if (status == DF_OPERATION_OK)
        {
          memcpy(&buffer[pos_data], pbOut, dwOutLength);
          length = pos_data + dwOutLength;
        } else
        {
          return status;
        }

      } else
      {
        /* Non-ISO authenticated: don't send command */
        BYTE pbOut[MAX_DATA_SIZE];
        DWORD dwOutLength = sizeof(pbOut);
        status = SAM_EncipherData(ctx->sam_context.hSam, &buffer[pos_data], len_data, 0, pbOut, &dwOutLength);
        if (status == DF_OPERATION_OK)
        {
          memcpy(&buffer[pos_data], pbOut, dwOutLength);
          length = pos_data + dwOutLength;
        } else
        {
          return status;
        }

      }

    } else
#endif
    {
      if (ctx->session_type & KEY_ISO_MODE)
      {
        /* at first we have to append the CRC bytes, computed other the whole buffer */
        Desfire_ComputeCrc32(SPROX_PARAM_P  buffer, length, &buffer[length]);
        length   += 4;
        len_data += 4;
      } else
      {
        /* we compute the CRC other the data only */
        Desfire_ComputeCrc16(SPROX_PARAM_P  &buffer[pos_data], len_data, &buffer[pos_data + len_data]);
        len_data += 2;
        length   += 2;
      }

      /* finally do the padding and the cipher operation on the data only */
      Desfire_CipherSend(SPROX_PARAM_P  &buffer[pos_data], &len_data, buffer_size - pos_data);

      length = pos_data + len_data;
    }

  } else
  if (comm_mode == DF_COMM_MODE_MACED)
  {
    if (ctx->session_type & KEY_ISO_MODE)
    {
      /* append the 8 bytes CMAC (computed over the whole buffer) */
#ifdef SPROX_DESFIRE_WITH_SAM
      if (ctx->sam_session_active)
      {
        BYTE pbMac[8];
        DWORD dwMacLength = sizeof(pbMac), k;

        status = SAM_GenerateMAC(ctx->sam_context.hSam, buffer, length, pbMac, &dwMacLength);
        if (status == DF_OPERATION_OK)
        {
          for (k=0; k<dwMacLength; k++)
            buffer[length++] = pbMac[k];
        } else
        {
          return status;
        }
      } else
#endif
      {
        Desfire_ComputeCmac(SPROX_PARAM_P  buffer, length, FALSE, &buffer[length]);
        length += 8;
      }

    } else
    {
      /* append the 4 bytes MAC (computed over the data only) */
#ifdef SPROX_DESFIRE_WITH_SAM
      if (ctx->sam_session_active)
      {
        BYTE pbMac[4];
        DWORD dwMacLength = sizeof(pbMac), k;

        status = SAM_GenerateMAC(ctx->sam_context.hSam, &buffer[length - size ], size, pbMac, &dwMacLength);
        if (status == DF_OPERATION_OK)
        {
          for (k=0; k<dwMacLength; k++)
            buffer[length++] = pbMac[k];
        } else
        {
          return status;
        }
      } else
#endif
      {
        Desfire_ComputeMac(SPROX_PARAM_P  data, size, &buffer[length]);
        length += 4;
      }
    }
  } else
  {
    /* if comm_mode is neither MACed nor ciphered we leave the data as it is */
    /* this means a plain communication                                      */
    if (ctx->session_type & KEY_ISO_MODE)
    {
      /* compute the 8 bytes CMAC, but do not send it */
#ifdef SPROX_DESFIRE_WITH_SAM
      if (ctx->sam_session_active)
      {
        BYTE pbMac[8];
        DWORD dwMacLength = sizeof(pbMac);
        SAM_GenerateMAC(ctx->sam_context.hSam, buffer, length, pbMac, &dwMacLength);
      } else
#endif
      {
        Desfire_ComputeCmac(SPROX_PARAM_P  buffer, length, FALSE, NULL);
      }
    }
  }


  /* Now our data is ready for being sent to the PICC */

  do
  {
    /* determine the limiting factor for the number of bytes to be appended to     */
    /* the ctx->xfer_buffer                                                        */
    /* this is either the maximum frame size or the number of bytes which are left */
    if ((length - done_length) > max_frame_length)
    {
      /* transfer the calculated number of bytes  */
      next_length = max_frame_length;
    } else
    {
      /* transfer all the bytes left  */
      next_length = length - done_length;
    }
    /* Only the first frame has its actual size, others has to be shortened by one */
    /* to put the ADDITIONAL_FRAME header                                          */
    if ((next_length == max_frame_length) && (done_length > 0))
      next_length--;

    if ((done_length + next_length) >= length)
    {
      /* This will be the last frame */
      comm_flags = CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK;
    } else
    {
      /* Chaining expected */
      comm_flags = WANTS_ADDITIONAL_FRAME;
    }

    /* Now put these bytes into the ctx->xfer_buffer */
    if (done_length == 0)
    {
      /* First frame */
      memcpy(&ctx->xfer_buffer[INF + 0], &buffer[0], next_length);
      ctx->xfer_length = (WORD) next_length;
    } else
    {
      /* Next frame */
      ctx->xfer_buffer[INF + 0] = DF_ADDITIONAL_FRAME;
      memcpy(&ctx->xfer_buffer[INF + 1], &buffer[done_length], next_length);
      ctx->xfer_length = (WORD) (1 + next_length);
    }

    /* only one byte response is allowed                                */
    /* allowed status codes are DF_ADDITIONAL_FRAME and DF_OPERATION_OK */
    status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  1, comm_flags);
    if (status != DF_OPERATION_OK)
    {
      break;
    }

    done_length += next_length;

  } while (done_length < length);

  /* because ( eNumOfBytesExtracted = eNBytesToWrite ) is the only way for     */
  /* leaving the loop correctly an interrupted write operation is detected via */
  /* a status code different from DF_OPERATION_OK                              */

  free(buffer);

  return status;
}

