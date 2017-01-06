/**h* PCSCMon/get_data.c
 *
 * NAME
 *   get_data.c :: Typical use of the PC/SC GET DATA command (C implementation)
 *
 * DESCRIPTION
 *   In this part we connect to the smart card and exchange APDUs to fetch
 *   interesting data. The main point is that the data come from the reader,
 *   and not from the card itself.
 *
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS 2008
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

#include <stdio.h>
#include <string.h>
#include <winscard.h>
#include "pcscmon.h"

WORD Test_Entry(SCARDHANDLE hCard, DWORD dwProtocol, DWORD dwSendLength,
                BYTE bWantLength, BYTE bWantDelay);

BOOL GetAndShowCardEntry(SCARDHANDLE hCard, DWORD dwProtocol, BYTE bParam1,
                         BYTE bParam2);
BOOL GetCardEntry(SCARDHANDLE hCard, DWORD dwProtocol, BYTE bParam1,
                  BYTE bParam2, BYTE pbDataOut[], DWORD * pdwDataOutLen);
void ShowCardEntry(BYTE bParam1, BYTE pParam2, BYTE pbData[], DWORD dwLength);

WORD GetReaderEntry(SCARDHANDLE hCard, DWORD dwProtocol, BYTE bParam,
                    BYTE pbDataOut[], DWORD * pdwDataOutLen);
WORD GetReaderSlotName(SCARDHANDLE hCard, DWORD dwProtocol, BYTE pbDataOut[],
                       DWORD * pdwDataOutLen);
void ShowReaderEntry(BYTE bParam, BYTE pbData[], DWORD dwLength);

//  BYTE         pbData[254];
//  DWORD        dwLength;
//  WORD         sw;

//#define _BENCHMARK

#ifdef _BENCHMARK
#ifdef WIN32
#include <time.h>
static clock_t t0, t1;

#define BENCH_T0 t0 = clock()
#define BENCH_T1 t1 = clock()
#define BENCH_PRINT() printf("-- %ld\n", t1 - t0);
#endif
#ifdef __linux__
#include <sys/time.h>
static struct timeval t0, t1;

#define BENCH_T0 gettimeofday(&t0, NULL)
#define BENCH_T1 gettimeofday(&t1, NULL)
#define BENCH_PRINT() do { long int dus; dus = t1.tv_sec - t0.tv_sec; dus *= 10000000; dus += t1.tv_usec - t0.tv_usec; printf("-- %ldus\n", dus); } while(0)
#endif
#else
#define BENCH_T0
#define BENCH_T1
#define BENCH_PRINT()
#endif

void ShowCardData(const char *szReader)
{
  SCARDCONTEXT hContext;
  SCARDHANDLE hCard;
  DWORD dwProtocol;
  LONG rc;

  /* Instanciate a new PC/SC context (we do this in order to have a  */
  /* standalone function, but in real-life application you can use a */
  /* global context, established by the caller.                      */
  rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
  if (rc != SCARD_S_SUCCESS)
    return;

  /*
   * Connect to the card, accept either T=0 or T=1
   * ---------------------------------------------
   */
  rc = SCardConnect(hContext,
                    szReader,
                    SCARD_SHARE_SHARED,
                    SCARD_PROTOCOL_T0 | SCARD_PROTOCOL_T1,
                    &hCard, &dwProtocol);
  if (rc != SCARD_S_SUCCESS)
  {
    printf("\tSCardConnect error %lXh (%ld)\n", rc, rc);
    goto done_no_card;
  }

  printf("\tConnected to the card, protocol ");
  switch (dwProtocol)
  {
    case SCARD_PROTOCOL_T0:
      printf("T=0");
      break;
    case SCARD_PROTOCOL_T1:
      printf("T=1");
      break;
    default:
      printf("%08lXh", dwProtocol);
  }
  printf("\n");

  /*
   * Get every available data (and explain)
   * --------------------------------------
   */

  /* This part shoul'd be common to all PC/SC readers,  */
  /* at least for contactless cards (but actually a lot */
  /* are not compliant...)                              */
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0x00, 0x00))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0x01, 0x00))
    goto failed;

  /* This part is specific to SPRINGCARD readers for    */
  /* contactless smartcards or memory cards             */
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xF0, 0x00))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xF1, 0x00))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xF1, 0x01))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xF2, 0x00))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFA, 0x00))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFC, 0x00))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFC, 0x01))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFC, 0x02))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFC, 0x03))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFF, 0x00))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFF, 0x01))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFF, 0x02))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFF, 0x81))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFF, 0x82))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFF, 0x83))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFF, 0x84))
    goto failed;
  if (!GetAndShowCardEntry(hCard, dwProtocol, 0xFF, 0x85))
    goto failed;

  /*
   * Done, disconnect from the card, release the context
   * ---------------------------------------------------
   */
