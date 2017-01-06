
/**h* PCSCMon/atr_analysis.c
 *
 * NAME
 *   atr_analysis.c :: Explain an ISO 7816-3 ATR (C implementation)
 *
 * DESCRIPTION
 *   The ATR_analysis function decodes a smartcard ATR, an explain its
 *   fields.
 *   The ATR_identify function finds and display information from the
 *   smartcard_list.txt file.
 *
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS 2008
 *   Thanks Ludovic Rousseau, Christophe Levantis for the skeleton of the ATR_analysis
 *   function (the function is a port of their PERL script to C)
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

/*
 * For more information about the ATR see ISO 7816-3 1997, pages 12 and up
 * 
 * TS	initial character
 * T0	format character, Y1 | K
 * 	interfaces characters
 * TA(1)	global, codes  FI and DI
 * TB(1)	global, codes II and PI
 * TC(1)	global, codes N
 * TD(1)	codes Y(2) and T
 * TA(2)	global, codes specific mode
 * TB(2)	global, codes PI2
 * TC(2)	specific
 * TD(2)	codes Y(3) and T
 * TA(3)	TA(i), TB(i), TC(i) (i>2)
 * 		- specific after T!=15
 * 		- global after T=15
 * TD(i)	codes Y(i+1) and T
 * T1	historical bytes
 * 		max 15 characters
 * TK
 * TCK	check character
 */
static const char BINQ[16][5] = {
  "0000", "0001", "0010", "0011", "0100", "0101", "0110", "0111",
  "1000", "1001", "1010", "1011", "1100", "1101", "1110", "1111"
};

static const char Fi[16][5] = {
  "372", "372", "558", "744", "1116", "1488", "1860", "RFU",
  "RFU", "512", "768", "1024", "1536", "2048", "RFU", "RFU"
};

static const char Di[16][5] = {
  "RFU", "1", "2", "4", "8", "16", "32", "RFU",
  "12", "20", "RFU", "RFU", "RFU", "RFU", "RFU", "RFU"
};

void ATR_analysis_TA(BYTE counter, BYTE T, DWORD * offset, BYTE atr[],
                     DWORD atrlen);
void ATR_analysis_TB(BYTE counter, BYTE T, DWORD * offset, BYTE atr[],
                     DWORD atrlen);
void ATR_analysis_TC(BYTE counter, BYTE T, DWORD * offset, BYTE atr[],
                     DWORD atrlen);
void ATR_analysis_TD(BYTE counter, DWORD * offset, BYTE atr[], DWORD atrlen);

/*     _   _____  _____
 *    / \ |_   _||  _  \
 *   / _ \  | |  | |_| |
 *  / ___ \ | |  |  __ \
 * /_/   \_\|_|  |_|  \_\
 */
void ATR_analysis(BYTE atr[], DWORD atrlen)
{
  DWORD offset = 0;

  BYTE Y1;
  BYTE K;


  /*
   * Explain ATR content
   * -------------------
   */

  /* TS */
  /* -- */
  printf("\t\t+ TS  = %02X --> ", atr[0]);
  switch (atr[0])
  {
    case 0x3B:
      printf("Direct Convention");
      break;
    case 0x3F:
      printf("Inverse Convention");
      break;
    default:
      printf("UNDEFINED");
      break;
  }
  printf("\n");

  /* T0 */
  /* -- */
  Y1 = atr[1] / 0x10;
  K = atr[1] % 0x10;
  printf("\t\t+ T0  = %02X, Y1=%s, K=%d (historical bytes)\n", atr[1],
         BINQ[Y1], K);

  /* TA, TB, TC, TD */
  /* -------------- */

  offset = 2;

  if (Y1 & 0x01)
    ATR_analysis_TA(1, 0, &offset, atr, atrlen);

  if (Y1 & 0x02)
    ATR_analysis_TB(1, 0, &offset, atr, atrlen);

  if (Y1 & 0x04)
    ATR_analysis_TC(1, 0, &offset, atr, atrlen);

  if (Y1 & 0x08)
    ATR_analysis_TD(1, &offset, atr, atrlen);

  /* Historical bytes */
  /* ---------------- */
  if (K)
  {
    DWORD i;

    printf("\t\t+ %d historical byte%s:\n", K, K > 1 ? "s" : "");
    printf("\t\t       ");

    for (i = 0; i < K; i++)
    {
      if (offset >= atrlen)
      {
        printf("\n\t\tERROR! ATR is truncated; %ld byte(s) missing\n", K - i);
        break;
      }
      printf(" %02X", atr[offset++]);
    }
    printf("\n");
  }

  /* Check TCK */
  /* --------- */
  if (K)
  {
    DWORD i;
    BYTE TCK = 0;

    for (i = 1; i < (atrlen - 1); i++)
    {
      TCK ^= atr[i];
    }

    printf("\t\t+ TCK = %02X ", TCK);

    if (TCK == atr[atrlen - 1])
    {
      printf("(correct checksum)");
    } else
    {
      printf("!= %02X", atr[atrlen - 1]);
      printf("\n\t\tERROR! Checksum is invalid\n");
    }
    printf("\n");
  }

  printf("\n");
}



/*  _____  _    
 * |_   _|/ \   
 *   | | / _ \  
 *   | |/ ___ \ 
 *   |_/_/   \_\
 */
