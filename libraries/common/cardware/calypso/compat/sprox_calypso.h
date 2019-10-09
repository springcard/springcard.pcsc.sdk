#ifndef __SPROX_CALYPSO_H__
#define __SPROX_CALYPSO_H__

#include "products/springprox/api/springprox.h"

#include <stdio.h>
#include <string.h>

#ifdef WIN32
  #include <windows.h>
  #include <stdio.h>
  #include <stdlib.h>
  #include <string.h>
  #include <tchar.h>

  #ifndef SPROX_CALYPSO_LIB
    #define SPROX_CALYPSO_LIB __declspec( dllimport )
  #endif

  #ifndef UNDER_CE
    #define SPROX_CALYPSO_API __cdecl
  #else
    #define SPROX_CALYPSO_API __stdcall
  #endif

  #ifdef UNICODE
    #define Calypso_GetErrorMessageW Calypso_GetErrorMessage
  #else
    #define Calypso_GetErrorMessageA Calypso_GetErrorMessage
  #endif

#else

  /* Not Win32 */
  #define SPROX_CALYPSO_API
  #define SPROX_CALYPSO_LIB

#endif

#ifdef __cplusplus
extern  "C"
{
#endif

/*
 * Calypso exported functions
 * --------------------------
 */

/*
 * sprox_calypso_radio.c
 * ---------------------
 */
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_ConfigureForCalypso(void);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_APGEN(BYTE pdc_addr,
                                                              BOOL long_apgen,
                                                              BOOL with_atr,
                                                              BYTE occupancy,
                                                              BYTE *repgen,
                                                              BYTE *repgen_len,
                                                              BYTE *picc_uid4);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_ATTRIB(BYTE pcd_addr,
                                                               BYTE picc_addr,
                                                               BYTE *picc_uid4);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_COM_R(BYTE pcd_addr,
                                                              BYTE picc_addr,
                                                              const BYTE *iso_in_data,
                                                              BYTE iso_in_data_len,
                                                              BYTE *iso_out_data,
                                                              BYTE *iso_out_data_len);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_COM_RA(BYTE pcd_addr,
                                                               BYTE picc_addr,
                                                               const BYTE *picc_uid4,
                                                               const BYTE *iso_in_data,
                                                               BYTE iso_in_data_len,
                                                               BYTE *iso_out_data,
                                                               BYTE *iso_out_data_len);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_DISC(BYTE pcd_addr,
                                                             BYTE picc_addr);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_WAIT(BYTE pcd_addr,
                                                             BYTE picc_addr);

/*
 * sprox_calypso_card.c
 * --------------------
 */
#define CALYPSO_LEGACYRF_SLOT 0xFE
#define CALYPSO_ISO14443_SLOT 0xFF

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_Select(BYTE slot);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_Connect(BYTE uid[8]);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_Disconnect(void);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_Exchange(const BYTE *send_buffer,
                                                                 WORD       send_length,
                                                                 BYTE       *recv_buffer,
                                                                 WORD       *recv_length);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_OpenSecureSession(BYTE p1,
                                                                          BYTE p2,
                                                                          const BYTE sam_chal[4],
                                                                          BYTE card_resp_buffer[],
                                                                          WORD *card_resp_length);                                                                                                                                                   
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_Calypso_CloseSecureSession(const BYTE sam_mac[4],
                                                                           BYTE card_mac[4]);

SPROX_CALYPSO_LIB WORD SPROX_CALYPSO_API SPROX_Calypso_SW(void);

/*
 * sprox_calypso_sam.c
 * -------------------
 */
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_Select(BYTE slot);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_Connect(BYTE *atr_buffer,
                                                                   WORD *atr_length);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_Disconnect(void);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_Exchange(const BYTE *send_buffer,
                                                                    WORD send_length,
                                                                    BYTE *recv_buffer,
                                                                    WORD *recv_length);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_SelectDiversifier(const BYTE card_uid[8]);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_GetChallenge(BYTE sam_chal[4]);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_DigestInit(BYTE kif,
                                                                      BYTE kvc,
                                                                      const BYTE card_resp_buffer[],
                                                                      WORD card_resp_length);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_DigestInit_Old(BYTE key_record,
                                                                          const BYTE card_resp_buffer[],
                                                                          WORD card_resp_length);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_DigestUpdate(const BYTE * card_buffer,
                                                                        WORD card_buflen);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_DigestClose(BYTE sam_mac[4]);
SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROX_CalypsoSam_DigestAuthenticate(const BYTE card_mac[4]);

SPROX_CALYPSO_LIB WORD SPROX_CALYPSO_API SPROX_CalypsoSam_SW(void);

/*
 * sprox_calypso_app.c
 * -------------------
 */



SPROX_CALYPSO_LIB void SPROX_CALYPSO_API SPROX_Calypso_SetTraceFile(const TCHAR *filename);

#ifdef __cplusplus
}
#endif

/**d* SpringProxINS/CALYPSO_ERR_WRONG_ADDR
 *
 * NAME
 *   CALYPSO_ERR_WRONG_ADDR
 *
 * DESCRIPTION
 *   Calypso/Innovatron error : invalid address in card's frame
 *
 **/
#define CALYPSO_ERR_WRONG_ADDR   -1000

/**d* SpringProxINS/CALYPSO_ERR_WRONG_RESP
 *
 * NAME
 *   CALYPSO_ERR_WRONG_RESP
 *
 * DESCRIPTION
 *   Calypso/Innovatron error : invalid command code in card's frame
 *
 **/
