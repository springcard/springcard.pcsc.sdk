#ifndef __SPROX_MIFPLUS_H__
#define __SPROX_MIFPLUS_H__

#include "products/springprox/api/springprox.h"
#include "mifplus_card.h"

#ifdef WIN32
  #include <windows.h>
  #include <tchar.h>

  #ifndef SPROX_MIFPLUS_LIB
    #define SPROX_MIFPLUS_LIB __declspec( dllimport )
  #endif

  #ifndef SPROX_MIFPLUS_API
    #ifndef UNDER_CE
      /* Under Desktop Windows we use the cdecl calling convention */
      #define SPROX_MIFPLUS_API __cdecl
    #else
      /* Under Windows CE we use the stdcall calling convention */
      #define SPROX_MIFPLUS_API __stdcall
    #endif
  #endif
#else
  #define SPROX_MIFPLUS_LIB
  #define SPROX_MIFPLUS_API
#endif

#include "mifplus_card.h"

#ifdef __cplusplus
extern  "C"
{
#endif

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_SelectCid(BYTE cid);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_SelectCard(BYTE sak, const BYTE ats[], BYTE ats_len);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_EnterTcl(void);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_LeaveTcl(void);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_GetCardLevel(BYTE *level);


SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_WritePerso(WORD address,
                                                                   const BYTE value[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_CommitPerso(void);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_ResetAuthentication(void);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_Read(WORD address,
																													   BYTE data[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_ReadM(WORD address,
																														  BYTE count,
																													    BYTE data[]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_ReadEx(BYTE cmd_code,
                                                               WORD address,
																														   BYTE count,
																													     BYTE data[]);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_Write(WORD address,
																														  const BYTE data[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_WriteM(WORD address,
																														   BYTE count,
																														   const BYTE data[]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_WriteEx(BYTE cmd_code,
                                                                WORD address,
																															  BYTE count,
																														    const BYTE data[]);
																																				
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_FirstAuthenticate(WORD key_address,
																																				  const BYTE key_value[16],
																																				  const BYTE pcd_cap[],
																																				  BYTE pcd_cap_len,
																																				  BYTE picc_cap[6]);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_FollowingAuthenticate(WORD key_address,
                                                                              const BYTE key_value[16]);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_VirtualCardSupport(const BYTE install_id[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_VirtualCardSupportLast(const BYTE install_id[16],
																																						   const BYTE pcd_rnd[12],
																																				       const BYTE pcd_cap[],
																																				       BYTE pcd_cap_len,
																																				       BYTE picc_data[16],
																																						   BYTE picc_mac[8]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_VirtualCardSupportCheck(const BYTE pcd_rnd[12],
																																				        const BYTE pcd_cap[],
																																				        BYTE pcd_cap_len,
																																				        const BYTE picc_data[16],
																																							  const BYTE picc_mac[8],
																																							  BYTE *picc_info,
																																				        BYTE picc_cap[2],
																																				        BYTE picc_uid[],
																																				        BYTE *picc_uid_len,
																																						    const BYTE install_enc_key[16],
																																						    const BYTE install_mac_key[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_SelectVirtualCard(const BYTE picc_cap[2],
																																				  const BYTE picc_uid[],
																																				  BYTE picc_uid_len,
																																				   const BYTE install_mac_key[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_DeselectVirtualCard(void);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_VirtualCard(const BYTE install_id[16],
																																	  const BYTE polling_enc_key[16],
																																	  const BYTE polling_mac_key[16],
                                                                    const BYTE pcd_cap[],
																																	  BYTE pcd_cap_len,
																																	  BYTE *picc_info,
																																	  BYTE picc_cap[2],
																																	  BYTE picc_uid[],
																																	  BYTE *picc_uid_len);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROX_MifPlus_VirtualCardEx(const BYTE install_ids[],
																																		  const BYTE polling_enc_keys[],
																																		  const BYTE polling_mac_keys[],
																																		  const BYTE select_mac_keys[],
																																		  BYTE install_count,
																																		  const BYTE pcd_cap[],
																																		  BYTE pcd_cap_len,
																																		  BYTE *picc_info,
																																		  BYTE picc_cap[2],
																																		  BYTE picc_uid[],
																																		  BYTE *picc_uid_len,
																																		  BYTE *install_idx);

#ifdef __cplusplus
}
#endif


#endif
