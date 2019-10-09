#ifndef __CALYPSO_CARD_COMMANDS_I_H__
#define __CALYPSO_CARD_COMMANDS_I_H__

#ifdef CALYPSO_TRACE
  #define RETURN(n)   if (rc) CalypsoTraceRC(TR_DEBUG|TR_TRACE|TR_CARD, n " err.", rc); return rc
#else
  #define RETURN(n)   return rc
#endif

CALYPSO_RC CalypsoCardSetSW(CALYPSO_CTX_ST *ctx, SIZE_T recv_len);
CALYPSO_RC CalypsoCardGetResponse(CALYPSO_CTX_ST *ctx, SIZE_T *recv_len);

#endif
