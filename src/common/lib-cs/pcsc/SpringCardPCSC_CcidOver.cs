/**h* SpringCard/PCSC_CcidOver
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
	public abstract class SCardReaderList_CcidOver : SCardReaderList
	{	
		#region Variables
		
		protected enum DeviceState
		{
			NotActive,
			Active,
			Error
		}
		
		private Object deviceLocker = new Object();
		protected DeviceState deviceState;
		
		private byte bSequence = 0;
		
		private Thread WorkerThread;        

        private byte[] DeviceDescriptor;
		private byte[] ConfigurationDescriptor;
        private byte[] LastControlAnswer;
		
		public string VendorName { get; protected set; }
		public string ProductName { get; protected set; }
		public string SerialNumber { get; protected set; }
		public string Version { get; protected set; }
		
		public int SlotCount { get; protected set; }
		
		protected AutoResetEvent CommandSyncEvent = new AutoResetEvent(false);		
        public int CommandTimeout = 120000;		
		
		private AutoResetEvent ControlRecvEvent = new AutoResetEvent(false);
		private AutoResetEvent BulkRecvEvent = new AutoResetEvent(false);		
		
		private AutoResetEvent WorkerWakeupEvent = new AutoResetEvent(false);
		
		public delegate bool RawMessageReceivedCallback(byte[] Message);
		public RawMessageReceivedCallback OnRawControlIn = null;
		public RawMessageReceivedCallback OnRawInterruptIn = null;
		public RawMessageReceivedCallback OnRawBulkIn = null;
		
		public delegate void DisconnectedCallback();
		public DisconnectedCallback OnDisconnect = null;
		
		protected volatile bool ReadersReady = false;
		
		private byte[] pendingRecvBuffer = null;
		
		#endregion

		#region Reader object
		
		private class ChildReader
		{
			public SCardReader_CcidOver ReaderObject;
			
			private enum SlotState
			{
				CardAbsent,
				CardNewlyInserted,
				CardUnpowered,
				CardPowerUpPending,
				CardPowered
			}
			
			private Object slotLocker = new Object();
			private volatile SlotState slotState;
			private volatile bool slotiInUse = false;
			private int usedByProcessId;
			private int usedByThreadId;
			private byte[] cardAtr;

			private CCID.RDR_to_PC_Block LastResponse;
			
			public AutoResetEvent StateChangedEvent;
			private AutoResetEvent ExchangeDoneEvent;
			
			public ChildReader(SCardReader_CcidOver ReaderObject)
			{
				StateChangedEvent = new AutoResetEvent(false);
				ExchangeDoneEvent = new AutoResetEvent(false);
				this.ReaderObject = ReaderObject;
			}
			
			#region State

			public byte[] GetAtr()
			{
				return cardAtr;
			}

			public uint GetState()
			{
				if ((ReaderObject == null) || (!ReaderObject.Available))
					return SCARD.STATE_UNAVAILABLE;
				
				uint state = 0;
				
				lock(slotLocker)
				{
					switch (slotState)
					{
						case SlotState.CardUnpowered :
							state = SCARD.STATE_PRESENT | SCARD.STATE_UNPOWERED;
							if (slotiInUse)
								state |= SCARD.STATE_INUSE;
							break;
						case SlotState.CardPowered :
							state = SCARD.STATE_PRESENT;
							if (slotiInUse)
								state |= SCARD.STATE_INUSE;						
							break;
						default :
							state = SCARD.STATE_EMPTY;
							break;
					}
				}
				
				return state;				
			}
			
			public uint GetState(uint old_state = 0)
			{
				uint new_state = GetState();
				
				if (new_state != old_state) {
					new_state |= SCARD.STATE_CHANGED;
				}
				
				return new_state;
			}
			
			public bool IsUser()
			{
				return IsUser(Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId);
			}
			
			public bool IsUser(int processId, int threadId)
			{
				lock(slotLocker)
				{
					if (!slotiInUse) return false;
					if (usedByProcessId != processId) return false;
					if (usedByThreadId != threadId) return false;
					return true;
				}				
			}

			public void SetInUse()
			{
				SetInUse(Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId);
			}
			
			public void SetInUse(int processId, int threadId)
			{
				lock(slotLocker)
				{
					usedByProcessId = processId;
					usedByThreadId = threadId;
					slotiInUse = true;
				}
			}
			
			public void ClearInUse()
			{
				lock(slotLocker)
				{
					slotiInUse = false;
				}
			}
			#endregion

			public byte RDR_to_PC_Message()
			{
				return LastResponse.Message;
			}

			public byte[] RDR_to_PC_Data()
			{
				return LastResponse.Data;
			}
			
			public void RDR_to_PC(CCID.RDR_to_PC_Block block)
			{
				LastResponse = block;
				
				Logger.Trace("RDR_to_PC>" + BinConvert.ToHex(block.Message) + " " + BinConvert.ToHex(block.Status) + " " + BinConvert.ToHex(block.Error) + " " + BinConvert.ToHex(block.Data));
				
				SlotState localState;
				lock(slotLocker)
				{
					localState = slotState;
				}

				switch (block.Status & 0x03) {
					case 0:
						if ((localState == SlotState.CardPowerUpPending) && (block.Message == CCID.RDR_TO_PC_DATABLOCK))
						{
							cardAtr = block.Data;
							Logger.Trace("Card ATR is " + BinConvert.ToHex(cardAtr));
							localState = SlotState.CardPowered;
						}
						break;
					case 1:
						localState = SlotState.CardUnpowered;
						break;
					case 2:
						localState = SlotState.CardAbsent;
						break;
					default :
						break;
				}
				
				lock(slotLocker)
				{
					if (localState != slotState)
					{
						Logger.Trace("New state is " + localState.ToString());
						slotState = localState;
					}
				}
				
				ExchangeDoneEvent.Set();
			}

			public void Wait_RDR_to_PC_Reset()
			{
				ExchangeDoneEvent.WaitOne(0);
			}
			
			public bool Wait_RDR_to_PC(int timeout_ms)
			{
				return ExchangeDoneEvent.WaitOne(timeout_ms);
			}
			
			public void NotifyInsert()
			{
				lock(slotLocker)
				{
					if (slotState < SlotState.CardNewlyInserted)
						slotState = SlotState.CardNewlyInserted;
				}
			}
			
			public void NotifyRemove()
			{
				lock(slotLocker)
				{
					slotiInUse = false;
					slotState = SlotState.CardAbsent;
				}
				StateChangedEvent.Set();
				ExchangeDoneEvent.Set();				
			}
			
			public bool PowerUpRequired()
			{
				lock(slotLocker)
				{
					return (slotState == SlotState.CardNewlyInserted);
				}
			}
			
			public void SetPowerUpPending()
			{
				lock(slotLocker)
				{
					slotState = SlotState.CardPowerUpPending;
				}
			}

		}
		
		#endregion

		ChildReader[] Children;
		volatile bool running;
		
		protected abstract void CloseDevice();
		
		public override void Release()
		{
            Logger.Trace("CcidOver:Release...");
            StopMonitor();
			StopDevice();
			CloseDevice();
			Logger.Trace("CcidOver:Released");
		}
		
        protected virtual void StopDevice()
		{
			if (running)
			{
				running = false;
                if (WorkerThread != null)
                {
                    Logger.Trace(String.Format("CcidOver:Worker:Stop..."));
                    WorkerThread.Interrupt();
                    WorkerThread.Join();
                    Logger.Trace(String.Format("CcidOver:Worker:Stopped."));                    
                    WorkerThread = null;
                }                
            }
        }
		
		protected virtual bool StartDevice()
		{		
			running = true;
			WorkerThread = new Thread(CcidWorkerProc);
			WorkerThread.Start();
			return true;
		}
		
		private void CcidWorkerProc()
		{
            Logger.Trace(String.Format("CcidOver:Worker:Running..."));
            while (running)
            {
                try
                {
                    WorkerWakeupEvent.WaitOne(250);
                    if (!running)
                        break;

                    for (byte slot = 0; slot < SlotCount; slot++)
                    {
                        if (Children[slot].PowerUpRequired())
                        {
                            Logger.Trace(String.Format("CcidOver:Worker:Power UP slot " + slot));
                            Children[slot].SetPowerUpPending();
                            PC_to_RDR(slot, CCID.PC_TO_RDR_ICCPOWERON);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadInterruptedException)
                    {
                    	Logger.Trace(String.Format("CcidOver:Worker:Interrupted"));
                    } else
                    {
                        Logger.Error(String.Format("CcidOver:Worker:Exception {0}", e.Message));
                    }
                }
            }
			
			Logger.Trace(String.Format("CcidOver:Worker:Exiting..."));
		}
		
		#region Communication endpoints
		
		public bool Probe()
		{
            Logger.Debug("Probe...");
            if (!sendControl(0, 0, 0, 0))
            	return false;
            if (!WaitControl())
            	return false;
            return true;
        }
		
		public bool Available
		{
			get
			{
				lock(deviceLocker)
				{
					if (deviceState != DeviceState.Active)
					{
						Logger.Debug("Not running: " + deviceState.ToString());
						return false;
					}
					return true;
				}
			}
		}

		protected abstract bool Send(byte endpoint, byte[] buffer);

		private int BufferDataLength(byte[] buffer)
		{
            if ((buffer == null) || (buffer.Length < 11))
                return 0; 

            /* byte 0 is endpoint */
            /* byte 1 is message code */
            /* Length of data is on bytes 2 to 4 */
            long dataLength;
            dataLength  = buffer[5]; dataLength *= 0x00000100;
			dataLength += buffer[4]; dataLength *= 0x00000100;
			dataLength += buffer[3]; dataLength *= 0x00000100;
			dataLength += buffer[2];
			
			if (dataLength > int.MaxValue)
				return 0;
            
			return (int) dataLength;
		}
		
        private bool IsBufferComplete(byte[] buffer)
        {
            if ((buffer == null) || (buffer.Length < 11))
                return false;
            
            int dataLength = BufferDataLength(buffer);

            if (buffer.Length >= (11 + dataLength))
                return true;

            return false;
        }

        protected void Recv(byte[] buffer)
        {
        	Logger.Trace(">" + BinConvert.ToHex(buffer));
        	
        	if (pendingRecvBuffer == null)
        	{
        		if (IsBufferComplete(buffer))
	        	{
	            	/* Process this buffer */
	        		Recv(buffer, out pendingRecvBuffer);
        		}
        		else
        		{
	            	/* Store this buffer for later use */
	            	pendingRecvBuffer = buffer;
        		}
        	}        		
        	else
            {
            	/* Append new buffer after the one we already have received */
				byte[] newPendingRecvBuffer = new byte[pendingRecvBuffer.Length + buffer.Length];
				Array.Copy(pendingRecvBuffer, 0, newPendingRecvBuffer, 0, pendingRecvBuffer.Length);
				Array.Copy(buffer, 0, newPendingRecvBuffer, pendingRecvBuffer.Length, buffer.Length);
				pendingRecvBuffer = newPendingRecvBuffer;

				if (IsBufferComplete(pendingRecvBuffer))
				{
					/* Process this buffer */
					Recv(pendingRecvBuffer, out pendingRecvBuffer);
				}
            }
        }
        
        private void Recv(byte[] buffer, out byte[] remaining)
        {
        	remaining = null;
        	
            if ((buffer == null) || (buffer.Length < 11))
                return;
            
            int dataLength = BufferDataLength(buffer);
        	
            if (buffer.Length < (11 + dataLength))
                return;
        	
            if (buffer.Length > (11 + dataLength))
            {
            	remaining = new byte[buffer.Length - (11 + dataLength)];
            	Array.Copy(buffer, 11 + dataLength, remaining, 0, remaining.Length);
            }
            
            byte endpoint = buffer[0];
            byte[] frame = new byte[10 + dataLength];
            Array.Copy(buffer, 1, frame, 0, frame.Length);
            
            Recv(endpoint, frame);
        }

        protected void Recv(byte endpoint, byte[] buffer)
		{
        	Logger.Trace("\t" + BinConvert.ToHex(endpoint) + " > " + BinConvert.ToHex(buffer));
        	
			switch (endpoint) {
				case CCID.EP_Control_To_PC:
					if (OnRawControlIn != null)
						if (OnRawControlIn(buffer))
							break;
					recvControl(buffer);
					break;
					
				case CCID.EP_Bulk_RDR_To_PC:
					if (OnRawBulkIn != null)
						if (OnRawBulkIn(buffer))
							break;
					if (ReadersReady)
						recvBulk(buffer);
                    break;
					
				case CCID.EP_Interrupt:
					if (OnRawInterruptIn != null)
						if (OnRawInterruptIn(buffer))
							break;
					if (ReadersReady)
						recvInterrupt(buffer);
					break;
					
				default :
					break;
			}
        	
        	if ((endpoint == CCID.EP_Bulk_RDR_To_PC) || (endpoint == CCID.EP_Control_To_PC))
        		CommandSyncEvent.Set();
		}

		protected bool sendControl(byte bRequest, ushort wValue = 0, ushort wIndex = 0, byte bOption = 0)
		{
			byte[] buffer = new byte[10];

            //ControlRecvEvent.Set();

            buffer[0] = bRequest;
			buffer[5] = (byte)(wValue % 256);
			buffer[6] = (byte)(wValue / 256);
			buffer[7] = (byte)(wIndex % 256);
			buffer[8] = (byte)(wIndex / 256);
			buffer[9] = bOption;
			
			return Send(CCID.EP_Control_To_RDR, buffer);
		}
		
		void recvControl(byte[] buffer)
		{
			byte bRequest = buffer[0];
			ushort wValue = (ushort)(buffer[5] + 256 * buffer[6]);
			ushort wIndex = (ushort)(buffer[7] + 256 * buffer[8]);
			byte bStatus = buffer[9];
			byte[] abData = RawData.CopyBuffer(buffer, 10);

            LastControlAnswer = buffer;

            recvControl(bRequest, wValue, wIndex, bStatus, abData);
		}

		void recvControl(byte bRequest, ushort wValue = 0, ushort wIndex = 0, byte bStatus = 0, byte[] abData = null)
		{
			switch (bRequest) {
				case CCID.GET_DESCRIPTOR:
					switch (wValue) {
						case 0x0001:
							DeviceDescriptor = abData;
							if (abData.Length > 13)
								Version = String.Format("{0:X}.{1:X02}", abData[13], abData[12]);
							break;
						case 0x0002:
							ConfigurationDescriptor = abData;
							if (abData.Length > 22)
                            {
                                SlotCount = abData[22]+1;
                            }								
							break;
						case 0x0103:
							VendorName = DescriptorString(abData);
							break;
						case 0x0203:
							ProductName = DescriptorString(abData);
							break;
						case 0x0303:
							SerialNumber = DescriptorString(abData);
							break;
					}
					break;
					
				case CCID.SET_CONFIGURATION:
					switch (bStatus)
					{
						case 0x00 :
							deviceState = DeviceState.NotActive;
							break;
						case 0x01 :
							deviceState = DeviceState.Active;
							break;
						default :
							deviceState = DeviceState.Error;
							break;					
					}
					break;
					
				case CCID.GET_STATE:
					if (bStatus == 0xFF)
					{
						lock(deviceLocker)
						{
							deviceState = DeviceState.Error;
						}
					}
					break;
					
				default :
					break;
			}
            ControlRecvEvent.Set();
		}
		
		protected bool WaitControl()
		{
            if (!ControlRecvEvent.WaitOne(5000))
            {
                return false;
            }			
            return true;
		}
		
		void recvBulk(byte[] buffer)
		{
			Logger.Debug("RecvBulk " + BinConvert.ToHex(buffer));
			CCID.RDR_to_PC_Block block = new CCID.RDR_to_PC_Block(buffer);
			Logger.Debug("Calling child's RDR_to_PC");
			Children[block.Slot].RDR_to_PC(block);
			Logger.Debug("Done with child's RDR_to_PC");
		}

		void recvInterrupt(byte[] buffer)
		{
			byte bRequest = buffer[0];
			byte[] abData = RawData.CopyBuffer(buffer, 10);

			if (bRequest == CCID.RDR_TO_PC_NOTIFYSLOTCHANGE) {
				if (abData != null) {
					int slot = 0;
					
					Trace.WriteLine("Interrupt: " + BinConvert.ToHex(abData));
					
					for (int i = 0; i < abData.Length; i++) {
						byte b = abData[i];
						for (int j = 0; i < 4; j++) {
							if (slot > SlotCount)
								break;
							
							if ((b & 0x02) != 0) {
								/* Change on this slot */
								if ((b & 0x01) != 0) {
									/* Card inserted */
									Children[slot].NotifyInsert();
								} else {
									/* Card removed */
									Children[slot].NotifyRemove();
								}
							}
							
							b /= 4;
							slot++;
						}
					}
					
					WorkerWakeupEvent.Set();
				}
			}
		}
		
