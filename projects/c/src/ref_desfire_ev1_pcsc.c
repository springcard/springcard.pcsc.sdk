/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2011 SpringCard SAS - www.springcard.com
   
  ref_desfire_pcsc.c
  ------------------

  This is the reference applications that validates the Pcsc_Desfire.dll
  with Philips DESFire cards.
  
  JDA 13/06/2008 : created from ref_desfire.c (CSB legacy SDK)
  JDA 07/12/2011 : added random sleeps
  JDA 17/07/2015 : changing the master key becomes optionnal

*/
#include "pcsc_helpers.h"
#include "cardware/desfire/pcsc_desfire.h"
#include <time.h>

BOOL TestDesfire(SCARDCONTEXT hContext, const char *szReader);
BOOL TestDesfire_Ex(SCARDHANDLE hCard, BYTE iTestMode, BOOL fLegacyCard);

BOOL gbChangeMasterKey = FALSE;

BOOL gbVerbose = TRUE;
BOOL gbRandomSleep = FALSE;

int main(int argc, char **argv)
{
  SCARDCONTEXT      hContext;
  SCARD_READERSTATE rgscState[MAXIMUM_SMARTCARD_READERS];

  DWORD             dwReaders, dwReadersOld = 0;

  LPSTR  szReaders = NULL;
  DWORD  dwReadersSz;

  BOOL   first_run = TRUE;
  DWORD  i, j;
  LONG   rc;
  
  if ((argc >= 2) && (!strcmp(argv[1], "-m")))
    gbChangeMasterKey = TRUE;

  if (gbRandomSleep)
  {
    srand((unsigned int) time(NULL));
  }

  /*
   * Welcome message, check parameters
   * ---------------------------------
   */
  printf("SpringCard SDK for PC/SC (CSB6 family)\n");
  printf("\n");
  printf("NXP DESFIRE EV1 reference demo\n");
  printf("------------------------------\n");
  printf("www.springcard.com\n\n");
  printf("\n");
  printf("Press <Ctrl+C> to exit\n\n");

  /*
   * Get a handle to the PC/SC resource manager
   * ------------------------------------------
   */
  rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
  if(rc != SCARD_S_SUCCESS)
  {
    printf("SCardEstablishContext :%08lX\n",rc);
	  return EXIT_FAILURE;
  }

  /*
   * Infinite loop, we'll exit only when killed by Ctrl+C
   * ----------------------------------------------------
   */
  for (;;)
  {
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
	    dwReaders = 0;
	  } else if (rc != SCARD_S_SUCCESS)
	  {
      printf("SCardListReaders error %08lX\n",rc);
	    break;
	  } else
	  {
	    /* Track events on found readers. */
	    LPTSTR pReader = szReaders;

	    for (dwReaders=0; dwReaders<MAXIMUM_SMARTCARD_READERS; dwReaders++)
	    {
        /* End of multi-string array */
		    if (*pReader == '\0') break;

        /* Remember this reader's name */
		    rgscState[dwReaders].szReader = pReader;

        /* Jump to next entry in multi-string array */
		    pReader += strlen(pReader) + 1;
	    }
	  }

	  if (first_run)
    {
      /* Program startup, display the number of readers */
      if (dwReaders == 0)
      {
        printf("No PC/SC reader\n\n");
      } else
      {
        printf("%lu PC/SC reader%s found\n\n", dwReaders, dwReaders ? "s" : "");
      }
    }
      
	  if (dwReadersOld != dwReaders)
	  {
	    /* Reader added, or reader removed           */
      /* (re)set the initial state for all readers */
	    for (i=0; i<dwReaders; i++)
		    rgscState[i].dwCurrentState = SCARD_STATE_UNAWARE;

      if (!first_run)
	    {
        int c;

	      /* Not the program startup, explain the event */
	      if (dwReadersOld > dwReaders)
		    {
          c = (int) dwReadersOld-dwReaders;
		      printf("%d reader%s ha%s been removed from the system\n\n", c, (c==1)?" ":"s", (c==1)?"s":"ve");
		    } else
		    {
          c = (int) dwReaders-dwReadersOld;
		      printf("%d reader%s ha%s been added to the system\n\n", c, (c==1)?" ":"s", (c==1)?"s":"ve");
		    }
	    }

      dwReadersOld = dwReaders;
	  }

    if (dwReaders == 0)
    {
      /* We must wait here, because the SCardGetStatusChange doesn't wait  */
      /* at all in case there's no reader in the system. Silly, isn't it ? */
#ifdef WIN32
      Sleep(1000);
#endif
#ifdef __linux__
      sleep(1);
#endif
    }

	  /*
     * Interesting part of the job : call SCardGetStatusChange to monitor all changes
     * taking place in the PC/SC reader(s)
     */
	
	  rc = SCardGetStatusChange(hContext, INFINITE, rgscState, dwReaders);	
		
	  if (rc != SCARD_S_SUCCESS)
    {
      printf("SCardGetStatusChange error %08lX\n",rc);
      break;
    }

	  for (i=0; i<dwReaders; i++)
	  {
	    BOOL just_inserted = FALSE;

      if (rgscState[i].dwEventState & SCARD_STATE_CHANGED)
	    {
        /* Something changed since last loop        */
        if ( (rgscState[i].dwEventState & SCARD_STATE_PRESENT)
         && !(rgscState[i].dwCurrentState & SCARD_STATE_PRESENT) )
          just_inserted = TRUE;

        /* Remember new current state for next loop */
        rgscState[i].dwCurrentState = rgscState[i].dwEventState;
	    } else
	    {
        /* Nothing new, don't display anything for this reader */
        /* (unless we're in the first run)                     */
        if (!first_run)
          continue;
	    }

      /*
       * Display current reader's state
       * ------------------------------
       */

	    /* Reader's name */
      printf("Reader %lu: %s\n", i, rgscState[i].szReader);

      /* Complete status */
      printf("\tCard state:\n");

      if (rgscState[i].dwEventState & SCARD_STATE_IGNORE)
        printf("\t\tIgnore this reader\n");

      if (rgscState[i].dwEventState & SCARD_STATE_UNKNOWN)
        printf("\t\tReader unknown\n");

      if (rgscState[i].dwEventState & SCARD_STATE_UNAVAILABLE)
        printf("\t\tStatus unavailable\n");

      if (rgscState[i].dwEventState & SCARD_STATE_EMPTY)
        printf("\t\tNo card in the reader\n");

      if (rgscState[i].dwEventState & SCARD_STATE_PRESENT)
        printf("\t\tCard present\n");

      if (rgscState[i].dwEventState & SCARD_STATE_ATRMATCH)
        printf("\t\tATR match\n");
  
      if (rgscState[i].dwEventState & SCARD_STATE_EXCLUSIVE)
        printf("\t\tReader reserved for exclusive use\n");

      if (rgscState[i].dwEventState & SCARD_STATE_INUSE)
        printf("\t\tReader/card in use\n");

      if (rgscState[i].dwEventState & SCARD_STATE_MUTE)
        printf("\t\tCard mute\n");

      if (just_inserted)
	    {
        /*
         * Display card's ATR
         * ------------------
         */

        if (rgscState[i].cbAtr)
		    {
	        printf("\tCard ATR:");
		      for (j=0; j<rgscState[i].cbAtr; j++)
            printf(" %02X", rgscState[i].rgbAtr[j]);
          printf("\n");
		    }

       /*
        * Do the Desfire library test on this card
        * ----------------------------------------
        */
        TestDesfire(hContext, rgscState[i].szReader);
	    }
	  }

    /* Free the list of readers  */
    if (szReaders != NULL)
    {
      SCardFreeMemory(hContext, szReaders);
      szReaders = NULL;
    }
      
    /* Not the first run anymore */
		first_run = FALSE;
	}


  rc = SCardReleaseContext(hContext);

  /* Never go here (Ctrl+C kill us before !) */
  return EXIT_FAILURE;
}

#define TM_UNSET       0
#define TM_LEGACY      1
#define TM_ISO_3DES2K  2
#define TM_ISO_3DES3K  3
#define TM_ISO_AES     4

