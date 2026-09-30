/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.PCSC;
using SpringCard.PCSC.ReaderHelpers;
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
        bool quiet;
        Settings settings;
        SCardReaderList readers;
        List<CardForm> card_forms = new List<CardForm>();
        Brush brush_selected = SystemBrushes.ControlLight;
        Brush brush_unselected = SystemBrushes.Window;
        Brush brush_text = SystemBrushes.WindowText;
        Font font_text = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
        Font font_atr = new Font("Consolas", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
        int row_height = 22;

        enum ColumnIndexes : int
        {
            Empty,
            ReaderImage,
            ReaderName,
            StatusImage,
            StatusText,
            CardAtr
        }

        public class ListEntryTag
        {
            public string ReaderName;
            public int ReaderImage;
            public int StatusImage;
            public ListEntryTag(string ReaderName, int ReaderImage, int StatusImage)
            {
                this.ReaderName = ReaderName;
                this.ReaderImage = ReaderImage;
                this.StatusImage = StatusImage;
            }
        }

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

        public MainForm(bool quiet)
        {
            InitializeComponent();
            this.quiet = quiet;
            

            if ((Screen.PrimaryScreen.Bounds.Width <= 1200) || (Screen.PrimaryScreen.Bounds.Height < 800))
            {
                WindowState = FormWindowState.Maximized;
            }

			Text = AppUtils.ApplicationTitle(true);

            Logger.Trace("Loading settings from registry");

            settings = new Settings();
            settings.Load();

            Logger.Trace("Building GUI");

            if (settings.Maximized)
                WindowState = FormWindowState.Maximized;
            else
                WindowState = FormWindowState.Normal;

            SetHeight(lvReaders, row_height);

            foreach (Control control in Controls)
                SetHintHandler(control);

            Logger.Trace("Ready");
        }

        private void SetHeight(ListView listView, int height)
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, height);
            listView.SmallImageList = imgList;
        }

        void LvReaderSelectedIndexChanged(object sender, EventArgs e)
        {
            string t;
            if (lvReaders.SelectedItems.Count == 1)
            {
                t = "Reader selected : " + lvReaders.SelectedItems[0].SubItems[(int)ColumnIndexes.ReaderName].Text;
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
                ListViewItem item = lvReaders.SelectedItems[0];

                if (item.SubItems[(int)ColumnIndexes.CardAtr].Text.Equals(""))
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
                ListViewItem item = lvReaders.Items[i];

                if ((item.Tag as ListEntryTag).ReaderName.Equals(ReaderName))
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
                        else if ((ReaderState & SCARD.STATE_MUTE) != 0)
                        {
                            /* Card is mute */
                            statusText = "Mute";
                            statusImage = StatusImageMute;
                        }
                        else if ((ReaderState & SCARD.STATE_UNPOWERED) != 0)
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
                    else if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
                    {
                        /* Problem */
                        statusText = "Reserved (direct)";
                        statusImage = StatusImageUnavailable;
                    }
                    else if ((ReaderState & SCARD.STATE_IGNORE) != 0)
                    {
                        /* Problem */
                        statusText = "Error (ignore)";
                        statusImage = StatusImageError;
                    }
                    else if ((ReaderState & SCARD.STATE_UNKNOWN) != 0)
                    {
                        /* Problem */
                        statusText = "Error (status unknown)";
                        statusImage = StatusImageUnknown;
                    }
                    else if ((ReaderState & SCARD.STATE_EMPTY) != 0)
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

                    (item.Tag as ListEntryTag).StatusImage = statusImage;
                    item.SubItems[(int)ColumnIndexes.StatusText].Text = statusText;

                    if (CardAtr != null)
                        item.SubItems[(int)ColumnIndexes.CardAtr].Text = CardAtr.AsString("");
                    else
                        item.SubItems[(int)ColumnIndexes.CardAtr].Text = "";

                    

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
            public string VendorName { get; private set; } = "";
            public string ProductName { get; private set; } = "";
            public string SlotName { get; private set; } = "";
            public int SlotOrder { get; private set; }
            public string Index { get; private set; }
            public int ReaderImage { get; private set; } = ReaderImageGeneric;

            public string Key
            {
                get
                {
                    return string.Format("{0}|{1}|{2}", VendorName, ProductName, Index);
                }
            }

            public string Title
            {
                get
                {
                    return string.Format("{0} {1}", ProductName, Index);
                }
            }

            public ReaderInfo(string ReaderName)
            {
                Logger.Debug("Processing reader {0}", ReaderName);

                this.ReaderName = ReaderName;

                ReaderInfos.ExplainReaderName(ReaderName, out string VendorName, out string ProductName, out string SlotName, out string Index);

                Logger.Debug("\tVendorName: {0}", VendorName);
                Logger.Debug("\tProductName: {0}", ProductName);
                Logger.Debug("\tSlotName: {0}", SlotName);
                Logger.Debug("\tIndex: {0}", Index);

                this.VendorName = VendorName;
                this.ProductName = ProductName;
                this.SlotName = SlotName;
                this.Index = Index;
                this.IsSpringCard = ReaderInfos.IsSpringCard(ReaderName);

                switch (SlotName.ToLower())
                {
                    case "nfc":
                        SlotOrder = 0;
                        ReaderImage = ReaderImageContactless;
                        break;
                    case "rfid":
                        SlotOrder = 1;
                        ReaderImage = ReaderImageContactless;
                        break;
                    case "contactless":
                        SlotOrder = 2;
                        ReaderImage = ReaderImageContactless;
                        break;
                    case "contact":
                        SlotOrder = 3;
                        ReaderImage = ReaderImageContact;
                        break;
                    case "id-1":
                        SlotOrder = 4;
                        ReaderImage = ReaderImageContact;
                        break;
                    case "sam":
                        SlotOrder = 5;
                        ReaderImage = ReaderImageSimSam;
                        break;
                    case "sim/sam":
                        SlotOrder = 6;
                        ReaderImage = ReaderImageSimSam;
                        break;
                    case "se":
                        SlotOrder = 7;
                        ReaderImage = ReaderImageSimSam;
                        break;
                    default:
                        SlotOrder = 10; break;
                }
            }
        }

        void UpdateReaderList()
        {
            Logger.Trace("The list of reader(s) has changed");

            lvReaders.BeginUpdate();
            lvReaders.Groups.Clear();
            lvReaders.Items.Clear();
            
            if (readers.Readers != null)
            {
                List<string> readerNames = new List<string>(readers.Readers);

                if (readerNames.Count == 0)
                {
                    lbReaders.Text = "No PC/SC reader";
                }
                else if (readerNames.Count == 1)
                {
                    lbReaders.Text = "1 PC/SC reader";
                }
                else
                {
                    lbReaders.Text = string.Format("{0} PC/SC readers", readerNames.Count);
                }

                readerNames.Sort(new ReaderInfos.ReaderNamesComparer());

                ListViewGroup non_springcard_readers_group = null;
                ListViewGroup this_springcard_reader_group = null;
                string last_reader_key = "";

                Logger.Info("Found {0} PC/SC readers", readerNames.Count);
                foreach (string readerName in readerNames)
                {
                    Logger.Trace("\t{0}", readerName);

                    ReaderInfo readerInfo = new ReaderInfo(readerName);

                    Logger.Trace("\t\tVendorName : {0}", readerInfo.VendorName);
                    Logger.Trace("\t\tProductName: {0}", readerInfo.ProductName);
                    Logger.Trace("\t\tStotName   : {0}", readerInfo.SlotName);
                    Logger.Trace("\t\tNumber     : {0}", readerInfo.Index);
                    Logger.Trace("\t\tKey        : {0}", readerInfo.Key);
                    Logger.Trace("\t\tTitle      : {0}", readerInfo.Title);

                    if (last_reader_key != readerInfo.Key)
                    {
                        this_springcard_reader_group = null;
                        last_reader_key = readerInfo.Key;
                    }

                    ListViewGroup reader_group;

                    if (readerInfo.IsSpringCard)
                    {
                        if (this_springcard_reader_group == null)
                        {
                            Logger.Debug("Creating group for reader");
                            Logger.Debug("\tKey  : {0}", readerInfo.Key);
                            Logger.Debug("\tTitle: {0}", readerInfo.Title);
                            this_springcard_reader_group = new ListViewGroup(readerInfo.Key, readerInfo.Title);
                            this_springcard_reader_group.HeaderAlignment = HorizontalAlignment.Center;
                            lvReaders.Groups.Add(this_springcard_reader_group);
                        }
                        else
                        {
                            Logger.Debug("Re-using reader group");
                        }
                        reader_group = this_springcard_reader_group;
                    }
                    else
                    {
                        if (non_springcard_readers_group == null)
                        {
                            Logger.Debug("Creating default group");
                            non_springcard_readers_group = new ListViewGroup("", "Non-SpringCard readers");
                            non_springcard_readers_group.HeaderAlignment = HorizontalAlignment.Center;
                            lvReaders.Groups.Add(non_springcard_readers_group);
                        }
                        else
                        {
                            Logger.Debug("Re-using default group");
                        }
                        reader_group = non_springcard_readers_group;
                    }

                    ListViewItem item = new ListViewItem();

                    item.Tag = new ListEntryTag(readerInfo.ReaderName, readerInfo.ReaderImage, -1);
                    item.UseItemStyleForSubItems = false;

                    item.SubItems.Add("X");
                    item.SubItems.Add(readerInfo.ReaderName);
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");

                    item.SubItems[(int)ColumnIndexes.ReaderName].Font = font_text;
                    item.SubItems[(int)ColumnIndexes.StatusText].Font = font_text;
                    item.SubItems[(int)ColumnIndexes.CardAtr].Font = font_atr;

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
            MainFormSizeChanged(sender, e);

            if (!quiet)
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

            settings.Save();
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

            ListViewItem item = lvReaders.SelectedItems[0];

            bool AtrEmpty = item.SubItems[(int)ColumnIndexes.CardAtr].Text.Equals(""); /* ATR empty -> No card in the reader */

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
                ListViewItem item = lvReaders.SelectedItems[0];
                if (!item.SubItems[(int)ColumnIndexes.CardAtr].Text.Equals(""))
                    Clipboard.SetText(item.SubItems[(int)ColumnIndexes.CardAtr].Text);
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
        void OpenCardForm(string readerName, Image readerImage, uint ShareMode, uint Protocol)
        {
            CardForm f = null;

            /* Try to retrieve an existing CardForm belonging to this reader */
            /* ------------------------------------------------------------- */

            for (int i = 0; i < card_forms.Count; i++)
            {
                if (card_forms[i].ReaderName.Equals(readerName))
                {
                    f = card_forms[i];
                    break;
                }
            }

            if (f == null)
            {
                /* Create a new CardForm */
                /* --------------------- */

                f = new CardForm(settings, readerName, readerImage, OnCloseCardForm);

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
                ListViewItem item = lvReaders.SelectedItems[0];
                OpenCardForm((item.Tag as ListEntryTag).ReaderName, readerImages.Images[(item.Tag as ListEntryTag).ReaderImage], ShareMode, Protocol);
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
                ListViewItem item = lvReaders.SelectedItems[0];

                bool AtrEmpty = item.SubItems[(int)ColumnIndexes.CardAtr].Text.Equals("");

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
                            Clipboard.SetText(item.SubItems[(int)ColumnIndexes.CardAtr].Text);
                            e.Handled = true;
                        }
                        break;

                    case 'R':
                    case 'r':

                        /* Reader info */
                        if ((item.Tag as ListEntryTag).ReaderName.Contains("SpringCard"))
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
                ListViewItem item = lvReaders.SelectedItems[0];
                ReaderInfoForm f = new ReaderInfoForm((item.Tag as ListEntryTag).ReaderName);
                f.ShowDialog();
            }
        }

        void MiAtrAnalysisClick(object sender, EventArgs e)
        {
            if (lvReaders.SelectedItems.Count == 1)
            {
                ListViewItem item = lvReaders.SelectedItems[0];
                if (!item.SubItems[(int)ColumnIndexes.CardAtr].Text.Equals(""))
                {
                    CardInfoForm f = new CardInfoForm((item.Tag as ListEntryTag).ReaderName, item.SubItems[(int)ColumnIndexes.CardAtr].Text);
                    f.ShowDialog();
                }
            }
        }

        void PCSCOptionsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ContextAndListForm f = new ContextAndListForm(settings);
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

            switch (settings.ContextScope)
            {
                case SCARD.SCOPE_USER:
                    s += "User scope";
                    break;
                case SCARD.SCOPE_SYSTEM:
                    s += "System scope";
                    break;
                default:
                    s += "Scope=" + settings.ContextScope.ToString();
                    break;
            }

            s += ", ";

            if (settings.ListGroup.Equals(SCARD.ALL_READERS))
            {
                s += "All readers";
            }
            else
                if (settings.ListGroup.Equals(SCARD.DEFAULT_READERS))
            {
                s += "Default readers";
            }
            else
            {
                s += settings.ListGroup;
            }

            lbOptions.Text = s;

            readers = new SCardReaderList(settings.ContextScope, settings.ListGroup);
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
            ListViewItem item = lvReaders.SelectedItems[0];
            if (!item.SubItems[(int)ColumnIndexes.CardAtr].Text.Equals(""))
            {
                bool Added;
                CardBuffer Atr = new CardBuffer(item.SubItems[(int)ColumnIndexes.CardAtr].Text);

                if (NoSmartcardDriver.DisableDriverForThisAtr(Atr, out Added))
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
            settings.Maximized = (WindowState == FormWindowState.Maximized);

            if (SystemInfo.GetRuntimeEnvironment() == SystemInfo.DotNetEnvironment.MS_Net)
                lvReaders.Columns[(int)ColumnIndexes.CardAtr].Width = lvReaders.ClientRectangle.Width
                    - lvReaders.Columns[(int)ColumnIndexes.Empty].Width
                    - lvReaders.Columns[(int)ColumnIndexes.ReaderImage].Width
                    - lvReaders.Columns[(int)ColumnIndexes.ReaderName].Width
                    - lvReaders.Columns[(int)ColumnIndexes.StatusImage].Width
                    - lvReaders.Columns[(int)ColumnIndexes.StatusText].Width;
        }

        private void lvReaders_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(brush_selected, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(brush_unselected, e.Bounds);
            }

            if (e.ColumnIndex == (int)ColumnIndexes.ReaderImage)
            {
                int imageIndex = (e.Item.Tag as ListEntryTag).ReaderImage;
                var imageRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 1, 17, 17);
                e.Graphics.DrawImage(readerImages.Images[imageIndex], imageRect);
            }
            else if (e.ColumnIndex == (int)ColumnIndexes.StatusImage)
            {
                int imageIndex = (e.Item.Tag as ListEntryTag).StatusImage;
                var imageRect = new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 2, 16, 16);
                e.Graphics.DrawImage(statusImages.Images[imageIndex], imageRect);
            }
            else if (e.ColumnIndex == (int)ColumnIndexes.CardAtr)
            {
                Rectangle rectangle = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height - 2);
                StringFormat string_format = new StringFormat();

                string_format.Alignment = StringAlignment.Near;
                string_format.LineAlignment = StringAlignment.Far;
                string_format.FormatFlags = StringFormatFlags.NoWrap;
                e.Graphics.DrawString(e.Item.SubItems[(int)ColumnIndexes.CardAtr].Text, font_atr, brush_text, rectangle, string_format);
            }
            else
            {
                e.DrawText(TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl);
            }
        }

        private void lvReaders_DrawItem(object sender, DrawListViewItemEventArgs e)
        {

        }

        private void lvReaders_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawText(TextFormatFlags.TextBoxControl);
        }
    }
}