#endregion

#region Descriptors: query, parsing and handling

		private bool GetDescriptors()
		{
			DeviceDescriptor = null;
			if (!sendControl(CCID.GET_DESCRIPTOR, 0x0001))
				return false;
			if (!WaitControl())
				return false;
			if (DeviceDescriptor == null)
				return false;
			
			ConfigurationDescriptor = null;
			if (!sendControl(CCID.GET_DESCRIPTOR, 0x0002))
				return false;
			if (!WaitControl())
				return false;
			if (ConfigurationDescriptor == null)
				return false;
			
			VendorName = null;
			if (!sendControl(CCID.GET_DESCRIPTOR, 0x0103))
				return false;
			if (!WaitControl())
				return false;
			if (VendorName == null)
				return false;

			ProductName = null;
			if (!sendControl(CCID.GET_DESCRIPTOR, 0x0203))
				return false;
			if (!WaitControl())
				return false;
			if (ProductName == null)
				return false;
			
			SerialNumber = null;
			if (!sendControl(CCID.GET_DESCRIPTOR, 0x0303))
				return false;
			if (!WaitControl())
				return false;
			if (SerialNumber == null)
				return false;
			
			return true;
		}
		
		private string DescriptorString(byte[] buffer)
		{
			string s = "";
			
			for (int i = 2; i < buffer.Length; i += 2)
				s = s + (char)buffer[i];
			
			return s;
		}

        public bool RawDescriptor(byte[] c_ctrl, out byte[] r_ctrl)
        {
            r_ctrl = null;

            if (Send(CCID.EP_Control_To_RDR, c_ctrl) == false)
                return false;

            if (!WaitControl() )
            {
                return false;
            }
            r_ctrl = new byte[LastControlAnswer.Length];
            Array.Copy(LastControlAnswer, 0, r_ctrl, 0, r_ctrl.Length);    

            //r_ctrl = _lastControlAnswer;
            return true;
        }