failed:
  SCardDisconnect(hCard, SCARD_LEAVE_CARD);
done_no_card:
  SCardReleaseContext(hContext);
  printf("\n");
}

void ShowReaderData(const char *szReader)
{
  SCARDCONTEXT hContext;
  SCARDHANDLE hCard;
  DWORD dwProtocol;
  BYTE pbData[254];
  DWORD dwLength;
  LONG rc;
  WORD sw;

  /* Instanciate a new PC/SC context (we do this in order to have a  */
  /* standalone function, but in real-life application you can use a */
  /* global context, established by the caller.                      */
  rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
  if (rc != SCARD_S_SUCCESS)
    return;

  /*
   * Connect to the card, accept either T=0 or T=1
   * ---------------------------------------------
   */
  rc = SCardConnect(hContext,
                    szReader,
                    SCARD_SHARE_SHARED,
                    SCARD_PROTOCOL_T0 | SCARD_PROTOCOL_T1,
                    &hCard, &dwProtocol);
  if (rc != SCARD_S_SUCCESS)
  {
    printf("\tSCardConnect error %lXh (%ld)\n", rc, rc);
    goto done_no_card;
  }

  printf("\tReader information :\n");

  /*
   * Get every available reader data (and explain)
   * ---------------------------------------------
   */

  sw = GetReaderEntry(hCard, dwProtocol, 0x01, pbData, &dwLength);
  if (sw == (WORD) - 1)
    goto done;
  if ((sw == 0x9000) && (dwLength))
    ShowReaderEntry(0x01, pbData, dwLength);

  sw = GetReaderEntry(hCard, dwProtocol, 0x02, pbData, &dwLength);
  if (sw == (WORD) - 1)
    goto done;
  if ((sw == 0x9000) && (dwLength))
    ShowReaderEntry(0x02, pbData, dwLength);

  sw = GetReaderEntry(hCard, dwProtocol, 0x03, pbData, &dwLength);
  if (sw == (WORD) - 1)
    goto done;
  if ((sw == 0x9000) && (dwLength))
    ShowReaderEntry(0x03, pbData, dwLength);

  sw = GetReaderEntry(hCard, dwProtocol, 0x04, pbData, &dwLength);
  if (sw == (WORD) - 1)
    goto done;
  if ((sw == 0x9000) && (dwLength))
    ShowReaderEntry(0x04, pbData, dwLength);

  sw = GetReaderEntry(hCard, dwProtocol, 0x05, pbData, &dwLength);
  if (sw == (WORD) - 1)
    goto done;
  if ((sw == 0x9000) && (dwLength))
    ShowReaderEntry(0x05, pbData, dwLength);

  sw = GetReaderSlotName(hCard, dwProtocol, pbData, &dwLength);
  if (sw == (WORD) - 1)
    goto done;
  if ((sw == 0x9000) && (dwLength))
    ShowReaderEntry(0x21, pbData, dwLength);

  /*
   * Done, disconnect from the card, release the context
   * ---------------------------------------------------
   */
done:
  SCardDisconnect(hCard, SCARD_LEAVE_CARD);
done_no_card:
  SCardReleaseContext(hContext);
  printf("\n");
}

