namespace getReaderInformation
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
			this.label5 = new System.Windows.Forms.Label();
			this.txtInfoReader = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnRead = new System.Windows.Forms.Button();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.cbReaders = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 22);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(73, 13);
			this.label5.TabIndex = 30;
			this.label5.Text = "Reader\'s data";
			// 
			// txtInfoReader
			// 
			this.txtInfoReader.Location = new System.Drawing.Point(147, 19);
			this.txtInfoReader.Multiline = true;
			this.txtInfoReader.Name = "txtInfoReader";
			this.txtInfoReader.Size = new System.Drawing.Size(354, 178);
			this.txtInfoReader.TabIndex = 29;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.txtInfoReader);
			this.groupBox1.Location = new System.Drawing.Point(8, 65);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(511, 203);
			this.groupBox1.TabIndex = 47;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Result";
			// 
			// btnRead
			// 
			this.btnRead.Enabled = false;
			this.btnRead.Location = new System.Drawing.Point(152, 36);
			this.btnRead.Name = "btnRead";
			this.btnRead.Size = new System.Drawing.Size(106, 23);
			this.btnRead.TabIndex = 46;
			this.btnRead.Text = "Read information";
			this.btnRead.UseVisualStyleBackColor = true;
			this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
			// 
			// btnRefresh
			// 
			this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
			this.btnRefresh.Location = new System.Drawing.Point(491, 5);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(26, 30);
			this.btnRefresh.TabIndex = 45;
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// cbReaders
			// 
			this.cbReaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbReaders.FormattingEnabled = true;
			this.cbReaders.Location = new System.Drawing.Point(152, 11);
			this.cbReaders.Name = "cbReaders";
			this.cbReaders.Size = new System.Drawing.Size(333, 21);
			this.cbReaders.TabIndex = 40;
			this.cbReaders.SelectedIndexChanged += new System.EventHandler(this.cbReaders_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 39;
			this.label1.Text = "Available readers:";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 273);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnRead);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.cbReaders);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "Get reader information";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtInfoReader;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ComboBox cbReaders;
        private System.Windows.Forms.Label label1;
    }
}

