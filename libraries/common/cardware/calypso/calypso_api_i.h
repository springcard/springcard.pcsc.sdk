/**h* CalypsoAPI/calypso_api_i.h
 *
 * NAME
 *   SpringCard Calypso API :: private prototypes
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 **/
#ifndef __CALYPSO_API_I_H__
#define __CALYPSO_API_I_H__

#define CALYPSO_API_VERSION "1.11"

#ifdef UNDER_CE
  #ifndef WIN32
    #define WIN32
  #endif
#endif

#ifdef __linux__
  #ifndef LINUX
    #define LINUX
  #endif
#endif

#ifdef WIN32
  #include <windows.h>
  #define CALYPSO_HOST
  #ifndef CALYPSO_LIB
    #define CALYPSO_LIB __declspec( dllexport )
  #endif
  #ifdef UNDER_CE
    #ifndef CALYPSO_NO_TIME_T
      #define CALYPSO_NO_TIME_T
    #endif
  #endif
#endif

#ifdef LINUX

  #include <wintypes.h>
  #define CALYPSO_HOST
  
  #include <termios.h>
  #include <unistd.h>  
  
  #ifndef MAX_PATH
  	#define MAX_PATH 256
  #endif

  #define _stprintf sprintf
  #define _tcscat strcat
  #define _tcsicmp strcmp
  #define _tcsncmp strncmp
  #define _tcsncpy strncpy
  #define _tcscpy strcpy
  #define _tcslen strlen
  #define _tcsclen strlen
  #define _tfopen fopen
  #define _ftprintf fprintf
  #define _T(x) x

#endif

#ifdef CALYPSO_HOST

  #ifdef SPROX_API
    #define CALYPSO_LEGACY
  #endif

  #include <stdlib.h>
  #include <stdio.h>
  #include <string.h>
  #include <time.h>

  #ifdef _USE_PCSC
    #include <winscard.h>
  #endif

  #include "lib-c/utils/binconvert.h"

  #ifdef CALYPSO_LEGACY
    #ifdef SPROX_API_REENTRANT
      #include "products/springprox/api/springprox_ex.h"
    #else
      #include "products/springprox/api/springprox.h"
    #endif
  #endif
  
#else
	
  #include "project.h"

#endif

#ifndef CALYPSO_MAX_DATA_SZ
  #define CALYPSO_MAX_DATA_SZ 256
#endif
#ifndef CALYPSO_MAX_APDU_SZ
  #define CALYPSO_MAX_APDU_SZ 262
#endif
#ifndef CALYPSO_MAX_FCI_SZ
  #define CALYPSO_MAX_FCI_SZ CALYPSO_MAX_DATA_SZ
#endif

#ifndef CALYPSO_TRACE
  #ifdef SPROX_INS_LIB_TRACE
    #define CALYPSO_TRACE SPROX_INS_LIB_TRACE
  #else
    #define CALYPSO_TRACE 1
  #endif
#endif
#ifndef CALYPSO_WITH_XML
  #define CALYPSO_WITH_XML 1
#endif
#ifndef CALYPSO_WITH_SAM
  #define CALYPSO_WITH_SAM 1
#endif
#ifndef CALYPSO_WITH_STOREDVALUE
  #define CALYPSO_WITH_STOREDVALUE 1
#endif

#include "calypso_types.h"
#include "calypso_card.h"
#include "calypso_strings.h"

#ifndef TRUE
  #define TRUE 1
#endif
#ifndef FALSE
  #define FALSE 0
#endif

#define CALYPSO_TYPE_PCSC         0x01
#define CALYPSO_TYPE_LEGACY       0x02

#ifdef _USE_PCSC
#include <winscard.h>
typedef struct
{
  BYTE        bTrace;
  SCARDHANDLE hCard;
  DWORD       dwProtocol;
  LONG        lLastResult;
} PCSC_CTX_ST;
#endif

#ifdef CALYPSO_LEGACY
typedef struct
{
  BYTE           bTrace;
  WORD           Proto;
  BYTE           CidOrSlot;
  SWORD          swLastResult;
  
#ifdef SPROX_API_REENTRANT
  SPROX_INSTANCE rInst;
#endif
  
} LEGACY_CTX_ST;
#endif