void TestReader(const char *szReader)
{
  SCARDCONTEXT hContext;
  SCARDHANDLE hCard;
  DWORD dwProtocol;
  LONG rc;
  WORD sw;
  BYTE t;
  WORD l;


  rc = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
  if (rc != SCARD_S_SUCCESS)
    return;

  rc = SCardConnect(hContext,
                    szReader,
                    SCARD_SHARE_SHARED,
                    SCARD_PROTOCOL_T0 | SCARD_PROTOCOL_T1,
                    &hCard, &dwProtocol);
  if (rc != SCARD_S_SUCCESS)
  {
    printf("\tSCardConnect error %lXh (%ld)\n", rc, rc);
    goto done_no_card;
  }

  printf("\tConnected to the card, protocol ");
  switch (dwProtocol)
  {
    case SCARD_PROTOCOL_T0:
      printf("T=0");
      break;
    case SCARD_PROTOCOL_T1:
      printf("T=1");
      break;
    default:
      printf("%08lXh", dwProtocol);
  }
  printf("\n");


  for (t = 0; t < 64; t += 7)
  {
    sw = Test_Entry(hCard, dwProtocol, 0, 30, t);
    if (sw != 0x9000)
      break;
  }

  for (l = 0; l < 256; l++)
  {
    sw = Test_Entry(hCard, dwProtocol, l, 0, 0);
    if (sw != 0x9000)
      break;
  }

  for (l = 0; l < 256; l++)
  {
    sw = Test_Entry(hCard, dwProtocol, 0, (BYTE) l, 0);
    if (sw != 0x9000)
      break;
  }

  SCardDisconnect(hCard, SCARD_LEAVE_CARD);
done_no_card:
  SCardReleaseContext(hContext);
  printf("\n");
}

BOOL GetCardEntry(SCARDHANDLE hCard, DWORD dwProtocol, BYTE bParam1,
                  BYTE bParam2, BYTE pbDataOut[], DWORD * pdwDataOutLen)
{
  LONG rc;
  WORD sw;
  const SCARD_IO_REQUEST *pioSendPci;
  BYTE pbSendBuffer[5];
  BYTE pbRecvBuffer[256];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  switch (dwProtocol)
  {
    case SCARD_PROTOCOL_T0:
      pioSendPci = SCARD_PCI_T0;
      break;
    case SCARD_PROTOCOL_T1:
      pioSendPci = SCARD_PCI_T1;
      break;

    default:
      pioSendPci = NULL;
  }

  /*
   * Prepare the APDU
   * ----------------
   */
  pbSendBuffer[0] = 0xFF;       /* CLA for the embedded APDU interpreter */
  pbSendBuffer[1] = 0xCA;       /* INS for the GET DATA command */
  pbSendBuffer[2] = bParam1;    /* P1 */
  pbSendBuffer[3] = bParam2;    /* P2 */
  pbSendBuffer[4] = 0x00;       /* Le : accept extact actual length */

  /*
   * Do the APDU exchange
   * --------------------
   */
  BENCH_T0;
  rc = SCardTransmit(hCard,
                     pioSendPci,
                     pbSendBuffer,
                     sizeof(pbSendBuffer),
                     NULL, pbRecvBuffer, &pcbRecvLength);
  BENCH_T1;
  if (rc != SCARD_S_SUCCESS)
  {
    printf("\tSCardTransmit error %lXh (%ld)\n", rc, rc);
    return FALSE;
  }
  BENCH_PRINT();

  /*
   * Get the status word
   * -------------------
   */
  if (pcbRecvLength < 2)
  {
    printf("SCardTransmit : response too small (%ld)\n", pcbRecvLength);
    return FALSE;
  }

  sw = pbRecvBuffer[pcbRecvLength - 2] << 8;
  sw |= pbRecvBuffer[pcbRecvLength - 1];

  if (sw == 0x9000)
  {
    /*
     * Get the data (assume there's enough room in the buffer)
     * -------------------------------------------------------
     */
    memcpy(pbDataOut, pbRecvBuffer, pcbRecvLength - 2);
    *pdwDataOutLen = pcbRecvLength - 2;
  } else
  {
    /* No data */
    *pdwDataOutLen = 0;
  }

  return TRUE;
}

