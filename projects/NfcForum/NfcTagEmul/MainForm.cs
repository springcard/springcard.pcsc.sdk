/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using SpringCard.PCSC;
using SpringCard.NFC;
using SpringCard.LibCs;

namespace SpringCardApplication
{
	public partial class MainForm : Form
	{
		SCardReader reader = null;
		RtdControl control = null;
		
		bool card_available = false;
		bool emulation_running = false;
		bool auto_write = false;
		
		bool ready = false;
		
		public MainForm()
		{
			InitializeComponent();
			
			Settings s = new Settings();
			if (s.ShowConsole)
			{
				SystemConsole.Show();
				Trace.Listeners.Add(new ConsoleTraceListener());
			}
			miShowConsole.Checked = s.ShowConsole;
			
			ready = true;
		}
		
		public void ShowStatus()
		{
			rbType2.Enabled = !emulation_running;
			rbType4.Enabled = !emulation_running;
			cbNdefType.Enabled = emulation_running && card_available;
			pMain.Enabled = emulation_running && card_available;
			miWrite.Enabled = btnWrite.Enabled = emulation_running && card_available;
			miStart.Enabled = btnStart.Enabled = !emulation_running && !card_available;
			miStop.Enabled  = btnStop.Enabled  = emulation_running;
		}
		

		/*
		 * SetNfcMode
		 * ----------
		 * Configure the reader's NFC interface for NFC Forum Tag emulation
		 */
		void SetNfcMode(int type, bool silent)
		{
			string s;

			if ((type == 2) || (type == 4))
				s = String.Format("Failed to activate NFC Forum type {0} Tag emulation", type);
			else
				s = "Failed to leave NFC Tag emulation mode";
			
			/* 1st open a direct connection to the reader */
			/* ------------------------------------------ */
			
			SCardChannel channel = new SCardChannel(reader);
			channel.ShareMode = SCARD.SHARE_DIRECT;
			channel.Protocol = 0;
			
			if (!channel.Connect())
			{
				Trace.WriteLine("Connect error " + channel.LastError + " (" + channel.LastErrorAsString + ")");

				if (!silent)
				{
					MessageBox.Show("Direct connection to the reader has failed with error '" + channel.LastErrorAsString + "'.",
					                s,
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Exclamation);
				}
				return;
			}
			
			/* 2nd send a proprietary command within a SCardControl */
			/* ---------------------------------------------------- */

			byte[] command = null;
			
			switch (type)
			{
					/* Command to start the NFC card emulation */
					/* --------------------------------------- */

				case 2 :
					command = new byte[]{ 0x83, /* SPROX_NFC_FUNC */
						0x10, /* Control card emulation mode */
						0x02, /* Select NFC Forum type 2 Tag emulation */
						0x11  /* First activation */
					};
					break;
					
				case 4 :
					command = new byte[]{ 0x83, /* SPROX_NFC_FUNC */
						0x10, /* Control card emulation mode */
						0x04, /* Select NFC Forum type 4 Tag emulation */
						0x11  /* First activation */
					};
					break;
					
					/* Command to stop the NFC card emulation */
					/* -------------------------------------- */
					
				case 0 :
					command = new byte[]{ 0x83, /* SPROX_NFC_FUNC */
						0x10, /* Control card emulation mode */
						0x00, /* Stop card emulation */
						0x00
					};
					break;
					
					default :
						MessageBox.Show("Internal error.");
					return;
			}
			
			Trace.WriteLine("< " + new CardBuffer(command).AsString(" "));

			byte[] response = channel.Control(command);
			
			if ((response == null) || (response.Length == 0))
			{
				/* No response -> SCardControl error */
				/* --------------------------------- */
				
				Trace.WriteLine("Control error " + channel.LastError + " (" + channel.LastErrorAsString + ")");
				
				channel.Disconnect();
				if (!silent)
				{
					MessageBox.Show("The command has not been delivered to the reader, error is '" + channel.LastErrorAsString + "'.",
					                s,
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Exclamation);
				}
				
				return;
			}
			
			Trace.WriteLine("> " + new CardBuffer(response).AsString(" "));
			
			if (response[0] != 0)
			{
				/* Error code returned by the reader */
				/* --------------------------------- */
				
				channel.Disconnect();
				if (!silent)
				{
					MessageBox.Show("The command has not been executed by the reader, error code is '-" + response[0] + "'.",
					                s,
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Exclamation);
				}
				
				emulation_running = false;
				return;
			}
			
			/* Drive the LEDs and buzzer */
			/* ------------------------- */

			if (type > 0)
			{
				emulation_running = true;
				channel.Leds(1, 1, 1);
				channel.Buzzer(100);
			} else
			{
				emulation_running = false;
				channel.LedsDefault();
				channel.BuzzerDefault();
			}
			
			/* Done */
			/* ---- */
			
			channel.Disconnect();
			ShowStatus();
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
				this.BeginInvoke(new ReaderStatusChangedInvoker(ReaderStatusChanged), ReaderState, CardAtr);
				return;
			}
			
