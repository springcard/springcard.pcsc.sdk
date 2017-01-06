/**h* SpringCard/PCSC
 *
 * NAME
 *   PCSC
 * 
 * DESCRIPTION
 *   SpringCard's wrapper for PC/SC API
 *
 * COPYRIGHT
 *   Copyright (c) 2010-2012 SpringCard - www.springcard.com
 *
 * AUTHOR
 *   Johann.D and Emilie.C / SpringCard
 *
 * HISTORY
 *   ECL ../../2009 : early drafts
 *   JDA 21/04/2010 : first official release
 *   JDA 20/11/2010 : improved the SCardChannel object: implemented SCardControl, exported the hCard
 *   JDA 24/01/2011 : added static DefaultReader and DefaultCardChannel to ease 'quick and dirty' development for simple applications
 *   JDA 25/01/2011 : added SCardChannel.Reconnect methods
 *   JDA 16/01/2012 : improved CardBuffer, CAPDU and RAPDU objects for robustness
 *   JDA 12/02/2012 : added the SCardReaderList object to monitor all the readers
 *   JDA 26/03/2012 : added SCARD_PCI_T0, SCARD_PCI_T1 and SCARD_PCI_RAW
 *   JDA 07/02/2012 : minor improvements
 *
 * PORTABILITY
 *   .NET on Win32 (not yet validated on Win64 --> please compile for x86 target)
 *
 **/
