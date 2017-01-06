/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2011 SpringCard SAS - www.springcard.com
*/

#include "pcsc_helpers.h"

#ifdef WIN32
#pragma comment(lib,"winscard.lib")
#endif

BOOL GetReaderList(char ***aszReaders)
{
	SCARDCONTEXT      hContext;
	LPSTR             szReaders = NULL;
  DWORD             dwReadersSz;
	LPTSTR            pReader;
	int               cReaders, i;
  LONG              rc;
	char              **lReaders;

	if (aszReaders == NULL)
	  return FALSE;

	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext error %08lX\n", rc);
		return FALSE;
	}

  dwReadersSz = SCARD_AUTOALLOCATE;
	rc = SCardListReaders(hContext,
                        NULL,                /* Any group */
                        (LPTSTR) &szReaders, /* Diabolic cast for buffer auto-allocation */
                        &dwReadersSz);       /* Beg for auto-allocation */

#ifdef SCARD_E_NO_READERS_AVAILABLE
	if (rc == SCARD_E_NO_READERS_AVAILABLE)
	{
		/* No reader at all */
		SCardReleaseContext(hContext);
		return TRUE;
	}
#endif

  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardListReaders error %08lX\n",rc);
		SCardReleaseContext(hContext);
		return FALSE;
	}

	if (szReaders == NULL)
	{
    printf("SCardListReaders returned a NULL pointer\n");
		SCardReleaseContext(hContext);
		return FALSE;
	}

  /* Count the readers */
	cReaders = 0;
  pReader  = szReaders;
	while (*pReader != '\0')
	{
	  cReaders++;
    /* Jump to next entry in multi-string array */
		pReader += strlen(pReader) + 1;
	}

	if (!cReaders)
	{
		/* No reader at all */
    SCardFreeMemory(hContext, szReaders);
		SCardReleaseContext(hContext);
		return TRUE;
	}

  /* Allocate a list of pointers (cReader+1 to terminate the list by NULL) */
	lReaders = calloc(cReaders+1, sizeof(void *));
	if (lReaders == NULL)
	{
	  printf("Memory allocation failed\n");
		SCardFreeMemory(hContext, szReaders);
		SCardReleaseContext(hContext);
		return FALSE;
	}

  /* Populate the list */
  i = 0;
  pReader = szReaders;
	while (*pReader != '\0')
	{
	  /* Allocate and copy the name */
	  lReaders[i] = strdup(pReader);

		if (lReaders[i]  == NULL)
		{
			printf("Memory allocation failed\n");
			FreeReaderList(lReaders);
			SCardFreeMemory(hContext, szReaders);
			SCardReleaseContext(hContext);
			return FALSE;
		}

    /* Jump to next entry in multi-string array */
		pReader += strlen(pReader) + 1;
	  i++;
	}

  /* Done */
	SCardFreeMemory(hContext, szReaders);
	SCardReleaseContext(hContext);
	*aszReaders = lReaders;
	return TRUE;
}

DWORD GetReaderCount(char **aszReaders)
{
  int c = 0;

  if (aszReaders != NULL)
	{
	  while (aszReaders[c] != NULL) c++;
	}

  return c;
}

void FreeReaderList(char **aszReaders)
{
  int i;

  if (aszReaders != NULL)
	{
	  for (i=0; aszReaders[i] != NULL; i++)
		{
			free(aszReaders[i]);
		}

		free(aszReaders);
	}
}

BOOL PrintReaderStatus(const char *szReaderName)
{
	SCARDCONTEXT      hContext;
  SCARD_READERSTATE rgscState;
	unsigned int i;
	LONG rc;

	if (szReaderName == NULL)
	  return FALSE;

	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext error %08lX\n", rc);
		return FALSE;
	}

  /* Get status */
  rgscState.szReader = szReaderName;
  rgscState.dwCurrentState = SCARD_STATE_UNAWARE;

 	rc = SCardGetStatusChange(hContext,
			                      0,
			                      &rgscState,
			                      1);	

  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardGetStatusChange(%s) error %08lX\n", szReaderName, rc);
		SCardReleaseContext(hContext);
		return FALSE;
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

  SCardReleaseContext(hContext);
  return TRUE;
}

