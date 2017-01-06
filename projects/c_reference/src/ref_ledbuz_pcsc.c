/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2011 SpringCard SAS - www.springcard.com
   
  ref_ledbuz_pcsc.c
  -----------------

*/
#include "pcsc_helpers.h"

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

LONG Control(SCARDHANDLE hCard, BYTE c_apdu[], DWORD c_apdu_len, BYTE r_apdu[], DWORD r_apdu_size, DWORD *r_apdu_len)
{
  LONG rc;

#ifdef WIN32
  *r_apdu_len = r_apdu_size;
  rc = SCardControl(hCard, IOCTL_VENDOR_ESCAPE_SPRINGCARD, c_apdu, c_apdu_len, r_apdu, r_apdu_size, r_apdu_len);     
	if ((rc == ERROR_INVALID_FUNCTION) || (rc == ERROR_NOT_SUPPORTED) || (rc == RPC_X_BAD_STUB_DATA))
	{
		*r_apdu_len = r_apdu_size;
    rc = SCardControl(hCard, IOCTL_VENDOR_ESCAPE_MS_CCID, c_apdu, c_apdu_len, r_apdu, r_apdu_size, r_apdu_len);
	}
#endif

#ifdef __linux__
  *r_apdu_len = r_apdu_size;
  rc = SCardControl(hCard, IOCTL_SMARTCARD_VENDOR_IFD_EXCHANGE, c_apdu, c_apdu_len, r_apdu, r_apdu_size, r_apdu_len);     
#endif

  if (rc != SCARD_S_SUCCESS)
  {
    printf("SCardControl error %08lX\n",rc);

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

	return rc;
}

BOOL SetLeds(SCARDCONTEXT hContext, const char *szReader, BYTE red, BYTE green, BYTE blue)
{
  BYTE c_apdu[256];
  DWORD c_apdu_len = 0;
  BYTE r_apdu[256];
  DWORD r_apdu_len;

  SCARDHANDLE hCard;
  DWORD dwProtocol;

  LONG rc;

  /* Try to connect to the card in the reader (if there's one) */
  rc = SCardConnect(hContext, szReader, SCARD_SHARE_SHARED, SCARD_PROTOCOL_T1, &hCard, &dwProtocol);
  if (rc == SCARD_S_SUCCESS)
  {
    /* Card connected, use the APDU method */
    c_apdu[c_apdu_len++] = 0xFF;
    c_apdu[c_apdu_len++] = 0xF0;
    c_apdu[c_apdu_len++] = 0x00;
    c_apdu[c_apdu_len++] = 0x00;
    c_apdu[c_apdu_len++] = 4;
    c_apdu[c_apdu_len++] = 0x1E;
    c_apdu[c_apdu_len++] = red;
    c_apdu[c_apdu_len++] = green;
    c_apdu[c_apdu_len++] = blue;
 
    r_apdu_len = sizeof(r_apdu);
    rc = SCardTransmit(hCard, NULL, c_apdu, c_apdu_len, NULL, r_apdu, &r_apdu_len);
    if (rc != SCARD_S_SUCCESS)
    {
      printf("SCardTransmit error %08lX\n",rc);
    }

    SCardDisconnect(hCard, SCARD_LEAVE_CARD);

  } else
	if ((rc == SCARD_E_NO_SMARTCARD)
	 || (rc == SCARD_E_PROTO_MISMATCH)
   || (rc == SCARD_W_UNRESPONSIVE_CARD)
   || (rc == SCARD_W_UNPOWERED_CARD)
   || (rc == SCARD_W_RESET_CARD)
   || (rc == SCARD_W_REMOVED_CARD))
  {
    /* No card in the reader (or any other error...) try a direct connection to the reader itself */
    rc = SCardConnect(hContext, szReader, SCARD_SHARE_DIRECT, 0, &hCard, &dwProtocol);
    if (rc == SCARD_S_SUCCESS)
    {
      c_apdu[c_apdu_len++] = 0x58;
      c_apdu[c_apdu_len++] = 0x1E;
      c_apdu[c_apdu_len++] = red;
      c_apdu[c_apdu_len++] = green;
      c_apdu[c_apdu_len++] = blue;

      rc = Control(hCard, c_apdu, c_apdu_len, r_apdu, sizeof(r_apdu), &r_apdu_len);

      SCardDisconnect(hCard, SCARD_LEAVE_CARD);
    } else
    {
      printf("SCardConnect(direct) error %08lX\n",rc);
    }
  } else
  {
    printf("SCardConnect(card) error %08lX\n",rc);
  }

  return (rc == SCARD_S_SUCCESS);
}

BOOL DoBuzzer(SCARDCONTEXT hContext, const char *szReader)
{
  BYTE c_apdu[256];
  DWORD c_apdu_len = 0;
  BYTE r_apdu[256];
  DWORD r_apdu_len;

  SCARDHANDLE hCard;
  DWORD dwProtocol;

  LONG rc;

  /* Try to connect to the card in the reader (if there's one) */
  rc = SCardConnect(hContext, szReader, SCARD_SHARE_SHARED, SCARD_PROTOCOL_T1, &hCard, &dwProtocol);
  if (rc == SCARD_S_SUCCESS)
  {
    /* Card connected, use the APDU method */
    c_apdu[c_apdu_len++] = 0xFF;
    c_apdu[c_apdu_len++] = 0xF0;
    c_apdu[c_apdu_len++] = 0x00;
    c_apdu[c_apdu_len++] = 0x00;
    c_apdu[c_apdu_len++] = 3;
    c_apdu[c_apdu_len++] = 0x1C;
    c_apdu[c_apdu_len++] = 0x01;
    c_apdu[c_apdu_len++] = 0x00;

    r_apdu_len = sizeof(r_apdu);
    rc = SCardTransmit(hCard, NULL, c_apdu, c_apdu_len, NULL, r_apdu, &r_apdu_len);
    if (rc != SCARD_S_SUCCESS)
    {
      printf("SCardTransmit error %08lX\n",rc);
    }

    SCardDisconnect(hCard, SCARD_LEAVE_CARD);

  } else
	if ((rc == SCARD_E_NO_SMARTCARD)
	 || (rc == SCARD_E_PROTO_MISMATCH)
   || (rc == SCARD_W_UNRESPONSIVE_CARD)
   || (rc == SCARD_W_UNPOWERED_CARD)
   || (rc == SCARD_W_RESET_CARD)
   || (rc == SCARD_W_REMOVED_CARD))
  {
    /* No card in the reader (or any other error...) try a direct connection to the reader itself */
    rc = SCardConnect(hContext, szReader, SCARD_SHARE_DIRECT, 0, &hCard, &dwProtocol);
    if (rc == SCARD_S_SUCCESS)
    {
      c_apdu[c_apdu_len++] = 0x58;
      c_apdu[c_apdu_len++] = 0x1C;
      c_apdu[c_apdu_len++] = 0x01;
      c_apdu[c_apdu_len++] = 0x00;

			rc = Control(hCard, c_apdu, c_apdu_len, r_apdu, sizeof(r_apdu), &r_apdu_len);

      SCardDisconnect(hCard, SCARD_LEAVE_CARD);
    } else
    {
      printf("SCardConnect(direct) error %08lX\n",rc);
    }
  } else
  {
    printf("SCardConnect(card) error %08lX\n",rc);
  }

  return (rc == SCARD_S_SUCCESS);
}

BOOL ShowDetails(SCARDCONTEXT hContext, const char *szReader)
{
  BYTE c_apdu[256];
  DWORD c_apdu_len;
  BYTE r_apdu[256];
  DWORD r_apdu_len;
	char vendor_name[256];
	char product_name[256];
	char product_version[256];
	char slot_name[256];

  SCARDHANDLE hCard;
  DWORD dwProtocol;

  LONG rc;

  rc = SCardConnect(hContext, szReader, SCARD_SHARE_DIRECT, 0, &hCard, &dwProtocol);
  if (rc == SCARD_S_SUCCESS)
  {
	  c_apdu_len = 0;
    c_apdu[c_apdu_len++] = 0x58;
    c_apdu[c_apdu_len++] = 0x20;
    c_apdu[c_apdu_len++] = 0x01;

		rc = Control(hCard, c_apdu, c_apdu_len, r_apdu, sizeof(r_apdu), &r_apdu_len);
		if (rc != SCARD_S_SUCCESS) goto done;

    if (r_apdu[0] == 0)
		{
		  strncpy(vendor_name, &r_apdu[1], r_apdu_len-1);
		  vendor_name[r_apdu_len-1] = '\0';
	  } else
		{
      sprintf(vendor_name, "error %02X", r_apdu[0]);
		}

	  c_apdu_len = 0;
    c_apdu[c_apdu_len++] = 0x58;
    c_apdu[c_apdu_len++] = 0x20;
    c_apdu[c_apdu_len++] = 0x02;

		rc = Control(hCard, c_apdu, c_apdu_len, r_apdu, sizeof(r_apdu), &r_apdu_len);
		if (rc != SCARD_S_SUCCESS) goto done;

    if (r_apdu[0] == 0)
		{
		  strncpy(product_name, &r_apdu[1], r_apdu_len-1);
		  product_name[r_apdu_len-1] = '\0';
	  } else
		{
      sprintf(product_name, "error %02X", r_apdu[0]);
		}

	  c_apdu_len = 0;
    c_apdu[c_apdu_len++] = 0x58;
    c_apdu[c_apdu_len++] = 0x20;
    c_apdu[c_apdu_len++] = 0x05;

		rc = Control(hCard, c_apdu, c_apdu_len, r_apdu, sizeof(r_apdu), &r_apdu_len);
		if (rc != SCARD_S_SUCCESS) goto done;

    if (r_apdu[0] == 0)
		{
		  strncpy(product_version, &r_apdu[1], r_apdu_len-1);
		  product_version[r_apdu_len-1] = '\0';
	  } else
		{
      sprintf(product_version, "error %02X", r_apdu[0]);
		}

	  c_apdu_len = 0;
    c_apdu[c_apdu_len++] = 0x58;
    c_apdu[c_apdu_len++] = 0x21;

		rc = Control(hCard, c_apdu, c_apdu_len, r_apdu, sizeof(r_apdu), &r_apdu_len);
		if (rc != SCARD_S_SUCCESS) goto done;

    if (r_apdu[0] == 0)
		{
		  strncpy(slot_name, &r_apdu[1], r_apdu_len-1);
		  slot_name[r_apdu_len-1] = '\0';
	  } else
		{
      sprintf(slot_name, "error %02X", r_apdu[0]);
		}

		printf("%s %s %s (slot: %s)\n", vendor_name, product_name, product_version, slot_name);

done:
    SCardDisconnect(hCard, SCARD_LEAVE_CARD);
  } else
  {
    printf("SCardConnect(direct) error %08lX\n",rc);
  }

  return (rc == SCARD_S_SUCCESS);
}

int main(int argc, char **argv)
{
	SCARDCONTEXT      hContext;
	LPSTR             szReaders = NULL;
  DWORD             dwReadersSz;
	LPTSTR            pReader;

	LONG   rc;
	
	UNUSED_PARAMETER(argc);
	UNUSED_PARAMETER(argv);

	/*
   * Get a handle to the PC/SC resource manager
   */
	rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
	if(rc != SCARD_S_SUCCESS)
	{
		printf("SCardEstablishContext error %08lX\n", rc);
		return EXIT_FAILURE;
	}

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
    return EXIT_FAILURE;
	}
  
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardListReaders error %08lX\n",rc);
		return EXIT_FAILURE;
	}

  /*
   * Loop withing reader(s), send the LED command to all of them
   */ 
  pReader = szReaders;
	while (*pReader != '\0')
	{
    /* Got a reader name */
    printf("\nPC/SC reader '%s' :\n", pReader);

		if (!ShowDetails(hContext, pReader))
		  goto next;

    printf("LEDs OFF...\n");
    if (!SetLeds(hContext, pReader, 0, 0, 0))
      goto next;

    printf("Buzzer...\n");
    if (!DoBuzzer(hContext, pReader))
      goto next;

#ifdef WIN32
    Sleep(1000);
#endif
#ifdef __linux__
    sleep(1);
#endif

    printf("Red ON...\n");
    if (!SetLeds(hContext, pReader, 1, 0, 0))
      goto next;

#ifdef WIN32
    Sleep(1000);
#endif
#ifdef __linux__
    sleep(1);
#endif

    printf("Green ON...\n");
    if (!SetLeds(hContext, pReader, 0, 1, 0))
      goto next;

#ifdef WIN32
    Sleep(1000);
#endif
#ifdef __linux__
    sleep(1);
#endif

    printf("Blue ON...\n");
    if (!SetLeds(hContext, pReader, 0, 0, 1))
      goto next;

#ifdef WIN32
    Sleep(1000);
#endif
#ifdef __linux__
    sleep(1);
#endif

    printf("Revert to default mode (LED controlled by firmware)\n");
    if (!SetLeds(hContext, pReader, 13, 13, 13))
      goto next;

    printf("\n");

next:
    /* Jump to next entry in multi-string array */
		pReader += strlen(pReader) + 1;
	}

  printf("Done...\n");

  /* Free the list of readers  */
  if (szReaders != NULL)
  {
    SCardFreeMemory(hContext, szReaders);
    szReaders = NULL;
  }

  SCardReleaseContext(hContext);

  return EXIT_SUCCESS;
}
