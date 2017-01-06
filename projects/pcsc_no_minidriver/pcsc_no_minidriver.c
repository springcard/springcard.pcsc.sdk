/*
 * NAME
 *   pcsc_no_minidriver.c
 *
 * DESCRIPTION
 *   Command line utility to disable the 'driver not found' message on
 *   Windows Seven for smartcards. Creates dummy driver entries for 
 *   smartcards based on their ATR.
 *
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS 2011
 *
 * BASED ON A SAMPLE CODE FROM MICROSOFT - SEE HEADER BELOW
 * 
 **/


//==============================================================;
//
//  Disable Smart card Plug and Play for specific cards
//
//  Abstract:
//      This is an example of how to create a new
//      Smart Card Database entry when a smart card is inserted
//      into the computer.
//
//  This source code is only intended as a supplement to existing Microsoft
//  documentation.
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED. THIS INCLUDES BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  Copyright (C) Microsoft Corporation.  All Rights Reserved.
//
//==============================================================;
#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <winscard.h>
#include <stdio.h>
#include <conio.h>

#ifdef WIN32
#pragma comment(lib,"winscard.lib")
#endif

#if (defined(_WIN64))
static const char progname[]    = "pcsc_no_minidriver64";
#else
static const char progname[]    = "pcsc_no_minidriver32";
#endif
static const char name_prefix[] = "NoMinidriver";
static const char card_csp[]    = "$DisableSCPnP$";

// Maximum ATR length plus alignment bytes. This value is
// used in the SCARD_READERSTATE structure
#define MAX_ATR_LEN         36

BOOL no_confirm = FALSE;


BOOL Confirm(const char *msg)
{
  int c;
  BOOL f = FALSE;

  if (no_confirm) return TRUE;

  printf("\n");
  printf(">>> %s\n", msg);
  printf(">>> [Y]es | Yes to [A]ll | [N]o ? ");

again:
  c = _getch();
  switch (c)
  {
    case 'y'  :
    case 'Y'  : printf("Yes...");
                f = TRUE;
								break;

    case 'n'  :
    case 'N'  : printf("No...");
                break;

    case 'a'  :
    case 'A'  : no_confirm = TRUE;
                printf("Yes to all...");
                f = TRUE;
								break;

    case 0x03 :
    case 0x1B :
    case 'c'  :
    case 'C'  : printf("Exiting...\n");
		            exit(EXIT_FAILURE);
                break;

    default   : goto again;
  }

  printf("\n\n");
  return f;
}

BOOL ComputeCardNameFromAtr(char *name, BYTE atr[], BYTE mask[], DWORD atrlen)
{
  DWORD i;

  for (i=0; i<atrlen; i++)
    sprintf(&name[2*i], "%02X", atr[i]);

  if (mask != NULL)
	{
		for (i=0; i<atrlen; i++)
		{
			if ((mask[i] & 0xF0) != 0xF0)
				name[2*i] = 'x';
			if ((mask[i] & 0x0F) != 0x0F)
				name[2*i+1] = 'x';
		}
	}

	return TRUE;
}

BOOL IntroduceCardAtr(SCARDCONTEXT hContext, BYTE atr[], BYTE mask[], DWORD atrlen, char *name)
{
  LONG rc;

  // Introduce the card to the system
  rc = SCardIntroduceCardType(hContext,
                              name,
                              NULL,
															NULL, 
															0,
															atr,
															mask,
															atrlen);
  if (rc != SCARD_S_SUCCESS)
  {
    printf("Failed to introduce card to system with error 0x%08lX (%lu).\n", rc, rc);
		return FALSE;
  }
	
  // Set the provider name
  rc = SCardSetCardTypeProviderName(hContext,
                                    name,
                                    SCARD_PROVIDER_CSP,
																		card_csp);
  if (rc != SCARD_S_SUCCESS)
  {
    printf("Failed to set CSP for card with error 0x%08lX (%lu).\n", rc, rc);
		return FALSE;
  }

  return TRUE;
}

BOOL ForgetCardAtr(SCARDCONTEXT hContext, char *name)
{
  LONG rc;

  // Introduce the card to the system
  rc = SCardForgetCardType(hContext,
                           name);
  if (rc != SCARD_S_SUCCESS)
  {
    printf("Failed to forget card with error 0x%08lX (%lu).\n", rc, rc);
		return FALSE;
  }
	
  return TRUE;
}

