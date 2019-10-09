#ifndef __CALYPSO_TYPES_H__
#define __CALYPSO_TYPES_H__

#ifdef SPROX_INS_LIB
  typedef SWORD CALYPSO_RC;
  #define CALYPSO_LIB
  #define CALYPSO_API
#endif

#ifdef WIN32
  #include <windows.h>
  typedef LONG  CALYPSO_RC;
#endif

#ifdef __linux__
  typedef signed long   CALYPSO_RC;
#endif

typedef unsigned short CALYPSO_BITS_SZ;

#ifdef CALYPSO_NO_TIME_T

#ifndef _DATETIME_DEFINED
#define _DATETIME_DEFINED

typedef struct
{
  BYTE day;
  BYTE month;
  WORD year;
} _DATE_ST;

typedef struct
{
  BYTE hour;
  BYTE minute;
  BYTE second;
} _TIME_ST;

typedef struct
{
  _DATE_ST _date;
  _TIME_ST _time;
} _DATETIME_ST;

#endif

BOOL CalypsoDate14ToDate(_DATE_ST *dst, WORD src);
BOOL CalypsoDate14ToTimestamp(_DATETIME_ST *dst, WORD src);
BOOL CalypsoTime11ToTimestamp(_DATETIME_ST *dst, WORD src);
WORD CalypsoDateToDate14(const _DATE_ST *src);
WORD CalypsoTimestampToDate14(const _DATETIME_ST *src);
BOOL CalypsoTime11ToTimestamp(_DATETIME_ST *dst, WORD src);
WORD CalypsoTimestampToTime11(const _DATETIME_ST *src);
BOOL CalypsoDate14IsBefore(const _DATETIME_ST *now, WORD value);
BOOL CalypsoDate14IsAfter(const _DATETIME_ST *now, WORD value);

#endif

#endif