BOOL GetCardAtr(const char *szReaderName, BYTE abAtr[], DWORD *pdwAtrLen)
{
	SCARDCONTEXT      hContext;
  SCARD_READERSTATE rgscState;
	LONG rc;

	if (szReaderName == NULL)
	  return FALSE;

	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext error %08lX\n", rc);
		return FALSE;
	}

  /* Get status */
  rgscState.szReader = szReaderName;
  rgscState.dwCurrentState = SCARD_STATE_UNAWARE;

 	rc = SCardGetStatusChange(hContext,
			                      0,
			                      &rgscState,
			                      1);	

  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardGetStatusChange(%s) error %08lX\n", szReaderName, rc);
		SCardReleaseContext(hContext);
		return FALSE;
	}

  if (rgscState.cbAtr)
	{
		memcpy(abAtr, rgscState.rgbAtr, rgscState.cbAtr);
    *pdwAtrLen = rgscState.cbAtr;
  } else
    *pdwAtrLen = 0;

  SCardReleaseContext(hContext);
  return TRUE;
}

BOOL ReaderGetStatus(const char *szReaderName, DWORD *pdwStatus)
{
	SCARDCONTEXT      hContext;
  SCARD_READERSTATE rgscState;
	LONG rc;

	if (szReaderName == NULL)
	  return FALSE;

	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext error %08lX\n", rc);
		return FALSE;
	}

  /* Get status */
  rgscState.szReader = szReaderName;
  rgscState.dwCurrentState = SCARD_STATE_UNAWARE;

 	rc = SCardGetStatusChange(hContext,
			                      0,
			                      &rgscState,
			                      1);	

  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardGetStatusChange(%s) error %08lX\n", szReaderName, rc);
		SCardReleaseContext(hContext);
		return FALSE;
	}

  *pdwStatus = rgscState.dwEventState;

  SCardReleaseContext(hContext);
  return TRUE;
}


BOOL WaitCardInsertion(const char *szReaderName, BYTE atr[], DWORD *atrlen)
{
  LONG rc;
  SCARDCONTEXT      hContext;
  SCARD_READERSTATE rgscState;

	if (szReaderName == NULL)
	  return FALSE;

	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext error %08lX\n", rc);
		return FALSE;
	}

  rgscState.szReader = szReaderName;
  rgscState.dwCurrentState = SCARD_STATE_UNAWARE;

  for (;;)
  {
    rc = SCardGetStatusChange(hContext, INFINITE, &rgscState, 1);

    if (rc != SCARD_S_SUCCESS)
    {
      printf("SCardGetStatusChange(%s) : %lX\n", szReaderName, rc);
      break;
    }

    if (rgscState.dwEventState & SCARD_STATE_PRESENT)
    {
      memcpy(atr, rgscState.rgbAtr, rgscState.cbAtr);
      *atrlen = rgscState.cbAtr;
      SCardReleaseContext(hContext);
      return TRUE;
    }

    rgscState.dwCurrentState = rgscState.dwEventState;
  }

  SCardReleaseContext(hContext);
  return FALSE;
}

BOOL WaitCardRemoval(const char *szReaderName)
{
  LONG rc;
  SCARDCONTEXT      hContext;
  SCARD_READERSTATE rgscState;

	if (szReaderName == NULL)
	  return FALSE;

	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext error %08lX\n", rc);
		return FALSE;
	}

  rgscState.szReader = szReaderName;
  rgscState.dwCurrentState = SCARD_STATE_UNAWARE;

  for (;;)
  {
    rc = SCardGetStatusChange(hContext, INFINITE, &rgscState, 1);

    if (rc != SCARD_S_SUCCESS)
    {
      printf("SCardGetStatusChange(%s) : %lX\n", szReaderName, rc);
      break;
    }

    if (!(rgscState.dwEventState & SCARD_STATE_PRESENT))
    {
      SCardReleaseContext(hContext);
      return TRUE;
    }

    rgscState.dwCurrentState = rgscState.dwEventState;
  }

  SCardReleaseContext(hContext);
  return FALSE;
}


