/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2011 SpringCard SAS - www.springcard.com
   
  ref_calypso_pcsc.c
  ------------------

  This is the reference applications that shows how to retrieve Calypso
  card's identifier using a PC/SC reader.

	The Calypso application selected in the card is '1TIC.ICA'.
 
  JDA 14/11/2008 : initial release, derivated from ref_calypso in Legacy SDK
	JDA 21/10/2010 : minor rewriting and port to Linux

*/
#include "pcsc_helpers.h"

static const BYTE CALYPSO_APPLICATION[] = { '1', 'T', 'I', 'C', '.', 'I', 'C', 'A' };
static const BYTE ENVIRONMENT_FILE[]    = { 0x20, 0x01 };

void CalypsoParseSelAppResponse(BYTE Resp[], int RespLen);
void CalypsoParseEnvAndHolderData(BYTE Resp[], int RespLen);

BOOL main_ex(SCARDCONTEXT hContext, const char *szReader, const BYTE atr[], DWORD atrlen)
{
  SCARDHANDLE hCard;
  DWORD dwProtocol;
  LONG rc;

  BYTE   c_apdu[128]; /* Buffer to pass the command to the card */
	BYTE   r_apdu[128]; /* Buffer to receive the response */
	DWORD  c_length;
	DWORD  r_length;

	DWORD  i;
	
	UNUSED_PARAMETER(atr);
	UNUSED_PARAMETER(atrlen);

  /* Connect to the card */
  rc = SCardConnect(hContext, szReader, SCARD_SHARE_EXCLUSIVE, SCARD_PROTOCOL_T0|SCARD_PROTOCOL_T1, &hCard, &dwProtocol);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardConnect error %08lX\n", rc);
		return FALSE;
	}

	/* Select the Calypso application within the card */
	/* ---------------------------------------------- */

	/* This is an ISO 7816-4 SELECT APDU with the CALYPSO_APPLICATION as data */
	c_length = 0;
	c_apdu[c_length++] = 0x94; /* CLA - we use 0x94 for compliance with legacy cards*/
	c_apdu[c_length++] = 0xA4; /* INS */
	c_apdu[c_length++] = 0x04; /* P1  */
	c_apdu[c_length++] = 0x00; /* P2  */
  c_apdu[c_length++] = sizeof(CALYPSO_APPLICATION); /* Lc */
	memcpy(&c_apdu[c_length], CALYPSO_APPLICATION, sizeof(CALYPSO_APPLICATION));
  c_length += sizeof(CALYPSO_APPLICATION);

	r_length = sizeof(r_apdu);
	rc = SCardTransmit(hCard, SCARD_PCI_T1, c_apdu, c_length, NULL, r_apdu, &r_length);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(SELECT CALYPSO_APPLICATION) error %08lX\n", rc);
		goto failed;
	}

	/* Check that we got SW=9000 */
	if ((r_apdu[r_length-2] != 0x90) || (r_apdu[r_length-1] != 0x00))
	{
    printf("SCardTransmit(SELECT CALYPSO_APPLICATION) -> SW=%02X%02X\n", r_apdu[r_length-2], r_apdu[r_length-1]);
	} else
	{
		/* Display the answer */
		printf("Calypso application '");
		for (i=0; i<sizeof(CALYPSO_APPLICATION); i++)
			printf("%c", CALYPSO_APPLICATION[i]);
		printf("' (");
		for (i=0; i<sizeof(CALYPSO_APPLICATION); i++)
			printf("%02X", CALYPSO_APPLICATION[i]);
		printf(") selected OK\n");
	}

  /* Parse the FCI returned by the application */
	/* ----------------------------------------- */

	if (r_length > 2)
	{
	  printf("The application has returned %lu bytes in answer:", r_length-2);
		for (i=0; i<r_length-2; i++)
		{
		  if (i % 32 == 0) printf("\n\t");
		  printf("%02X", r_apdu[i]);
		}
		printf("\n");

		/* Let us explain the FCI (and retrieve the serial number of the card in it) */
    CalypsoParseSelAppResponse(r_apdu, (int) (r_length-2));
	}

	/* Select the Environment file within the application */
	/* -------------------------------------------------- */

	/* This is an ISO 7816-4 SELECT APDU with the ENVIRONMENT file number as data */
	c_length = 0;
	c_apdu[c_length++] = 0x94; /* CLA - we use 0x94 for compliance with legacy cards */
	c_apdu[c_length++] = 0xA4; /* INS */
	c_apdu[c_length++] = 0x02; /* P1  */
	c_apdu[c_length++] = 0x00; /* P2  */
  c_apdu[c_length++] = sizeof(ENVIRONMENT_FILE); /* Lc */
	memcpy(&c_apdu[c_length], ENVIRONMENT_FILE, sizeof(ENVIRONMENT_FILE));
  c_length += sizeof(ENVIRONMENT_FILE);

	r_length = sizeof(r_apdu);
	rc = SCardTransmit(hCard, SCARD_PCI_T1, c_apdu, c_length, NULL, r_apdu, &r_length);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(SELECT ENVIRONMENT_FILE) error %08lX\n", rc);
		goto failed;
	}

	/* Check that we got SW=9000 */
	if ((r_apdu[r_length-2] != 0x90) || (r_apdu[r_length-1] != 0x00))
	{
    printf("SCardTransmit(SELECT ENVIRONMENT_FILE) -> SW=%02X%02X\n", r_apdu[r_length-2], r_apdu[r_length-1]);
	}

	/* Read record 1 of the Environment file */
	/* ------------------------------------- */

	/* This is an ISO 7816-4 READ RECORD APDU  */
	c_length = 0;
	c_apdu[c_length++] = 0x94; /* CLA - we use 0x94 for compliance with legacy cards*/
	c_apdu[c_length++] = 0xB2; /* INS */
	c_apdu[c_length++] = 1;    /* P1 = record number */
	c_apdu[c_length++] = 0x04; /* P2  */
  c_apdu[c_length++] = 0x00; /* Le */

	r_length = sizeof(r_apdu);
	rc = SCardTransmit(hCard, SCARD_PCI_T1, c_apdu, c_length, NULL, r_apdu, &r_length);
  if (rc != SCARD_S_SUCCESS)
	{
    printf("SCardTransmit(READ RECORD 1 from ENVIRONMENT FILE) error %08lX\n", rc);
		goto failed;
	}

	/* If SW=6700 we should provide Le=19 (legacy cards) */
	if ((r_apdu[r_length-2] == 0x67) && (r_apdu[r_length-1] == 0x00))
	{
		c_apdu[c_length-1] = 19; /* Le */

		r_length = sizeof(r_apdu);
		rc = SCardTransmit(hCard, SCARD_PCI_T1, c_apdu, c_length, NULL, r_apdu, &r_length);
		if (rc != SCARD_S_SUCCESS)
		{
			printf("SCardTransmit(READ RECORD 1 from ENVIRONMENT FILE) error %08lX\n", rc);
			goto failed;
		}
	}


	/* Check that we got SW=9000 */
	if ((r_apdu[r_length-2] != 0x90) || (r_apdu[r_length-1] != 0x00))
	{
    printf("SCardTransmit(READ RECORD 1 from ENVIRONMENT FILE) -> SW=%02X%02X\n", r_apdu[r_length-2], r_apdu[r_length-1]);
	}

  /* Display the record */
	if (r_length > 2)
	{
	  printf("%lu bytes of data have been returned:\n\t", r_length-2);
		for (i=0; i<r_length-2; i++)
		  printf("%02X", r_apdu[i]);
		printf("\n");

    /* Try to summarize its content */
		CalypsoParseEnvAndHolderData(r_apdu, (int) (r_length - 2));
	}

  /* OK, done */
  printf("Done!\n");
  SCardDisconnect(hCard, SCARD_RESET_CARD);
  return TRUE;

