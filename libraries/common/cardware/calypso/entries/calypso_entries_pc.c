/**h* CalypsoAPI/Calypso_PC_Entries.c
 *
 * NAME
 *   SpringCard Calypso API :: Context creation and destroy
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release
 *
 **/
#include "../calypso_api_i.h"

/**f* CSB6_Calypso/CalypsoCreateContext
 *
 * NAME
 *   CalypsoCreateContext
 *
 * DESCRIPTION
 *   Instanciate a new context for the library
 *
 * INPUTS
 *   none
 *
 * RETURNS
 *   a pointer on the allocated context, NULL upon error
 *
 * SEE ALSO
 *   CalypsoDestroyContext
 *
 **/
CALYPSO_LIB CALYPSO_CTX_ST * CALYPSO_API CalypsoCreateContext(void)
{
  CALYPSO_CTX_ST *ctx;

  ctx = malloc(sizeof(CALYPSO_CTX_ST));
  if (ctx != NULL)
  {
    memset(ctx, 0, sizeof(CALYPSO_CTX_ST));
  }

  return ctx;
}

/**f* CSB6_Calypso/CalypsoDestroyContext
 *
 * NAME
 *   CalypsoDestroyContext
 *
 * DESCRIPTION
 *   Destroy a context allocated by CalypsoCreateContext
 *
 * INPUTS
 *   P_CSB6_CALYPSO_CTX  p_ctx          : the context
 *
 * RETURNS
 *   none
 *
 **/
CALYPSO_LIB void CALYPSO_API CalypsoDestroyContext(CALYPSO_CTX_ST *ctx)
{
  if (ctx != NULL)
  {
#if (CALYPSO_WITH_XML)
    CalypsoClearOutput(ctx);
#endif
    CalypsoCardDispose(ctx);
#if (CALYPSO_WITH_SAM)
    CalypsoSamDispose(ctx);
#endif
    free(ctx);
  }
}

/**f* CSB6_Calypso/CalypsoCleanupContext
 *
 * NAME
 *   CalypsoCleanupContext
 *
 * DESCRIPTION
 *   Cleanup a context allocated by CalypsoCreateContext
 *
 * INPUTS
 *   P_CSB6_CALYPSO_CTX  p_ctx          : the context
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 **/
CALYPSO_PROC CalypsoCleanupContext(P_CALYPSO_CTX ctx)
{
  if (ctx == NULL)
    return CALYPSO_ERR_INVALID_CONTEXT;

#if (CALYPSO_WITH_XML)
  CalypsoClearOutput(ctx);
#endif
  CalypsoCardDispose(ctx);
#if (CALYPSO_WITH_SAM)
  CalypsoSamDispose(ctx);
#endif
  memset(ctx, 0, sizeof(CALYPSO_CTX_ST));
  
  return CALYPSO_SUCCESS;
}

/**f* CSB6_Calypso/CalypsoCardDispose
 *
 * NAME
 *   CalypsoCardDispose
 *
 * DESCRIPTION
 *   Terminate all pending actions on a Calypso card correctly ;
 *   this must be the last function called before shutting down the card
 *   (the caller must still call SCardDisconnect)
 *
 * INPUTS
 *   P_CSB6_CALYPSO_CTX  p_ctx          : library context
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 **/
CALYPSO_PROC CalypsoCardDispose(CALYPSO_CTX_ST *ctx)
{ 
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;  
  return 0;
}

/**f* CSB6_Calypso/CalypsoSamDispose
 *
 * NAME
 *   CalypsoSamDispose
 *
 * DESCRIPTION
 *   Terminate all pending actions on a Calypso card correctly ;
 *   this must be the last function called before shutting down the card
 *   (the caller must still call SCardDisconnect)
 *
 * INPUTS
 *   P_CSB6_CALYPSO_CTX  p_ctx          : library context
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 **/
#if (CALYPSO_WITH_SAM)
CALYPSO_PROC CalypsoSamDispose(CALYPSO_CTX_ST *ctx)
{ 
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;  
  return 0;
}
#endif
