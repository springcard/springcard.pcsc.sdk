/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2011 SpringCard SAS - www.springcard.com
   
  ref_memory_pcsc.c
  -----------------

*/
#include "pcsc_helpers.h"

BOOL memory_card_read(CARD_CHANNEL_T *card_channel);

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
  printf("SpringCard PC/SC SDK : minimalist Memory Card reader\n");
  printf("----------------------------------------------------\n");
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

			printf("Trying to read the card...\n");

      memory_card_read(&card_channel);


      /* Disconnect from the card */
      CardDisconnect(&card_channel, SCARD_RESET_CARD);
		}

	}
  printf("\n");

	FreeReaderList(pcsc_readers);
  return EXIT_SUCCESS;
}

BOOL memory_card_read(CARD_CHANNEL_T *card_channel)
{
  BYTE capdu[5];
	BYTE rapdu[256+2];
	DWORD rapdu_length;
	unsigned int address = 0;
	unsigned int i;

	for (address=0; address<0x8000; address++)
	{
    /* Try to read with a free-length (Le = 0x00) */
		capdu[0] = 0xFF;
		capdu[1] = 0xB0;
		capdu[2] = (BYTE) (address >> 8);
		capdu[3] = (BYTE) (address);
		capdu[4] = 0x00;

		rapdu_length = sizeof(rapdu);
		if (!CardTransmit(card_channel, capdu, sizeof(capdu), rapdu, &rapdu_length))
		{
      printf("Error at address %05d\n", address);
			return FALSE;
		}

		if ((rapdu[rapdu_length-2] == 0x6C))
		{
      /* The card says we shall try again with the returned Le */
		  BYTE le = rapdu[rapdu_length-1];

		  capdu[0] = 0xFF;
		  capdu[1] = 0xB0;
		  capdu[2] = (BYTE) (address >> 8);
		  capdu[3] = (BYTE) (address);
		  capdu[4] = le;

		  rapdu_length = sizeof(rapdu);
		  if (!CardTransmit(card_channel, capdu, sizeof(capdu), rapdu, &rapdu_length))
		  {
        printf("Error at address %05d\n", address);
			  return FALSE;
		  }
		}

    /* Status must be OK (0x9000) */
		if ((rapdu[rapdu_length-2] != 0x90) || (rapdu[rapdu_length-1] != 0x00))
		{
		  printf("Failed at address %05d, SW=%02X%02X\n", address, rapdu[rapdu_length-2], rapdu[rapdu_length-1]);
			return FALSE;
		}

		printf("%05d: ", address);

		for (i=0; i<rapdu_length-2; i++)
			printf("%02X", rapdu[i]);

		printf("  ");
    for (i=0; i<rapdu_length-2; i++)
      printf("%c", rapdu[i] >= ' ' ? rapdu[i] : '.');

		printf("\n");
	}
  
	printf("\n");
  return TRUE;
}
