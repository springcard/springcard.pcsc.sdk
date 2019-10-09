/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 14/09/2017
 * Time: 11:42
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_mac.
  /// </summary>
  public partial class Desfire
  {
    /**h* DesfireAPI/MAC
     *
     * NAME
     *   DesfireAPI :: MAC functions
     *
     * COPYRIGHT
     *   (c) 2009 SpringCard - www.springcard.com
     *
     * DESCRIPTION
     *   High-level implementation of the DESFIRE EV0 MAC scheme.
     *
     **/

    void ComputeMacBlocks(byte[] data, UInt32 block_count, byte[] result)
    {
      UInt32 i, j;
      byte[] carry = new byte[16];
      byte[] buffer = new byte[16];
    
      switch (session_type)
      {
        case KEY_LEGACY_DES :
          {    
            byte[] dummy_iv = new byte[8];
            for (int k=0; k<8; k++)
              dummy_iv[k] = 0;
            
            for (int k=0; k<8; k++)/* IV <- 00...00   */
              carry[k] = 0;
           
            for (i = 0; i < block_count; i++)
            {
              Array.ConstrainedCopy(data, (int) (8*i), buffer, 0, 8);
              for (j = 0; j < 8; j++)
                buffer[j] ^= carry[j];    /* P  <- P XOR IV  */
              carry = DesfireCrypto.TripleDES_Encrypt(buffer, session_key, dummy_iv);  /* IV <- DES(P)    */
            }

            Array.ConstrainedCopy(carry, 0, result, 0, 8);
          }
          break;
        case KEY_LEGACY_3DES :
          {
            byte[] dummy_iv = new byte[8];
            for (int k=0; k<8; k++)
              dummy_iv[k] = 0;
            
            for (int k=0; k<8; k++)/* IV <- 00...00   */
              carry[k] = 0;
           
            for (i = 0; i < block_count; i++)
            {
              Array.ConstrainedCopy(data, (int) (8*i), buffer, 0, 8);
              for (j = 0; j < 8; j++)
                buffer[j] ^= carry[j];    /* P  <- P XOR IV  */
              carry = DesfireCrypto.TripleDES_Encrypt(buffer, session_key, dummy_iv);  /* IV <- DES(P)    */
            }

            Array.ConstrainedCopy(carry, 0, result, 0, 8);
          }
          break;
      
        default :
          Console.WriteLine("INVALID FUNCTION CALL\n");
          break;
      }
    
    }
    
    void ComputeMac(byte[] data, UInt32 length, ref byte[] mac)
    {
      byte[] result = new byte[16];
      UInt32 block_size;
      
      block_size = (uint) ((session_type == KEY_ISO_AES) ? 16 : 8);
    
      if ((length % block_size) != 0)
      {
        UInt32 block_count;
        block_count = 1 + (length / block_size);

        byte[] buffer = new byte[block_size * block_count];        
        for (int k=0; k<block_size * block_count; k++)
          buffer[k] = 0;
        
        Array.ConstrainedCopy(data, 0, buffer, 0, (int) length);
        ComputeMacBlocks(buffer, block_count, result);
    
      } else
      {
       ComputeMacBlocks(data, length / block_size, result);
      }
    
      if (mac != null)
        Array.ConstrainedCopy(result, 0, mac, 0, 4);
    }
    
    public long VerifyMacRecv(byte[] recv_buffer, ref UInt32 recv_length)
    {
      byte[]  mac = new byte[4];
      UInt32 length;
       
      length = recv_length;
    
      if (length < 5)
        return DFCARD_WRONG_LENGTH;

      length -= 5;
  
      byte[] tmp = new byte[length];
      
    
      Array.ConstrainedCopy(recv_buffer, 1, tmp, 0, (int) length);
      
      
      ComputeMac(tmp, length, ref mac);
      bool are_equal = true;
      for (int k=0; k<4; k++)
        if (recv_buffer[1 + length + k] != mac[k])
          are_equal = false;
      
      if (!are_equal)
        return DFCARD_WRONG_MAC;
  
      /* Remove size of MAC */
      recv_length -= 4;
  
      return DF_OPERATION_OK;

    }

  }
}
