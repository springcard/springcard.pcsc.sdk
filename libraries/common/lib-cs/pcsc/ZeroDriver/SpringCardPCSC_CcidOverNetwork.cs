/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using System.Net.Sockets;
using System.Threading;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.PCSC.ZeroDriver
{
	public class SCardReaderList_CcidOverNetwork : SCardReaderList_CcidOver
	{
		TcpClient clientSocket;
		NetworkStream clientStream;
		Thread receiverThread;
        private object sync_request = new object();
        private byte[] recv_buffer;

        private AutoResetEvent syncEventWriteRead = new AutoResetEvent(false);
        private volatile bool syncWrite_Wait = false;
        private string ip_ID;

        //public bool SyncCommands = true;

        private SCardReaderList_CcidOverNetwork()
		{
		}
		
		public static SCardReaderList_CcidOverNetwork Instantiate(string Address, ushort Port = 3999, bool Start = true)
		{
			SCardReaderList_CcidOverNetwork r = new SCardReaderList_CcidOverNetwork();			

			if (!r.OpenDevice(Address, Port))
				return null;
			
			if (!r.MakeReaderList())
			{
				r.CloseDevice();
				return null;
			}

            if (Start)
            {
                if (!r.StartDevice())
                {
                    r.CloseDevice();
                    return null;
                }

                Logger.Trace("Device ready...");
            }
            else
            {
                r.SendControl(CCID.SET_CONFIGURATION, 0x0000);
                r.WaitControl();
            }
            r.ip_ID = Address;
            return r;
		}
		
		private static Thread instantiateThread;
		private class InstantiateParams
		{
			public BackgroundInstantiateCallback Callback;
			public string Address;
			public ushort Port;
		}
		
		public static void BackgroundInstantiate(BackgroundInstantiateCallback Callback, string Address, ushort Port = 3999)
		{
			if (Callback == null)
				return;
			
			InstantiateParams instantiateParams = new InstantiateParams();
			
			instantiateParams.Callback = Callback;
			instantiateParams.Address = Address;
			instantiateParams.Port = Port;
			
			instantiateThread = new Thread(InstantiateProc);
			instantiateThread.Start(instantiateParams);
		}

		private static void InstantiateProc(object p)
		{
			InstantiateParams instantiateParams = (InstantiateParams)p;
			
			Logger.Trace("CcidOverNetwork:Background instantiate");
			
			SCardReaderList_CcidOverNetwork instance = Instantiate(instantiateParams.Address, instantiateParams.Port);
			
			Logger.Trace("CcidOverNetwork:Calling the callback");
			
			instantiateParams.Callback(instance);
		}
		
		protected bool OpenDevice(string HostName, ushort TcpPort = 3999)
		{
            Logger.Trace("CcidOverNetwork:OpenDevice " + HostName + ":" + TcpPort + "...");
            try
			{
				clientSocket = new TcpClient(HostName, TcpPort);
				clientStream = clientSocket.GetStream();
				receiverThread = new Thread(ReceiverThread);			
				receiverThread.Start();				
			}
			catch
			{
				return false;
			}
			return true;
		}
		
		protected override void CloseDevice()
		{
            Logger.Trace("CcidOverNetwork:CloseDevice...");
            if (clientSocket != null)
			{
            	Logger.Trace("CcidOverNetwork:Closing the socket");
				clientSocket.Close();
				clientSocket = null;
			}
			if (receiverThread != null)
			{
				Logger.Trace("CcidOverNetwork:Stopping the receiver thread...");
				receiverThread.Interrupt();
				receiverThread.Join();
				Logger.Trace("CcidOverNetwork:Receiver thread stopped.");
				receiverThread = null;
			}
		}
		
		protected override bool StartDevice()
		{
			byte bOptions = 0;

            Logger.Trace("CcidOverNetwork:StartDevice...");

            deviceState = DeviceState.NotActive;
			
			if (!SendControl(CCID.SET_CONFIGURATION, 0x0001, 0, bOptions))
				return false;			
			if (!WaitControl())
				return false;

			if (deviceState != DeviceState.Active)
				return false;
			
			return base.StartDevice();
		}
		
		public bool Start()
		{
			return StartDevice();
		}

        /// <summary>
        /// Stop worker thread and put antenna off
        /// </summary>
        /// <param name="connected">send set configuration only if device still connected</param>
        /// <returns></returns>
        protected override void StopDevice()
        {
            base.StopDevice();

            ReadersReady = false;
            if (clientSocket != null)
            {
                if (SendControl(CCID.SET_CONFIGURATION, 0x0000))
                	WaitControl();
            }
        }

        public void Stop()
        {
            StopDevice();
        }

        private void ReceiverThread()
        {
            int done = -1;
            int bytesRead = 0;
            int offset = 0;
            recv_buffer = new byte[65536 + 34];

            while (clientSocket.Connected)
            {
                try
                {
                    bytesRead = clientStream.Read(recv_buffer, offset, (recv_buffer.Length - offset));
                    if (bytesRead <= 0)
                    {
                        continue;
                    }
                    if (!clientSocket.Connected)
                        break;

                    offset += bytesRead;

                    byte[] result = BinUtils.Copy(recv_buffer, 0, offset);

                    bool bFull = Recv(result, out done);
                    if (bFull == false)
                    {
                        byte[] result1 = BinUtils.Copy(recv_buffer, 0, offset);
                        Logger.Trace("wait rest of command>" + result1.Length);
                    }
                    else
                    {
                        if (done == 0)
                        {
                            offset = 0;
                            Logger.Trace(">" + BinConvert.ToHex(result));

                            /* notify slot change method is not concerned */
                            if ((result[0] != 0x83) && (result[1] != 0x50))
                            {
                                syncEventWriteRead.Set();
                            }
                        }
                        else if (done > 0)
                        {
                            Logger.Error("Write/Read synchronisation error between service and reader.");
                        }
                        else if (done < 0)
                        {
                            Logger.Error("Crazy CCID Command.");
                            offset = 0;
                            syncEventWriteRead.Set();
                        }
                    }

                }
                catch (Exception e)
                {
                    syncEventWriteRead.Set();
                    if (e is System.IO.IOException)
                    {
                        Logger.Debug("Socket has been closed");
                    }
                    else
                    {
                        Logger.Error("Error in receiver thread: " + e.Message);
                    }
                    break;
                }
            }

            clientSocket = null;
            Logger.Debug("Received thread ended");
        }

        /*protected override bool Send(byte endpoint, byte[] buffer)
        {
        	bool sync = false;
        	
        	if (SyncCommands)
        	{
        		if ((endpoint == CCID.EP_Bulk_PC_To_RDR) || (endpoint == CCID.EP_Control_To_RDR))
        			sync = true;
        	}
        	
        	return Send(endpoint, buffer, sync);
        }

        protected bool Send(byte endpoint, byte[] buffer, bool sync)*/
        protected override bool Send(byte endpoint, byte[] buffer)
        {
        	//Logger.Trace("\t" + BinConvert.ToHex(endpoint) + " < " + BinConvert.ToHex(buffer));
        	
			byte[] t = new byte[1 + buffer.Length];
			
			t[0] = endpoint;
			for (int i=0; i<buffer.Length; i++)
				t[1+i] = buffer[i];


            Logger.Trace("Send to " + ip_ID + " " + endpoint);

            /* lock due to multithread possible access */
            lock (sync_request)
            {
                try
                {
                    if (clientSocket.Connected)
                    {
                        if (syncWrite_Wait == true)
                        {
                            Logger.Trace("Busy <" + BinConvert.ToHex(t));
                            return true;
                        }
                        Logger.Trace("<" + BinConvert.ToHex(t));
                        clientStream.Write(t, 0, t.Length);
                        /* wait answer from reader */
                        syncWrite_Wait = true;
                        syncEventWriteRead.WaitOne();
                        syncWrite_Wait = false;
                        Logger.Trace("syncWrite_Wait OK");
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }

}
