/**
 *
 * \ingroup PCSC
 *
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D, Emilie.C and Jerome.I / SpringCard
 *
 */
/*
 *	This software is part of the SPRINGCARD SDK FOR PC/SC
 *
 *   Redistribution and use in source (source code) and binary
 *   (object code) forms, with or without modification, are
 *   permitted provided that the following conditions are met :
 *
 *   1. Redistributed source code or object code shall be used
 *   only in conjunction with products (hardware devices) either
 *   manufactured, distributed or developed by SPRINGCARD,
 *
 *   2. Redistributed source code, either modified or
 *   un-modified, must retain the above copyright notice,
 *   this list of conditions and the disclaimer below,
 *
 *   3. Redistribution of any modified code must be clearly
 *   identified "Code derived from original SPRINGCARD 
 *   copyrighted source code", with a description of the
 *   modification and the name of its author,
 *
 *   4. Redistributed object code must reproduce the above
 *   copyright notice, this list of conditions and the
 *   disclaimer below in the documentation and/or other
 *   materials provided with the distribution,
 *
 *   5. The name of SPRINGCARD may not be used to endorse
 *   or promote products derived from this software or in any
 *   other form without specific prior written permission from
 *   SPRINGCARD.
 *
 *   THIS SOFTWARE IS PROVIDED BY SPRINGCARD "AS IS".
 *   SPRINGCARD SHALL NOT BE LIABLE FOR INFRINGEMENTS OF THIRD
 *   PARTIES RIGHTS BASED ON THIS SOFTWARE.
 *
 *   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 *   FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *
 *   SPRINGCARD DOES NOT WARRANT THAT THE FUNCTIONS CONTAINED IN
 *   THIS SOFTWARE WILL MEET THE USER'S REQUIREMENTS OR THAT THE
 *   OPERATION OF IT WILL BE UNINTERRUPTED OR ERROR-FREE.
 *
 *   IN NO EVENT SHALL SPRINGCARD BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 *   DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 *   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 *   OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 *   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
 *   THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 *   OF SUCH DAMAGE. 
 *
 **/
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	/**
	 * \brief Static class that gives a direct access to PC/SC functions (SCard... provided by winscard.dll or libpcsclite)
	 *
	 **/
	public static partial class SCARD
	{
		/**
		 *
		 * \brief Translate a PC/SC error code (return value of any SCard... function) into its constant name
		 *
		 **/
		public static string ErrorCodeToString(uint code)
		{
			switch (code)
			{
				case SCARD.S_SUCCESS:
					return "SCARD_S_SUCCESS";
				case SCARD.F_INTERNAL_ERROR:
					return "SCARD_F_INTERNAL_ERROR";
				case SCARD.E_CANCELLED:
					return "SCARD_E_CANCELLED";
				case SCARD.E_INVALID_HANDLE:
					return "SCARD_E_INVALID_HANDLE";
				case SCARD.E_INVALID_PARAMETER:
					return "SCARD_E_INVALID_PARAMETER";
				case SCARD.E_INVALID_TARGET:
					return "SCARD_E_INVALID_TARGET";
				case SCARD.E_NO_MEMORY:
					return "SCARD_E_NO_MEMORY";
				case SCARD.F_WAITED_TOO_LONG:
					return "SCARD_F_WAITED_TOO_LONG";
				case SCARD.E_INSUFFICIENT_BUFFER:
					return "SCARD_E_INSUFFICIENT_BUFFER";
				case SCARD.E_UNKNOWN_READER:
					return "SCARD_E_UNKNOWN_READER";
				case SCARD.E_TIMEOUT:
					return "SCARD_E_TIMEOUT";
				case SCARD.E_SHARING_VIOLATION:
					return "SCARD_E_SHARING_VIOLATION";
				case SCARD.E_NO_SMARTCARD:
					return "SCARD_E_NO_SMARTCARD";
				case SCARD.E_UNKNOWN_CARD:
					return "SCARD_E_UNKNOWN_CARD";
				case SCARD.E_CANT_DISPOSE:
					return "SCARD_E_CANT_DISPOSE";
				case SCARD.E_PROTO_MISMATCH:
					return "SCARD_E_PROTO_MISMATCH";
				case SCARD.E_NOT_READY:
					return "SCARD_E_NOT_READY";
				case SCARD.E_INVALID_VALUE:
					return "SCARD_E_INVALID_VALUE";
				case SCARD.E_SYSTEM_CANCELLED:
					return "SCARD_E_SYSTEM_CANCELLED";
				case SCARD.F_COMM_ERROR:
					return "SCARD_F_COMM_ERROR";
				case SCARD.F_UNKNOWN_ERROR:
					return "SCARD_F_UNKNOWN_ERROR";
				case SCARD.E_INVALID_ATR:
					return "SCARD_E_INVALID_ATR";
				case SCARD.E_NOT_TRANSACTED:
					return "SCARD_E_NOT_TRANSACTED";
				case SCARD.E_READER_UNAVAILABLE:
					return "SCARD_E_READER_UNAVAILABLE";
				case SCARD.P_SHUTDOWN:
					return "SCARD_P_SHUTDOWN";
				case SCARD.E_PCI_TOO_SMALL:
					return "SCARD_E_PCI_TOO_SMALL";
				case SCARD.E_READER_UNSUPPORTED:
					return "SCARD_E_READER_UNSUPPORTED";
				case SCARD.E_DUPLICATE_READER:
					return "SCARD_E_DUPLICATE_READER";
				case SCARD.E_CARD_UNSUPPORTED:
					return "SCARD_E_CARD_UNSUPPORTED";
				case SCARD.E_NO_SERVICE:
					return "SCARD_E_NO_SERVICE";
				case SCARD.E_SERVICE_STOPPED:
					return "SCARD_E_SERVICE_STOPPED";
				case SCARD.E_UNEXPECTED:
					return "SCARD_E_UNEXPECTED";
				case SCARD.E_ICC_INSTALLATION:
					return "SCARD_E_ICC_INSTALLATION";
				case SCARD.E_ICC_CREATEORDER:
					return "SCARD_E_ICC_CREATEORDER";
				case SCARD.E_UNSUPPORTED_FEATURE:
					return "SCARD_E_UNSUPPORTED_FEATURE";
				case SCARD.E_DIR_NOT_FOUND:
					return "SCARD_E_DIR_NOT_FOUND";
				case SCARD.E_FILE_NOT_FOUND:
					return "SCARD_E_FILE_NOT_FOUND";
				case SCARD.E_NO_DIR:
					return "SCARD_E_NO_DIR";
				case SCARD.E_NO_FILE:
					return "SCARD_E_NO_FILE";
				case SCARD.E_NO_ACCESS:
					return "SCARD_E_NO_ACCESS";
				case SCARD.E_WRITE_TOO_MANY:
					return "SCARD_E_WRITE_TOO_MANY";
				case SCARD.E_BAD_SEEK:
					return "SCARD_E_BAD_SEEK";
				case SCARD.E_INVALID_CHV:
					return "SCARD_E_INVALID_CHV";
				case SCARD.E_UNKNOWN_RES_MNG:
					return "SCARD_E_UNKNOWN_RES_MNG";
				case SCARD.E_NO_SUCH_CERTIFICATE:
					return "SCARD_E_NO_SUCH_CERTIFICATE";
				case SCARD.E_CERTIFICATE_UNAVAILABLE:
					return "SCARD_E_CERTIFICATE_UNAVAILABLE";
				case SCARD.E_NO_READERS_AVAILABLE:
					return "SCARD_E_NO_READERS_AVAILABLE";
				case SCARD.E_COMM_DATA_LOST:
					return "SCARD_E_COMM_DATA_LOST";
				case SCARD.E_NO_KEY_CONTAINER:
					return "SCARD_E_NO_KEY_CONTAINER";
				//case SCARD.E_SERVER_TOO_BUSY : return "SCARD_E_SERVER_TOO_BUSY";
				case SCARD.W_UNSUPPORTED_CARD:
					return "SCARD_W_UNSUPPORTED_CARD";
				case SCARD.W_UNRESPONSIVE_CARD:
					return "SCARD_W_UNRESPONSIVE_CARD";
				case SCARD.W_UNPOWERED_CARD:
					return "SCARD_W_UNPOWERED_CARD";
				case SCARD.W_RESET_CARD:
					return "SCARD_W_RESET_CARD";
				case SCARD.W_REMOVED_CARD:
					return "SCARD_W_REMOVED_CARD";
				case SCARD.W_SECURITY_VIOLATION:
					return "SCARD_W_SECURITY_VIOLATION";
				case SCARD.W_WRONG_CHV:
					return "SCARD_W_WRONG_CHV";
				case SCARD.W_CHV_BLOCKED:
					return "SCARD_W_CHV_BLOCKED";
				case SCARD.W_EOF:
					return "SCARD_W_EOF";
				case SCARD.W_CANCELLED_BY_USER:
					return "SCARD_W_CANCELLED_BY_USER";
				case SCARD.W_CARD_NOT_AUTHENTICATED:
					return "SCARD_W_CARD_NOT_AUTHENTICATED";
				default:
					return BinConvert.ToHex(code);
			}
		}

		/**
		 *
		 * \brief Translate a PC/SC error code into a user-readable string
		 *
		 **/
		public static string ErrorToString(uint code)
		{
			string r = "";

			try
			{
				if ((code >= SCARD.F_INTERNAL_ERROR) && (code <= SCARD.W_CARD_NOT_AUTHENTICATED))
				{
					if (GetSystem() == System.Win32)
						r = (new Win32Exception((int)code)).Message;

				}
				else
				{
					r = (new Win32Exception((int)code)).Message;
				}
			}
			catch
			{

			}

			if (!string.IsNullOrEmpty(r))
				return r;

			return ErrorCodeToString(code);
		}

		/**
		 *
		 * \brief Translate a PC/SC error code into a user-readable string, including the error code itself
		 *
		 **/
		public static string ErrorToMessage(uint code)
		{
			string error_string = ErrorToString(code);
			return string.Format("{0}/{1:X08} ({2})", (long)code, code, error_string);
		}

		/**
		 *
		 * \brief Translate a PC/SC reader status into a comma-separated set of constant names
		 *
		 **/
		public static string ReaderStatusToString(uint state)
		{
			string r = "";

			if (state == SCARD.STATE_UNAWARE)
				r += ",UNAWARE";

			if ((state & SCARD.STATE_EMPTY) != 0)
				r += ",EMPTY";
			if ((state & SCARD.STATE_PRESENT) != 0)
				r += ",PRESENT";
			if ((state & SCARD.STATE_MUTE) != 0)
				r += ",MUTE";
			if ((state & SCARD.STATE_UNPOWERED) != 0)
				r += ",UNPOWERED";
			if ((state & SCARD.STATE_ATRMATCH) != 0)
				r += ",ATRMATCH";
			if ((state & SCARD.STATE_EXCLUSIVE) != 0)
				r += ",EXCLUSIVE";
			if ((state & SCARD.STATE_INUSE) != 0)
				r += ",INUSE";

			if ((state & SCARD.STATE_IGNORE) != 0)
				r += ",IGNORE";
			if ((state & SCARD.STATE_UNKNOWN) != 0)
				r += ",UNKNOWN";
			if ((state & SCARD.STATE_UNAVAILABLE) != 0)
				r += ",UNAVAILABLE";

			if ((state & SCARD.STATE_CHANGED) != 0)
				r += ",CHANGED";

			if (r.Length >= 1)
				r = r.Substring(1);

			return r;
		}

		/**
		 *
		 * \brief Translate a PC/SC protocol into its text explanation
		 *
		 **/
		public static string CardProtocolToString(uint protocol)
		{
			if (protocol == SCARD.PROTOCOL_NONE)
				return "";
			if (protocol == SCARD.PROTOCOL_T0)
				return "T=0";
			if (protocol == SCARD.PROTOCOL_T1)
				return "T=1";
			if (protocol == (SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1))
				return "T=0 or T=1";
			if (protocol == SCARD.PROTOCOL_RAW)
				return "RAW";

			return "Unknown";
		}

		/**
		 *
		 * \brief Translate a PC/SC share mode into its text explanation
		 *
		 **/
		public static string CardShareModeToString(uint share_mode)
		{
			if (share_mode == SCARD.SHARE_SHARED)
				return "SHARED";
			if (share_mode == SCARD.SHARE_EXCLUSIVE)
				return "EXCLUSIVE";
			if (share_mode == SCARD.SHARE_DIRECT)
				return "DIRECT";

			return "Unknown";
		}


		/**
		 *
		 * \brief Translate a smart card status word into a text explanation
		 *
		 **/
		public static string CardStatusWordsToString(ushort SW)
		{
			byte SW1 = (byte)(SW / 0x0100);
			byte SW2 = (byte)(SW % 0x0100);

			return CardStatusWordsToString(SW1, SW2);
		}

		/**
		 *
		 * \brief Translate a smart card status word into a text explanation
		 *
		 **/
		public static string CardStatusWordsToString(byte SW1, byte SW2)
		{
			switch (SW1)
			{
				case 0x60:
					return "null";

				case 0x61:
					return "Still " + SW2.ToString() +
						" bytes available. Use GET RESPONSE to access this data";

				case 0x62:
					switch (SW2)
					{
						case 0x81:
							return "Warning : returned data may be corrupted";
						case 0x82:
							return "Warning : EoF has been reached before ending";
						case 0x83:
							return "Warning : selected file invalidated";
						case 0x84:
							return "Warning : bad file control information format";
						default:
							return "Warning : state unchanged";
					}

				case 0x63:
					if ((SW2 >= 0xC0) && (SW2 <= 0xCF))
						return "Warning : counter value is " + SW2.ToString();

					if (SW2 == 81)
						return "Warning : file filled up with last write";
					return "Warning : state unchanged";

				case 0x64:
					return "Error : state unchanged";

				case 0x65:
					switch (SW2)
					{
						case 0x01:
							return "Memory failure, problem in writing the EEPROM";
						case 0x81:
							return "Error : memory failure";
						default:
							return "Error : state changed";
					}

				case 0x66:
					return "Security error";

				case 0x67:
					return "Check error - incorrect byte length";

				case 0x68:
					switch (SW2)
					{
						case 0x81:
							return "Check error - logical channel not supported";
						case 0x82:
							return "Check error : secure messaging not supported";
						default:
							return "Check error : request function not supported";
					}

				case 0x69:
					switch (SW2)
					{
						case 0x81:
							return "Check error : command incompatible with file structure";
						case 0x82:
							return "Check error : security status not statisfied";
						case 0x83:
							return "Check error : authentication method blocked";
						case 0x84:
							return "Check error : referenced data invalidated";
						case 0x85:
							return "Check error : conditions of use not satisfied";
						case 0x86:
							return "Check error : command not allowed (no current EF)";
						case 0x87:
							return "Check error : Expected SM data objects missing";
						case 0x88:
							return "Check error : SM data objects incorrect ";
						default:
							return
								"Unknow command, most probably erroneous typing, protocol violation or incorrect format";
					}

				case 0x6A:
					switch (SW2)
					{
						case 0x00:
							return "Check error : P1 or P2 incorrect";
						case 0x80:
							return "Check error : parameters in data field incorrect";
						case 0x81:
							return "Check error : function not supported";
						case 0x82:
							return "Check error : file not found";
						case 0x83:
							return "Check error : record not found";
						case 0x84:
							return "Check error : insufficient memory space in this file";
						case 0x85:
							return "Check error : Lc inconsistant with TLV structure";
						case 0x86:
							return "Check error : inconsistant parameters P1-P2";
						case 0x87:
							return "Check error : P3 inconsistant with P1-P2";
						case 0x88:
							return "Check error : referenced data not found";
						default:
							return "Check error : wrong parameters";
					}

				case 0x6B:
					return "Check error : reference P1,P2 incorrect";

				case 0x6C:
					return "Lc length incorrect, correct length :" + SW2.ToString();

				case 0x6D:
					return "Ins invalid or unsupported";

				case 0x6E:
					return "Cla insupported";

				case 0x6F:
					int rc = 0 - SW2;
					switch (rc)
					{
						/* Error codes taken from SpringProx API */
						case 0:
							return "Undiagnosticed error";
						case -1:
							return "No answer (no card / card is mute)";
						case -2:
							return "Invalid CRC in card's response";
						case -3:
							return "No frame received (NFC mode)";
						case -4:
							return "Card: Authentication failed or access denied";
						case -5:
							return "Invalid parity bit(s) in card's response";
						case -6:
							return "NACK or status indicating error";
						case -7:
							return "Too many anticollision loops";
						case -8:
							return "Wrong LRC in card's serial number";
						case -9:
							return "Card or block locked";
						case -10:
							return "Card: Authentication must be performed first";
						case -11:
							return "Wrong number of bits in card's answer";
						case -12:
							return "Wrong number of bytes in card's answer";
						case -13:
							return "Card: Counter is invalid";
						case -14:
							return "Card: Transaction error";
						case -15:
							return "Card: Write failed";
						case -16:
							return "Card: Counter increase failed";
						case -17:
							return "Card: Counter decrease failed";
						case -18:
							return "Card: Read failed";
						case -19:
							return "RC: FIFO overflow";
						case -20:
							return "Polling mode pending";
						case -21:
							return "Invalid framing in card's response";
						case -22:
							return "Card: Access error (bad address or denied)";
						case -23:
							return "RC: Unknown command";
						case -24:
							return "A collision has occurred";
						case -25:
							return "Command execution failed";
						case -26:
							return "Hardware error";
						case -27:
							return "RC: timeout";
						case -28:
							return "More than one card found, but at least one does not support anticollision";
						case -29:
							return "An external RF field has been detected";
						case -30:
							return "Polling terminated (timeout or break)";
						case -31:
							return "Bogus status in card's response";
						case -32:
							return "Card: Vendor specific error";
						case -33:
							return "Card: Command not supported";
						case -34:
							return "Card: Format of command invalid";
						case -35:
							return "Card: Option(s) of command invalid";
						case -36:
							return "Card: other error";
						case -59:
							return "Command not available in this mode";
						case -60:
							return "Wrong parameter for the command";
						case -71:
							return "No active card with this CID";
						case -75:
							return "Length error in card's ATS";
						case -76:
							return "Error in card's response to ATTRIB";
						case -77:
							return "Format error in card's ATS";
						case -78:
							return "Protocol error in card's response";
						case -87:
							return "Format error in card's PPS response";
						case -88:
							return "Other error in card's PPS response";
						case -93:
							return "A card is already active with this CID";
						case -100:
							return "Command not supported by the coupler";
						case -111:
							return "Internal error in the coupler";
						case -112:
							return "Internal buffer overflow";
						case -125:
							return "Wrong data length for the command";
						case -128:
							return "More time needed to process the command";
						default:
							return "Undiagnosticed error " + rc;
					}

				case 0x90:
					switch (SW2)
					{
						case 0x00:
							return "Success";
						case 0x01:
							return "Failed to write EEPROM";
						case 0x10:
							return "Wrong PIN (1st try)";
						case 0x20:
							return "Wrong PIN(2nd try)";
						case 0x40:
							return "Wrong PIN (3rd try)";
						case 0x80:
							return "Wrong PIN, card blocked";
						default:
							return "Unknown error";
					}

				case 0x92:
					switch (SW2)
					{
						case 0x00:
							return "Reference executed ok";
						case 0x02:
							return "Failed to write EEPROM";
						default:
							return "Unknow error";
					}

				case 0x94:
					return "No EF selected";

				case 0x98:
					switch (SW2)
					{
						case 0x02:
							return "Invalid PIN";
						case 0x04:
							return "Wrong PIN presentation";
						case 0x06:
							return "PIN cancelled";
						case 0x08:
							return "PIN inactivated";
						case 0x10:
							return "Security condition unsatisfied";
						case 0x20:
							return "PIN inactive";
						default:
							return "Unknown error";
					}

				default:
					return "Unknown error";
			}
		}
	}
}