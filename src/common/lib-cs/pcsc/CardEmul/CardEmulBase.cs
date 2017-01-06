/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:03
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Threading;
using System.Diagnostics;

namespace SpringCard.PCSC
{
	public abstract class CardEmulBase
	{
		public const byte NFC_EVENT_SELECT = 0x01;
		public const byte NFC_EVENT_C_APDU_READY = 0x02;
		public const byte NFC_EVENT_R_APDU_DONE = 0x03;
		public const byte NFC_EVENT_DESELECT = 0x04;
		
		private SCardChannel ReaderChannel = null;
		private string ReaderName = "";
		
		protected bool IsFirstCommand = true;
		protected bool UseApdus = true;
		
		private Thread CoreThread = null;
		volatile bool CoreRunning = false;
		
		public bool trace = false;
		public bool debug = false;
		
		public delegate void ErrorCallback();
		public ErrorCallback OnError = null;
		
		public delegate void StatusChangedCallback();
		public StatusChangedCallback OnCardSelected = null;
		public StatusChangedCallback OnCardDeselected = null;

		public delegate void CommandReceivedCallback(CardBuffer Command);
		public CommandReceivedCallback OnCommandReceived = null;
		
		public delegate void ResponseReadyCallback(CardBuffer Response);
		public ResponseReadyCallback OnResponseReady = null;
		
		public ushort EventTimeoutMs = 600;
		public ushort MaxIdleEventCount = 100;
		
		public CardEmulBase()
		{

		}

		~CardEmulBase()
		{
			RestoreReader();
			ReaderChannel = null;
		}
		
		private bool PrepareReader()
		{
			if (ReaderChannel != null)
			{
				ReaderChannel.Disconnect();
				ReaderChannel = null;
			}
			
			if (ReaderName.Equals(""))
				return false;
			
			ReaderChannel = new SCardChannel(ReaderName);
			
			ReaderChannel.ProtocolAsString = "DIRECT";
			
			if (!ReaderChannel.Connect())
			{
				if (trace)
				{
					Trace.WriteLine("Failed to connect to the reader");
					Trace.WriteLine("\terr." + ReaderChannel.LastError + " : " + ReaderChannel.LastErrorAsString);
				}
				ReaderChannel = null;
				return false;
			}
			
			if (!EmulStart())
			{
				if (trace)
					Trace.WriteLine("Failed to enter NFC emulation mode");
				EmulStop();
				ReaderChannel.Disconnect();
				ReaderChannel = null;
				return false;
			}
			
			DriveLeds(false);
			DriveBuzzer(true);
			
			return true;
		}
		
		private void RestoreReader()
		{
			EmulStop();
			if (ReaderChannel != null)
			{
				ReaderChannel.Disconnect();
				ReaderChannel = null;
			}
		}
		
		public bool Running()
		{
			return CoreRunning;
		}

		public void Stop()
		{
			CoreRunning = false;
			
			if (CoreThread != null)
			{
				System.Threading.Thread.Sleep(600);
				CoreThread.Abort();
				CoreThread.Join();
				CoreThread = null;
			}
			
			RestoreReader();
		}

		public bool Start(string reader)
		{
			Stop();
			
			ReaderName = reader;

			if (!PrepareReader())
				return false;
			
			CoreThread = new Thread(Core);
			CoreRunning = true;
			CoreThread.Start();
			
			return true;
		}

