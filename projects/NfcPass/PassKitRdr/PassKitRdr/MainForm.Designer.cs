namespace PassKitRdr
{
    partial class MainForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pTop = new System.Windows.Forms.Panel();
            this.lbEvaluation = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lkSetConfig = new System.Windows.Forms.LinkLabel();
            this.lkLicense = new System.Windows.Forms.LinkLabel();
            this.lkAbout = new System.Windows.Forms.LinkLabel();
            this.lkSubscribe = new System.Windows.Forms.LinkLabel();
            this.lkRefresh = new System.Windows.Forms.LinkLabel();
            this.imgCheck = new System.Windows.Forms.PictureBox();
            this.ePassTimestamp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ePassMessage = new System.Windows.Forms.TextBox();
            this.lbMessage = new System.Windows.Forms.Label();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.cbReaders = new System.Windows.Forms.ComboBox();
            this.lbReader = new System.Windows.Forms.Label();
            this.imgCross = new System.Windows.Forms.PictureBox();
            this.timerAutoClear = new System.Windows.Forms.Timer(this.components);
            this.pTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgCross)).BeginInit();
            this.SuspendLayout();
            // 
            // pTop
            // 
            this.pTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.pTop.Controls.Add(this.lbEvaluation);
            this.pTop.Controls.Add(this.pictureBox2);
            this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Name = "pTop";
            this.pTop.Size = new System.Drawing.Size(556, 86);
            this.pTop.TabIndex = 0;
            // 
            // lbEvaluation
            // 
            this.lbEvaluation.AutoSize = true;
            this.lbEvaluation.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbEvaluation.ForeColor = System.Drawing.Color.White;
            this.lbEvaluation.Location = new System.Drawing.Point(400, 30);
            this.lbEvaluation.Name = "lbEvaluation";
            this.lbEvaluation.Size = new System.Drawing.Size(144, 30);
            this.lbEvaluation.TabIndex = 2;
            this.lbEvaluation.Text = "EVALUATION";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::PassKitRdr.Properties.Resources.logoWhite;
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox2.Location = new System.Drawing.Point(16, 24);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(195, 36);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 320);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(556, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(439, 17);
            this.toolStripStatusLabel1.Text = "Select the reader, configure the merchant, and press the \"Play\" button when ready" +
    "";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.lkSetConfig);
            this.panel1.Controls.Add(this.lkLicense);
            this.panel1.Controls.Add(this.lkAbout);
            this.panel1.Controls.Add(this.lkSubscribe);
            this.panel1.Controls.Add(this.lkRefresh);
            this.panel1.Controls.Add(this.imgCheck);
            this.panel1.Controls.Add(this.ePassTimestamp);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.ePassMessage);
            this.panel1.Controls.Add(this.lbMessage);
            this.panel1.Controls.Add(this.btnPause);
            this.panel1.Controls.Add(this.btnPlay);
            this.panel1.Controls.Add(this.cbReaders);
            this.panel1.Controls.Add(this.lbReader);
            this.panel1.Controls.Add(this.imgCross);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(0, 86);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(556, 234);
            this.panel1.TabIndex = 2;
            // 
            // lkSetConfig
            // 
            this.lkSetConfig.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetConfig.AutoSize = true;
            this.lkSetConfig.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetConfig.Location = new System.Drawing.Point(416, 74);
            this.lkSetConfig.Name = "lkSetConfig";
            this.lkSetConfig.Size = new System.Drawing.Size(128, 20);
            this.lkSetConfig.TabIndex = 20;
            this.lkSetConfig.TabStop = true;
            this.lkSetConfig.Text = "Edit configuration";
            this.lkSetConfig.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lkSetConfig.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetConfig.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkSetConfiguration_LinkClicked);
            // 
            // lkLicense
            // 
            this.lkLicense.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkLicense.AutoSize = true;
            this.lkLicense.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkLicense.Location = new System.Drawing.Point(299, 193);
            this.lkLicense.Name = "lkLicense";
            this.lkLicense.Size = new System.Drawing.Size(57, 20);
            this.lkLicense.TabIndex = 19;
            this.lkLicense.TabStop = true;
            this.lkLicense.Text = "License";
            this.lkLicense.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            // 
            // lkAbout
            // 
            this.lkAbout.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkAbout.AutoSize = true;
            this.lkAbout.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkAbout.Location = new System.Drawing.Point(362, 193);
            this.lkAbout.Name = "lkAbout";
            this.lkAbout.Size = new System.Drawing.Size(50, 20);
            this.lkAbout.TabIndex = 11;
            this.lkAbout.TabStop = true;
            this.lkAbout.Text = "About";
            this.lkAbout.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkAbout_LinkClicked);
            // 
            // lkSubscribe
            // 
            this.lkSubscribe.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSubscribe.AutoSize = true;
            this.lkSubscribe.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSubscribe.Location = new System.Drawing.Point(420, 193);
            this.lkSubscribe.Name = "lkSubscribe";
            this.lkSubscribe.Size = new System.Drawing.Size(124, 20);
            this.lkSubscribe.TabIndex = 12;
            this.lkSubscribe.TabStop = true;
            this.lkSubscribe.Text = "Get your test Pass";
            this.lkSubscribe.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lkSubscribe.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSubscribe.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkSubscribe_LinkClicked);
            // 
            // lkRefresh
            // 
            this.lkRefresh.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkRefresh.AutoSize = true;
            this.lkRefresh.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkRefresh.Location = new System.Drawing.Point(416, 11);
            this.lkRefresh.Name = "lkRefresh";
            this.lkRefresh.Size = new System.Drawing.Size(128, 20);
            this.lkRefresh.TabIndex = 1;
            this.lkRefresh.TabStop = true;
            this.lkRefresh.Text = "Refresh reader list";
            this.lkRefresh.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lkRefresh.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkRefresh_LinkClicked);
            // 
            // imgCheck
            // 
            this.imgCheck.BackgroundImage = global::PassKitRdr.Properties.Resources._checked;
            this.imgCheck.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.imgCheck.Location = new System.Drawing.Point(210, 193);
            this.imgCheck.Name = "imgCheck";
            this.imgCheck.Size = new System.Drawing.Size(24, 24);
            this.imgCheck.TabIndex = 16;
            this.imgCheck.TabStop = false;
            this.imgCheck.Visible = false;
            this.imgCheck.Click += new System.EventHandler(this.imgCheck_Click);
            // 
            // ePassTimestamp
            // 
            this.ePassTimestamp.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePassTimestamp.Location = new System.Drawing.Point(16, 193);
            this.ePassTimestamp.Name = "ePassTimestamp";
            this.ePassTimestamp.ReadOnly = true;
            this.ePassTimestamp.Size = new System.Drawing.Size(187, 26);
            this.ePassTimestamp.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 170);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(126, 20);
            this.label2.TabIndex = 18;
            this.label2.Text = "Timestamp (UTC):";
            // 
            // ePassMessage
            // 
            this.ePassMessage.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePassMessage.Location = new System.Drawing.Point(17, 132);
            this.ePassMessage.Name = "ePassMessage";
            this.ePassMessage.ReadOnly = true;
            this.ePassMessage.Size = new System.Drawing.Size(527, 26);
            this.ePassMessage.TabIndex = 9;
            // 
            // lbMessage
            // 
            this.lbMessage.AutoSize = true;
            this.lbMessage.Location = new System.Drawing.Point(13, 109);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(101, 20);
            this.lbMessage.TabIndex = 17;
            this.lbMessage.Text = "Pass message:";
            // 
            // btnPause
            // 
            this.btnPause.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnPause.BackgroundImage = global::PassKitRdr.Properties.Resources.pause_button;
            this.btnPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPause.Enabled = false;
            this.btnPause.Location = new System.Drawing.Point(275, 68);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(32, 32);
            this.btnPause.TabIndex = 8;
            this.btnPause.UseVisualStyleBackColor = false;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnPlay.BackgroundImage = global::PassKitRdr.Properties.Resources.play_button;
            this.btnPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnPlay.Location = new System.Drawing.Point(237, 68);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(32, 32);
            this.btnPlay.TabIndex = 7;
            this.btnPlay.UseVisualStyleBackColor = false;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // cbReaders
            // 
            this.cbReaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbReaders.FormattingEnabled = true;
            this.cbReaders.Location = new System.Drawing.Point(16, 34);
            this.cbReaders.Name = "cbReaders";
            this.cbReaders.Size = new System.Drawing.Size(528, 28);
            this.cbReaders.TabIndex = 0;
            this.cbReaders.SelectedIndexChanged += new System.EventHandler(this.cbReaders_SelectedIndexChanged);
            // 
            // lbReader
            // 
            this.lbReader.AutoSize = true;
            this.lbReader.Location = new System.Drawing.Point(12, 12);
            this.lbReader.Name = "lbReader";
            this.lbReader.Size = new System.Drawing.Size(103, 20);
            this.lbReader.TabIndex = 5;
            this.lbReader.Text = "PC/SC Reader:";
            // 
            // imgCross
            // 
            this.imgCross.BackgroundImage = global::PassKitRdr.Properties.Resources.cross_remove_sign;
            this.imgCross.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.imgCross.Location = new System.Drawing.Point(210, 193);
            this.imgCross.Name = "imgCross";
            this.imgCross.Size = new System.Drawing.Size(24, 24);
            this.imgCross.TabIndex = 17;
            this.imgCross.TabStop = false;
            this.imgCross.Visible = false;
            this.imgCross.Click += new System.EventHandler(this.imgCross_Click);
            // 
            // timerAutoClear
            // 
            this.timerAutoClear.Tick += new System.EventHandler(this.timerAutoClear_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 342);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pass Kit Reader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgCross)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pTop;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cbReaders;
        private System.Windows.Forms.Label lbReader;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.TextBox ePassTimestamp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ePassMessage;
        private System.Windows.Forms.Label lbMessage;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox imgCheck;
        private System.Windows.Forms.PictureBox imgCross;
		private System.Windows.Forms.LinkLabel lkRefresh;
		private System.Windows.Forms.LinkLabel lkSubscribe;
		private System.Windows.Forms.LinkLabel lkAbout;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.LinkLabel lkLicense;
        private System.Windows.Forms.Label lbEvaluation;
        private System.Windows.Forms.LinkLabel lkSetConfig;
        private System.Windows.Forms.Timer timerAutoClear;
    }
}

