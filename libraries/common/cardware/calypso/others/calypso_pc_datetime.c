#include "../calypso_api_i.h"

#ifdef CALYPSO_NO_TIME_T

#ifdef UNDER_CE
BOOL get_datetime(_DATETIME_ST *dst)
{
  if (dst != NULL)
  {
	  SYSTEMTIME sys_time;
	  GetSystemTime(&sys_time);

    dst->_date.year = sys_time.wYear;
    dst->_date.month = (BYTE) sys_time.wMonth;
    dst->_date.day = (BYTE) sys_time.wDay;
    dst->_time.hour = (BYTE) sys_time.wHour;
    dst->_time.minute = (BYTE) sys_time.wMinute;
    dst->_time.second = (BYTE) sys_time.wSecond;
  }
  return TRUE;
}
#endif

BOOL is_date_valid(_DATETIME_ST *dt)
{  
  if (dt == NULL) return FALSE;
      
  if (dt->_date.year < 100)
  {
    if (dt->_date.year > 70)
      dt->_date.year += 1900;
    else
      dt->_date.year += 2000;
  }
  
  if ((dt->_date.month == 0) || (dt->_date.month > 12)) return FALSE;
  if ((dt->_date.day == 0)   || (dt->_date.day   > 31)) return FALSE;  
  return TRUE;    
}

static BOOL _leap_year(WORD year)
{
  return (year % 4) == 0 && ((year % 100) != 0 || (year % 400) == 0);
}

void datetime_next_day(_DATETIME_ST *dt)
{
  dt->_date.day++;
    
  switch (dt->_date.month)
  {     
    case 2 :
      /* February */
      if (_leap_year(dt->_date.year))
      {
        if (dt->_date.day <= 29) return;
      } else
      {
        if (dt->_date.day <= 28) return;
      }
      break;
      
    /* Monthes with 30 days */
    case 4 : case 6 : case 9 : case 11 :
      if (dt->_date.day <= 30) return;
      break;

    /* Monthes with 31 days */
    default:
      if (dt->_date.day <= 31) return;
      break;
  }
    
  dt->_date.day = 1;
  dt->_date.month++;
  if (dt->_date.month > 12)
  {
    dt->_date.month = 1;
    dt->_date.year++;
  }
}

void datetime_prev_day(_DATETIME_ST *dt)
{
  if (dt->_date.day <= 1)
  {
    if (dt->_date.month <= 1)
    {
      /* Previous year */
      dt->_date.year--;
      dt->_date.month = 12;
    } else
    {
      /* Previous month */
      dt->_date.month--;
    }

    switch (dt->_date.month)
    {     
      case 2 :
        /* February */
        if (_leap_year(dt->_date.year))
        {
          dt->_date.day = 29;
        } else
        {
          dt->_date.day = 28;
        }
        break;
        
      /* Monthes with 30 days */
      case 4 : case 6 : case 9 : case 11 :
        dt->_date.day = 30;
        break;
  
      /* Monthes with 31 days */
      default:
        dt->_date.day = 31;
        break;
    }    
  } else
  {
    /* Previous day */
    dt->_date.day--;
  }
}

BOOL CalypsoDate14ToTimestamp(_DATETIME_ST *dst, WORD src)
{
  if (dst == NULL) return FALSE;
  
  dst->_time.hour   = 0;
  dst->_time.minute = 0;
  dst->_time.second = 0;
  
  /* 01/01/1997 */
  dst->_date.day    = 1;
  dst->_date.month  = 1;
  dst->_date.year   = 1997;
  
  /* increment */
  while (src--)
    datetime_next_day(dst);    
  
  return is_date_valid(dst);
}

WORD CalypsoTimestampToDate14(const _DATETIME_ST *src)
{
  _DATETIME_ST timestamp;
  register WORD value = 0;
  
  if (src == NULL) goto done;

  memcpy(&timestamp, src, sizeof(_DATETIME_ST));
  timestamp._time.hour   = 0;
  timestamp._time.minute = 0;
  timestamp._time.second = 0;

  if (!is_date_valid(&timestamp)) goto done;
  
  if (timestamp._date.year < 1997) goto done;
  
  while ((timestamp._date.year  != 1997)
      || (timestamp._date.month != 1)
      || (timestamp._date.day   != 1))
  {
    datetime_prev_day(&timestamp);
    value++;
  }
  
done:
  return value;  
}

BOOL CalypsoTime11ToTimestamp(_DATETIME_ST *dst, WORD src)
{
  if (dst == NULL) return FALSE;

  dst->_time.hour   = (BYTE) (src / 60);
  dst->_time.minute = (BYTE) (src % 60);
  dst->_time.second = 0;
  
  return TRUE;
}

WORD CalypsoTimestampToTime11(const _DATETIME_ST *src)
{
  WORD value = 0;
  
  if (src == NULL) goto done;

  value  = src->_time.hour;
  value *= 60;
  value += src->_time.minute;

done:
  return value;
}

BOOL CalypsoDate14IsBefore(const _DATETIME_ST *now, WORD value)
{
  _DATETIME_ST chk, _now;

  if (now == NULL)
  {
    get_datetime(&_now);
    now = &_now;
  }
  
  CalypsoDate14ToTimestamp(&chk, value);
 
  if (chk._date.year  > now->_date.year) return TRUE;
  if ((chk._date.year == now->_date.year) && (chk._date.month > now->_date.month)) return TRUE;
  if ((chk._date.year == now->_date.year) && (chk._date.month == now->_date.month) && (chk._date.day >= now->_date.day)) return TRUE;

  return FALSE;
}

BOOL CalypsoDate14IsAfter(const _DATETIME_ST *now, WORD value)
{
  _DATETIME_ST chk, _now;

  if (now == NULL)
  {
    get_datetime(&_now);
    now = &_now;
  }

  CalypsoDate14ToTimestamp(&chk, value);
 
  if (chk._date.year  < now->_date.year) return TRUE;
  if ((chk._date.year == now->_date.year) && (chk._date.month < now->_date.month)) return TRUE;
  if ((chk._date.year == now->_date.year) && (chk._date.month == now->_date.month) && (chk._date.day <= now->_date.day)) return TRUE;

  return FALSE;
}

#endif
