namespace getCardUid
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.cbReaders = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblCardAtr = new System.Windows.Forms.Label();
            this.lblCardUid = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lblCardType = new System.Windows.Forms.TextBox();
            this.lblProtocol = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Available readers:";
            // 
            // cbReaders
            // 
            this.cbReaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbReaders.FormattingEnabled = true;
            this.cbReaders.Location = new System.Drawing.Point(151, 10);
            this.cbReaders.Name = "cbReaders";
            this.cbReaders.Size = new System.Drawing.Size(367, 21);
            this.cbReaders.TabIndex = 1;
            this.cbReaders.SelectedIndexChanged += new System.EventHandler(this.cbReaders_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Reader current status:";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(151, 42);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(109, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Reader current status";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Card\'s ATR:";
            // 
            // lblCardAtr
            // 
            this.lblCardAtr.AutoSize = true;
            this.lblCardAtr.Location = new System.Drawing.Point(151, 66);
            this.lblCardAtr.Name = "lblCardAtr";
            this.lblCardAtr.Size = new System.Drawing.Size(344, 13);
            this.lblCardAtr.TabIndex = 5;
            this.lblCardAtr.Text = "XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX";
            // 
            // lblCardUid
            // 
            this.lblCardUid.AutoSize = true;
            this.lblCardUid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCardUid.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.lblCardUid.Location = new System.Drawing.Point(151, 90);
            this.lblCardUid.Name = "lblCardUid";
            this.lblCardUid.Size = new System.Drawing.Size(140, 13);
            this.lblCardUid.TabIndex = 7;
            this.lblCardUid.Text = "XX XX XX XX XX XX XX XX";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Card\'s UID:";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.Location = new System.Drawing.Point(523, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(27, 30);
            this.btnRefresh.TabIndex = 8;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Card\'s type:";
            // 
            // lblCardType
            // 
            this.lblCardType.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblCardType.Location = new System.Drawing.Point(151, 138);
            this.lblCardType.Multiline = true;
            this.lblCardType.Name = "lblCardType";
            this.lblCardType.ReadOnly = true;
            this.lblCardType.Size = new System.Drawing.Size(367, 85);
            this.lblCardType.TabIndex = 11;
            // 
            // lblProtocol
            // 
            this.lblProtocol.AutoSize = true;
            this.lblProtocol.Location = new System.Drawing.Point(151, 114);
            this.lblProtocol.Name = "lblProtocol";
            this.lblProtocol.Size = new System.Drawing.Size(21, 13);
            this.lblProtocol.TabIndex = 13;
            this.lblProtocol.Text = "XX";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 114);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Protocol:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 233);
            this.Controls.Add(this.lblProtocol);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblCardType);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.lblCardUid);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblCardAtr);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbReaders);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Get card\'s UID";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbReaders;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblCardAtr;
        private System.Windows.Forms.Label lblCardUid;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox lblCardType;
        private System.Windows.Forms.Label lblProtocol;
        private System.Windows.Forms.Label label7;
    }
}

