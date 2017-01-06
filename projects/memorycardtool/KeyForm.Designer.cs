/*
 * Created by SharpDevelop.
 * User: herve.t
 * Date: 29/01/2016
 * Time: 10:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MemoryCardTool
{
	partial class KeyForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label lblWhat;
		private System.Windows.Forms.ComboBox cbKey;
		
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
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.lblWhat = new System.Windows.Forms.Label();
			this.cbKey = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(36, 59);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(117, 59);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "Ok";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
			// 
			// lblWhat
			// 
			this.lblWhat.AutoSize = true;
			this.lblWhat.Location = new System.Drawing.Point(4, 13);
			this.lblWhat.Name = "lblWhat";
			this.lblWhat.Size = new System.Drawing.Size(218, 13);
			this.lblWhat.TabIndex = 4;
			this.lblWhat.Text = "Please type the writing key for the sector {0}:";
			// 
			// cbKey
			// 
			this.cbKey.FormattingEnabled = true;
			this.cbKey.Location = new System.Drawing.Point(18, 30);
			this.cbKey.MaxLength = 12;
			this.cbKey.Name = "cbKey";
			this.cbKey.Size = new System.Drawing.Size(193, 21);
			this.cbKey.TabIndex = 0;
			// 
			// KeyForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(228, 95);
			this.ControlBox = false;
			this.Controls.Add(this.cbKey);
			this.Controls.Add(this.lblWhat);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "KeyForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Key";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
