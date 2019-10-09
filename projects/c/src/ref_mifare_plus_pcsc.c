/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2011 SpringCard SAS - www.springcard.com
   
  ref_mifplus_pcsc.c
  ------------------

  This is the reference applications that validates the Pcsc_MifPlus.dll
  with NXP Mifare Plus cards.
  
  JDA 23/02/2011 : first release
	

	IMPORTANT NOTICE:

	Due to the difficulties to get the different cards in the Mifare
	Plus family, this software has been fully tested only with some
	Mifare Plus X 4K cards.
	Please signal us any trouble that may appear with Mifare Plus S
	or with 2K cards.

	To date, only the READ / WRITE features of Level 3 are supported.
	Level 2 is not supported (as it doesn't exist in Mifare Plus S).
	VALUE features will be implemented in a later version.

*/
#include "pcsc_helpers.h"
#include "mifare_helpers.h"
#include "cardware/mifare/plus/pcsc_mifplus.h"

BOOL gbSkipConfirm = FALSE;
BOOL gbOverwrite = FALSE;
BOOL gbDoL0  = FALSE;
BOOL gbDoL1  = FALSE;
BOOL gbDoL2  = FALSE;
BOOL gbDoL3  = FALSE;
BOOL gbEnRid = FALSE;
BOOL gbGoL1  = FALSE;
BOOL gbGoL2  = FALSE;
BOOL gbGoL3  = FALSE;
BOOL gbTcl = FALSE;
BOOL gbGetVersion = FALSE;
BOOL gbGetSignature = FALSE;
BOOL gbCustomTest = FALSE;
BYTE gbSectors = 0;

BOOL MifPlus_Main(SCARDCONTEXT hContext, const char *reader, const BYTE atr[], DWORD atrlen);
BOOL MifPlus_L0_Perso(SCARDHANDLE hCard);
BOOL MifPlus_L1_Test(SCARDHANDLE hCard, const BYTE atr[]);
BOOL MifPlus_L1_CustomTest(SCARDHANDLE hCard, const BYTE atr[]);
BOOL MifPlus_L1_Go_L2(SCARDHANDLE hCard);
BOOL MifPlus_L1_Go_L3(SCARDHANDLE hCard);
BOOL MifPlus_L2_Test(SCARDHANDLE hCard);
BOOL MifPlus_L2_Go_L3(SCARDHANDLE hCard);
BOOL MifPlus_L3_Test(SCARDHANDLE hCard);
BOOL MifPlus_GetVersion(SCARDHANDLE hCard);
BOOL MifPlus_GetSignature(SCARDHANDLE hCard);
BOOL MifPlus_FirstAuthentication(SCARDHANDLE hCard, WORD key_address);
BOOL MifPlus_FollowingAuthentication(SCARDHANDLE hCard, WORD key_address, BOOL *last_block_reached);

static BOOL confirm(void)
{  
	char s[16];
	unsigned int i;

  for (;;)
	{
		printf("\nPlease enter YES to confirm or NO to exit: ");
		if (fgets(s, sizeof(s), stdin))
		{				  
			for (i=0; i<sizeof(s); i++)
			{
				if (s[i] == '\r') s[i] = '\0';
				if (s[i] == '\n') s[i] = '\0';
				if (s[i] == '\0') break;
				s[i] |= 0x40;
			}
			if (!stricmp(s, "yes"))
			{
				printf("\n");
				return TRUE;
			}
			if (!stricmp(s, "no"))
			{
				printf("\n");
				return FALSE;
			}
		}
	}
}

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
  
	for (i=1; i<(DWORD) argc; i++)
	{
		if (!strcmp(argv[i], "-f"))
			gbOverwrite = TRUE;
		if (!strcmp(argv[i], "-1K"))
			gbSectors = 16;
		if (!strcmp(argv[i], "-2K"))
			gbSectors = 32;
		if (!strcmp(argv[i], "-4K"))
			gbSectors = 40;
		if (!strcmp(argv[i], "-tl0"))
			gbDoL0 = TRUE;
		if (!strcmp(argv[i], "-tl1"))
			gbDoL1 = TRUE;
		if (!strcmp(argv[i], "-tl2"))
			gbDoL2 = TRUE;
		if (!strcmp(argv[i], "-tl3"))
			gbDoL3 = TRUE;
		if (!strcmp(argv[i], "-gl1"))
			gbGoL1 = TRUE;
		if (!strcmp(argv[i], "-gl2"))
			gbGoL2 = TRUE;
		if (!strcmp(argv[i], "-gl3"))
			gbGoL3 = TRUE;
		if (!strcmp(argv[i], "-rid"))
			gbEnRid = TRUE;
		if (!strcmp(argv[i], "-y"))
			gbSkipConfirm = TRUE;
		if (!strcmp(argv[i], "-tcl"))
			gbTcl = TRUE;
		if (!strcmp(argv[i], "-get-version"))
			gbGetVersion = TRUE;
		if (!strcmp(argv[i], "-get-signature"))
			gbGetSignature = TRUE;
		if (!strcmp(argv[i], "-custom-test"))
			gbCustomTest = TRUE;
	}

	/*
	 * Welcome message, check parameters
	 * ---------------------------------
	 */
	printf("SpringCard SDK for PC/SC\n");
	printf("\n");
	printf("NXP MIFARE Plus reference demo\n");
	printf("------------------------------\n");
	printf("www.springcard.com\n\n");
	printf("\n");
	printf("Press <Ctrl+C> to exit\n\n");

	/*
	* Get a handle to the PC/SC resource manager
	* ------------------------------------------
	*/
	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if (rc)
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
		else if (rc)
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
		
		if (rc)
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
				 * Work with this card
				 * -------------------
				 */
				MifPlus_Main(hContext, rgscState[i].szReader, rgscState[i].rgbAtr, rgscState[i].cbAtr);
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


BOOL MifPlus_Main(SCARDCONTEXT hContext, const char *reader, const BYTE atr[], DWORD atrlen)
{
	SCARDHANDLE hCard;
	DWORD dwProtocol;
	BYTE bCardLevel;
	BYTE abCardID[32];
	WORD wCardIDLen;
	WORD i;
	LONG rc;
	BOOL f = FALSE;

	(void) atrlen;

	/*
	 * Connect to the card, accept either T=0 or T=1
	 * ---------------------------------------------
	 */
	rc = SCardConnect(hContext,
		reader,
		SCARD_SHARE_SHARED,
		SCARD_PROTOCOL_T0|SCARD_PROTOCOL_T1,
		&hCard,
		&dwProtocol);
	if (rc)
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

	/* Link the Mifare Plus library to this card */
	/* ----------------------------------------- */

	rc = SCardMifPlus_AttachLibrary(hCard);
	if (rc)
	{
		printf("\tSCardMifPlus_AttachLibrary error %08lX\n",rc);
		goto done;
	}

	/* Retrieve and display the ISO 14443-3 A UID (depending on the configuration     */
	/* of the card, it may be be either a random number or its actual UID of the card */
	rc = SCardMifPlus_GetData(hCard, 0, 0, abCardID, sizeof(abCardID), &wCardIDLen);
	if (rc)
	{
		printf("\tSCardMifPlus_GetData error %08lX\n",rc);
		goto done;
	}

	printf("\tID=");
	for (i=0; i<wCardIDLen; i++)
		printf("%02X", abCardID[i]);
	printf("\n");

	/* Retrieve the security Level of the card */
	rc = SCardMifPlus_GetCardLevel(hCard, &bCardLevel);
	if (rc)
	{
		printf("\tSCardMifPlus_GetCardLevel error %08lX\n",rc);
		goto done;
	}

	switch (bCardLevel)
	{
		case 0:
			/* Handling of a card at Level 0 */
			/* ----------------------------- */

			printf("\tThis card seems to be a Mifare Plus at Level 0 (out of factory)\n");

			if (!gbDoL0 && !gbGoL1 && !gbGetVersion && !gbGetSignature)
			{
				printf("\n");
				printf("Nothing to do here - Try running the program specifying:\n");
				printf("\t-tl0 [-rid] to perform the Level 0-related tests (Write Perso)\n");
				printf("\t            -rid is optional and enables Random ID\n");
				printf("\t-gl1 to switch the card from Level 0 to Level 1 (Commit Perso)\n");
				printf("\t-get-version to read the version (EV1 only)\n");
				printf("\t-get-signature to read the signature (EV1 only)\n");
				printf("\n");
			}

			if (gbGetVersion)
			{
				f = MifPlus_GetVersion(hCard);
				if (!f)
					break;
			}

			if (gbGetSignature)
			{
				f = MifPlus_GetSignature(hCard);
				if (!f)
					break;
			}

			if (gbDoL0)
			{
				f = MifPlus_L0_Perso(hCard);
				if (!f)
					break;
			}

			if (gbGoL1)
			{
				/* The user asks to enter Level 1 */
				if (!gbSkipConfirm)
				{
					printf("We're now ready to COMMIT the personalization of this card and have it enter\n");
					printf("Level 1.\n");
					printf("Once this is done, IT IS NOT POSSIBLE to go back to Level 0, nor to change\n");
					printf("the keys neither the global parameters that have been defined.\n");
					printf("Are you really sure you want to do this ?\n");
					if (!confirm())
						exit(EXIT_FAILURE);
				}

				/* Commit the personalization. Card enters Level 1. THIS IS NOT REVERSIBLE */
				rc = SCardMifPlus_CommitPerso(hCard);
				if (rc)
				{
					printf("SCardMifPlus_CommitPerso failed, rc=%08lX (%ld)\n", rc, rc);
					f = FALSE;
					break;
				}
				else
				{
					f = TRUE;
				}

				printf("The card has been personalized, you must now remove it from the reader.\n\n");
			}
		break;

		case 1:
			/* Handling of a card at Level 1 */
			/* ----------------------------- */

			printf("\tThis card seems to be a Mifare Plus at Level 1\n\t(Mifare Classic emulation)\n");

			if (!gbDoL1 && !gbGoL2 && !gbGoL3 && !gbGetVersion && !gbGetSignature && !gbCustomTest)
			{
				printf("\n");
				printf("Nothing to do here - Try running the program specifying:\n");
				printf("\t-tl1 to perform the Level 1-related tests\n\t(Mifare emulation and AES authentication)\n");
				printf("\t-gl2 to switch the card from Level 1 to Level 2\n");
				printf("\t-gl3 to switch the card from Level 1 to Level 3\n");
				printf("\t-get-version to read the version (EV1 only)\n");
				printf("\t-get-signature to read the signature (EV1 only)\n");
				printf("\n");
			}

			if (gbTcl)
			{
				/* Some operations must be performed in T=CL */
				rc = SCardMifPlus_EnterTcl(hCard);
				if (rc)
				{
					printf("SCardMifPlus_EnterTcl failed, rc=%08lX (%ld)\n", rc, rc);
					return FALSE;
				}
			}

			if (gbGetVersion)
			{
				f = MifPlus_GetVersion(hCard);
				if (!f)
					break;
			}

			if (gbGetSignature)
			{
				f = MifPlus_GetSignature(hCard);
				if (!f)
					break;
			}

			if (gbCustomTest)
			{
				/* Test */
				f = MifPlus_L1_CustomTest(hCard, atr);
				if (!f)
					break;
			}

			if (gbDoL1)
			{
				/* Test */
				f = MifPlus_L1_Test(hCard, atr);
				if (!f)
					break;
			}

			if (gbGoL2)
			{
				/* The user asks to enter Level 2 */
				f = MifPlus_L1_Go_L2(hCard);
				if (!f)
					break;
			}
			else if (gbGoL3)
			{
				/* The user asks to enter Level 3 */
				f = MifPlus_L1_Go_L3(hCard);
				if (!f)
					break;
			}

		break;

		case 2  :
			/* Handling of a card at Level 2 */
			/* ----------------------------- */

			printf("\tThis card seems to be a Mifare Plus at Level 2\n");

			if (!gbDoL2 && !gbGoL3 && !gbGetVersion && !gbGetSignature)
			{
				printf("\n");
				printf("Nothing to do here - Try running the program specifying:\n");
				printf("\t-tl2 to perform the Level 2-related tests\n\n");
				printf("\t-gl3 to switch the card from Level 2 to Level 3\n");
				printf("\t-get-version to read the version (EV1 only)\n");
				printf("\t-get-signature to read the signature (EV1 only)\n");
				printf("\n");
			}

			if (gbGetVersion)
			{
				f = MifPlus_GetVersion(hCard);
				if (!f)
					break;
			}

			if (gbGetSignature)
			{
				f = MifPlus_GetSignature(hCard);
				if (!f)
					break;
			}

			if (gbDoL2)
			{
				/* Test */
				f = MifPlus_L2_Test(hCard);
				if (!f)
					break;
			}

			if (gbGoL3)
			{
				/* The user asks to enter Level 3 */
				f = MifPlus_L2_Go_L3(hCard);
				if (!f)
					break;
			}

		break;


		case 3  :
			/* Handling of a card at Level 3 */
			/* ----------------------------- */

			printf("\tThis card seems to be a Mifare Plus at Level 3\n");

			if (!gbDoL3 && !gbGetVersion && !gbGetSignature)
			{
				printf("\n");
				printf("Nothing to do here - Try running the program specifying:\n");
				printf("\t-tl3 to perform the Level 3-related tests\n\n");
				printf("\t-get-version to read the version (EV1 only)\n");
				printf("\t-get-signature to read the signature (EV1 only)\n");
				printf("\n");
			}

			if (gbGetVersion)
			{
				f = MifPlus_GetVersion(hCard);
				if (!f)
					break;
			}

			if (gbGetSignature)
			{
				f = MifPlus_GetSignature(hCard);
				if (!f)
					break;
			}

			if (gbDoL3)
			{
				/* Test */
				f = MifPlus_L3_Test(hCard);
				if (!f)
					break;
			}

		break;

		default :
			printf("\tLevel %d is unsupported!\n", bCardLevel);
	}

	if (f)
		printf("Success!\n");
 
	/* Unlink the Mifare Plus library */
	/* ------------------------------ */

done:
	SCardMifPlus_DetachLibrary(hCard);
	SCardDisconnect(hCard, SCARD_RESET_CARD);

	printf("\n\n\n");
	return f;
}

/*
 ***************************************************************************** 
 *
 * Initial personalisation of the Level 0 card
 *
 ***************************************************************************** 
 *
 * The card comes out of factory at 'Level 0'. All user-defined keys and
 * parameters shall be defined before entering one of the upper levels.
 *
 * We use an array of MIFPLUS_PERSO_ST structure to define the keys and
 * parameters to be writen when function MifPlus_L0_Perso runs.
 * Adapt the content of this array to your own values.
 *
 * DO NOT USE THOSE SAMPLE KEYS AND PARAMETERS IN A REAL-WORLD APPLICATION.
 *
 * ONCE AGAIN, REMEMBER THAT MOST OF THE KEYS AND PARAMETERS CAN'T BE
 * CHANGED AFTERWARDS. SWITCHING TO AN UPPER LEVEL IS NOT REVERSIBLE.
 * DO NOT USE THIS SOFTWARE AND YOUR CARD WITHOUT KNOWING WHAT IT WILL BE
 * DOING, AND WHY YOU ACTUALLY WANT TO HAVE IT DONE.
 *
 */

typedef struct
{
	BYTE option;
	WORD address;
	BYTE value[16];
} MIFPLUS_PERSO_ST;

/*
 * The MIFPLUS_PERSO_ST.option field tells whether the option is applicable
 * to Mifare Plus X or Mifare Plus S, or both, and whether it is applicable
 * to Mifare Plus 2K or Mifare Plus 4K.
 * The following defines shall be used:
 */

/* Option to Mifare Plus X only */
#define OPT_TYPE_X 0x02
/* Option applicable to Mifare Plus 4K only */
#define OPT_SIZE_4K 0x20

#define OPT_ANY 0x00

/*
 * Our array of MIFPLUS_PERSO_ST holds all the keys and parameter we
 * want to put in our card, for test and demonstration purpose.
 * Adapt the content of this array to your own values.
 * 
 * DO NOT USE THOSE SAMPLE KEYS AND PARAMETERS IN A REAL-WORLD APPLICATION.
 *
 * Diversification of the keys, using the cards' serial numbers as seed, is
 * a good idea to enhance the overall security of your system.
 * Anyway, ALWAYS KEEP 'VC POLLING ENC' AND 'VC POLLING MAC' KEYS AS CONSTANT,
 * because once randon-id and VC feature are activated, you won't be able to
 * retrieve the card's serial number without a successfull VC handshaking...
 */
static const MIFPLUS_PERSO_ST mifplus_perso[] =
{
	/* Card Master Key */
	{ OPT_ANY,    0x9000, { "Card master key " }},
	/* Card Configuration Key */
	{ OPT_ANY,    0x9001, { "Card config key " }},
	/* Level 2 switch key */
	{ OPT_TYPE_X, 0x9002, { "Go to level 2!  " }},
	/* Level 3 switch key */
	{ OPT_ANY,    0x9003, { "Go to level 3!  " }},
	/* SL1 Card Authentication Key */
	{ OPT_ANY,    0x9004, { "Level 1 AES auth" }},
	/* Select VC key */
	{ OPT_TYPE_X, 0xA000, { "Select VC key   " }},
	/* Proximity check key */
	{ OPT_TYPE_X, 0xA001, { "Proximity check " }},

	/* VC polling ENC key - must be constant among the system */
	{ OPT_TYPE_X, 0xA080, { "VC polling ENC  " }},
	/* VC polling MAC key - must be constant among the system */
	{ OPT_TYPE_X, 0xA081, { "VC polling MAC  " }},

	/* AES sector keys - sectors 0 to 31 */
	{ OPT_ANY, 0x4000, { "AES key A sect00" }}, { OPT_ANY, 0x4001, { "AES key B sect00" }},
	{ OPT_ANY, 0x4002, { "AES key A sect01" }}, { OPT_ANY, 0x4003, { "AES key B sect01" }},
	{ OPT_ANY, 0x4004, { "AES key A sect02" }}, { OPT_ANY, 0x4005, { "AES key B sect02" }},
	{ OPT_ANY, 0x4006, { "AES key A sect03" }}, { OPT_ANY, 0x4007, { "AES key B sect03" }},
	{ OPT_ANY, 0x4008, { "AES key A sect04" }}, { OPT_ANY, 0x4009, { "AES key B sect04" }},
	{ OPT_ANY, 0x400A, { "AES key A sect05" }}, { OPT_ANY, 0x400B, { "AES key B sect05" }},
	{ OPT_ANY, 0x400C, { "AES key A sect06" }}, { OPT_ANY, 0x400D, { "AES key B sect06" }},
	{ OPT_ANY, 0x400E, { "AES key A sect07" }}, { OPT_ANY, 0x400F, { "AES key B sect07" }},
	{ OPT_ANY, 0x4010, { "AES key A sect08" }}, { OPT_ANY, 0x4011, { "AES key B sect08" }},
	{ OPT_ANY, 0x4012, { "AES key A sect09" }}, { OPT_ANY, 0x4013, { "AES key B sect09" }},
	{ OPT_ANY, 0x4014, { "AES key A sect10" }}, { OPT_ANY, 0x4015, { "AES key B sect10" }},
	{ OPT_ANY, 0x4016, { "AES key A sect11" }}, { OPT_ANY, 0x4017, { "AES key B sect11" }},
	{ OPT_ANY, 0x4018, { "AES key A sect12" }}, { OPT_ANY, 0x4019, { "AES key B sect12" }},
	{ OPT_ANY, 0x401A, { "AES key A sect13" }}, { OPT_ANY, 0x401B, { "AES key B sect13" }},
	{ OPT_ANY, 0x401C, { "AES key A sect14" }}, { OPT_ANY, 0x401D, { "AES key B sect14" }},
	{ OPT_ANY, 0x401E, { "AES key A sect15" }}, { OPT_ANY, 0x401F, { "AES key B sect15" }},
	{ OPT_ANY, 0x4020, { "AES key A sect16" }}, { OPT_ANY, 0x4021, { "AES key B sect16" }},
	{ OPT_ANY, 0x4022, { "AES key A sect17" }}, { OPT_ANY, 0x4023, { "AES key B sect17" }},
	{ OPT_ANY, 0x4024, { "AES key A sect18" }}, { OPT_ANY, 0x4025, { "AES key B sect18" }},
	{ OPT_ANY, 0x4026, { "AES key A sect19" }}, { OPT_ANY, 0x4027, { "AES key B sect19" }},
	{ OPT_ANY, 0x4028, { "AES key A sect20" }}, { OPT_ANY, 0x4029, { "AES key B sect20" }},
	{ OPT_ANY, 0x402A, { "AES key A sect21" }}, { OPT_ANY, 0x402B, { "AES key B sect21" }},
	{ OPT_ANY, 0x402C, { "AES key A sect22" }}, { OPT_ANY, 0x402D, { "AES key B sect22" }},
	{ OPT_ANY, 0x402E, { "AES key A sect23" }}, { OPT_ANY, 0x402F, { "AES key B sect23" }},
	{ OPT_ANY, 0x4030, { "AES key A sect24" }}, { OPT_ANY, 0x4031, { "AES key B sect24" }},
	{ OPT_ANY, 0x4032, { "AES key A sect25" }}, { OPT_ANY, 0x4033, { "AES key B sect25" }},
	{ OPT_ANY, 0x4034, { "AES key A sect26" }}, { OPT_ANY, 0x4035, { "AES key B sect26" }},
	{ OPT_ANY, 0x4036, { "AES key A sect27" }}, { OPT_ANY, 0x4037, { "AES key B sect27" }},
	{ OPT_ANY, 0x4038, { "AES key A sect29" }}, { OPT_ANY, 0x4039, { "AES key B sect28" }},
	{ OPT_ANY, 0x403A, { "AES key A sect29" }}, { OPT_ANY, 0x403B, { "AES key B sect29" }},
	{ OPT_ANY, 0x403C, { "AES key A sect30" }}, { OPT_ANY, 0x403D, { "AES key B sect30" }},
	{ OPT_ANY, 0x403E, { "AES key A sect31" }}, { OPT_ANY, 0x403F, { "AES key B sect31" }},
	/* AES sector keys - sectors 32 to 39 */
	{ OPT_SIZE_4K, 0x4040, { "AES key A sect32" }}, { OPT_SIZE_4K, 0x4041, { "AES key B sect32" }},
	{ OPT_SIZE_4K, 0x4042, { "AES key A sect33" }}, { OPT_SIZE_4K, 0x4043, { "AES key B sect33" }},
	{ OPT_SIZE_4K, 0x4044, { "AES key A sect34" }}, { OPT_SIZE_4K, 0x4045, { "AES key B sect34" }},
	{ OPT_SIZE_4K, 0x4046, { "AES key A sect35" }}, { OPT_SIZE_4K, 0x4047, { "AES key B sect35" }},
	{ OPT_SIZE_4K, 0x4048, { "AES key A sect36" }}, { OPT_SIZE_4K, 0x4049, { "AES key B sect36" }},
	{ OPT_SIZE_4K, 0x404A, { "AES key A sect37" }}, { OPT_SIZE_4K, 0x404B, { "AES key B sect37" }},
	{ OPT_SIZE_4K, 0x404C, { "AES key A sect38" }}, { OPT_SIZE_4K, 0x404D, { "AES key B sect38" }},
	{ OPT_SIZE_4K, 0x404E, { "AES key A sect39" }}, { OPT_SIZE_4K, 0x404F, { "AES key B sect39" }},

	/* MFP configuration block - see MifPlux X datasheet § 10.11 */ 
	//{ OPT_TYPE_X, 0xB000, { ...define new MFP configuration block here... }},

	/* MFP installation identifier - see MifPlux X datasheet § 9.7.7 */ 
	{ OPT_ANY, 0xB001, { "SpringCard  test" }},

	/* ATS information - here we could modify the ATS. WE'D BETTER NOT!!! */ 
	//{ OPT_ANY, 0xB002, { ...define new ATS here... }},

	/* Field Configuration Block - see MifPlux X datasheet § 10.10 */ 
	//{ OPT_ANY, 0xB003, { ...define new Field Configuration Block here... }},
};

/* Return the value associated to an address in our array */
static const BYTE *get_perso_value(WORD address)
{
	unsigned int i;

	for (i=0; i<(sizeof(mifplus_perso)/sizeof(mifplus_perso[0])); i++)
		if (mifplus_perso[i].address == address)
			return mifplus_perso[i].value;

	return NULL;
}

BOOL MifPlus_L0_Perso(SCARDHANDLE hCard)
{
	BOOL is_s  = FALSE;
	BOOL is_x  = FALSE;
	BOOL is_2k = FALSE;
	BOOL is_4k = FALSE;
	unsigned int i;
	LONG rc;

	printf("Doing Level 0 personalization...\n");

	if (gbEnRid)
	{
		/* Enable Random ID - see MifPlux X datasheet § 10.10 */ 
		BYTE FCB[16] =
		{
			0x00,
			0xAA, /* Use Random ID */
			0x55, /* Proximity Check is not mandatory */
			0x00,
			0x00, /* Picc Cap 1.2 */
			0x00, 0x00, 0x00, 0x00,
			0x00, /* Picc Cap 2.5 */
			0x00, /* Picc Cap 2.6 */
			0x00, 0x00, 0x00, 0x00, 0x00, 
		};
		rc = SCardMifPlus_WritePerso(hCard, 0xB003, FCB);
		if (rc)
		{
			printf("SCardMifPlus_WritePerso(%04X) failed, rc=%08lX (%ld)\n", 0xB003, rc, rc);
			return FALSE;
		}
	}

	for (i = 0; i < (sizeof(mifplus_perso) / sizeof(mifplus_perso[0])); i++)
	{
		/* Skip options not applicable to this card. */
		if (mifplus_perso[i].option & OPT_TYPE_X)
			if (is_s)
				continue;

		if (mifplus_perso[i].option & OPT_SIZE_4K)
			if (is_2k)
				continue;

		rc = SCardMifPlus_WritePerso(hCard, mifplus_perso[i].address, mifplus_perso[i].value);
		if (rc)
		{
			if (rc == (MFP_ERROR - MFP_ERR_INVALID_BLOCK_NUMBER))
			{
				/* Invalid block number */
				if (mifplus_perso[i].option & OPT_TYPE_X)
				{
					if (!is_x)
					{
						/* The card is not likely to be an X */
						is_s = TRUE;
						continue;  /* Card is not X, so it is S. Not yet an error */
					}
				}
				if (mifplus_perso[i].option & OPT_SIZE_4K)
				{
					if (!is_4k)
					{
						/* The card is not likely to be a 4K */
						is_2k = TRUE;
						continue; /* Card is not 4K, so it is 2K. Not yet an error */
					}
				}

				printf("SCardMifPlus_WritePerso(%04X) failed, rc=%08lX (%ld)\n", mifplus_perso[i].address, rc, rc);
				return FALSE;
			}
		}

		/* No error - This helps knowing what the card is */
		if (mifplus_perso[i].option & OPT_TYPE_X)
			is_x = TRUE; /* Only X cards could be OK for this command */
		if (mifplus_perso[i].option & OPT_SIZE_4K)
			is_4k = TRUE; /* Only 4K cards could be OK for this command */

		if (is_s && is_x)
		{
			printf("Incoherent card behaviour - seem to be both Mifare Plus S and X\n");
			return FALSE;
		}

		if (is_2k && is_4k)
		{
			printf("Incoherent card behaviour - seem to be both Mifare Plus 2K and 4K\n");
			return FALSE;
		}
	}


	return TRUE;
}

/*
 ***************************************************************************** 
 *
 * Test suite for the card running at Level 1
 *
 ***************************************************************************** 
 *
 * At Level 1, the Mifare Plus emulates a plain-old Mifare Classic. Therefore,
 * the basis is to pass the same tests as ref_mifare_pcsc.c.
 *
 * The Mifare Plus also features a 'SL1 Card Authentication Key' that may be
 * used to check whether the card is genuine or not (yet it doesn't protect
 * the data stored on it).
 * We start and end by performing such an authentication.
 *
 */

/* Test function, from springprox_mifare.c */
extern BOOL MifareTestCore(SCARDCONTEXT hContext);

BOOL MifPlus_L1_Test(SCARDHANDLE hCard, const BYTE atr[])
{
	LONG rc;
	BYTE sectors;

	if (atr[14] == 0x02)
		sectors = 40;
	else
		sectors = 32;

	if ((gbSectors != 0) && (sectors > gbSectors))
		sectors = gbSectors;

	/* Mifare Plus Level 1 AES Authentication */
	/* -------------------------------------- */

	/* Verify that the card is genuine */
	printf("Performing AES authentication with SL1 Card Authentication key\n");
	if (!MifPlus_FollowingAuthentication(hCard, 0x9004, NULL))
		return FALSE;

	/* Mifare Plus Level 1 - Mifare Classic emulation */
	/* ---------------------------------------------- */

	/* Load the keys we'll need into reader's memory */
	rc = MifareTest_LoadKeys(hCard);
	if (rc != RC_TRUE)
	{
		printf("Failed to load the Mifare Classic keys into the reader\n");
		return FALSE;
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
			return FALSE;
		}
	}
	else if (rc != RC_TRUE)
	{
		printf("Failed to verify whether the card is in transport conditions, or not\n");
		return FALSE;
	}

  /* Format the card to make it ready for the first tests */
#if 1
	printf("Formating the card, please wait...\n");
	rc = MifareTest_Format(hCard, sectors, FMT_TEST_KEYS_A_B);
	if (rc != RC_TRUE)
	{
		printf("Failed to format the card\n");
		return FALSE;
	}
#endif

	/* OK, let's run the first serie of tests */
#if 1
	if (!MifareTest_Standard_Blocks(hCard, sectors, FALSE, FMT_TEST_KEYS_A_B))
		return FALSE;
	if (!MifareTest_Standard_Sectors(hCard, sectors, FALSE, FMT_TEST_KEYS_A_B))
		return FALSE;
#endif

#if 1
	if (!MifareTest_Specific_Blocks_A(hCard, sectors, FALSE))
		return FALSE;
	if (!MifareTest_Specific_Sectors_A(hCard, sectors, FALSE))
		return FALSE;
#endif

#if 1
	if (!MifareTest_Specific_Blocks_K(hCard, sectors, FALSE, FMT_TEST_KEYS_A_B))
		return FALSE;
	if (!MifareTest_Specific_Sectors_K(hCard, sectors, FALSE, FMT_TEST_KEYS_A_B))
		return FALSE;
#endif

#if 1
	if (!MifareTest_Specific_Blocks_I(hCard, sectors, FALSE, FMT_TEST_KEYS_A_B))
		return FALSE;
	if (!MifareTest_Specific_Sectors_I(hCard, sectors, FALSE, FMT_TEST_KEYS_A_B))
		return FALSE;
#endif

#if 1
	/* Put back the card into transport condition */
	printf("Formating the card, please wait...\n");
	rc = MifareTest_Format(hCard, sectors, FMT_TRANSPORT);
	if (rc != RC_TRUE)
	{
		printf("Failed to put back the card in transport conditions\n");
		return FALSE;
	}
#endif

	/* OK, let's run the second serie of tests */
#if 1
	if (!MifareTest_Standard_Blocks(hCard, sectors, TRUE, FMT_TRANSPORT))
		return FALSE;
	if (!MifareTest_Standard_Sectors(hCard, sectors, TRUE, FMT_TRANSPORT))
		return FALSE;
#endif

#if 1
	if (!MifareTest_Specific_Blocks_A(hCard, sectors, TRUE))
		return FALSE;
	if (!MifareTest_Specific_Sectors_A(hCard, sectors, TRUE))
		return FALSE;
#endif

#if 1
	if (!MifareTest_Specific_Blocks_K(hCard, sectors, TRUE, FMT_TRANSPORT))
		return FALSE;
	if (!MifareTest_Specific_Sectors_K(hCard, sectors, TRUE, FMT_TRANSPORT))
		return FALSE;
#endif

#if 1
	if (!MifareTest_Specific_Blocks_I(hCard, sectors, TRUE, FMT_TRANSPORT))
		return FALSE;
	if (!MifareTest_Specific_Sectors_I(hCard, sectors, TRUE, FMT_TRANSPORT))
		return FALSE;
#endif

	/* Mifare Plus Level 1 AES Authentication (again) */
	/* ---------------------------------------------- */

	printf("Performing AES authentication with SL1 Card Authentication key\n");
	if (!MifPlus_FollowingAuthentication(hCard, 0x9004, NULL))
		return FALSE;

	return TRUE;
}

