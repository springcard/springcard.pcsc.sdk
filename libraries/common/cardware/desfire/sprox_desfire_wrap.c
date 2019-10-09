/**h* DesfireAPI/Wrapper
 *
 * NAME
 *   DesfireAPI :: DESFIRE APDU wrapper
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   This module wraps DESFIRE commands and response into an ISO 7816-4 stream.
 *
 **/
#include "sprox_desfire_i.h"

/*
 **************************************************************************************************
 *
 *  Desfire command to APDU wrapper
 *
 *************************************************************************************************
 */ 
BOOL BuildIsoApdu(const BYTE native_buffer[], DWORD native_buflen, BYTE iso_buffer[], DWORD *iso_buflen, BOOL use_reader_wrapping)
{
  if ((native_buffer == NULL)
   || (iso_buffer    == NULL)
   || (iso_buflen    == NULL)) return FALSE;

  /* We can't accept an empty command */   
  if (native_buflen == 0)
    return FALSE;      
  
  if (use_reader_wrapping)
  {
    /* Send an encapsulated APDU to the CSB6's embedded interpreter */
    /* ------------------------------------------------------------ */

    /* Check target length */
    if (*iso_buflen < (6+native_buflen))
      return FALSE;
    
    /* Build the APDU */    
    iso_buffer[0] = 0xFF;                                  /* CLA  : CSB6's embedded interpreter */
    iso_buffer[1] = 0xFE;                                  /* INS  : ENCAPSULATE                 */
    iso_buffer[2] = 0x00;                                  /* P1   : dummmy                      */
    iso_buffer[3] = 0x00;                                  /* P2   : dummy                       */
    iso_buffer[4] = (BYTE) native_buflen;                  /* LC   : size of native buffer       */
    memcpy(&iso_buffer[5], native_buffer, native_buflen);  /* Data : native buffer itself        */
    iso_buffer[5+native_buflen] = 0;                       /* LE   : accept any length in return */
    
    *iso_buflen = 6+native_buflen;
    
  } else
  {
    /* Use the Desfire ISO wrapping */
    /* ---------------------------- */    

    /* Check target length */
    if (*iso_buflen < (5+native_buflen))
      return FALSE;
    
    /* Build the APDU */
    iso_buffer[0] = 0x90;                                  /* CLA  : Desfire specifc class       */
    iso_buffer[1] = native_buffer[0];                      /* INS  : Desfire command code        */
    iso_buffer[2] = 0x00;                                  /* P1   : dummmy                      */
    iso_buffer[3] = 0x00;                                  /* P2   : dummy                       */
    
    if (native_buflen > 1)
    {
      iso_buffer[4] = (BYTE) (native_buflen-1);            /* LC   : length of command params    */
      if (native_buflen>1)                                 /* Data : Desfire command parameters  */
        memcpy(&iso_buffer[5], &native_buffer[1], native_buflen-1);  
      iso_buffer[5+native_buflen-1] = 0;                   /* LE   : accept any length in return */        
      *iso_buflen   = 5+native_buflen;
    } else
    {
      iso_buffer[4] = 0x00;                                /* LE   : accept any length in return */        
      *iso_buflen   = 5;
    }  
    

  }
  
  return TRUE;
}