BOOL ProcessCardAtr(SCARDCONTEXT hContext, BYTE atr[], DWORD atrlen)
{
  LONG lReturn = NO_ERROR;
  DWORD dwActiveProtocol = 0;
  DWORD cbAtr = MAX_ATR_LEN;
  DWORD dwIndex = 0;
  DWORD cchCards;
  char *pmszCards = NULL;
	BYTE name[MAX_PATH];
	LONG rc;
  BOOL success = TRUE;


  /* Determine if the ATR is already in the Smart Card Database */
  cchCards = SCARD_AUTOALLOCATE;
  rc = SCardListCards(hContext,
                      atr,
                      NULL,
											0,
											(char *) &pmszCards,
											&cchCards);
  if (rc != SCARD_S_SUCCESS)
  {
	  printf("SCardListCards error 0x%08lX (%lu).\n", rc, rc);
		return FALSE;
  }

	if ((pmszCards == NULL) || (*pmszCards == 0))
  {
    /* Card not found. We need to add it. */
    printf("\tThis card is currently not recognized by the system.\n");

		if (!Confirm("Do you want to add this card to the 'no driver' list ?"))
			return TRUE;

		printf("\tAdding card's ATR...\n");


    sprintf(name, "%s-", name_prefix);
		if (!ComputeCardNameFromAtr(&name[strlen(name)], atr, NULL, atrlen))
		{
      printf("Failed to compute a name for the card.\n");
			return FALSE;
		}

    success = IntroduceCardAtr(hContext, atr, NULL, atrlen, name);
		if (success)
		  printf("\tOK!\n");

  } else
  {
    printf("\tThis card is already known by the system.\n");
		printf("\t(%s)\n", pmszCards);
  }

  // Free resources
  if (pmszCards != NULL)
  {
    SCardFreeMemory(hContext, pmszCards);
  }

  return success;
}

BOOL MonitorReaders(SCARDCONTEXT hContext)
{
  DWORD  dwReaders, dwReadersOld = 0;
  SCARD_READERSTATE *rgscState;
	LPSTR  szReaders = NULL;
  DWORD  dwReadersSz; 
  DWORD  maxReaders;
	BOOL   bFirstRun = TRUE;
  DWORD  i, j;
	LONG   rc;

  /* Constant MAXIMUM_SMARTCARD_READERS is only 10 in Windows SDK */
  /* This is not enough for most of our tests. 64 sounds good...  */
  maxReaders = MAXIMUM_SMARTCARD_READERS;
  if (maxReaders<64) maxReaders = 64;

  rgscState = calloc(maxReaders, sizeof(SCARD_READERSTATE));
  if (rgscState == NULL)
  {
    printf("Out of memory\n");
    return FALSE;
  }

	for (;;)
	{
  	/* Get the list of available readers */
    dwReadersSz = SCARD_AUTOALLOCATE;
		rc = SCardListReaders(hContext,
                          NULL,                /* Any group */
                          (LPTSTR) &szReaders, /* Diabolic cast for buffer auto-allocation */
                          &dwReadersSz);       /* Beg for auto-allocation */
	  
		if (rc == SCARD_E_NO_READERS_AVAILABLE)
		{
		  /* No reader at all */
			dwReaders = 0;
		} else
    if (rc != SCARD_S_SUCCESS)
		{
      printf("SCardListReaders error 0x%08lX (%lu).\n", rc, rc);
			break;
		} else
		{
			/* Track events on found readers. */
			LPTSTR pReader = szReaders;

			for (dwReaders=0; dwReaders<maxReaders; dwReaders++)
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

		if (bFirstRun)
    {
      /* Program startup, display the number of readers */
      if (dwReaders == 0)
      {
        printf("No PC/SC reader\n\n");
      } else
      {
        printf("%ld PC/SC reader%s found\n\n", dwReaders, dwReaders ? "s" : "");
      }
    }
      
		if (dwReadersOld != dwReaders)
		{
			/* Reader added, or reader removed           */
      /* (re)set the initial state for all readers */
			for (i=0; i<dwReaders; i++)
				rgscState[i].dwCurrentState = SCARD_STATE_UNAWARE;

      if (!bFirstRun)
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
      Sleep(1000);
    }

		/*
     * Interesting part of the job : call SCardGetStatusChange to monitor all changes
     * taking place in the PC/SC reader(s)
     */
		rc = SCardGetStatusChange(hContext,
			                        INFINITE,
			                        rgscState,
			                        dwReaders);	
		
		if (rc != SCARD_S_SUCCESS)
    {
      printf("SCardGetStatusChange error 0x%08lX (%lu).\n", rc, rc);

      if (rc == SCARD_F_INTERNAL_ERROR)
      {
        /* Oups ? Restard from scratch after 2500ms */
        if (szReaders != NULL)
        {
          SCardFreeMemory(hContext, szReaders);
          szReaders = NULL;
        }
        Sleep(2500);
        continue;
      }
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
        if (!bFirstRun)
          continue;
      }

			if (just_inserted)
			{
				printf("Card inserted in %s\n", rgscState[i].szReader);

				if (rgscState[i].dwEventState & SCARD_STATE_MUTE)
				{
					printf("\tCard is mute\n");
				} else
				{
					printf("\tATR: ");
					for (j=0; j<rgscState[i].cbAtr; j++)
						printf("%02X", rgscState[i].rgbAtr[j]);
					printf("\n");

					if (!ProcessCardAtr(hContext, rgscState[i].rgbAtr, rgscState[i].cbAtr))
            return FALSE;

					printf("\n");
				}
			}
		}

    /* Free the list of readers  */
    if (szReaders != NULL)
    {
      SCardFreeMemory(hContext, szReaders);
      szReaders = NULL;
    }
      
    /* Not the first run anymore */
		bFirstRun = FALSE;
	}


  /* Never go here (Ctrl+C kill us before !) */
  free(rgscState);
  return TRUE;
}