using System;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace SpringCard.PCSC
{

	/**c* SpringCardPCSC/SCARD
	 *
	 * NAME
	 *   SCARD
	 * 
	 * DESCRIPTION
	 *   Static class that gives access to PC/SC functions (SCard... provided by winscard.dll)
	 *
	 **/
	public abstract partial class SCARD
	{
		public static SCardReader DefaultReader = null;
		public static SCardChannel DefaultCardChannel = null;

		#region Constants for parameters and status

		public const uint SCOPE_USER = 0;
		public const uint SCOPE_TERMINAL = 1;
		public const uint SCOPE_SYSTEM = 2;
		
		public const string ALL_READERS = "SCard$AllReaders\0\0";
		public const string DEFAULT_READERS = "SCard$DefaultReaders\0\0";
		public const string LOCAL_READERS = "SCard$LocalReaders\0\0";
		public const string SYSTEM_READERS = "SCard$SystemReaders\0\0";

		public const uint SHARE_EXCLUSIVE = 1;
		public const uint SHARE_SHARED = 2;
		public const uint SHARE_DIRECT = 3;

		public const uint PROTOCOL_NONE = 0;
		public const uint PROTOCOL_T0 = 1;
		public const uint PROTOCOL_T1 = 2;
		public const uint PROTOCOL_RAW = 4;

		public const uint LEAVE_CARD = 0; // Don't do anything special on close
		public const uint RESET_CARD = 1; // Reset the card on close
		public const uint UNPOWER_CARD = 2; // Power down the card on close
		public const uint EJECT_CARD = 3; // Eject the card on close

		public const uint STATE_UNAWARE = 0x00000000;
		public const uint STATE_IGNORE = 0x00000001;
		public const uint STATE_CHANGED = 0x00000002;
		public const uint STATE_UNKNOWN = 0x00000004;
		public const uint STATE_UNAVAILABLE = 0x00000008;
		public const uint STATE_EMPTY = 0x00000010;
		public const uint STATE_PRESENT = 0x00000020;
		public const uint STATE_ATRMATCH = 0x00000040;
		public const uint STATE_EXCLUSIVE = 0x00000080;
		public const uint STATE_INUSE = 0x00000100;
		public const uint STATE_MUTE = 0x00000200;
		public const uint STATE_UNPOWERED = 0x00000400;

		public const uint IOCTL_CSB6_PCSC_ESCAPE = 0x00312000;
		public const uint IOCTL_MS_CCID_ESCAPE = 0x003136B0;

		#endregion

		#region Error codes
		public const uint S_SUCCESS = 0x00000000;
		public const uint F_INTERNAL_ERROR = 0x80100001;
		public const uint E_CANCELLED = 0x80100002;
		public const uint E_INVALID_HANDLE = 0x80100003;
		public const uint E_INVALID_PARAMETER = 0x80100004;
		public const uint E_INVALID_TARGET = 0x80100005;
		public const uint E_NO_MEMORY = 0x80100006;
		public const uint F_WAITED_TOO_LONG = 0x80100007;
		public const uint E_INSUFFICIENT_BUFFER = 0x80100008;
		public const uint E_UNKNOWN_READER = 0x80100009;
		public const uint E_TIMEOUT = 0x8010000A;
		public const uint E_SHARING_VIOLATION = 0x8010000B;
		public const uint E_NO_SMARTCARD = 0x8010000C;
		public const uint E_UNKNOWN_CARD = 0x8010000D;
		public const uint E_CANT_DISPOSE = 0x8010000E;
		public const uint E_PROTO_MISMATCH = 0x8010000F;
		public const uint E_NOT_READY = 0x80100010;
		public const uint E_INVALID_VALUE = 0x80100011;
		public const uint E_SYSTEM_CANCELLED = 0x80100012;
		public const uint F_COMM_ERROR = 0x80100013;
		public const uint F_UNKNOWN_ERROR = 0x80100014;
		public const uint E_INVALID_ATR = 0x80100015;
		public const uint E_NOT_TRANSACTED = 0x80100016;
		public const uint E_READER_UNAVAILABLE = 0x80100017;
		public const uint P_SHUTDOWN = 0x80100018;
		public const uint E_PCI_TOO_SMALL = 0x80100019;
		public const uint E_READER_UNSUPPORTED = 0x8010001A;
		public const uint E_DUPLICATE_READER = 0x8010001B;
		public const uint E_CARD_UNSUPPORTED = 0x8010001C;
		public const uint E_NO_SERVICE = 0x8010001D;
		public const uint E_SERVICE_STOPPED = 0x8010001E;
		public const uint E_UNEXPECTED = 0x8010001F;
		public const uint E_ICC_INSTALLATION = 0x80100020;
		public const uint E_ICC_CREATEORDER = 0x80100021;
		public const uint E_UNSUPPORTED_FEATURE = 0x80100022;
		public const uint E_DIR_NOT_FOUND = 0x80100023;
		public const uint E_FILE_NOT_FOUND = 0x80100024;
		public const uint E_NO_DIR = 0x80100025;
		public const uint E_NO_FILE = 0x80100026;
		public const uint E_NO_ACCESS = 0x80100027;
		public const uint E_WRITE_TOO_MANY = 0x80100028;
		public const uint E_BAD_SEEK = 0x80100029;
		public const uint E_INVALID_CHV = 0x8010002A;
		public const uint E_UNKNOWN_RES_MNG = 0x8010002B;
		public const uint E_NO_SUCH_CERTIFICATE = 0x8010002C;
		public const uint E_CERTIFICATE_UNAVAILABLE = 0x8010002D;
		public const uint E_NO_READERS_AVAILABLE = 0x8010002E;
		public const uint E_COMM_DATA_LOST = 0x8010002F;
		public const uint E_NO_KEY_CONTAINER = 0x80100030;
		public const uint W_UNSUPPORTED_CARD = 0x80100065;
		public const uint W_UNRESPONSIVE_CARD = 0x80100066;
		public const uint W_UNPOWERED_CARD = 0x80100067;
		public const uint W_RESET_CARD = 0x80100068;
		public const uint W_REMOVED_CARD = 0x80100069;
		public const uint W_SECURITY_VIOLATION = 0x8010006A;
		public const uint W_WRONG_CHV = 0x8010006B;
		public const uint W_CHV_BLOCKED = 0x8010006C;
		public const uint W_EOF = 0x8010006D;
		public const uint W_CANCELLED_BY_USER = 0x8010006E;
		public const uint W_CARD_NOT_AUTHENTICATED = 0x8010006F;
		#endregion

		#region Definition of the 'native' structures
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] public struct READERSTATE
		{
			internal string szReader;
			internal IntPtr pvUserData;
			internal uint dwCurrentState;
			internal uint dwEventState;
			internal uint cbAtr;
			[MarshalAs (UnmanagedType.ByValArray, SizeConst = 0x24, ArraySubType = UnmanagedType.U1)]
			internal byte[] rgbAtr;
		}
		#endregion

		#region The ugly SCARD_PCI_xx global variables

		[DllImport("kernel32.dll")]
		private extern static IntPtr LoadLibrary(string fileName) ;
		
		[DllImport("kernel32.dll")]
		private extern static void FreeLibrary(IntPtr handle) ;
		
		[DllImport("kernel32.dll")]
		private extern static IntPtr GetProcAddress(IntPtr handle, string procName);
		
		/* Get the address of SCARD_PCI_T0 from "Winscard.dll" */
		private static IntPtr _scard_pci_t0 = IntPtr.Zero;
		public static IntPtr PCI_T0()
		{
			if (_scard_pci_t0 == IntPtr.Zero)
			{
				IntPtr handle = LoadLibrary("Winscard.dll") ;
				_scard_pci_t0 = GetProcAddress(handle, "g_rgSCardT0Pci");
				FreeLibrary(handle) ;
			}
			return _scard_pci_t0;
		}

		/* Get the address of SCARD_PCI_T1 from "Winscard.dll" */
		private static IntPtr _scard_pci_t1 = IntPtr.Zero;
		public static IntPtr PCI_T1()
		{
			if (_scard_pci_t1 == IntPtr.Zero)
			{
				IntPtr handle = LoadLibrary("Winscard.dll") ;
				_scard_pci_t1 = GetProcAddress(handle, "g_rgSCardT1Pci");
				FreeLibrary(handle) ;
			}
			return _scard_pci_t1;
		}

		/* Get the address of SCARD_PCI_RAW from "Winscard.dll" */
		private static IntPtr _scard_pci_raw = IntPtr.Zero;
		public static IntPtr PCI_RAW()
		{
			if (_scard_pci_raw == IntPtr.Zero)
			{
				IntPtr handle = LoadLibrary("Winscard.dll") ;
				_scard_pci_raw = GetProcAddress(handle, "g_rgSCardRawPci");
				FreeLibrary(handle) ;
			}
			return _scard_pci_raw;
		}
		#endregion
		
		#region Static methods, provided by the 'native' WINSCARD library

		/**f* SCARD/EstablishContext
		 *
		 * NAME
		 *   SCARD.EstablishContext
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardEstablishContext
		 *
		 **/
		[DllImport("WinScard.dll", EntryPoint = "SCardEstablishContext")]
		public static extern uint EstablishContext(uint dwScope,
		                                           IntPtr nNotUsed1,
		                                           IntPtr nNotUsed2,
		                                           ref IntPtr phContext);

		/**f* SCARD/ReleaseContext
		 *
		 * NAME
		 *   SCARD.ReleaseContext
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardReleaseContext
		 *
		 **/
		[DllImport("WinScard.dll", EntryPoint = "SCardReleaseContext")]
		public static extern uint ReleaseContext(IntPtr Context);

		/**f* SCARD/ListReaders
		 *
		 * NAME
		 *   SCARD.ListReaders
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardListReaders (UNICODE implementation)
		 *
		 **/
		[DllImport
		 ("winscard.dll", EntryPoint = "SCardListReadersW", SetLastError =
		  true, CharSet =
		  CharSet.Unicode)] public static extern uint ListReaders(IntPtr context,
		                                                        string groups,
		                                                        string readers,
		                                                        ref uint size);

		/**f* SCARD/GetStatusChange
		 *
		 * NAME
		 *   SCARD.GetStatusChange
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardGetStatusChange (UNICODE implementation)
		 *
		 **/
		[DllImport
		 ("winscard.dll", EntryPoint = "SCardGetStatusChangeW", CharSet =
		  CharSet.
		  Unicode)] public static extern uint GetStatusChange(IntPtr hContext,
		                                                    uint dwTimeout,[In,
		                                                                    Out,
		                                                                    MarshalAs
		                                                                    (UnmanagedType.
		                                                                     LPArray,
		                                                                     SizeParamIndex
		                                                                     =
		                                                                     3)]
		                                                    SCARD.
		                                                    READERSTATE
		                                                    []rgReaderState,
		                                                    uint cReaders);

		/**f* SCARD/Connect
		 *
		 * NAME
		 *   SCARD.Connect
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardConnect (UNICODE implementation)
		 *
		 **/
		[DllImport
		 ("WinScard.dll", EntryPoint = "SCardConnectW", CharSet =
		  CharSet.Unicode)] public static extern uint Connect(IntPtr hContext,
		                                                    string cReaderName,
		                                                    uint dwShareMode,
		                                                    uint dwPrefProtocol,
		                                                    ref IntPtr phCard,
		                                                    ref uint
		                                                    ActiveProtocol);

		/**f* SCARD/Reconnect
		 *
		 * NAME
		 *   SCARD.Reconnect
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardReconnect
		 *
		 **/
		[DllImport("winscard.dll", EntryPoint = "SCardReconnect")]
		public static extern uint Reconnect(IntPtr hCard,
		                                    uint dwShareMode,
		                                    uint dwPrefProtocol, uint swInit,
		                                    ref uint ActiveProtocol);

		/**f* SCARD/Disconnect
		 *
		 * NAME
		 *   SCARD.Disconnect
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardDisconnect
		 *
		 **/
		[DllImport("WinScard.dll", EntryPoint = "SCardDisconnect")]
		public static extern uint Disconnect(IntPtr hCard, uint Disposition);

		/**f* SCARD/Status
		 *
		 * NAME
		 *   SCARD.Status
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardStatus (UNICODE version)
		 *
		 **/
		[DllImport
		 ("winscard.dll", EntryPoint = "SCardStatusW", SetLastError =
		  true, CharSet =
		  CharSet.Unicode)] public static extern uint Status(IntPtr hCard,
		                                                   IntPtr
		                                                   mszReaderNames,
		                                                   ref uint
		                                                   pcchReaderLen,
		                                                   ref uint readerState,
		                                                   ref uint protocol,
		                                                   [In,
		                                                    Out]
		                                                   byte[]atr_bytes,
		                                                   ref uint atr_length);

		/**f* SCARD/Transmit
		 *
		 * NAME
		 *   SCARD.Transmit
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardTransmit
		 *
		 **/
		[DllImport
		 ("winscard.dll", EntryPoint = "SCardTransmit", SetLastError =
		  true)] public static extern uint Transmit(IntPtr hCard,
		                                          IntPtr pioSendPci,
		                                          byte[]pbSendBuffer,
		                                          uint cbSendLength,
		                                          IntPtr pioRecvPci,[In,
		                                                             Out]
		                                          byte[]pbRecvBuffer,[In,
		                                                              Out] ref
		                                          uint pcbRecvLength);

		/**f* SCARD/Control
		 *
		 * NAME
		 *   SCARD.Control
		 *
		 * DESCRIPTION
		 *   .NET wrapper for SCardControl
		 *
		 **/
		[DllImport
		 ("winscard.dll", EntryPoint = "SCardControl", SetLastError =
		  true)] public static extern uint Control(IntPtr hCard, uint ctlCode,
		                                         [In] byte[]pbSendBuffer,
		                                         uint cbSendLength,[In,
		                                                            Out]
		                                         byte[]pbRecvBuffer,
		                                         uint RecvBuffsize,
		                                         ref uint pcbRecvLength);
		#endregion

		#region Static methods - easy access to the list of readers
		public static string[] GetReaderList(IntPtr hContext, string Groups)
		{
			int i, j = 0;
			string s = "";
			uint rc;
			uint readers_size = 0;
			int readers_count = 0;

			rc = SCARD.ListReaders(hContext, Groups, null, ref readers_size);
			if (rc != SCARD.S_SUCCESS)
				return null;

			string readers_str = new string(' ', (int) readers_size);
			
			rc = SCARD.ListReaders(hContext, Groups, readers_str, ref readers_size);
			if (rc != SCARD.S_SUCCESS)
				return null;

			for (i = 0; i < readers_size; i++)
			{
				if (readers_str[i] == '\0')
				{
					if (i > 0)
						readers_count++;
					if (readers_str[i + 1] == '\0')
						break;
				}
			}
			
			string[] readers = new string[readers_count];

			if (readers_count > 0)
			{
				for (i = 0; i < readers_size; i++)
				{
					if (readers_str[i] == '\0')
					{
						readers[j++] = s;
						if (readers_str[i + 1] == '\0')
							break;
						s = "";
					} else
					{
						s = s + readers_str[i];
					}
				}
			}

			return readers;
		}
		
		public static string[] GetReaderList(IntPtr hContext)
		{
			return GetReaderList(hContext, null);
		}

		public static string[] GetReaderList(uint Scope, string Groups)
		{
			IntPtr hContext = IntPtr.Zero;
			uint rc;

			rc = SCARD.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
			if (rc != SCARD.S_SUCCESS)
				return null;

			string[] readers = GetReaderList(hContext, Groups);

			SCARD.ReleaseContext(hContext);

			return readers;
		}
		
		public static string[] GetReaderList()
		{
			return GetReaderList(SCARD.SCOPE_SYSTEM, null);
		}


		/**f* SCARD/Readers
		 *
		 * NAME
		 *   SCARD.Readers
		 *
		 * DESCRIPTION
		 *   Provides the list of the connected PC/SC readers
		 *
		 * SYNOPSIS
		 *   string[] SCARD.Readers
		 *
		 **/
		public static string[] Readers
		{
			get
			{
				return GetReaderList();
			}
		}
		#endregion

		#region Static methods - helpers to format status and errors

		/**f* SCARD/ErrorToString
		 *
		 * NAME
		 *   SCARD.ErrorToString
		 *
		 * DESCRIPTION
		 *   Translate a PC/SC error code into a user-readable string
		 *
		 * SYNOPSIS
		 *   string SCARD.ErrorToString( uint code );
		 *
		 **/
		public static string ErrorToString(uint code)
		{
			string r = "";
			try
			{
				r = (new Win32Exception((int) code)).Message;
			} catch (Exception)
			{
				
			}
			
			if (!r.Equals(""))
				return r;
			
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
					return r;
			}
		}

		/**f* SCARD/ReaderStatusToString
		 *
		 * NAME
		 *   SCARD.ReaderStatusToString
		 *
		 * DESCRIPTION
		 *   Translate the Status of the reader into a user-readable string
		 *
		 * SYNOPSIS
		 *   string SCARD.ReaderStatusToString( uint state );
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

		/**f* SCARD/CardProtocolToString
		 *
		 * NAME
		 *   SCARD.CardProtocolToString
		 *
		 * DESCRIPTION
		 *   Translate the Protocol of the card into a user-readable string
		 *
		 * SYNOPSIS
		 *   string SCARD.CardProtocolToString( uint protocol );
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
			if (protocol == SCARD.PROTOCOL_RAW)
				return "RAW";

			return "Unknown";
		}

		/**f* SCARD/CardShareModeToString
		 *
		 * NAME
		 *   SCARD.CardShareModeToString
		 *
		 * DESCRIPTION
		 *   Translate the Share Mode of the card into a user-readable string
		 *
		 * SYNOPSIS
		 *   string SCARD.CardShareModeToString( uint share_mode );
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

		/**f* SCARD/CardStatusWordsToString
		 *
		 * NAME
		 *   SCARD.CardStatusWordsToString
		 *
		 * DESCRIPTION
		 *   Translate the Status Word of the card into a user-readable string
		 *
		 * SYNOPSIS
		 *   string SCARD.CardStatusWordsToString( byte SW1, byte SW2 );
		 *   string SCARD.CardStatusWordsToString( ushort SW );
		 *
		 **/
		public static string CardStatusWordsToString(ushort SW)
		{
			byte SW1 = (byte) (SW / 0x0100);
			byte SW2 = (byte) (SW % 0x0100);

			return CardStatusWordsToString(SW1, SW2);
		}

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
					switch(rc)
					{
							/* Error codes taken from SpringProx API */
						case 0 :
							return "Undiagnosticed error";
						case -1 :
							return "No answer (no card / card is mute)";
						case -2 :
							return "Invalid CRC in card's response";
						case -3 :
							return "No frame received (NFC mode)";
						case -4 :
							return "Card: Authentication failed or access denied";
						case -5 :
							return "Invalid parity bit(s) in card's response";
						case -6 :
							return "NACK or status indicating error";
						case -7 :
							return "Too many anticollision loops";
						case -8 :
							return "Wrong LRC in card's serial number";
						case -9 :
							return "Card or block locked";
						case -10 :
							return "Card: Authentication must be performed first";
						case -11 :
							return "Wrong number of bits in card's answer";
						case -12 :
							return "Wrong number of bytes in card's answer";
						case -13 :
							return "Card: Counter is invalid";
						case -14 :
							return "Card: Transaction error";
						case -15 :
							return "Card: Write failed";
						case -16 :
							return "Card: Counter increase failed";
						case -17 :
							return "Card: Counter decrease failed";
						case -18 :
							return "Card: Read failed";
						case -19 :
							return "RC: FIFO overflow";
						case -20 :
							return "Polling mode pending";
						case -21 :
							return "Invalid framing in card's response";
						case -22 :
							return "Card: Access error (bad address or denied)";
						case -23 :
							return "RC: Unknown command";
						case -24 :
							return "A collision has occurred";
						case -25 :
							return "Command execution failed";
						case -26 :
							return "Hardware error";
						case -27 :
							return "RC: timeout";
						case -28 :
							return "More than one card found, but at least one does not support anticollision";
						case -29 :
							return "An external RF field has been detected";
						case -30 :
							return "Polling terminated (timeout or break)";
						case -31 :
							return "Bogus status in card's response";
						case -32 :
							return "Card: Vendor specific error";
						case -33 :
							return "Card: Command not supported";
						case -34 :
							return "Card: Format of command invalid";
						case -35 :
							return "Card: Option(s) of command invalid";
						case -36 :
							return "Card: other error";
						case -59 :
							return "Command not available in this mode";
						case -60 :
							return "Wrong parameter for the command";
						case -71 :
							return "No active card with this CID";
						case -75 :
							return "Length error in card's ATS";
						case -76 :
							return "Error in card's response to ATTRIB";
						case -77 :
							return "Format error in card's ATS";
						case -78 :
							return "Protocol error in card's response";
						case -87 :
							return "Format error in card's PPS response";
						case -88 :
							return "Other error in card's PPS response";
						case -93 :
							return "A card is already active with this CID";
						case -100 :
							return "Command not supported by the coupler";
						case -111 :
							return "Internal error in the coupler";
						case -112 :
							return "Internal buffer overflow";
						case -125 :
							return "Wrong data length for the command";
						case -128 :
							return "More time needed to process the command";
							default :
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
	#endregion
}

