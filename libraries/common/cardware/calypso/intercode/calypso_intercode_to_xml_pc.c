/**h* CalypsoAPI/calypso_intercode_to_xml_ins.c
 *
 * NAME
 *   calypso_intercode_to_xml_ins.c
 *
 * DESCRIPTION
 *   Translation of INTERCODE types to XML, large footprint implementation for PC
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

#define DUMMY_DATE "??" "/" "??" "/" "????"
#define DUMMY_TIME "??" ":" "??" ":" "??"

#include <stdarg.h>

static void ParserPrintV(CALYPSO_CTX_ST *ctx, const char *fmt, va_list arg_ptr)
{
  if (ctx->Parser.TargetFile != NULL)
  {
    vfprintf(ctx->Parser.TargetFile, fmt, arg_ptr);
  } else
  if (ctx->Parser.TargetString != NULL)
  {
    char buffer[512+1];
    vsprintf(buffer, fmt, arg_ptr);
    if ((strlen(buffer) + strlen(ctx->Parser.TargetString)) < ctx->Parser.TargetLength)
      strcat(ctx->Parser.TargetString, buffer);
  } else
  {
    vprintf(fmt, arg_ptr);
  }
}

static void ParserPrint(CALYPSO_CTX_ST *ctx, const char *fmt, ...)
{
  va_list arg_ptr;

  va_start(arg_ptr, fmt);
  ParserPrintV(ctx, fmt, arg_ptr);
  va_end(arg_ptr);   
}

static void ParserOutBefore(CALYPSO_CTX_ST *ctx, const char *varname)
{
  DWORD i;

  if (ctx->Parser.OutputXml)
  {
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      ParserPrint(ctx, "  ");
    ParserPrint(ctx, "<%s>", varname);
  }
  if (ctx->Parser.OutputIni)
  {
    ParserPrint(ctx, "%s=", varname);
  }
}

static void ParserOutBeforeId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid)
{
  DWORD i;

  if (ctx->Parser.OutputXml)
  {
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      ParserPrint(ctx, "  ");
    ParserPrint(ctx, "<%s id=\"%ld\">", varname, varid);
  }
  if (ctx->Parser.OutputIni)
  {
    ParserPrint(ctx, "%s.%ld=", varname, varid);
  }
}

static void ParserOutAfter(CALYPSO_CTX_ST *ctx, const char *varname)
{
  if (ctx->Parser.OutputXml)
  {
    ParserPrint(ctx, "</%s>", varname);
    ParserPrint(ctx, "\n");
  }
  if (ctx->Parser.OutputIni)
  {
    ParserPrint(ctx, "\n");
  }
}


static void ParserOut_VarEx(CALYPSO_CTX_ST *ctx, const char *varname, const char *fmt, ...)
{
  va_list arg_ptr;

  ParserOutBefore(ctx, varname);

  if ((ctx->Parser.OutputXml) || (ctx->Parser.OutputIni))
  {
    va_start(arg_ptr, fmt);
    ParserPrintV(ctx, fmt, arg_ptr);
    va_end(arg_ptr);   
  }

  ParserOutAfter(ctx, varname);
}

static void ParserOut_VarIdEx(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, const char *fmt, ...)
{
  va_list arg_ptr;

  ParserOutBeforeId(ctx, varname, varid);

  if ((ctx->Parser.OutputXml) || (ctx->Parser.OutputIni))
  {
    va_start(arg_ptr, fmt);
    ParserPrintV(ctx, fmt, arg_ptr);
    va_end(arg_ptr);   
  }

  ParserOutAfter(ctx, varname);
}

void ParserOut_Hex(CALYPSO_CTX_ST *ctx, const char *varname, const BYTE value[], SIZE_T size)
{
  DWORD i;

  ParserOutBefore(ctx, varname);

  if ((ctx->Parser.OutputXml) || (ctx->Parser.OutputIni))
  {
    for (i=0; i<size; i++)
      ParserPrint(ctx, "%02X", value[i]);
  }

  ParserOutAfter(ctx, varname);
}

void ParserOut_Bin(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value, BYTE bits)
{
  BYTE i;
  char t[32+1];

  if (bits>32) bits=32;
  for (i=0; i<bits; i++)
  {
    if (value & 0x00000001)
      t[bits-i-1] = '1';
    else
      t[bits-i-1] = '0';
    value >>= 1;
  }
  t[bits] = '\0';


  ParserOut_VarEx(ctx, varname, "%s", t);
}

void ParserOut_Dec(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  ParserOut_VarEx(ctx, varname, "%ld", value);
}

void ParserOut_DecId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, DWORD value)
{
  ParserOutBeforeId(ctx, varname, varid);
  ParserPrint(ctx, "%ld", value);
  ParserOutAfter(ctx, varname);
}

void ParserOut_Hex4(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  ParserOut_VarEx(ctx, varname, "%01X", value & 0x0F);
}

void ParserOut_Hex8(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  ParserOut_VarEx(ctx, varname, "%02X", value & 0x0FF);
}

void ParserOut_Hex12(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  ParserOut_VarEx(ctx, varname, "%03X", value & 0x0FFF);
}

void ParserOut_Hex16(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  ParserOut_VarEx(ctx, varname, "%04X", value & 0x0FFFF);
}

void ParserOut_Hex24(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  ParserOut_VarEx(ctx, varname, "%06lX", value & 0x0FFFFFF);
}

void ParserOut_Hex32(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  ParserOut_VarEx(ctx, varname, "%08lX", value);
}

void ParserOut_IdfZones(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  char temp[33];
  BYTE i;

  memset(temp, '\0', sizeof(temp));

  for (i=0; i<8; i++)
  {
    if (!value) break;

    if (value & 0x00000001)
    {
      if (i < 10)
        temp[i] = '1' + i;
      else
        temp[i] = 'A' + i - 11;
    } else
      temp[i] = '-';

    value >>= 1;
  }

  ParserOut_VarEx(ctx, varname, temp);
}

void ParserOut_Str(CALYPSO_CTX_ST *ctx, const char *varname, const char *value)
{
  if (ctx->Parser.OutputXml)
  {
    const unsigned char *p = (unsigned char *) value;

    ParserOutBefore(ctx, varname);

    while (*p != '\0')
    {
      if ((*p >= ' ') && (*p <= 0x7F) && (*p != '"') && (*p != '<') && (*p != '>') && (*p != '&'))
      {
        ParserPrint(ctx, "%c", (char) *p);
      } else
      {
        ParserPrint(ctx, "%%%02x", *p);
      }
      p++;
    }
      
    ParserOutAfter(ctx, varname);
  } else
    ParserOut_VarEx(ctx, varname, value);
}

#ifndef CALYPSO_NO_TIME_T

void ParserOut_Date(CALYPSO_CTX_ST *ctx, const char *varname, time_t value)
{
  char temp[64];
  struct tm *p_tm;
  p_tm = localtime(&value);
  if (p_tm != NULL)
    strftime(temp, sizeof(temp), "%d/%m/%Y", p_tm);
  else
    strcpy(temp, DUMMY_DATE);
  ParserOut_VarEx(ctx, varname, temp);
}

void ParserOut_DateId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, time_t value)
{
  char temp[64];
  struct tm *p_tm;
  p_tm = localtime(&value);
  if (p_tm != NULL)
    strftime(temp, sizeof(temp), "%d/%m/%Y", p_tm);
  else
    strcpy(temp, DUMMY_DATE);
  ParserOut_VarIdEx(ctx, varname, varid, temp);
}

void ParserOut_Time(CALYPSO_CTX_ST *ctx, const char *varname, time_t value)
{
  char temp[64];
  struct tm *p_tm;
  p_tm = localtime(&value);
  if (p_tm != NULL)
    strftime(temp, sizeof(temp), "%H:%M:%S", p_tm);
  else
    strcpy(temp, DUMMY_TIME);
  ParserOut_VarEx(ctx, varname, temp);
}

void ParserOut_TimeReal(CALYPSO_CTX_ST *ctx, const char *varname, time_t value)
{
  char temp[64];
  struct tm *p_tm;
  p_tm = localtime(&value);
  if (p_tm != NULL)
    strftime(temp, sizeof(temp), "%d/%m/%Y %H:%M:%S", p_tm);
  else
    strcpy(temp, DUMMY_DATE " " DUMMY_TIME);
  ParserOut_VarEx(ctx, varname, temp);
}

#else

void  ParserOut_Date(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value)
{
  char temp[64];
  sprintf(temp, "%02d/%02d/%04d", value->_date.day, value->_date.month, value->_date.year);
  ParserOut_VarEx(ctx, varname, temp);	
}

void  ParserOut_DateId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, _DATETIME_ST *value)
{
  char temp[64];
  sprintf(temp, "%02d/%02d/%04d", value->_date.day, value->_date.month, value->_date.year);
  ParserOut_VarIdEx(ctx, varname, varid, temp);		
}

void  ParserOut_Time(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value)
{
  char temp[64];
  sprintf(temp, "%02d:%02d:%02d", value->_time.hour, value->_time.minute, value->_time.second);
  ParserOut_VarEx(ctx, varname, temp);	
}

void  ParserOut_TimeReal(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value)
{
  char temp[64];
  sprintf(temp, "%02d/%02d/%04d %02d:%02d:%02d", value->_date.day, value->_date.month, value->_date.year, value->_time.hour, value->_time.minute, value->_time.second);
  ParserOut_VarEx(ctx, varname, temp);		
}

#endif


void ParserOut_Remark(CALYPSO_CTX_ST *ctx, const char *remark)
{
  if (ctx->Parser.OutputXml)
  {
    ParserPrint(ctx, "<!-- ");
  }
  if (ctx->Parser.OutputIni)
  {
    ParserPrint(ctx, "; ");
  }

  if ((ctx->Parser.OutputXml) || (ctx->Parser.OutputIni))
  {
    ParserPrint(ctx, remark);
  }

  if (ctx->Parser.OutputXml)
  {
    ParserPrint(ctx, " -->\n");
  }
  if (ctx->Parser.OutputIni)
  {
    ParserPrint(ctx, "\n");
  }
}

void ParserOut_SectionBegin(CALYPSO_CTX_ST *ctx, const char *sectionname)
{
  DWORD i;

  if (ctx->Parser.OutputXml)
  {
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      ParserPrint(ctx, "  ");
    ParserPrint(ctx, "<%s>\n", sectionname);
  }
  if (ctx->Parser.OutputIni)
  {
    if (ctx->Parser.SectionDepth > 0)
    {
      ParserOut_Remark(ctx, "Nested sections not supported by INI format.");
      ParserOut_Remark(ctx, "Didn't we forgot to close previous section ?");
    }

    ParserPrint(ctx, "\n[%s]\n", sectionname);
  }

  ctx->Parser.SectionDepth++;
}

void ParserOut_SectionBeginId(CALYPSO_CTX_ST *ctx, const char *sectionname, DWORD sectionid)
{
  DWORD i;

  if (ctx->Parser.OutputXml)
  {
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      ParserPrint(ctx, "  ");
    ParserPrint(ctx, "<%s id=\"%ld\">\n", sectionname, sectionid);
  }
  if (ctx->Parser.OutputIni)
  {
    if (ctx->Parser.SectionDepth > 1)
    {
      ParserOut_Remark(ctx, "Nested sections not supported by INI format.");
      ParserOut_Remark(ctx, "Didn't we forgot to close previous section ?");
    }

    ParserPrint(ctx, "\n[%s.%ld]\n", sectionname, sectionid);
  }

  ctx->Parser.SectionDepth++;
}

void ParserOut_SectionEnd(CALYPSO_CTX_ST *ctx, const char *sectionname)
{
  DWORD i;

  if (ctx->Parser.SectionDepth)
    ctx->Parser.SectionDepth--;

  if (ctx->Parser.OutputXml)
  {
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      ParserPrint(ctx, "  ");
    ParserPrint(ctx, "</%s>\n", sectionname);
  }
  if (ctx->Parser.OutputIni)
  {
    ParserPrint(ctx, "\n");
  }
}

void ParserOut_SectionEndId(CALYPSO_CTX_ST *ctx, const char *sectionname, DWORD sectionid)
{
  DWORD i;

  if (ctx->Parser.SectionDepth)
    ctx->Parser.SectionDepth--;

  if (ctx->Parser.OutputXml)
  {
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      ParserPrint(ctx, "  ");
    ParserPrint(ctx, "</%s> <!-- id=\"%ld\" -->\n", sectionname, sectionid);
  }
  if (ctx->Parser.OutputIni)
  {
    ParserPrint(ctx, "\n");
  }
}

/**f* CSB6_Calypso/CalypsoSetXmlOutput
 *
 * NAME
 *   CalypsoSetXmlOutput
 *
 * DESCRIPTION
 *   Instanciate a parser object with XML output
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   const char     *filename : target XML file
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 * SEE ALSO
 *   CalypsoClearOutput
 *   CalypsoSetXmlOutputStr
 *
 **/
