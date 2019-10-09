/*
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED
  TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.

  Copyright (c) 2003-2012 SpringCard SAS - www.springcard.com
*/

#include "pcsc_helpers.h"
#include "reader_helpers.h"

BOOL ReaderLeds(const char *szReaderName, BYTE red, BYTE green, BYTE blue)
{
  BYTE c_apdu[256];
  DWORD c_apdu_len;
  BYTE r_apdu[256];
  DWORD r_apdu_len;

  CARD_CHANNEL_T Channel = { 0 };
  BOOL rc = FALSE;  

  Channel.bSilent = TRUE;
  
  /* Try to connect to the card in the reader (if there's one) */
  if (CardConnect(szReaderName, &Channel))
  {
    /* Card connected, use the APDU method */   
    c_apdu_len = 0;
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
    rc = CardTransmit(&Channel, c_apdu, c_apdu_len, r_apdu, &r_apdu_len);

    CardDisconnect(&Channel, SCARD_LEAVE_CARD);
  }
  
  if (!rc)
  {
    /* No card in the reader (or any other error...) try a direct connection to the reader itself */
    if (ReaderConnect(szReaderName, &Channel))
    {
      c_apdu_len = 0;
      c_apdu[c_apdu_len++] = 0x58;
      c_apdu[c_apdu_len++] = 0x1E;
      c_apdu[c_apdu_len++] = red;
      c_apdu[c_apdu_len++] = green;
      c_apdu[c_apdu_len++] = blue;

      r_apdu_len = sizeof(r_apdu);
      rc = ReaderControl(&Channel, c_apdu, c_apdu_len, r_apdu, &r_apdu_len);

      CardDisconnect(&Channel, SCARD_LEAVE_CARD);
    }
  }

  return rc;
}

BOOL ReaderBuzzer(const char *szReaderName, WORD buz_time_ms)
{
  BYTE c_apdu[256];
  DWORD c_apdu_len;
  BYTE r_apdu[256];
  DWORD r_apdu_len;

  CARD_CHANNEL_T Channel = { 0 };
  BOOL rc = FALSE;  

  Channel.bSilent = TRUE;

  /* Try to connect to the card in the reader (if there's one) */
  if (CardConnect(szReaderName, &Channel))
  {
    /* Card connected, use the APDU method */   
    c_apdu_len = 0;
    c_apdu[c_apdu_len++] = 0xFF;
    c_apdu[c_apdu_len++] = 0xF0;
    c_apdu[c_apdu_len++] = 0x00;
    c_apdu[c_apdu_len++] = 0x00;
    c_apdu[c_apdu_len++] = 3;
    c_apdu[c_apdu_len++] = 0x1C;
    c_apdu[c_apdu_len++] = (BYTE) (buz_time_ms / 0x0100);
    c_apdu[c_apdu_len++] = (BYTE) (buz_time_ms % 0x0100);

    r_apdu_len = sizeof(r_apdu);
    rc = CardTransmit(&Channel, c_apdu, c_apdu_len, r_apdu, &r_apdu_len);

    CardDisconnect(&Channel, SCARD_LEAVE_CARD);
  }
  
  if (!rc)
  {
    /* No card in the reader (or any other error...) try a direct connection to the reader itself */
    if (ReaderConnect(szReaderName, &Channel))
    {
      c_apdu_len = 0;
      c_apdu[c_apdu_len++] = 0x58;
      c_apdu[c_apdu_len++] = 0x1C;
      c_apdu[c_apdu_len++] = (BYTE) (buz_time_ms / 0x0100);
      c_apdu[c_apdu_len++] = (BYTE) (buz_time_ms % 0x0100);

      r_apdu_len = sizeof(r_apdu);
      rc = ReaderControl(&Channel, c_apdu, c_apdu_len, r_apdu, &r_apdu_len);

      CardDisconnect(&Channel, SCARD_LEAVE_CARD);
    }
  }

  return rc;
}

BOOL ReaderDetails(const char *szReaderName)
{
  BYTE c_apdu[256];
  DWORD c_apdu_len;
  BYTE r_apdu[256];
  DWORD r_apdu_len;
  
	char vendor_name[256];
	char product_name[256];
	char product_version[256];
	char slot_name[256];

  CARD_CHANNEL_T Channel = { 0 };

  if (!ReaderConnect(szReaderName, &Channel))
    return FALSE;

  c_apdu_len = 0;
  c_apdu[c_apdu_len++] = 0x58;
  c_apdu[c_apdu_len++] = 0x20;
  c_apdu[c_apdu_len++] = 0x01;

  if (!ReaderControl(&Channel, c_apdu, c_apdu_len, r_apdu, &r_apdu_len))
    return FALSE;
    
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

  if (!ReaderControl(&Channel, c_apdu, c_apdu_len, r_apdu, &r_apdu_len))
    goto failed;

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

  if (!ReaderControl(&Channel, c_apdu, c_apdu_len, r_apdu, &r_apdu_len))
    goto failed;

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

  if (!ReaderControl(&Channel, c_apdu, c_apdu_len, r_apdu, &r_apdu_len))
    goto failed;

  if (r_apdu[0] == 0)
  {
    strncpy(slot_name, &r_apdu[1], r_apdu_len-1);
    slot_name[r_apdu_len-1] = '\0';
  } else
  {
    sprintf(slot_name, "error %02X", r_apdu[0]);
  }

  printf("%s %s %s (slot: %s)\n", vendor_name, product_name, product_version, slot_name);

  CardDisconnect(&Channel, SCARD_LEAVE_CARD);
  return TRUE;

failed:
  CardDisconnect(&Channel, SCARD_LEAVE_CARD);
  return FALSE;
}