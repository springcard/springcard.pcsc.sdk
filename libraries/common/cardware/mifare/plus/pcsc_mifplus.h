#ifndef __PCSC_MIFPLUS_H__
#define __PCSC_MIFPLUS_H__

#include <winscard.h>

#ifdef WIN32
  #ifndef SPROX_MIFPLUS_LIB
    #define SPROX_MIFPLUS_LIB __declspec( dllimport )
  #endif
	#ifndef SPROX_MIFPLUS_API
	  #define SPROX_MIFPLUS_API __cdecl
	#endif
#else
	#define SPROX_MIFPLUS_LIB
	#define SPROX_MIFPLUS_API
#endif

#include "mifplus_card.h"

/* ATR of a Mifare Plus running in T=CL mode (Level 0 or Level 3) */
#define MIFPLUS_ATR_TCL_BEGIN { 0x3B, 0x87, 0x80, 0x01, 0xC1, 0x05 };

/* First bytes of ATR of Mifare Plus running not in T=CL mode (Level 1 or Level 2) */
/* The last bytes depend how the card is actually recognized by the reader: maybe  */
/* a Mifare Classic 1K or 4K, or an unsupported card in the ISO 14443-3 'A' group  */
#define MIFPLUS_ATR_RAW_BEGIN { 0x3B, 0x8F, 0x80, 0x01, 0x80, 0x4F, 0x0C, 0xA0, 0x00, 0x00, 0x03, 0x06, 0x03 };

#ifdef __cplusplus
extern  "C"
{
#endif

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_AttachLibrary(SCARDHANDLE hCard);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_DetachLibrary(SCARDHANDLE hCard);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_SlotControl(SCARDHANDLE hCard, BYTE _P1, BYTE _P2);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_GetData(SCARDHANDLE hCard, BYTE _P1, BYTE _P2, BYTE rsp_buffer[], WORD rsp_max_len, WORD *rsp_got_len);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_EnterTcl(SCARDHANDLE hCard);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_LeaveTcl(SCARDHANDLE hCard);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_GetCardLevel(SCARDHANDLE hCard, BYTE *level);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_SuspendTracking(SCARDHANDLE hCard);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_ResumeTracking(SCARDHANDLE hCard);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_WakeUp(SCARDHANDLE hCard);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_WritePerso(SCARDHANDLE hCard, WORD address, const BYTE value[16]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_CommitPerso(SCARDHANDLE hCard);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_ResetAuthentication(SCARDHANDLE hCard);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_Read(SCARDHANDLE hCard, WORD address, BYTE data[16]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_ReadM(SCARDHANDLE hCard, WORD address, BYTE count, BYTE data[]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_ReadEx(SCARDHANDLE hCard, BYTE cmd_code, WORD address, BYTE count, BYTE data[]);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_Write(SCARDHANDLE hCard, WORD address, const BYTE data[16]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_WriteM(SCARDHANDLE hCard, WORD address, BYTE count, const BYTE data[]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_WriteEx(SCARDHANDLE hCard, BYTE cmd_code, WORD address, BYTE count, const BYTE data[]);
																																				
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_FirstAuthenticate(SCARDHANDLE hCard, WORD key_address, const BYTE key_value[16], const BYTE pcd_cap[], BYTE pcd_cap_len, BYTE picc_cap[6]);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_FollowingAuthenticate(SCARDHANDLE hCard, WORD key_address, const BYTE key_value[16]);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_VirtualCardSupport(SCARDHANDLE hCard, const BYTE install_id[16]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_VirtualCardSupportLast(SCARDHANDLE hCard, const BYTE install_id[16], const BYTE pcd_rnd[12], const BYTE pcd_cap[], BYTE pcd_cap_len, BYTE picc_data[16], BYTE picc_mac[8]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_VirtualCardSupportCheck(SCARDHANDLE hCard, const BYTE pcd_rnd[12], const BYTE pcd_cap[], BYTE pcd_cap_len, const BYTE picc_data[16], const BYTE picc_mac[8], BYTE *picc_info, BYTE picc_cap[2], BYTE picc_uid[], BYTE *picc_uid_len, const BYTE install_enc_key[16], const BYTE install_mac_key[16]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_SelectVirtualCard(SCARDHANDLE hCard, const BYTE picc_cap[2], const BYTE picc_uid[], BYTE picc_uid_len, const BYTE install_mac_key[16]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_DeselectVirtualCard(SCARDHANDLE hCard);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_VirtualCard(SCARDHANDLE hCard, const BYTE install_id[16], const BYTE polling_enc_key[16], const BYTE polling_mac_key[16], const BYTE pcd_cap[], BYTE pcd_cap_len, BYTE *picc_info, BYTE picc_cap[2], BYTE picc_uid[], BYTE *picc_uid_len);

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_VirtualCardEx(SCARDHANDLE hCard, const BYTE install_ids[], const BYTE polling_enc_keys[], const BYTE polling_mac_keys[], const BYTE select_mac_keys[], BYTE install_count, const BYTE pcd_cap[], BYTE pcd_cap_len, BYTE *picc_info, BYTE picc_cap[2], BYTE picc_uid[], BYTE *picc_uid_len, BYTE *install_idx);

/* New 19/08/08 for Mifare Plus EV1 */
#define MIFPLUS_ISO_WRAPPING_OFF         0
#define MIFPLUS_ISO_WRAPPING_CARD        1
#define MIFPLUS_ISO_WRAPPING_READER      2

SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_IsoWrapping(SCARDHANDLE hCard, BYTE mode);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_GetVersion(SCARDHANDLE hCard, BYTE version[], WORD max_size, WORD *got_size);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_GetSignature(SCARDHANDLE hCard, BYTE signature[], WORD max_size, WORD *got_size);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_FirstAuthenticateEV0(SCARDHANDLE hCard, WORD key_address, const BYTE key_value[16], BYTE picc_cap[6]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_FirstAuthenticateEV1(SCARDHANDLE hCard, WORD key_address, const BYTE key_value[16], BYTE picc_cap[6]);
SPROX_MIFPLUS_LIB LONG SPROX_MIFPLUS_API SCardMifPlus_WriteConfigSL1(SCARDHANDLE hCard, BYTE options, const BYTE key_b_sector_0[6]);

#ifdef __cplusplus
}
#endif


#endif
