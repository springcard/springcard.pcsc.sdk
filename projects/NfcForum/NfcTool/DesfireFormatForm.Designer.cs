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
			this.tOldRootKey = new System.Windows.Forms.MaskedTextBox();
			this.tNewRootKey = new System.Windows.Forms.MaskedTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tSize = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cbAllMemory = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// tOldRootKey
			// 
			this.tOldRootKey.Location = new System.Drawing.Point(133, 13);
			this.tOldRootKey.Name = "tOldRootKey";
			this.tOldRootKey.PasswordChar = '*';
			this.tOldRootKey.Size = new System.Drawing.Size(241, 20);
			this.tOldRootKey.TabIndex = 0;
			// 
			// tNewRootKey
			// 
			this.tNewRootKey.Location = new System.Drawing.Point(133, 39);
			this.tNewRootKey.Name = "tNewRootKey";
			this.tNewRootKey.PasswordChar = '*';
			this.tNewRootKey.Size = new System.Drawing.Size(241, 20);
			this.tNewRootKey.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label1.Location = new System.Drawing.Point(3, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(124, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Enter actual root key :";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.Location = new System.Drawing.Point(12, 37);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(115, 23);
			this.label2.TabIndex = 3;
			this.label2.Text = "Enter new root key:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tSize
			// 
			this.tSize.Enabled = false;
			this.tSize.Location = new System.Drawing.Point(298, 69);
			this.tSize.Name = "tSize";
			this.tSize.Size = new System.Drawing.Size(76, 20);
			this.tSize.TabIndex = 4;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(209, 67);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(81, 23);
			this.label3.TabIndex = 5;
			this.label3.Text = "Size (in bytes):";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cbAllMemory
			// 
			this.cbAllMemory.Checked = true;
			this.cbAllMemory.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbAllMemory.Location = new System.Drawing.Point(30, 67);
			this.cbAllMemory.Name = "cbAllMemory";
			this.cbAllMemory.Size = new System.Drawing.Size(182, 24);
			this.cbAllMemory.TabIndex = 6;
			this.cbAllMemory.Text = "Use all available free memory";
			this.cbAllMemory.UseVisualStyleBackColor = true;
			this.cbAllMemory.CheckedChanged += new System.EventHandler(this.CheckBox1CheckedChanged);
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(69, 104);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 7;
			this.button1.Text = "Format";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.Button1Click);
			// 
			// button2
			// 
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new System.Drawing.Point(247, 104);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 8;
			this.button2.Text = "Cancel";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.Button2Click);
			// 
			// DesfireFormatForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(392, 139);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.cbAllMemory);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.tSize);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tNewRootKey);
			this.Controls.Add(this.tOldRootKey);
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
		private System.Windows.Forms.TextBox tSize;
		private System.Windows.Forms.CheckBox cbAllMemory;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.MaskedTextBox tNewRootKey;
		private System.Windows.Forms.MaskedTextBox tOldRootKey;

	}
}
