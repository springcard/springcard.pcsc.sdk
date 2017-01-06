/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCardApplication
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
			this.miReaderChange = new System.Windows.Forms.ToolStripMenuItem();
			this.miShowConsole = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.miQuit = new System.Windows.Forms.ToolStripMenuItem();
			this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.miStart = new System.Windows.Forms.ToolStripMenuItem();
			this.miStop = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.miWrite = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tooltipReader = new System.Windows.Forms.ToolTip(this.components);
			this.stripStatus = new System.Windows.Forms.StatusStrip();
			this.tmrWatchReaders = new System.Windows.Forms.Timer(this.components);
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.pControl = new System.Windows.Forms.Panel();
			this.btnStart = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.rbType4 = new System.Windows.Forms.RadioButton();
			this.rbType2 = new System.Windows.Forms.RadioButton();
			this.btnReaderChange = new System.Windows.Forms.Button();
			this.eReaderStatus = new System.Windows.Forms.TextBox();
			this.lbStatus = new System.Windows.Forms.Label();
			this.eCardAtr = new System.Windows.Forms.TextBox();
			this.lbCardAtr = new System.Windows.Forms.Label();
			this.eReaderName = new System.Windows.Forms.TextBox();
			this.lbReader = new System.Windows.Forms.Label();
			this.pTitle = new System.Windows.Forms.Panel();
			this.label19 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.imgHeader = new System.Windows.Forms.PictureBox();
			this.btnWrite = new System.Windows.Forms.Button();
			this.pNdefType = new System.Windows.Forms.Panel();
			this.cbNdefType = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.pMain = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.menuMain.SuspendLayout();
			this.pControl.SuspendLayout();
			this.pTitle.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.imgHeader)).BeginInit();
			this.pNdefType.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// menuMain
			// 
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem,
			this.controlToolStripMenuItem,
			this.aboutToolStripMenuItem});
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.Name = "menuMain";
			this.menuMain.Size = new System.Drawing.Size(794, 24);
			this.menuMain.TabIndex = 1;
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.miReaderChange,
			this.miShowConsole,
			this.toolStripMenuItem1,
			this.miQuit});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// miReaderChange
			// 
			this.miReaderChange.Image = ((System.Drawing.Image)(resources.GetObject("miReaderChange.Image")));
			this.miReaderChange.Name = "miReaderChange";
			this.miReaderChange.Size = new System.Drawing.Size(184, 22);
			this.miReaderChange.Text = "Change &reader";
			this.miReaderChange.Click += new System.EventHandler(this.BtnReaderChangeClick);
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
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(181, 6);
			// 
			// miQuit
			// 
			this.miQuit.Image = ((System.Drawing.Image)(resources.GetObject("miQuit.Image")));
			this.miQuit.Name = "miQuit";
			this.miQuit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.miQuit.Size = new System.Drawing.Size(184, 22);
			this.miQuit.Text = "&Quit";
			this.miQuit.Click += new System.EventHandler(this.QuitToolStripMenuItemClick);
			// 
			// controlToolStripMenuItem
			// 
			this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.miStart,
			this.miStop,
			this.toolStripMenuItem3,
			this.miWrite});
			this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
			this.controlToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
			this.controlToolStripMenuItem.Text = "&Control";
			// 
			// miStart
			// 
			this.miStart.Image = ((System.Drawing.Image)(resources.GetObject("miStart.Image")));
			this.miStart.Name = "miStart";
			this.miStart.ShortcutKeys = System.Windows.Forms.Keys.F2;
			this.miStart.Size = new System.Drawing.Size(282, 22);
			this.miStart.Text = "Start NFC Tag emulation";
			this.miStart.Click += new System.EventHandler(this.BtnStartClick);
			// 
			// miStop
			// 
			this.miStop.Image = ((System.Drawing.Image)(resources.GetObject("miStop.Image")));
			this.miStop.Name = "miStop";
			this.miStop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
			this.miStop.Size = new System.Drawing.Size(282, 22);
			this.miStop.Text = "Stop NFC Tag emulation";
			this.miStop.Click += new System.EventHandler(this.BtnStopClick);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(279, 6);
			// 
			// miWrite
			// 
			this.miWrite.Image = ((System.Drawing.Image)(resources.GetObject("miWrite.Image")));
			this.miWrite.Name = "miWrite";
			this.miWrite.ShortcutKeyDisplayString = "";
			this.miWrite.ShortcutKeys = System.Windows.Forms.Keys.F6;
			this.miWrite.Size = new System.Drawing.Size(282, 22);
			this.miWrite.Text = "Write the NDEF to the emulated Tag";
			this.miWrite.Click += new System.EventHandler(this.BtnWriteClick);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
			this.aboutToolStripMenuItem.Text = "&About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
			// 
			// stripStatus
			// 
			this.stripStatus.Location = new System.Drawing.Point(0, 553);
			this.stripStatus.Name = "stripStatus";
			this.stripStatus.Size = new System.Drawing.Size(794, 22);
			this.stripStatus.SizingGrip = false;
			this.stripStatus.TabIndex = 2;
			// 
			// tmrWatchReaders
			// 
			this.tmrWatchReaders.Interval = 250;
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "stop.png");
			this.imageList.Images.SetKeyName(1, "pause.png");
			this.imageList.Images.SetKeyName(2, "play.png");
			this.imageList.Images.SetKeyName(3, "record.png");
			this.imageList.Images.SetKeyName(4, "settings.png");
			this.imageList.Images.SetKeyName(5, "delete.png");
			this.imageList.Images.SetKeyName(6, "error.png");
			this.imageList.Images.SetKeyName(7, "download.png");
			this.imageList.Images.SetKeyName(8, "sinchronize.png");
			this.imageList.Images.SetKeyName(9, "upload.png");
			this.imageList.Images.SetKeyName(10, "online.png");
			this.imageList.Images.SetKeyName(11, "card_in_use.png");
			this.imageList.Images.SetKeyName(12, "card_inserting.png");
			// 
			// pControl
			// 
			this.pControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(217)))), ((int)(((byte)(213)))));
			this.pControl.Controls.Add(this.btnStart);
			this.pControl.Controls.Add(this.btnStop);
			this.pControl.Controls.Add(this.rbType4);
			this.pControl.Controls.Add(this.rbType2);
			this.pControl.Controls.Add(this.btnReaderChange);
			this.pControl.Controls.Add(this.eReaderStatus);
			this.pControl.Controls.Add(this.lbStatus);
			this.pControl.Controls.Add(this.eCardAtr);
			this.pControl.Controls.Add(this.lbCardAtr);
			this.pControl.Controls.Add(this.eReaderName);
			this.pControl.Controls.Add(this.lbReader);
			this.pControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.pControl.Location = new System.Drawing.Point(0, 114);
			this.pControl.Name = "pControl";
			this.pControl.Size = new System.Drawing.Size(794, 117);
			this.pControl.TabIndex = 4;
			// 
			// btnStart
			// 
			this.btnStart.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
			this.btnStart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
			this.btnStart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gainsboro;
			this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnStart.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.btnStart.ImageIndex = 2;
			this.btnStart.ImageList = this.imageList;
			this.btnStart.Location = new System.Drawing.Point(665, 15);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(107, 83);
			this.btnStart.TabIndex = 13;
			this.btnStart.Text = "Start NFC Tag emulation";
			this.btnStart.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.BtnStartClick);
			// 
			// btnStop
			// 
			this.btnStop.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
			this.btnStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
			this.btnStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gainsboro;
			this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnStop.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.btnStop.ImageIndex = 0;
			this.btnStop.ImageList = this.imageList;
			this.btnStop.Location = new System.Drawing.Point(552, 15);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(107, 83);
			this.btnStop.TabIndex = 12;
			this.btnStop.Text = "Stop NFC Tag emulation";
			this.btnStop.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.BtnStopClick);
			// 
			// rbType4
			// 
			this.rbType4.Checked = true;
			this.rbType4.Location = new System.Drawing.Point(305, 61);
			this.rbType4.Name = "rbType4";
			this.rbType4.Size = new System.Drawing.Size(172, 24);
			this.rbType4.TabIndex = 11;
			this.rbType4.TabStop = true;
			this.rbType4.Text = "Emulate a type 4 NFC Tag";
			this.rbType4.UseVisualStyleBackColor = true;
			// 
			// rbType2
			// 
			this.rbType2.Location = new System.Drawing.Point(100, 61);
			this.rbType2.Name = "rbType2";
			this.rbType2.Size = new System.Drawing.Size(172, 24);
			this.rbType2.TabIndex = 10;
			this.rbType2.Text = "Emulate a type 2 NFC Tag";
			this.rbType2.UseVisualStyleBackColor = true;
			// 
			// btnReaderChange
			// 
			this.btnReaderChange.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
			this.btnReaderChange.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
			this.btnReaderChange.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gainsboro;
			this.btnReaderChange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnReaderChange.ImageIndex = 12;
			this.btnReaderChange.ImageList = this.imageList;
			this.btnReaderChange.Location = new System.Drawing.Point(502, 13);
			this.btnReaderChange.Name = "btnReaderChange";
			this.btnReaderChange.Size = new System.Drawing.Size(34, 26);
			this.btnReaderChange.TabIndex = 9;
			this.btnReaderChange.UseVisualStyleBackColor = true;
			this.btnReaderChange.Click += new System.EventHandler(this.BtnReaderChangeClick);
			// 
			// eReaderStatus
			// 
			this.eReaderStatus.BackColor = System.Drawing.Color.White;
			this.eReaderStatus.Location = new System.Drawing.Point(100, 40);
			this.eReaderStatus.Name = "eReaderStatus";
			this.eReaderStatus.ReadOnly = true;
			this.eReaderStatus.Size = new System.Drawing.Size(396, 20);
			this.eReaderStatus.TabIndex = 8;
			// 
			// lbStatus
			// 
			this.lbStatus.Location = new System.Drawing.Point(18, 43);
			this.lbStatus.Name = "lbStatus";
			this.lbStatus.Size = new System.Drawing.Size(100, 17);
			this.lbStatus.TabIndex = 7;
			this.lbStatus.Text = "Status :";
			// 
			// eCardAtr
			// 
			this.eCardAtr.BackColor = System.Drawing.Color.White;
			this.eCardAtr.Location = new System.Drawing.Point(100, 91);
			this.eCardAtr.Name = "eCardAtr";
			this.eCardAtr.ReadOnly = true;
			this.eCardAtr.Size = new System.Drawing.Size(396, 20);
			this.eCardAtr.TabIndex = 6;
			// 
			// lbCardAtr
			// 
			this.lbCardAtr.Location = new System.Drawing.Point(19, 94);
			this.lbCardAtr.Name = "lbCardAtr";
			this.lbCardAtr.Size = new System.Drawing.Size(100, 17);
			this.lbCardAtr.TabIndex = 5;
			this.lbCardAtr.Text = "Card\'s ATR :";
			// 
			// eReaderName
			// 
			this.eReaderName.BackColor = System.Drawing.Color.White;
			this.eReaderName.Location = new System.Drawing.Point(100, 15);
			this.eReaderName.Name = "eReaderName";
			this.eReaderName.ReadOnly = true;
			this.eReaderName.Size = new System.Drawing.Size(396, 20);
			this.eReaderName.TabIndex = 1;
			// 
			// lbReader
			// 
			this.lbReader.Location = new System.Drawing.Point(19, 18);
			this.lbReader.Name = "lbReader";
			this.lbReader.Size = new System.Drawing.Size(100, 17);
			this.lbReader.TabIndex = 0;
			this.lbReader.Text = "Reader :";
			// 
			// pTitle
			// 
			this.pTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(141)))), ((int)(((byte)(128)))));
			this.pTitle.Controls.Add(this.label19);
			this.pTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.pTitle.Location = new System.Drawing.Point(0, 84);
			this.pTitle.Name = "pTitle";
			this.pTitle.Size = new System.Drawing.Size(794, 30);
			this.pTitle.TabIndex = 24;
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label19.ForeColor = System.Drawing.Color.White;
			this.label19.Location = new System.Drawing.Point(10, 7);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(153, 18);
			this.label19.TabIndex = 20;
			this.label19.Text = "NFC Tag emulation";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(141)))), ((int)(((byte)(128)))));
			this.panel1.Controls.Add(this.label1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 231);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(794, 30);
			this.panel1.TabIndex = 25;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(10, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(341, 18);
			this.label1.TabIndex = 20;
			this.label1.Text = "NDEF message to put into the emulated Tag";
			// 
			// imgHeader
			// 
			this.imgHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(73)))), ((int)(((byte)(67)))));
			this.imgHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.imgHeader.Location = new System.Drawing.Point(0, 24);
			this.imgHeader.Name = "imgHeader";
			this.imgHeader.Size = new System.Drawing.Size(794, 60);
			this.imgHeader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.imgHeader.TabIndex = 26;
			this.imgHeader.TabStop = false;
			// 
			// btnWrite
			// 
			this.btnWrite.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
			this.btnWrite.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
			this.btnWrite.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gainsboro;
			this.btnWrite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnWrite.Image = ((System.Drawing.Image)(resources.GetObject("btnWrite.Image")));
			this.btnWrite.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnWrite.Location = new System.Drawing.Point(552, 5);
			this.btnWrite.Name = "btnWrite";
			this.btnWrite.Size = new System.Drawing.Size(220, 38);
			this.btnWrite.TabIndex = 12;
			this.btnWrite.Text = "Write to the Tag";
			this.btnWrite.UseVisualStyleBackColor = true;
			this.btnWrite.Click += new System.EventHandler(this.BtnWriteClick);
			// 
			// pNdefType
			// 
			this.pNdefType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(217)))), ((int)(((byte)(213)))));
			this.pNdefType.Controls.Add(this.cbNdefType);
			this.pNdefType.Controls.Add(this.btnWrite);
			this.pNdefType.Controls.Add(this.label2);
			this.pNdefType.Dock = System.Windows.Forms.DockStyle.Top;
			this.pNdefType.Location = new System.Drawing.Point(0, 261);
			this.pNdefType.Name = "pNdefType";
			this.pNdefType.Size = new System.Drawing.Size(794, 48);
			this.pNdefType.TabIndex = 28;
			// 
			// cbNdefType
			// 
			this.cbNdefType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbNdefType.FormattingEnabled = true;
			this.cbNdefType.Items.AddRange(new object[] {
			"Text",
			"URI",
			"SmartPoster",
			"vCard",
			"Arbitrary MIME Media (text)",
			});
			this.cbNdefType.Location = new System.Drawing.Point(100, 16);
			this.cbNdefType.Name = "cbNdefType";
			this.cbNdefType.Size = new System.Drawing.Size(396, 21);
			this.cbNdefType.TabIndex = 7;
			this.cbNdefType.SelectedIndexChanged += new System.EventHandler(this.CbNdefTypeSelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(19, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 17);
			this.label2.TabIndex = 6;
			this.label2.Text = "Type :";
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.SystemColors.Window;
			this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel3.Location = new System.Drawing.Point(0, 309);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(94, 244);
			this.panel3.TabIndex = 30;
			// 
			// pMain
			// 
			this.pMain.BackColor = System.Drawing.SystemColors.Window;
			this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pMain.Location = new System.Drawing.Point(94, 309);
			this.pMain.Name = "pMain";
			this.pMain.Size = new System.Drawing.Size(700, 244);
			this.pMain.TabIndex = 31;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(73)))), ((int)(((byte)(67)))));
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
			this.pictureBox1.Location = new System.Drawing.Point(602, 32);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(192, 52);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 32;
			this.pictureBox1.TabStop = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(794, 575);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.pMain);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.pNdefType);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.pControl);
			this.Controls.Add(this.pTitle);
			this.Controls.Add(this.imgHeader);
			this.Controls.Add(this.stripStatus);
			this.Controls.Add(this.menuMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuMain;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SpringCard NfcTagEmul";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormFormClosed);
			this.Load += new System.EventHandler(this.MainFormLoad);
			this.Shown += new System.EventHandler(this.MainFormShown);
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.pControl.ResumeLayout(false);
			this.pControl.PerformLayout();
			this.pTitle.ResumeLayout(false);
			this.pTitle.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.imgHeader)).EndInit();
			this.pNdefType.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.ToolStripMenuItem miShowConsole;
		private System.Windows.Forms.Panel pMain;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbNdefType;
		private System.Windows.Forms.Panel pNdefType;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.RadioButton rbType2;
		private System.Windows.Forms.RadioButton rbType4;
		private System.Windows.Forms.ToolStripMenuItem miWrite;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem miStop;
		private System.Windows.Forms.ToolStripMenuItem miStart;
		private System.Windows.Forms.ToolStripMenuItem controlToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem miReaderChange;
		private System.Windows.Forms.Button btnWrite;
		private System.Windows.Forms.PictureBox imgHeader;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Panel pTitle;
		private System.Windows.Forms.Button btnReaderChange;
		private System.Windows.Forms.Label lbReader;
		private System.Windows.Forms.TextBox eReaderName;
		private System.Windows.Forms.Label lbCardAtr;
		private System.Windows.Forms.TextBox eCardAtr;
		private System.Windows.Forms.Label lbStatus;
		private System.Windows.Forms.TextBox eReaderStatus;
		private System.Windows.Forms.Panel pControl;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.MenuStrip menuMain;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem miQuit;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolTip tooltipReader;
		private System.Windows.Forms.StatusStrip stripStatus;
		private System.Windows.Forms.Timer tmrWatchReaders;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}
