/**h* CalypsoAPI/calypso_intercode_to_struct.c
 *
 * NAME
 *   calypso_intercode_to_struct.c
 *
 * DESCRIPTION
 *   Translation of INTERCODE records to C structures
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 25/03/2009 : first public release
 *   JDA 16/12/2009 : major rework, separated pure parser from XML stuff
 *
 **/
#include "../calypso_api_i.h"

#define SET_TARGET_VALUE(n, v)       target->n = v;

/**f* CalypsoAPI/CalypsoDecodeEnvAndHolderRecord
 *
 * NAME
 *   CalypsoDecodeEnvAndHolderRecord
 *
 * DESCRIPTION
 *   Parse the ENV
 *
 * INPUTS
 *   P_CALYPSO_CTX            ctx    : library context
 *   const BYTE               data[] : the record
 *   DWORD                    size   : size of the record
 *   CALYPSO_ENVANDHOLDER_ST *target : destination structure
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_PROC CalypsoDecodeEnvAndHolderRecord(P_CALYPSO_CTX ctx, const BYTE data[], SIZE_T datasize, CALYPSO_ENVANDHOLDER_ST *target)
{
  DWORD gen_bitmap, sub_bitmap;
  DWORD value;
  CALYPSO_BITS_SZ bit_offset = 0;
  
  CalypsoTraceStr(TR_TRACE|TR_CARD, "CalypsoDecodeEnvAndHolderRecord");
  
  if (ctx == NULL)    return CALYPSO_ERR_INVALID_CONTEXT;
  if (target == NULL) return CALYPSO_ERR_INVALID_PARAM;

  memset(target, 0, sizeof(CALYPSO_ENVANDHOLDER_ST));

  /* Environment part */
  /* ---------------- */

  if (!get_dword_bits(data, datasize, &bit_offset, 6, &value)) goto malform;
  SET_TARGET_VALUE(VersionNumber, (BYTE) value);
  
  ctx->CardData.EnvVersion = (BYTE) value;

  if (!get_dword_bits(data, datasize, &bit_offset, 7, &gen_bitmap)) goto malform;
  SET_TARGET_VALUE(_NotEmpty, (gen_bitmap) ? TRUE : FALSE);
  
  if (gen_bitmap & 0x00000001) /* 0 */
  {
    /* EnvNetworkId */
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform;
    SET_TARGET_VALUE(NetworkId, value);
  }
  if (gen_bitmap & 0x00000002) /* 1 */
  {
    /* EnvApplicationIssuerId */
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    SET_TARGET_VALUE(ApplicationIssuerId, (BYTE) value);
  }
  if (gen_bitmap & 0x00000004) /* 2 */
  {
    /* EnvApplicationEndDate */
    if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;
    SET_TARGET_VALUE(ApplicationEndDate, (WORD) value);
  }
  if (gen_bitmap & 0x00000008) /* 3 */
  {
    /* EnvPayMethod */
    if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
  }
  if (gen_bitmap & 0x00000010) /* 4 */
  {
    /* EnvAuthenticator */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    SET_TARGET_VALUE(Authenticator, (WORD) value);
  }
  if (gen_bitmap & 0x00000020) /* 5 */
  {
    /* EnvSelectList */
    if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform;
  }
  if (gen_bitmap & 0x00000040) /* 6 */
  {
    /* EnvData */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      if (sub_bitmap & 0x00000001)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 1, &value)) goto malform;
        
        if (value)
          SET_TARGET_VALUE(_TestCard, TRUE);
      }
      if (sub_bitmap & 0x00000002)
      {
        /* TODO */
      }
    }
  }
  
  /* Holder part */
  /* ----------- */

  if (!get_dword_bits(data, datasize, &bit_offset, 8, &gen_bitmap)) return 0;

  if (gen_bitmap & 0x00000001) /* 0 */
  {
    /* Name */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {    
      if (sub_bitmap & 0x00000001)
      {
        bit_offset += 85;
      }
      if (sub_bitmap & 0x00000002)
      {
        bit_offset += 85;
      }
    }
  }
  if (gen_bitmap & 0x00000002) /* 1 */
  {
    /* Birth */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform;
  } else
  {
    sub_bitmap = 0;
  }
    
  if (sub_bitmap || (gen_bitmap & 0x00000004)) /* 2 */
  {    
    if (sub_bitmap & 0x00000001)
    {
      bit_offset += 32;
    }
    if (sub_bitmap & 0x00000002)
    {
      bit_offset += 115;
    }
    if (gen_bitmap & 0x00000004) /* 2 */
    {
      /* BirthName */
      bit_offset += 85;
    }
  }

  if (gen_bitmap & 0x00000008) /* 3 */
  {
    /* IdNumber */
    bit_offset += 32;
  }
  if (gen_bitmap & 0x00000010) /* 4 */
  {
    /* Country */
    bit_offset += 24;
  }
  if (gen_bitmap & 0x00000020) /* 5 */
  {
    /* Company */
    bit_offset += 32;
  }
  if (gen_bitmap & 0x00000020) /* 6 */
  {
    /* Profiles */
    DWORD l, i;
    
    if (!get_dword_bits(data, datasize, &bit_offset, 4, &l)) goto malform;
    
    for (i=0; i<4; i++)
    {
      if (l & (1 << i))
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 3, &sub_bitmap)) goto malform;
        
        if (sub_bitmap)
        {
          if (sub_bitmap & 0x00000001) /* 0 */
          {
            bit_offset += 24;
          }
          if (gen_bitmap & 0x00000002) /* 1 */
          {
            bit_offset += 8;
          }
          if (gen_bitmap & 0x00000004) /* 2 */
          {
            bit_offset += 14;
          }
        }
      }      
    }
  }
  if (gen_bitmap & 0x00000040) /* 7 */
  {
    /* Data */
    if (!get_dword_bits(data, datasize, &bit_offset, 12, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    { 
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        /* HolderDataCardStatus */
        if (!get_dword_bits(data, datasize, &bit_offset, 4, &value)) goto malform;
        SET_TARGET_VALUE(HolderDataCardStatus, (BYTE) value);
        SET_TARGET_VALUE(_HolderDataCardStatus, TRUE);
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        /* HolderDataTelereglement */
        bit_offset += 4;
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        /* HolderDataResidence */
        bit_offset += 17;
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        /* HolderDataCommercialID */
        bit_offset += 6;
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        /* HolderDataWorkPlace */
        bit_offset += 17;
      }
      if (sub_bitmap & 0x00000020) /* 5 */
      {
        /* HolderDataStudyPlace */
        bit_offset += 17;
      }
      if (sub_bitmap & 0x00000040) /* 6 */
      {
        /* HolderDataSalePlace */
        bit_offset += 16;
      }
      if (sub_bitmap & 0x00000080) /* 7 */
      {
        /* HolderDataAuthenticator */
        bit_offset += 16;
      }
      if (sub_bitmap & 0x00000080) /* 8 */
      {
        /* HolderDataProfileStartDate1 */
        bit_offset += 14;
      }
      if (sub_bitmap & 0x00000100) /* 9 */
      {
        /* HolderDataProfileStartDate2 */
        bit_offset += 14;
      }
      if (sub_bitmap & 0x00000200) /* 10 */
      {
        /* HolderDataProfileStartDate3 */
        bit_offset += 14;
      }
      if (sub_bitmap & 0x00000400) /* 11 */
      {
        /* HolderDataProfileStartDate4 */
        bit_offset += 14;
      }
    }
  }
  
  return 0;
  
malform:  
  return CALYPSO_CARD_DATA_MALFORMED;
}

