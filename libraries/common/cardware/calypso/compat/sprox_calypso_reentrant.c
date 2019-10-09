#include "sprox_calypso_i.h"

SPROX_CALYPSO_CTX_ST calypso_ctx =
{
  CALYPSO_ISO14443_SLOT, // BYTE card_slot;
  1,                     // BYTE sam_slot;
  10,                    // BYTE pcd_addr;
  10,                    // BYTE picc_addr;
  1,                     // BYTE picc_seq;
  FALSE,                 // BOOL in_session;
  0xFFFF,                // WORD card_sw;
  0xFFFF                 // WORD sam_sw;     
};

#ifdef SPROX_API_REENTRANT

#define MAX_INSTANCES 16

typedef struct
{
  SPROX_INSTANCE       sprox_ctx;
  SPROX_CALYPSO_CTX_ST calypso_ctx;

} INSTANCE_ST;

INSTANCE_ST instances[MAX_INSTANCES] = { 0 };

/*
 * Calypso higher level functions
 * ------------------------------
 */
//static BYTE calypso_slot = CALYPSO_ISO14443_SLOT;
//static BYTE calypso_pcd_addr = 10;
//static BYTE calypso_picc_addr = 10;
//static BOOL calypso_in_session = FALSE;
//static WORD ctx->card_sw = 0xFFFF;
//static BYTE ctx->sam_slot = 1;
//static WORD ctx->sam_sw = 0xFFFF;

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_AttachCalypsoLibrary(SPROX_INSTANCE rInst)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].sprox_ctx == rInst)
      goto take_this_one;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].sprox_ctx == NULL)
      goto take_this_one;
  
  return CALYPSO_TOO_MANY_INST;
  
take_this_one:
  instances[i].sprox_ctx = rInst;
  memcpy(&instances[i].calypso_ctx, &calypso_ctx, sizeof(SPROX_CALYPSO_CTX_ST));  
  return MI_OK;
}

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_DetachCalypsoLibrary(SPROX_INSTANCE rInst)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
  {
    if (instances[i].sprox_ctx == rInst)
    {
      instances[i].sprox_ctx = NULL;
      return MI_OK;
    }
  }

  return CALYPSO_NO_SUCH_INST;  
}

SPROX_CALYPSO_CTX_ST *calypso_get_ctx(SPROX_INSTANCE rInst)
{
  BYTE i;
  
  for (i=0; i<MAX_INSTANCES; i++)
    if (instances[i].sprox_ctx == rInst)
      return &instances[i].calypso_ctx;

  return NULL;  
}

#endif