failed:
  SCardDisconnect(hCard, SCARD_RESET_CARD);
  return FALSE;
}

int main(int argc, char **argv)
{
	SCARDCONTEXT      hContext;
  BOOL              bContextOK = FALSE;
	LPSTR             szReaders = NULL;
  DWORD             dwReadersSz;
	LPTSTR            pReader;
  SCARD_READERSTATE rgscState;

	LONG   rc;
	
	UNUSED_PARAMETER(argc);
	UNUSED_PARAMETER(argv);

  printf("\n");
  printf("SpringCard PC/SC SDK : Calypso card demo\n");
  printf("----------------------------------------\n");
  printf("v.250111 (c) SpringCard SAS 2011\n");
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
      /* Call core function */
      main_ex(hContext, pReader, rgscState.rgbAtr, rgscState.cbAtr);
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





/*
 * TLVLoop
 * --------
 * A really (too much ?) simple ASN1 T,L,V parser
 */
BOOL TLVLoop(BYTE buffer[], int *offset, WORD *tag, int *length, BYTE *value[])
{
  WORD t;
  int o, l;

  if (buffer == NULL)
    return FALSE;

  if (offset != NULL)
    o = *offset;
  else
    o = 0;

  if ((buffer[o] == 0x00) || (buffer[o] == 0xFF))
    return FALSE;

  /* Read the tag */
  if ((buffer[o] & 0x1F) != 0x1F)
  {
    /* Short tag */
    t = buffer[o++];
  } else
  {
    /* Long tag */
    t = buffer[o++];
    t <<= 8;
    t |= buffer[o++];
  }

  if (tag != NULL)
    *tag = t;

  /* Read the length */
  if (buffer[o] & 0x80)
  {
    /* Multi-byte lenght */
    switch (buffer[o++] & 0x7F)
    {
      case 0x01:
        l = buffer[o++];
        break;
      case 0x02:
        l = buffer[o++];
        l <<= 8;
        l += buffer[o++];
        break;
      default:
        return FALSE;
    }
  } else
  {
    /* Length on a single byte */
    l = buffer[o++];
  }

  if (l > 65535)
    return FALSE;              /* Overflow */

  if (length != NULL)
    *length = (WORD) l;

  /* Get a pointer on data */
  if (value != NULL)
    *value = &buffer[o];

  /* Jump to the end of data */
  o += l;

  if (offset != NULL)
    *offset = (WORD) o;

  return TRUE;
}


/*
 * Parse Calypso FCI Discretionary Data
 */
void CalypsoParseFCIDisc(BYTE FciDisc[], int FciDiscLen)
{
  int offset, length;
  WORD tag;
  BYTE *value;
  int i;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < FciDiscLen) && TLVLoop(FciDisc, &offset, &tag, &length, &value))
  {
    if (tag == 0xC7)
    {
      /* Full serial number */
      printf("\tSerial Number      = ");
      for (i=0; i<length; i++)
        printf("%02X", value[i]);
      printf("\n");
    } else
    if (tag == 0x53)
    {
      /* Discretionary Data */
			printf("\tSessionMaxMods     = %02X\n", value[0]);
			printf("\tPlatform           = %02X\n", value[1]);
			printf("\tType               = %02X\n", value[2]);
			printf("\tSubType            = %02X\n", value[3]);
			printf("\tSoftIssuer         = %02X\n", value[4]);
			printf("\tSoftVersion        = %02X\n", value[5]);
			printf("\tSoftRevision       = %02X\n", value[6]);

    } else
    {
      printf("Tag %04X, length=%d (unhandled)\n", tag, length);
    }
  }
}

