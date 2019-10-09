/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 14/09/2017
 * Time: 14:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_read.
  /// </summary>
  public partial class Desfire
  {
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
    
    /* DesfireAPI/ReadDataEx
     *
     * NAME
     *   ReadDataEx
     *
     * DESCRIPTION
     *   Allows to read data from a Standard Data File, a Backup Data File, a Cyclic File or a Linear Record File
     *
     * INPUTS
     *   byte read_command : command to send, DF_READ_DATA or DF_READ_RECORDS
     *   byte file_id      : ID of the file
     *   byte comm_mode    : communication mode
     *   UInt32 from_offset : starting position for the read operation
     *   UInt32 item_count  : maximum data length to read. Set to 0 to read whole file
     *   UInt32 item_size   : size if the item to read
     *   byte data[]       :  buffer to receive the data
     *   UInt32 *done_size  : actual data length read
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
    public long ReadDataEx(byte read_command, byte file_id, byte comm_mode, UInt32 from_offset, UInt32 item_count, UInt32 item_size, ref byte[] data, ref UInt32 done_size)
    {
      long status;
      UInt32    recv_length = 0;
      UInt32    byte_count, buffer_size;
      UInt32    temp;
    
      /* We have to calculate the number of bytes as this function works at byte granularity.
         Cast the UInt32 to double in the multiplication to detect an overflow in the result.
         If an overflow occurs, continue with the maximum number provided by a UInt32. The PICC
         will refuse the read request in this case with a BOUNDARY_ERROR. */
      byte_count = item_count * item_size;

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
    
      byte[] recv_buffer = new byte[buffer_size];
    
      xfer_length = 0;
    
      xfer_buffer[xfer_length++] = read_command;
      /* we do not have to take the item_size into account for the
         generation of the command header as the PICC resolves the meaning of this
         parameter due to the specific command code */
      xfer_buffer[xfer_length++] = file_id;
    
      temp = from_offset;
      xfer_buffer[xfer_length++] = (byte) (temp & 0x000000FF); temp >>= 8;
      xfer_buffer[xfer_length++] = (byte) (temp & 0x000000FF); temp >>= 8;
      xfer_buffer[xfer_length++] = (byte) (temp & 0x000000FF);
    
      temp = item_count;
      xfer_buffer[xfer_length++] = (byte) (temp & 0x000000FF); temp >>= 8;
      xfer_buffer[xfer_length++] = (byte) (temp & 0x000000FF); temp >>= 8;
      xfer_buffer[xfer_length++] = (byte) (temp & 0x000000FF);
    
      recv_buffer[recv_length++] = DF_OPERATION_OK;
    
      for (;;)
      {
        status = Command(0, COMPUTE_COMMAND_CMAC | FAST_CHAINING_ALLOWED | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);

        if (status != DF_OPERATION_OK)
          goto done;
    
        Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int) recv_length, (int) (xfer_length - 1));
        recv_length += (xfer_length - 1);
    
        if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
          break;
        
        xfer_length = 1;
      }
      /*
      {
        Console.Write("Received: ");
        for (int k=0; k<recv_length; k++)
          Console.Write(String.Format("{0:x02}", recv_buffer[k]));
        Console.Write("\n");
      }
     */
   
      /* correct termination of this loop only is possible if we received
         a frame with the status code sgbOPERATION_OK
         this means that this was the last frame sent by the PICC
         the additional frame status code which was prepared at the end of the loop
         is not sent but discarded */
      
      if (((session_type & KEY_ISO_MODE) != 0) && (comm_mode != DF_COMM_MODE_ENCIPHERED))
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
        status = VerifyCmacRecv(recv_buffer, ref recv_length);
      } else
      if (comm_mode == DF_COMM_MODE_MACED)
      {
        /* MACed communication */
        if ((session_type & KEY_ISO_MODE) != 0)
        {
          status = VerifyCmacRecv(recv_buffer, ref recv_length);
        } else
        {
          status = VerifyMacRecv(recv_buffer, ref recv_length);
        }
      } else
      if (comm_mode == DF_COMM_MODE_ENCIPHERED)
      {
        /* Enciphered communication */
        if ((session_type & KEY_ISO_MODE) != 0)
        {
          status = DecipherAfterReadIso(recv_buffer, ref recv_length, item_count, byte_count);
        } else
        {
          status = DecipherAfterRead(recv_buffer, ref recv_length, item_count, byte_count);
        }
        
      }
    
      if (status != DF_OPERATION_OK)
        goto done;
 
      
      /* Remove status byte from actual length */
      if (recv_length > 0) 
        recv_length--;
    
      if (item_count == 0)
      {
        /* if no length parameter has been passed,
           eNbytes is set with the number of received bytes
           this is valid for both, record files and data files */
        byte_count = recv_length;
      } else
      if (recv_length != byte_count)
      {
        /* if a length parameter has been passed we check, whether we got exactly that much data. */
        status = DFCARD_WRONG_LENGTH;
        goto done;
      }
    
      /* return the number of bytes if a pointer was passed */
     done_size = byte_count;
    
      /* copy data */
      if (data != null)
        Array.ConstrainedCopy(recv_buffer, 1, data, 0, (int) byte_count);
    
    done:
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
     *   SUInt16 SPROX_Desfire_ReadData(byte file_id,
     *                                     byte comm_mode,
     *                                     UInt32 from_offset,
     *                                     UInt32 max_count,
     *                                     byte data[],
     *                                     UInt32 *done_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ReadData(SPROX_INSTANCE rInst,
     *                                     byte file_id,
     *                                     byte comm_mode,
     *                                     UInt32 from_offset,
     *                                     UInt32 max_count,
     *                                     byte data[],
     *                                     UInt32 *done_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ReadData(SCARDHANDLE hCard,
     *                                     byte file_id,
     *                                     byte comm_mode,
     *                                     UInt32 from_offset,
     *                                     UInt32 max_count,
     *                                     byte data[],
     *                                     UInt32 *done_count);
     *
     * INPUTS
     *   byte file_id      : File IDentifier
     *   byte comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
     *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
     *                       datasheet of mifare DesFire MF3ICD40 for more information)
     *   UInt32 from_offset : starting position for the read operation
     *   UInt32 max_count   : maximum data length to read. Set to 0 to read whole file
     *   byte data[]       : buffer to receive the data
     *   UInt32 *done_count : actual data length read
     *
     * RETURNS
     *   DF_OPERATION_OK   : success, data has been read
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ReadData2
     *
     **/
    public long ReadData(byte file_id, byte comm_mode, UInt32 from_offset, UInt32 max_count, ref byte[] data, ref UInt32 done_count)
    {
      return ReadDataEx(DF_READ_DATA, file_id, comm_mode, from_offset, max_count, 1, ref data, ref done_count);
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
     *   SUInt16 SPROX_Desfire_ReadData2(byte file_id,
     *                                     UInt32 from_offset,
     *                                     UInt32 max_count,
     *                                     byte data[],
     *                                     UInt32 *done_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ReadData2(SPROX_INSTANCE rInst,
     *                                     byte file_id,
     *                                     UInt32 from_offset,
     *                                     UInt32 max_count,
     *                                     byte data[],
     *                                     UInt32 *done_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ReadData2(SCARDHANDLE hCard,
     *                                     byte file_id,
     *                                     UInt32 from_offset,
     *                                     UInt32 max_count,
     *                                     byte data[],
     *                                     UInt32 *done_count);
     *
     * INPUTS
     *   byte file_id      : File IDentifier
     *   UInt32 from_offset : starting position for the read operation in bytes
     *   UInt32 max_count   : maximum data length to read. Set to 0 to read whole file
     *   byte data[]       : buffer to receive the data
     *   UInt32 *done_count : actual data length read
     *
     * RETURNS
     *   DF_OPERATION_OK   : success, data has been read
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ReadData
     *
     **/

    public long ReadData2(byte file_id, UInt32 from_offset, UInt32 max_count, ref byte[] data, ref UInt32 done_count)
    {
      byte    comm_mode = 0;
      UInt16  access_rights = 0;
      byte    read_only_access;
      byte    read_write_access;
      long   status;

      // we have to receive the communications mode first 
      byte file_type;
      DF_FILE_SETTINGS file_settings;
      status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out file_settings);
      if (status != DF_OPERATION_OK)
        return status;
    
      // Depending on the AccessRights field (settings r and r/w) we have to decide whether    
      // we are able to communicate in the mode indicated by comm_mode.                        
      // If auth_key does neither match r nor r/w and one of this settings                
      // contains the value "ever" (0x0E) communication has to be done in plain mode regardless 
      // of the mode indicated by comm_mode.                                                  
      read_only_access  = (byte) ((access_rights & DF_READ_ONLY_ACCESS_MASK)  >> DF_READ_ONLY_ACCESS_SHIFT);
      read_write_access = (byte) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);
    
      if ((read_only_access  != session_key_id)
       && (read_write_access != session_key_id)
       && ((read_only_access == 0x0E) || (read_write_access == 0x0E)))
      {
        comm_mode = DF_COMM_MODE_PLAIN;
      }
    
      // Now execute the command
      return ReadData(file_id, comm_mode, from_offset, max_count, ref data, ref done_count);
    }

    long DecipherAfterRead(byte[] recv_buffer, ref UInt32 recv_length, UInt32 item_count, UInt32 byte_count)
    {
      UInt32 length;
    
      length = recv_length;
    
      /* check whether the received data count is multiple of 8 */
      length--;
      if ((length % 8) != 0)
      {
        return DF_LENGTH_ERROR;
      }
    
      /* at first we have to decipher the recv_buffer */
      byte[] tmp = new byte[length];
      Array.ConstrainedCopy(recv_buffer, 1, tmp, 0, (int) length);
      CipherRecv(ref tmp, ref length);
      Array.ConstrainedCopy(tmp, 0, recv_buffer, 1, (int) length);
      
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
      tmp = new byte[byte_count+2];
      Array.ConstrainedCopy(recv_buffer, 1, tmp, 0, (int) (byte_count+2));
      if (VerifyCrc16( tmp, byte_count, null) != DF_OPERATION_OK)
      {
        /* abortion due to integrity error -> wrong CRC */
        return DFCARD_WRONG_CRC;
      }
    
      recv_length = byte_count + 1;
      return DF_OPERATION_OK;
    }
    
    long DecipherAfterReadIso(byte[] recv_buffer, ref UInt32 recv_length, UInt32 item_count, UInt32 byte_count)
    {
      UInt32 length, block_size;

      length = recv_length;
    
      if (length < 1)
        return DFCARD_WRONG_LENGTH;
      
      length--;
    
      /* check whether the received data count is multiple of block size */
      block_size = (UInt32) ((session_type == KEY_ISO_AES) ? 16 : 8);
      if ((length % block_size) != 0)
        return DF_LENGTH_ERROR;
    
      /* at first we have to decipher the recv_buffer */
      byte[] tmp = new byte[length];
      Array.ConstrainedCopy(recv_buffer, 1, tmp, 0, (int) length);
      
      CipherRecv(ref tmp, ref length);
      Array.ConstrainedCopy(tmp, 0, recv_buffer, 1, (int) length);

    
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
      if (VerifyCrc32(recv_buffer, byte_count+1, true, null) != DF_OPERATION_OK)
      {
        /* abortion due to integrity error -> wrong CRC */
        return DFCARD_WRONG_CRC;
      }
    
      recv_length = byte_count+1;
      return DF_OPERATION_OK;
    }
    

  }
}
