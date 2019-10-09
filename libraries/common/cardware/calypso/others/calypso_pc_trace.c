/**h* CalypsoAPI/Calypso_PC_Trace.c
 *
 * NAME
 *   SpringCard Calypso API :: Trace and debugging stuff
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

#if (CALYPSO_TRACE)

#include <stdarg.h>

BYTE calypso_trace = 0;

#ifndef MAX_PATH
  #define MAX_PATH 1024
#endif

static char  trace_file[MAX_PATH] = { 0 };
static FILE *trace_fp    = NULL;

/**f* CSB6_Calypso/CalypsoSetTraceLevel
 *
 * NAME
 *   CalypsoSetTraceLevel
 *
 * DESCRIPTION
 *   Define the verbosity of the DLL
 *
 * INPUTS
 *   BYTE level
 *
 * RETURNS
 *   none
 *
 * NOTES
 *   level is an array of bits :
 *   
 *   ********
 *   +--------- RFU
 *    +-------- trace the transactions
 *     +------- trace the SAM related functions
 *      +------ trace the Card related functions
 *       +----- trace errors
 *        +---- trace function calls
 *         +--- debug the library
 *          +-- debug at APDU level
 *
 **/
CALYPSO_LIB void CALYPSO_API CalypsoSetTraceLevel(BYTE level)
{
  calypso_trace = level;
}

/**f* CSB6_Calypso/CalypsoSetTraceFile
 *
 * NAME
 *   CalypsoSetTraceFile
 *
 * DESCRIPTION
 *   Define the output of the DLL traces
 *
 * INPUTS
 *   const char *filename
 *
 * RETURNS
 *   none
 *
 * NOTES
 *   Use CalypsoSetTraceFile("stdout") to read the output in the console
 *
 **/
CALYPSO_LIB void CALYPSO_API CalypsoSetTraceFile(const char *filename)
{
	if (filename != NULL)
	{
		strncpy(trace_file, filename, MAX_PATH);
    trace_file[MAX_PATH-1] = '\0';
	} else
	  trace_file[0] = '\0';	 
}






static BOOL CalypsoTraceBegin(BYTE level)
{
  char l;

  if (!(calypso_trace & level)) return FALSE;
  if (!strlen(trace_file)) return FALSE;

	if (!strcmp(trace_file, "CON") || !strcmp(trace_file, "stdout"))
	{
		/* Output to console */
		trace_fp = stdout;
	} else
	if (!strcmp(trace_file, "ERR") || !strcmp(trace_file, "stderr"))
	{
		/* Output to console */
		trace_fp = stderr;
	} else
	{
		/* Output to file */
    trace_fp = fopen(trace_file, "at+");
    if (trace_fp == NULL) return FALSE;
  }
  
  switch (level & 0xF0)
  {
    case 0        : l = ' '; break;
    case TR_CARD  : l = 'C'; break;
    case TR_SAM   : l = 'S'; break;
    case TR_TRANS : l = 'T'; break;
    default       : l = '?'; break;
  }

  fprintf(trace_fp, "# %01X %c ", level & 0x0F, l);
  return TRUE;
}

static void CalypsoTraceEnd(void)
{
  fprintf(trace_fp, "\n");

  if ((trace_fp != stdout) && (trace_fp != stderr))
  	fclose(trace_fp);
  else
    fflush(trace_fp);
}

void CalypsoTraceEx(BYTE level, const char *fmt, ...)
{
  va_list arg_ptr;

  if (!CalypsoTraceBegin(level)) return;

  va_start(arg_ptr, fmt);
  vfprintf(trace_fp, fmt, arg_ptr);
  va_end(arg_ptr);   

  CalypsoTraceEnd();
}

void CalypsoTraceStr(BYTE level, const char *info)
{
  if (!CalypsoTraceBegin(level)) return;

  fprintf(trace_fp, "%s", info);

  CalypsoTraceEnd();
}

void CalypsoTraceHex(BYTE level, const char *info, const BYTE data[], SIZE_T size)
{
  SIZE_T i;
  char c = '\0';

  if (!CalypsoTraceBegin(level)) return;

  if (info != NULL)
  {
    c = info[strlen(info)-1];

    if ((c != '=') && (c != ':'))
      fprintf(trace_fp, "%s ", info);
    else
      fprintf(trace_fp, "%s", info);
  }

  for (i=0; i<size; i++)
    fprintf(trace_fp, "%02X", data[i]);

  CalypsoTraceEnd();
}


