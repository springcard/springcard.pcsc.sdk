/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using EXControls;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;
using System.Windows.Forms;

namespace PcscDiag2
{
    public partial class MainForm : Form
    {
        SCardReaderList readers;
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

        bool ShowSplash = true;

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

            if ((Screen.PrimaryScreen.Bounds.Width <= 1200) || (Screen.PrimaryScreen.Bounds.Height < 800))
            {
                WindowState = FormWindowState.Maximized;
            }

			Text = AppUtils.ApplicationTitle(true);

            Logger.Trace("Loading settings from registry");

            Settings.Load();

            Logger.Trace("Building GUI");

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

            Logger.Trace("Ready");
        }

        void LvReaderSelectedIndexChanged(object sender, EventArgs e)
        {
            string t;
            if (lvReaders.SelectedItems.Count == 1)
            {
                t = "Reader selected : " + lvReaders.SelectedItems[0].SubItems[1].Text;
            }
            else
            if (lvReaders.Items.Count == 0)
            {
                t = "No PC/SC reader found. Please install one.";
            }
            else
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
                EXImageListViewItem item = (EXImageListViewItem)lvReaders.SelectedItems[0];

                if (item.SubItems[4].Text.Equals(""))
                {
                    /* No ATR --> can't connect to a card, must connect to the reader */
                    OpenCardForm(SCARD.SHARE_DIRECT, SCARD.PROTOCOL_DIRECT());
                }
                else
                {
                    /* Trying to connect to the card, not to the reader */
                    OpenCardForm(SCARD.SHARE_SHARED, SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
                }
            }
        }

        void ShowHint(object sender, EventArgs e)
        {
            if (sender is Control)
            {
                string t = toolTip.GetToolTip((Control)sender);
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
            Logger.Trace("Status changed for '" + ReaderName + "'");
            Logger.Trace("\tState: " + SCARD.ReaderStatusToString(ReaderState));
            if (CardAtr != null)
                Logger.Trace("\tATR: " + CardAtr.AsString(" "));

            for (int i = 0; i < lvReaders.Items.Count; i++)
            {
                EXImageListViewItem item = (EXImageListViewItem)lvReaders.Items[i];

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
                            }
                            else
                            {
                                statusText = "In use (shared)";
                                statusImage = StatusImageInUse;
                            }
                        }
                        else
                            if ((ReaderState & SCARD.STATE_MUTE) != 0)
                        {
                            /* Card is mute */
                            statusText = "Mute";
                            statusImage = StatusImageMute;
                        }
                        else
                            if ((ReaderState & SCARD.STATE_UNPOWERED) != 0)
                        {
                            /* Card is not powered */
                            statusText = "Present, not powered";
                            statusImage = StatusImagePresent;
                        }
                        else
                        {
                            /* Card is powered */
                            statusText = "Present, powered";
                            statusImage = StatusImagePresent;
                        }
                    }
                    else
                        if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
                    {
                        /* Problem */
                        statusText = "Reserved (direct)";
                        statusImage = StatusImageUnavailable;
                    }
                    else
                        if ((ReaderState & SCARD.STATE_IGNORE) != 0)
                    {
                        /* Problem */
                        statusText = "Error (ignore)";
                        statusImage = StatusImageError;
                    }
                    else
                        if ((ReaderState & SCARD.STATE_UNKNOWN) != 0)
                    {
                        /* Problem */
                        statusText = "Error (status unknown)";
                        statusImage = StatusImageUnknown;
                    }
                    else
                        if ((ReaderState & SCARD.STATE_EMPTY) != 0)
                    {
                        /* No card */
                        statusText = "Absent";
                        statusImage = StatusImageAbsent;
                    }
                    else
                    {
                        /* Problem */
                        statusText = "Bad status";
                        statusImage = StatusImageError;
                    }

                    EXImageListViewSubItem subitem = (EXImageListViewSubItem)item.SubItems[2];
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

        private class ReaderInfo
        {
            public string ReaderName { get; private set; }
            public bool IsSpringCard { get; private set; }
            private string _VendorName = "";
            public string VendorName { get { return _VendorName; } }
            private string _ProductName = "";
            public string ProductName { get { return _ProductName; } }
            private string _SlotName = "";
            public string SlotName { get { return _SlotName; } }
            private int _SlotOrder;
            public int SlotOrder { get { return _SlotOrder; } }
            private int _Number;
            public int Number { get { return _Number; } }
            private int _Image = ReaderImageGeneric;
            public int Image { get { return _Image; } }

