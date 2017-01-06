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
using SpringCard.PCSC;

namespace scscriptorxv
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		string Title;
		
		public string ScriptFileName = "";

		SCardReaderList ReaderList = null;
		SCardReader Reader = null;
		SCardChannel Card = null;
		
		Thread ScriptThread = null;
		
		public MainForm()
		{
			InitializeComponent();
			
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
			
			FileVersionInfo i = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
			Text = i.CompanyName + " " + i.ProductName + " v. " + i.ProductVersion;
			Title = i.CompanyName + " " + i.ProductName;
			
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
			AppConfig.SaveFormAspect(this);
			AppConfig.WriteBoolean("stop_on_error", cbStopOnError.Checked);
			AppConfig.WriteBoolean("stop_on_status", cbStopOnStatus.Checked);
			if (rbHex.Checked) AppConfig.WriteString("result_mode", "hex"); else
				if (rbAscii.Checked) AppConfig.WriteString("result_mode", "ascii"); else AppConfig.WriteString("result_mode", "both");
			AppConfig.WriteBoolean("run_loop", cbLoop.Checked);
			AppConfig.WriteBoolean("run_auto", cbAutorun.Checked);
		}
		
		delegate void OnSerialReaderListInstantiatedInvoker(SCardReaderList_CcidOverSerial SerialReaders);
		void OnSerialReaderListInstantiated(SCardReaderList_CcidOverSerial SerialReaders)
		{
			if (InvokeRequired)
			{
				IAsyncResult ar = this.BeginInvoke(new OnSerialReaderListInstantiatedInvoker(OnSerialReaderListInstantiated), SerialReaders);
				this.EndInvoke(ar);
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

		delegate void OnNetworkReaderListInstantiatedInvoker(SCardReaderList_CcidOverNetwork NetworkReaders);
		void OnNetworkReaderListInstantiated(SCardReaderList_CcidOverNetwork NetworkReaders)
		{
			if (InvokeRequired)
			{
				IAsyncResult ar = this.BeginInvoke(new OnNetworkReaderListInstantiatedInvoker(OnNetworkReaderListInstantiated), NetworkReaders);
				this.EndInvoke(ar);
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
		
		void MainFormShown(object sender, EventArgs e)
		{
			if (AppConfig.ReadBoolean("reader_reconnect"))
			{
				if (AppConfig.ReadString("reader_mode") == "serial")
				{
					SCardReaderList_CcidOverSerial.BackgroundInstantiate(
						new SCardReaderList_CcidOverSerial.BackgroundInstantiateCallback(OnSerialReaderListInstantiated),
						AppConfig.ReadString("serial_port"),
						true,
						AppConfig.ReadBoolean("serial_use_lpcd"));
				}
				else
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
			
			DisplayReaderState(ReaderState, CardAtr);
			
			if (ReaderState == SCARD.STATE_UNAWARE)
			{
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
				if (Card != null)
				{
					AddToResult("\nCard removed\n", Color.Black, true);
					Card.Disconnect();
					Card = null;
				}
			}
			else if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
			{
				
			}
			else if ((ReaderState & SCARD.STATE_MUTE) != 0)
			{
				
			}
			else if ((ReaderState & SCARD.STATE_INUSE) != 0)
			{
				
			}
			else if ((ReaderState & SCARD.STATE_PRESENT) != 0)
			{
				if (Card == null)
				{
					/* New card -> leave edit mode */
					Card = Reader.GetChannel();
					
					if (Card.Connect())
					{
						AddToResult("\nCard inserted\n", Color.Black, true);
						if (cbAutorun.Checked)
						{
							if (!StartScript())
							{
								AddToResult("Autorun failed\n", Color.Red);
							}
						}
					}
					else
					{
						MessageBox.Show(this, "Failed to connect to the card in the reader. Please verify that you don't have another application running in the background, that tries to access to the smartcards in the same reader.", Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						Card = null;
					}
				}
				else
				{
					AddToResult("Application state error, please remove the card\n", Color.Orange);
				}
			}
		}
		
		public void SaveScriptToFile()
		{
			/* Getting user entry	*/
			string Script = richTextBoxScript.Text;
			char[] delimiter = new char[1];
			delimiter[0] = '\n';
			
			/* seperating apdus (delimiter = '\n')	*/
			string[] lines = Script.Split(delimiter);

			Stream writeFileStream = new FileStream(ScriptFileName, FileMode.OpenOrCreate, FileAccess.Write);
			StreamWriter writeFileStreamWriter = new StreamWriter(writeFileStream);
			
			foreach (string line in lines)
				writeFileStreamWriter.WriteLine(line);
			
			writeFileStreamWriter.Close();
			writeFileStream.Close();
			
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
				if (StartScript())
				{
					
				}
			}
		}
		
		public bool StartScript()
		{
			string scriptText = richTextBoxScript.Text;
			string[] scriptLines = scriptText.Split(new char[] { '\n' });
			
			List<CAPDU> capduList = new List<CAPDU>();
			
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
				
				/* Retrieve the C-APDU */
				CAPDU capdu;
				try
				{
					capdu = new CAPDU(command);
				}
				catch
				{
					AddToResult("Syntax error on line " + lineCount + "\n", Color.Red);
					return false;
				}
				command = "";
				
				capduList.Add(capdu);
			}
			
			if (capduList.Count == 0)
			{
				AddToResult("The script is empty\n", Color.Orange);
				return false;
			}
			
			RunScriptParams runParams = new RunScriptParams();

			runParams.card = Card;
			runParams.script = capduList;
			runParams.loop = cbLoop.Checked;
			runParams.stopOnSWnot9000 = cbStopOnStatus.Checked;
			runParams.stopOnPCSCError = cbStopOnError.Checked;
			runParams.showHex = rbHex.Checked || rbHexAndAscii.Checked;
			runParams.showAscii = rbAscii.Checked || rbHexAndAscii.Checked;
			
			this.Enabled = false;
			
			AddToResult("Execution starting at " + DateTime.Now.ToLongTimeString() + "\n\n");
			
			ScriptThread = new Thread(RunScript);
			ScriptThread.Start(runParams);
			return true;
		}
		
		private class RunScriptParams
		{
			public SCardChannel card;
			public List<CAPDU> script;
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
				foreach (CAPDU capdu in runParams.script)
				{
					AddToResult("\nCommand:");
					AddToResult(capdu.AsString(" "), Color.Blue);
					
					/* Perform the exchange */
					RAPDU rapdu = runParams.card.Transmit(capdu);
					
					/* Error during the exchange? */
					if (rapdu == null)
					{
						AddToResult("\nError " + BinConvert.ToHex(runParams.card.LastError) + "\n", Color.Red);
						AddToResult(runParams.card.LastErrorAsString + "\n", Color.Red);
						if (runParams.stopOnPCSCError)
							fatalError = true;
						runParams.loop = false;
						break;
					}
					
					/* Status word? */
					Color responseColor;
					bool responseError = false;
					if ((rapdu.SW == 0x9000) || (rapdu.SW == 0x9100) || (rapdu.SW == 0x91AF))
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
						AddToResult("\nResponse: (" + rapdu.Length + "B)\n");
						AddToResult(rapdu.AsString(" "), responseColor);
					}
					
					if (runParams.showAscii)
					{
						if ((rapdu.data != null) && (rapdu.data.Length > 0))
						{
							AddToResult("\nResponse (ASCII):\n");
							AddToResult(ConvertToString(rapdu.data.GetBytes()) + " / SW=" + BinConvert.ToHex(rapdu.SW), responseColor);
						}
						else if (!runParams.showHex)
						{
							AddToResult("\nResponse:\n");
							AddToResult("SW=" + BinConvert.ToHex(rapdu.SW), responseColor);
						}
					}
					
					AddToResult("\n(" + rapdu.SWString + ")\n", responseColor);
					
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
			AddToResult("\nExecution ended at " + DateTime.Now.ToLongTimeString() + "\n\n");
			
			this.Enabled = true;
		}
		
		public void AddToResult(string line)
		{
			AddToResult(line, Color.Black, false);
		}

		public void AddToResult(string line, Color color)
		{
			AddToResult(line, color, false);
		}
		
		delegate void AddToResultInvoker(string line, Color color, bool bold);
		public void AddToResult(string line, Color color, bool bold)
		{
			if (InvokeRequired)
			{
				IAsyncResult ar = this.BeginInvoke(new AddToResultInvoker(AddToResult), line, color, bold);
				this.EndInvoke(ar);
				return;
			}

			if (bold)
				richTextBoxResult.SelectionFont = new Font(richTextBoxResult.Font, FontStyle.Bold);
			else
				richTextBoxResult.SelectionFont = new Font(richTextBoxResult.Font, FontStyle.Regular);
			richTextBoxResult.SelectionColor = color;
			richTextBoxResult.AppendText(line);
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
				else
				{
					CloseDevice();
				}
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
					
				}
				else if (f.Mode == "serial")
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
				}
				else if (f.Mode == "network")
				{
					string Address;
					ushort Port;
					
					f.GetNetworkParameters(out Address, out Port);
					
					SCardReaderList_CcidOverNetwork.BackgroundInstantiate(
						new SCardReaderList_CcidOverNetwork.BackgroundInstantiateCallback(OnNetworkReaderListInstantiated),
						Address,
						Port);
				}
			}
		}


		void CbLoopCheckedChanged(object sender, EventArgs e)
		{
			
		}
		
		void AboutToolStripMenuItemClick(object sender, EventArgs e)
		{
			AboutForm f = new AboutForm();
			f.ShowDialog();
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
		
		void SaveToolStripMenuItemClick(object sender, EventArgs e)
		{
			string Script = richTextBoxScript.Text;
			
			/* Is there something to save?	*/
			if (Script != "")
			{
				if (ScriptFileName == "")
				{
					saveFileDialog.Filter = "Script files(*.txt)|*.txt|All files (*.*)|*.*";
					saveFileDialog.FilterIndex = 1;
					saveFileDialog.RestoreDirectory = true;
					saveFileDialog.DefaultExt = ".txt";
					saveFileDialog.ShowDialog();
				} else
				{
					SaveScriptToFile();
				}
			}
		}
		
		void SaveresultAsToolStripMenuItemClick(object sender, EventArgs e)
		{
			
		}
		
		void ClearResultToolStripMenuItemClick(object sender, EventArgs e)
		{
			richTextBoxResult.Text = "";
		}
		
	}
}
