#include "../calypso_api_i.h"

#ifndef CALYPSO_NO_DATETIME

BOOL CalypsoDate14ToDate(_DATE_ST *dst, WORD src)
{
  if (dst == NULL) return FALSE;
  
  /* 01/01/1997 */
  dst->day    = 1;
  dst->month  = 1;
  dst->year   = 1997;
  
  /* increment */
  while (src--)
    date_next_day(dst);
  
  return is_date_valid(dst);
}

BOOL CalypsoDate14ToTimestamp(_DATETIME_ST *dst, WORD src)
{
  if (dst == NULL) return FALSE;
  
  dst->_time.hour   = 0;
  dst->_time.minute = 0;
  dst->_time.second = 0;
  
  return CalypsoDate14ToDate(&dst->_date, src);
}

WORD CalypsoDateToDate14(const _DATE_ST *src)
{
  _DATE_ST t;
  register WORD value = 0;
  
  if (src == NULL) goto done;

  memcpy(&t, src, sizeof(_DATE_ST));

  if (!is_date_valid(&t)) goto done;
  
  if (t.year < 1997) goto done;
  
  while ((t.year  != 1997)
      || (t.month != 1)
      || (t.day   != 1))
  {
    date_prev_day(&t);
    value++;
  }
  
done:
  return value;
}

WORD CalypsoTimestampToDate14(const _DATETIME_ST *src)
{
  return CalypsoDateToDate14(&src->_date);  
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