CALYPSO_PROC CalypsoSetXmlOutput(CALYPSO_CTX_ST *ctx, const char *filename)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoClearOutput(ctx);

  if (filename != NULL)
  {
    ctx->Parser.TargetFile = fopen(filename, "wt");
    if (ctx->Parser.TargetFile == NULL) return CALYPSO_ERR_INVALID_PARAM;
  }
  ctx->Parser.OutputXml = TRUE;

  ParserOut_SectionBegin(ctx, "calypso_card");
  return 0;
}

/**f* CSB6_Calypso/CalypsoSetXmlOutputStr
 *
 * NAME
 *   CalypsoSetXmlOutputStr
 *
 * DESCRIPTION
 *   Same as CalypsoSetXmlOutput, but the XML output is constructed in memory instead of
 *   being written in a file
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   char           *target   : buffer to receive the XML text
 *   DWORD          length    : size of target buffer
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 * SEE ALSO
 *   CalypsoClearOutput
 *
 **/
CALYPSO_PROC CalypsoSetXmlOutputStr(CALYPSO_CTX_ST *ctx, char *target, SIZE_T length)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoClearOutput(ctx);

  if (target == NULL) return CALYPSO_ERR_INVALID_PARAM;
  if (length < 1) return CALYPSO_ERR_INVALID_PARAM;

  memset(target, '\0', length);

  ctx->Parser.TargetString = target;
  ctx->Parser.TargetLength = length;
  ctx->Parser.OutputXml = TRUE;

  return 0;
}


