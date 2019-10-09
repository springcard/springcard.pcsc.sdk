/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2011 SpringCard SAS - www.springcard.com
   
  ref_desfire_benchmark_pcsc.c
  ---------------------------*

  This reference application test the overall communication speed
  with a NXP DESFire EV1 cards.
  
  JDA 14/03/2013 : initial release

*/
#include "pcsc_helpers.h"
#include "cardware/desfire/pcsc_desfire.h"
#include <time.h>

BOOL BenchDesfire(SCARDCONTEXT hContext, const char *szReader);
BOOL BenchDesfire_Ex(SCARDHANDLE hCard);

BOOL gbLoop = FALSE;
BOOL gbVerbose = FALSE;
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
	
	for (i=1; (int) i<argc; i++)
	{
		if (!strcmp(argv[i], "-v"))
			gbVerbose = TRUE;
		if (!strcmp(argv[i], "-l"))
			gbLoop = TRUE;
		if (!strcmp(argv[i], "-r"))
			gbRandomSleep = TRUE;		
	}

	if (gbRandomSleep)
	{
		srand((int) time(NULL));
	}

	/*
	* Welcome message, check parameters
	* ---------------------------------
	*/
	printf("SpringCard SDK for PC/SC reader\n");
	printf("\n");
	printf("NXP DESFIRE benchmark\n");
	printf("---------------------\n");
	printf("www.springcard.com\n\n");
	printf(__DATE__ " " __TIME__);
	printf("\n");
	printf("Press <Ctrl+C> to exit\n\n");

	/*
	* Get a handle to the PC/SC resource manager
	* ------------------------------------------
	*/
	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if (rc != SCARD_S_SUCCESS)
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
		}
		else if (rc != SCARD_S_SUCCESS)
		{
			printf("SCardListReaders error %08lX\n",rc);
			break;
		}
		else
		{
			/* Track events on found readers. */
			LPTSTR pReader = szReaders;

			for (dwReaders=0; dwReaders<MAXIMUM_SMARTCARD_READERS; dwReaders++)
			{
				/* End of multi-string array */
				if (*pReader == '\0')
					break;

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
			}
			else
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
				}
				else
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
			}
			else
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
				if (BenchDesfire(hContext, rgscState[i].szReader))
					printf("Bench terminated, success\n");
				else
					printf("Bench terminated, error\n");
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

