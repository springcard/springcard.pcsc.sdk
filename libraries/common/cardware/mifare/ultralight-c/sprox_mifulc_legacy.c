/**h* MifUlCAPI/Legacy
 *
 * NAME
 *   MifUlCAPI :: Specific entry points for Legacy re-entrant and non-reentrant DLL
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * SEE ALSO
 *   LegacyEX
 *
 **/

#include "sprox_mifulc_i.h"

#ifndef _USE_PCSC

SPROX_RC MifUlC_Command(SPROX_PARAM  const BYTE send_buffer[], WORD send_length, BYTE recv_buffer[], WORD *recv_length)
{
  SWORD rc;
  WORD l_recv_length;
  BYTE dummy;
  BYTE *l_recv_buffer;
  
  if ((recv_buffer == NULL) && (recv_length == NULL))
  {
    l_recv_buffer = &dummy;
    l_recv_length = 0;
  } else
  {
    l_recv_buffer = recv_buffer;
    l_recv_length = *recv_length;
  }

#if (SPROX_INS_LIB_TRACE)
  if (trace_level & TRACE_LEVEL_USER_1)
  {
    TRACE_S(TRACE_MICORE, "MifUlC<");
    trace_h(send_buffer, send_length, FALSE);
    TRACE_CRLF(TRACE_MICORE);
  }
#endif

  send_length += 2;

  rc = SPROX_API_CALL(A_Exchange) (SPROX_PARAM_P  send_buffer, send_length, l_recv_buffer, &l_recv_length, TRUE, 1024);

  if ((rc == MI_OK) && (recv_length != NULL))
  {
#if (SPROX_INS_LIB_TRACE)
    if (trace_level & TRACE_LEVEL_USER_1)
    {
      TRACE_S(TRACE_MICORE, "MifUlC>");
      trace_h(l_recv_buffer, l_recv_length, FALSE);
      TRACE_CRLF(TRACE_MICORE);
    }
#endif

    if (l_recv_length >= 2)
    {
      l_recv_length -= 2;
    } else
    {
      rc = MI_EMPTY;
    }
    
    *recv_length = l_recv_length;
  }

  return rc;
}

#endif