            public ReaderInfo(string ReaderName)
            {
                Logger.Debug("Processing reader {0}", ReaderName);

                this.ReaderName = ReaderName;

                string[] a = ReaderName.Split(' ');

                switch (a.Length)
                {
                    case 0:
                    case 1:
                        return;

                    case 2:
                        _ProductName = a[0];
                        int.TryParse(a[1], out _Number);
                        break;

                    case 3:
                        _VendorName = a[0];
                        _ProductName = a[1];
                        int.TryParse(a[2], out _Number);
                        break;

                    case 4:
                        _VendorName = a[0];
                        _ProductName = a[1];
                        _SlotName = a[2];
                        int.TryParse(a[3], out _Number);
                        break;

                    default:
                        _VendorName = a[0];
                        for (int i = 1; i < a.Length - 2; i++)
                        {
                            if (!string.IsNullOrEmpty(_ProductName)) _ProductName += " ";
                            _ProductName += a[i];
                        }
                        _SlotName = a[a.Length - 2];
                        int.TryParse(a[a.Length - 1], out _Number);
                        break;
                }

                Logger.Debug("\tVendorName: {0}", _VendorName);
                Logger.Debug("\tProductName: {0}", _ProductName);
                Logger.Debug("\tSlotName: {0}", _SlotName);
                Logger.Debug("\tNumber: {0}", _Number);

                IsSpringCard = _VendorName.ToLower().Equals("springcard");

                switch (_SlotName.ToLower())
                {
                    case "nfc":
                        _SlotOrder = 0;
                        _Image = ReaderImageContactless;
                        break;
                    case "rfid":
                        _SlotOrder = 1;
                        _Image = ReaderImageContactless;
                        break;
                    case "contactless":
                        _SlotOrder = 2;
                        _Image = ReaderImageContactless;
                        break;
                    case "contact":
                        _SlotOrder = 3;
                        _Image = ReaderImageContact;
                        break;
                    case "id-1":
                        _SlotOrder = 4;
                        _Image = ReaderImageContact;
                        break;
                    case "sam":
                        _SlotOrder = 5;
                        _Image = ReaderImageSimSam;
                        break;
                    case "sim/sam":
                        _SlotOrder = 6;
                        _Image = ReaderImageSimSam;
                        break;
                    case "se":
                        _SlotOrder = 7;
                        _Image = ReaderImageSimSam;
                        break;
                    default:
                        _SlotOrder = 10; break;
                }
            }
        }

        private class ReaderComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                ReaderInfo x_r = new ReaderInfo(x);
                ReaderInfo y_r = new ReaderInfo(y);

                int x_i, y_i, r;

                x_i = x_r.IsSpringCard ? 0 : 1;
                y_i = y_r.IsSpringCard ? 0 : 1;
                if (x_i != y_i) return x_i - y_i;

                r = x_r.VendorName.CompareTo(y_r.VendorName);
                if (r != 0) return r;

                r = x_r.ProductName.CompareTo(y_r.ProductName);
                if (r != 0) return r;

                x_i = x_r.Number;
                y_i = y_r.Number;
                if (x_i != y_i) return x_i - y_i;

                x_i = x_r.SlotOrder;
                y_i = y_r.SlotOrder;
                if (x_i != y_i) return x_i - y_i;

