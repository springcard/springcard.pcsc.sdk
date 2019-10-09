#ifndef __CALYPSO_AUDIT_H__
#define __CALYPSO_AUDIT_H__

#ifdef WIN32
  #define CALYPSO_HOST

  #include <windows.h> 
  #include <winscard.h>
  #include <stdio.h>

  BOOL PcscStartup(void);
  void PcscCleanup(void);
  DWORD PcscReaderCount(void);
  char *PcscReaderName(DWORD idx);
  DWORD PcscReaderIndex(const char *name);
  BOOL PcscCardPresent(DWORD idx);
  BOOL PcscCardAtr(DWORD idx, BYTE atr[], DWORD *atrlen);
  BOOL PcscWaitCard(DWORD idx, BYTE atr[], DWORD *atrlen);
  BOOL PcscConnect(DWORD idx, SCARDHANDLE *hCard);
  BOOL PcscDisconnect(SCARDHANDLE hCard, DWORD dwDisposition);
  BOOL PcscDirectTransmit(DWORD idx, const BYTE send_cmd[], DWORD send_len, BYTE recv_cmd[], DWORD *recv_len);

#endif

#ifdef __linux__
  #define CALYPSO_HOST
#endif

#ifdef CALYPSO_HOST
  #include "products/springprox/api/springprox.h"
  #include "cardware/calypso/calypso_api.h"
#else
  #include "project.h"
#endif


typedef struct
{
  BOOL  Verbose;
  BOOL  Quiet;
  BOOL  Test;

  BOOL  SamPcsc;
  DWORD SamPcscIndex;
  BYTE  SamLegacySlot;

  BOOL  CardPcsc;
  DWORD CardPcscIndex;
  WORD  CardLegacyProtos;

  BYTE  LogLevel;
#ifdef CALYPSO_HOST
  BOOL  LogCreate;
  char  LogFile[MAX_PATH];
#endif


} CALYPSO_AUDIT_CFG_ST;

extern CALYPSO_AUDIT_CFG_ST audit_config;

extern P_CALYPSO_CTX cal_ctx;
extern SWORD         rdr_rc;
extern CALYPSO_RC    cal_rc;

#ifdef CALYPSO_HOST

extern SCARDHANDLE   hCardPcsc;
extern SCARDHANDLE   hSamPcsc;

extern BYTE          SamAtr[32];
extern SIZE_T    SamAtrLen;

void CalypsoAuditStartup(int argc, char **argv);

#define TRACE_S(v)                printf((v == NULL) ? "\n" : v)
#define TRACE_B(v)                printf("%c", v)
#define TRACE_D(v,l)              printf("%ld", v)
#define TRACE_H(v,l,d)            { int i; for (i=0; i<(int)l; i++) printf("%02X", v[i]); }
#define TRACE_HB(v)               printf("%02X", v)
#define TRACE_HW(v)               printf("%04X", v)

void set_leds_t(WORD value, WORD ms);

#else

#define TRACE_S                   trace_s
#define TRACE_D                   trace_d
#define TRACE_B                   trace_b
#define TRACE_H                   trace_h
#define TRACE_HB                  trace_hb
#define TRACE_HW                  trace_hw

#endif

void TRACE_RC(const char *s);
void poFatal(void);

#endif
