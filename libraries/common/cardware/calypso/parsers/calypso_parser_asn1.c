/**h* CalypsoAPI/Calypso_TLV_Parser.c
 *
 * NAME
 *   SpringCard Calypso API :: Bit-level access to card's data (internal access only)
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release
 *
 **/
#include "../calypso_api_i.h"

BOOL get_char5_bits(const BYTE data[], SIZE_T size, CALYPSO_BITS_SZ *bit_offset, BYTE bit_count, char value[], BYTE length)
{
  CALYPSO_BITS_SZ  byte_offset;
  BYTE pos_in_byte, byte_mask, i;
  BYTE r = 0;  

  if (data == NULL)       return FALSE;
  if (bit_offset == NULL) return FALSE;
  if (value == NULL)      return FALSE;
  
  memset(value, 0, length);
    
  for (i=0; i<bit_count; i++)
  {
    byte_offset = *bit_offset / 8;

    if (byte_offset >= size) return FALSE;

    pos_in_byte = (BYTE) (7 - (*bit_offset % 8));
    byte_mask   = (BYTE) (1 << pos_in_byte);

    r <<= 1;
    if (data[byte_offset] & byte_mask)
    {
      r |= 1;
    }
    
    if ((i % 5) == 4)
    {
      if ((i / 5) >= length)
        break;
      
      /* TODO : find a documentation ? This switch has been written only from reverse engineering */
      switch (r)
      {        
        case 0 :
          value[i / 5] = '@';
          break;
        case 27 :
          value[i / 5] = ' ';
          break;
        default :
          value[i / 5] = 'A' + (r - 1);
      }
      r = 0;
    }

    *bit_offset = *bit_offset + 1;
  }

  return TRUE;
}

BOOL get_dword_bits(const BYTE data[], SIZE_T size, CALYPSO_BITS_SZ *bit_offset, BYTE bit_count, DWORD *value)
{
  CALYPSO_BITS_SZ  byte_offset;
  BYTE  pos_in_byte, byte_mask, i;
  DWORD r = 0;
  

  if (data == NULL)       return FALSE;
  if (bit_offset == NULL) return FALSE;
  if (value == NULL)      return FALSE;
  if (bit_count > 32)     return FALSE;

  for (i=0; i<bit_count; i++)
  {
    byte_offset = *bit_offset / 8;

    if (byte_offset >= size) return FALSE;

    pos_in_byte = (BYTE) (7 - (*bit_offset % 8));
    byte_mask   = (BYTE) (1 << pos_in_byte);

    r <<= 1;
    if (data[byte_offset] & byte_mask)
    {
      r |= 1;
    }

    *bit_offset = *bit_offset + 1;
  }

  *value = r;
  return TRUE;
}

CALYPSO_BITS_SZ count_zeros(const BYTE buffer[], CALYPSO_BITS_SZ bit_count)
{
  register CALYPSO_BITS_SZ i, r = 0;
  register BYTE b = 0, m = 0;

  for (i=0; i<bit_count; i++)
  {
    if (i & 0x00000007)
    {
      m >>= 1;
    } else
    {
      b = buffer[i / 8];
      m = 0x80;
    }
    
    if (!(b & m)) r++;
  }
    
  return r;
}

BOOL set_dword_bits(BYTE data[], SIZE_T size, CALYPSO_BITS_SZ *bit_offset, BYTE bit_count, DWORD value)
{
  CALYPSO_BITS_SZ byte_offset;
  BYTE pos_in_byte, byte_mask, i;
  DWORD bit_mask;
  
  bit_mask   = 1;
  bit_mask <<= bit_count;
  for (i=0; i<bit_count; i++)
  {   
    bit_mask >>= 1;
    
    byte_offset = *bit_offset / 8;
    
    if (byte_offset >= size) return FALSE;

    pos_in_byte = (BYTE) (7 - (*bit_offset % 8));    
    byte_mask   = (BYTE) (1 << pos_in_byte);
   
    if (value & bit_mask)
    {
      /* Bit must be set */
      data[byte_offset] |= byte_mask;
    } else
    {
      /* Bit must be cleared */
      if (data[byte_offset] & byte_mask)
        data[byte_offset] ^= byte_mask;      
    }
    
    *bit_offset = *bit_offset + 1;   
  }
  
  return TRUE;
}

CALYPSO_LIB BOOL CalypsoIsRecordEmpty(const BYTE data[], SIZE_T datasize)
{
  SIZE_T i;
  
  if (data == NULL) return TRUE;
  for (i=0; i<datasize; i++)
    if (data[i] != 0x00) return FALSE;
  
  return TRUE;
}