/**f* CSB6_Calypso/CalypsoSetIniOutput
 *
 * NAME
 *   CalypsoSetIniOutput
 *
 * DESCRIPTION
 *   Instanciate a parser object with INI output
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   const char     *filename : target INI file
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 * SEE ALSO
 *   CalypsoClearOutput
 *   CalypsoSetIniOutputStr
 *
 **/
CALYPSO_PROC CalypsoSetIniOutput(CALYPSO_CTX_ST *ctx, const char *filename)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoClearOutput(ctx);

  if (filename != NULL)
  {
    ctx->Parser.TargetFile = fopen(filename, "wt");
    if (ctx->Parser.TargetFile == NULL) return CALYPSO_ERR_INVALID_PARAM;
  }
  ctx->Parser.OutputIni = TRUE;

  return 0;
}

/**f* CSB6_Calypso/CalypsoSetIniOutputStr
 *
 * NAME
 *   CalypsoSetIniOutputStr
 *
 * DESCRIPTION
 *   Same as CalypsoSetIniOutput, but the INI output is constructed in memory instead of
 *   being written in a file
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *   char           *target   : buffer to receive the INI text
 *   DWORD          length    : size of target buffer
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 * SEE ALSO
 *   CalypsoClearOutput
 *
 **/

CALYPSO_PROC CalypsoSetIniOutputStr(CALYPSO_CTX_ST *ctx, char *target, SIZE_T length)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoClearOutput(ctx);

  if (target == NULL) return CALYPSO_ERR_INVALID_PARAM;
  if (length < 1) return CALYPSO_ERR_INVALID_PARAM;

  memset(target, '\0', length);

  ctx->Parser.TargetString = target;
  ctx->Parser.TargetLength = length;
  ctx->Parser.OutputIni = TRUE;

  return 0;
}

/**f* CSB6_Calypso/CalypsoClearOutput
 *
 * NAME
 *   CalypsoClearOutput
 *
 * DESCRIPTION
 *   Terminate the parser created by either CalypsoSetXmlOutput or CalypsoSetIniOutput
 *
 * INPUTS
 *   CALYPSO_CTX_ST *ctx      : library context
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 **/
CALYPSO_PROC CalypsoClearOutput(CALYPSO_CTX_ST *ctx)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  if (ctx->Parser.TargetFile != NULL)
  {
    if (ctx->Parser.OutputXml)
      ParserOut_SectionEnd(ctx, "calypso_card");

    fclose(ctx->Parser.TargetFile);
    ctx->Parser.TargetFile = NULL;
  }

  memset(&ctx->Parser, 0, sizeof(ctx->Parser));
  return 0;
}
