/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 16/04/2012
 * Time: 14:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCardApplication
{
	partial class DesfireFormatForm
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
            this.eCustomRootKey = new System.Windows.Forms.MaskedTextBox();
            this.btnFormat = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbUseCustomRootKey = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // eCustomRootKey
            // 
            this.eCustomRootKey.Enabled = false;
            this.eCustomRootKey.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eCustomRootKey.Location = new System.Drawing.Point(128, 138);
            this.eCustomRootKey.Name = "eCustomRootKey";
            this.eCustomRootKey.Size = new System.Drawing.Size(241, 22);
            this.eCustomRootKey.TabIndex = 0;
            this.eCustomRootKey.Text = "00000000000000000000000000000000";
            // 
            // btnFormat
            // 
            this.btnFormat.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnFormat.Location = new System.Drawing.Point(172, 69);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new System.Drawing.Size(75, 23);
            this.btnFormat.TabIndex = 7;
            this.btnFormat.Text = "Format";
            this.btnFormat.UseVisualStyleBackColor = true;
            this.btnFormat.Click += new System.EventHandler(this.BtnFormatClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(253, 69);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // cbUseCustomRootKey
            // 
            this.cbUseCustomRootKey.AutoSize = true;
            this.cbUseCustomRootKey.Location = new System.Drawing.Point(128, 115);
            this.cbUseCustomRootKey.Name = "cbUseCustomRootKey";
            this.cbUseCustomRootKey.Size = new System.Drawing.Size(135, 17);
            this.cbUseCustomRootKey.TabIndex = 9;
            this.cbUseCustomRootKey.Text = "Use a custom root key:";
            this.cbUseCustomRootKey.UseVisualStyleBackColor = true;
            this.cbUseCustomRootKey.CheckedChanged += new System.EventHandler(this.cbUseCustomRootKey_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 175);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "If a custom root key is specified,";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 190);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(272, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "- the application will try to use this key for authentication,";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 205);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(466, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "- if the card is blank (authentication OK with default key), the root key will be" +
    " changed by this one.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(244, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "The application is ready to format the Desfire card.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(360, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Any existing data on the card will be lost. Are you sure you want to do that?";
            // 
            // DesfireFormatForm
            // 
            this.AcceptButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(488, 231);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbUseCustomRootKey);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnFormat);
            this.Controls.Add(this.eCustomRootKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DesfireFormatForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Format Desfire Card";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnFormat;
		private System.Windows.Forms.MaskedTextBox eCustomRootKey;
        private System.Windows.Forms.CheckBox cbUseCustomRootKey;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}
