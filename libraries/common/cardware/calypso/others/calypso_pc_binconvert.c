#include "../calypso_api_i.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

char *btoh(char *s, BYTE b)
{
  register BYTE c;
  
  if (s == NULL) return s;
        
  c = b / 0x10;
  if (c < 0x0A) c += 48; else c += 55;
  s[0] = c;
  
  c = b % 0x10;
  if (c < 0x0A) c += 48; else c += 55;
  s[1] = c;
  
  s[2] = '\0';  
  return &s[2];
}

char *wtoh(char *s, WORD w)
{
  s = btoh(s, (BYTE) (w / 0x0100));
  s = btoh(s, (BYTE) (w % 0x0100));
  return s;
}

char *dwtoh(char *s, DWORD dw)
{
  s = wtoh(s, (WORD) (dw / 0x00010000));
  s = wtoh(s, (WORD) (dw % 0x00010000));
  return s;
}

BOOL ishexq(const char q)
{
  if ( ((q >= '0') && (q <= '9'))
    || ((q >= 'A') && (q <= 'F'))
    || ((q >= 'a') && (q <= 'f'))) return TRUE;
  return FALSE;  
}

BOOL ishexb(const char s[2])
{
  if (s == NULL) return FALSE;
  
  return (ishexq(s[0]) && ishexq(s[1]));
}

void atoh(char *s, const BYTE *data, WORD len)
{
  WORD i;
  
  for (i=0; i<len; i++)
  {
    BYTE c;
    c = data[i] / 0x10;
    if (c < 0x0A) c += 48; else c += 55;
    s[2*i + 0] = c;
    c = data[i] % 0x10;
    if (c < 0x0A) c += 48; else c += 55;
    s[2*i + 1] = c;
    s[2*i + 2] = '\0';    
  }
}

BYTE htoq(char q)
{
  return (((q >= '0') && (q <= '9')) ? (q - '0') : (((q >= 'A') && (q <= 'F')) ? (q + 10 - 'A') : (((q >= 'a') && (q <= 'f')) ? (q + 10 - 'a') : 0)));
}

BYTE htob(const char s[2])
{
  register BYTE i;
  register BYTE r = 0;
  
  for (i=0; i<2; i++)
  {
    r <<= 4;
    r  |= htoq(s[i]);
  }
  
  return r;
}

WORD htow(const char s[4])
{
  register BYTE i;
  register WORD r = 0;
  
  for (i=0; i<4; i++)
  {
    r <<= 4;
    r  |= htoq(s[i]);
  }
  
  return r;
}

DWORD htodw(const char s[8])
{
  register BYTE  i;
  register DWORD r = 0;
  
  for (i=0; i<8; i++)
  {
    r <<= 4;
    r  |= htoq(s[i]);
  }
  
  return r;
}

BYTE dtob2(const char s[2])
{
  return (htoq(*(s + 0)) * 10
        + htoq(*(s + 1)));
}

WORD dtow4(const char s[4])
{
  return (htoq(*(s + 0)) * 1000
        + htoq(*(s + 1)) * 100
        + htoq(*(s + 2)) * 10
        + htoq(*(s + 3)) );
}

WORD hstob(const char *str, BYTE *data, WORD size)
{
  WORD len = 0;
  WORD offset = 0;
  
  /* Left trim */
  while ((str[offset] == '=') || (str[offset] == ':') || (str[offset] == ' ') || (str[offset] == '\t'))
    offset += 1;

  if (str[offset] == '@')
  {
    /* ASCII mode ? */
    offset += 1;
    while (str[offset] != '\0')
    {
      data[len++] = str[offset];
      if (len >= size) break;
      offset += 1;
    }
  } else
  {
    /* Hexadecimal mode */
    while ((str[offset] != '\0') && (str[offset+1] != '\0'))
    {
      data[len++] = htob(&str[offset]);
      if (len >= size) break;
      offset += 2;
      while ((str[offset] == ' ') || (str[offset] == '\t') || (str[offset] == '.')  || (str[offset] == ':'))
        offset += 1;
    }
  }
  
  return len;
}

void btod2(char s[2], BYTE b)
{
  if (b > 100)
  {
    s[0] = 'F';  
    s[1] = 'F';
  } else
  {
    s[0] = '0' + b / 10;
    s[1] = '0' + b % 10;
  }
  s[2] = '\0';
}

BYTE to_bcd(BYTE b)
{
  register BYTE r = 0;
  r  = (b / 10) << 4;
  r += (b % 10);
  return r;  
}

BYTE bcdtob(BYTE b)
{
  register BYTE r = 0;
  r += 10 * ((b >> 4) & 0x0F);
  r += (b & 0x0F);
  return r;
}

WORD bcdtow(WORD w)
{
  register WORD r = 0;
  r += 1000 * ((w >> 12) & 0x000F);
  r += 100 * ((w >> 8) & 0x000F);
  r += 10 * ((w >> 4) & 0x000F);
  r += (w & 0x000F);
  return r;  
}

DWORD bcdtodw(DWORD dw)
{
  register DWORD r = 0;
  r += 1000000 * ((dw >> 24) & 0x0000000F);
  r += 100000 * ((dw >> 20) & 0x0000000F);
  r += 10000 * ((dw >> 16) & 0x0000000F);
  r += 1000 * ((dw >> 12) & 0x0000000F);
  r += 100 * ((dw >> 8) & 0x0000000F);
  r += 10 * ((dw >> 4) & 0x0000000F);
  r += (dw & 0x0000000F);
  return r;  
}

BOOL isbcdb(BYTE b)
{
  if ((b & 0x0F) > 0x09) return FALSE;
  if ((b & 0xF0) > 0x90) return FALSE;
  return TRUE;
}
