/**h* CalypsoAPI/calypso_intercode_to_xml_core.c
 *
 * NAME
 *   calypso_intercode_to_xml_core.c
 *
 * DESCRIPTION
 *   Translation of INTERCODE records to XML
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

CALYPSO_RC __CalypsoOutputEnvAndHolderRecordEx(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T datasize, BYTE param)
{
  DWORD gen_bitmap, sub_bitmap;
  DWORD value;
  CALYPSO_BITS_SZ bit_offset = 0;
  const char *in_section = NULL;
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_RAW))
    ParserOut_Hex(ctx, strRaw, data, datasize);

  /* Environment part */
  /* ---------------- */

  if (param == 0x03)
    ParserOut_SectionBegin(ctx, strEnvironment);

  if (!get_dword_bits(data, datasize, &bit_offset, 6, &value)) goto malform_e;
  if (param != 0x02)
    ParserOut_Dec(ctx, strVersion, value);
  
  ctx->CardData.EnvVersion = (BYTE) value;

  if (!get_dword_bits(data, datasize, &bit_offset, 7, &gen_bitmap)) goto malform_e;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
    if (param != 0x02)
      ParserOut_Bin(ctx, strBitmap, gen_bitmap, 7);
 
  if (gen_bitmap & 0x00000001) /* 0 */
  {
    /* EnvNetworkId */
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform_e;
    if (param != 0x02)
      ParserOut_Hex24(ctx, strNetwork, value);
    
    if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_EXPLAIN))
    {
      if (param != 0x02)
      {
        ParserOut_Hex12(ctx, strNetworkCountry, value >> 12);
        ParserOut_Hex12(ctx, strNetworkIndex, value);
      }
    }
  }
  if (gen_bitmap & 0x00000002) /* 1 */
  {
    /* EnvApplicationIssuerId */
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform_e;
    if (param != 0x02)
      ParserOut_Dec(ctx, strIssuer, value);
  }
  if (gen_bitmap & 0x00000004) /* 2 */
  {
    /* EnvApplicationEndDate */
    if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform_e;
    if (param != 0x02)
      ParserOut_Date(ctx, strEndDate, translate_datestamp_14(value));
  }
  if (gen_bitmap & 0x00000008) /* 3 */
  {
    /* EnvPayMethod */
    if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform_e;
    if (param != 0x02)
      ParserOut_Dec(ctx, strPayMethod, value);
  }
  if (gen_bitmap & 0x00000010) /* 4 */
  {
    /* EnvAuthenticator */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform_e;
    if (param != 0x02)
      ParserOut_Hex16(ctx, strAuthenticator, value);
  }
  if (gen_bitmap & 0x00000020) /* 5 */
  {
    /* EnvSelectList */
    if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform_e;
    if (param != 0x02)
      ParserOut_Bin(ctx, strSelect, value, 32);
  }
  if (gen_bitmap & 0x00000040) /* 6 */
  {
    /* EnvData */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform_e;
    
    if (sub_bitmap)
    {
      if (param != 0x02)
      {
        in_section = strData;
        ParserOut_SectionBegin(ctx, in_section);
      }

      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        if (param != 0x02)
          ParserOut_Bin(ctx, strBitmap, sub_bitmap, 2);
      
      if (sub_bitmap & 0x00000001)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 1, &value)) goto malform_e;
        if (param != 0x02)
          ParserOut_Dec(ctx, strStatus, value);
      }
      if (sub_bitmap & 0x00000002)
      {
        /* TODO */
      }

      if (param != 0x02)
      {
        ParserOut_SectionEnd(ctx, in_section);
        in_section = NULL;
      }
    }

  }
  
  if (param == 0x03)
    ParserOut_SectionEnd(ctx, strEnvironment);
  if (param == 0x01)
    return 0;

  /* Holder part */
  /* ----------- */

  if (!get_dword_bits(data, datasize, &bit_offset, 8, &gen_bitmap)) return 0;

  if (param == 0x03)
    ParserOut_SectionBegin(ctx, strHolder);

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
    ParserOut_Bin(ctx, strBitmap, gen_bitmap, 8);

  if (gen_bitmap & 0x00000001) /* 0 */
  {
    /* Name */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform_h;    
    if (sub_bitmap)
    {    
      char buffer[18];
      
      in_section = strName;
      ParserOut_SectionBegin(ctx, in_section);

      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 2);

      if (sub_bitmap & 0x00000001)
      {
        if (!get_char5_bits(data, datasize, &bit_offset, 85, buffer, sizeof(buffer))) goto malform_h;
        ParserOut_Str(ctx, strLast, buffer);
      }
      if (sub_bitmap & 0x00000002)
      {
        if (!get_char5_bits(data, datasize, &bit_offset, 85, buffer, sizeof(buffer))) goto malform_h;
        ParserOut_Str(ctx, strFirst, buffer);
      }

      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }
  if (gen_bitmap & 0x00000002) /* 1 */
  {
    /* Birth */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform_h;
  } else
  {
    sub_bitmap = 0;
  }
    
  if (sub_bitmap || (gen_bitmap & 0x00000004)) /* 2 */
  {    
    char buffer[24];

    in_section = strBirth;
    ParserOut_SectionBegin(ctx, in_section);

    if (sub_bitmap && (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP)))
      ParserOut_Bin(ctx, strBitmap, sub_bitmap, 2);

    if (sub_bitmap & 0x00000001)
    {
      if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform_h;
      ParserOut_Date(ctx, strDate, translate_date_bcd32(value));
    }
    if (sub_bitmap & 0x00000002)
    {
      if (!get_char5_bits(data, datasize, &bit_offset, 115, buffer, sizeof(buffer))) goto malform_h;
      ParserOut_Str(ctx, strPlace, buffer);
    }
    if (gen_bitmap & 0x00000004) /* 2 */
    {
      /* BirthName */
      if (!get_char5_bits(data, datasize, &bit_offset, 85, buffer, sizeof(buffer))) goto malform_h;
      ParserOut_Str(ctx, strName, buffer);
    }

    ParserOut_SectionEnd(ctx, in_section);
    in_section = NULL;
  }

  if (gen_bitmap & 0x00000008) /* 3 */
  {
    /* IdNumber */
    if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform_h;
    ParserOut_Dec(ctx, strNumber, value);
  }
  if (gen_bitmap & 0x00000010) /* 4 */
  {
    /* Country */
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform_h;
    ParserOut_Dec(ctx, strCountry, value);
  }
  if (gen_bitmap & 0x00000020) /* 5 */
  {
    /* Company */
    if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform_h;
    ParserOut_Dec(ctx, strCompany, value);
  }
  if (gen_bitmap & 0x00000020) /* 6 */
  {
    /* Profiles */
    DWORD l, i;
    
    if (!get_dword_bits(data, datasize, &bit_offset, 4, &l)) goto malform_h;
    
    for (i=0; i<4; i++)
    {
      if (l & (1 << i))
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 3, &sub_bitmap)) goto malform_h;
        
        if (sub_bitmap)
        {
          in_section = strProfile;
          ParserOut_SectionBeginId(ctx, in_section, i+1);
          
          if (sub_bitmap & 0x00000001) /* 0 */
          {
            if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform_h;
            ParserOut_Hex24(ctx, strNetwork, value);
            
            if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_EXPLAIN))
            {
              ParserOut_Hex12(ctx, strNetworkCountry, value >> 12);
              ParserOut_Hex12(ctx, strNetworkIndex, value);
            }
          }
          if (gen_bitmap & 0x00000002) /* 1 */
          {
            if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform_h;
            ParserOut_Dec(ctx, strNumber, value);
          }
          if (gen_bitmap & 0x00000004) /* 2 */
          {
            if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform_h;
            ParserOut_Date(ctx, strDate, translate_datestamp_14(value));
          }

          ParserOut_SectionEnd(ctx, in_section);
          in_section = NULL;
        }
      }      
    }
  }
  if (gen_bitmap & 0x00000040) /* 7 */
  {
    /* Data */
    if (!get_dword_bits(data, datasize, &bit_offset, 12, &sub_bitmap)) goto malform_h;
    
    if (sub_bitmap)
    { 
      in_section = strData;   
      ParserOut_SectionBegin(ctx, in_section);

      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 12);

      if (sub_bitmap & 0x00000001) /* 0 */
      {
        /* HolderDataCardStatus */
        if (!get_dword_bits(data, datasize, &bit_offset, 4, &value)) goto malform_h;
        ParserOut_Dec(ctx, strStatus, value);
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        /* HolderDataTelereglement */
        if (!get_dword_bits(data, datasize, &bit_offset, 4, &value)) goto malform_h;
        ParserOut_Dec(ctx, strRemotePay, value);
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        /* HolderDataResidence */
        if (!get_dword_bits(data, datasize, &bit_offset, 17, &value)) goto malform_h;
        ParserOut_Dec(ctx, strLivePlace, value);
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        /* HolderDataCommercialID */
        if (!get_dword_bits(data, datasize, &bit_offset, 6, &value)) goto malform_h;
        ParserOut_Dec(ctx, strProduct, value);
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        /* HolderDataWorkPlace */
        if (!get_dword_bits(data, datasize, &bit_offset, 17, &value)) goto malform_h;
        ParserOut_Dec(ctx, strWorkPlace, value);
      }
      if (sub_bitmap & 0x00000020) /* 5 */
      {
        /* HolderDataStudyPlace */
        if (!get_dword_bits(data, datasize, &bit_offset, 17, &value)) goto malform_h;
        ParserOut_Dec(ctx, strStudyPlace, value);
      }
      if (sub_bitmap & 0x00000040) /* 6 */
      {
        /* HolderDataSalePlace */
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform_h;
        ParserOut_Dec(ctx, strDevice, value);
      }
      if (sub_bitmap & 0x00000080) /* 7 */
      {
        /* HolderDataAuthenticator */
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform_h;
        ParserOut_Hex16(ctx, strAuthenticator, value);
      }
      if (sub_bitmap & 0x00000080) /* 8 */
      {
        /* HolderDataProfileStartDate1 */
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform_h;
        ParserOut_DateId(ctx, strStartDate, 1, translate_datestamp_14(value));
      }
      if (sub_bitmap & 0x00000100) /* 9 */
      {
        /* HolderDataProfileStartDate2 */
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform_h;
        ParserOut_DateId(ctx, strStartDate, 2, translate_datestamp_14(value));
      }
      if (sub_bitmap & 0x00000200) /* 10 */
      {
        /* HolderDataProfileStartDate3 */
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform_h;
        ParserOut_DateId(ctx, strStartDate, 3, translate_datestamp_14(value));
      }
      if (sub_bitmap & 0x00000400) /* 11 */
      {
        /* HolderDataProfileStartDate4 */
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform_h;
        ParserOut_DateId(ctx, strStartDate, 4, translate_datestamp_14(value));
      }

      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }

  if (param == 0x03)
    ParserOut_SectionEnd(ctx, strHolder);

  return 0;
  
