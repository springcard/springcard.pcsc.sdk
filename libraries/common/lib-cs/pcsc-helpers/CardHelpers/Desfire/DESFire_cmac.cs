/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 14:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_core.
  /// </summary>
  public partial class Desfire
  {
    int XferCmacSend(bool append)
    {
      byte[] cmac = new byte[8];
      
      if ( (session_type & KEY_ISO_MODE) != KEY_ISO_MODE)
        return DF_OPERATION_OK;
      
      if (xfer_buffer[0] == DF_ADDITIONAL_FRAME)
      {
        if (xfer_length == 1)
        {
          return DF_OPERATION_OK;
        } else
        {
          return DFCARD_UNEXPECTED_CHAINING;
        }
      }
      
      ComputeCmac(xfer_buffer, xfer_length, false, ref cmac);
      
      if (append)
      {
        Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int) xfer_length, 8);
        xfer_length += 8;
      }
      
      return DF_OPERATION_OK;
    
    }
    
    
    int XferCmacRecv()
    {
      if (xfer_buffer[0] == DF_ADDITIONAL_FRAME)
        return DFCARD_UNEXPECTED_CHAINING;

      return VerifyCmacRecv(xfer_buffer, ref xfer_length);
    }
    
    
    void InitCmac()
    {
      byte bMSB;
      byte block_size, rb_xor_value;
      UInt32 t, i;

      rb_xor_value = (byte) ((session_type == KEY_ISO_AES) ? 0x87 : 0x1B);
      block_size   = (byte) ((session_type == KEY_ISO_AES) ?   16 :    8);
    
      cmac_subkey_1 = new byte[block_size];
      cmac_subkey_2 = new byte[block_size];
      
      byte[] abSavedInitVktr = new byte[init_vector.Length];
      
      // Save the InitVector:
      Array.ConstrainedCopy(init_vector, 0, abSavedInitVktr, 0, init_vector.Length);
        
      // Generate the padding bytes for O-MAC by enciphering a zero block
      // with the actual session key:
      for (i=0; i< cmac_subkey_1.Length; i++)
        cmac_subkey_1[i] = 0;

    
      t = block_size;
      /*
      {
        Console.Write("Before CipherSend, cmac_subkey_1=");
        for (int k=0; k<t; k++)
          Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
        Console.Write("\n");
      }
      */
      CipherSend(ref cmac_subkey_1, ref t, t);
      /*
      {
        Console.Write("After CipherSend, cmac_subkey_1=");
        for (int k=0; k<t; k++)
          Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
        Console.Write("\n");
      }
      */
     
      // If the MSBit of the generated cipher == 1 -> K1 = (cipher << 1) ^ Rb ...
      // store MSB:
      bMSB = cmac_subkey_1[0];
    
      // Shift the complete cipher for 1 bit ==> K1:
      byte tmp;
      for (i=0; i<(UInt32) (block_size-1); i++)
      {
        tmp = (byte) ((cmac_subkey_1[i] << 1) & 0x00FE);
        cmac_subkey_1[i] = tmp;
        
        // add the carry over bit:
        cmac_subkey_1[i] |= (byte) ( ((cmac_subkey_1[i+1] & 0x80) != 0) ? 0x01:0x00);
      }
      
      tmp = (byte) ((cmac_subkey_1[block_size-1] << 1) & 0x00FE);
      cmac_subkey_1[block_size-1] = tmp;
      if ((bMSB & 0x80) != 0)
      {
        // XOR with Rb:
        cmac_subkey_1[block_size-1] ^= rb_xor_value;
      }
      
      /*
      {
        Console.Write("After shift, cmac_subkey_1=");
        for (int k=0; k<t; k++)
          Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
        Console.Write("\n");
      }
      */
     
      // store MSB:
      bMSB = cmac_subkey_1[0];
    
      // Shift K1 ==> K2:
      for (i=0; i<(UInt32) (block_size-1); i++)
      {
        cmac_subkey_2[i]  = (byte) ((cmac_subkey_1[i] << 1) & 0x00FE);
        cmac_subkey_2[i] |= (byte) ( ((cmac_subkey_1[i+1] & 0x80) != 0) ? 0x01:0x00);
      }
      cmac_subkey_2[block_size-1] = (byte) ((cmac_subkey_1[block_size-1] << 1) & 0x00FE);
    
      if ((bMSB & 0x80) == 0x80)
      {
        // XOR with Rb:
        cmac_subkey_2[block_size - 1] ^= rb_xor_value;
      }
    
      /*
      {
        Console.Write("After shift, cmac_subkey_2=");
        for (int k=0; k<t; k++)
          Console.Write(String.Format("{0:x02}", cmac_subkey_2[k]));
        Console.Write("\n");
      }
      */
     
      // We have to restore the InitVector:
      Array.ConstrainedCopy(abSavedInitVktr, 0, init_vector, 0, init_vector.Length);
    
    }
    
    void ComputeCmac(byte[] data, UInt32 length, bool move_status, ref byte[] cmac)
    {
      UInt32 i, actual_length, block_size;
    
      /*
      {
        Console.Write("Data to CMAC over: ");
        for (int k=0; k<length; k++)
          Console.Write(String.Format("{0:x02}", data[k]));
        Console.Write("\n");
      }
      */
     
      if ((session_type & KEY_ISO_MODE) != KEY_ISO_MODE)
        Console.WriteLine("INVALID FUNCTION CALL 'Compute CMAC'\n");

      // Adapt the crypto mode if the sessionkey is done in CBC_Send_Decrypt:
      // enCryptoMethod = (m_SessionKeyCryptoMethod == CRM_3DES_DF4 ? CRM_3DES_ISO:m_SessionKeyCryptoMethod);
    
      block_size = (uint) ((session_type == KEY_ISO_AES) ? 16 : 8);
    
      // First we enlarge eNumOfBytes to a multiple of the cipher block size for allocating
      // memory of the intermediate buffer. Zero padding will be done by the DF8Encrypt function.
      // If we are ISO-authenticated, we have to do the spetial padding for the O-MAC:
      actual_length = length;
      while ((actual_length % block_size) != 0)
        actual_length++;
    
      byte[] buffer = new byte[actual_length];
      for (i=0; i<actual_length; i++)
        buffer[i] = 0;

    
      if (move_status)
      {
        Array.ConstrainedCopy(data, 1, buffer, 0, (int) (length-1));
        buffer[length-1] = data[0];
      } else
      {
        Array.ConstrainedCopy(data, 0, buffer, 0, (int) (length));
      }
    
      /*
      {
        Console.Write("Before padding, buffer: ");
        for (int k=0; k<actual_length; k++)
          Console.Write(String.Format("{0:x02}", buffer[k]));
        Console.Write("\n");
      }
      */
     
      /* Do the ISO padding and/or XORing */
    
      if ((length % block_size) != 0)
      {

        /* Block incomplete -> padding */
        buffer[length++] = 0x80;

        /*
        {
          Console.Write("after padding, buffer: ");
          for (int k=0; k<actual_length; k++)
            Console.Write(String.Format("{0:x02}", buffer[k]));
          Console.Write("\n");
        }
        */
       
        /* XOR the last eight bytes with CMAC_SubKey2 */
        length = actual_length - block_size;
        for (i=0; i<block_size; i++)
        {
          buffer[length + i] ^= (byte) (cmac_subkey_2[i]);
        }
      } else
      {

        /* Block complete -> no padding */
    
        /* XOR the last eight bytes with CMAC_SubKey1 */
        length = actual_length - block_size;
        for (i=0; i<block_size; i++)
        {
          buffer[length + i] ^= (byte) (cmac_subkey_1[i]);
        }
      }
      /*
      {
        Console.Write("After padding, buffer: ");
        for (int k=0; k<actual_length; k++)
          Console.Write(String.Format("{0:x02}", buffer[k]));
        Console.Write("\n");
      }
      */
      
      CipherSend(ref buffer, ref actual_length, actual_length);
    
      // Save the current init vector, which is the last cipher block of the cryptogram:
      Array.ConstrainedCopy(buffer, (int) (actual_length-block_size), init_vector, 0, (int) block_size);
      
      if (cmac != null)
      {
        // The mac is the first half of the init vector:
        Array.ConstrainedCopy(init_vector, 0, cmac, 0, 8);
      }

    }
    
    int VerifyCmacRecv(byte[] buffer, ref UInt32 length)
    {
      UInt32 l;
      byte[] cmac = new byte[8];
    
      if ( (session_type & KEY_ISO_MODE) != KEY_ISO_MODE )
        return DF_OPERATION_OK;
    
      l = length;
    
      if (l < 9)
        return DFCARD_WRONG_LENGTH;
    
      l -= 8;
      /*
      {
        Console.Write("Verify - CMAC calculated on :");
        for (int i=0; i<l; i++)
          Console.Write(String.Format("{0:x02}", buffer[i]));
        Console.Write("\n");
      }
      */
     
      ComputeCmac(buffer, l, true, ref cmac);
      
      /*
      {
        Console.Write("CMAC calculated :");
        for (int i=0; i<8; i++)
          Console.Write(String.Format("{0:x02}", cmac[i]));
        Console.Write("\n");
      }    
      */
      
      for (int i=0; i<8; i++)
        if (buffer[l+i] != cmac[i])
          return DFCARD_WRONG_MAC;

      length = l;
      return DF_OPERATION_OK;

    
    }

  }
}
