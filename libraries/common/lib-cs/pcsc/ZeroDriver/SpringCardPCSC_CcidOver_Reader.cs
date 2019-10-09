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
	public class SCardReader_CcidOver : SCardReader
	{
		SCardReaderList_CcidOver ParentReaderList;
		byte ReaderSlot;
		
		public SCardReader_CcidOver(SCardReaderList_CcidOver ParentReaderList, byte ReaderSlot)
		{
			this.ParentReaderList = ParentReaderList;
			this.ReaderSlot = ReaderSlot;
			readerName = ParentReaderList.Readers[ReaderSlot];
		}
		
		~SCardReader_CcidOver()
		{
			
		}

        internal void SetReaderName(string readerName)
        {
            this.readerName = readerName;
        }
		
		public bool Available			
		{
			get
			{
				if (ParentReaderList == null) return false;
				return ParentReaderList.Available;
			}
		}
		
		public override SCardChannel CreateChannel()
		{
			return new SCardChannel_CcidOver(ParentReaderList, ReaderSlot);
		}
		
		protected override void MonitorProc()
		{
			uint state = 0;
			
			while (statusMonitorRunning) {
				
				uint rc;

				try {
					rc = ParentReaderList.GetStatusChange(ReaderSlot, ref state, 250);
				}
				catch (ThreadInterruptedException) {
					break;
				}

				if (!statusMonitorRunning)
					break;

				if (rc == SCARD.E_TIMEOUT)
					continue;

				if (rc != SCARD.S_SUCCESS) {
					_last_error = rc;
					if (onReaderStateChange != null)
						onReaderStateChange(0, null);
					break;
				}

				if ((state & SCARD.STATE_CHANGED) != 0) {
					state = state & ~SCARD.STATE_CHANGED;
					
					if (onReaderStateChange != null) {
						CardBuffer card_atr = null;

						if ((state & SCARD.STATE_PRESENT) != 0)
							card_atr = new CardBuffer(ParentReaderList.GetAtr(ReaderSlot));

						onReaderStateChange(state, card_atr);
					}
				}
			}
		}
	}
}
