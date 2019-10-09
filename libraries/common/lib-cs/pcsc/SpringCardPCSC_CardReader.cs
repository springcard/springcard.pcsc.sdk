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
using System.Threading;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	/**
	 *
	 * \brief The SCardReader object is used to monitor one PC/SC reader (i.e. wait for card events)
	 *
	 **/
	public class SCardReader
	{
		protected uint _last_error;
		uint _scope = SCARD.SCOPE_SYSTEM;
		protected string readerName;
		uint readerState = SCARD.STATE_UNAWARE;
		CardBuffer cardAtr = null;

        protected CardConnectedCallback onCardConnected = null;
        protected CardRemovedCallback onCardRemoved = null;
        protected ReaderErrorCallback onCardWaitReaderError = null;
        Thread waitCardThread = null;
        protected volatile bool waitCardRunning = false;

        protected StatusChangeCallback onReaderStateChange = null;
        protected ReaderErrorCallback onReaderStateError = null;
        Thread statusMonitorThread = null;
		protected volatile bool statusMonitorRunning = false;

		/**
		 * \brief Create a SCardReader for the given PC/SC scope and reader name
		 */
		public SCardReader(uint Scope, string ReaderName)
		{
			_scope = Scope;
			readerName = ReaderName;
            if (SCARD.UseLogger)
                Logger.Debug("{0}:new SCardReader", readerName);
        }

		/**
		 * \brief Create a SCardReader for the given and reader name
		 */
		public SCardReader(string ReaderName)
		{
			readerName = ReaderName;
            if (SCARD.UseLogger)
                Logger.Debug("{0}:new SCardReader", readerName);
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

		/**
		 * \brief Create a SCardChannel object from this reader
		 */
		public virtual SCardChannel CreateChannel()
		{
			return new SCardChannel(this);
		}

		/**
		 * \brief Create a SCardChannel object from this reader
		 *
		 * \deprecated Use CreateChannel() instead
		 */
		public SCardChannel GetChannel()
		{
			return CreateChannel();
		}

		/**
		 * \brief The PC/SC context handle. See SCARD.CreateContext()
		 */
		public uint Scope
		{
			get
			{
				return _scope;
			}
		}

		/**
		 * \brief The name of the reader
		 */
		public string Name
		{
			get
			{
				return readerName;
			}
		}

        /**
		 *
		 * \brief Definition of the callback that will be called by the background thread launched by StartWaitCard() when a reader error occurs.
		 *
		 * \warning The callback is invoked in the context of a background thread. This implies that it is not allowed to access the GUI's components directly.
		 *
		 **/
        public delegate void ReaderErrorCallback();

        /**
		 *
		 * \brief Definition of the callback that will be called by the background thread launched by StartWaitCard() when a card is ready (present, connected).
		 *
		 * \warning The callback is invoked in the context of a background thread. This implies that it is not allowed to access the GUI's components directly.
		 *
		 **/
        public delegate void CardConnectedCallback(SCardChannel cardChannel);

        /**
		 *
		 * \brief Definition of the callback that will be called by the background thread launched by StartWaitCard() when a card is removed.
		 *
		 * \warning The callback is invoked in the context of a background thread. This implies that it is not allowed to access the GUI's components directly.
		 *
		 **/
        public delegate void CardRemovedCallback();

        /**
		 * \brief Create a background thread to monitor the reader associated to the object, and to connect to a card as soon as there is one available.
		 **/
        public void StartWaitCard(CardConnectedCallback onCardConnected)
        {
            StartWaitCard(onCardConnected, null, null);
        }

        /**
		 * \brief Create a background thread to monitor the reader associated to the object, and to connect to a card as soon as there is one available.
		 **/
        public void StartWaitCard(CardConnectedCallback onCardConnected, CardRemovedCallback onCardRemoved)
        {
            StartWaitCard(onCardConnected, onCardRemoved, null);
        }

        /**
		 * \brief Create a background thread to monitor the reader associated to the object, and to connect to a card as soon as there is one available.
		 **/
        public void StartWaitCard(CardConnectedCallback onCardConnected, CardRemovedCallback onCardRemoved, ReaderErrorCallback onReaderError)
        {
            this.onCardConnected = onCardConnected;
            this.onCardRemoved = onCardRemoved;
            this.onCardWaitReaderError = onReaderError;

            if (onCardConnected != null)
            {
                waitCardRunning = true;
                waitCardThread = new Thread(WaitCardProc);
                waitCardThread.Start();
            }
        }

        /**
		 * \brief Stop the background thread previously launched by StartWaitCard().
		 **/
        public void StopWaitCard()
        {
            onCardConnected = null;
            onCardRemoved = null;
            waitCardRunning = false;

            if (waitCardThread != null)
            {
                waitCardThread.Interrupt();
                waitCardThread.Join();
                waitCardThread = null;
            }
        }

        protected virtual void WaitCardProc()
        {
            uint rc;
            IntPtr hContext = IntPtr.Zero;
            bool bWaitingForRemoval = false;

            rc = SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
            if (rc != SCARD.S_SUCCESS)
            {
                if (SCARD.UseLogger)
                    Logger.Warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
                return;
            }

            SCARD.READERSTATE[] states = new SCARD.READERSTATE[1];

            states[0] = new SCARD.READERSTATE();
            states[0].szReader = readerName;
            states[0].pvUserData = IntPtr.Zero;
            states[0].dwCurrentState = 0;
            states[0].dwEventState = 0;
            states[0].cbAtr = 0;
            states[0].rgbAtr = null;

            while (waitCardRunning)
            {
                try
                {
                    rc = SCARD.GetStatusChange(hContext, 1000, states, 1);
                }
                catch (ThreadInterruptedException)
                {
                    rc = SCARD.E_CANCELLED;
                    break;
                }

                if (!waitCardRunning)
                    break;

                if (rc == SCARD.E_TIMEOUT)
                    continue;

                if (rc != SCARD.S_SUCCESS)
                {
                    if (SCARD.UseLogger)
                        Logger.Warning("SCardGetStatusChange failed with error " + SCARD.ErrorToMessage(rc));

                    SCARD.ReleaseContext(hContext);

                    if (onCardWaitReaderError != null)
                        onCardWaitReaderError();
                    break;
                }

                if ((states[0].dwEventState & SCARD.STATE_CHANGED) != 0)
                {
                    states[0].dwCurrentState = states[0].dwEventState;

                    if (((states[0].dwCurrentState & SCARD.STATE_PRESENT) != 0) && ((states[0].dwCurrentState & SCARD.STATE_MUTE) == 0))
                    {
                        /* A card is present */
                        if ((states[0].dwCurrentState & SCARD.STATE_INUSE) == 0)
                        {
                            /* The card is not in use */
                            SCardChannel cardChannel = new SCardChannel(readerName);
                            if (cardChannel.ConnectExclusive())
                            {
                                /* I'm connected to the card! - Let's perform a transaction */
                                if (onCardConnected != null)
                                    onCardConnected(cardChannel);

                                /* Disconnect from the card */
                                if (cardChannel.Connected)
                                    cardChannel.Disconnect(SCARD.RESET_CARD);

                                /* Now wait for removal */
                                bWaitingForRemoval = true;
                            }
                        }
                    }
                    else
                    {
                        /* No card present */
                        if (bWaitingForRemoval)
                        {
                            if (onCardRemoved != null)
                                onCardRemoved();

                            bWaitingForRemoval = false;
                        }
                    }
                }
            }

            SCARD.ReleaseContext(hContext);
        }

        /**
		 *
		 * \brief Definition of the callback that will be called by the background thread launched by StartMonitor() everytime the status of the reader is changed.
		 *
		 * \warning The callback is invoked in the context of a background thread. This implies that it is not allowed to access the GUI's components directly.
		 *
		 **/
        public delegate void StatusChangeCallback(uint ReaderState, CardBuffer CardAtr);

        /**
		 * \brief Create a background thread to monitor the reader associated to the object. Everytime the status of the reader is changed, the callback is invoked.
		 **/
        public void StartMonitor(StatusChangeCallback onReaderStateChange)
        {
            StartMonitor(onReaderStateChange, null);
        }

		/**
		 * \brief Create a background thread to monitor the reader associated to the object. Everytime the status of the reader is changed, the callback is invoked.
		 **/
		public void StartMonitor(StatusChangeCallback onReaderStateChange, ReaderErrorCallback onReaderError)
		{
			StopMonitor();

			this.onReaderStateChange = onReaderStateChange;
            this.onReaderStateError = onReaderError;

            if (onReaderStateChange != null)
            {
                statusMonitorRunning = true;
                statusMonitorThread = new Thread(MonitorProc);
				statusMonitorThread.Start();
			}
		}

		/**
		 * \brief Stop the background thread previously launched by StartMonitor().
		 **/
		public void StopMonitor()
		{
			onReaderStateChange = null;
			statusMonitorRunning = false;

			if (statusMonitorThread != null)
			{
				statusMonitorThread.Interrupt();
				statusMonitorThread.Join();
				statusMonitorThread = null;
			}
		}

		protected virtual void MonitorProc()
		{
			uint rc;

			IntPtr hContext = IntPtr.Zero;

			readerState = SCARD.STATE_UNAWARE;
			cardAtr = null;

			rc = SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
			if (rc != SCARD.S_SUCCESS)
			{
				if (SCARD.UseLogger)
					Logger.Warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
				return;
			}

			SCARD.READERSTATE[] states = new SCARD.READERSTATE[1];

			states[0] = new SCARD.READERSTATE();
			states[0].szReader = readerName;
			states[0].pvUserData = IntPtr.Zero;
			states[0].dwCurrentState = 0;
			states[0].dwEventState = 0;
			states[0].cbAtr = 0;
			states[0].rgbAtr = null;

			while (statusMonitorRunning)
			{
				try
				{
					rc = SCARD.GetStatusChange(hContext, 1000, states, 1);
				}
				catch (ThreadInterruptedException)
				{
					rc = SCARD.E_CANCELLED;
					break;
				}

				if (!statusMonitorRunning)
					break;

				if (rc == SCARD.E_TIMEOUT)
					continue;

				if (rc != SCARD.S_SUCCESS)
				{
					_last_error = rc;

					SCARD.ReleaseContext(hContext);

					if (onReaderStateChange != null)
						onReaderStateChange(0, null);

                    if (onReaderStateError != null)
                        onReaderStateError();

                    break;
				}

				if ((states[0].dwEventState & SCARD.STATE_CHANGED) != 0)
				{
					states[0].dwCurrentState = states[0].dwEventState;

					if (onReaderStateChange != null)
					{
						CardBuffer abCardAtr = null;

						if ((states[0].dwEventState & SCARD.STATE_PRESENT) != 0)
							abCardAtr = new CardBuffer(states[0].rgbAtr, (int)states[0].cbAtr);

						onReaderStateChange(states[0].dwEventState & ~SCARD.STATE_CHANGED, abCardAtr);
					}
				}
			}

			SCARD.ReleaseContext(hContext);
		}

		private void UpdateState()
		{
			uint rc;

            if (SCARD.UseLogger)
                Logger.Debug("{0}:UpdateState", readerName);

            IntPtr hContext = IntPtr.Zero;

			readerState = SCARD.STATE_UNAWARE;
			cardAtr = null;

			rc = SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
			if (rc != SCARD.S_SUCCESS)
			{
				if (SCARD.UseLogger)
					Logger.Warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
				_last_error = rc;
				return;
			}

			SCARD.READERSTATE[] states = new SCARD.READERSTATE[1];

			states[0] = new SCARD.READERSTATE();
			states[0].szReader = readerName;
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
                if (SCARD.UseLogger)
                    Logger.Debug("SCardGetStatusChange has been interrupted " + SCARD.ErrorToMessage(rc));
                rc = SCARD.E_CANCELLED;
			}

			if (rc != SCARD.S_SUCCESS)
			{
                if (SCARD.UseLogger)
                    Logger.Warning("SCardGetStatusChange failed with error " + SCARD.ErrorToMessage(rc));
                SCARD.ReleaseContext(hContext);
				return;
			}

			SCARD.ReleaseContext(hContext);

			readerState = states[0].dwEventState;
            if (SCARD.UseLogger)
                Logger.Debug("{0}:ReaderState={1:X08}", readerName, readerState);

            if ((readerState & SCARD.STATE_PRESENT) != 0)
			{
                cardAtr = new CardBuffer(states[0].rgbAtr, (int)states[0].cbAtr);
                if (SCARD.UseLogger)
                    Logger.Debug("{0}:CardAtr={1}", readerName, BinConvert.ToHex(cardAtr.Bytes));
            }
		}

		/**
		 * \brief Current status of the reader
		 **/
		public uint Status
		{
			get
			{
				UpdateState();
				return readerState;
			}
		}

		/**
		 * \brief Current status of the reader, using SCARD.ReaderStatusToString() as formatter
		 **/
		public string StatusAsString
		{
			get
			{
				UpdateState();
				return SCARD.ReaderStatusToString(readerState & ~SCARD.STATE_CHANGED);
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
				if ((readerState & SCARD.STATE_PRESENT) != 0)
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
				if (((readerState & SCARD.STATE_PRESENT) != 0)
					&& ((readerState & SCARD.STATE_MUTE) == 0)
					&& ((readerState & SCARD.STATE_INUSE) == 0))
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
				UpdateState();
				return cardAtr;
			}
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control() and retrieve its answer.
		 */
		public bool Control(CardBuffer cctrl, out CardBuffer rctrl)
		{
			rctrl = null;
			SCardChannel channel = new SCardChannel(this);
			if (!channel.ConnectDirect())
				return false;
			bool rc = channel.Control(cctrl, out rctrl);
			channel.DisconnectLeave();
			return rc;
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control() and retrieve its answer.
		 */
		public CardBuffer Control(CardBuffer cctrl)
		{
			CardBuffer _rctrl;

			if (!Control(cctrl, out _rctrl))
				return null;

			return _rctrl;
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control() and retrieve its answer.
		 */
		public bool Control(CAPDU cctrl, out RAPDU rctrl)
		{
			rctrl = null;
			CardBuffer _rctrl;

			if (!Control(cctrl, out _rctrl))
				return false;

			rctrl = new RAPDU(_rctrl);
			return true;
		}

		/**
		 * \brief Send a direct command the reader through SCARD.Control() and retrieve its answer.
		 */
		public RAPDU Control(CAPDU cctrl)
		{
			CardBuffer _rctrl;

			if (!Control(cctrl, out _rctrl))
				return null;

			return new RAPDU(_rctrl);
		}

		/**
		 * \brief Return the last error encountered by this SCardReader
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