void CalypsoTraceRC(BYTE level, const char *info, CALYPSO_RC rc)
{
  if (info == NULL)
  {
    CalypsoTraceEx(level, "%04X", rc);
  } else
  {
    CalypsoTraceEx(level, "%s %04X", info, rc);
  }
}

void CalypsoTraceValH(BYTE level, const char *info, DWORD value, BYTE len)
{
  char *s = "";
  
  if (info == NULL)
  {
    info = "";
  } else
  if (strlen(info) > 1)
  {
    char c = info[strlen(info)-1];
    if ((strlen(info) > 2) && (c == ' ')) c--;
    if ((c != '=') && (c != ':'))
      s = "=";
  }

  switch (len)
  {
    case 1  : CalypsoTraceEx(level, "%s%s%01lX", info, s, value); break;
    case 2  : CalypsoTraceEx(level, "%s%s%02lX", info, s, value); break;
    case 3  : CalypsoTraceEx(level, "%s%s%03lX", info, s, value); break;
    case 4  : CalypsoTraceEx(level, "%s%s%04lX", info, s, value); break;
    case 5  : CalypsoTraceEx(level, "%s%s%05lX", info, s, value); break;
    case 6  : CalypsoTraceEx(level, "%s%s%06lX", info, s, value); break;
    case 7  : CalypsoTraceEx(level, "%s%s%07lX", info, s, value); break;
    case 8  : CalypsoTraceEx(level, "%s%s%08lX", info, s, value); break;
    default : CalypsoTraceEx(level, "%s%s%lX",   info, s, value); break;
  }
}

void CalypsoTraceValD(BYTE level, const char *info, DWORD value, BYTE len)
{
  char *s = "";
  
  if (info == NULL)
  {
    info = "";
  } else
  if (strlen(info) > 1)
  {
    char c = info[strlen(info)-1];
    if ((strlen(info) > 2) && (c == ' ')) c--;
    if ((c != '=') && (c != ':'))
      s = "=";
  }

  switch (len)
  {
    case 0  : CalypsoTraceEx(level, "%s%s%ld",   info, s, value); break;
    case 1  : CalypsoTraceEx(level, "%s%s%01lu", info, s, value); break;
    case 2  : CalypsoTraceEx(level, "%s%s%02lu", info, s, value); break;
    case 3  : CalypsoTraceEx(level, "%s%s%03lu", info, s, value); break;
    case 4  : CalypsoTraceEx(level, "%s%s%04lu", info, s, value); break;
    case 5  : CalypsoTraceEx(level, "%s%s%05lu", info, s, value); break;
    case 6  : CalypsoTraceEx(level, "%s%s%06lu", info, s, value); break;
    case 7  : CalypsoTraceEx(level, "%s%s%07lu", info, s, value); break;
    case 8  : CalypsoTraceEx(level, "%s%s%08lu", info, s, value); break;
    default : CalypsoTraceEx(level, "%s%s%lu",   info, s, value); break;
  }
}

#if 0
void trace_begin(const char *msg)
{
  trace_s(msg);
  trace_b(':');
  trace_crlf();
  trace_b(0x09);
}

void trace_end(void)
{
  trace_crlf();
}

void trace_sw(WORD sw)
{
  trace_s("SW=");
  trace_hb((BYTE) (sw / 0x0100));
  trace_hb((BYTE) (sw % 0x0100));
}

void trace_rc(CALYPSO_RC rc)
{
  trace_s("rc=");
  trace_d(rc, 0);
}

void trace_crlf(void)
{
  printf("\n");
}

void trace_s(const char *s)
{
  if (s == NULL) 
    printf("\n");
  else
    printf("%s", s);
}

void trace_b(BYTE b)
{
  printf("%c", b);
}

void trace_hb(BYTE b)
{
  printf("%02X", b);
}

void trace_hw(WORD w)
{
  printf("%04X", w);
}

void trace_hdw(DWORD dw)
{
  printf("%08lX", dw);
}

void trace_d(SDWORD d, BYTE l)
{
  printf("%ld", d);
}

void trace_u(DWORD d, BYTE l)
{
  printf("%lu", d);
}

void trace_h(const BYTE *a, DWORD l, BOOL r)
{
  DWORD i;

  if (r)
  {
    for (i=0; i<l; i++)
      printf("%02X", a[l-i-1]);
  } else
  {
    for (i=0; i<l; i++)
      printf("%02X", a[i]);
  }

}
#endif

#endif
