/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using System.Diagnostics;
using System.Threading;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.ZeroDriver
{
	public class SCardChannel_CcidOver : SCardChannel
	{
		SCardReaderList_CcidOver ReaderDevice;
		byte Slot;
		enum ConnexionState { NotConnected, Connected, ConnectedDirect };
		ConnexionState connexionState = ConnexionState.NotConnected;
		
		public SCardChannel_CcidOver(SCardReaderList_CcidOver ReaderDevice, byte Slot)
		{
			this.ReaderDevice = ReaderDevice;
			this.Slot = Slot;			
		}
		
		protected override void UpdateState()
		{
			_reader_state = ReaderDevice.State(Slot);
			Logger.Debug("UpdateState:" + BinConvert.ToHex(_reader_state));
			if ((_reader_state & SCARD.STATE_PRESENT) != 0)
				_card_atr = new CardBuffer(ReaderDevice.GetAtr(Slot));
			else
				_card_atr = null;				
		}
		
		public override bool Connected {
			get {
				return (connexionState != ConnexionState.NotConnected);
			}			
		}

		public override bool ConnectedDirect {
			get {
				return (connexionState == ConnexionState.ConnectedDirect);
			}			
		}
		
		public override bool Connect()
		{
			if (connexionState == ConnexionState.Connected)
				return true;
			if (connexionState == ConnexionState.ConnectedDirect)
				Disconnect(SCARD.LEAVE_CARD);
			
			_last_error = ReaderDevice.ConnectTo(Slot);
			
			if (_last_error != SCARD.S_SUCCESS)
			{
				Logger.Trace("Connect failed: " + BinConvert.ToHex(_last_error));
				return false;
			}
			
			connexionState = ConnexionState.Connected;
			return true;
		}
		
		public override bool ConnectDirect()
		{
			if (connexionState == ConnexionState.ConnectedDirect)
				return true;
			if (connexionState == ConnexionState.Connected)
				Disconnect(SCARD.LEAVE_CARD);
			
			_last_error = ReaderDevice.ConnectDirectTo(Slot);
			
			if (_last_error != SCARD.S_SUCCESS)
				return false;
			
			connexionState = ConnexionState.ConnectedDirect;
			return true;			
		}
		
		public override bool Disconnect(uint disposition)
		{
			if (connexionState != ConnexionState.NotConnected)
				return false;
			
			connexionState = ConnexionState.NotConnected;
			
			Logger.Trace("Disconnect, disposition=" + disposition);
			
			bool powerDown = false;
			
			if ((disposition & SCARD.UNPOWER_CARD) != 0)
				powerDown = true;
			if ((disposition & SCARD.EJECT_CARD) != 0)
				powerDown = true;
			
			_last_error = ReaderDevice.DisconnectFrom(Slot, powerDown);

			if (_last_error != SCARD.S_SUCCESS)
				return false;
			
			return true;
		}
		
		public override bool Reconnect(uint disposition)
		{
			return Disconnect(disposition) && Connect();
		}
		
		public override bool Transmit()
		{
			byte[] buffer;

            Logger.Trace("Transmit<" + _capdu.AsString());

            _last_error = ReaderDevice.Transmit(Slot, _capdu.GetBytes(), out buffer);

			if (_last_error != SCARD.S_SUCCESS)
				return false;
		
			_rapdu = new RAPDU(buffer);
			
			Logger.Trace("Transmit>" + _rapdu.AsString());
			
			return true;			

		}
		
		public override byte[] Control(byte[] cctrl)
		{
			byte[] buffer;
			
			_last_error = ReaderDevice.Control(Slot, cctrl, out buffer);
			
			if (_last_error != SCARD.S_SUCCESS)
				return null;
			
			return buffer;
		}
	}
}
