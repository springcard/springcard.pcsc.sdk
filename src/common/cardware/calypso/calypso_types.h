#ifndef __CALYPSO_TYPES_H__
#define __CALYPSO_TYPES_H__

#ifdef SPROX_INS_LIB
  typedef SWORD CALYPSO_RC;
  typedef WORD  CALYPSO_SZ;
  typedef WORD  CALYPSO_BITS_SZ;
  #define CALYPSO_LIB
  #define CALYPSO_API
#endif

#ifdef WIN32
  #include <windows.h>
  typedef LONG  CALYPSO_RC;
  typedef DWORD CALYPSO_SZ;
  typedef DWORD CALYPSO_BITS_SZ;
#endif

#ifdef __linux__
  typedef signed long   CALYPSO_RC;
  typedef unsigned long CALYPSO_SZ;
  typedef unsigned long CALYPSO_BITS_SZ;
#endif

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

#endif

#endif