#endregion

#region PC_to_RDR

        public bool DoRawBulkOut(byte[] buffer)
		{
			Trace.WriteLine("PC_To_Rdr:" + BinConvert.ToHex(buffer));
			
			return Send(CCID.EP_Bulk_PC_To_RDR, buffer);
		}

		protected uint PC_to_RDR(byte slot, byte command, byte[] data = null)
		{
			byte[] buffer;
			
			if (data != null)
				buffer = new byte[10 + data.Length];
			else
				buffer = new byte[10];
			
			buffer[0] = command;
			
			buffer[1] = (byte)((buffer.Length - 10) % 256);
			buffer[2] = (byte)((buffer.Length - 10) / 256);
			
			buffer[5] = slot;
			buffer[6] = bSequence;
			
			if (bSequence < 255)
				bSequence++;
			else
				bSequence = 0;
			
			if (data != null) {
				for (int i = 0; i < data.Length; i++)
					buffer[i + 10] = data[i];
			}
			
			Trace.WriteLine("PC_To_Rdr:" + BinConvert.ToHex(buffer));
			
			if (Send(CCID.EP_Bulk_PC_To_RDR, buffer))
				return SCARD.S_SUCCESS;
			
			return SCARD.E_COMM_DATA_LOST;
		}
		
#endregion

