/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2011 SpringCard SAS - www.springcard.com
   
  springprox_mifare.c
  -------------------

*/
#include "pcsc_helpers.h"
#include "mifare_helpers.h"

/* Keyring */
/* ------- */

/* Mifare transport condition */
const BYTE ACC_COND_TRANSPORT[4]   = { 0x00, 0x00, 0x00, 0x01 };
/* Mifare usage condition (with data blocks, not counters) */
const BYTE ACC_COND_DATA_SECTOR[4] = { 0x04, 0x04, 0x04, 0x03 };

/* Default Mifare transport key */
const BYTE KEY_TRANSPORT[6]   = { 0xFF,0xFF,0xFF,0xFF,0xFF,0xFF };
const BYTE KEY_EMPTY[6]       = { 0x00,0x00,0x00,0x00,0x00,0x00 };

/* The keys used for our tests */
const BYTE KEY_TEST_A[6]      = { 0xAA,0xAB,0xAC,0xAD,0xAE,0xAF };
const BYTE KEY_TEST_B[6]      = { 0xBA,0xBB,0xBC,0xBD,0xBE,0xBF };

/* Constants */
/* --------- */

#define APDU_BUFFERS_SZ 280

#define KEY_LOCATION      0x00  /* We put the keys in volatile memory */

#define IDX_TEST_KEY_A    0x00  /* Index for test key A (type A, offset 0) */
#define IDX_TEST_KEY_B    0x10  /* Index for test key B (type B, offset 0) */
#define IDX_FF_KEY_A      0x01  /* Index for transport key used as key A (type A, offset 1) */
#define IDX_FF_KEY_B      0x11  /* Index for transport key used as key B (type B, offset 1) */
#define IDX_00_KEY_A      0x02  /* Index for empty key used as key A (type A, offset 2) */
#define IDX_00_KEY_B      0x12  /* Index for empty key used as key B (type B, offset 2) */

#ifdef WIN32
void StartInterval(void);
DWORD GetInterval(void);
#endif

/*static BYTE block_count(WORD sector);*/
static BYTE first_block(WORD sector);
static BYTE last_block(WORD sector);
static BYTE data_size(WORD sector);

BOOL select_key_index(BOOL readonly, CARD_FORMAT_T card_format, BYTE *key_index)
{
  switch (card_format)
  {
    case FMT_TRANSPORT :
      printf("\tTransport condition, transport key as key A\n");
      *key_index = IDX_FF_KEY_A;
      break;
    case FMT_TEST_KEYS_A_B :
      if (readonly)
      {
        printf("\tUsing application key A\n");
        *key_index = IDX_TEST_KEY_A;
      } else
      {
        printf("\tUsing application key B\n");
        *key_index = IDX_TEST_KEY_B;
      }
      break;
    case FMT_TEST_KEYS_F_F :
      if (readonly)
      {
        printf("\tUsing transport key as key A\n");
        *key_index = IDX_FF_KEY_A;
      } else
      {
        printf("\tUsing transport key as key B\n");
        *key_index = IDX_FF_KEY_B;
      }
      break;
    default :
      printf("Unallowed value for Format parameter (%d)\n", card_format);
      return FALSE;
  }

  return TRUE;
}

BOOL select_key_value(BOOL readonly, CARD_FORMAT_T card_format, const BYTE **key_value)
{
  switch (card_format)
  {
    case FMT_TRANSPORT :
    case FMT_TEST_KEYS_F_F :
      printf("\tUsing transport key\n");
      *key_value = KEY_TRANSPORT;
      break;
    case FMT_TEST_KEYS_A_B :
      if (readonly)
      {
        printf("\tUsing application key A\n");
        *key_value = KEY_TEST_A;
      } else
      {
        printf("\tUsing application key B\n");
        *key_value = KEY_TEST_B;
      }
      break;
    default :
      printf("Unallowed value for Format parameter (%d)\n", card_format);
      return FALSE;
  }

  return TRUE;
}

/*
 * MifareTest_Standard_Blocks
 * --------------------------
 * Benchmark using standard PC/SC commands (GENERAL_AUTHENTICATE + READ_BINARY
 * + UPDATE_BINARY) at block level.
 */
BOOL MifareTest_Standard_Blocks(SCARDHANDLE hCard, BYTE sectors, BOOL readonly, CARD_FORMAT_T card_format)
{
  BYTE block_data[16];
  WORD sector, block;
  RC_T rc;
    
  BYTE key_index;

  printf("Test : %s, STANDARD mode, BLOCK level\n", readonly ? "READONLY" : "READ/WRITE");

  if (!select_key_index(readonly, card_format, &key_index))
    return FALSE;

#ifdef WIN32
  StartInterval();
#endif

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Test sector %d... \r", sector);

    /* Get authenticated on the sector */
    rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, key_index);
    if (rc != RC_TRUE)
    {
      printf("Authentication on sector %d failed\n", sector);
      return FALSE;
    }

    for (block = first_block(sector); block <= last_block(sector); block++)
    {
      /* Read the block (even if it is a sector trailer) */
      rc = SCardReadBinary(hCard, block, block_data, 16);
      if (rc != RC_TRUE)
      {
        printf("Read on sector %d, block %d failed\n", sector, block);
        return FALSE;
      }

      if (!readonly)
      {
        if ((block != 0) && (block != last_block(sector)))
        {
          /* Write the block (appart sector trailers and read-only block 0) */
          rc = SCardUpdateBinary(hCard, block, block_data, 16);
          if (rc != RC_TRUE)
          {
            printf("Write on sector %d, block %d failed\n", sector, block);
            return FALSE;
          }
        }
      }
    }
  }

  printf("\r");
#ifdef WIN32
  printf("Elapsed time : %ldms\n", GetInterval());
#endif
  return TRUE;
}

/*
 * MifareTest_Standard_BlockDelay
 * ------------------------------
 */
