using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SpringCard.PCSC;
using SpringCard.LibCs;
using SpringCard.NFC;

namespace SpringCardApplication
{
	public partial class MainForm : Form
	{		
		public ArrayList ProcessActions = new ArrayList();
		
		SCardReader reader = null;
		SCardChannel cardchannel = null;
		NfcTag tag = null;
		Thread cardthread;
		
		RtdControl control = null;
		
		TypeSelectButton VCardButton;
		TypeSelectButton SmartPosterButton;
		TypeSelectButton UriButton;
		TypeSelectButton TextButton;
		TypeSelectButton MediaButton;
		//TypeSelectButton WifiHandoverButton;
		
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
			miEnableLock.Checked = s.EnableLock;
			cbLock.Visible = s.EnableLock;

			/* WIFI HANDOVER */
			/* ------------- */
			
			/*WifiHandoverButton = new TypeSelectButton("Wifi Handover");
			WifiHandoverButton.Dock = DockStyle.Top;
			pLeft.Controls.Add(WifiHandoverButton);
			WifiHandoverButton.OnSelected = new System.EventHandler(OnWifiHandoverSelected);*/
			
			/* VCARD */
			/* ----- */
			
			VCardButton = new TypeSelectButton("vCard");
			VCardButton.Dock = DockStyle.Top;
			pLeft.Controls.Add(VCardButton);
			VCardButton.OnSelected = new System.EventHandler(OnVCardSelected);

			/* MEDIA */
			/* ----- */
			
			MediaButton = new TypeSelectButton("MIME Media");
			MediaButton.Dock = DockStyle.Top;
			pLeft.Controls.Add(MediaButton);
			MediaButton.OnSelected = new System.EventHandler(OnMediaSelected);

			/* TEXT */
			/* ---- */
			
			TextButton = new TypeSelectButton("Text");
			TextButton.Dock = DockStyle.Top;
			pLeft.Controls.Add(TextButton);
			TextButton.OnSelected = new System.EventHandler(OnTextSelected);

			/* URI */
			/* --- */
			
			UriButton = new TypeSelectButton("URI");
			UriButton.Dock = DockStyle.Top;
			pLeft.Controls.Add(UriButton);
			UriButton.OnSelected = new System.EventHandler(OnUriSelected);

			/* SMARTPOSTER */
			/* ----------- */

			SmartPosterButton = new TypeSelectButton("SmartPoster");
			SmartPosterButton.Dock = DockStyle.Top;
			pLeft.Controls.Add(SmartPosterButton);
			SmartPosterButton.OnSelected = new System.EventHandler(OnSmartPosterSelected);

			
			/* Default is URI */
			SelectUri();
			
			Trace.WriteLine("Starting up");
			
			ready = true;
		}

		
		private void menuItemQuit_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void setEditable(bool yes)
		{
			btCreateNfcTag.Visible = false;
			btCreateNfcTag.Enabled = false;
			btnWrite.Visible = true;
			btnWrite.Enabled = yes;
			cbLock.Enabled = yes;
			if (control != null)
				control.SetEditable(yes);
		}
		

