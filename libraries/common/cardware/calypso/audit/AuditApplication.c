
/*====================================================================================================
	\file	AuditApplication.c

	\brief	Audit application: example implementation of a terminal application for the Calypso Registration Audit
	
	Please refer to the document 090602-LT-AuditApplication for more explanations.

	This file may be used freely for any Calypso project.
	This file is given as an example, on an "as-is" basis, without any warranty.
	Using this file is under the responsibility of the party using it.

	Copyright (c) 2009, Spirtech. All rights reserved.

HISTORY:
	090904-chy	Created

=====================================================================================================

 */

#include "calypso_audit.h"

// Debug flag
#ifndef kaaDEBUG
#define	kaaDEBUG 1              // set to 1 to print debug informations
#endif

// includes necessary only for debug functions
#if kaaDEBUG
#include <stdarg.h>             // for debug: vprintf in debugPrintf
#include <stdio.h>              // for printf
#endif

// system includes
#include <string.h>             // for memcpy and memset
#include <time.h>               // for date/time computations and for pauses

// low level api of the card reader
#include "LowLevelAPI.h"


long kAntiPassBackDelay = 600;
long kInterchangeDelay  = 3600;

// -------------------------------------------------------------------------
//                CONSTANTS
// -------------------------------------------------------------------------

// Version of this Audit Application
#define kVersion						"1.0.5"

// Audit errors, as defined in 090602-LT-AuditApplication
#define kSTATE_NOT_OPEN					-1
#define kSTATE_OK						0
#define kSTATE_NOK						1
#define kSTATE_ABORT					2
#define kSTATE_ACTION_REQUIRED			3

// Network ID of Brussels for personalization
#define kBRUSSEL_NETWK_ID				0x056002  // as defined in 090602-LT-AuditApplication

// Location ID of the terminal to be added in events, etc.
#define kLocationId						0x4348  // location ID, must not be null

// Event codes
#define kEventSubwayEntry				0x31
#define kEventSubwayEntryInterchange	0x36
#define kEventSubwayContractLoading		0x3D
#define kEventSubwayPersonalization		0x37
#define kEventSubwayInvalidation		0x3F
#define kEventSubwayRehabilitation		0x34
#define kEventSubwayPINverified			0x32
#define kEventSubwayPINchanged			0x33
#define kEventSubwayTest				0x35

// Version of the different data structures
#define kVersionEnvApplication			1
#define kVersionId						1
#define kVersionContract				1
#define kVersionEvent					1
#define kVersionSpecialEvent			1

// Tariff
#define kTARIFF_BOOK_COUNTER			0
#define kTARIFF_SV						1
#define kTARIFF_BOOK_COUNTER_SE			2
#define kTARIFF_SEASON					3
#define kTARIFF_SEASON_AUTH				4

// AID max length
#define kAidMaxLength					16

// buffer length for file records and commands
#define kBufferLength					128

// Event fields offsets (in bytes)
#define kEventVersionNumber				 0
#define kEventDateStamp					 1
#define kEventTimeStamp					 3
#define kEventFirstDateStamp			 5
#define kEventFirstTimeStamp			 7
#define kEventLocationId				 9
#define kEventCode						11


// -------------------------------------------------------------------------
//                GLOBALS
// -------------------------------------------------------------------------

unsigned char gAID___[] = { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41 };


// AID used during the audit (constants)

// "1TIC.ICALT"
unsigned char gAID_LT[] =
  { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41, 0x4C, 0x54 };

// "1TIC.ICALS"
unsigned char gAID_LS[] =
  { 0x31, 0x54, 0x49, 0x43, 0x2E, 0x49, 0x43, 0x41, 0x4C, 0x53 };

// Constant: PIN

unsigned char gPIN[] = { 0x31, 0x32, 0x33, 0x34 };
unsigned char gNewPIN[] = { 0x34, 0x33, 0x32, 0x31 };

// coupler informations
unsigned char gCouplerVersion[16];

// current card informations
unsigned char gAID[kAidMaxLength];
unsigned char gAIDLength;
unsigned char gSerialNumber[8];
unsigned char gInfos[7];
unsigned char gRatified;

// records read in the files of the current card
unsigned char gEnvironmentRecord[kBufferLength];
unsigned char gEnvironmentRecordLength;

unsigned char gEventRecord[kBufferLength];
unsigned char gEventRecordLength;


// -------------------------------------------------------------------------
//                DEBUG FUNCTIONS
// -------------------------------------------------------------------------

#define __TRACE() \
  if (!audit_config.Quiet) \
    { TRACE_S(NULL); TRACE_S("--- Error on line "); TRACE_D(__LINE__-1,0); TRACE_S(" - vPoResult="); TRACE_D(vPoResult, 0); TRACE_S(" SW="); TRACE_HB(vSW[0]); TRACE_HB(vSW[1]); TRACE_S(NULL); TRACE_S(NULL); }

/*
	Print a byte array in hexadecimal, with the array name
 */
void debugPrintByteArray(char *inName,  // Name of the array
                         unsigned char *inArray,  // Array to display
                         int inArrayLength  // Length of the array
  )
{
  if (!audit_config.Quiet)
  {
    int vi;

    printf("    %s (%d)\n      ", inName, inArrayLength);
    for (vi = 0; vi < inArrayLength; vi++)
    {
      printf("%02X", inArray[vi]);
    }

    printf("\n");
  }
}


//------------------------------------------------------

/**
 * Function to display log to the console in debug mode.
 * @param inMessage		Message to display
 */
void debugPrintf(const char *inFormatMessage, ...)
{
  if (!audit_config.Quiet)
  {
    va_list vListArg;

    va_start(vListArg, inFormatMessage);
    vprintf(inFormatMessage, vListArg);
  }
}



//-------------------------------------------------------------------------
//            DATE  TIME  FUNCTIONS
//-------------------------------------------------------------------------


//------------------------------------------------------

/*
	Returns the difference between the current date and the horodate 
	composed with inDateCompact and inTimecompact
 */
long diffSeconds                // number of seconds elapsed since inDateCompact/inTimeCompact
  (unsigned char *inDateCompact,  // date in days    on 2 bytes (msb first), 0 = 1/1/1997
   unsigned char *inTimeCompact // time in seconds on 2 bytes (msb first), 0 = midnight
  )
{
  struct tm vLocalDate;
  long vTimeTmp;
  long vUtcDiff;
  time_t vDateToCompare = 0;
  time_t vCurrentTime = time(NULL);

  // compute the utc / local time difference
  vLocalDate = *localtime(&vCurrentTime);
  vUtcDiff =
    (long) difftime(mktime(&vLocalDate), mktime(gmtime(&vCurrentTime)));

  // date 1st January 1997 (local time)
  vLocalDate.tm_sec = 0;
  vLocalDate.tm_min = 0;
  vLocalDate.tm_hour = 0;
  vLocalDate.tm_mday = 1;
  vLocalDate.tm_mon = 1 - 1;
  vLocalDate.tm_year = 1997 - 1900;

  // Compute the date given in seconds in system utc time
  vDateToCompare = mktime(&vLocalDate) + vUtcDiff;  // 1/1/1997 in UTC time

  vTimeTmp =
    24 * 3600 * ((((long) inDateCompact[0]) << 8) + inDateCompact[1]);
  vDateToCompare += vTimeTmp;

  vTimeTmp = 60 * ((((long) inTimeCompact[0]) << 8) + inTimeCompact[1]);
  vDateToCompare += vTimeTmp;


  debugPrintByteArray("inDateCompact", inDateCompact, 2);
  debugPrintByteArray("inTimeCompact", inTimeCompact, 2);
  debugPrintf("vLocalDate = %ld \n", vDateToCompare);
  debugPrintf("diffTime = %ld \n",
              ((long) difftime(vCurrentTime, vDateToCompare)));

  return (long) difftime(vCurrentTime, vDateToCompare);
}


