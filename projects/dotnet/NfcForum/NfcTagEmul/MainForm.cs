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
using SpringCard.NfcForum;
using SpringCard.LibCs;
using SpringCardApplication.Controls;
using SpringCard.LibCs.Windows;
using SpringCard.PCSC.Forms;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.NfcForum.Tags;
using SpringCard.NfcForum.Ndef;
using SpringCard.PCSC.ReaderHelper;

namespace SpringCardApplication
{
	public partial class MainForm : Form
	{
		SCardReader reader = null;
		RtdControl control = null;
        enum Running { Unknown, True, False };
        Running running;

        bool ShowSplash = true;

        bool card_available = false;
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
			
			Text = AppUtils.ApplicationTitle(true);
			
			ready = true;
		}
		
		public void ShowStatus()
		{
			cbNdefType.Enabled = (running == Running.True) && card_available;
			pMain.Enabled = (running == Running.True) && card_available;
			miWrite.Enabled = btnWrite.Enabled = (running == Running.True) && card_available;
			miStart.Enabled = btnStart.Enabled = (running != Running.True) && !card_available;
			miStop.Enabled  = btnStop.Enabled  = (running != Running.False);
		}
		



		/*
		 * SetNfcMode
		 * ----------
		 * Configure the reader's NFC interface for NFC Forum Tag emulation
		 */
		void SetNfcMode(bool t4t_active, bool silent)
		{
			string s;

			if (t4t_active)
				s = String.Format("Failed to activate NFC Forum type {0} Tag emulation", 4);
			else
				s = "Failed to leave NFC Tag emulation mode";

            ReaderHelper readerHelper = new ReaderHelper(reader);

            if (t4t_active)
            {
                /* Listener */
                if (!readerHelper.NfcListener(0x34))
                {
                    running = Running.Unknown;
                    MessageBox.Show(this, "Failed to activate tag emulation.", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    running = Running.True;
                }
            }
            else
            {
                /* Poller */
                if (!readerHelper.NfcPoller())
                {
                    running = Running.Unknown;
                    MessageBox.Show(this, "Failed to set the device back in reader mode.", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    running = Running.False;
                }
            }

			
            /* Done */
            /* ---- */

            readerHelper.Disconnect();
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

				MessageBox.Show(this,
					            "The reader we were working on has disappeared from the system. This application will terminate now.",
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
            string readerName = null;

            if (ShowSplash)
            {
                Logger.Trace("Showing splash form");
                SplashForm.DoShowDialog(this, FormStyle.ModernMarroon);
            }

            if (AppConfig.ReadBoolean("reader_reconnect", false))
            {
                readerName = AppConfig.ReadString("reader_name", "");
                if (!SCARD.ReaderExists(readerName))
                    readerName = null;
            }

            while (string.IsNullOrEmpty(readerName))
            {
                ReaderSelectForm f = new ReaderSelectForm();
                f.Preselect("SpringCard|NFC|tactless");
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (f.SelectedReader != null)
                    {
                        readerName = f.SelectedReader;
                        break;
                    }
                }

                if (MessageBox.Show(this,
                                    "This application can't run without a reader. Do you want to leave the application now ?",
                                    "No reader selected",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    Application.Exit();
                    return;
                }
            }
			
			eReaderName.Text = readerName;
			eReaderStatus.Text = "";
			eCardAtr.Text = "";
			
			reader = new SCardReader(readerName);
			
			Settings s = new Settings();
			
			if ((s.SelectedType >= 0) && (s.SelectedType < cbNdefType.Items.Count))
				cbNdefType.SelectedIndex = s.SelectedType;
			else
				cbNdefType.SelectedIndex = 0;
			CbNdefTypeSelectedIndexChanged(null, null);

			SetNfcMode(false, false);
			
			reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
		}
		
		void AboutToolStripMenuItemClick(object sender, EventArgs e)
		{
            AboutForm.DoShowDialog(this);
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

			SetNfcMode(false, false);

			reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
		}
		
		void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (reader != null)
			{
				/* Stop NFC emulation when exiting */
				SetNfcMode(false, true);
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
				MessageBox.Show(this, "Failed to connect to the card.");
				return;
			}
			
			if (NfcTagType4.Recognize(channel))
			{
				Trace.WriteLine("This card is a NFC type 4 Tag");
				nfcTag  = NfcTagType4.Create(channel);
			} else
			{
				Trace.WriteLine("This card is not a NFC type 4 Tag, sorry");
			}

			if (nfcTag == null)
			{
				channel.Disconnect();
				MessageBox.Show(this, "This card is not supported. Are you sure the emulution mode is running?");
				return;
			}
			
			if (!nfcTag.IsFormatted())
			{
				channel.Disconnect();
				MessageBox.Show(this, "The card is not formatted as a NFC Tag.");
				return;
			}
			
			nfcTag.Content.Clear();

            NdefObject ndef = control.GetContent();
			
			nfcTag.Content.Add(ndef);
			
			if (!nfcTag.Write(true))
			{
				channel.Disconnect();
				MessageBox.Show(this, "Failed to write the NFC Tag.");
				return;
			}
			
			channel.Disconnect();
		}
		
		
		void BtnStopClick(object sender, EventArgs e)
		{
			SetNfcMode(false, false);
		}
		
		void BtnStartClick(object sender, EventArgs e)
		{
			auto_write = true;
			SetNfcMode(true, false);
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
                SystemConsole.Hide();
			}
		}
	}
}