typedef struct
{
 
  struct
  {
    BYTE        Fci[CALYPSO_MAX_FCI_SZ];
    SIZE_T  FciLen;

    BYTE        StartupInfo[7];

    BYTE        DFName[16];
    SIZE_T  DFNameLen;


    BYTE        Platform;
    BYTE        Type;
    BYTE        SubType;
    BYTE        SoftIssuer;
    BYTE        SoftVersion;
    BYTE        SoftRevision;  
    BYTE        SessionMaxMods;

    BYTE        Revision;
    BYTE        UID[8];

    BOOL        SessionActive : 1;
    BOOL        Invalidated : 1;

    BOOL        WithStoredValue : 1;
    BOOL        NeedRatificationFrame : 1;
    BOOL        WithPin : 1;

    BYTE        SessionCurMods;

    BYTE        CurrentPin[4];
    BYTE        CurrentKif;
    BYTE        CurrentKvc;       

  } CardApplication;

  struct
  {
    BYTE        EnvVersion;

  } CardData;

#ifndef CALYPSO_NO_SAM

  struct
  {
    BYTE        UID[4];

  } SamApplication;

#endif

#ifdef CALYPSO_STOREDVALUE
  struct
  {
    BYTE       GetData[CALYPSO_MAX_DATA_SZ];
    SIZE_T GetDataSize;
  } CardStoredValue;
#endif

  struct
  {

#ifdef CALYPSO_HOST
    BYTE          Type;
  #ifdef _USE_PCSC
    PCSC_CTX_ST   Pcsc;
  #endif
  #ifdef CALYPSO_LEGACY
    LEGACY_CTX_ST Legacy;
  #endif
#else
    BOOL          Active;
    WORD          Proto;
    BYTE          Cid;
#endif

    BYTE          CLA;
    BOOL          T1;

    BYTE          Atr[32];
    SIZE_T    AtrLen;

    BYTE          Buffer[CALYPSO_MAX_APDU_SZ];
    WORD          SW;

  } Card;

#ifndef CALYPSO_NO_SAM
  struct
  {
#ifdef CALYPSO_HOST
    BYTE          Type;
  #ifdef _USE_PCSC
    PCSC_CTX_ST   Pcsc;
  #endif
  #ifdef CALYPSO_LEGACY
    LEGACY_CTX_ST Legacy;
  #endif
#else
    BOOL          Active;
    BYTE          Slot;
#endif

    BYTE          CLA;

    BYTE          Atr[32];
    SIZE_T    AtrLen;

    BYTE          Buffer[CALYPSO_MAX_APDU_SZ];
    WORD          SW;

  } Sam; 
#endif
  
  BYTE        LastCardSelectResp[CALYPSO_MAX_DATA_SZ];
  SIZE_T  LastCardSelectRespLen;

  BYTE        LastError;
  
#ifdef CALYPSO_BENCHMARK
  struct
  {
    BYTE  nb_select;
    BYTE  nb_read;
    BYTE  nb_write;
    WORD  card_dialog;
    WORD  sam_dialog;
  } benchmark;
#endif

  struct
  {    
    DWORD     SectionDepth;
    BYTE      OutputOptions;
    
    BOOL      OutputXml;   
#ifdef CALYPSO_HOST
    BOOL      OutputIni;
    FILE     *TargetFile;
    char     *TargetString;
    SIZE_T    TargetLength;
#endif

  } Parser;  

#ifdef CALYPSO_LEGACY
  #ifdef SPROX_API_REENTRANT
  SPROX_INSTANCE rInst;
  #endif
#endif

} CALYPSO_CTX_ST;

#define P_CALYPSO_CTX  CALYPSO_CTX_ST *

#include "calypso_api.h"


#define PARSER_OUT_NO_RAW      0x01
#define PARSER_OUT_NO_BITMAP   0x02
#define PARSER_OUT_NO_EXPLAIN  0x04
#define PARSER_OUT_NO_AUTH_C   0x08
#define PARSER_OUT_NO_TABS     0x40
#define PARSER_OUT_DIRTY       0x80

extern BOOL TrySamAutoUpdate;

void  CalypsoSetLastError(CALYPSO_CTX_ST *ctx, DWORD LastError);

void  ParserOut_Bin(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value, BYTE bitcount);
void  ParserOut_Hex(CALYPSO_CTX_ST *ctx, const char *varname, const BYTE value[], SIZE_T size);
void  ParserOut_Hex4(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value);
void  ParserOut_Hex8(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value);
void  ParserOut_Hex12(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value);
void  ParserOut_Hex16(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value);
void  ParserOut_Hex24(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value);
void  ParserOut_Hex32(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value);
#ifndef CALYPSO_NO_TIME_T
void  ParserOut_Date(CALYPSO_CTX_ST *ctx, const char *varname, time_t value);
void  ParserOut_DateId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, time_t value);
void  ParserOut_Time(CALYPSO_CTX_ST *ctx, const char *varname, time_t value);
void  ParserOut_TimeReal(CALYPSO_CTX_ST *ctx, const char *varname, time_t value);
#else
void  ParserOut_Date(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value);
void  ParserOut_DateId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, _DATETIME_ST *value);
void  ParserOut_Time(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value);
void  ParserOut_TimeReal(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value);
#endif    
void  ParserOut_Dec(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value);
void  ParserOut_DecId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, DWORD value);
void  ParserOut_Str(CALYPSO_CTX_ST *ctx, const char *varname, const char *value);
void  ParserOut_StrRaw(CALYPSO_CTX_ST *ctx, const char *varname, const char *value);
void  ParserOut_Remark(CALYPSO_CTX_ST *ctx, const char *remark);
void  ParserOut_SectionBegin(CALYPSO_CTX_ST *ctx, const char *sectionname);
void  ParserOut_SectionBeginId(CALYPSO_CTX_ST *ctx, const char *sectionname, DWORD sectionid);
void  ParserOut_SectionEnd(CALYPSO_CTX_ST *ctx, const char *sectionname);
void  ParserOut_IdfZones(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value);
void  ParserOut_SectionXml(CALYPSO_CTX_ST *ctx, const char *sectionname, const char *xmlcontent);
void  ParserOut_SectionRaw(CALYPSO_CTX_ST *ctx, const char *sectionname, const char *rawcontent);