#define CALYPSO_ERR_WRONG_RESP   -1001

/**d* SpringProxINS/CALYPSO_ERR_WRONG_LENGTH
 *
 * NAME
 *   CALYPSO_ERR_WRONG_LENGTH
 *
 * DESCRIPTION
 *   Calypso/Innovatron error : invalid size in card's frame
 *
 **/
#define CALYPSO_ERR_WRONG_LENGTH -1002

/**d* SpringProxINS/CALYPSO_ERR_FUNCTION_CALL
 *
 * NAME
 *   CALYPSO_ERR_FUNCTION_CALL
 *
 * DESCRIPTION
 *   Calypso/Innovatron error : invalid parameter passed to the library
 *
 **/
#define CALYPSO_ERR_FUNCTION_CALL  -1003

/**d* SpringProxINS/CALYPSO_ERR_OVERFLOW
 *
 * NAME
 *   CALYPSO_ERR_OVERFLOW
 *
 * DESCRIPTION
 *   Calypso/Innovatron error : card's frame is too long
 *
 **/
#define CALYPSO_ERR_OVERFLOW     -1004


/**d* SpringProxINS/CALYPSO_ERR_PROTO
 *
 * NAME
 *   CALYPSO_ERR_PROTO
 *
 * DESCRIPTION
 *   Calypso/ISO error : card's answer is too short
 *
 **/
#define CALYPSO_ERR_PROTO        -1006

/**d* SpringProxINS/CALYPSO_TOO_MANY_INST
 *
 * NAME
 *   CALYPSO_TOO_MANY_INST
 *
 * DESCRIPTION
 *   Calypso/Innovatron error : too many active instances of the library
 *
 **/
#define CALYPSO_TOO_MANY_INST    -1018

/**d* SpringProxINS/CALYPSO_NO_SUCH_INST
 *
 * NAME
 *   CALYPSO_NO_SUCH_INST
 *
 * DESCRIPTION
 *   Calypso/Innovatron error : the specified instance doesn't exist
 *
 **/
#define CALYPSO_NO_SUCH_INST     -1019

/**d* SpringProxINS/CALYPSO_ERR_PARAM
 *
 * NAME
 *   CALYPSO_ERR_PARAM
 *
 * DESCRIPTION
 *   Calypso error : invalid parameter (wrong value for P1 or P2)
 *
 **/
#define CALYPSO_ERR_PARAM        -1020

/**d* SpringProxINS/CALYPSO_ERR_LENGTH
 *
 * NAME
 *   CALYPSO_ERR_LENGTH
 *
 * DESCRIPTION
 *   Calypso error : invalid length (wrong value for P3 = Lc or Le)
 *
 **/
#define CALYPSO_ERR_LENGTH       -1021

/**d* SpringProxINS/CALYPSO_NO_SUCH_KEY
 *
 * NAME
 *   CALYPSO_NO_SUCH_KEY
 *
 * DESCRIPTION
 *   Calypso error : the selected key (P1 or KIF+KVC) doesn't exist
 *
 **/
#define CALYPSO_NO_SUCH_KEY      -1022

/**d* SpringProxINS/CALYPSO_IN_SESSION
 *
 * NAME
 *   CALYPSO_IN_SESSION
 *
 * DESCRIPTION
 *   Calypso error : a session is already active
 *
 **/
#define CALYPSO_IN_SESSION       -1028

/**d* SpringProxINS/CALYPSO_NO_SESSION
 *
 * NAME
 *   CALYPSO_NO_SESSION
 *
 * DESCRIPTION
 *   Calypso error : no session currently active
 *
 **/
#define CALYPSO_NO_SESSION       -1029

/**d* SpringProxINS/CALYPSO_ERR_STATUS
 *
 * NAME
 *   CALYPSO_ERR_STATUS
 *
 * DESCRIPTION
 *   Calypso/ISO error : card's status word is not 90 00
 *
 **/
#define CALYPSO_ERR_STATUS       -1030

/**d* SpringProxINS/CALYPSO_ERR_DATA
 *
 * NAME
 *   CALYPSO_ERR_DATA
 *
 * DESCRIPTION
 *   Calypso error : mandatory data not found in card's answer
 *
 **/
#define CALYPSO_ERR_DATA         -1031

/**d* SpringProxINS/CALYPSO_ERR_COUNTER
 *
 * NAME
 *   CALYPSO_ERR_COUNTER
 *
 * DESCRIPTION
 *   Calypso error : counter overflow, or increase denied
 *
 **/
#define CALYPSO_ERR_COUNTER      -1032

/**d* SpringProxINS/CALYPSO_ERR_DENIED
 *
 * NAME
 *   CALYPSO_ERR_DENIED
 *
 * DESCRIPTION
 *   Calypso error : access to this function is forbidden
 *
 **/
#define CALYPSO_ERR_DENIED       -1033

/**d* SpringProxINS/CALYPSO_SAM_LOCKED
 *
 * NAME
 *   CALYPSO_SAM_LOCKED
 *
 * DESCRIPTION
 *   Calypso SAM error : SAM must be unlocked before operation
 *
 **/
#define CALYPSO_SAM_LOCKED       -1034

/**d* SpringProxINS/CALYPSO_WRONG_MAC
 *
 * NAME
 *   CALYPSO_WRONG_MAC
 *
 * DESCRIPTION
 *   Calypso security error : wrong signature
 *
 **/
#define CALYPSO_WRONG_MAC        -1035

#endif
