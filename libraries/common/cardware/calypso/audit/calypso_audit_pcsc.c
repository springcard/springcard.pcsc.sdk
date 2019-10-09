#include "calypso_audit.h"

#ifdef WIN32
#pragma comment (lib, "winscard.lib")
#endif

#include <stdlib.h>
#include <string.h>
#include <stdio.h>

BOOL bContextInited = FALSE;
SCARDCONTEXT hContext;
char *szReaders = NULL;

BOOL PcscStartup(void)
{
  LONG rc;
  DWORD dwReadersSz;

  PcscCleanup();

	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext : %lX\n",rc);
		return FALSE;
	}

  bContextInited = TRUE;

  dwReadersSz = SCARD_AUTOALLOCATE;
	rc = SCardListReaders(hContext,
                        NULL,                /* Any group */
                        (LPTSTR) &szReaders, /* Diabolic cast for buffer auto-allocation */
                        &dwReadersSz);       /* Beg for auto-allocation */

	if (rc == SCARD_E_NO_READERS_AVAILABLE)
	{
		/* No reader at all, but function succesfull */
    return TRUE;
	}

  if (rc != SCARD_S_SUCCESS)
	{
    /* Function failed */
    printf("SCardListReaders error %Xh\n", rc);
		return FALSE;
	}

  /* OK */
  return TRUE;
}

void PcscCleanup(void)
{
  if (bContextInited)
  {
    if (szReaders != NULL)
    {
      SCardFreeMemory(hContext, szReaders);
      szReaders = NULL;
    }

	  SCardReleaseContext(hContext);
    bContextInited = FALSE;
  }
}

DWORD PcscReaderCount(void)
{
  DWORD c = 0;
  char *p = szReaders;

  if (szReaders == NULL) return 0;
  if (*p == '\0') return 0;

  while (*p != '\0')
  {
    c++;
    p += strlen(p);
    p += 1;
  }

  return c;
}

char *PcscReaderName(DWORD idx)
{
  DWORD c = 0;
  char *p = szReaders;

  if (szReaders == NULL) return NULL;
  if (*p == '\0') return NULL;

  while (*p != '\0')
  {
    if (c == idx) return p;
    c++;
    p += strlen(p);
    p += 1;
  }

  return NULL;
}

DWORD PcscReaderIndex(const char *name)
{
  DWORD idx;

  for (idx=0; idx<PcscReaderCount(); idx++)
  {
    if (!strcmp(PcscReaderName(idx), name))
      return idx;
  }

  return (DWORD) -1;
}

BOOL PcscCardPresent(DWORD idx)
{
  LONG rc;
  SCARD_READERSTATE rgscState;
  char *szReader = PcscReaderName(idx);

  if (!bContextInited) return FALSE;
  if (szReader == NULL) return FALSE;

  rgscState.szReader = szReader;
  rgscState.dwCurrentState = SCARD_STATE_UNAWARE;

  rc = SCardGetStatusChange(hContext, 0, &rgscState, 1);	

	if (rc != SCARD_S_SUCCESS)
	{
		printf("SCardGetStatusChange(%s) : %lX\n", szReader, rc);
		return FALSE;
	}

  if (rgscState.dwEventState & SCARD_STATE_PRESENT)
  {
    return TRUE;
  }
  return FALSE;
}

BOOL PcscCardAtr(DWORD idx, BYTE atr[], DWORD *atrlen)
{
  LONG rc;
  SCARD_READERSTATE rgscState;
  char *szReader = PcscReaderName(idx);

  if (!bContextInited) return FALSE;  
  if (szReader == NULL) return FALSE;

  rgscState.szReader = szReader;
  rgscState.dwCurrentState = SCARD_STATE_UNAWARE;

  rc = SCardGetStatusChange(hContext, 0, &rgscState, 1);	

	if (rc != SCARD_S_SUCCESS)
	{
		printf("SCardGetStatusChange(%s) : %lX\n", szReader, rc);
		return FALSE;
	}

  if (rgscState.dwEventState & SCARD_STATE_PRESENT)
  {
    if (atrlen != NULL)
    {
      if ((*atrlen !=0) && (*atrlen < rgscState.cbAtr))
      {
        *atrlen = 0;
        return TRUE;
      }
    }

    if (atr != NULL)
      memcpy(atr, rgscState.rgbAtr, rgscState.cbAtr);

    if (atrlen != NULL)
      *atrlen = rgscState.cbAtr;

    return TRUE;
  }
  return FALSE;
}

