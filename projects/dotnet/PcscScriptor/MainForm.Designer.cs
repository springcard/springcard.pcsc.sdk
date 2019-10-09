/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 04/24/2015
 * Time: 13:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace scscriptorxv
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Panel pLeftBottom;
		private System.Windows.Forms.Panel pTop;
		private System.Windows.Forms.Panel pBottom;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Panel pLeftTop;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel pRightTop;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RichTextBox richTextBoxScript;
		private System.Windows.Forms.RichTextBox richTextBoxResult;
		private System.Windows.Forms.RadioButton rbHexAndAscii;
		private System.Windows.Forms.RadioButton rbHex;
		private System.Windows.Forms.RadioButton rbAscii;
		private System.Windows.Forms.CheckBox cbStopOnStatus;
		private System.Windows.Forms.CheckBox cbStopOnError;
		private System.Windows.Forms.CheckBox cbAutorun;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.CheckBox cbLoop;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Label lbReaderName;
		private System.Windows.Forms.Label lbReaderStatus;
		private System.Windows.Forms.Label lbCardAtr;
		private System.Windows.Forms.LinkLabel linkReader;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.Panel pLeftTools;
		private System.Windows.Forms.LinkLabel linkLoadScript;
		private System.Windows.Forms.LinkLabel linkSaveScript;
		private System.Windows.Forms.Panel pRightTools;
		private System.Windows.Forms.LinkLabel linkSaveResult;
		private System.Windows.Forms.LinkLabel linkClearResult;
		private System.Windows.Forms.Panel pLogo;
		private System.Windows.Forms.Panel pLeftMain;
		private System.Windows.Forms.Panel pRightMain;
		private System.Windows.Forms.Panel pRightBottom;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Panel pAction;
		
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pTop = new System.Windows.Forms.Panel();
            this.pLogo = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.linkReader = new System.Windows.Forms.LinkLabel();
            this.lbCardAtr = new System.Windows.Forms.Label();
            this.lbReaderStatus = new System.Windows.Forms.Label();
            this.lbReaderName = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.cbLoop = new System.Windows.Forms.CheckBox();
            this.cbAutorun = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pLeftMain = new System.Windows.Forms.Panel();
            this.richTextBoxScript = new System.Windows.Forms.RichTextBox();
            this.pLeftBottom = new System.Windows.Forms.Panel();
            this.cbStopOnStatus = new System.Windows.Forms.CheckBox();
            this.cbStopOnError = new System.Windows.Forms.CheckBox();
            this.pLeftTop = new System.Windows.Forms.Panel();
            this.pLeftTools = new System.Windows.Forms.Panel();
            this.linkSaveScript = new System.Windows.Forms.LinkLabel();
            this.linkLoadScript = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.pRightMain = new System.Windows.Forms.Panel();
            this.richTextBoxResult = new System.Windows.Forms.RichTextBox();
            this.pRightBottom = new System.Windows.Forms.Panel();
            this.pAction = new System.Windows.Forms.Panel();
            this.rbHexAndAscii = new System.Windows.Forms.RadioButton();
            this.rbAscii = new System.Windows.Forms.RadioButton();
            this.rbHex = new System.Windows.Forms.RadioButton();
            this.pRightTop = new System.Windows.Forms.Panel();
            this.pRightTools = new System.Windows.Forms.Panel();
            this.linkSaveResult = new System.Windows.Forms.LinkLabel();
            this.linkClearResult = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.pBottom = new System.Windows.Forms.Panel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getAPDUFromLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveresultAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearResultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.readerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnIccPwrOn = new System.Windows.Forms.Button();
            this.btnIccPwrOff = new System.Windows.Forms.Button();
            this.pTop.SuspendLayout();
            this.pLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pLeftMain.SuspendLayout();
            this.pLeftBottom.SuspendLayout();
            this.pLeftTop.SuspendLayout();
            this.pLeftTools.SuspendLayout();
            this.pRightMain.SuspendLayout();
            this.pRightBottom.SuspendLayout();
            this.pAction.SuspendLayout();
            this.pRightTop.SuspendLayout();
            this.pRightTools.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
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
            this.pTop.Size = new System.Drawing.Size(1319, 86);
            this.pTop.TabIndex = 37;
            // 
            // pLogo
            // 
            this.pLogo.Controls.Add(this.pictureBox1);
            this.pLogo.Dock = System.Windows.Forms.DockStyle.Right;
            this.pLogo.Location = new System.Drawing.Point(1055, 0);
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
            this.lbCardAtr.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbCardAtr.Location = new System.Drawing.Point(181, 51);
            this.lbCardAtr.Name = "lbCardAtr";
            this.lbCardAtr.Size = new System.Drawing.Size(79, 23);
            this.lbCardAtr.TabIndex = 6;
            this.lbCardAtr.Text = "Card ATR";
            this.lbCardAtr.DoubleClick += new System.EventHandler(this.lbCardAtr_DoubleClick);
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
            // btnRun
            // 
            this.btnRun.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRun.Location = new System.Drawing.Point(189, 0);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(77, 28);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.BtnRunClick);
            // 
            // cbLoop
            // 
            this.cbLoop.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbLoop.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLoop.Location = new System.Drawing.Point(114, 0);
            this.cbLoop.Name = "cbLoop";
            this.cbLoop.Size = new System.Drawing.Size(69, 28);
            this.cbLoop.TabIndex = 1;
            this.cbLoop.Text = "Loop";
            this.cbLoop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbLoop.UseVisualStyleBackColor = true;
            this.cbLoop.CheckedChanged += new System.EventHandler(this.CbLoopCheckedChanged);
            // 
            // cbAutorun
            // 
            this.cbAutorun.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbAutorun.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbAutorun.Location = new System.Drawing.Point(272, 0);
            this.cbAutorun.Name = "cbAutorun";
            this.cbAutorun.Size = new System.Drawing.Size(72, 28);
            this.cbAutorun.TabIndex = 3;
            this.cbAutorun.Text = "Autorun";
            this.cbAutorun.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbAutorun.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 110);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pLeftMain);
            this.splitContainer1.Panel1.Controls.Add(this.pLeftBottom);
            this.splitContainer1.Panel1.Controls.Add(this.pLeftTop);
            this.splitContainer1.Panel1MinSize = 240;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pRightMain);
            this.splitContainer1.Panel2.Controls.Add(this.pRightBottom);
            this.splitContainer1.Panel2.Controls.Add(this.pRightTop);
            this.splitContainer1.Panel2MinSize = 240;
            this.splitContainer1.Size = new System.Drawing.Size(1319, 604);
            this.splitContainer1.SplitterDistance = 415;
            this.splitContainer1.TabIndex = 39;
            // 
            // pLeftMain
            // 
            this.pLeftMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pLeftMain.Controls.Add(this.richTextBoxScript);
            this.pLeftMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pLeftMain.Location = new System.Drawing.Point(0, 28);
            this.pLeftMain.Name = "pLeftMain";
            this.pLeftMain.Padding = new System.Windows.Forms.Padding(12);
            this.pLeftMain.Size = new System.Drawing.Size(415, 543);
            this.pLeftMain.TabIndex = 5;
            // 
            // richTextBoxScript
            // 
            this.richTextBoxScript.BackColor = System.Drawing.Color.WhiteSmoke;
            this.richTextBoxScript.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxScript.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxScript.Location = new System.Drawing.Point(12, 12);
            this.richTextBoxScript.Name = "richTextBoxScript";
            this.richTextBoxScript.Size = new System.Drawing.Size(391, 519);
            this.richTextBoxScript.TabIndex = 4;
            this.richTextBoxScript.Text = "";
            // 
            // pLeftBottom
            // 
            this.pLeftBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pLeftBottom.Controls.Add(this.cbStopOnStatus);
            this.pLeftBottom.Controls.Add(this.cbStopOnError);
            this.pLeftBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pLeftBottom.Location = new System.Drawing.Point(0, 571);
            this.pLeftBottom.Name = "pLeftBottom";
            this.pLeftBottom.Size = new System.Drawing.Size(415, 33);
            this.pLeftBottom.TabIndex = 4;
            // 
            // cbStopOnStatus
            // 
            this.cbStopOnStatus.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbStopOnStatus.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbStopOnStatus.Location = new System.Drawing.Point(175, 0);
            this.cbStopOnStatus.Name = "cbStopOnStatus";
            this.cbStopOnStatus.Size = new System.Drawing.Size(157, 28);
            this.cbStopOnStatus.TabIndex = 5;
            this.cbStopOnStatus.Text = "Stop on SW not 9000";
            this.cbStopOnStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbStopOnStatus.UseVisualStyleBackColor = true;
            // 
            // cbStopOnError
            // 
            this.cbStopOnError.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbStopOnError.Checked = true;
            this.cbStopOnError.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStopOnError.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbStopOnError.Location = new System.Drawing.Point(12, 0);
            this.cbStopOnError.Name = "cbStopOnError";
            this.cbStopOnError.Size = new System.Drawing.Size(157, 28);
            this.cbStopOnError.TabIndex = 4;
            this.cbStopOnError.Text = "Stop on error";
            this.cbStopOnError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbStopOnError.UseVisualStyleBackColor = true;
            // 
            // pLeftTop
            // 
            this.pLeftTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pLeftTop.Controls.Add(this.pLeftTools);
            this.pLeftTop.Controls.Add(this.label1);
            this.pLeftTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pLeftTop.Location = new System.Drawing.Point(0, 0);
            this.pLeftTop.Name = "pLeftTop";
            this.pLeftTop.Size = new System.Drawing.Size(415, 28);
            this.pLeftTop.TabIndex = 0;
            // 
            // pLeftTools
            // 
            this.pLeftTools.Controls.Add(this.linkSaveScript);
            this.pLeftTools.Controls.Add(this.linkLoadScript);
            this.pLeftTools.Dock = System.Windows.Forms.DockStyle.Right;
            this.pLeftTools.Location = new System.Drawing.Point(307, 0);
            this.pLeftTools.Name = "pLeftTools";
            this.pLeftTools.Size = new System.Drawing.Size(108, 28);
            this.pLeftTools.TabIndex = 1;
            // 
            // linkSaveScript
            // 
            this.linkSaveScript.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkSaveScript.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkSaveScript.Font = new System.Drawing.Font("Calibri Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkSaveScript.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkSaveScript.Location = new System.Drawing.Point(58, 10);
            this.linkSaveScript.Name = "linkSaveScript";
            this.linkSaveScript.Size = new System.Drawing.Size(49, 23);
            this.linkSaveScript.TabIndex = 1;
            this.linkSaveScript.TabStop = true;
            this.linkSaveScript.Text = "Save";
            this.linkSaveScript.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkSaveScript.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkSaveScriptLinkClicked);
            // 
            // linkLoadScript
            // 
            this.linkLoadScript.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkLoadScript.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkLoadScript.Font = new System.Drawing.Font("Calibri Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLoadScript.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkLoadScript.Location = new System.Drawing.Point(3, 10);
            this.linkLoadScript.Name = "linkLoadScript";
            this.linkLoadScript.Size = new System.Drawing.Size(49, 23);
            this.linkLoadScript.TabIndex = 0;
            this.linkLoadScript.TabStop = true;
            this.linkLoadScript.Text = "Load";
            this.linkLoadScript.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkLoadScript.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLoadScriptLinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Script";
            // 
            // pRightMain
            // 
            this.pRightMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pRightMain.Controls.Add(this.richTextBoxResult);
            this.pRightMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pRightMain.Location = new System.Drawing.Point(0, 28);
            this.pRightMain.Name = "pRightMain";
            this.pRightMain.Padding = new System.Windows.Forms.Padding(12);
            this.pRightMain.Size = new System.Drawing.Size(900, 543);
            this.pRightMain.TabIndex = 5;
            // 
            // richTextBoxResult
            // 
            this.richTextBoxResult.BackColor = System.Drawing.Color.WhiteSmoke;
            this.richTextBoxResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxResult.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxResult.Location = new System.Drawing.Point(12, 12);
            this.richTextBoxResult.Name = "richTextBoxResult";
            this.richTextBoxResult.ReadOnly = true;
            this.richTextBoxResult.Size = new System.Drawing.Size(876, 519);
            this.richTextBoxResult.TabIndex = 4;
            this.richTextBoxResult.Text = "";
            // 
            // pRightBottom
            // 
            this.pRightBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pRightBottom.Controls.Add(this.btnIccPwrOff);
            this.pRightBottom.Controls.Add(this.btnIccPwrOn);
            this.pRightBottom.Controls.Add(this.pAction);
            this.pRightBottom.Controls.Add(this.rbHexAndAscii);
            this.pRightBottom.Controls.Add(this.rbAscii);
            this.pRightBottom.Controls.Add(this.rbHex);
            this.pRightBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pRightBottom.Location = new System.Drawing.Point(0, 571);
            this.pRightBottom.Name = "pRightBottom";
            this.pRightBottom.Size = new System.Drawing.Size(900, 33);
            this.pRightBottom.TabIndex = 4;
            // 
            // pAction
            // 
            this.pAction.Controls.Add(this.btnRun);
            this.pAction.Controls.Add(this.cbAutorun);
            this.pAction.Controls.Add(this.cbLoop);
            this.pAction.Dock = System.Windows.Forms.DockStyle.Right;
            this.pAction.Location = new System.Drawing.Point(553, 0);
            this.pAction.Name = "pAction";
            this.pAction.Size = new System.Drawing.Size(347, 33);
            this.pAction.TabIndex = 9;
            // 
            // rbHexAndAscii
            // 
            this.rbHexAndAscii.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbHexAndAscii.Checked = true;
            this.rbHexAndAscii.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbHexAndAscii.Location = new System.Drawing.Point(154, 0);
            this.rbHexAndAscii.Name = "rbHexAndAscii";
            this.rbHexAndAscii.Size = new System.Drawing.Size(103, 28);
            this.rbHexAndAscii.TabIndex = 8;
            this.rbHexAndAscii.TabStop = true;
            this.rbHexAndAscii.Text = "Hex + ASCII";
            this.rbHexAndAscii.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbHexAndAscii.UseVisualStyleBackColor = true;
            // 
            // rbAscii
            // 
            this.rbAscii.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbAscii.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbAscii.Location = new System.Drawing.Point(12, 0);
            this.rbAscii.Name = "rbAscii";
            this.rbAscii.Size = new System.Drawing.Size(65, 28);
            this.rbAscii.TabIndex = 6;
            this.rbAscii.Text = "ASCII";
            this.rbAscii.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbAscii.UseVisualStyleBackColor = true;
            // 
            // rbHex
            // 
            this.rbHex.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbHex.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbHex.Location = new System.Drawing.Point(83, 0);
            this.rbHex.Name = "rbHex";
            this.rbHex.Size = new System.Drawing.Size(65, 28);
            this.rbHex.TabIndex = 7;
            this.rbHex.Text = "Hex";
            this.rbHex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbHex.UseVisualStyleBackColor = true;
            // 
            // pRightTop
            // 
            this.pRightTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pRightTop.Controls.Add(this.pRightTools);
            this.pRightTop.Controls.Add(this.label2);
            this.pRightTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pRightTop.Location = new System.Drawing.Point(0, 0);
            this.pRightTop.Name = "pRightTop";
            this.pRightTop.Size = new System.Drawing.Size(900, 28);
            this.pRightTop.TabIndex = 1;
            // 
            // pRightTools
            // 
            this.pRightTools.Controls.Add(this.linkSaveResult);
            this.pRightTools.Controls.Add(this.linkClearResult);
            this.pRightTools.Dock = System.Windows.Forms.DockStyle.Right;
            this.pRightTools.Location = new System.Drawing.Point(792, 0);
            this.pRightTools.Name = "pRightTools";
            this.pRightTools.Size = new System.Drawing.Size(108, 28);
            this.pRightTools.TabIndex = 2;
            // 
            // linkSaveResult
            // 
            this.linkSaveResult.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkSaveResult.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkSaveResult.Font = new System.Drawing.Font("Calibri Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkSaveResult.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkSaveResult.Location = new System.Drawing.Point(58, 10);
            this.linkSaveResult.Name = "linkSaveResult";
            this.linkSaveResult.Size = new System.Drawing.Size(49, 23);
            this.linkSaveResult.TabIndex = 2;
            this.linkSaveResult.TabStop = true;
            this.linkSaveResult.Text = "Save";
            this.linkSaveResult.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkSaveResult.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkSaveResultLinkClicked);
            // 
            // linkClearResult
            // 
            this.linkClearResult.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkClearResult.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkClearResult.Font = new System.Drawing.Font("Calibri Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkClearResult.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkClearResult.Location = new System.Drawing.Point(3, 10);
            this.linkClearResult.Name = "linkClearResult";
            this.linkClearResult.Size = new System.Drawing.Size(49, 23);
            this.linkClearResult.TabIndex = 1;
            this.linkClearResult.TabStop = true;
            this.linkClearResult.Text = "Clear";
            this.linkClearResult.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkClearResult.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkClearResultLinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "Output";
            // 
            // pBottom
            // 
            this.pBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Location = new System.Drawing.Point(0, 714);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(1319, 27);
            this.pBottom.TabIndex = 40;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
            this.saveFileDialog.RestoreDirectory = true;
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1319, 24);
            this.menuStrip1.TabIndex = 42;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.getAPDUFromLibraryToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveresultAsToolStripMenuItem,
            this.clearResultToolStripMenuItem,
            this.toolStripSeparator3,
            this.readerToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItemClick);
            // 
            // getAPDUFromLibraryToolStripMenuItem
            // 
            this.getAPDUFromLibraryToolStripMenuItem.Image = global::PcscScriptor.Properties.Resources.open_book;
            this.getAPDUFromLibraryToolStripMenuItem.Name = "getAPDUFromLibraryToolStripMenuItem";
            this.getAPDUFromLibraryToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.getAPDUFromLibraryToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.getAPDUFromLibraryToolStripMenuItem.Text = "Get APDU from library";
            this.getAPDUFromLibraryToolStripMenuItem.Click += new System.EventHandler(this.getAPDUFromLibraryToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.openToolStripMenuItem.Text = "&Load script";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(228, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.saveToolStripMenuItem.Text = "&Save script";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItemClick);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.saveAsToolStripMenuItem.Text = "Save script &As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(228, 6);
            // 
            // saveresultAsToolStripMenuItem
            // 
            this.saveresultAsToolStripMenuItem.Name = "saveresultAsToolStripMenuItem";
            this.saveresultAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.saveresultAsToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.saveresultAsToolStripMenuItem.Text = "Save &result As";
            this.saveresultAsToolStripMenuItem.Click += new System.EventHandler(this.SaveresultAsToolStripMenuItemClick);
            // 
            // clearResultToolStripMenuItem
            // 
            this.clearResultToolStripMenuItem.Name = "clearResultToolStripMenuItem";
            this.clearResultToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.clearResultToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.clearResultToolStripMenuItem.Text = "&Clear result";
            this.clearResultToolStripMenuItem.Click += new System.EventHandler(this.ClearResultToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(228, 6);
            // 
            // readerToolStripMenuItem
            // 
            this.readerToolStripMenuItem.Name = "readerToolStripMenuItem";
            this.readerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.readerToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.readerToolStripMenuItem.Text = "Reader";
            this.readerToolStripMenuItem.Click += new System.EventHandler(this.readerToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(228, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
            // 
            // btnIccPwrOn
            // 
            this.btnIccPwrOn.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIccPwrOn.Location = new System.Drawing.Point(361, 0);
            this.btnIccPwrOn.Name = "btnIccPwrOn";
            this.btnIccPwrOn.Size = new System.Drawing.Size(88, 28);
            this.btnIccPwrOn.TabIndex = 4;
            this.btnIccPwrOn.Text = "IccPwrOn";
            this.btnIccPwrOn.UseVisualStyleBackColor = true;
            this.btnIccPwrOn.Click += new System.EventHandler(this.btnIccPwrOn_Click);
            // 
            // btnIccPwrOff
            // 
            this.btnIccPwrOff.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIccPwrOff.Location = new System.Drawing.Point(455, 0);
            this.btnIccPwrOff.Name = "btnIccPwrOff";
            this.btnIccPwrOff.Size = new System.Drawing.Size(88, 28);
            this.btnIccPwrOff.TabIndex = 10;
            this.btnIccPwrOff.Text = "IccPwrOff";
            this.btnIccPwrOff.UseVisualStyleBackColor = true;
            this.btnIccPwrOff.Click += new System.EventHandler(this.btnIccPwrOff_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(1319, 741);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pTop);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.menuStrip1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "scscriptorxv";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
            this.Shown += new System.EventHandler(this.MainFormShown);
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            this.pLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pLeftMain.ResumeLayout(false);
            this.pLeftBottom.ResumeLayout(false);
            this.pLeftTop.ResumeLayout(false);
            this.pLeftTop.PerformLayout();
            this.pLeftTools.ResumeLayout(false);
            this.pRightMain.ResumeLayout(false);
            this.pRightBottom.ResumeLayout(false);
            this.pAction.ResumeLayout(false);
            this.pRightTop.ResumeLayout(false);
            this.pRightTop.PerformLayout();
            this.pRightTools.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.ToolStripMenuItem clearResultToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveresultAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem getAPDUFromLibraryToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem readerToolStripMenuItem;
        private System.Windows.Forms.Button btnIccPwrOff;
        private System.Windows.Forms.Button btnIccPwrOn;
    }
}
