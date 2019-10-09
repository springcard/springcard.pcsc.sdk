/**
 *
 * \defgroup DesfireWrapper
 *
 * \brief .NET wrapper for the pcsc_desfire.dll native library
 *
 * \author
 *   Johann.D et al. / SpringCard
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Runtime.InteropServices;
using SpringCard.PCSC;

namespace SpringCard.PCSC.CardLibraries.Desfire
{
	public abstract partial class SCARD_DESFIRE
	{

		#region Library entry points

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_AttachLibrary")]
		public static extern int AttachLibrary(IntPtr hCard);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_DetachLibrary")]
		public static extern int DetachLibrary(IntPtr hCard);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoWrapping")]
		public static extern int IsoWrapping(IntPtr hCard, byte mode);

        /*
		 * Library helpers
		 * ---------------
		 */

        [DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetLibraryVersion", CharSet=CharSet.Ansi)]
		public static extern string GetLibraryVersion();
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetErrorMessage", CharSet=CharSet.Ansi)]
		public static extern string GetErrorMessage(int status);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ExplainDataFileSettings")]
		public static extern int ExplainDataFileSettings(byte[] additionnal_settings_array,
		                                                 ref UInt32 eFileSize);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ExplainValueFileSettings")]
		public static extern int ExplainValueFileSettings(byte[] additionnal_settings_array,
		                                                  ref int lLowerLimit,
		                                                  ref int lUpperLimit,
		                                                  ref UInt32 eLimitedCredit,
		                                                  ref byte bLimitedCreditEnabled);
		
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ExplainRecordFileSettings")]
		public static extern int ExplainRecordFileSettings(byte[] additionnal_settings_array,
		                                                   ref UInt32 eRecordSize,
		                                                   ref UInt32 eMaxNRecords,
		                                                   ref UInt32 eCurrNRecords);
		

		/*
		 * Desfire EV0 functions
		 * ---------------------
		 */
		
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_Authenticate")]
		public static extern int Authenticate(IntPtr hCard,
		                                      byte bKeyNumber,
		                                      byte[] pbAccessKey);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ChangeKeySettings")]
		public static extern int ChangeKeySettings(IntPtr hCard,
		                                           byte key_settings);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetKeySettings")]
		public static extern int GetKeySettings(IntPtr hCard,
		                                        ref byte key_settings,
		                                        ref byte key_count);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ChangeKey")]
		public static extern int ChangeKey(IntPtr hCard,
		                                   byte key_number,
		                                   byte[] new_key,
		                                   byte[] old_key);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetKeyVersion")]
		public static extern int GetKeyVersion(IntPtr hCard,
		                                       byte bKeyNumber,
		                                       ref byte pbKeyVersion);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_FormatPICC")]
		public static extern int FormatPICC(IntPtr hCard);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateApplication")]
		public static extern int CreateApplication(IntPtr hCard,
		                                           UInt32 aid,
		                                           byte key_setting_1,
		                                           byte key_setting_2);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_DeleteApplication")]
		public static extern int DeleteApplication(IntPtr hCard,
		                                           UInt32 aid);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetApplicationIDs")]
		public static extern int GetApplicationIDs(IntPtr hCard,
		                                           byte aid_max_count,
		                                           UInt32[] aid_list,
		                                           ref byte aid_count);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SelectApplication")]
		public static extern int SelectApplication(IntPtr hCard,
		                                           UInt32 aid);
		
		/*
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetVersion")]
			public static extern int GetVersion(IntPtr hCard,
				DF_VERSION_INFO *pVersionInfo);
		 */
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetVersion")]
		public static extern int GetVersion(IntPtr hCard,
		                                    byte[] pVersionInfo);
		
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetFileIDs")]
		public static extern int GetFileIDs(IntPtr hCard,
		                                    byte fid_max_count,
		                                    byte[] fid_list,
		                                    ref byte fid_count);
		
		
		/*
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetFileSettings")]
			public static extern int GetFileSettings(IntPtr hCard,
				byte file_id,
				ref byte file_type,
				ref byte comm_mode,
				ref ushort access_rights,
				DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
		 */
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ChangeFileSettings")]
		public static extern int ChangeFileSettings(IntPtr hCard,
		                                            byte file_id,
		                                            byte comm_mode,
		                                            ushort access_rights);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateStdDataFile")]
		public static extern int CreateStdDataFile(IntPtr hCard,
		                                           byte file_id,
		                                           byte comm_mode,
		                                           ushort access_rights,
		                                           UInt32 file_size);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateBackupDataFile")]
		public static extern int CreateBackupDataFile(IntPtr hCard,
		                                              byte file_id,
		                                              byte comm_mode,
		                                              ushort access_rights,
		                                              UInt32 file_size);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateValueFile")]
		public static extern int CreateValueFile(IntPtr hCard,
		                                         byte file_id,
		                                         byte comm_mode,
		                                         ushort access_rights,
		                                         int lower_limit,
		                                         int upper_limit,
		                                         int initial_value,
		                                         byte limited_credit_enabled);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateLinearRecordFile")]
		public static extern int CreateLinearRecordFile(IntPtr hCard,
		                                                byte file_id,
		                                                byte comm_mode,
		                                                ushort access_rights,
		                                                UInt32 record_size,
		                                                UInt32 max_records);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateCyclicRecordFile")]
		public static extern int CreateCyclicRecordFile(IntPtr hCard,
		                                                byte file_id,
		                                                byte comm_mode,
		                                                ushort access_rights,
		                                                UInt32 record_size,
		                                                UInt32 max_records);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_DeleteFile")]
		public static extern int DeleteFile(IntPtr hCard,
		                                    byte file_id);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ReadData")]
		public static extern int ReadData(IntPtr hCard,
		                                  byte file_id,
		                                  byte comm_mode,
		                                  UInt32 from_offset,
		                                  UInt32 max_count,
		                                  byte[] data,
		                                  ref UInt32 done_count);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ReadData2")]
		public static extern int ReadData2(IntPtr hCard,
		                                   byte file_id,
		                                   UInt32 from_offset,
		                                   UInt32 max_count,
		                                   byte[] data,
		                                   ref UInt32 done_count);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_WriteData")]
		public static extern int WriteData(IntPtr hCard,
		                                   byte file_id,
		                                   byte comm_mode,
		                                   UInt32 from_offset,
		                                   UInt32 size,
		                                   byte[] data);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_WriteData2")]
		public static extern int WriteData2(IntPtr hCard,
		                                    byte file_id,
		                                    UInt32 from_offset,
		                                    UInt32 size,
		                                    byte[] data);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetValue")]
		public static extern int GetValue(IntPtr hCard,
		                                  byte file_id,
		                                  byte comm_mode,
		                                  ref int value);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetValue2")]
		public static extern int GetValue2(IntPtr hCard,
		                                   byte file_id,
		                                   ref int value);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ReadRecords")]
		public static extern int ReadRecords(IntPtr hCard,
		                                     byte file_id,
		                                     byte comm_mode,
		                                     UInt32 from_record,
		                                     UInt32 max_record_count,
		                                     UInt32 record_size,
		                                     byte[] data,
		                                     ref UInt32 record_count);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ReadRecords2")]
		public static extern int ReadRecords2(IntPtr hCard,
		                                      byte file_id,
		                                      UInt32 from_record,
		                                      UInt32 max_record_count,
		                                      UInt32 record_size,
		                                      byte[] data,
		                                      ref UInt32 record_count);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_WriteRecord")]
		public static extern int WriteRecord(IntPtr hCard,
		                                     byte file_id,
		                                     byte comm_mode,
		                                     UInt32 from_offset,
		                                     UInt32 size,
		                                     byte[] data);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_WriteRecord2")]
		public static extern int WriteRecord2(IntPtr hCard,
		                                      byte file_id,
		                                      UInt32 from_offset,
		                                      UInt32 size,
		                                      byte[] data);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_LimitedCredit")]
		public static extern int LimitedCredit(IntPtr hCard,
		                                       byte file_id,
		                                       byte comm_mode,
		                                       int amount);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_LimitedCredit2")]
		public static extern int LimitedCredit2(IntPtr hCard,
		                                        byte file_id,
		                                        int amount);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_Credit")]
		public static extern int Credit(IntPtr hCard,
		                                byte file_id,
		                                byte comm_mode,
		                                int amount);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_Credit2")]
		public static extern int Credit2(IntPtr hCard,
		                                 byte file_id,
		                                 int amount);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_Debit")]
		public static extern int Debit(IntPtr hCard,
		                               byte file_id,
		                               byte comm_mode,
		                               int amount);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_Debit2")]
		public static extern int Debit2(IntPtr hCard,
		                                byte file_id,
		                                int amount);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ClearRecordFile")]
		public static extern int ClearRecordFile(IntPtr hCard,
		                                         byte file_id);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CommitTransaction")]
		public static extern int CommitTransaction(IntPtr hCard);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_AbortTransaction")]
		public static extern int AbortTransaction(IntPtr hCard);
		
		
		/*
		 * Desfire EV1 functions
		 * ---------------------
		 */
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_AuthenticateIso")]
		public static extern int AuthenticateIso(IntPtr hCard,
		                                         byte bKeyNumber,
		                                         byte[] pbAccessKey);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_AuthenticateIso24")]
		public static extern int AuthenticateIso24(IntPtr hCard,
		                                           byte bKeyNumber,
		                                           byte[] pbAccessKey);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_AuthenticateAes")]
		public static extern int AuthenticateAes(IntPtr hCard,
		                                         byte bKeyNumber,
		                                         byte[] pbAccessKey);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ChangeKey24")]
		public static extern int ChangeKey24(IntPtr hCard,
		                                     byte key_number,
		                                     byte[] new_key,
		                                     byte[] old_key);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_ChangeKeyAes")]
		public static extern int ChangeKeyAes(IntPtr hCard,
		                                      byte key_number,
		                                      byte key_version,
		                                      byte[] new_key,
		                                      byte[] old_key);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetFreeMemory")]
		public static extern int GetFreeMemory(IntPtr hCard,
		                                       ref UInt32 pdwFreeBytes);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SetConfiguration")]
		public static extern int SetConfiguration(IntPtr hCard,
		                                          byte option,
		                                          byte[] data,
		                                          byte length);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetCardUID")]
		public static extern int GetCardUID(IntPtr hCard,
		                                    byte[] uid);

		
		
		/*
		 * Desfire ISO-related functions
		 * -----------------------------
		 */
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateIsoApplication")]
		public static extern int CreateIsoApplication(IntPtr hCard,
		                                              UInt32 aid,
		                                              byte key_settings,
		                                              byte keys_count,
		                                              ushort iso_df_id,
		                                              byte[] iso_df_name,
		                                              byte iso_df_namelen);
		
		/*
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetIsoApplications")]
			public static extern int GetIsoApplications(IntPtr hCard,
				byte app_max_count,
				DF_ISO_APPLICATION_ST *app_list,
				ref byte app_count);
		 */
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_GetIsoFileIDs")]
		public static extern int GetIsoFileIDs(IntPtr hCard,
		                                       byte fid_max_count,
		                                       ushort[] fid_list,
		                                       ref byte fid_count);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateIsoStdDataFile")]
		public static extern int CreateIsoStdDataFile(IntPtr hCard,
		                                              byte file_id,
		                                              ushort iso_ef_id,
		                                              byte comm_mode,
		                                              ushort access_rights,
		                                              UInt32 file_size);
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateIsoBackupDataFile")]
		public static extern int CreateIsoBackupDataFile(IntPtr hCard,
		                                                 byte file_id,
		                                                 ushort iso_ef_id,
		                                                 byte comm_mode,
		                                                 ushort access_rights,
		                                                 UInt32 file_size);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateIsoLinearRecordFile")]
		public static extern int CreateIsoLinearRecordFile(IntPtr hCard,
		                                                   byte file_id,
		                                                   ushort iso_ef_id,
		                                                   byte comm_mode,
		                                                   ushort access_rights,
		                                                   UInt32 record_size,
		                                                   UInt32 max_records);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_CreateIsoCyclicRecordFile")]
		public static extern int CreateIsoCyclicRecordFile(IntPtr hCard,
		                                                   byte file_id,
		                                                   ushort iso_ef_id,
		                                                   byte comm_mode,
		                                                   ushort access_rights,
		                                                   UInt32 record_size,
		                                                   UInt32 max_records);
		

		
		/*
		 * Desfire ISO functions
		 * ---------------------
		 */
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoApdu")]
		public static extern int IsoApdu(IntPtr hCard,
		                                 byte INS,
		                                 byte P1,
		                                 byte P2,
		                                 byte Lc,
		                                 byte[] data_in,
		                                 byte Le,
		                                 byte[] data_out,
		                                 ref ushort data_out_len,
		                                 ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoSelectApplet")]
		public static extern int IsoSelectApplet(IntPtr hCard,
		                                         ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoSelectDF")]
		public static extern int IsoSelectDF(IntPtr hCard,
		                                     ushort fid,
		                                     byte[] fci,
		                                     ushort fci_max_length,
		                                     ref ushort fci_length,
		                                     ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoSelectDFName")]
		public static extern int IsoSelectDFName(IntPtr hCard,
		                                         byte[] df_name,
		                                         byte df_name_len,
		                                         byte[] fci,
		                                         ushort fci_max_length,
		                                         ref ushort fci_length,
		                                         ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoSelectEF")]
		public static extern int IsoSelectEF(IntPtr hCard,
		                                     ushort fid,
		                                     ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoReadBinary")]
		public static extern int IsoReadBinary(IntPtr hCard,
		                                       ushort offset,
		                                       byte[] data,
		                                       byte want_length,
		                                       ref ushort length,
		                                       ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoUpdateBinary")]
		public static extern int IsoUpdateBinary(IntPtr hCard,
		                                         ushort offset,
		                                         byte[] data,
		                                         byte length,
		                                         ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoReadRecord")]
		public static extern int IsoReadRecord(IntPtr hCard,
		                                       byte number,
		                                       bool read_all,
		                                       byte[] data,
		                                       ushort max_length,
		                                       ref ushort length,
		                                       ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoAppendRecord")]
		public static extern int IsoAppendRecord(IntPtr hCard,
		                                         byte[] data,
		                                         byte length,
		                                         ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoGetChallenge")]
		public static extern int IsoGetChallenge(IntPtr hCard,
		                                         byte chal_size,
		                                         byte[] card_chal_1,
		                                         ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoExternalAuthenticate")]
		public static extern int IsoExternalAuthenticate(IntPtr hCard,
		                                                 byte key_algorithm,
		                                                 byte key_reference,
		                                                 byte chal_size,
		                                                 byte[] card_chal_1,
		                                                 byte[] host_chal_1,
		                                                 byte[] key_value,
		                                                 ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoInternalAuthenticate")]
		public static extern int IsoInternalAuthenticate(IntPtr hCard,
		                                                 byte key_algorithm,
		                                                 byte key_reference,
		                                                 byte chal_size,
		                                                 byte[] host_chal_2,
		                                                 byte[] card_chal_2,
		                                                 byte[] key_value,
		                                                 ref ushort SW);
		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_IsoMutualAuthenticate")]
		public static extern int IsoMutualAuthenticate(IntPtr hCard,
		                                               byte key_algorithm,
		                                               byte key_reference,
		                                               byte[] key_value,
		                                               ref ushort SW);

		#endregion
		
		#region Working with the NXP SAM AV2

		
		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_AttachSAM")]
		public static extern int AttachSAM(IntPtr hCard,
		                                   IntPtr hSam);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_DetachSAM")]
		public static extern int DetachSAM(IntPtr hCard,
		                                   IntPtr hSam,
		                                   UInt32 dwDisposition);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_Unlock")]
		public static extern int SAM_Unlock(IntPtr hCard,
		                                    byte bKeyNumberSam,
		                                    byte bKeyVersion,
		                                    byte[] pbKeyValue);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_SelectApplication")]
		public static extern int SAM_SelectApplication(IntPtr hCard,
		                                               UInt32 aid);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_AuthenticateEx")]
		public static extern int SAM_AuthenticateEx(IntPtr hCard,
		                                            byte bAuthMethod,
		                                            byte bKeyNumberCard,
		                                            byte bSamParamP1,
		                                            byte bSamParamP2,
		                                            byte bKeyNumberSam,
		                                            byte bKeyVersion,
		                                            byte[] pbDivInp,
		                                            byte bDivInpLength);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_Authenticate")]
		public static extern int SAM_Authenticate(IntPtr hCard,
		                                          byte bKeyNumberCard,
		                                          bool fApplicationKeyNo,
		                                          byte bKeyNumberSam,
		                                          byte bKeyVersion,
		                                          byte[] pbDivInp,
		                                          byte bDivInpLength,
		                                          bool fDivAv2Mode,
		                                          bool fDivTwoRounds);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_AuthenticateIso")]
		public static extern int SAM_AuthenticateIso(IntPtr hCard,
		                                             byte bKeyNumberCard,
		                                             bool fApplicationKeyNo,
		                                             byte bKeyNumberSam,
		                                             byte bKeyVersion,
		                                             byte[] pbDivInp,
		                                             byte bDivInpLength,
		                                             bool fDivAv2Mode,
		                                             bool fDivTwoRounds);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_AuthenticateAes")]
		public static extern int SAM_AuthenticateAes(IntPtr hCard,
		                                             byte bKeyNumberCard,
		                                             bool fApplicationKeyNo,
		                                             byte bKeyNumberSam,
		                                             byte bKeyVersion,
		                                             byte[] pbDivInp,
		                                             byte bDivInpLength,
		                                             bool fDivAv2Mode,
		                                             bool fDivTwoRounds);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_ChangeKeyEx")]
		public static extern int SAM_ChangeKeyEx(IntPtr hCard,
		                                         byte bKeyNumberCard,
		                                         byte bSamParamP1,
		                                         byte bSamParamP2,
		                                         byte bOldKeyNumberSam,
		                                         byte bOldKeyVersion,
		                                         byte bNewKeyNumberSam,
		                                         byte bNewKeyVersion,
		                                         byte[] pbDivInp,
		                                         byte bDivInpLength);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_ChangeKey1")]
		public static extern int SAM_ChangeKey1(IntPtr hCard,
		                                        byte bKeyNumberCard,
		                                        bool fIsCardMasterKey,
		                                        byte bNewKeyNumberSam,
		                                        byte bNewKeyVersion,
		                                        byte[] pbDivInp,
		                                        byte bDivInpLength,
		                                        bool fDivAv2Mode,
		                                        bool fNewDivEnable,
		                                        bool fNewDivTwoRounds);

		[DllImport("pcsc_desfire.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCardDesfire_SAM_ChangeKey2")]
		public static extern int SAM_ChangeKey2(IntPtr hCard,
		                                        byte bKeyNumberCard,
		                                        byte bOldKeyNumberSam,
		                                        byte bOldKeyVersion,
		                                        byte bNewKeyNumberSam,
		                                        byte bNewKeyVersion,
		                                        byte[] pbDivInp,
		                                        byte bDivInpLength,
		                                        bool fDivAv2Mode,
		                                        bool fOldDivEnable,
		                                        bool fOldDivTwoRounds,
		                                        bool fNewDivEnable,
		                                        bool fNewDivTwoRounds);
		
		#endregion
		
	}
}
