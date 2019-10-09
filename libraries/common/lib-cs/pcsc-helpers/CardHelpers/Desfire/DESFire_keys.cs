/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 15/09/2017
 * Time: 10:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_keys.
  /// </summary>
  public partial class Desfire
  {
    /**h* DesfireAPI/Keys
     *
     * NAME
     *   DesfireAPI :: Key management functions
     *
     * COPYRIGHT
     *   (c) 2009 SpringCard - www.springcard.com
     *
     * DESCRIPTION
     *   Implementation of management functions to change keys or
     *   key settings withing a DESFIRE application.
     *
     **/
    
    /**f* DesfireAPI/ChangeKeySettings
     *
     * NAME
     *   ChangeKeySettings
     *
     * DESCRIPTION
     *   Changes the key settings of the currently selected application
     *   (or of card's master key if root application is selected)
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_ChangeKeySettings (byte key_settings);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ChangeKeySettings(SPROX_INSTANCE rInst,
     *                                          byte key_settings);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ChangeKeySettings  (SCARDHANDLE hCard,
     *                                          byte key_settings);
     *
     * INPUTS
     *   byte key_settings  : new key settings (see chapter 4.3.2 of datasheet of mifare
     *                        DesFire MF3ICD40 for more information)
     *
     * RETURNS
     *   DF_OPERATION_OK    : change succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   Authenticate
     *   GetKeySettings
     *   ChangeKey
     *   GetKeyVersion
     *
     **/
    public long ChangeKeySettings(byte key_settings)
    {
      xfer_length = 0;
    
      /* Create the info block containing the command code */
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY_SETTINGS;
      xfer_buffer[xfer_length++] = key_settings;
    

      /* Append the CRC value corresponding to the key_settings byte */
      /* Then 'Encrypt' the bNewKeySettings byte and its CRC bytes.  */
      XferAppendCrc(1);
      XferCipherSend(1);

    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
    
    
    }
    
    /**f* DesfireAPI/GetKeySettings
     *
     * NAME
     *   GetKeySettings
     *
     * DESCRIPTION
     *   Gets information on the DesFire card and application master key settings. In addition it returns
     *   the maximum number of keys which can be stored within the selected application.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_GetKeySettings (byte *key_settings,
     *                                       byte *key_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_GetKeySettings(SPROX_INSTANCE rInst,
     *                                       byte *key_settings,
     *                                       byte *key_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetKeySettings  (SCARDHANDLE hCard,
     *                                       byte *key_settings,
     *                                       byte *key_count);
     *
     * INPUTS
     *   byte *key_settings          : master key settings (see chapter 4.3.2 of datasheet of mifare
     *                                 DesFire MF3ICD40 for more information)
     *   byte *key_count             : maximum number of keys
     *
     * RETURNS
     *   DF_OPERATION_OK    : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   Authenticate
     *   ChangeKeySettings
     *   ChangeKey
     *   GetKeyVersion
     *
     **/
    public long GetKeySettings(ref byte key_settings, ref byte key_count)
    {
      long  status;

      /* Create the info block containing the command code */
      xfer_buffer[INF + 0] = DF_GET_KEY_SETTINGS;
      xfer_length = 1;
    
      /* Communicate the info block to the card and check the operation's return status. */
      /* GetKeySettings returns 3 bytes, the status and two bytes of information.        */
      status = Command(3, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* Return the requested key settings bytes. */
      key_settings = xfer_buffer[INF + 1];
      key_count = xfer_buffer[INF + 2];
    
      /* Success */
      return DF_OPERATION_OK;
    }
    
    /**f* DesfireAPI/ChangeKey
     *
     * NAME
     *   ChangeKey
     *
     * DESCRIPTION
     *   Change a DES, or 3DES2K key in the selected Desfire application.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_ChangeKey(byte key_number,
     *                                 const byte new_key[16],
     *                                 const byte old_key[16]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ChangeKey(SPROX_INSTANCE rInst,
     *                                  byte key_number,
     *                                  const byte new_key[16],
     *                                  const byte old_key[16]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ChangeKey(SCARDHANDLE hCard,
     *                                byte key_number,
     *                                const byte new_key[16],
     *                                const byte old_key[16]);
     *
     * INPUTS
     *   byte key_number        : number of the key (KeyNo)
     *   const byte new_key[16] : 16-byte New Key (DES/3DES keys)
     *   const byte old_key[16] : 16-byte Old Key (DES/3DES keys)
     *
     * RETURNS
     *   DF_OPERATION_OK    : change succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
     *   * If the 2nd half of the key string is equal to the 1st half, the key is
     *   handled as a single DES key by the DesFire card.
     *   * If the 2nd half of the key string is NOT equal to the 1st half, the key
     *   is handled as a 3DES key.
     *
     *   After a successful change of the key used to reach the current authentication status, this
     *   authentication is invalidated, an authentication with the new key is necessary for subsequent
     *   operations.
     *
     *   If authentication has been performed before calling ChangeKey with the old key,
     *   use null instead of old_key.
     *
     * SEE ALSO
     *   ChangeKey24
     *   ChangeKeyAes
     *   Authenticate
     *   ChangeKeySettings
     *   GetKeySettings
     *   GetKeyVersion
     *
     **/
    public long ChangeKey(byte key_number, byte[] new_key, byte[] old_key)
    {
      byte b;
      byte[] buffer = new byte[24];
    
      if ( (current_aid != 0x000000) && ((key_number & DF_APPLSETTING2_AES) != 0) ) /* Update 27/07/2011 to check the current AID (patch submitted by Evaldas Auryla) */
        return DFCARD_LIB_CALL_ERROR;
    
      if ( (session_type == KEY_ISO_DES) || (session_type == KEY_ISO_3DES2K) || (session_type == KEY_ISO_3DES3K) || (session_type == KEY_ISO_AES) )
      {
        byte[] new_key_24 = new byte[24];
        byte[] old_key_24 = new byte[24];
    
        
        
        if (new_key != null)
        {
          Array.ConstrainedCopy(new_key, 0, new_key_24, 0, 16);
          Array.ConstrainedCopy(new_key, 0, new_key_24, 16, 8);       
          new_key = new_key_24;
        }
    
        if (old_key != null)
        {
          Array.ConstrainedCopy(old_key, 0, old_key_24, 0, 16); 
          Array.ConstrainedCopy(old_key, 0, old_key_24, 16, 8);
          old_key = old_key_24;
        }
    
        
        return ChangeKey24(key_number, new_key, old_key);
      }

      for (int k=0; k<buffer.Length; k++)
        buffer[k] = 0;

      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY;
      xfer_buffer[xfer_length++] = key_number;
    
      byte[] crc16 = new byte[2];
      if (old_key != null)
      {
        /* If a 'previous key' has been passed, format the 24 byte cryptogram according to the     */
        /* three key procedure (new key, previous key, all encrypted with the current session key) */
    
        /* Take the new key into the buffer. */
        Array.ConstrainedCopy(new_key, 0, buffer, 0, 16);
        
        /* XOR the previous key to the new key. */
        for (b = 0; b < 16; b++)
          buffer[b] ^= old_key[b];
    
        /* Append a first CRC, computed over the XORed key combination. */
        ComputeCrc16(buffer, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 16, 2);
        
        /* Append a second CRC, computed over the new key only. */
        ComputeCrc16(new_key, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 18, 2);
        
      } else
      {
        /* If no 'previous key' has been passed, format the 24 byte cryptogram according to the */
        /* two key procedure (new key, encrypted with the current session key).                 */
    
        /* Take the new key into the buffer. */
        for (int k =0; k< 16; k++)
          buffer[k] = new_key[k];
        
        /* Append the CRC computed over the new key. */
        ComputeCrc16(new_key, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 16, 2);
      
      }
    
      /* Append the 24 byte buffer to the command string. */
      Array.ConstrainedCopy(buffer, 0, xfer_buffer, (int) xfer_length, 24);
      xfer_length += 24;

      /* 'Encrypt' the 24 bytes */
      XferCipherSend(2);
    
      /* Forget the current authentication state if we're changing the current key */
      if ((key_number & 0x3F) == (session_key_id & 0x3F))
        CleanupAuthentication();
    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, WANTS_OPERATION_OK);
    }
    
    /**f* DesfireAPI/ChangeKey24
     *
     * NAME
     *   ChangeKey24
     *
     * DESCRIPTION
     *   Change a 3DES3K key in the selected Desfire application.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_ChangeKey24(byte key_number,
     *                                   const byte new_key[24],
     *                                   const byte old_key[24]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ChangeKey24(SPROX_INSTANCE rInst,
     *                                    byte key_number,
     *                                    const byte new_key[24],
     *                                    const byte old_key[24]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ChangeKey24(SCARDHANDLE hCard,
     *                                  byte key_number,
     *                                  const byte new_key[24],
     *                                  const byte old_key[24]);
     *
     * INPUTS
     *   byte key_number        : number of the key (KeyNo)
     *   const byte new_key[24] : 24-byte New Key (3DES keys)
     *   const byte old_key[24] : 24-byte Old Key (3DES keys)
     *
     * RETURNS
     *   DF_OPERATION_OK    : change succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ChangeKey16
     *   ChangeKeyAes
     *   Authenticate
     *   ChangeKeySettings
     *   GetKeySettings
     *   GetKeyVersion
     *
     **/
    public long ChangeKey24(byte key_number, byte[] new_key, byte[] old_key)
    {
      byte  key_size;
      byte  b;

      if ((key_number & DF_APPLSETTING2_AES) != 0)
        return DFCARD_LIB_CALL_ERROR;
    
      if ((key_number & DF_APPLSETTING2_3DES3K) != 0)
      {
        key_size = 24;
    
      } else
      {
        switch (session_type)
        {
          case KEY_ISO_AES    :
          case KEY_ISO_DES    :
          case KEY_ISO_3DES2K : key_size = 16; break;
    
          case KEY_ISO_3DES3K : key_size = 24; break;
    
          default             : return DFCARD_LIB_CALL_ERROR;
        }
      }

      for (int k=0; k<xfer_buffer.Length; k++)
        xfer_buffer[k] = 0;
      
      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY;
      xfer_buffer[xfer_length++] = key_number;

      /* Take the new key into the buffer. */
      Array.ConstrainedCopy(new_key, 0, xfer_buffer, (int) xfer_length, key_size);

      xfer_length += key_size;
      
      /*
      {
        Console.Write("xfer_buffer: ");
        for (int k=0; k<xfer_length; k++)
          Console.Write(String.Format("{0:x02}", xfer_buffer[k]));
        Console.Write("\n");
      }
      */
     
      byte[] crc32 = new byte[4];
      if (old_key != null)
      {
        /* If a 'previous key' has been passed, format the 40 byte cryptogram according to the     */
        /* three key procedure (new key, previous key, all encrypted with the current session key) */
    
        /* XOR the previous key with the new key. */
        for (b = 0; b < key_size; b++)
          xfer_buffer[xfer_length - key_size + b] ^= old_key[b];
    
        /* Append a first CRC, computed over the XORed key combination and the header */
        ComputeCrc32(xfer_buffer, xfer_length, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int) xfer_length, 4);
        xfer_length += 4;
    
        /* Append a second CRC, computed over the new key only. */
        ComputeCrc32(new_key, key_size, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int) xfer_length, 4);
        
        xfer_length += 4;
    
      } else
      {
        /* If no 'previous key' has been passed, format the 32 byte cryptogram according to the */
        /* two key procedure (new key, encrypted with the current session key).                 */
    
        /* Append the CRC computed over the new key and the header */
        ComputeCrc32(xfer_buffer, xfer_length, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int) xfer_length, 4);
        xfer_length += 4;
      }
    
      /* 'Encrypt' the buffer */
      XferCipherSend(2);
    
      /* Forget the current authentication state if we're changing the current key */
      if ((key_number & 0x3F) == (session_key_id & 0x3F))
        CleanupAuthentication();
    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, LOOSE_RESPONSE_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
    }
    
    /**f* DesfireAPI/ChangeKeyAes
     *
     * NAME
     *   ChangeKeyAes
     *
     * DESCRIPTION
     *   Change an AES key in the selected Desfire application.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_ChangeKeyAes(byte key_number,
     *                                    byte key_version,
     *                                    const byte new_key[16],
     *                                    const byte old_key[16]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ChangeKeyAes(SPROX_INSTANCE rInst,
     *                                     byte key_number,
     *                                     byte key_version,
     *                                     const byte new_key[16],
     *                                     const byte old_key[16]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ChangeKeyAes(SCARDHANDLE hCard,
     *                                   byte key_number,
     *                                   byte key_version,
     *                                   const byte new_key[16],
     *                                   const byte old_key[16]);
     *
     * INPUTS
     *   byte key_number        : number of the key (KeyNo)
     *   byte key_version       : version number to be stored together with the key
     *   const byte new_key[16] : 16-byte New Key (AES key)
     *   const byte old_key[16] : 16-byte Old Key (AES key)
     *
     * RETURNS
     *   DF_OPERATION_OK    : change succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ChangeKey
     *   ChangeKey24
     *   Authenticate
     *   ChangeKeySettings
     *   GetKeySettings
     *   GetKeyVersion
     *
     **/
    public long ChangeKeyAes(byte key_number, byte key_version, byte[] new_key, byte[] old_key)
    {
      byte  b;

      for (int k=0; k<xfer_buffer.Length; k++)
        xfer_buffer[k] = 0;
      
    
      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY;
      xfer_buffer[xfer_length++] = key_number;
    
      /* Take the new key into the buffer. */
      Array.ConstrainedCopy(new_key, 0, xfer_buffer, (int) xfer_length, 16);
      xfer_length += 16;
    
      byte[] crc32 = new byte[4];
      if (old_key != null)
      {
        /* If a 'previous key' has been passed, format the 40 byte cryptogram according to the     */
        /* three key procedure (new key, previous key, all encrypted with the current session key) */
    
        /* XOR the previous key with the new key. */
        for (b = 0; b < 16; b++)
          xfer_buffer[xfer_length - 16 + b] ^= old_key[b];
    
        /* The key version goes here */
        xfer_buffer[xfer_length++] = key_version;
    
        /* Append a first CRC, computed over the XORed key combination and the header */

        ComputeCrc32(xfer_buffer, xfer_length, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int) xfer_length, 4);
        
        xfer_length += 4;
    
        /* Append a second CRC, computed over the new key only. */
        ComputeCrc32(new_key, 16, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int) xfer_length, 4);
        xfer_length += 4;
    
      } else
      {
        /* If no 'previous key' has been passed, format the 32 byte cryptogram according to the */
        /* two key procedure (new key, encrypted with the current session key).                 */
    
        /* The key version goes here */
        xfer_buffer[xfer_length++] = key_version;
    
        /* Append the CRC computed over the new key and the header */
        ComputeCrc32( xfer_buffer, xfer_length, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int) xfer_length, 4);
        xfer_length += 4;
      }
    
      /* 'Encrypt' the buffer */
      XferCipherSend(2);
    
      /* Forget the current authentication state if we're changing the current key */
      if ((key_number & 0x3F) == (session_key_id & 0x3F))
        CleanupAuthentication();
    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, LOOSE_RESPONSE_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
    }
    
    /**f* DesfireAPI/GetKeyVersion
     *
     * NAME
     *   GetKeyVersion
     *
     * DESCRIPTION
     *   Reads out the current key version of any key stored on the PICC
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_GetKeyVersion(byte bKeyNumber,
     *                                     byte *pbKeyVersion);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_GetKeyVersion(SPROX_INSTANCE rInst,
     *                                      byte bKeyNumber,
     *                                      byte *pbKeyVersion);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetKeyVersion(SCARDHANDLE hCard,
     *                                    byte bKeyNumber,
     *                                    byte *pbKeyVersion);
     *
     * INPUTS
     *   byte bKeyNumber             : number of the key (KeyNo)
     *   byte *pbKeyVersion          : current version of the specified key
     *
     * RETURNS
     *   DF_OPERATION_OK    : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   This command can be issued without valid authentication.
     *
     * SEE ALSO
     *   Authenticate
     *   GetKeySettings
     *   ChangeKeySettings
     *   ChangeKey
     *
     **/
    public long GetKeyVersion(byte bKeyNumber, ref byte pbKeyVersion)
    {
      long status;
    
      /* Create the info block containing the command code and the key number argument. */
      xfer_buffer[INF + 0] = DF_GET_KEY_VERSION;
      xfer_buffer[INF + 1] = bKeyNumber;
      xfer_length = 2;
    
      /* Communicate the info block to the card and check the operation's return status.   */
      /* The returned info block must contain two bytes, the status code and the requested */
      /* key version byte.                                                                 */
      status = Command(2, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* Get the key version byte. */
      pbKeyVersion = xfer_buffer[INF + 1];
    
      /* Success. */
      return DF_OPERATION_OK;
    }

  }
}
