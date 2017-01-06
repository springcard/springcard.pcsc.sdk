/**h* SpringCard/PCSC_SCardChannel
 *
 * NAME
 *   PCSC : SCardChannel
 * 
 * DESCRIPTION
 *   SpringCard's wrapper for PC/SC API
 *
 * COPYRIGHT
 *   Copyright (c) 2010-2015 SpringCard - www.springcard.com
 *
 * AUTHOR
 *   Johann.D and Emilie.C / SpringCard
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
	/**c* SpringCardPCSC/SCardChannel
	 *
	 * NAME
	 *   SCardChannel
	 * 
	 * DESCRIPTION
	 *   The SCardChannel object provides the actual connection to the smartcard through the PC/SC reader
	 *
	 * SYNOPSIS
	 *   SCardChannel( string reader_name );
	 *   SCardChannel( SCardReader reader );
	 *
	 **/

	public class SCardChannel
	{		
		private IntPtr _hContext = IntPtr.Zero;
		private IntPtr _hCard = IntPtr.Zero;
		TransmitDoneCallback _transmit_done_callback = null;
		Thread _transmit_thread = null;

		protected string _reader_name;
		protected uint _active_protocol;
		protected uint _want_protocols = SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1;
		protected uint _share_mode = SCARD.SHARE_SHARED;		
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

		public SCardChannel(uint Scope, string ReaderName)
		{
			Instanciate(Scope, ReaderName);
		}

		public SCardChannel(string ReaderName)
		{
			Instanciate(SCARD.SCOPE_SYSTEM, ReaderName);
		}
		
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
				_hContext = IntPtr.Zero;
				_last_error = rc;
			}

			_reader_name = ReaderName;
		}

		public IntPtr hContext
		{
			get
			{
				return _hContext;
			}
		}

		public IntPtr hCard
		{
			get
			{
				return _hCard;
			}
		}
		
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
				byte[]atr_buffer = new byte[36];
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

				_card_atr = new CardBuffer(atr_buffer, (int) atr_length);


			} else
			{
				SCARD.READERSTATE[]states = new SCARD.READERSTATE[1];

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
					_card_atr = new CardBuffer(states[0].rgbAtr, (int) states[0].cbAtr);
				}
			}
		}

		/**v* SCardChannel/CardPresent
		 *
		 * NAME
		 *   SCardChannel.CardPresent
		 *
		 * SYNOPSIS
		 *   bool CardPresent
		 *
		 * OUTPUT
		 *   Returns true if a card is present in the reader associated to the SCardChannel object.
		 *   Returns false if there's no smartcard in the reader.
		 * 
		 * SEE ALSO
		 *   SCardChannel.CardAvailable
		 *   SCardChannel.CardAtr
		 *
		 **/

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

		/**v* SCardChannel/CardAvailable
		 *
		 * NAME
		 *   SCardChannel.CardAvailable
		 *
		 * SYNOPSIS
		 *   bool CardAvailable
		 *
		 * OUTPUT
		 *   Returns true if a card is available in the reader associated to the SCardChannel object.
		 *   Returns false if there's no smartcard in the reader, or if it is already used by another process/thread.
		 * 
		 * SEE ALSO
		 *   SCardChannel.CardPresent
		 *   SCardChannel.CardAtr
		 *
		 **/

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

		/**v* SCardChannel/CardAtr
		 *
		 * NAME
		 *   SCardChannel.CardAtr
		 *
		 * SYNOPSIS
		 *   CardBuffer CardAtr
		 *
		 * OUTPUT
		 *   Returns the ATR of the smartcard in the reader, or null if no smartcard is present.
		 * 
		 * SEE ALSO
		 *   SCardChannel.CardPresent
		 *
		 **/

		public CardBuffer CardAtr
		{
			get
			{
				UpdateState();
				return _card_atr;
			}
		}

		/**v* SCardChannel/Connected
		 *
		 * NAME
		 *   SCardChannel.Connected
		 *
		 * SYNOPSIS
		 *   bool Connected
		 *
		 * OUTPUT
		 *   Returns true if the SCardChannel object is actually connected to a smartcard.
		 *   Returns false if not.
		 *
		 **/

		public virtual bool Connected
		{
			get
			{
				if (_hCard != IntPtr.Zero)
					return true;

				return false;
			}
		}

		public virtual bool ConnectedDirect
		{
			get
			{
				if (!Connected)
					return false;

				return (_share_mode == SCARD.SHARE_DIRECT);
			}
		}
		
		/**v* SCardChannel/Protocol
		 *
		 * NAME
		 *   SCardChannel.Protocol
		 *
		 * SYNOPSIS
		 *   uint Protocol
		 * 
		 * INPUTS
		 *   Before the smartcard has been
		 * 
		 * 
		 * , set Protocol to specify the communication protocol(s) to be used
		 *   by Connect(). Allowed values are SCARD.PROTOCOL_T0, SCARD.PROTOCOL_T1 or SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1.
		 *
		 * OUTPUT
		 *   Once the smartcard has been connected (Connected == true), Protocol is the current communication protocol.
		 *   Possible values are SCARD.PROTOCOL_T0 or SCARD.PROTOCOL_T1.
		 * 
		 * SEE ALSO
		 *   SCardChannel.ProtocolAsString
		 *
		 **/
		public uint Protocol
		{
			get
			{
				return _active_protocol;
			}
			set
			{
				_want_protocols = value;
			}
		}


		/**v* SCardChannel/ProtocolAsString
		 *
		 * NAME
		 *   SCardChannel.ProtocolAsString
		 *
		 * SYNOPSIS
		 *   string ProtocolAsString
		 * 
		 * INPUTS
		 *   Before the smartcard has been connected, set ProtocolAsString to specify the communication protocol(s) to be used
		 *   by Connect(). Allowed values are "T=0", "T=1" or "*" (or "T=0|T=1").
		 *
		 * OUTPUT
		 *   Once the smartcard has been connected (Connected == true), ProtocolAsString is the current communication protocol.
		 *   Possible values are "T=0" or "T=1".
		 *
		 * SEE ALSO
		 *   SCardChannel.Protocol
		 *
		 **/
		public string ProtocolAsString
		{
			get
			{
				return SCARD.CardProtocolToString(_active_protocol);
			}
			set
			{
				value = value.ToUpper();

				if (value.Equals("T=0"))
				{
					_want_protocols = SCARD.PROTOCOL_T0;
				} else
					if (value.Equals("T=1"))
				{
					_want_protocols = SCARD.PROTOCOL_T1;
				} else
					if (value.Equals("*") || value.Equals("AUTO") || value.Equals("T=0|T=1"))
				{
					_want_protocols = SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1;
				} else
					if (value.Equals("RAW"))
				{
					_want_protocols = SCARD.PROTOCOL_RAW;
				} else
					if (value.Equals("") || value.Equals("NONE") || value.Equals("DIRECT"))
				{
					_want_protocols = SCARD.PROTOCOL_NONE;
					_share_mode = SCARD.SHARE_DIRECT;
				}
			}
		}

		/**v* SCardChannel/ShareMode
		 *
		 * NAME
		 *   SCardChannel.ShareMode
		 *
		 * SYNOPSIS
		 *   uint ShareMode
		 * 
		 * INPUTS
		 *   Before the smartcard has been connected, set ShareMode to specify the sharing mode to be used
		 *   by Connect(). Allowed values are SCARD.SHARE_EXCLUSIVE, SCARD.SHARE_SHARED or SCARD.SHARE_DIRECT.
		 *
		 * SEE ALSO
		 *   SCardChannel.ShareModeAsString
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

		/**v* SCardChannel/ShareModeAsString
		 *
		 * NAME
		 *   SCardChannel.ShareModeAsString
		 *
		 * SYNOPSIS
		 *   string ShareModeAsString
		 * 
		 * INPUTS
		 *   Before the smartcard has been connected, set ShareModeAsString to specify the sharing mode to be used
		 *   by Connect(). Allowed values are "EXCLUSIVE", "SHARED" or "DIRECT".
		 *
		 * SEE ALSO
		 *   SCardChannel.ShareMode
		 *
		 **/
		public string ShareModeAsString
		{
			get
			{
				return SCARD.CardShareModeToString(_share_mode);
			}
			set
			{
				value = value.ToUpper();

				if (value.Equals("EXCLUSIVE"))
				{
					_share_mode = SCARD.SHARE_EXCLUSIVE;
				} else
					if (value.Equals("SHARED"))
				{
					_share_mode = SCARD.SHARE_SHARED;
				} else
					if (value.Equals("DIRECT"))
				{
					_want_protocols = SCARD.PROTOCOL_NONE;
					_share_mode = SCARD.SHARE_DIRECT;
				}
			}
		}

		/**m* SCardChannel/Connect
		 *
		 * NAME
		 *   SCardChannel.Connect()
		 *
		 * SYNOPSIS
		 *   bool Connect()
		 * 
		 * DESCRIPTION
		 *   Open the connection channel to the smartcard (according to the specified Protocol, default is either T=0 or T=1)
		 *
		 * OUTPUT
		 *   Returns true if the connection has been successfully established.
		 *   Returns false if not. See LastError for details.
		 * 
		 * SEE ALSO
		 *   SCardChannel.CardPresent
		 *   SCardChannel.CardAtr
		 *   SCardChannel.Protocol
		 *   SCardChannel.Transmit
		 *
		 **/

		public virtual bool Connect()
		{
			uint rc;

			if (Connected)
				return false;
			
			Trace.WriteLine("Connect to '" + _reader_name + "', share=" + _share_mode + ", protocol=" + _want_protocols);

			rc = SCARD.Connect(_hContext, _reader_name, _share_mode, _want_protocols, ref _hCard, ref _active_protocol);
			if (rc != SCARD.S_SUCCESS)
			{
				Trace.WriteLine("Connect error: " + rc);
				_hCard = IntPtr.Zero;
				_last_error = rc;
				return false;
			}

			UpdateState();
			return true;
		}

		/**m* SCardChannel/Disconnect
		 *
		 * NAME
		 *   SCardChannel.Disconnect()
		 *
		 * SYNOPSIS
		 *   bool Disconnect()
		 *   bool Disconnect(uint disposition)
		 * 
		 * DESCRIPTION
		 *   Close the connection channel
		 *
		 * INPUTS
		 *   The disposition parameter must take one of the following values:
		 *   - SCARD.EJECT_CARD
		 *   - SCARD.UNPOWER_CARD
		 *   - SCARD.RESET_CARD
		 *   - SCARD.LEAVE_CARD
		 *   If this parameter is omitted, it defaults to SCARD.RESET_CARD
		 * 
		 * SEE ALSO
		 *   SCardChannel.Connect
		 *
		 **/

		public virtual bool Disconnect(uint disposition)
		{
			uint rc;
			
			Trace.WriteLine("Disconnect, disposition=" + disposition);

			rc = SCARD.Disconnect(_hCard, disposition);
			if (rc != SCARD.S_SUCCESS)
				_last_error = rc;

			_hCard = IntPtr.Zero;
			
			if (rc != SCARD.S_SUCCESS)
				return false;
			
			return true;
		}

		public bool Disconnect()
		{
			return DisconnectReset();
		}

		/**m* SCardChannel/DisconnectEject
		 *
		 * NAME
		 *   SCardChannel.DisconnectEject()
		 *
		 * SYNOPSIS
		 *   bool DisconnectEject()
		 * 
		 * DESCRIPTION
		 *   Same as SCardChannel.Disconnect(SCARD.EJECT_CARD)
		 *
		 **/

		public bool DisconnectEject()
		{
			return Disconnect(SCARD.EJECT_CARD);
		}

		/**m* SCardChannel/DisconnectUnpower
		 *
		 * NAME
		 *   SCardChannel.DisconnectUnpower()
		 *
		 * SYNOPSIS
		 *   bool DisconnectUnpower()
		 * 
		 * DESCRIPTION
		 *   Same as SCardChannel.Disconnect(SCARD.UNPOWER_CARD)
		 *
		 **/

		public bool DisconnectUnpower()
		{
			return Disconnect(SCARD.UNPOWER_CARD);
		}

		/**m* SCardChannel/DisconnectReset
		 *
		 * NAME
		 *   SCardChannel.DisconnectReset()
		 *
		 * SYNOPSIS
		 *   bool DisconnectReset()
		 * 
		 * DESCRIPTION
		 *   Same as SCardChannel.Disconnect(SCARD.RESET_CARD)
		 *
		 **/

		public bool DisconnectReset()
		{
			return Disconnect(SCARD.RESET_CARD);
		}

		/**m* SCardChannel/DisconnectLeave
		 *
		 * NAME
		 *   SCardChannel.DisconnectLeave()
		 *
		 * SYNOPSIS
		 *   void DisconnectLeave()
		 * 
		 * DESCRIPTION
		 *   Same as SCardChannel.Disconnect(SCARD.LEAVE_CARD)
		 *
		 **/

		public bool DisconnectLeave()
		{
			return Disconnect(SCARD.LEAVE_CARD);
		}

		/**m* SCardChannel/Reconnect
		 *
		 * NAME
		 *   SCardChannel.Reconnect()
		 *
		 * SYNOPSIS
		 *   bool Reconnect()
		 *   bool Reconnect(uint disposition)
		 * 
		 * DESCRIPTION
		 *   Re-open the connection channel to the smartcard
		 *
		 * INPUTS
		 *   The disposition parameter must take one of the following values:
		 *   - SCARD.EJECT_CARD
		 *   - SCARD.UNPOWER_CARD
		 *   - SCARD.RESET_CARD
		 *   - SCARD.LEAVE_CARD
		 *   If this parameter is omitted, it defaults to SCARD.RESET_CARD
		 *
		 * OUTPUT
		 *   Returns true if the connection has been successfully re-established.
		 *   Returns false if not. See LastError for details.
		 * 
		 * SEE ALSO
		 *   SCardChannel.Connect
		 *   SCardChannel.Disconnect
		 *
		 **/

		public virtual bool Reconnect(uint disposition)
		{
			uint rc;

			if (!Connected)
				return false;

			rc =
				SCARD.Reconnect(_hCard, _share_mode, _want_protocols, disposition, ref _active_protocol);
			if (rc != SCARD.S_SUCCESS)
			{
				_hCard = IntPtr.Zero;
				_last_error = rc;
				return false;
			}

			UpdateState();
			return true;
		}

		public void Reconnect()
		{
			ReconnectReset();
		}

		/**m* SCardChannel/ReconnectEject
		 *
		 * NAME
		 *   SCardChannel.ReconnectEject()
		 *
		 * SYNOPSIS
		 *   void ReconnectEject()
		 * 
		 * DESCRIPTION
		 *   Same as SCardChannel.Reconnect(SCARD.EJECT_CARD)
		 *
		 **/

		public void ReconnectEject()
		{
			Reconnect(SCARD.EJECT_CARD);
		}

		/**m* SCardChannel/ReconnectUnpower
		 *
		 * NAME
		 *   SCardChannel.ReconnectUnpower()
		 *
		 * SYNOPSIS
		 *   void ReconnectUnpower()
		 * 
		 * DESCRIPTION
		 *   Same as SCardChannel.Reconnect(SCARD.UNPOWER_CARD)
		 *
		 **/

		public void ReconnectUnpower()
		{
			Reconnect(SCARD.UNPOWER_CARD);
		}

		/**m* SCardChannel/ReconnectReset
		 *
		 * NAME
		 *   SCardChannel.ReconnectReset()
		 *
		 * SYNOPSIS
		 *   void ReconnectReset()
		 * 
		 * DESCRIPTION
		 *   Same as SCardChannel.Disconnect(SCARD.RESET_CARD)
		 *
		 **/

		public void ReconnectReset()
		{
			Reconnect(SCARD.RESET_CARD);
		}

		/**m* SCardChannel/ReconnectLeave
		 *
		 * NAME
		 *   SCardChannel.ReconnectLeave()
		 *
		 * SYNOPSIS
		 *   void ReconnectLeave()
		 * 
		 * DESCRIPTION
		 *   Same as SCardChannel.Reconnect(SCARD.LEAVE_CARD)
		 *
		 **/

		public void ReconnectLeave()
		{
			Reconnect(SCARD.LEAVE_CARD);
		}
		
		/**m* SCardChannel/Transmit
		 *
		 * NAME
		 *   SCardChannel.Transmit()
		 *
		 * SYNOPSIS
		 *   bool  Transmit()
		 *   RAPDU Transmit(CAPDU capdu)
		 *   bool  Transmit(CAPDU capdu, out RAPDU rapdu)
		 *   void  Transmit(CAPDU capdu, TransmitDoneCallback callback)
		 * 
		 * DESCRIPTION
		 *   Sends a command APDU (CAPDU) to the connected card, and retrieves its response APDU (RAPDU)
		 *
		 * SOURCE
		 *
		 *   SCardChannel card = new SCardChannel( ... reader ... );
		 *   if (!card.Connect( SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1 ))
		 *   {
		 *     // handle error
		 *   }
		 *
		 *
		 *   // Example 1
		 *   // ---------
		 *
		 *   card.Command = new CAPDU("00 A4 00 00 02 3F 00");
		 *   if (!card.Transmit())
		 *   {
		 *     // handle error
		 *   }
		 *   MessageBox.Show("Card answered: " + card.Response.AsString(" "));
		 *
		 *
		 *   // Example 2
		 *   // ---------
		 *
		 *   RAPDU response = card.Transmit(new CAPDU("00 A4 00 00 02 3F 00")))
		 *   if (response == null)
		 *   {
		 *     // handle error
		 *   }
		 *   MessageBox.Show("Card answered: " + response.AsString(" "));
		 *
		 *
		 *   // Example 3
		 *   // ---------
		 *
		 *   CAPDU command  = new CAPDU("00 A4 00 00 02 3F 00");
		 *   RAPDU response = new RAPDU();
		 *   if (!card.Transmit(command, out response))
		 *   {
		 *     // handle error
		 *   }
		 *   MessageBox.Show("Card answered: " + response.AsString(" "));
		 *
		 *
		 *   // Example 4
		 *   // ---------
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
		 *
		 * 
		 * SEE ALSO
		 *   SCardChannel.Connect
		 *   SCardChannel.Transmit
		 *   SCardChannel.Command
		 *   SCardChannel.Response
		 *
		 **/
		#region Transmit
		public virtual bool Transmit()
		{
			uint rsp_length = 32*1024;
			byte[]rsp_buffer = new byte[rsp_length];
			uint rc;
			IntPtr SendPci = IntPtr.Zero;
			
			switch (_active_protocol)
			{
				case SCARD.PROTOCOL_T0 :
					SendPci = SCARD.PCI_T0();
					break;
				case SCARD.PROTOCOL_T1 :
					SendPci = SCARD.PCI_T1();
					break;
				case SCARD.PROTOCOL_RAW :
					SendPci = SCARD.PCI_RAW();
					break;
					default :
						break;
			}

			_rapdu = null;
			
			Trace.WriteLine("Transmit << " + _capdu.AsString());

			rc = SCARD.Transmit(_hCard,
			                    SendPci,
			                    _capdu.GetBytes(),
			                    (uint) _capdu.Length,
			                    IntPtr.Zero, /* RecvPci is likely to remain NULL */
			                    rsp_buffer,
			                    ref rsp_length);

			if (rc != SCARD.S_SUCCESS)
			{
				Trace.WriteLine("Transmit : " + rc);
				_last_error = rc;
				return false;
			}

			_rapdu = new RAPDU(rsp_buffer, (int) rsp_length);
			
			Trace.WriteLine("Transmit >> " + _rapdu.AsString());
			
			return true;
		}

		public bool Transmit(CAPDU capdu, out RAPDU rapdu)
		{
			rapdu = null;
			_capdu = capdu;

			if (!Transmit())
				return false;

			rapdu = _rapdu;
			return true;
		}

		public RAPDU Transmit(CAPDU capdu)
		{
			_capdu = capdu;
			
			if (!Transmit())
				return null;

			return _rapdu;
		}

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
			} else
			{
				if (_transmit_done_callback != null)
					_transmit_done_callback(null);
			}
		}
		

		/**v* SCardChannel/Command
		 *
		 * NAME
		 *   SCardChannel.Command
		 *
		 * SYNOPSIS
		 *   CAPDU Command
		 * 
		 * DESCRIPTION
		 *   C-APDU to be sent to the card through SCardChannel.Transmit
		 *
		 **/
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

		/**v* SCardChannel/Response
		 *
		 * NAME
		 *   SCardChannel.Response
		 *
		 * SYNOPSIS
		 *   RAPDU Response
		 * 
		 * DESCRIPTION
		 *   R-APDU returned by the card after a succesfull call to SCardChannel.Transmit
		 *
		 **/
		public RAPDU Response
		{
			get
			{
				return _rapdu;
			}
		}
		#endregion

		#region Control
		public virtual byte[] Control(byte[] cctrl)
		{
			byte[] rctrl = new byte[280];
			uint rl = 0;
			uint rc;
			
			Trace.WriteLine("Control << " + (new CardBuffer(cctrl)).AsString());

			rc = SCARD.Control(_hCard,
			                   SCARD.IOCTL_CSB6_PCSC_ESCAPE,
			                   cctrl,
			                   (uint) cctrl.Length,
			                   rctrl,
			                   280,
			                   ref rl);

			if (rc == 1)
			{
				rc = SCARD.Control(_hCard,
				                   SCARD.IOCTL_MS_CCID_ESCAPE,
				                   cctrl,
				                   (uint) cctrl.Length,
				                   rctrl,
				                   280,
				                   ref rl);

			}
			
			if (rc != SCARD.S_SUCCESS)
			{
				Trace.WriteLine("Control: " + rc);
				_last_error = rc;
				rctrl = null;
				return null;
			}
			
			byte[] r = new byte[rl];
			for (int i=0; i<rl; i++)
				r[i] = rctrl[i];
			
			Trace.WriteLine("Control >> " + (new CardBuffer(r)).AsString());

			return r;
		}

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
		
		public bool Control(CardBuffer cctrl, out CardBuffer rctrl)
		{
			rctrl = null;
			_cctrl = cctrl;

			if (!Control())
				return false;
			
			rctrl = _rctrl;
			return true;
		}

		public CardBuffer Control(CardBuffer cctrl)
		{
			_cctrl = cctrl;

			if (!Control())
				return null;

			return _rctrl;
		}

		public bool Control(CAPDU cctrl, out RAPDU rctrl)
		{
			rctrl = null;
			_cctrl = cctrl;

			if (!Control())
				return false;
			
			rctrl = new RAPDU(_rctrl);
			return true;
		}

		public RAPDU Control(CAPDU cctrl)
		{
			_cctrl = cctrl;

			if (!Control())
				return null;

			return new RAPDU(_rctrl);
		}
		
		public bool Leds(byte red, byte green, byte blue)
		{
			byte[]buffer = new byte[5];

			buffer[0] = 0x58;
			buffer[1] = 0x1E;
			buffer[2] = red;
			buffer[3] = green;
			buffer[4] = blue;

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}
		
		public bool LedsDefault()
		{
			byte[]buffer = new byte[2];

			buffer[0] = 0x58;
			buffer[1] = 0x1E;

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}

		public bool Buzzer(ushort duration_ms)
		{
			byte[]buffer = new byte[4];

			buffer[0] = 0x58;
			buffer[1] = 0x1C;
			buffer[2] = (byte) (duration_ms / 0x0100);
			buffer[3] = (byte) (duration_ms % 0x0100);

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}
		
		public bool BuzzerDefault()
		{
			byte[]buffer = new byte[2];

			buffer[0] = 0x58;
			buffer[1] = 0x1C;

			if (Control(new CardBuffer(buffer)) != null)
				return true;

			return false;
		}
		#endregion


		/**v* SCardChannel/LastError
		 *
		 * NAME
		 *   SCardChannel.LastError
		 *
		 * SYNOPSIS
		 *   uint LastError
		 * 
		 * OUTPUT
		 *   Returns the last error encountered by the object when working with SCARD functions.
		 *
		 * SEE ALSO
		 *   SCardChannel.LastErrorAsString
		 *
		 **/
		public uint LastError
		{
			get
			{
				return _last_error;
			}
		}

		/**v* SCardChannel/LastErrorAsString
		 *
		 * NAME
		 *   SCardChannel.LastErrorAsString
		 *
		 * SYNOPSIS
		 *   string LastErrorAsString
		 * 
		 * OUTPUT
		 *   Returns the last error encountered by the object when working with SCARD functions.
		 *
		 * SEE ALSO
		 *   SCardChannel.LastError
		 *
		 **/
		public string LastErrorAsString
		{
			get
			{
				return SCARD.ErrorToString(_last_error);
			}
		}

		public string SerialNumber()
		{
			/* Serial number */
			CardBuffer r = Control(new CardBuffer("582003"));
			if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
			{
				string s = new String(r.GetChars(1, -1));
				return s;
			}
			return "";
		}
		
		public void Wink()
		{
			Control(new CardBuffer("581C0800"));
			Control(new CardBuffer("581E010100000800"));
		}
	}
}