/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 14:26
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_wrap.
  /// </summary>
  public partial class Desfire
  {
    bool BuildIsoApdu(byte[] native_buffer, UInt32 native_buflen, byte[] iso_buffer, ref UInt32 iso_buflen, bool use_reader_wrapping)
    {
      if ((native_buffer == null) || (iso_buffer    == null) )
        return false;
    
      /* We can't accept an empty command */   
      if (native_buflen == 0)
        return false;      
      
      if (use_reader_wrapping)
      {
        /* Send an encapsulated APDU to the CSB6's embedded interpreter */
        /* ------------------------------------------------------------ */
    
        /* Check target length */
        if (iso_buflen < (6+native_buflen))
          return false;
        
        /* Build the APDU */    
        iso_buffer[0] = 0xFF;                                  /* CLA  : CSB6's embedded interpreter */
        iso_buffer[1] = 0xFE;                                  /* INS  : ENCAPSULATE                 */
        iso_buffer[2] = 0x00;                                  /* P1   : dummmy                      */
        iso_buffer[3] = 0x00;                                  /* P2   : dummy                       */
        iso_buffer[4] = (byte) native_buflen;                  /* LC   : size of native buffer       */
        Array.ConstrainedCopy(native_buffer, 0, iso_buffer, 5, (int) native_buflen); /* Data : native buffer itself        */
        iso_buffer[5+native_buflen] = 0;                       /* LE   : accept any length in return */
        
        iso_buflen = 6+native_buflen;
        
      } else
      {
        /* Use the Desfire ISO wrapping */
        /* ---------------------------- */    
    
        /* Check target length */
        if (iso_buflen < (5+native_buflen))
          return false;
        
        /* Build the APDU */
        iso_buffer[0] = 0x90;                                  /* CLA  : Desfire specifc class       */
        iso_buffer[1] = native_buffer[0];                      /* INS  : Desfire command code        */
        iso_buffer[2] = 0x00;                                  /* P1   : dummmy                      */
        iso_buffer[3] = 0x00;                                  /* P2   : dummy                       */
        
        if (native_buflen > 1)
        {
          iso_buffer[4] = (byte) (native_buflen-1);            /* LC   : length of command params    */
          if (native_buflen>1)                                 /* Data : Desfire command parameters  */ 
            Array.ConstrainedCopy(native_buffer, 1, iso_buffer, 5, (int)  (native_buflen - 1));
          iso_buffer[5+native_buflen-1] = 0;                   /* LE   : accept any length in return */
          iso_buflen   = 5+native_buflen;
        } else
        {
          iso_buffer[4] = 0x00;                                /* LE   : accept any length in return */        
          iso_buflen   = 5;
        }  
        
    
      }
      
      return true;
    }
  }
}