BOOL TestDesfire(SCARDCONTEXT hContext, const char *szReader)
{
  SCARDHANDLE hCard;
  DWORD dwProtocol;
  LONG rc;
  BOOL f, l;

  printf("Testing the Desfire library on this card...\n");

  /*
   * Connect to the card, accept either T=0 or T=1
   * ---------------------------------------------
   */
  rc = SCardConnect(hContext,
                    szReader,
                    SCARD_SHARE_SHARED,
                    SCARD_PROTOCOL_T0|SCARD_PROTOCOL_T1,
                    &hCard,
                    &dwProtocol);
	if (rc != SCARD_S_SUCCESS)
  {
    printf("\tSCardConnect error %08lX\n",rc);
		return FALSE;
  }

  printf("\tConnected to the card, protocol ");
  switch (dwProtocol)
  {
    case SCARD_PROTOCOL_T0 : printf("T=0"); break;
    case SCARD_PROTOCOL_T1 : printf("T=1"); break;
    default                : printf("%08lX", dwProtocol);
  }
  printf("\n");

  
  SCardDesfire_AttachLibrary(hCard);


  l = FALSE;

  {
		DF_VERSION_INFO stVersionInfo;
		WORD wDesFireSoftwareVersion;
		LONG rc;

		//  Get the card's version information.
		rc = SCardDesfire_GetVersion(hCard, &stVersionInfo);
		if (rc == 0)
		{
		  wDesFireSoftwareVersion = (unsigned int) ((unsigned int) stVersionInfo.bSwMajorVersion << 8 | stVersionInfo.bSwMinorVersion);
			if (wDesFireSoftwareVersion < 0x0100)
			{
				printf("This is a Desfire EV0!\n");
				l = TRUE;
			}
		}
	}

  printf("Desfire EV0 compatibility test (DES/3DES2K)\n");

	if (!TestDesfire_Ex(hCard, TM_LEGACY, l))
	{
		f = FALSE;
		goto done;
	}

	if (!l)
	{
		printf("Desfire EV1, ISO mode with 3DES2K\n");

		if (!TestDesfire_Ex(hCard, TM_ISO_3DES2K, l))
		{
			f = FALSE;
			goto done;
		}

		printf("Desfire EV1, ISO mode with 3DES3K\n");

		if (!TestDesfire_Ex(hCard, TM_ISO_3DES3K, l))
		{
			f = FALSE;
			goto done;
		}

		printf("Desfire EV1, ISO mode with AES\n");

		if (!TestDesfire_Ex(hCard, TM_ISO_AES, l))
		{
			f = FALSE;
			goto done;
		}
	}

done:
  SCardDesfire_DetachLibrary(hCard);
  SCardDisconnect(hCard, SCARD_EJECT_CARD);

  printf("\n\n\n");
  return f;
}

/* Check DESFire library functions */
/* ------------------------------- */

const BYTE abNullKey[24]         = { 0 };

const BYTE abRootKey1K[24]       = { "ABCDEFGHABCDEFGHABCDEFGH" };
const BYTE abRootKey2K[24]       = { "Card Master Key!Card Mas" };
const BYTE abRootKey3K[24]       = { "Card Master Key!        " };

const BYTE abTestKeyDes1K[24]    = { "ABCDEFGHABCDEFGHABCDEFGH" };
const BYTE abTestKeyDes2K[24]    = { "ABCDEFGHIJKLMNOPABCDEFGH" };
const BYTE abTestKeyDes3K[24]    = { "ABCDEFGHIJKLMNOPQRSTUVWX" };

const BYTE appBKeyMaster0_16[16] = { "App.B Master Key" };
const BYTE appBKeyMaster1_16[16] = { "@qq/C!L`ruds!Kdx" };
const BYTE appBChangeKey_16[16]  = { "B's Chg Keys Key" };

const BYTE appBKeyMaster0_24[24] = { "App.B Master KeyApp. Key" };
const BYTE appBKeyMaster1_24[24] = { "@qq/C!L`ruds!Kdx@qq/!Kdx" };
const BYTE appBChangeKey_24[24]  = { "B's Chg Keys KeyB's  Key" };

const BYTE appBKey1_16[16]       = { "App.B Key #1.   " };
const BYTE appBKey2_16[16]       = { "App.B Key #2..  " };
const BYTE appBKey3_16[16]       = { "App.B Key #3... " };
const BYTE appBKey4_16[16]       = { "App.B Key #4...." };

const BYTE appBKey1_24[24]       = { "App.B Key #1.   .   .   " };
const BYTE appBKey2_24[24]       = { "App.B Key #2..  ..  ..  " };
const BYTE appBKey3_24[24]       = { "App.B Key #3... ... ... " };
const BYTE appBKey4_24[24]       = { "App.B Key #4............" };

const BYTE appCKey1_16[16]       = { "App.C Key #1.   " };
const BYTE appCKey2_16[16]       = { "App.C Key #2..  " };
const BYTE appCKey3_16[16]       = { "App.C Key #3... " };
const BYTE appCKey4_16[16]       = { "App.C Key #4...." };

const BYTE appCKey1_24[24]       = { "App.C Key #1.   .   .   " };
const BYTE appCKey2_24[24]       = { "App.C Key #2..  ..  ..  " };
const BYTE appCKey3_24[24]       = { "App.C Key #3... ... ... " };
const BYTE appCKey4_24[24]       = { "App.C Key #4............" };