void  CalypsoRecognizeRevision(CALYPSO_CTX_ST *ctx);

#define TR_CARD     0x10
#define TR_SAM      0x20
#define TR_TRANS    0x40

#define TR_TRANSMIT 0x01
#define TR_DEBUG    0x02
#define TR_TRACE    0x04
#define TR_ERROR    0x08

#if (CALYPSO_TRACE)

  #ifndef CALYPSO_HOST

    extern BYTE calypso_trace_level;
  
    #define CalypsoTraceStr(m,i)       if ((m)&calypso_trace_level) { trace_s(i); trace_crlf(); }
    #define CalypsoTraceRC(m,i,v)      if ((m)&calypso_trace_level) { trace_s(i); trace_d(v,0); trace_crlf(); }
    #define CalypsoTraceValS(m,i,v)    if ((m)&calypso_trace_level) { trace_s(i); trace_s(v); trace_crlf(); }
    #define CalypsoTraceValH(m,i,v,l)  if ((m)&calypso_trace_level) { trace_s(i); if(l<=2)trace_hb(v);else if(l<=4)trace_hw(v);else trace_hdw(v); trace_crlf(); }
    #define CalypsoTraceValD(m,i,v,l)  if ((m)&calypso_trace_level) { trace_s(i); trace_d(v,l); trace_crlf(); }
    #define CalypsoTraceHex(m,i,v,l)   if ((m)&calypso_trace_level) { trace_s(i); trace_h(v,l); trace_crlf(); }

  #else

    void  CalypsoTraceStr(BYTE level, const char *info);
    void  CalypsoTraceRC(BYTE level, const char *info, CALYPSO_RC rc);
    void  CalypsoTraceValS(BYTE level, const char *info, const char *value);
    void  CalypsoTraceValH(BYTE level, const char *info, DWORD value, BYTE len);
    void  CalypsoTraceValD(BYTE level, const char *info, DWORD value, BYTE len);
    void  CalypsoTraceHex(BYTE level, const char *info, const BYTE data[], SIZE_T size);

  #endif
  
#else
  
#define CalypsoTraceStr(m,i)       {}
#define CalypsoTraceRC(m,i,v)      {}
#define CalypsoTraceValS(m,i,v)    {}
#define CalypsoTraceValH(m,i,v,l)  {}
#define CalypsoTraceValD(m,i,v,l)  {}
#define CalypsoTraceHex(m,i,v,l)   {}

#endif

#ifdef _USE_PCSC
CALYPSO_RC CalypsoPcscTransmit(PCSC_CTX_ST *pcsc_ctx, const BYTE in_buffer[], SIZE_T in_length, BYTE out_buffer[], SIZE_T *out_length);
CALYPSO_RC CalypsoPcscGetAtr(PCSC_CTX_ST *pcsc_ctx, BYTE atr[], SIZE_T *atrlen);
#endif

#ifdef CALYPSO_LEGACY
CALYPSO_RC CalypsoLegacyTransmit(LEGACY_CTX_ST *legacy_ctx, const BYTE in_buffer[], SIZE_T in_length, BYTE out_buffer[], SIZE_T *out_length);
CALYPSO_RC CalypsoLegacyGetAtr(BYTE atr[], SIZE_T *atrlen);
#endif

#ifndef CALYPSO_NO_TIME_T
#include <time.h>
time_t translate_date_bcd32(DWORD value);
time_t translate_datestamp_14(DWORD value);
time_t translate_timestamp_11(DWORD value);
time_t translate_timereal_30(DWORD value);
time_t translate_timereal_32(DWORD value);
#else
_DATETIME_ST *translate_date_bcd32(DWORD value);
_DATETIME_ST *translate_datestamp_14(DWORD value);
_DATETIME_ST *translate_timestamp_11(DWORD value);
_DATETIME_ST *translate_timereal_30(DWORD value);
_DATETIME_ST *translate_timereal_32(DWORD value);
#endif 
  
BYTE  bcdtob(BYTE b);
WORD  bcdtow(WORD w);
DWORD bcdtodw(DWORD dw);
BYTE  btobcd(BYTE b);
BOOL  isbcdb(BYTE b);



BOOL get_char5_bits(const BYTE data[], SIZE_T size, CALYPSO_BITS_SZ *bit_offset, BYTE bit_count, char value[], BYTE length);
BOOL get_dword_bits(const BYTE data[], SIZE_T size, CALYPSO_BITS_SZ *bit_offset, BYTE bit_count, DWORD *value);
BOOL set_dword_bits(BYTE data[], SIZE_T size, CALYPSO_BITS_SZ *bit_offset, BYTE bit_count, DWORD value);

CALYPSO_BITS_SZ count_zeros(const BYTE buffer[], CALYPSO_BITS_SZ bit_count);

#ifndef UNUSED_PARAMETER
  #define UNUSED_PARAMETER(a) (void) (a)
#endif

#endif

