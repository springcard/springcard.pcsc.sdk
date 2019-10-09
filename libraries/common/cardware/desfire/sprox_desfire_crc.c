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
#include "sprox_desfire_i.h"

static void UpdateDesfireCrc16(BYTE ch, WORD * lpwCrc)
{
  ch = (ch ^ (BYTE) ((*lpwCrc) & 0x00FF));

#ifdef _DEBUG_CRC
  printf("%02X ", ch);
#endif

  ch = (ch ^ (ch << 4));

#ifdef _DEBUG_CRC
  printf("%02X ", ch);
#endif

  *lpwCrc = (*lpwCrc >> 8) ^ ((WORD) ch << 8) ^ ((WORD) ch << 3) ^ ((WORD) ch >> 4);

#ifdef _DEBUG_CRC
  printf("%04X ", *lpwCrc);
#endif
}

WORD ComputeCrc16(const BYTE buffer[], DWORD size, BYTE crc[2])
{
  BYTE  chBlock;
  WORD  wCrc = 0x6363; /* ITU-V.41 */
  BYTE *p = (BYTE *) buffer;

  if (buffer == NULL)
    return wCrc;

#ifdef _DEBUG_CRC
  printf("CRC16, init=%04X\n", wCrc);
#endif
  
  do
  {
    chBlock = *p++;

#ifdef _DEBUG_CRC
    printf("\t%04X %02X   ", wCrc, chBlock);
#endif

    UpdateDesfireCrc16(chBlock, &wCrc);

#ifdef _DEBUG_CRC
    printf("\n");
#endif

  } while (--size);

#ifdef _DEBUG_CRC
  printf("\t%04X\n", wCrc);
#endif

    if (crc != NULL)
  {
    crc[0] = (BYTE) (wCrc & 0xFF);
    crc[1] = (BYTE) ((wCrc >> 8) & 0xFF);
  }

  return wCrc;
}

////////////////////////////////////////////////////////////////////////////////////////////////////
/**************************************************************************************************/
//	this function calculates a 32 bit CRC value (Ethernet AAL5 standard )
//	The generator polynome is: x32 + x26 + x23 + x22 + x16 + x12 + x11 + x10 + x8 + x7 + x5 + x4 + x2 + x1 + 1
//INPUT:
//      unsigned char bInput.............the 8 bit value is added to the 32 bit CRC value (peReg)
//OUTPUT:
//      unsigned long *peReg............pointer to the 32 bit CRC value

static void UpdateDesfireCrc32(BYTE ch, DWORD *pdwCrc)
{
  BYTE b;

  *pdwCrc ^= ch;

  //	bit wise calcualtion of the CRC value
  for (b=0; b<8; b++)
  {
    if (*pdwCrc & 0x00000001)
    {
      *pdwCrc >>= 1;
      *pdwCrc  ^= 0xEDB88320;
    } else
    {
      *pdwCrc >>= 1;
    }
  }
}

DWORD ComputeCrc32(const BYTE buffer[], DWORD size, BYTE crc[4])
{
  DWORD dwCrc = 0xFFFFFFFF;
  BYTE *p = (BYTE *) buffer;
 
  do
  {
    UpdateDesfireCrc32(*p++, &dwCrc);
  } while (--size);

  if (crc != NULL)
  {
    crc[0] = (BYTE) (dwCrc);
    crc[1] = (BYTE) (dwCrc >> 8);
    crc[2] = (BYTE) (dwCrc >> 16);
    crc[3] = (BYTE) (dwCrc >> 24);
  }

  return dwCrc;
}





void Desfire_XferAppendCrc(SPROX_PARAM  DWORD start_offset)
{  
  SPROX_DESFIRE_GET_CTX_V();

  if (start_offset >= ctx->xfer_length) return;

  if (ctx->session_type & KEY_ISO_MODE)
  {
    if ((ctx->xfer_length + 4) > sizeof(ctx->xfer_buffer)) return;

    ComputeCrc32(&ctx->xfer_buffer[0], ctx->xfer_length, &ctx->xfer_buffer[ctx->xfer_length]);
    ctx->xfer_length += 4;

  } else
  {
    if ((ctx->xfer_length + 2) > sizeof(ctx->xfer_buffer)) return;

    ComputeCrc16(&ctx->xfer_buffer[start_offset], ctx->xfer_length - start_offset, &ctx->xfer_buffer[ctx->xfer_length]);
    ctx->xfer_length += 2;
  }
}

void Desfire_ComputeCrc16(SPROX_PARAM  const BYTE data[], DWORD length, BYTE crc[2])
{
	SPROX_DESFIRE_GET_CTX_V();
	
  ComputeCrc16(data, length, crc);
}

SPROX_RC Desfire_VerifyCrc16(SPROX_PARAM  BYTE data[], DWORD length, BYTE crc[2])
{
  BYTE check[2];
  SPROX_DESFIRE_GET_CTX();

  if (data == NULL)
    return DFCARD_LIB_CALL_ERROR;

  if (crc == NULL)
    crc = &data[length];

  ComputeCrc16(data, length, check);

  if (memcmp(check, crc, 2))
    return DFCARD_WRONG_CRC;

  return DF_OPERATION_OK;
}

void Desfire_ComputeCrc32(SPROX_PARAM  const BYTE data[], DWORD length, BYTE crc[4])
{
	SPROX_DESFIRE_GET_CTX_V();
	
  ComputeCrc32(data, length, crc);
}

SPROX_RC Desfire_VerifyCrc32(SPROX_PARAM  BYTE data[], DWORD length, BOOL move_status, BYTE crc[4])
{
  BYTE check[4];
  BYTE status;  
  SPROX_DESFIRE_GET_CTX();

  if (data == NULL)
    return DFCARD_LIB_CALL_ERROR;

  if (crc == NULL)
    crc = &data[length];

  if (move_status)
  {
    DWORD i;

    status = data[0];
    for (i=0; i<length-1; i++)
      data[i] = data[i+1];
    data[length-1] = status;
  }

  ComputeCrc32(data, length, check);

  if (move_status)
  {
    DWORD i;

    status = data[length-1];
    for (i=length-1; i>=1; i--)
      data[i] = data[i-1];
    data[0] = status;
  }

  if (memcmp(check, crc, 4))
    return DFCARD_WRONG_CRC;

  return DF_OPERATION_OK;
}