			eReaderStatus.Text = SCARD.ReaderStatusToString(ReaderState);
			
			if (CardAtr != null)
			{
				eCardAtr.Text = CardAtr.AsString(" ");
			} else
			{
				eCardAtr.Text = "";
			}
			
			if (ReaderState == SCARD.STATE_UNAWARE)
			{
//			  lbReaderStatus.Text = "";

				MessageBox.Show("The reader we were working on has disappeared from the system. This application will terminate now.",
				                "The reader has been removed",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation);

				Application.Exit();
				
			} else
				if ((ReaderState & SCARD.STATE_EMPTY) != 0)
			{
//			  lbReaderStatus.Text = "No card in the reader";
				card_available = false;
			} else
				if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
			{
//			  lbReaderStatus.Text = "Reader in use";
				card_available = false;
			} else
				if ((ReaderState & SCARD.STATE_MUTE) != 0)
			{
//			  lbReaderStatus.Text = "Card is mute";
				card_available = false;
			} else
				if ((ReaderState & SCARD.STATE_INUSE) != 0)
			{
//			  lbReaderStatus.Text = "Card in use";
				card_available = false;
			} else
				if ((ReaderState & SCARD.STATE_PRESENT) != 0)
			{
				//lbReaderStatus.Text = "Card ready";
				card_available = true;
				
				if (auto_write)
				{
					BtnWriteClick(null, null);
					auto_write = false;
				}
			}
			
			ShowStatus();
		}
		

		void MainFormShown(object sender, EventArgs e)
		{
			if (true)
			{
				SplashForm splash = new SplashForm();
				splash.ShowDialog();
			}
			
			ReaderSelectForm readerSelect = new ReaderSelectForm(imgHeader.BackColor);
			
			if (readerSelect.SelectedReader == null)
			{
				readerSelect.Preselect("SpringCard|NFC|tactless");
				for (;;)
				{
					readerSelect.ShowDialog();
					if (readerSelect.SelectedReader != null)
						break;
					
					if (MessageBox.Show("This application can't run without a reader. Do you want to leave the application now ?",
					                    "No reader selected",
					                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
					{
						Application.Exit();
						return;
					}
				}
			}
			
			eReaderName.Text = readerSelect.SelectedReader;
			eReaderStatus.Text = "";
			eCardAtr.Text = "";
			
			reader = new SCardReader(eReaderName.Text);
			
			Settings s = new Settings();
			
			if ((s.SelectedType >= 0) && (s.SelectedType < cbNdefType.Items.Count))
				cbNdefType.SelectedIndex = s.SelectedType;
			else
				cbNdefType.SelectedIndex = 0;
			CbNdefTypeSelectedIndexChanged(null, null);

			SetNfcMode(0, false);
			ShowStatus();
			
			reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
		}
		
		void AboutToolStripMenuItemClick(object sender, EventArgs e)
		{
			AboutForm f = new AboutForm(imgHeader.BackColor);
			f.ShowDialog();
		}
		
		void MainFormFormClosed(object sender, FormClosedEventArgs e)
		{
			if (reader != null)
			{
				reader.StopMonitor();
				reader = null;
			}
			
			Settings s = new Settings();
			s.SelectedType = cbNdefType.SelectedIndex;
			s.Save();
		}
		
		void ChangeReader()
		{
			if (reader != null)
			{
				reader.StopMonitor();
				reader = null;
			}
			
			eReaderStatus.Text = "";
			eCardAtr.Text = "";

			ReaderSelectForm readerSelect = new ReaderSelectForm(imgHeader.BackColor);
			readerSelect.SelectedReader = eReaderName.Text;
			readerSelect.ShowDialog();
			if (readerSelect.SelectedReader != null)
				eReaderName.Text = readerSelect.SelectedReader;
			
			reader = new SCardReader(eReaderName.Text);

			SetNfcMode(0, false);

			reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
		}
		
		void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (reader != null)
			{
				/* Stop NFC emulation when exiting */
				SetNfcMode(0, true);
			}
		}
		
		void imgLogoClick(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.springcard.com");
		}
		
		
		void BtnReaderChangeClick(object sender, System.EventArgs e)
		{
			ChangeReader();
		}
		
		void QuitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}
		
		
		