BOOL CardPresent(const char *szReaderName)
{
  DWORD dwStatus;

	if (!ReaderGetStatus(szReaderName, &dwStatus))
	  return FALSE;

  return ((dwStatus & SCARD_STATE_PRESENT) && !(dwStatus & SCARD_STATE_MUTE));
}

BOOL CardAvailable(const char *szReaderName)
{
  DWORD dwStatus;

	if (!ReaderGetStatus(szReaderName, &dwStatus))
	  return FALSE;

  return ((dwStatus & SCARD_STATE_PRESENT) && !(dwStatus & SCARD_STATE_MUTE) && !(dwStatus & SCARD_STATE_INUSE));
}

LONG CardLastError(CARD_CHANNEL_T *pChannel)
{
  if (pChannel == NULL)
	  return FALSE;

  return pChannel->lLastError;
}

BOOL CardConnect(const char *szReaderName, CARD_CHANNEL_T *pChannel)
{
  if (pChannel == NULL)
	  return FALSE;

	pChannel->lLastError = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &pChannel->hContext);
	if(pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
		  printf("SCardEstablishContext error %08lX\n", pChannel->lLastError);
		return FALSE;
	}

  /* Connect to the card */
  pChannel->lLastError = SCardConnect(pChannel->hContext, szReaderName, SCARD_SHARE_EXCLUSIVE, SCARD_PROTOCOL_T0|SCARD_PROTOCOL_T1, &pChannel->hCard, &pChannel->dwProtocol);
  if (pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
      printf("SCardConnect(%s) error %08lX\n", szReaderName, pChannel->lLastError);
		SCardReleaseContext(pChannel->hContext);
		return FALSE;
	}

  pChannel->dwAtrLength = sizeof(pChannel->cbAtr);
	pChannel->lLastError = SCardStatus(pChannel->hCard, NULL, NULL, NULL, NULL, pChannel->cbAtr, &pChannel->dwAtrLength);
  if (pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
      printf("SCardStatus error %08lX\n", pChannel->lLastError);
		SCardDisconnect(pChannel->hCard, SCARD_EJECT_CARD);
		SCardReleaseContext(pChannel->hContext);
		return FALSE;
	}

  return TRUE;
}

BOOL CardTransmit(CARD_CHANNEL_T *pChannel, const BYTE *abCommandApdu, DWORD dwCommandApduLength, BYTE *abResponseApdu, DWORD *dwResponseApduLength)
{
	const SCARD_IO_REQUEST *pioSendPci;

  if (pChannel == NULL)
	  return FALSE;

	switch (pChannel->dwProtocol)
	{
	  case SCARD_PROTOCOL_T0 : pioSendPci = SCARD_PCI_T0; break;
    case SCARD_PROTOCOL_T1 : pioSendPci = SCARD_PCI_T1; break;
		default                : pioSendPci = NULL;
	}

  pChannel->lLastError = SCardTransmit(pChannel->hCard, pioSendPci, abCommandApdu, dwCommandApduLength, NULL, abResponseApdu, dwResponseApduLength);
  if (pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
      printf("SCardTransmit error %08lX\n", pChannel->lLastError);
		return FALSE;
	}

  return TRUE;
}

BOOL CardDisconnect(CARD_CHANNEL_T *pChannel, DWORD dwDisposition)
{
  if (pChannel == NULL)
	  return FALSE;

	pChannel->lLastError = SCardDisconnect(pChannel->hCard, dwDisposition);
  if (pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
      printf("SCardDisconnect error %08lX\n", pChannel->lLastError);
		SCardReleaseContext(pChannel->hContext);
		return FALSE;
	}

	pChannel->lLastError = SCardReleaseContext(pChannel->hContext);
  if (pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
      printf("SCardReleaseContext error %08lX\n", pChannel->lLastError);
		return FALSE;
	}

  return TRUE;
}

BOOL ReaderConnect(const char *szReaderName, CARD_CHANNEL_T *pChannel)
{
  if (pChannel == NULL)
	  return FALSE;

	pChannel->lLastError = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &pChannel->hContext);
	if(pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
		  printf("SCardEstablishContext error %08lX\n", pChannel->lLastError);
		return FALSE;
	}

  /* Connect to the card */
  pChannel->lLastError = SCardConnect(pChannel->hContext, szReaderName, SCARD_SHARE_DIRECT, 0, &pChannel->hCard, &pChannel->dwProtocol);
  if (pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
      printf("SCardConnect(%s) error %08lX\n", szReaderName, pChannel->lLastError);
		SCardReleaseContext(pChannel->hContext);
		return FALSE;
	}

  return TRUE;
}