BOOL MifareTest_Standard_BlocksDelay(SCARDHANDLE hCard, BYTE sector, BOOL readonly, CARD_FORMAT_T card_format)
{
  BYTE block_data[16];
  WORD block;
  RC_T rc;
    
  BYTE key_index;

  printf("Test : %s, STANDARD mode, BLOCK level, with DELAY\n", readonly ? "READONLY" : "READ/WRITE");

  if (!select_key_index(readonly, card_format, &key_index))
    return FALSE;

	printf("Test sector %d... \r", sector);

  /* Get authenticated on the sector */
  rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, key_index);
  if (rc != RC_TRUE)
  {
    printf("Authentication on sector %d failed\n", sector);
    return FALSE;
  }

  for (block = first_block(sector); block <= last_block(sector); block++)
  {
    Sleep(1000);

    /* Read the block (even if it is a sector trailer) */
    rc = SCardReadBinary(hCard, block, block_data, 16);
    if (rc != RC_TRUE)
    {
      printf("Read on sector %d, block %d failed\n", sector, block);
      return FALSE;
    }

    if (!readonly)
    {
      Sleep(1000);

      if ((block != 0) && (block != last_block(sector)))
      {
        /* Write the block (appart sector trailers and read-only block 0) */
        rc = SCardUpdateBinary(hCard, block, block_data, 16);
        if (rc != RC_TRUE)
        {
          printf("Write on sector %d, block %d failed\n", sector, block);
          return FALSE;
        }
      }
    }
  }

  printf("\r");

  return TRUE;
}

/*
 * MifareTest_Standard_Sectors
 * ---------------------------
 * Benchmark using standard PC/SC commands (GENERAL_AUTHENTICATE + READ_BINARY
 * + UPDATE_BINARY) at sector level.
 */
BOOL MifareTest_Standard_Sectors(SCARDHANDLE hCard, BYTE sectors, BOOL readonly, CARD_FORMAT_T card_format)
{
  BYTE sector_data[240];
  WORD sector;
  RC_T rc;

  BYTE key_index;
  
  printf("Test : %s, STANDARD mode, SECTOR level\n", readonly ? "READONLY" : "READ/WRITE");

  if (!select_key_index(readonly, card_format, &key_index))
    return FALSE;

#ifdef WIN32
  StartInterval();
#endif

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Test sector %d... \r", sector);

    /* Get authenticated on the sector */
    rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, key_index);
    if (rc != RC_TRUE)
    {
      printf("Authentication on sector %d failed\n", sector);
      return FALSE;
    }

    /* Read the sector (data only) */
    rc = SCardReadBinary(hCard, first_block(sector), sector_data, data_size(sector));
    if (rc != RC_TRUE)
    {
      printf("Read on sector %d failed\n", sector);
      return FALSE;
    }

    /* Write the sector (data only) */
    if (!readonly)
    {
      rc = SCardUpdateBinary(hCard, first_block(sector), sector_data, data_size(sector));
      if (rc != RC_TRUE)
      {
        printf("Write on sector %d failed\n", sector);
        return FALSE;
      }
    }
  }

  printf("\r");
#ifdef WIN32
  printf("Elapsed time : %ldms\n", GetInterval());
#endif
  return TRUE;
}


/*
 * MifareTest_Specific_Blocks_A
 * ----------------------------
 * Benchmark using specific PC/SC commands (MIFARE_CLASSIC_READ
 * + MIFARE_CLASSIC_WRITE) at block level. The reader tries to
 * find the correct key automatically.
 */
BOOL MifareTest_Specific_Blocks_A(SCARDHANDLE hCard, BYTE sectors, BOOL readonly)
{
  BYTE block_data[16];
  WORD sector, block;
  RC_T rc;

  printf("Test : %s, SPECIFIC mode, BLOCK level, AUTO authentication\n", readonly ? "READONLY" : "READ/WRITE");

#ifdef WIN32
  StartInterval();
#endif

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Test sector %d... \r", sector);

    for (block = first_block(sector); block <= last_block(sector); block++)
    {
      /* Read the block (even if it is a sector trailer) */
      rc = SCardMifareClassicRead_Auto(hCard, block, block_data, 16);
      if (rc != RC_TRUE)
      {
        printf("Read on sector %d, block %d failed\n", sector, block);
        return FALSE;
      }

      if (!readonly)
      {
        if ((block != 0) && (block != last_block(sector)))
        {
          /* Write the block (appart sector trailers and read-only block 0) */
          rc = SCardMifareClassicWrite_Auto(hCard, block, block_data, 16);
          if (rc != RC_TRUE)
          {
            printf("Write on sector %d, block %d failed\n", sector, block);
            return FALSE;
          }
        }
      }
    }
  }

  printf("\r");
#ifdef WIN32
  printf("Elapsed time : %ldms\n", GetInterval());
#endif
  return TRUE;
}

/*
 * MifareTest_Specific_Sectors_A
 * -----------------------------
 * Benchmark using specific PC/SC commands (MIFARE_CLASSIC_READ
 * + MIFARE_CLASSIC_WRITE) at sector level. The reader tries to
 * find the correct key automatically.
 */
BOOL MifareTest_Specific_Sectors_A(SCARDHANDLE hCard, BYTE sectors, BOOL readonly)
{
  BYTE sector_data[240];
  WORD sector;
  RC_T rc;
   
  printf("Test : %s, SPECIFIC mode, SECTOR level, AUTO authentication\n", readonly ? "READONLY" : "READ/WRITE");

#ifdef WIN32
  StartInterval();
#endif

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Test sector %d... \r", sector);

    /* Read the sector (data only) */
    rc = SCardMifareClassicRead_Auto(hCard, first_block(sector), sector_data, data_size(sector));
    if (rc != RC_TRUE)
    {
      printf("Read on sector %d failed\n", sector);
      return FALSE;
    }

    if (!readonly)
    {
      /* Write the sector (data only) */
      rc = SCardMifareClassicWrite_Auto(hCard, first_block(sector), sector_data, data_size(sector));
      if (rc != RC_TRUE)
      {
        printf("Write on sector %d failed\n", sector);
        return FALSE;
      }
    }
  }

  printf("\r");
