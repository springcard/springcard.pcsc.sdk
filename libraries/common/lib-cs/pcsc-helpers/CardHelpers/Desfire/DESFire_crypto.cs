/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 12/09/2017
 * Time: 10:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Security.Cryptography;
using System.Reflection;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of Crypto.
  /// </summary>
  internal static class DesfireCrypto
  {
    public const int TDES_CRC16  = 0;
    public const int TDES_CRC32  =  1;
    public const int AES          = 2;
    public const int MIFARE      = 3;
    
    public static byte[] TripleDES_Decrypt(byte[] CypherText, byte[] key, byte[] iv)
    {
      byte[] result = new byte[CypherText.Length];  
      TripleDES tripleDESalg = TripleDES.Create();
      TripleDESCryptoServiceProvider tDes = tripleDESalg as TripleDESCryptoServiceProvider;
      MethodInfo mi =  tDes.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
      tDes.Mode = CipherMode.CBC;
      tDes.Padding = PaddingMode.Zeros;
      object[] Par = { key, tDes.Mode, iv, tDes.FeedbackSize, 64 }; //64
      ICryptoTransform trans = mi.Invoke(tDes, Par) as ICryptoTransform;
      trans.TransformBlock(CypherText, 0, CypherText.Length, result, 0);
      
      return result;
    }
    
    public static byte[] TripleDES_Encrypt(byte[] Plaintext, byte[] key, byte[] iv)
    {
      byte[] result = new byte[Plaintext.Length];

      TripleDES tripleDESalg = TripleDES.Create();
      TripleDESCryptoServiceProvider tDes = tripleDESalg as TripleDESCryptoServiceProvider;
      MethodInfo mi =  tDes.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
      tDes.Mode = CipherMode.CBC;
      object[] Par = { key, tDes.Mode, iv, tDes.FeedbackSize, 0 };
      ICryptoTransform trans = mi.Invoke(tDes, Par) as ICryptoTransform;
      trans.TransformBlock(Plaintext, 0, Plaintext.Length, result, 0);
      
      return result;
    }
    
    public static byte[] AES_Decrypt(byte[] CypherText, byte[] key, byte[] iv)
    {
      AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
      aesCSP.BlockSize = 128;
      aesCSP.Key = key;
      aesCSP.IV = iv;
      aesCSP.Padding = PaddingMode.Zeros;
      aesCSP.Mode = CipherMode.CBC;
      
      ICryptoTransform xfrm = aesCSP.CreateDecryptor(key, iv);
      byte[] result = xfrm.TransformFinalBlock(CypherText, 0, CypherText.Length);
      
      return result;

    }
    
    public static byte[] AES_Encrypt(byte[] PlainText, byte[] key, byte[] iv)
    {
      AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
      aesCSP.BlockSize = 128;
      aesCSP.Key = key;
      aesCSP.IV = iv;
      aesCSP.Padding = PaddingMode.Zeros;
      aesCSP.Mode = CipherMode.CBC;
      
      ICryptoTransform xfrm = aesCSP.CreateEncryptor(key, iv);
      byte[] result = xfrm.TransformFinalBlock(PlainText, 0, PlainText.Length);
      
      return result;
    }
    
    public static byte[] rotate_left(byte[] entry)
    {
      byte[] result = new byte[entry.Length];
      for (int i = 0; i < (entry.Length) - 1; i++)
        result[i] = entry[i+1];
      result[entry.Length - 1] = entry[0];
      return result;
    }
      
    public static byte[] rotate_right(byte[] entry)
    {
      byte[] result = new byte[entry.Length];
      for (int i = 0; i < (entry.Length) - 1; i++)
        result[i+1] = entry[i];
      result[0] = entry[entry.Length - 1];
      return result;
    }
      
    public static byte[] ComputeCrc16(byte[] buffer)
    {
      byte chBlock;
      ushort wCrc = 0x6363; /* ITU-V.41 */
      byte[] crc = new byte[2];
     
      for (int i = 0 ; i< buffer.Length; i++)
      {
        chBlock = (byte) (buffer[i] ^ (byte) ((wCrc) & 0x00FF));
        chBlock = (byte) (chBlock ^ ((chBlock << 4)) & 0x00FF);
        wCrc = (ushort) ((wCrc >> 8) ^ ((ushort) chBlock << 8) ^ ((ushort) chBlock << 3) ^ ((ushort) chBlock >> 4));
      }
      crc[0] = (byte) (wCrc & 0x00FF);
      crc[1] = (byte) (wCrc >> 8);
      return crc;
    }
    
    public static byte[] ComputeCrc32(byte[] buffer)
    {
      UInt32 dwCrc = 0xFFFFFFFF;
      byte[] crc = new byte[4];
     
      for (int i = 0; i < buffer.Length ; i++)
      {
        dwCrc ^= buffer[i];
        for (byte b=0; b<8; b++)
        {
          if ((dwCrc & 0x00000001) == 0x00000001)
          {
            dwCrc >>= 1;
            dwCrc  ^= 0xEDB88320;
          } else
          {
            dwCrc >>= 1;
          }
        }        
      }

      crc[0] = (byte) (dwCrc & 0x000000FF);
      crc[1] = (byte) ((dwCrc >> 8) & 0x000000FF);
      crc[2] = (byte) ((dwCrc >> 16) & 0x000000FF);
      crc[3] = (byte) ((dwCrc >> 24) & 0x000000FF);
      return crc;
    }        
  
    public static byte[] CalculateCMAC(byte[] Key, byte[] IV, byte[] input)
    {
      // First : calculate subkey1 and subkey2
      byte[] Zeros = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
      
      //byte[] K = { 0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c } ;

      byte[] L = AES_Encrypt(Zeros, Key, IV);
      
      /*
      Console.WriteLine("CIPHk(0128)=");
      for (int k = 0; k< L.Length; k++)
        Console.Write("-" + String.Format("{0:x02}", L[k]));
      Console.Write("\n");
      */
      
      byte[] Key1;
      byte[] Key2;
      int i = 0;
      byte Rb = 0x87;
      byte MSB_L = L[0];
      UInt32 decal;
        
      // calcul de Key 1
      for (i = 0; i < L.Length - 1 ; i++)
      {
        decal = (UInt32) (L[i] << 1);
        L[i] = (byte) (decal & 0x00FF);
        if ( (L[i+1] & 0x80) == 0x80 )
        {
          L[i] |= 0x01;
        } else
        {
          L[i] |= 0x00;
        }
      }

      decal = (UInt32) (L[i] << 1);
      L[i] = (byte) (decal & 0x00FF);
      
      if ( MSB_L >= 0x80 )
        L[L.Length - 1] ^= Rb;
      
      Key1 = L;
      
      /*
      Console.Write("Key1=");
      for (int k = 0; k< L.Length; k++)
        Console.Write("-" + String.Format("{0:x02}", Key1[k]));
      Console.Write("\n");
      */
     
      byte[] tmp = new byte[Key1.Length];
      for (int k = 0; k<Key1.Length; k++)
        tmp[k] = Key1[k];
      
      // Calcul de key 2
      byte MSB_K1 = Key1[0];
      for (i=0 ; i<L.Length-1 ; i++)
      {
        decal = (UInt32) (tmp[i] << 1);
        tmp[i] = (byte) (decal & 0x00FF);
        if ( (tmp[i+1] & 0x80) == 0x80 )
        {
          tmp[i] |= 0x01;
        } else
        {
          tmp[i] |= 0x00;
        }        
      }
      decal = (UInt32) (tmp[i] << 1);
      tmp[i] = (byte) (decal & 0x00FF);
      if (MSB_K1 >= 0x80)
        tmp[tmp.Length - 1] ^= Rb;
    
      Key2 = tmp;
      
      /*
      Console.Write("Key2=");
      for (int k = 0; k< L.Length; k++)
        Console.Write("-" + String.Format("{0:x02}", Key2[k]));
      Console.Write("\n");    
      */
        
      byte[] result;
      
      /*-------------------------------------------------*/
      /* Cas 1 : la chaine est vide    */
      /* a- On concatene avec 0x80000000..00  (data) */
      /* b- on X-or avec Key2  (M1)*/
      /* c- on encrypte en AES-128 avec K et IV */
      /**/
      if (input == null)
      {
        byte[] data = { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] M1 = new byte[16];
        for (int k = 0; k< 16; k++)
          M1[k] = (byte) (data[k] ^ Key2[k]); // input      
      
        result = AES_Encrypt(M1, Key, IV);
      } else
      
      /**/
      
      /*--------------------------------------------------*/
      /* Cas 2 ! la chaine n'est pas vide et contient 16 octets  */
      /* a- on X-or avec Key 1 (data)  */
      /* b- on encrypte en AES-128 avec K et IV  */
      // byte[] data = { 0x6b, 0xc1, 0xbe, 0xe2, 0x2e, 0x40, 0x9f, 0x96, 0xe9, 0x3d, 0x7e, 0x11, 0x73, 0x93, 0x17, 0x2a };      
      
      
      if (input.Length == 16)
      {
        byte[] M1 = new byte[input.Length];
        for (int k = 0; k< input.Length; k++)
          M1[k] = (byte) (input[k] ^ Key1[k]); 
        
        result = AES_Encrypt(M1, Key, IV);
      } else
      {
        byte[] M = new byte[input.Length+16];
        int offset = 0;
        for (i = 0; i<input.Length; i+=16)
        {
          if ((i+16) < input.Length )
          {
            /* block entier - on ne padde pas */
            for (int j=0; j<16; j++)
              M[offset++] = (byte) (input[i+j]);// ^ Key1[j]);
            
          } else
          if ((i+16) == input.Length )
          {
            /* block entier, on doit padder avec Key 1 */
            for (int j=0; j<16; j++)
              M[offset++] = (byte) (input[i+j] ^ Key1[j]);           
            
          } else
          {
            /* block terminal */
            byte remaining = (byte) (input.Length - i);
            byte NbPadd = (byte) (16 - remaining);
            
            
            for (int j=0; j<remaining; j++)
              M[offset++] = (byte) (input[i+j] ^ Key2[j]);
            
            byte key2_index_when_input_ends = (byte) (input.Length % 16);
            M[offset++] = (byte) (0x80 ^ Key2[key2_index_when_input_ends]);
            NbPadd --;
            key2_index_when_input_ends++;
            for (int j = 1; j <= NbPadd ; j++)
              M[offset++] = Key2[remaining + j];
            
          }
          
        }
        
        byte[] Message = new byte[offset];
        Array.ConstrainedCopy(M, 0, Message, 0, offset);
       
        result = AES_Encrypt(Message, Key, IV);
        
        /*
        string s = "K1 =";
        for (int k=0; k<Key1.Length; k++)
          s += String.Format("{0:x02}", Key1[k]);
        s += "\r\nK2 =";
        for (int k=0; k<Key2.Length; k++)
          s += String.Format("{0:x02}", Key2[k]);    
        s += "\r\nMessage =";
        for (int k=0; k<Message.Length; k++)
          s += String.Format("{0:x02}", Message[k]);         
        s += "\r\nresult =";
        for (int k=0; k<result.Length; k++)
          s += String.Format("{0:x02}", result[k]);       
        System.Windows.Forms.MessageBox.Show(s);
          */
        
      }
      
      
      /*
      byte[] M1 = new byte[input.Length + 1];
      int offset = 0;
      for (int k = 0; k< input.Length; k++)
        M1[offset++] = input[k];
      
      M1[offset] = 0x80;
      
      
      Console.Write("M1=");
      for (int k = 0; k< M1.Length; k++)
        Console.Write("-" + String.Format("{0:x02}", M1[k]));
      Console.Write("\n");        
  
      
      byte[] M2 = new byte[M1.Length];
      
      for (int k = 0; k< M1.Length; k++)
        M2[k] = (byte) (M1[k] ^ Key2[k]);
      
      Console.Write("M2=");
      for (int k = 0; k < M2.Length; k++)
        Console.Write("-" + String.Format("{0:x02}", M2[k]));
      Console.Write("\n");  
      
      byte[] result = AES_Encrypt(M2, Key, IV);
      */
      
      /*
      Console.Write("CMAC=");
      for (int k = 0; k< L.Length; k++)
        Console.Write("-" + String.Format("{0:x02}", result[k]));
      Console.Write("\n");  
      */
     
     
     
       /*
      string s = "K1 =";
      for (int k=0; k<Key1.Length; k++)
        s += String.Format("{0:x02}", Key1[k]);
      s += "\r\nK2 =";
      for (int k=0; k<Key2.Length; k++)
        s += String.Format("{0:x02}", Key2[k]);    
      s += "\r\nresult =";
      for (int k=0; k<result.Length; k++)
        s += String.Format("{0:x02}", result[k]);         
      s += "\r\nresult =";
      for (int k=0; k<result.Length; k++)
        s += String.Format("{0:x02}", result[k]);       
      System.Windows.Forms.MessageBox.Show(s);
       */
       
      return result;
      
    }   
  }
}
