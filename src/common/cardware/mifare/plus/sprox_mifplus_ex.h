#ifndef __SPROX_MIFPLUS_EX_H__
#define __SPROX_MIFPLUS_EX_H__

#include "products/springprox/springprox_ex.h"
#include "sprox_mifplus.h"

#ifdef __cplusplus
extern  "C"
{
#endif

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_SelectCid(SPROX_INSTANCE rInst, BYTE cid);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_EnterTcl(SPROX_INSTANCE rInst);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_LeaveTcl(SPROX_INSTANCE rInst);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_GetCardLevel(SPROX_INSTANCE rInst, BYTE *level);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_WritePerso(SPROX_INSTANCE rInst,
                                                                   WORD address,
                                                                   const BYTE value[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_CommitPerso(SPROX_INSTANCE rInst);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_ResetAuthentication(SPROX_INSTANCE rInst);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_Read(SPROX_INSTANCE rInst,
                                                              WORD address,
																													    BYTE data[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_ReadM(SPROX_INSTANCE rInst,
                                                               WORD address,
																														   BYTE count,
																													     BYTE data[]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_ReadEx(SPROX_INSTANCE rInst,
                                                                BYTE cmd_code,
                                                                WORD address,
																														    BYTE count,
																													      BYTE data[]);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_Write(SPROX_INSTANCE rInst,
                                                               WORD address,
																														   const BYTE data[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_WriteM(SPROX_INSTANCE rInst,
                                                                WORD address,
																														    BYTE count,
																														    const BYTE data[]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_WriteEx(SPROX_INSTANCE rInst,
                                                                 BYTE cmd_code,
                                                                 WORD address,
																															   BYTE count,
																														     const BYTE data[]);
																																				
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_FirstAuthenticate(SPROX_INSTANCE rInst,
                                                                           WORD key_address,
																																				   const BYTE key_value[16],
																																				   const BYTE pcd_cap[],
																																				   BYTE pcd_cap_len,
																																				   BYTE picc_cap[6]);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_FollowingAuthenticate(SPROX_INSTANCE rInst,
                                                                               WORD key_address,
                                                                               const BYTE key_value[16]);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_VirtualCardSupport(SPROX_INSTANCE rInst,
                                                                            const BYTE install_id[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_VirtualCardSupportLast(SPROX_INSTANCE rInst,
                                                                                const BYTE install_id[16],
																																						    const BYTE pcd_rnd[12],
																																				        const BYTE pcd_cap[],
																																				        BYTE pcd_cap_len,
																																				        BYTE picc_data[16],
																																						    BYTE picc_mac[8]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_VirtualCardSupportCheck(SPROX_INSTANCE rInst,
                                                                                 const BYTE pcd_rnd[12],
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
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_SelectVirtualCard(SPROX_INSTANCE rInst,
                                                                           const BYTE picc_cap[2],
																																				   const BYTE picc_uid[],
																																				   BYTE picc_uid_len,
																																				   const BYTE install_mac_key[16]);
SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_DeselectVirtualCard(SPROX_INSTANCE rInst);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_VirtualCard(SPROX_INSTANCE rInst,
                                                                     const BYTE install_id[16],
																																	   const BYTE polling_enc_key[16],
																																	   const BYTE polling_mac_key[16],
                                                                     const BYTE pcd_cap[],
																																	   BYTE pcd_cap_len,
																																	   BYTE *picc_info,
																																	   BYTE picc_cap[2],
																																	   BYTE picc_uid[],
																																	   BYTE *picc_uid_len);

SPROX_MIFPLUS_LIB SWORD SPROX_MIFPLUS_API SPROXx_MifPlus_VirtualCardEx(SPROX_INSTANCE rInst,
                                                                       const BYTE install_ids[],
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