#ifdef WIN32
  printf("Elapsed time : %ldms\n", GetInterval());
#endif
  return TRUE;
}


/*
 * MifareTest_Specific_Blocks_K
 * ----------------------------
 * Benchmark using specific PC/SC commands (MIFARE_CLASSIC_READ
 * + MIFARE_CLASSIC_WRITE) at block level. The value of the key
 * is passed on every function call.
 */
BOOL MifareTest_Specific_Blocks_K(SCARDHANDLE hCard, BYTE sectors, BOOL readonly, CARD_FORMAT_T card_format)
{
  BYTE block_data[16];
  WORD sector, block;
  RC_T rc;

  const BYTE *key_value;

  printf("Test : %s, SPECIFIC mode, BLOCK level, KEY VALUE supplied\n", readonly ? "READONLY" : "READ/WRITE");

  if (!select_key_value(readonly, card_format, &key_value))
    return FALSE;

#ifdef WIN32
  StartInterval();
#endif

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Test sector %d... \r", sector);

    for (block = first_block(sector); block <= last_block(sector); block++)
    {
      /* Read the block (even if it is a sector trailer) */
      rc = SCardMifareClassicRead_KeyValue(hCard, block, block_data, 16, key_value);
      if (rc != RC_TRUE)
      {
        printf("Read on sector %d, block %d failed\n", sector, block);
        return FALSE;
      }

      if (!readonly)
      {
        if ((block != 0) && (block != last_block(sector)))
        {
          /* Write the block (appart sector trailers and read-only block 0) */
          rc = SCardMifareClassicWrite_KeyValue(hCard, block, block_data, 16, key_value);
          if (rc != RC_TRUE)
          {
            printf("Write on sector %d, block %d failed\n", sector, block);
            return FALSE;
          }
        }
      }
    }
  }

  printf("\r");
#ifdef WIN32
  printf("Elapsed time : %ldms\n", GetInterval());
#endif
  return TRUE;
}

/*
 * MifareTest_Specific_Sectors_K
 * -----------------------------
 * Benchmark using specific PC/SC commands (MIFARE_CLASSIC_READ
 * + MIFARE_CLASSIC_WRITE) at sector level. The value of the key
 * is passed on every function call.
 */
BOOL MifareTest_Specific_Sectors_K(SCARDHANDLE hCard, BYTE sectors, BOOL readonly, CARD_FORMAT_T card_format)
{
  BYTE sector_data[240];
  WORD sector;
  RC_T rc;
  
  const BYTE *key_value;
  
  printf("Test : %s, SPECIFIC mode, SECTOR level, KEY VALUE supplied\n", readonly ? "READONLY" : "READ/WRITE");

  if (!select_key_value(readonly, card_format, &key_value))
    return FALSE;

#ifdef WIN32
  StartInterval();
#endif

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Test sector %d... \r", sector);

    /* Read the sector (data only) */
    rc = SCardMifareClassicRead_KeyValue(hCard, first_block(sector), sector_data, data_size(sector), key_value);
    if (rc != RC_TRUE)
    {
      printf("Read on sector %d failed\n", sector);
      return FALSE;
    }

    if (!readonly)
    {
      /* Write the sector (data only) */
      rc = SCardMifareClassicWrite_KeyValue(hCard, first_block(sector), sector_data, data_size(sector), key_value);
      if (rc != RC_TRUE)
      {
        printf("Write on sector %d failed\n", sector);
        return FALSE;
      }
    }
  }

  printf("\r");
#ifdef WIN32
  printf("Elapsed time : %ldms\n", GetInterval());
#endif
  return TRUE;
}

/*
 * MifareTest_Specific_Blocks_I
 * ----------------------------
 * Benchmark using specific PC/SC commands (MIFARE_CLASSIC_READ
 * + MIFARE_CLASSIC_WRITE) at block level. The index of the key
 * is passed on every function call.
 */
BOOL MifareTest_Specific_Blocks_I(SCARDHANDLE hCard, BYTE sectors, BOOL readonly, CARD_FORMAT_T card_format)
{
  BYTE block_data[16];
  WORD sector, block;
  RC_T rc;

  BYTE key_index;

  printf("Test : %s, SPECIFIC mode, BLOCK level, KEY INDEX supplied\n", readonly ? "READONLY" : "READ/WRITE");

  if (!select_key_index(readonly, card_format, &key_index))
    return FALSE;

#ifdef WIN32
  StartInterval();
#endif

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Test sector %d... \r", sector);

    for (block = first_block(sector); block <= last_block(sector); block++)
    {
      /* Read the block (even if it is a sector trailer) */
      rc = SCardMifareClassicRead_KeyIndex(hCard, block, block_data, 16, KEY_LOCATION, key_index);
      if (rc != RC_TRUE)
      {
        if (rc == RC_ACCEPTABLE_ERROR)
        {
          printf("This version of reader's firmware doesn't support this new feature\n");
          return TRUE;
        }
        printf("Read on sector %d, block %d failed\n", sector, block);
        return FALSE;
      }

      if (!readonly)
      {
        if ((block != 0) && (block != last_block(sector)))
        {
          /* Write the block (appart sector trailers and read-only block 0) */
          rc = SCardMifareClassicWrite_KeyIndex(hCard, block, block_data, 16, KEY_LOCATION, key_index);
          if (rc != RC_TRUE)
          {
            if (rc == RC_ACCEPTABLE_ERROR)
            {
              printf("This version of reader's firmware doesn't support this new feature\n");
              return TRUE;
            }
            printf("Write on sector %d, block %d failed\n", sector, block);
            return FALSE;
          }
        }
      }
    }
  }

  printf("\r");
