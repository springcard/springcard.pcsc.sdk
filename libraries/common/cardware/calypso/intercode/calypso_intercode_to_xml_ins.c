/**h* CalypsoAPI/calypso_intercode_to_xml_ins.c
 *
 * NAME
 *   calypso_intercode_to_xml_ins.c
 *
 * DESCRIPTION
 *   Translation of INTERCODE types to XML, small footprint implementation to be embedded into a SpringProx device
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 14/12/2009 : first public release
 *
 **/
#include "../calypso_api_i.h"

static void ParserOutBefore(CALYPSO_CTX_ST *ctx, const char *varname)
{
  DWORD i;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_TABS))
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      print_f("\t");

  if (ctx->Parser.OutputOptions & PARSER_OUT_DIRTY)
  {
    print_f("%s=", varname);
  } else
  {
    print_f("<%s>", varname);
  }
}

static void ParserOutBeforeId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid)
{
  DWORD i;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_TABS))
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      print_f("\t");
    
  if (ctx->Parser.OutputOptions & PARSER_OUT_DIRTY)
  {   
    print_f("%s(%d)=", varname, varid);
  } else
  {
    print_f("<%s id=\"%d\">", varname, varid);
  }
}

static void ParserOutAfter(CALYPSO_CTX_ST *ctx, const char *varname)
{
  if (ctx->Parser.OutputOptions & PARSER_OUT_DIRTY)
  {
    print_crlf();
  } else
  {
    print_f("</%s>\n", varname);
  }
}


void ParserOut_Hex(CALYPSO_CTX_ST *ctx, const char *varname, const BYTE value[], SIZE_T size)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBefore(ctx, varname);
  print_f("%*H", size, value);
  ParserOutAfter(ctx, varname);
}

void ParserOut_Bin(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value, BYTE bits)
{
  BYTE i;
  char t[32+1];
  
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

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

  ParserOutBefore(ctx, varname);
  print_f(t);
  ParserOutAfter(ctx, varname);
}

void ParserOut_Dec(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBefore(ctx, varname);
  print_f("%d", value);
  ParserOutAfter(ctx, varname);
}

void ParserOut_DecId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, DWORD value)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBeforeId(ctx, varname, varid);
  print_f("%d", value);
  ParserOutAfter(ctx, varname);
}

static void __ParserOut_Hex(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value, BYTE length)
{
  char t[8+1];
  char *p = t;
  
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;
    
  length += 3;
  length /= 4;
  if (length > 8) length = 8;
    
  dwtoh(t, value);
  p = &t[8 - length];

  ParserOutBefore(ctx, varname);
  print_f(p);
  ParserOutAfter(ctx, varname);  
}

void ParserOut_Hex4(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  __ParserOut_Hex(ctx, varname, value, 4);
}

void ParserOut_Hex8(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  __ParserOut_Hex(ctx, varname, value, 8);
}

void ParserOut_Hex12(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  __ParserOut_Hex(ctx, varname, value, 12);
}

void ParserOut_Hex16(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  __ParserOut_Hex(ctx, varname, value, 16);
}

void ParserOut_Hex24(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  __ParserOut_Hex(ctx, varname, value, 24);
}

void ParserOut_Hex32(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  __ParserOut_Hex(ctx, varname, value, 32);
}

void ParserOut_IdfZones(CALYPSO_CTX_ST *ctx, const char *varname, DWORD value)
{
  char t[33];
  BYTE i;

  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  memset(t, '\0', sizeof(t));

  for (i=0; i<8; i++)
  {
    if (!value) break;

    if (value & 0x00000001)
    {
      if (i < 10)
        t[i] = '1' + i;
      else
        t[i] = 'A' + i - 11;
    } else
      t[i] = '-';

    value >>= 1;
  }

  ParserOutBefore(ctx, varname);
  print_f(t);
  ParserOutAfter(ctx, varname);
}

void ParserOut_Str(CALYPSO_CTX_ST *ctx, const char *varname, const char *value)
{
  const char *p = value;

  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBefore(ctx, varname);

  while (*p != '\0')
  {
    if ((*p >= ' ') && (*p <= 0x7F) && (*p != '"') && (*p != '<') && (*p != '>') && (*p != '&'))
    {
      print_f("%c", (char) *p);
    } else
    {
      print_f("%%%02X", (BYTE) *p);
    }
    p++;
  }
    
  ParserOutAfter(ctx, varname);
}

void ParserOut_StrRaw(CALYPSO_CTX_ST *ctx, const char *varname, const char *value)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBefore(ctx, varname);
  print_f(value);    
  ParserOutAfter(ctx, varname);
}

void ParserOut_Remark(CALYPSO_CTX_ST *ctx, const char *remark)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  print_f("<!-- %s -->\n", remark);
}

