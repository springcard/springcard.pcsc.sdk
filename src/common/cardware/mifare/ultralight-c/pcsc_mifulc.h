#ifndef __PCSC_MIFULC_H__
#define __PCSC_MIFULC_H__

#include <winscard.h>

#ifdef WIN32
  #ifndef SPROX_MIFULC_LIB
    #define SPROX_MIFULC_LIB __declspec( dllimport )
  #endif
	#ifndef SPROX_MIFULC_API
	  #define SPROX_MIFULC_API __cdecl
	#endif
#else
	#define SPROX_MIFULC_LIB
	#define SPROX_MIFULC_API
#endif


#ifdef __cplusplus
extern  "C"
{
#endif

SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_AttachLibrary(SCARDHANDLE hCard);
SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_DetachLibrary(SCARDHANDLE hCard);

SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_SuspendTracking(SCARDHANDLE hCard);
SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_ResumeTracking(SCARDHANDLE hCard);
SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_WakeUp(SCARDHANDLE hCard);

SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_Authenticate(SCARDHANDLE hCard,
                                                                const BYTE key_value[16]);

SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_ChangeKey(SCARDHANDLE hCard,
                                                             const BYTE key_value[16]);

SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_Read(SCARDHANDLE hCard,
                                                        BYTE address,
                                                        BYTE data[16]);
SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_Write(SCARDHANDLE hCard,
                                                         BYTE address,
                                                         const BYTE data[16]);
SPROX_MIFULC_LIB LONG SPROX_MIFULC_API SCardMifUlC_Write4(SCARDHANDLE hCard,
                                                          BYTE address,
                                                          const BYTE data[4]);

#ifdef __cplusplus
}
#endif


#endif
