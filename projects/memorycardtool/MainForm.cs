/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 14/06/2013
 * Time: 13:47
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using SpringCard.PCSC;
using SpringCard.LibCs;
using SpringCardApplication;
using SpringCardMemoryCard;
using Be.Windows.Forms;

namespace MemoryCardTool
{
	public partial class MainForm : Form
	{
		string Title;

		SCardReaderList ReaderList = null;
		SCardReader Reader = null;

		SCardChannel cardChannel = null;
		MemoryCard memoryCard = null;
		UserControl cardControl = null;

		WaitForm waitForm = null;

		Thread cardReaderThread;

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
		}

		void SaveConfig()
		{
			AppConfig.SaveFormAspect(this);
		}

		void DisplayReaderPresent()
		{
			lbReaderName.Text = Reader.Name;
		}

		void DisplayReaderAbsent()
		{
			lbReaderName.Text = "";
			lbReaderStatus.Text = "";
			lbCardAtr.Text = "";
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

			DisplayReaderState(ReaderState, CardAtr);

			if (ReaderState == SCARD.STATE_UNAWARE)
			{
				if (cardChannel != null)
				{
					cardChannel.Disconnect();
					cardChannel = null;
				}
				if (Reader != null)
				{
					Reader.StopMonitor();
					Reader.Release();
					Reader = null;
				}
				if (ReaderList != null)
				{
					ReaderList.Release();
					ReaderList = null;
				}

				btnReadAgain.Enabled = false;
				btnWrite.Enabled = false;

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
				if (cardChannel != null)
				{
					cardChannel.Disconnect();
					cardChannel = null;
				}

				btnReadAgain.Enabled = false;
				btnWrite.Enabled = false;
			}
			else if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
			{
				btnReadAgain.Enabled = false;
				btnWrite.Enabled = false;
			}
			else if ((ReaderState & SCARD.STATE_MUTE) != 0)
			{
				btnReadAgain.Enabled = false;
				btnWrite.Enabled = false;
			}
			else if ((ReaderState & SCARD.STATE_INUSE) != 0)
			{

			}
			else if ((ReaderState & SCARD.STATE_PRESENT) != 0)
			{
				if (cardChannel == null)
				{
					/* New card -> leave edit mode */
					cardChannel = Reader.GetChannel();

					DisplayWorking(false);

					if (cardChannel.Connect())
					{
						cardReaderThread = new Thread(ReadCardFirst);
						cardReaderThread.Start();
					}
					else
					{
						ClearDisplay();
						MessageBox.Show(this,
						                "Failed to connect to the card in the reader. Please verify that you don't have another application running in the background, that tries to access to the smartcards in the same reader.",
						                Title,
						                MessageBoxButtons.OK,
						                MessageBoxIcon.Exclamation);
						cardChannel = null;
					}
				}
			}
		}

		void BtnReadAgainClick(object sender, EventArgs e)
		{
			DisplayWorking(true);
			cardReaderThread = new Thread(ReadCardAgain);
			cardReaderThread.Start();
		}

		void BtnWriteClick(object sender, EventArgs e)
		{
			if (cardControl != null)
			{
				if (cardControl is MemoryCardControl)
				{
					((MemoryCardControl) cardControl).WriteToCard();
				}
				else if (cardControl is MemoryCardMifareClassicControl)
				{
					((MemoryCardMifareClassicControl) cardControl).WriteToCard();
				}
			}
		}

		void DisplayWorking(bool keepCardInfo)
		{
			ClearDisplay(keepCardInfo);

			MoveWaitForm();
			waitForm.Show(this);
		}

		void ClearDisplay(bool keepCardInfo = false)
		{
			waitForm.Hide();

			cardControl = null;
			pMain.Controls.Clear();

			if (!keepCardInfo)
			{
				eCardName.Text = "(none)";
				eCardUID.Text = "(none)";
				btnReadAgain.Enabled = false;
				btnWrite.Enabled = false;
			}
		}

		private void ReadCardFirst()
		{
			/*
			 * 1st step, is this a memory card ?
			 * ---------------------------------
			 */
			memoryCard = MemoryCard.Create(cardChannel);

			if (memoryCard == null)
			{
				this.BeginInvoke(new OnErrorInvoker(OnError), "This is not a memory card");
				return;
			}

			/* Display the type and serial number */
			this.BeginInvoke(new OnCardFoundInvoker(OnCardFound));

			/*
			 * 2nd step, read the card
			 * -----------------------
			 */

			if (!memoryCard.Read())
			{
				this.BeginInvoke(new OnErrorInvoker(OnError), "Failed to read the card");
				return;
			}

			/*
			 * 3rd step, display the card's content
			 * ------------------------------------
			 */

			this.BeginInvoke(new OnCardReadInvoker(OnCardRead));
		}

		private void ReadCardAgain()
		{
			/*
			 * 1st step, is this a memory card ?
			 * ---------------------------------
			 */

			if (memoryCard == null)
				return;

			/*
			 * 2nd step, read the card
			 * -----------------------
			 */

			if (!memoryCard.Read())
			{
				this.BeginInvoke(new OnErrorInvoker(OnError), "Failed to read the card");
				return;
			}

			/*
			 * 3rd step, display the card's content
			 * ------------------------------------
			 */

			this.BeginInvoke(new OnCardReadInvoker(OnCardRead));
		}

		delegate void OnErrorInvoker(string msg);
		void OnError(string msg)
		{
			Logger.Trace("Error: " + msg);
			ClearDisplay();
			MessageBox.Show(this, msg);
			if (cardChannel != null)
			{
				cardChannel.DisconnectLeave();
				cardChannel = null;
			}
		}

