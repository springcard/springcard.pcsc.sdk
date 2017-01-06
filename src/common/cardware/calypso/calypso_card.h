#ifndef __CALYPSO_CARD_H__
#define __CALYPSO_CARD_H__

#define CALYPSO_MIN_SESSION_MODIFS 3
#define CALYPSO_MIN_RECORD_SIZE    29


#define CALYPSO_SFI_ENVIRONMENT    0x07
#define CALYPSO_SFI_TRANSPORT_LOG  0x08
#define CALYPSO_SFI_CONTRACTS      0x09
#define CALYPSO_SFI_COUNTERS       0x19
#define CALYPSO_SFI_CONTRACT_LIST  0x1E

#define CALYPSO_KEY_ISSUER         0x01
#define CALYPSO_KIF_ISSUER         0x21
#define CALYPSO_KNO_ISSUER         0x0C

#define CALYPSO_KEY_LOAD           0x02
#define CALYPSO_KIF_LOAD           0x27
#define CALYPSO_KNO_LOAD           0x02

#define CALYPSO_KEY_DEBIT          0x03
#define CALYPSO_KIF_DEBIT          0x30
#define CALYPSO_KNO_DEBIT          0x0D

#define CALYPSO_INS_GET_RESPONSE   0xC0
#define CALYPSO_INS_SELECT         0xA4
#define CALYPSO_INS_INVALIDATE     0x04
#define CALYPSO_INS_REHABILITATE   0x44
#define CALYPSO_INS_APPEND_RECORD  0xE2
#define CALYPSO_INS_DECREASE       0x30
#define CALYPSO_INS_INCREASE       0x32
#define CALYPSO_INS_READ_BINARY    0xB0
#define CALYPSO_INS_READ_RECORD    0xB2
#define CALYPSO_INS_UPDATE_BINARY  0xD6
#define CALYPSO_INS_UPDATE_RECORD  0xDC
#define CALYPSO_INS_WRITE_RECORD   0xD2
#define CALYPSO_INS_OPEN_SESSION   0x8A
#define CALYPSO_INS_CLOSE_SESSION  0x8E
#define CALYPSO_INS_GET_CHALLENGE  0x84
#define CALYPSO_INS_CHANGE_PIN     0xD8
#define CALYPSO_INS_VERIFY_PIN     0x20

#define CALYPSO_INS_SV_GET         0x7C
#define CALYPSO_INS_SV_DEBIT       0xBA
#define CALYPSO_INS_SV_RELOAD      0xB8
#define CALYPSO_INS_SV_UN_DEBIT    0xBC

#define CALYPSO_INS_SAM_SV_DEBIT   0x54
#define CALYPSO_INS_SAM_SV_RELOAD  0x56


typedef struct
{ 
  BOOL  _NotEmpty : 1;
  BOOL  _TestCard : 1;
  
  BOOL  _HolderDataCardStatus : 1;
  
  BYTE  VersionNumber;
  DWORD NetworkId;
  BYTE  ApplicationIssuerId;
  WORD  ApplicationEndDate;
  WORD  Authenticator;
  
  BYTE  HolderDataCardStatus;

} CALYPSO_ENVANDHOLDER_ST;

typedef struct
{
  BOOL  _NotEmpty : 1;
  BOOL  _StartDate : 1;
  BOOL  _EndDate : 1;
  BOOL  _NetworkId : 1;
  BOOL  _SerialNumber : 1;
  
  BYTE  Provider;    
  WORD  Tariff;  

  DWORD NetworkId;
  DWORD SerialNumber;
  WORD  StartDate;
  WORD  EndDate;
  BYTE  Areas;

  BYTE  Status;
  WORD  Authenticator;

  WORD  Authenticator_C;    
  
} CALYPSO_CONTRACT_ST;

typedef struct
{
  BOOL  _NotOKCounter : 1;
  BOOL  _LocationId : 1;
  BOOL  _Device : 1;
  BOOL  _RouteNumber : 1;
  BOOL  _JourneyRun : 1;
  BOOL  _VehicleId : 1;
  
  WORD  EventDate;
  WORD  EventTime;
  BYTE  Code;
  BYTE  ServiceProvider;
  BYTE  NotOKCounter;
  WORD  LocationId;
  WORD  Device;
  WORD  RouteNumber;
  WORD  JourneyRun;
  WORD  VehicleId;
  
  BYTE  ContractPointer;
  
} CALYPSO_EVENT_ST;

typedef struct
{
  BYTE  ShortId;
  BYTE  Type;
  BYTE  SubType;
  BYTE  RecSize;
  BYTE  NumRec;
  DWORD AC;
  DWORD NKey;
  BYTE  Status;
  BYTE  KVC[3];

} FILE_INFO_ST;



typedef struct
{
  BOOL   _IsReload;
  BOOL   _IsDebit;
  BOOL   _HasSerialNumber;

  signed long CurrentBalance;
  BYTE   CurrentKVC;

  struct
  {
    WORD   Date;
    WORD   Time;

    signed long Amount;
    signed long Balance;

    BYTE   KVC;
    BYTE   SamId[4];

  } ReloadOrDebit;

  struct
  {
    BYTE Free1;
    BYTE Free2;
  } Reload;

  BYTE   SerialNumber[8];

} CALYPSO_STOREDVALUE_ST;

#endif
