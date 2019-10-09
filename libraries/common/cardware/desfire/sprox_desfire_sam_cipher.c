#include "sprox_desfire_i.h"

#ifdef SPROX_DESFIRE_WITH_SAM


LONG SAM_GenerateMAC(SCARDHANDLE hSam, BYTE *pbIn, DWORD dwInLength, BYTE *pbOut, DWORD * dwOutLength)
{
  LONG rc;
  DWORD i;
  BYTE capdu[MAX_DATA_SIZE+6], rapdu[MAX_DATA_SIZE+6];
  DWORD capdu_len = 0;
	DWORD rapdu_len = MAX_DATA_SIZE+6;

  if (dwInLength > 0xF6)
    return DFCARD_ERROR;

  capdu[capdu_len++] = 0x80;
  capdu[capdu_len++] = 0x7C;
  capdu[capdu_len++] = 0x00; //all data transmitted
  capdu[capdu_len++] = 0x00; //offset
  capdu[capdu_len++] = (BYTE) dwInLength;
  for(i = 0; i< (int) dwInLength; i++)
  {
    if (capdu_len<sizeof(capdu))
    {
      capdu[capdu_len++] = pbIn[i];
    } else
    {
      return DFCARD_OVERFLOW;
    }
  }
  if (capdu_len < sizeof(capdu))
  {
    capdu[capdu_len++] = 0x00;
  } else
  {
    return DFCARD_OVERFLOW;
  }

# ifdef _DEBUG_SAM_CIPHER
  printf("SAM_GenerateMAC Capdu=");
  for (i=0; i<capdu_len; i++)
    printf("%2.2x", capdu[i]);
  printf("\n");
# endif

  rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
  if (rc != SCARD_S_SUCCESS)
    return rc;

# ifdef _DEBUG_SAM_CIPHER
  printf("Rapdu=");
  for (i=0; i<rapdu_len; i++)
    printf("%2.2x", rapdu[i]);
  printf("\n");
# endif

  if ((rapdu[rapdu_len-2] != 0x90) && (rapdu[rapdu_len-1] != 0x00))
    return SCARD_W_CARD_NOT_AUTHENTICATED;

  if ( (*dwOutLength) < (DWORD) (rapdu_len-2))
    return DFCARD_OVERFLOW;

  for (i=0; i<rapdu_len -2; i++)
    pbOut[i] = rapdu[i];

  (*dwOutLength) = rapdu_len - 2;

  return DF_OPERATION_OK;

}

