/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2011 SpringCard SAS - www.springcard.com
   
  ref_mifulc_pcsc.c
  -----------------

*/
#include "pcsc_helpers.h"
#include "cardware/mifare/ultralight-c/pcsc_mifulc.h"

#define NO_TRACKING

BOOL mifulc_test(CARD_CHANNEL_T *card_channel);

int main(int argc, char **argv)
{
  int i;
  char **pcsc_readers;
	CARD_CHANNEL_T card_channel;
	
	UNUSED_PARAMETER(argc);
	UNUSED_PARAMETER(argv);

  /* Welcome message, check parameters */
  /* --------------------------------- */

  printf("\n");
  printf("SpringCard PC/SC SDK : NXP Mifare UltraLight C reference\n");
  printf("--------------------------------------------------------\n");
  printf("(c) SpringCard SAS 2010\n");

	/* Retrieve the list of PC/SC readers */
	/* ---------------------------------- */
	if (!GetReaderList(&pcsc_readers))
	{
	  printf("GetReaderList failed\n");
		return EXIT_FAILURE;
	}

  /* Loop through the found PC/SC readers */
	for (i=0; pcsc_readers[i] != NULL; i++)
	{
    printf("\n%s\n", pcsc_readers[i]);

    /* Display the status */
    PrintReaderStatus(pcsc_readers[i]);

    /* If a card is available... */
		/* ------------------------- */
		if (CardAvailable(pcsc_readers[i]))
	  {
		  /* Let's try to read it... */
			/* ----------------------- */

			if (!CardConnect(pcsc_readers[i], &card_channel))
			{
			  /* Could not connect to the card */
		    printf("Failed to connect to the card...\n");
        continue;
			}


      mifulc_test(&card_channel);


      /* Disconnect from the card */
      CardDisconnect(&card_channel, SCARD_RESET_CARD);
		}

	}
  printf("\n");


	FreeReaderList(pcsc_readers);
  return EXIT_SUCCESS;
}

BOOL mifulc_is_page_locked(BYTE page, BYTE lock_bytes[4])
{
  if (page < 16)
	{
	  if (lock_bytes[page / 8] & (1 << (page % 8)))
		  return TRUE;
  }	else
	if (page < 20)
	{
    if (lock_bytes[2] & 0x02)
	    return TRUE;
  }	else
	if (page < 24)
	{
    if (lock_bytes[2] & 0x04)
	    return TRUE;
  }	else
	if (page < 28)
	{
    if (lock_bytes[2] & 0x08)
	    return TRUE;
	} else
	if (page < 32)
	{
    if (lock_bytes[2] & 0x20)
	    return TRUE;
  }	else
	if (page < 36)
	{
    if (lock_bytes[2] & 0x40)
	    return TRUE;
  }	else
	if (page < 40)
	{
    if (lock_bytes[2] & 0x80)
	    return TRUE;
	} else
	if (page == 41)
	{
    if (lock_bytes[3] & 0x10)
	    return TRUE;
	} else
	if (page == 42)
	{
    if (lock_bytes[3] & 0x20)
	    return TRUE;
	} else
	if (page == 43)
	{
    if (lock_bytes[3] & 0x40)
	    return TRUE;
	} else
	if (page < 48)
	{
    if (lock_bytes[3] & 0x80)
	    return TRUE;
	}
	return FALSE;
}

BOOL mifulc_authenticate(CARD_CHANNEL_T *card_channel)
{
  LONG rc;

  BYTE blank_key[16]     = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                             0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

  BYTE transport_key[16] = { 0x49, 0x45, 0x4D, 0x4B, 0x41, 0x45, 0x52, 0x42,
                             0x21, 0x4E, 0x41, 0x43, 0x55, 0x4F, 0x59, 0x46 };

  SCardMifUlC_WakeUp(card_channel->hCard);

  rc = SCardMifUlC_Authenticate(card_channel->hCard, blank_key);
  if (rc != 0)
  {
    SCardMifUlC_WakeUp(card_channel->hCard);

    rc = SCardMifUlC_Authenticate(card_channel->hCard, transport_key);
    if (rc != 0)
    {
      SCardMifUlC_WakeUp(card_channel->hCard);
    }
  }

  if (rc == 0) return TRUE;

  printf("SCardMifUlC_Authenticate : rc=%ld %08lX\n", rc, rc);
	return FALSE;
}