malform_e:  
  if (in_section != NULL)
    ParserOut_SectionEnd(ctx, in_section);
  if (param == 0x03)
    ParserOut_SectionEnd(ctx, strEnvironment);
  return CALYPSO_CARD_DATA_MALFORMED;

malform_h:  
  if (in_section != NULL)
    ParserOut_SectionEnd(ctx, in_section);
  if (param == 0x03)
    ParserOut_SectionEnd(ctx, strHolder);
  return CALYPSO_CARD_DATA_MALFORMED;
}



CALYPSO_PROC CalypsoOutputEnvironmentRecordEx(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T datasize)
{
  return __CalypsoOutputEnvAndHolderRecordEx(ctx, data, datasize, 1);
}

CALYPSO_PROC CalypsoOutputHolderRecordEx(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T datasize)
{
  return __CalypsoOutputEnvAndHolderRecordEx(ctx, data, datasize, 2);
}

/**f* Calypso_API/CalypsoOutputEnvAndHolderRecordEx
 *
 * NAME
 *   CalypsoOutputEnvAndHolderRecordEx
 *
 * DESCRIPTION
 *   Parse the ENV
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx  : library context
 *   const BYTE    data[] : the record
 *   DWORD         size   : size of the record
 *   BOOL          flat
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_PROC CalypsoOutputEnvAndHolderRecordEx(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T datasize, BOOL flat)
{
  if (flat)
    return __CalypsoOutputEnvAndHolderRecordEx(ctx, data, datasize, 0);
  else
    return __CalypsoOutputEnvAndHolderRecordEx(ctx, data, datasize, 3);
}


/**f* CSB6_Calypso/CalypsoOutputContractRecordEx
 *
 * NAME
 *   CalypsoOutputContractRecordEx
 *
 * DESCRIPTION
 *   Parse a record from the Contracts file
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx  : library context
 *   const BYTE    data[] : the record
 *   DWORD         size   : size of the record
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_PROC CalypsoOutputContractRecordEx(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T datasize)
{
  DWORD gen_bitmap, sub_bitmap;
  DWORD value;
  CALYPSO_BITS_SZ bit_offset = 0;
  CALYPSO_BITS_SZ zeros;
  const char *in_section = NULL;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  ParserOut_Hex(ctx, strRaw, data, datasize);
  
  if (!get_dword_bits(data, datasize, &bit_offset, 20, &gen_bitmap)) goto malform;
  
  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
    ParserOut_Bin(ctx, strBitmap, gen_bitmap, 20);
     
  if (gen_bitmap & 0x00000001) /* 0 */
  {
    /* ContractNetworkId */
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform;
    ParserOut_Hex24(ctx, strNetwork, value);
    
    if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_EXPLAIN))
    {
      ParserOut_Hex12(ctx, strNetworkCountry, value >> 12);
      ParserOut_Hex12(ctx, strNetworkIndex, value);
    }
  }
  if (gen_bitmap & 0x00000002) /* 1 */
  {
    /* ContractProvider */
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strProvider, value);
  }
  if (gen_bitmap & 0x00000004) /* 2 */
  {
    /* ContractTariff */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strTariff, value);
  }
  if (gen_bitmap & 0x00000008) /* 3 */
  {
    /* ContractSerialNumber */
    if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform;
    ParserOut_Dec(ctx, strNumber, value);
  }
  if (gen_bitmap & 0x00000010) /* 4 */
  {
    /* ContractCustomer */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {    
      in_section = strCustomer;
      ParserOut_SectionBegin(ctx, in_section);

      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 2);

      if (sub_bitmap & 0x00000001)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 6, &value)) goto malform;
        ParserOut_Dec(ctx, strProfile, value);
      }
      if (sub_bitmap & 0x00000002)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform;
        ParserOut_Dec(ctx, strNumber, value);
      }
      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }
  if (gen_bitmap & 0x00000020) /* 5 */
  {
    /* ContractPassenger */
    if (!get_dword_bits(data, datasize, &bit_offset, 2, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      in_section = strPassenger;
      ParserOut_SectionBegin(ctx, in_section);

      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 2);
      
      if (sub_bitmap & 0x00000001)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
        ParserOut_Dec(ctx, strClass, value);
      }
      if (sub_bitmap & 0x00000002)
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
        ParserOut_Dec(ctx, strTotal, value);      
      }
      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }
  if (gen_bitmap & 0x00000040) /* 6 */
  {
    /* ContractVehicleClassAllowed */
    if (!get_dword_bits(data, datasize, &bit_offset, 6, &value)) goto malform;
    ParserOut_Dec(ctx, strClassAllowed, value);
  }
  if (gen_bitmap & 0x00000080) /* 7 */
  {
    // TODO
    /* ContractPaymentPointer */
    if (!get_dword_bits(data, datasize, &bit_offset, 32, &value)) goto malform;

    ParserOut_DecId(ctx, strPayPointer, 1, (value >> 24) & 0x000000FF);
    ParserOut_DecId(ctx, strPayPointer, 2, (value >> 16) & 0x000000FF);
    ParserOut_DecId(ctx, strPayPointer, 3, (value >> 8)  & 0x000000FF);
    ParserOut_DecId(ctx, strPayPointer, 4, value         & 0x000000FF);
  }
  if (gen_bitmap & 0x00000100) /* 8 */
  {
    /* ContractPayMethod */
    if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
    ParserOut_Dec(ctx, strPayMethod, value);
  }
  if (gen_bitmap & 0x00000200) /* 9 */
  {
    /* ContractServices */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strServices, value);
  }
  if (gen_bitmap & 0x00000400) /* 10 */
  {
    /* ContractPriceAmount */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strPriceAmount, value);
  }
  if (gen_bitmap & 0x00000800) /* 11 */
  {
    /* ContractPriceUnit */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strPriceUnit, value);
  }
  if (gen_bitmap & 0x00001000) /* 12 */
  {   
    /* ContractRestrict */
    if (!get_dword_bits(data, datasize, &bit_offset, 7, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      in_section = strRestrict;
      ParserOut_SectionBegin(ctx, in_section);

      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 7);
    
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
        ParserOut_Time(ctx, strStart, translate_timestamp_11(value));
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
        ParserOut_Time(ctx, strEnd, translate_timestamp_11(value));
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
        ParserOut_Dec(ctx, strDay, value);
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
        ParserOut_Dec(ctx, strTimeCode, value);
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
        ParserOut_Dec(ctx, strCode, value);
      }
      if (sub_bitmap & 0x00000020) /* 5 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strProduct, value);
      }
      if (sub_bitmap & 0x00000040) /* 6 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strLocation, value);
      }
      
      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }
  if (gen_bitmap & 0x00002000) /* 13 */
  {
    /* ContractValidity */
    if (!get_dword_bits(data, datasize, &bit_offset, 9, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      in_section = strValidity;
      ParserOut_SectionBegin(ctx, in_section);
      
      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 9);
      
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;      
        ParserOut_Date(ctx, strStartDate, translate_datestamp_14(value));
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
        ParserOut_Time(ctx, strStartTime, translate_timestamp_11(value));
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;      
        ParserOut_Date(ctx, strEndDate, translate_datestamp_14(value));
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
        ParserOut_Time(ctx, strEndTime, translate_timestamp_11(value));
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
        ParserOut_Dec(ctx, strDuration, value);
      }
      if (sub_bitmap & 0x00000020) /* 5 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;      
        ParserOut_Date(ctx, strLimitDate, translate_datestamp_14(value));
      }
      if (sub_bitmap & 0x00000040) /* 6 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
        ParserOut_Dec(ctx, strZones, value);
        
        if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_EXPLAIN))
          ParserOut_IdfZones(ctx, strZoneList, value);
      }
      if (sub_bitmap & 0x00000080) /* 7 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strValidJourneys, value);
      }
      if (sub_bitmap & 0x00000100) /* 8 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strPeriodJourneys, value);
      }
      
      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }
  if (gen_bitmap & 0x00004000) /* 14 */
  {
    /* ContractJourney */
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      in_section = strJourney;
      ParserOut_SectionBegin(ctx, in_section);
  
      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 8);
      
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strOrigin, value);
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strDestination, value);
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strRouteNumbers, value);
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
        ParserOut_Dec(ctx, strRouteVariants, value);
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strRun, value);
      }
      if (sub_bitmap & 0x00000020) /* 5 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strVia, value);
      }
      if (sub_bitmap & 0x00000040) /* 6 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strDistance, value);
      }
      if (sub_bitmap & 0x00000080) /* 7 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
        ParserOut_Dec(ctx, strInterchanges, value);
      }
      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }
  if (gen_bitmap & 0x00008000) /* 15 */
  {
    /* ContractSale */
    if (!get_dword_bits(data, datasize, &bit_offset, 4, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      in_section = strSale;
      ParserOut_SectionBegin(ctx, in_section);
  
      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 4);
      
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        /* ContractSaleDate */
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;      
        ParserOut_Date(ctx, strDate, translate_datestamp_14(value));
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        /* ContractSaleTime */
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;      
        ParserOut_Time(ctx, strTime, translate_timestamp_11(value));
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        /* ContractSaleCompany */
        if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
        ParserOut_Dec(ctx, strProvider, value);
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        /* ContractSaleDevice */
        if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
        ParserOut_Dec(ctx, strDevice, value);
      }
      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }
 
  zeros = count_zeros(data, bit_offset);
  
  if (gen_bitmap & 0x00010000) /* 16 */
  {
    /* ContractStatus */
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strStatus, value);
  }
  if (gen_bitmap & 0x00020000) /* 17 */
  {
    /* ContractLoyaltyPoints */
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;      
    ParserOut_Dec(ctx, strLoyalty, value);
  }
  if (gen_bitmap & 0x00040000) /* 18 */
  {
    /* ContractAuthenticator */
    if (ctx->CardData.EnvVersion & 0x38)
    {
      if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
      ParserOut_Hex16(ctx, strAuthenticator, value);
    } else
    {
      if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
      ParserOut_Hex8(ctx, strAuthenticator, value);
    }
  }
  
  if (gen_bitmap & 0x00080000) /* 19 */
  {
    /* ContractData */
    // TODO
    
  }
 
  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_AUTH_C))
    ParserOut_Hex16(ctx, str_Authenticator_C, zeros+5);

  return 0;
  
