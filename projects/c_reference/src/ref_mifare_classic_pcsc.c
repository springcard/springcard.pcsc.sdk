/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2015 SpringCard SAS - www.springcard.com
   
  ref_mifare_pcsc.c
  -----------------

*/
#include "pcsc_helpers.h"
#include "mifare_helpers.h"

static const BYTE PCSC_MIFARE_ATR_BASE[] = { 0x3B, 0x8F, 0x80, 0x01, 0x80, 0x4F, 0x0C, 0xA0, 0x00, 0x00, 0x03, 0x06, 0x03 };

static const BYTE PCSC_MIFARE_CLASSIC_1K_NN[]  = { 0x00, 0x01 };
static const BYTE PCSC_MIFARE_CLASSIC_4K_NN[]  = { 0x00, 0x02 };

static const BYTE PCSC_MIFARE_PLUS_SL1_2K_NN[] = { 0x00, 0x36 };
static const BYTE PCSC_MIFARE_PLUS_SL1_4K_NN[] = { 0x00, 0x37 };

/* Test function */
BOOL MifareTestCore(SCARDCONTEXT hContext, const char *szReader, const BYTE atr[], DWORD atrlen);

BYTE gbSectors = 0;
BOOL gbOverwrite = FALSE;

int main(int argc, char **argv)
{
	SCARDCONTEXT      hContext;
  BOOL              bContextOK = FALSE;
	LPSTR             szReaders = NULL;
  DWORD             dwReadersSz;
	LPTSTR            pReader;
  SCARD_READERSTATE rgscState;

  int i;
	LONG   rc;

  for (i=1; i<argc; i++)
  {
	  if (!strcmp(argv[1], "-f"))
	    gbOverwrite = TRUE;
	  if (!strcmp(argv[i], "-1K"))
	    gbSectors = 16;
	  if (!strcmp(argv[i], "-2K"))
	    gbSectors = 32;
	  if (!strcmp(argv[i], "-4K"))
	    gbSectors = 40;
  }

  /*
   * Welcome message, check parameters
   * ---------------------------------
   */
  printf("\n");
  printf("SpringCard PC/SC SDK : Mifare Classic Test and Benchmark\n");
  printf("--------------------------------------------------------\n");
  printf("v.150203 (c) SpringCard SAS 2008-2015\n");
	printf("Press <Ctrl+C> to exit\n\n");

	/*
   * Get a handle to the PC/SC resource manager
   */
	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext error %08lX\n", rc);
		goto leave;
	}
  bContextOK = TRUE;

  /*
   * Get the list of available readers
   */
  dwReadersSz = SCARD_AUTOALLOCATE;
	rc = SCardListReaders(hContext,
                        NULL,                /* Any group */
                        (LPTSTR) &szReaders, /* Diabolic cast for buffer auto-allocation */
                        &dwReadersSz);       /* Beg for auto-allocation */
	  
	if (rc == SCARD_E_NO_READERS_AVAILABLE)
	{
		/* No reader at all */
		printf("No PC/SC reader\n");
    goto leave;
	}
  
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardListReaders error %08lX\n",rc);
		goto leave;
	}

  /*
   * Loop withing reader(s) to find a Mifare card
   */ 
  pReader = szReaders;
	while (*pReader != '\0')
	{
    /* Got a reader name */
    printf("\nPC/SC reader '%s' :\n", pReader);

    /* Get status */
    rgscState.szReader = pReader;
    rgscState.dwCurrentState = SCARD_STATE_UNAWARE;
 		rc = SCardGetStatusChange(hContext,
			                        0,
			                        &rgscState,
			                        1);	

    if (rc != SCARD_S_SUCCESS)
	  {
      printf("SCardGetStatusChange error %08lX\n", rc);
		  goto leave;
	  }

    /* Show reader's status */
    if (rgscState.dwEventState & SCARD_STATE_IGNORE)
      printf("\tIgnore this reader\n");

    if (rgscState.dwEventState & SCARD_STATE_UNKNOWN)
      printf("\tReader unknown\n");

    if (rgscState.dwEventState & SCARD_STATE_UNAVAILABLE)
      printf("\tStatus unavailable\n");

    if (rgscState.dwEventState & SCARD_STATE_EMPTY)
      printf("\tNo card in the reader\n");

    if (rgscState.dwEventState & SCARD_STATE_PRESENT)
      printf("\tCard present\n");

    /* Show ATR (if some) */
    if (rgscState.cbAtr)
		{
      BYTE i;

			printf("\tCard ATR: ");
			for (i=0; i<rgscState.cbAtr; i++)
				printf("%02X", rgscState.rgbAtr[i]);
      printf("\n");
    }

    /* Show card's status */
    if (rgscState.dwEventState & SCARD_STATE_ATRMATCH)
      printf("\tATR match\n");

    if (rgscState.dwEventState & SCARD_STATE_INUSE)
      printf("\tCard (or reader) in use\n");

    if (rgscState.dwEventState & SCARD_STATE_EXCLUSIVE)
      printf("\tCard (or reader) reserved for exclusive use\n");

    if (rgscState.dwEventState & SCARD_STATE_MUTE)
      printf("\tCard is mute\n");

    if ( (rgscState.dwEventState & SCARD_STATE_PRESENT)
     && !(rgscState.dwEventState & SCARD_STATE_MUTE)
     && !(rgscState.dwEventState & SCARD_STATE_INUSE)
     && !(rgscState.dwEventState & SCARD_STATE_EXCLUSIVE))
    {
      /* Call test core function */
      MifareTestCore(hContext, pReader, rgscState.rgbAtr, rgscState.cbAtr);
    }

    /* Jump to next entry in multi-string array */
		pReader += strlen(pReader) + 1;
	}

  printf("Exiting...\n");