WORD GetReaderEntry(SCARDHANDLE hCard, DWORD dwProtocol, BYTE bParam,
                    BYTE pbDataOut[], DWORD * pdwDataOutLen)
{
  LONG rc;
  WORD sw;
  const SCARD_IO_REQUEST *pioSendPci;
  BYTE pbSendBuffer[7];
  BYTE pbRecvBuffer[256];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  switch (dwProtocol)
  {
    case SCARD_PROTOCOL_T0:
      pioSendPci = SCARD_PCI_T0;
      break;
    case SCARD_PROTOCOL_T1:
      pioSendPci = SCARD_PCI_T1;
      break;

    default:
      pioSendPci = NULL;
  }

  /*
   * Prepare the APDU
   * ----------------
   */
  pbSendBuffer[0] = 0xFF;       /* CLA for the embedded APDU interpreter */
  pbSendBuffer[1] = 0xF0;       /* INS for the READER CONTROL command */
  pbSendBuffer[2] = 0x00;       /* P1 */
  pbSendBuffer[3] = 0x00;       /* P2 */
  pbSendBuffer[4] = 0x02;       /* Lc */
  pbSendBuffer[5] = 0x20;       /* USB STRING */
  pbSendBuffer[6] = bParam;     /* Parameter */

  /*
   * Do the APDU exchange
   * --------------------
   */
  BENCH_T0;
  rc = SCardTransmit(hCard,
                     pioSendPci,
                     pbSendBuffer,
                     sizeof(pbSendBuffer),
                     NULL, pbRecvBuffer, &pcbRecvLength);
  BENCH_T1;
  if (rc != SCARD_S_SUCCESS)
  {
    printf("\tSCardTransmit error %lXh (%ld)\n", rc, rc);
    return (WORD) - 1;
  }
  BENCH_PRINT();

  /*
   * Get the status word
   * -------------------
   */
  if (pcbRecvLength < 2)
  {
    printf("SCardTransmit : response too small (%ld)\n", pcbRecvLength);
    return (WORD) - 1;
  }

  sw = pbRecvBuffer[pcbRecvLength - 2] << 8;
  sw |= pbRecvBuffer[pcbRecvLength - 1];

  /*
   * Get the data (assume there's enough room in the buffer)
   * -------------------------------------------------------
   */
  memcpy(pbDataOut, pbRecvBuffer, pcbRecvLength - 2);
  *pdwDataOutLen = pcbRecvLength - 2;

  return sw;
}

WORD GetReaderSlotName(SCARDHANDLE hCard, DWORD dwProtocol, BYTE pbDataOut[],
                       DWORD * pdwDataOutLen)
{
  LONG rc;
  WORD sw;
  const SCARD_IO_REQUEST *pioSendPci;
  BYTE pbSendBuffer[6];
  BYTE pbRecvBuffer[256];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  switch (dwProtocol)
  {
    case SCARD_PROTOCOL_T0:
      pioSendPci = SCARD_PCI_T0;
      break;
    case SCARD_PROTOCOL_T1:
      pioSendPci = SCARD_PCI_T1;
      break;

    default:
      pioSendPci = NULL;
  }

  /*
   * Prepare the APDU
   * ----------------
   */
  pbSendBuffer[0] = 0xFF;       /* CLA for the embedded APDU interpreter */
  pbSendBuffer[1] = 0xF0;       /* INS for the READER CONTROL command */
  pbSendBuffer[2] = 0x00;       /* P1 */
  pbSendBuffer[3] = 0x00;       /* P2 */
  pbSendBuffer[4] = 0x01;       /* Lc */
  pbSendBuffer[5] = 0x21;       /* SLOT NAME */

  /*
   * Do the APDU exchange
   * --------------------
   */
  BENCH_T0;
  rc = SCardTransmit(hCard,
                     pioSendPci,
                     pbSendBuffer,
                     sizeof(pbSendBuffer),
                     NULL, pbRecvBuffer, &pcbRecvLength);
  BENCH_T1;
  if (rc != SCARD_S_SUCCESS)
  {
    printf("\tSCardTransmit error %lXh (%ld)\n", rc, rc);
    return (WORD) - 1;
  }
  BENCH_PRINT();

  /*
   * Get the status word
   * -------------------
   */
  if (pcbRecvLength < 2)
  {
    printf("SCardTransmit : response too small (%ld)\n", pcbRecvLength);
    return (WORD) - 1;
  }

  sw = pbRecvBuffer[pcbRecvLength - 2] << 8;
  sw |= pbRecvBuffer[pcbRecvLength - 1];

  /*
   * Get the data (assume there's enough room in the buffer)
   * -------------------------------------------------------
   */
  memcpy(pbDataOut, pbRecvBuffer, pcbRecvLength - 2);
  *pdwDataOutLen = pcbRecvLength - 2;

  return sw;
}