		delegate void OnCardFoundInvoker();
		void OnCardFound()
		{
			Logger.Trace("Card found");

			if (memoryCard == null)
			{
				MessageBox.Show(this, "Internal error, card is null!");
				return;
			}

			eCardName.Text = memoryCard.CardName;
			eCardUID.Text = memoryCard.CardUID;
		}

		delegate void OnCardReadInvoker();
		void OnCardRead()
		{
			Logger.Trace("Card read OK");

			waitForm.Hide();

			if (memoryCard == null)
			{
				MessageBox.Show(this, "Internal error, card is null!");
				return;
			}

			if (memoryCard is MemoryCardMifareClassic)
			{
				DisplayMemoryCardMifareClassic();
			}
			else
			{
				DisplayMemoryCard();
			}
		}

		public void DataChanged()
		{
			btnWrite.Enabled = btnReadAgain.Enabled;
		}

		void DisplayMemoryCard()
		{
			btnReadAgain.Enabled = true;
			MemoryCardControl c = new MemoryCardControl(memoryCard);
			c.DataChanged += new MemoryCardControl.DataChangeHandler(DataChanged);
			cardControl = c;
			pMain.Controls.Add(cardControl);
			showBtnWrite();
		}

		void DisplayMemoryCardMifareClassic()
		{
			btnReadAgain.Enabled = true;
			MemoryCardMifareClassicControl c = new MemoryCardMifareClassicControl((MemoryCardMifareClassic) memoryCard);
			c.DataChanged += new MemoryCardMifareClassicControl.DataChangeHandler(DataChanged);
			cardControl = c;
			pMain.Controls.Add(cardControl);
			hideBtnWrite();
		}
		
		private void hideBtnWrite()
		{
			btnWrite.Enabled = false;
			btnWrite.Visible = false;
		}
		
		private void showBtnWrite()
		{
			btnWrite.Enabled = true;
			btnWrite.Visible = true;			
		}

		void SelectReader()
		{
			if ((ReaderList != null) || (Reader != null))
			{
				if (!UserWantsToClose())
					return;

				CloseDevice();
			}

			ReaderSelectForm f = new ReaderSelectForm();
			bool rc = (f.ShowDialog(this) == DialogResult.OK);

			if (rc)
			{
				string ReaderName = f.SelectedReader;

				ReaderList = new SCardReaderList();
				if (ReaderList != null)
				{
					Reader = ReaderList.GetReader(ReaderName);
					if (Reader != null)
					{
						DisplayReaderPresent();
						Reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
						return;
					}
					else
					{
						MessageBox.Show(this,
						                "Failed to connect to the PC/SC reader. Please click the 'Reader' link to select another reader.",
						                Title,
						                MessageBoxButtons.OK,
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

		void MainFormShown(object sender, EventArgs e)
		{
			waitForm = new WaitForm();

			SplashForm f = new SplashForm();
			f.ShowDialog();

			if (AppConfig.ReadBoolean("reader_reconnect"))
			{
				ReaderList = new SCardReaderList();
				if (ReaderList != null)
				{
					string reader_name = AppConfig.ReadString("reader_name");
					if (ReaderList.Contains(reader_name))
					{
						Reader = new SCardReader(reader_name);
						DisplayReaderPresent();
						Reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
						return;
					}
					ReaderList = null;
				}
			}

			SelectReader();
		}

		void MainFormLoad(object sender, EventArgs e)
		{

		}

		void ChangeReaderToolStripMenuItemClick(object sender, EventArgs e)
		{
			SelectReader();
		}

		void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}



		void AboutToolStripMenuItemClick(object sender, EventArgs e)
		{
			AboutForm about = new AboutForm();
			about.ShowDialog(this);
		}
		

		void LinkReaderLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SelectReader();
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

		bool UserWantsToClose()
		{
			bool rc;
			if(AppUtils.ReadSettingBool("QuitWithoutAsking", false))
				return true;
			
			rc = (MessageBox.Show(this,
			                      "You are currently connected. Are you sure you want to disconnect from this reader now?",
			                      Title,
			                      MessageBoxButtons.YesNo,
			                      MessageBoxIcon.Question,
			                      MessageBoxDefaultButton.Button1) == DialogResult.Yes);
			return rc;
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

		void MainFormSizeChanged(object sender, EventArgs e)
		{
			MoveWaitForm();
		}

		void MainFormLocationChanged(object sender, EventArgs e)
		{
			MoveWaitForm();
		}

		void MoveWaitForm()
		{
			if (waitForm != null)
			{
				int x = this.Left;
				x += this.Width / 2 - 64;
				waitForm.Left = x;
				int y = this.Top;
				y += this.Height / 2;
				waitForm.Top = y;
				waitForm.Refresh();
				Refresh();
			}
		}
		
		void PictureBox1DoubleClick(object sender, EventArgs e)
		{
			// @TODO: REMOVE
		}
		void LbCardAtrDoubleClick(object sender, EventArgs e)
		{
			if(!lbCardAtr.Text.Trim().Equals("")) {
				Clipboard.SetText(lbCardAtr.Text.Trim());
				MessageBox.Show(this, "Card's ATR has been copied to the clipboard");
			}
		}
		void ECardNameDoubleClick(object sender, EventArgs e)
		{
			if(!eCardName.Text.Trim().Equals("")) {
				Clipboard.SetText(eCardName.Text.Trim());
				MessageBox.Show(this, "Card's family has been copied to the clipboard");
			}
		}
		void ECardUIDDoubleClick(object sender, EventArgs e)
		{
			if(!eCardUID.Text.Trim().Equals("")) {
				Clipboard.SetText(eCardUID.Text.Trim());
				MessageBox.Show(this, "Serial number has been copied to the clipboard");
			}	
		}
	}
}
