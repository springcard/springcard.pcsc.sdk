/**
 *
 * \ingroup PCSC  
 *
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D and Emilie.C / SpringCard 
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
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	/**
	 *
	 * \brief The SCardChannel object provides the actual connection to the smartcard through the PC/SC reader
	 *
	 **/
	public class SCardChannel : ICardApduTransmitter, ICardTransmitter
    {
		private IntPtr _hContext = IntPtr.Zero;
		private IntPtr _hCard = IntPtr.Zero;
		TransmitDoneCallback _transmit_done_callback = null;
		Thread _transmit_thread = null;

		protected string _reader_name;
		protected uint _active_protocol;
		protected uint _expect_protocols = SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1;
		protected uint _share_mode = SCARD.SHARE_EXCLUSIVE;
		protected uint _reader_state;

		protected CAPDU _capdu;
		protected RAPDU _rapdu;
		protected CardBuffer _cctrl;
		protected CardBuffer _rctrl;
		protected CardBuffer _card_atr;

		protected uint _last_error;

		public delegate void TransmitDoneCallback(RAPDU rapdu);

		/* Protected constructor for derived classes */
		protected SCardChannel()
		{

		}

		/**
		 * \brief Create a SCardChannel for the given PC/SC scope and reader name
		 */
		public SCardChannel(uint Scope, string ReaderName)
		{
			Instanciate(Scope, ReaderName);
		}

		/**
		 * \brief Create a SCardChannel for the given reader name
		 */
		public SCardChannel(string ReaderName)
		{
			Instanciate(SCARD.SCOPE_SYSTEM, ReaderName);
		}

		/**
		 * \brief Create a SCardChannel from a existing SCardReader
		 */
		public SCardChannel(SCardReader Reader)
		{
			Instanciate(Reader.Scope, Reader.Name);
		}

		~SCardChannel()
		{
			if (Connected)
				DisconnectReset();

			if (_hContext != IntPtr.Zero)
				SCARD.ReleaseContext(_hContext);
		}

		private void Instanciate(uint Scope, string ReaderName)
		{
			uint rc;

			rc = SCARD.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, ref _hContext);
			if (rc != SCARD.S_SUCCESS)
			{
				if (SCARD.UseLogger)
					Logger.Warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
				_hContext = IntPtr.Zero;
				_last_error = rc;
			}

			_reader_name = ReaderName;
		}

		/**
		 * \brief The PC/SC context handle. See SCARD.CreateContext()
		 */
		public IntPtr hContext
		{
			get
			{
				return _hContext;
			}
		}

		/**
		 * \brief The card handle. See SCARD.Connect()
		 */
		public IntPtr hCard
		{
			get
			{
				return _hCard;
			}
		}

		/**
		 * \brief The name of the reader
		 */
		public string ReaderName
		{
			get
			{
				return _reader_name;
			}
		}

		protected virtual void UpdateState()
		{
			uint rc;

			_reader_state = SCARD.STATE_UNAWARE;
			_card_atr = null;

			if (Connected)
			{
				byte[] atr_buffer = new byte[36];
				uint atr_length = 36;

				uint dummy = 0;

				rc =
					SCARD.Status(_hCard, IntPtr.Zero, ref dummy,
								 ref _reader_state, ref _active_protocol, atr_buffer,
								 ref atr_length);
				if (rc != SCARD.S_SUCCESS)
				{
					_last_error = rc;
					return;
				}

				_card_atr = new CardBuffer(atr_buffer, (int)atr_length);


			}
			else
			{
				SCARD.READERSTATE[] states = new SCARD.READERSTATE[1];

				states[0] = new SCARD.READERSTATE();
				states[0].szReader = _reader_name;
				states[0].pvUserData = IntPtr.Zero;
				states[0].dwCurrentState = 0;
				states[0].dwEventState = 0;
				states[0].cbAtr = 0;
				states[0].rgbAtr = null;

				try
				{
					rc = SCARD.GetStatusChange(_hContext, 0, states, 1);
				}
				catch (ThreadInterruptedException)
				{
					rc = SCARD.E_CANCELLED;
				}

				if (rc != SCARD.S_SUCCESS)
				{
					_last_error = rc;
					return;
				}

				_reader_state = states[0].dwEventState;

				if ((_reader_state & SCARD.STATE_PRESENT) != 0)
				{
					_card_atr = new CardBuffer(states[0].rgbAtr, (int)states[0].cbAtr);
				}
			}
		}

		/**
		 * \brief Is there a card in the reader?
		 */
		public bool CardPresent
		{
			get
			{
				UpdateState();

				if ((_reader_state & SCARD.STATE_PRESENT) != 0)
					return true;

				return false;
			}
		}

		/**
		 * \brief Is there a card in the reader, and the card has not been connected by another application yet?
		 */
		public bool CardAvailable
		{
			get
			{
				UpdateState();

				if (((_reader_state & SCARD.STATE_PRESENT) != 0)
					&& ((_reader_state & SCARD.STATE_MUTE) == 0)
					&& ((_reader_state & SCARD.STATE_INUSE) == 0))
					return true;

				return false;
			}
		}

		/**
		 * \brief The ATR of the card in the reader. null if no card is present.
		 */
		public CardBuffer CardAtr
		{
			get
			{
				return GetAtr();
			}
		}

		public CardBuffer GetAtr()
		{
			UpdateState();
			return _card_atr;
		}

		/**
		 * \brief Is the SCardChannel object actually connected to a card?
		 */
		public virtual bool Connected
		{
			get
			{
				if (_hCard != IntPtr.Zero)
					return true;

				return false;
			}
		}

		/**
		 * \brief Is the SCardChannel object directly connected to the reader?
		 */
		public virtual bool ConnectedDirect
		{
			get
			{
				if (!Connected)
					return false;

				return (_share_mode == SCARD.SHARE_DIRECT);
			}
		}

		/**
		 * \brief Card protocol
		 *
		 * Before the smartcard has been connected, set Protocol to specify the communication protocol(s) to be used by Connect().
		 * Allowed values are SCARD.PROTOCOL_T0, SCARD.PROTOCOL_T1 or SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1.
		 *
		 * Once the smartcard has been connected (Connected == true), Protocol is the current communication protocol.
		 * Possible values are SCARD.PROTOCOL_T0 or SCARD.PROTOCOL_T1.
		 */
		public uint Protocol
		{
			get
			{
				return _active_protocol;
			}
			set
			{
				_expect_protocols = value;
			}
		}


		/**
		 * \brief Translation of Protocol to/from string
		 *
		 * Before the smartcard has been connected, set ProtocolAsString to specify the communication protocol(s) to be used by Connect().
		 * Allowed values are "T=0", "T=1" or "*" (or "T=0|T=1").
		 *
		 * Once the smartcard has been connected (Connected == true), ProtocolAsString is the current communication protocol.
		 * Possible values are "T=0" or "T=1".
		 */
		public string ProtocolAsString
		{
			get
			{
				return SCARD.CardProtocolToString(_active_protocol);
			}
			set
			{
				value = value.ToUpper();

                switch (value)
                {
                    case "T=0":
					    _expect_protocols = SCARD.PROTOCOL_T0;
                        break;
                    case "T=1":
    					_expect_protocols = SCARD.PROTOCOL_T1;
                        break;
                    case "*":
                    case "AUTO":
                    case "T=0|T=1":
    					_expect_protocols = SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1;
                        break;
                    case "RAW":
					    _expect_protocols = SCARD.PROTOCOL_RAW;
                        break;
                    case "":
                    case "NONE":
                        _expect_protocols = SCARD.PROTOCOL_NONE;
                        break;
                    case "DIRECT":
                        _expect_protocols = SCARD.PROTOCOL_DIRECT();
                        _share_mode = SCARD.SHARE_DIRECT;
                        break;

                    default:
                        throw new WarningException("Invalid Protocol string");
                }
			}
		}

		/**
		 * \brief Card share mode
		 *
		 * Before the smartcard has been connected, set ShareMode to specify the share mode to be used by Connect().
		 * Allowed values are SCARD.SHARE_EXCLUSIVE, SCARD.SHARE_SHARED or SCARD.SHARE_DIRECT.
		 *
		 **/
		public uint ShareMode
		{
			get
			{
				return _share_mode;
			}
			set
			{
				_share_mode = value;
			}
		}

		/**
		 * \brief Translation of ShareMode to/from string
		 * 
		 * Before the smartcard has been connected, set ShareModeAsString to specify the sharing mode to be used by Connect().
		 * Allowed values are "EXCLUSIVE", "SHARED" or "DIRECT".
		 **/
		public string ShareModeAsString
		{
			get
			{
				return SCARD.CardShareModeToString(_share_mode);
			}
			set
			{
				switch (value.ToUpper())
                {
                    case "EXCLUSIVE":
    					_share_mode = SCARD.SHARE_EXCLUSIVE;
                        break;
                    case "SHARED":
					    _share_mode = SCARD.SHARE_SHARED;
                        break;
                    case "DIRECT":
					    _expect_protocols = SCARD.PROTOCOL_DIRECT();
					    _share_mode = SCARD.SHARE_DIRECT;
                        break;

                    default:
                        throw new WarningException("Invalid ShareMode string");
                }
			}
		}

		/**
		 * \brief Open the connection channel to the smart card (according to the specified Protocol, default is either T=0 or T=1)
		 **/
		public virtual bool Connect()
		{
			uint rc;

			if (Connected)
				return false;

			if (SCARD.UseLogger)
				Logger.Trace("Connect to '" + _reader_name + "', share=" + _share_mode + ", protocol=" + _expect_protocols);

			rc = SCARD.Connect(_hContext, _reader_name, _share_mode, _expect_protocols, ref _hCard, ref _active_protocol);
			if (rc != SCARD.S_SUCCESS)
			{
				if (SCARD.UseLogger)
					Logger.Trace(String.Format("Connect error {0:X08} ({0})", rc));
				_hCard = IntPtr.Zero;
				_last_error = rc;
				return false;
			}

			UpdateState();
			return true;
		}

		/**
		 * \brief Set ShareMode to SCARD.SHARE_EXCLUSIVE and call Connect()
		 **/
		public virtual bool ConnectExclusive()
		{
			if (Connected)
				return false;

			_share_mode = SCARD.SHARE_EXCLUSIVE;

			return Connect();
		}

		/**
		 * \brief Set ShareMode to SCARD.SHARE_SHARED and call Connect()
		 **/
		public virtual bool ConnectShared()
		{
			if (Connected)
				return false;

			_share_mode = SCARD.SHARE_SHARED;

			return Connect();
		}

		/**
		 * \brief Set ShareMode to SCARD.SHARE_DIRECT and Protocol to SCARD.PROTOCOL_DIRECT(), and call Connect()
		 **/
		public virtual bool ConnectDirect()
		{
			if (Connected)
				return false;

			_share_mode = SCARD.SHARE_DIRECT;
            _expect_protocols = SCARD.PROTOCOL_DIRECT();

            return Connect();
		}

		/**
		 * \brief Close the connection channel with the card
		 *
		 * The disposition parameter must take one of the following values:
		 * - SCARD.EJECT_CARD
		 * - SCARD.UNPOWER_CARD
		 * - SCARD.RESET_CARD
		 * - SCARD.LEAVE_CARD
		 * If this parameter is omitted, it defaults to SCARD.RESET_CARD
		 */
		public virtual bool Disconnect(uint disposition = SCARD.RESET_CARD)
		{
			uint rc;

			if (SCARD.UseLogger)
				Logger.Trace("Disconnect, disposition=" + disposition);

			rc = SCARD.Disconnect(_hCard, disposition);
			if (rc != SCARD.S_SUCCESS)
				_last_error = rc;

			_hCard = IntPtr.Zero;

			if (rc != SCARD.S_SUCCESS)
				return false;

			return true;
		}

		/**
		 * \brief Same as Disconnect(SCARD.RESET_CARD)
		 */
		public void Disconnect()
		{
			DisconnectReset();
		}

		/**
		 * \brief Shortcut for Disconnect(SCARD.EJECT_CARD)
		 */
		public bool DisconnectEject()
		{
			return Disconnect(SCARD.EJECT_CARD);
		}

		/**
		 * \brief Shortcut for Disconnect(SCARD.UNPOWER_CARD)
		 */
		public bool DisconnectUnpower()
		{
			return Disconnect(SCARD.UNPOWER_CARD);
		}

		/**
		 * \brief Shortcut for Disconnect(SCARD.RESET_CARD)
		 */
		public bool DisconnectReset()
		{
			return Disconnect(SCARD.RESET_CARD);
		}

		/**
		 * \brief Shortcut for Disconnect(SCARD.LEAVE_CARD)
		 */
		public bool DisconnectLeave()
		{
			return Disconnect(SCARD.LEAVE_CARD);
		}

		/**
		 * \brief Keep the connection channel with the card open, but physically disconnect/reconnect the card
		 *
		 * The disposition parameter must take one of the following values:
		 * - SCARD.EJECT_CARD
		 * - SCARD.UNPOWER_CARD
		 * - SCARD.RESET_CARD
		 * - SCARD.LEAVE_CARD
		 * If this parameter is omitted, it defaults to SCARD.RESET_CARD
		 */
		public virtual bool Reconnect(uint disposition)
		{
			uint rc;

			if (!Connected)
				return false;

			rc =
				SCARD.Reconnect(_hCard, _share_mode, _expect_protocols, disposition, ref _active_protocol);
			if (rc != SCARD.S_SUCCESS)
			{
				_hCard = IntPtr.Zero;
				_last_error = rc;
				return false;
			}

			UpdateState();
			return true;
		}

		/**
		 * \brief Same as Reconnect(SCARD.RESET_CARD)
		 */
		public void Reconnect()
		{
			ReconnectReset();
		}

		/**
		 * \brief Shortcut for Reconnect(SCARD.UNPOWER_CARD)
		 */
		public void ReconnectUnpower()
		{
			Reconnect(SCARD.UNPOWER_CARD);
		}

		/**
		 * \brief Shortcut for Reconnect(SCARD.RESET_CARD)
		 */
		public void ReconnectReset()
		{
			Reconnect(SCARD.RESET_CARD);
		}

		/**
		 * \brief Shortcut for Reconnect(SCARD.LEAVE_CARD)
		 */
		public void ReconnectLeave()
		{
			Reconnect(SCARD.LEAVE_CARD);
		}

		#region Transmit

		/**
		 * \brief Send a command APDU to the connected card through SCARD.Transmit(), and retrieve its response APDU. Return null in case the card is not connected or has been lost.
		 *
		 * \code{.cs}
		 *
		 *   SCardChannel card = new SCardChannel( ... reader ... );
		 *   if (!card.Connect( SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1 ))
		 *   {
		 *     // handle error
		 *   }
		 *
		 *   byte[] response = card.Transmit(new byte[] {0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00} );
		 *   if (response == null)
		 *   {
		 *     // handle error
		 *   }
		 *   MessageBox.Show("Card answered!");
		 */
		public byte[] Transmit(byte[] capdu)
		{
			uint rsp_length = 32 * 1024;
			byte[] rsp_buffer = new byte[rsp_length];
			uint rc;
			IntPtr SendPci = IntPtr.Zero;

			switch (_active_protocol)
			{
				case SCARD.PROTOCOL_T0:
					SendPci = SCARD.PCI_T0();
					break;
				case SCARD.PROTOCOL_T1:
					SendPci = SCARD.PCI_T1();
					break;
				case SCARD.PROTOCOL_RAW:
					SendPci = SCARD.PCI_RAW();
					break;
				default:
					break;
			}

			if (SCARD.UseLogger)
				Logger.Debug("Transmit << " + BinConvert.ToHex(capdu));

			rc = SCARD.Transmit(_hCard,
								SendPci,
								capdu,
								(uint)capdu.Length,
								IntPtr.Zero, /* RecvPci is likely to remain NULL */
								rsp_buffer,
								ref rsp_length);

			if (rc != SCARD.S_SUCCESS)
			{
				if (SCARD.UseLogger)
					Logger.Trace("Transmit : " + rc);
				_last_error = rc;
				return null;
			}

			byte[] rapdu = new byte[rsp_length];
			Array.Copy(rsp_buffer, rapdu, rsp_length);

			if (SCARD.UseLogger)
				Logger.Debug("Transmit >> " + BinConvert.ToHex(rapdu));

			return rapdu;
		}

		/**
		 * \brief Send a command APDU (CAPDU) to the connected card through SCARD.Transmit(), and retrieve its response APDU (RAPDU).
		 *
		 * \code{.cs}
		 *
		 *   SCardChannel card = new SCardChannel( ... reader ... );
		 *   if (!card.Connect( SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1 ))
		 *   {
		 *     // handle error
		 *   }
		 *
		 *   card.Command = new CAPDU("00 A4 00 00 02 3F 00");
		 *   if (!card.Transmit())
		 *   {
		 *     // handle error
		 *   }
		 *   RAPDU response = card.Response;
		 *   MessageBox.Show("Card answered: " + response.AsString(" "));
		 */
		public virtual bool Transmit()
		{
			uint rsp_length = 32 * 1024;
			byte[] rsp_buffer = new byte[rsp_length];
			uint rc;
			IntPtr SendPci = IntPtr.Zero;

			switch (_active_protocol)
			{
				case SCARD.PROTOCOL_T0:
					SendPci = SCARD.PCI_T0();
					break;
				case SCARD.PROTOCOL_T1:
					SendPci = SCARD.PCI_T1();
					break;
				case SCARD.PROTOCOL_RAW:
					SendPci = SCARD.PCI_RAW();
					break;
				default:
					break;
			}

			_rapdu = null;

			if (SCARD.UseLogger)
				Logger.Debug("Transmit << " + _capdu.AsString());

			rc = SCARD.Transmit(_hCard,
								SendPci,
								_capdu.GetBytes(),
								(uint)_capdu.Length,
								IntPtr.Zero, /* RecvPci is likely to remain NULL */
								rsp_buffer,
								ref rsp_length);

			if (rc != SCARD.S_SUCCESS)
			{
				if (SCARD.UseLogger)
					Logger.Trace("Transmit : " + rc);
				_last_error = rc;
				return false;
			}

			_rapdu = new RAPDU(rsp_buffer, (int)rsp_length);

			if (SCARD.UseLogger)
				Logger.Debug("Transmit >> " + _rapdu.AsString());

			return true;
		}

		/**
		 * \brief Command APDU to send to the card through Transmit()
		 */
		public CAPDU Command
		{
			get
			{
				return _capdu;
			}
			set
			{
				_capdu = value;
			}
		}

		/**
		 * \brief Response APDU returned by the card after Transmit()
		 */
		public RAPDU Response
		{
			get
			{
				return _rapdu;
			}
		}

		/**
		 * \brief Send a command APDU (CAPDU) to the connected card through SCARD.Transmit(), and retrieve its response APDU (RAPDU). Return false in case the card is not connected or has been lost.
		 *
		 * \code{.cs}
		 *
		 *   SCardChannel card = new SCardChannel( ... reader ... );
		 *   if (!card.Connect( SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1 ))
		 *   {
		 *     // handle error
		 *   }
		 *
		 *   CAPDU command = new CAPDU("00 A4 00 00 02 3F 00");
		 *   RAPDU response;
		 *   if (!card.Transmit(command, out response))
		 *   {
		 *     // handle error
		 *   }
		 *   MessageBox.Show("Card answered: " + response.AsString(" "));
		 */
		public bool Transmit(CAPDU capdu, out RAPDU rapdu)
		{
			rapdu = null;
			_capdu = capdu;

			if (!Transmit())
				return false;

			rapdu = _rapdu;
			return true;
		}

		/**
		 * \brief Send a command APDU (CAPDU) to the connected card through SCARD.Transmit(), and retrieve its response APDU (RAPDU). Return null in case the card is not connected or has been lost.
		 *
		 * \code{.cs}
		 *
		 *   SCardChannel card = new SCardChannel( ... reader ... );
		 *   if (!card.Connect( SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1 ))
		 *   {
		 *     // handle error
		 *   }
		 *
		 *   RAPDU response = card.Transmit(new CAPDU("00 A4 00 00 02 3F 00")))
		 *   if (response == null)
		 *   {
		 *     // handle error
		 *   }
		 *   MessageBox.Show("Card answered: " + response.AsString(" "));
		 */
		public RAPDU Transmit(CAPDU capdu)
		{
			_capdu = capdu;

			if (!Transmit())
				return null;

			return _rapdu;
		}

		/**
		 * \brief Send a command APDU (CAPDU) to the connected card through SCARD.Transmit() in a background thread. A callback is fired to provide the response APDU (RAPDU) when done.
		 *
		 * \code{.cs}
		 *
		 *   SCardChannel card = new SCardChannel( ... reader ... );
		 *   if (!card.Connect( SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1 ))
		 *   {
		 *     // handle error
		 *   }
		 *
		 *   // In this example the Transmit is performed by a background thread
		 *   // We supply a delegate so the main class (window/form) will be notified
		 *   // when Transmit will return
		 *
		 *   delegate void OnTransmitDoneInvoker(RAPDU response);
		 *
		 *   void OnTransmitDone(RAPDU response)
		 *   {
		 *     // Ensure we're back in the context of the main thread (application's message pump)
		 *     if (this.InvokeRequired)
		 *     {
		 *       this.BeginInvoke(new OnTransmitDoneInvoker(OnTransmitDone), response);
		 *       return;
		 *     }
		 *
		 *     if (response == null)
		 *     {
		 *       // handle error
		 *     }
		 *
		 *     MessageBox.Show("Card answered: " + response.AsString(" "));
		 *   }
		 *
		 *  card.Transmit(new CAPDU("00 A4 00 00 02 3F 00"), new SCardChannel.TransmitDoneCallback(OnTransmitDone));
		 */
		public void Transmit(CAPDU capdu, TransmitDoneCallback callback)
		{
			if (_transmit_thread != null)
				_transmit_thread = null;

			_capdu = capdu;

			if (callback != null)
			{
				_transmit_done_callback = callback;
				_transmit_thread = new Thread(TransmitFunction);
				_transmit_thread.Start();
			}
		}

		private void TransmitFunction()
		{
			if (Transmit())
			{
				if (_transmit_done_callback != null)
					_transmit_done_callback(_rapdu);
			}
			else
			{
				if (_transmit_done_callback != null)
					_transmit_done_callback(null);
			}
		}



		#endregion

		#region Control

		/**
		 * \brief Send a direct command the reader through SCARD.Control(). Return null in case the reader does not answer or has been lost.
		 */
		public virtual byte[] Control(byte[] cctrl)
		{
			byte[] rctrl = new byte[280];
			uint rl = 0;
			uint rc;

			if (SCARD.UseLogger)
				Logger.Debug("Control << " + (new CardBuffer(cctrl)).AsString());

			rc = SCARD.Control(_hCard,
							   SCARD.IOCTL_CSB6_PCSC_ESCAPE,
							   cctrl,
							   (uint)cctrl.Length,
							   rctrl,
							   280,
							   ref rl);

			if (rc == 1)
			{
				rc = SCARD.Control(_hCard,
								   SCARD.IOCTL_MS_CCID_ESCAPE,
								   cctrl,
								   (uint)cctrl.Length,
								   rctrl,
								   280,
								   ref rl);

			}

			if (rc != SCARD.S_SUCCESS)
			{
				if (SCARD.UseLogger)
					Logger.Trace("Control: " + rc);
				_last_error = rc;
				rctrl = null;
				return null;
			}

			byte[] r = new byte[rl];
			for (int i = 0; i < rl; i++)
				r[i] = rctrl[i];

			if (SCARD.UseLogger)
				Logger.Debug("Control >> " + (new CardBuffer(r)).AsString());

			return r;
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control(). Return false in case the reader does not answer or has been lost.
		 */
		public bool Control()
		{
			byte[] r = Control(_cctrl.GetBytes());

			if (r == null)
				return false;

			_rctrl = null;

			if (r.Length > 0)
				_rctrl = new CardBuffer(r);

			return true;
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control(). Return false in case the reader does not answer or has been lost.
		 */
		public bool Control(CardBuffer cctrl, out CardBuffer rctrl)
		{
			rctrl = null;
			_cctrl = cctrl;

			if (!Control())
				return false;

			rctrl = _rctrl;
			return true;
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control(). Return null in case the reader does not answer or has been lost.
		 */
		public CardBuffer Control(CardBuffer cctrl)
		{
			_cctrl = cctrl;

			if (!Control())
				return null;

			return _rctrl;
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control(). Return false in case the reader does not answer or has been lost.
		 */
		public bool Control(CAPDU cctrl, out RAPDU rctrl)
		{
			rctrl = null;
			_cctrl = cctrl;

			if (!Control())
				return false;

			rctrl = new RAPDU(_rctrl);
			return true;
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control(). Return null in case the reader does not answer or has been lost.
		 */
		public RAPDU Control(CAPDU cctrl)
		{
			_cctrl = cctrl;

			if (!Control())
				return null;

			return new RAPDU(_rctrl);
		}

		/**
		 * \brief Send a command to the reader to retrieve its serial number.
		 *
		 * \deprecated Use the SpringCard.PCSC.ReaderHelper library instead
		 */
		public string SerialNumber()
		{
			/* Serial number */
			CardBuffer r = Control(new CardBuffer("582003"));
			if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
			{
				string s = new String(r.GetChars(1, r.Length - 1));
				return s;
			}
			return "";
		}

		/**
		 * \brief Send a command to the reader to wink it.
		 *
		 * \deprecated Use the SpringCard.PCSC.ReaderHelper library instead
		 */
		public void Wink()
		{
			Control(new CardBuffer("581C0800"));
			Control(new CardBuffer("581E010100000800"));
		}

		/**
		 * \brief Send a LED command to the reader.
		 *
		 * \deprecated Use the SpringCard.PCSC.ReaderHelper library instead
		 */
		public bool Leds(byte red, byte green, byte blue)
		{
			byte[] buffer = new byte[5];

			buffer[0] = 0x58;
			buffer[1] = 0x1E;
			buffer[2] = red;
			buffer[3] = green;
			buffer[4] = blue;

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}

		/**
		 * \brief Restore the reader's LED to their default behaviour.
		 *
		 * \deprecated Use the SpringCard.PCSC.ReaderHelper library instead
		 */
		public bool LedsDefault()
		{
			byte[] buffer = new byte[2];

			buffer[0] = 0x58;
			buffer[1] = 0x1E;

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}

		/**
		 * \brief Send a buzzer command to the reader
		 *
		 * \deprecated Use the SpringCard.PCSC.ReaderHelper library instead
		 */

		public bool Buzzer(ushort duration_ms)
		{
			byte[] buffer = new byte[4];

			buffer[0] = 0x58;
			buffer[1] = 0x1C;
			buffer[2] = (byte)(duration_ms / 0x0100);
			buffer[3] = (byte)(duration_ms % 0x0100);

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}

		/**
		 * \brief Restore the reader's buzzer to its default behaviour.
		 *
		 * \deprecated Use the SpringCard.PCSC.ReaderHelper library instead
		 */
		public bool BuzzerDefault()
		{
			byte[] buffer = new byte[2];

			buffer[0] = 0x58;
			buffer[1] = 0x1C;

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}

		/**
		 * \brief Change the reader's debug level dynamically
		 *
		 * \deprecated Use the SpringCard.PCSC.ReaderHelper library instead
		 */
		public bool DebugLevel(ushort debug_level)
		{
			byte[] buffer = new byte[5];

			buffer[0] = 0x58;
			buffer[1] = 0x8D;
			buffer[2] = 0xCD;
			buffer[3] = (byte)(debug_level / 0x0100);
			buffer[4] = (byte)(debug_level % 0x0100);

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}
		#endregion

		#region GetAttrib
		public virtual byte[] GetAttrib(uint AttrId)
		{
			byte[] attr = new byte[280];
			uint attrlen = 280;
			uint rc;

			if (SCARD.UseLogger)
				Logger.Trace("GetAttrib({0})", BinConvert.ToHex(AttrId));

			rc = SCARD.GetAttrib(_hCard,
								 AttrId,
								 attr,
								 ref attrlen);

			if (rc != SCARD.S_SUCCESS)
			{
				if (SCARD.UseLogger)
					Logger.Trace("GetAttrib({0}) -> {1}", BinConvert.ToHex(AttrId), rc);
				_last_error = rc;
				return null;
			}

			byte[] result = new byte[attrlen];
			Array.Copy(attr, 0, result, 0, attrlen);

			if (SCARD.UseLogger)
				Logger.Trace("GetAttrib({0}) >> {1}", BinConvert.ToHex(AttrId), BinConvert.ToHex(result));

			return result;
		}

		public virtual string GetAttribString(uint AttrId)
		{
			byte[] attr = GetAttrib(AttrId);

			string result = "";

			if (attr != null)
			{
				if ((attr.Length > 1) && (attr[0] == 0))
				{
					for (int i = 1; i < attr.Length; i++)
						result += (char)attr[i];
				}
				else
				{
					for (int i = 0; i < attr.Length; i++)
						result += (char)attr[i];
				}
			}

			if (SCARD.UseLogger)
				Logger.Trace("GetAttrib({0}) >> {1}", BinConvert.ToHex(AttrId), result);

			return result;
		}

		public virtual uint GetAttribDWord(uint AttrId)
		{
			byte[] attr = GetAttrib(AttrId);

			uint result = 0;

			if ((attr != null) && (attr.Length == 4))
			{
				for (int i = 0; i < attr.Length; i++)
				{
					result *= 256;
					result += attr[i];
				}
			}

			if (SCARD.UseLogger)
				Logger.Trace("GetAttrib({0}) >> {1}", BinConvert.ToHex(AttrId), result);

			return result;
		}

		public uint GetAttribChannelId()
		{
			return GetAttribDWord(SCARD.ATTR_CHANNEL_ID);
		}

		public uint GetAttribCharacteristics()
		{
			return GetAttribDWord(SCARD.ATTR_CHARACTERISTICS);
		}

		public string GetAttribFriendlyName()
		{
			return GetAttribString(SCARD.ATTR_DEVICE_FRIENDLY_NAME);
		}

		public string GetAttribSystemName()
		{
			return GetAttribString(SCARD.ATTR_DEVICE_SYSTEM_NAME);
		}

		public uint GetAttribDeviceUnit()
		{
			return GetAttribDWord(SCARD.ATTR_DEVICE_UNIT);
		}

		public string GetAttribSerialNumber()
		{
			return GetAttribString(SCARD.ATTR_VENDOR_IFD_SERIAL_NO);
		}

		public string GetAttribProductName()
		{
			return GetAttribString(SCARD.ATTR_VENDOR_IFD_TYPE);
		}

		public uint GetAttribProductVersion()
		{
			return GetAttribDWord(SCARD.ATTR_VENDOR_IFD_VERSION);
		}

		public string GetAttribVendorName()
		{
			return GetAttribString(SCARD.ATTR_VENDOR_NAME);
		}

		#endregion

		/**
		 * \brief Return the last error encountered by this SCardChannel
		 */
		public uint LastError
		{
			get
			{
				return _last_error;
			}
		}

		/**
		 * \brief Return the last error encountered by this SCardChannel, as a string
		 */
		public string LastErrorAsString
		{
			get
			{
				return SCARD.ErrorToString(_last_error);
			}
		}

	}
}