#ifdef WIN32
  printf("Elapsed time : %ldms\n", GetInterval());
#endif
  return TRUE;
}

/*
 * MifareTest_Specific_Sectors_K
 * -----------------------------
 * Benchmark using specific PC/SC commands (MIFARE_CLASSIC_READ
 * + MIFARE_CLASSIC_WRITE) at sector level. The index of the key
 * is passed on every function call.
 */
BOOL MifareTest_Specific_Sectors_I(SCARDHANDLE hCard, BYTE sectors, BOOL readonly, CARD_FORMAT_T card_format)
{
  BYTE sector_data[240];
  WORD sector;
  RC_T rc;

  BYTE key_index;
  
  printf("Test : %s, SPECIFIC mode, SECTOR level, KEY INDEX supplied\n", readonly ? "READONLY" : "READ/WRITE");

  if (!select_key_index(readonly, card_format, &key_index))
    return FALSE;

#ifdef WIN32
  StartInterval();
#endif

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Test sector %d... \r", sector);

    /* Read the sector (data only) */
    rc = SCardMifareClassicRead_KeyIndex(hCard, first_block(sector), sector_data, data_size(sector), KEY_LOCATION, key_index);
    if (rc != RC_TRUE)
    {
      if (rc == RC_ACCEPTABLE_ERROR)
      {
        printf("This version of reader's firmware doesn't support this new feature\n");
        return TRUE;
      }
      printf("Read on sector %d failed\n", sector);
      return FALSE;
    }

    /* Write the sector (data only) */
    if (!readonly)
    {
      rc = SCardMifareClassicWrite_KeyIndex(hCard, first_block(sector), sector_data, data_size(sector), KEY_LOCATION, key_index);
      if (rc != RC_TRUE)
      {
        if (rc == RC_ACCEPTABLE_ERROR)
        {
          printf("This version of reader's firmware doesn't support this new feature\n");
          return TRUE;
        }
        printf("Write on sector %d failed\n", sector);
        return FALSE;
      }
    }
  }

  printf("\r");
#ifdef WIN32
  printf("Elapsed time : %ldms\n", GetInterval());
#endif
  return TRUE;
}




BOOL prefer_specific = FALSE;

/**f* PCSC/SCardLoadKey
 *
 * NAME
 *   SCardLoadKey
 *
 * DESCRIPTION
 *   Load a Mifare authentication key into the reader.
 *
 * INPUTS
 *   SCARDHANDLE hCard       : handle to the PC/SC smartcard
 *   BYTE key_location       : set to 0x00 to put the key in volatile memory
 *                             set to 0x20 to put the key in non-volatile memory
 *   BYTE key_index          : 0x00 to 0x0F are 'A' keys
 *                             0x10 to 0x1F are 'B' keys
 *   const BYTE key_value[]  : value of the key
 *   BYTE key_size           : must be 6 for a Mifare key
 *
 * OUTPUTS
 *   OK or error
 *
 * WARNING
 *   Non-volatile keys are stored in the E2PROM of the RC chipset.
 *   The E2PROM has a limited write endurance, therefore you mustn't overwrite
 *   the permanent keys "too often".
 *
 *   Typically, load your application keys in E2PROM once, when installating the
 *   product install, and then forget them...
 *
 * SEE ALSO
 *   SCardGeneralAuthenticate
 *
 **/
RC_T SCardLoadKey(SCARDHANDLE hCard, BYTE key_location, BYTE key_index, const BYTE key_value[], BYTE key_size)
{
  BYTE  pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;

  if (key_value == NULL) return RC_PARAMETER_ERROR;
  if (key_size + 5 > APDU_BUFFERS_SZ) return RC_PARAMETER_ERROR;
  
  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0x82;
  pbSendBuffer[2] = key_location;
  pbSendBuffer[3] = key_index;
  pbSendBuffer[4] = key_size;

  memcpy(&pbSendBuffer[5], key_value, key_size);

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, key_size+5, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(LOAD_KEY) error %08lX\n", rc);
		return RC_FATAL_ERROR;
	}

  if (pcbRecvLength != 2)
  {
    printf("SCardTransmit(LOAD_KEY) returned %ld bytes\n", pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pbRecvBuffer[0] != 0x90) || (pbRecvBuffer[1] != 0x00))
  {
    printf("LOAD_KEY failed, SW=%02X%02X\n", pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FATAL_ERROR;
  }

  return RC_TRUE;
}

/**f* PCSC/SCardGeneralAuthenticate
 *
 * NAME
 *   SCardGeneralAuthenticate
 *
 * DESCRIPTION
 *   Perform Mifare authentication, using one of the keys already loaded in the
 *   reader, on the specified block.
 *
 * INPUTS
 *   SCARDHANDLE hCard       : handle to the PC/SC smartcard
 *   WORD address            : block number to get authenticated on
 *                             (remember: the __block__, not the __sector__)
 *   BYTE key_location       : as specified in SCardLoadKey
 *   BYTE key_index          : as specified in SCardLoadKey
 *
 * OUTPUTS
 *   OK or error
 *
 * SEE ALSO
 *   SCardLoadKey
 *
 **/
RC_T SCardGeneralAuthenticate(SCARDHANDLE hCard, WORD address, BYTE key_location, BYTE key_index)
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;
  
  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0x86;
  pbSendBuffer[2] = 0x00;
  pbSendBuffer[3] = 0x00;
  pbSendBuffer[4] = 0x05;

  pbSendBuffer[5] = 0x01;
  pbSendBuffer[6] = (BYTE) (address >> 8);
  pbSendBuffer[7] = (BYTE) address;
  pbSendBuffer[8] = key_location;
  pbSendBuffer[9] = key_index;

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 10, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(GENERAL_AUTHENTICATE) error %08lX\n", rc);
		return RC_FATAL_ERROR;
	}

  if (pcbRecvLength != 2)
  {
    printf("SCardTransmit(GENERAL_AUTHENTICATE) returned %ld bytes\n", pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pbRecvBuffer[0] != 0x90) || (pbRecvBuffer[1] != 0x00))
  {
    if ((pbRecvBuffer[0] != 0x69) || (pbRecvBuffer[1] != 0x82))
      printf("GENERAL_AUTHENTICATE failed, SW=%02X%02X\n", pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FALSE;
  }

  return RC_TRUE;
}

