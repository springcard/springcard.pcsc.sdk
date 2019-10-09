/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 11/10/2013
 * Time: 14:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using SpringCard.PCSC;

namespace SpringCard.NFC
{
	public class SCardNfcAdapter
	{
		private SCardReader _reader;
		private LLCP _llcp;
		
		public delegate Ndef CreateNdefCallback();
		private CreateNdefCallback _CreateNdefCallback;
		public delegate void NdefSentCallback();
		private NdefSentCallback _NdefSentCallback;
		public delegate void AdapterErrorCallback();
		private AdapterErrorCallback _AdapterErrorCallback;
		
		public SCardNfcAdapter(string ReaderName)
		{
			_reader = new SCardReader(ReaderName);
		}
		
		public SCardNfcAdapter(SCardReader Reader)
		{
			_reader = Reader;
		}
		
		public void Start()
		{
			Start(null);
		}
		
		public void Start(AdapterErrorCallback _AdapterError)
		{
			_AdapterErrorCallback = _AdapterError;
			_reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
		}
		
		public void Stop()
		{
			if (_reader != null)
			{
				_reader.StopMonitor();
			}
			if (_llcp != null)
			{
				_llcp.Stop();
				_llcp = null;
			}
		}
		
		public void EnableNdefSend(CreateNdefCallback _CreateNdef, NdefSentCallback _NdefSent)
		{
			_CreateNdefCallback = _CreateNdef;
			_NdefSentCallback = _NdefSent;
		}
		
		delegate void ReaderStatusChangedInvoker(uint ReaderState, CardBuffer CardAtr);
		void ReaderStatusChanged(uint ReaderState, CardBuffer CardAtr)
		{
			if (ReaderState == SCARD.STATE_UNAWARE)
			{
				Trace.WriteLine("*** Reader removed ***");

				if (_AdapterErrorCallback != null)
					_AdapterErrorCallback();
				
				if (_llcp != null)
				{
					_llcp.Stop();
					_llcp = null;
				}
				_reader = null;
				return;				
			}
			
			if ((ReaderState & SCARD.STATE_EMPTY) != 0)
			{
				if (_llcp != null)
				{
					_llcp.Stop();
					_llcp = null;
				}

				Trace.WriteLine("*** Target removed ***");				
			} else
				if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
			{

			} else
				if ((ReaderState & SCARD.STATE_MUTE) != 0)
			{

			} else
				if ((ReaderState & SCARD.STATE_INUSE) != 0)
			{

			} else
				if ((ReaderState & SCARD.STATE_PRESENT) != 0)
			{
				Trace.WriteLine("*** Target arrived ***");				
			
				if (_llcp == null)
				{
					_llcp = new LlcpInitiator(_reader);
					
					if (_CreateNdefCallback != null)
					{
						Ndef ndef = _CreateNdefCallback();
						
						if (ndef != null)
						{						
							Trace.WriteLine("Create SNEP Client (sender)");
		
							SNEP_Client client = new SNEP_Client(ndef);
							
							client.OnMessageSent = new SNEP_Client.SNEPMessageSentCallback(_NdefSentCallback);
							client.MessageSentCallbackOnAcknowledge = true;
		
							_llcp.RegisterClient(client);
						}
					}
					
					_llcp.Start();
				}
			}			
		}		
	}
}