leave:
  /* Free the list of readers  */
  if (szReaders != NULL)
  {
    SCardFreeMemory(hContext, szReaders);
    szReaders = NULL;
  }

  if (bContextOK)
  {      
	  SCardReleaseContext(hContext);
  }

  return EXIT_SUCCESS;
}

BOOL MifareTestStandard(SCARDHANDLE hCard, BYTE sectors, CARD_FORMAT_T format)
{
  /* Standard commands, block level. */
  if (!MifareTest_Standard_Blocks(hCard, sectors, TRUE, format))
    return FALSE;
  if (!MifareTest_Standard_Blocks(hCard, sectors, FALSE, format))
    return FALSE;
  if (!MifareTest_Standard_BlocksDelay(hCard, 1, FALSE, format))
    return FALSE;
  if (!MifareTest_Standard_BlocksDelay(hCard, (BYTE) (sectors-1), FALSE, format))
    return FALSE;
  /* Standard commands, sector level. */
  if (!MifareTest_Standard_Sectors(hCard, sectors, TRUE, format))
    return FALSE;
  if (!MifareTest_Standard_Sectors(hCard, sectors, FALSE, format))
    return FALSE;
  return TRUE;
}

BOOL MifareTestSpecific(SCARDHANDLE hCard, BYTE sectors, CARD_FORMAT_T format)
{
  /* Vendor specific, block level. The reader tries to guess the key */
  if (!MifareTest_Specific_Blocks_A(hCard, sectors, TRUE))
    return FALSE;
  if (!MifareTest_Specific_Blocks_A(hCard, sectors, FALSE))
    return FALSE;
  /* Vendor specific, sector level. The reader tries to guess the key */
  if (!MifareTest_Specific_Sectors_A(hCard, sectors, TRUE))
    return FALSE;
  if (!MifareTest_Specific_Sectors_A(hCard, sectors, FALSE))
    return FALSE;

  /* Vendor specific, block level. The value of the key is passed on every function call */
  if (!MifareTest_Specific_Blocks_K(hCard, sectors, TRUE, format))
    return FALSE;
  if (!MifareTest_Specific_Blocks_K(hCard, sectors, FALSE, format))
    return FALSE;
  /* Vendor specific, sector level. The value of the key is passed on every function call */
  if (!MifareTest_Specific_Sectors_K(hCard, sectors, TRUE, format))
    return FALSE;
  if (!MifareTest_Specific_Sectors_K(hCard, sectors, FALSE, format))
    return FALSE;

  /* Vendor specific, block level. The index of the key is passed on every function call */
  if (!MifareTest_Specific_Blocks_I(hCard, sectors, TRUE, format))
    return FALSE;
  if (!MifareTest_Specific_Blocks_I(hCard, sectors, FALSE, format))
    return FALSE;
  /* Vendor specific, sector level. The index of the key is passed on every function call */
  if (!MifareTest_Specific_Sectors_I(hCard, sectors, TRUE, format))
    return FALSE;
  if (!MifareTest_Specific_Sectors_I(hCard, sectors, FALSE, format))
    return FALSE;

  return TRUE;
}


