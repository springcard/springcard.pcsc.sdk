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
using System.Net.Sockets;
using System.Threading;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	public class SCardReaderList_CcidOverNetwork : SCardReaderList_CcidOver
	{
		TcpClient clientSocket;
		NetworkStream clientStream;
		Thread receiverThread;
        private object sync_request = new object();
        private byte[] recv_buffer;
        public bool SyncCommands = true;

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
                r.sendControl(CCID.SET_CONFIGURATION, 0x0000);
                r.WaitControl();
            }
			
			return r;
		}		
		
		public delegate void BackgroundInstantiateCallback(SCardReaderList_CcidOverNetwork instance);
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
			
			if (!sendControl(CCID.SET_CONFIGURATION, 0x0001, 0, bOptions))
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
                if (sendControl(CCID.SET_CONFIGURATION, 0x0000))
                	WaitControl();
            }
        }

        public void Stop()
        {
            StopDevice();
        }
        
        private void ReceiverThread()
        {
            int bytesRead = 0;           
            recv_buffer = new byte[1492];

            while (clientSocket.Connected)
            {
                try
                {
                    bytesRead = clientStream.Read(recv_buffer, 0, recv_buffer.Length);
                    if (bytesRead <= 0)
                        continue;
                    
                    if (!clientSocket.Connected)
                        break;
                    
                    byte[] buffer = new byte[bytesRead];
                    Array.Copy(recv_buffer, buffer, buffer.Length);

                    Recv(buffer);
                }
                catch (Exception e)
                {
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
			CommandSyncEvent.Set();
            clientSocket = null;
            Logger.Debug("Received thread ended");
        }
        
        protected override bool Send(byte endpoint, byte[] buffer)
        {
        	bool sync = false;
        	
        	if (SyncCommands)
        	{
        		if ((endpoint == CCID.EP_Bulk_PC_To_RDR) || (endpoint == CCID.EP_Control_To_RDR))
        			sync = true;
        	}
        	
        	return Send(endpoint, buffer, sync);
        }

        protected bool Send(byte endpoint, byte[] buffer, bool sync)
		{
        	Logger.Trace("\t" + BinConvert.ToHex(endpoint) + " < " + BinConvert.ToHex(buffer));
        	
			byte[] t = new byte[1 + buffer.Length];
			
			t[0] = endpoint;
			for (int i=0; i<buffer.Length; i++)
				t[1+i] = buffer[i];
			
			if (!clientSocket.Connected)
				return false;
			
			bool success = false;

			try
			{
				if (sync)
				{
					lock (sync_request)
					{
		           		CommandSyncEvent.Reset();
		           		clientStream.Write(t, 0, t.Length);
	           			success = CommandSyncEvent.WaitOne(CommandTimeout);
					}
				}
				else
				{
					clientStream.Write(t, 0, t.Length);
					success = true;
				}
            }
			catch (Exception e)
            {
            	Logger.Error("Error while sending to device: " + e.Message);
            	success = false;
            }

            if (!success)
            {
            	Logger.Error("Device error");
            	clientSocket.Close();
            	return false;
            }

            return true;
		}        
	}
}
