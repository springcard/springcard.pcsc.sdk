/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 15:46
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

    void SetKey(byte[] pbAccessKey)
    {
      session_key = pbAccessKey;
    }
    
    void CipherRecv(ref byte[] data, ref UInt32 length)
    {
      byte[] buffer;
      byte[] dummy_iv;
      UInt16  block_count;
      UInt32 i, j;
    
      if (data == null)
        return;

      switch (session_type)
      {
        case KEY_LEGACY_DES :

          buffer = new byte[8];
          for (i=0; i<8; i++) //B  <- 00...00  
               buffer[i] = 0;
               
          dummy_iv = new byte[8];
          for (int k=0; k<8; k++)
            dummy_iv[k] = 0;
          
           block_count = (UInt16) (length / 8);
          for (i = 0; i < block_count; i++)
          {

            Array.ConstrainedCopy(buffer, 0, init_vector, 0, 8);      // IV <- B         
            Array.ConstrainedCopy(data, (int) (8*i), buffer, 0, 8);   // B  <- P        
            
            byte[] tmp  = DesfireCrypto.TripleDES_Decrypt(buffer, session_key, dummy_iv);
            Array.ConstrainedCopy(tmp, 0, data, (int) (8*i), 8);// P  <- iDES(B)   

            for (j = 0; j < 8; j++)
              data[8 * i + j] ^= (byte) (init_vector[j]);  // P  <- P XOR IV 
          }

        break;
    
        case KEY_LEGACY_3DES :
          buffer = new byte[8];
          for (i=0; i<8; i++) //B  <- 00...00  
               buffer[i] = 0;
               
          dummy_iv = new byte[8];
          for (int k=0; k<8; k++)
            dummy_iv[k] = 0;
          
           block_count = (UInt16) (length / 8);
          for (i = 0; i < block_count; i++)
          {

            Array.ConstrainedCopy(buffer, 0, init_vector, 0, 8);      // IV <- B         
            Array.ConstrainedCopy(data, (int) (8*i), buffer, 0, 8);   // B  <- P        
            
            byte[] tmp  = DesfireCrypto.TripleDES_Decrypt(buffer, session_key, dummy_iv);
            Array.ConstrainedCopy(tmp, 0, data, (int) (8*i), 8);// P  <- iDES(B)   

            for (j = 0; j < 8; j++)
              data[8 * i + j] ^= (byte) (init_vector[j]);  // P  <- P XOR IV 
          }

        break;
    
        case KEY_ISO_DES    :
        case KEY_ISO_3DES2K :
        case KEY_ISO_3DES3K :
        
          buffer = new byte[8];
          block_count = (UInt16) (length / 8);
          dummy_iv = new byte[8];
          for (int k=0; k<8; k++)
            dummy_iv[k] = 0;

          for (i = 0; i < block_count; i++)
          {           
            //1.
            Array.ConstrainedCopy(data, (int) (8*i), buffer, 0, 8);   // B  <- P
            
            
            //2.
            byte[] tmp = DesfireCrypto.TripleDES_Decrypt(buffer, session_key, dummy_iv);
            Array.ConstrainedCopy(tmp, 0, data, (int) (8*i), 8);
            
            //3. 
            for (j = 0; j < 8; j++)
              data[8 * i + j] ^= (byte) (init_vector[j]);  // P  <- P XOR IV           
            
            //4. 
            Array.ConstrainedCopy(buffer, 0, init_vector, 0, 8);// IV <- B  

          }
          
          break;
    
          
          
        case KEY_ISO_AES :
          
          buffer = new byte[16];
          block_count = (UInt16) (length / 16);
          dummy_iv = new byte[16];
          for (int k=0; k<16; k++)
            dummy_iv[k] = 0;

          for (i = 0; i < block_count; i++)
          {           
            //1.
            Array.ConstrainedCopy(data, (int) (16*i), buffer, 0, 16);   // B  <- P
            
            //2.
            byte[] tmp = DesfireCrypto.AES_Decrypt(buffer, session_key, dummy_iv);
            Array.ConstrainedCopy(tmp, 0, data, (int) (16*i), 16);
            
            //3. 
            for (j = 0; j < 16; j++)
              data[16 * i + j] ^= (byte) (init_vector[j]);  // P  <- P XOR IV           
            
            //4. 
            Array.ConstrainedCopy(buffer, 0, init_vector, 0, 16);// IV <- B  

          }
          
          break;
      }

    }
    
    void CipherSend(ref byte[] data, ref UInt32 length, UInt32 max_length)
    {
      UInt32 actual_length;
      UInt32 block_size;
      UInt32 block_count;
      UInt32 i, j;

      if (data == null)
        return;

    
      actual_length = length;
    
      /* Step 1 : padding */
      block_size = (uint) ((session_type == KEY_ISO_AES) ? 16 : 8);
      while ((actual_length % block_size) != 0)
      {
        if (actual_length >= max_length)
          return;
        data[actual_length++] = 0x00;
      }
    
      block_count = (actual_length / block_size);
    
      byte[] dummy_iv;
      
      switch (session_type)
      {
        case KEY_LEGACY_DES :
          dummy_iv = new byte[8];
          for (int k=0; k<8; k++)
              dummy_iv[k] = 0;
          
          /* IV <- 0 */
          for (i=0; i< init_vector.Length; i++)
            init_vector[i] = 0;

          for (i = 0; i < block_count; i++)
          {

            for (j = 0; j < 8; j++)
              data[8 * i + j] ^= (byte) (init_vector[j]);  /* P  <- P XOR IV  */

            /* Legacy mode : PICC always encrypts, PCD always decrypts */
            byte[] tmp = new byte[block_size];
            Array.ConstrainedCopy(data, (int) (8*i), tmp, 0, (int) block_size);
            init_vector = DesfireCrypto.TripleDES_Decrypt(tmp, session_key, dummy_iv);
          
            Array.ConstrainedCopy(init_vector, 0, data, (int) (8*i), 8);// P  <- IV
          }
          
          break;
    
        case KEY_LEGACY_3DES :
          dummy_iv = new byte[8];
          for (int k=0; k<8; k++)
              dummy_iv[k] = 0;
          
          /* IV <- 0 */
          for (i=0; i< init_vector.Length; i++)
            init_vector[i] = 0;

          for (i = 0; i < block_count; i++)
          {

            for (j = 0; j < 8; j++)
              data[8 * i + j] ^= (byte) (init_vector[j]);  /* P  <- P XOR IV  */

            /* Legacy mode : PICC always encrypts, PCD always decrypts */
            byte[] tmp = new byte[block_size];
            Array.ConstrainedCopy(data, (int) (8*i), tmp, 0, (int) block_size);
            init_vector = DesfireCrypto.TripleDES_Decrypt(tmp, session_key, dummy_iv);
          
            Array.ConstrainedCopy(init_vector, 0, data, (int) (8*i), 8);// P  <- IV
          }
          
          break;
    
        case KEY_ISO_DES    :
        case KEY_ISO_3DES2K :
        case KEY_ISO_3DES3K :

          dummy_iv = new byte[8];
          for (int k=0; k<8; k++)
              dummy_iv[k] = 0;
          /*
          {
            Console.Write("session_key: ");
            for (int k=0; k<session_key.Length; k++)
              Console.Write(String.Format("{0:x02}", session_key[k]));
            Console.Write("\n");
          }
          */
          for (i = 0; i < block_count; i++)
          {
            /* Keep last IV */
            
            //1.
            for (j = 0; j < 8; j++)
              data[8 * i + j] ^= (byte) (init_vector[j]);  // P  <- P XOR IV
            
            //2.
            byte[] tmp = new byte[block_size];
            Array.ConstrainedCopy(data, (int) (8*i), tmp, 0, (int) block_size);
            init_vector = DesfireCrypto.TripleDES_Encrypt(tmp, session_key, dummy_iv);
            
            //3.
            Array.ConstrainedCopy(init_vector, 0, data, (int) (8*i), 8);// P  <- IV
  
          }

          break;
    
        case KEY_ISO_AES :
          dummy_iv = new byte[16];
          for (int k=0; k<8; k++)
              dummy_iv[k] = 0;
          
          
          /* Keep last IV */
          for (i = 0; i < block_count; i++)
          {
            for (j = 0; j < 16; j++)
              data[16 * i + j] ^= (byte) (init_vector[j]);  /* P  <- P XOR IV  */
            
            //2.
            byte[] tmp = new byte[block_size];
            Array.ConstrainedCopy(data, (int) (16*i), tmp, 0, (int) block_size);
            init_vector = DesfireCrypto.AES_Encrypt(tmp, session_key, dummy_iv);

            //3.
            Array.ConstrainedCopy(init_vector, 0, data, (int) (16*i), 16);// P  <- IV
          }
          
          break;
      }
    
      length = actual_length;

    }
    
    void CipherSend_AES(byte[] data, ref UInt32 length, UInt32 max_length, byte[] Key, byte[] IV)
    {
    
      UInt32 actual_length;
      UInt32 block_size;
      UInt32 block_count;
      UInt32 i, j;
    
      if (data == null)
        return;
    
      if (( Key == null) || (IV == null) )
          return;
      
      if ((Key.Length != 16) || (IV.Length != 16))
        return;
    
      actual_length = length;
    
      /* Padding  */
      block_size = 16 ;
      while ((actual_length % block_size) != 0)
      {
        if (actual_length >= max_length)
          return;
        
        data[actual_length++] = 0x00;
      }
    
      block_count = (actual_length / block_size);
    
      for (i = 0; i < block_count; i++)
      {
        for (j = 0; j < 16; j++)
          data[16 * i + j] ^= (byte) IV[j];  /* P  <- P XOR IV */
    
        /* ISO mode : sending means encrypting  */
        Array.ConstrainedCopy(IV, 0, data, (int) (16*i), 16);/* P  <- IV      */
      }
    
      length = actual_length;
    
    }
    
    
    void XferCipherSend(UInt32 start_offset)
    {
      UInt32 block_size;
      UInt32 actual_length;

      if (start_offset > xfer_length)
        return;
    
      block_size = (uint) ((session_type == KEY_ISO_AES) ? 16 : 8);
    
      /* Padd until we are aligned on block boundary */
      actual_length = xfer_length - start_offset;
      while ((actual_length % block_size) != 0)
      {
        if (xfer_length >= xfer_buffer.Length)
          return;
    
        xfer_buffer[xfer_length++] = 0x00;
        actual_length++;
      }
    
      /* Cipher the data */
      byte[] tmp = new byte[actual_length];
      Array.ConstrainedCopy(xfer_buffer, (int) start_offset, tmp, 0, (int) actual_length);
      CipherSend(ref tmp, ref actual_length, actual_length);
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, (int) start_offset, (int) actual_length);
      
      
    }
    
    
    
    
    /**f* DesfireAPI/DES_SetKey
     *
     * NAME
     *   DES_SetKey
     *
     * DESCRIPTION
     *   Init a DES context with a 64-bit key
     *   Parity bits are not checked
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_DES_SetKey(const BYTE key[8]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   Not applicable.
     *
     *   [[pcsc_desfire.dll]]
     *   Not applicable.
     *
     * RETURNS
     *   DF_OPERATION_OK   : success, data has been written
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   Desfire_DES_Encrypt
     *   Desfire_DES_Decrypt
     *
     **/
    int DES_SetKey(byte[] key)
    {

    
      if (key == null) 
        return DF_PARAMETER_ERROR;
    
      if (key.Length != 8)
        return DF_PARAMETER_ERROR;
    
      /* Success. */
      return DF_OPERATION_OK;
    
    }

        
  }
}