BOOL MifareTestCore(SCARDCONTEXT hContext, const char *szReader, const BYTE atr[], DWORD atrlen)
{
  SCARDHANDLE hCard;
  DWORD dwProtocol;
  LONG rc;
	BYTE sectors = 0;

  if (atrlen > (sizeof(PCSC_MIFARE_ATR_BASE) + 2))
  {
    if (!memcmp(atr, PCSC_MIFARE_ATR_BASE, sizeof(PCSC_MIFARE_ATR_BASE)))
    {
      if (!memcmp(&atr[sizeof(PCSC_MIFARE_ATR_BASE)], PCSC_MIFARE_CLASSIC_1K_NN, 2))
      {
	      printf("Recognized the ATR of a Mifare Classic 1K\n");
		    sectors = 16;
      } else
      if (!memcmp(&atr[sizeof(PCSC_MIFARE_ATR_BASE)], PCSC_MIFARE_CLASSIC_4K_NN, 2))
      {
	      printf("Recognized the ATR of a Mifare Classic 4K\n");
		    sectors = 40;
      } else
      if (!memcmp(&atr[sizeof(PCSC_MIFARE_ATR_BASE)], PCSC_MIFARE_PLUS_SL1_2K_NN, 2))
      {
	      printf("Recognized the ATR of a Mifare Plus 2K in SL1\n");
		    sectors = 32;
      } else
      if (!memcmp(&atr[sizeof(PCSC_MIFARE_ATR_BASE)], PCSC_MIFARE_PLUS_SL1_4K_NN, 2))
      {
	      printf("Recognized the ATR of a Mifare Plus 4K in SL1\n");
		    sectors = 40;
      }
    }
  }

  if (sectors == 0)
	{
    printf("Unrecognized ATR!\n");
		return TRUE;
	}

  if ((gbSectors != 0) && (sectors > gbSectors))
    sectors = gbSectors;

  /* Connect to the card */
  rc = SCardConnect(hContext, szReader, SCARD_SHARE_EXCLUSIVE, SCARD_PROTOCOL_T1, &hCard, &dwProtocol);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardConnect error %08lX\n", rc);
		return FALSE;
	}

  /* Load the keys we'll need into reader's memory */
  rc = MifareTest_LoadKeys(hCard);
  if (rc != RC_TRUE)
  {
    printf("Failed to load the keys\n");
    goto failed;
  }

  /* Determine whether the card is already in transport conditions, or not */
  printf("Reading the card, please wait...\n");
  rc = MifareTest_IsTransport(hCard, sectors);
  if (rc == RC_FALSE)
  {
    printf("This card is not in transport condition !\n\n");

    if (!gbOverwrite)
    {
      printf("Test is interrupted to protect the data that are currently on the card.\n");
      printf("Please run this tool again, with parameter -f to format the card.\n");
      goto failed;
    }
  } else
  if (rc != RC_TRUE)
  {
    printf("Failed to verify whether the card is in transport conditions, or not\n");
    goto failed;
  }

  /* Format the card to make it ready for the first tests */
  printf("Formating the card, please wait...\n");
  rc = MifareTest_Format(hCard, sectors, FMT_TEST_KEYS_A_B);
  if (rc != RC_TRUE)
  {
    printf("Failed to format the card\n");
    goto failed;
  }

  /* OK, let's run the first serie of tests */
  if (!MifareTestStandard(hCard, sectors, FMT_TEST_KEYS_A_B))
    goto failed;
  if (!MifareTestSpecific(hCard, sectors, FMT_TEST_KEYS_A_B))
    goto failed;

  /* Format the card to make it ready for the second tests */
  printf("Formating the card, please wait...\n");
  rc = MifareTest_Format(hCard, sectors, FMT_TEST_KEYS_F_F);
  if (rc != RC_TRUE)
  {
    printf("Failed to format the card\n");
    goto failed;
  }

  /* OK, let's run the second serie of tests */
  if (!MifareTestStandard(hCard, sectors, FMT_TEST_KEYS_F_F))
    goto failed;
  if (!MifareTestSpecific(hCard, sectors, FMT_TEST_KEYS_F_F))
    goto failed;

  /* Put back the card into transport condition */
  printf("Formating the card, please wait...\n");
  rc = MifareTest_Format(hCard, sectors, FMT_TRANSPORT);
  if (rc != RC_TRUE)
  {
    printf("Failed to put back the card in transport conditions\n");
    goto failed;
  }

  /* OK, let's run the third serie of tests */
  if (!MifareTestStandard(hCard, sectors, FMT_TRANSPORT))
    goto failed;
  if (!MifareTestSpecific(hCard, sectors, FMT_TRANSPORT))
    goto failed;

  /* OK, done */
  printf("Done!\n");
  SCardDisconnect(hCard, SCARD_RESET_CARD);
  return TRUE;

failed:
  SCardDisconnect(hCard, SCARD_RESET_CARD);
  return FALSE;
}
