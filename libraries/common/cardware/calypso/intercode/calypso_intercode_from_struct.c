#include "../calypso_api_i.h"

CALYPSO_PROC CalypsoEncodeEventRecord(P_CALYPSO_CTX ctx, BYTE data[], BYTE size, CALYPSO_EVENT_ST *values)
{
  DWORD gen_bitmap = 0;
  CALYPSO_BITS_SZ pos_bitmap = 0;
  CALYPSO_BITS_SZ bit_offset = 0;
 
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;
  if ((data == NULL) || (values == NULL)) return CALYPSO_ERR_INVALID_PARAM;

  memset(data, 0, size);
   
  /* Put date and time */
  if (!set_dword_bits(data, size, &bit_offset, 14, values->EventDate)) goto failed;
  if (!set_dword_bits(data, size, &bit_offset, 11, values->EventTime)) goto failed;
  
  /* Leave room for the general bitmap (will be update later) */
  pos_bitmap = bit_offset;
  if (!set_dword_bits(data, size, &bit_offset, 28, 0)) goto failed;
    
  if (TRUE)
  {
    /* Code */
    gen_bitmap |= 0x00000004;
    if (!set_dword_bits(data, size, &bit_offset, 8, values->Code)) goto failed;
  }
  if (TRUE)
  {
    /* Service provider */
    gen_bitmap |= 0x00000010;
    if (!set_dword_bits(data, size, &bit_offset, 8, values->ServiceProvider)) goto failed;
  }
  if (values->_NotOKCounter)
  {
    /* Not OK Counter */
    gen_bitmap |= 0x00000020;
    if (!set_dword_bits(data, size, &bit_offset, 8, values->NotOKCounter)) goto failed;
  }
  if (values->_LocationId)
  {
    /* LocationId */
    gen_bitmap |= 0x00000100;
    if (!set_dword_bits(data, size, &bit_offset, 16, values->LocationId)) goto failed;
  }
  if (values->_Device)
  {
    /* Device */
    gen_bitmap |= 0x00000400;
    if (!set_dword_bits(data, size, &bit_offset, 16, values->Device)) goto failed;
  }
  if (values->_RouteNumber)
  {
    /* RouteNumber */
    gen_bitmap |= 0x00000800;
    if (!set_dword_bits(data, size, &bit_offset, 16, values->RouteNumber)) goto failed;
  }
  if (values->_JourneyRun)
  {
    /* JourneyRun */
    gen_bitmap |= 0x00002000;
    if (!set_dword_bits(data, size, &bit_offset, 16, values->JourneyRun)) goto failed;
  }
  if (values->_VehicleId)
  {
    /* VehicleId */
    gen_bitmap |= 0x00004000;
    if (!set_dword_bits(data, size, &bit_offset, 16, values->VehicleId)) goto failed;
  }
  if (TRUE)
  {
    /* ContractPointer */
    gen_bitmap |= 0x02000000;
    if (!set_dword_bits(data, size, &bit_offset, 5, values->ContractPointer)) goto failed;
  }
  
  /* Put back the actual bitmap */
  set_dword_bits(data, size, &pos_bitmap, 28, gen_bitmap);
  
  /* Here we go ! */
  return 0;
  
failed:
  return CALYPSO_ERR_BUFFER_TOO_SHORT;  
}


