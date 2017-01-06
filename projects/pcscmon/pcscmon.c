/**h* PCSCMon/pcscmon.c
 *
 * NAME
 *   pcscmon.c :: PC/SC Monitor (C implementation)
 *
 * DESCRIPTION
 *   This sample program shows how to use SCardGetStatusChange to track smartcard events
 *   (add/remove reader to the system, insert/remove a card in a reader).
 *   Once a card is inserted, the program displays its information (ATR analysis).
 *
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS 2008-2013
 *
 * LICENSE
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 **/

#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <winscard.h>
#include <time.h>
#include "pcscmon.h"

/* Visual Studio : link to Windows' winscard library */
#ifdef WIN32
#pragma comment(lib,"winscard.lib")
#endif

static void print_now(void);

int main(int argc, char **argv)
{
  SCARDCONTEXT hContext;
  DWORD dwReaders, dwReadersOld = 0;

  /* Constant MAXIMUM_SMARTCARD_READERS is only 10 in Windows SDK */
  /* This is not enough for most of our tests. 64 sounds good...  */
  SCARD_READERSTATE *rgscState;

  LPSTR szReaders = NULL;
  LPSTR szReaders_old = NULL;

  DWORD dwReadersSz;

  DWORD maxReaders;

  BOOL bFirstRun = TRUE;
  BOOL bShowAtrData = TRUE;
  BOOL bShowCardData = TRUE;
  BOOL bShowReaderData = TRUE;
  BOOL bTestReader = FALSE;
  DWORD i, j;
  LONG rc;

  BOOL bReadersListEqual = FALSE;
  BOOL bContextValid = FALSE;

  /* Constant MAXIMUM_SMARTCARD_READERS is only 10 in Windows SDK */
  /* This is not enough for most of our tests. 64 sounds good...  */
  maxReaders = MAXIMUM_SMARTCARD_READERS;
  if (maxReaders < 64)
    maxReaders = 64;

  for (i = 1; (int) i < argc; i++)
  {
    if (!strcmp(argv[i], "-t"))
      bTestReader = TRUE;
    else if (!strcmp(argv[i], "-nsa"))
      bShowAtrData = FALSE;
    else if (!strcmp(argv[i], "-nsc"))
      bShowCardData = FALSE;
    else if (!strcmp(argv[i], "-nsr"))
      bShowReaderData = FALSE;
  }


  /*
   * Welcome message, check parameters
   * ---------------------------------
   */
  printf("\n");
  printf("SpringCard PC/SC Monitor\n");
  printf("------------------------\n");
  printf("This program is free software (GPL)\n");
  printf("v.131016 (c) SpringCard SAS 2008-2013\n");
  printf("Press <Ctrl+C> to exit\n\n");

  rgscState = calloc(maxReaders, sizeof(SCARD_READERSTATE));
  if (rgscState == NULL)
  {
    printf("Out of memory\n");
    return EXIT_FAILURE;
  }

  /*
   * Get a handle to the PC/SC resource manager
   * ------------------------------------------
   */
  rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
  if (rc != SCARD_S_SUCCESS)
  {
    printf("SCardEstablishContext :%lXh\n", rc);
    free(rgscState);
    return EXIT_FAILURE;
  }
  bContextValid = TRUE;

  /*
   * Infinite loop, we'll exit only when killed by Ctrl+C
   * ----------------------------------------------------
   */
  for (;;)
  {
    if (!bContextValid)
    {
      /*
       * Get a handle to the PC/SC resource manager, if needed
       */
      rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
      if (rc != SCARD_S_SUCCESS)
      {
        printf("SCardEstablishContext :%lXh\n", rc);
        free(rgscState);
        return EXIT_FAILURE;
      }
      bContextValid = TRUE;
    }


    /*
     * Get the list of available readers
     */
    if (szReaders != NULL)
    {
      SCardFreeMemory(hContext, szReaders);
      szReaders = NULL;
    }

    dwReadersSz = SCARD_AUTOALLOCATE;
    rc = SCardListReaders(hContext, NULL, /* Any group */
                          (LPTSTR) & szReaders, /* Cast for buffer auto-allocation */
                          &dwReadersSz);  /* Beg for auto-allocation */

    if (rc == SCARD_E_NO_READERS_AVAILABLE)
    {
      /* No reader at all */
      dwReaders = 0;

    } else if ((rc == SCARD_E_SERVICE_STOPPED) || (rc == SCARD_E_NO_SERVICE))
    {
      if (szReaders != NULL)
      {
        SCardFreeMemory(hContext, szReaders);
        szReaders = NULL;
      }
      SCardReleaseContext(hContext);
      bContextValid = FALSE;

#ifdef WIN32
      Sleep(1000);
#else
      sleep(1);
#endif
      continue;

    } else
    if (rc != SCARD_S_SUCCESS)
    {
      print_now();
      printf("SCardListReaders error %lXh\n", rc);
      break;
    } else
    {
      LPTSTR pReader = szReaders;

      /* Check if the readers list has changed */
      bReadersListEqual = TRUE;
      if (szReaders_old == NULL)
      {
        bReadersListEqual = FALSE;
      } else
      {
        for (i = 0; i < dwReadersSz; i++)
        {
          if (szReaders_old[i] != szReaders[i])
          {
            bReadersListEqual = FALSE;
            break;
          }
        }
      }

      if (!bReadersListEqual)
      {
        if (szReaders_old != NULL)
        {
          SCardFreeMemory(hContext, szReaders_old);
          szReaders_old = NULL;
        }

        szReaders_old = szReaders;
      }

      /* Track events on found readers. */
      for (dwReaders = 0; dwReaders < maxReaders; dwReaders++)
      {
        /* End of multi-string array */
        if (*pReader == '\0')
          break;

        /* Remember this reader's name */
        rgscState[dwReaders].szReader = pReader;

        /* If the reader list has changed --> state unknown  */
        if (!bReadersListEqual)
          rgscState[dwReaders].dwCurrentState = SCARD_STATE_UNAWARE;

        /* Jump to next entry in multi-string array */
        pReader += strlen(pReader) + 1;

      }

    }

    if (bFirstRun)
    {
      /* Program startup, display the number of readers */
      print_now();
      if (dwReaders == 0)
      {
        printf("No PC/SC reader\n\n");
      } else
      {
        printf("%ld PC/SC reader%s found\n\n", dwReaders,
               dwReaders ? "s" : "");
      }
    }

    if (dwReadersOld != dwReaders)
    {
      /* Reader added, or reader removed           */
      /* (re)set the initial state for all readers */
      for (i = 0; i < dwReaders; i++)
        rgscState[i].dwCurrentState = SCARD_STATE_UNAWARE;

      if (!bFirstRun)
      {
        int c;

        print_now();

        /* Not the program startup, explain the event */
        if (dwReadersOld > dwReaders)
        {
          c = (int) dwReadersOld - dwReaders;
          printf("%d reader%s ha%s been removed from the system\n\n", c,
                 (c == 1) ? " " : "s", (c == 1) ? "s" : "ve");
        } else
        {
          c = (int) dwReaders - dwReadersOld;
          printf("%d reader%s ha%s been added to the system\n\n", c,
                 (c == 1) ? " " : "s", (c == 1) ? "s" : "ve");
        }
      }

      dwReadersOld = dwReaders;
    }

    if (dwReaders == 0)
    {
      /* We must wait here, because the SCardGetStatusChange doesn't wait  */
      /* at all in case there's no reader in the system.                   */
#ifdef WIN32
      Sleep(1000);
#else
      sleep(1);
#endif
    }

    if (dwReaders > MAXIMUM_SMARTCARD_READERS)
    {
      printf("The number of readers is limited to %d -- readers in excess are not monitored\n", MAXIMUM_SMARTCARD_READERS);
      dwReaders = MAXIMUM_SMARTCARD_READERS;
    }

    /*
     * Interesting part of the job : call SCardGetStatusChange to monitor all changes
     * taking place in the PC/SC reader(s)
     */
    rc = SCardGetStatusChange(hContext, INFINITE, rgscState, dwReaders);

    if (rc != SCARD_S_SUCCESS)
    {
      if ((rc == SCARD_E_SERVICE_STOPPED) || (rc == SCARD_E_NO_SERVICE))
      {
        /* Oups ? Restart from scratch after 2000ms */
        if (szReaders != NULL)
        {
          SCardFreeMemory(hContext, szReaders);
          szReaders = NULL;
        }

        SCardReleaseContext(hContext);
        bContextValid = FALSE;

#ifdef WIN32
        Sleep(1000);
#else
        sleep(1);
#endif
        continue;

      } else
        if ((rc == SCARD_F_INTERNAL_ERROR)
            || (rc == SCARD_E_NO_READERS_AVAILABLE))
      {
        /* Oups ? Restart from scratch after 3000ms */
        if (szReaders != NULL)
        {
          SCardFreeMemory(hContext, szReaders);
          szReaders = NULL;
        }
#ifdef WIN32
        Sleep(3000);
#else
        sleep(3);
#endif
        continue;
      }
      print_now();
      printf("SCardGetStatusChange error %lXh\n", rc);
      break;
    }

    for (i = 0; i < dwReaders; i++)
    {
      BOOL just_inserted = FALSE;

      if (rgscState[i].dwEventState & SCARD_STATE_CHANGED)
      {
        /* Something changed since last loop        */
        if ((rgscState[i].dwEventState & SCARD_STATE_PRESENT)
            && !(rgscState[i].dwCurrentState & SCARD_STATE_PRESENT))
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

      /*
       * Display current reader's state
       * ------------------------------
       */

      print_now();

      /* Reader's name */
      printf("Reader %ld: %s\n", i, rgscState[i].szReader);

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

      if (rgscState[i].dwEventState & SCARD_STATE_INUSE)
        printf("\t\tCard (or reader) in use\n");

      if (rgscState[i].dwEventState & SCARD_STATE_EXCLUSIVE)
        printf("\t\tCard (or reader) reserved for exclusive use\n");

      if (rgscState[i].dwEventState & SCARD_STATE_MUTE)
        printf("\t\tCard is mute\n");

      if (just_inserted)
      {
        /*
         * Display card's ATR
         * ------------------
         */

        if (rgscState[i].cbAtr)
        {
          printf("\tCard ATR:");
          for (j = 0; j < rgscState[i].cbAtr; j++)
            printf(" %02X", rgscState[i].rgbAtr[j]);
          printf("\n");

          if (bShowAtrData)
            ATR_analysis(rgscState[i].rgbAtr, rgscState[i].cbAtr);
        }

        /*
         * Use the SpringCard vendor specific GET DATA command
         * (embedded APDU interpreter) to get more information
         * on the card
         *
         * Some items are specified by PC/SC and should work
         * with most readers. Some other items are specific
         * and will work only with SpringCard readers
         */
        if (bShowCardData)
          ShowCardData(rgscState[i].szReader);

        /*
         * Use the SpringCard vendor specific READER CONTROL
         * command (embedded APDU interpreter) to get more
         * information on the reader.
         *
         * This will work only on a SpringCard reader
         */
        if (bShowReaderData)
          ShowReaderData(rgscState[i].szReader);


        /*
         * Do the tests...
         * ---------------
         */
        if (bTestReader)
          TestReader(rgscState[i].szReader);

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


  rc = SCardReleaseContext(hContext);

  /* Never go here (Ctrl+C kill us before !) */
  free(rgscState);

  Sleep(3000);
  return EXIT_SUCCESS;
}

static void print_now(void)
{
  time_t t = time(NULL);
  struct tm l_tm;
  
  localtime_s(&l_tm, &t);

  printf("# %02d:%02d:%02d\n", l_tm.tm_hour, l_tm.tm_min, l_tm.tm_sec);
}
