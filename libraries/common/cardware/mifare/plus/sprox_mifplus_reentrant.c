/**h* MifPlusAPI/LegacyEX
 *
 * NAME
 *   MifPlusAPI :: Specific entry points for Legacy re-entrant DLL
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Legacy
 *
 **/
#include "sprox_mifplus_i.h"

#ifndef _USE_PCSC
#ifdef SPROX_API_REENTRANT

#define MAX_INSTANCES 16

typedef struct
{
  SPROX_INSTANCE        sprox_ctx;
  SPROX_MIFPLUS_CTX_ST  *mifplus_ctx;

} INSTANCE_ST;

INSTANCE_ST instances[MAX_INSTANCES] = { 0 };


/**f* MifPlusAPI/[Legacy]AttachLibrary
 *
 * NAME
 *   [Legacy]AttachLibrary
 *
 * DESCRIPTION
 *   Associates the Mifare Plus DLL with an instance of SpringProx re-entrant DLL.
 *   Call this function immediately after SPROXx_ReaderOpen to be able to
 *   work with any Mifare Plus card on this SpringProx/CSB reader.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   Not applicable.
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_AttachMifPlusLibrary(SPROX_INSTANCE rInst);
 *
 *   [[pcsc_mifplus.dll]]
 *   Not applicable. See [PCSC]AttachLibrary.
 *
 * INPUTS
 *   SPROX_INSTANCE rInst : instance of springprox_ex.dll
 *
 * RETURNS
 *   MI_OK : library attached
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   [Legacy]DetachLibrary
 *
 **/
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_AttachMifPlusLibrary(SPROX_INSTANCE rInst)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].sprox_ctx == rInst)
      goto take_this_one;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].sprox_ctx == NULL)
      goto take_this_one;
  
  return MI_LIB_INTERNAL_ERROR;
  
take_this_one:

  instances[i].mifplus_ctx = malloc(sizeof(SPROX_MIFPLUS_CTX_ST));
  if (instances[i].mifplus_ctx == NULL)
    return MI_OUT_OF_MEMORY_ERROR;   
  instances[i].sprox_ctx = rInst;
  
  memset(instances[i].mifplus_ctx, 0, sizeof(SPROX_MIFPLUS_CTX_ST));
  instances[i].mifplus_ctx->cid = 0xFF;
  
  return MI_OK;
}

/**f* MifPlusAPI/[Legacy]DetachLibrary
 *
 * NAME
 *   [Legacy]DetachLibrary
 *
 * DESCRIPTION
 *   Remove the attachement between the Mifare Plus DLL and an instance
 *   of SpringProx re-entrant DLL.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   Not applicable.
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_DetachMifPlusLibrary(SPROX_INSTANCE rInst);
 *
 *   [[pcsc_mifplus.dll]]
 *   Not applicable. See [PCSC]DetachLibrary.
 *
 * INPUTS
 *   SPROX_INSTANCE rInst : instance of springprox_ex.dll
 *
 * RETURNS
 *   MI_OK : library attached
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   [Legacy]AttachLibrary
 *
 **/
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_DetachMifPlusLibrary(SPROX_INSTANCE rInst)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
  {
    if (instances[i].sprox_ctx == rInst)
    {
      if (instances[i].mifplus_ctx != NULL)
        free(instances[i].mifplus_ctx);
      instances[i].mifplus_ctx = NULL;
      instances[i].sprox_ctx = NULL;
      return MI_OK;
    }
  }

  return MI_INVALID_READER_CONTEXT;  
}

SPROX_MIFPLUS_CTX_ST *mifplus_get_ctx(SPROX_INSTANCE rInst)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].sprox_ctx == rInst)
      return instances[i].mifplus_ctx;

  return NULL;  
}

#endif
#endif