/**f* PCSC/SCardReadBinary
 *
 * NAME
 *   SCardReadBinary
 *
 * DESCRIPTION
 *   Read data from a contactless card.
 *
 * INPUTS
 *   SCARDHANDLE hCard       : handle to the PC/SC smartcard
 *   WORD address            : start address
 *   BYTE data[]             : buffer to receive the data
 *   BYTE size               : size of buffer
 *
 * OUTPUTS
 *   OK or error
 *
 **/
RC_T SCardReadBinary(SCARDHANDLE hCard, WORD address, BYTE data[], BYTE size)
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;

  if (prefer_specific)
    return SCardMifareClassicRead_Auto(hCard, address, data, size);
  
  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0xB0;
  pbSendBuffer[2] = (BYTE) (address >> 8);
  pbSendBuffer[3] = (BYTE) address;
  pbSendBuffer[4] = size;

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 5, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(READ_BINARY) error %08lX\n", rc);
		return RC_FATAL_ERROR;
	}

  if ((pcbRecvLength != (DWORD) (2+size)) && (pcbRecvLength != 2))
  {
    printf("SCardTransmit(READ_BINARY) returned %ld bytes\n", pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pcbRecvLength == 2) || (pbRecvBuffer[pcbRecvLength-2] != 0x90) || (pbRecvBuffer[pcbRecvLength-1] != 0x00))
  {
    if ((pbRecvBuffer[pcbRecvLength-2] == 0x6F) && (pbRecvBuffer[pcbRecvLength-1] == 0x0A))
    {
      /* Version 1-48 or earlier bug */
      if (SCardMifareClassicRead_Auto(hCard, address, data, size) == RC_TRUE)
      {
        prefer_specific = TRUE;
        return RC_TRUE;
      }
    }

    printf("READ_BINARY failed, SW=%02X%02X\n", pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FATAL_ERROR;
  }

  if (data != NULL)
    memcpy(data, pbRecvBuffer, size);

  return RC_TRUE;
}

/**f* PCSC/SCardUpdateBinary
 *
 * NAME
 *   SCardUpdateBinary
 *
 * DESCRIPTION
 *   Write data to a contactless card.
 *
 * INPUTS
 *   SCARDHANDLE hCard       : handle to the PC/SC smartcard
 *   WORD address            : start address
 *   BYTE data[]             : buffer holding the data
 *   BYTE size               : size of buffer
 *
 * OUTPUTS
 *   OK or error
 *
 **/
RC_T SCardUpdateBinary(SCARDHANDLE hCard, WORD address, const BYTE data[], BYTE size)
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;

  if (prefer_specific)
    return SCardMifareClassicWrite_Auto(hCard, address, data, size);

  if (data == NULL) return RC_PARAMETER_ERROR;
  if (size + 5 > APDU_BUFFERS_SZ) return RC_PARAMETER_ERROR;
  
  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0xD6;
  pbSendBuffer[2] = (BYTE) (address >> 8);
  pbSendBuffer[3] = (BYTE) address;
  pbSendBuffer[4] = size;

  memcpy(&pbSendBuffer[5], data, size);

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 5+size, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(UPDATE_BINARY %04X) error %08lX\n", address, rc);
		return RC_FATAL_ERROR;
	}

  if (pcbRecvLength != 2)
  {
    printf("SCardTransmit(UPDATE_BINARY %04X) returned %ld bytes\n", address, pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pbRecvBuffer[0] != 0x90) || (pbRecvBuffer[1] != 0x00))
  {
    if ((pbRecvBuffer[pcbRecvLength-2] == 0x6F) && (pbRecvBuffer[pcbRecvLength-1] == 0x0F))
    {
      /* Version 1-48 or earlier bug */
      if (SCardMifareClassicWrite_Auto(hCard, address, data, size) == RC_TRUE)
      {
        prefer_specific = TRUE;
        return RC_TRUE;
      }
    }

    printf("UPDATE_BINARY %04X failed, SW=%02X%02X\n", address, pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FALSE;
  }

  return RC_TRUE;
}


RC_T SCardMifareClassicRead_Auto(SCARDHANDLE hCard, WORD address, BYTE data[], BYTE size)
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;
  
  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0xF3;
  pbSendBuffer[2] = (BYTE) (address >> 8);
  pbSendBuffer[3] = (BYTE) address;
  pbSendBuffer[4] = size;

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 5, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(MIFARE_CLASSIC_READ) error %08lX\n", rc);
		return RC_FATAL_ERROR;
	}

  if ((pcbRecvLength != (DWORD) (2+size)) && (pcbRecvLength != 2))
  {
    printf("SCardTransmit(MIFARE_CLASSIC_READ) returned %ld bytes\n", pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pcbRecvLength == 2) || (pbRecvBuffer[pcbRecvLength-2] != 0x90) || (pbRecvBuffer[pcbRecvLength-1] != 0x00))
  {
    printf("MIFARE_CLASSIC_READ failed, SW=%02X%02X\n", pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FATAL_ERROR;
  }

  if (data != NULL)
    memcpy(data, pbRecvBuffer, size);

  return RC_TRUE;
}

