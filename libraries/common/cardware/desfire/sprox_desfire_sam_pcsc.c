#ifdef SPROX_DESFIRE_WITH_SAM
/**h* DesfireAPI/SamEntries
 *
 * NAME
 *   DesfireAPI :: Using the Desfire card together with a NXP SAM AV2 smartcard
 *
 * COPYRIGHT
 *   (c) 2014 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Low-level functions to bind the library to the SAM.
 *
 **/
#include "sprox_desfire_i.h"

extern INSTANCE_ST instances[MAX_INSTANCES];

SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_AttachSAM(SCARDHANDLE hCard,
                                                                SCARDHANDLE hSam)
{
  BYTE i;

  for (i=0; i<MAX_INSTANCES; i++)
    if ((instances[i].bInited) && (instances[i].hCard == hCard))
      goto attach_to_this_one;

  return SCARD_E_INVALID_HANDLE;

attach_to_this_one:
  instances[i].desfire_ctx->sam_session_active = FALSE;
  instances[i].desfire_ctx->sam_context.bInited = TRUE;
  instances[i].desfire_ctx->sam_context.hSam = hSam;

  return SCARD_S_SUCCESS;

}


SPROX_DESFIRE_LIB LONG SPROX_DESFIRE_API SCardDesfire_DetachSAM(SCARDHANDLE hCard,
                                                                SCARDHANDLE hSam,
                                                                DWORD dwDisposition)
{

  BYTE i;

  for (i=0; i<MAX_INSTANCES; i++)
  {
    if (instances[i].hCard == hCard)
    {
      if (instances[i].desfire_ctx != NULL)
      {
        instances[i].desfire_ctx->sam_session_active  = FALSE;
        instances[i].desfire_ctx->sam_context.bInited = FALSE;
        instances[i].desfire_ctx->sam_context.hSam    = 0;

        return SCARD_S_SUCCESS;

      }
    }
  }

  return SCARD_E_INVALID_HANDLE;
}

#endif