BOOL mifulc_test(CARD_CHANNEL_T *card_channel)
{
  LONG rc;
  BOOL f = FALSE;

  BYTE buffer[16];

  BYTE page;
	BYTE card_data[48 * 4];
  BYTE i;

	BYTE lock_bytes[4];

  printf("Testing the Mifare Ul C library on this card...\n");

  SCardMifUlC_AttachLibrary(card_channel->hCard);
  SCardMifUlC_SuspendTracking(card_channel->hCard);

	if (mifulc_authenticate(card_channel))
	{
	  printf("We are authenticated!\n");
	}

  printf("Reading\n");
  for (page=0; page<44; page+=4)
  {
    rc = SCardMifUlC_Read(card_channel->hCard, page, &card_data[4 * page]);

    if (rc != 0)
    {
      printf("SCardMifUlC_Read(%d) : rc=%ld %08lX\n", page, rc, rc);
			mifulc_authenticate(card_channel);
      continue;
    }

    printf("%02d:", page);
    for (i=0; i<16; i++)
		{
		  if ((i % 4) == 0) printf(" ");
      printf(" %02X", card_data[4 * page + i]);
		}
    printf("   ");
    for (i=0; i<16; i++)
      printf("%c", card_data[4 * page + i] >= ' ' ? card_data[4 * page + i] : '.');
    printf("\n");
  }

	/* Display OTP bytes */
	printf("OTP bytes  = %02X%02X%02X%02X\n", card_data[3 * 4 + 0],
	                                         card_data[3 * 4 + 1],
																					 card_data[3 * 4 + 2],
																					 card_data[3 * 4 + 3]);

	printf("Auth0      = %02X", card_data[4 * 42 + 0]);
	printf("\t(authentication is required for pages >= %d)\n", card_data[4 * 42 + 0]);
	printf("Auth1      = %02X", card_data[4 * 43 + 0]);
	if (card_data[4 * 43 + 0] & 0x01)
	  printf("\t(freely readable, write requires authentication)\n");
	else
	  printf("\t(both read and write require authentication)\n");

  lock_bytes[0] = card_data[4 *  2 + 2];
	lock_bytes[1] = card_data[4 *  2 + 3];
  lock_bytes[2] = card_data[4 * 40 + 0];
	lock_bytes[3] = card_data[4 * 40 + 1];


	printf("Lock bytes = %02X%02X%02X%02X\n", lock_bytes[0],
	                                          lock_bytes[1],
																					  lock_bytes[2],
																					  lock_bytes[3]);

  /* Display lock bytes */

	if (lock_bytes[0] & 0x01)
	  printf("OTP bytes are locked\n");
	  printf("Lock bit of Counter is locked\n");

  for (page = 4; page < 40; page++)
	{
	  if (mifulc_is_page_locked(page, lock_bytes))
		  printf("Page %d is locked\n", page);

		if (page == 9)
		{
			if (lock_bytes[0] & 0x02)
				printf("Lock bits of pages 4-9 are locked\n");
		} else
		if (page == 15)
		{
			if (lock_bytes[0] & 0x04)
				printf("Lock bits of pages 10-15 are locked\n");
		} else
		if (page == 27)
		{
		  if (lock_bytes[2] & 0x01)
			  printf("Lock bits of pages 16-27 are locked\n");
		} else
		if (page == 39)
		{
			if (lock_bytes[2] & 0x10)
				printf("Lock bits of pages 28-39 are locked\n");
		}
		
  }	

	if (mifulc_is_page_locked(41, lock_bytes))
	  printf("Counter (page 41) is locked\n");
	if (mifulc_is_page_locked(42, lock_bytes))
	  printf("Auth0 (page 42) is locked\n");
	if (lock_bytes[2] & 0x02)
	  printf("Lock bit of Auth0 is locked\n");
	if (mifulc_is_page_locked(43, lock_bytes))
	  printf("Auth1 (page 43) is locked\n");
	if (lock_bytes[2] & 0x04)
	  printf("Lock bit of Auth1 is locked\n");
	if (mifulc_is_page_locked(44, lock_bytes))
	  printf("Key (pages 44-47) is locked\n");
	if (lock_bytes[2] & 0x08)
	  printf("Lock bit of Key is locked\n");

  if (1) 
  {
    for (page=4; page<40; page++)
    {
			if (mifulc_is_page_locked(page, lock_bytes))
			  continue;

      memset(buffer, 0xCD, sizeof(buffer));

      buffer[0] = 'S';
      buffer[1] = 'C';
      buffer[2] = '0' + page / 10;
      buffer[3] = '0' + page % 10;

      rc = SCardMifUlC_Write4(card_channel->hCard, page, buffer);

      if (rc != 0)
      {
        printf("SCardMifUlC_Write4(%d) : rc=%ld %08lX\n", page, rc, rc);
        mifulc_authenticate(card_channel);
      }
    }

    for (page=0; page<44; page+=4)
    {
      rc = SCardMifUlC_Read(card_channel->hCard, page, buffer);

      if (rc != 0)
      {
        printf("SCardMifUlC_Read(%d) : rc=%ld %08lX\n", page, rc, rc);
        mifulc_authenticate(card_channel);
      }

      printf("%02d:", page);
      for (i=0; i<16; i++)
			{
			  if ((i % 4) == 0) printf(" ");
        printf(" %02X", buffer[i]);
			}
      printf("   ");
      for (i=0; i<16; i++)
        printf("%c", buffer[i] >= ' ' ? buffer[i] : '.');

			if (mifulc_is_page_locked(page, lock_bytes))
			  printf("  [L]");
      printf("\n");
    }

    for (page=4; page<40; page++)
    {
			if (mifulc_is_page_locked(page, lock_bytes))
			  continue;

      memset(buffer, 0, sizeof(buffer));

      rc = SCardMifUlC_Write4(card_channel->hCard, page, buffer);

      if (rc != 0)
      {
        printf("SCardMifUlC_Write4(%d) : rc=%ld %08lX\n", page, rc, rc);
        mifulc_authenticate(card_channel);
      }
    }
  }

  SCardMifUlC_ResumeTracking(card_channel->hCard);
  SCardMifUlC_DetachLibrary(card_channel->hCard);


  printf("\n\n\n");
  return f;
}