BOOL ProcessCommandLineAtr(SCARDCONTEXT hContext, char *p_atr, char *p_name, BOOL forget_not_introduce)
{
  BYTE atr[MAX_ATR_LEN];
	BYTE mask[MAX_ATR_LEN];
	char name[MAX_PATH];
	BYTE atrlen = 0;
	unsigned int i, j;

  j = 0;
	for (i=0; i<strlen(p_atr); i++)
	{
	  BYTE c_atr, c_mask = 0x0F;

		if (atrlen >= MAX_ATR_LEN)
		{
      printf("The ATR is too long.\n");
			return FALSE;
		}

		if ((p_atr[i] == ' ') || (p_atr[i] == ':'))
      continue;

		if ((p_atr[i] >= '0') && (p_atr[i] <= '9'))
		{
      c_atr = p_atr[i] - '0';
		} else
		if ((p_atr[i] >= 'A') && (p_atr[i] <= 'F'))
		{
      c_atr = p_atr[i] - 'A' + 10;
		} else
		if ((p_atr[i] >= 'a') && (p_atr[i] <= 'f'))
		{
      c_atr = p_atr[i] - 'a' + 10;
		} else
		if ((p_atr[i] == 'x') || (p_atr[i] == 'X') || (p_atr[i] == '?') || (p_atr[i] == '.'))
		{
      c_atr  = 0;
			c_mask = 0;
		} else
		{
      printf("Invalid character '%c' in the ATR.\n", p_atr[i]);
			return FALSE;
		}

		if (j)
		{
      atr[atrlen]  <<= 4;
			atr[atrlen]   |= c_atr;
			mask[atrlen] <<= 4;
			mask[atrlen]  |= c_mask;
			atrlen++;
			j = 0;
		} else
		{
			atr[atrlen]    = c_atr;
			mask[atrlen]   = c_mask;
			j = 1;
		}
	}

	if (j)
	{
    printf("Odd number of characters in the ATR.\n");
    return FALSE;
	}

	if (p_name != NULL)
	{
    sprintf(name, "%s-%s", name_prefix, p_name);
	} else
	{
    sprintf(name, "%s-", name_prefix);
		if (!ComputeCardNameFromAtr(&name[strlen(name)], atr, mask, atrlen))
		{
      printf("Failed to compute a name for the card.\n");
			return FALSE;
		}
	}
  
  if (forget_not_introduce)
  {
    if (!Confirm("Do you want to remove this card from the 'no driver' list ?"))
      return TRUE;

    return ForgetCardAtr(hContext, name);    
  } else
  {
    if (!Confirm("Do you want to add this card to the 'no driver' list ?"))
      return TRUE;

    return IntroduceCardAtr(hContext, atr, mask, atrlen, name);
  }
}

