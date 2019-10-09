#include "LowLevelAPI.h"

#include "calypso_audit.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>

#define kAidMaxLength           16
#define kBufferLength					  128
#define kVersionEnvApplication  1
#define kBRUSSEL_NETWK_ID				0x056002  // as defined in 090602-LT-AuditApplication


unsigned char gAID___[] = { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41 };
unsigned char gAID_LT[] = { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41, 0x4C, 0x54 };
unsigned char gAID_LS[] = { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41, 0x4C, 0x53 };

unsigned char gCouplerVersion[16];

unsigned char gAID[kAidMaxLength];
unsigned char gAIDLength;
unsigned char gSerialNumber[8];
unsigned char gInfos[7];
unsigned char gRatified;

unsigned char gEnvironmentRecord[kBufferLength];
unsigned char gEnvironmentRecordLength;

void debugPrintByteArray(char *inName,  // Name of the array
                         unsigned char *inArray,  // Array to display
                         int inArrayLength  // Length of the array
  )
{
  int vi;

  printf("%s: (%d) ", inName, inArrayLength);
  for (vi = 0; vi < inArrayLength; vi++)
  {
    printf("%02X", inArray[vi]);
  }

  printf("\n");
}

void debugPrintf(const char *inFormatMessage, ...)
{
  va_list vListArg;

  va_start(vListArg, inFormatMessage);
  vprintf(inFormatMessage, vListArg);
}

BOOL CardPerso(unsigned char EnvHolderId)
{
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;

  vPoResult = poSessionOpen(1, 0, 0, &gRatified, NULL, NULL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    return FALSE;

  gEnvironmentRecordLength = 0;
  gEnvironmentRecord[gEnvironmentRecordLength++] = kVersionEnvApplication;
  gEnvironmentRecord[gEnvironmentRecordLength++] = (kBRUSSEL_NETWK_ID >> 16) & 0xFF;
  gEnvironmentRecord[gEnvironmentRecordLength++] = (kBRUSSEL_NETWK_ID >> 8) & 0xFF;
  gEnvironmentRecord[gEnvironmentRecordLength++] =  kBRUSSEL_NETWK_ID & 0xFF;
  gEnvironmentRecord[gEnvironmentRecordLength++] = 0;
  gEnvironmentRecord[gEnvironmentRecordLength++] = 0;
  gEnvironmentRecord[gEnvironmentRecordLength++] = 0;
  gEnvironmentRecord[gEnvironmentRecordLength++] = EnvHolderId;

  vPoResult = poRecordUpdate(1, kSfiEnvironment, gEnvironmentRecord, gEnvironmentRecordLength, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    return FALSE;

  poSessionClose(kCLOSE_IMMEDIATE_RATIFICATION, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    return FALSE;

  return TRUE;
}



// -------------------------------------------------------------------------
//              MAIN LOOP
// -------------------------------------------------------------------------

int main(int argc, char **argv)
{
  unsigned char vSW[2];         // Status Word returned by the card
  int vPoResult;                // Card Reader function result
  int value = 0;
  int i;
  BOOL f = FALSE;

  CalypsoAuditStartup(argc, argv);

  for (i=1; i<argc; i++)
  {
    if (!stricmp(argv[i], "-t"))
    {
      i++; if (i>= argc) break;
      value = atoi(argv[i]);
      f = TRUE;
    }
  }

  if (!f)
  {
    printf("Test value must be specified with -t\n");
    return EXIT_FAILURE;
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

    if (CardPerso((unsigned char) value))
    {
      printf("PERSO OK\n");
      poRelease();
      break;
    }

    printf("PERSO FAILED\n");
    poRelease();
  }
}