//------------------------------------------------------



//-------------------------------------------------------------------------
//            PSO COMPUTE
//-------------------------------------------------------------------------


//------------------------------------------------------

/*
	Compute PSO using the SAM.

	On entrance, the data given is the data to sign plus two bytes
	On exit, the data is updated with the SAM traceability information and the two bytes are
			updated with the authenticator.

 */
int PSO_compute                 // return 0 if no error
  (unsigned char *ioContractRecord, // record to sign, plus two bytes (for the authenticator)
   unsigned char inContractLength // length of the record to sign plus 2
  )
{
  unsigned char vCommand[kBufferLength];
  unsigned char vCommandLength = 0;
  unsigned char vResponse[kBufferLength];
  unsigned char vResponseLength = 8 + inContractLength + 2; // card serial number, buffer with authenticator, Status Word

  int vResult = 0;

  // make up the SAM command APDU
  vCommand[vCommandLength++] = 0x94;  // CLA
  vCommand[vCommandLength++] = 0x2A;  // INS
  vCommand[vCommandLength++] = 0x9E;  // P1
  vCommand[vCommandLength++] = 0x9A;  // P2
  vCommand[vCommandLength++] = 0x29;  // Lc

  vCommand[vCommandLength++] = 0xFF;  // SigneKeyNum
  vCommand[vCommandLength++] = 0x2B;  // KIF
  vCommand[vCommandLength++] = 0xA5;  // KVC
  vCommand[vCommandLength++] = 0x62;  // OpMode
  vCommand[vCommandLength++] = 0x00;  // TraceOffset 1
  vCommand[vCommandLength++] = 0x50;  // TraceOffset 2

  memcpy(vCommand + vCommandLength, gSerialNumber, 8);  // Card serial number
  vCommandLength += 8;
  memcpy(vCommand + vCommandLength, ioContractRecord, inContractLength - 2);  // Contrat data without signature
  vCommandLength += inContractLength - 2;

  vResult = poSamSendReceive(vCommand, vCommandLength, vResponse, &vResponseLength);

  // return an error if the length is incorrect or we received an APDU error
  if (vResult == 0)
    if (vResponseLength != 8 + inContractLength + 2 ||
        vResponse[vResponseLength - 2] != 0x90 ||
        vResponse[vResponseLength - 1] != 0x00)
      vResult = -1;

  // Fill out parameters
  if (vResult == 0)
  {
    memcpy(ioContractRecord, vResponse + 8, inContractLength);  // contract modified with authenticator added

    debugPrintByteArray("Contract ", ioContractRecord, inContractLength);
  }

  return vResult;
}



//-------------------------------------------------------------------------
//            EVENT MANAGEMENT
//-------------------------------------------------------------------------


//------------------------------------------------------

/*
	Append an Event in the event file

 */
int EventAppend                 // returns 0 if no error
  (unsigned char inEventCode,   // Event Code
   int inFillFirstTime,         // 1: set FirstTimeStamp and FirstDateStamp with the current date, else copy the one in gEventRecord
   unsigned char outSW[2]       // Status Word received from the card
  )
{
  unsigned char vNewEventDate[4]; // Date time in DateCompact / TimeCOmpact format
  unsigned char vEventRecord[kBufferLength];  // New event to append
  unsigned char vEventRecordLength;

  vEventRecordLength = 0;

  // Version
  vEventRecord[vEventRecordLength++] = kVersionEvent;

  // CurrentTime
  getCurrentTime(vNewEventDate);
  memcpy(vEventRecord + vEventRecordLength, vNewEventDate, 4);
  vEventRecordLength += 4;

  // Set the FirstTime: from current time or from last event read
  if (inFillFirstTime == 1 || inEventCode == kEventSubwayEntry)
  {
    memcpy(vEventRecord + vEventRecordLength, vNewEventDate, 4);
  } else
  {
    memcpy(vEventRecord + vEventRecordLength,
           gEventRecord + vEventRecordLength, 4);
  }
  vEventRecordLength += 4;

  // Set the locationID
  vEventRecord[vEventRecordLength++] = (kLocationId >> 8) & 0xFF;
  vEventRecord[vEventRecordLength++] = kLocationId & 0xFF;

  // Set the Event Code
  vEventRecord[vEventRecordLength++] = inEventCode;
  debugPrintByteArray("vEventRecord", vEventRecord, vEventRecordLength);

  return poRecordAppend(kSfiEvents, vEventRecord, vEventRecordLength, outSW);
}


//------------------------------------------------------

/*
	Update the first record of the Special Events file (SFI=1Dh)
 */
int SpecialEventUpdate          // returns 0 if no error
  (unsigned char outSW[2]       // resulting SW code from the reader
  )
{
  unsigned char vSERecord[kBufferLength];
  unsigned char vSERecordLength = 0;

  // initialize the event record to 0
  memset(vSERecord, 0, kBufferLength);

  // SEventVersionNumber =  1
  vSERecord[vSERecordLength++] = kVersionSpecialEvent;  // 0000 0001

  // SEventDataReaderVersion =  Version of the reader
  memcpy(vSERecord + vSERecordLength, gCouplerVersion, 16);
  vSERecordLength += 16;

  // SEventDataAIDLen = Length of the EnvDataAID significant bytes
  vSERecord[vSERecordLength++] = gAIDLength;

  // SEventDataAID =  AID returned by the Select Application command, 
  memcpy(vSERecord + vSERecordLength, gAID, gAIDLength);
  vSERecordLength += gAIDLength;

  //  the bytes over SEventDataAIDLen are set to 0
  memset(vSERecord + vSERecordLength, 0, kAidMaxLength - gAIDLength);
  vSERecordLength += (kAidMaxLength - gAIDLength);

  // SEventDataCardSerialNumber = Serial number as returned by the Select Application command
  memcpy(vSERecord + vSERecordLength, gSerialNumber, 8);
  vSERecordLength += 8;

  // SEventDataStartupInfo =  Startup information as returned by the Select Application command
  memcpy(vSERecord + vSERecordLength, gInfos, 7);
  vSERecordLength += 7;

  return poRecordUpdate(1, kSfiSpecialEvent, vSERecord, vSERecordLength, outSW);
}

// -------------------------------------------------------------------------
//                PROCESSES
// -------------------------------------------------------------------------