/**f* Calypso_API/CalypsoDecodeContractRecord
 *
 * NAME
 *   CalypsoDecodeContractRecord
 *
 * DESCRIPTION
 *   Parse a record from the Contracts file
 *
 * INPUTS
 *   P_CALYPSO_CTX        ctx    : library context
 *   const BYTE           data[] : the record
 *   DWORD                size   : size of the record
 *   CALYPSO_CONTRACT_ST *target : destination structure
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_PROC CalypsoDecodeContractRecord(P_CALYPSO_CTX ctx, const BYTE data[], SIZE_T datasize, CALYPSO_CONTRACT_ST *target)
{
  DWORD gen_bitmap, sub_bitmap;
  DWORD value;
  CALYPSO_BITS_SZ bit_offset = 0;
  SIZE_T zeros;
  
  CalypsoTraceStr(TR_TRACE|TR_CARD, "CalypsoDecodeContractRecord");

  if (ctx == NULL)    return CALYPSO_ERR_INVALID_CONTEXT;
  if (target == NULL) return CALYPSO_ERR_INVALID_PARAM;

  memset(target, 0, sizeof(CALYPSO_CONTRACT_ST));
  
  if (!get_dword_bits(data, datasize, &bit_offset, 20, &gen_bitmap)) goto malform; 
  SET_TARGET_VALUE(_NotEmpty, (gen_bitmap) ? TRUE : FALSE);
    
  if (gen_bitmap & 0x00000001) /* 0 */
  {
    /* ContractNetworkId */
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform;
    SET_TARGET_VALUE(NetworkId, value);
    SET_TARGET_VALUE(_NetworkId, TRUE);
  }
  if (gen_bitmap & 0x00000002) /* 1 */
  {
    /* ContractProvider */
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    SET_TARGET_VALUE(Provider, (BYTE) value);
  }
  if (gen_bitmap & 0x00000004) /* 2 */
  {
    /* ContractTariff */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    SET_TARGET_VALUE(Tariff, (WORD) value);
  }
  if (gen_bitmap & 0x00000008) /* 3 */
  {
    /* ContractSerialNumber */
    if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform;
    SET_TARGET_VALUE(SerialNumber, (DWORD) value);
    SET_TARGET_VALUE(_SerialNumber, TRUE);
  }
  if (gen_bitmap & 0x00000010) /* 4 */
  {
    /* ContractCustomer */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {    
      if (sub_bitmap & 0x00000001)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 6, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000002)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform;
      }
    }
  }
  if (gen_bitmap & 0x00000020) /* 5 */
  {
    /* ContractPassenger */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      if (sub_bitmap & 0x00000001)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000002)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
      }
    }
  }
  if (gen_bitmap & 0x00000040) /* 6 */
  {
    /* ContractVehicleClassAllowed */
    if (!get_dword_bits(data, datasize, &bit_offset, 6, &value)) goto malform;
  }
  if (gen_bitmap & 0x00000080) /* 7 */
  {
    /* ContractPaymentPointer */
    if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform;
  }
  if (gen_bitmap & 0x00000100) /* 8 */
  {
    /* ContractPayMethod */
    if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
  }
  if (gen_bitmap & 0x00000200) /* 9 */
  {
    /* ContractServices */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
  }
  if (gen_bitmap & 0x00000400) /* 10 */
  {
    /* ContractPriceAmount */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
  }
  if (gen_bitmap & 0x00000800) /* 11 */
  {
    /* ContractPriceUnit */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
  }
  if (gen_bitmap & 0x00001000) /* 12 */
  {   
    /* ContractRestrict */
    if (!get_dword_bits(data, datasize, &bit_offset, 7, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000020) /* 5 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000040) /* 6 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }     
    }
  }
  if (gen_bitmap & 0x00002000) /* 13 */
  {
    /* ContractValidity */
    if (!get_dword_bits(data, datasize, &bit_offset, 9, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;      
        SET_TARGET_VALUE(StartDate, (WORD) value);
        SET_TARGET_VALUE(_StartDate, TRUE);
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;      
        SET_TARGET_VALUE(EndDate, (WORD) value);
        SET_TARGET_VALUE(_EndDate, TRUE);
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
      }
      if (sub_bitmap & 0x00000020) /* 5 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000040) /* 6 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
        SET_TARGET_VALUE(Areas, (BYTE) value);
      }
      if (sub_bitmap & 0x00000080) /* 7 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000100) /* 8 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
    }
  }
  if (gen_bitmap & 0x00004000) /* 14 */
  {
    /* ContractJourney */
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000020) /* 5 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000040) /* 6 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000080) /* 7 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
      }
    }
  }
  if (gen_bitmap & 0x00008000) /* 15 */
  {
    /* ContractSale */
    if (!get_dword_bits(data, datasize, &bit_offset, 4, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        /* ContractSaleDate */
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        /* ContractSaleTime */
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        /* ContractSaleCompany */
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        /* ContractSaleDevice */
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
      }
    }
  }
 
  zeros = count_zeros(data, bit_offset);
  
  if (gen_bitmap & 0x00010000) /* 16 */
  {
    /* ContractStatus */
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    SET_TARGET_VALUE(Status, (BYTE) value);
  }
  if (gen_bitmap & 0x00020000) /* 17 */
  {
    /* ContractLoyaltyPoints */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
  }
  if (gen_bitmap & 0x00040000) /* 18 */
  {
    /* ContractAuthenticator */
    if (ctx->CardData.EnvVersion & 0x38)
    {
      if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    } else
    {
      if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
    }
    SET_TARGET_VALUE(Authenticator, (WORD) value);
  }
  
  if (gen_bitmap & 0x00080000) /* 19 */
  {
    /* ContractData */
    // TODO
    
  }

  SET_TARGET_VALUE(Authenticator_C, (WORD) (zeros + 5));

  return 0;
  
malform:
  return CALYPSO_CARD_DATA_MALFORMED;
}
