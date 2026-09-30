
namespace SmartTapRdr
{
    partial class GoogleVASPublicKeyForm
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
            this.ePrivateKey = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.ePublicKey = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ePublicKeyPem = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lkClose = new System.Windows.Forms.LinkLabel();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ePrivateKey
            // 
            this.ePrivateKey.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePrivateKey.Location = new System.Drawing.Point(12, 31);
            this.ePrivateKey.Name = "ePrivateKey";
            this.ePrivateKey.ReadOnly = true;
            this.ePrivateKey.Size = new System.Drawing.Size(613, 26);
            this.ePrivateKey.TabIndex = 10;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(9, 8);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(82, 20);
            this.label13.TabIndex = 9;
            this.label13.Text = "Private Key";
            // 
            // ePublicKey
            // 
            this.ePublicKey.AcceptsReturn = true;
            this.ePublicKey.AcceptsTab = true;
            this.ePublicKey.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePublicKey.Location = new System.Drawing.Point(12, 85);
            this.ePublicKey.Multiline = true;
            this.ePublicKey.Name = "ePublicKey";
            this.ePublicKey.ReadOnly = true;
            this.ePublicKey.Size = new System.Drawing.Size(613, 57);
            this.ePublicKey.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 20);
            this.label1.TabIndex = 11;
            this.label1.Text = "Public Key (hex)";
            // 
            // ePublicKeyPem
            // 
            this.ePublicKeyPem.AcceptsReturn = true;
            this.ePublicKeyPem.AcceptsTab = true;
            this.ePublicKeyPem.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ePublicKeyPem.Location = new System.Drawing.Point(13, 169);
            this.ePublicKeyPem.Multiline = true;
            this.ePublicKeyPem.Name = "ePublicKeyPem";
            this.ePublicKeyPem.ReadOnly = true;
            this.ePublicKeyPem.Size = new System.Drawing.Size(612, 105);
            this.ePublicKeyPem.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 20);
            this.label2.TabIndex = 13;
            this.label2.Text = "Public Key (PEM)";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.Controls.Add(this.lkClose);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 294);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(637, 65);
            this.panel2.TabIndex = 15;
            // 
            // lkClose
            // 
            this.lkClose.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClose.AutoSize = true;
            this.lkClose.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClose.Location = new System.Drawing.Point(580, 20);
            this.lkClose.Name = "lkClose";
            this.lkClose.Size = new System.Drawing.Size(45, 20);
            this.lkClose.TabIndex = 2;
            this.lkClose.TabStop = true;
            this.lkClose.Text = "Close";
            this.lkClose.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lkClose.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkClose_LinkClicked);
            // 
            // GoogleVASPublicKeyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(637, 359);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.ePublicKeyPem);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ePublicKey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ePrivateKey);
            this.Controls.Add(this.label13);
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "GoogleVASPublicKeyForm";
            this.Text = "Merchant Key";
            this.Shown += new System.EventHandler(this.GoogleVASPublicKeyForm_Shown);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ePrivateKey;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox ePublicKey;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ePublicKeyPem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.LinkLabel lkClose;
    }
}