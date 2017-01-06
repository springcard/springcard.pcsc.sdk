/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 14/06/2013
 * Time: 13:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MemoryCardTool
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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.changeReaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pnCardType = new System.Windows.Forms.Panel();
			this.eCardUID = new System.Windows.Forms.Label();
			this.lbSerialNumber = new System.Windows.Forms.Label();
			this.pCardButtons = new System.Windows.Forms.Panel();
			this.btnWrite = new System.Windows.Forms.Button();
			this.btnReadAgain = new System.Windows.Forms.Button();
			this.eCardName = new System.Windows.Forms.Label();
			this.lbCardName = new System.Windows.Forms.Label();
			this.pMain = new System.Windows.Forms.Panel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.pTop = new System.Windows.Forms.Panel();
			this.pLogo = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.linkReader = new System.Windows.Forms.LinkLabel();
			this.lbCardAtr = new System.Windows.Forms.Label();
			this.lbReaderStatus = new System.Windows.Forms.Label();
			this.lbReaderName = new System.Windows.Forms.Label();
			this.pBottom = new System.Windows.Forms.Panel();
			this.menuStrip1.SuspendLayout();
			this.pnCardType.SuspendLayout();
			this.pCardButtons.SuspendLayout();
			this.pTop.SuspendLayout();
			this.pLogo.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem,
			this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1184, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.changeReaderToolStripMenuItem,
			this.toolStripSeparator1,
			this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// changeReaderToolStripMenuItem
			// 
			this.changeReaderToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("changeReaderToolStripMenuItem.Image")));
			this.changeReaderToolStripMenuItem.Name = "changeReaderToolStripMenuItem";
			this.changeReaderToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.changeReaderToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
			this.changeReaderToolStripMenuItem.Text = "Change reader";
			this.changeReaderToolStripMenuItem.Click += new System.EventHandler(this.ChangeReaderToolStripMenuItemClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(189, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("exitToolStripMenuItem.Image")));
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("aboutToolStripMenuItem.Image")));
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
			// 
			// pnCardType
			// 
			this.pnCardType.BackColor = System.Drawing.Color.White;
			this.pnCardType.Controls.Add(this.eCardUID);
			this.pnCardType.Controls.Add(this.lbSerialNumber);
			this.pnCardType.Controls.Add(this.pCardButtons);
			this.pnCardType.Controls.Add(this.eCardName);
			this.pnCardType.Controls.Add(this.lbCardName);
			this.pnCardType.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnCardType.Location = new System.Drawing.Point(0, 110);
			this.pnCardType.Name = "pnCardType";
			this.pnCardType.Size = new System.Drawing.Size(1184, 65);
			this.pnCardType.TabIndex = 32;
			// 
			// eCardUID
			// 
			this.eCardUID.AutoSize = true;
			this.eCardUID.Font = new System.Drawing.Font("Calibri", 13F);
			this.eCardUID.ForeColor = System.Drawing.Color.Black;
			this.eCardUID.Location = new System.Drawing.Point(181, 35);
			this.eCardUID.Name = "eCardUID";
			this.eCardUID.Size = new System.Drawing.Size(57, 22);
			this.eCardUID.TabIndex = 4;
			this.eCardUID.Text = "(none)";
			this.eCardUID.DoubleClick += new System.EventHandler(this.ECardUIDDoubleClick);
			// 
			// lbSerialNumber
			// 
			this.lbSerialNumber.AutoSize = true;
			this.lbSerialNumber.Font = new System.Drawing.Font("Calibri Light", 12F);
			this.lbSerialNumber.ForeColor = System.Drawing.Color.Black;
			this.lbSerialNumber.Location = new System.Drawing.Point(12, 38);
			this.lbSerialNumber.Name = "lbSerialNumber";
			this.lbSerialNumber.Size = new System.Drawing.Size(105, 19);
			this.lbSerialNumber.TabIndex = 3;
			this.lbSerialNumber.Text = "Serial number:";
			// 
			// pCardButtons
			// 
			this.pCardButtons.Controls.Add(this.btnWrite);
			this.pCardButtons.Controls.Add(this.btnReadAgain);
			this.pCardButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.pCardButtons.Location = new System.Drawing.Point(920, 0);
			this.pCardButtons.Name = "pCardButtons";
			this.pCardButtons.Size = new System.Drawing.Size(264, 65);
			this.pCardButtons.TabIndex = 2;
			// 
			// btnWrite
			// 
			this.btnWrite.Font = new System.Drawing.Font("Calibri Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnWrite.Location = new System.Drawing.Point(137, 11);
			this.btnWrite.Name = "btnWrite";
			this.btnWrite.Size = new System.Drawing.Size(115, 46);
			this.btnWrite.TabIndex = 1;
			this.btnWrite.Text = "Write changes";
			this.btnWrite.UseVisualStyleBackColor = true;
			this.btnWrite.Click += new System.EventHandler(this.BtnWriteClick);
			// 
			// btnReadAgain
			// 
			this.btnReadAgain.Font = new System.Drawing.Font("Calibri Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnReadAgain.Location = new System.Drawing.Point(12, 12);
			this.btnReadAgain.Name = "btnReadAgain";
			this.btnReadAgain.Size = new System.Drawing.Size(119, 45);
			this.btnReadAgain.TabIndex = 0;
			this.btnReadAgain.Text = "Read again";
			this.btnReadAgain.UseVisualStyleBackColor = true;
			this.btnReadAgain.Click += new System.EventHandler(this.BtnReadAgainClick);
			// 
			// eCardName
			// 
			this.eCardName.AutoSize = true;
			this.eCardName.Font = new System.Drawing.Font("Calibri", 13F);
			this.eCardName.ForeColor = System.Drawing.Color.Black;
			this.eCardName.Location = new System.Drawing.Point(181, 9);
			this.eCardName.Name = "eCardName";
			this.eCardName.Size = new System.Drawing.Size(57, 22);
			this.eCardName.TabIndex = 1;
			this.eCardName.Text = "(none)";
			this.eCardName.DoubleClick += new System.EventHandler(this.ECardNameDoubleClick);
			// 
			// lbCardName
			// 
			this.lbCardName.AutoSize = true;
			this.lbCardName.Font = new System.Drawing.Font("Calibri Light", 12F);
			this.lbCardName.ForeColor = System.Drawing.Color.Black;
			this.lbCardName.Location = new System.Drawing.Point(12, 12);
			this.lbCardName.Name = "lbCardName";
			this.lbCardName.Size = new System.Drawing.Size(89, 19);
			this.lbCardName.TabIndex = 0;
			this.lbCardName.Text = "Card family:";
			// 
			// pMain
			// 
			this.pMain.AutoScroll = true;
			this.pMain.AutoSize = true;
			this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pMain.Location = new System.Drawing.Point(0, 175);
			this.pMain.Name = "pMain";
			this.pMain.Size = new System.Drawing.Size(1184, 560);
			this.pMain.TabIndex = 33;
			// 
			// pTop
			// 
			this.pTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
			this.pTop.Controls.Add(this.pLogo);
			this.pTop.Controls.Add(this.linkReader);
			this.pTop.Controls.Add(this.lbCardAtr);
			this.pTop.Controls.Add(this.lbReaderStatus);
			this.pTop.Controls.Add(this.lbReaderName);
			this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.pTop.ForeColor = System.Drawing.Color.White;
			this.pTop.Location = new System.Drawing.Point(0, 24);
			this.pTop.Name = "pTop";
			this.pTop.Size = new System.Drawing.Size(1184, 86);
			this.pTop.TabIndex = 38;
			// 
			// pLogo
			// 
			this.pLogo.Controls.Add(this.pictureBox1);
			this.pLogo.Dock = System.Windows.Forms.DockStyle.Right;
			this.pLogo.Location = new System.Drawing.Point(920, 0);
			this.pLogo.Name = "pLogo";
			this.pLogo.Size = new System.Drawing.Size(264, 86);
			this.pLogo.TabIndex = 10;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(12, 16);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(240, 48);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.DoubleClick += new System.EventHandler(this.PictureBox1DoubleClick);
			// 
			// linkReader
			// 
			this.linkReader.ActiveLinkColor = System.Drawing.Color.White;
			this.linkReader.AutoSize = true;
			this.linkReader.Cursor = System.Windows.Forms.Cursors.Hand;
			this.linkReader.DisabledLinkColor = System.Drawing.Color.Gainsboro;
			this.linkReader.Font = new System.Drawing.Font("Calibri Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkReader.LinkColor = System.Drawing.Color.White;
			this.linkReader.Location = new System.Drawing.Point(12, 9);
			this.linkReader.Name = "linkReader";
			this.linkReader.Size = new System.Drawing.Size(56, 19);
			this.linkReader.TabIndex = 9;
			this.linkReader.TabStop = true;
			this.linkReader.Text = "Reader";
			this.linkReader.VisitedLinkColor = System.Drawing.Color.White;
			this.linkReader.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkReaderLinkClicked);
			// 
			// lbCardAtr
			// 
			this.lbCardAtr.AutoSize = true;
			this.lbCardAtr.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbCardAtr.Location = new System.Drawing.Point(181, 51);
			this.lbCardAtr.Name = "lbCardAtr";
			this.lbCardAtr.Size = new System.Drawing.Size(68, 19);
			this.lbCardAtr.TabIndex = 6;
			this.lbCardAtr.Text = "Card ATR";
			this.lbCardAtr.DoubleClick += new System.EventHandler(this.LbCardAtrDoubleClick);
			// 
			// lbReaderStatus
			// 
			this.lbReaderStatus.AutoSize = true;
			this.lbReaderStatus.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbReaderStatus.Location = new System.Drawing.Point(12, 51);
			this.lbReaderStatus.Name = "lbReaderStatus";
			this.lbReaderStatus.Size = new System.Drawing.Size(112, 23);
			this.lbReaderStatus.TabIndex = 5;
			this.lbReaderStatus.Text = "ReaderStatus";
			// 
			// lbReaderName
			// 
			this.lbReaderName.AutoSize = true;
			this.lbReaderName.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbReaderName.Location = new System.Drawing.Point(12, 28);
			this.lbReaderName.Name = "lbReaderName";
			this.lbReaderName.Size = new System.Drawing.Size(109, 23);
			this.lbReaderName.TabIndex = 4;
			this.lbReaderName.Text = "ReaderName";
			// 
			// pBottom
			// 
			this.pBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pBottom.Location = new System.Drawing.Point(0, 735);
			this.pBottom.Name = "pBottom";
			this.pBottom.Size = new System.Drawing.Size(1184, 27);
			this.pBottom.TabIndex = 41;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.ClientSize = new System.Drawing.Size(1184, 762);
			this.Controls.Add(this.pMain);
			this.Controls.Add(this.pBottom);
			this.Controls.Add(this.pnCardType);
			this.Controls.Add(this.pTop);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MemoryCardTool";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
			this.Load += new System.EventHandler(this.MainFormLoad);
			this.Shown += new System.EventHandler(this.MainFormShown);
			this.LocationChanged += new System.EventHandler(this.MainFormLocationChanged);
			this.SizeChanged += new System.EventHandler(this.MainFormSizeChanged);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.pnCardType.ResumeLayout(false);
			this.pnCardType.PerformLayout();
			this.pCardButtons.ResumeLayout(false);
			this.pTop.ResumeLayout(false);
			this.pTop.PerformLayout();
			this.pLogo.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Panel pMain;
		private System.Windows.Forms.Panel pnCardType;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem changeReaderToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.Panel pTop;
		private System.Windows.Forms.Panel pLogo;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.LinkLabel linkReader;
		private System.Windows.Forms.Label lbCardAtr;
		private System.Windows.Forms.Label lbReaderStatus;
		private System.Windows.Forms.Label lbReaderName;
		private System.Windows.Forms.Panel pBottom;
		private System.Windows.Forms.Label eCardName;
		private System.Windows.Forms.Label lbCardName;
		private System.Windows.Forms.Panel pCardButtons;
		private System.Windows.Forms.Button btnWrite;
		private System.Windows.Forms.Button btnReadAgain;
		private System.Windows.Forms.Label eCardUID;
		private System.Windows.Forms.Label lbSerialNumber;
	}
}