#region Slot status

		public uint GetStatusChange(int slot, ref uint state, int timeout)
		{
			state = Children[slot].GetState(state);
			
			if ((state & SCARD.STATE_CHANGED) != 0)
				return SCARD.S_SUCCESS;
			
			try
			{
				if (!Children[slot].StateChangedEvent.WaitOne(timeout))
					return SCARD.E_TIMEOUT;
			}
			catch (ThreadInterruptedException)
			{
				return SCARD.E_CANCELLED;
			}

			state = Children[slot].GetState(state);
			
			return SCARD.S_SUCCESS;
		}

		public uint GetState(byte slot, uint state)
		{
			return Children[slot].GetState(state);
		}

		public uint State(byte slot)
		{
			return Children[slot].GetState();
		}
		
#endregion
		
#region Command to slots

		private uint PC_to_RDR_GetSlotStatus(byte slot)
		{
			return PC_to_RDR(slot, 0x65, null);
		}

		private uint PC_to_RDR_IccPowerOn(byte slot)
		{
			return PC_to_RDR(slot, 0x62, null);
		}

		private uint PC_to_RDR_IccPowerOff(byte slot)
		{
			return PC_to_RDR(slot, 0x63, null);
		}
		
		private uint PC_to_RDR_XfrBlock(byte slot, byte[] apdu)
		{
			return PC_to_RDR(slot, 0x6F, apdu);
		}

		private uint PC_to_RDR_Escape(byte slot, byte[] ctrl)
		{
			return PC_to_RDR(slot, 0x6B, ctrl);
		}
		