void ATR_analysis_TA(BYTE counter, BYTE proto, DWORD * offset, BYTE atr[],
                     DWORD atrlen)
{
  BYTE value;

  value = atr[*offset];
  *offset = *offset + 1;

  printf("\t\t+ TA%d = %02X --> ", counter, value);

  if (counter == 1)
  {
    /* TA1 */
    BYTE F = value / 0x10;
    BYTE D = value % 0x10;

    printf("Fi=%s, Di=%s", Fi[F], Di[D]);

  } else if (counter == 2)
  {
    /* TA2 */
    BYTE F = value / 0x10;
    BYTE D = value % 0x10;

    printf("Protocol to be used in spec mode: T=%d", D);
    if (F & 0x08)
    {
      printf(" - unable to change");
    } else
    {
      printf(" - capable to change");
    }
    if (F & 0x01)
    {
      printf(" - implicity defined");
    } else
    {
      printf(" - defined by interface bytes");
    }
  } else if (counter >= 3)
  {
    /* TA3 (and other) */
    if (proto == 1)
    {
      /* T=1 protocol */
      printf("IFSC: %d", value);
    } else
    {
      /* Other protocol than T=1 */
      BYTE F = value / 0x40;
      BYTE D = value % 0x40;

      printf("Class: ");

      if (D & 0x01)
        printf("A 5V ");
      if (D & 0x02)
        printf("B 3V ");
      if (D & 0x04)
        printf("C 1.8V ");
      if (D & 0x08)
        printf("D RFU ");
      if (D & 0x10)
        printf("E RFU ");

      printf("Clock stop: ");
      switch (F)
      {
        case 0:
          printf("not supported");
          break;
        case 1:
          printf("state L");
          break;
        case 2:
          printf("state H");
          break;
        case 3:
          printf("no preference");
      }
    }
  }

  printf("\n");
}

/*  _____ ____  
 * |_   _| __ ) 
 *   | | |  _ \ 
 *   | | | |_) |
 *   |_| |____/ 
 */
void ATR_analysis_TB(BYTE counter, BYTE T, DWORD * offset, BYTE atr[],
                     DWORD atrlen)
{
  BYTE value;

  value = atr[*offset];
  *offset = *offset + 1;

  printf("\t\t+ TB%d = %02X --> ", counter, value);


  if (counter == 1)
  {
    /* TB1 */
    BYTE I = value / 0x20;
    BYTE PI = value % 0x20;

    if (PI == 0)
    {
      printf("Vpp not connected");
    } else
    {
      printf("Programming params P:%dV, I:%dmA", PI, I);
    }
  } else if (counter == 2)
  {
    /* TB2 */
    printf("Programming param PI2 (PI1 should be ignored): ");
    if ((value > 49) || (value < 251))
    {
      printf("%d(dV)", value);
    } else
    {
      printf("RFU value %d", value);
    }
  } else if (counter >= 3)
  {
    if (T == 1)
    {
      BYTE BWI = value / 0x10;
      BYTE CWI = value % 0x10;

      printf("BWI=%d - CWI=%d", BWI, CWI);
    }
  }



  printf("\n");
}

/*
 *  _____ ____ 
 * |_   _/ ___|
 *   | || |    
 *   | || |___ 
 *   |_| \____|
 */
void ATR_analysis_TC(BYTE counter, BYTE T, DWORD * offset, BYTE atr[],
                     DWORD atrlen)
{
  BYTE value;

  value = atr[*offset];
  *offset = *offset + 1;

  printf("\t\t+ TC%d = %02X --> ", counter, value);

  if (counter == 1)
  {
    /* TC1 */
    printf("EGT=%d", value);
    if (value == 0xFF)
      printf(" (special value)");
  } else if (counter == 2)
  {
    /* TC2 */
    printf("Work waiting time= 960 x %d x (Fi/F)", value);
  } else if (counter == 3)
  {
    /* TC3 */
    if (T == 1)
    {
      printf("Error dectection code: ");
      if (value == 1)
      {
        printf("CRC");
      } else if (value == 0)
      {
        printf("LRC");
      } else
      {
        printf("RFU");
      }
    }
  }

  printf("\n");
}

/*  _____ ____  
 * |_   _|  _ \ 
 *   | | | | | |
 *   | | | |_| |
 *   |_| |____/ 
 */
void ATR_analysis_TD(BYTE counter, DWORD * offset, BYTE atr[], DWORD atrlen)
{
  BYTE value, Y, T;

  value = atr[*offset];
  *offset = *offset + 1;

  Y = value / 0x10;
  T = value % 0x10;

  if (T == 15)
  {
    printf("\t\tGlobal interface bytes following\n");
  }

  printf("\t\t+ TD%d = %02X --> Y%d=%s, protocol T=%d\n", counter, value,
         counter + 1, BINQ[Y], T);
  printf("\t\t-----------\n");

  counter++;

  if (Y & 0x01)
    ATR_analysis_TA(counter, T, offset, atr, atrlen);

  if (Y & 0x02)
    ATR_analysis_TB(counter, T, offset, atr, atrlen);

  if (Y & 0x04)
    ATR_analysis_TC(counter, T, offset, atr, atrlen);

  if (Y & 0x08)
    ATR_analysis_TD(counter, offset, atr, atrlen);

}
