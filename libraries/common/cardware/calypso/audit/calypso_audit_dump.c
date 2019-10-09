#include "LowLevelAPI.h"

#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>

#define kAidMaxLength           16
#define kBufferLength					  29

unsigned char gAID___[] = { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41 };
unsigned char gAID_LT[] = { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41, 0x4C, 0x54 };
unsigned char gAID_LS[] = { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41, 0x4C, 0x53 };

unsigned char gCouplerVersion[16];

unsigned char gAID[kAidMaxLength];
unsigned char gAIDLength;
unsigned char gSerialNumber[8];
unsigned char gInfos[7];
unsigned char gRatified;

unsigned char gRecord[kBufferLength];
unsigned char gRecordLength;

unsigned char gSW[2];

void debugPrintByteArray(char *inName,  // Name of the array
                         unsigned char *inArray,  // Array to display
                         int inArrayLength  // Length of the array
  )
{
  int vi;

  printf("%s: (%d) ", inName, inArrayLength);
  for (vi = 0; vi < inArrayLength; vi++)
  {
    printf("%02X ", inArray[vi]);
  }

  printf("\n");
}

void debugPrintf(const char *inFormatMessage, ...)
{
  va_list vListArg;

  va_start(vListArg, inFormatMessage);
  vprintf(inFormatMessage, vListArg);
}

void FileDump(const char *display, unsigned char inSFI)
{
  int rv;
  unsigned char inRecNum;

  for (inRecNum = 1; inRecNum <= 20; inRecNum++)
  {
    gRecordLength = sizeof(gRecord);
    rv = poRecordRead(inRecNum, inSFI, gRecord, &gRecordLength, gSW);

    if ((rv == kSUCCESS) && (gSW[0] == 0x90))
    {
      debugPrintf("SFI=%02X ('%s') record %d\n", inSFI, display, inRecNum);
      debugPrintByteArray("", gRecord, gRecordLength);
    }
  }
}

void CardDump(void)
{
  FileDump("Environment", kSfiEnvironment);
  FileDump("ID", kSfiID);
  FileDump("ContractList", kSfiContractList);
  FileDump("Contract", kSfiContract);
  FileDump("Counters", kSfiCounters);
  FileDump("Events", kSfiEvents);
  FileDump("SpecialEvent", kSfiSpecialEvent);
  FileDump("Free", kSfiFree);
}

// -------------------------------------------------------------------------
//              MAIN LOOP
// -------------------------------------------------------------------------

int main(int argc, char **argv)
{
  unsigned char vSW[2];         // Status Word returned by the card
  int vPoResult;                // Card Reader function result
  int value = 0;

  if (argc > 1)
  {
    value = atoi(argv[1]);
  }

  vPoResult = poGetVersion(gCouplerVersion);

  debugPrintf("#  Reader Version: ");
  if (vPoResult != kSUCCESS)
  {
    debugPrintf("Not found.\n");
    gCouplerVersion[0] = 0;
  } else
    debugPrintf("%s\n", gCouplerVersion);

  // main loop of the audit application
  for (;;)
  {
    gAIDLength = 0;             // JDA
    vPoResult = poDetect(gAID_LT, sizeof(gAID_LT), gAID, &gAIDLength, gSerialNumber, gInfos);
    if (vPoResult != kSUCCESS)
      continue;

    if (gAIDLength == 0)
      vPoResult = poApplicationSelect(gAID_LS, sizeof(gAID_LS), gAID, &gAIDLength, gSerialNumber, gInfos, vSW);

    if (gAIDLength == 0)
      vPoResult = poApplicationSelect(gAID___, sizeof(gAID___), gAID, &gAIDLength, gSerialNumber, gInfos, vSW);

    if (vPoResult != kSUCCESS || gAIDLength == 0)
      continue;

    debugPrintByteArray("gAID", gAID, gAIDLength);
    debugPrintByteArray("gSerialNumber", gSerialNumber,
                        sizeof(gSerialNumber));
    debugPrintByteArray("gInfos", gInfos, sizeof(gInfos));

    CardDump();
    poRelease();
  }
}