#endregion

#region Response from slots

		private uint Wait_RDR_to_PC(byte slot, byte code, out byte[] buffer)
		{
			buffer = null;
			
			if (!Children[slot].Wait_RDR_to_PC(CommandTimeout)) {
				Logger.Warning("Wait_RDR_to_PC:Timeout");
				return SCARD.E_TIMEOUT;
			}
			
			if (Children[slot].RDR_to_PC_Message() != code) {
				Logger.Warning("Wait_RDR_to_PC:Wrong code");
				return SCARD.E_UNEXPECTED;				
			}

			buffer = Children[slot].RDR_to_PC_Data();
			return SCARD.S_SUCCESS;			
		}

		private uint Wait_RDR_to_PC(byte slot, byte code)
		{					
			if (!Children[slot].Wait_RDR_to_PC(CommandTimeout)) {
				Logger.Warning("Wait_RDR_to_PC:Timeout");
				return SCARD.E_TIMEOUT;
			}
			
			if (Children[slot].RDR_to_PC_Message() != code) {
				Logger.Warning("Wait_RDR_to_PC:Wrong code");
				return SCARD.E_UNEXPECTED;				
			}
			
			return SCARD.S_SUCCESS;
		}
		
		private void Wait_RDR_to_PC_Reset(byte slot)
		{
			Children[slot].Wait_RDR_to_PC_Reset();
		}
		
