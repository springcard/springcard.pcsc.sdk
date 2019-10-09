/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace PcscDiag2
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.menuMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pCSCOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.readerPopupMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.miOpenShared = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpenExclusive = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpenDirect = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.miAtrAnalysis = new System.Windows.Forms.ToolStripMenuItem();
			this.miAtrCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.miATRRegistry = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.miReaderInfo = new System.Windows.Forms.ToolStripMenuItem();
			this.readerImages = new System.Windows.Forms.ImageList(this.components);
			this.pMain = new System.Windows.Forms.Panel();
			this.pTitle = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lbOptions = new System.Windows.Forms.Label();
			this.lbReaders = new System.Windows.Forms.Label();
			this.pControl = new System.Windows.Forms.Panel();
			this.lbMessage = new System.Windows.Forms.Label();
			this.statusImages = new System.Windows.Forms.ImageList(this.components);
			this.pHeader = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.pTop = new System.Windows.Forms.Panel();
			this.pLogo = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.menuMain.SuspendLayout();
			this.readerPopupMenu.SuspendLayout();
			this.pTitle.SuspendLayout();
			this.panel1.SuspendLayout();
			this.pControl.SuspendLayout();
			this.pHeader.SuspendLayout();
			this.pTop.SuspendLayout();
			this.pLogo.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// menuMain
			// 
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem,
			this.aboutToolStripMenuItem});
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.Name = "menuMain";
			this.menuMain.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
			this.menuMain.Size = new System.Drawing.Size(1008, 25);
			this.menuMain.TabIndex = 1;
			this.menuMain.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.MenuMainItemClicked);
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.pCSCOptionsToolStripMenuItem,
			this.toolStripMenuItem1,
			this.quitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 19);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// pCSCOptionsToolStripMenuItem
			// 
			this.pCSCOptionsToolStripMenuItem.Name = "pCSCOptionsToolStripMenuItem";
			this.pCSCOptionsToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.pCSCOptionsToolStripMenuItem.Text = "PC/SC options";
			this.pCSCOptionsToolStripMenuItem.Click += new System.EventHandler(this.PCSCOptionsToolStripMenuItemClick);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(148, 6);
			// 
			// quitToolStripMenuItem
			// 
			this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
			this.quitToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.quitToolStripMenuItem.Text = "&Quit";
			this.quitToolStripMenuItem.Click += new System.EventHandler(this.QuitToolStripMenuItemClick);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 19);
			this.aboutToolStripMenuItem.Text = "&About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
			// 
			// readerPopupMenu
			// 
			this.readerPopupMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.miOpenShared,
			this.miOpenExclusive,
			this.miOpenDirect,
			this.toolStripSeparator1,
			this.miAtrAnalysis,
			this.miAtrCopy,
			this.miATRRegistry,
			this.toolStripSeparator2,
			this.miReaderInfo});
			this.readerPopupMenu.Name = "menuReaderPopup";
			this.readerPopupMenu.Size = new System.Drawing.Size(363, 192);
			this.readerPopupMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ReaderPopupMenuOpening);
			// 
			// miOpenShared
			// 
			this.miOpenShared.Name = "miOpenShared";
			this.miOpenShared.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.miOpenShared.Size = new System.Drawing.Size(362, 22);
			this.miOpenShared.Text = "Transmit (shared)";
			this.miOpenShared.ToolTipText = "Connect to the card (using T=0 or T=1 protocol, shared mode) and open the dialog " +
	"to Transmit APDUs to the card";
			this.miOpenShared.Click += new System.EventHandler(this.MiOpenSharedClick);
			// 
			// miOpenExclusive
			// 
			this.miOpenExclusive.Name = "miOpenExclusive";
			this.miOpenExclusive.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.miOpenExclusive.Size = new System.Drawing.Size(362, 22);
			this.miOpenExclusive.Text = "Transmit (e&xclusive)";
			this.miOpenExclusive.ToolTipText = "Connect to the card (using T=0 or T=1 protocol, exclusive mode) and open the dial" +
	"og to Transmit APDUs to the card";
			this.miOpenExclusive.Click += new System.EventHandler(this.MiOpenExclusiveClick);
			// 
			// miOpenDirect
			// 
			this.miOpenDirect.Name = "miOpenDirect";
			this.miOpenDirect.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
			this.miOpenDirect.Size = new System.Drawing.Size(362, 22);
			this.miOpenDirect.Text = "Control (&direct)";
			this.miOpenDirect.ToolTipText = "Connect directly to the reader (DIRECT protocol) and open the dialog to send Cont" +
	"rol commands to the reader";
			this.miOpenDirect.Click += new System.EventHandler(this.MiOpenDirectClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(359, 6);
			// 
			// miAtrAnalysis
			// 
			this.miAtrAnalysis.Name = "miAtrAnalysis";
			this.miAtrAnalysis.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
			this.miAtrAnalysis.Size = new System.Drawing.Size(362, 22);
			this.miAtrAnalysis.Text = "&ATR analysis, card info";
			this.miAtrAnalysis.ToolTipText = "Parse the ATR and try to recognize the card";
			this.miAtrAnalysis.Click += new System.EventHandler(this.MiAtrAnalysisClick);
			// 
			// miAtrCopy
			// 
			this.miAtrCopy.Name = "miAtrCopy";
			this.miAtrCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.miAtrCopy.Size = new System.Drawing.Size(362, 22);
			this.miAtrCopy.Text = "&Copy ATR to clipboard";
			this.miAtrCopy.ToolTipText = "Copy the ATR to the clipboard, so that you may paste it in another application";
			this.miAtrCopy.Click += new System.EventHandler(this.MiAtrCopyClick);
			// 
			// miATRRegistry
			// 
			this.miATRRegistry.Name = "miATRRegistry";
			this.miATRRegistry.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
			this.miATRRegistry.Size = new System.Drawing.Size(362, 22);
			this.miATRRegistry.Text = "Add this ATR to the no-driver list (Admin. only)";
			this.miATRRegistry.ToolTipText = "Copy the ATR to the local registry and add a minidriver for this card";
			this.miATRRegistry.Click += new System.EventHandler(this.MiATRRegistryClick);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(359, 6);
			// 
			// miReaderInfo
			// 
			this.miReaderInfo.Name = "miReaderInfo";
			this.miReaderInfo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.miReaderInfo.Size = new System.Drawing.Size(362, 22);
			this.miReaderInfo.Text = "&Reader info";
			this.miReaderInfo.ToolTipText = "Retrieve reader informations";
			this.miReaderInfo.Click += new System.EventHandler(this.MiReaderInfoClick);
			// 
			// readerImages
			// 
			this.readerImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("readerImages.ImageStream")));
			this.readerImages.TransparentColor = System.Drawing.Color.White;
			this.readerImages.Images.SetKeyName(0, "generic-reader.png");
			this.readerImages.Images.SetKeyName(1, "contactless.png");
			this.readerImages.Images.SetKeyName(2, "card.png");
			this.readerImages.Images.SetKeyName(3, "simsam.png");
			// 
			// pMain
			// 
			this.pMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pMain.Location = new System.Drawing.Point(0, 155);
			this.pMain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pMain.Name = "pMain";
			this.pMain.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pMain.Size = new System.Drawing.Size(1008, 545);
			this.pMain.TabIndex = 5;
			this.pMain.Paint += new System.Windows.Forms.PaintEventHandler(this.PMainPaint);
			// 
			// pTitle
			// 
			this.pTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.pTitle.Controls.Add(this.panel1);
			this.pTitle.Controls.Add(this.lbReaders);
			this.pTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.pTitle.Location = new System.Drawing.Point(0, 95);
			this.pTitle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pTitle.Name = "pTitle";
			this.pTitle.Size = new System.Drawing.Size(1008, 32);
			this.pTitle.TabIndex = 28;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.lbOptions);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(436, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(572, 32);
			this.panel1.TabIndex = 22;
			// 
			// lbOptions
			// 
			this.lbOptions.ForeColor = System.Drawing.Color.Black;
			this.lbOptions.Location = new System.Drawing.Point(48, 7);
			this.lbOptions.Name = "lbOptions";
			this.lbOptions.Size = new System.Drawing.Size(521, 22);
			this.lbOptions.TabIndex = 21;
			this.lbOptions.Text = "?";
			this.lbOptions.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lbReaders
			// 
			this.lbReaders.AutoSize = true;
			this.lbReaders.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbReaders.ForeColor = System.Drawing.SystemColors.MenuText;
			this.lbReaders.Location = new System.Drawing.Point(8, 7);
			this.lbReaders.Name = "lbReaders";
			this.lbReaders.Size = new System.Drawing.Size(95, 18);
			this.lbReaders.TabIndex = 20;
			this.lbReaders.Text = "PC/SC readers";
			// 
			// pControl
			// 
			this.pControl.BackColor = System.Drawing.SystemColors.Control;
			this.pControl.Controls.Add(this.lbMessage);
			this.pControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pControl.Location = new System.Drawing.Point(0, 700);
			this.pControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pControl.Name = "pControl";
			this.pControl.Size = new System.Drawing.Size(1008, 29);
			this.pControl.TabIndex = 29;
			// 
			// lbMessage
			// 
			this.lbMessage.AutoSize = true;
			this.lbMessage.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbMessage.ForeColor = System.Drawing.SystemColors.MenuText;
			this.lbMessage.Location = new System.Drawing.Point(8, 4);
			this.lbMessage.Name = "lbMessage";
			this.lbMessage.Size = new System.Drawing.Size(0, 18);
			this.lbMessage.TabIndex = 21;
			// 
			// statusImages
			// 
			this.statusImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("statusImages.ImageStream")));
			this.statusImages.TransparentColor = System.Drawing.Color.White;
			this.statusImages.Images.SetKeyName(0, "unknown.png");
			this.statusImages.Images.SetKeyName(1, "error.png");
			this.statusImages.Images.SetKeyName(2, "reader-unavailable.png");
			this.statusImages.Images.SetKeyName(3, "card-absent.png");
			this.statusImages.Images.SetKeyName(4, "card-present.png");
			this.statusImages.Images.SetKeyName(5, "card-mute.png");
			this.statusImages.Images.SetKeyName(6, "card-exclusive.png");
			this.statusImages.Images.SetKeyName(7, "card-inuse.png");
			// 
			// pHeader
			// 
			this.pHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.pHeader.Controls.Add(this.label3);
			this.pHeader.Controls.Add(this.label2);
			this.pHeader.Controls.Add(this.label1);
			this.pHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.pHeader.Location = new System.Drawing.Point(0, 127);
			this.pHeader.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pHeader.Name = "pHeader";
			this.pHeader.Size = new System.Drawing.Size(1008, 28);
			this.pHeader.TabIndex = 30;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.label3.Location = new System.Drawing.Point(550, 7);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(61, 18);
			this.label3.TabIndex = 2;
			this.label3.Text = "Card ATR";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.label2.Location = new System.Drawing.Point(405, 7);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(46, 18);
			this.label2.TabIndex = 1;
			this.label2.Text = "Status";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.label1.Location = new System.Drawing.Point(30, 7);
			this.label1.Name = "label1";
			this.label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label1.Size = new System.Drawing.Size(89, 18);
			this.label1.TabIndex = 0;
			this.label1.Text = "Reader Name";
			// 
			// toolTip
			// 
			this.toolTip.Active = false;
			this.toolTip.AutomaticDelay = 150;
			// 
			// pTop
			// 
			this.pTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
			this.pTop.Controls.Add(this.pLogo);
			this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.pTop.Location = new System.Drawing.Point(0, 25);
			this.pTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pTop.Name = "pTop";
			this.pTop.Size = new System.Drawing.Size(1008, 70);
			this.pTop.TabIndex = 31;
			// 
			// pLogo
			// 
			this.pLogo.Controls.Add(this.pictureBox1);
			this.pLogo.Dock = System.Windows.Forms.DockStyle.Right;
			this.pLogo.Location = new System.Drawing.Point(699, 0);
			this.pLogo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pLogo.Name = "pLogo";
			this.pLogo.Size = new System.Drawing.Size(309, 70);
			this.pLogo.TabIndex = 11;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(3, 4);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(300, 60);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.ClientSize = new System.Drawing.Size(1008, 729);
			this.Controls.Add(this.pMain);
			this.Controls.Add(this.pHeader);
			this.Controls.Add(this.pControl);
			this.Controls.Add(this.pTitle);
			this.Controls.Add(this.pTop);
			this.Controls.Add(this.menuMain);
			this.Font = new System.Drawing.Font("Calibri Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuMain;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MinimumSize = new System.Drawing.Size(1024, 768);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SpringCard PC/SC Diagnostic";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormFormClosed);
			this.Load += new System.EventHandler(this.MainFormLoad);
			this.Shown += new System.EventHandler(this.MainFormShown);
			this.SizeChanged += new System.EventHandler(this.MainFormSizeChanged);
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.readerPopupMenu.ResumeLayout(false);
			this.pTitle.ResumeLayout(false);
			this.pTitle.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.pControl.ResumeLayout(false);
			this.pControl.PerformLayout();
			this.pHeader.ResumeLayout(false);
			this.pHeader.PerformLayout();
			this.pTop.ResumeLayout(false);
			this.pLogo.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.ToolStripMenuItem miATRRegistry;
		private System.Windows.Forms.Label lbOptions;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem pCSCOptionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem miOpenShared;
		private System.Windows.Forms.ToolStripMenuItem miOpenDirect;
		private System.Windows.Forms.ToolStripMenuItem miOpenExclusive;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.ToolStripMenuItem miReaderInfo;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem miAtrCopy;
		private System.Windows.Forms.ToolStripMenuItem miAtrAnalysis;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel pHeader;
		private System.Windows.Forms.ImageList statusImages;
		private System.Windows.Forms.ImageList readerImages;
		private System.Windows.Forms.Panel pControl;
		private System.Windows.Forms.Label lbReaders;
		private System.Windows.Forms.Panel pTitle;
		private System.Windows.Forms.MenuStrip menuMain;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip readerPopupMenu;
		private System.Windows.Forms.Panel pMain;
		private System.Windows.Forms.Panel pTop;
		private System.Windows.Forms.Panel pLogo;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lbMessage;
		

	}
}