RC_T SCardMifareClassicRead_KeyIndex(SCARDHANDLE hCard, WORD address, BYTE data[], BYTE size, BYTE key_location, BYTE key_index)
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;

  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0xF3;
  pbSendBuffer[2] = (BYTE) (address >> 8);
  pbSendBuffer[3] = (BYTE) address;
  pbSendBuffer[4] = 2;
  pbSendBuffer[5] = key_location;
  pbSendBuffer[6] = key_index;
  pbSendBuffer[7] = size;

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 8, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(MIFARE_CLASSIC_READ) error %08lX\n", rc);
		return RC_FATAL_ERROR;
	}

  if ((pcbRecvLength != (DWORD) (2+size)) && (pcbRecvLength != 2))
  {
    printf("SCardTransmit(MIFARE_CLASSIC_READ) returned %ld bytes\n", pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pcbRecvLength == 2) || (pbRecvBuffer[pcbRecvLength-2] != 0x90) || (pbRecvBuffer[pcbRecvLength-1] != 0x00))
  {
    if ((pbRecvBuffer[0] == 0x67) && (pbRecvBuffer[1] == 0x00))
    {
      /* Acceptable value, old versions don't support this */
      return RC_ACCEPTABLE_ERROR;
    }
    printf("MIFARE_CLASSIC_READ failed, SW=%02X%02X\n", pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FATAL_ERROR;
  }

  if (data != NULL)
    memcpy(data, pbRecvBuffer, size);

  return RC_TRUE;
}

RC_T SCardMifareClassicRead_KeyValue(SCARDHANDLE hCard, WORD address, BYTE data[], BYTE size, const BYTE key_value[6])
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;

  if (key_value == NULL) return RC_PARAMETER_ERROR;
  
  pbSendBuffer[0]  = 0xFF;
  pbSendBuffer[1]  = 0xF3;
  pbSendBuffer[2] = (BYTE) (address >> 8);
  pbSendBuffer[3] = (BYTE) address;
  pbSendBuffer[4]  = 6;
  memcpy(&pbSendBuffer[5], key_value, 6);
  pbSendBuffer[11] = size;

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 12, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(MIFARE_CLASSIC_READ) error %08lX\n", rc);
		return RC_FATAL_ERROR;
	}

  if ((pcbRecvLength != (DWORD) (2+size)) && (pcbRecvLength != 2))
  {
    printf("SCardTransmit(MIFARE_CLASSIC_READ) returned %ld bytes\n", pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pcbRecvLength == 2) || (pbRecvBuffer[pcbRecvLength-2] != 0x90) || (pbRecvBuffer[pcbRecvLength-1] != 0x00))
  {
    printf("MIFARE_CLASSIC_READ failed, SW=%02X%02X\n", pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FATAL_ERROR;
  }

  if (data != NULL)
    memcpy(data, pbRecvBuffer, size);

  return RC_TRUE;
}

RC_T SCardMifareClassicWrite_Auto(SCARDHANDLE hCard, WORD address, const BYTE data[], BYTE size)
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;

  if (data == NULL) return RC_PARAMETER_ERROR;
  if (size + 5 > APDU_BUFFERS_SZ) return RC_PARAMETER_ERROR;
  
  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0xF4;
  pbSendBuffer[2] = (BYTE) (address >> 8);
  pbSendBuffer[3] = (BYTE) address;
  pbSendBuffer[4] = size;

  memcpy(&pbSendBuffer[5], data, size);

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 5+size, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(MIFARE_CLASSIC_WRITE %04X) error %08lX\n", address, rc);
		return RC_FATAL_ERROR;
	}

  if (pcbRecvLength != 2)
  {
    printf("SCardTransmit(MIFARE_CLASSIC_WRITE %04X) returned %ld bytes\n", address, pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pbRecvBuffer[0] != 0x90) || (pbRecvBuffer[1] != 0x00))
  {
    printf("MIFARE_CLASSIC_WRITE %04X failed, SW=%02X%02X\n", address, pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FALSE;
  }

  return RC_TRUE;
}

RC_T SCardMifareClassicWrite_KeyIndex(SCARDHANDLE hCard, WORD address, const BYTE data[], BYTE size, BYTE key_location, BYTE key_index)
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;

  if (data == NULL) return RC_PARAMETER_ERROR;
  if (size + 5 + 2 > APDU_BUFFERS_SZ) return RC_PARAMETER_ERROR;
  
  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0xF4;
  pbSendBuffer[2] = (BYTE) (address >> 8);
  pbSendBuffer[3] = (BYTE) address;
  pbSendBuffer[4] = size + 2;

  memcpy(&pbSendBuffer[5], data, size);
  pbSendBuffer[5 + size] = key_location;
  pbSendBuffer[6 + size] = key_index;

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 7+size, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(MIFARE_CLASSIC_WRITE %04X) error %08lX\n", address, rc);
		return RC_FATAL_ERROR;
	}

  if (pcbRecvLength != 2)
  {
    printf("SCardTransmit(MIFARE_CLASSIC_WRITE %04X) returned %ld bytes\n", address, pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pbRecvBuffer[0] != 0x90) || (pbRecvBuffer[1] != 0x00))
  {
    if ((pbRecvBuffer[0] == 0x67) && (pbRecvBuffer[1] == 0x00))
    {
      /* Acceptable value, old versions don't support this */
      return RC_ACCEPTABLE_ERROR;
    }
    printf("MIFARE_CLASSIC_WRITE %04X failed, SW=%02X%02X\n", address, pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FALSE;
  }

  return RC_TRUE;
}