void banner(void)
{
  printf("%s\n", progname);
	printf("Prevent Windows 7 to display a 'driver not found' message for smartcards\n\n");
	printf("Based on sample code (c) MICROSOFT CORPORATION.\n");
	printf("All rights reserved.\n");
	printf("See http://support.microsoft.com/kb/976832 for details.\n");
	printf("\n");
	printf("THIS SOFTWARE IS PROVIDED \"AS IS\" WITH NO EXPRESSED OR IMPLIED WARRANTY.\n");
	printf("You're using it at your own risks.\n");
	printf("\n");
}

void usage(void)
{
	printf("Usage:\n\n");
	printf("%s -m\n", progname);
	printf("\tEnter monitor mode. Disable the warning for every card inserted\n\tin any PC/SC reader.\n\n");
	printf("%s -a <ATR> [-n <Name>]\n", progname);
	printf("\tDisable the warning for the specified card.\n\tOptionally provide a friendly name to identify the smarted.\n\n");
  printf("Options:\n\n");
	printf("\t-q\tSuppress startup banner.\n");
	printf("\t-y\tSkip confirmation.\n\n");
}

int main(int argc, char **argv)
{
  char *p_atr = NULL;
	char *p_name = NULL;
	BOOL do_monitor = FALSE;
	BOOL no_banner = FALSE;
  int i;
  LONG rc;
  SCARDCONTEXT hContext;
  BOOL forget_not_introduce = FALSE;
	BOOL success = TRUE;

#if (!defined(_WIN64))
  /* Not a 64-bit program */
  BOOL f64 = FALSE;
  if (IsWow64Process(GetCurrentProcess(), &f64) && f64)
  {
    /* We are a 32-bit program running on a 64-bit machine ! */
    printf("This machine is 64-bit. Please use the 64-bit version of the software instead\n");
    return EXIT_FAILURE;
  }
#endif

  for (i=1; i<argc; i++)
  {
	  if (!_stricmp(argv[i], "-a") && i<argc-1)
	  {
      p_atr = argv[++i];
      forget_not_introduce = FALSE;
	  } else
	  if (!_stricmp(argv[i], "-d") && i<argc-1)
	  {
      p_atr = argv[++i];
      forget_not_introduce = TRUE;
	  } else      
	  if (!_stricmp(argv[i], "-n") && i<argc-1)
	  {
      p_name = argv[++i];
	  } else
	  if (!_stricmp(argv[i], "-m"))
	  {
      do_monitor = TRUE;
	  } else
	  if (!_stricmp(argv[i], "-y"))
	  {
      no_confirm = TRUE;
	  } else
	  if (!_stricmp(argv[i], "-q"))
	  {
      no_banner = TRUE;
	  } else
	  if (!_stricmp(argv[i], "-h") || !_stricmp(argv[i], "-?"))
	  {
		  banner();
		  usage();
		  return EXIT_FAILURE;
	  } else
	  {
		  banner();
      printf("Invalid command line. Use %s -h for help.\n", progname);
		  return EXIT_FAILURE;
	  }
  }

  if (!no_banner)
  {
    banner();
  }

  if (!p_atr && !do_monitor)
  {
    printf("Please specify either -a or -m.\n");
	  return EXIT_FAILURE;
  }

  if (p_atr && do_monitor)
  {
    printf("Please specify either -a or -m, not both.\n");
	  return EXIT_FAILURE;
  }

  if (p_name && !p_atr)
  {
    printf("Please specify the ATR and not only the name.\n");
	  return EXIT_FAILURE;
  }

  /* Establish a system-level context with the Smart Card Service */
  rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
  if (rc != SCARD_S_SUCCESS)
  {
    printf("Failed to establish context with the Smart Card Service with error 0x%08lX (%ld).\n", rc, rc);
    return FALSE;
  }

  if (do_monitor)
  {
	  printf("Entering monitor mode, hit Ctrl+C to exit\n");
	  success = MonitorReaders(hContext);
  }

  if (p_atr)
  {
    success = ProcessCommandLineAtr(hContext, p_atr, p_name, forget_not_introduce);
  }

  // Cleanup resources
  SCardReleaseContext(hContext);

  if (success)
  {
    printf("Done.\n");
    return EXIT_SUCCESS;
  } else
  {
	  printf("Failed.\n");
    return EXIT_FAILURE;
  }
}
