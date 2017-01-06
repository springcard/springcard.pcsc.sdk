#ifndef __READER_HELPERS_H__
#define __READER_HELPERS_H__

#include "pcsc_helpers.h"

BOOL ReaderLeds(const char *szReaderName, BYTE red, BYTE green, BYTE blue);
BOOL ReaderBuzzer(const char *szReaderName, WORD buz_time_ms);
BOOL ReaderDetails(const char *szReaderName);

#endif