                return x.CompareTo(y);
            }
        }

        void UpdateReaderList()
        {
            Logger.Trace("The list of reader(s) has changed");

            lvReaders.BeginUpdate();
            lvReaders.Groups.Clear();
            lvReaders.Items.Clear();
            string[] readers = this.readers.Readers;

            if (readers != null)
            {
                if (readers.Length == 0)
                {
                    lbReaders.Text = "No PC/SC reader";
                }
                else
                    if (readers.Length == 1)
                {
                    lbReaders.Text = "1 PC/SC reader";
                }
                else
                {
                    lbReaders.Text = String.Format("{0} PC/SC readers", readers.Length);
                }

                //Array.Sort(readers, new ReaderComparer());

                ListViewGroup non_springcard_readers_group = null;
                ListViewGroup this_springcard_reader_group = null;
                int last_reader_number = -1;

                foreach (string readerName in readers)
                {
                    Logger.Debug("Adding {0} to the view", readerName);
                    ReaderInfo readerInfo = new ReaderInfo(readerName);

                    Logger.Trace("\t" + readerInfo.ReaderName);

                    if (last_reader_number != readerInfo.Number)
                    {
                        this_springcard_reader_group = null;
                        last_reader_number = readerInfo.Number;
                    }

                    ListViewGroup reader_group;

                    if (readerInfo.IsSpringCard)
                    {
                        if (this_springcard_reader_group == null)
                        {
                            this_springcard_reader_group = new ListViewGroup("", readerInfo.ProductName + " " + readerInfo.Number);
                            lvReaders.Groups.Add(this_springcard_reader_group);
                        }
                        reader_group = this_springcard_reader_group;
                    }
                    else
                    {
                        if (non_springcard_readers_group == null)
                        {
                            non_springcard_readers_group = new ListViewGroup("", "Other readers");
                            lvReaders.Groups.Add(non_springcard_readers_group);
                        }
                        reader_group = non_springcard_readers_group;
                    }

                    EXImageListViewItem item = new EXImageListViewItem(readerImages.Images[readerInfo.Image]);

                    item.MyValue = readerInfo.ReaderName;
                    item.UseItemStyleForSubItems = false;

                    item.SubItems.Add(new EXImageListViewSubItem(readerInfo.ReaderName));
                    item.SubItems.Add(new EXImageListViewSubItem(""));
                    item.SubItems.Add(new EXImageListViewSubItem(""));
                    item.SubItems.Add(new EXImageListViewSubItem(""));
                    item.SubItems[4].Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

                    reader_group.Items.Add(item);

                    lvReaders.Items.Add(item);
                }
            }
            else
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
                Logger.Debug("ReaderListChanged (in background thread)");
                this.BeginInvoke(new ReaderListChangedInvoker(ReaderListChanged), ReaderName, ReaderState, CardAtr);
                return;
            }

            Logger.Debug("ReaderListChanged (in main thread)");

            if (ReaderName != null)
            {
                /* A reader-related event */
                /* ---------------------- */

                UpdateReaderState(ReaderName, ReaderState, CardAtr);

            }
            else
            {
                /* The list of readers has changed, let's rebuild it */
                /* ------------------------------------------------- */

                UpdateReaderList();
            }
        }

        void MainFormShown(object sender, EventArgs e)
        {
            if (ShowSplash)
            {
                Logger.Trace("Showing splash form");
                SplashForm.DoShowDialog(this, FormStyle.ModernRed);
            }

            /* Check if run by admin (to allow adding ATR into registry)	*/
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (isAdmin)
            {
                miATRRegistry.Enabled = true;
            }
            else
            {
                miATRRegistry.Enabled = false;
            }

            Logger.Trace("Starting reader monitor thread");

            StartReaderMonitor();
        }

        void PictureBox1Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.springcard.com");
        }

        void MainFormLoad(object sender, EventArgs e)
        {

        }

        void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernRed);
        }

        void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            if (readers != null)
            {
                readers.StopMonitor();
                readers = null;
            }

            for (int i = 0; i < card_forms.Count; i++)
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

            EXImageListViewItem item = (EXImageListViewItem)lvReaders.SelectedItems[0];

            bool AtrEmpty = item.SubItems[4].Text.Equals(""); /* ATR empty -> No card in the reader */

            /* We could connect to the card only if there's one */
            miOpenShared.Enabled = !AtrEmpty;
            miOpenExclusive.Enabled = !AtrEmpty;

            /* We could work on the ATR only if there's one */
            miAtrAnalysis.Enabled = !AtrEmpty;
            miAtrCopy.Enabled = !AtrEmpty;

            miReaderInfo.Enabled = true;
        }

        void MiAtrCopyClick(object sender, EventArgs e)
        {
            if (lvReaders.SelectedItems.Count == 1)
            {
                /* Copy the ATR to the clipboard */
                EXImageListViewItem item = (EXImageListViewItem)lvReaders.SelectedItems[0];
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
            for (int i = 0; i < card_forms.Count; i++)
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

            for (int i = 0; i < card_forms.Count; i++)
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
                }
                else
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
                EXImageListViewItem item = (EXImageListViewItem)lvReaders.SelectedItems[0];
                OpenCardForm(item.MyValue, item.MyImage, ShareMode, Protocol);
            }
        }

        void MiOpenSharedClick(object sender, EventArgs e)
        {
            OpenCardForm(SCARD.SHARE_SHARED, SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
        }

        void MiOpenExclusiveClick(object sender, EventArgs e)
        {
            OpenCardForm(SCARD.SHARE_EXCLUSIVE, SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
        }

        void MiOpenDirectClick(object sender, EventArgs e)
        {
            OpenCardForm(SCARD.SHARE_DIRECT, SCARD.PROTOCOL_DIRECT());
        }

        void LvReaderKeyPress(object sender, KeyPressEventArgs e)
        {
            if (lvReaders.SelectedItems.Count == 1)
            {
                /* A reader is selected */
                EXImageListViewItem item = (EXImageListViewItem)lvReaders.SelectedItems[0];

                bool AtrEmpty = item.SubItems[4].Text.Equals("");

                switch (e.KeyChar)
                {
                    case (char)Keys.Return:

                        /* Enter key -> open the CardForm */
                        if (AtrEmpty)
                        {
                            /* ATR empty -> No card in the reader -> DIRECT mode */
                            OpenCardForm(SCARD.SHARE_DIRECT, SCARD.PROTOCOL_DIRECT());
                        }
                        else
                        {
                            /* Connect to the card (default is SHARED mode) */
                            OpenCardForm(SCARD.SHARE_SHARED, SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
                        }

                        e.Handled = true;
                        break;

                    case 'S':
                    case 's':

                        /* Shared */
                        if (!AtrEmpty)
                        {
                            OpenCardForm(SCARD.SHARE_SHARED, SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
                            e.Handled = true;
                        }
                        break;

                    case 'X':
                    case 'x':

                        /* Exclusive */
                        if (!AtrEmpty)
                        {
                            OpenCardForm(SCARD.SHARE_EXCLUSIVE, SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
                            e.Handled = true;
                        }
                        break;

                    case 'D':
                    case 'd':

                        /* Direct */
                        OpenCardForm(SCARD.SHARE_DIRECT, SCARD.PROTOCOL_DIRECT());
                        e.Handled = true;
                        break;

                    case 'A':
                    case 'a':

                        /* ATR analysis */
                        if (!AtrEmpty)
                        {

                            e.Handled = true;
                        }
                        break;

                    case 'C':
                    case 'c':

                        /* ATR copy */
                        if (!AtrEmpty)
                        {
                            Clipboard.SetText(item.SubItems[4].Text);
                            e.Handled = true;
                        }
                        break;

                    case 'R':
                    case 'r':

                        /* Reader info */
                        if (item.MyValue.Contains("SpringCard"))
                        {
                            e.Handled = true;
                        }
                        break;

                    default:
                        break;
                }

            }
        }

        void MiReaderInfoClick(object sender, EventArgs e)
        {
            if (lvReaders.SelectedItems.Count == 1)
            {
                EXImageListViewItem item = (EXImageListViewItem)lvReaders.SelectedItems[0];
                ReaderInfoForm f = new ReaderInfoForm(item.MyValue);
                f.ShowDialog();
            }
        }

        void MiAtrAnalysisClick(object sender, EventArgs e)
        {
            if (lvReaders.SelectedItems.Count == 1)
            {
                EXImageListViewItem item = (EXImageListViewItem)lvReaders.SelectedItems[0];
                if (!item.SubItems[4].Text.Equals(""))
                {
                    CardInfoForm f = new CardInfoForm(item.MyValue, item.SubItems[4].Text);
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
            if (readers != null)
            {
                readers.StopMonitor();
                readers = null;
            }

            string s = "";

            switch (Settings.ContextScope)
            {
                case SCARD.SCOPE_USER:
                    s += "User scope";
                    break;
                case SCARD.SCOPE_SYSTEM:
                    s += "System scope";
                    break;
                default:
                    s += "Scope=" + Settings.ContextScope.ToString();
                    break;
            }

            s += ", ";

            if (Settings.ListGroup.Equals(SCARD.ALL_READERS))
            {
                s += "All readers";
            }
            else
                if (Settings.ListGroup.Equals(SCARD.DEFAULT_READERS))
            {
                s += "Default readers";
            }
            else
            {
                s += Settings.ListGroup;
            }

            lbOptions.Text = s;

            readers = new SCardReaderList(Settings.ContextScope, Settings.ListGroup);
            readers.StartMonitor(new SCardReaderList.StatusChangeCallback(ReaderListChanged));
        }

        void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.Trace("Closing...");
        }

        void MenuMainItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        void PMainPaint(object sender, PaintEventArgs e)
        {

        }

        void MiATRRegistryClick(object sender, EventArgs e)
        {
            EXImageListViewItem item = (EXImageListViewItem)lvReaders.SelectedItems[0];
            if (!item.SubItems[4].Text.Equals(""))
            {
                bool Added;
                CardBuffer Atr = new CardBuffer(item.SubItems[4].Text);

                if (SpringCard.PCSC.ReaderHelper.NoSmartcardDriver.DisableDriverForThisAtr(Atr, out Added))
                {
                    if (Added)
                    {
                        MessageBox.Show(this, "This ATR has been added to the no-driver list.");
                    }
                    else
                    {
                        MessageBox.Show(this, "This ATR was already present in the no-driver list.");
                    }
                }
                else
                {
                    MessageBox.Show(this, "The application has been unable to add this ATR to the no-driver list.");
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