LONG SAM_VerifyMAC(SCARDHANDLE hSam, BYTE status, BYTE *pbIn, DWORD dwInLength, BYTE bMacLength, BOOL fKeepStatus)
{
  LONG rc;
  DWORD i;
  BYTE capdu[MAX_DATA_SIZE+5], rapdu[MAX_DATA_SIZE+5];
  DWORD capdu_len = 0;
	DWORD rapdu_len = MAX_DATA_SIZE+5;

  if (dwInLength > 0xFF)
  {
    /* Several frames need to be sent to the SAM */
    /* ----------------------------------------- */
    DWORD dwRemainingLength;
    DWORD index = 0x00;

    /* Construct a buffer, intercaling the status byte (00) between data and CMAC, if needed */
    BYTE *pbBuffer;

    if (fKeepStatus)
    {
      pbBuffer = malloc(dwInLength+1);
    } else
    {
      pbBuffer = malloc(dwInLength);
    }
    if (pbBuffer == NULL)
      return DFCARD_OUT_OF_MEMORY;


    if (fKeepStatus)
    {
      memcpy(pbBuffer, pbIn, dwInLength - bMacLength);
      pbBuffer[dwInLength - bMacLength] = status;
      memcpy(&pbBuffer[dwInLength - bMacLength + 1], &pbIn[dwInLength - bMacLength], bMacLength);
    } else
    {
      memcpy(pbBuffer, pbIn, dwInLength);
    }

    dwRemainingLength = dwInLength;
    if (fKeepStatus)
      dwRemainingLength++;

    while (dwRemainingLength > 0xFF)
    {
      capdu_len = 0;
      capdu[capdu_len++] = 0x80;
      capdu[capdu_len++] = 0x5C;
      capdu[capdu_len++] = 0xAF; //P1
      capdu[capdu_len++] = 00;   //P2
      capdu[capdu_len++] = 0xFF;
      for (i=0; i<0xFF; i++)
        capdu[capdu_len++] = pbBuffer[index++];

#ifdef _DEBUG_SAM_CIPHER
      printf("SAM_VerifyMAC Capdu=");
      for (i=0; i<capdu_len; i++)
        printf("%2.2x", capdu[i]);
      printf("\n");
#endif

      rapdu_len = rapdu_len = MAX_DATA_SIZE+5;

      rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
      if (rc != SCARD_S_SUCCESS)
      {
        free(pbBuffer);
        return rc;
      }

#ifdef _DEBUG_SAM_CIPHER
      printf("Rapdu=");
      for (i=0; i<rapdu_len; i++)
        printf("%2.2x", rapdu[i]);
      printf("\n");
#endif
      dwRemainingLength -= 0xFF;

    }

    /* This is the last APDU  */
    capdu_len = 0;
    capdu[capdu_len++] = 0x80;
    capdu[capdu_len++] = 0x5C;
    capdu[capdu_len++] = 0x00;        //P1
    capdu[capdu_len++] = bMacLength;  //P2
    capdu[capdu_len++] = (BYTE) dwRemainingLength;
    for (i=0; i<dwRemainingLength; i++)
      capdu[capdu_len++] = pbBuffer[index++];

# ifdef _DEBUG_SAM_CIPHER
    printf("SAM_VerifyMAC Capdu=");
    for (i=0; i<capdu_len; i++)
      printf("%2.2x", capdu[i]);
    printf("\n");
# endif

    free(pbBuffer);

    rapdu_len = MAX_DATA_SIZE+5;

    rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
    if (rc != SCARD_S_SUCCESS)
      return rc;

  } else
  {
    /* Only one frame needs to be sent to the SAM */
    /* ------------------------------------------ */
    capdu[capdu_len++] = 0x80;
    capdu[capdu_len++] = 0x5C;
    capdu[capdu_len++] = 0x00; //all data transmitted
    capdu[capdu_len++] = bMacLength; //offset
    if (fKeepStatus)
    {
      capdu[capdu_len++] = (BYTE) dwInLength + 1;
    } else
    {
      capdu[capdu_len++] = (BYTE) dwInLength;
    }
    for( i = 0; i< (int) (dwInLength-bMacLength); i++)
    {
      if (capdu_len<sizeof(capdu))
      {
        capdu[capdu_len++] = pbIn[i];
      } else
      {
        return DFCARD_OVERFLOW;
      }
    }
    if (fKeepStatus)
    {
      if (capdu_len<sizeof(capdu))
      {
        capdu[capdu_len++] = status;
      } else
      {
        return DFCARD_OVERFLOW;
      }
    }
    for (i=dwInLength-bMacLength; i<dwInLength; i++)
    {
      if (capdu_len<sizeof(capdu))
      {
        capdu[capdu_len++] = pbIn[i];
      } else
      {
        return DFCARD_OVERFLOW;
      }
    }

#ifdef _DEBUG_SAM_CIPHER
    printf("SAM_VerifyMAC Capdu=");
    for (i=0; i<capdu_len; i++)
      printf("%2.2x", capdu[i]);
    printf("\n");
#endif

    rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
    if (rc != SCARD_S_SUCCESS)
      return rc;
  }

#ifdef _DEBUG_SAM_CIPHER
  printf("Rapdu=");
  for (i=0; i<rapdu_len; i++)
    printf("%2.2x", rapdu[i]);
  printf("\n");
#endif

  if ((rapdu[rapdu_len-2] != 0x90) || (rapdu[rapdu_len-1] != 0x00))
    return DFCARD_WRONG_MAC;

  return DF_OPERATION_OK;

}

