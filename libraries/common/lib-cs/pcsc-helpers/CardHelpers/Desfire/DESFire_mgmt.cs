/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 12/09/2017
 * Time: 14:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace SpringCard.PCSC.CardHelpers
{
	/// <summary>
	/// Description of DESFire_mgmt.
	/// </summary>
	public partial class Desfire
	{
		/**
     * NAME
     *   DesfireAPI :: Card management functions
     *
     * COPYRIGHT
     *   (c) 2009 SpringCard - www.springcard.com
     *
     * DESCRIPTION
     *   Implementation of management functions to personalize or format the
     *   DESFIRE card.
     *
     **/

		/**f* DesfireAPI/GetFreeMemory
     *
     * NAME
     *   GetFreeMemory
     *
     * DESCRIPTION
     *   Reads out the number of available bytes on the PICC
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_GetFreeMemory(DWORD *pdwFreeBytes);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_GetFreeMemory(SPROX_INSTANCE rInst,
     *                                      DWORD *pdwFreeBytes);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetFreeMemory(SCARDHANDLE hCard,
     *                                    DWORD *pdwFreeBytes);
     *
     * INPUTS
     *   DWORD *pdwFreeBytes : number of free bytes on the PICC
     *
     * RETURNS
     *   DF_OPERATION_OK : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   This command can be issued without valid authentication.
     *
     **/
		public long GetFreeMemory(ref UInt32 pdwFreeBytes)
		{
			long status;
    
			/* Create the info block containing the command code and the key number argument. */
			xfer_buffer[INF + 0] = DF_GET_FREE_MEMORY;
			xfer_length = 1;
    
			/* Communicate the info block to the card and check the operation's return status. */
			/* The returned info block must contain 4 bytes, the status code and the requested */
			/* information.                                                                    */
			status = Command(4, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
			if (status != DF_OPERATION_OK)
				return status;
    
			/* Get the actual value. */
			pdwFreeBytes = 0;
			pdwFreeBytes += xfer_buffer[INF + 3];
			pdwFreeBytes <<= 8;
			pdwFreeBytes += xfer_buffer[INF + 2];
			pdwFreeBytes <<= 8;
			pdwFreeBytes += xfer_buffer[INF + 1];

    
			/* Success. */
			return DF_OPERATION_OK;
		}
    
		/**f* DesfireAPI/GetCardUID
     *
     * NAME
     *   GetCardUID
     *
     * DESCRIPTION
     *   Reads out the 7-byte serial number of the PICC
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_GetCardUID(BYTE uid[7]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_GetCardUID(SPROX_INSTANCE rInst,
     *                                   BYTE uid[7]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetCardUID(SCARDHANDLE hCard,
     *                                 BYTE uid[7]);
     *
     *
     * RETURNS
     *   DF_OPERATION_OK : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   This command must be preceded by an authentication (with any key).
     *
     **/
		public long GetCardUID(out byte[] uid)
		{
			uid = new byte[7];
			
			long status;
    
			/* Create the info block containing the command code and the key number argument. */
			xfer_buffer[INF + 0] = DF_GET_CARD_UID;
			xfer_length = 1;
    
			/* Communicate the info block to the card and check the operation's return status. */
			/* The returned info block must contain 4 bytes, the status code and the requested */
			/* information.                                                                    */
			status = Command(17, COMPUTE_COMMAND_CMAC | WANTS_OPERATION_OK);
			if (status != DF_OPERATION_OK)
				return status;

			/* at first we have to decipher the recv_buffer */
			xfer_length -= 1;
  
			byte[] tmp = new byte[xfer_length];
			Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)xfer_length);
			CipherRecv(ref tmp, ref xfer_length);
			Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)xfer_length);
      
			/* Get the actual value. */
			Array.ConstrainedCopy(xfer_buffer, 1, uid, 0, 7);

			/* Success. */
			return DF_OPERATION_OK;
    
		}
    
    
    
		/**f* DesfireAPI/FormatPICC
     *
     * NAME
     *   FormatPICC
     *
     * DESCRIPTION
     *   Releases the DesFire card user memory
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_FormatPICC(void);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_FormatPICC(SPROX_INSTANCE rInst);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_FormatPICC(SCARDHANDLE hCard);
     *
     * RETURNS
     *   DF_OPERATION_OK    : format succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   All applications are deleted and all files within those applications  are deleted.
     *   This command always requires a preceding authentication with the DesFire card master key.
     *
     **/
		public long FormatPICC()
		{
    
			/* Create the info block containing the command code. */
			xfer_buffer[INF + 0] = DF_FORMAT_PICC;
			xfer_length = 1;
    
			/* Communicate the info block to the card and check the operation's return status. */
			return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
		}
    
		/**f* DesfireAPI/CreateApplication
     *
     * NAME
     *   CreateApplication
     *
     * DESCRIPTION
     *   Create a new application on the DesFire card
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_CreateApplication(DWORD aid,
     *                                         BYTE key_setting_1,
     *                                         BYTE key_setting_2);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_CreateApplication(SPROX_INSTANCE rInst,
     *                                          DWORD aid,
     *                                          BYTE key_setting_1,
     *                                          BYTE key_setting_2);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateApplication(SCARDHANDLE hCard,
     *                                        DWORD aid,
     *                                        BYTE key_setting_1,
     *                                        BYTE key_setting_2);
     *
     * INPUTS
     *   DWORD aid          : Application IDentifier
     *   BYTE key_setting_1 : Settings of the Application master key (see chapter 4.3.2 of datasheet
     *                        of mifare DesFire MF3ICD40 for more information)
     *   BYTE key_setting_2 : Number of keys that can be stored within the application for
     *                        cryptographic purposes, plus flags to specify cryptographic method and
     *                        to enable giving ISO names to the EF.
     *
     * RETURNS
     *   DF_OPERATION_OK : application created successfully
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   CreateApplicationIso
     *   DeleteApplication
     *   GetApplicationIDs
     *   SelectApplication
     *
     **/
		public long CreateApplication(UInt32 aid, byte key_setting_1, byte key_setting_2)
		{
    
			/* Create the info block containing the command code and the given parameters. */
			xfer_length = 0;
			xfer_buffer[xfer_length++] = DF_CREATE_APPLICATION;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
			aid >>= 8;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
			aid >>= 8;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
			xfer_buffer[xfer_length++] = key_setting_1;
			xfer_buffer[xfer_length++] = key_setting_2;
    
			/* Communicate the info block to the card and check the operation's return status. */
			return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
		}
    
		/**f* DesfireAPI/CreateIsoApplication
     *
     * NAME
     *   CreateIsoApplication
     *
     * DESCRIPTION
     *   Create a new application on the DesFire card, and defines the ISO identifier
     *   and name of the application
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_CreateIsoApplication(DWORD aid,
     *                                            BYTE key_setting_1,
     *                                            BYTE key_setting_2,
     *                                            WORD iso_df_id,
     *                                            const BYTE iso_df_name[],
     *                                            BYTE iso_df_namelen);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_CreateIsoApplication(SPROX_INSTANCE rInst,
     *                                             DWORD aid,
     *                                             BYTE key_setting_1,
     *                                             BYTE key_setting_2,
     *                                             WORD iso_df_id,
     *                                             const BYTE iso_df_name[],
     *                                             BYTE iso_df_namelen);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateIsoApplication(SCARDHANDLE hCard,
     *                                           DWORD aid,
     *                                           BYTE key_setting_1,
     *                                           BYTE key_setting_2,
     *                                           WORD iso_df_id,
     *                                           const BYTE iso_df_name[],
     *                                           BYTE iso_df_namelen);
     *
     * INPUTS
     *   DWORD aid                : Application IDentifier
     *   BYTE key_setting_1       : Settings of the Application master key (see chapter 4.3.2 of datasheet
     *                              of mifare DesFire MF3ICD40 for more information)
     *   BYTE key_setting_2       : Number of keys that can be stored within the application for
     *                              cryptographic purposes, plus flags to specify cryptographic method and
     *                              to enable giving ISO names to the EF.
     *   BYTE iso_df_id           : ID of the ISO DF
     *   const BYTE iso_df_name[] : name of the ISO DF
     *   BYTE iso_df_namelen      : length of iso_df_name
     *
     * RETURNS
     *   DF_OPERATION_OK : application created successfully
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   CreateApplication
     *   DeleteApplication
     *   GetApplicationIDs
     *   SelectApplication
     *
     **/
		public long CreateIsoApplication(UInt32 aid, byte key_setting_1, byte key_setting_2, UInt16 iso_df_id, byte[] iso_df_name, byte iso_df_namelen)
		{

			xfer_length = 0;
    
			/* Create the info block containing the command code and the given parameters. */
			xfer_buffer[xfer_length++] = DF_CREATE_APPLICATION;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
			aid >>= 8;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
			aid >>= 8;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
			xfer_buffer[xfer_length++] = key_setting_1;
			xfer_buffer[xfer_length++] = key_setting_2;
			xfer_buffer[xfer_length++] = (byte)(iso_df_id); // JDA
			xfer_buffer[xfer_length++] = (byte)(iso_df_id >> 8); // JDA
    
			if (iso_df_name != null) {
				if (iso_df_namelen == 0)
					iso_df_namelen = (byte)iso_df_name.Length;
				if (iso_df_namelen > 16)
					return DFCARD_LIB_CALL_ERROR;

				Array.ConstrainedCopy(iso_df_name, 0, xfer_buffer, (int)xfer_length, iso_df_namelen);
				xfer_length += iso_df_namelen;
			}
    
			/* Communicate the info block to the card and check the operation's return status. */
			return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
		}
    
		/**f* DesfireAPI/DeleteApplication
     *
     * NAME
     *   DeleteApplication
     *
     * DESCRIPTION
     *   Permanently deactivates an application on the DesFire card
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_DeleteApplication(DWORD aid);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_DeleteApplication(SPROX_INSTANCE rInst,
     *                                     DWORD aid);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_DeleteApplication(SCARDHANDLE hCard,
     *                                     DWORD aid);
     *
     * INPUTS
     *   DWORD aid                   : Application IDentifier
     *
     * RETURNS
     *   DF_OPERATION_OK    : application deleted successfully
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   CreateApplication
     *   GetApplicationIDs
     *   SelectApplication
     *
     **/
		public long  DeleteApplication(UInt32 aid)
		{
			long status;
    
			/* Create the info block containing the command code and the given parameters. */
			xfer_length = 0;
			xfer_buffer[xfer_length++] = DF_DELETE_APPLICATION;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
			aid >>= 8;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
			aid >>= 8;
			xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
    
			/* Communicate the info block to the card and check the operation's return status. */
			status = Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | LOOSE_RESPONSE_CMAC | WANTS_OPERATION_OK);
    
			/* If the current application is deleted, the root application is implicitly selected */
			if ((status == DF_OPERATION_OK) && (current_aid == aid)) {
				current_aid = 0;
				CleanupAuthentication();
			}
    
			return status;
		}
    
		/**f* DesfireAPI/GetApplicationIDs
     *
     * NAME
     *   GetApplicationIDs
     *
     * DESCRIPTION
     *   Returns the Application IDentifiers of all active applications on a DesFire card
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_GetApplicationIDs(BYTE aid_max_count,
     *                                     DWORD aid_list[],
     *                                     BYTE *aid_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_GetApplicationIDs(SPROX_INSTANCE rInst,
     *                                     BYTE aid_max_count,
     *                                     DWORD aid_list[],
     *                                     BYTE *aid_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetApplicationIDs(SCARDHANDLE hCard,
     *                                     BYTE aid_max_count,
     *                                     DWORD aid_list[],
     *                                     BYTE *aid_count);
     *
     * INPUTS
     *   BYTE aid_max_count          : maximum number of Application IDentifiers
     *   DWORD aid_list[]            : Application IDentifier list
     *   BYTE *aid_count             : number of Application IDentifiers on DesFire card
     *
     * RETURNS
     *   DF_OPERATION_OK    : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   CreateApplication
     *   DeleteApplication
     *   SelectApplication
     *
     **/
		public long GetApplicationIDs(byte aid_max_count, ref UInt32[] aid_list, ref byte aid_count)
		{
			byte i;
			UInt32 recv_length = 1;
			byte[] recv_buffer = new byte[256];
			long status;
    
			/* Set the number of applications to zero */
			aid_count = 0;
    
			/* create the info block containing the command code */
			xfer_length = 0;
			xfer_buffer[xfer_length++] = DF_GET_APPLICATION_IDS;
    
			for (;;) {
				status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
    
				if (status != DF_OPERATION_OK)
					goto done;

				Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(xfer_length - 1));
				recv_length += (xfer_length - 1);
    
				if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
					break;
    
				xfer_length = 1;
			}
    
			recv_buffer[0] = DF_OPERATION_OK;
    
			/* Check the CMAC */
			status = VerifyCmacRecv(recv_buffer, ref recv_length);
			if (status != DF_OPERATION_OK)
				goto done;
    
    
			recv_length -= 1;     /* substract 1 byte for the received status */
    
			/* ByteCount must be in multiples of APPLICATION_ID_SIZE bytes
         we check this to proof the format integrity
         if zero bytes have been received this is ok -> means no
         applications existing */
			if ((recv_length % APPLICATION_ID_SIZE) != 0) {
				status = DFCARD_WRONG_LENGTH;
				goto done;
			}
    
			for (i = 0; i < (recv_length / APPLICATION_ID_SIZE); i++) {
				/* Extract AID */
				if (i < aid_max_count) {
					UInt32 aid;
    
					aid = recv_buffer[INF + 3 + APPLICATION_ID_SIZE * i];
					aid <<= 8;
					aid += recv_buffer[INF + 2 + APPLICATION_ID_SIZE * i];
					aid <<= 8;
					aid += recv_buffer[INF + 1 + APPLICATION_ID_SIZE * i];
    
					aid_list[i] = aid;
				}
			}
    
			aid_count = i;
    
			if (i >= aid_max_count)
				status = DFCARD_OVERFLOW;
    
			done:
			return status;
		}
    
		/**f* DesfireAPI/Desfire_GetIsoApplications
     *
     * NAME
     *   Desfire_GetIsoApplications
     *
     * DESCRIPTION
     *   Returns the Application IDentifiers, ISO DF IDs and ISO DF Names of all active
     *   applications on a DesFire card having an ISO DF ID / DF Name
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_GetIsoApplications(BYTE app_max_count,
     *                                          DF_ISO_APPLICATION_ST app_list[],
     *                                          BYTE *app_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_GetIsoApplications(SPROX_INSTANCE rInst,
     *                                           BYTE app_max_count,
     *                                           DF_ISO_APPLICATION_ST app_list[],
     *                                           BYTE *app_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetIsoApplications(SCARDHANDLE hCard,
     *                                         BYTE app_max_count,
     *                                         DF_ISO_APPLICATION_ST app_list[],
     *                                         BYTE *app_count);
     *
     * INPUTS
     *   BYTE app_max_count               : maximum number of Applications
     *   DF_ISO_APPLICATION_ST app_list[] : list of Applications
     *   BYTE *app_count                  : number of Applications on DesFire card
     *
     * RETURNS
     *   DF_OPERATION_OK    : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   CreateIsoApplication
     *   GetApplicationIDs
     *
     **/
		public long GetIsoApplications(byte app_max_count, List<DF_ISO_APPLICATION_ST> app_list, ref byte app_count)
		{
			byte i;
			UInt32 recv_length = 1;
			byte[] recv_buffer = new byte[1024];
			long status;
    
			/* Set the number of applications to zero */
			app_count = 0;
    
			/* create the info block containing the command code */
			xfer_length = 0;
			xfer_buffer[xfer_length++] = DF_GET_DF_NAMES;
    
			i = 0;
			for (;;) {
				status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
    
				if (status != DF_OPERATION_OK)
					goto done;
    
				if ((xfer_length != 1) && (xfer_length < 6)) {
					status = DFCARD_WRONG_LENGTH;
					goto done;
				}
    
				/* Extract application data */
				if ((app_list != null) && (i < app_max_count)) {
					DF_ISO_APPLICATION_ST app = new DF_ISO_APPLICATION_ST();
    
					app.dwAid = xfer_buffer[INF + 3];
					app.dwAid <<= 8;
					app.dwAid += xfer_buffer[INF + 2];
					app.dwAid <<= 8;
					app.dwAid += xfer_buffer[INF + 1];
    
					app.wIsoId = xfer_buffer[INF + 5];
					app.wIsoId <<= 8;
					app.wIsoId += xfer_buffer[INF + 4];
    
					for (app.bIsoNameLen = 0; app.bIsoNameLen < app.abIsoName.Length; app.bIsoNameLen++) {
						if ((UInt16)(INF + 6 + app.bIsoNameLen) >= xfer_length)
							break;
						app.abIsoName[app.bIsoNameLen] = xfer_buffer[INF + 6 + app.bIsoNameLen];
					}
					app_list.Add(app);

				}
				i++;
    
				/* Remember the frame for later CMAC processing */
				Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(xfer_length - 1));
				recv_length += (xfer_length - 1);
    
				if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
					break;
    
				xfer_length = 1;
      
			}
    
			recv_buffer[0] = DF_OPERATION_OK;
    
			/* Check the CMAC */
			status = VerifyCmacRecv(recv_buffer, ref recv_length);
			if (status != DF_OPERATION_OK)
				goto done;
    
			recv_length -= 1;     /* substract 1 byte for the received status */
    
			app_count = i;
    
			if (i >= app_max_count)
				status = DFCARD_OVERFLOW;
    
			done:
			return status;
    
		}
    
		/**f* DesfireAPI/SelectApplication
     *
     * NAME
     *   SelectApplication
     *
     * DESCRIPTION
     *   Selects one specific application for further access
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_SelectApplication(DWORD aid);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_SelectApplication(SPROX_INSTANCE rInst,
     *                                     DWORD aid);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_SelectApplication(SCARDHANDLE hCard,
     *                                     DWORD aid);
     *
     * INPUTS
     *   DWORD aid                   : Application IDentifier
     *
     * RETURNS
     *   DF_OPERATION_OK    : application selected
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   CreateApplication
     *   DeleteApplication
     *   GetApplicationIDs
     *
     **/
		public long SelectApplication(UInt32 aid)
		{
			long status;
            UInt32 aid_copy = aid; /* MBA: save it ! */

            /* Each SelectApplication causes a currently valid authentication state to be lost */
            CleanupAuthentication();
    
			/* Create the info block containing the command code and the given parameters. */
			xfer_buffer[INF + 0] = DF_SELECT_APPLICATION;
			xfer_buffer[INF + 1] = (byte)(aid & 0x000000FF);
			aid >>= 8;
			xfer_buffer[INF + 2] = (byte)(aid & 0x000000FF);
			aid >>= 8;
			xfer_buffer[INF + 3] = (byte)(aid & 0x000000FF);
			xfer_length = 4;
    
			/* Communicate the info block to the card and check the operation's return status. */
			status = Command(0, WANTS_OPERATION_OK);
    
			if (status == DF_OPERATION_OK)
				current_aid = aid_copy;

            return status;
		}
    
		/**t* DesfireAPI/DF_VERSION_INFO
     *
     * NAME
     *   DF_VERSION_INFO
     *
     * DESCRIPTION
     *   Structure for returning the information supplied by the GetVersion command.
     *
     * SOURCE
     *   typedef struct
     *   {
     *     // hardware related information
     *     BYTE    bHwVendorID;     // vendor ID (0x04 for NXP)
     *     BYTE    bHwType;         // type (0x01)
     *     BYTE    bHwSubType;      // subtype (0x01)
     *     BYTE    bHwMajorVersion; // major version number
     *     BYTE    bHwMinorVersion; // minor version number
     *     BYTE    bHwStorageSize;  // storage size (0x18 = 4096 bytes)
     *     BYTE    bHwProtocol;     // communication protocol type (0x05 meaning ISO 14443-2 and -3)
    
     *     // software related information
     *     BYTE    bSwVendorID;     // vendor ID (0x04 for NXP)
     *     BYTE    bSwType;         // type (0x01)
     *     BYTE    bSwSubType;      // subtype (0x01)
     *     BYTE    bSwMajorVersion; // major version number
     *     BYTE    bSwMinorVersion; // minor version number
     *     BYTE    bSwStorageSize;  // storage size (0x18 = 4096 bytes)
     *     BYTE    bSwProtocol;     // communication protocol type (0x05 meaning ISO 14443-3 and -4)
     *
     *     BYTE    abUid[7];        // unique serial number
     *     BYTE    abBatchNo[5];    // production batch number
     *     BYTE    bProductionCW;   // calendar week of production
     *     BYTE    bProductionYear; // year of production
     *   } DF_VERSION_INFO;
     *
     **/
    
		/**f* DesfireAPI/GetVersion
     *
     * NAME
     *   GetVersion
     *
     * DESCRIPTION
     *   Returns manufacturing related data of the DesFire card
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_GetVersion(DF_VERSION_INFO *pVersionInfo);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_GetVersion(SPROX_INSTANCE rInst,
     *                                     DF_VERSION_INFO *pVersionInfo);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetVersion(SCARDHANDLE hCard,
     *                                     DF_VERSION_INFO *pVersionInfo);
     *
     * INPUTS
     *   DF_VERSION_INFO *pVersionInfo : card's version information
     *
     * RETURNS
     *   DF_OPERATION_OK    : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     **/
		public long GetVersion(out DF_VERSION_INFO VersionInfo)
		{
			VersionInfo = new DF_VERSION_INFO();
			VersionInfo.abUid = new byte[7];
      		VersionInfo.abBatchNo = new byte[5];
			
			UInt32 recv_length = 1;
			byte[] recv_buffer = new byte[256];
			long status;
    
			/* create the info block containing the command code */
			xfer_length = 0;
			xfer_buffer[xfer_length++] = DF_GET_VERSION;
    
			for (;;) {
				status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
				if (status != DF_OPERATION_OK)
					goto done;

				Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(xfer_length - 1));
        
				recv_length += (xfer_length - 1);
    
				if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
					break;
    
				xfer_length = 1;
			}
    
			recv_buffer[0] = DF_OPERATION_OK;
    
			/* Check the CMAC */
			status = VerifyCmacRecv(recv_buffer, ref recv_length);
    
			if (status != DF_OPERATION_OK)
				goto done;
    
			recv_length -= 1;     /* substract 1 byte for the received status */
      
			if (recv_length != 28) {
				status = DFCARD_WRONG_LENGTH;
				goto done;      	
			}
			
			VersionInfo.bHwVendorID = recv_buffer[1];
      		VersionInfo.bHwType = recv_buffer[2];
      		VersionInfo.bHwSubType = recv_buffer[3];
      		VersionInfo.bHwMajorVersion = recv_buffer[4];
      		VersionInfo.bHwMinorVersion = recv_buffer[5];
      		VersionInfo.bHwStorageSize = recv_buffer[6];
      		VersionInfo.bHwProtocol = recv_buffer[7];
      		VersionInfo.bSwVendorID = recv_buffer[8];
      		VersionInfo.bSwType = recv_buffer[9];
      		VersionInfo.bSwSubType = recv_buffer[10];
      		VersionInfo.bSwMajorVersion = recv_buffer[11];
      		VersionInfo.bSwMinorVersion = recv_buffer[12];
      		VersionInfo.bSwStorageSize = recv_buffer[13];
      		VersionInfo.bSwProtocol = recv_buffer[14];
      		Array.Copy(recv_buffer, 15, VersionInfo.abUid, 0, 7);
      		Array.Copy(recv_buffer, 22, VersionInfo.abBatchNo, 0, 5);
      		VersionInfo.bProductionCW = recv_buffer[27];
      		VersionInfo.bProductionYear= recv_buffer[28];
			
		done:
			return status;    
		}
    
		/**f* DesfireAPI/SetConfiguration
     *
     * NAME
     *   SetConfiguration
     *
     * DESCRIPTION
     *   Sends the SetConfiguration command to the DESFIRE card.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_SetConfiguration(BYTE option,
     *                                        const BYTE data[],
     *                                        BYTE length);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_SetConfiguration(SPROX_INSTANCE rInst,
     *                                         BYTE option,
     *                                         const BYTE data[],
     *                                         BYTE length);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_SetConfiguration(SCARDHANDLE hCard,
     *                                       BYTE option,
     *                                       const BYTE data[],
     *                                       BYTE length);
     *
     * INPUTS
     *
     * RETURNS
     *   DF_OPERATION_OK : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   Read DESFIRE EV1 manual, chapter 9.4.9 for details.
     *   DO NOT USE THIS COMMAND unless you're really sure you know
     *   what you're doing!!!
     *
     **/
		public long SetConfiguration(byte option, byte[] data, byte length)
		{
			xfer_length = 0;
    
			/* Create the info block containing the command code and the key number argument. */
			xfer_buffer[xfer_length++] = DF_SET_CONFIGURATION;
			xfer_buffer[xfer_length++] = option;
    
			if (data != null) {
				Array.ConstrainedCopy(data, 0, xfer_buffer, (int)xfer_length, length);
				xfer_length += length;
			}
    
			/* Add the CRC */
			XferAppendCrc(2);
    
			/* Cipher the option, the data and the CRC */
			XferCipherSend(2);
    
			return Command(1, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
		}

    
	}
}
