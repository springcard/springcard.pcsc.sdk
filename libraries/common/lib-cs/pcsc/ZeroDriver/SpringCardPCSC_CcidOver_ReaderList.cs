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
	public abstract partial class SCardReaderList_CcidOver : SCardReaderList
	{
        #region Variables

        protected SecureConnectionParameters secureConnectionParameters;

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
		
		public delegate void BackgroundInstantiateCallback(SCardReaderList_CcidOver instance);
		
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

        protected class ChildReader
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
                {
                    Logger.Debug("Device lost");
                    return SCARD.STATE_UNAVAILABLE;
                }

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

            internal void SetState(uint new_state)
            {
                if ((new_state & SCARD.STATE_PRESENT) != 0)
                {
                    if (slotState == SlotState.CardAbsent)
                    {
                        slotState = SlotState.CardNewlyInserted;
                    }
                    else if ((new_state & SCARD.STATE_UNPOWERED) != 0)
                    {
                        slotState = SlotState.CardUnpowered;
                    }
                    else
                    {
                        slotState = SlotState.CardPowered;
                    }
                }
                else
                {
                    slotState = SlotState.CardAbsent;
                }
            }
			
			public uint GetState(uint old_state = 0)
			{
				uint new_state = GetState();
				
				if (new_state != old_state) {
					new_state |= SCARD.STATE_CHANGED;
				}
				
				return new_state;
			}
			
			public bool IsCardPresent()
			{
				lock(slotLocker)
				{
					if (slotState == SlotState.CardAbsent) return false;
					return true;
				}				
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
							
				SlotState localState;
				lock(slotLocker)
				{
					localState = slotState;
				}

                switch (block.Status & 0x03)
                {
					case 0:
                        if (block.Message == CCID.RDR_TO_PC_SLOTSTATUS)
                        {
                            if (localState == SlotState.CardAbsent)
                            {
                                Logger.Debug("Card newly inserted");
                                localState = SlotState.CardNewlyInserted;
                            }
                            else if (localState != SlotState.CardPowered)
                            {
                                Logger.Fatal("Card is now powered");
                                localState = SlotState.CardPowered;
                            }
                        }
                        else if (block.Message == CCID.RDR_TO_PC_DATABLOCK)
                        {
                            if (localState == SlotState.CardPowerUpPending)
                            {
                                cardAtr = block.Data;
                                Logger.Trace("Card ATR is " + BinConvert.ToHex(cardAtr));
                                localState = SlotState.CardPowered;
                            }
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
				try
				{
					return ExchangeDoneEvent.WaitOne(timeout_ms);
				}
				catch (Exception e)
				{            
	                if (e is ThreadInterruptedException)
	                {
	                	Logger.Trace("CcidOver:Wait_RDR_to_PC:Interrupted");
	                } else
	                {
	                	Logger.Trace("CcidOver:Wait_RDR_to_PC:Exception: {0}", e.Message);
	                }
	                
	                return false;
				}
			}
			
			public void NotifyParentLost()
			{
                Logger.Trace("CcidOver:ParentLost");

                lock (slotLocker)
				{
					slotiInUse = false;
					slotState = SlotState.CardAbsent;
					ReaderObject = null;
				}

				StateChangedEvent.Set();
				ExchangeDoneEvent.Set();				
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

		protected ChildReader[] Children;
        protected bool PollStatus = false;
        private int ProbeTimeoutMs = 3000;
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
                    Logger.Trace("CcidOver:Worker:Stop...");
                    WorkerThread.Interrupt();
                    WorkerThread.Join();
                    Logger.Trace("CcidOver:Worker:Stopped.");
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
            Logger.Trace("CcidOver:Worker:Running...");
            while (running)
            {
                try
                {
					Logger.Debug("CcidOver:Worker:Waiting...");
					WorkerWakeupEvent.WaitOne(250);

                    if (!running)
                    {
                        Logger.Debug("CcidOver:Worker:Not running");
                        break;
                    }
                    if (deviceState != DeviceState.Active)
                    {
                        Logger.Debug("CcidOver:Worker:Not active");
                        break;
                    }

                    for (byte slot = 0; slot < SlotCount; slot++)
                    {
                        if (Children[slot].PowerUpRequired())
                        {
                            Logger.Trace("CcidOver:Worker:Power UP slot {0}", slot);
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
                    	Logger.Trace("CcidOver:Worker:Interrupted");
                    	break;
                    } else
                    {
                        Logger.Error("CcidOver:Worker:Exception {0}", e.Message);
                    }
                }
            }
			
			Logger.Trace("CcidOver:Worker:Exiting...");
		}
		
		#region Communication endpoints
		
		private DateTime probeTimeout = DateTime.MinValue;

        private void ValidMessageReceived()
        {
            lock (deviceLocker)
            {
                probeTimeout = DateTime.Now.AddMilliseconds(ProbeTimeoutMs);
            }
        }
		
		private bool Probe()
		{
            Logger.Debug("Probe...");
            if (!SendControl(0, 0, 0, 0))
            {
            	Logger.Warning("Failed to send probe (device lost?)");
            	
            	deviceState = DeviceState.Error;
            	probeTimeout = DateTime.MinValue;
	            
            	return false;
            }
            
           	probeTimeout = DateTime.Now.AddMilliseconds(ProbeTimeoutMs);
            return true;
        }
		
		public virtual bool Available
		{
			get
			{
				lock(deviceLocker)
				{
					if (probeTimeout < DateTime.Now)
					{
						Probe();
					}
					
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

        private bool IsBufferComplete(byte[] buffer, out int deal)
        {
            deal = 0;
            if ((buffer == null) || (buffer.Length < 11))
            {
                Trace.WriteLine("IsBufferComplete: Lack Error buffer.Length: " + buffer.Length);
                return false;
            }
            
            /* byte 0 is endpoint */
            /* byte 1 is message code */
            /* Length of data is on bytes 2 to 4 */
            int dataLength;
            dataLength = buffer[5]; dataLength *= 0x00000100;
            dataLength += buffer[4]; dataLength *= 0x00000100;
            dataLength += buffer[3]; dataLength *= 0x00000100;
            dataLength += buffer[2];

            if ((dataLength < 0) || (dataLength > 0x00010000))
            {
                deal = -1;
                //Trace.WriteLine("IsBufferComplete: Error buffer.Length: " + buffer.Length + " dataLength: " + dataLength);
                return true;
            }

            if (buffer.Length == (11 + dataLength))
            {
                //Trace.WriteLine("IsBufferComplete: buffer.Length: " + buffer.Length + " dataLength: " + dataLength);
                return true;
            }

            /* two commands inside the same trame */
            if (buffer.Length > (11 + dataLength))
            {
                //Trace.WriteLine("IsBufferComplete+: buffer.Length: " + buffer.Length + " dataLength: " + dataLength);
                deal = (11 + dataLength);
                return true;
            }

            //Trace.WriteLine("IsBufferComplete-: buffer.Length: " + buffer.Length + " dataLength: " + dataLength);

            return false;
        }

        protected bool Recv(byte[] buffer, out int deal_done)
        {
            deal_done = 0;

            if (IsBufferComplete(buffer, out deal_done))
            {
                /* means crazy size from ccid command */
                if (deal_done == -1)
                {
                    return true;
                }

                Recv(buffer[0], BinUtils.Copy(buffer, 1, deal_done));
                return true;
            }
            return false;
        }

        protected void Recv(byte endpoint, byte[] buffer)
        {
            switch (endpoint)
            {
                case CCID.EP_Control_To_PC:
                    if (OnRawControlIn != null)
                        if (OnRawControlIn(buffer))
                            break;
                    RecvControl(buffer);
                    break;

                case CCID.EP_Bulk_RDR_To_PC:
                    if (OnRawBulkIn != null)
                        if (OnRawBulkIn(buffer))
                            break;
                    if (ReadersReady && (deviceState == DeviceState.Active))
                        RecvBulk(buffer);
                    else if (ReadersReady)
                        RecvBulk(buffer);
                    break;

                case CCID.EP_Interrupt:
                    if (OnRawInterruptIn != null)
                        if (OnRawInterruptIn(buffer))
                            break;
                    if (ReadersReady && (deviceState == DeviceState.Active))
                        RecvInterrupt(buffer);
                    break;

                default:
                    break;
            }
        }

        protected bool SendControl(byte bRequest, ushort wValue = 0, ushort wIndex = 0, byte bOption = 0)
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
		
		void RecvControl(byte[] buffer)
		{
			byte bRequest = buffer[0];
			ushort wValue = (ushort)(buffer[5] + 256 * buffer[6]);
			ushort wIndex = (ushort)(buffer[7] + 256 * buffer[8]);
			byte bStatus = buffer[9];
			byte[] abData = BinUtils.Copy(buffer, 10);

            LastControlAnswer = buffer;

            RecvControl(bRequest, wValue, wIndex, bStatus, abData);
		}

		void RecvControl(byte bRequest, ushort wValue = 0, ushort wIndex = 0, byte bStatus = 0, byte[] abData = null)
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
			try
			{			
	            if (!ControlRecvEvent.WaitOne(5000))
	            {
	                return false;
	            }			
	            return true;
			}
			catch (Exception e)
			{            
                if (e is ThreadInterruptedException)
                {
                	Logger.Trace("CcidOver:WaitControl:Interrupted");
                } else
                {
                	Logger.Trace("CcidOver:WaitControl:Exception: {0}", e.Message);
                }
                
                return false;
			}            
		}

		void RecvBulk(byte[] buffer)
		{
            /* sanity check */
            if (buffer == null)
            {
                Logger.Trace("RecvBulk: invalid buffer");
                deviceState = DeviceState.Error;
                return;
            }

            Logger.Debug("   >" + BinConvert.ToHex(buffer));

            /* new message */
            if (pendingRecvBuffer == null)
            {
                if (buffer.Length < 10)
                {
                    Logger.Trace("RecvBulk: buffer is too short");
                    deviceState = DeviceState.Error;
                    return;
                }

                /* save this part */
                pendingRecvBuffer = new byte[buffer.Length];                
                Array.Copy(buffer, pendingRecvBuffer, buffer.Length);
            } else
            {
                /* append to previous part */
                int previous_length = pendingRecvBuffer.Length;
                Array.Resize(ref pendingRecvBuffer, (previous_length + buffer.Length));
                Array.Copy(buffer, 0, pendingRecvBuffer, previous_length, buffer.Length);
            }

            /* is the message complete ? */
            uint DataLength = 0;
            bool Secure = ((pendingRecvBuffer[4] & 0x80) != 0);
            DataLength += (byte) (pendingRecvBuffer[4] & 0x7F); DataLength *= 256;
            DataLength += pendingRecvBuffer[3]; DataLength *= 256;
            DataLength += pendingRecvBuffer[2]; DataLength *= 256;
            DataLength += pendingRecvBuffer[1];

            if ((pendingRecvBuffer.Length) == (10 + DataLength))
            {
                byte[] ccid_buffer = pendingRecvBuffer;
                pendingRecvBuffer = null;

                /* Decipher the message */
                if (Secure)
                {
                    ccid_buffer = DecryptCcidBuffer(ccid_buffer);
                    if (ccid_buffer == null)
                    {
                        Logger.Error("CCID decryption failed");
                        return; /* Oops */
                    }
                }

                /* Yes! This is a valid message */
                ValidMessageReceived();
                /* Handle it */
                CCID.RDR_to_PC_Block block = new CCID.RDR_to_PC_Block(ccid_buffer);
                pendingRecvBuffer = null;
                Children[block.Slot].RDR_to_PC(block);
            }
		}

		void RecvInterrupt(byte[] buffer)
		{
			byte bRequest = buffer[0];
			byte[] abData = BinUtils.Copy(buffer, 10);

			if (bRequest == CCID.RDR_TO_PC_NOTIFYSLOTCHANGE) {
				if (abData != null) {
					int slot = 0;
					
					Logger.Debug("Interrupt: " + BinConvert.ToHex(abData));
					
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

		protected virtual bool GetDescriptors()
		{
			DeviceDescriptor = null;
			if (!SendControl(CCID.GET_DESCRIPTOR, 0x0001))
				return false;
			if (!WaitControl())
				return false;
			if (DeviceDescriptor == null)
				return false;

            SlotCount = 1; // TODO GET SLOT COUNT
			
			ConfigurationDescriptor = null;
			if (!SendControl(CCID.GET_DESCRIPTOR, 0x0002))
				return false;
			if (!WaitControl())
				return false;
			if (ConfigurationDescriptor == null)
				return false;
			
			VendorName = null;
			if (!SendControl(CCID.GET_DESCRIPTOR, 0x0103))
				return false;
			if (!WaitControl())
				return false;
			if (VendorName == null)
				return false;

			ProductName = null;
			if (!SendControl(CCID.GET_DESCRIPTOR, 0x0203))
				return false;
			if (!WaitControl())
				return false;
			if (ProductName == null)
				return false;
			
			SerialNumber = null;
			if (!SendControl(CCID.GET_DESCRIPTOR, 0x0303))
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

        private void PC_to_RDR_SetLength(byte[] buffer, int dataLength, bool secure)
        {
            buffer[1] = (byte)(dataLength & 0x0FF);
            buffer[2] = (byte)((dataLength >> 8) & 0x0FF);
            buffer[3] = (byte)((dataLength >> 16) & 0x0FF);
            buffer[4] = 0;
            if (secure) buffer[4] |= 0x80;
        }

        public bool DoRawBulkOut(byte[] buffer)
		{
			Logger.Debug("PC_To_Rdr:" + BinConvert.ToHex(buffer));
	
			return Send(CCID.EP_Bulk_PC_To_RDR, buffer);
		}

		protected uint PC_to_RDR(byte slot, byte command, byte[] data = null)
		{
            /* clear a potential -not terminated- recv buffer */
            pendingRecvBuffer = null;

            if (data == null)
                data = new byte[0];

            /* Prepare the header */
            /* ------------------ */

            byte[] ccid_header = new byte[10];

            ccid_header[0] = command;
            PC_to_RDR_SetLength(ccid_header, data.Length, false);
            ccid_header[5] = slot;
            ccid_header[6] = bSequence;

            if (bSequence < 255)
				bSequence++;
			else
				bSequence = 0;

            /* Assemble the buffer */
            /* ------------------- */

            byte[] ccid_buffer = BinUtils.Concat(ccid_header, data);

            Logger.Debug("   <{0}", BinConvert.ToHex(ccid_buffer));

            /* Handle secure communication */
            /* --------------------------- */

            if (SecureCommMode == SecureConnectionParameters.CommunicationMode.Secure)
            {
                ccid_buffer = EncryptCcidBuffer(ccid_buffer);
            }
			
			if (Send(CCID.EP_Bulk_PC_To_RDR, ccid_buffer))
				return SCARD.S_SUCCESS;
			
			return SCARD.E_COMM_DATA_LOST;
		}
		
#endregion

#region Slot status

		public uint GetStatusChange(int slot, ref uint state, int timeout)
		{
            if (PollStatus)
            {
                uint old_state = state;
                int loops = 1 + timeout / 100;

                while (loops-- > 0)
                {
                    Logger.Debug("Poll status...");

                    uint rc = GetSlotStatus((byte)slot, out uint new_state);
                    if (rc != SCARD.S_SUCCESS)
                        return rc;

                    if (new_state != old_state)
                    {
                        state = new_state;
                        state |= SCARD.STATE_CHANGED;
                        return SCARD.S_SUCCESS;
                    }
                    if (timeout == 0)
                    {
                        state = new_state;
                        return SCARD.S_SUCCESS;
                    }

                    if (loops > 0)
                        Thread.Sleep(100);
                }

                return SCARD.E_TIMEOUT;
            }

            state = Children[slot].GetState(state);
			
			if ((state & SCARD.STATE_CHANGED) != 0)
				return SCARD.S_SUCCESS;
			
			try
			{
				if (!Children[slot].StateChangedEvent.WaitOne(timeout))
					return SCARD.E_TIMEOUT;

                Logger.Debug("StateChangedEvent");

            }
			catch (Exception e)
			{            
                if (e is ThreadInterruptedException)
                {
                	return SCARD.E_CANCELLED;
                } else
                {
                	Logger.Trace("GetStatusChange:Exception: {0}", e.Message);
                	return SCARD.E_UNEXPECTED;
                }
			}

            Logger.Debug("GetState?");
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
			if (!Children[slot].Wait_RDR_to_PC(CommandTimeout))
			{
				Logger.Warning("Wait_RDR_to_PC:Timeout");
				return SCARD.E_TIMEOUT;
			}
			
			if (Children[slot].RDR_to_PC_Message() != code)
			{
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

            state = GetState(slot, state);
			
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

        public uint Control(byte slot, byte[] c_ctrl)
        {
            return Control(slot, c_ctrl, out byte[] dummy);
        }

        public uint Escape(byte[] c_ctrl, out byte[] r_ctrl)
        {
            return Control(0, c_ctrl, out r_ctrl);
        }

        public uint Escape(byte[] c_ctrl)
        {
            return Control(0, c_ctrl, out byte[] dummy);
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
			{
				Logger.Trace("GetDescriptor failed");
				return false;
			}

            if (SlotCount <= 0)
                throw new Exception("Unsupported CCID reader (0 slot)");
			
			_reader_names = new string[SlotCount];
			
			Children = new ChildReader[SlotCount];
			
			for (int i = 0; i < SlotCount; i++)
            {
				_reader_names[i] = VendorName + " " + ProductName + " " + string.Format("Slot {0}", i);
				Logger.Trace("Adding child reader: " + _reader_names[i]);
				Children[i] = new ChildReader(new SCardReader_CcidOver(this, 0));
			}
			
			ReadersReady = true;
			
			return true;
		}

		public new SCardReader_CcidOver GetReader(int slot)
		{
            Logger.Debug("Selection of reader {0} ({1})", slot, _reader_names[slot]);
            return Children[slot].ReaderObject;
		}

		public byte[] GetAtr(byte slot)
		{						
			if ((Children[slot].GetState() & SCARD.STATE_PRESENT) != 0)
				return Children[slot].GetAtr();
			return null;
		}

        protected bool UpdateReaderNames()
        {
            Logger.Trace("GetReaderNames...");

            for (int i = 0; i < _reader_names.Length; i++)
            {
                if (Control((byte)i, new byte[] { 0x58, 0x21, (byte)i }, out byte[] r_ctrl) == 0)
                {
                    if ((r_ctrl != null) && (r_ctrl.Length > 0) && (r_ctrl[0] == 0x00))
                    {
                        string s = StrUtils.ToStr(r_ctrl, 1);
                        Logger.Debug("Name of slot {0} is {1}", i, s);
                        _reader_names[i] = VendorName + " " + ProductName + " " + s;
                        Children[i].ReaderObject.SetReaderName(_reader_names[i]);
                    }
                }
            }

            return true;
        }

#endregion
	}
}