/*
	Validate a card
*/
int processValidation(void)
{
  long vInterchangeTime;        // Delay between the current date/time and the first entrance in the network
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processValidation\n");

  //a.  If the date and time of the first validation (EventFirstDateStamp, EventFirstTimeStamp) is within the interchange period,
  // append an interchange event into the file SFI=08h (Events) setting EventCode to 36h (Subway – Interchange entry).
  vInterchangeTime = diffSeconds(gEventRecord + kEventFirstDateStamp, gEventRecord + kEventFirstTimeStamp);
  debugPrintf("Interchange Time=%d\n", vInterchangeTime);
  if (vInterchangeTime < kInterchangeDelay)
  {
    // interchange, since the last event was less than one hour ago
    vPoResult = EventAppend(kEventSubwayEntryInterchange, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //b.  Else
  else
  {
    //b.1.  Read the record 1 of the file SFI=09h (Contract).
    unsigned char vContractRecord[kBufferLength];
    unsigned char vContractRecordLength = 29;

    vPoResult = poRecordRead(1, kSfiContract, vContractRecord, &vContractRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }

    //b.2.  If the ContractVersionNumber is not equal to kVersionContract, return NOK.
    else if (vContractRecord[0] != kVersionContract)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }

    else
    {
      debugPrintf(">> processValidation(%d)\n", vContractRecord[1]);

      switch (vContractRecord[1]) // ContractTariff
      {
          //b.3.  If the ContractTariff is equal to 0 (book of ticket),
        case 0:
        {
          //Decrement by 1 the counter 1 of the file SFI=19h (Counters).
          vPoResult = poDecrease(1, kSfiCounters, 1, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
            vResult = kSTATE_NOK;
          break;
        }

          //b.4.  Else if the ContractTariff is equal to 1 (contract using SV),
        case 1:
        {
          //Get the SV
          long vStoredValue;    // (the current balance is ignored in the audit application)

          vPoResult = poSVGet(1, &vStoredValue, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
            vResult = kSTATE_NOK;
          // then debit SV by 123
          else
          {
            vPoResult = poSVDebit(-123, vSW);
            if (vPoResult != kSUCCESS ||
                (vSW[0] != 0x90 && vSW[0] != 0x62) || vSW[1] != 0x00)
              vResult = kSTATE_NOK;
          }
          break;
        }

          //b.5.  Else if the ContractTariff is equal to 2 (book of ticket using the counter and registering a special event), 
        case 2:
        {
          //Decrement by 1 the counter 1 of the file SFI=19h (Counters).
          vPoResult = poDecrease(1, kSfiCounters, 1, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
            vResult = kSTATE_NOK;
          else
          {
            // Update the record 1 of file SFI=1Dh (Special events):
            vPoResult = SpecialEventUpdate(vSW);
            if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
              vResult = kSTATE_NOK;
          }
          break;
        }

          //b.6.  Else if the ContractTariff is equal to 3 or 4 (season contract)
        case 3:
        case 4:
          // (nothing to debit) continue to step b.8 to append the event.
          break;
          //b.7.  Else, this contract is unknown return NOK.
        default:
          vResult = kSTATE_NOK;
          break;
      }
    }

    //b.8.  Append an entry event into the file SFI=08h (Events) setting EventCode to 31h (Subway – Entry)
    if (vResult == kSTATE_OK)
    {
      vPoResult = EventAppend(kEventSubwayEntry, 0, vSW);
      if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
        vResult = kSTATE_NOK;
    }
  }

  //c.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Load a contract, according to EnvHolderId (lsb)
*/
int processLoad(void)
{
  unsigned char vContractRecord[kBufferLength]; // Contract to write in the card
  unsigned char vContractRecordLength;
  unsigned char gCountersRecord[kBufferLength]; // Counters to write in the card
  unsigned char gCountersRecordLength = 29;
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processLoad\n");

  //a.    Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
  {
    __TRACE();
    vResult = kSTATE_NOK;
  }

  //b.  Open a session with key #2, reading the record 1 of the file SFI=07h (Environment).
  else
  {
    vPoResult = poSessionOpen(2, 1, kSfiEnvironment, &gRatified, gEnvironmentRecord,
                    &gEnvironmentRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }

  if (vResult == kSTATE_OK)
  {
    memset(vContractRecord, 0, 29);
    switch (gEnvironmentRecord[7])  // lsb of EnvHolderId defines the process
    {
        //c.  If EnvHolderId is equal to 1 (load a book of one ticket),
      case 1:
        //c.1.  Update the record 1 of the file SFI=09h (Contract):
        vContractRecordLength = 0;
        //ContractVersionNumber = 1
        vContractRecord[vContractRecordLength++] = kVersionContract;
        //ContractTariff =  0 (Book of ticket using the counter)
        vContractRecord[vContractRecordLength++] = kTARIFF_BOOK_COUNTER;
        //Other fields to 0.
        // 27 = 4 (ContractSaleSamId) + 3 (ContractSaleSamCounter) + 18 (ContractRfu) + 2 (ContractAuthenticator)
        vContractRecordLength += 27;

        vPoResult = poRecordUpdate(1, kSfiContract, vContractRecord, vContractRecordLength, vSW);
        if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
        {
          __TRACE();
          vResult = kSTATE_NOK;
        }

        //c.2.    Update the counter 1 of the file SFI=19h (Counters):
        // read the current counter 1 value
        if (vResult == kSTATE_OK)
        {
          vPoResult = poRecordRead(1, kSfiCounters, gCountersRecord, &gCountersRecordLength, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
          {
            __TRACE();
            vResult = kSTATE_NOK;
          }
        }
        //CounterValue = 1
        if (vResult == kSTATE_OK)
        {
          gCountersRecord[0] = 0;
          gCountersRecord[1] = 0;
          gCountersRecord[2] = 1;
          vPoResult = poRecordUpdate(1, kSfiCounters, gCountersRecord, gCountersRecordLength, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
          {
            __TRACE();
            vResult = kSTATE_NOK;
          }
        }
        break;

        //d.  If EnvHolderId is equal to 2 (load a contract and the SV),
      case 2:
        //d.1.  Update the record 1 of the file SFI=09h (Contract):
        vContractRecordLength = 0;
        //ContractVersionNumber = 1
        vContractRecord[vContractRecordLength++] = kVersionContract;
        //ContractTariff =  1 (Ticket using SV)
        vContractRecord[vContractRecordLength++] = kTARIFF_SV;
        //Other fields to 0.
        // 27 = 4 (ContractSaleSamId) + 3 (ContractSaleSamCounter) + 18 (ContractRfu) + 2 (ContractAuthenticator)
        vContractRecordLength += 27;

        vPoResult = poRecordUpdate(1, kSfiContract, vContractRecord, vContractRecordLength, vSW);
        if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
        {
          __TRACE();
          vResult = kSTATE_NOK;
        }

        //d.2.  Get the SV then load the SV with 123
        if (vResult == kSTATE_OK)
        {
          long vStoredValue;

          vPoResult = poSVGet(0, &vStoredValue, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
          {
            __TRACE();
            vResult = kSTATE_NOK;
          }
        }
        if (vResult == kSTATE_OK)
        {
          vPoResult = poSVLoad(123, vSW);
          if (vPoResult != 0 || (vSW[0] != 0x90 && vSW[0] != 0x62) || vSW[1] != 0x00)
          {
            __TRACE();
            vResult = kSTATE_NOK;
          }
        }
        break;

        //e.  If EnvHolderId is equal to 3 (load a book of one ticket and a special event),
      case 3:
        //e.1.  Update the record 1 of the file SFI=09h (Contract):
        vContractRecordLength = 0;
        //ContractVersionNumber = 1
        vContractRecord[vContractRecordLength++] = kVersionContract;
        //ContractTariff =  2 (Book of ticket using the counter recording a special event)
        vContractRecord[vContractRecordLength++] = kTARIFF_BOOK_COUNTER_SE;
        //Other fields to 0.
        // 27 = 4 (ContractSaleSamId) + 3 (ContractSaleSamCounter) + 18 (ContractRfu) + 2 (ContractAuthenticator)
        vContractRecordLength += 27;


        vPoResult = poRecordUpdate(1, kSfiContract, vContractRecord, vContractRecordLength, vSW);
        if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
        {
          __TRACE();
          vResult = kSTATE_NOK;
        }

        //e.2.  Update the counter 1 of the file SFI=19h (Counters):
        // read the current counter 1 value
        if (vResult == kSTATE_OK)
        {
          vPoResult = poRecordRead(1, kSfiCounters, gCountersRecord, &gCountersRecordLength, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
          {
            __TRACE();
            vResult = kSTATE_NOK;
          }
        }
        //CounterValue = 1
        if (vResult == kSTATE_OK)
        {
          gCountersRecord[0] = 0;
          gCountersRecord[1] = 0;
          gCountersRecord[2] = 1;
          vPoResult = poRecordUpdate(1, kSfiCounters, gCountersRecord, gCountersRecordLength, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
          {
            __TRACE();
            vResult = kSTATE_NOK;
          }
        }
        //e.3.  Update the record 1 of file SFI=1Dh (Special events):
        if (vResult == kSTATE_OK)
        {
          vPoResult = SpecialEventUpdate(vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
          {
            __TRACE();
            vResult = kSTATE_NOK;
          }
        }
        break;

        //f.  If EnvHolderId is equal to 4 (load a season contract),
      case 4:
        //f.1.  Update the record 1 of the file SFI=09h (Contract):
        vContractRecordLength = 0;
        //ContractVersionNumber = 1
        vContractRecord[vContractRecordLength++] = kVersionContract;
        //ContractTariff =  3 (season contract)
        vContractRecord[vContractRecordLength++] = kTARIFF_SEASON;
        //Other fields to 0.
        // 27 = 4 (ContractSaleSamId) + 3 (ContractSaleSamCounter) + 18 (ContractRfu) + 2 (ContractAuthenticator)
        vContractRecordLength += 27;

        vPoResult = poRecordUpdate(1, kSfiContract, vContractRecord, vContractRecordLength, vSW);
        if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
        {
          __TRACE();
          vResult = kSTATE_NOK;
        }
        break;

        //g.  If EnvHolderId is equal to 5 (load a season contract with authenticator)
      case 5:
        //g.1.  Prepare the record to write into the card.
        //g.1.  And Compute the authenticator using the PSO Compute command (see below).
        vContractRecordLength = 0;
        //ContractVersionNumber = 1
        vContractRecord[vContractRecordLength++] = kVersionContract;
        //ContractTariff =  4 (Book of ticket giving fidelity point)
        vContractRecord[vContractRecordLength++] = kTARIFF_SEASON_AUTH;
        //ContractSaleSamId (will be set by the SAM in PSO_Compute)
        vContractRecordLength += 4;
        //ContractSaleSamCounter (will be set by the SAM in PSO_Compute)
        vContractRecordLength += 3;
        //  ContractRfu = 0
        vContractRecordLength += 18;
        //ContractAuthenticator (will be set by the SAM in PSO_Compute)
        vContractRecordLength += 2;

        vPoResult = PSO_compute(vContractRecord, vContractRecordLength);
        if (vPoResult != kSUCCESS)
        {
          __TRACE();
          vResult = kSTATE_NOK;
        }

        //g.2.  Update the record 1 of the file SFI=09h (Contract):
        if (vResult == kSTATE_OK)
        {
          vPoResult = poRecordUpdate(1, kSfiContract, vContractRecord, vContractRecordLength, vSW);
          if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
          {
            __TRACE();
            vResult = kSTATE_NOK;
          }
        }
        break;

      default:
        //h.  Else,
        //h.1.  Return NOK.
        __TRACE();
        vResult = kSTATE_NOK;
    }
  }
  //i.  Append an event into the file SFI=08h (Events) setting EventCode to 3Dh (Subway – Contract loading)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayContractLoading, 0, vSW);
    if (vPoResult != 0 || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //j.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Personnalization of the card
*/
int processPerso(void)
{
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processPerso\n");

  //a.    Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Open a session with key #1, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(1, 0, 0, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  Update the first record of the file SFI=07h (Environment):
  if (vResult == kSTATE_OK)
  {
    gEnvironmentRecordLength = 0;
    //EnvApplicationVersionNumber = 1
    gEnvironmentRecord[gEnvironmentRecordLength++] = kVersionEnvApplication;
    //EnvNetworkId =  Brussels area
    gEnvironmentRecord[gEnvironmentRecordLength++] =
      (kBRUSSEL_NETWK_ID >> 16) & 0xFF;
    gEnvironmentRecord[gEnvironmentRecordLength++] =
      (kBRUSSEL_NETWK_ID >> 8) & 0xFF;
    gEnvironmentRecord[gEnvironmentRecordLength++] = kBRUSSEL_NETWK_ID & 0xFF;
    //EnvHolderId = 0
    gEnvironmentRecord[gEnvironmentRecordLength++] = 0;

    vPoResult =
      poRecordUpdate(1, kSfiEnvironment, gEnvironmentRecord,
                     gEnvironmentRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //d.  Append an event into the file SFI=08h (Events) setting EventCode to 37h (Subway – Personalization)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayPersonalization, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //e.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Invalidation
*/
int processInvalidation(void)
{
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processInvalidation\n");

  //a.  Append an event into the file SFI=08h (Events) setting EventCode to 3Fh (Subway – Invalidation).
  vPoResult = EventAppend(kEventSubwayInvalidation, 0, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Invalidate the selected DF.
  if (vResult == kSTATE_OK)
  {
    vPoResult = poInvalidate(vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Rehabilitation
*/
int processRehabilitation(void)
{
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processRehabilitation\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Open a session with key #1, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(1, 0, 0, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  Rehabilitate the current DF.
  if (vResult == kSTATE_OK)
  {
    vPoResult = poRehabilitate(vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //d.  Append an event into the file SFI=08h (Events) setting EventCode to 34h (Subway – Rehabilitation)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayRehabilitation, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //e.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Verify PIN plain (in session)
*/
int processVerifyPinPlain(void)
{
  unsigned char vRecord[kBufferLength];
  unsigned char vRecordLength = 29;
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processVerifyPinPlain\n");

  //a.  Verify the PIN is correct in plain mode
  vPoResult = poVerifyPin(0, gPIN, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
  {
    __TRACE();
    vResult = kSTATE_NOK;
  }

  //b.  Read the file SFI=03h (ID)
  if (vResult == kSTATE_OK)
  {
    vPoResult = poRecordRead(1, kSfiID, vRecord, &vRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }

  //c.  If the IdVersionNumber field is different from 1, return NOK.
  if (vResult == kSTATE_OK && vRecord[0] != kVersionId)
  {
    __TRACE();
    vResult = kSTATE_NOK;
  }

  //d.  Append an event into the file SFI=07h (Event) setting EventCode to 32h (Subway – PIN verified)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayPINverified, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //e.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Verify PIN encrypted (in session)
*/
int processVerifyPinEncrypted(void)
{
  unsigned char vRecord[kBufferLength];
  unsigned char vRecordLength = 29;
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processVerifyPinEncrypted\n");

  //a.  Verify the PIN is correct in ciphered mode
  vPoResult = poVerifyPin(1, gPIN, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Read the file SFI=03h (ID)
  if (vResult == kSTATE_OK)
  {
    vPoResult = poRecordRead(1, kSfiID, vRecord, &vRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  If the IdVersionNumber field is different from 1 return NOK.
  if (vResult == kSTATE_OK && vRecord[0] != kVersionId)
    vResult = kSTATE_NOK;

  //d.  Append an event into the file SFI=07h (Event) setting EventCode to 32h (Subway – PIN verified)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayPINverified, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //e.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Change PIN Plain
*/
int processChangePinPlain(void)
{
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processChangePinPlain\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
  {
    __TRACE();
    vResult = kSTATE_NOK;
  }

  //b.  Verify the PIN "1234" (0x31323334) is correct in ciphered mode.
  if (vResult == kSTATE_OK)
  {
    vPoResult = poVerifyPin(1, gPIN, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //c.  Change the PIN in plain mode to the value "4321" (0x34333231).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poChangePin(0, gNewPIN, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //d.  Open a session with key #3, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    unsigned char vRatified;

    vPoResult = poSessionOpen(3, 0, 0, &vRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //e.  Append an event into the file SFI=08h (Events) setting EventCode to 33h (Subway – PIN changed)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayPINchanged, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }

  //f.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Change PIn Encrypted
*/
int processChangePinEncrypted(void)
{
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processChangePinEncrypted\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  // JDA : TODO TODO TODO TODO
  {
    unsigned char vRatified;

    poSessionOpen(1, 0, 0, &vRatified, NULL, NULL, vSW);
    poSessionClose(kCLOSE_CANCEL, vSW);
  }

  //b.  Change the PIN in ciphered mode to the value "4321" (0x34333231).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poChangePin(1, gNewPIN, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  Open a session with key #3, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    unsigned char vRatified;

    vPoResult = poSessionOpen(3, 0, 0, &vRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //d.  Append an event into the file SFI=07h (Event) setting EventCode to 33h (Subway – PIN changed)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayPINchanged, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //e.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Close without ratification
*/
int processRatification(void)
{
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_ACTION_REQUIRED;

  debugPrintf(">> processRatification\n");

  //a.    Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Open a session with key #3, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_ACTION_REQUIRED)
  {
    vPoResult = poSessionOpen(3, 0, 0x00, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  Append an event
  if (vResult == kSTATE_ACTION_REQUIRED)
  {
    vPoResult = EventAppend(kEventSubwayTest, 1, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //d.  Return ACTION_REQUIRED.
  return vResult;
}


//------------------------------------------------------

/*
	Maximum session bufer size
*/
int processSessionBufferSize(void)
{
  unsigned char vRecord[kBufferLength]; // Record to write
  unsigned char vRecordLength;
  unsigned char vRecordIndex; // JDA (was: int)
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processSessionBufferSize\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
  {
    __TRACE();
    vResult = kSTATE_NOK;
  }

  //b.  Open a session with key #1, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(1, 0, 0x00, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }

  //c.  Do a card modification of the maximum bytes (215 bytes for Session Modification value 6):
  if (vResult == kSTATE_OK)
  {
    //140 session bytes - Update the entire contracts ( 4 x 29 bytes)  into the
    //  file SFI=09h (Contracts):
    vRecordLength = 0;
    //  ContractVersionNumber = 1
    vRecord[vRecordLength++] = kVersionContract;
    // ContractTariff =   2 (Book of ticket using the counter recording a special event)
    vRecord[vRecordLength++] = kTARIFF_BOOK_COUNTER_SE;
    // Other fields to 0.
    // 27 = 4 (ContractSaleSamId) + 3 (ContractSaleSamCounter) + 18 (ContractRfu) + 2 (ContractAuthenticator)
    memset(vRecord + vRecordLength, 0, 27);
    vRecordLength += 27;

    for (vRecordIndex = 1; vRecordIndex <= 4 && vResult == kSTATE_OK; vRecordIndex++)
    {
      vPoResult = poRecordUpdate(vRecordIndex, kSfiContract, vRecord, vRecordLength, vSW);
      if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      {
        __TRACE();
        vResult = kSTATE_NOK;
      }
    }
  }
  //21 session bytes -  Update the 15 first bytes of the file SFI=19h (Counters), the 4 first
  //  counters set to 1 and the last one to 0. 
  if (vResult == kSTATE_OK)
  {
    memset(vRecord, 0, 15);     // set all counters to 0
    vRecord[2] = 1;             // counter 1 lsb
    vRecord[5] = 1;             // counter 2 lsb
    vRecord[8] = 1;             // counter 3 lsb
    vRecord[11] = 1;            // counter 4 lsb
    vRecordLength = 15;

    vPoResult = poRecordUpdate(1, kSfiCounters, vRecord, vRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }

  //54 session bytes -  Append 3 times the same event (3x12 bytes) into the file SFI=07h
  //  (Event) setting EventCode to 35h (Subway – Test)
  for (vRecordIndex = 0; vRecordIndex < 3 && vResult == kSTATE_OK; vRecordIndex++)
  {
    vPoResult = EventAppend(kEventSubwayTest, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }

  //d.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Session buffer overflow
*/
int processSessionBufferOverflow(void)
{
  unsigned char vRecord[kBufferLength]; // Record to write
  unsigned char vRecordLength;
  unsigned char vRecordIndex; // JDA (was: int)
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;


  debugPrintf(">> processSessionBufferOverflow\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
  {
    __TRACE();
    vResult = kSTATE_NOK;
  }

  //b.  Open a session with key #1, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(1, 0, 0, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //c.  Do a card modification of the maximum bytes (215 bytes for Session Modification value 6):
  if (vResult == kSTATE_OK)
  {
    //140 session bytes - Update the entire contracts ( 4 x 29 bytes)  into the
    //  file SFI=09h (Contracts):
    vRecordLength = 0;
    //  ContractVersionNumber = 1
    vRecord[vRecordLength++] = kVersionContract;
    //  ContractTariff =    2 (Book of ticket using the counter recording a special event)
    vRecord[vRecordLength++] = kTARIFF_BOOK_COUNTER_SE;
    //  Other fields to 0
    // 27 = 4 (ContractSaleSamId) + 3 (ContractSaleSamCounter) + 18 (ContractRfu) + 2 (ContractAuthenticator)
    memset(vRecord + vRecordLength, 0, 27);
    vRecordLength += 27;

    for (vRecordIndex = 1; vRecordIndex <= 4 && vResult == kSTATE_OK; vRecordIndex++)
    {
      vPoResult = poRecordUpdate(vRecordIndex, kSfiContract, vRecord, vRecordLength, vSW);
      if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      {
        __TRACE();
        vResult = kSTATE_NOK;
      }
    }
  }
  //21 session bytes -  Update the 15 first bytes of the file SFI=19h (Counters), the 4 first
  //  counters set to 1 and the last one to 0. 
  if (vResult == kSTATE_OK)
  {
    memset(vRecord, 0, 15);     // set all counters to 0
    vRecord[2] = 1;             // counter 1 lsb
    vRecord[5] = 1;             // counter 2 lsb
    vRecord[8] = 1;             // counter 3 lsb
    vRecord[11] = 1;            // counter 4 lsb
    vRecordLength = 15;

    vPoResult = poRecordUpdate(1, kSfiCounters, vRecord, vRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //36 session bytes -  Append 2 times the same event (2x12 bytes) into the file SFI=07h
  //  (Event) setting EventCode to 35h (Subway – Test)
  for (vRecordIndex = 0; vRecordIndex < 2 && vResult == kSTATE_OK; vRecordIndex++)
  {
    vPoResult = EventAppend(kEventSubwayTest, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }

  //19 session bytes -  Append 1 event with one more byte set to EEh (13 bytes) into the file 
  //  SFI=07h (Event) setting EventCode to 35h (Subway – Test)
  if (vResult == kSTATE_OK)
  {
    unsigned char vNewEventDate[4];

    getCurrentTime(vNewEventDate);

    vRecordLength = 0;
    // Version number
    vRecord[vRecordLength++] = 1;
    // Set Date Stamp and Time Stamp
    memcpy(vRecord + vRecordLength, vNewEventDate, 4);
    vRecordLength += 4;
    // Set First Date Stamp and First Time Stamp
    memcpy(vRecord + vRecordLength, gEventRecord + vRecordLength, 4);
    vRecordLength += 4;
    // Set the location ID
    vRecord[vRecordLength++] = (kLocationId >> 8) & 0xFF;
    vRecord[vRecordLength++] = kLocationId & 0xFF;
    // Set the Event Code
    vRecord[vRecordLength++] = kEventSubwayTest;
    vRecord[vRecordLength++] = 0xEE;
    debugPrintByteArray("vEvent with an extra byte", vRecord, vRecordLength);

    vPoResult = poRecordAppend(kSfiEvents, vRecord, vRecordLength, vSW);

    //d.  If Low Level API does not return an error due to the overflow, return NOK
    if (vPoResult == kSUCCESS && vSW[0] == 0x90 && vSW[1] == 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //e.  Close the session discarding the possible modifications.
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //f.  Open a session with key #3, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(3, 0, 0, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //g.  Append an event into the file SFI=08h (Events) setting EventCode to 35h (Subway – Test)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayTest, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //h.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Session not limited in number of command
*/
int processSessionCmdNum(void)
{
  unsigned char vRecordToUpdate[] =
    { 0xA5, 0xA5, 0xA5, 0xA5, 0xA5, 0xA5, 0xA5, 0xA5, 0xA5, 0xA5 };
  unsigned char vRecordIndex; // JDA (was: int)
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processSessionCmdNum\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
  {
    __TRACE();
    vResult = kSTATE_NOK;
  }

  //b.  Open a session with key #1, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(1, 0, 0x00, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }

  //c.  Do more than 6 modification commands:
  //Update the 10 first records of 10 bytes of the file SFI=1Eh (ContractList) to A5h.
  if (vResult == kSTATE_OK)
  {
    for (vRecordIndex = 1; vRecordIndex <= 10; vRecordIndex++)
    {
      vPoResult = poRecordUpdate(vRecordIndex, kSfiContractList, vRecordToUpdate, sizeof(vRecordToUpdate), vSW);
      if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      {
        __TRACE();
        vResult = kSTATE_NOK;
      }
    }
  }
  //d.  Append an event into the file SFI=08h (Events) setting EventCode to 35h (Subway – Test)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayTest, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_NOK;
    }
  }
  //e.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Field range: display card detected
*/
int processFieldRange(void)
{
#ifndef WIN32
  clock_t vStartClockTime;      // end time of the pause of about 100ms
#endif
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processFieldRange\n");

  while (vResult == kSTATE_OK)
  {
    //a.  Abort the session, discarding the possible modifications.
    vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    {
      __TRACE();
      vResult = kSTATE_ABORT;
    }

    //b.  OK_SHORT to user
    if (vResult == kSTATE_OK)
      (void) poSignalState(kSignal_OK_SHORT);

    //c.  Pause 100 ms
    // compute the ending clock time (current clock plus 100 ms)
    // WARNING: this function supposes that clock() and CLOCKS_PER_SEC are available on the platform.
#ifndef WIN32
    vStartClockTime = clock();
    while (clock() - vStartClockTime <= (clock_t) (CLOCKS_PER_SEC / 10))  // wait until 100 ms are elapsed
    {
      // do nothing
    }
#else
    Sleep(100);
#endif

    //d.  Open a session with key #3, without reading a file (SFI=00h, RecNum=0).
    if (vResult == kSTATE_OK)
    {
      vPoResult = poSessionOpen(3, 0, 0x00, &gRatified, NULL, NULL, vSW);
      //e.  If no error occurred, continue to step a.
      //f.  Return ABORT
      if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      {
        __TRACE();
        vResult = kSTATE_ABORT;
      }
    }
  }

  return vResult;
}


//------------------------------------------------------

/*
	Free exchanges
*/
int processUpdateFree(void)
{
  unsigned char vSW[2];
  unsigned char vRecordToUpdate[29];  // Record to write in the card
  unsigned char vRecordLength = sizeof(vRecordToUpdate);  // size of the record to write
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processUpdateFree\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Update the record 1 of the file SFI=01h (Free):
  //  The 29 bytes are set to A5h
  if (vResult == kSTATE_OK)
  {
    memset(vRecordToUpdate, 0xA5, vRecordLength);
    vPoResult =
      poRecordUpdate(1, kSfiFree, vRecordToUpdate, vRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  Open a session with key #3, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(3, 0, 0, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //d.  Append an event into the file SFI=08h (Events) setting EventCode to 35h (Subway – Test)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayTest, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //e.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Update a binary file
*/
int processUpdateBinary(void)
{
  unsigned char vDataToUpdate[64];  // Data to write in the binary file
  unsigned char vDataLength = sizeof(vDataToUpdate);  // Size of the data to write
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;


  debugPrintf(">> processUpdateBinary\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Open a session with key #1, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(1, 0, 0, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  Update the file SFI=05h (Binary):
  //The 64 bytes are set to A5h.
  if (vResult == kSTATE_OK)
  {
    memset(vDataToUpdate, 0xA5, vDataLength);
    vPoResult = poBinaryUpdate(0, 5, vDataToUpdate, vDataLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //d.  Append an event into the file SFI=08h (Events) setting EventCode to 35h (Subway – Test).
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayTest, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //e.  Return OK.
  return vResult;
}


//------------------------------------------------------

/*
	Process Update 120 bytes
*/
int processUpdate120Bytes(void)
{
  unsigned char vNewEventDate[4]; // Current date in DateCompact / TimeCompact format
  unsigned char vSW[2];
  unsigned char vSERecord[kBufferLength]; // Special Event to write in the card
  unsigned char vSERecordLength = 0;
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  getCurrentTime(vNewEventDate);

  debugPrintf(">> processUpdate120Bytes\n");

  //a.  Update the record 1 of the file SFI=1Dh (Special event):
  memset(vSERecord, 0, sizeof(vSERecord));  // set all the record to 0
  //126 session bytes - Update the entire special event (120 bytes) setting the
  //non significant bytes to EEh into the file SFI=1Dh (Special Event):
  vSERecord[vSERecordLength++] = kVersionSpecialEvent;
  //EventDateStamp =  Current Date
  //EventTimeStamp =  Current Time
  memcpy(vSERecord + vSERecordLength, vNewEventDate, 4);
  vSERecordLength += 4;
  //EventFirstDateStamp = 0 (already to 0)
  //EventFirstTimeStamp = 0 (already to 0)
  vSERecordLength += 4;
  //EventLocationId =   An identifier of the terminal location
  vSERecord[vSERecordLength++] = (kLocationId >> 8) & 0xFF;
  vSERecord[vSERecordLength++] = kLocationId & 0xFF;
  //EventCode =   35h (Subway – Test)
  vSERecord[vSERecordLength++] = kEventSubwayTest;
  //EventDataAIDLen = Length of the EnvDataAID significant bytes
  vSERecord[vSERecordLength++] = gAIDLength;
  //EventDataAID =    AID returned by the Select Application command, the bytes over EventDataAIDLen remain to 0
  memcpy(vSERecord + vSERecordLength, gAID, gAIDLength);
  vSERecordLength += kAidMaxLength;
  //EventDataCardSerialNumber = Serial number as returned by the Select Application command
  memcpy(vSERecord + vSERecordLength, gSerialNumber, 8);
  vSERecordLength += 8;
  //EventDataStartupInfo =  Startup information as returned by the Select Application
  //    command
  memcpy(vSERecord + vSERecordLength, gInfos, 7);
  vSERecordLength += 7;
  // non significant bytes to EEh into the file SFI=1Dh
  memset(vSERecord + vSERecordLength, 0xEE, 120 - vSERecordLength);
  vSERecordLength = 120;

  vPoResult =
    poRecordUpdate(1, kSfiSpecialEvent, vSERecord, vSERecordLength, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Append an event into the file SFI=08h (Events) setting EventCode to 35h (Subway – Test).
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayTest, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  OK to user and return.
  return vResult;
}


//------------------------------------------------------

/*
	Process Read Outside of a Session
*/
int processReadOutsideSession(void)
{
  unsigned char vRecord[kBufferLength]; // Record read from the card outside of session
  unsigned char vRecordLength = 29; // Length to read
  unsigned char vSW[2];
  int vPoResult = kSUCCESS;
  int vResult = kSTATE_OK;

  debugPrintf(">> processReadOutsideSession\n");

  //a.  Close the session discarding the possible modifications.
  vPoResult = poSessionClose(kCLOSE_CANCEL, vSW);
  if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
    vResult = kSTATE_NOK;

  //b.  Read the record 1 of the file SFI=09h (Contracts).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poRecordRead(1, kSfiContract, vRecord, &vRecordLength, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //c.  Open a session with key #3, without reading a file (SFI=00h, RecNum=0).
  if (vResult == kSTATE_OK)
  {
    vPoResult = poSessionOpen(3, 0, 0, &gRatified, NULL, NULL, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //d.  Append an event into the file SFI=08h (Events) setting EventCode to 35h (Subway – Test)
  if (vResult == kSTATE_OK)
  {
    vPoResult = EventAppend(kEventSubwayTest, 0, vSW);
    if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
      vResult = kSTATE_NOK;
  }
  //e.  Return OK.
  return vResult;
}


// -------------------------------------------------------------------------
//              MAIN LOOP
// -------------------------------------------------------------------------

int main(int argc,              // standard main arguments, not used here
         char **argv            // ...
  )
{
  unsigned char vSW[2];         // Status Word returned by the card
  int vPoResult;                // Card Reader function result
  int vCurrentState;            // Current algorithm state

  CalypsoAuditStartup(argc, argv);

  if (audit_config.Test)
  {
    kAntiPassBackDelay = 20;
    kInterchangeDelay  = 60;
  }

  //--------------------------
  //a.  Do all required initializations and get the version of the OEM Reader firmware.
  //--------------------------
  debugPrintf
    ("# Audit Application - V%s\n#  Simple terminal application for the Calypso Registration Audit.\n",
     kVersion);

  // Get coupler Version and display it
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
    vCurrentState = kSTATE_OK;  // no error for now, initialized before every new card search

    //--------------------------
    //b.  Search a portable object (with automatic selection with AID beginning with "1TIC.ICALT").
    //--------------------------

    gAIDLength = 0;             // JDA
    vPoResult = poDetect(gAID_LT, sizeof(gAID_LT), gAID, &gAIDLength, gSerialNumber, gInfos);

    // if card not found, loop to search a new card
    if (vPoResult != kSUCCESS)
      continue;                 //>>>>>>>>>> CONTINUE

    //--------------------------
    //c.  If a portable object is found, but not the application requested, select the application "1TIC.ICALS".
    //--------------------------
    if (gAIDLength == 0)
      vPoResult =
        poApplicationSelect(gAID_LS, sizeof(gAID_LS), gAID, &gAIDLength,
                            gSerialNumber, gInfos, vSW);

    if (audit_config.Test)
    {
      // JDA ::: try with default AID
      if (gAIDLength == 0)
        vPoResult =
          poApplicationSelect(gAID___, sizeof(gAID___), gAID, &gAIDLength,
                              gSerialNumber, gInfos, vSW);
    }

    //--------------------------
    //d.  If the application is not found, NOK to user, and continue to step m.
    //--------------------------
    // use gAIDLength and not vSW because vSW is not intialized, if poDetect returns the right AID 
    if (vPoResult != kSUCCESS || gAIDLength == 0)
    {
      (void) poSignalState(kSignal_NOK);
      vCurrentState = kSTATE_NOT_OPEN;
    }
    //--------------------------
    //e.  Start a validation by opening a session with key #3, reading the record 1 of the file SFI=07h (Environment).
    //--------------------------
    if (vCurrentState == kSTATE_OK)
    {
      debugPrintByteArray("gAID", gAID, gAIDLength);
      debugPrintByteArray("gSerialNumber", gSerialNumber,
                          sizeof(gSerialNumber));
      debugPrintByteArray("gInfos", gInfos, sizeof(gInfos));

      vPoResult =
        poSessionOpen(3, 1, kSfiEnvironment, &gRatified, gEnvironmentRecord,
                      &gEnvironmentRecordLength, vSW);

      if (vPoResult != kSUCCESS || vSW[0] != 0x90 || vSW[1] != 0x00)
        vCurrentState = kSTATE_ABORT;
      else
        debugPrintByteArray("gEnvironmentRecord", gEnvironmentRecord,
                            gEnvironmentRecordLength);
    }

    //--------------------------
    //f.  If the EnvApplicationVersionNumber is equal to 0 (virgin environment), call Process Perso setting the returned value in Result, then continue to step l.
    //--------------------------
    // EnvApplicationVersionNumber => First byte of gEnvironmentRecord
    if (vCurrentState == kSTATE_OK && gEnvironmentRecord[0] == 0)
    {
      vCurrentState = processPerso();
    } else if (vCurrentState == kSTATE_OK)
    {
      //--------------------------
      //g.  If the EnvApplicationVersionNumber is not equal to 1, continue to step l with Result = NOK.
      //--------------------------
      if (gEnvironmentRecord[0] != kVersionEnvApplication)
        vCurrentState = kSTATE_NOK;

      //--------------------------
      //h.  If the EnvNetworkId field is different from Brussels (056002h), continue to step l with Result = NOK.
      //--------------------------
      // Network ID is the 3 next bytes : gEnvironmentRecord[1], gEnvironmentRecord[2] and gEnvironmentRecord[3]
      else if
        (gEnvironmentRecord[1] != ((kBRUSSEL_NETWK_ID >> 16) & 0xFF) ||
         gEnvironmentRecord[2] != ((kBRUSSEL_NETWK_ID >> 8) & 0xFF) ||
         gEnvironmentRecord[3] != (kBRUSSEL_NETWK_ID & 0xFF))
        vCurrentState = kSTATE_NOK;

      //--------------------------
      //i.  Read the record 1 of the file SFI=08h (Events).
      //--------------------------
      else
      {
        gEventRecordLength = 12;  // This file contains 12 bytes
        vPoResult =
          poRecordRead(1, kSfiEvents, gEventRecord, &gEventRecordLength, vSW);

        if (vPoResult != 0 || vSW[0] != 0x90 || vSW[1] != 0x00)
          vCurrentState = kSTATE_ABORT;
        else
          debugPrintByteArray("gEventRecord 1", gEventRecord,
                              gEventRecordLength);
      }

      //--------------------------
      //j.  Manage anti-passback: If the most recent event is not empty and it occurred less than 10 minutes ago
      //    (EventDateStamp, EventTimeStamp) at the same Location ID (EventLocationId), continue to step l with Result = ABORT.
      //--------------------------
      if (vCurrentState == kSTATE_OK)
      {
        if (0x00 != gEventRecord[kEventVersionNumber] &&
            (kLocationId >> 8) == gEventRecord[kEventLocationId] &&
            (kLocationId & 0xFF) == gEventRecord[kEventLocationId + 1] &&
            diffSeconds(gEventRecord + kEventDateStamp,
                        gEventRecord + kEventTimeStamp) < kAntiPassBackDelay)
          vCurrentState = kSTATE_ABORT;
      }
      //--------------------------
      //k.  According to the value of EnvHolderId call the corresponding process and get the value returned in Result:
      //--------------------------
      debugPrintf("# Test %d\n", gEnvironmentRecord[7]);
      if (vCurrentState == kSTATE_OK)
      {
        switch (gEnvironmentRecord[7])
        {
            //000001h to 000005h: call Process Load
          case 0x01:
          case 0x02:
          case 0x03:
          case 0x04:
          case 0x05:
            vCurrentState = processLoad();
            break;
            //000006: call Process Invalidation
          case 0x06:
            //processRehabilitation();
            vCurrentState = processInvalidation();
            break;
            //000007: call Process Rehabilitation
          case 0x07:
            vCurrentState = processRehabilitation();
            break;
            //000008: call Process Verify Pin Plain
          case 0x08:
            vCurrentState = processVerifyPinPlain();
            break;
            //000009: call Process Verify Pin Encrypted
          case 0x09:
            vCurrentState = processVerifyPinEncrypted();
            break;
            //00000A: call Process Change Pin Plain
          case 0x0A:
            vCurrentState = processChangePinPlain();
            break;
            //00000B: call Process Change Pin Encrypted
          case 0x0B:
            vCurrentState = processChangePinEncrypted();
            break;
            //00000C: call Process Ratification
          case 0x0C:
            vCurrentState = processRatification();
            break;
            //00000D: call Process Session Buffer Size
          case 0x0D:
            vCurrentState = processSessionBufferSize();
            break;
            //00000E: call Process Session Buffer Overflow
          case 0x0E:
            vCurrentState = processSessionBufferOverflow();
            break;
            //00000F: call Process Session Cmd Num
          case 0x0F:
            vCurrentState = processSessionCmdNum();
            break;
            //000010: call Process Field Range
          case 0x10:
            vCurrentState = processFieldRange();
            break;
            //000011: call Process Update Free
          case 0x11:
            vCurrentState = processUpdateFree();
            break;
            //000012: call Process Update Binary
          case 0x12:
            vCurrentState = processUpdateBinary();
            break;
            //000013: call Process Update 120 bytes
          case 0x13:
            vCurrentState = processUpdate120Bytes();
            break;
            //000014: call Process Read Outside of a Session  
          case 0x14:
            vCurrentState = processReadOutsideSession();
            break;
            //Other:  call Process Validation.
          default:
            vCurrentState = processValidation();
        }
      }
    }
    //--------------------------
    //l.  According to Result:
    //--------------------------
    if (vCurrentState != kSTATE_NOT_OPEN)
    {
      switch (vCurrentState)
      {
          //OK: Close the session correctly correctly (without ratification after Close Session)..
          //According to the Close Session result: OK or NOK to user
        case kSTATE_OK:
          vPoResult = poSessionClose(kCLOSE_EXPLICIT_RATIFICATION, vSW);
          if (vPoResult != 0 || vSW[0] != 0x90 || vSW[1] != 0x00)
            (void) poSignalState(kSignal_NOK);
          else
            (void) poSignalState(kSignal_OK);
          break;
          //NOK:  Abort the session, discarding possible modifications
          //NOK to user
        case kSTATE_NOK:
          (void) poSessionClose(kCLOSE_CANCEL, vSW);
          (void) poSignalState(kSignal_NOK);
          break;
          //ABORT:  Abort the session, discarding possible modifications
        case kSTATE_ABORT:
          (void) poSessionClose(kCLOSE_CANCEL, vSW);
          (void) poSignalState(kSignal_NOK_BLINK); // JDA
          break;
          //ACTION_REQUIRED:  Close the session correctly (without ratification).
          //OK_BLINK to user (the led will blink for 4 seconds and the auditor must remove the card as soon as possible)
        case kSTATE_ACTION_REQUIRED:
          (void) poSessionClose(kCLOSE_WITHOUT_RATIFICATION, vSW);
          (void) poSignalState(kSignal_OK_BLINK);
          break;
        default:
          debugPrintf("Unknown Result %d\n", vCurrentState);
          break;
      }
    }
    //--------------------------
    //m.  Deselect the portable object (with reset of the radio field).
    //--------------------------
    (void) poRelease();

    //--------------------------
    //n.  Continue to step b.
    //--------------------------
  }
}

//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
