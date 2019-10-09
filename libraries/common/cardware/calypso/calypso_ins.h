#ifndef __CALYPSO_INS_H__
#define __CALYPSO_INS_H__

#include "calypso_errors.h"
#include "calypso_api_i.h"

BOOL CalypsoGetCurrentTimestamp(_DATETIME_ST *dst);

BOOL CalypsoIsTimestampValid(_DATETIME_ST *timestamp);
BOOL CalypsoIsDateValid(_DATETIME_ST *timestamp);
BOOL CalypsoIsTimeValid(_DATETIME_ST *timestamp);

WORD CalypsoTimestampToDate14(const _DATETIME_ST *src);
WORD CalypsoTimestampToTime11(const _DATETIME_ST *src);

BOOL CalypsoDate14IsBefore(const _DATETIME_ST *now, WORD value);
BOOL CalypsoDate14IsAfter(const _DATETIME_ST *now, WORD value);

extern DWORD micore_picc_time;
extern DWORD micore_tcl_time;
extern DWORD micore_calypso_time;

#endif