		void BtnWriteClick(object sender, EventArgs e)
		{
			if (control == null)
				return;
			
			NfcTag nfcTag = null;
			
			SCardChannel channel = new SCardChannel(reader);
			if (!channel.Connect())
			{
				MessageBox.Show("Failed to connect to the card.");
				return;
			}
			
			if (rbType2.Checked)
			{
				if (NfcTagType2.Recognize(channel))
				{
					Trace.WriteLine("This card is a NFC type 2 Tag");
					nfcTag = NfcTagType2.Create(channel);
				} else
				{
					Trace.WriteLine("This card is not a NFC type 2 Tag, sorry");
				}
			} else
				if (rbType4.Checked)
			{
				if (NfcTagType4.Recognize(channel))
				{
					Trace.WriteLine("This card is a NFC type 4 Tag");
					nfcTag  = NfcTagType4.Create(channel);
				} else
				{
					Trace.WriteLine("This card is not a NFC type 4 Tag, sorry");
				}
			}

			if (nfcTag == null)
			{
				channel.Disconnect();
				MessageBox.Show("This card is not supported. Are you sure the emulution mode is running?");
				return;
			}
			
			if (!nfcTag.IsFormatted())
			{
				channel.Disconnect();
				MessageBox.Show("The card is not formatted as a NFC Tag.");
				return;
			}
			
			nfcTag.Content.Clear();
			
			Ndef ndef = control.GetContent();
			
			nfcTag.Content.Add(ndef);
			
			if (!nfcTag.Write(true))
			{
				channel.Disconnect();
				MessageBox.Show("Failed to write the NFC Tag.");
				return;
			}
			
			channel.Disconnect();
		}
		
		
		void BtnStopClick(object sender, EventArgs e)
		{
			SetNfcMode(0, false);
			ShowStatus();
		}
		
		void BtnStartClick(object sender, EventArgs e)
		{
			if (rbType2.Checked)
			{
				auto_write = true;
				SetNfcMode(2, false);
			} else
				if (rbType4.Checked)
			{
				auto_write = true;
				SetNfcMode(4, false);
			}
			ShowStatus();
		}
		
		void CbNdefTypeSelectedIndexChanged(object sender, EventArgs e)
		{
			if (control != null)
			{
				pMain.Controls.Remove(control);
				control.Dispose();
				control = null;
			}
			
			switch (cbNdefType.SelectedIndex)
			{
				case 0:
					/* Text */
					control = new RtdTextControl();
					break;

				case 1:
					/* URI */
					control = new RtdUriControl();
					break;

				case 2:
					/* SmartPoster */
					control = new RtdSmartPosterControl();
					break;

				case 3:
					/* vCard */
					control = new RtdVCardControl();
					break;

				case 4:
					/* Arbitrary MIME Media (text) */
					control = new RtdMediaControl();
					break;

				case 5:
					/* WifiHandover */
					control = new RtdWifiHandoverControl();
					break;
										
				default :
					break;
			}
			
			if (control != null)
			{
				control.Dock = DockStyle.Fill;
				pMain.Controls.Add(control);
			}
		}
		
		void MiShowConsoleCheckedChanged(object sender, EventArgs e)
		{
			if (!ready)
				return;
			
			Settings s = new Settings();
			s.ShowConsole = miShowConsole.Checked;
			s.Save();
			if (miShowConsole.Checked)
			{
				SystemConsole.Show();
				Trace.Listeners.Add(new ConsoleTraceListener());
			} else
			{
				Trace.Listeners.Clear();
				SystemConsole.Free();
			}
		}
	}
}
