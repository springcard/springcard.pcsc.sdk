#ifndef __SPROX_MIFULC_H__
#define __SPROX_MIFULC_H__

#include "products/springprox/api/springprox.h"
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#ifdef WIN32
  #include <windows.h>
  #include <tchar.h>

  #ifndef SPROX_MIFULC_LIB
    #define SPROX_MIFULC_LIB __declspec( dllimport )
  #endif

  #ifndef UNDER_CE
    /* Under Desktop Windows we use the cdecl calling convention */
    #define SPROX_MIFULC_API __cdecl
  #else
    /* Under Windows CE we use the stdcall calling convention */
    #define SPROX_MIFULC_API __stdcall
  #endif
#else
  #define SPROX_MIFULC_LIB
  #define SPROX_MIFULC_API
#endif

#ifdef __cplusplus
extern  "C"
{
#endif

SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROX_MifUlC_Authenticate(const BYTE key_value[16]);
SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROX_MifUlC_ChangeKey(const BYTE key_value[16]);

SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROX_MifUlC_Read(BYTE address, BYTE data[16]);
SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROX_MifUlC_Write4(BYTE address, const BYTE data[4]);
SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROX_MifUlC_Write(BYTE address, const BYTE data[16]);


#ifdef __cplusplus
}
#endif


#endif
