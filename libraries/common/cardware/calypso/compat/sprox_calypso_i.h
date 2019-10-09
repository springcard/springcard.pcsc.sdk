#ifndef __SPROX_CALYPSO_I_H__
#define __SPROX_CALYPSO_I_H__

#ifdef WIN32
#define SPROX_CALYPSO_LIB __declspec( dllexport )
#endif

#define SPROX_CALYPSO_FUNC(a)     SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_ ## a
#define SPROX_CALYPSO_FUNC2(a, b) SPROX_CALYPSO_LIB a SPROX_CALYPSO_API SPROX_ ## b

#ifndef SPROX_API_REENTRANT
  #define SPROX_CALYPSO_FUNC_EX(a)     SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_ ## a
  #define SPROX_CALYPSO_FUNC_EX2(a, b) SPROX_CALYPSO_LIB a SPROX_CALYPSO_API SPROX_ ## b
  #define SPROX_CALYPSO_GET_CTX()      SPROX_CALYPSO_CTX_ST *ctx = &calypso_ctx;
  #define SPROX_API_CALL(a)            SPROX_ ## a
  #define SPROX_PARAM
  #define SPROX_PARAM_V      void
  #define SPROX_PARAM_P
  #define SPROX_PARAM_PV
  
  #include "sprox_innovatron.h"
  
#else
  #define SPROX_CALYPSO_FUNC_EX(a)     SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_ ## a
  #define SPROX_CALYPSO_FUNC_EX2(a, b) SPROX_CALYPSO_LIB a SPROX_CALYPSO_API SPROXx_ ## b
  #define SPROX_CALYPSO_GET_CTX()      SPROX_CALYPSO_CTX_ST *ctx = calypso_get_ctx(rInst); if (ctx == NULL) return CALYPSO_NO_SUCH_INST;
  #define SPROX_API_CALL(a)            SPROXx_ ## a
  #define SPROX_PARAM        SPROX_INSTANCE rInst, 
  #define SPROX_PARAM_V      SPROX_INSTANCE rInst
  #define SPROX_PARAM_P      rInst,
  #define SPROX_PARAM_PV     rInst  
  
  #include "sprox_calypso_ex.h"
  
#endif


void sprox_calypso_trace(const char *fmt, ...);

typedef struct
{
  BYTE card_slot;
  BYTE sam_slot;
  BYTE pcd_addr;
  BYTE picc_addr;
  BYTE picc_seq;
  BOOL in_session;
  WORD card_sw;
  WORD sam_sw; 
} SPROX_CALYPSO_CTX_ST;

#ifndef SPROX_API_REENTRANT
  extern SPROX_CALYPSO_CTX_ST calypso_ctx;
#else
  SPROX_CALYPSO_CTX_ST *calypso_get_ctx(SPROX_INSTANCE rInst);
#endif



#endif