RC_T SCardMifareClassicWrite_KeyValue(SCARDHANDLE hCard, WORD address, const BYTE data[], BYTE size, const BYTE key_value[6])
{
  BYTE pbSendBuffer[APDU_BUFFERS_SZ];
  BYTE  pbRecvBuffer[APDU_BUFFERS_SZ];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  LONG rc;

  if (data == NULL) return RC_PARAMETER_ERROR;
  if (key_value == NULL) return RC_PARAMETER_ERROR;
  if (size + 5 + 6 > APDU_BUFFERS_SZ) return RC_PARAMETER_ERROR;
  
  pbSendBuffer[0] = 0xFF;
  pbSendBuffer[1] = 0xF4;
  pbSendBuffer[2] = (BYTE) (address >> 8);
  pbSendBuffer[3] = (BYTE) address;
  pbSendBuffer[4] = size + 6;

  memcpy(&pbSendBuffer[5], data, size);
  memcpy(&pbSendBuffer[5 + size], key_value, 6);

  rc = SCardTransmit(hCard, SCARD_PCI_T1, pbSendBuffer, 5+size+6, NULL, pbRecvBuffer, &pcbRecvLength);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(MIFARE_CLASSIC_WRITE %04X) error %08lX\n", address, rc);
		return RC_FATAL_ERROR;
	}

  if (pcbRecvLength != 2)
  {
    printf("SCardTransmit(MIFARE_CLASSIC_WRITE %04X) returned %ld bytes\n", address, pcbRecvLength);
    return RC_FATAL_ERROR;
  }

  if ((pbRecvBuffer[0] != 0x90) || (pbRecvBuffer[1] != 0x00))
  {
    printf("MIFARE_CLASSIC_WRITE %04X failed, SW=%02X%02X\n", address, pbRecvBuffer[0], pbRecvBuffer[1]);
    return RC_FALSE;
  }

  return RC_TRUE;
}



/*
 * MifareTest_LoadKeys
 * -------------------
 * Load the keys we are to use into reader's memory
 */
RC_T MifareTest_LoadKeys(SCARDHANDLE hCard)
{
  RC_T rc;
  
  rc = SCardLoadKey(hCard, KEY_LOCATION, IDX_TEST_KEY_A, KEY_TEST_A, sizeof(KEY_TEST_A));
  if (rc != RC_TRUE)
  {
    printf("Failed to load test key A\n");
    return rc;
  }

  rc = SCardLoadKey(hCard, KEY_LOCATION, IDX_TEST_KEY_B, KEY_TEST_B, sizeof(KEY_TEST_B));
  if (rc != RC_TRUE)
  {
    printf("Failed to load test key B\n");
    return rc;
  }

  rc = SCardLoadKey(hCard, KEY_LOCATION, IDX_FF_KEY_A, KEY_TRANSPORT, sizeof(KEY_TRANSPORT));
  if (rc != RC_TRUE)
  {
    printf("Failed to load transport key A\n");
    return rc;
  }

  rc = SCardLoadKey(hCard, KEY_LOCATION, IDX_FF_KEY_B, KEY_TRANSPORT, sizeof(KEY_TRANSPORT));
  if (rc != RC_TRUE)
  {
    printf("Failed to load transport key B\n");
    return rc;
  }

  rc = SCardLoadKey(hCard, KEY_LOCATION, IDX_00_KEY_A, KEY_EMPTY, sizeof(KEY_EMPTY));
  if (rc != RC_TRUE)
  {
    printf("Failed to load empty key A\n");
    return rc;
  }

  rc = SCardLoadKey(hCard, KEY_LOCATION, IDX_00_KEY_B, KEY_EMPTY, sizeof(KEY_EMPTY));
  if (rc != RC_TRUE)
  {
    printf("Failed to load empty key B\n");
    return rc;
  }

  return RC_TRUE;
}

/*
 * SCardMifareIsTransport
 * ----------------------
 * Verify that the card is in transport condition (KEY_TRANSPORT OK on every sector,
 * all data are 0x00, writing is possible with KEY_TRANSPORT)
 */
RC_T MifareTest_IsTransport(SCARDHANDLE hCard, BYTE sectors)
{
  BYTE data[240];
  BYTE sector, i;
  RC_T rc;

  for (sector=0; sector<sectors; sector++)
  {
	  printf("\rAnalyze sector %d... ", sector);

    /* KEY_TRANSPORT must be OK */
    rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, IDX_FF_KEY_A);
    if (rc == RC_FALSE)
    {
      printf("Sector %d : authentication with transport key failed\n", sector);
      return rc;
    }
    if (rc != RC_TRUE)
    {
      printf("Sector %d : ERROR during authentication with transport key\n", sector);
      return rc;
    }

    /* Read 3 blocks */
    rc = SCardReadBinary(hCard, first_block(sector), data, data_size(sector));
    if (rc == RC_FALSE)
    {
      printf("Sector %d : read with transport key failed\n", sector);
      return rc;
    }
    if (rc != RC_TRUE)
    {
      printf("Sector %d : ERROR while reading with transport key\n", sector);
      return rc;
    }

    /* Check that the blocks are all blanks */
    for (i=sector?0:16; i<data_size(sector); i++)
    {      
      if (data[i] != 0x00)
      {
        printf("data[%d] = %02X\n", i, data[i]); 
        break;
      } 
    }

    if (i < data_size(sector))
    {
      printf("Sector %d is not blank\n", sector);
      return RC_FALSE;
    }

    /* Re-write (1 block is enough and faster, don't write block 0 of sector 0) */
    rc = SCardUpdateBinary(hCard, (BYTE) (first_block(sector) + 1), &data[16], 16);
    if (rc == RC_FALSE)
    {
      printf("Sector %d : write with transport key failed\n", sector);
      return rc;
    }
    if (rc != RC_TRUE)
    {
      printf("Sector %d : ERROR while writing with transport key\n", sector);
      return rc;
    }
  }

  /* OK, card is actually virgin ! */
  printf("\r");
  return RC_TRUE;
}

/*
 * SCardMifareFormatTransport
 * --------------------------
 * Put back the card in transport condition
 */
