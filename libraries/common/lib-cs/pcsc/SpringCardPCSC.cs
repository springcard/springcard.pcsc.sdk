/**
 *
 * \defgroup PCSC
 *
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D, Emilie.C and Jerome.I / SpringCard
 *
 * \page License
 *
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
/*
 *
 * CHANGELOG
 *   ECL ../../2009 : early drafts
 *   JDA 21/04/2010 : first official release
 *   JDA 20/11/2010 : improved the SCardChannel object: implemented SCardControl, exported the hCard
 *   JDA 24/01/2011 : added static DefaultReader and DefaultCardChannel to ease 'quick and dirty' development for simple applications
 *   JDA 25/01/2011 : added SCardChannel.Reconnect methods
 *   JDA 16/01/2012 : improved CardBuffer, CAPDU and RAPDU objects for robustness
 *   JDA 12/02/2012 : added the SCardReaderList object to monitor all the readers
 *   JDA 26/03/2012 : added SCARD_PCI_T0, SCARD_PCI_T1 and SCARD_PCI_RAW
 *   JDA 07/02/2012 : minor improvements
 *   JDA 02/03/2016 : added Linux/Unix portability, support for execution on Mono
 *
 **/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	/**
	 * \brief Static class that gives a direct access to PC/SC functions (SCard... provided by winscard.dll or libpcsclite)
	 *
	 **/
	public static partial class SCARD
	{
		private const string NotImplementedOnPcscLite = "This function is not implemented on PC/SC Lite systems";

		/**
		 *
		 * \brief Tell whether the PC/SC library provides trace/debug messages using SpringCard.LibCs.Logger class. Set to false to disable.
		 *
		 * \warning Sensitive content may be leaked if the Debug level is enabled in SpringCard.LibCs.Logger.
		 * 
		 **/
		public static bool UseLogger = false;

		/**
		 *
		 * \brief Native methods.
		 *
		 **/		
		public static class NativeMethods
		{
			/**
			 *
			 * \brief Win32 native methods, from winscard.dll
			 *
			 **/
			public static class Win32
			{
				public const string DllName = "winscard.dll";

				[DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
				internal extern static IntPtr LoadLibrary(string fileName);
				[DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
				internal extern static void FreeLibrary(IntPtr handle);
				[DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
				internal extern static IntPtr GetProcAddress(IntPtr handle, string procName);
				
				[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
				public struct READERSTATE
				{
					internal string szReader;
					internal IntPtr pvUserData;
					internal uint dwCurrentState;
					internal uint dwEventState;
					internal uint cbAtr;
					[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24, ArraySubType = UnmanagedType.U1)]
					internal byte[] rgbAtr;
				}

				[DllImport(DllName, EntryPoint = "SCardEstablishContext")]
				public static extern uint EstablishContext(uint dwScope,
														IntPtr nNotUsed1,
														IntPtr nNotUsed2,
														ref IntPtr phContext);

				[DllImport(DllName, EntryPoint = "SCardReleaseContext")]
				public static extern uint ReleaseContext(IntPtr Context);

				[DllImport(DllName, EntryPoint = "SCardListReadersW", SetLastError = true, CharSet = CharSet.Unicode)]
				public static extern uint ListReaders(IntPtr context,
													  string groups,
													  string readers,
													  ref uint size);

				[DllImport(DllName, EntryPoint = "SCardListReadersWithDeviceInstanceIdW", SetLastError = true, CharSet = CharSet.Unicode)]
				public static extern uint ListReadersWithDeviceInstanceId(IntPtr context,
                                                                          string device_instance_id,
                                                                          string readers,
                                                                          ref uint size);

                [DllImport(DllName, EntryPoint = "SCardGetReaderDeviceInstanceIdW", SetLastError = true, CharSet = CharSet.Unicode)]
                public static extern uint GetReaderDeviceInstanceId(IntPtr context,
                                                                    string reader_name,
                                                                    string device_instance_id,
                                                                    ref uint size);

                [DllImport(DllName, EntryPoint = "SCardGetStatusChangeW", CharSet = CharSet.Unicode)]
				public static extern uint GetStatusChange(IntPtr hContext,
				                                          uint dwTimeout,
                                                          [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] READERSTATE[] rgReaderState,
                                                          uint cReaders);

				[DllImport(DllName, EntryPoint = "SCardCancel")]
				public static extern uint Cancel(IntPtr hContext);

				[DllImport(DllName, EntryPoint = "SCardConnectW", CharSet = CharSet.Unicode)]
				public static extern uint Connect(IntPtr hContext,
												  string cReaderName,
												  uint dwShareMode,
												  uint dwPrefProtocol,
												  ref IntPtr phCard,
												  ref uint ActiveProtocol);

				[DllImport(DllName, EntryPoint = "SCardReconnect")]
				public static extern uint Reconnect(IntPtr hCard,
													uint dwShareMode,
													uint dwPrefProtocol,
													uint swInit,
													ref uint ActiveProtocol);

				[DllImport(DllName, EntryPoint = "SCardDisconnect")]
				public static extern uint Disconnect(IntPtr hCard, uint Disposition);

				[DllImport(DllName, EntryPoint = "SCardStatusW", SetLastError = true, CharSet = CharSet.Unicode)]
				public static extern uint Status(IntPtr hCard,
												IntPtr mszReaderNames,
												ref uint pcchReaderLen,
												ref uint readerState,
												ref uint protocol,
												[In, Out] byte[] atr_bytes,
												ref uint atr_length);

				[DllImport(DllName, EntryPoint = "SCardTransmit", SetLastError = true)]
				public static extern uint Transmit(IntPtr hCard,
												 IntPtr pioSendPci,
												 byte[] pbSendBuffer,
												 uint cbSendLength,
												 IntPtr pioRecvPci,
												 [In, Out] byte[] pbRecvBuffer,
												 [In, Out] ref uint pcbRecvLength);

				[DllImport(DllName, EntryPoint = "SCardGetAttrib", SetLastError = true)]
				public static extern uint GetAttrib(IntPtr hCard, uint dwAttrId,
									  [In, Out] byte[] pbAttr,
									  [In, Out] ref uint pcbAttrLength);

				[DllImport(DllName, EntryPoint = "SCardControl", SetLastError = true)]
				public static extern uint Control(IntPtr hCard, uint ctlCode,
												[In] byte[] pbSendBuffer,
												uint cbSendLength,
												[In, Out] byte[] pbRecvBuffer,
												uint RecvBuffsize,
												[In, Out] ref uint pcbRecvLength);

				[DllImport(DllName, EntryPoint = "SCardBeginTransaction")]
				public static extern uint BeginTransaction(IntPtr hCard);

				[DllImport(DllName, EntryPoint = "SCardEndTransaction")]
				public static extern uint EndTransaction(IntPtr hCard, uint Disposition);

				[DllImport(DllName, EntryPoint = "SCardListCards")]
				public static extern uint ListCards(IntPtr phContext, byte[] pbAtr, byte[] rgguiInterfaces, uint cguidInterfaceCount, string mszCards, ref int pcchCards);

				[DllImport(DllName, EntryPoint = "SCardIntroduceCardType")]
				public static extern uint IntroduceCardType(IntPtr phContext, string szCardName, byte[] pguidPrimaryProvider, byte[] rgguidInterfaces, uint dwInterfaceCount, byte[] atr, byte[] pbAtrMask, uint cbAtrLen);

				[DllImport(DllName, EntryPoint = "SCardSetCardTypeProviderName")]
				public static extern uint SetCardTypeProviderName(IntPtr phContext, string szCardName, uint dwProviderId, string szProvider);
			}

			/**
			 *
			 * \brief Linux native methods, from libpcsclite.so.1
			 *
			 **/			
			public static class Linux
			{
				public const string DllName = "libpcsclite.so.1";

				[DllImport("libdl.so", EntryPoint = "dlopen")]
				internal static extern IntPtr dlopen(string fileName, int flags);
				[DllImport("libdl.so", EntryPoint = "dlsym")]
				internal static extern IntPtr dlsym(IntPtr handle, string symbol);
				[DllImport("libdl.so", EntryPoint = "dlclose")]
				internal static extern int dlclose(IntPtr handle);
				[DllImport("libdl.so", EntryPoint = "dlerror")]
				internal static extern IntPtr dlerror();

                [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
                public struct READERSTATE
				{
					[MarshalAs(UnmanagedType.LPStr)] internal string szReader;
					internal IntPtr pvUserData;
					internal IntPtr dwCurrentState;
					internal IntPtr dwEventState;
					internal IntPtr cbAtr;
					[MarshalAs(UnmanagedType.ByValArray, SizeConst = 33, ArraySubType = UnmanagedType.U1)]
					internal byte[] rgbAtr;
				}

				[DllImport(DllName, EntryPoint = "SCardEstablishContext")]
				public static extern uint EstablishContext(uint dwScope,
														  IntPtr nNotUsed1,
														  IntPtr nNotUsed2,
														  ref IntPtr phContext);

				[DllImport(DllName, EntryPoint = "SCardReleaseContext")]
				public static extern uint ReleaseContext(IntPtr Context);

				[DllImport(DllName, EntryPoint = "SCardListReaders", SetLastError = true, CharSet = CharSet.Unicode)]
				public static extern uint ListReaders(IntPtr context,
													  [MarshalAs(UnmanagedType.LPStr)] string groups,
													  byte[] readers,
													  ref uint size);

				[DllImport(DllName, EntryPoint = "SCardGetStatusChange", CharSet = CharSet.Unicode)]
				public static extern uint GetStatusChange(IntPtr hContext, uint dwTimeout,
														 [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] READERSTATE[] rgReaderState,
														 uint cReaders);

				[DllImport(DllName, EntryPoint = "SCardCancel")]
				public static extern uint Cancel(IntPtr hContext);

				[DllImport(DllName, EntryPoint = "SCardConnect", CharSet = CharSet.Unicode)]
				public static extern uint Connect(IntPtr hContext,
												   [MarshalAs(UnmanagedType.LPStr)] string cReaderName,
												   uint dwShareMode,
												   uint dwPrefProtocol,
												   ref IntPtr phCard,
												   ref uint ActiveProtocol);

				[DllImport(DllName, EntryPoint = "SCardReconnect")]
				public static extern uint Reconnect(IntPtr hCard,
													uint dwShareMode,
													uint dwPrefProtocol, uint swInit,
													ref uint ActiveProtocol);

				[DllImport(DllName, EntryPoint = "SCardDisconnect")]
				public static extern uint Disconnect(IntPtr hCard, uint Disposition);

				[DllImport(DllName, EntryPoint = "SCardStatus", SetLastError = true, CharSet = CharSet.Unicode)]
				public static extern uint Status(IntPtr hCard,
												  IntPtr mszReaderNames,
												  ref uint pcchReaderLen,
												  ref uint readerState,
												  ref uint protocol,
												  [In, Out] byte[] atr_bytes,
												  ref uint atr_length);

				[DllImport(DllName, EntryPoint = "SCardTransmit", SetLastError = true)]
				public static extern uint Transmit(IntPtr hCard,
													IntPtr pioSendPci,
													byte[] pbSendBuffer,
													uint cbSendLength,
													IntPtr pioRecvPci,
													[In, Out] byte[] pbRecvBuffer,
													[In, Out] ref uint pcbRecvLength);

				[DllImport(DllName, EntryPoint = "SCardGetAttrib", SetLastError = true)]
				public static extern uint GetAttrib(IntPtr hCard, uint dwAttrId,
									  [In, Out] byte[] pbAttr,
									  [In, Out] ref uint pcbAttrLength);

				[DllImport(DllName, EntryPoint = "SCardControl", SetLastError = true)]
				public static extern uint Control(IntPtr hCard, uint ctlCode,
												   [In] byte[] pbSendBuffer,
												   uint cbSendLength,
												   [In, Out] byte[] pbRecvBuffer,
												   uint RecvBuffsize,
												   [In, Out] ref uint pcbRecvLength);

				[DllImport(DllName, EntryPoint = "SCardBeginTransaction")]
				public static extern uint BeginTransaction(IntPtr hCard);

				[DllImport(DllName, EntryPoint = "SCardEndTransaction")]
				public static extern uint EndTransaction(IntPtr hCard, uint Disposition);

			}

			/**
			 *
			 * \brief MacOS native methods, from PCSC.framework/PCSC
			 *
			 **/			
			public static class MacOs
			{
				public const string DllName = "PCSC.framework/PCSC";

				[DllImport("libdl.dylib", EntryPoint = "dlopen")]
				internal static extern IntPtr dlopen(string fileName, int flags);
				[DllImport("libdl.dylib", EntryPoint = "dlsym")]
				internal static extern IntPtr dlsym(IntPtr handle, string symbol);
				[DllImport("libdl.dylib", EntryPoint = "dlclose")]
				internal static extern int dlclose(IntPtr handle);
				[DllImport("libdl.dylib", EntryPoint = "dlerror")]
				internal static extern IntPtr dlerror();

				[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
				public struct READERSTATE
				{
					[MarshalAs(UnmanagedType.LPStr)] internal string szReader;
					internal IntPtr pvUserData;
					internal uint dwCurrentState;
					internal uint dwEventState;
					internal uint cbAtr;
					[MarshalAs(UnmanagedType.ByValArray, SizeConst = 33, ArraySubType = UnmanagedType.U1)] internal byte[] rgbAtr;
				}

				[DllImport(DllName, EntryPoint = "SCardEstablishContext")]
				public static extern uint EstablishContext(uint dwScope,
														   IntPtr nNotUsed1,
														   IntPtr nNotUsed2,
														   ref IntPtr phContext);

				[DllImport(DllName, EntryPoint = "SCardReleaseContext")]
				public static extern uint ReleaseContext(IntPtr Context);

				[DllImport(DllName, EntryPoint = "SCardListReaders", SetLastError = true, CharSet = CharSet.Unicode)]
				public static extern uint ListReaders(IntPtr context,
													   [MarshalAs(UnmanagedType.LPStr)] string groups,
													   byte[] readers,
													   ref uint size);

				[DllImport(DllName, EntryPoint = "SCardGetStatusChange", CharSet = CharSet.Unicode)]
				public static extern uint GetStatusChange(IntPtr hContext, uint dwTimeout,
														  [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] READERSTATE[] rgReaderState,
														  uint cReaders);

				[DllImport(DllName, EntryPoint = "SCardCancel")]
				public static extern uint Cancel(IntPtr hContext);

				[DllImport(DllName, EntryPoint = "SCardConnect", CharSet = CharSet.Unicode)]
				public static extern uint Connect(IntPtr hContext,
													[MarshalAs(UnmanagedType.LPStr)] string cReaderName,
													uint dwShareMode,
													uint dwPrefProtocol,
													ref IntPtr phCard,
													ref uint ActiveProtocol);

				[DllImport(DllName, EntryPoint = "SCardReconnect")]
				public static extern uint Reconnect(IntPtr hCard,
													 uint dwShareMode,
													 uint dwPrefProtocol, uint swInit,
													 ref uint ActiveProtocol);

				[DllImport(DllName, EntryPoint = "SCardDisconnect")]
				public static extern uint Disconnect(IntPtr hCard, uint Disposition);

				[DllImport(DllName, EntryPoint = "SCardStatus", SetLastError = true, CharSet = CharSet.Unicode)]
				public static extern uint Status(IntPtr hCard,
												   IntPtr mszReaderNames,
												   ref uint pcchReaderLen,
												   ref uint readerState,
												   ref uint protocol,
												   [In, Out] byte[] atr_bytes,
												   ref uint atr_length);

				[DllImport(DllName, EntryPoint = "SCardTransmit", SetLastError = true)]
				public static extern uint Transmit(IntPtr hCard,
													IntPtr pioSendPci,
													byte[] pbSendBuffer,
													uint cbSendLength,
													IntPtr pioRecvPci,
													[In, Out] byte[] pbRecvBuffer,
													[In, Out] ref uint pcbRecvLength);

				[DllImport(DllName, EntryPoint = "SCardGetAttrib", SetLastError = true)]
				public static extern uint GetAttrib(IntPtr hCard, uint dwAttrId,
									  [In, Out] byte[] pbAttr,
									  [In, Out] ref uint pcbAttrLength);

				[DllImport(DllName, EntryPoint = "SCardControl132", SetLastError = true)]
				public static extern uint Control(IntPtr hCard, uint ctlCode,
													[In] byte[] pbSendBuffer,
													uint cbSendLength,
													[In, Out] byte[] pbRecvBuffer,
													uint RecvBuffsize,
													[In, Out] ref uint pcbRecvLength);

				[DllImport(DllName, EntryPoint = "SCardBeginTransaction")]
				public static extern uint BeginTransaction(IntPtr hCard);


				[DllImport(DllName, EntryPoint = "SCardEndTransaction")]
				public static extern uint EndTransaction(IntPtr hCard, uint Disposition);

			}
		}

		#region Constants for parameters and status

		/**
		 * \brief dwScope parameter for EstablishContext(): scope is user space. Same as SCARD_SCOPE_USER in winscard
		 */
		public const uint SCOPE_USER = 0;
		/**
		 * \brief dwScope parameter for EstablishContext(): scope is the terminal. Same as SCARD_SCOPE_TERMINAL in winscard
		 */
		public const uint SCOPE_TERMINAL = 1;
		/**
		 * \brief dwScope parameter for EstablishContext(): scope is system. Same as SCARD_SCOPE_SYSTEM in winscard
		 */
		public const uint SCOPE_SYSTEM = 2;

		/**
		 * \brief groups parameter for ListReaders(): list all readers. Same as SCARD_ALL_READERS in winscard
		 */
		public const string ALL_READERS = "SCard$AllReaders\0\0";
		/**
		 * \brief groups parameter for ListReaders(): list the readers that are not in a specific group. Same as SCARD_DEFAULT_READERS in winscard
		 */
		public const string DEFAULT_READERS = "SCard$DefaultReaders\0\0";
		/**
		 * \brief groups parameter for ListReaders(): list local readers (deprecated and unused). Same as SCARD_LOCAL_READERS in winscard
		 */
		public const string LOCAL_READERS = "SCard$LocalReaders\0\0";
		/**
		 * \brief groups parameter for ListReaders(): list system readers (deprecated and unused). Same as SCARD_SYSTEM_READERS in winscard
		 */
		public const string SYSTEM_READERS = "SCard$SystemReaders\0\0";

		/**
		 * \brief share mode parameter for Connect(): take an exclusive access to the card. Same as SCARD_SHARE_EXCLUSIVE in winscard
		 */
		public const uint SHARE_EXCLUSIVE = 1;
		/**
		 * \brief share mode parameter for Connect(): accept to share the access to the card. Same as SCARD_SHARE_SHARED in winscard
		 */
		public const uint SHARE_SHARED = 2;
		/**
		 * \brief share mode parameter for Connect(): take a direct access to the reader (even if there is no card in the reader). Same as SCARD_SHARE_DIRECT in winscard
		 */
		public const uint SHARE_DIRECT = 3;

		/**
		 * \brief protocol parameter for Connect() and Status(): no active protocol (no card, direct access to the reader). Same as SCARD_PROTOCOL_UNSET in winscard
		 */
		public const uint PROTOCOL_NONE = 0;
		/**
		 * \brief protocol parameter for Connect() and Status(): protocol is T=0. Same as SCARD_PROTOCOL_T0 in winscard
		 */
		public const uint PROTOCOL_T0 = 1;
		/**
		 * \brief protocol parameter for Connect() and Status(): protocol is T=1. Same as SCARD_PROTOCOL_T1 in winscard
		 */
		public const uint PROTOCOL_T1 = 2;
		/**
		 * \brief protocol parameter for Connect() and Status(): protocol is RAW. Same as SCARD_PROTOCOL_RAW in winscard
		 */
		public const uint PROTOCOL_RAW = 4;

        /**
		 * \brief protocol parameter for Connect() and Status(): protocol is either NONE or RAW, depending on the operating system (NONE on Windows and Linux, RAW on Mac)
		 */
        public static uint PROTOCOL_DIRECT()
        {
            if (GetSystem() == System.MacOs)
            {
                return PROTOCOL_RAW;
            }
            else
            {
                return PROTOCOL_NONE;
            }
        }

        /**
		 * \brief disposition parameter for Disconnect() and Reconnect(): leave the card as is. Same as SCARD_LEAVE_CARD in winscard
		 */
        public const uint LEAVE_CARD = 0;
		/**
		 * \brief disposition parameter for Disconnect() and Reconnect(): warm reset the card. Same as SCARD_RESET_CARD in winscard
		 */
		public const uint RESET_CARD = 1;
		/**
		 * \brief disposition parameter for Disconnect() and Reconnect(): power down the card. Same as SCARD_UNPOWER_CARD in winscard
		 */
		public const uint UNPOWER_CARD = 2;
		/**
		 * \brief disposition parameter for Disconnect() and Reconnect(): power down the card, and eject it in case of a motorized reader. Same as SCARD_EJECT_CARD in winscard
		 */
		public const uint EJECT_CARD = 3;

		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): no flag set. Same as SCARD_STATE_UNAWARE in winscard
		 */
		public const uint STATE_UNAWARE = 0x00000000;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): no information required. Same as SCARD_STATE_IGNORE in winscard
		 */
		public const uint STATE_IGNORE = 0x00000001;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the reader's state has changed since the last call. Same as SCARD_STATE_CHANGED in winscard
		 */
		public const uint STATE_CHANGED = 0x00000002;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the reader does not exist. Same as SCARD_STATE_UNKNOWN in winscard
		 */
		public const uint STATE_UNKNOWN = 0x00000004;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the reader's state is not available. Same as SCARD_STATE_UNAVAILABLE in winscard
		 */
		public const uint STATE_UNAVAILABLE = 0x00000008;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): there is no card in the reader. Same as SCARD_STATE_EMPTY in winscard
		 */
		public const uint STATE_EMPTY = 0x00000010;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): there is a card in the reader. Same as SCARD_STATE_PRESENT in winscard
		 */
		public const uint STATE_PRESENT = 0x00000020;

		public const uint STATE_ATRMATCH = 0x00000040;

		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the card in the reader is reserved for exclusive use by an application. Same as SCARD_STATE_EXCLUSIVE in winscard
		 */
		public const uint STATE_EXCLUSIVE = 0x00000080;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the card in the reader is connected by an application. Same as SCARD_STATE_INUSE in winscard
		 */
		public const uint STATE_INUSE = 0x00000100;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the card in the reader is unresponsive. Same as SCARD_STATE_MUTE in winscard
		 */
		public const uint STATE_MUTE = 0x00000200;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the card in the reader has been powered down. Same as SCARD_STATE_UNPOWERED in winscard
		 */
		public const uint STATE_UNPOWERED = 0x00000400;

		public const uint IOCTL_CSB6_PCSC_ESCAPE = 0x00312000;
		public const uint IOCTL_MS_CCID_ESCAPE = 0x003136B0;
		public const uint IOCTL_PCSCLITE_ESCAPE = 0x42000000 + 1;
		#endregion

		#region GetAttrib/SetAttrib
		public const uint ATTR_ATR_STRING = 0x00090303;
		public const uint ATTR_CHANNEL_ID = 0x00020110;
		public const uint ATTR_CHARACTERISTICS = 0x00060150;
		public const uint ATTR_CURRENT_BWT = 0x00080209;
		public const uint ATTR_CURRENT_CLK = 0x00080202;
		public const uint ATTR_CURRENT_CWT = 0x0008020A;
		public const uint ATTR_CURRENT_D = 0x00080204;
		public const uint ATTR_CURRENT_EBC_ENCODING = 0x0008020B;
		public const uint ATTR_CURRENT_F = 0x00080203;
		public const uint ATTR_CURRENT_IFSC = 0x00080207;
		public const uint ATTR_CURRENT_IFSD = 0x00080208;
		public const uint ATTR_CURRENT_N = 0x00080205;
		public const uint ATTR_CURRENT_PROTOCOL_TYPE = 0x00080201;
		public const uint ATTR_CURRENT_W = 0x00080206;
		public const uint ATTR_DEFAULT_CLK = 0x00030121;
		public const uint ATTR_DEFAULT_DATA_RATE = 0x00030123;
		public const uint ATTR_DEVICE_FRIENDLY_NAME = 0x7FFF0003;
		public const uint ATTR_DEVICE_IN_USE = 0x7FFF0002;
		public const uint ATTR_DEVICE_SYSTEM_NAME = 0x7FFF0003;
		public const uint ATTR_DEVICE_UNIT = 0x7FFF0001;
		public const uint ATTR_ICC_INTERFACE_STATUS = 0x00090301;
		public const uint ATTR_ICC_PRESENCE = 0x00090300;
		public const uint ATTR_ICC_TYPE_PER_ATR = 0x00090304;
		public const uint ATTR_MAX_CLK = 0x00030122;
		public const uint ATTR_MAX_DATA_RATE = 0x00030124;
		public const uint ATTR_MAX_IFSD = 0x00030125;
		public const uint ATTR_POWER_MGMT_SUPPORT = 0x00040131;
		public const uint ATTR_PROTOCOL_TYPES = 0x00030126;
		public const uint ATTR_VENDOR_IFD_SERIAL_NO = 0x00010103;
		public const uint ATTR_VENDOR_IFD_TYPE = 0x00010101;
		public const uint ATTR_VENDOR_IFD_VERSION = 0x00010102;
		public const uint ATTR_VENDOR_NAME = 0x00010100;
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

		enum System
		{
			Unknown,
			Win32,
			Unix,
			MacOs
		}

		;
		private static System system = System.Unknown;

		private static bool IsRunningOnMacOs()
		{
			IntPtr buf = IntPtr.Zero;
			try
			{
				buf = Marshal.AllocHGlobal(8192);
				/* Hacktastic way of getting sysname from uname () */
				if (uname(buf) == 0)
				{
					string os = Marshal.PtrToStringAnsi(buf);
					if (os == "Darwin")
						return true;
				}
			}
			catch
			{
			}
			finally
			{
				if (buf != IntPtr.Zero)
					Marshal.FreeHGlobal(buf);
			}
			return false;
		}

		[DllImport("libc")]
		static extern int uname(IntPtr buf);

		private static System GetSystem()
		{
			if (system == System.Unknown)
			{
				OperatingSystem os = Environment.OSVersion;
				PlatformID pid = os.Platform;
				if (pid == PlatformID.Unix)
				{
					system = System.Unix;
					if (IsRunningOnMacOs())
						system = System.MacOs;

				}
				else
				{
					system = System.Win32;
				}

			}

			return system;
		}

		#region Load library stuff

		public static IntPtr LoadLibrary_Unix(string fileName)
		{
			const int RTLD_NOW = 2;
			if (GetSystem() != System.MacOs)
			{
				return NativeMethods.Linux.dlopen(fileName, RTLD_NOW);
			}
			else
			{
				return NativeMethods.MacOs.dlopen(fileName, RTLD_NOW);
			}
		}

		public static void FreeLibrary_Unix(IntPtr handle)
		{
			if (GetSystem() != System.MacOs)
			{
				NativeMethods.Linux.dlclose(handle);
			}
			else
			{
				NativeMethods.MacOs.dlclose(handle);
			}
		}

		public static IntPtr GetProcAddress_Unix(IntPtr dllHandle, string name)
		{
			IntPtr res, errPtr;

			if (GetSystem() != System.MacOs)
			{
				NativeMethods.Linux.dlerror();
			}
			else
			{
				NativeMethods.MacOs.dlerror();
			}

			if (GetSystem() != System.MacOs)
			{
				res = NativeMethods.Linux.dlsym(dllHandle, name);
				errPtr = NativeMethods.Linux.dlerror();
			}
			else
			{
				res = NativeMethods.MacOs.dlsym(dllHandle, name);
				errPtr = NativeMethods.MacOs.dlerror();
			}

			if (errPtr != IntPtr.Zero)
			{
				throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
			}

			return res;
		}

		private static IntPtr LoadLibrary(string fileName)
		{
			if (GetSystem() != System.Win32)
			{
				return LoadLibrary_Unix(fileName);
			}
			else
			{
				return NativeMethods.Win32.LoadLibrary(fileName);
			}
		}

		private static void FreeLibrary(IntPtr handle)
		{
			if (GetSystem() != System.Win32)
			{
				FreeLibrary_Unix(handle);
			}
			else
			{
				NativeMethods.Win32.FreeLibrary(handle);
			}
		}

		private static IntPtr GetProcAddress(IntPtr handle, string procName)
		{
			if (GetSystem() != System.Win32)
			{
				return GetProcAddress_Unix(handle, procName);
			}
			else
			{
				return NativeMethods.Win32.GetProcAddress(handle, procName);
			}
		}

		/* Handle of the library */
		private static IntPtr DllHandle = IntPtr.Zero;

		private static bool LoadLibrary()
		{
			if (DllHandle == IntPtr.Zero)
			{
				if (GetSystem() == System.Unix)
				{
					DllHandle = LoadLibrary(NativeMethods.Linux.DllName);
				}
				else if (GetSystem() == System.MacOs)
				{
					DllHandle = LoadLibrary(NativeMethods.MacOs.DllName);
				}
				else
				{
					DllHandle = LoadLibrary(NativeMethods.Win32.DllName);
				}
			}
			return (DllHandle != IntPtr.Zero);
		}

		private static IntPtr _scard_pci_t0 = IntPtr.Zero;
		
		/**
		 * \brief .NET wrapper for PCI_T0
		 *
		 * Use this constant with Transmit() if card protocol is T=0
		 */				
		public static IntPtr PCI_T0()
		{
			if (_scard_pci_t0 == IntPtr.Zero)
			{
				if (LoadLibrary())
					_scard_pci_t0 = GetProcAddress(DllHandle, "g_rgSCardT0Pci");
			}
			return _scard_pci_t0;
		}
	
		private static IntPtr _scard_pci_t1 = IntPtr.Zero;
		
		/**
		 * \brief .NET wrapper for PCI_T1
		 *
		 * Use this constant with Transmit() if card protocol is T=1
		 */				
		public static IntPtr PCI_T1()
		{
			if (_scard_pci_t1 == IntPtr.Zero)
			{
				if (LoadLibrary())
					_scard_pci_t1 = GetProcAddress(DllHandle, "g_rgSCardT1Pci");
			}
			return _scard_pci_t1;
		}

		private static IntPtr _scard_pci_raw = IntPtr.Zero;
		
		/**
		 * \brief .NET wrapper for PCI_RAW
		 *
		 * Use this constant with Transmit() if card protocol is RAW
		 */		
		public static IntPtr PCI_RAW()
		{
			if (_scard_pci_raw == IntPtr.Zero)
			{
				if (LoadLibrary())
					_scard_pci_raw = GetProcAddress(DllHandle, "g_rgSCardRawPci");
			}
			return _scard_pci_raw;
		}
		#endregion

		#region Static methods, provided by the 'native' WINSCARD library


		/**
		 * \brief .NET wrapper for SCardEstablishContext
		 *
		 * The application shall open a resource manager context for every thread, and must call ReleaseContext for all open contexts when exiting.
		 */
		public static uint EstablishContext(uint dwScope,
											IntPtr nNotUsed1,
											IntPtr nNotUsed2,
											ref IntPtr phContext)
		{

			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.EstablishContext(dwScope, nNotUsed1, nNotUsed2, ref phContext);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.EstablishContext(dwScope, nNotUsed1, nNotUsed2, ref phContext);
			}
			else
			{
				return NativeMethods.Win32.EstablishContext(dwScope, nNotUsed1, nNotUsed2, ref phContext);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardReleaseContext
		 *
		 */
		public static uint ReleaseContext(IntPtr Context)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.ReleaseContext(Context);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.ReleaseContext(Context);
			}
			else
			{
				return NativeMethods.Win32.ReleaseContext(Context);
			}
		}

		/**
		 *
		 * \brief Internal structure used by GetStatusChange() (manipulate with care)
		 *
		 **/
		public struct READERSTATE
		{
			public string szReader;
			public IntPtr pvUserData;
			public uint dwCurrentState;
			public uint dwEventState;
			public uint cbAtr;
			public byte[] rgbAtr;
		}

		/**
		 * \brief .NET wrapper for SCardGetStatusChange
		 * 
		 * \details
		 *  This function blocks the execution of the thread until the state of a reader changes, or a timeout occurs.
		 */
		public static uint GetStatusChange(IntPtr hContext, uint dwTimeout,
										   READERSTATE[] rgReaderState,
										   uint cReaders)
		{
			if (GetSystem() == System.Unix)
			{
				NativeMethods.Linux.READERSTATE[] rgReaderState_native = new NativeMethods.Linux.READERSTATE[rgReaderState.Length];

				for (int i = 0; i < rgReaderState.Length; i++)
				{
					rgReaderState_native[i].szReader = rgReaderState[i].szReader;
					rgReaderState_native[i].pvUserData = rgReaderState[i].pvUserData;
					rgReaderState_native[i].dwCurrentState = (IntPtr) rgReaderState[i].dwCurrentState;
					rgReaderState_native[i].dwEventState = (IntPtr) rgReaderState[i].dwEventState;
					rgReaderState_native[i].cbAtr = (IntPtr) 0;
					rgReaderState_native[i].rgbAtr = new byte[33];
                }

                uint rc = NativeMethods.Linux.GetStatusChange(hContext, dwTimeout, rgReaderState_native, cReaders);

				for (int i = 0; i < rgReaderState.Length; i++)
				{
                    rgReaderState[i].szReader = rgReaderState_native[i].szReader;
					rgReaderState[i].pvUserData = rgReaderState_native[i].pvUserData;
					rgReaderState[i].dwCurrentState = (uint) rgReaderState_native[i].dwCurrentState.ToInt64();
					rgReaderState[i].dwEventState = (uint) rgReaderState_native[i].dwEventState.ToInt64();
					rgReaderState[i].cbAtr = (uint) rgReaderState_native[i].cbAtr.ToInt64();
					rgReaderState[i].rgbAtr = rgReaderState_native[i].rgbAtr;
				}

				return rc;

			}
			else if (GetSystem() == System.MacOs)
			{
				NativeMethods.MacOs.READERSTATE[] rgReaderState_native = new NativeMethods.MacOs.READERSTATE[rgReaderState.Length];

				for (int i = 0; i < rgReaderState.Length; i++)
				{
					rgReaderState_native[i].szReader = rgReaderState[i].szReader;
					rgReaderState_native[i].pvUserData = rgReaderState[i].pvUserData;
					rgReaderState_native[i].dwCurrentState = rgReaderState[i].dwCurrentState;
					rgReaderState_native[i].dwEventState = rgReaderState[i].dwEventState;
					rgReaderState_native[i].cbAtr = rgReaderState[i].cbAtr;
					rgReaderState_native[i].rgbAtr = rgReaderState[i].rgbAtr;

				}

				uint rc = NativeMethods.MacOs.GetStatusChange(hContext, dwTimeout, rgReaderState_native, cReaders);

				for (int i = 0; i < rgReaderState.Length; i++)
				{
					rgReaderState[i].szReader = rgReaderState_native[i].szReader;
					rgReaderState[i].pvUserData = rgReaderState_native[i].pvUserData;
					rgReaderState[i].dwCurrentState = rgReaderState_native[i].dwCurrentState;
					rgReaderState[i].dwEventState = rgReaderState_native[i].dwEventState;
					rgReaderState[i].cbAtr = rgReaderState_native[i].cbAtr;
					rgReaderState[i].rgbAtr = rgReaderState_native[i].rgbAtr;
				}
				return rc;

			}
			else
			{
				NativeMethods.Win32.READERSTATE[] rgReaderState_native = new NativeMethods.Win32.READERSTATE[rgReaderState.Length];
				for (int i = 0; i < rgReaderState.Length; i++)
				{
					rgReaderState_native[i].szReader = rgReaderState[i].szReader;
					rgReaderState_native[i].pvUserData = rgReaderState[i].pvUserData;
					rgReaderState_native[i].dwCurrentState = rgReaderState[i].dwCurrentState;
					rgReaderState_native[i].dwEventState = rgReaderState[i].dwEventState;
					rgReaderState_native[i].cbAtr = rgReaderState[i].cbAtr;
					rgReaderState_native[i].rgbAtr = rgReaderState[i].rgbAtr;

				}

				uint rc = NativeMethods.Win32.GetStatusChange(hContext, dwTimeout, rgReaderState_native, cReaders);

				for (int i = 0; i < rgReaderState.Length; i++)
				{
					rgReaderState[i].szReader = rgReaderState_native[i].szReader;
					rgReaderState[i].pvUserData = rgReaderState_native[i].pvUserData;
					rgReaderState[i].dwCurrentState = rgReaderState_native[i].dwCurrentState;
					rgReaderState[i].dwEventState = rgReaderState_native[i].dwEventState;
					rgReaderState[i].cbAtr = rgReaderState_native[i].cbAtr;
					rgReaderState[i].rgbAtr = rgReaderState_native[i].rgbAtr;
				}
				return rc;
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardCancel
		 *
		 **/
		public static uint Cancel(IntPtr hContext)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.Cancel(hContext);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.Cancel(hContext);
			}
			else
			{
				return NativeMethods.Win32.Cancel(hContext);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardConnect
		 *
		 **/
		public static uint Connect(IntPtr hContext,
								   string cReaderName,
								   uint dwShareMode,
								   uint dwPrefProtocol,
								   ref IntPtr phCard,
								   ref uint ActiveProtocol)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.Connect(hContext, cReaderName, dwShareMode, dwPrefProtocol, ref phCard, ref ActiveProtocol);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.Connect(hContext, cReaderName, dwShareMode, dwPrefProtocol, ref phCard, ref ActiveProtocol);
			}
			else
			{
				return NativeMethods.Win32.Connect(hContext, cReaderName, dwShareMode, dwPrefProtocol, ref phCard, ref ActiveProtocol);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardReconnect
		 *
		 **/
		public static uint Reconnect(IntPtr hCard, uint dwShareMode,
									 uint dwPrefProtocol, uint swInit,
									 ref uint ActiveProtocol)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.Reconnect(hCard, dwShareMode, dwPrefProtocol, swInit, ref ActiveProtocol);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.Reconnect(hCard, dwShareMode, dwPrefProtocol, swInit, ref ActiveProtocol);
			}
			else
			{
				return NativeMethods.Win32.Reconnect(hCard, dwShareMode, dwPrefProtocol, swInit, ref ActiveProtocol);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardDisconnect
		 *
		 **/
		public static uint Disconnect(IntPtr hCard, uint Disposition)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.Disconnect(hCard, Disposition);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.Disconnect(hCard, Disposition);
			}
			else
			{
				return NativeMethods.Win32.Disconnect(hCard, Disposition);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardStatus
		 *
		 **/
		public static uint Status(IntPtr hCard, IntPtr mszReaderNames,
								  ref uint pcchReaderLen,
								  ref uint readerState,
								  ref uint protocol,
								  [In, Out] byte[] atr_bytes,
								  ref uint atr_length)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.Status(hCard, mszReaderNames, ref pcchReaderLen, ref readerState, ref protocol, atr_bytes, ref atr_length);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.Status(hCard, mszReaderNames, ref pcchReaderLen, ref readerState, ref protocol, atr_bytes, ref atr_length);
			}
			else
			{
				return NativeMethods.Win32.Status(hCard, mszReaderNames, ref pcchReaderLen, ref readerState, ref protocol, atr_bytes, ref atr_length);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardTransmit
		 *
		 **/
		public static uint Transmit(IntPtr hCard, IntPtr pioSendPci,
									byte[] pbSendBuffer,
									uint cbSendLength,
									IntPtr pioRecvPci,
									[In, Out] byte[] pbRecvBuffer,
									[In, Out] ref uint pcbRecvLength)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.Transmit(hCard, pioSendPci, pbSendBuffer, cbSendLength, pioRecvPci, pbRecvBuffer, ref pcbRecvLength);
			}
			if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.Transmit(hCard, pioSendPci, pbSendBuffer, cbSendLength, pioRecvPci, pbRecvBuffer, ref pcbRecvLength);
			}
			else
			{
				return NativeMethods.Win32.Transmit(hCard, pioSendPci, pbSendBuffer, cbSendLength, pioRecvPci, pbRecvBuffer, ref pcbRecvLength);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardGetAttrib
		 *
		 **/
		public static uint GetAttrib(IntPtr hCard, uint dwAttrId,
								   [In, Out] byte[] pbAttr,
								   [In, Out] ref uint pcbAttrLength)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.GetAttrib(hCard, dwAttrId, pbAttr, ref pcbAttrLength);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.GetAttrib(hCard, dwAttrId, pbAttr, ref pcbAttrLength);
			}
			else
			{
				return NativeMethods.Win32.GetAttrib(hCard, dwAttrId, pbAttr, ref pcbAttrLength);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardControl
		 *
		 **/
		public static uint Control(IntPtr hCard, uint ctlCode,
								   [In] byte[] pbSendBuffer,
								   uint cbSendLength,
								   [In, Out] byte[] pbRecvBuffer,
								   uint RecvBuffsize,
								   ref uint pcbRecvLength)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.Control(hCard, IOCTL_PCSCLITE_ESCAPE, pbSendBuffer, cbSendLength, pbRecvBuffer, RecvBuffsize, ref pcbRecvLength);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.Control(hCard, IOCTL_PCSCLITE_ESCAPE, pbSendBuffer, cbSendLength, pbRecvBuffer, RecvBuffsize, ref pcbRecvLength);
			}
			else
			{
				return NativeMethods.Win32.Control(hCard, ctlCode, pbSendBuffer, cbSendLength, pbRecvBuffer, RecvBuffsize, ref pcbRecvLength);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardBeginTransaction
		 *
		 **/
		public static uint BeginTransaction(IntPtr hCard)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.BeginTransaction(hCard);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.BeginTransaction(hCard);
			}
			else
			{
				return NativeMethods.Win32.BeginTransaction(hCard);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardEndTransaction
		 *
		 **/
		public static uint EndTransaction(IntPtr hCard, uint Disposition)
		{
			if (GetSystem() == System.Unix)
			{
				return NativeMethods.Linux.EndTransaction(hCard, Disposition);
			}
			else if (GetSystem() == System.MacOs)
			{
				return NativeMethods.MacOs.EndTransaction(hCard, Disposition);
			}
			else
			{
				return NativeMethods.Win32.EndTransaction(hCard, Disposition);
			}
		}
		#endregion


		#region Static methods - ATR database

		/**
		 *
		 * \brief .NET wrapper for SCardListCards (Windows only)
		 *
		 **/		
		public static uint ListCards(IntPtr phContext, byte[] pbAtr, byte[] rgguiInterfaces, uint cguidInterfaceCount, string mszCards, ref int pcchCards)
		{
			if (GetSystem() == System.Unix)
			{
				throw new Exception(NotImplementedOnPcscLite);
			}
			else if (GetSystem() == System.MacOs)
			{
				throw new Exception(NotImplementedOnPcscLite);
			}
			else
			{
				return NativeMethods.Win32.ListCards(phContext, pbAtr, rgguiInterfaces, cguidInterfaceCount, mszCards, ref pcchCards);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardIntroduceCardType (Windows only)
		 *
		 **/				
		public static uint IntroduceCardType(IntPtr phContext, string szCardName, byte[] pguidPrimaryProvider, byte[] rgguidInterfaces, uint dwInterfaceCount, byte[] atr, byte[] pbAtrMask, uint cbAtrLen)
		{
			if (GetSystem() == System.Unix)
			{
				throw new Exception(NotImplementedOnPcscLite);
			}
			else if (GetSystem() == System.MacOs)
			{
				throw new Exception(NotImplementedOnPcscLite);
			}
			else
			{
				return NativeMethods.Win32.IntroduceCardType(phContext, szCardName, pguidPrimaryProvider, rgguidInterfaces, dwInterfaceCount, atr, pbAtrMask, cbAtrLen);
			}
		}

		/**
		 *
		 * \brief .NET wrapper for SCardSetCardTypeProviderName (Windows only)
		 *
		 **/		
		public static uint SetCardTypeProviderName(IntPtr phContext, string szCardName, uint dwProviderId, string szProvider)
		{
			if (GetSystem() == System.Unix)
			{
				throw new Exception(NotImplementedOnPcscLite);
			}
			else if (GetSystem() == System.MacOs)
			{
				throw new Exception(NotImplementedOnPcscLite);
			}
			else
			{
				return NativeMethods.Win32.SetCardTypeProviderName(phContext, szCardName, dwProviderId, szProvider);
			}
		}

		#endregion

		#region Static methods - easy access to the list of readers

		private static uint ListReaders(IntPtr hContext, string Groups, ref string ReaderNames, ref uint Size)
		{
			uint rc;

			byte[] r = null;
			if (ReaderNames != null)
			{
				/* The terminal '00' shall not be added here */
				/* Two '00' characters will be added by the  */
				/* SCardListReaders function in library      */
				r = new byte[ReaderNames.Length];
				for (int i = 0; i < ReaderNames.Length; i++)
					r[i] = (byte)ReaderNames[i];
			}

			if (GetSystem() == System.Unix)
			{
				rc = NativeMethods.Linux.ListReaders(hContext, Groups, r, ref Size);
			}
			else
			{
				rc = NativeMethods.MacOs.ListReaders(hContext, Groups, r, ref Size);
			}

			if (r != null)
			{
				ReaderNames = "";
				for (int i = 0; i < r.Length; i++)
					ReaderNames += (char)r[i];
			}

			return rc;
		}

		private static uint ListReaders(IntPtr hContext, string Groups, string ReaderNames, ref uint Size)
		{
			return NativeMethods.Win32.ListReaders(hContext, Groups, ReaderNames, ref Size);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * \deprecated See ListReaders()
		 *
		 */
		public static string[] GetReaderList(IntPtr hContext, string Groups = null)
		{
			return ListReaders(hContext, Groups);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * \deprecated See ListReaders()
		 *
		 */
		public static string[] GetReaderList(uint Scope, string Groups)
		{
			return ListReaders(Scope, Groups);
		}
		
		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * \deprecated See ListReaders()
		 *
		 */
		public static string[] GetReaderList()
		{
			return ListReaders();
		}

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * A valid hContext must be supplied - see EstablishContext()
		 *
		 */
		public static string[] ListReaders(IntPtr hContext, string Groups = null)
		{
			int i;
			string s = "";
			uint rc;
			uint readers_size = 0;
			int readers_count = 0;

			if (GetSystem() != System.Win32)
			{
				string dummy = null;
				rc = SCARD.ListReaders(hContext, Groups, ref dummy, ref readers_size);
			}
			else
			{
				rc = SCARD.ListReaders(hContext, Groups, null, ref readers_size);
			}

			if (rc != SCARD.S_SUCCESS)
			{
				if (UseLogger)
					Logger.Trace("SCardListReaders failed with error " + SCARD.ErrorToMessage(rc));
				return new string[0];
			}

			string readers_str = new string('\0', (int)readers_size);

			if (GetSystem() != System.Win32)
			{
				rc = SCARD.ListReaders(hContext, Groups, ref readers_str, ref readers_size);
			}
			else
			{
				rc = SCARD.ListReaders(hContext, Groups, readers_str, ref readers_size);
			}

			if (rc != SCARD.S_SUCCESS)
			{
				if (UseLogger)
					Logger.Trace("SCardListReaders failed with error " + SCARD.ErrorToMessage(rc));
                return new string[0];
            }

			for (i = 0; i < readers_size; i++)
			{
				if (readers_str[i] == '\0')
				{
					if (i > 0)
						readers_count++;
					if ((i + 1) < readers_size)
						if (readers_str[i + 1] == '\0')
							break;
				}
			}

			string[] readers = new string[readers_count];

			if (readers_count > 0)
			{
				s = "";
				int j = 0;
				for (i = 0; i < readers_size; i++)
				{
					if (readers_str[i] == '\0')
					{
						readers[j++] = s;
						s = "";
						if (readers_str[i + 1] == '\0')
							break;
					}
					else
					{
						s = s + (char)readers_str[i];
					}
				}
			}

			return readers;
		}

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * A temporary context is created in the specified Scope
		 *
		 */		
		public static string[] ListReaders(uint Scope, string Groups)
		{
			IntPtr hContext = IntPtr.Zero;
			uint rc;

            rc = SCARD.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
			if (rc != SCARD.S_SUCCESS)
			{
				if (UseLogger)
					Logger.Warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
				return new string[0];
			}

			string[] readers = GetReaderList(hContext, Groups);

			SCARD.ReleaseContext(hContext);

			return readers;
		}

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * A temporary context is created in the system scope
		 *
		 */		
		public static string[] ListReaders(string Groups = null)
		{
			return ListReaders(SCARD.SCOPE_SYSTEM, Groups);
		}

		/**
		 *
		 * \brief Alias for ListReaders()
		 *
		 */
		public static string[] Readers
		{
			get
			{
				return GetReaderList();
			}
		}

        /**
		 *
		 * \brief Does a reader exist?
		 *
		 */
        public static bool ReaderExists(string ReaderName)
        {
            string[] readers = ListReaders();
            if (readers != null)
            {
                foreach (string reader in readers)
                {
                    if (reader == ReaderName)
                        return true;
                }
            }
            return false;
        }

        /**
		 *
		 * \brief Find a reader given a search pattern
		 *
		 */
        public static string ReaderLike(string[] ReaderNames, string SearchPattern)
        {
            if ((ReaderNames == null) || (ReaderNames.Length == 0))
                return null;
            if ((SearchPattern == null) || (SearchPattern.Trim() == ""))
                return ReaderNames[0];

            SearchPattern = "^" + Regex.Escape(SearchPattern).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            var regexp = new Regex(SearchPattern, RegexOptions.IgnoreCase);
            foreach (string reader in ReaderNames)
                if (regexp.IsMatch(reader))
                    return reader;

            return null;
        }

        /**
		 *
		 * \brief Find a reader given a search pattern
		 *
		 */
        public static string ReaderLike(string SearchPattern)
        {
            return ReaderLike(ListReaders(), SearchPattern);
        }

        #endregion

        /**
		 *
		 * \brief .NET wrapper for SCardListReadersWithDeviceInstanceId (Windows only)
		 *
		 **/
        public static uint ListReadersWithDeviceInstanceId(IntPtr hContext, string InstanceId, string ReaderNames, ref uint Size)
		{
			if (GetSystem() == System.Unix)
			{
				throw new Exception(NotImplementedOnPcscLite);
			}
			else if (GetSystem() == System.MacOs)
			{
				throw new Exception(NotImplementedOnPcscLite);
			}
			else
			{		
				return NativeMethods.Win32.ListReadersWithDeviceInstanceId(hContext, InstanceId, ReaderNames, ref Size);
			}
		}

        /**
		 *
		 * \brief .NET wrapper for SCardGetReaderDeviceInstanceId (Windows only)
		 *
		 **/
        public static uint GetReaderDeviceInstanceId(IntPtr hContext, string ReaderName, string InstanceId, ref uint Size)
        {
            if (GetSystem() == System.Unix)
            {
                throw new Exception(NotImplementedOnPcscLite);
            }
            else if (GetSystem() == System.MacOs)
            {
                throw new Exception(NotImplementedOnPcscLite);
            }
            else
            {
                return NativeMethods.Win32.GetReaderDeviceInstanceId(hContext, ReaderName, InstanceId, ref Size);
            }
        }


        /**
		 *
		 * \brief .NET wrapper for SCardListReadersWithDeviceInstanceId (Windows only)
		 *
		 **/
        public static string[] GetReaderListWithDeviceInstanceId(IntPtr hContext, string InstanceId)
		{
			int i;
			string s = "";
			uint rc;
			uint readers_size = 0;
			int readers_count = 0;

			if (GetSystem() != System.Win32)
			{
				rc = SCARD.E_UNEXPECTED;
			}
			else
			{
				rc = SCARD.ListReadersWithDeviceInstanceId(hContext, InstanceId, null, ref readers_size);
			}

			if (rc != SCARD.S_SUCCESS)
			{
				if (UseLogger)
					Logger.Trace("SCardListReadersWithDeviceInstanceId failed with error " + SCARD.ErrorToMessage(rc));
				return null;
			}

			if (readers_size == 0)
			{
				if (UseLogger)
					Logger.Trace("SCardListReadersWithDeviceInstanceId returned an empty list");
				return null;
			}

			string readers_str = new string('\0', (int)readers_size);

			if (GetSystem() != System.Win32)
			{
				rc = SCARD.E_UNEXPECTED;
			}
			else
			{
				rc = SCARD.ListReadersWithDeviceInstanceId(hContext, InstanceId, readers_str, ref readers_size);
			}

			if (rc != SCARD.S_SUCCESS)
			{
				if (UseLogger)
					Logger.Trace("SCardListReadersWithDeviceInstanceId failed with error " + SCARD.ErrorToMessage(rc));
				return null;
			}

			for (i = 0; i < readers_size; i++)
			{
				if (readers_str[i] == '\0')
				{
					if (i > 0)
						readers_count++;
					if ((i + 1) < readers_size)
						if (readers_str[i + 1] == '\0')
							break;
				}
			}

			string[] readers = new string[readers_count];

			if (readers_count > 0)
			{
				s = "";
				int j = 0;
				for (i = 0; i < readers_size; i++)
				{
					if (readers_str[i] == '\0')
					{
						readers[j++] = s;
						s = "";
						if (readers_str[i + 1] == '\0')
							break;
					}
					else
					{
						s = s + (char)readers_str[i];
					}
				}
			}

			return readers;
		}

		public static string[] GetReaderListWithDeviceInstanceId(uint Scope, string InstanceId)
		{
			IntPtr hContext = IntPtr.Zero;
			uint rc;

			rc = SCARD.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
			if (rc != SCARD.S_SUCCESS)
			{
				if (UseLogger)
					Logger.Warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
				return null;
			}

			string[] readers = GetReaderListWithDeviceInstanceId(hContext, InstanceId);

			SCARD.ReleaseContext(hContext);

			return readers;
		}

		public static string[] GetReaderListWithDeviceInstanceId(string InstanceId)
		{
			return GetReaderListWithDeviceInstanceId(SCARD.SCOPE_SYSTEM, InstanceId);
		}

        /**
		 *
		 * \brief .NET wrapper for SCardGetReaderDeviceInstanceId (Windows only)
		 *
		 **/
        public static string GetReaderDeviceInstanceId(IntPtr hContext, string ReaderName)
        {
            uint rc;
            uint instanceIdSize = 0;

            if (GetSystem() != System.Win32)
            {
                rc = SCARD.E_UNEXPECTED;
            }
            else
            {
                rc = SCARD.GetReaderDeviceInstanceId(hContext, ReaderName, null, ref instanceIdSize);
            }

            if (rc != SCARD.S_SUCCESS)
            {
                if (UseLogger)
                    Logger.Trace("SCardGetReaderDeviceInstanceId failed with error " + SCARD.ErrorToMessage(rc));
                return null;
            }

            if (instanceIdSize == 0)
            {
                if (UseLogger)
                    Logger.Trace("SCardGetReaderDeviceInstanceId returned an empty instance ID");
                return null;
            }

            string instanceIdStr = new string('\0', (int)instanceIdSize);

            if (GetSystem() != System.Win32)
            {
                rc = SCARD.E_UNEXPECTED;
            }
            else
            {
                rc = SCARD.GetReaderDeviceInstanceId(hContext, ReaderName, instanceIdStr, ref instanceIdSize);
            }

            if (rc != SCARD.S_SUCCESS)
            {
                if (UseLogger)
                    Logger.Trace("SCardListReadersWithDeviceInstanceId failed with error " + SCARD.ErrorToMessage(rc));
                return null;
            }

            string result = "";

            for (int i = 0; i < instanceIdStr.Length; i++)
            {
                if (instanceIdStr[i] == '\0')
                    break;
                result += instanceIdStr[i];
            }
            
            return result;
        }

        public static Dictionary<string, string> ListReadersWithInstanceId(IntPtr hContext, string Groups = null)
        {
            string[] readerNames = ListReaders(hContext, Groups);
            if (readerNames == null)
                return null;
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string readerName in readerNames)
            {
                result[readerName] = GetReaderDeviceInstanceId(hContext, readerName);
            }
            return result;
        }

        public static Dictionary<string, string> ListReadersWithInstanceId(uint Scope, string Groups = null)
        {
            IntPtr hContext = IntPtr.Zero;
            uint rc;

            rc = SCARD.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
            if (rc != SCARD.S_SUCCESS)
            {
                if (UseLogger)
                    Logger.Warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> result = ListReadersWithInstanceId(hContext, Groups);

            SCARD.ReleaseContext(hContext);

            return result;
        }

        public static Dictionary<string, string> ListReadersWithInstanceId(string Groups = null)
        {
            return ListReadersWithInstanceId(SCARD.SCOPE_SYSTEM, Groups);
        }

        public static Dictionary<string, string> ListReadersByInstanceId(IntPtr hContext, string Groups = null)
        {
            string[] readerNames = ListReaders(hContext, Groups);
            if (readerNames == null)
                return null;
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string readerName in readerNames)
            {
                string deviceId = GetReaderDeviceInstanceId(hContext, readerName);
                if (result.ContainsKey(deviceId))
                {
                    result[deviceId] += "|";
                    result[deviceId] += readerName;
                }
                else
                {
                    result[deviceId] = readerName;
                }
            }
            return result;
        }

        public static Dictionary<string, string> ListReadersByInstanceId(uint Scope, string Groups = null)
        {
            IntPtr hContext = IntPtr.Zero;
            uint rc;

            rc = SCARD.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
            if (rc != SCARD.S_SUCCESS)
            {
                if (UseLogger)
                    Logger.Warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> result = ListReadersByInstanceId(hContext, Groups);

            SCARD.ReleaseContext(hContext);

            return result;
        }

        public static Dictionary<string, string> ListReadersByInstanceId(string Groups = null)
        {
            return ListReadersByInstanceId(SCARD.SCOPE_SYSTEM, Groups);
        }
    }
}

