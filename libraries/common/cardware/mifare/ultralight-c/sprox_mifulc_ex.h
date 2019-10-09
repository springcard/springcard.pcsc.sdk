#ifndef __SPROX_MIFULC_EX_H__
#define __SPROX_MIFULC_EX_H__

#include "products/springprox/api/springprox_ex.h"
#include "sprox_mifulc.h"

#ifdef __cplusplus
extern  "C"
{
#endif

SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROXx_MifUlC_Authenticate(SPROX_INSTANCE rInst, const BYTE key_value[16]);
SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROXx_MifUlC_ChangeKey(SPROX_INSTANCE rInst, const BYTE key_value[16]);

SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROXx_MifUlC_Read(SPROX_INSTANCE rInst, BYTE address, BYTE data[16]);
SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROXx_MifUlC_Write4(SPROX_INSTANCE rInst, BYTE address, const BYTE data[4]);
SPROX_MIFULC_LIB SWORD SPROX_MIFULC_API SPROXx_MifUlC_Write(SPROX_INSTANCE rInst, BYTE address, const BYTE data[16]);


#ifdef __cplusplus
}
#endif

#endif
