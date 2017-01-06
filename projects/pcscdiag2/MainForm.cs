/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using SpringCard.PCSC;
using SpringCard.LibCs;
using SpringCardApplication;
using EXControls;

namespace PcscDiag2
{
	public partial class MainForm : Form
	{
		CardAtrList smartcardList;
		SCardReaderList reader_list;
		EXListView lvReaders;
		List<CardForm> card_forms = new List<CardForm>();
		
		const string NowFormat = "HH:mm:ss";
		
		const int ReaderImageGeneric = 0;
		const int ReaderImageContactless = 1;
		const int ReaderImageContact = 2;
		const int ReaderImageSimSam = 3;
		const int StatusImageUnknown = 0;
		const int StatusImageError = 1;
		const int StatusImageUnavailable = 2;
		const int StatusImageAbsent = 3;
		const int StatusImagePresent = 4;
		const int StatusImageMute = 5;
		const int StatusImageExclusive = 6;
		const int StatusImageInUse = 7;
		
		public MainForm(string[] args)
		{
			if (args != null)
			{
				bool verbose = false;
				
				for (int i=0; i<args.Length; i++)
				{
					if (args[i].Equals("-v"))
						verbose = true;
				}
				
				if (verbose)
					SpringCard.LibCs.SystemConsole.Show();
			}
			
			InitializeComponent();
			
			if ((Screen.PrimaryScreen.Bounds.Width <= 1200) || (Screen.PrimaryScreen.Bounds.Height < 800))
			{
				WindowState = FormWindowState.Maximized;
			}
			
			FileVersionInfo info = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
			Text = Text + " v." + info.ProductVersion;
			
			Settings.Load();
			
			lvReaders = new EXListView();
			pMain.Controls.Add(lvReaders);

			lvReaders.AllowColumnReorder = false;
			lvReaders.AutoArrange = false;
			lvReaders.AutoSize = false;
			lvReaders.Dock = DockStyle.Fill;
			lvReaders.FullRowSelect = true;
			lvReaders.HeaderStyle = ColumnHeaderStyle.None;
			
			lvReaders.MyHighlightBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
			
			lvReaders.MultiSelect = false;
			lvReaders.SmallImageList = readerImages;
			
			lvReaders.ContextMenuStrip = readerPopupMenu;
			lvReaders.ShowItemToolTips = true;
			
			lvReaders.ShowGroups = true;			
			
			EXColumnHeader col;
			
			col = new EXColumnHeader("", 20);
			lvReaders.Columns.Add(col);
			
			col = new EXColumnHeader("Reader Name", 355);
			lvReaders.Columns.Add(col);

			col = new EXColumnHeader("", 20);
			lvReaders.Columns.Add(col);

			col = new EXColumnHeader("Status", 145);
			lvReaders.Columns.Add(col);
			
			col = new EXColumnHeader("Card ATR", lvReaders.ClientRectangle.Width - 560);
			lvReaders.Columns.Add(col);
			
			lvReaders.SelectedIndexChanged += new System.EventHandler(LvReaderSelectedIndexChanged);
			lvReaders.DoubleClick += new System.EventHandler(LvReaderDoubleClicked);
			lvReaders.KeyPress += new System.Windows.Forms.KeyPressEventHandler(LvReaderKeyPress);
			
			foreach (Control control in Controls)
				SetHintHandler(control);
		}
		
		void LvReaderSelectedIndexChanged(object sender, EventArgs e)
		{
			string t;
			if (lvReaders.SelectedItems.Count == 1)
			{
				t = "Reader selected : " + lvReaders.SelectedItems[0].SubItems[1].Text;
			} else
			if (lvReaders.Items.Count == 0)
			{
				t = "No PC/SC reader found. Please install one.";
			} else
			{
				t = "Select a reader";
			}
			toolTip.SetToolTip(lvReaders, t);
			lbMessage.Text = t;
		}
		