BOOL ReaderDisconnect(CARD_CHANNEL_T *pChannel)
{
  if (pChannel == NULL)
	  return FALSE;

	pChannel->lLastError = SCardDisconnect(pChannel->hCard, SCARD_LEAVE_CARD);
  if (pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
      printf("SCardDisconnect error %08lX\n", pChannel->lLastError);
		SCardReleaseContext(pChannel->hContext);
		return FALSE;
	}

	pChannel->lLastError = SCardReleaseContext(pChannel->hContext);
  if (pChannel->lLastError != SCARD_S_SUCCESS)
	{
    if (!pChannel->bSilent)
      printf("SCardReleaseContext error %08lX\n", pChannel->lLastError);
		return FALSE;
	}

  return TRUE;
}


#ifdef WIN32
  #define IOCTL_VENDOR_ESCAPE_SPRINGCARD      SCARD_CTL_CODE(2048)
  #define IOCTL_VENDOR_ESCAPE_MS_CCID         SCARD_CTL_CODE(3500)

	static BOOL use_ms_ioctl         = TRUE;
	static BOOL use_springcard_ioctl = TRUE;
#endif

#ifdef __linux__
  #include <reader.h>
  #define IOCTL_SMARTCARD_VENDOR_IFD_EXCHANGE SCARD_CTL_CODE(1)
#endif

BOOL ReaderControl(CARD_CHANNEL_T *pChannel, const BYTE *abCommand, DWORD dwCommandLength, BYTE *abResponse, DWORD *dwResponseLength)
{
  DWORD MaxLen = *dwResponseLength;
  DWORD CurLen = *dwResponseLength;

  if (pChannel == NULL)
	  return FALSE;

#ifdef WIN32
  pChannel->lLastError = SCardControl(pChannel->hCard, IOCTL_VENDOR_ESCAPE_SPRINGCARD, abCommand, dwCommandLength, abResponse, MaxLen, &CurLen);     
	if ((pChannel->lLastError == ERROR_INVALID_FUNCTION) || (pChannel->lLastError == ERROR_NOT_SUPPORTED) || (pChannel->lLastError == RPC_X_BAD_STUB_DATA))
	{
		CurLen = *dwResponseLength;
    pChannel->lLastError = SCardControl(pChannel->hCard, IOCTL_VENDOR_ESCAPE_MS_CCID, abCommand, dwCommandLength, abResponse, MaxLen, &CurLen);
	}
#endif

#ifdef __linux__
  pChannel->lLastError = SCardControl(pChannel->hCard, IOCTL_SMARTCARD_VENDOR_IFD_EXCHANGE, abCommand, dwCommandLength, abResponse, MaxLen, &CurLen);
#endif

  if (pChannel->lLastError != SCARD_S_SUCCESS)
  {
    if (!pChannel->bSilent)
    {
      printf("SCardControl error %08lX\n", pChannel->lLastError);

#ifdef WIN32
      printf("\n");
      printf("If using MS's CCID driver, make sure you've allowed the 'Vendor Escape'\n");
		  printf("in registry. You may use software ms_ccid_escape_enable to do so.\n");
		  printf("\n");
#endif
#ifdef __linux__
      printf("\n");
      printf("Make sure you've allowed the 'Vendor Escape' in Info.plist\n");
		  printf("Locate the file Info.plist (command: 'locate Info.plist')\n");
		  printf("Edit the file (you must be root to do so)\n");
		  printf("Locate the line '<ifdDriverOptions>', set bit 1 to 1 to\n");
		  printf("allow the 'CCID Exchange' : '<string>0x0001<string>'.\n");
		  printf("Restart the PC/SC service after saving file Info.plist .\n");
		  printf("\n");
#endif
    }

    return FALSE;
  }

  *dwResponseLength = CurLen;
	return TRUE;
}
