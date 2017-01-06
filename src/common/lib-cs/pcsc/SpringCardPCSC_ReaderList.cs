/**h* SpringCard/PCSC_SCardReaderList
 *
 * NAME
 *   PCSC : SCardReaderList
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
	/**c* SpringCard.PCSC/SCardReaderList
	 *
	 * NAME
	 *   SCardReaderList
	 *
	 * DESCRIPTION
	 *   The SCardReaderList object is used to monitor a set of PC/SC readers (i.e. wait for card events)
	 *
	 * SYNOPSIS
	 *   SCardReaderList( string[] reader_names );
	 *
	 **/

	public class SCardReaderList
	{
		uint _last_error;
		uint _scope = SCARD.SCOPE_SYSTEM;
		string _groups = null;
		protected string[] _reader_names;
		bool _auto_update_list;
		StatusChangeCallback _status_change_callback = null;
		Thread _status_change_thread = null;
		IntPtr _status_change_context = IntPtr.Zero;
		volatile bool _status_change_running = false;
		
		private void InitList()
		{
			_reader_names = SCARD.GetReaderList(_scope, _groups);			
		}

		public SCardReaderList(uint Scope, string Groups)
		{
			_scope = Scope;
			_groups = Groups;			
			InitList();
			_auto_update_list = true;
		}

		public SCardReaderList()
		{
			InitList();
			_auto_update_list = true;
		}

		public SCardReaderList(string[] reader_names)
		{
			_reader_names = reader_names;
			_auto_update_list = false;
		}

		/*~SCardReaderList()
		{
			Release();
		}*/
		
		public virtual void Release()
		{			
			StopMonitor();			
		}

		/**t* SCardReaderList/StatusChangeCallback
		 *
		 * NAME
		 *   SCardReaderList.StatusChangeCallback
		 *
		 * SYNOPSIS
		 *   delegate void StatusChangeCallback(string ReaderName, uint ReaderState, CardBuffer CardAtr);
		 *
		 * DESCRIPTION
		 *   Typedef for the callback that will be called by the background thread launched
		 *   by SCardReaderList.StartMonitor(), everytime the status of one of the readers is changed.
		 *
		 * NOTES
		 *   The callback is invoked in the context of a background thread. This implies that
		 *   it is not allowed to access the GUI's components directly.
		 *
		 **/
		public delegate void StatusChangeCallback(string ReaderName,
		                                          uint ReaderState,
		                                          CardBuffer CardAtr);

		/**m* SCardReaderList/StartMonitor
		 *
		 * NAME
		 *   SCardReaderList.StartMonitor()
		 *
		 * SYNOPSIS
		 *   SCardReaderList.StartMonitor(SCardReaderList.StatusChangeCallback callback);
		 *
		 * DESCRIPTION
		 *   Create a background thread to monitor the reader associated to the object.
		 *   Everytime the status of the reader is changed, the callback is invoked.
		 *
		 * SEE ALSO
		 *   SCardReaderList.StatusChangeCallback
		 *   SCardReaderList.StopMonitor()
		 *
		 **/
		public void StartMonitor(StatusChangeCallback callback)
		{
			StopMonitor();

			if (callback != null)
			{
				_status_change_callback = callback;
				_status_change_thread = new Thread(StatusChangeMonitor);
				_status_change_running = true;
				_status_change_thread.Start();
			}
		}

		/**m* SCardReaderList/StopMonitor
		 *
		 * NAME
		 *   SCardReaderList.StopMonitor()
		 *
		 * DESCRIPTION
		 *   Stop the background thread previously launched by SCardReaderList.StartMonitor().
		 *
		 **/
		public void StopMonitor()
		{
			_status_change_callback = null;
			_status_change_running = false;

			if (_status_change_thread != null)
			{
				if (_status_change_context != IntPtr.Zero)
				{
					SCARD.Cancel(_status_change_context);
				}
				else
				{
					_status_change_thread.Interrupt();
				}
				_status_change_thread.Join();
				_status_change_thread = null;
			}
		}

		private void StatusChangeMonitor()
		{
			try
			{			
				_last_error = SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref _status_change_context);
				if (_last_error != SCARD.S_SUCCESS)
				{
					_status_change_callback(null, 0, null);
					return;
				}
				
				uint global_notification_state = SCARD.STATE_UNAWARE;
				
				while (_status_change_running)
				{
					
					if (_status_change_context == IntPtr.Zero)
					{
						_last_error = SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref _status_change_context);
						if (_last_error != SCARD.S_SUCCESS)
						{
							_status_change_callback(null, 0, null);
							return;
						}
					}
					
					/* Construct the list of readers we'll have to monitor */
					/* --------------------------------------------------- */
					bool global_notification_fired = false;
					int monitor_count = 0;
					
					if (_auto_update_list)
					{
						if (_reader_names != null)
						{
							monitor_count = _reader_names.Length;
							if (monitor_count > 10)
							{
								Trace.WriteLine("PC/SC: not able to monitor more than 10 readers (Windows limitation)");
								monitor_count = 10;
							}
						}						
						monitor_count += 1;
					} else
					{
						if (_reader_names == null)
						{
							SCARD.ReleaseContext(_status_change_context);
							_status_change_context = IntPtr.Zero;
							_status_change_callback(null, 0, null);
							return;
						}
						monitor_count = _reader_names.Length;
						if (monitor_count > 10)
						{
							Trace.WriteLine("PC/SC: not able to monitor more than 10 readers (Windows limitation)");
							monitor_count = 10;
						}
					}
					
					SCARD.READERSTATE[] states = new SCARD.READERSTATE[monitor_count];
					
					for (int i=0; i<states.Length; i++)
					{
						states[i] = new SCARD.READERSTATE();
						if (_auto_update_list && (i == 0))
						{
							/* Magic string to be notified of reader arrival/removal */
							states[i].szReader = "\\\\?PNP?\\NOTIFICATION";
							states[i].dwCurrentState = global_notification_state;
						} else
						{
							/* Reader name */
							states[i].szReader = _reader_names[i-1];
							states[i].dwCurrentState = SCARD.STATE_UNAWARE;
						}
						states[i].dwEventState = 0;
						states[i].cbAtr = 0;
						states[i].rgbAtr = null;
						states[i].pvUserData = IntPtr.Zero;
					}
					
					/* Now wait for an event */
					/* --------------------- */
					
					while (_status_change_running && !global_notification_fired)
					{
						uint rc = SCARD.GetStatusChange(_status_change_context, 250, states, (uint) states.Length);
						
						if ((rc == SCARD.E_SERVICE_STOPPED) || (rc == SCARD.E_NO_SERVICE))
						{
							Trace.WriteLine("PC/SC: no service");
							SCARD.ReleaseContext(_status_change_context);
							_status_change_context = IntPtr.Zero;
							continue;
						}
						
						if (!_status_change_running)
							break;
						
						if (rc == SCARD.E_TIMEOUT)
							continue;
						
						if (rc != SCARD.S_SUCCESS)
						{
							Trace.WriteLine("PC/SC: monitor failed with error " + rc);
							
							_last_error = rc;
							/* Broadcast a message saying we have a problem! */
							for (int i=0; i<states.Length; i++)
								states[i].dwEventState = 0|SCARD.STATE_CHANGED;
						}
						
						for (int i=0; i<states.Length; i++)
						{
							if ((states[i].dwEventState & SCARD.STATE_CHANGED) != 0)
							{
								/* This reader has fired an event */
								/* ------------------------------ */
	
								if (_auto_update_list && (i == 0))
								{
									/* Not a reader but \\\\?PNP?\\NOTIFICATION */
									/* ---------------------------------------- */
									
									Trace.WriteLine("PC/SC: the list of readers has changed");
									
									global_notification_fired = true;
									global_notification_state = states[0].dwEventState;									
									
									SCARD.ReleaseContext(_status_change_context);
									_last_error = SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref _status_change_context);
									if (_last_error != SCARD.S_SUCCESS)
									{
										_status_change_callback(null, 0, null);
										return;
									}									
									
									/* Refresh the list of readers */
									_reader_names = SCARD.GetReaderList(_status_change_context, _groups);
									
									/* Notify the application that the list of readers has changed */
									try
									{
										_status_change_callback(null, global_notification_state & ~SCARD.STATE_CHANGED, null);
									}
									catch {}
	
								} else
								{
									/* This is a reader */
									/* ---------------- */
									
									Trace.WriteLine("PC/SC: status of reader " + states[i].szReader + " has changed");
									
									states[i].dwCurrentState = states[i].dwEventState;
									if ((states[i].dwCurrentState & SCARD.STATE_IGNORE) != 0)
										states[i].dwCurrentState = SCARD.STATE_UNAVAILABLE;
									
									CardBuffer card_atr = null;
									
									/* Is there a card involved ? */
									if ((states[i].dwEventState & SCARD.STATE_PRESENT) != 0)
									{
										try
										{
											card_atr = new CardBuffer(states[i].rgbAtr, (int) states[i].cbAtr);
										}
										catch {}
									}
									
									try
									{
										_status_change_callback(states[i].szReader, states[i].dwEventState & ~SCARD.STATE_CHANGED, card_atr);
									}
									catch {}
								}
							}
						}
					}
				}
				
				SCARD.ReleaseContext(_status_change_context);
				_status_change_context = IntPtr.Zero;
			}
			catch (ThreadInterruptedException) { } /* Hide Interrupt */
		}
		
		/**f* SCardReaderList/Readers
		 *
		 * NAME
		 *   SCardReaderList.Readers
		 *
		 * DESCRIPTION
		 *   Provides the list of the monitored PC/SC readers
		 *
		 * SYNOPSIS
		 *   string[] SCardReaderList.Readers
		 *
		 **/
		public string[] Readers
		{
			get
			{
				return _reader_names;
			}
		}
		
		public bool Contains(string ReaderName)
		{
			if (_reader_names != null)
				foreach (string s in _reader_names)
					if (s == ReaderName)
						return true;
			return false;
		}

		/**v* SCardReaderList/LastError
		 *
		 * NAME
		 *   uint SCardReaderList.LastError
		 * 
		 * OUTPUT
		 *   Returns the last error encountered by the object when working with SCARD functions.
		 *
		 * SEE ALSO
		 *   SCardReaderList.LastErrorAsString
		 *
		 **/
		public uint LastError
		{
			get
			{
				return _last_error;
			}
		}

		/**v* SCardReaderList/LastErrorAsString
		 *
		 * NAME
		 *   string SCardReaderList.LastErrorAsString
		 * 
		 * OUTPUT
		 *   Returns the last error encountered by the object when working with SCARD functions.
		 *
		 * SEE ALSO
		 *   SCardReaderList.LastError
		 *
		 **/
		public string LastErrorAsString
		{
			get
			{
				return SCARD.ErrorToString(_last_error);
			}
		}

		public SCardReader GetReader(int index)
		{
			if (_reader_names != null)
				if (index < _reader_names.Length)
					return GetReader(_reader_names[index]);
			return null;
		}
		
		public SCardReader GetReader(string reader_name)
		{
			return new SCardReader(reader_name);
		}

	}
}