		private void Core()
		{
			bool FatalError = false;
			
			try
			{
				int NoEventCounter = 0;
				bool ResetCoupler = false;
				
				if (ReaderChannel == null)
				{
					return;
				}
				
				while (CoreRunning && !FatalError)
				{
					byte evtcode = 0;
					byte evtflags = 0;
					
					if (!WaitEvent(ref evtcode, ref evtflags, EventTimeoutMs))
					{
						if (trace)
							Trace.WriteLine("Failed to get next event");
						FatalError = true;
						break;
					}
					
					if (evtcode == 0)
					{
						NoEventCounter++;
					} else
					{
						NoEventCounter = 0;
					}
					
					switch (evtcode)
					{
						case NFC_EVENT_SELECT :
							Trace.WriteLine("Select");
							DriveLeds(true);
							OnCardSelect();
							if (OnCardSelected != null)
								OnCardSelected();
							break;
							
						case NFC_EVENT_C_APDU_READY :
							Trace.WriteLine("C-APDU ready");
							
							CardBuffer inBuffer = null;
							
							if (!PeekCommand(ref inBuffer))
							{
								if (debug)
									Trace.WriteLine("Failed to get the C-APDU");
								ResetCoupler = true;
								break;
							}
							
							if (debug)
								Trace.WriteLine("C> " + inBuffer.AsString());
							
							if (OnCommandReceived != null)
								OnCommandReceived(inBuffer);
							
							CardBuffer outBuffer = Process(inBuffer);
							
							if (debug)
								Trace.WriteLine("R< " + outBuffer.AsString());
							
							if (OnResponseReady != null)
								OnResponseReady(outBuffer);
							
							if (!PokeResponse(outBuffer))
							{
								if (debug)
									Trace.WriteLine("Failed to put the R-APDU");
								ResetCoupler = true;
							}
							break;
							
						case NFC_EVENT_R_APDU_DONE :
							if (debug)
								Trace.WriteLine("R-APDU done");
							break;
							
						case NFC_EVENT_DESELECT :
							if (debug)
								Trace.WriteLine("Deselect");
							DriveLeds(false);
							OnCardDeselect();
							if (OnCardDeselected != null)
								OnCardDeselected();
							break;
							
						case 0 :
							if (debug)
								Trace.WriteLine("No event...");
							break;
							
							default :
								if (debug)
								Trace.WriteLine("Unsupported event: " + evtcode);
							break;
					}
					
					if (NoEventCounter > MaxIdleEventCount)
						ResetCoupler = true;
					
					if (ResetCoupler)
					{
						ResetCoupler = false;
						if (!EmulStart())
							FatalError = true;
					}
				}
			}
			
			catch (Exception e)
			{
				if (trace)
				{
					Trace.WriteLine("Exception caught:");
					Trace.WriteLine(e.Message);
				}
			}
			
			try
			{
				RestoreReader();
			}
			catch (Exception)
			{
				
			}
			
			if (CoreRunning)
			{
				CoreRunning = false;
				if (OnError != null)
					OnError();
			}
		}
		
		private bool DriveLeds(bool active)
		{
			if (ReaderChannel == null)
				return false;

			CardBuffer RESP;
			CardBuffer CTRL;
			
			if (active)
				CTRL = new CardBuffer("581E010101");
			else
				CTRL = new CardBuffer("581E050505");
			
			RESP = ReaderChannel.Control(CTRL);
			
			if ((RESP == null) || (RESP.Length < 1) || (RESP.GetByte(0) != 0x00))
				return false;
			
			return true;
		}

		private bool DriveBuzzer(bool active)
		{
			if (ReaderChannel == null)
				return false;

			CardBuffer RESP;
			CardBuffer CTRL;
			
			if (active)
				CTRL = new CardBuffer("581C0040");
			else
				CTRL = new CardBuffer("581C0200");
			
			RESP = ReaderChannel.Control(CTRL);
			
			if ((RESP == null) || (RESP.Length < 1) || (RESP.GetByte(0) != 0x00))
				return false;
			
			return true;
		}

		private bool EmulStart()
		{
			if (ReaderChannel == null)
				return false;
			
			if (trace)
				Trace.WriteLine("Starting NFC card emulation");
			
			CardBuffer RESP;
			CardBuffer CTRL = new CardBuffer("83100100");
			
			RESP = ReaderChannel.Control(CTRL);
			if ((RESP == null) || (RESP.Length < 1) || (RESP.GetByte(0) != 0x00))
			{
				if (trace)
				{
					Trace.WriteLine("Control(" + CTRL.AsString() + ") failed");
					if (RESP == null)
						Trace.WriteLine("\terr." + ReaderChannel.LastError + " : " + ReaderChannel.LastErrorAsString);
					else
						Trace.WriteLine("\tResp= " + RESP.AsString());
				}

				return false;
			}

			if (debug)
				Trace.WriteLine("=");
			return true;
		}

		private bool EmulStop()
		{
			if (ReaderChannel == null)
				return false;
			
			if (trace)
				Trace.WriteLine("Stopping NFC card emulation");
			
			CardBuffer RESP;
			CardBuffer CTRL_LED = new CardBuffer("581E");
			CardBuffer CTRL_BUZ = new CardBuffer("581C");
			CardBuffer CTRL_NFC = new CardBuffer("83100000");
			
			RESP = ReaderChannel.Control(CTRL_NFC);
			if ((RESP == null) || (RESP.Length < 1) || ((RESP.GetByte(0) != 0x00) && (RESP.GetByte(0) != 0x1D)))
			{
				if (trace)
				{
					Trace.WriteLine("Control(" + CTRL_NFC.AsString() + ") failed");
					if (RESP == null)
						Trace.WriteLine("\terr." + ReaderChannel.LastError + " : " + ReaderChannel.LastErrorAsString);
					else
						Trace.WriteLine("\tResp= " + RESP.AsString());
				}

				return false;
			}

			ReaderChannel.Control(CTRL_LED);
			ReaderChannel.Control(CTRL_BUZ);

			return true;
		}
		
