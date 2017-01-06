namespace writeMifareClassic
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
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.txtApduSent = new System.Windows.Forms.TextBox();
			this.txtDataSent = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtFinalStatus = new System.Windows.Forms.TextBox();
			this.btnWrite = new System.Windows.Forms.Button();
			this.txtData = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.lblCardAtr = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cbReaders = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.numBlockNumber = new System.Windows.Forms.NumericUpDown();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBlockNumber)).BeginInit();
			this.SuspendLayout();
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(7, 217);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(63, 13);
			this.label7.TabIndex = 32;
			this.label7.Text = "Final status:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(7, 115);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(56, 13);
			this.label6.TabIndex = 31;
			this.label6.Text = "Data sent:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 22);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(63, 13);
			this.label5.TabIndex = 30;
			this.label5.Text = "APDU sent:";
			// 
			// txtApduSent
			// 
			this.txtApduSent.Location = new System.Drawing.Point(151, 19);
			this.txtApduSent.Multiline = true;
			this.txtApduSent.Name = "txtApduSent";
			this.txtApduSent.Size = new System.Drawing.Size(354, 84);
			this.txtApduSent.TabIndex = 29;
			// 
			// txtDataSent
			// 
			this.txtDataSent.Location = new System.Drawing.Point(151, 112);
			this.txtDataSent.Multiline = true;
			this.txtDataSent.Name = "txtDataSent";
			this.txtDataSent.Size = new System.Drawing.Size(351, 96);
			this.txtDataSent.TabIndex = 27;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.txtApduSent);
			this.groupBox1.Controls.Add(this.txtFinalStatus);
			this.groupBox1.Controls.Add(this.txtDataSent);
			this.groupBox1.Location = new System.Drawing.Point(7, 177);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(511, 242);
			this.groupBox1.TabIndex = 38;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Result && status";
			// 
			// txtFinalStatus
			// 
			this.txtFinalStatus.Location = new System.Drawing.Point(151, 214);
			this.txtFinalStatus.Name = "txtFinalStatus";
			this.txtFinalStatus.Size = new System.Drawing.Size(351, 20);
			this.txtFinalStatus.TabIndex = 28;
			// 
			// btnWrite
			// 
			this.btnWrite.Enabled = false;
			this.btnWrite.Location = new System.Drawing.Point(154, 146);
			this.btnWrite.Name = "btnWrite";
			this.btnWrite.Size = new System.Drawing.Size(75, 23);
			this.btnWrite.TabIndex = 37;
			this.btnWrite.Text = "Write data";
			this.btnWrite.UseVisualStyleBackColor = true;
			this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
			// 
			// txtData
			// 
			this.txtData.Location = new System.Drawing.Point(154, 88);
			this.txtData.MaxLength = 16;
			this.txtData.Name = "txtData";
			this.txtData.Size = new System.Drawing.Size(128, 20);
			this.txtData.TabIndex = 36;
			this.txtData.Text = "ABCDEFGHIJKLMNOP";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 91);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(98, 13);
			this.label4.TabIndex = 35;
			this.label4.Text = "ASCII data to write:";
			// 
			// btnRefresh
			// 
			this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
			this.btnRefresh.Location = new System.Drawing.Point(483, 5);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(26, 30);
			this.btnRefresh.TabIndex = 34;
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// lblCardAtr
			// 
			this.lblCardAtr.AutoSize = true;
			this.lblCardAtr.Location = new System.Drawing.Point(151, 67);
			this.lblCardAtr.Name = "lblCardAtr";
			this.lblCardAtr.Size = new System.Drawing.Size(344, 13);
			this.lblCardAtr.TabIndex = 33;
			this.lblCardAtr.Text = "XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 67);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 13);
			this.label3.TabIndex = 32;
			this.label3.Text = "Card\'s ATR:";
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(151, 41);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(109, 13);
			this.lblStatus.TabIndex = 31;
			this.lblStatus.Text = "Reader current status";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(112, 13);
			this.label2.TabIndex = 30;
			this.label2.Text = "Reader current status:";
			// 
			// cbReaders
			// 
			this.cbReaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbReaders.FormattingEnabled = true;
			this.cbReaders.Location = new System.Drawing.Point(151, 11);
			this.cbReaders.Name = "cbReaders";
			this.cbReaders.Size = new System.Drawing.Size(326, 21);
			this.cbReaders.TabIndex = 29;
			this.cbReaders.SelectedIndexChanged += new System.EventHandler(this.cbReaders_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 28;
			this.label1.Text = "Available readers:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 116);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(48, 13);
			this.label8.TabIndex = 39;
			this.label8.Text = "Address:";
			// 
			// numBlockNumber
			// 
			this.numBlockNumber.Enabled = false;
			this.numBlockNumber.Location = new System.Drawing.Point(154, 114);
			this.numBlockNumber.Name = "numBlockNumber";
			this.numBlockNumber.Size = new System.Drawing.Size(75, 20);
			this.numBlockNumber.TabIndex = 50;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 424);
			this.Controls.Add(this.numBlockNumber);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnWrite);
			this.Controls.Add(this.txtData);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.lblCardAtr);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cbReaders);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "Write Mifare Classic";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBlockNumber)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtApduSent;
        private System.Windows.Forms.TextBox txtDataSent;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtFinalStatus;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblCardAtr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbReaders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numBlockNumber;
    }
}

