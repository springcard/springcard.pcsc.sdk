/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 04/24/2015
 * Time: 13:34
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.Bluetooth;
using SpringCard.PCSC;
using SpringCard.PCSC.Forms;
using SpringCard.PCSC.ZeroDriver;

namespace scscriptorxv
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		string Title;

		public string ScriptFileName = "";
		private string ResultFileName = "";

		SCardReaderList ReaderList = null;
		SCardReader Reader = null;
		SCardChannel Card = null;

		Thread ScriptThread = null;

		private bool isSavingScript = false;
		private bool isSavingOutput = false;

        bool ShowSplash = true;

        class ScriptEntry
        {
            public enum TypeE
            {
                Transmit,
                Control
            };
            public TypeE Type;
            public byte[] Content;
        }

		public MainForm(string[] args)
		{
			InitializeComponent();

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string s = args[i].ToLower();
                    if ((s == "--no_splash") || (s == "-q"))
                    {
                        ShowSplash = false;
                    }
                }
            }

            if (Screen.PrimaryScreen.Bounds.Width < 1280)
			{
				Left = 0;
				Width = Screen.PrimaryScreen.Bounds.Width;
			}

			if (Screen.PrimaryScreen.Bounds.Height < 768)
			{
				Top = 0;
				Height = Screen.PrimaryScreen.Bounds.Width;
			}

			Text = AppUtils.ApplicationTitle(true);
			Title = AppUtils.ApplicationTitle(false);

			LoadConfig();

			DisplayReaderState(0, null);
			DisplayReaderAbsent();
		}

		void LoadConfig()
		{
			AppConfig.LoadFormAspect(this);
			cbStopOnError.Checked = AppConfig.ReadBoolean("stop_on_error");
			cbStopOnStatus.Checked = AppConfig.ReadBoolean("stop_on_status");
			if (AppConfig.ReadString("result_mode") == "hex") rbHex.Checked = true; else
				if (AppConfig.ReadString("result_mode") == "ascii") rbAscii.Checked = true; else rbHexAndAscii.Checked = true;
			cbLoop.Checked = AppConfig.ReadBoolean("run_loop");
			cbAutorun.Checked = AppConfig.ReadBoolean("run_auto");
		}

		void SaveConfig()
		{
			try
			{
				AppConfig.SaveFormAspect(this);
				AppConfig.WriteBoolean("stop_on_error", cbStopOnError.Checked);
				AppConfig.WriteBoolean("stop_on_status", cbStopOnStatus.Checked);
				if (rbHex.Checked) AppConfig.WriteString("result_mode", "hex");
				else
					if (rbAscii.Checked) AppConfig.WriteString("result_mode", "ascii"); else AppConfig.WriteString("result_mode", "both");
				AppConfig.WriteBoolean("run_loop", cbLoop.Checked);
				AppConfig.WriteBoolean("run_auto", cbAutorun.Checked);

			}
			catch (Exception E)
			{
				MessageBox.Show("Error : " + E.Message);
			}
		}

		void OnSerialReaderListInstantiated(SCardReaderList_CcidOver SerialReaders)
		{
			if (InvokeRequired)
			{
				Invoke(new SCardReaderList_CcidOver.BackgroundInstantiateCallback(OnSerialReaderListInstantiated), SerialReaders);
				return;
			}

			if (SerialReaders == null)
			{
				MessageBox.Show(this, "Failed to connect to the serial reader. Please check your hardware and your configuration, and click the 'Reader' link to try to re-open the reader, or to select another reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			SCardReader_CcidOver SerialReader = SerialReaders.GetReader(0);
			if (SerialReader == null)
			{
				MessageBox.Show(this, "The serial reader has not been correctly activated. Please click the 'Reader' link to try to re-open the reader, or to select another reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			ReaderList = SerialReaders;
			Reader = SerialReader;
			DisplayReaderPresent();

			SerialReader.StartMonitor(new SCardReader_CcidOver.StatusChangeCallback(ReaderStatusChanged));
		}

		void OnNetworkReaderListInstantiated(SCardReaderList_CcidOver NetworkReaders)
		{
			if (InvokeRequired)
			{
				Invoke(new SCardReaderList_CcidOver.BackgroundInstantiateCallback(OnNetworkReaderListInstantiated), NetworkReaders);
				return;
			}

			if (NetworkReaders == null)
			{
				MessageBox.Show(this, "Failed to connect to the network reader. Please check your hardware and your configuration, and click the 'Reader' link to try to re-open the reader, or to select another reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			SCardReader_CcidOver NetworkReader = NetworkReaders.GetReader(0);
			if (NetworkReader == null)
			{
				MessageBox.Show(this, "The network reader has not been correctly activated. Please click the 'Reader' link to try to re-open the reader, or to select another reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			ReaderList = NetworkReaders;
			Reader = NetworkReader;
			DisplayReaderPresent();

			NetworkReader.StartMonitor(new SCardReader_CcidOver.StatusChangeCallback(ReaderStatusChanged));
		}

		void OnBleReaderListInstantiated(SCardReaderList_CcidOver BleReaders)
		{
			Logger.Debug("BleReaderListInstantiated");
			
			if (InvokeRequired)
			{
				Invoke(new SCardReaderList_CcidOver.BackgroundInstantiateCallback(OnBleReaderListInstantiated), BleReaders);
				return;
			}

			if (BleReaders == null)
			{
				MessageBox.Show(this, "Failed to connect to the BLE reader. Please check your hardware and your configuration, and click the 'Reader' link to try to re-open the reader, or to select another reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			SCardReader_CcidOver BleReader = BleReaders.GetReader(0);
			if (BleReader == null)
			{
				MessageBox.Show(this, "The BLE reader has not been correctly activated. Please click the 'Reader' link to try to re-open the reader, or to select another reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			ReaderList = BleReaders;
			Reader = BleReader;
			DisplayReaderPresent();

			Logger.Debug("BleReader.StartMonitor");
			BleReader.StartMonitor(new SCardReader_CcidOver.StatusChangeCallback(ReaderStatusChanged));
		}
				
		void MainFormShown(object sender, EventArgs e)
		{
            if (ShowSplash)
            {
                Logger.Trace("Showing splash form");
                SplashForm.DoShowDialog(this, FormStyle.ModernRed);
            }

            if (AppConfig.ReadBoolean("reader_reconnect"))
			{
				if (AppConfig.ReadString("reader_mode") == "serial")
				{

				} else
				if (AppConfig.ReadString("reader_mode") == "network")
				{

				} else					
				if (AppConfig.ReadString("reader_mode") == "ble")
				{

				} else
				{
					ReaderList = new SCardReaderList();
					if (ReaderList != null)
					{
						string reader_name = AppConfig.ReadString("reader_name");
						Logger.Trace("Looking for PC/SC reader '" + reader_name + "'");
						if (ReaderList.Contains(reader_name))
						{
							Logger.Trace("Found!");
							Reader = new SCardReader(reader_name);
							DisplayReaderPresent();
							Reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
							return;
						}
						else
						{
							Logger.Trace("Not found!");
							MessageBox.Show(this,
							                "Failed to reconnect to the previous PC/SC reader. Please click the 'Reader' link to select another reader.",
							                Title, MessageBoxButtons.OK,
							                MessageBoxIcon.Exclamation);
						}
						ReaderList = null;
					}
					else
					{
						MessageBox.Show(this,
						                "The PC/SC subsystem doesn't seem to be running. Please check your system's configuration.",
						                Title,
						                MessageBoxButtons.OK,
						                MessageBoxIcon.Exclamation);
					}
				}
			}
			else
			{
				SelectReader();
			}
		}

		bool UserWantsToClose()
		{
			if (Reader != null)
				if (MessageBox.Show(this,
				                    "You are currently connected. Are you sure you want to disconnect from this reader now?",
				                    Title,
				                    MessageBoxButtons.YesNo,
				                    MessageBoxIcon.Question,
				                    MessageBoxDefaultButton.Button1) != DialogResult.Yes)
						return false;
			return true;
		}

		void CloseDevice()
		{
			if (Reader != null)
			{
				Logger.Trace("Release the Reader...");
				Reader.Release();
				Reader = null;
			}
			if (ReaderList != null)
			{
				Logger.Trace("Release the ReaderList...");
				ReaderList.Release();
				ReaderList = null;
			}

			DisplayReaderState(0, null);
			DisplayReaderAbsent();
		}

		void DisplayReaderPresent()
		{
            Logger.Debug("Looking for reader name...");
			lbReaderName.Text = Reader.Name;
			btnRun.Enabled = true;
		}

		void DisplayReaderAbsent()
		{
			lbReaderName.Text = "";
			lbReaderStatus.Text = "";
			lbCardAtr.Text = "";
			btnRun.Enabled = false;
		}

		void DisplayReaderState(uint ReaderState, CardBuffer CardAtr)
		{
			lbReaderStatus.Text = SCARD.ReaderStatusToString(ReaderState);

			if (CardAtr != null)
			{
				lbCardAtr.Text = CardAtr.AsString(" ");
			}
			else
			{
				lbCardAtr.Text = "";
			}

			bool runable = false;
			if ((ReaderState & SCARD.STATE_PRESENT) != 0)
				if ((ReaderState & SCARD.STATE_UNAVAILABLE) == 0)
					if ((ReaderState & SCARD.STATE_MUTE) == 0)
						runable = true;

			btnRun.Enabled = runable;
		}

		delegate void ReaderStatusChangedInvoker(uint ReaderState, CardBuffer CardAtr);
		void ReaderStatusChanged(uint ReaderState, CardBuffer CardAtr)
		{
			/* The ReaderStatusChanged function is called as a delegate (callback) by the SCardReader object    */
			/* within its backgroung thread. Therefore we must use the BeginInvoke syntax to switch back from   */
			/* the context of the background thread to the context of the application's main thread. Overwise   */
			/* we'll get a security violation when trying to access the window's visual components (that belong */
			/* to the application's main thread and can't be safely manipulated by background threads).         */
			if (InvokeRequired)
			{
				IAsyncResult ar = this.BeginInvoke(new ReaderStatusChangedInvoker(ReaderStatusChanged), ReaderState, CardAtr);
				this.EndInvoke(ar);
				return;
			}

            Logger.Debug("ReaderStatusChanged {0:X08}", ReaderState);

			DisplayReaderState(ReaderState, CardAtr);

            if ((ReaderState == SCARD.STATE_UNAWARE) || (ReaderState == SCARD.STATE_UNAVAILABLE))
            {
                Logger.Debug("Reader lost!");

				if (Card != null)
				{
					Card.Disconnect();
					Card = null;
				}
				if (Reader != null)
				{
					Reader.Release();
					Reader = null;
				}
				if (ReaderList != null)
				{
					ReaderList.Release();
					ReaderList = null;
				}

				MessageBox.Show(this,
				                "The reader we were working on has been removed from the system. Please click the 'Reader' link to select another reader.",
				                Title,
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation);

				DisplayReaderState(0, null);
				DisplayReaderAbsent();
				return;
			}

			if ((ReaderState & SCARD.STATE_EMPTY) != 0)
			{
                Logger.Debug("Empty");

                if (Card != null)
				{
					WriteLine("Card removed", Color.Black, true);
					if (Card.Connected)
						Card.Disconnect();
					Card = null;
				}
			}
			else if ((ReaderState & SCARD.STATE_MUTE) != 0)
			{
                Logger.Debug("Mute");
            }
			else if ((ReaderState & SCARD.STATE_INUSE) != 0)
			{
                Logger.Debug("In use");
            }
			else if ((ReaderState & SCARD.STATE_PRESENT) != 0)
			{
                Logger.Debug("Present");
                if (Card == null)
				{
					/* New card -> leave edit mode */
					Card = Reader.GetChannel();
                    WriteLine();
                    WriteLine("Card inserted", Color.Black, true);
					
					if (cbAutorun.Checked)
					{
						if (Card.Connect())
						{
                            WriteLine();
                            WriteLine("Card activated", Color.Black, true);
							if (!StartScript())
								WriteLine("Autorun failed", Color.Red);
						}
						else
						{
							MessageBox.Show(this, "Failed to connect to the card in the reader. Please verify that you don't have another application running in the background, that tries to access to the smartcards in the same reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
					}
				}
			}
		}

		private void LoadScriptFromFile()
		{
			Stream userFileStream = new FileStream(ScriptFileName, FileMode.Open, FileAccess.Read);
			StreamReader userFileStreamReader = new StreamReader(userFileStream);
			string userFileString = userFileStreamReader.ReadToEnd();
			richTextBoxScript.Text = userFileString;
			userFileStreamReader.Close();
			userFileStream.Close();
		}

		void BtnRunClick(object sender, EventArgs e)
		{
			if (Card != null)
			{
				if (!Card.Connected)
				{
					if (Card.Connect())
					{
                        WriteLine();
                        WriteLine("Card activated", Color.Black, true);
					}
					else
					{
						MessageBox.Show(this, "Failed to connect to the card in the reader. Please verify that you don't have another application running in the background, that tries to access to the smartcards in the same reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}
				}
				
				if (!StartScript())
					WriteLine("Run failed", Color.Red);
			}
		}

		public bool StartScript()
		{
			string scriptText = richTextBoxScript.Text;
			string[] scriptLines = scriptText.Split(new char[] { '\n' });

			List<ScriptEntry> script = new List<ScriptEntry>();

			int lineCount = 0;
			string command = "";

			foreach (string scriptLine in scriptLines)
			{
				string line = scriptLine;
				lineCount++;

				/* Skip comments	*/
				if (line.StartsWith("#"))
					continue;
				if (line.StartsWith(";"))
					continue;
				/* Skip blank lines	*/
				line = line.Replace(" ","");
				if (line.Equals(""))
					continue;
				/* If line ends with \, concatenation with next line	*/
				if (line.EndsWith("\\"))
				{
					command += line.Remove(line.Length - 1);
					continue;
				}
				command += line;

                ScriptEntry entry = new ScriptEntry();

                if (command.StartsWith("^"))
                {
                    /* This is a control */
                    entry.Type = ScriptEntry.TypeE.Control;
                    command = command.Substring(1);
                }
                else
                {
                    /* This is a transmit */
                    entry.Type = ScriptEntry.TypeE.Transmit;
                }

                /* Retrieve the C-APDU or the control command */
				try
				{
					entry.Content = BinConvert.HexToBytes(command);
				}
				catch
				{
					WriteLine("Syntax error on line " + lineCount, Color.Red);
					return false;
				}
				command = "";

				script.Add(entry);
			}

			if (script.Count == 0)
			{
				WriteLine("The script is empty", Color.Orange);
				return false;
			}

			RunScriptParams runParams = new RunScriptParams();

			runParams.card = Card;
			runParams.script = script;
			runParams.loop = cbLoop.Checked;
			runParams.stopOnSWnot9000 = cbStopOnStatus.Checked;
			runParams.stopOnPCSCError = cbStopOnError.Checked;
			runParams.showHex = rbHex.Checked || rbHexAndAscii.Checked;
			runParams.showAscii = rbAscii.Checked || rbHexAndAscii.Checked;

			this.Enabled = false;

            WriteLine();
            WriteLine("Execution starting at " + DateTime.Now.ToLongTimeString());

            ScriptThread = new Thread(RunScript);
			ScriptThread.Start(runParams);
			return true;
		}

		private class RunScriptParams
		{
			public SCardChannel card;
			public List<ScriptEntry> script;
			public bool loop;
			public bool stopOnSWnot9000;
			public bool stopOnPCSCError;
			public bool showHex;
			public bool showAscii;
		}

		private void RunScript(object threadParams)
		{
			bool fatalError = false;
			RunScriptParams runParams = (RunScriptParams) threadParams;

			do
			{
				foreach (ScriptEntry entry in runParams.script)
				{
                    WriteLine();

                    if (entry.Type == ScriptEntry.TypeE.Transmit)
                    {
                        WriteLine("Command:");
                        WriteLine(BinConvert.ToHex_delim(entry.Content, " "), Color.Blue);
                    }
                    else if (entry.Type == ScriptEntry.TypeE.Control)
                    {
                        WriteLine("Control:");
                        WriteLine(BinConvert.ToHex_delim(entry.Content, " "), Color.DarkBlue);
                    }
                    else
                    { 
                        continue;
                    }

                    bool responseError = false;
                    Color responseColor;

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    /* Perform the exchange */                   
                    if (entry.Type == ScriptEntry.TypeE.Transmit)
                    {
                        /* Transmit */
                        /* -------- */

                        CAPDU capdu = new CAPDU(entry.Content);
                        RAPDU rapdu = null;
                        rapdu = runParams.card.Transmit(capdu);
                        stopWatch.Stop();

                        /* Error during the exchange? */
                        if (rapdu == null)
                        {
                            WriteLine("Error " + BinConvert.ToHex(runParams.card.LastError), Color.Red);
                            WriteLine(runParams.card.LastErrorAsString, Color.Red);
                            if (runParams.stopOnPCSCError)
                                fatalError = true;
                            runParams.loop = false;
                            break;
                        }

                        /* Status word */
                        if ((rapdu.SW & 0xF000) == 0x9000)
                        {
                            responseColor = Color.Green;
                        }
                        else if (rapdu.SW1 != 0x6F)
                        {
                            responseColor = Color.DarkOrange;
                            responseError = true;
                        }
                        else
                        {
                            responseColor = Color.Red;
                            responseError = true;
                        }

                        if (runParams.showHex)
                        {
                            WriteLine("Response: (" + rapdu.Length + "B)");
                            WriteLine(rapdu.AsString(" "), responseColor);
                        }

                        if (runParams.showAscii && (rapdu.data != null) && (rapdu.data.Length > 0))
                        {
                            WriteLine("Response (ASCII):");
                            WriteLine(ConvertToString(rapdu.data.GetBytes()), responseColor);
                        }

                        WriteLine("SW=" + BinConvert.ToHex(rapdu.SW) + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")", responseColor);
                    }
                    else if (entry.Type == ScriptEntry.TypeE.Control)
                    {
                        /* Control */
                        /* ------- */

                        byte[] response = null;
                        response = runParams.card.Control(entry.Content);
                        stopWatch.Stop();

                        /* Error during the exchange? */
                        if ((response == null) || (response.Length == 0))
                        {
                            WriteLine("Error " + BinConvert.ToHex(runParams.card.LastError), Color.Red);
                            WriteLine(runParams.card.LastErrorAsString, Color.Red);
                            if (runParams.stopOnPCSCError)
                                fatalError = true;
                            runParams.loop = false;
                            break;
                        }

                        if (response[0] == 0)
                        {
                            responseColor = Color.DarkGreen;
                        }
                        else
                        {
                            responseColor = Color.Red;
                            responseError = true;
                        }

                        if (runParams.showHex)
                        {
                            WriteLine("Response: (" + response.Length + "B)");
                            WriteLine(BinConvert.ToHex_delim(response, " "), responseColor);
                        }

                        if (runParams.showAscii && (response[0] == 0x00) && (response.Length > 1))
                        {
                            WriteLine("Response (ASCII):");
                            byte[] data = new byte[response.Length - 1];
                            Array.Copy(response, 1, data, 0, data.Length);
                            WriteLine(ConvertToString(data), responseColor);
                        }

                        WriteLine("Status=" + BinConvert.ToHex(response[0]), responseColor);
                    }

                    TimeSpan ts = stopWatch.Elapsed;
                    WriteLine(String.Format("{0}ms", ts.TotalMilliseconds), Color.Gray);

                    if (runParams.stopOnSWnot9000 && responseError)
					{
						runParams.loop = false;
						break;
					}
				}
			}
			while (runParams.loop);

			ScriptExited(fatalError);
		}

		private bool isASCIIChar(byte b)
		{
			if ((b < 0x20) || (b>= 0x7F))
				return false;

			return true;
		}

		private string getASCIIChar(byte b)
		{
			byte[] ab = new byte[1];
			ab[0] = b;
			return System.Text.Encoding.ASCII.GetString(ab);
		}

		private string ConvertToString(byte[] array)
		{
			string s = "";
			for (int i=0; i<array.Length; i++)
			{
				if (isASCIIChar(array[i]))
				{
					s += getASCIIChar(array[i]);
				} else
				{
					s += " ";
				}
			}
			return s;
		}

		delegate void ScriptExitedInvoker(bool fatalError);
		public void ScriptExited(bool fatalError)
		{
			if (InvokeRequired)
			{
				IAsyncResult ar = this.BeginInvoke(new ScriptExitedInvoker(ScriptExited), fatalError);
				this.EndInvoke(ar);
				return;
			}

			ScriptThread = null;

            WriteLine();
            WriteLine("Execution ended at " + DateTime.Now.ToLongTimeString());
            WriteLine();

            this.Enabled = true;
		}

        public void WriteLine()
        {
            WriteLine("");
        }

        public void WriteLine(string line)
		{
			WriteLine(line, Color.Black, false);
		}

		public void WriteLine(string line, Color color)
		{
			WriteLine(line, color, false);
		}

		delegate void WriteLineInvoker(string line, Color color, bool bold);
		public void WriteLine(string line, Color color, bool bold)
		{
			if (InvokeRequired)
			{
				IAsyncResult ar = this.BeginInvoke(new WriteLineInvoker(WriteLine), line, color, bold);
				this.EndInvoke(ar);
				return;
			}

			if (bold)
				richTextBoxResult.SelectionFont = new Font(richTextBoxResult.Font, FontStyle.Bold);
			else
				richTextBoxResult.SelectionFont = new Font(richTextBoxResult.Font, FontStyle.Regular);

			richTextBoxResult.SelectionColor = color;
			richTextBoxResult.AppendText(line + "\n");
			richTextBoxResult.ScrollToCaret();
		}

		void BtnCloseClick(object sender, EventArgs e)
		{
			CloseDevice();
		}

		void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if ((ReaderList != null) || (Reader != null))
			{
				if (!UserWantsToClose())
				{
					e.Cancel = true;
					return;
				}
				CloseDevice();
			}
			SaveConfig();
		}

		void LinkReaderLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SelectReader();
		}

		void SelectReader()
		{
			if ((ReaderList != null) || (Reader != null))
			{
				if (!UserWantsToClose())
					return;

				CloseDevice();
			}

			ReaderSelectAnyForm f = new ReaderSelectAnyForm();
			bool rc = (f.ShowDialog(this) == DialogResult.OK);

			if (rc)
			{
				if (f.Mode == "pc/sc")
				{
					string ReaderName;
					f.GetPCSCParameters(out ReaderName);

					ReaderList = new SCardReaderList();
					if (ReaderList != null)
					{
						Reader = ReaderList.GetReader(ReaderName);
						if (Reader != null)
						{
							DisplayReaderPresent();
							Reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
						}
						else
						{
							MessageBox.Show(this, "Failed to connect to the PC/SC reader. Please click the 'Reader' link to select another reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
					}
					else
					{
						MessageBox.Show(this, "The PC/SC subsystem doesn't seem to be running. Please check your system's configuration.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}

				} else
				if (f.Mode == "serial")
				{
					string PortName;
					bool UseNotifications;
					bool UseLpcdPolling;

					f.GetSerialParameters(out PortName, out UseNotifications, out UseLpcdPolling);

					SCardReaderList_CcidOverSerial.BackgroundInstantiate(
						new SCardReaderList_CcidOverSerial.BackgroundInstantiateCallback(OnSerialReaderListInstantiated),
						PortName,
						UseNotifications,
						UseLpcdPolling);
				} else
				if (f.Mode == "network")
				{
					string Address;
					ushort Port;

					f.GetNetworkParameters(out Address, out Port);

					SCardReaderList_CcidOverNetwork.BackgroundInstantiate(
						new SCardReaderList_CcidOverNetwork.BackgroundInstantiateCallback(OnNetworkReaderListInstantiated),
						Address,
						Port);
				} else
				if (f.Mode == "ble")
				{
					BLE.Adapter bleAdapter;
					BluetoothAddress deviceAddress;
					BluetoothUuid devicePrimaryService;

					f.GetBleParameters(out bleAdapter, out deviceAddress, out devicePrimaryService);
					
					SCardReaderList_CcidOverBle.BackgroundInstantiate(new SCardReaderList_CcidOverBle.BackgroundInstantiateCallback(OnBleReaderListInstantiated), bleAdapter, deviceAddress, devicePrimaryService);
				}
			}
		}


		void CbLoopCheckedChanged(object sender, EventArgs e)
		{

		}

		void AboutToolStripMenuItemClick(object sender, EventArgs e)
		{
            AboutForm.DoShowDialog(this, FormStyle.ModernRed);
		}

		void LinkLoadScriptLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenToolStripMenuItemClick(sender, e);
		}
		void LinkSaveScriptLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SaveresultAsToolStripMenuItemClick(sender, e);
		}
		void LinkClearResultLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ClearResultToolStripMenuItemClick(sender, e);
		}
		void LinkSaveResultLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SaveToolStripMenuItemClick(sender, e);
		}
		void NewToolStripMenuItemClick(object sender, EventArgs e)
		{
			richTextBoxScript.Text = "";
		}

		void OpenToolStripMenuItemClick(object sender, EventArgs e)
		{
			openFileDialog.Filter = "Script files(*.txt)|*.txt|All files (*.*)|*.*";
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			openFileDialog.DefaultExt = ".txt";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				ScriptFileName = openFileDialog.FileName;
				LoadScriptFromFile();
			}
		}

		/// <summary>
		/// Save script's content
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void SaveToolStripMenuItemClick(object sender, EventArgs e)
		{
			string Script = richTextBoxScript.Text.Trim();
			if(Script.Equals(""))
			{
				MessageBox.Show("There's nothing to save");
				return;
			}
			isSavingScript = true;
			if (ScriptFileName == "")
				saveFileDialog.ShowDialog();
			else
				SaveScriptToFile(ScriptFileName);
		}

		/// <summary>
		/// Save output result
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void SaveresultAsToolStripMenuItemClick(object sender, EventArgs e)
		{
			string output = richTextBoxResult.Text;
			if (richTextBoxResult.Text.Trim().Equals(""))
			{
				MessageBox.Show("There's nothing to save");
				return;
			}

			isSavingOutput = true;
			if (ResultFileName == "")
				saveFileDialog.ShowDialog();
			else
				SaveOutpoutToFile(ResultFileName);
		}

		void ClearResultToolStripMenuItemClick(object sender, EventArgs e)
		{
			richTextBoxResult.Text = "";
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void getAPDUFromLibraryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LibraryForm library = new LibraryForm();
			if(library.ShowDialog() != DialogResult.OK)
				return;
			string selectedItem = library.GetSelectedItem().Replace(":", " ").Trim();
			richTextBoxScript.Text = selectedItem;
		}

		/// <summary>
		/// Save current script to a file
		/// </summary>
		/// <param name="outputFileName"></param>
		public void SaveScriptToFile(string outputFileName)
		{
			File.WriteAllText(outputFileName, richTextBoxScript.Text);
			isSavingScript = false;
			ScriptFileName = outputFileName;
		}

		/// <summary>
		/// Save output result to a file
		/// </summary>
		/// <param name="outputFileName"></param>
		private void SaveOutpoutToFile(string outputFileName)
		{
			File.WriteAllText(outputFileName, richTextBoxScript.Text);
			isSavingOutput = false;
			ResultFileName = outputFileName;
		}

		/// <summary>
		/// When user selected to save a file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (isSavingScript)
				SaveScriptToFile(saveFileDialog.FileName);
			else
				SaveOutpoutToFile(saveFileDialog.FileName);
		}

		/// <summary>
		/// Save script AS
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string Script = richTextBoxScript.Text.Trim();
			if (Script.Equals(""))
			{
				MessageBox.Show("There's nothing to save");
				return;
			}
			isSavingScript = true;
			saveFileDialog.ShowDialog();
		}

		private void lbCardAtr_DoubleClick(object sender, EventArgs e)
		{
			Clipboard.SetText(lbCardAtr.Text);
			MessageBox.Show("ATR copied to clipboard");
		}

		private void readerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SelectReader();
		}

        private void btnIccPwrOn_Click(object sender, EventArgs e)
        {
            bool result = Card.Connect();
        }

        private void btnIccPwrOff_Click(object sender, EventArgs e)
        {
            Card.DisconnectUnpower();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
      
        }
    }
}