malform:
  if (in_section != NULL)
    ParserOut_SectionEnd(ctx, in_section);
  return CALYPSO_CARD_DATA_MALFORMED;
}

/**f* Calypso_API/CalypsoOutputEventRecordEx
 *
 * NAME
 *   CalypsoOutputEventRecordEx
 *
 * DESCRIPTION
 *   Parse a record from the Transport Log or Special Event file
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx  : library context
 *   const BYTE    data[] : the record
 *   DWORD         size   : size of the record
 *
 * RETURNS
 *   CALYPSO_RC
 *
 **/
CALYPSO_PROC CalypsoOutputEventRecordEx(CALYPSO_CTX_ST *ctx, const BYTE data[], SIZE_T datasize)
{
  DWORD gen_bitmap, sub_bitmap;
  DWORD value;
  CALYPSO_BITS_SZ bit_offset = 0;
  const char *in_section = NULL;

  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_RAW))
    ParserOut_Hex(ctx, strRaw, data, datasize);

  if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;
  ParserOut_Date(ctx, strDate, translate_datestamp_14(value));

  if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
  ParserOut_Time(ctx, strTime, translate_timestamp_11(value));

  if (!get_dword_bits(data, datasize, &bit_offset, 28, &gen_bitmap)) goto malform;
  
  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
    ParserOut_Bin(ctx, strBitmap, gen_bitmap, 28);
  
  if (gen_bitmap & 0x00000001) /* 0 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strDisplayData, value);
  }
  if (gen_bitmap & 0x00000002) /* 1 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform;
    ParserOut_Hex24(ctx, strNetwork, value);
    
    if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_EXPLAIN))
    {
      ParserOut_Hex12(ctx, strNetworkCountry, value >> 12);
      ParserOut_Hex12(ctx, strNetworkIndex, value);
    }
  }
  if (gen_bitmap & 0x00000004) /* 2 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strCode, value);
  }
  if (gen_bitmap & 0x00000008) /* 3 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strResult, value);
  }
  if (gen_bitmap & 0x00000010) /* 4 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strProvider, value);
  }
  if (gen_bitmap & 0x00000020) /* 5 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strNotOKCounter, value);
  }
  if (gen_bitmap & 0x00000040) /* 6 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform;
    ParserOut_Dec(ctx, strNumber, value);
  }
  if (gen_bitmap & 0x00000080) /* 7 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strDestination, value);
  }
  if (gen_bitmap & 0x00000100) /* 8 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strLocation, value);
  }
  if (gen_bitmap & 0x00000200) /* 9 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strLocationGate, value);
  }
  if (gen_bitmap & 0x00000400) /* 10 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strDevice, value);
  }
  if (gen_bitmap & 0x00000800) /* 11 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strRouteNumber, value);
  }
  if (gen_bitmap & 0x00001000) /* 12 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strRouteVariant, value);
  }
  if (gen_bitmap & 0x00002000) /* 13 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strJourneyRun, value);
  }
  if (gen_bitmap & 0x00004000) /* 14 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strVehicle, value);
  }
  if (gen_bitmap & 0x00008000) /* 15 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strVehicleClass, value);
  }
  if (gen_bitmap & 0x00010000) /* 16 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 5, &value)) goto malform;
    ParserOut_Dec(ctx, strLocationType, value);
  }
  if (gen_bitmap & 0x00020000) /* 17 */
  {
    // TODO
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform;
    ParserOut_Dec(ctx, strEmployee, value);
  }
  if (gen_bitmap & 0x00040000) /* 18 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strLocationRef, value);
  }
  if (gen_bitmap & 0x00080000) /* 19 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strJourneyInterchanges, value);
  }
  if (gen_bitmap & 0x00100000) /* 20 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strPeriodJourneys, value);
  }
  if (gen_bitmap & 0x00200000) /* 21 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strTotalJourneys, value);
  }
  if (gen_bitmap & 0x00400000) /* 22 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strJourneyDistance, value);
  }
  if (gen_bitmap & 0x00800000) /* 23 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strPriceAmount, value);
  }
  if (gen_bitmap & 0x01000000) /* 24 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
    ParserOut_Dec(ctx, strPriceUnit, value);
  }
  if (gen_bitmap & 0x02000000) /* 25 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 5, &value)) goto malform;
    ParserOut_Dec(ctx, strContract, value);
  }
  if (gen_bitmap & 0x04000000) /* 26 */
  {
    if (ctx->CardData.EnvVersion & 0x38)
    {
      if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
      ParserOut_Hex16(ctx, strAuthenticator, value);
    } else
    {
      if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;      
      ParserOut_Hex8(ctx, strAuthenticator, value);
    }
  }
  if (gen_bitmap & 0x08000000) /* 27 */
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 5, &sub_bitmap)) goto malform;
    
    if (sub_bitmap)
    {
      /* EventData */
      in_section = strData;
      ParserOut_SectionBegin(ctx, in_section);
      
      if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
        ParserOut_Bin(ctx, strBitmap, sub_bitmap, 5);
      
      if (sub_bitmap & 0x00000001) /* 0 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 14, &value)) goto malform;
        ParserOut_Date(ctx, strStartDate, translate_datestamp_14(value));        
      }
      if (sub_bitmap & 0x00000002) /* 1 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 11, &value)) goto malform;
        ParserOut_Time(ctx, strStartTime, translate_timestamp_11(value));                
      }
      if (sub_bitmap & 0x00000004) /* 2 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 1, &value)) goto malform;
        ParserOut_Dec(ctx, strSimul, value);                
      }
      if (sub_bitmap & 0x00000008) /* 3 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 2, &value)) goto malform;
        ParserOut_Dec(ctx, strTrip, value);
      }
      if (sub_bitmap & 0x00000010) /* 4 */
      {
        if (!get_dword_bits(data, datasize, &bit_offset, 2, &value)) goto malform;
        ParserOut_Dec(ctx, strDirection, value);
      }   
    
      ParserOut_SectionEnd(ctx, in_section);
      in_section = NULL;
    }
  }

/*
  if (!get_dword_bits(data, datasize, &bit_offset, 4, &gen_bitmap)) return 0;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_BITMAP))
    ParserOut_Bin(ctx, strBitmap, gen_bitmap, 4);

  if (gen_bitmap & 0x00000001)
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 24, &value)) goto malform;
    ParserOut_Hex24(ctx, strListNetworkId, value);
  }
  if (gen_bitmap & 0x00000002)
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 8, &value)) goto malform;
    ParserOut_Dec(ctx, strContractListTariff, value);
  }
  if (gen_bitmap & 0x00000004)
  {
    if (!get_dword_bits(data, datasize, &bit_offset, 5, &value)) goto malform;
    ParserOut_Dec(ctx, strContractListPointer, value);
  }

  if (!get_dword_bits(data, datasize, &bit_offset, 16, &value)) goto malform;
  ParserOut_Dec(ctx, strDiagnosticCounter, value);
*/
  return 0;  

malform:  
  if (in_section != NULL)
    ParserOut_SectionEnd(ctx, in_section);
  return CALYPSO_CARD_DATA_MALFORMED;
}