RC_T MifareTest_Format(SCARDHANDLE hCard, BYTE sectors, CARD_FORMAT_T card_format)
{
  BYTE data[240];
  BYTE trailer[16];
  BYTE sector;
  RC_T rc;
  BYTE key_index = 0xFF;

  /* We'll write blank data */
  memset(data, 0x00, sizeof(data));

  /* We'll format every sector this way : */
  switch (card_format)
  {
    case FMT_TRANSPORT :
      key_index = IDX_FF_KEY_A;
      MakeMifareSectorTrailer(trailer, KEY_TRANSPORT, KEY_EMPTY, ACC_COND_TRANSPORT);
      break;
    case FMT_TEST_KEYS_A_B :
      key_index = IDX_TEST_KEY_B;
      MakeMifareSectorTrailer(trailer, KEY_TEST_A, KEY_TEST_B, ACC_COND_DATA_SECTOR);
      break;
    case FMT_TEST_KEYS_F_F :
      key_index = IDX_FF_KEY_A;
      MakeMifareSectorTrailer(trailer, KEY_TRANSPORT, KEY_TRANSPORT, ACC_COND_DATA_SECTOR);
      break;
    default :
      printf("Invalid parameter to Format function\n");
      return FALSE;
  }

  for (sector=0; sector<sectors; sector++)
  {
	  printf("Format sector %d... \r", sector);

    /* Let's get authenticated */
    rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, IDX_FF_KEY_B);
    if (rc == RC_FALSE)
      rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, IDX_FF_KEY_A);
    if (rc == RC_FALSE)
      rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, IDX_TEST_KEY_B);
    if (rc == RC_FALSE)
      rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, IDX_00_KEY_B);
    if (rc == RC_FALSE)
    {
      printf("Sector %d : authentication on block %d failed, don't know the key !\n", sector, last_block(sector));
      return rc;
    }
    if (rc != RC_TRUE)
    {
      printf("Sector %d : ERROR during authentication\n", sector);
      return rc;
    }

    /* Write the transport key (+ transport access conditions) in sector trailer */
    rc = SCardUpdateBinary(hCard, last_block(sector), trailer, 16);
    if (rc == RC_FALSE)
    {
      printf("Sector %d : write sector trailer failed\n", sector);
      return rc;
    }
    if (rc != RC_TRUE)
    {
      printf("Sector %d : ERROR while writing sector trailer\n", sector);
      return rc;
    }

    /* Get authenticated again */
    rc = SCardGeneralAuthenticate(hCard, last_block(sector), KEY_LOCATION, key_index);
    if (rc == RC_FALSE)
    {
      printf("Sector %d : (new) authentication failed\n", sector);
      return rc;
    }
    if (rc != RC_TRUE)
    {
      printf("Sector %d : ERROR (new) authentication failed\n", sector);
      return rc;
    }

    /* Write blank data */
    rc = SCardUpdateBinary(hCard, first_block(sector), data, data_size(sector));
    if (rc == RC_FALSE)
    {
      printf("Sector %d : write blank data failed\n", sector);
      return rc;
    }
    if (rc != RC_TRUE)
    {
      printf("Sector %d : ERROR while writing blank data\n", sector);
      return rc;
    }
  }

  /* OK */
  printf("\r");
  return RC_TRUE;
}



#define Bit2(a) ((a&0x04)?1:0)
#define Bit1(a) ((a&0x02)?1:0)
#define Bit0(a) ((a&0x01)?1:0)

void MakeMifareSectorTrailer(BYTE trailer[16], const BYTE key_a[6], const BYTE key_b[6], const BYTE ac[4])
{
  BYTE C3, C2, C1;

  if (trailer == NULL) return;

  memset(trailer, 0xFF, 16);

  /* Put the 2 keys */
  if (key_a != NULL)
    memcpy(&trailer[0], key_a, 6);

  if (key_b != NULL)
    memcpy(&trailer[10], key_b, 6);

  /* Build the access conditions group */  
  C3  = Bit0(ac[3]);
  C3 <<= 1;
  C3 |= Bit0(ac[2]);
  C3 <<= 1;
  C3 |= Bit0(ac[1]);
  C3 <<= 1;
  C3 |= Bit0(ac[0]);

  C2  = Bit1(ac[3]);
  C2 <<= 1;
  C2 |= Bit1(ac[2]);
  C2 <<= 1;
  C2 |= Bit1(ac[1]);
  C2 <<= 1;
  C2 |= Bit1(ac[0]);

  C1  = Bit2(ac[3]);
  C1 <<= 1;
  C1 |= Bit2(ac[2]);
  C1 <<= 1;
  C1 |= Bit2(ac[1]);
  C1 <<= 1;
  C1 |= Bit2(ac[0]);

  trailer[6] = (~(C2 << 4) & 0xF0) | (~(C1) & 0x0F);
  trailer[7] = ((C1 << 4) & 0xF0) | (~(C3) & 0x0F);
  trailer[8] = ((C3 << 4) & 0xF0) | (C2 & 0x0F);
  trailer[9] = 0x00;
}


#ifdef WIN32

static LARGE_INTEGER Before = { 0 };

void StartInterval(void)
{
  QueryPerformanceCounter(&Before);
}

DWORD GetInterval(void)
{
  DWORD r;
  LARGE_INTEGER Now, Freq;

  QueryPerformanceCounter(&Now);

  QueryPerformanceFrequency(&Freq);
  Freq.QuadPart /= 1000;

  r = (DWORD) ( (Now.QuadPart - Before.QuadPart) / Freq.QuadPart );
  return r;
}

#endif


/*
static BYTE block_count(WORD sector)
{
  if (sector < 32) return 4;
	if (sector < 40) return 16;
	return 0;
}
*/

static BYTE first_block(WORD sector)
{
  if (sector < 32) return 4 * sector;
	if (sector < 40) return (16 * (sector - 32)) + 128;
	return 0;
}

static BYTE last_block(WORD sector)
{
  if (sector < 32) return (4 * sector) + 3;
	if (sector < 40) return (16 * (sector - 32)) + 128 + 15;
	return 0;
}

static BYTE data_size(WORD sector)
{
  if (sector < 32) return 48;
	if (sector < 40) return 240;
	return 0;
}