/**h* CalypsoAPI/calypso_intercode_datetime.c
 *
 * NAME
 *   calypso_intercode_datetime.c
 *
 * DESCRIPTION
 *   Translation of INTERCODE date and time types
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release 
 *   JDA 28/12/2009 : minor refactoring
 *
 **/
#include "../calypso_api_i.h"

#ifdef CALYPSO_NO_TIME_T

static _DATETIME_ST tms;

/*
_DATETIME_ST *translate_timereal_30(DWORD value)
{
  return &tms;
}
*/

_DATETIME_ST *translate_date_bcd32(DWORD value)
{
  memset(&tms, 0, sizeof(tms));
  
  tms._date.day    = bcdtob((BYTE) ( value        & 0x000000FF));
  tms._date.month  = bcdtob((BYTE) ((value >> 8)  & 0x000000FF));
  tms._date.year   = bcdtow((WORD) ((value >> 16) & 0x0000FFFF));
  
  return &tms;
}

_DATETIME_ST *translate_datestamp_14(DWORD value)
{
  CalypsoDate14ToTimestamp(&tms, (WORD) value);  
  return &tms;
}

_DATETIME_ST *translate_timestamp_11(DWORD value)
{
  CalypsoTime11ToTimestamp(&tms, (WORD) value);
  return &tms;
}

#else
  
time_t translate_timereal_30(DWORD value)
{
  struct tm d;
  time_t t;
  
  memset(&d, 0, sizeof(d));

  /* EN 1545-1 reference day is 01/01/1997 */
  d.tm_year = 97;
  d.tm_mday = 1;
  d.tm_mon  = 0;

  t  = mktime(&d);

  /* Increment in seconds */
  t += value;

  return t;
}

time_t translate_datestamp_14(DWORD value)
{
  struct tm d;
  time_t t;
  
  memset(&d, 0, sizeof(d));

  if (value > 12053)
  {
    /* A year over 2038 may cause an overflow */
    d.tm_year = 130;
    d.tm_mday = 1;
    d.tm_mon  = 0;

    t  = mktime(&d);

  } else
  {

    /* EN 1545-1 reference day is 01/01/1997 */
    d.tm_year = 97;
    d.tm_mday = 1;
    d.tm_mon  = 0;

    t  = mktime(&d);

    t += value * 3600 * 24;
  }

  return t;
}

time_t translate_timestamp_11(DWORD value)
{
  struct tm d;
  time_t t;
  
  memset(&d, 0, sizeof(d));

  d.tm_year = 97;
  d.tm_mday = 1;
  d.tm_mon  = 0;

  d.tm_hour = value / 60; value %= 60;
  d.tm_min  = value;

  t  = mktime(&d);

  return t;
}

time_t translate_date_bcd32(DWORD value)
{
  int _d, _m, _y;
  struct tm d;
  time_t t;
  
  memset(&d, 0, sizeof(d));
 
  _d = bcdtob((BYTE) ( value        & 0x000000FF));
  if ((_d < 1) || (_d > 31)) _d = 1;

  _m = bcdtob((BYTE) ((value >> 8)  & 0x000000FF));
  if ((_m < 1) || (_m > 12)) _m = 1;

  _y = bcdtow((WORD) ((value >> 16) & 0x0000FFFF));
  if ((_y < 1900) || (_y > 2100)) _y = 2000;

  d.tm_mday = _d;
  d.tm_mon  = _m - 1;
  d.tm_year = _y - 1900;
  
  t  = mktime(&d);
  return t;
}
  
#endif