LONG SAM_EncipherData(SCARDHANDLE hSam, BYTE *pbIn, DWORD dwInLength, BYTE offset, BYTE *pbOut, DWORD * dwOutLength)
{
  LONG rc;
  int i;
  BYTE capdu[MAX_DATA_SIZE+6], rapdu[MAX_DATA_SIZE+6];
  int capdu_len = 0;
	int rapdu_len = MAX_DATA_SIZE+6;

  if (dwInLength > 0xF6)
    return DFCARD_ERROR;

  capdu[capdu_len++] = 0x80;
  capdu[capdu_len++] = 0xED;
  capdu[capdu_len++] = 0x00; //all data transmitted
  capdu[capdu_len++] = offset;
  capdu[capdu_len++] = (BYTE) dwInLength;
  for( i = 0; i< (int) dwInLength; i++)
  {
    if (capdu_len < sizeof(capdu))
    {
      capdu[capdu_len++] = pbIn[i];
    } else
    {
      return DFCARD_OVERFLOW;
    }
  }
  if (capdu_len < sizeof(capdu))
  {
    capdu[capdu_len++] = 0x00;
  } else
  {
    return DFCARD_OVERFLOW;
  }

# ifdef _DEBUG_SAM_CIPHER
  printf("Encipher Capdu=");
  for (i=0; i<capdu_len; i++)
    printf("%2.2x", capdu[i]);
  printf("\n");
# endif

  rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
  if (rc != SCARD_S_SUCCESS)
    return rc;

# ifdef _DEBUG_SAM_CIPHER
  printf("Rapdu=");
  for (i=0; i<rapdu_len; i++)
    printf("%2.2x", rapdu[i]);
  printf("\n");
# endif

  if ((rapdu[rapdu_len-2] != 0x90) || (rapdu[rapdu_len-1] != 0x00))
    return SCARD_W_CARD_NOT_AUTHENTICATED;

  if ( (*dwOutLength) < (DWORD) (rapdu_len-2))
    return DFCARD_OVERFLOW;

  for (i=0; i<rapdu_len -2; i++)
    pbOut[i] = rapdu[i];

  (*dwOutLength) = rapdu_len - 2;

  return DF_OPERATION_OK;

}

