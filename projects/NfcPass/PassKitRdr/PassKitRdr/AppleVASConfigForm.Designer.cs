namespace PassKitRdr
{
    partial class AppleVASConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabMerchants = new System.Windows.Forms.TabControl();
            this.tabMerchant1 = new System.Windows.Forms.TabPage();
            this.eKeyId1 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.eMerchant1Id = new System.Windows.Forms.TextBox();
            this.eMerchant1Name = new System.Windows.Forms.TextBox();
            this.ePublicKey1 = new System.Windows.Forms.TextBox();
            this.ePrivateKey1 = new System.Windows.Forms.TextBox();
            this.lbMerchantName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbPrivateKey = new System.Windows.Forms.Label();
            this.tabMerchant2 = new System.Windows.Forms.TabPage();
            this.eKeyId2 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.eMerchant2Id = new System.Windows.Forms.TextBox();
            this.eMerchant2Name = new System.Windows.Forms.TextBox();
            this.ePublicKey2 = new System.Windows.Forms.TextBox();
            this.ePrivateKey2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.udClearAfter = new System.Windows.Forms.NumericUpDown();
            this.cbAutoClear = new System.Windows.Forms.CheckBox();
            this.rbHex = new System.Windows.Forms.RadioButton();
            this.rbBase64 = new System.Windows.Forms.RadioButton();
            this.lkSetDefault = new System.Windows.Forms.LinkLabel();
            this.lkClose = new System.Windows.Forms.LinkLabel();
            this.pBottom = new System.Windows.Forms.Panel();
            this.pMain = new System.Windows.Forms.Panel();
            this.tabMerchants.SuspendLayout();
            this.tabMerchant1.SuspendLayout();
            this.tabMerchant2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udClearAfter)).BeginInit();
            this.pBottom.SuspendLayout();
            this.pMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMerchants
            // 
            this.tabMerchants.Controls.Add(this.tabMerchant1);
            this.tabMerchants.Controls.Add(this.tabMerchant2);
            this.tabMerchants.Controls.Add(this.tabPage1);
            this.tabMerchants.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMerchants.Location = new System.Drawing.Point(6, 6);
            this.tabMerchants.Name = "tabMerchants";
            this.tabMerchants.SelectedIndex = 0;
            this.tabMerchants.Size = new System.Drawing.Size(655, 347);
            this.tabMerchants.TabIndex = 21;
            // 
            // tabMerchant1
            // 
            this.tabMerchant1.Controls.Add(this.eKeyId1);
            this.tabMerchant1.Controls.Add(this.label9);
            this.tabMerchant1.Controls.Add(this.label1);
            this.tabMerchant1.Controls.Add(this.eMerchant1Id);
            this.tabMerchant1.Controls.Add(this.eMerchant1Name);
            this.tabMerchant1.Controls.Add(this.ePublicKey1);
            this.tabMerchant1.Controls.Add(this.ePrivateKey1);
            this.tabMerchant1.Controls.Add(this.lbMerchantName);
            this.tabMerchant1.Controls.Add(this.label3);
            this.tabMerchant1.Controls.Add(this.lbPrivateKey);
            this.tabMerchant1.Location = new System.Drawing.Point(4, 22);
            this.tabMerchant1.Name = "tabMerchant1";
            this.tabMerchant1.Padding = new System.Windows.Forms.Padding(3);
            this.tabMerchant1.Size = new System.Drawing.Size(647, 321);
            this.tabMerchant1.TabIndex = 0;
            this.tabMerchant1.Text = "Merchant #1";
            this.tabMerchant1.UseVisualStyleBackColor = true;
            // 
            // eKeyId1
            // 
            this.eKeyId1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eKeyId1.Location = new System.Drawing.Point(9, 216);
            this.eKeyId1.Name = "eKeyId1";
            this.eKeyId1.ReadOnly = true;
            this.eKeyId1.Size = new System.Drawing.Size(154, 26);
            this.eKeyId1.TabIndex = 27;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 201);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(42, 13);
            this.label9.TabIndex = 28;
            this.label9.Text = "Key ID:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(202, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Merchant ID (hash of passTypeIdentifier):";
            // 
            // eMerchant1Id
            // 
            this.eMerchant1Id.BackColor = System.Drawing.Color.WhiteSmoke;
            this.eMerchant1Id.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eMerchant1Id.Location = new System.Drawing.Point(9, 81);
            this.eMerchant1Id.Name = "eMerchant1Id";
            this.eMerchant1Id.ReadOnly = true;
            this.eMerchant1Id.Size = new System.Drawing.Size(632, 26);
            this.eMerchant1Id.TabIndex = 4;
            this.eMerchant1Id.TextChanged += new System.EventHandler(this.eMerchant1Id_TextChanged);
            // 
            // eMerchant1Name
            // 
            this.eMerchant1Name.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eMerchant1Name.Location = new System.Drawing.Point(9, 31);
            this.eMerchant1Name.Name = "eMerchant1Name";
            this.eMerchant1Name.Size = new System.Drawing.Size(632, 26);
            this.eMerchant1Name.TabIndex = 2;
            this.eMerchant1Name.TextChanged += new System.EventHandler(this.eMerchantName1_TextChanged);
            // 
            // ePublicKey1
            // 
            this.ePublicKey1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ePublicKey1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePublicKey1.Location = new System.Drawing.Point(9, 265);
            this.ePublicKey1.Multiline = true;
            this.ePublicKey1.Name = "ePublicKey1";
            this.ePublicKey1.ReadOnly = true;
            this.ePublicKey1.Size = new System.Drawing.Size(632, 48);
            this.ePublicKey1.TabIndex = 6;
            // 
            // ePrivateKey1
            // 
            this.ePrivateKey1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePrivateKey1.Location = new System.Drawing.Point(9, 131);
            this.ePrivateKey1.Multiline = true;
            this.ePrivateKey1.Name = "ePrivateKey1";
            this.ePrivateKey1.Size = new System.Drawing.Size(632, 64);
            this.ePrivateKey1.TabIndex = 5;
            this.ePrivateKey1.TextChanged += new System.EventHandler(this.ePrivateKey1_TextChanged);
            // 
            // lbMerchantName
            // 
            this.lbMerchantName.AutoSize = true;
            this.lbMerchantName.Location = new System.Drawing.Point(6, 16);
            this.lbMerchantName.Name = "lbMerchantName";
            this.lbMerchantName.Size = new System.Drawing.Size(131, 13);
            this.lbMerchantName.TabIndex = 13;
            this.lbMerchantName.Text = "Name (passTypeIdentifer):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 250);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Merchant\'s Public Key:";
            // 
            // lbPrivateKey
            // 
            this.lbPrivateKey.AutoSize = true;
            this.lbPrivateKey.Location = new System.Drawing.Point(5, 116);
            this.lbPrivateKey.Name = "lbPrivateKey";
            this.lbPrivateKey.Size = new System.Drawing.Size(181, 13);
            this.lbPrivateKey.TabIndex = 15;
            this.lbPrivateKey.Text = "Merchant\'s Private Key or Key Index:";
            // 
            // tabMerchant2
            // 
            this.tabMerchant2.Controls.Add(this.eKeyId2);
            this.tabMerchant2.Controls.Add(this.label8);
            this.tabMerchant2.Controls.Add(this.label4);
            this.tabMerchant2.Controls.Add(this.eMerchant2Id);
            this.tabMerchant2.Controls.Add(this.eMerchant2Name);
            this.tabMerchant2.Controls.Add(this.ePublicKey2);
            this.tabMerchant2.Controls.Add(this.ePrivateKey2);
            this.tabMerchant2.Controls.Add(this.label5);
            this.tabMerchant2.Controls.Add(this.label6);
            this.tabMerchant2.Controls.Add(this.label7);
            this.tabMerchant2.Location = new System.Drawing.Point(4, 22);
            this.tabMerchant2.Name = "tabMerchant2";
            this.tabMerchant2.Padding = new System.Windows.Forms.Padding(3);
            this.tabMerchant2.Size = new System.Drawing.Size(647, 321);
            this.tabMerchant2.TabIndex = 1;
            this.tabMerchant2.Text = "Merchant #2";
            this.tabMerchant2.UseVisualStyleBackColor = true;
            // 
            // eKeyId2
            // 
            this.eKeyId2.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eKeyId2.Location = new System.Drawing.Point(9, 216);
            this.eKeyId2.Name = "eKeyId2";
            this.eKeyId2.ReadOnly = true;
            this.eKeyId2.Size = new System.Drawing.Size(154, 26);
            this.eKeyId2.TabIndex = 25;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 201);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "Key ID:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(202, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Merchant ID (hash of passTypeIdentifier):";
            // 
            // eMerchant2Id
            // 
            this.eMerchant2Id.BackColor = System.Drawing.Color.WhiteSmoke;
            this.eMerchant2Id.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eMerchant2Id.Location = new System.Drawing.Point(9, 81);
            this.eMerchant2Id.Name = "eMerchant2Id";
            this.eMerchant2Id.ReadOnly = true;
            this.eMerchant2Id.Size = new System.Drawing.Size(632, 26);
            this.eMerchant2Id.TabIndex = 18;
            this.eMerchant2Id.TextChanged += new System.EventHandler(this.eMerchant2Id_TextChanged);
            // 
            // eMerchant2Name
            // 
            this.eMerchant2Name.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eMerchant2Name.Location = new System.Drawing.Point(9, 31);
            this.eMerchant2Name.Name = "eMerchant2Name";
            this.eMerchant2Name.Size = new System.Drawing.Size(632, 26);
            this.eMerchant2Name.TabIndex = 17;
            this.eMerchant2Name.TextChanged += new System.EventHandler(this.eMerchant2Name_TextChanged);
            // 
            // ePublicKey2
            // 
            this.ePublicKey2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ePublicKey2.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePublicKey2.Location = new System.Drawing.Point(9, 265);
            this.ePublicKey2.Multiline = true;
            this.ePublicKey2.Name = "ePublicKey2";
            this.ePublicKey2.ReadOnly = true;
            this.ePublicKey2.Size = new System.Drawing.Size(632, 48);
            this.ePublicKey2.TabIndex = 20;
            // 
            // ePrivateKey2
            // 
            this.ePrivateKey2.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePrivateKey2.Location = new System.Drawing.Point(9, 131);
            this.ePrivateKey2.Multiline = true;
            this.ePrivateKey2.Name = "ePrivateKey2";
            this.ePrivateKey2.Size = new System.Drawing.Size(632, 64);
            this.ePrivateKey2.TabIndex = 19;
            this.ePrivateKey2.TextChanged += new System.EventHandler(this.ePrivateKey2_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(131, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Name (passTypeIdentifer):";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 250);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(115, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "Merchant\'s Public Key:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 116);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(181, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "Merchant\'s Private Key or Key Index:";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.udClearAfter);
            this.tabPage1.Controls.Add(this.cbAutoClear);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(617, 321);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "UI Settings";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(204, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "seconds";
            // 
            // udClearAfter
            // 
            this.udClearAfter.Location = new System.Drawing.Point(128, 18);
            this.udClearAfter.Name = "udClearAfter";
            this.udClearAfter.Size = new System.Drawing.Size(70, 20);
            this.udClearAfter.TabIndex = 1;
            // 
            // cbAutoClear
            // 
            this.cbAutoClear.AutoSize = true;
            this.cbAutoClear.Location = new System.Drawing.Point(20, 19);
            this.cbAutoClear.Name = "cbAutoClear";
            this.cbAutoClear.Size = new System.Drawing.Size(102, 17);
            this.cbAutoClear.TabIndex = 0;
            this.cbAutoClear.Text = "Clear pass after:";
            this.cbAutoClear.UseVisualStyleBackColor = true;
            this.cbAutoClear.CheckedChanged += new System.EventHandler(this.cbAutoClear_CheckedChanged);
            // 
            // rbHex
            // 
            this.rbHex.AutoSize = true;
            this.rbHex.Location = new System.Drawing.Point(86, 8);
            this.rbHex.Name = "rbHex";
            this.rbHex.Size = new System.Drawing.Size(44, 17);
            this.rbHex.TabIndex = 30;
            this.rbHex.Text = "Hex";
            this.rbHex.UseVisualStyleBackColor = true;
            this.rbHex.CheckedChanged += new System.EventHandler(this.rbBase_CheckedChanged);
            // 
            // rbBase64
            // 
            this.rbBase64.AutoSize = true;
            this.rbBase64.Checked = true;
            this.rbBase64.Location = new System.Drawing.Point(10, 8);
            this.rbBase64.Name = "rbBase64";
            this.rbBase64.Size = new System.Drawing.Size(61, 17);
            this.rbBase64.TabIndex = 29;
            this.rbBase64.TabStop = true;
            this.rbBase64.Text = "Base64";
            this.rbBase64.UseVisualStyleBackColor = true;
            this.rbBase64.CheckedChanged += new System.EventHandler(this.rbBase_CheckedChanged);
            // 
            // lkSetDefault
            // 
            this.lkSetDefault.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetDefault.AutoSize = true;
            this.lkSetDefault.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetDefault.Location = new System.Drawing.Point(277, 10);
            this.lkSetDefault.Name = "lkSetDefault";
            this.lkSetDefault.Size = new System.Drawing.Size(110, 13);
            this.lkSetDefault.TabIndex = 22;
            this.lkSetDefault.TabStop = true;
            this.lkSetDefault.Text = "Use test configuration";
            this.lkSetDefault.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lkSetDefault.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkSetDefault.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkSetDefault_LinkClicked);
            // 
            // lkClose
            // 
            this.lkClose.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClose.AutoSize = true;
            this.lkClose.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClose.Location = new System.Drawing.Point(618, 10);
            this.lkClose.Name = "lkClose";
            this.lkClose.Size = new System.Drawing.Size(33, 13);
            this.lkClose.TabIndex = 23;
            this.lkClose.TabStop = true;
            this.lkClose.Text = "Close";
            this.lkClose.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClose.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkClose_LinkClicked);
            // 
            // pBottom
            // 
            this.pBottom.Controls.Add(this.rbHex);
            this.pBottom.Controls.Add(this.lkSetDefault);
            this.pBottom.Controls.Add(this.rbBase64);
            this.pBottom.Controls.Add(this.lkClose);
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Location = new System.Drawing.Point(0, 359);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(667, 33);
            this.pBottom.TabIndex = 24;
            // 
            // pMain
            // 
            this.pMain.Controls.Add(this.tabMerchants);
            this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pMain.Location = new System.Drawing.Point(0, 0);
            this.pMain.Name = "pMain";
            this.pMain.Padding = new System.Windows.Forms.Padding(6);
            this.pMain.Size = new System.Drawing.Size(667, 359);
            this.pMain.TabIndex = 25;
            // 
            // AppleVASConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 392);
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.pBottom);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AppleVASConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AppleVASConfigForm";
            this.Load += new System.EventHandler(this.AppleVASConfigForm_Load);
            this.tabMerchants.ResumeLayout(false);
            this.tabMerchant1.ResumeLayout(false);
            this.tabMerchant1.PerformLayout();
            this.tabMerchant2.ResumeLayout(false);
            this.tabMerchant2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udClearAfter)).EndInit();
            this.pBottom.ResumeLayout(false);
            this.pBottom.PerformLayout();
            this.pMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabMerchants;
        private System.Windows.Forms.TabPage tabMerchant1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox eMerchant1Id;
        private System.Windows.Forms.TextBox eMerchant1Name;
        private System.Windows.Forms.TextBox ePublicKey1;
        private System.Windows.Forms.TextBox ePrivateKey1;
        private System.Windows.Forms.Label lbMerchantName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbPrivateKey;
        private System.Windows.Forms.TabPage tabMerchant2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox eMerchant2Id;
        private System.Windows.Forms.TextBox eMerchant2Name;
        private System.Windows.Forms.TextBox ePublicKey2;
        private System.Windows.Forms.TextBox ePrivateKey2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.LinkLabel lkSetDefault;
        private System.Windows.Forms.LinkLabel lkClose;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udClearAfter;
        private System.Windows.Forms.CheckBox cbAutoClear;
        private System.Windows.Forms.TextBox eKeyId1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox eKeyId2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel pBottom;
        private System.Windows.Forms.Panel pMain;
        private System.Windows.Forms.RadioButton rbHex;
        private System.Windows.Forms.RadioButton rbBase64;
    }
}