/* AES authentication to switch from Level 1 to Level 2 */
/* ---------------------------------------------------- */
BOOL MifPlus_L1_Go_L2(SCARDHANDLE hCard)
{
  LONG rc;

	if (!gbSkipConfirm)
	{
		printf("We're now ready to put the card in Level 2.\n");
		printf("Once this is done, IT IS NOT POSSIBLE to go back to Level 1.\n");
		printf("Are you really sure you want to do this ?\n");
		if (!confirm())
			exit(EXIT_FAILURE);
	}

	/* In Level 1, the Level-switch authentication must be performed in T=CL */
	rc = SCardMifPlus_EnterTcl(hCard);
	if (rc)
	{
		printf("SCardMifPlus_EnterTcl failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

  /* The Level-switch authentication is a Following Authentication */
	printf("Performing AES authentication with Level 2 Switch Key\n");
	if (!MifPlus_FollowingAuthentication(hCard, 0x9002, NULL))
	  return FALSE;

	printf("The security level has been changed, you must now remove the card from the reader.\n\n");
  return TRUE;
}

/* AES authentication to switch from Level 1 to Level 3 */
/* ---------------------------------------------------- */
BOOL MifPlus_L1_Go_L3(SCARDHANDLE hCard)
{
  LONG rc;

	if (!gbSkipConfirm)
	{
		printf("We're now ready to put the card in Level 3.\n");
		printf("Once this is done, IT IS NOT POSSIBLE to go back to Level 1.\n");
		printf("Are you really sure you want to do this ?\n");
		if (!confirm())
			exit(EXIT_FAILURE);
	}

  /* In Level 1, the Level-switch authentication must be performed in T=CL */
	rc = SCardMifPlus_EnterTcl(hCard);
	if (rc)
	{
		printf("SCardMifPlus_EnterTcl failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

  /* The Level-switch authentication is a Following Authentication */
	printf("Performing AES authentication with Level 3 Switch Key\n");
	if (!MifPlus_FollowingAuthentication(hCard, 0x9003, NULL))
	  return FALSE;

	printf("The security level has been changed, you must now remove the card from the reader.\n\n");
  return TRUE;
}

/*
 ***************************************************************************** 
 *
 * Test suite for the card running at Level 2
 *
 ***************************************************************************** 
 *
 */

BOOL MifPlus_L2_Test(SCARDHANDLE hCard)
{
  (void) hCard;

  printf("\n\n**** Level 2 is not implemented! ****\n\n");
  return FALSE;
}

BOOL MifPlus_L2_Go_L3(SCARDHANDLE hCard)
{
  LONG rc;

	if (!gbSkipConfirm)
	{
		printf("We're now ready to put the card in Level 3.\n");
		printf("Once this is done, IT IS NOT POSSIBLE to go back to Level 2.\n");
		printf("Are you really sure you want to do this ?\n");
		if (!confirm())
			exit(EXIT_FAILURE);
	}

  /* In Level 2, the Level-switch authentication must be performed in T=CL */
	rc = SCardMifPlus_EnterTcl(hCard);
	if (rc)
	{
		printf("SCardMifPlus_EnterTcl failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

  /* The Level-switch authentication is a Following Authentication */
	printf("Performing AES authentication with Level 3 Switch Key\n");
	if (!MifPlus_FollowingAuthentication(hCard, 0x9003, NULL))
	  return FALSE;

	printf("The security level has been changed, you must now remove the card from the reader.\n\n");
  return TRUE;
}

/*
 ***************************************************************************** 
 *
 * Test suite for the card running at Level 3
 *
 ***************************************************************************** 
 *
 */

/*
 * Test of the Virtual Card feature
 * --------------------------------
 */
BOOL MifPlus_L3_Test_VirtualCard(SCARDHANDLE hCard)
{
  BYTE i;
  BYTE picc_info;
	BYTE picc_cap[2];
	BYTE picc_uid[10];
	BYTE picc_uid_len;
	const BYTE pcd_cap[3]       = { 0x00, 0x00, 0x00 };
	const BYTE dummy_id[16]     = { 0 };
  const BYTE *install_id      = get_perso_value(0xB001); /* Installation Identifier */
	const BYTE *polling_enc_key = get_perso_value(0xA080); /* VC Polling Enc Key */
	const BYTE *polling_mac_key = get_perso_value(0xA081); /* VC Polling Mac Key */

	LONG rc;

	/* Check that we can run a Virtual Card selection with the card */
	/* ------------------------------------------------------------ */

	printf("Doing VC select with valid keys... must succeed!\n");
	rc = SCardMifPlus_VirtualCard(hCard, install_id, 
	                                     polling_enc_key,
																			 polling_mac_key,
																			 pcd_cap,
																			 sizeof(pcd_cap),
																			 &picc_info,
																			 picc_cap,
																			 picc_uid, 
																			 &picc_uid_len);
	if (rc)
	{
		printf("SCardMifPlus_VirtualCard failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

	printf("\tVC OK, Info=%02X, UID=", picc_info);
	for (i=0; i<picc_uid_len; i++)
	  printf("%02X", picc_uid[i]);
	printf(" Cap1=%02X%02X\n", picc_cap[0], picc_cap[1]);

	rc = SCardMifPlus_DeselectVirtualCard(hCard);
	if (rc)
	{
		printf("SCardMifPlus_DeselectVirtualCard failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

	/* Check that Virtual Card selection fails when Installation ID or keys are unknown */
	/* -------------------------------------------------------------------------------- */

	printf("Doing VC select with invalid MAC key... must fail!\n");
	rc = SCardMifPlus_VirtualCard(hCard, install_id, 
	                                     polling_enc_key,
																			 polling_enc_key,
																			 pcd_cap,
																			 sizeof(pcd_cap),
																			 &picc_info,
																			 picc_cap,
																			 picc_uid, 
																			 &picc_uid_len);
	if (!rc)
	{
		printf("SCardMifPlus_VirtualCard : oups... line %d\n", __LINE__);
		return FALSE;
	}
	rc = SCardMifPlus_DeselectVirtualCard(hCard);
	if (rc)
	{
		printf("SCardMifPlus_DeselectVirtualCard failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

  printf("Doing VC select with invalid Encryption key but with valid MAC key...\n");
	rc = SCardMifPlus_VirtualCard(hCard, install_id, 
	                                     polling_mac_key,
																			 polling_mac_key,
																			 pcd_cap,
																			 sizeof(pcd_cap),
																			 &picc_info,
																			 picc_cap,
																			 picc_uid, 
																			 &picc_uid_len);
	if (rc)
	{
		printf("SCardMifPlus_VirtualCard failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}
	rc = SCardMifPlus_DeselectVirtualCard(hCard);
	if (rc)
	{
		printf("SCardMifPlus_DeselectVirtualCard failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

  /* Volontary oups ;-) */
	printf("\tVC ??, Info=%02X, UID=", picc_info);
	for (i=0; i<picc_uid_len; i++)
	  printf("%02X", picc_uid[i]);
	printf(" Cap1=%02X%02X\n", picc_cap[0], picc_cap[1]);

  printf("Doing VC select with valid keys but with invalid identifier...\n");
	rc = SCardMifPlus_VirtualCard(hCard, dummy_id, 
	                                     polling_enc_key,
																			 polling_mac_key,
																			 pcd_cap,
																			 sizeof(pcd_cap),
																			 &picc_info,
																			 picc_cap,
																			 picc_uid, 
																			 &picc_uid_len);
	if (!rc)
	{
		printf("SCardMifPlus_VirtualCard : oups... line %d\n", __LINE__);
		return FALSE;
	}
	rc = SCardMifPlus_DeselectVirtualCard(hCard);
	if (rc)
	{
		printf("SCardMifPlus_DeselectVirtualCard failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}


	/* Run a Virtual Card selection again, so we leave the card in an 'available' state */
	/* -------------------------------------------------------------------------------- */

	rc = SCardMifPlus_VirtualCard(hCard, install_id, 
	                                     polling_enc_key,
																			 polling_mac_key,
																			 pcd_cap,
																			 sizeof(pcd_cap),
																			 &picc_info,
																			 picc_cap,
																			 picc_uid, 
																			 &picc_uid_len);
	if (rc)
	{
		printf("SCardMifPlus_VirtualCard failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

  return TRUE;
}

BOOL MifPlus_L3_Read(SCARDHANDLE hCard, WORD address, BYTE data[])
{
  LONG rc;

  rc = SCardMifPlus_Read(hCard, address, data);
	if (rc)
	{
		printf("SCardMifPlus_Read(%04X) failed, rc=%08lX (%ld)\n", address, rc, rc);
		return FALSE;
	}

  return TRUE;
}

BOOL MifPlus_L3_Write(SCARDHANDLE hCard, WORD address, BYTE const data[])
{
  LONG rc;

  rc = SCardMifPlus_Write(hCard, address, data);
	if (rc)
	{
		printf("SCardMifPlus_Write(%04X) failed, rc=%08lX (%ld)\n", address, rc, rc);
		return FALSE;
	}

  return TRUE;
}

BOOL MifPlus_L3_Test(SCARDHANDLE hCard)
{
	WORD sector;
	BOOL last_block_reached;

  printf("Testing the VirtualCard feature\n");
  if (!MifPlus_L3_Test_VirtualCard(hCard))
	  return FALSE;

	printf("Performing AES authentication with Card Configuration Key\n");
  if (!MifPlus_FirstAuthentication(hCard, 0x9001))
	  return FALSE;

	printf("Performing AES authentication with Card Master Key\n");
  if (!MifPlus_FirstAuthentication(hCard, 0x9000))
	  return FALSE;

	printf("Reading and writing with A keys\n");
	last_block_reached = FALSE;
	for (sector = 0; sector < 40; sector++)
	{
	  BYTE data[16];
		BYTE block, count;
	  WORD auth_address = 0x4000 + 2 * sector;

    if ((gbSectors != 0) && (sector >= gbSectors))
    {
      last_block_reached = TRUE;
      break;
    }

		if (sector == 0)
		{
			if (!MifPlus_FirstAuthentication(hCard, auth_address))
        return FALSE;
		} else
		{
			if (!MifPlus_FollowingAuthentication(hCard, auth_address, &last_block_reached))
			{
			  if (last_block_reached) break;
        return FALSE;
			}
		}

		if (sector < 32)
		{
      block = 4 * sector;
			count = 3;
		} else
		{
      block = 128 + 16 * (sector - 32);
		  count = 15;
		}

    do
		{
			if (!MifPlus_L3_Read(hCard, block, data))
			{
				return FALSE;
			}

			if (block > 0)
			{
				if (!MifPlus_L3_Write(hCard, block, data))
					return FALSE;
			}

			block++;
			count--;
		} while (count);
	}

	printf("%d sectors OK\n", sector);

	printf("Reading and writing with B keys\n");
	last_block_reached = FALSE;
	for (sector = 0; sector < 40; sector++)
	{
	  BYTE data[16];
		BYTE block, count;
	  WORD auth_address = 0x4001 + 2 * sector;

    if ((gbSectors != 0) && (sector >= gbSectors))
    {
      last_block_reached = TRUE;
      break;
    }

		if (sector == 0)
		{
			if (!MifPlus_FirstAuthentication(hCard, auth_address))
        return FALSE;
		} else
		{
			if (!MifPlus_FollowingAuthentication(hCard, auth_address, &last_block_reached))
			{
				if (last_block_reached) break;
				return FALSE;
			}
		}

		if (sector < 32)
		{
      block = 4 * sector;
			count = 3;
		} else
		{
      block = 128 + 16 * (sector - 32);
		  count = 15;
		}

    do
		{
			if (!MifPlus_L3_Read(hCard, block, data))
				return FALSE;

			if (block > 0)
			{
				if (!MifPlus_L3_Write(hCard, block, data))
					return FALSE;
			}

			block++;
			count--;
		} while (count);
	}

	printf("%d sectors OK\n", sector);

	printf("Performing AES authentication over sector 0\n");
  if (!MifPlus_FirstAuthentication(hCard, 0x4000))
	  return FALSE;

	printf("Performing AES authentication with Card Configuration key\n");
  if (!MifPlus_FollowingAuthentication(hCard, 0x9001, NULL))
	  return FALSE;

	printf("Performing AES authentication with Card Master key\n");
  if (!MifPlus_FollowingAuthentication(hCard, 0x9000, NULL))
	  return FALSE;

  return TRUE;
}


BOOL MifPlus_FirstAuthentication(SCARDHANDLE hCard, WORD key_address)
{
	BYTE pcd_cap[6] = { 0xB0,0xB1,0xB2,0xB3,0xB4,0xB5 };
	const BYTE *key_value = get_perso_value(key_address);
	LONG rc;

	if (key_value == NULL)
	{
		printf("Internal error, key %04X is unknown\n", key_address);
		exit(EXIT_FAILURE);
	}

	rc = SCardMifPlus_FirstAuthenticate(hCard, key_address, key_value, pcd_cap, 6, NULL);
	if (rc)
	{
		printf("SCardMifPlus_FirstAuthenticate(%04X) failed, rc=%08lX (%ld)\n", key_address, rc, rc);
		return FALSE;
	}

	return TRUE;
}

BOOL MifPlus_FollowingAuthentication(SCARDHANDLE hCard, WORD key_address, BOOL *last_block_reached)
{
	const BYTE *key_value = get_perso_value(key_address);
	LONG rc;

	if (key_value == NULL)
	{
		printf("Internal error, key %04X is unknown\n", key_address);
		exit(EXIT_FAILURE);
	}

	rc = SCardMifPlus_FollowingAuthenticate(hCard, key_address, key_value);

	if ((rc == (MFP_ERROR-MFP_ERR_INVALID_BLOCK_NUMBER)) 
	&&  (last_block_reached != NULL)
	&&  ((key_address == 0x4040) || (key_address == 0x4041)) )
	{
		/* End of 2K card */
		*last_block_reached = TRUE;
		return FALSE;
	}

	if (rc)
	{
		printf("SCardMifPlus_FollowingAuthenticate(%04X) failed, rc=%08lX (%ld)\n", key_address, rc, rc);
		return FALSE;
	}

	return TRUE;
}


BOOL MifPlus_GetVersion(SCARDHANDLE hCard)
{
	BYTE buffer[256];
	WORD size = 0;
	WORD i;

	LONG rc = SCardMifPlus_GetVersion(hCard, buffer, sizeof(buffer), &size);

	if (rc)
	{
		printf("SCardMifPlus_GetVersion failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

	printf("Version=");
	for (i = 0; i < size; i++)
		printf("%02X", buffer[i]);
	printf("\n");

	return TRUE;
}

BOOL MifPlus_GetSignature(SCARDHANDLE hCard)
{
	BYTE buffer[256];
	WORD size = 0;
	WORD i;

	LONG rc = SCardMifPlus_GetSignature(hCard, buffer, sizeof(buffer), &size);

	if (rc)
	{
		printf("SCardMifPlus_GetSignature failed, rc=%08lX (%ld)\n", rc, rc);
		return FALSE;
	}

	printf("Signature=");
	for (i = 0; i < size; i++)
		printf("%02X", buffer[i]);
	printf("\n");

	return TRUE;
}


BOOL MifPlus_L1_CustomTest(SCARDHANDLE hCard, const BYTE atr[])
{
	const BYTE key_value_0[16] = { 0x66, 0xa0, 0x87, 0x2e, 0x0d, 0x93, 0x6b, 0x80, 0x26, 0x4f, 0x52, 0xb5, 0x5d, 0xe2, 0x9e, 0xe8 };
	const BYTE key_value_1[16] = { 0x4a, 0x69, 0xa7, 0x7e, 0xf2, 0x24, 0xb3, 0x88, 0x7a, 0x5a, 0xc3, 0x5a, 0x12, 0xd3, 0xe0, 0x2b };
	WORD key_address = 0x400D; /* Sector 6, key B */
	BYTE picc_cap[6];
	BYTE data[16];
	LONG rc;

	printf("Following authentication on sector 6, key B, 1st key value\n");
	rc = SCardMifPlus_FollowingAuthenticate(hCard, key_address, key_value_0);
	printf("rc=%08lX (%ld)\n", rc, rc);
	if (rc == 0)
		goto try_to_read;

	printf("Following authentication on sector 6, key B, 2nd key value\n");
	rc = SCardMifPlus_FollowingAuthenticate(hCard, key_address, key_value_1);
	printf("rc=%08lX (%ld)\n", rc, rc);
	if (rc == 0)
		goto try_to_read;

	printf("First authentication on sector 6, key B, 1st key value\n");
	rc = SCardMifPlus_FirstAuthenticate(hCard, key_address, key_value_0, NULL, 0, picc_cap);
	printf("rc=%08lX (%ld)\n", rc, rc);
	if (rc == 0)
		goto try_to_read;

	printf("First authentication on sector 6, key B, 2nd key value\n");
	rc = SCardMifPlus_FirstAuthenticate(hCard, key_address, key_value_1, NULL, 0, picc_cap);
	printf("rc=%08lX (%ld)\n", rc, rc);
	if (rc == 0)
		goto try_to_read;

	return FALSE;

try_to_read:
	printf("Reading\n");
	rc = SCardMifPlus_ReadEx(hCard, MFP_CMD_READ_MACED, 6 * 4, 1, data);
	printf("rc=%08lX (%ld)\n", rc, rc);
	if (rc == 0)
		return TRUE;

	return FALSE;
}
