#ifndef __PCSCMON_H__
#define __PCSCMON_H__

#include <winscard.h>

#ifndef TRUE
  #define TRUE  1
#endif
#ifndef FALSE
  #define FALSE 0
#endif
#ifndef MAXIMUM_SMARTCARD_READERS
  #define MAXIMUM_SMARTCARD_READERS 64
#endif


void ShowCardData(const char *szReader);
void ShowReaderData(const char *szReader);
void TestReader(const char *szReader);
void ATR_analysis(BYTE atr[], DWORD atrlen);

#endif