		void LvReaderDoubleClicked(object sender, EventArgs e)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				EXImageListViewItem item = (EXImageListViewItem) lvReaders.SelectedItems[0];
				
				if (item.SubItems[4].Text.Equals(""))
				{
					/* No ATR --> can't connect to a card, must connect to the reader */
					OpenCardForm(SCARD.SHARE_DIRECT, SCARD.PROTOCOL_NONE);
				} else
				{
					/* Trying to connect to the card, not to the reader */
					OpenCardForm(SCARD.SHARE_SHARED, (uint) (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1));
				}
			}
		}
		
		void ShowHint(object sender, EventArgs e)
		{
			if (sender is Control)
			{
				string t = toolTip.GetToolTip((Control) sender);
				lbMessage.Text = t;
			}
		}

		void SetHintHandler(Control control)
		{
			control.MouseHover += new System.EventHandler(ShowHint);
			foreach (Control child in control.Controls)
				SetHintHandler(child);
		}
		
		void UpdateReaderState(string ReaderName, uint ReaderState, CardBuffer CardAtr)
		{
			Console.WriteLine(DateTime.Now.ToString(NowFormat));
			Console.WriteLine("\tStatus changed for '" + ReaderName + "'");
			Console.WriteLine("\t\tState: " + SCARD.ReaderStatusToString(ReaderState));
			if (CardAtr != null)
				Console.WriteLine("\t\tATR: " + CardAtr.AsString(" "));
			
			for (int i=0; i<lvReaders.Items.Count; i++)
			{
				EXImageListViewItem item = (EXImageListViewItem) lvReaders.Items[i];
				
				if (item.MyValue.Equals(ReaderName))
				{
					/* Reader found in the list */
					/* ------------------------ */
					
					lvReaders.BeginUpdate();
					
					/* Set status image */
					int statusImage;
					string statusText;

					if ((ReaderState & SCARD.STATE_PRESENT) != 0)
					{
						/* Card is present */
						if ((ReaderState & SCARD.STATE_INUSE) != 0)
						{
							/* Card in use */
							if ((ReaderState & SCARD.STATE_EXCLUSIVE) != 0)
							{
								/* Card in exclusive use */
								statusText = "In use (exclusive)";
								statusImage = StatusImageExclusive;
							} else
							{
								statusText = "In use (shared)";
								statusImage = StatusImageInUse;
							}
						} else
							if ((ReaderState & SCARD.STATE_MUTE) != 0)
						{
							/* Card is mute */
							statusText = "Mute";
							statusImage = StatusImageMute;
						} else
							if ((ReaderState & SCARD.STATE_UNPOWERED) != 0)
						{
							/* Card is not powered */
							statusText = "Present, not powered";
							statusImage = StatusImagePresent;
						} else
						{
							/* Card is powered */
							statusText = "Present, powered";
							statusImage = StatusImagePresent;
						}
					} else
						if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
					{
						/* Problem */
						statusText = "Reserved (direct)";
						statusImage = StatusImageUnavailable;
					} else
						if ((ReaderState & SCARD.STATE_IGNORE) != 0)
					{
						/* Problem */
						statusText = "Error (ignore)";
						statusImage = StatusImageError;
					} else
						if ((ReaderState & SCARD.STATE_UNKNOWN) != 0)
					{
						/* Problem */
						statusText = "Error (status unknown)";
						statusImage = StatusImageUnknown;
					} else
						if ((ReaderState & SCARD.STATE_EMPTY) != 0)
					{
						/* No card */
						statusText = "Absent";
						statusImage = StatusImageAbsent;
					} else
					{
						/* Problem */
						statusText = "Bad status";
						statusImage = StatusImageError;
					}
					
					EXImageListViewSubItem subitem = (EXImageListViewSubItem) item.SubItems[2];
					subitem.MyImage = statusImages.Images[statusImage];
					item.SubItems[3].Text = statusText;

					if (CardAtr != null)
						item.SubItems[4].Text = CardAtr.AsString("");
					else
						item.SubItems[4].Text = "";

					lvReaders.EndUpdate();
					break;
				}
				/* NB : we ignore the event in case the reader is not already listed */
			}
		}
		
		private class ReaderComparer: IComparer<string>
		{
			public static string ReaderName(string s)
			{
				if (s.StartsWith("SpringCard"))
				{
					if (s.Contains(" Nfc")) s = s.Substring(0, s.LastIndexOf(" Nfc")); else
					if (s.Contains(" Rfid")) s = s.Substring(0, s.LastIndexOf(" Rfid")); else
					if (s.Contains(" Contactless")) s = s.Substring(0, s.LastIndexOf(" Contactless")); else
					if (s.Contains(" Contact")) s = s.Substring(0, s.LastIndexOf(" Contact")); else
					if (s.Contains(" SAM ")) s = s.Substring(0, s.LastIndexOf(" SAM"));
				}
				
				return s;
			}
			
			public static int ReaderNumber(string s)
			{
				int r = -1;
				int i = s.LastIndexOf(' ');
				if (i >= 0)
				{
					s = s.Substring(i + 1);
					try
					{
						r = int.Parse(s);
					}
					catch (Exception)
					{
						r = -1;
					}
				}
				return r;
			}
			
			public static int ReaderSlot(string s)
			{
				if (s.Contains(" Nfc")) return 0;
				if (s.Contains(" Rfid")) return 1;
				if (s.Contains(" Contactless")) return 2;
				if (s.Contains(" Contact")) return 3;
				if (s.Contains(" SAM ")) return 4;
				return 10;
			}
			
			public int Compare(string x, string y)
			{
				int x_i, y_i;
				
				x_i = x.StartsWith("SpringCard") ? 0 : 1;
				y_i = y.StartsWith("SpringCard") ? 0 : 1;
				if (x_i != y_i) return x_i - y_i;

				x_i = ReaderNumber(x);
				y_i = ReaderNumber(y);
				if (x_i != y_i) return x_i - y_i;
				
				x_i = ReaderSlot(x);
				y_i = ReaderSlot(y);
				if (x_i != y_i) return x_i - y_i;
				
				return x.CompareTo(y);
			}
		}
		
		void UpdateReaderList()
		{
			Console.WriteLine(DateTime.Now.ToString(NowFormat));
			Console.WriteLine("\tThe list of reader(s) has changed");
			
			lvReaders.BeginUpdate();
			lvReaders.Groups.Clear();
			lvReaders.Items.Clear();
			string[] readers = reader_list.Readers;
			
			if (readers != null)
			{
				if (readers.Length == 0)
				{
					lbReaders.Text = "No PC/SC reader";
				} else
					if (readers.Length == 1)
				{
					lbReaders.Text = "1 PC/SC reader";
				} else
				{
					lbReaders.Text = String.Format("{0} PC/SC readers", readers.Length);					
				}
								
				Array.Sort(readers, new ReaderComparer());
				
				ListViewGroup non_springcard_readers_group = null;
				ListViewGroup this_springcard_reader_group = null;
				int last_reader_number = -1;
				
				for (int i=0; i<readers.Length; i++)
				{
					string reader_name = readers[i];
					
					Console.WriteLine("\t\t" + reader_name);
					
					int reader_number = ReaderComparer.ReaderNumber(reader_name);
					if (last_reader_number != reader_number)
					{
						this_springcard_reader_group = null;
						last_reader_number = reader_number;
					}
					
					ListViewGroup reader_group;
					int reader_image = ReaderImageGeneric;

					if (reader_name.StartsWith("SpringCard"))
					{
						if (reader_name.Contains(" Contactless")) reader_image = ReaderImageContactless; else
						if (reader_name.Contains(" Nfc")) reader_image = ReaderImageContactless; else
						if (reader_name.Contains(" Rfid")) reader_image = ReaderImageContactless; else
						if (reader_name.Contains(" Contact")) reader_image = ReaderImageContact; else
						if (reader_name.Contains(" SAM")) reader_image = ReaderImageSimSam;

						if (this_springcard_reader_group == null)
						{
							this_springcard_reader_group = new ListViewGroup("", ReaderComparer.ReaderName(reader_name) + " " + reader_number);
							lvReaders.Groups.Add(this_springcard_reader_group);
						}
						reader_group = this_springcard_reader_group;
					} else
					{
						if (non_springcard_readers_group == null)
						{
							non_springcard_readers_group = new ListViewGroup("", "Other readers");
							lvReaders.Groups.Add(non_springcard_readers_group);
						}
						reader_group = non_springcard_readers_group;
					}
					
					EXImageListViewItem item = new EXImageListViewItem(readerImages.Images[reader_image]);
					
					item.MyValue = reader_name;
					item.UseItemStyleForSubItems = false;					
					
					item.SubItems.Add(new EXImageListViewSubItem(reader_name));
					item.SubItems.Add(new EXImageListViewSubItem(""));					
					item.SubItems.Add(new EXImageListViewSubItem(""));					
					item.SubItems.Add(new EXImageListViewSubItem(""));					
					item.SubItems[4].Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
										
					reader_group.Items.Add(item);

					lvReaders.Items.Add(item);
				}
			} else
			{
				lbReaders.Text = "No PC/SC reader";
			}
			lvReaders.EndUpdate();
			LvReaderSelectedIndexChanged(null, null);
		}
		
		delegate void ReaderListChangedInvoker(string ReaderName, uint ReaderState, CardBuffer CardAtr);
		void ReaderListChanged(string ReaderName, uint ReaderState, CardBuffer CardAtr)
		{
			/* The ReaderListChanged function is called as a delegate (callback) by the SCardReaderList object  */
			/* withing its backgroung thread. Therefore we must use the BeginInvoke syntax to switch back from  */
			/* the context of the background thread to the context of the application's main thread. Overwise   */
			/* we'll get a security violation when trying to access the window's visual components (that belong */
			/* to the application's main thread and can't be safely manipulated by background threads).         */
			if (InvokeRequired)
			{
				Console.WriteLine("ReaderListChanged (in background thread)");
				this.BeginInvoke(new ReaderListChangedInvoker(ReaderListChanged), ReaderName, ReaderState, CardAtr);
				return;
			}
			
			Console.WriteLine("ReaderListChanged (in main thread)");
			
			if (ReaderName != null)
			{
				/* A reader-related event */
				/* ---------------------- */
				
				UpdateReaderState(ReaderName, ReaderState, CardAtr);
				
			} else
			{
				/* The list of readers has changed, let's rebuild it */
				/* ------------------------------------------------- */
				
				UpdateReaderList();
			}
		}
		
		void MainFormShown(object sender, EventArgs e)
		{
			SplashForm f = new SplashForm();
			f.ShowDialog();
			
			/* Check if run by admin (to allow adding ATR into registry)	*/
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
			
			if (isAdmin)
			{
				miATRRegistry.Enabled = true;
			} else
			{
				miATRRegistry.Enabled = false;
			}
			
			StartReaderMonitor();
		}
		
		void PictureBox1Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.springcard.com");
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
			smartcardList = new CardAtrList();
		}
		
		void AboutToolStripMenuItemClick(object sender, EventArgs e)
		{
			AboutForm f = new AboutForm();
			f.ShowDialog();
		}
		
		void MainFormFormClosed(object sender, FormClosedEventArgs e)
		{
			if (reader_list != null)
			{
				reader_list.StopMonitor();
				reader_list = null;
			}
			
			for (int i=0; i<card_forms.Count; i++)
			{
				CardForm f = card_forms[i];
				f.Close();
				f.Dispose();
			}
			
			Settings.Save();
		}

		/*
		 * Popup menu when the user right-clicks on a reader in the list
		 * -------------------------------------------------------------
		 */
		void ReaderPopupMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (lvReaders.SelectedItems.Count != 1)
			{
				/* The menu shall appear only when a reader is selected */
				e.Cancel = true;
				return;
			}

			EXImageListViewItem item = (EXImageListViewItem) lvReaders.SelectedItems[0];
			
			bool AtrEmpty = item.SubItems[4].Text.Equals(""); /* ATR empty -> No card in the reader */

			/* We could connect to the card only if there's one */
			miOpenShared.Enabled = !AtrEmpty;
			miOpenExclusive.Enabled = !AtrEmpty;
			
			/* We could work on the ATR only if there's one */
			miAtrAnalysis.Enabled = !AtrEmpty;
			miAtrCopy.Enabled = !AtrEmpty;

			/* Only SpringCard readers could provided informations */
			miReaderInfo.Enabled = (item.MyValue.Contains("SpringCard"));
		}
		
		void MiAtrCopyClick(object sender, EventArgs e)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				/* Copy the ATR to the clipboard */
				EXImageListViewItem item = (EXImageListViewItem) lvReaders.SelectedItems[0];
				if (!item.SubItems[4].Text.Equals(""))
					Clipboard.SetText(item.SubItems[4].Text);
			}
		}
		
		/*
		 * When a CardForm is closed, we shall remove it from the list of known forms
		 * --------------------------------------------------------------------------
		 */
		void OnCloseCardForm(string ReaderName)
		{
			for (int i=0; i<card_forms.Count; i++)
			{
				if (card_forms[i].ReaderName.Equals(ReaderName))
				{
					card_forms.RemoveAt(i);
					break;
				}
			}
		}

		/*
		 * Open (or re-focus) a CardForm for the specified reader
		 * ------------------------------------------------------
		 */
		void OpenCardForm(string ReaderName, Image ReaderImage, uint ShareMode, uint Protocol)
		{
			CardForm f = null;
			
			/* Try to retrieve an existing CardForm belonging to this reader */
			/* ------------------------------------------------------------- */
			
			for (int i=0; i<card_forms.Count; i++)
			{
				if (card_forms[i].ReaderName.Equals(ReaderName))
				{
					f = card_forms[i];
					break;
				}
			}
			
			if (f == null)
			{
				/* Create a new CardForm */
				/* --------------------- */
				
				f = new CardForm(ReaderName, ReaderImage, OnCloseCardForm);
				
				/* Location */
				f.Left = Left + 80 + 20 * card_forms.Count;
				f.Top = Top + 40 + 20 * card_forms.Count;
				
				/* Remember this CardForm in our list */
				card_forms.Add(f);
				
				if (ShareMode == SCARD.SHARE_DIRECT)
				{
					/* Direct mode -> preload control page */
					f.HistoryControl(1);
				} else
				{
					/* Other mode -> preload transmit page */
					f.HistoryTransmit(1);
				}

			}

			/* Make the form visible, on top, focused */
			/* -------------------------------------- */
			
			f.Show();
			f.BringToFront();
			f.Focus();
			
			/* Connect (or reconnect) to the reader/card */
			/* ----------------------------------------- */
			f.Connect(ShareMode, Protocol);
		}
		
		/*
		 * Open (or re-focus) a CardForm for the currently selected reader
		 * ---------------------------------------------------------------
		 */
		void OpenCardForm(uint ShareMode, uint Protocol)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				EXImageListViewItem item = (EXImageListViewItem) lvReaders.SelectedItems[0];
				OpenCardForm(item.MyValue, item.MyImage, ShareMode, Protocol);
			}
		}
		
		void MiOpenSharedClick(object sender, EventArgs e)
		{
			OpenCardForm(SCARD.SHARE_SHARED, (uint) (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1));
		}
		
		void MiOpenExclusiveClick(object sender, EventArgs e)
		{
			OpenCardForm(SCARD.SHARE_EXCLUSIVE, (uint) (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1));
		}
		
		void MiOpenDirectClick(object sender, EventArgs e)
		{
			OpenCardForm(SCARD.SHARE_DIRECT, SCARD.PROTOCOL_NONE);
		}
		
		void LvReaderKeyPress(object sender, KeyPressEventArgs e)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				/* A reader is selected */
				EXImageListViewItem item = (EXImageListViewItem) lvReaders.SelectedItems[0];
				
				bool AtrEmpty = item.SubItems[4].Text.Equals("");

				switch (e.KeyChar)
				{
					case (char) Keys.Return :

						/* Enter key -> open the CardForm */
						if (AtrEmpty)
						{
							/* ATR empty -> No card in the reader -> DIRECT mode */
							OpenCardForm(SCARD.SHARE_DIRECT, SCARD.PROTOCOL_NONE);
						} else
						{
							/* Connect to the card (default is SHARED mode) */
							OpenCardForm(SCARD.SHARE_SHARED, (uint) (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1));
						}
						
						e.Handled = true;
						break;
						
					case 'S' :
					case 's' :
						
						/* Shared */
						if (!AtrEmpty)
						{
							OpenCardForm(SCARD.SHARE_SHARED, (uint) (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1));
							e.Handled = true;
						}
						break;

					case 'X' :
					case 'x' :
						
						/* Exclusive */
						if (!AtrEmpty)
						{
							OpenCardForm(SCARD.SHARE_EXCLUSIVE, (uint) (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1));
							e.Handled = true;
						}
						break;

					case 'D' :
					case 'd' :
						
						/* Direct */
						OpenCardForm(SCARD.SHARE_DIRECT, SCARD.PROTOCOL_NONE);
						e.Handled = true;
						break;
						
					case 'A' :
					case 'a' :
						
						/* ATR analysis */
						if (!AtrEmpty)
						{

							e.Handled = true;
						}
						break;

					case 'C' :
					case 'c' :
						
						/* ATR copy */
						if (!AtrEmpty)
						{
							Clipboard.SetText(item.SubItems[4].Text);
							e.Handled = true;
						}
						break;

					case 'R' :
					case 'r' :
						
						/* Reader info */
						if (item.MyValue.Contains("SpringCard"))
						{
							e.Handled = true;
						}
						break;
						
						default :
							break;
				}

			}
		}
		
		void MiReaderInfoClick(object sender, EventArgs e)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				EXImageListViewItem item = (EXImageListViewItem) lvReaders.SelectedItems[0];
				ReaderInfoForm f = new ReaderInfoForm(item.MyValue);
				f.ShowDialog();
			}
		}
		
		void MiAtrAnalysisClick(object sender, EventArgs e)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				EXImageListViewItem item = (EXImageListViewItem) lvReaders.SelectedItems[0];
				if (!item.SubItems[4].Text.Equals(""))
				{
					CardInfoForm f = new CardInfoForm(smartcardList, item.MyValue, item.SubItems[4].Text);
					f.ShowDialog();
				}
			}
		}
		
		void PCSCOptionsToolStripMenuItemClick(object sender, EventArgs e)
		{
			ContextAndListForm f = new ContextAndListForm();
			f.ShowDialog();
		
			StartReaderMonitor();
		}
		
		void StartReaderMonitor()
		{
			if (reader_list != null)
			{
				reader_list.StopMonitor();
				reader_list = null;
			}
			
			string s = "";
			
			switch (Settings.ContextScope)
			{
				case SCARD.SCOPE_USER :
					s += "User scope";
					break;
				case SCARD.SCOPE_SYSTEM :
					s += "System scope";
					break;
					default :
						s += "Scope=" + Settings.ContextScope.ToString();
					break;
			}
			
			s += ", ";
			
			if (Settings.ListGroup.Equals(SCARD.ALL_READERS))
			{
				s += "All readers";
			} else
				if (Settings.ListGroup.Equals(SCARD.DEFAULT_READERS))
			{
				s += "Default readers";
			} else
			{
				s += Settings.ListGroup;
			}
			
			lbOptions.Text = s;

			reader_list = new SCardReaderList(Settings.ContextScope, Settings.ListGroup);
			reader_list.StartMonitor(new SCardReaderList.StatusChangeCallback(ReaderListChanged));
		}
		
		void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			
		}
		
		void MenuMainItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			
		}
		
		void PMainPaint(object sender, PaintEventArgs e)
		{
			
		}
		
	   [DllImport
     ("winscard.dll", EntryPoint = "SCardListCards", SetLastError =
		  true)] public static extern uint SCardListCards(IntPtr phContext, 
		                                                byte[] pbAtr,
		                                                byte[] rgguiInterfaces,
		                                          			uint cguidInterfaceCount, 
		                                          			string mszCards,
		                                          			ref int pcchCards);
		  
	   [DllImport
     ("winscard.dll", EntryPoint = "SCardIntroduceCardType", SetLastError =
		  true)] public static extern uint SCardIntroduceCardType(IntPtr phContext, 
		                                                string szCardName,
		                                               	byte[] pguidPrimaryProvider,
		                                               	byte[] rgguidInterfaces,
		                                               	uint dwInterfaceCount,
		                                               	byte[] atr,
		                                               	byte[] pbAtrMask,
		                                               	uint cbAtrLen);

		[DllImport
     ("winscard.dll", EntryPoint = "SCardSetCardTypeProviderName", SetLastError =
		  true)] public static extern uint SCardSetCardTypeProviderName 	(IntPtr phContext, 
		                                                string szCardName,
		                                               	uint dwProviderId,
		                                               	string szProvider);
		
		void MiATRRegistryClick(object sender, EventArgs e)
		{
		  EXImageListViewItem item = (EXImageListViewItem) lvReaders.SelectedItems[0];
			if (!item.SubItems[4].Text.Equals(""))
			{				
				IntPtr hContext = IntPtr.Zero;
				uint _last_error = SCARD.EstablishContext(SCARD.SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, ref hContext);
				if (_last_error != SCARD.S_SUCCESS)
				{
					MessageBox.Show("Error: can't establish context");
					return;
				}
				
				CardBuffer cardBufAtr = new CardBuffer(item.SubItems[4].Text);
				byte[] atr = cardBufAtr.GetBytes();
				
				byte[] dummy = new byte[99];
				int cchCards = -1; /*	SCARD_AUTOALLOCATE */
			  _last_error = SCardListCards(hContext,
				                    atr,
			                      null,
														0,
														null,
														ref cchCards);
				
				if (_last_error != SCARD.S_SUCCESS)
				{
					MessageBox.Show("Error: can't list cards");
					return;
				}
		
				if (cchCards <= 1)
			  {
			    /* Card not found. We need to add it. */
			    string name = "NoMinidriver-"+ item.SubItems[4].Text;
			    _last_error = SCardIntroduceCardType(hContext,
                              name,
                              null,
															null, 
															0,
															atr,
															null,
															(uint) atr.Length);
			  	
			    if (_last_error != SCARD.S_SUCCESS)
					{
						MessageBox.Show("Error: can't introduce card type");
						return;
					}
			    				
		      _last_error = SCardSetCardTypeProviderName(hContext,
                                    name,
                                    2,  			/*	SCARD_PROVIDER_CSP	*/
																		"$DisableSCPnP$");
			    
			    if (_last_error != SCARD.S_SUCCESS)
					{
						MessageBox.Show("Error: can't set card type provider name");
						return;
					} else
			    {
			    	MessageBox.Show("This card will now be recognized by the system");
			    }
			    
			    
			  } else
				{
					MessageBox.Show("This card is already recognized by the system");
				}
			}
		}
		
		void QuitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}
		
		void MainFormSizeChanged(object sender, EventArgs e)
		{
			lvReaders.Columns[4].Width = lvReaders.ClientRectangle.Width - 460;
		}
	}
}