#endregion

#region Synchronous actions

		protected uint PowerOn(byte slot)
		{
			uint rc;
			
			Wait_RDR_to_PC_Reset(slot);
			
			rc = PC_to_RDR_IccPowerOn(slot);
			if (rc != SCARD.S_SUCCESS)
				return rc;
			
			rc = Wait_RDR_to_PC(slot, 0x81);			
			if (rc != SCARD.S_SUCCESS) {
				
			}
			
			return rc;
		}

		protected uint PowerOff(byte slot)
		{
			uint rc;
			
			Wait_RDR_to_PC_Reset(slot);
			
			rc = PC_to_RDR_IccPowerOff(slot);
			if (rc != SCARD.S_SUCCESS)
				return rc;
			
			rc = Wait_RDR_to_PC(slot, 0x81);
			if (rc != SCARD.S_SUCCESS) {
				
			}
			
			return rc;
		}
		
		public uint GetSlotStatus(byte slot, out uint state)
		{
			uint rc;
			
			state = 0;
			
			Wait_RDR_to_PC_Reset(slot);
			
			rc = PC_to_RDR_GetSlotStatus(slot);
			if (rc != SCARD.S_SUCCESS)
				return rc;
			
			rc = Wait_RDR_to_PC(slot, 0x81);
			if (rc != SCARD.S_SUCCESS) {
				
			}
			
			return rc;
		}

		public uint Transmit(byte slot, byte[] c_apdu, out byte[] r_apdu)
		{
			uint state;
			uint rc;
			
			r_apdu = null;			
			
			state = State(slot);			
			if ((state & SCARD.STATE_PRESENT) == 0)
				return SCARD.W_REMOVED_CARD;
			if ((state & SCARD.STATE_UNPOWERED) != 0)
				return SCARD.W_UNPOWERED_CARD;
			
			Trace.WriteLine("Transmit");
			
			Wait_RDR_to_PC_Reset(slot);			
			
			rc = PC_to_RDR_XfrBlock(slot, c_apdu);
			if (rc != SCARD.S_SUCCESS)
				return rc;
			
			rc = Wait_RDR_to_PC(slot, 0x80, out r_apdu);
			if (rc != SCARD.S_SUCCESS)
				return rc;
			
			state = State(slot);			
			if ((state & SCARD.STATE_PRESENT) == 0)
				return SCARD.W_REMOVED_CARD;
			if ((state & SCARD.STATE_UNPOWERED) != 0)
				return SCARD.W_UNPOWERED_CARD;			
			
			return SCARD.S_SUCCESS;
		}
		
		public uint Control(byte slot, byte[] c_ctrl, out byte[] r_ctrl)
		{
			uint rc;
			
			Wait_RDR_to_PC_Reset(slot);
			
			r_ctrl = null;
			
			rc = PC_to_RDR_Escape(slot, c_ctrl);
			if (rc != SCARD.S_SUCCESS)
				return rc;
			
			rc = Wait_RDR_to_PC(slot, 0x83, out r_ctrl);
			
			return rc;			
		}
        
        public uint ConnectTo(byte slot)
		{
			uint state = Children[slot].GetState();
			
			if ((state & SCARD.STATE_INUSE) != 0)
				if (!Children[slot].IsUser())
					return SCARD.E_SHARING_VIOLATION;
			
			if ((state & SCARD.STATE_PRESENT) == 0)
				return SCARD.E_NO_SMARTCARD;
			if ((state & SCARD.STATE_UNPOWERED) != 0) {
				uint rc = PowerOn(slot);
				if (rc != SCARD.S_SUCCESS)
					return rc;
			}
			
			Children[slot].SetInUse();
			return SCARD.S_SUCCESS;
		}

        public uint ConnectDirectTo(byte slot)
		{
			uint state = Children[slot].GetState();
			
			if ((state & SCARD.STATE_INUSE) != 0)
				if (!Children[slot].IsUser())
					return SCARD.E_SHARING_VIOLATION;
			
			Children[slot].SetInUse();
			return SCARD.S_SUCCESS;
		}
        
		public uint DisconnectFrom(byte slot, bool bPowerDown = true)
		{
			uint state = Children[slot].GetState();
			
			if (((state & SCARD.STATE_INUSE) == 0) || (!Children[slot].IsUser()))
				return SCARD.E_UNEXPECTED;
			if ((state & SCARD.STATE_PRESENT) == 0)
				return SCARD.E_NO_SMARTCARD;
			Children[slot].ClearInUse();
			
			if (bPowerDown)
				PowerOff(slot);
			
			return SCARD.S_SUCCESS;
		}
		
