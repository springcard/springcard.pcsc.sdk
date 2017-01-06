/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 21/06/2013
 * Time: 16:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MemoryCardTool
{
	partial class UnlockForm
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
			this.btOk = new System.Windows.Forms.Button();
			this.btCancel = new System.Windows.Forms.Button();
			this.lbKeyA = new System.Windows.Forms.Label();
			this.lbKeyB = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cbKeyA = new System.Windows.Forms.ComboBox();
			this.cbKeyB = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// btOk
			// 
			this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btOk.Location = new System.Drawing.Point(231, 38);
			this.btOk.Name = "btOk";
			this.btOk.Size = new System.Drawing.Size(75, 23);
			this.btOk.TabIndex = 3;
			this.btOk.Text = "Ok";
			this.btOk.UseVisualStyleBackColor = true;
			this.btOk.Click += new System.EventHandler(this.BtOkClick);
			// 
			// btCancel
			// 
			this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btCancel.Location = new System.Drawing.Point(231, 15);
			this.btCancel.Name = "btCancel";
			this.btCancel.Size = new System.Drawing.Size(75, 23);
			this.btCancel.TabIndex = 4;
			this.btCancel.Text = "Cancel";
			this.btCancel.UseVisualStyleBackColor = true;
			this.btCancel.Click += new System.EventHandler(this.BtCancelClick);
			// 
			// lbKeyA
			// 
			this.lbKeyA.Location = new System.Drawing.Point(24, 15);
			this.lbKeyA.Name = "lbKeyA";
			this.lbKeyA.Size = new System.Drawing.Size(58, 23);
			this.lbKeyA.TabIndex = 2;
			this.lbKeyA.Text = "Key A";
			this.lbKeyA.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lbKeyB
			// 
			this.lbKeyB.Location = new System.Drawing.Point(24, 38);
			this.lbKeyB.Name = "lbKeyB";
			this.lbKeyB.Size = new System.Drawing.Size(58, 23);
			this.lbKeyB.TabIndex = 3;
			this.lbKeyB.Text = "Key B";
			this.lbKeyB.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(74, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(143, 23);
			this.label1.TabIndex = 12;
			this.label1.Text = "(Leave blank if unknown)";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cbKeyA
			// 
			this.cbKeyA.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbKeyA.FormattingEnabled = true;
			this.cbKeyA.Location = new System.Drawing.Point(88, 17);
			this.cbKeyA.Name = "cbKeyA";
			this.cbKeyA.Size = new System.Drawing.Size(121, 20);
			this.cbKeyA.TabIndex = 13;
			// 
			// cbKeyB
			// 
			this.cbKeyB.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbKeyB.FormattingEnabled = true;
			this.cbKeyB.Location = new System.Drawing.Point(88, 40);
			this.cbKeyB.Name = "cbKeyB";
			this.cbKeyB.Size = new System.Drawing.Size(121, 20);
			this.cbKeyB.TabIndex = 14;
			// 
			// UnlockForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.ClientSize = new System.Drawing.Size(328, 96);
			this.Controls.Add(this.cbKeyB);
			this.Controls.Add(this.cbKeyA);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lbKeyB);
			this.Controls.Add(this.lbKeyA);
			this.Controls.Add(this.btCancel);
			this.Controls.Add(this.btOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UnlockForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Specify Keys";
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.ComboBox cbKeyB;
		private System.Windows.Forms.ComboBox cbKeyA;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lbKeyB;
		private System.Windows.Forms.Label lbKeyA;
		private System.Windows.Forms.Button btCancel;
		private System.Windows.Forms.Button btOk;
	}
}