		public bool GetEvent(ref byte evtcode, ref byte evtflags)
		{
			if (ReaderChannel == null)
				return false;

			CardBuffer RESP;
			CardBuffer CTRL = new CardBuffer("8300");
			
			RESP = ReaderChannel.Control(CTRL);
			if ((RESP == null) || (RESP.Length < 3) || (RESP.GetByte(0) != 0x00))
			{
				Trace.WriteLine("Control(" + CTRL.AsString() + ") failed");

				if (RESP == null)
					Trace.WriteLine("\terr." + ReaderChannel.LastError + " : " + ReaderChannel.LastErrorAsString);
				else
					Trace.WriteLine("\tResp= " + RESP.AsString());

				return false;
			}
			
			evtcode = RESP.GetByte(1);
			evtflags = RESP.GetByte(2);
			
			return true;
		}

		public bool WaitEvent(ref byte evtcode, ref byte evtflags, ushort timeout_ms)
		{
			CardBuffer RESP;
			
			byte[] _ctrl = new byte[4];
			
			_ctrl[0] = 0x83;
			_ctrl[1] = 0x00;
			_ctrl[2] = (byte) (timeout_ms / 0x0100);
			_ctrl[3] = (byte) (timeout_ms % 0x0100);
			
			CardBuffer CTRL = new CardBuffer(_ctrl);
			
			RESP = ReaderChannel.Control(CTRL);
			if ((RESP == null) || (RESP.Length < 3) || (RESP.GetByte(0) != 0x00))
			{
				Trace.WriteLine("Control(" + CTRL.AsString() + ") failed");

				if (RESP == null)
					Trace.WriteLine("\terr." + ReaderChannel.LastError + " : " + ReaderChannel.LastErrorAsString);
				else
					Trace.WriteLine("\tResp= " + RESP.AsString());

				return false;
			}

			evtcode = RESP.GetByte(1);
			evtflags = RESP.GetByte(2);
			
			return true;
		}
		
		private bool PeekCommand(ref CardBuffer inBuffer)
		{
			CardBuffer RESP;
			CardBuffer CTRL = new CardBuffer("84");
			
			RESP = ReaderChannel.Control(CTRL);
			if ((RESP == null) || (RESP.Length <= 1) || (RESP.GetByte(0) != 0x00))
			{
				Trace.WriteLine("Control(" + CTRL.AsString() + ") failed");

				if (RESP == null)
					Trace.WriteLine("\terr." + ReaderChannel.LastError + " : " + ReaderChannel.LastErrorAsString);
				else
					Trace.WriteLine("\tResp= " + RESP.AsString());

				return false;
			}

			byte[] buffer = new byte[RESP.Length - 1];
			
			for (int i=0; i<RESP.Length - 1; i++)
				buffer[i] = RESP.GetByte(i + 1);
			
			inBuffer = new CardBuffer(buffer);
			return true;
		}
		
		private bool PokeResponse(CardBuffer outBuffer)
		{
			CardBuffer RESP;
			CardBuffer CTRL;
			
			byte[] buffer = new byte[outBuffer.Length + 1];
			buffer[0] = 0x84;
			for (int i=0; i<outBuffer.Length; i++)
				buffer[i + 1] = outBuffer.GetByte(i);
			
			CTRL = new CardBuffer(buffer);
			
			RESP = ReaderChannel.Control(CTRL);
			if ((RESP == null) || (RESP.Length < 1) || (RESP.GetByte(0) != 0x00))
			{
				Trace.WriteLine("Control(" + CTRL.AsString() + ") failed");

				if (RESP == null)
					Trace.WriteLine("\terr." + ReaderChannel.LastError + " : " + ReaderChannel.LastErrorAsString);
				else
					Trace.WriteLine("\tResp= " + RESP.AsString());

				return false;
			}

			return true;
		}
		
		public CardBuffer Process(CardBuffer inBuffer)
		{
			CardBuffer outBuffer;

			if (UseApdus)
			{
				try
				{
					/* Transform incoming frame into an APDU */
					CAPDU capdu = new CAPDU(inBuffer.GetBytes());
					/* Process the APDU */
					outBuffer = OnApdu(new CAPDU(inBuffer.GetBytes()));
				} catch (Exception)
				{
					/* Incoming frame is not a valid APDU, return 6700 */
					outBuffer = new RAPDU(0x67, 0x00);
				}
				
			} else
			{
				/* Process the frame */
				outBuffer = OnFrame(inBuffer);
			}
			
			IsFirstCommand = false;

			return outBuffer;
		}

		protected virtual CardBuffer OnFrame(CardBuffer cmd)
		{
			return new RAPDU(0x67, 00);
		}
		
		protected abstract RAPDU OnApdu(CAPDU capdu);
		protected abstract void OnCardSelect();
		protected abstract void OnCardDeselect();
	}
}