/*
 * Parse Calypso FCI Proprietary Data
 */
void CalypsoParseFCIProp(BYTE FciProp[], int FciPropLen)
{
  int offset, length;
  WORD tag;
  BYTE *value;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < FciPropLen) && TLVLoop(FciProp, &offset, &tag, &length, &value))
  {
    if (tag == 0xBF0C)
    {
      /* This is the FCI Issuer Discreationary template */
      CalypsoParseFCIDisc(value, length);
    } else
    {
      printf("Tag %04X, length=%d (unhandled)\n", tag, length);
    }
  }
}

/*
 * Parse Calypso FCI
 */
void CalypsoParseFCI(BYTE Fci[], int FciLen)
{
  int offset, length;
  WORD tag;
  BYTE *value;
	int i;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < FciLen) && TLVLoop(Fci, &offset, &tag, &length, &value))
  {
    if (tag == 0x84)
    {
      /* DF name */
      printf("\tDF name (%02d)       = '", length);
			for (i=0; i<length; i++)
			  printf("%c", value[i]);
			printf("'\n\t                    (");
			for (i=0; i<length; i++)
			  printf("%02X", value[i]);
			printf(")\n");
    } else
    if (tag == 0xA5)
    {
      /* This is the FCI Proprietary template */
      CalypsoParseFCIProp(value, length);
    } else
    {
      printf("Tag %04X, length=%d (unhandled)\n", tag, length);
    }
  }
}

