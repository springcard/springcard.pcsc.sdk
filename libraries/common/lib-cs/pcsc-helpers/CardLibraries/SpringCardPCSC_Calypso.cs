/**
 *
 * \defgroup CalypsoWrapper
 *
 * \brief .NET wrapper for the pcsc_calypso.dll native library
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

using SCARD_CALYPSO_CTX = System.IntPtr;

namespace SpringCard.PCSC.CardLibraries.Calypso
{
	public class SCardCalypso
	{
		private SCARD_CALYPSO_CTX _ctx;
		private int _rc = 0;
		private bool _activated = false;
		
		public int GetLastError()
		{
			return _rc;
		}
		
		public bool Activated()
		{
			return _activated;
		}
		
		public SCardCalypso()
		{
			_ctx = CreateContext();
		}

		public SCardCalypso(SCardChannel scard)
		{
			_ctx = CreateContext();
			
			_rc = CardBindPcsc(_ctx, scard.hCard);
			if (_rc == 0)
			{
				_rc = CardActivate(_ctx, null, 0);
				if (_rc == 0)
				{
					_activated = true;
				}
			}
		}
		
		~SCardCalypso()
		{
			DestroyContext(_ctx);
		}
		
		public string ParseToXml()
		{
			uint i, size = 256*1024;
			byte[] output = new byte[size];
			
			SetXmlOutputStr(_ctx, output, size);
			
			_rc = ExploreAndParse(_ctx);
			if (_rc != 0)
				return "";
			
			ClearOutput(_ctx);
			
			string result = "";
			for (i=0; i<size; i++)
			{
				if (output[i] == 0) break;
				result = result + (char) output[i];
			}
			
			return result;
		}
		
		
		
		public struct CALYPSO_CD97_FILE_INFO_ST
		{
			public byte  ShortId;
			public byte  Type;
			public byte  SubType;
			public byte  RecSize;
			public byte  NumRec;
			public uint AC;
			public uint NKey;
			public byte Status;
			public byte[] KVC;
		}


		[DllImport
		 ("pcsc_calypso.dll", CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoBench")] public static extern int Bench(int reset);

		[DllImport
		 ("pcsc_calypso.dll", CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSetTraceLevel")] public static extern int SetTraceLevel(byte level);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSetTraceFile")] public static extern int SetTraceFile(string filename);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoGetLastError")] public static extern int GetLastError(SCARD_CALYPSO_CTX p_ctx);



		#region context manipulation
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCreateContext")] public static extern SCARD_CALYPSO_CTX CreateContext();
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoDestroyContext")] public static extern void DestroyContext(SCARD_CALYPSO_CTX p_ctx);
		
		#endregion

		#region access to the card and the SAM
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardBindPcsc")] public static extern int CardBindPcsc(SCARD_CALYPSO_CTX p_ctx, System.IntPtr hCard);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardTransmit")] public static extern int CardTransmit(SCARD_CALYPSO_CTX p_ctx,
		                                                              byte[] in_buffer,
		                                                              uint in_length,
		                                                              byte[] out_buffer,
		                                                              ref uint out_length);
		
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamBindPcsc")] public static extern int SamBindPcsc(SCARD_CALYPSO_CTX p_ctx, System.IntPtr hCard);

		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamTransmit")] public static extern int SamTransmit(SCARD_CALYPSO_CTX p_ctx,
		                                                            byte[] in_buffer,
		                                                            uint in_length,
		                                                            byte[] out_buffer,
		                                                            ref uint out_length);
		#endregion

		#region card manipulation
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardGetAtr")] public static extern int CardGetAtr(SCARD_CALYPSO_CTX p_ctx, byte[] atr, ref uint atrsize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardSelectApplication")] public static extern int CardSelectApplication(SCARD_CALYPSO_CTX p_ctx, byte[] aid, uint aidsize, byte[] fci, ref uint fcisize);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardSelectFile")] public static extern int CardSelectFile(SCARD_CALYPSO_CTX p_ctx, uint file_id, byte[] resp, ref uint respsize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardReadRecord")] public static extern int CardReadRecord(SCARD_CALYPSO_CTX p_ctx, byte rec_no, byte rec_size, byte[] data, ref uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardAppendRecord")] public static extern int CardAppendRecord(SCARD_CALYPSO_CTX p_ctx, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardUpdateRecord")] public static extern int CardUpdateRecord(SCARD_CALYPSO_CTX p_ctx, byte rec_no, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardDecrease")] public static extern int CardDecrease(SCARD_CALYPSO_CTX p_ctx, byte rec_no, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardOpenSecureSession")] public static extern int CardOpenSecureSession(SCARD_CALYPSO_CTX p_ctx, byte apdu_p1, byte apdu_p2, byte[] sam_chal,byte[] resp, ref uint respsize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardCloseSecureSession")] public static extern int CardCloseSecureSession(SCARD_CALYPSO_CTX p_ctx, int ratify_now, byte[] sam_sign, byte[] resp, ref uint respsize);

		#endregion

		#region card related information
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardActivate")] public static extern int CardActivate(SCARD_CALYPSO_CTX p_ctx, byte[] aid, uint aidsize);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardDispose")] public static extern int CardDispose(SCARD_CALYPSO_CTX p_ctx);
		
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardRevision")] public static extern int CardRevision(SCARD_CALYPSO_CTX p_ctx, ref uint revision);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardDFName")] public static extern int CardDFName(SCARD_CALYPSO_CTX p_ctx, byte[] name, ref uint namesize);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCardSessionModifs")] public static extern int CardSessionModifs(SCARD_CALYPSO_CTX p_ctx, ref uint cur_mods, ref uint max_mods);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoTranslateCD97SelectResp")] public static extern int TranslateCD97SelectResp(SCARD_CALYPSO_CTX p_ctx, byte[] resp, uint respsize, ref CALYPSO_CD97_FILE_INFO_ST file_info);

		#endregion

		#region SAM manipulation
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamSelectDiversifier")] public static extern int SamSelectDiversifier(SCARD_CALYPSO_CTX p_ctx, byte[] card_uid);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamGetChallenge")] public static extern int SamGetChallenge(SCARD_CALYPSO_CTX p_ctx, byte[] sam_chal);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamDigestInit")] public static extern int SamDigestInit(SCARD_CALYPSO_CTX p_ctx, byte kif, byte kvc, byte[] card_resp_buffer, uint card_resp_length);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamDigestUpdate")] public static extern int SamDigestUpdate(SCARD_CALYPSO_CTX p_ctx, byte[] card_buffer, uint card_buflen);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamDigestClose")] public static extern int SamDigestClose(SCARD_CALYPSO_CTX p_ctx, byte[] sam_sign);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamDigestAuthenticate")] public static extern int SamDigestAuthenticate(SCARD_CALYPSO_CTX p_ctx, byte[] card_sign);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamDispose")] public static extern int SamDispose(SCARD_CALYPSO_CTX p_ctx);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamSetAutoUpdate")] public static extern int SamSetAutoUpdate(SCARD_CALYPSO_CTX p_ctx, int enable);
		
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSamSetCommSpeed")] public static extern int SamSetCommSpeed(SCARD_CALYPSO_CTX p_ctx, int fast);
		#endregion

		#region all in one transaction management
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoStartTransaction")] public static extern int StartTransaction(SCARD_CALYPSO_CTX p_ctx, byte key_no);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoStartTransactionEx")] public static extern int StartTransactionEx(SCARD_CALYPSO_CTX p_ctx, byte key_no, byte kif);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCommitTransaction")] public static extern int CommitTransaction(SCARD_CALYPSO_CTX p_ctx, int ratify_now);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoCancelTransaction")] public static extern int CancelTransaction(SCARD_CALYPSO_CTX p_ctx);

		/* Shortcuts */
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoReadRecordTransaction")] public static extern int ReadRecordTransaction(SCARD_CALYPSO_CTX p_ctx, byte key_no, byte rec_no, byte rec_size, byte[] data, ref uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoAppendRecordTransaction")] public static extern int AppendRecordTransaction(SCARD_CALYPSO_CTX p_ctx, byte key_no, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoUpdateRecordTransaction")] public static extern int UpdateRecordTransaction(SCARD_CALYPSO_CTX p_ctx, byte key_no, byte rec_no, byte[] data, uint datasize);

		/* Unit test */
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoReadRecordTransactionE")] public static extern int ReadRecordTransactionE(SCARD_CALYPSO_CTX p_ctx, uint file_id, byte key_no, byte rec_no, byte[] data, ref uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoAppendRecordTransactionE")] public static extern int AppendRecordTransactionE(SCARD_CALYPSO_CTX p_ctx, uint file_id, byte key_no, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoUpdateRecordTransactionE")] public static extern int CalypsoUpdateRecordTransactionE(SCARD_CALYPSO_CTX p_ctx, uint file_id, byte key_no, byte rec_no, byte[] data, uint datasize);

		// Parser
		// ------
		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSetXmlOutput")] public static extern int SetXmlOutput(SCARD_CALYPSO_CTX p_ctx, string filename);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSetIniOutput")] public static extern int SetIniOutput(SCARD_CALYPSO_CTX p_ctx, string filename);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSetXmlOutputStr")] public static extern int SetXmlOutputStr(SCARD_CALYPSO_CTX p_ctx, byte [] target, uint length);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoSetIniOutputStr")] public static extern int SetIniOutputStr(SCARD_CALYPSO_CTX p_ctx, byte [] target, uint length);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoClearOutput")] public static extern int ClearOutput(SCARD_CALYPSO_CTX p_ctx);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoParseCardAtr")] public static extern int ParseCardAtr(SCARD_CALYPSO_CTX p_ctx, byte[] atr, uint atrsize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoParseFci")] public static extern int ParseFci(SCARD_CALYPSO_CTX p_ctx, byte[] fci, uint fcisize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoParseCD97SelectResp")] public static extern int ParseCD97SelectResp(SCARD_CALYPSO_CTX p_ctx, byte[] resp, uint respsize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoParseEnvironmentRecord")] public static extern int ParseEnvironmentRecord(SCARD_CALYPSO_CTX p_ctx, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoParseTransportLogRecord")] public static extern int ParseTransportLogRecord(SCARD_CALYPSO_CTX p_ctx, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoParseContractRecord")] public static extern int ParseContractRecord(SCARD_CALYPSO_CTX p_ctx, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoParseCountersRecord")] public static extern int ParseCountersRecord(SCARD_CALYPSO_CTX p_ctx, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoParseEventRecord")] public static extern int ParseEventRecord(SCARD_CALYPSO_CTX p_ctx, byte[] data, uint datasize);

		[DllImport
		 ("pcsc_calypso.dll", CharSet = CharSet.Ansi, CallingConvention =
		  CallingConvention.Cdecl, EntryPoint =
		  "CalypsoExploreAndParse")] public static extern int ExploreAndParse(SCARD_CALYPSO_CTX p_ctx);

		#endregion

		#region constants
		public const int S_SUCCESS               = 0;
		/* Definition of Calypso keys */
		/* -------------------------- */
		public const int CALYPSO_ISSUER_KEY      = 1;
		public const int CALYPSO_LOAD_KEY        = 2;
		public const int CALYPSO_DEBIT_KEY       = 3;
		
		/* Definition of Calypso error codes */
		/* --------------------------------- */

		public const short CALYPSO_ERR_INVALID_CONTEXT    = 0x0001;
		public const short CALYPSO_ERR_INVALID_PARAM      = 0x0002;
		public const short CALYPSO_ERR_INVALID_TARGET     = 0x0003;

		public const short CALYPSO_ERR_COMMAND_OVERFLOW   = 0x0010;
		public const short CALYPSO_ERR_RESPONSE_OVERFLOW  = 0x0011;
		public const short CALYPSO_ERR_INTERNAL_OVERFLOW  = 0x0012;
		public const short CALYPSO_ERR_INTERNAL_NULL_PTR  = 0x0013;

		public const short CALYPSO_INF_TRANSACTION_LIMIT  = 0x0020;
		public const short CALYPSO_ERR_ALREADY_TRANSACTED = 0x0021;
		public const short CALYPSO_ERR_NOT_TRANSACTED     = 0x0022;

		public const short CALYPSO_ERR_NOT_IMPLEMENTED    = 0x0100;
		public const short CALYPSO_ERR_KIF_UNKNOWN        = 0x0101;

		public const short CALYPSO_ERR_STATUS_WORD        = 0x0200;
		public const short CALYPSO_ERR_RESP_TOO_SHORT     = 0x0201;
		public const short CALYPSO_ERR_RESP_TOO_LONG      = 0x0202;
		public const short CALYPSO_SW_WRONG_P1P2          = 0x0203;
		public const short CALYPSO_SW_WRONG_P3            = 0x0204;
		public const short CALYPSO_SW_NO_RESPONSE         = 0x0205;

		public const short CALYPSO_ERR_NO_CARD            = 0x0400;

		public const short CALYPSO_CARD_NOT_ACTIVATED     = 0x0410;
		public const short CALYPSO_CARD_NOT_SUPPORTED     = 0x0411;
		public const short CALYPSO_CARD_ATR_INVALID       = 0x0412;
		public const short CALYPSO_CARD_FCI_INVALID       = 0x0413;

		public const short CALYPSO_CARD_FILE_INFO_INVALID = 0x0420;
		public const short CALYPSO_CARD_DATA_MALFORMED    = 0x0421;
		public const short CALYPSO_CARD_DATA_OVERSIZED    = 0x0422;

		public const short CALYPSO_CARD_EEPROM_PROBLEM    = 0x0430;
		public const short CALYPSO_CARD_FILE_INVALIDATED  = 0x0431;
		public const short CALYPSO_CARD_ACCESS_FORBIDDEN  = 0x0432;
		public const short CALYPSO_CARD_FILE_NOT_FOUND    = 0x0433;
		public const short CALYPSO_CARD_RECORD_NOT_FOUND  = 0x0434;
		public const short CALYPSO_CARD_WRONG_FILE_TYPE   = 0x0435;
		public const short CALYPSO_CARD_NOT_IN_SESSION    = 0x0436;
		public const short CALYPSO_CARD_IN_SESSION        = 0x0437;
		public const short CALYPSO_CARD_NO_SUCH_KEY       = 0x0438;

		public const short CALYPSO_CARD_DENIED_SAM_SIGN   = 0x0440;

		public const short CALYPSO_ERR_NO_SAM             = 0x0500;

		public const short CALYPSO_SAM_IS_LOCKED          = 0x0531;
		public const short CALYPSO_SAM_COUNTER_LIMIT      = 0x0532;
		public const short CALYPSO_SAM_KEY_NOT_USABLE     = 0x0535;
		public const short CALYPSO_SAM_NOT_IN_SESSION     = 0x0536;
		public const short CALYPSO_SAM_NO_SUCH_KEY        = 0x0538;

		public const short CALYPSO_SAM_DENIED_CARD_SIGN   = 0x0540;

		public const short CALYPSO_ERR_FATAL_             = 0x0800;

		public const short CALYPSO_ERR_CARD_ABSENT        = 0x0C01;
		public const short CALYPSO_ERR_CARD_REMOVED       = 0x0C02;
		public const short CALYPSO_ERR_CARD_MUTE          = 0x0C03;
		public const short CALYPSO_ERR_CARD_DIALOG        = 0x0C04;

		public const short CALYPSO_ERR_SAM_ABSENT         = 0x0D01;
		public const short CALYPSO_ERR_SAM_REMOVED        = 0x0D02;
		public const short CALYPSO_ERR_SAM_MUTE           = 0x0D03;
		public const short CALYPSO_ERR_SAM_DIALOG         = 0x0D04;

		#endregion
	}

}
