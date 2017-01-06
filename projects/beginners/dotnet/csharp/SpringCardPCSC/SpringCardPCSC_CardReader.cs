/**h* SpringCard/PCSC_SCardReader
 *
 * NAME
 *   PCSC : SCardReader
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
	/**c* SpringCardPCSC/SCardReader
	 *
	 * NAME
	 *   SCardReader
	 *
	 * DESCRIPTION
	 *   The SCardReader object is used to monitor a PC/SC reader (i.e. wait for card events)
	 *
	 * SYNOPSIS
	 *   SCardReader( string reader_name );
	 *
	 **/

	public class SCardReader
	{
		protected uint _last_error;
		uint _scope = SCARD.SCOPE_SYSTEM;
		protected string _reader_name;
		uint _reader_state = SCARD.STATE_UNAWARE;
		CardBuffer _card_atr = null;
		protected StatusChangeCallback _status_change_callback = null;
		Thread _status_change_thread = null;
		protected volatile bool _status_change_running = false;

		public SCardReader(uint Scope, string ReaderName)
		{
			_scope = Scope;
			_reader_name = ReaderName;
		}

		public SCardReader(string ReaderName)
		{
			_reader_name = ReaderName;
		}
		
		protected SCardReader()
		{
			
		}

		~SCardReader()
		{
			Release();
		}
		
		public void Release()
		{
			StopMonitor();
		}
		
		public virtual SCardChannel GetChannel()
		{
			return new SCardChannel(this);
		}
		
		public uint Scope {
			get {
				return _scope;
			}
		}

		/**v* SCardReader/Name
		 *
		 * NAME
		 *   SCardReader.Name
		 *
		 * SYNOPSIS
		 *   string Name
		 *
		 * OUTPUT
		 *   Return the name of the reader specified when instanciating the object.
		 *
		 **/

		public string Name {
			get {
				return _reader_name;
			}
		}

		/**t* SCardReader/StatusChangeCallback
		 *
		 * NAME
		 *   SCardReader.StatusChangeCallback
		 *
		 * SYNOPSIS
		 *   delegate void StatusChangeCallback(uint ReaderState, CardBuffer CardAtr);
		 *
		 * DESCRIPTION
		 *   Typedef for the callback that will be called by the background thread launched
		 *   by SCardReader.StartMonitor(), everytime the status of the reader is changed.
		 *
		 * NOTES
		 *   The callback is invoked in the context of a background thread. This implies that
		 *   it is not allowed to access the GUI's components directly.
		 *
		 **/
		public delegate void StatusChangeCallback(uint ReaderState,
		                                          CardBuffer CardAtr);

		/**m* SCardReader/StartMonitor
		 *
		 * NAME
		 *   SCardReader.StartMonitor()
		 *
		 * SYNOPSIS
		 *   SCardReader.StartMonitor(SCardReader.StatusChangeCallback callback);
		 *
		 * DESCRIPTION
		 *   Create a background thread to monitor the reader associated to the object.
		 *   Everytime the status of the reader is changed, the callback is invoked.
		 *
		 * SEE ALSO
		 *   SCardReader.StatusChangeCallback
		 *   SCardReader.StopMonitor()
		 *
		 **/

		public void StartMonitor(StatusChangeCallback callback)
		{
			StopMonitor();

			if (callback != null) {
				_status_change_callback = callback;
				_status_change_thread = new Thread(StatusChangeMonitor);
				_status_change_running = true;
				_status_change_thread.Start();
			}
		}

		/**m* SCardReader/StopMonitor
		 *
		 * NAME
		 *   SCardReader.StopMonitor()
		 *
		 * DESCRIPTION
		 *   Stop the background thread previously launched by SCardReader.StartMonitor().
		 *
		 **/

		public void StopMonitor()
		{
			_status_change_callback = null;
			_status_change_running = false;

			if (_status_change_thread != null) {
				_status_change_thread.Interrupt();
				_status_change_thread.Join();
				_status_change_thread = null;
			}
		}

		protected virtual void StatusChangeMonitor()
		{
			uint rc;

			IntPtr hContext = IntPtr.Zero;

			_reader_state = SCARD.STATE_UNAWARE;
			_card_atr = null;

			rc =
				SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
			if (rc != SCARD.S_SUCCESS)
				return;

			SCARD.READERSTATE[] states = new SCARD.READERSTATE[1];

			states[0] = new SCARD.READERSTATE();
			states[0].szReader = _reader_name;
			states[0].pvUserData = IntPtr.Zero;
			states[0].dwCurrentState = 0;
			states[0].dwEventState = 0;
			states[0].cbAtr = 0;
			states[0].rgbAtr = null;

			while (_status_change_running) {
				try {
					rc = SCARD.GetStatusChange(hContext, 1000, states, 1);
				} catch (ThreadInterruptedException) {
					break;
				}

				if (!_status_change_running)
					break;

				if (rc == SCARD.E_TIMEOUT)
					continue;

				if (rc != SCARD.S_SUCCESS) {
					_last_error = rc;

					SCARD.ReleaseContext(hContext);
					if (_status_change_callback != null)
						_status_change_callback(0, null);
					break;
				}

				if ((states[0].dwEventState & SCARD.STATE_CHANGED) != 0) {
					states[0].dwCurrentState = states[0].dwEventState;
					
					if (_status_change_callback != null) {
						CardBuffer card_atr = null;

						if ((states[0].dwEventState & SCARD.STATE_PRESENT) != 0)
							card_atr =
								new CardBuffer(states[0].rgbAtr, (int)states[0].cbAtr);

						_status_change_callback(states[0].dwEventState & ~SCARD.
						                        STATE_CHANGED, card_atr);
					}
				}
			}

			SCARD.ReleaseContext(hContext);
		}

		private void UpdateState()
		{
			uint rc;

			IntPtr hContext = IntPtr.Zero;

			_reader_state = SCARD.STATE_UNAWARE;
			_card_atr = null;

			rc =
				SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
			if (rc != SCARD.S_SUCCESS) {
				_last_error = rc;
				return;
			}

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
				rc = SCARD.GetStatusChange(hContext, 0, states, 1);
			}
			catch (ThreadInterruptedException)
			{
				rc = SCARD.E_CANCELLED;
			}
			
			if (rc != SCARD.S_SUCCESS) {
				SCARD.ReleaseContext(hContext);
				return;
			}

			SCARD.ReleaseContext(hContext);

			_reader_state = states[0].dwEventState;

			if ((_reader_state & SCARD.STATE_PRESENT) != 0) {
				_card_atr = new CardBuffer(states[0].rgbAtr, (int)states[0].cbAtr);
			}
		}

		/**v* SCardReader/Status
		 *
		 * NAME
		 *   SCardReader.Status
		 * 
		 * SYNOPSIS
		 *   uint Status
		 *
		 * OUTPUT
		 *   Returns the current status of the reader.
		 *
		 * SEE ALSO
		 *   SCardReader.CardPresent
		 *   SCardReader.StatusAsString
		 *
		 **/

		public uint Status {
			get {
				UpdateState();
				return _reader_state;
			}
		}

		/**v* SCardReader/StatusAsString
		 *
		 * NAME
		 *   SCardReader.StatusAsString
		 * 
		 * SYNOPSIS
		 *   string StatusAsString
		 *
		 * OUTPUT
		 *   Returns the current status of the reader, using SCARD.ReaderStatusToString as formatter.
		 *
		 * SEE ALSO
		 *   SCardReader.Status
		 *
		 **/

		public string StatusAsString {
			get {
				UpdateState();
				return SCARD.ReaderStatusToString(_reader_state);
			}
		}

		/**v* SCardReader/CardPresent
		 *
		 * NAME
		 *   SCardReader.CardPresent
		 * 
		 * SYNOPSIS
		 *   bool CardPresent
		 *
		 * OUTPUT
		 *   Returns true if a card is present in the reader.
		 *   Returns false if there's no smartcard in the reader.
		 *
		 * SEE ALSO
		 *   SCardReader.CardAtr
		 *   SCardReader.CardAvailable
		 *   SCardReader.Status
		 *
		 **/

		public bool CardPresent {
			get {
				UpdateState();
				if ((_reader_state & SCARD.STATE_PRESENT) != 0)
					return true;
				return false;
			}
		}

		/**v* SCardReader/CardAvailable
		 *
		 * NAME
		 *   SCardReader.CardAvailable
		 *
		 * SYNOPSIS
		 *   bool CardAvailable
		 *
		 * OUTPUT
		 *   Returns true if a card is available in the reader.
		 *   Returns false if there's no smartcard in the reader, or if it is already used by another process/thread.
		 * 
		 * SEE ALSO
		 *   SCardReader.CardAtr
		 *   SCardReader.CardPresent
		 *   SCardReader.Status
		 *
		 **/

		public bool CardAvailable {
			get {
				UpdateState();
				if (((_reader_state & SCARD.STATE_PRESENT) != 0)
				    && ((_reader_state & SCARD.STATE_MUTE) == 0)
				    && ((_reader_state & SCARD.STATE_INUSE) == 0))
					return true;
				return false;
			}
		}

		/**v* SCardReader/CardAtr
		 *
		 * NAME
		 *   SCardReader.CardAtr
		 *
		 * SYNOPSIS
		 *   CardBuffer CardAtr
		 *
		 * OUTPUT
		 *   If a smartcard is present in the reader (SCardReader.CardPresent == true), returns the ATR of the card.
		 *   Returns null overwise.
		 * 
		 * SEE ALSO
		 *   SCardReader.CardPresent
		 *   SCardReader.Status
		 *
		 **/

		public CardBuffer CardAtr {
			get {
				UpdateState();
				return _card_atr;
			}
		}

		/**v* SCardReader/LastError
		 *
		 * NAME
		 *   uint SCardReader.LastError
		 * 
		 * OUTPUT
		 *   Returns the last error encountered by the object when working with SCARD functions.
		 *
		 * SEE ALSO
		 *   SCardReader.LastErrorAsString
		 *
		 **/
		public uint LastError {
			get {
				return _last_error;
			}
		}

		/**v* SCardReader/LastErrorAsString
		 *
		 * NAME
		 *   string SCardReader.LastErrorAsString
		 * 
		 * OUTPUT
		 *   Returns the last error encountered by the object when working with SCARD functions.
		 *
		 * SEE ALSO
		 *   SCardReader.LastError
		 *
		 **/
		public string LastErrorAsString {
			get {
				return SCARD.ErrorToString(_last_error);
			}
		}

	}
}