BOOL PcscWaitCard(DWORD idx, BYTE atr[], DWORD *atrlen)
{
  LONG rc;
  SCARD_READERSTATE rgscState;
  char *szReader = PcscReaderName(idx);

  if (!bContextInited) return FALSE;
  if (szReader == NULL) return FALSE;

  for (;;)
  {
    rgscState.szReader = szReader;
    rgscState.dwCurrentState = SCARD_STATE_UNAWARE;
  
    rc = SCardGetStatusChange(hContext, INFINITE, &rgscState, 1);	

	  if (rc != SCARD_S_SUCCESS)
	  {
		  printf("SCardGetStatusChange(%s) : %lX\n", szReader, rc);
		  return FALSE;
	  }

    if (rgscState.dwEventState & SCARD_STATE_PRESENT)
    {
      memcpy(atr, rgscState.rgbAtr, rgscState.cbAtr);
      *atrlen = rgscState.cbAtr;
      return TRUE;
    }
  }

  return FALSE;
}

BOOL PcscConnect(DWORD idx, SCARDHANDLE *hCard)
{
  LONG rc;
  SCARDHANDLE h;
  DWORD dwProtocol;
  char *szReader = PcscReaderName(idx);

  if (!bContextInited) return FALSE;
  if (szReader == NULL) return FALSE;

  rc = SCardConnect(hContext,
                    szReader,
                    SCARD_SHARE_SHARED,
                    SCARD_PROTOCOL_T0|SCARD_PROTOCOL_T1,
                    &h,
                    &dwProtocol);

	if (rc != SCARD_S_SUCCESS)
	{
		printf("SCardConnect(%s) : %lX\n", szReader, rc);
		return FALSE;
	}

  if (hCard != NULL)
    *hCard = h;
  return TRUE;
}

BOOL PcscDisconnect(SCARDHANDLE hCard, DWORD dwDisposition)
{
  LONG rc;

  rc = SCardDisconnect(hCard, dwDisposition);
	if (rc != SCARD_S_SUCCESS)
	{
		printf("SCardDisconnect : %lX\n", rc);
		return FALSE;
	}

  return TRUE;
}

BOOL PcscDirectTransmit(DWORD idx, const BYTE send_cmd[], DWORD send_len, BYTE recv_cmd[], DWORD *recv_len)
{
  LONG rc;
  SCARDHANDLE h;
  BYTE dummy[256];
  DWORD dummy_len;
  DWORD dwProtocol;
  char *szReader = PcscReaderName(idx);

  if (!bContextInited) return FALSE;
  if (szReader == NULL) return FALSE;

  if ((recv_cmd == NULL) && (recv_len == NULL))
  {
    dummy_len = sizeof(dummy);
    recv_cmd  = dummy;
    recv_len  = &dummy_len;
  } else
  if ((recv_cmd == NULL) || (recv_len == NULL))
  {
    return FALSE;
  }

  rc = SCardConnect(hContext,
                    szReader,
                    SCARD_SHARE_DIRECT,
                    0,
                    &h,
                    &dwProtocol);
  if (rc != SCARD_S_SUCCESS)
  {
		printf("SCardConnect(%s) : %lX\n", szReader, rc);
    return FALSE;
  }

  rc = SCardControl(h, SCARD_CTL_CODE(2048), send_cmd, send_len, recv_cmd, *recv_len, recv_len);

  SCardDisconnect(h, SCARD_LEAVE_CARD);

  if (rc != SCARD_S_SUCCESS)
  {
    printf("SCardControl : %lX\n", rc);
    return FALSE;
  }

  return TRUE;
}
