/**h* DesfireAPI/LegacyEX
 *
 * NAME
 *   DesfireAPI :: Specific entry points for Legacy re-entrant DLL
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * SEE ALSO
 *   Legacy
 *
 **/
#include "sprox_desfire_i.h"

#ifndef _USE_PCSC
#ifdef SPROX_API_REENTRANT

#define MAX_INSTANCES 16

typedef struct
{
  SPROX_INSTANCE        sprox_ctx;
  SPROX_DESFIRE_CTX_ST *desfire_ctx;

} INSTANCE_ST;

INSTANCE_ST instances[MAX_INSTANCES] = { 0 };

/**f* DesfireAPI/[Legacy]AttachLibrary
 *
 * NAME
 *   [Legacy]AttachLibrary
 *
 * DESCRIPTION
 *   Associates the Desfire DLL with an instance of SpringProx re-entrant DLL.
 *   Call this function immediately after SPROXx_ReaderOpen to be able to
 *   work with any Desfire card on this SpringProx/CSB reader.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   Not applicable.
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_AttachDesfireLibrary(SPROX_INSTANCE rInst);
 *
 *   [[pcsc_desfire.dll]]
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
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_AttachDesfireLibrary(SPROX_INSTANCE rInst)
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

  instances[i].desfire_ctx = malloc(sizeof(SPROX_DESFIRE_CTX_ST));
  if (instances[i].desfire_ctx == NULL)
    return MI_OUT_OF_MEMORY_ERROR;   
  instances[i].sprox_ctx = rInst;
  
  memset(instances[i].desfire_ctx, 0, sizeof(SPROX_DESFIRE_CTX_ST));
  instances[i].desfire_ctx->tcl_cid = 0xFF;
  instances[i].desfire_ctx->session_type = KEY_EMPTY;  
  instances[i].desfire_ctx->iso_wrapping = DF_ISO_WRAPPING_OFF;
  
  return MI_OK;
}

/**f* DesfireAPI/[Legacy]DetachLibrary
 *
 * NAME
 *   [Legacy]DetachLibrary
 *
 * DESCRIPTION
 *   Remove the attachement between the Desfire DLL and an instance
 *   of SpringProx re-entrant DLL.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   Not applicable.
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_DetachDesfireLibrary(SPROX_INSTANCE rInst);
 *
 *   [[pcsc_desfire.dll]]
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
SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_DetachDesfireLibrary(SPROX_INSTANCE rInst)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
  {
    if (instances[i].sprox_ctx == rInst)
    {
      if (instances[i].desfire_ctx != NULL)
        free(instances[i].desfire_ctx);
      instances[i].desfire_ctx = NULL;
      instances[i].sprox_ctx = NULL;
      return MI_OK;
    }
  }

  return MI_INVALID_READER_CONTEXT;  
}

SPROX_DESFIRE_CTX_ST *desfire_get_ctx(SPROX_INSTANCE rInst)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].sprox_ctx == rInst)
      return instances[i].desfire_ctx;

  return NULL;  
}

#endif
#endif
