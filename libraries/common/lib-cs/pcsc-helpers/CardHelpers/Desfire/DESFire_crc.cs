/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 12/09/2017
 * Time: 16:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_crc.
  /// </summary>
  public partial class Desfire
  {
    /**h* DesfireAPI/CRC
     *
     * NAME
     *   DesfireAPI :: CRC module
     *
     * COPYRIGHT
     *   (c) 2009 SpringCard - www.springcard.com
     *
     * DESCRIPTION
     *   Implementation of the DESFIRE CRC16 and CRC32 functions.
     *
     * HISTORY
     *   14/11/2011 JDA : corrected bug in Desfire_VerifyCrc32
     *                    (buffer not restored correctly if move_status == TRUE)
     *
     **/
    void UpdateDesfireCrc16(byte ch, ref UInt16 lpwCrc)
    {
      ch = (byte) (ch ^ (byte) (lpwCrc & 0x00FF));
      ch = (byte) (ch ^ (byte) ((ch << 4) & 0x00FF));
   
      lpwCrc = (UInt16) ((lpwCrc >> 8) ^ ((UInt16) ch << 8) ^ ((UInt16) ch << 3) ^ ((UInt16) ch >> 4));
    }
    
    UInt16 ComputeCrc16(byte[] buffer, UInt32 size, ref byte[] crc)
    {
      byte  chBlock;
      UInt16  wCrc = 0x6363; /* ITU-V.41 */
      byte[] p = buffer;
    
      if (buffer == null)
        return wCrc;

      int offset=0;
      do
      {
        chBlock = p[offset++];
        UpdateDesfireCrc16(chBlock, ref wCrc);
      } while ((--size) > 0);

    
      if (crc != null)
      {
        crc[0] = (byte) (wCrc & 0xFF);
        crc[1] = (byte) ((wCrc >> 8) & 0xFF);
      }
    
      return wCrc;
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /**************************************************************************************************/
    //  this function calculates a 32 bit CRC value (Ethernet AAL5 standard )
    //  The generator polynome is: x32 + x26 + x23 + x22 + x16 + x12 + x11 + x10 + x8 + x7 + x5 + x4 + x2 + x1 + 1
    //INPUT:
    //      unsigned char bInput.............the 8 bit value is added to the 32 bit CRC value (peReg)
    //OUTPUT:
    //      unsigned long *peReg............pointer to the 32 bit CRC value
    
    static void UpdateDesfireCrc32(byte ch, ref UInt32 pdwCrc)
    {
      byte b;
    
      pdwCrc ^= ch;
    
      //  bit wise calcualtion of the CRC value
      for (b=0; b<8; b++)
      {
        if ((pdwCrc & 0x00000001) != 0)
        {
          pdwCrc >>= 1;
          pdwCrc  ^= 0xEDB88320;
        } else
        {
          pdwCrc >>= 1;
        }
      }
    }
    
    UInt32 ComputeCrc32(byte[] buffer, UInt32 size, ref byte[] crc)
    {
      UInt32 dwCrc = 0xFFFFFFFF;
      byte[] p = buffer;
     
      int offset=0;
      do
      {
        UpdateDesfireCrc32(p[offset++], ref dwCrc);
      } while ((--size) > 0);
    
      if (crc != null)
      {
        crc[0] = (byte) ((dwCrc)       & 0x000000FF);
        crc[1] = (byte) ((dwCrc >> 8)  & 0x000000FF);
        crc[2] = (byte) ((dwCrc >> 16) & 0x000000FF);
        crc[3] = (byte) ((dwCrc >> 24) & 0x000000FF);
      }
    
      return dwCrc;
    }
    
    
    
    
    
    void XferAppendCrc(UInt32 start_offset)
    {  
      if (start_offset >= xfer_length) 
        return;
    
      if ((session_type & KEY_ISO_MODE) != 0)
      {
        if ((xfer_length + 4) > xfer_buffer.Length) 
          return;
    
        byte[] tmp = new byte[4];
        Array.ConstrainedCopy(xfer_buffer, (int) xfer_length, tmp, 0, 4);
        ComputeCrc32(xfer_buffer, xfer_length, ref tmp);
        Array.ConstrainedCopy(tmp, 0, xfer_buffer, (int) xfer_length, 4);
        
        xfer_length += 4;
    
      } else
      {
        if ((xfer_length + 2) > xfer_buffer.Length)
          return;
        
        byte[] tmp = new byte[xfer_length - start_offset];
        Array.ConstrainedCopy(xfer_buffer, (int) start_offset, tmp, 0, (int) (xfer_length - start_offset));
        byte[] crc = new byte[2];
        ComputeCrc16(tmp, xfer_length - start_offset, ref crc);
        Array.ConstrainedCopy(crc, 0, xfer_buffer, (int) xfer_length, 2);
        xfer_length += 2;
      }
    }
    
   
    long VerifyCrc16(byte[] data, UInt32 length, byte[] crc)
    {
      byte[] check = new byte[2];
      
      if (data == null)
        return DFCARD_LIB_CALL_ERROR;
          
      if (crc == null)
      {
        crc = new byte[2];
        crc[0] = data[length];
        crc[1] = data[length+1];
      }
      ComputeCrc16(data, length, ref check);

      
      for (int i=0; i<2; i++)
        if (check[i] != crc[i])
          return DFCARD_WRONG_CRC;

      return DF_OPERATION_OK;
    }

   
    long VerifyCrc32(byte[] data, UInt32 length, bool move_status, byte[] crc)
    {
      byte[] check = new byte[4];
      byte status;  

      if (data == null)
        return DFCARD_LIB_CALL_ERROR;
      
      if (crc == null)
      {
        crc = new byte[4];
        crc[0] = data[length];
        crc[1] = data[length+1];
        crc[2] = data[length+2];
        crc[3] = data[length+3];
      }
    
      if (move_status)
      {
        UInt32 i;
    
        status = data[0];
        for (i=0; i<length-1; i++)
          data[i] = data[i+1];
        data[length-1] = status;
      }
    
      ComputeCrc32(data, length, ref check);
    
      if (move_status)
      {
        UInt32 i;
    
        status = data[length-1];
        for (i=length-1; i>=1; i--)
          data[i] = data[i-1];
        data[0] = status;
      }
    
      for (int i=0; i<4; i++)
        if (check[i] != crc[i])
          return DFCARD_WRONG_CRC;
    
      return DF_OPERATION_OK;
    }
    

  }
}
