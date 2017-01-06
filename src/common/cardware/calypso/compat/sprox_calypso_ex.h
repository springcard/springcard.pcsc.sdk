#ifndef __SPROX_CALYPSO_EX_H__
#define __SPROX_CALYPSO_EX_H__

#include "products/springprox/springprox_ex.h"
#include "sprox_calypso.h"
#include <stdio.h>
#include <string.h>

#ifdef __cplusplus
extern  "C"
{
#endif

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_AttachCalypsoLibrary(SPROX_INSTANCE rInst);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_DetachCalypsoLibrary(SPROX_INSTANCE rInst);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_ConfigureForCalypso(SPROX_INSTANCE rInst);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_APGEN(SPROX_INSTANCE rInst,
                                                               BYTE pdc_addr,
                                                               BOOL long_apgen,
                                                               BOOL with_atr,
                                                               BYTE occupancy,
                                                               BYTE repgen[],
                                                               BYTE *repgen_len,
                                                               BYTE picc_uid[4]);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_ATTRIB(SPROX_INSTANCE rInst,
                                                                BYTE pcd_addr,
                                                                BYTE picc_addr,
                                                                BYTE picc_uid[4]);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_COM_R(SPROX_INSTANCE rInst,
                                                               BYTE pcd_addr,
                                                               BYTE picc_addr,
                                                               const BYTE iso_in_data[],
                                                               BYTE iso_in_data_len,
                                                               BYTE iso_out_data[],
                                                               BYTE *iso_out_data_len);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_COM_RA(SPROX_INSTANCE rInst,
                                                                BYTE pcd_addr,
                                                                BYTE picc_addr,
                                                                const BYTE picc_uid[4],
                                                                const BYTE iso_in_data[],
                                                                BYTE iso_in_data_len,
                                                                BYTE iso_out_data[],
                                                                BYTE *iso_out_data_len);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_DISC(SPROX_INSTANCE rInst,
                                                              BYTE pcd_addr,
                                                              BYTE picc_addr);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_WAIT(SPROX_INSTANCE rInst,
                                                              BYTE pcd_addr,
                                                              BYTE picc_addr);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_Select(SPROX_INSTANCE rInst,
                                                               BYTE slot);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_Connect(SPROX_INSTANCE rInst,
                                                                 BYTE uid[8]);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_Disconnect(SPROX_INSTANCE rInst);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_Exchange(SPROX_INSTANCE rInst,
                                                                  const BYTE send_buffer[],
                                                                  WORD       send_length,
                                                                  BYTE       recv_buffer[],
                                                                  WORD       *recv_length);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_OpenSecureSession(SPROX_INSTANCE rInst,
                                                                           BYTE p1,
                                                                           BYTE p2,
                                                                           const BYTE sam_chal[4],
                                                                           BYTE card_resp_buffer[],
                                                                           WORD *card_resp_length);                                                                                                                                                   

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_Calypso_CloseSecureSession(SPROX_INSTANCE rInst,
                                                                            const BYTE sam_mac[4],
                                                                            BYTE card_mac[4]);

SPROX_CALYPSO_LIB WORD SPROX_CALYPSO_API SPROXx_Calypso_SW(SPROX_INSTANCE rInst);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_Select(SPROX_INSTANCE rInst,
                                                                   BYTE slot);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_Connect(SPROX_INSTANCE rInst,
                                                                    BYTE atr_buffer[],
                                                                    WORD *atr_length);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_Disconnect(SPROX_INSTANCE rInst);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_Exchange(SPROX_INSTANCE rInst,
                                                                     const BYTE send_buffer[],
                                                                     WORD send_length,
                                                                     BYTE recv_buffer[],
                                                                     WORD *recv_length);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_SelectDiversifier(SPROX_INSTANCE rInst,
                                                                              const BYTE card_uid[8]);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_GetChallenge(SPROX_INSTANCE rInst,
                                                                         BYTE sam_chal[4]);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_DigestInit(SPROX_INSTANCE rInst,
                                                                       BYTE kif,
                                                                       BYTE kvc,
                                                                       const BYTE card_resp_buffer[],
                                                                       WORD card_resp_length);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_DigestInit_Old(SPROX_INSTANCE rInst,
                                                                           BYTE key_record,
                                                                           const BYTE card_resp_buffer[],
                                                                           WORD card_resp_length);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_DigestUpdate(SPROX_INSTANCE rInst,
                                                                         const BYTE card_buffer[],
                                                                         WORD card_buflen);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_DigestClose(SPROX_INSTANCE rInst,
                                                                        BYTE sam_mac[4]);

SPROX_CALYPSO_LIB SWORD SPROX_CALYPSO_API SPROXx_CalypsoSam_DigestAuthenticate(SPROX_INSTANCE rInst,
                                                                               const BYTE card_mac[4]);

SPROX_CALYPSO_LIB WORD SPROX_CALYPSO_API SPROXx_CalypsoSam_SW(SPROX_INSTANCE rInst);

SPROX_CALYPSO_LIB void SPROX_CALYPSO_API SPROXx_Calypso_SetTraceFile(const TCHAR *filename);

#ifdef __cplusplus
}
#endif

#endif
