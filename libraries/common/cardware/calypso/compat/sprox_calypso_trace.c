#include "../calypso_api_i.h"
#include "sprox_calypso_i.h"

#include <stdarg.h>
#include <time.h>

static TCHAR trace_file[MAX_PATH] = _T("");
static TCHAR last_trace_head = '\0';

SPROX_CALYPSO_LIB void SPROX_CALYPSO_API SPROX_Calypso_SetTraceFile(const TCHAR *filename)
{
	if (filename != NULL)
	{
	  if (_tcscmp(trace_file, filename))
		  sprox_calypso_trace("Trace file set to %s", filename);
		_tcscpy(trace_file, filename);
		sprox_calypso_trace("Trace file set to %s", filename);
	} else
	  trace_file[0] = '\0';	 
}

void sprox_calypso_trace(const char *fmt, ...)
{
#ifdef _UNICODE
  TCHAR   line_u[MAX_PATH];
  size_t  i;
#endif
  va_list arg_ptr;
  char    line_a[MAX_PATH];
  FILE   *file_out;
  TCHAR  *line;

  if (trace_file[0] == '\0') return;  /* No output file        */

	if (!_tcsncmp(trace_file, _T("DLG"), 3))	
	{
		/* Output to message box */
		file_out = NULL;
	} else
	if (!_tcsncmp(trace_file, _T("CON"), 3) || !_tcsncmp(trace_file, _T("stdout"), 6))
	{
		/* Output to console */
		file_out = stdout;
	} else
	if (!_tcsncmp(trace_file, _T("ERR"), 3) || !_tcsncmp(trace_file, _T("stderr"), 6))
	{
		/* Output to console */
		file_out = stderr;
	} else
	{
		/* Output to file */
    file_out = _tfopen(trace_file, _T("at+"));
    if (file_out == NULL) return;
  }

  /* Prepare the line */
  if (fmt != NULL)
  {
    va_start(arg_ptr, fmt);		
    vsprintf(line_a, fmt, arg_ptr);
    va_end(arg_ptr);   
	}
  
#ifdef _UNICODE
  /* Convert the line to unicode if needed */
  for (i = 0; i <= strlen(line_a); i++)
    line_u[i] = line_a[i];
  line = line_u;
#else
  line = line_a;
#endif

  if (file_out == NULL)
  {
#ifdef WIN32
  	/* Message Box */
  	if (fmt != NULL)
  	{
  		MessageBox(0, line, _T("SpringProx Calypso API"), MB_APPLMODAL + MB_TOPMOST);
  	}
#endif
    return;
  }
  
  if (fmt == NULL)
  {
    _ftprintf(file_out, _T("\n"));
    last_trace_head = '\0';   
  } else
  {
    switch (line[0])
    {
      case '+':
      case '-':
        if (last_trace_head != line[0])
        {
          _ftprintf(file_out, _T("\n"));
          last_trace_head = line[0];
        }
        break;
  
      case '>':
      case ':':
      case '.':
        break;
  
      case '_':
        line++;
        break;
  
      default:
        _ftprintf(file_out, _T("\n"));
        last_trace_head = '\0';
        break;
    }
    _ftprintf(file_out, line);
  }

  if ((file_out != stdout) && (file_out != stderr))
  	fclose(file_out);
  else
    fflush(file_out);
}