LONG SAM_DecipherData(SCARDHANDLE hSam, BYTE bCardStatus, DWORD dwResultLength, BYTE *pbCipher, DWORD dwCipherLength, BOOL fSkipStatus, BYTE *pbOut, DWORD * dwOutLength)
{
  LONG rc;
  DWORD i;
  BYTE capdu[MAX_DATA_SIZE+6], rapdu[MAX_DATA_SIZE+6];
  DWORD capdu_len = 0;
	DWORD rapdu_len = MAX_DATA_SIZE+6;


  if (dwCipherLength > 0xFF)
  {
    /* Several frames need to be sent to the SAM */
    /* ----------------------------------------- */
    DWORD dwRemainingLength;
    DWORD index = 0x00;

    BYTE *pbBuffer = NULL;

    if (fSkipStatus)
    {
      pbBuffer = malloc(dwCipherLength+3);
      dwRemainingLength = dwCipherLength+3;
    } else
    {
      pbBuffer = malloc(dwCipherLength+4);
      dwRemainingLength = dwCipherLength+4;
    }
    if (pbBuffer == NULL)
      return DFCARD_OUT_OF_MEMORY;

    /* Construct a buffer, adding 3 bytes for length at the begining + 1 byte of status if needed */
    pbBuffer[0] = (BYTE) (dwResultLength & 0x000000FF);
    dwResultLength>>=8;
    pbBuffer[1] = (BYTE) (dwResultLength & 0x000000FF);
    dwResultLength>>=8;
    pbBuffer[2] = (BYTE) (dwResultLength & 0x000000FF);

    memcpy(&pbBuffer[3], pbCipher, dwCipherLength);
    if (!fSkipStatus)
      pbBuffer[3+dwCipherLength] = bCardStatus;


    while (dwRemainingLength > 0xFF)
    {
      capdu_len = 0;
      capdu[capdu_len++] = 0x80;
      capdu[capdu_len++] = 0xDD;
      capdu[capdu_len++] = 0xAF; //P1
      capdu[capdu_len++] = 00;   //P2
      capdu[capdu_len++] = 0xFF;
      for (i=0; i<0xFF; i++)
        capdu[capdu_len++] = pbBuffer[index++];
      capdu[capdu_len++] = 0x00;

# ifdef _DEBUG_SAM_CIPHER
      printf("SAM_Decipher Capdu=");
      for (i=0; i<capdu_len; i++)
        printf("%2.2x", capdu[i]);
      printf("\n");
# endif

      rapdu_len = rapdu_len = MAX_DATA_SIZE+5;

      rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
      if (rc != SCARD_S_SUCCESS)
      {
        free(pbBuffer);
        return rc;
      }

#ifdef _DEBUG_SAM_CIPHER
      printf("Rapdu=");
      for (i=0; i<rapdu_len; i++)
        printf("%2.2x", rapdu[i]);
      printf("\n");
#endif
      dwRemainingLength -= 0xFF;

    }

    /* This is the last APDU  */
    capdu_len = 0;
    capdu[capdu_len++] = 0x80;
    capdu[capdu_len++] = 0xDD;
    capdu[capdu_len++] = 0x00;  //P1
    capdu[capdu_len++] = 0x00;  //P2
    capdu[capdu_len++] = (BYTE) dwRemainingLength;
    for (i=0; i<dwRemainingLength; i++)
      capdu[capdu_len++] = pbBuffer[index++];
    capdu[capdu_len++] = 0x00;

# ifdef _DEBUG_SAM_CIPHER
    printf("SAM_VerifyMAC Capdu=");
    for (i=0; i<capdu_len; i++)
      printf("%2.2x", capdu[i]);
    printf("\n");
# endif

    free(pbBuffer);

    rapdu_len = rapdu_len = MAX_DATA_SIZE+5;

    rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
    if (rc != SCARD_S_SUCCESS)
      return rc;

  } else
  {
    /* Only one frame needs to be sent to the SAM */
    /* ------------------------------------------ */

    capdu[capdu_len++] = 0x80;
    capdu[capdu_len++] = 0xDD;
    capdu[capdu_len++] = 0x00; //all data transmitted
    capdu[capdu_len++] = 0x00; //offset
    if (fSkipStatus)
    {
      capdu[capdu_len++] = (BYTE) dwCipherLength + 3; /* ResultLength (3 bytes) */
    } else
    {
      capdu[capdu_len++] = (BYTE) dwCipherLength + 1 + 3; /* status + ResultLength (3 bytes) */
    }
    capdu[capdu_len++] = (BYTE) (dwResultLength & 0x000000FF);
    dwResultLength>>=8;
    capdu[capdu_len++] = (BYTE) (dwResultLength & 0x000000FF);
    dwResultLength>>=8;
    capdu[capdu_len++] = (BYTE) (dwResultLength & 0x000000FF);

    for( i = 0; i< (int) dwCipherLength; i++)
    {
      if (capdu_len < sizeof(capdu))
      {
        capdu[capdu_len++] = pbCipher[i];
      } else
      {
        return DFCARD_OVERFLOW;
      }
    }

    if (!fSkipStatus)
    {
      if (capdu_len < sizeof(capdu))
      {
        capdu[capdu_len++] = bCardStatus;
      } else
      {
        return DFCARD_OVERFLOW;
      }
    }

    if (capdu_len < sizeof(capdu))
    {
      capdu[capdu_len++] = 0x00;
    } else
    {
      return DFCARD_OVERFLOW;
    }

  # ifdef _DEBUG_SAM_CIPHER
    printf("Decipher Capdu=");
    for (i=0; i<capdu_len; i++)
      printf("%2.2x", capdu[i]);
    printf("\n");
  # endif

    rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, capdu_len, NULL, rapdu, &rapdu_len);
    if (rc != SCARD_S_SUCCESS)
      return rc;
  }

# ifdef _DEBUG_SAM_CIPHER
  printf("Rapdu=");
  for (i=0; i<rapdu_len; i++)
    printf("%2.2x", rapdu[i]);
  printf("\n");
# endif

  if ((rapdu[rapdu_len-2] != 0x90) || (rapdu[rapdu_len-1] != 0x00))
    return SCARD_W_CARD_NOT_AUTHENTICATED;

  if ( (*dwOutLength) < (DWORD) (rapdu_len-2))
    return DFCARD_OVERFLOW;

  for (i=0; i<rapdu_len -2; i++)
    pbOut[i] = rapdu[i];

  (*dwOutLength) = rapdu_len - 2;

  return DF_OPERATION_OK;

}

#endif