/*
 * Parse Calypso response to select application (must be a FCI)
 */
void CalypsoParseSelAppResponse(BYTE Resp[], int RespLen)
{
  int offset, length;
  WORD tag;
  BYTE *value;

  /* Parse the TLV structure */
  offset = 0;
  while ((offset < RespLen) && TLVLoop(Resp, &offset, &tag, &length, &value))
  {
    if (tag == 0x6F)
    {
      /* This is the FCI template */
			printf("Found an FCI template\n");
      CalypsoParseFCI(value, length);
    } else
    {
      printf("Tag %04X, length=%d (unhandled)\n", tag, length);
    }
  }

}

/*
 * Data within the records of a Calypso cards are not aligned on byte boundaries, so
 * we must be able to get then bit by bit
 */
unsigned short GetBits(unsigned char buffer[], unsigned int *offset, unsigned char count)
{
  unsigned char i;
  unsigned short r = 0;

  if (count > 16) return r;

  for (i=0; i<count; i++)
  {
    unsigned int byte_offset = *offset / 8;
    unsigned int bit_offset  = 7 - (*offset % 8);
    unsigned char mask = 1 << bit_offset;

    r <<= 1;
    r |= (buffer[byte_offset] & mask) ? 1 : 0;

    *offset = *offset + 1;
  }

  return r;
}

/*
 * Minimalistic parser of the EnvironmentAndHolder file
 */
void CalypsoParseEnvAndHolderData(BYTE Resp[], int RespLen)
{
  unsigned int offset = 0;
  unsigned char bitmap, version, app_issuer;
  unsigned short network_country, network_ident, end_date;
  
  UNUSED_PARAMETER(RespLen); // TODO : check the length before getting bits

  version         = (unsigned char) GetBits(Resp, &offset, 6);
  printf("envVersionNumber       = %d\n", version);

  bitmap          = (unsigned char) GetBits(Resp, &offset, 7);
  printf("  bitmap=%02X\n", bitmap);

  if (bitmap & 0x01)
  {
    network_country = GetBits(Resp, &offset, 12);
    network_ident   = GetBits(Resp, &offset, 12);
    printf("envNetworkId           = %03X %03X\n", network_country, network_ident);
  }
  if (bitmap & 0x02)
  {
    app_issuer      = (unsigned char) GetBits(Resp, &offset, 8);
    printf("envApplicationIssuerId = %02X\n", app_issuer);
  }
  if (bitmap & 0x04)
  {
    end_date        = GetBits(Resp, &offset, 14);
    printf("envApplicationEndDate  = %d\n", end_date);
  }
}
