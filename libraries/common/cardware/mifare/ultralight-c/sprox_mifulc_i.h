#ifndef __SPROX_MIFULC_I_H__
#define __SPROX_MIFULC_I_H__

#ifdef WIN32
  #include <windows.h>
  #include <tchar.h>
  #define SPROX_MIFULC_LIB __declspec( dllexport )
#endif

#ifndef _USE_PCSC

  /*
   * SpringProx API compliant library
   * --------------------------------
   */
   
    #define SPROX_RC                     SWORD
	
	#define SUCCESS                      MI_OK

  #ifndef SPROX_API_REENTRANT

    #define SPROX_API_FUNC_T(a, b)       SPROX_MIFULC_LIB a     SPROX_MIFULC_API SPROX_ ## b
    #define SPROX_API_FUNC(a)            SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROX_ ## a
    #define SPROX_MIFULC_GET_CTX()       
    #define SPROX_MIFULC_GET_CTX_V()     
    #define SPROX_API_CALL(a)            SPROX_ ## a
    #define SPROX_PARAM
    #define SPROX_PARAM_V                void
    #define SPROX_PARAM_P
    #define SPROX_PARAM_PV
  
    #include "products/springprox/api/springprox.h"
    #include "sprox_mifulc.h"
    
  #else

    #define SPROX_API_FUNC_T(a, b)       SPROX_MIFULC_LIB a     SPROX_MIFULC_API SPROXx_ ## b
    #define SPROX_API_FUNC(a)            SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROXx_ ## a
    #define SPROX_MIFULC_GET_CTX()       if (rInst == NULL) return MI_INVALID_READER_CONTEXT;
    #define SPROX_MIFULC_GET_CTX_V()     if (rInst == NULL) return;
    #define SPROX_API_CALL(a)            SPROXx_ ## a
    #define SPROX_PARAM                  SPROX_INSTANCE rInst, 
    #define SPROX_PARAM_V                SPROX_INSTANCE rInst
    #define SPROX_PARAM_P                rInst,
    #define SPROX_PARAM_PV               rInst
    
    #include "products/springprox/api/springprox_ex.h"
    #include "sprox_mifulc_ex.h"
     
  #endif
  
#else
  
  /*
   * PC/SC API compliant library
   * ---------------------------
   */
    
    #define SPROX_RC                     LONG
	
	#define SUCCESS                      SCARD_S_SUCCESS

    #define SPROX_API_FUNC_T(a, b)       SPROX_MIFULC_LIB a     SPROX_MIFULC_API SCard ## b
    #define SPROX_API_FUNC(a)            SPROX_MIFULC_LIB LONG  SPROX_MIFULC_API SCard ## a
    #define SPROX_MIFULC_GET_CTX()       if (!hCard) return SCARD_F_INTERNAL_ERROR;
    #define SPROX_MIFULC_GET_CTX_V()     if (!hCard) return;
    #define SPROX_API_CALL(a)            SCard ## a
    #define SPROX_PARAM                  SCARDHANDLE hCard, 
    #define SPROX_PARAM_V                SCARDHANDLE hCard
    #define SPROX_PARAM_P                hCard,
    #define SPROX_PARAM_PV               hCard
   
    #include "pcsc_mifulc.h"

#endif


#ifndef TRUE
  #define TRUE 1
#endif
#ifndef FALSE
  #define FALSE 0
#endif


/* DES and 3-DES ciphering must be embedded in this library */
/* -------------------------------------------------------- */

typedef struct
{
  DWORD encrypt_subkeys[32];
  DWORD decrypt_subkeys[32];  
} DES_CTX_ST;

typedef struct
{
  DES_CTX_ST key1_ctx;
  DES_CTX_ST key2_ctx;
  DES_CTX_ST key3_ctx;
} TDES_CTX_ST;

void DES_Init(DES_CTX_ST *context, const BYTE key[8]);

void DES_Encrypt(DES_CTX_ST *context, BYTE inoutbuf[8]);
void DES_Decrypt(DES_CTX_ST *context, BYTE inoutbuf[8]);

void DES_Encrypt2(DES_CTX_ST *context, BYTE outbuf[8], const BYTE inbuf[8]);
void DES_Decrypt2(DES_CTX_ST *context, BYTE outbuf[8], const BYTE inbuf[8]);

void TDES_Init(TDES_CTX_ST *context, const BYTE key1[8], const BYTE key2[8], const BYTE key3[8]);

void TDES_Encrypt(TDES_CTX_ST *context, BYTE inoutbuf[8]);
void TDES_Decrypt(TDES_CTX_ST *context, BYTE inoutbuf[8]);

void TDES_Encrypt2(TDES_CTX_ST *context, BYTE outbuf[8], const BYTE inbuf[8]);
void TDES_Decrypt2(TDES_CTX_ST *context, BYTE outbuf[8], const BYTE inbuf[8]);

void GetRandomBytes(SPROX_PARAM  BYTE rnd[], DWORD size);

#ifndef _USE_PCSC
SPROX_RC MifUlC_Command(SPROX_PARAM  const BYTE send_buffer[], WORD send_length, BYTE recv_buffer[], WORD *recv_length);
#else
SPROX_RC MifUlC_Command(SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len);
SPROX_RC MifUlC_CommandEncapsulated(SPROX_PARAM  const BYTE cmd_buffer[], WORD cmd_len, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len);
#endif


#undef D
#define D(x)

void dump_buffer(const BYTE * buffer, WORD len);

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#endif
