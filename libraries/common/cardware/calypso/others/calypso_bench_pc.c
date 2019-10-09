/**h* CalypsoAPI/Calypso_PC_Benchmark.c
 *
 * NAME
 *   SpringCard Calypso API :: Benchmarking stuff
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

static DWORD ReadInterval(void);
static void  ResetInterval(void);

/**f* CSB6_Calypso/CalypsoBench
 *
 * NAME
 *   CalypsoBench
 *
 * DESCRIPTION
 *   Returns an interval milliseconds. If the reset parameter is set, the interval counter is cleared
 *
 * INPUTS
 *   BOOL reset
 *
 * RETURNS
 *   The number of milliseconds since the last call to CalypsoBench(TRUE)
 *
 **/
CALYPSO_LIB DWORD CALYPSO_API CalypsoBench(BOOL reset)
{
  DWORD r;
  r = ReadInterval();
  if (reset)
    ResetInterval();
  return r;
}

#ifdef WIN32

static LARGE_INTEGER Before = { 0 };

void ResetInterval(void)
{
  QueryPerformanceCounter(&Before);
}

DWORD ReadInterval(void)
{
  DWORD r;
  LARGE_INTEGER Now, Freq;

  QueryPerformanceCounter(&Now);

  QueryPerformanceFrequency(&Freq);
  Freq.QuadPart /= 1000;

  r = (DWORD) ( (Now.QuadPart - Before.QuadPart) / Freq.QuadPart );
  return r;
}

#endif

#ifdef __linux__

#include <sys/time.h>
#include <unistd.h>

static struct timeval Before;

void ResetInterval(void)
{
  struct timezone TZ;

  gettimeofday(&Before, &TZ);
}

DWORD ReadInterval(void)
{
  DWORD r;
  struct timeval  Now;
  struct timezone TZ;

  gettimeofday(&Now, &TZ);

  r  = Now.tv_sec * 1000;
  r += Now.tv_usec / 1000;
  r -= Before.tv_sec * 1000;
  r -= Before.tv_usec / 1000;

  return r;
}

#endif