		public void setReader(string readerName)
		{
			Trace.WriteLine("Selecting reader: " + readerName);
			
			eReaderName.Text = readerName;
			eReaderStatus.Text = "";
			eCardAtr.Text = "";
			
			reader = new SCardReader(readerName);
			reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
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
				if (cardchannel != null)
				{
					cardchannel.Disconnect();
					cardchannel = null;
				}
				
				MessageBox.Show("The reader we were working on has disappeared from the system. This application will terminate now.",
				                "The reader has been removed",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation);
				
				Application.Exit();
				
			} else
				if ((ReaderState & SCARD.STATE_EMPTY) != 0)
			{
				Trace.WriteLine("Reader: EMPTY");
				
				if (cardchannel != null)
				{
					cardchannel.Disconnect();
					cardchannel = null;
				}
				
				/* No card -> leave edit mode */
				setEditable(false);
				
			} else
				if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
			{
				Trace.WriteLine("Reader: UNAVAILABLE");
			} else
				if ((ReaderState & SCARD.STATE_MUTE) != 0)
			{
				Trace.WriteLine("Reader: MUTE");
			} else
				if ((ReaderState & SCARD.STATE_INUSE) != 0)
			{
				Trace.WriteLine("Reader: INUSE");
			} else
				if ((ReaderState & SCARD.STATE_PRESENT) != 0)
			{
				Trace.WriteLine("Reader: PRESENT");
				
				if (cardchannel == null)
				{
					/* New card -> leave edit mode */
					setEditable(false);
					
					cardchannel = new SCardChannel(reader);
					
					if (cardchannel.Connect())
					{
						Trace.WriteLine("Connected to the card");
						cardthread = new Thread(card_read_proc);
						cardthread.Start();
						
					} else
					{
						Trace.WriteLine("Connection to the card failed");

						MessageBox.Show("NfcTool failed to connect to the card in the reader. Check that you don't have another application running in background that tries to work with the smartcards in the same time as NFCTool");		

						cardchannel = null;
					}
				}
				
				
				//card_status = 1;
			}
		}
		
		delegate void OnErrorInvoker(string text, string caption);
		void OnError(string text, string caption)
		{
			Trace.WriteLine("Error: " + text);			
			MessageBox.Show(text, caption);
		}
		
		delegate void OnTagWriteInvoker(NfcTag _tag);
		void OnTagWrite(NfcTag _tag)
		{
			MessageBox.Show("Writing tag OK", "Operation succesful", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Trace.WriteLine("Write OK");
		}
		
		delegate void OnTagReadInvoker(NfcTag _tag);
		void OnTagRead(NfcTag _tag)
		{
			tag = _tag;
			
			Trace.WriteLine("Read terminated");
			
			if (tag == null)
			{
				MessageBox.Show("Internal error, tag is null!");
				return;
			}
			
			if ((tag.Content == null) || (tag.Content.Count == 0))
			{
				if (!tag.IsLocked())
				{
					MessageBox.Show("The Tag has no valid content yet. You may create your own content and write it onto the tag", "This NFC Tag is empty");
					setEditable(true);
				} else
				{
					MessageBox.Show("The Tag has no valid content, but is not writable", "This NFC Tag is empty");
				}
			} else
			{
				Unselect();
				
				for (int i=0; i<tag.Content.Count; i++)
				{
					/* Display the first record we support in the tag's content */
					Ndef ndef = tag.Content[i];
					
					if (ndef is RtdSmartPoster)
					{
						SelectSmartPoster();
					} else
						if (ndef is RtdUri)
					{
						SelectUri();
					} else
						if (ndef is RtdText)
					{
						SelectText();
					} else
						if (ndef is RtdVCard)
					{
						SelectVCard();
					} else
						if (ndef is RtdMedia)
					{
						SelectMedia();
					} else
						if (ndef is RtdHandoverSelector)
					{
						SelectWifiHandover();
					}
					
					if (control != null)
					{
						control.SetContent(ndef);
						break;
					}
					
				}
				
				if (!tag.IsLocked())
				{
					/* It will be possible to rewrite the tag */
					setEditable(true);
				}
				
				if (control == null)
				{
					/* No supported record has been found */
					Unselect();
					MessageBox.Show("This Tag contains a valid content, but this application doesn't know how to display it", "This NFC Tag is not supported");
				}
			}
			
		}
		
		/*
		 * card_read_proc
		 * --------------
		 * This is the core function to try read a card, and maybe recognize it as an NFC tag
		 * The function is executed in a background thread, so the application's window keeps
		 * responding during the dialog
		 */
		private void card_read_proc()
		{
			NfcTag tag = null;
			string msg = null;
			bool Desfire_formatable = false;
			
			Trace.WriteLine("Is the card a NFC Forum Tag ???");
			
			/*
			 * 1st step, is the card a NFC Forum Tag ?
			 * ---------------------------------------
			 */
			
			
			if ( ! NfcTag.Recognize(cardchannel, out tag, out msg, out Desfire_formatable))
				Trace.WriteLine("Unrecognized or unsupported tag");

			
			/*
			 * 2nd step, tell the application we've got something, and die
			 * -----------------------------------------------------------
			 */
			
			if (tag != null)
			{
				this.BeginInvoke(new OnTagReadInvoker(OnTagRead), tag);
			} else
			{
				
				if (Desfire_formatable)
					this.BeginInvoke(new EnableFormatButtonInvoker(EnableFormatButton));
				
				this.BeginInvoke(new OnErrorInvoker(OnError), msg, "This is not a valid NFC Tag");
			}
		}

		/*
		 * card_write_proc
		 * ---------------
		 * This is the core function to try read a card, and maybe recognize it as an NFC tag
		 * The function is executed in a background thread, so the application's window keeps
		 * responding during the dialog
		 */
		private void card_write_proc(object _tag)
		{
			if (_tag == null)
				return; /* Oups */
			
			if (!(_tag is NfcTag))
				return; /* Oups */
			
			NfcTag tag = _tag as NfcTag;

			if (!tag.IsFormatted() && !tag.Format())
			{
				this.BeginInvoke(new OnErrorInvoker(OnError), "This application has been unable to format the Tag", "Failed to encode the NFC Tag");
				return;
			}

			if (!tag.Write())
			{
				this.BeginInvoke(new OnErrorInvoker(OnError), "This application has been unable to write onto the Tag", "Failed to encode the NFC Tag");
				return;
			}
			
			if (cbLock.Visible && cbLock.Checked)
			{
				/* Try to lock the Tag */
				if (tag.IsLockable())
				{
					if (!tag.Lock())
					{
						this.BeginInvoke(new OnErrorInvoker(OnError), "This application has been unable to lock the Tag in read-only state", "Failed to encode the NFC Tag");
						return;
					}
				} else
				{
					/* This Tag is not locackable */
					this.BeginInvoke(new OnErrorInvoker(OnError), "The Tag has been successfully written, but there's no method to put it in read-only state", "This NFC Tag can't be locked");
					return;
				}
			}

			/* Done */
			this.BeginInvoke(new OnTagWriteInvoker(OnTagWrite), tag);
		}
		
		delegate void EnableFormatButtonInvoker();
		void EnableFormatButton()
		{
			btCreateNfcTag.Visible = true;
			btCreateNfcTag.Enabled = true;
			btnWrite.Visible = false;
			btnWrite.Enabled = false;
		}
		
		void BtnWriteClick(object sender, EventArgs e)
		{
			if (tag == null)
				return; /* Oups */
			
			if (control == null)
				return; /* Oups */
						
			if (!control.ValidateUserContent())
			{
				/* The user did not provide a valid content */
				return;
			}
			
			Ndef ndef = control.GetContent();
			
			if (ndef == null)
			{
				MessageBox.Show("Failed to create the NDEF message to be written onto the Tag");
				return;
			}
			
			tag.Content.Clear();
			tag.Content.Add(ndef);
			
			long content_size = tag.ContentSize();
			long tag_capacity = tag.Capacity();
			
			if (content_size > tag_capacity)
			{
				MessageBox.Show("The capacity of the Tag is " + tag_capacity + "B, but the content you're trying to write makes " + content_size + "B", "This Tag is too small");
				return;
			}
			
			if (!tag.IsEmpty())
			{
				/* Ask for confirmation before overwriting */
				if (MessageBox.Show("The Tag already contains data. Do you really want to overwrite its content?", "Confirm overwrite", MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
					return;
				}
			}

			if (!tag.IsFormatted())
			{
				/* Ask for confirmation before formatting */
				if (MessageBox.Show("The Tag is not yet formatted. Do you really want to format it?", "Confirm formatting", MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
					return;
				}
			}

			if (cbLock.Visible && cbLock.Checked)
			{
				if (!tag.IsLockable())
				{
					/* The Tag can't be locked */
				} else
				{
					/* Ask for confirmation before locking */
					if (MessageBox.Show("Locking the Tag in read-only state is permanent and can't be cancelled. Are you really sure you want to do this?", "Confirm locking", MessageBoxButtons.YesNo) != DialogResult.Yes)
					{
						return;
					}
				}
			}
			
			cardthread = new Thread(new ParameterizedThreadStart(card_write_proc));
			cardthread.Start(tag);

		}

		public void OnSmartPosterSelected(object sender, EventArgs e)
		{
			SelectSmartPoster();
		}
		
		public void OnUriSelected(object sender, EventArgs e)
		{
			SelectUri();
		}

		public void OnTextSelected(object sender, EventArgs e)
		{
			SelectText();
		}

		public void OnVCardSelected(object sender, EventArgs e)
		{
			SelectVCard();
		}

		public void OnMediaSelected(object sender, EventArgs e)
		{
			SelectMedia();
		}
		
		public void  OnWifiHandoverSelected(object sender, EventArgs e)
		{
			SelectWifiHandover();
		}
		
		void Panel1Paint(object sender, PaintEventArgs e)
		{
			
		}
		
		void Unselect()
		{
			if (control != null)
			{
				pMain.Controls.Remove(control);
				control.Dispose();
				control = null;
			}

			SmartPosterButton.SetSelected(false);
			UriButton.SetSelected(false);
			TextButton.SetSelected(false);
			VCardButton.SetSelected(false);
			MediaButton.SetSelected(false);
			//WifiHandoverButton.SetSelected(false);
		}
		
		void SelectUri()
		{
			Unselect();
			UriButton.SetSelected(true);
			control = new RtdUriControl();
			control.Dock = DockStyle.Fill;
			pMain.Controls.Add(control);
		}

		void SelectText()
		{
			Unselect();
			TextButton.SetSelected(true);
			control = new RtdTextControl();
			control.Dock = DockStyle.Fill;
			pMain.Controls.Add(control);
		}

		void SelectSmartPoster()
		{
			Unselect();
			SmartPosterButton.SetSelected(true);
			control = new RtdSmartPosterControl();
			control.Dock = DockStyle.Fill;
			pMain.Controls.Add(control);
		}
		
		void SelectVCard()
		{
			Unselect();
			VCardButton.SetSelected(true);
			control = new RtdVCardControl();
			control.Dock = DockStyle.Fill;
			pMain.Controls.Add(control);
		}
		
		void SelectMedia()
		{
			Unselect();
			MediaButton.SetSelected(true);
			control = new RtdMediaControl();
			control.Dock = DockStyle.Fill;
			pMain.Controls.Add(control);
		}

		void SelectWifiHandover()
		{
			Unselect();
			//WifiHandoverButton.SetSelected(true);
			control = new RtdWifiHandoverControl();
			control.Dock = DockStyle.Fill;
			pMain.Controls.Add(control);
			
		}

		void AboutToolStripMenuItem1Click(object sender, EventArgs e)
		{
			AboutForm about = new AboutForm(this.imgHeader.BackColor);
			about.ShowDialog();
		}
		
		void QuitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
		}
		
		void MainFormShown(object sender, EventArgs e)
		{
			SplashForm splash = new SplashForm();
			splash.ShowDialog();

			ReaderSelectForm readerSelect = new ReaderSelectForm(this.imgHeader.BackColor);
			
			if (readerSelect.SelectedReader == null)
			{
				readerSelect.Preselect("SpringCard|ontactless");
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
			
			setReader(readerSelect.SelectedReader);
		}
		
		void MainFormClosed(object sender, FormClosedEventArgs e)
		{
			if (cardchannel != null)
			{
				cardchannel.Disconnect();
				cardchannel = null;
			}

			if (reader != null)
			{
				reader.StopMonitor();
				reader = null;
			}
			
			SystemConsole.Free();
		}
		
		void ImgHeaderClick(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.springcard.com");
		}
		
		void MiReaderClick(object sender, EventArgs e)
		{
			ReaderSelectForm readerSelect = new ReaderSelectForm(this.imgHeader.BackColor);
			readerSelect.SelectedReader = eReaderName.Text;
			readerSelect.ShowDialog();

			if (readerSelect.SelectedReader != null)
				setReader(readerSelect.SelectedReader);
		}
		
		void MiEnableLockCheckedChanged(object sender, EventArgs e)
		{
			if (!ready)
				return;

			Settings s = new Settings();
			s.EnableLock = miEnableLock.Checked;
			s.Save();
			cbLock.Visible = miEnableLock.Checked;
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
		
		void BtCreateNfcTagClick(object sender, EventArgs e)
		{
			int read=0;
			for (int i = 0 ; i< SCARD.Readers.Length ; i++)
				if (SCARD.Readers[i].Equals(reader.Name))
					read = i;

			DesfireFormatForm format = new DesfireFormatForm(read);
			format.ShowDialog();
			
			if (format.format_ok)
			{
				Trace.WriteLine("Formating ok");
				cardchannel = new SCardChannel(reader);
				if (cardchannel.Connect())
				{
					Trace.WriteLine("Connected to the card");
					cardthread = new Thread(card_read_proc);
					cardthread.Start();
				}
			} else
			{
				Trace.WriteLine("Formating ko");
			}
		}
		void PictureBox1Click(object sender, EventArgs e)
		{
	
		}
	}

}