void ParserOut_Date(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBefore(ctx, varname);
  print_f("%02d/%02d/%04d", value->_date.day, value->_date.month, value->_date.year);
  ParserOutAfter(ctx, varname);
}

void ParserOut_DateId(CALYPSO_CTX_ST *ctx, const char *varname, DWORD varid, _DATETIME_ST *value)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBeforeId(ctx, varname, varid);
  print_f("%02d/%02d/%04d", value->_date.day, value->_date.month, value->_date.year);
  ParserOutAfter(ctx, varname);
}

void ParserOut_Time(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBefore(ctx, varname);
  print_f("%02d:%02d:%02d", value->_time.hour, value->_time.minute, value->_time.second);
  ParserOutAfter(ctx, varname);
}

void ParserOut_TimeReal(CALYPSO_CTX_ST *ctx, const char *varname, _DATETIME_ST *value)
{
  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOutBefore(ctx, varname);
  print_f("%02d/%02d/%04d %02d:%02d:%02d", 
    value->_date.day, value->_date.month, value->_date.year,
    value->_time.hour, value->_time.minute, value->_time.second);
  ParserOutAfter(ctx, varname);
}

void ParserOut_SectionRaw(CALYPSO_CTX_ST *ctx, const char *sectionname, const char *rawcontent)
{
  const char *p = rawcontent;
  BOOL on_new_line = TRUE;

  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOut_SectionBegin(ctx, sectionname);
  
  while (*p != '\0')
  {
    if ((*p >= ' ') && (*p <= 0x7F))
    {
      print_b((char) *p);
      on_new_line = FALSE;
    } else
    if ((*p == '\r') || (*p == '\n'))
    {
      if (!on_new_line)
        print_crlf();
      on_new_line = TRUE;
    } else
    {
      /* Just ignore unallowed chars */
    }
    p++;
  }

  if (!on_new_line)
    print_crlf();
    
  ParserOut_SectionEnd(ctx, sectionname);
}

void ParserOut_SectionXml(CALYPSO_CTX_ST *ctx, const char *sectionname, const char *xmlcontent)
{
  const char *p = xmlcontent;
  DWORD i;
  BOOL on_new_line = TRUE;

  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  ParserOut_SectionBegin(ctx, sectionname);
  
  while (*p != '\0')
  {
    if ((*p >= ' ') && (*p <= 0x7F))
    {
      if (on_new_line)
      {
        if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_TABS))
          for (i=0; i<ctx->Parser.SectionDepth; i++)
            print_b('\t');
      }
    
      print_b((char) *p);
      on_new_line = FALSE;
    } else
    if ((*p == '\r') || (*p == '\n'))
    {
      if (!on_new_line)
        print_crlf();
      on_new_line = TRUE;
    } else
    {
      /* Just ignore unallowed chars */
    }
    p++;
  }

  if (!on_new_line)
    print_crlf();
    
  ParserOut_SectionEnd(ctx, sectionname);
}

void ParserOut_SectionBegin(CALYPSO_CTX_ST *ctx, const char *sectionname)
{
  DWORD i;

  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_TABS))
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      print_b('\t');

  print_f("<%s>\n", sectionname);

  ctx->Parser.SectionDepth++;
}

void ParserOut_SectionBeginId(CALYPSO_CTX_ST *ctx, const char *sectionname, DWORD sectionid)
{
  DWORD i;

  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_TABS))
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      print_b('\t');

  print_f("<%s id=\"%d\">\n", sectionname, sectionid);

  ctx->Parser.SectionDepth++;
}

void ParserOut_SectionEnd(CALYPSO_CTX_ST *ctx, const char *sectionname)
{
  DWORD i;

  if ((ctx == NULL) || !ctx->Parser.OutputXml)
    return;

  if (ctx->Parser.SectionDepth)
    ctx->Parser.SectionDepth--;

  if (!(ctx->Parser.OutputOptions & PARSER_OUT_NO_TABS))
    for (i=0; i<ctx->Parser.SectionDepth; i++)
      print_b('\t');
    
  print_f("</%s>\n", sectionname);
}

CALYPSO_PROC CalypsoSetOutputOptions(P_CALYPSO_CTX ctx, BYTE options)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  ctx->Parser.OutputOptions = options;

  return 0;  
}

CALYPSO_PROC CalypsoSetXmlOutput(CALYPSO_CTX_ST *ctx, const char *dummy)
{
  UNUSED_PARAMETER(dummy);
  
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  CalypsoClearOutput(ctx);

  ctx->Parser.OutputXml = TRUE;

  return 0;
}

CALYPSO_PROC CalypsoClearOutput(CALYPSO_CTX_ST *ctx)
{
  if (ctx == NULL) return CALYPSO_ERR_INVALID_CONTEXT;

  memset(&ctx->Parser, 0, sizeof(ctx->Parser));
  return 0;
}
