namespace SmartTapRdr
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pTop = new System.Windows.Forms.Panel();
            this.lbEvaluation = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lkLicense = new System.Windows.Forms.LinkLabel();
            this.lkClear = new System.Windows.Forms.LinkLabel();
            this.lkAbout = new System.Windows.Forms.LinkLabel();
            this.lkSubscribe = new System.Windows.Forms.LinkLabel();
            this.lkRefresh = new System.Windows.Forms.LinkLabel();
            this.lkSetConfig = new System.Windows.Forms.LinkLabel();
            this.eSmartTapData = new System.Windows.Forms.TextBox();
            this.lbMessage = new System.Windows.Forms.Label();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.cbReaders = new System.Windows.Forms.ComboBox();
            this.lbReader = new System.Windows.Forms.Label();
            this.pTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
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
            this.pTop.Size = new System.Drawing.Size(784, 86);
            this.pTop.TabIndex = 0;
            // 
            // lbEvaluation
            // 
            this.lbEvaluation.AutoSize = true;
            this.lbEvaluation.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbEvaluation.ForeColor = System.Drawing.Color.White;
            this.lbEvaluation.Location = new System.Drawing.Point(627, 30);
            this.lbEvaluation.Name = "lbEvaluation";
            this.lbEvaluation.Size = new System.Drawing.Size(144, 30);
            this.lbEvaluation.TabIndex = 3;
            this.lbEvaluation.Text = "EVALUATION";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::SmartTapRdr.Properties.Resources.logoWhite;
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 639);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(438, 17);
            this.toolStripStatusLabel1.Text = "Select the reader, verify the configuration, and press the \"Play\" button when rea" +
    "dy";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.lkLicense);
            this.panel1.Controls.Add(this.lkClear);
            this.panel1.Controls.Add(this.lkAbout);
            this.panel1.Controls.Add(this.lkSubscribe);
            this.panel1.Controls.Add(this.lkRefresh);
            this.panel1.Controls.Add(this.lkSetConfig);
            this.panel1.Controls.Add(this.eSmartTapData);
            this.panel1.Controls.Add(this.lbMessage);
            this.panel1.Controls.Add(this.btnPause);
            this.panel1.Controls.Add(this.btnPlay);
            this.panel1.Controls.Add(this.cbReaders);
            this.panel1.Controls.Add(this.lbReader);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(0, 86);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(784, 553);
            this.panel1.TabIndex = 2;
            // 
            // lkLicense
            // 
            this.lkLicense.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkLicense.AutoSize = true;
            this.lkLicense.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkLicense.Location = new System.Drawing.Point(496, 525);
            this.lkLicense.Name = "lkLicense";
            this.lkLicense.Size = new System.Drawing.Size(57, 20);
            this.lkLicense.TabIndex = 20;
            this.lkLicense.TabStop = true;
            this.lkLicense.Text = "License";
            this.lkLicense.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            // 
            // lkClear
            // 
            this.lkClear.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClear.AutoSize = true;
            this.lkClear.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClear.Location = new System.Drawing.Point(728, 114);
            this.lkClear.Name = "lkClear";
            this.lkClear.Size = new System.Drawing.Size(43, 20);
            this.lkClear.TabIndex = 18;
            this.lkClear.TabStop = true;
            this.lkClear.Text = "Clear";
            this.lkClear.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lkClear.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkClear_LinkClicked);
            // 
            // lkAbout
            // 
            this.lkAbout.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkAbout.AutoSize = true;
            this.lkAbout.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkAbout.Location = new System.Drawing.Point(572, 525);
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
            this.lkSubscribe.Location = new System.Drawing.Point(647, 525);
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
            this.lkRefresh.Location = new System.Drawing.Point(644, 12);
            this.lkRefresh.Name = "lkRefresh";
            this.lkRefresh.Size = new System.Drawing.Size(128, 20);
            this.lkRefresh.TabIndex = 1;
            this.lkRefresh.TabStop = true;
            this.lkRefresh.Text = "Refresh reader list";
            this.lkRefresh.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lkRefresh.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkRefresh_LinkClicked);
            // 
            // lkSetConfig
            // 
            this.lkSetConfig.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetConfig.AutoSize = true;
            this.lkSetConfig.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetConfig.Location = new System.Drawing.Point(644, 77);
            this.lkSetConfig.Name = "lkSetConfig";
            this.lkSetConfig.Size = new System.Drawing.Size(128, 20);
            this.lkSetConfig.TabIndex = 3;
            this.lkSetConfig.TabStop = true;
            this.lkSetConfig.Text = "Edit configuration";
            this.lkSetConfig.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lkSetConfig.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetConfig.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkSetDefault_LinkClicked);
            // 
            // eSmartTapData
            // 
            this.eSmartTapData.AcceptsReturn = true;
            this.eSmartTapData.AcceptsTab = true;
            this.eSmartTapData.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eSmartTapData.Location = new System.Drawing.Point(16, 137);
            this.eSmartTapData.Multiline = true;
            this.eSmartTapData.Name = "eSmartTapData";
            this.eSmartTapData.ReadOnly = true;
            this.eSmartTapData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.eSmartTapData.Size = new System.Drawing.Size(756, 385);
            this.eSmartTapData.TabIndex = 9;
            // 
            // lbMessage
            // 
            this.lbMessage.AutoSize = true;
            this.lbMessage.Location = new System.Drawing.Point(12, 114);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(112, 20);
            this.lbMessage.TabIndex = 17;
            this.lbMessage.Text = "Smart Tap data:";
            // 
            // btnPause
            // 
            this.btnPause.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnPause.BackgroundImage = global::SmartTapRdr.Properties.Resources.pause_button;
            this.btnPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPause.Enabled = false;
            this.btnPause.Location = new System.Drawing.Point(399, 77);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(32, 30);
            this.btnPause.TabIndex = 8;
            this.btnPause.UseVisualStyleBackColor = false;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnPlay.BackgroundImage = global::SmartTapRdr.Properties.Resources.play_button;
            this.btnPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnPlay.Location = new System.Drawing.Point(361, 77);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(32, 30);
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
            this.cbReaders.Size = new System.Drawing.Size(756, 28);
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 661);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart Tap Reader";
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
        private System.Windows.Forms.TextBox eSmartTapData;
        private System.Windows.Forms.Label lbMessage;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.LinkLabel lkSetConfig;
        private System.Windows.Forms.LinkLabel lkRefresh;
        private System.Windows.Forms.LinkLabel lkSubscribe;
        private System.Windows.Forms.LinkLabel lkAbout;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.LinkLabel lkClear;
        private System.Windows.Forms.Label lbEvaluation;
        private System.Windows.Forms.LinkLabel lkLicense;
    }
}

