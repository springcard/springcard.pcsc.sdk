/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 10:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_auth.
  /// </summary>
  public partial class Desfire
  {
    void CleanupInitVector()
    {
      for (int i=0; i< init_vector.Length; i++)
        init_vector[i] = 0;
    }

    /**f* DesfireAPI/Authenticate
     *
     * NAME
     *   Authenticate
     *
     * DESCRIPTION
     *   Perform authentication using the specified DES or 3DES key on the currently
     *   selected DESFIRE application.
     *   This is the legacy function, available even on DESFIRE EV0.
     *   The generated session key is afterwards used for non-ISO ciphering or macing.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_Authenticate(BYTE bKeyNumber,
     *                                    const BYTE pbAccessKey[16]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_Authenticate(SPROX_INSTANCE rInst,
     *                                     BYTE bKeyNumber,
     *                                     const BYTE pbAccessKey[16]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_Authenticate(SCARDHANDLE hCard,
     *                                   BYTE bKeyNumber,
     *                                   const BYTE pbAccessKey[16]);
     *
     * INPUTS
     *   BYTE bKeyNumber             : number of the key (KeyNo)
     *   const BYTE pbAccessKey[16]  : 16-byte Access Key (DES/3DES2K keys)
     *
     * RETURNS
     *   DF_OPERATION_OK    : authentication succeed
     *   Other code if internal or communication error has occured. 
     *
     * NOTES
     *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
     *   * If the 2nd half of the key string is equal to the 1st half, the key is
     *   handled as a single DES key by the DESFIRE card.
     *   * If the 2nd half of the key string is NOT equal to the 1st half, the key
     *   is handled as a 3DES key.
     *
     * SEE ALSO
     *   AuthenticateIso24
     *   AuthenticateIso
     *   AuthenticateAes
     *   ChangeKeySettings
     *   GetKeySettings
     *   ChangeKey
     *   GetKeyVersion
     *
     **/
    public long Authenticate(byte bKeyNumber, byte[] pbAccessKey)
    {
      UInt32 t;
      long status;
      byte[] abRndB = new byte[8];
      byte[] abRndA = new byte[8];

      if (pbAccessKey == null)
        return DFCARD_LIB_CALL_ERROR;

      if (pbAccessKey.Length != 16  )
        return DFCARD_LIB_CALL_ERROR;
            
      /* Each new Authenticate must invalidate the current authentication status. */
      CleanupAuthentication();
    
      /* Check whether a TripleDES key was passed (both key halves are different). */
      bool areEqual = true;
      for (int i=0; i<8; i++)
        if (pbAccessKey[i] != pbAccessKey[8+i])
          areEqual = false;
      
      if (!areEqual)
      {
        /* If the two key halves are not identical, we are doing TripleDES. */
        /* We have to remember that TripleDES is in effect, because the manner of building
           the session key is different in this case. */
        SetKey(pbAccessKey);
        session_type = KEY_LEGACY_3DES;
      } else
      {
        SetKey(pbAccessKey);
        session_type = KEY_LEGACY_DES;
      }
    
      /* Create the command string consisting of the command byte and the parameter byte. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_AUTHENTICATE;
      xfer_buffer[xfer_length++] = bKeyNumber;
    
      /* Send the command string to the PICC and get its response (1st frame exchange).
         The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
      status = Command(0, WANTS_ADDITIONAL_FRAME);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* Check the number of bytes received, we expect 9 bytes. */
      if (xfer_length != 9)
      {
        /* Error: block with inappropriate number of bytes received from the card. */
        return DFCARD_WRONG_LENGTH;
      }

    
      /* OK, we received the 8 bytes Ek( RndB ) from the PICC.
         Decipher Ek( RndB ) to get RndB in ctx->xfer_buffer.
         Note that the status code has already been extracted from the queue. */
      t = 8;
      byte[] tmp = new byte[8];
      Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, 8);
      /*
      {
        Console.Write("To decipher: " );
        for (int k=0; k< tmp.Length; k++)
          Console.Write(String.Format("{0:x02}", tmp[k]));
        Console.Write("\n");
      }
      */
      
      CipherRecv(ref tmp, ref t);
      /*
      {
        Console.Write("Deciphered: " );
        for (int k=0; k< tmp.Length; k++)
          Console.Write(String.Format("{0:x02}", tmp[k]));
        Console.Write("\n");
      }
      */
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int) t);
      
      
      /* Store this RndB (is needed later on for generating the session key). */
      Array.ConstrainedCopy(xfer_buffer, 1, abRndB, 0, 8);
    
      /* Now the PCD has to generate RndA. */
      //GetRandomBytes(SPROX_PARAM_P  abRndA, 8);
      Random rand = new Random();
      abRndA = new byte[8];
      for (int i=0; i<abRndA.Length; i++)
        abRndA[i] = (byte) rand.Next(0x00, 0xFF);
    
      /* Start the second frame with a status byte indicating to the PICC that the Authenticate
         command is continued. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_ADDITIONAL_FRAME;
    
      /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
         after the status byte. */
      Array.ConstrainedCopy(abRndA, 0, xfer_buffer, (int) xfer_length, 8);
      xfer_length += 8;

      Array.ConstrainedCopy(abRndB, 1, xfer_buffer, (int) xfer_length, 7);
      xfer_length += 7;
      xfer_buffer[xfer_length++] = abRndB[0]; /* first byte move to last byte */

    
      /* Apply the DES send operation to the 16 argument bytes before sending the second frame
         to the PICC ( do not include the status byte in the DES operation ). */
      t = 16;
      tmp = new byte[t];
      Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int) t);
      CipherSend(ref tmp, ref t, t);
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int) t);
      
    
      /* Send the 2nd frame to the PICC and get its response. */
      status = Command(0, WANTS_OPERATION_OK);
      if (status != DF_OPERATION_OK)
        return status;

    
      /* We should now have Ek( RndA' ) in our buffer.
         RndA' was made from RndA by the PICC by rotating the string one byte to the left.
         Decipher Ek( RndA' ) to get RndA' in ctx->xfer_buffer. */
      t = 8;
      tmp = new byte[t];
      Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int) t);
      CipherRecv(ref tmp, ref t);
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int) t);
    
      /* Now we have RndA' in our buffer.
         We have to check whether it matches our local copy of RndA.
         If one of the subsequent comparisons fails, we do not trust the PICC and therefore
         abort the authentication procedure ( no session key is generated ). */
    
      /* First compare the bytes 1 to 7 of RndA with the first 7 bytes in the queue. */
      for (int i=0; i<7; i++)
        if (xfer_buffer[INF+1+i] != abRndA[1+i])
          return DFCARD_WRONG_KEY;
    
      /* Then compare the leftmost byte of RndA with the last byte in the queue. */
      if (xfer_buffer[INF + 8] != abRndA[0])
        return DFCARD_WRONG_KEY;

      /* The actual authentication has succeeded.
         Finally we have to generate the session key from both random numbers RndA and RndB.
         The first half of the session key is the concatenation of RndA[0-3] + RndB[0-3]. */
      /* If the original key passed through pbAccessKey is a TripleDES key, the session
         key must also be a TripleDES key. */
    
      if (session_type == KEY_LEGACY_DES)
      {
        /*
        memcpy(ctx->session_key +  0, abRndA + 0, 4);
        memcpy(ctx->session_key +  4, abRndB + 0, 4);
        memcpy(ctx->session_key +  8, ctx->session_key, 8);
        memcpy(ctx->session_key + 16, ctx->session_key, 8);
    
        
        Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, NULL, NULL);
        */
       
        session_key   = new byte[16];
        Array.ConstrainedCopy(abRndA, 0, session_key, 0, 4);
        Array.ConstrainedCopy(abRndB, 0, session_key, 4, 4);
        Array.ConstrainedCopy(session_key, 0, session_key, 8, 8);
        
        SetKey(session_key);
        
        
      } else
      if (session_type == KEY_LEGACY_3DES)
      {
        /* For TripleDES generate the second part of the session key.
           This is the concatenation of RndA[4-7] + RndB[4-7]. */
        /*
        memcpy(ctx->session_key +  0, abRndA + 0, 4);
        memcpy(ctx->session_key +  4, abRndB + 0, 4);
        memcpy(ctx->session_key +  8, abRndA + 4, 4);
        memcpy(ctx->session_key + 12, abRndB + 4, 4);
        memcpy(ctx->session_key + 16, ctx->session_key, 8);
    
        Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, ctx->session_key+8, NULL);
        */
       
        session_key   = new byte[16];
        Array.ConstrainedCopy(abRndA, 0, session_key, 0, 4);
        Array.ConstrainedCopy(abRndB, 0, session_key, 4, 4);
        Array.ConstrainedCopy(abRndA, 4, session_key, 8, 4);
        Array.ConstrainedCopy(abRndB, 4, session_key, 12, 4);

        SetKey(session_key);
        
        
      }

    
      /* Authenticate succeeded, therefore we remember the number of the key which was used
         to obtain the current authentication status. */
      session_key_id   = bKeyNumber;
    
      /* Reset the init vector */
      CleanupInitVector();
    
      /* Success. */
      return DF_OPERATION_OK;
    }

    
    /**f* DesfireAPI/AuthenticateIso
     *
     * NAME
     *   AuthenticateIso
     *
     * DESCRIPTION
     *   Perform authentication using the specified 3DES key on the currently
     *   selected DESFIRE application.
     *   The generated session key is afterwards used for ISO ciphering or CMACing.
     *   This function is not available on DESFIRE EV0 cards.
     *
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_AuthenticateIso(byte bKeyNumber,
     *                                       const byte pbAccessKey[16]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_AuthenticateIso(SPROX_INSTANCE rInst,
     *                                        byte bKeyNumber,
     *                                        const byte pbAccessKey[16]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_AuthenticateIso(SCARDHANDLE hCard,
     *                                      byte bKeyNumber,
     *                                      const byte pbAccessKey[16]);
     *
     * INPUTS
     *   byte bKeyNumber             : number of the key (KeyNo)
     *   const byte pbAccessKey[16]  : 16-byte Access Key (DES/3DES2K keys)
     *
     * RETURNS
     *   DF_OPERATION_OK    : authentication succeed
     *   Other code if internal or communication error has occured. 
     *
     * NOTES
     *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
     *   - If the 2nd half of the key string is equal to the 1st half, the 
     *   64-bit key is handled as a single DES key by the DESFIRE card
     *   (well, actually there are only 56 significant bits).
     *   - If the 2nd half of the key string is NOT equal to the 1st half, the
     *   key is a 128 bit 3DES key
     *   (well, actually there are only 112 significant bits).
     *
     * SEE ALSO
     *   Authenticate
     *   AuthenticateIso24
     *   AuthenticateAes
     *   ChangeKeySettings
     *   GetKeySettings
     *   ChangeKey
     *   GetKeyVersion
     *
     **/
    public long AuthenticateIso(byte bKeyNumber, byte[] pbAccessKey)
    {
      byte[] bKeyBuffer24 = new byte[24];
      if (pbAccessKey == null)
        return DFCARD_LIB_CALL_ERROR;
      
      if (pbAccessKey.Length != 16)
        return DFCARD_LIB_CALL_ERROR;
    
      Array.ConstrainedCopy(pbAccessKey, 0, bKeyBuffer24, 0, 16);
      Array.ConstrainedCopy(pbAccessKey, 0, bKeyBuffer24, 16, 8);
      
      return AuthenticateIso24(bKeyNumber, bKeyBuffer24);
    }
    
    /**f* DesfireAPI/AuthenticateIso24
     *
     * NAME
     *   AuthenticateIso24
     *
     * DESCRIPTION
     *   Perform authentication using the specified 3DES key on the currently
     *   selected DESFIRE application.
     *   The generated session key is afterwards used for ISO ciphering or CMACing.
     *   This function is not available on DESFIRE EV0 cards.
     *
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_AuthenticateIso24(byte bKeyNumber,
     *                                         const byte pbAccessKey[24]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_AuthenticateIso24(SPROX_INSTANCE rInst,
     *                                          byte bKeyNumber,
     *                                          const byte pbAccessKey[24]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_AuthenticateIso24(SCARDHANDLE hCard,
     *                                        byte bKeyNumber,
     *                                        const byte pbAccessKey[24]);
     *
     * INPUTS
     *   byte bKeyNumber             : number of the key (KeyNo)
     *   const byte pbAccessKey[24]  : 24-byte Access Key (DES/3DES2K/3DES3K keys)
     *
     * RETURNS
     *   DF_OPERATION_OK    : authentication succeed
     *   Other code if internal or communication error has occured. 
     *
     * NOTES
     *   Both DES and 3DES keys are stored in strings consisting of 24 bytes :
     *   - If the 2nd third of the key string is equal to the 1st third, the 
     *   64-bit key is handled as a single DES key by the DESFIRE card
     *   (well, actually there are only 56 significant bits).
     *   - If the 2nd third of the key string is NOT equal to the 1st third AND
     *   the 3rd third is equal to the 1st third, the key is a 128 bit 3DES key
     *   (well, actually there are only 112 significant bits).
     *   - Overwise, the key is a 192 bit 3DES key "3DES3K mode" (well, actually
     *   (well, actually there are only 168 significant bits).
     *
     * SEE ALSO
     *   Authenticate
     *   AuthenticateIso
     *   AuthenticateAes
     *   ChangeKeySettings
     *   GetKeySettings
     *   ChangeKey
     *   GetKeyVersion
     *
     **/    
    public long AuthenticateIso24(byte bKeyNumber, byte[] pbAccessKey)
    {
      byte rnd_size=0;
      long status;
      UInt32 t;
      byte[]  abRndB = new byte[16];
      byte[]  abRndA = new byte[16];
      
      if (pbAccessKey == null)
        return DFCARD_LIB_CALL_ERROR;
    
      /* Each new Authenticate must invalidate the current authentication status. */
      CleanupAuthentication();
    
      /* Create the command string consisting of the command byte and the parameter byte. */
      xfer_buffer[INF + 0] = DF_AUTHENTICATE_ISO;
      xfer_buffer[INF + 1] = bKeyNumber;
      xfer_length = 2;
    
      /* Send the command string to the PICC and get its response (1st frame exchange).
         The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
      status = Command(0, WANTS_ADDITIONAL_FRAME);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* Check the number of bytes received, we expect 9 or 17 bytes. */
      if (xfer_length == 9)
      {
        /* This is a 3DES2K (or a DES) */
        rnd_size = 8;
        bool are_equal = true;
        for (int i=0; i<8; i++)
          if (pbAccessKey[i] != pbAccessKey[i+8])
            are_equal = false;
        
        if (are_equal)
        {
          session_type = KEY_ISO_DES;
          SetKey(pbAccessKey);
          //InitCrypto3Des(pbAccessKey, pbAccessKey+8, null);
        } else
        {
          session_type = KEY_ISO_3DES2K;
          SetKey(pbAccessKey);
          //InitCrypto3Des(pbAccessKey, null, null);
        }
        
      } else
      if (xfer_length == 17)
      {                
        /* This is a 3DES3K */
        rnd_size = 16;
        session_type = KEY_ISO_3DES3K;
        SetKey(pbAccessKey);
        //InitCrypto3Des(pbAccessKey, pbAccessKey+8, pbAccessKey+16);
      } else
      {
        /* Error: block with inappropriate number of bytes received from the card. */
        return DFCARD_WRONG_LENGTH;
      }
    
      /* OK, we received the cryptogram Ek( RndB ) from the PICC.
         Decipher Ek( RndB ) to get RndB in xfer_buffer.
         Note that the status code has already been extracted from the queue. */
      t = rnd_size;
      byte[] tmp = new byte[rnd_size];
      Array.ConstrainedCopy(xfer_buffer, INF + 1, tmp, 0, rnd_size);
      CipherRecv(ref tmp, ref t);
    
      /* Store this RndB (is needed later on for generating the session key). */
      Array.ConstrainedCopy(tmp, 0, abRndB, 0, rnd_size);
      /*
      {
        Console.WriteLine("abRndB: ");
        for (int k=0; k<rnd_size; k++)
          Console.Write(String.Format("{0:x02}", abRndB[k]));
        Console.Write("\n");
      }
      */
     
      /* Now the PCD has to generate RndA. */
      //GetRandombytes(SPROX_PARAM_P  abRndA, rnd_size);
      Random rand = new Random();
        //abRndA = new byte[8];
        abRndA = new byte[ rnd_size]; /* MBA: 3K3DES use 16 bytes long random */
        for (int i=0; i<abRndA.Length; i++)
            abRndA[i] = (byte) rand.Next(0x00, 0xFF);
      
      /*
      {
        Console.WriteLine("abRndA: ");
        for (int k=0; k<abRndA.Length; k++)
          Console.Write(String.Format("{0:x02}", abRndA[k]));
        Console.Write("\n");
      }
      */   
    
      /* Start the second frame with a status byte indicating to the PICC that the Authenticate
         command is continued. */
      xfer_buffer[0] = DF_ADDITIONAL_FRAME;

      /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
        after the status byte. */            
      Array.ConstrainedCopy(abRndA, 0, xfer_buffer, 1, rnd_size);            
      Array.ConstrainedCopy(abRndB, 1, xfer_buffer, 1+rnd_size, rnd_size - 1);            

      xfer_buffer[1 + 2 * rnd_size - 1] = abRndB[0]; /* first byte move to last byte */
      xfer_length = (uint) (1 + 2 * rnd_size);
  
      /*
      {
        Console.WriteLine("xfer_buffer: ");
        for (int k=0; k<xfer_length; k++)
          Console.Write(String.Format("{0:x02}", xfer_buffer[k]));
        Console.Write("\n");
      }      
      */
      
      /* Apply the DES send operation to the argument bytes before sending the second frame
         to the PICC ( do not include the status byte in the DES operation ). */
      t = (uint) (2 * rnd_size);
      tmp = new byte[t];
      Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int) t);
      CipherSend(ref tmp, ref t, t);
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int) t);
      
      /* Send the 2nd frame to the PICC and get its response. */
      status = Command(0, WANTS_OPERATION_OK);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* We should now have Ek( RndA' ) in our buffer.
         RndA' was made from RndA by the PICC by rotating the string one byte to the left.
         Decipher Ek( RndA' ) to get RndA' in xfer_buffer. */
      t = rnd_size;
      tmp = new byte[rnd_size];
      Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int) t);
      CipherRecv(ref tmp, ref t);
      
      /*
      {
        Console.WriteLine("tmp: ");
        for (int k=0; k<t; k++)
          Console.Write(String.Format("{0:x02}", tmp[k]));
        Console.Write("\n");
      }
      */
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int) t);
      
      
      
      /* Now we have RndA' in our buffer.
         We have to check whether it matches our local copy of RndA.
         If one of the subsequent comparisons fails, we do not trust the PICC and therefore
         abort the authentication procedure ( no session key is generated ). */
      
      /* First compare the bytes 1 to bRndLen-1 of RndA with the first bRndLen-1 bytes in the queue. */
      /*
      {
        Console.WriteLine("xfer_buffer: ");
        for (int k=0; k<t+1; k++)
          Console.Write(String.Format("{0:x02}", xfer_buffer[k]));
        Console.Write("\n");
      }
      */
     
      for (byte i=1; i<rnd_size-1; i++)
        if (xfer_buffer[i] != abRndA[i])
          return DFCARD_WRONG_KEY;

      /* Then compare the leftmost byte of RndA with the last byte in the queue. */
      if (xfer_buffer[1 + rnd_size - 1] != abRndA[0])
        return DFCARD_WRONG_KEY;

      /* The actual authentication has succeeded.
         Finally we have to generate the session key from both random numbers RndA and RndB. */
      if (session_type == KEY_ISO_DES)
      {
        
        /*
        memcpy(session_key +  0, abRndA +  0, 4);
        memcpy(session_key +  4, abRndB +  0, 4);
        memcpy(session_key +  8, session_key, 8);
        memcpy(session_key + 16, session_key, 8);
        */
        
        session_key   = new byte[16];
        Array.ConstrainedCopy(abRndA, 0, session_key, 0, 4);
        Array.ConstrainedCopy(abRndB, 0, session_key, 4, 4);
        Array.ConstrainedCopy(session_key, 0, session_key, 8, 8);
        //Array.ConstrainedCopy(session_key, 0, session_key, 16, 8);
        
        //Console.WriteLine("KEY_ISO_DES");
        
        SetKey(session_key);
        
        //InitCrypto3Des(SPROX_PARAM_P  session_key, null, null);
        
      } else
      if (session_type == KEY_ISO_3DES2K)
      {
        /*
        memcpy(session_key +  0, abRndA +  0, 4);
        memcpy(session_key +  4, abRndB +  0, 4);
        memcpy(session_key +  8, abRndA +  4, 4);
        memcpy(session_key + 12, abRndB +  4, 4);
        memcpy(session_key + 16, session_key, 8);
        */
        
        
        session_key   = new byte[16];
        Array.ConstrainedCopy(abRndA, 0, session_key, 0, 4);
        Array.ConstrainedCopy(abRndB, 0, session_key, 4, 4);
        Array.ConstrainedCopy(abRndA, 4, session_key, 8, 4);
        Array.ConstrainedCopy(abRndB, 4, session_key, 12, 4);
        //Array.ConstrainedCopy(session_key, 0, session_key, 16, 8); 
        
        //byte[] test_cmac_session_key = { 0x4c, 0xf1, 0x51, 0x34, 0xa2, 0x85, 0x0d, 0xd5, 0x8a, 0x3d, 0x10, 0xba, 0x80, 0x57, 0x0d, 0x38 } ;
        
       
        //Console.WriteLine("KEY_ISO_3DES2K");
         
        SetKey(session_key);
        
        //InitCrypto3Des(session_key, session_key+8, null);
        
      } else
      if (session_type == KEY_ISO_3DES3K)
      {
        /*
        memcpy(session_key +  0, abRndA +  0, 4);
        memcpy(session_key +  4, abRndB +  0, 4);
        memcpy(session_key +  8, abRndA +  6, 4);
        memcpy(session_key + 12, abRndB +  6, 4);
        memcpy(session_key + 16, abRndA + 12, 4);
        memcpy(session_key + 20, abRndB + 12, 4);
        */
   
        Array.ConstrainedCopy(abRndA, 0, session_key, 0, 4);
        Array.ConstrainedCopy(abRndB, 0, session_key, 4, 4);
        Array.ConstrainedCopy(abRndA, 6, session_key, 8, 4);
        Array.ConstrainedCopy(abRndB, 6, session_key, 12, 4);
        Array.ConstrainedCopy(abRndA, 12, session_key, 16, 4);
        Array.ConstrainedCopy(abRndB, 12, session_key, 20, 4);
        
         //Console.WriteLine("KEY_ISO_3DES3K");
         
        SetKey(session_key);
        
        //Desfire_InitCrypto3Des(session_key, session_key+8, session_key+16);
      }
    
      /* Authenticate succeeded, therefore we remember the number of the key which was used
         to obtain the current authentication status. */
      session_key_id   = bKeyNumber;
    
      /* Reset the init vector */
      CleanupInitVector();
      
      /* Initialize the CMAC calculator */
      InitCmac();
    
      /* Success. */
      return DF_OPERATION_OK;
      
    }

    /**f* DesfireAPI/AuthenticateAes
     *
     * NAME
     *   AuthenticateAes
     *
     * DESCRIPTION
     *   Perform authentication using the specified AES key on the currently
     *   selected DESFIRE application.
     *   This function is not available on DESFIRE EV0 cards.
     *   The generated session key is afterwards used for ISO ciphering or CMACing.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_AuthenticateAes(byte bKeyNumber,
     *                                       const byte pbAccessKey[16]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_AuthenticateAes(SPROX_INSTANCE rInst,
     *                                        byte bKeyNumber,
     *                                        const byte pbAccessKey[16]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_AuthenticateAes(SCARDHANDLE hCard,
     *                                      byte bKeyNumber,
     *                                      const byte pbAccessKey[16]);
     *
     * INPUTS
     *   byte bKeyNumber             : number of the key (KeyNo)
     *   const byte pbAccessKey[16]  : 16-byte Access Key (AES)
     *
     * RETURNS
     *   DF_OPERATION_OK    : authentication succeed
     *   Other code if internal or communication error has occured. 
     *
     * NOTES
     *   AES keys are always 128-bit long.
     *
     * SEE ALSO
     *   Authenticate
     *   AuthenticateIso24
     *   AuthenticateIso
     *   ChangeKeySettings
     *   GetKeySettings
     *   ChangeKey
     *   GetKeyVersion
     *
     **/
    public long AuthenticateAes(byte bKeyNumber, byte[] pbAccessKey)
    {
      long status;
      UInt32 t;
      byte[] abRndB = new byte[16];
      byte[] abRndA = new byte[16];

      if (pbAccessKey == null)
        return DFCARD_LIB_CALL_ERROR;
    
      /* Each new Authenticate must invalidate the current authentication status. */
      CleanupAuthentication();
    
      /* Initialize the cipher unit with the authentication key */
      session_type = KEY_ISO_AES;
      SetKey(pbAccessKey);//InitCryptoAes(pbAccessKey);
    
      /* Create the command string consisting of the command byte and the parameter byte. */
      xfer_buffer[INF + 0] = DF_AUTHENTICATE_AES;
      xfer_buffer[INF + 1] = bKeyNumber;
      xfer_length = 2;
    
      /* Send the command string to the PICC and get its response (1st frame exchange).
         The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
      status = Command(0, WANTS_ADDITIONAL_FRAME);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* Check the number of bytes received, we expect 17 bytes. */
      if (xfer_length != 17)
      {
        /* Error: block with inappropriate number of bytes received from the card. */
        return DFCARD_WRONG_LENGTH;
      }
    
      /* OK, we received the cryptogram Ek( RndB ) from the PICC.
         Decipher Ek( RndB ) to get RndB in xfer_buffer.
         Note that the status code has already been extracted from the queue. */
      t = 16;
      byte[] tmp = new byte[t];
      Array.ConstrainedCopy(xfer_buffer, INF+1, tmp, 0, (int) t);
      CipherRecv(ref tmp, ref t);
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, INF+1, (int) t);
      
      /* Store this RndB (is needed later on for generating the session key). */
      Array.ConstrainedCopy(xfer_buffer, INF+1, abRndB, 0, 16);
      
      /* Now the PCD has to generate RndA. */
      //GetRandombytes(SPROX_PARAM_P  abRndA, 16);
      Random rand = new Random();
      abRndA = new byte[16];
      for (int i=0; i<abRndA.Length; i++)
        abRndA[i] = (byte) rand.Next(0x00, 0xFF);
      
      /* Start the second frame with a status byte indicating to the PICC that the Authenticate
         command is continued. */
      xfer_buffer[INF + 0] = DF_ADDITIONAL_FRAME;
    
      /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
         after the status byte. */
      Array.ConstrainedCopy(abRndA, 0, xfer_buffer, INF + 1, 16);
      Array.ConstrainedCopy(abRndB, 1, xfer_buffer, INF + 1 + 16, 15);
      xfer_buffer[INF + 1 + 31] = abRndB[0]; /* first byte move to last byte */
      xfer_length = 1 + 32;
    
      /* Apply the DES send operation to the argument bytes before sending the second frame
         to the PICC ( do not include the status byte in the DES operation ). */
      t = 32;
      tmp = new byte[t];
      Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int) t);
      CipherSend(ref tmp, ref t, t);
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int) t);
      
      /* Send the 2nd frame to the PICC and get its response. */
      status = Command(0, WANTS_OPERATION_OK);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* We should now have Ek( RndA' ) in our buffer.
         RndA' was made from RndA by the PICC by rotating the string one byte to the left.
         Decipher Ek( RndA' ) to get RndA' in xfer_buffer. */
      t = 16;
      tmp = new byte[t];
      Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int) t);
      CipherRecv(ref tmp, ref t);
      Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int) t);
      
      /* Now we have RndA' in our buffer.
         We have to check whether it matches our local copy of RndA.
         If one of the subsequent comparisons fails, we do not trust the PICC and therefore
         abort the authentication procedure ( no session key is generated ). */
    
      /* First compare the bytes 1 to bRndLen-1 of RndA with the first bRndLen-1 bytes in the queue. */
      for (int k=0; k<15; k++)
        if (xfer_buffer[INF + 1 + k] != abRndA[1 + k])
          return DFCARD_WRONG_KEY;

    
      /* Then compare the leftmost byte of RndA with the last byte in the queue. */
      if (xfer_buffer[INF + 1 + 15] != abRndA[0])
        return DFCARD_WRONG_KEY;

    
      /* The actual authentication has succeeded.
         Finally we have to generate the session key from both random numbers RndA and RndB. */
      /*
      memcpy(session_key +  0, abRndA +  0, 4);
      memcpy(session_key +  4, abRndB +  0, 4);
      memcpy(session_key +  8, abRndA + 12, 4);
      memcpy(session_key + 12, abRndB + 12, 4);
      memset(session_key + 16, 0, 8);
      */
      session_key   = new byte[16];
      Array.ConstrainedCopy(abRndA, 0, session_key, 0, 4);
      Array.ConstrainedCopy(abRndB, 0, session_key, 4, 4);
      Array.ConstrainedCopy(abRndA, 12, session_key, 8, 4);
      Array.ConstrainedCopy(abRndB, 12, session_key, 12, 4);
      
      /* Initialize the cipher unit with the session key */
      session_type = KEY_ISO_AES;
      SetKey(session_key);
      //Desfire_InitCryptoAes(SPROX_PARAM_P  session_key);
    
      /* Authenticate succeeded, therefore we remember the number of the key which was used
         to obtain the current authentication status. */
      session_key_id   = bKeyNumber;
    
      /* Reset the init vector */
      CleanupInitVector();
    
      /* Initialize the CMAC calculator */
      InitCmac();
    
      /* Success. */
      return DF_OPERATION_OK;
      
    }

    
    
    void CleanupAuthentication()
    {

      session_key_id = -1;
      session_type = KEY_EMPTY;
      init_vector = new byte[16];
      for (int i=0; i<init_vector.Length; i++)
        init_vector[i] = 0;
    
    }

    
  }
}