BOOL BenchDesfire(SCARDCONTEXT hContext, const char *szReader)
{
	SCARDHANDLE hCard;
	DWORD dwProtocol;
	LONG rc;
	BOOL f = FALSE;

	printf("Benchmarking the Desfire library on this card...\n");

	/*
	* Connect to the card, accept either T=0 or T=1
	* ---------------------------------------------
	*/
	rc = SCardConnect(hContext,
		szReader,
		SCARD_SHARE_EXCLUSIVE,
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

	do
	{
		f = BenchDesfire_Ex(hCard);
	} while (f && gbLoop);

	SCardDesfire_DetachLibrary(hCard);
	SCardDisconnect(hCard, SCARD_EJECT_CARD);

	printf("\n\n\n");
	return f;
}



static void clock_increment_diff(struct timeval *to, const struct timeval *inc, const struct timeval *dec)
{
  to->tv_sec += inc->tv_sec;
  to->tv_sec -= dec->tv_sec;

  to->tv_usec += inc->tv_usec;
  to->tv_usec -= dec->tv_usec;
  
  while (to->tv_usec > 1000000)
  {
    to->tv_usec -= 1000000;
	to->tv_sec += 1;
  }
}

static unsigned long clock_to_ms(const struct timeval *c)
{
  unsigned long r;
  
  r  = c->tv_sec * 1000;
  r += c->tv_usec / 1000;
  
  return r;
}

static float clock_bitrate(const struct timeval *c, unsigned long nBytes)
{
  float r, t;
  
  t  = (float) c->tv_sec;
  t += (float) c->tv_usec / (float) 1000000.0;
  
  r  = (float) nBytes / t;
  r *= 8.0; /* bytes -> bits */
  r /= 1024.0;
  
  return r;
}

#ifdef WIN32
int gettimeofday(struct timeval* p, void* tz)
{
  ULARGE_INTEGER ul; // As specified on MSDN.
  FILETIME ft;

  // Returns a 64-bit value representing the number of
  // 100-nanosecond intervals since January 1, 1601 (UTC).
  GetSystemTimeAsFileTime(&ft);

  // Fill ULARGE_INTEGER low and high parts.
  ul.LowPart = ft.dwLowDateTime;
  ul.HighPart = ft.dwHighDateTime;
  // Convert to microseconds.
  ul.QuadPart /= 10UL;
  // Remove Windows to UNIX Epoch delta.
  ul.QuadPart -= 11644473600000000UL;
  // Modulo to retrieve the microseconds.
  p->tv_usec = (long) (ul.QuadPart % 1000000UL);
  // Divide to retrieve the seconds.
  p->tv_sec = (long) (ul.QuadPart / 1000000UL);

  return 0;
}
#endif

#ifdef __linux__
#include <inttypes.h>
#include <sys/time.h>
#include <sys/resource.h>
#endif

#ifdef WIN32
#define CHECK_RC() { if (rc!=0) { printf("\nline %d - failed (%08lX / %d) %s\n", __LINE__-1, rc, rc, SCardDesfire_GetErrorMessage(rc)); return FALSE; } }
#endif

#ifdef __linux__
#define CHECK_RC() { if (rc!=0) { printf("\nline %d - failed (%08lX / %d) %s\n", __LINE__-1, rc, rc, SCardDesfire_GetErrorMessage(rc)); return FALSE; } }
#endif

const BYTE abNullKey[24]         = { 0 };

const BYTE abRootKey1K[24]       = { "ABCDEFGHABCDEFGHABCDEFGH" };
const BYTE abRootKey2K[24]       = { "Card Master Key!Card Mas" };
const BYTE abRootKey3K[24]       = { "Card Master Key!        " };

BOOL BenchDesfire_Ex(SCARDHANDLE hCard)
{
	DF_VERSION_INFO stVersionInfo;
	WORD wDesFireSoftwareVersion;
	LONG rc;
	BYTE bKeyVersion;
	BYTE bNApplicationsFound;
	BYTE bStdDataFileID = 1;
	DWORD dwNbBytesRead;
	BYTE abDataBuffer[2048];

	int i;
	DWORD nBytes = 0;
	struct timeval t_read;
	struct timeval t_write;
	struct timeval t0, t1;
	
	BOOL may_reconnect = TRUE;

again:
	
	t_read.tv_sec = t_read.tv_usec = 0;
	t_write.tv_sec = t_write.tv_usec = 0;
  
	//  Get the card's version information.
	rc = SCardDesfire_GetVersion(hCard, &stVersionInfo);
	if ((rc == (DFCARD_ERROR-DF_ILLEGAL_COMMAND_CODE)) && (may_reconnect))
	{
		DWORD dwProtocol;
		LONG rc = SCardReconnect(hCard,
		                         SCARD_SHARE_EXCLUSIVE,
					          SCARD_PROTOCOL_T0|SCARD_PROTOCOL_T1,
					          SCARD_RESET_CARD,
					          &dwProtocol);
		if (rc != SCARD_S_SUCCESS)
		{
			printf("Failed to reset the card\n");
			return rc;
		}
		may_reconnect = FALSE;
		goto again;
	}
	
	CHECK_RC();
	wDesFireSoftwareVersion = (unsigned int) ((unsigned int) stVersionInfo.bSwMajorVersion << 8 | stVersionInfo.bSwMinorVersion);
	printf("\n");

  if (wDesFireSoftwareVersion < 0x0100)
  {
	  printf("This is a Desfire EV0!\n");

  } else
	{
    printf("This is a Desfire EV1!\n");
  }
   
	//  After activating a DesFire card the currently selected 'application' is the card
	//  iTestMode (AID = 0). Therefore the next command is redundant.
	//  We use the command to check that the card is responding correctly.
	rc = SCardDesfire_SelectApplication(hCard, 0);
	CHECK_RC();

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

	  //  Authenticate again
	  rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
	  CHECK_RC();

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

	//  Create one application
	//  It has no keys and its key settings are least restrictive.
	//  Note: since there are no keys, the Change Keys key settting must be assigned
	//        the value 0xE or 0xF.
  rc = SCardDesfire_CreateApplication(hCard, 0xAAAAAA, 0xFF, 0);
	CHECK_RC();

	//  Create one file in application A:
	//  Since the application does not have any keys associated with it, the file access rights
	//  settings 0xE (public access granted) and 0xF (access denied) are the only permissable
	//  ones.
	rc = SCardDesfire_SelectApplication(hCard, 0xAAAAAA);
	CHECK_RC();

	//  Create a the Standard Data file.
	rc = SCardDesfire_CreateStdDataFile(hCard, bStdDataFileID, 0, 0xEEEE, 2048);
	CHECK_RC();

	//  Fill the Standard Data file with some data.
	memset(abDataBuffer, 0xDA, sizeof(abDataBuffer));

  printf("Performing benchmark, please wait...\n");

  for (i=0; i<16; i++)
  {
    //  Write the file content at once
    gettimeofday(&t0, NULL);
	  rc = SCardDesfire_WriteData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 0, 2048, abDataBuffer);
    gettimeofday(&t1, NULL);
	  CHECK_RC();
    clock_increment_diff(&t_write, &t1, &t0);
    printf("."); fflush(NULL);
  
	  //  Read the file content at once
    gettimeofday(&t0, NULL);
	  rc = SCardDesfire_ReadData(hCard, bStdDataFileID, DF_COMM_MODE_PLAIN, 0, 2048, abDataBuffer, &dwNbBytesRead);
    gettimeofday(&t1, NULL);
	  CHECK_RC();
    clock_increment_diff(&t_read, &t1, &t0);
    printf("."); fflush(NULL);

    nBytes += 2048;
  }

  printf("\nCleanup...\n");
  
	//  Back to the root application
	rc = SCardDesfire_SelectApplication(hCard, 0);
	CHECK_RC();

	//  Get authenticated
	rc = SCardDesfire_Authenticate(hCard, 0, abNullKey);
	CHECK_RC();

	//  Delete all demonstration data from the card.
	rc = SCardDesfire_FormatPICC(hCard);
	CHECK_RC();

  printf("Read : %dB received in %ldms (%fkbit/s)\n", nBytes, clock_to_ms(&t_read), clock_bitrate(&t_read, nBytes));
  printf("Write: %dB sent in %ldms (%fkbit/s)\n", nBytes, clock_to_ms(&t_write), clock_bitrate(&t_write, nBytes));
  
#ifdef __linux__
  {
    struct rusage usage;
    getrusage(RUSAGE_SELF, &usage);
    printf("user CPU time:                %ld.%06ld\n", usage.ru_utime.tv_sec, usage.ru_utime.tv_usec);
	printf("system CPU time:              %ld.%06ld\n", usage.ru_stime.tv_sec, usage.ru_stime.tv_usec);
	printf("voluntary context switches:   %ld\n", usage.ru_nvcsw);
	printf("involuntary context switches: %ld\n", usage.ru_nivcsw);
  }
#endif

  printf("\n");
	return TRUE;
}