BOOL GetAndShowCardEntry(SCARDHANDLE hCard, DWORD dwProtocol, BYTE bParam1, BYTE bParam2)
{
  BYTE pbData[254];
  DWORD dwLength;

  if (!GetCardEntry(hCard, dwProtocol, bParam1, bParam2, pbData, &dwLength))
    return FALSE;

  if (dwLength)
    ShowCardEntry(bParam1, bParam2, pbData, dwLength);

  return TRUE;
}

void ShowCardEntry(BYTE bParam1, BYTE bParam2, BYTE pbData[], DWORD dwLength)
{
  DWORD i;
  WORD wParam = (bParam1 << 8) | bParam2;
  BOOL is_ascii = FALSE;

  printf("\t");
  switch (wParam)
  {
    case 0x0000:
      printf("Card UID             : ");
      break;
    case 0x0100:
      printf("Historical bytes     : ");
      break;
    case 0xF000:
      printf("Complete Card data   : ");
      break;
    case 0xF100:
      printf("PIX.SS, PIX.NN       : %02X, %02X%02X\n", pbData[0], pbData[1],
             pbData[2]);
      return;
    case 0xF101:
      printf("NFC Forum Tag Type   : %d\n", pbData[0]);
      return;
    case 0xF200:
      printf("Truncated Card UID   : ");
      break;
    case 0xFA00:
      printf("Card ATR             : ");
      break;
    case 0xFC00:
      printf("ISO 14443 DSI,DRI    : %d,%d\n", pbData[0], pbData[1]);
      return;
    case 0xFC01:
      printf("Card->Reader kbit/s  : %d\n", (pbData[0] << 8) | pbData[1]);
      return;
    case 0xFC02:
      printf("Reader->Card kbit/s  : %d\n", (pbData[0] << 8) | pbData[1]);
      return;
    case 0xFC03:
      printf("Active antenna       : %d\n", pbData[0]);
      return;
    case 0xFF00:
      printf("Reader serial number : ");
      break;
    case 0xFF01:
      printf("Micore chipset code  : ");
      break;
    case 0xFF02:
      printf("Micore chipset name  : ");
      is_ascii = TRUE;
      break;
    case 0xFF81:
      printf("Vendor name          : ");
      is_ascii = TRUE;
      break;
    case 0xFF82:
      printf("Product name         : ");
      is_ascii = TRUE;
      break;
    case 0xFF83:
      printf("Product serial number: ");
      is_ascii = TRUE;
      break;
    case 0xFF84:
      printf("USB VID/PID          : ");
      is_ascii = TRUE;
      break;
    case 0xFF85:
      printf("Product version      : ");
      is_ascii = TRUE;
      break;

    default:
      printf("Unknown entry P1=%02X,P2=%02X : ", bParam1, bParam2);
  }

  if (is_ascii)
  {
    for (i = 0; i < dwLength; i++)
      printf("%c", pbData[i]);
  } else
  {
    for (i = 0; i < dwLength; i++)
      printf("%02X ", pbData[i]);
  }

  printf("\n");
}