BOOL TestDesfire_Ex(SCARDHANDLE hCard, BYTE iTestMode, BOOL fLegacyCard)
{
  #define CHECK_RC() \
    { if (rc!=SCARD_S_SUCCESS) { printf("\nline %d, mode %02X - failed (%08lX/%ld) %s\n", __LINE__-1, iTestMode, rc, rc, SCardDesfire_GetErrorMessage(rc)); return FALSE; } \
      if (trace) printf("%s %d\n", __FILE__, __LINE__); else printf("."); fflush(NULL); \
      if (gbRandomSleep) Sleep(rand() % 6000); \
    }

	LONG rc;
	LONG lAmount;
	BYTE bCommMode, bKeyVersion;
	DWORD eNRecordsRead, eOffset, eLength, dwFreeBytes = 4096;
	BYTE bStdDataFileID, bFileType, bNFilesFound, bNApplicationsFound;
	DWORD dwNbBytesRead;
	DWORD adwApplications[32];
	BYTE  abFiles[32];
	DF_ISO_APPLICATION_ST astIsoApplications[32];
	WORD  awIsoFiles[32];
	BYTE abDataBuffer[8192];
	WORD wAccessRights;
	BOOL trace = FALSE;
	int i, j, iTransaction, iFileIndex;
	DF_VERSION_INFO stVersionInfo;
	DF_ADDITIONAL_FILE_SETTINGS unFileSettings;

	//  After activating a DesFire card the currently selected 'application' is the card
	//  iTestMode (AID = 0). Therefore the next command is redundant.
	//  We use the command to check that the card is responding correctly.
	rc = SCardDesfire_SelectApplication(hCard, 0);
	CHECK_RC();

	if (!fLegacyCard)
	{
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
	}

	//  Which Card Master Key is currently in use ?
	//  Beginning with DesFire version 0.1 the GetKeyVersion command can help determining
	//  which key is currently effective.
	rc = SCardDesfire_GetKeyVersion(hCard, 0, &bKeyVersion);
	CHECK_RC();

	//  Authenticate with the key which corresponds to the retieved key version.
	if (bKeyVersion == 0)
	{
		rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
		if (rc == (DFCARD_ERROR-DF_AUTHENTICATION_ERROR))
		{
	    rc = SCardDesfire_AuthenticateIso(hCard, 0, abNullKey);
			CHECK_RC();

      //  The key is configured for 3DES3K
			//  for obvious reasons we cannot revert the config to 3DES2K, but a hook by AES permits it
			rc = SCardDesfire_ChangeKeyAes(hCard, DF_APPLSETTING2_AES, 0xAE, abRootKey2K, NULL);
			CHECK_RC();
			rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		}
		CHECK_RC();


	} else
	if (bKeyVersion == 0xAA)
	{
		rc = SCardDesfire_Authenticate(hCard, 0, abRootKey1K);
		if (rc == (DFCARD_ERROR-DF_AUTHENTICATION_ERROR))
		{
	    rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey1K);

      //  The key is configured for 3DES3K
			//  for obvious reasons we cannot revert the config to 3DES2K, but a hook by AES permits it
			rc = SCardDesfire_ChangeKeyAes(hCard, DF_APPLSETTING2_AES, 0xAE, abRootKey2K, NULL);
			CHECK_RC();
			rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		}
	  CHECK_RC();

	} else
	if (bKeyVersion == 0xC7)
	{
		rc = SCardDesfire_Authenticate(hCard, 0, abRootKey2K);
		if (rc == (DFCARD_ERROR-DF_AUTHENTICATION_ERROR))
		{
			rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey2K);

      //  The key is configured for 3DES3K
			//  for obviously we cannot revert the config to 3DES2K, but a hook by AES permits it
			rc = SCardDesfire_ChangeKeyAes(hCard, DF_APPLSETTING2_AES, 0xAE, abRootKey2K, NULL);
			CHECK_RC();
			rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		}
	  CHECK_RC();

	} else
	if (bKeyVersion == 0xAE)
	{
    rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
	  CHECK_RC();

	} else
	{
		//  Who knows the key ?
		printf("Sorry, the master key is unknown (version = %02X)\n", bKeyVersion);
		return FALSE;
	}

  if (bKeyVersion != 0)
  {
    //  Back to default (null) key
    rc = SCardDesfire_ChangeKey(hCard, 0, abNullKey, NULL);
    CHECK_RC();
  }

	//  Authenticate again
	rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
	CHECK_RC();

  if (gbChangeMasterKey)
  {
	  //  Try to allow to change the key
	  rc = SCardDesfire_ChangeKeySettings(hCard, 0xF);
	  CHECK_RC();
  }

	//  List the existing applications
	rc = SCardDesfire_GetApplicationIDs(hCard, 0, NULL, &bNApplicationsFound);
	CHECK_RC();

  if (bNApplicationsFound)
	{
		//  Startup with a blank card !!!
		rc = SCardDesfire_FormatPICC(hCard);
		CHECK_RC();
	}

  //  Card is ready...
	printf("\n");

	//  Authenticate again
	rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
	CHECK_RC();

	if (!fLegacyCard)
	{
		// Get the UID of the card
		rc = SCardDesfire_GetCardUID(hCard, abDataBuffer);
		CHECK_RC();
	}
   
  if (gbChangeMasterKey)
  {
	  //  Change the Card master key to a SingleDES key.
	  rc = SCardDesfire_ChangeKey(hCard, 0, abRootKey1K, NULL);
	  CHECK_RC();

	  rc = SCardDesfire_GetKeyVersion(hCard, 0, &bKeyVersion);
	  CHECK_RC();

	  if (iTestMode == TM_LEGACY)
	  {
		  rc = SCardDesfire_Authenticate(hCard, 0, abRootKey1K);
		  CHECK_RC();
		  rc = SCardDesfire_ChangeKey(hCard, 0, abRootKey2K, NULL);
		  CHECK_RC();

	  } else
	  if (iTestMode == TM_ISO_3DES2K)
	  {
		  rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey1K);
		  CHECK_RC();
		  rc = SCardDesfire_ChangeKey(hCard, 0, abRootKey2K, NULL);
		  CHECK_RC();

	  } else
	  if (iTestMode == TM_ISO_3DES3K)
	  {
		  rc = SCardDesfire_AuthenticateIso24(hCard, 0, abRootKey1K);
		  CHECK_RC();
		  rc = SCardDesfire_ChangeKey24(hCard, DF_APPLSETTING2_3DES3K, abRootKey2K, NULL);
		  CHECK_RC();

	  } else
	  if (iTestMode == TM_ISO_AES)
	  {
		  rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey1K);
		  CHECK_RC();
		  rc = SCardDesfire_ChangeKeyAes(hCard, DF_APPLSETTING2_AES, 0xAE, abRootKey2K, NULL);
		  CHECK_RC();

	  }
  }

	/*
	// Reconfigure the card
	{
		BYTE b = 0x02;
		//BYTE ats[] = { 0x06, 0x75, 0x77, 0x81, 0x02, 0xFF };
		//rc = SCardDesfire_SetConfiguration(hCard, 0x02, ats, sizeof(ats));
		rc = SCardDesfire_SetConfiguration(hCard, 0x00, &b, 1);
		CHECK_RC();
	}
	*/

	if (!fLegacyCard)
	{		
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
	}

	rc = SCardDesfire_GetKeyVersion(hCard, 0, &bKeyVersion);
	CHECK_RC();
	rc = SCardDesfire_GetKeyVersion(hCard, 0, &bKeyVersion);
	CHECK_RC();

  if (gbChangeMasterKey)
  {
	  if (iTestMode == TM_LEGACY)
	  {
		  rc = SCardDesfire_Authenticate(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES2K)
	  {
		  rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES3K)
	  {
		  rc = SCardDesfire_AuthenticateIso24(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_AES)
	  {
		  rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  }
  }

	rc = SCardDesfire_GetKeySettings(hCard, &abDataBuffer[0], &abDataBuffer[1]);
	CHECK_RC();

	if (!fLegacyCard)
	{		
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
	}

	//  At this point we would like to begin with a blank card.
	rc = SCardDesfire_FormatPICC(hCard);
	CHECK_RC();

	if (!fLegacyCard)
	{		
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
	}

	//  Create three applications:

	//  Application A is the most open one.
	//  It has no keys and its key settings are least restrictive.
	//  Note: since there are no keys, the Change Keys key settting must be assigned
	//        the value 0xE or 0xF.
	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xAAAAAA, 0xFF, 0);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xAAAAAA, 0xFF, 0);
		CHECK_RC();
  } else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xAAAAAA, 0xFF, DF_APPLSETTING2_3DES3K | 0);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xAAAAAA, 0xFF, DF_APPLSETTING2_AES | 0);
		CHECK_RC();
	}

	//  Application B's key settings can be changed at a later time.
	//  It has six keys.
	//  Authentication with a given key allows also to change that key.
	//  (the Change Keys key settting is 0xE)
	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xBBBBBB, 0xEF, 6);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xBBBBBB, 0xEF, 6);
		CHECK_RC();
  } else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xBBBBBB, 0xEF, DF_APPLSETTING2_3DES3K | 6);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xBBBBBB, 0xEF, DF_APPLSETTING2_AES | 6);
		CHECK_RC();
	}

	//  Application C keeps everything private.
	//  Even getting its file directory is not publicly allowed.
	//  The application has the maximum of 14 keys.
	//  We make key #0xC the Change Keys key.
	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xCCCCCC, 0xC0, 14);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xCCCCCC, 0xC0, 14);
		CHECK_RC();
  } else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xCCCCCC, 0xC0, DF_APPLSETTING2_3DES3K | 14);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xCCCCCC, 0xC0, DF_APPLSETTING2_AES | 14);
		CHECK_RC();
	}

	//  Verify that the applications have been created.
	rc = SCardDesfire_GetApplicationIDs(hCard, 32, adwApplications, &bNApplicationsFound);
	CHECK_RC();

	// New applications for ISO testing 
	if (!fLegacyCard)
	{
		// The ISO DF=2000 application will allow ISO EFs
		rc = SCardDesfire_CreateIsoApplication(hCard, 0xFF2000, 0xFF, DF_APPLSETTING2_ISO_EF_IDS, 0x2000, (const unsigned char *) "1TIC.ICA", 8);
		CHECK_RC();
		rc = SCardDesfire_CreateIsoApplication(hCard, 0xFF3000, 0xFF, DF_APPLSETTING2_ISO_EF_IDS, 0x3000, (const unsigned char *) "2MPP.EPP", 8);
		CHECK_RC();
		rc = SCardDesfire_CreateIsoApplication(hCard, 0xFF4000, 0xFF, DF_APPLSETTING2_ISO_EF_IDS, 0x4000, (const unsigned char *) "3JDA.JDA", 8);
		CHECK_RC();
		rc = SCardDesfire_CreateIsoApplication(hCard, 0xFF3F00, 0xFF, DF_APPLSETTING2_ISO_EF_IDS, 0x3F00, (const unsigned char *) "Test Master File", 16);
		CHECK_RC();
	}

	// More applications
	if (dwFreeBytes > 4096)
	{
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF00, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF01, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF02, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF03, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF04, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF05, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF06, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF07, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF08, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF0A, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF0B, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF0C, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF0D, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF0E, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF0F, 0xFF, 0);
		CHECK_RC();

		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF10, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF20, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF30, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF40, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF50, 0xFF, 0);
		CHECK_RC();
		rc = SCardDesfire_CreateApplication(hCard, 0xFFFF60, 0xFF, 0);
		CHECK_RC();
	}

	//  Verify that the applications have been created.
	rc = SCardDesfire_GetApplicationIDs(hCard, 32, adwApplications, &bNApplicationsFound);
	CHECK_RC();

	// Cleanup authentication state
	rc = SCardDesfire_SelectApplication(hCard, 0);
	CHECK_RC();

	// Retrieve the list of applications
	rc = SCardDesfire_GetApplicationIDs(hCard, 32, adwApplications, &bNApplicationsFound);
	CHECK_RC();

	if (gbVerbose)
	{
		printf("\n");
		printf("%d application(s) found:", bNApplicationsFound);
		for (i=0; i<bNApplicationsFound; i++)
		{
			if ((i % 8) == 0)
				printf("\n- ");
			printf("%06lX   ", adwApplications[i]);
		}
		printf("\n");
	}

	if (!fLegacyCard)
	{
		// Retrieve the list of ISO DF
		rc = SCardDesfire_GetIsoApplications(hCard, 32, astIsoApplications, &bNApplicationsFound);
		CHECK_RC();

		if (gbVerbose)
		{
			printf("\n");
			printf("%d ISO application(s) found:\n", bNApplicationsFound);
			for (i=0; i<bNApplicationsFound; i++)
			{
				printf("- %06lX = ISO %04X  ", astIsoApplications[i].dwAid, astIsoApplications[i].wIsoId);
				for (j=0; j<astIsoApplications[i].bIsoNameLen; j++)
					printf("%02X", astIsoApplications[i].abIsoName[j]);
				for (   ; j<16; j++)
					printf("  ");
				printf("  ");
				for (j=0; j<astIsoApplications[i].bIsoNameLen; j++)
					printf("%c", (astIsoApplications[i].abIsoName[j] >= ' ') ? astIsoApplications[i].abIsoName[j] : '.');
				printf("\n");
			}
		}
	}

	if (!fLegacyCard)
	{
		// Create ISO files in ISO application 2000
		rc = SCardDesfire_SelectApplication(hCard, 0xFF2000);
		CHECK_RC();

		rc = SCardDesfire_CreateIsoStdDataFile(hCard, 1, 0x2001, 0, 0xEEEE, 32);
		CHECK_RC();

		rc = SCardDesfire_CreateIsoBackupDataFile(hCard, 2, 0x2002, 0, 0xEEEE, 32);
		CHECK_RC();

		rc = SCardDesfire_CreateIsoLinearRecordFile(hCard, 3, 0x2003, 0, 0xEEEE, 16, 4);
		CHECK_RC();

		rc = SCardDesfire_CreateIsoCyclicRecordFile(hCard, 4, 0x2004, 0, 0xEEEE, 16, 4);
		CHECK_RC();

		//  Get the file IDs
		rc = SCardDesfire_GetFileIDs(hCard, 32, abFiles, &bNFilesFound);
		CHECK_RC();

		if (gbVerbose)
		{
			printf("\n");
			printf("%d file(s) found:", bNFilesFound);
			for (i=0; i<bNFilesFound; i++)
			{
				if ((i % 16) == 0)
					printf("\n- ");
				printf("%02X   ", abFiles[i]);
			}
			printf("\n");
		}

		//  Get the ISO file IDs
		rc = SCardDesfire_GetIsoFileIDs(hCard, 32, awIsoFiles, &bNFilesFound);
		CHECK_RC();

		if (gbVerbose)
		{
			printf("\n");
			printf("%d ISO file(s) found:", bNFilesFound);
			for (i=0; i<bNFilesFound; i++)
			{
				if ((i % 12) == 0)
					printf("\n- ");
				printf("%04X   ", awIsoFiles[i]);
			}
			printf("\n");
		}

	}

	bStdDataFileID = 15;

	//  Create the files in application A:
	//  Since the application does not have any keys associated with it, the file access rights
	//  settings 0xE (public access granted) and 0xF (access denied) are the only permissable
	//  ones.
	rc = SCardDesfire_SelectApplication(hCard, 0xAAAAAA);
	CHECK_RC();

	//  Create a the Standard Data file.
	rc = SCardDesfire_CreateStdDataFile(hCard, bStdDataFileID, 0, 0xEEEE, 640);
	CHECK_RC();

	//  Create a 64 byte Backup file.
	rc = SCardDesfire_CreateBackupDataFile(hCard, 5, 0, 0xEEEE, 64);
	CHECK_RC();

	//  Create a Value file allowing values from 0 to 1000.
	//  The initial value is 0.
	//  The Limited Credit feature is disabled.
	rc = SCardDesfire_CreateValueFile(hCard, 4, 0, 0xEEEE, 0, 1000, 0, 0);
	CHECK_RC();

	//  And finally create a Cyclic Record file capable of holding 10 records of 4 bytes each.
	rc = SCardDesfire_CreateCyclicRecordFile(hCard, 0, 0, 0xEEEE, 4, 10);
	CHECK_RC();

	//  Fill the Standard Data file with some data.
	memset(abDataBuffer, 0xDA, sizeof(abDataBuffer));

	rc = SCardDesfire_WriteData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 0, 640, abDataBuffer);
	CHECK_RC();
	rc = SCardDesfire_WriteData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 0, 30, (unsigned char *) "This is the 1st block written.");
	CHECK_RC();
	rc = SCardDesfire_WriteData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 34, 22, (unsigned char *) "This is the 2nd block.");
	CHECK_RC();

	 //  Then make the file permanently read-only.
	rc = SCardDesfire_ChangeFileSettings(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 0xEFFF);
	CHECK_RC();

	//  Read part of the file's contents.
	rc = SCardDesfire_ReadData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 10, 50, abDataBuffer, &dwNbBytesRead);
	CHECK_RC();

	//  Get all data in one block. 
	//  Note: Must make sure that buffer is large enough for all data!!
	rc = SCardDesfire_ReadData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 0, 0, abDataBuffer, &dwNbBytesRead);
	CHECK_RC();

  //  Test different lengths
	for (eLength = 45; eLength < 65; eLength++)
	{
		rc = SCardDesfire_ReadData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 0, eLength, abDataBuffer, &dwNbBytesRead);
		CHECK_RC();
	}

	//  Try to overwrite the file.
	//  Since we have made the file read-only, this should fail.

	rc = SCardDesfire_WriteData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 20, 5, (const unsigned char *) "Essai");
	if (rc != (DFCARD_ERROR - DF_PERMISSION_DENIED))
		CHECK_RC();

	//  Do 15 transactions.
	for (iTransaction = 0; iTransaction < 15; iTransaction++)
	{
		//  Write to the Backup Data file.
		sprintf((char *) abDataBuffer, "%02d,", iTransaction);
		rc = SCardDesfire_WriteData(hCard, 5, DF_COMM_MODE_PLAIN, 3 * iTransaction, 3,
														abDataBuffer);
		CHECK_RC();

		//  Manipulate the Value file.
		rc = SCardDesfire_Credit(hCard, 4, DF_COMM_MODE_PLAIN, 100);
		CHECK_RC();
		rc = SCardDesfire_Debit(hCard, 4, DF_COMM_MODE_PLAIN, 93);
		CHECK_RC();

		//  Write to the Cyclic Record file.
		rc = SCardDesfire_WriteRecord(hCard, 0, DF_COMM_MODE_PLAIN, 2, 2, abDataBuffer);
		CHECK_RC();
		//  The following Write Record will write to the same record as above
		rc = SCardDesfire_WriteRecord(hCard, 0, DF_COMM_MODE_PLAIN, 0, 2,
															(unsigned char *) "r.");
		CHECK_RC();

		//  Verify that the 'official' contents of the three files has not changed
		//  before the CommitTransaction command.
		//  Must make sure that buffer is large enough for all data!
		rc = SCardDesfire_ReadData(hCard, 5, DF_COMM_MODE_PLAIN, 0, 0, abDataBuffer, &dwNbBytesRead);
		CHECK_RC();
		rc = SCardDesfire_GetValue(hCard, 4, DF_COMM_MODE_PLAIN, &lAmount);
		CHECK_RC();

		//  Note: reading from an empty record file returns an error.
		//        beginning with version 0.1 this aborts the transaction.
		if (iTransaction != 0)
		{
			rc = SCardDesfire_ReadRecords(hCard, 0, DF_COMM_MODE_PLAIN, 0, 0, 4, abDataBuffer, &eNRecordsRead);
			CHECK_RC();
		}
		//  Declare the transaction valid.
		rc = SCardDesfire_CommitTransaction(hCard);
		CHECK_RC();

		//  Verify that the transaction has become effective.
		//  Note: Must make sure that buffer is large enough for all data!
		rc = SCardDesfire_ReadData(hCard, 5, DF_COMM_MODE_PLAIN, 0, 0, abDataBuffer, &dwNbBytesRead);
		CHECK_RC();
		rc = SCardDesfire_GetValue(hCard, 4, DF_COMM_MODE_PLAIN, &lAmount);
		CHECK_RC();
		rc = SCardDesfire_ReadRecords(hCard, 0, DF_COMM_MODE_PLAIN, 0, 0, 4, abDataBuffer, &eNRecordsRead);
		CHECK_RC();
	}

	//  Limited Credit has been disabled, so the following call should fail.
	rc = SCardDesfire_LimitedCredit2(hCard, 4, 20);
	if (rc != (DFCARD_ERROR - DF_PERMISSION_DENIED))
		CHECK_RC();

	//  Get the file IDs of the current application.
	rc = SCardDesfire_GetFileIDs(hCard, 32, abFiles, &bNFilesFound);
	CHECK_RC();

	if (gbVerbose)
	{
		printf("\n");
		printf("%d file(s) found:", bNFilesFound);
		for (i=0; i<bNFilesFound; i++)
		{
			if ((i % 16) == 0)
				printf("\n- ");
			printf("%02X   ", abFiles[i]);
		}
		printf("\n");
	}

	//  Get information about the application's files.
	//  Delete each file after retrieving information about it.
	for (iFileIndex = 0; iFileIndex < bNFilesFound; iFileIndex++)
	{
		rc = SCardDesfire_GetFileSettings(hCard, abFiles[iFileIndex], &bFileType,
																	&bCommMode, &wAccessRights,
																	&unFileSettings);
		CHECK_RC();
		rc = SCardDesfire_DeleteFile(hCard, abFiles[iFileIndex]);
		CHECK_RC();
	}

	//  Verify that there are no files in the application.
	rc = SCardDesfire_GetFileIDs(hCard, 32, abFiles, &bNFilesFound);
	CHECK_RC();

	//  Delete application A.
	//  Since this application doesn't have an Application Master Key,
	//  this requires Card Master Key authentication.
	rc = SCardDesfire_SelectApplication(hCard, 0);
	CHECK_RC();

  if (gbChangeMasterKey)
  {
	  if (iTestMode == TM_LEGACY)
	  {
		  rc = SCardDesfire_Authenticate(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES2K)
	  {
		  rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES3K)
	  {
		  rc = SCardDesfire_AuthenticateIso24(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_AES)
	  {
		  rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  }
  } else
  {
		rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
		CHECK_RC();
  }

	rc = SCardDesfire_DeleteApplication(hCard, 0xAAAAAA);
	CHECK_RC();

	//  Verify that application A has been deleted.
	rc = SCardDesfire_GetApplicationIDs(hCard, 32, adwApplications, &bNApplicationsFound);
	CHECK_RC();

	//  Changing application B's keys:
	rc = SCardDesfire_SelectApplication(hCard, 0xBBBBBB);
	CHECK_RC();

	//  Use a TripleDES key as the Application Master Key.
	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
		CHECK_RC();
  } else
  if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 0, abNullKey);
		CHECK_RC();
	} else
  if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 0, abNullKey);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 0, abNullKey);
		CHECK_RC();
	}

	rc = SCardDesfire_GetVersion(hCard, &stVersionInfo);
	CHECK_RC();

	if (gbVerbose)
	{
		printf("\n");
		printf("bHwVendorID=%02X\n", stVersionInfo.bHwVendorID);
		printf("bHwType=%02X\n", stVersionInfo.bHwType);
		printf("bHwSubType=%02X\n", stVersionInfo.bHwSubType);
		printf("bHwMajorVersion=%02X\n", stVersionInfo.bHwMajorVersion);
		printf("bHwMinorVersion=%02X\n", stVersionInfo.bHwMinorVersion);
		printf("bHwStorageSize=%02X\n", stVersionInfo.bHwStorageSize);
		printf("bHwProtocol=%02X\n", stVersionInfo.bHwProtocol);
		printf("bSwVendorID=%02X\n", stVersionInfo.bSwVendorID);
		printf("bSwType=%02X\n", stVersionInfo.bSwType);
		printf("bSwSubType=%02X\n", stVersionInfo.bSwSubType);
		printf("bSwMajorVersion=%02X\n", stVersionInfo.bSwMajorVersion);
		printf("bSwMinorVersion=%02X\n", stVersionInfo.bSwMinorVersion);
		printf("bSwStorageSize=%02X\n", stVersionInfo.bSwStorageSize);
		printf("bSwProtocol=%02X\n", stVersionInfo.bSwProtocol);
		printf("abUid=%02X:%02X:%02X:%02X:%02X:%02X:%02X\n", stVersionInfo.abUid[0],stVersionInfo.abUid[1],stVersionInfo.abUid[2],stVersionInfo.abUid[3],stVersionInfo.abUid[4],stVersionInfo.abUid[5],stVersionInfo.abUid[6]);
		printf("abBatchNo=%02X:%02X:%02X:%02X:%02X\n", stVersionInfo.abBatchNo[0],stVersionInfo.abBatchNo[1],stVersionInfo.abBatchNo[2],stVersionInfo.abBatchNo[3],stVersionInfo.abBatchNo[4]);
		printf("bProductionCW=%02X\n", stVersionInfo.bProductionCW);
		printf("bProductionYear=%02X\n", stVersionInfo.bProductionYear);
	}

	//  Set the new application master key and get authenticated with it

  if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_ChangeKey(hCard, 0, appBKeyMaster0_16, NULL);
		CHECK_RC();
		rc = SCardDesfire_Authenticate(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_ChangeKey24(hCard, 0, appBKeyMaster0_16, NULL); // JDA
		CHECK_RC();
		rc = SCardDesfire_AuthenticateIso(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_ChangeKey24(hCard, 0, appBKeyMaster0_24, NULL); // JDA
		CHECK_RC();
		rc = SCardDesfire_AuthenticateIso24(hCard, 0, appBKeyMaster0_24);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_ChangeKeyAes(hCard, 0, 0, appBKeyMaster0_16, NULL); // JDA
		CHECK_RC();
		rc = SCardDesfire_AuthenticateAes(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
	}

	if (!fLegacyCard)
	{		
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
	}

	//  Beginning with DesFire version 0.1 we can query the version number of a key.
	rc = SCardDesfire_GetKeyVersion(hCard, 0, &bKeyVersion);
	CHECK_RC();

	//  Authenticate with the new application master key.
	//  This time use the opposite parity in all key bytes (will not work for AES!).
  if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 0, appBKeyMaster1_16);
		CHECK_RC();
  } else
  if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 0, appBKeyMaster1_16);
		CHECK_RC();
	} else
  if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 0, appBKeyMaster1_24);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{


	}

	//  Change key #1 to a different SingleDES key (both key halves are the same).
	//  Authentication with that key is necessary for changing it.
  if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 1, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 1, abTestKeyDes1K, NULL);
		CHECK_RC();
  } else
  if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 1, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 1, abTestKeyDes1K, NULL);
		CHECK_RC();
	} else
  if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 1, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 1, abTestKeyDes1K, NULL);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 1, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKeyAes(hCard, 1, 0x33, abTestKeyDes1K, NULL);
		CHECK_RC();
	}

	//  Change key #5 to a TripeDES key.
	//  Authentication with that key is necessary for changing it.
  if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 5, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 5, appBChangeKey_16, NULL);
		CHECK_RC();
  } else
  if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 5, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 5, appBChangeKey_16, NULL);
		CHECK_RC();
	} else
  if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 5, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 5, appBChangeKey_24, NULL);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 5, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKeyAes(hCard, 5, 0x77, appBChangeKey_16, NULL);
		CHECK_RC();
	}

	//  Get authenticated again with appl's Master key
  if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
  } else
  if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
	} else
  if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 0, appBKeyMaster0_24);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
	}

	if (!fLegacyCard)
	{		
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
	}

	//  Make key #5 the Change Keys Key.
	rc = SCardDesfire_ChangeKeySettings(hCard, 0x5F);
	CHECK_RC();

	//  Verify the new key settings.
	rc = SCardDesfire_GetKeySettings(hCard, &abDataBuffer[0], &abDataBuffer[1]);
	CHECK_RC();

	//  Change keys #1 through #4 using the three key procedure.
	//  Authentication with the Change Keys Key is now necessary for changing ordinary keys.
	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 5, appBChangeKey_16);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 1, appBKey1_16, abTestKeyDes1K);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 2, appBKey2_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 3, appBKey3_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 4, appBKey4_16, abNullKey);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 5, appBChangeKey_16);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 1, appBKey1_16, abTestKeyDes1K);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 2, appBKey2_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 3, appBKey3_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 4, appBKey4_16, abNullKey);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 5, appBChangeKey_24);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 1, appBKey1_24, abTestKeyDes1K);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 2, appBKey2_24, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 3, appBKey3_24, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 4, appBKey4_24, abNullKey);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 5, appBChangeKey_16);
		CHECK_RC();
		rc = SCardDesfire_ChangeKeyAes(hCard, 1, 0x33, appBKey1_16, abTestKeyDes1K);
		CHECK_RC();
		rc = SCardDesfire_ChangeKeyAes(hCard, 2, 0x55, appBKey2_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKeyAes(hCard, 3, 0x33, appBKey3_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKeyAes(hCard, 4, 0x55, appBKey4_16, abNullKey);
		CHECK_RC();
	}

	//  For demonstrating the three possible communication modes we create an instance
	//  of each basic file type:
	//  a Data file:  Read = 1, Write = 2, ReadWrite = 3, ChangeConfig = 4
	//  a Value file: Debit = 1, LimitedCredit = 3, Credit = 2, ChangeConfig = 4
	//  and a Linear Record file: Read = 1, Write = 3, ReadWrite = 2, ChangeConfig = 4
	//  Note: Must make sure that buffer is large enough for all data!
	bStdDataFileID--;
	rc = SCardDesfire_CreateStdDataFile(hCard, bStdDataFileID, 0, 0x1234, 100);
	CHECK_RC();
	rc = SCardDesfire_CreateValueFile(hCard, 4, 0, 0x1324, -987654321, -1000, -1000000, 1);
	CHECK_RC();
	rc = SCardDesfire_CreateLinearRecordFile(hCard, 1, 0, 0x1324, 25, 4);
	CHECK_RC();

	if (!fLegacyCard)
	{		
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
	}

	//  Do 7 transactions.
	for (iTransaction = 0; iTransaction < 7; iTransaction++)
	{
		//  Change the communication mode for all files.
		bCommMode = iTransaction % 3;
		if (bCommMode == 2) bCommMode = 3;

		//  This requires authentication with the key defined for changing the configuration.
		if (iTestMode == TM_LEGACY)
		{
			rc = SCardDesfire_Authenticate(hCard, 4, appBKey4_16);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES2K)
		{
			rc = SCardDesfire_AuthenticateIso(hCard, 4, appBKey4_16);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES3K)
		{
			rc = SCardDesfire_AuthenticateIso24(hCard, 4, appBKey4_24);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_AES)
		{
			rc = SCardDesfire_AuthenticateAes(hCard, 4, appBKey4_16);
			CHECK_RC();
		}

		rc = SCardDesfire_ChangeFileSettings(hCard, bStdDataFileID, bCommMode, 0x1234);
		CHECK_RC();
		rc = SCardDesfire_ChangeFileSettings(hCard, 4, bCommMode, 0x1324);
		CHECK_RC();
		rc = SCardDesfire_ChangeFileSettings(hCard, 1, bCommMode, 0x1324);
		CHECK_RC();

		//  Authenticate with the key which allows reading the files
		if (iTestMode == TM_LEGACY)
		{
			rc = SCardDesfire_Authenticate(hCard, 1, appBKey1_16);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES2K)
		{
			rc = SCardDesfire_AuthenticateIso(hCard, 1, appBKey1_16);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES3K)
		{
			rc = SCardDesfire_AuthenticateIso24(hCard, 1, appBKey1_24);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_AES)
		{
			rc = SCardDesfire_AuthenticateAes(hCard, 1, appBKey1_16);
			CHECK_RC();
		}

		if (iTransaction < 3)
		{
			//  Test different lengths
			for (eLength = 45; eLength < 65; eLength++)
			{
				rc = SCardDesfire_ReadData(hCard, bStdDataFileID, bCommMode, 100-eLength, eLength, abDataBuffer, &dwNbBytesRead);
				CHECK_RC();
			}
			for (eLength = 10; eLength <= 100; eLength+=10)
			{
				rc = SCardDesfire_ReadData(hCard, bStdDataFileID, bCommMode, 100-eLength, eLength, abDataBuffer, &dwNbBytesRead);
				CHECK_RC();
			}
		}


		rc = SCardDesfire_ReadData(hCard, bStdDataFileID, bCommMode, 0, 0, abDataBuffer, &dwNbBytesRead);
		CHECK_RC();

		//  Authenticate with the key which allows writing to files and increasing the Value
		//  file's value.
		if (iTestMode == TM_LEGACY)
		{
			rc = SCardDesfire_Authenticate(hCard, 2, appBKey2_16);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES2K)
		{
			rc = SCardDesfire_AuthenticateIso(hCard, 2, appBKey2_16);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES3K)
		{
			rc = SCardDesfire_AuthenticateIso24(hCard, 2, appBKey2_24);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_AES)
		{
			rc = SCardDesfire_AuthenticateAes(hCard, 2, appBKey2_16);
			CHECK_RC();
		}

		//  Write to the Standard Data file.
		for (i = 0; i < 100; i++)
			abDataBuffer[i] = ((iTransaction << 6) + i + 0);

		if (iTransaction < 3)
		{
			rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 0, 8, abDataBuffer);
			CHECK_RC();
			rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 0, 16, abDataBuffer);
			CHECK_RC();

			rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 0, 1, abDataBuffer);
			CHECK_RC();
			rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 0, 2, abDataBuffer);
			CHECK_RC();


			rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 0, 10, abDataBuffer);
			CHECK_RC();

			rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 10, 10, abDataBuffer);
			CHECK_RC();

			rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 20, 10, abDataBuffer);
			CHECK_RC();

			rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 30, 70, abDataBuffer);
			CHECK_RC();
		}

		rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 0, 100, abDataBuffer);
		CHECK_RC();

		sprintf((char *) abDataBuffer, " Transaction #%d ", iTransaction);
		rc = SCardDesfire_WriteData(hCard, bStdDataFileID, bCommMode, 5, strlen((char *) abDataBuffer), abDataBuffer);
		CHECK_RC();

		//  Write to the Linear Record file.
		//  If it turns out, that the file is full, clear its contents and repeat writing.
		for (i = 0; i < 2; i++)
		{
			rc = SCardDesfire_WriteRecord(hCard, 1, bCommMode, 0, 25,
																		(unsigned char *)
																		"0123456789012345678901234");
			if (rc == 0)
			{
				sprintf((char *) abDataBuffer, " Transaction #%d ", iTransaction);
				rc = SCardDesfire_WriteRecord(hCard, 1, bCommMode, 5,
																	strlen((char *) abDataBuffer),
																	abDataBuffer);
			}
			//  Was the WriteRecord successful ?
			if (rc == 0)
				break;              // yes

			// The current authentication status has been lost. We must get authenticated again
			if (iTestMode == TM_LEGACY)
			{
				rc = SCardDesfire_Authenticate(hCard, 2, appBKey2_16);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_3DES2K)
			{
				rc = SCardDesfire_AuthenticateIso(hCard, 2, appBKey2_16);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_3DES3K)
			{
				rc = SCardDesfire_AuthenticateIso24(hCard, 2, appBKey2_24);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_AES)
			{
				rc = SCardDesfire_AuthenticateAes(hCard, 2, appBKey2_16);
				CHECK_RC();
			}

			//  Clear the record file.
			rc = SCardDesfire_ClearRecordFile(hCard, 1);
			CHECK_RC();

			//  It is not allowed to write to the file before a CommitTransaction.
			//  So the following call will fail !
			rc = SCardDesfire_WriteRecord(hCard, 1, bCommMode, 5,
																		strlen((char *) abDataBuffer),
																		abDataBuffer);
			if (rc != (DFCARD_ERROR - DF_PERMISSION_DENIED))
				CHECK_RC();

			// The current authentication status has been lost. We must get authenticated again
			if (iTestMode == TM_LEGACY)
			{
				rc = SCardDesfire_Authenticate(hCard, 2, appBKey2_16);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_3DES2K)
			{
				rc = SCardDesfire_AuthenticateIso(hCard, 2, appBKey2_16);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_3DES3K)
			{
				rc = SCardDesfire_AuthenticateIso24(hCard, 2, appBKey2_24);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_AES)
			{
				rc = SCardDesfire_AuthenticateAes(hCard, 2, appBKey2_16);
				CHECK_RC();
			}

			//  Version 0.1 and above execute an implicit AbortTransaction after any
			//  error. Therefore the ClearRecordFile has been cancelled. We have to repeat it.
			rc = SCardDesfire_ClearRecordFile(hCard, 1);
			CHECK_RC();

			//  After the following the Record file is again ready for data.
			rc = SCardDesfire_CommitTransaction(hCard);
			CHECK_RC();
		}

		//  Modify the Value file.
		rc = SCardDesfire_Debit(hCard, 4, bCommMode, 1300);
		CHECK_RC();
		rc = SCardDesfire_Credit(hCard, 4, bCommMode, 20);
		CHECK_RC();
		rc = SCardDesfire_Debit(hCard, 4, bCommMode, 1700);
		CHECK_RC();

		//  Make all changes current.
		rc = SCardDesfire_CommitTransaction(hCard);
		CHECK_RC();

		//  Return the whole debited amount to the Value File.
		rc = SCardDesfire_LimitedCredit(hCard, 4, bCommMode, 3000);
		CHECK_RC();

		//  Make the change current.
		rc = SCardDesfire_CommitTransaction(hCard);
		CHECK_RC();


		//  Authenticate with the key which allows reading files and retrieving the Value
		//  file's value.
		if (iTestMode == TM_LEGACY)
		{
			rc = SCardDesfire_Authenticate(hCard, 1, appBKey1_16);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES2K)
		{
			rc = SCardDesfire_AuthenticateIso(hCard, 1, appBKey1_16);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES3K)
		{
			rc = SCardDesfire_AuthenticateIso24(hCard, 1, appBKey1_24);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_AES)
		{
			rc = SCardDesfire_AuthenticateAes(hCard, 1, appBKey1_16);
			CHECK_RC();
		}

		//  Read the first half of the Standard Data file's data specifying the exact
		//  number of bytes to read.
		rc = SCardDesfire_ReadData(hCard, bStdDataFileID, bCommMode, 0, 50, abDataBuffer, &dwNbBytesRead);
		CHECK_RC();
		//  Read the second half of the data with an open read (give me all data available).
		rc = SCardDesfire_ReadData(hCard, bStdDataFileID, bCommMode, 50, 0, abDataBuffer, &dwNbBytesRead);
		CHECK_RC();

		//  Get the Value file's current balance.
		rc = SCardDesfire_GetValue(hCard, 4, bCommMode, &lAmount);
		CHECK_RC();

		//  Get the number of records in the Linear Record file.
		rc = SCardDesfire_GetFileSettings(hCard, 1, 0, 0, 0, &unFileSettings);
		CHECK_RC();

		//  Read the oldest record from the file.
		rc = SCardDesfire_ReadRecords(hCard, 1, bCommMode,
															unFileSettings.stRecordFileSettings.
															eCurrNRecords - 1, 1,
															unFileSettings.stRecordFileSettings.
															eRecordSize, abDataBuffer, &eNRecordsRead);
		CHECK_RC();

		//  Read all records from the file.
		rc = SCardDesfire_ReadRecords(hCard, 1, bCommMode, 0, 0,
															unFileSettings.stRecordFileSettings.
															eRecordSize, abDataBuffer, &eNRecordsRead);
		CHECK_RC();
	}

	//  Get the file IDs of the current application.
	rc = SCardDesfire_GetFileIDs(hCard, 32, abFiles, &bNFilesFound);
	CHECK_RC();

	//  Get information about the application's files.
	//  Delete each file after retrieving information about it.
	for (iFileIndex = 0; iFileIndex < bNFilesFound; iFileIndex++)
	{
		rc =
			SCardDesfire_GetFileSettings(hCard, abFiles[iFileIndex], &bFileType,
																	&bCommMode, &wAccessRights,
																	&unFileSettings);
		CHECK_RC();
		rc = SCardDesfire_DeleteFile(hCard, abFiles[iFileIndex]);
		CHECK_RC();
	}

	//  Verify that there are no files in the application.
	rc = SCardDesfire_GetFileIDs(hCard, 32, abFiles, &bNFilesFound);
	CHECK_RC();

	//  Delete application B.
	//  This time we can (and do) use the Application Master Key.
	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 0, appBKeyMaster0_24);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 0, appBKeyMaster0_16);
		CHECK_RC();
	}

	rc = SCardDesfire_DeleteApplication(hCard, 0xBBBBBB);
	CHECK_RC();

	//  Verify that application B has been deleted.
	rc = SCardDesfire_GetApplicationIDs(hCard, 32, adwApplications, &bNApplicationsFound);
	CHECK_RC();

	if (dwFreeBytes < 2000)
	{
		rc = SCardDesfire_SelectApplication(hCard, 0);
		CHECK_RC();

    if (gbChangeMasterKey)
    {
		  if (iTestMode == TM_LEGACY)
		  {
			  rc = SCardDesfire_Authenticate(hCard, 0, abRootKey2K);
			  CHECK_RC();
		  } else
		  if (iTestMode == TM_ISO_3DES2K)
		  {
			  rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey2K);
			  CHECK_RC();
		  } else
		  if (iTestMode == TM_ISO_3DES3K)
		  {
			  rc = SCardDesfire_AuthenticateIso24(hCard, 0, abRootKey2K);
			  CHECK_RC();
		  } else
		  if (iTestMode == TM_ISO_AES)
		  {
			  rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
			  CHECK_RC();
		  }
    }

		rc = SCardDesfire_FormatPICC(hCard);
		CHECK_RC();

		if (iTestMode == TM_LEGACY)
		{
			rc = SCardDesfire_CreateApplication(hCard, 0xCCCCCC, 0xC0, 14);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES2K)
		{
			rc = SCardDesfire_CreateApplication(hCard, 0xCCCCCC, 0xC0, 14);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES3K)
		{
			rc = SCardDesfire_CreateApplication(hCard, 0xCCCCCC, 0xC0, DF_APPLSETTING2_3DES3K | 14);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_AES)
		{
			rc = SCardDesfire_CreateApplication(hCard, 0xCCCCCC, 0xC0, DF_APPLSETTING2_AES | 14);
			CHECK_RC();
		}
	}

	//  Working with a larger file (a Cyclic Record File) using application C:
	rc = SCardDesfire_SelectApplication(hCard, 0xCCCCCC);
	CHECK_RC();

	//  Define keys #1 and #2 using the Change Keys key (#12).
	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 12, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 1, appCKey1_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 2, appCKey2_16, abNullKey);
		CHECK_RC();
		// Back to master key
		rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 12, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 1, appCKey1_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey(hCard, 2, appCKey2_16, abNullKey);
		CHECK_RC();
		// Back to master key
		rc = SCardDesfire_AuthenticateIso(hCard, 0, abNullKey);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 12, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 1, appCKey1_24, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKey24(hCard, 2, appCKey2_24, abNullKey);
		CHECK_RC();
		// Back to master key
		rc = SCardDesfire_AuthenticateIso24(hCard, 0, abNullKey);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 12, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKeyAes(hCard, 1, 0, appCKey1_16, abNullKey);
		CHECK_RC();
		rc = SCardDesfire_ChangeKeyAes(hCard, 2, 0, appCKey2_16, abNullKey);
		CHECK_RC();
		// Back to master key
		rc = SCardDesfire_AuthenticateAes(hCard, 0, abNullKey);
		CHECK_RC();
	}

	//  Create the file - it has 16 records of 100 bytes
	rc = SCardDesfire_CreateCyclicRecordFile(hCard, 6, 0, 0x12E0, 100, 16);
	CHECK_RC();

	//  Do 50 transactions.
	for (iTransaction = 0; iTransaction < 50; iTransaction++)
	{
		//  Change the file's communication mode.
		bCommMode = (iTransaction % 3);
		if (bCommMode == 2)
			bCommMode++;

		//  This requires authentication with the key defined for changing the configuration.
		//  For this file it is the Application Master Key.
		if (iTestMode == TM_LEGACY)
		{
			rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES2K)
		{
			rc = SCardDesfire_AuthenticateIso(hCard, 0, abNullKey);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_3DES3K)
		{
			rc = SCardDesfire_AuthenticateIso24(hCard, 0, abNullKey);
			CHECK_RC();
		} else
		if (iTestMode == TM_ISO_AES)
		{
			rc = SCardDesfire_AuthenticateAes(hCard, 0, abNullKey);
			CHECK_RC();
		}

		rc = SCardDesfire_ChangeFileSettings(hCard, 6, bCommMode, 0x12E0);
		CHECK_RC();

		//  The file can be written using either the Write access key (0x.2..) or the
		//  Read/Write key (0x..E.).
		//  A valid authentication with key #2 causes the communication to take place in
		//  the mode defined with ChangeFileSettings above.
		//  Not being authenticated with key #2 causes the Read/Write access right to be used
		//  forcing the communication to plain mode.
		if (iTransaction & 4)
		{
			if (iTestMode == TM_LEGACY)
			{
				rc = SCardDesfire_Authenticate(hCard, 2, appCKey2_16);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_3DES2K)
			{
				rc = SCardDesfire_AuthenticateIso(hCard, 2, appCKey2_16);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_3DES3K)
			{
				rc = SCardDesfire_AuthenticateIso24(hCard, 2, appCKey2_24);
				CHECK_RC();
			} else
			if (iTestMode == TM_ISO_AES)
			{
				rc = SCardDesfire_AuthenticateAes(hCard, 2, appCKey2_16);
				CHECK_RC();
			}

		} else
			bCommMode = 0;

		//  Write a new record.
		memset(abDataBuffer, 0, 100);
		sprintf((char *) abDataBuffer, " Transaction #%d !TEST!?!TEST! ", iTransaction);
		rc = SCardDesfire_WriteRecord(hCard, 6, bCommMode, 0, 100, abDataBuffer);

		//  Cancel every 7th WriteRecord operation.
		if (iTransaction % 7 == 0)
			rc = SCardDesfire_AbortTransaction(hCard);
		else
			rc = SCardDesfire_CommitTransaction(hCard);

		if (rc != (DFCARD_ERROR - DF_NO_CHANGES))
			if (rc != (DFCARD_ERROR - DF_COMMAND_ABORTED))
				CHECK_RC();
	}

	//  Read each individual record using the designated read key.
	//  Note that increasing the offset lets us go back in the record history.
	//  Note also that only 16-1 records (one less than specified in CreateCyclicRecordFile)
	//  can be accessed.
	bCommMode = 1;

	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 1, appCKey1_16);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 1, appCKey1_16);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 1, appCKey1_24);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 1, appCKey1_16);
		CHECK_RC();
	}

	for (eOffset = 0; eOffset < 16; eOffset++)
	{
		rc = SCardDesfire_ReadRecords(hCard, 6, bCommMode, eOffset, 1, 100, abDataBuffer, &eNRecordsRead);
		if (rc != (DFCARD_ERROR - DF_BOUNDARY_ERROR))
			CHECK_RC();
	}

	//  Following the Boundary Error we lose the authentication - so we must get authenticated again
	if (iTestMode == TM_LEGACY)
	{
		rc = SCardDesfire_Authenticate(hCard, 1, appCKey1_16);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES2K)
	{
		rc = SCardDesfire_AuthenticateIso(hCard, 1, appCKey1_16);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_3DES3K)
	{
		rc = SCardDesfire_AuthenticateIso24(hCard, 1, appCKey1_24);
		CHECK_RC();
	} else
	if (iTestMode == TM_ISO_AES)
	{
		rc = SCardDesfire_AuthenticateAes(hCard, 1, appCKey1_16);
		CHECK_RC();
	}


	rc = SCardDesfire_ReadRecords(hCard, 6, bCommMode, 0, 1, 100, abDataBuffer, &eNRecordsRead);
	CHECK_RC();

	//  Read the whole record file with a single ReadRecords call.
	rc = SCardDesfire_ReadRecords(hCard, 6, bCommMode, 0, 15, 100, abDataBuffer, &eNRecordsRead);
	CHECK_RC();

	rc = SCardDesfire_ReadRecords(hCard, 6, bCommMode, 0, 0, 100, abDataBuffer, &eNRecordsRead );
	CHECK_RC();

	//  Change the master key settings such that master key authentication will be required
	//  for all card operations.
	//  Leave the ChangeConfiguration bit (bit 3) set to allow undoing the change.
	rc = SCardDesfire_SelectApplication(hCard, 0);
	CHECK_RC();

  if (gbChangeMasterKey)
  {
	  if (iTestMode == TM_LEGACY)
	  {
		  rc = SCardDesfire_Authenticate(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES2K)
	  {
		  rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES3K)
	  {
		  rc = SCardDesfire_AuthenticateIso24(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_AES)
	  {
		  rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  }

	  rc = SCardDesfire_ChangeKeySettings(hCard, 0x8);
	  CHECK_RC();

	  //  Clear the authentication state and try to get application IDs.
	  //  Also try to delete application C or to create a new application.
	  rc = SCardDesfire_SelectApplication(hCard, 0);
	  CHECK_RC();
	  //  The following three calls will report an Authentication Error (0xAE).
	  rc = SCardDesfire_GetApplicationIDs(hCard, 32, adwApplications, &bNApplicationsFound);
	  if (rc != (DFCARD_ERROR - DF_AUTHENTICATION_ERROR))
		  CHECK_RC();
	  rc = SCardDesfire_DeleteApplication(hCard, 0xCCCCCC);
	  if (rc != (DFCARD_ERROR - DF_AUTHENTICATION_ERROR))
		  CHECK_RC();

	  // This one must fail
	  rc = SCardDesfire_CreateApplication(hCard, 0xDDDDDD, 0xEF, 0);
	  if (rc != (DFCARD_ERROR - DF_AUTHENTICATION_ERROR))
		  CHECK_RC();

	  //  With authentication all three should work.
	  if (iTestMode == TM_LEGACY)
	  {
		  rc = SCardDesfire_Authenticate(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES2K)
	  {
		  rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES3K)
	  {
		  rc = SCardDesfire_AuthenticateIso24(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_AES)
	  {
		  rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  }
  } else
  {
		rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
		CHECK_RC();
  }

	rc = SCardDesfire_GetApplicationIDs(hCard, 32, adwApplications, &bNApplicationsFound);
	CHECK_RC();
	rc = SCardDesfire_DeleteApplication(hCard, 0xCCCCCC);
	CHECK_RC();
	rc = SCardDesfire_CreateApplication(hCard, 0xDDDDDD, 0xEF, 0);
	CHECK_RC();

  if (gbChangeMasterKey)
  {
	  //  Change the master key settings back to the default which is least restrictive.
	  rc = SCardDesfire_ChangeKeySettings(hCard, 0xF);
	  CHECK_RC();

	  //  Change the Card master key back to the 0 key.
	  if (iTestMode == TM_LEGACY)
	  {
		  rc = SCardDesfire_Authenticate(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES2K)
	  {
		  rc = SCardDesfire_AuthenticateIso(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  } else
	  if (iTestMode == TM_ISO_3DES3K)
	  {
		  rc = SCardDesfire_AuthenticateIso24(hCard, 0, abRootKey2K);
		  CHECK_RC();

		  //  for obvious reasons we cannot revert the config to 3DES2K, but a hook by AES permits it
		  rc = SCardDesfire_ChangeKeyAes(hCard, DF_APPLSETTING2_AES | 0, 0xAE, abRootKey2K, NULL);
		  CHECK_RC();
		  rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		  CHECK_RC();

	  } else
	  if (iTestMode == TM_ISO_AES)
	  {
		  rc = SCardDesfire_AuthenticateAes(hCard, 0, abRootKey2K);
		  CHECK_RC();
	  }

	  rc = SCardDesfire_ChangeKey(hCard, 0, abNullKey, NULL);
	  CHECK_RC();
  }

	//  Delete all demonstration data from the card.
	rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
	CHECK_RC();

	if (!fLegacyCard)
	{		
		rc = SCardDesfire_GetFreeMemory(hCard, &dwFreeBytes);
		CHECK_RC();
	}

	rc = SCardDesfire_FormatPICC(hCard);
	CHECK_RC();

  printf("\n");
	return TRUE;
}

