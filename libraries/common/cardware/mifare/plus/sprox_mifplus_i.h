#ifndef __SPROX_MIFPLUS_I_H__
#define __SPROX_MIFPLUS_I_H__

//#define MIFPLUS_DEBUG

#ifdef WIN32
  #include <windows.h>
  #include <tchar.h>
  #define SPROX_MIFPLUS_LIB __declspec( dllexport )
#endif

#ifndef _USE_PCSC

  /*
   * SpringProx API compliant library
   * --------------------------------
   */
   
    #define SPROX_RC                     SWORD

  #ifndef SPROX_API_REENTRANT

    #define SPROX_API_FUNC_T(a, b)       SPROX_MIFPLUS_LIB a     SPROX_MIFPLUS_API SPROX_ ## b
    #define SPROX_API_FUNC(a)            SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_ ## a
    #define SPROX_MIFPLUS_GET_CTX()      SPROX_MIFPLUS_CTX_ST *ctx = &mifplus_ctx; (void) ctx;
    #define SPROX_MIFPLUS_GET_CTX_V()    SPROX_MIFPLUS_CTX_ST *ctx = &mifplus_ctx; (void) ctx;
    #define SPROX_API_CALL(a)            SPROX_ ## a
    #define SPROX_PARAM
    #define SPROX_PARAM_V                void
    #define SPROX_PARAM_P
    #define SPROX_PARAM_PV
  
    #include "products/springprox/api/springprox.h"
    #include "sprox_mifplus.h"
    
  #else

    #define SPROX_API_FUNC_T(a, b)       SPROX_MIFPLUS_LIB a     SPROX_MIFPLUS_API SPROXx_ ## b
    #define SPROX_API_FUNC(a)            SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_ ## a
    #define SPROX_MIFPLUS_GET_CTX()      SPROX_MIFPLUS_CTX_ST *ctx = mifplus_get_ctx(rInst); if (ctx == NULL) return MI_INVALID_READER_CONTEXT;
    #define SPROX_MIFPLUS_GET_CTX_V()    SPROX_MIFPLUS_CTX_ST *ctx = mifplus_get_ctx(rInst); if (ctx == NULL) return;
    #define SPROX_API_CALL(a)            SPROXx_ ## a
    #define SPROX_PARAM                  SPROX_INSTANCE rInst, 
    #define SPROX_PARAM_V                SPROX_INSTANCE rInst
    #define SPROX_PARAM_P                rInst,
    #define SPROX_PARAM_PV               rInst
    
    #include "products/springprox/api/springprox_ex.h"
    #include "sprox_mifplus_ex.h"
     
  #endif
  
#else
  
  /*
   * PC/SC API compliant library
   * ---------------------------
   */
    
    #define SPROX_RC                     LONG

    #define SPROX_API_FUNC_T(a, b)       SPROX_MIFPLUS_LIB a     SPROX_MIFPLUS_API SCard ## b
    #define SPROX_API_FUNC(a)            SPROX_MIFPLUS_LIB LONG  SPROX_MIFPLUS_API SCard ## a
    #define SPROX_MIFPLUS_GET_CTX()      SPROX_MIFPLUS_CTX_ST *ctx = mifplus_get_ctx(hCard); if (ctx == NULL) return SCARD_E_INVALID_HANDLE;
    #define SPROX_MIFPLUS_GET_CTX_V()    SPROX_MIFPLUS_CTX_ST *ctx = mifplus_get_ctx(hCard); if (ctx == NULL) return;
    #define SPROX_API_CALL(a)            SCard ## a
    #define SPROX_PARAM                  SCARDHANDLE hCard, 
    #define SPROX_PARAM_V                SCARDHANDLE hCard
    #define SPROX_PARAM_P                hCard,
    #define SPROX_PARAM_PV               hCard
   
    #include "pcsc_mifplus.h"

#endif


#ifndef TRUE
  #define TRUE 1
#endif
#ifndef FALSE
  #define FALSE 0
#endif


/* AES cipher must be embedded in this library */
/* ------------------------------------------- */

typedef struct
{
  DWORD enc_schd[60];  /* Key schedule                          */
  DWORD dec_schd[60];  /* Key schedule                          */
  DWORD key_bits;      /* Size of the key (bits)                */
  DWORD rounds;        /* Key-length-dependent number of rounds */
} AES_CTX_ST;

void AES_Init(AES_CTX_ST *context, const BYTE key[16]);
void AES_Encrypt(AES_CTX_ST *context, BYTE data[16]);
void AES_Encrypt2(AES_CTX_ST *context, BYTE outbuf[16], BYTE inbuf[16]);
void AES_Decrypt(AES_CTX_ST *context, BYTE data[16]);
void AES_Decrypt2(AES_CTX_ST *context, BYTE outbuf[16], BYTE inbuf[16]);

void GetRandomBytes(SPROX_PARAM  BYTE rnd[], DWORD size);
void GetRandomBytes_Hook(SPROX_PARAM  BYTE rnd[], DWORD size);

void MifPlus_InitCmac(SPROX_PARAM  const BYTE key[16]);
void MifPlus_ComputeCmac(SPROX_PARAM  const BYTE data[], DWORD length, BYTE cmac[]);
void MifPlus_GetCbcVector_Command(SPROX_PARAM  BYTE vector[16]);
void MifPlus_GetCbcVector_Response(SPROX_PARAM  BYTE vector[16]);
void MifPlus_ComputeKey(SPROX_PARAM  const BYTE rnd_pcd[16], const BYTE rnd_picc[16], BYTE key_constant, BYTE key_value[16]);

SPROX_API_FUNC(MifPlus_Command) (SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len);

SPROX_RC MifPlus_CommandHook(SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len);


SPROX_RC MifPlus_Result(BYTE rsp_buffer[], WORD rsp_got_len, WORD rsp_expect_len);


typedef struct
{
  BYTE level;
	BYTE cid;
  BOOL tcl;

	BOOL in_session;
	BYTE transaction_id[4];
	WORD read_counter;
	WORD write_counter;

	BYTE session_enc_key[16];
	BYTE session_mac_key[16];

	AES_CTX_ST main_cipher;
	AES_CTX_ST cmac_cipher;
	BYTE cmac_subkey_1[16];
	BYTE cmac_subkey_2[16];

} SPROX_MIFPLUS_CTX_ST;

#ifndef _USE_PCSC
  #ifndef SPROX_API_REENTRANT
    extern SPROX_MIFPLUS_CTX_ST mifplus_ctx;
  #else
    SPROX_MIFPLUS_CTX_ST *mifplus_get_ctx(SPROX_INSTANCE rInst);
  #endif
#else
    SPROX_MIFPLUS_CTX_ST *mifplus_get_ctx(SCARDHANDLE hCard);
#endif

#ifdef _WITH_SELFTEST
  extern BOOL in_selftest;
#endif

#undef D
#define D(x)

void dump_buffer(const BYTE * buffer, WORD len);

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#endif