void ShowReaderEntry(BYTE bParam, BYTE pbData[], DWORD dwLength)
{
  DWORD i;

  printf("\t");
  switch (bParam)
  {
    case 0x01:
      printf("Vendor               : ");
      break;
    case 0x02:
      printf("Product              : ");
      break;
    case 0x03:
      printf("Serial number        : ");
      break;
    case 0x04:
      printf("USB identifier       : ");
      break;
    case 0x05:
      printf("Version              : ");
      break;

    case 0x21:
      printf("Slot name            : ");
      break;

    default:
      printf("Unknown entry %02X : ", bParam);
  }

  for (i = 0; i < dwLength; i++)
    printf("%c", pbData[i]);

  printf("\n");
}



WORD Test_Entry(SCARDHANDLE hCard, DWORD dwProtocol, DWORD dwSendLength,
                BYTE bWantLength, BYTE bWantDelay)
{
  LONG rc;
  WORD sw;
  const SCARD_IO_REQUEST *pioSendPci;
  BYTE pbSendBuffer[262];
  BYTE pbRecvBuffer[258];
  DWORD pcbRecvLength = sizeof(pbRecvBuffer);

  DWORD i;

  printf("\tTesting: sending %ldB, asking %dB with %ds of delay... ",
         dwSendLength, bWantLength, bWantDelay);

  switch (dwProtocol)
  {
    case SCARD_PROTOCOL_T0:
      pioSendPci = SCARD_PCI_T0;
      break;
    case SCARD_PROTOCOL_T1:
      pioSendPci = SCARD_PCI_T1;
      break;

    default:
      pioSendPci = NULL;
  }

  /*
   * Prepare the APDU
   * ----------------
   */
  pbSendBuffer[0] = 0xFF;       /* CLA for the embedded APDU interpreter */
  pbSendBuffer[1] = 0xFD;       /* INS for the TEST command */
  pbSendBuffer[2] = bWantLength;  /* P1 */
  pbSendBuffer[3] = bWantDelay; /* P2 */
  if (dwSendLength == 0)
  {
    if (bWantLength == 0)
    {
      /* Case 1S */
      dwSendLength = 4;
    } else
    {
      /* Case 3S */
      pbSendBuffer[4] = 0x00;   /* Le : accept extact actual length */
      dwSendLength = 5;
    }
  } else
  {
    if (dwSendLength < 256)
    {
      pbSendBuffer[4] = (BYTE) dwSendLength;
    } else
    {
      pbSendBuffer[4] = 0x00;
      dwSendLength = 256;
    }
    dwSendLength += 5;

    //pbSendBuffer[dwSendLength++] = 0x00;
  }

  printf("\n");
  for (i = 0; i < dwSendLength; i++)
    printf("%02X ", pbSendBuffer[i]);
  printf("\n");

  /*
   * Do the APDU exchange
   * --------------------
   */
  BENCH_T0;
  rc = SCardTransmit(hCard,
                     pioSendPci,
                     pbSendBuffer,
                     dwSendLength, NULL, pbRecvBuffer, &pcbRecvLength);
  BENCH_T1;
  if (rc != SCARD_S_SUCCESS)
  {
    printf("\n\tSCardTransmit error %lXh (%ld)\n", rc, rc);
    return (WORD) - 1;
  }
  BENCH_PRINT();

  /*
   * Get the status word
   * -------------------
   */
  if (pcbRecvLength < 2)
  {
    printf("\n\tSCardTransmit : response too small (%ld)\n", pcbRecvLength);
    return (WORD) - 1;
  }

  sw = pbRecvBuffer[pcbRecvLength - 2] << 8;
  sw |= pbRecvBuffer[pcbRecvLength - 1];

  /*
   * Get the data (assume there's enough room in the buffer)
   * -------------------------------------------------------
   */
  if ((pcbRecvLength - 2) == bWantLength)
    printf("OK, SW=%04X\n", sw);
  else
    printf("\n\tERROR, got %ldB instead of %dB! SW=%04X\n", pcbRecvLength - 2,
           bWantLength, sw);
  return sw;
}