#endregion

#region SCARD Helpers

		protected bool MakeReaderList()
		{
            Logger.Trace("MakeReaderList...");

            ReadersReady = false;
			
			if (!GetDescriptors())
				return false;

            if (SlotCount <= 0)
                throw new Exception("Unsupported CCID reader (0 slot)");
			
			_reader_names = new string[SlotCount];
			
			Children = new ChildReader[SlotCount];
			
			for (int i = 0; i < SlotCount; i++) {
				_reader_names[i] = VendorName + " " + ProductName + " Contactless " + i;
				Children[i] = new ChildReader(new SCardReader_CcidOver(this, 0));
			}
			
			ReadersReady = true;
			
			return true;
		}

		public new SCardReader_CcidOver GetReader(int slot)
		{
			return Children[slot].ReaderObject;
		}

		public byte[] GetAtr(byte slot)
		{						
			if ((Children[slot].GetState() & SCARD.STATE_PRESENT) != 0)
				return Children[slot].GetAtr();
			return null;
		}

#endregion
	}
	
	public class SCardReader_CcidOver : SCardReader
	{
		SCardReaderList_CcidOver ParentReaderList;
		byte ReaderSlot;
		
		public SCardReader_CcidOver(SCardReaderList_CcidOver ParentReaderList, byte ReaderSlot)
		{
			this.ParentReaderList = ParentReaderList;
			this.ReaderSlot = ReaderSlot;
			_reader_name = ParentReaderList.Readers[ReaderSlot];
		}
		
		~SCardReader_CcidOver()
		{
			
		}
		
		public bool Available			
		{
			get
			{
				if (ParentReaderList == null) return false;
				return ParentReaderList.Available;
			}
		}
		
		public override SCardChannel GetChannel()
		{
			return new SCardChannel_CcidOver(ParentReaderList, ReaderSlot);
		}
		
		protected override void StatusChangeMonitor()
		{
			uint state = 0;
			
			while (_status_change_running) {
				
				uint rc;

				try {
					rc = ParentReaderList.GetStatusChange(ReaderSlot, ref state, 250);
				}
				catch (ThreadInterruptedException) {
					break;
				}

				if (!_status_change_running)
					break;

				if (rc == SCARD.E_TIMEOUT)
					continue;

				if (rc != SCARD.S_SUCCESS) {
					_last_error = rc;
					if (_status_change_callback != null)
						_status_change_callback(0, null);
					break;
				}

				if ((state & SCARD.STATE_CHANGED) != 0) {
					state = state & ~SCARD.STATE_CHANGED;
					
					if (_status_change_callback != null) {
						CardBuffer card_atr = null;

						if ((state & SCARD.STATE_PRESENT) != 0)
							card_atr = new CardBuffer(ParentReaderList.GetAtr(ReaderSlot));

						_status_change_callback(state, card_atr);
					}
				}
			}
		}
	}
	
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
		
		public bool ConnectDirect()
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
			
			Trace.WriteLine("Disconnect, disposition=" + disposition);
			
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
			
			_last_error = ReaderDevice.Transmit(Slot, _capdu.GetBytes(), out buffer);

			if (_last_error != SCARD.S_SUCCESS)
				return false;
			
			Trace.WriteLine("Transmit>" + BinConvert.ToHex(buffer));			
			
			_rapdu = new RAPDU(buffer);
			
			Trace.WriteLine("Transmit>" + _rapdu.AsString());
			
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
