namespace SpringCardApplication
{
  partial class MainForm
  {
    /// <summary>
    /// Variable nécessaire au concepteur.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }
    
    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.Windows.Forms.ContextMenu contextMenu;
    private System.Windows.Forms.MenuItem menuItemSettings;
    private System.Windows.Forms.MenuItem menuItemEnabled;
    private System.Windows.Forms.MenuItem menuItemAlwaysOpen;
    private System.Windows.Forms.MenuItem menuItemQuit;

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
    	this.components = new System.ComponentModel.Container();
    	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
    	this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
    	this.contextMenu = new System.Windows.Forms.ContextMenu();
    	this.menuItemQuit = new System.Windows.Forms.MenuItem();
    	this.menuItemSettings = new System.Windows.Forms.MenuItem();
    	this.menuItemEnabled = new System.Windows.Forms.MenuItem();
    	this.menuItemAlwaysOpen = new System.Windows.Forms.MenuItem();
    	this.menuStrip1 = new System.Windows.Forms.MenuStrip();
    	this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    	this.miReader = new System.Windows.Forms.ToolStripMenuItem();
    	this.miEnableLock = new System.Windows.Forms.ToolStripMenuItem();
    	this.miShowConsole = new System.Windows.Forms.ToolStripMenuItem();
    	this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
    	this.miQuit = new System.Windows.Forms.ToolStripMenuItem();
    	this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    	this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
    	this.statusStrip1 = new System.Windows.Forms.StatusStrip();
    	this.lbReaderName = new System.Windows.Forms.ToolStripStatusLabel();
    	this.eReaderName = new System.Windows.Forms.ToolStripStatusLabel();
    	this.lbReaderStatus = new System.Windows.Forms.ToolStripStatusLabel();
    	this.eReaderStatus = new System.Windows.Forms.ToolStripStatusLabel();
    	this.lbCardAtr = new System.Windows.Forms.ToolStripStatusLabel();
    	this.eCardAtr = new System.Windows.Forms.ToolStripStatusLabel();
    	this.pLeft = new System.Windows.Forms.Panel();
    	this.pBottom = new System.Windows.Forms.Panel();
    	this.btCreateNfcTag = new System.Windows.Forms.Button();
    	this.cbLock = new System.Windows.Forms.CheckBox();
    	this.btnWrite = new System.Windows.Forms.Button();
    	this.pMain = new System.Windows.Forms.Panel();
    	this.imgHeader = new System.Windows.Forms.PictureBox();
    	this.pictureBox1 = new System.Windows.Forms.PictureBox();
    	this.menuStrip1.SuspendLayout();
    	this.statusStrip1.SuspendLayout();
    	this.pBottom.SuspendLayout();
    	((System.ComponentModel.ISupportInitialize)(this.imgHeader)).BeginInit();
    	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
    	this.SuspendLayout();
    	// 
    	// notifyIcon
    	// 
    	this.notifyIcon.ContextMenu = this.contextMenu;
    	this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
    	this.notifyIcon.Text = "NFC Tool";
    	this.notifyIcon.Visible = true;
    	// 
    	// contextMenu
    	// 
    	this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.menuItemQuit});
    	// 
    	// menuItemQuit
    	// 
    	this.menuItemQuit.Index = 0;
    	this.menuItemQuit.Text = "&Quit";
    	this.menuItemQuit.Click += new System.EventHandler(this.menuItemQuit_Click);
    	// 
    	// menuItemSettings
    	// 
    	this.menuItemSettings.Index = -1;
    	this.menuItemSettings.Text = "";
    	// 
    	// menuItemEnabled
    	// 
    	this.menuItemEnabled.Index = -1;
    	this.menuItemEnabled.Text = "";
    	// 
    	// menuItemAlwaysOpen
    	// 
    	this.menuItemAlwaysOpen.Index = -1;
    	this.menuItemAlwaysOpen.Text = "";
    	// 
    	// menuStrip1
    	// 
    	this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem,
			this.aboutToolStripMenuItem});
    	this.menuStrip1.Location = new System.Drawing.Point(0, 0);
    	this.menuStrip1.Name = "menuStrip1";
    	this.menuStrip1.Size = new System.Drawing.Size(875, 24);
    	this.menuStrip1.TabIndex = 22;
    	this.menuStrip1.Text = "menuStrip1";
    	// 
    	// fileToolStripMenuItem
    	// 
    	this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.miReader,
			this.miEnableLock,
			this.miShowConsole,
			this.toolStripSeparator1,
			this.miQuit});
    	this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
    	this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
    	this.fileToolStripMenuItem.Text = "File";
    	// 
    	// miReader
    	// 
    	this.miReader.Image = ((System.Drawing.Image)(resources.GetObject("miReader.Image")));
    	this.miReader.Name = "miReader";
    	this.miReader.Size = new System.Drawing.Size(184, 22);
    	this.miReader.Text = "Change reader";
    	this.miReader.Click += new System.EventHandler(this.MiReaderClick);
    	// 
    	// miEnableLock
    	// 
    	this.miEnableLock.CheckOnClick = true;
    	this.miEnableLock.Image = ((System.Drawing.Image)(resources.GetObject("miEnableLock.Image")));
    	this.miEnableLock.Name = "miEnableLock";
    	this.miEnableLock.Size = new System.Drawing.Size(184, 22);
    	this.miEnableLock.Text = "Enable Tag locking";
    	this.miEnableLock.CheckedChanged += new System.EventHandler(this.MiEnableLockCheckedChanged);
    	// 
    	// miShowConsole
    	// 
    	this.miShowConsole.CheckOnClick = true;
    	this.miShowConsole.Image = ((System.Drawing.Image)(resources.GetObject("miShowConsole.Image")));
    	this.miShowConsole.Name = "miShowConsole";
    	this.miShowConsole.Size = new System.Drawing.Size(184, 22);
    	this.miShowConsole.Text = "Show debug console";
    	this.miShowConsole.CheckedChanged += new System.EventHandler(this.MiShowConsoleCheckedChanged);
    	// 
    	// toolStripSeparator1
    	// 
    	this.toolStripSeparator1.Name = "toolStripSeparator1";
    	this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
    	// 
    	// miQuit
    	// 
    	this.miQuit.Image = ((System.Drawing.Image)(resources.GetObject("miQuit.Image")));
    	this.miQuit.Name = "miQuit";
    	this.miQuit.Size = new System.Drawing.Size(184, 22);
    	this.miQuit.Text = "E&xit";
    	this.miQuit.Click += new System.EventHandler(this.QuitToolStripMenuItemClick);
    	// 
    	// aboutToolStripMenuItem
    	// 
    	this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.aboutToolStripMenuItem1});
    	this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
    	this.aboutToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
    	this.aboutToolStripMenuItem.Text = "Help";
    	// 
    	// aboutToolStripMenuItem1
    	// 
    	this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
    	this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
    	this.aboutToolStripMenuItem1.Text = "About";
    	this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.AboutToolStripMenuItem1Click);
    	// 
    	// statusStrip1
    	// 
    	this.statusStrip1.BackColor = System.Drawing.SystemColors.Control;
    	this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.lbReaderName,
			this.eReaderName,
			this.lbReaderStatus,
			this.eReaderStatus,
			this.lbCardAtr,
			this.eCardAtr});
    	this.statusStrip1.Location = new System.Drawing.Point(0, 553);
    	this.statusStrip1.Name = "statusStrip1";
    	this.statusStrip1.Size = new System.Drawing.Size(875, 22);
    	this.statusStrip1.SizingGrip = false;
    	this.statusStrip1.TabIndex = 26;
    	this.statusStrip1.Text = "statusStrip1";
    	// 
    	// lbReaderName
    	// 
    	this.lbReaderName.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.lbReaderName.Name = "lbReaderName";
    	this.lbReaderName.Size = new System.Drawing.Size(46, 17);
    	this.lbReaderName.Text = "Reader:";
    	// 
    	// eReaderName
    	// 
    	this.eReaderName.AutoSize = false;
    	this.eReaderName.BackColor = System.Drawing.SystemColors.ControlLight;
    	this.eReaderName.Name = "eReaderName";
    	this.eReaderName.Size = new System.Drawing.Size(250, 17);
    	this.eReaderName.Text = "No reader selected";
    	this.eReaderName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
    	// 
    	// lbReaderStatus
    	// 
    	this.lbReaderStatus.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.lbReaderStatus.Name = "lbReaderStatus";
    	this.lbReaderStatus.Size = new System.Drawing.Size(42, 17);
    	this.lbReaderStatus.Text = "Status:";
    	// 
    	// eReaderStatus
    	// 
    	this.eReaderStatus.AutoSize = false;
    	this.eReaderStatus.BackColor = System.Drawing.SystemColors.ControlLight;
    	this.eReaderStatus.Name = "eReaderStatus";
    	this.eReaderStatus.Size = new System.Drawing.Size(120, 17);
    	this.eReaderStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
    	// 
    	// lbCardAtr
    	// 
    	this.lbCardAtr.Name = "lbCardAtr";
    	this.lbCardAtr.Size = new System.Drawing.Size(59, 17);
    	this.lbCardAtr.Text = "Card ATR:";
    	// 
    	// eCardAtr
    	// 
    	this.eCardAtr.BackColor = System.Drawing.SystemColors.ControlLight;
    	this.eCardAtr.Name = "eCardAtr";
    	this.eCardAtr.Size = new System.Drawing.Size(0, 17);
    	// 
    	// pLeft
    	// 
    	this.pLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(141)))), ((int)(((byte)(128)))));
    	this.pLeft.Dock = System.Windows.Forms.DockStyle.Left;
    	this.pLeft.Location = new System.Drawing.Point(0, 84);
    	this.pLeft.Name = "pLeft";
    	this.pLeft.Size = new System.Drawing.Size(206, 421);
    	this.pLeft.TabIndex = 28;
    	// 
    	// pBottom
    	// 
    	this.pBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(73)))), ((int)(((byte)(67)))));
    	this.pBottom.Controls.Add(this.btCreateNfcTag);
    	this.pBottom.Controls.Add(this.cbLock);
    	this.pBottom.Controls.Add(this.btnWrite);
    	this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
    	this.pBottom.Location = new System.Drawing.Point(0, 505);
    	this.pBottom.Name = "pBottom";
    	this.pBottom.Size = new System.Drawing.Size(875, 48);
    	this.pBottom.TabIndex = 29;
    	// 
    	// btCreateNfcTag
    	// 
    	this.btCreateNfcTag.Anchor = System.Windows.Forms.AnchorStyles.Right;
    	this.btCreateNfcTag.BackColor = System.Drawing.SystemColors.ControlLight;
    	this.btCreateNfcTag.Enabled = false;
    	this.btCreateNfcTag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btCreateNfcTag.Image = ((System.Drawing.Image)(resources.GetObject("btCreateNfcTag.Image")));
    	this.btCreateNfcTag.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
    	this.btCreateNfcTag.Location = new System.Drawing.Point(730, 7);
    	this.btCreateNfcTag.Name = "btCreateNfcTag";
    	this.btCreateNfcTag.Size = new System.Drawing.Size(133, 38);
    	this.btCreateNfcTag.TabIndex = 18;
    	this.btCreateNfcTag.Text = "Create an NFC Tag";
    	this.btCreateNfcTag.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
    	this.btCreateNfcTag.UseVisualStyleBackColor = false;
    	this.btCreateNfcTag.Visible = false;
    	this.btCreateNfcTag.Click += new System.EventHandler(this.BtCreateNfcTagClick);
    	// 
    	// cbLock
    	// 
    	this.cbLock.ForeColor = System.Drawing.Color.White;
    	this.cbLock.Location = new System.Drawing.Point(486, 14);
    	this.cbLock.Name = "cbLock";
    	this.cbLock.Size = new System.Drawing.Size(157, 24);
    	this.cbLock.TabIndex = 17;
    	this.cbLock.Text = "Lock the Tag after writing";
    	this.cbLock.UseVisualStyleBackColor = true;
    	// 
    	// btnWrite
    	// 
    	this.btnWrite.Anchor = System.Windows.Forms.AnchorStyles.Right;
    	this.btnWrite.BackColor = System.Drawing.SystemColors.ControlLight;
    	this.btnWrite.Enabled = false;
    	this.btnWrite.Image = ((System.Drawing.Image)(resources.GetObject("btnWrite.Image")));
    	this.btnWrite.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
    	this.btnWrite.Location = new System.Drawing.Point(730, 6);
    	this.btnWrite.Name = "btnWrite";
    	this.btnWrite.Size = new System.Drawing.Size(133, 38);
    	this.btnWrite.TabIndex = 16;
    	this.btnWrite.Text = "Write to the Tag";
    	this.btnWrite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
    	this.btnWrite.UseVisualStyleBackColor = false;
    	this.btnWrite.Click += new System.EventHandler(this.BtnWriteClick);
    	// 
    	// pMain
    	// 
    	this.pMain.BackColor = System.Drawing.Color.White;
    	this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.pMain.Location = new System.Drawing.Point(206, 84);
    	this.pMain.Name = "pMain";
    	this.pMain.Padding = new System.Windows.Forms.Padding(3);
    	this.pMain.Size = new System.Drawing.Size(669, 421);
    	this.pMain.TabIndex = 30;
    	// 
    	// imgHeader
    	// 
    	this.imgHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(73)))), ((int)(((byte)(67)))));
    	this.imgHeader.Dock = System.Windows.Forms.DockStyle.Top;
    	this.imgHeader.Location = new System.Drawing.Point(0, 24);
    	this.imgHeader.Name = "imgHeader";
    	this.imgHeader.Size = new System.Drawing.Size(875, 60);
    	this.imgHeader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
    	this.imgHeader.TabIndex = 27;
    	this.imgHeader.TabStop = false;
    	this.imgHeader.Click += new System.EventHandler(this.ImgHeaderClick);
    	// 
    	// pictureBox1
    	// 
    	this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(73)))), ((int)(((byte)(67)))));
    	this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
    	this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
    	this.pictureBox1.Location = new System.Drawing.Point(683, 32);
    	this.pictureBox1.Name = "pictureBox1";
    	this.pictureBox1.Size = new System.Drawing.Size(192, 52);
    	this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
    	this.pictureBox1.TabIndex = 31;
    	this.pictureBox1.TabStop = false;
    	this.pictureBox1.Click += new System.EventHandler(this.PictureBox1Click);
    	// 
    	// MainForm
    	// 
    	this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
    	this.BackColor = System.Drawing.SystemColors.Control;
    	this.ClientSize = new System.Drawing.Size(875, 575);
    	this.Controls.Add(this.pictureBox1);
    	this.Controls.Add(this.pMain);
    	this.Controls.Add(this.pLeft);
    	this.Controls.Add(this.pBottom);
    	this.Controls.Add(this.imgHeader);
    	this.Controls.Add(this.statusStrip1);
    	this.Controls.Add(this.menuStrip1);
    	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
    	this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
    	this.MainMenuStrip = this.menuStrip1;
    	this.MaximizeBox = false;
    	this.Name = "MainForm";
    	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
    	this.Text = "NFC Tool";
    	this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormClosed);
    	this.Load += new System.EventHandler(this.MainFormLoad);
    	this.Shown += new System.EventHandler(this.MainFormShown);
    	this.menuStrip1.ResumeLayout(false);
    	this.menuStrip1.PerformLayout();
    	this.statusStrip1.ResumeLayout(false);
    	this.statusStrip1.PerformLayout();
    	this.pBottom.ResumeLayout(false);
    	((System.ComponentModel.ISupportInitialize)(this.imgHeader)).EndInit();
    	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
    	this.ResumeLayout(false);
    	this.PerformLayout();

    }
    
    private System.Windows.Forms.Button btCreateNfcTag;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem miShowConsole;
    private System.Windows.Forms.CheckBox cbLock;
    private System.Windows.Forms.ToolStripStatusLabel eCardAtr;
    private System.Windows.Forms.ToolStripStatusLabel lbCardAtr;
    private System.Windows.Forms.ToolStripStatusLabel eReaderStatus;
    private System.Windows.Forms.ToolStripStatusLabel lbReaderStatus;
    private System.Windows.Forms.ToolStripStatusLabel eReaderName;
    private System.Windows.Forms.ToolStripStatusLabel lbReaderName;
    private System.Windows.Forms.Panel pMain;
    private System.Windows.Forms.Panel pBottom;
    private System.Windows.Forms.Panel pLeft;
    private System.Windows.Forms.PictureBox imgHeader;
    private System.Windows.Forms.StatusStrip statusStrip1;
//    private System.Windows.Forms.TextBox Birthday;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem miEnableLock;
    private System.Windows.Forms.Button btnWrite;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem miReader;
    private System.Windows.Forms.ToolStripMenuItem miQuit;
    private System.Windows.Forms.PictureBox pictureBox1;


  }
}
