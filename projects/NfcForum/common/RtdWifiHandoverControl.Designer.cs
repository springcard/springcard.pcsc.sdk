/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 21/05/2013
 * Time: 16:02
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCardApplication.Controls
{
	partial class RtdWifiHandoverControl
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
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
			this.lbSSID = new System.Windows.Forms.Label();
			this.lbKeyType = new System.Windows.Forms.Label();
			this.lbKey = new System.Windows.Forms.Label();
			this.tbSSID = new System.Windows.Forms.TextBox();
			this.tbKey = new System.Windows.Forms.TextBox();
			this.cbKeyType = new System.Windows.Forms.ComboBox();
			this.lbCarrierPowerState = new System.Windows.Forms.Label();
			this.cbPowerState = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// lbSSID
			// 
			this.lbSSID.Location = new System.Drawing.Point(3, 53);
			this.lbSSID.Name = "lbSSID";
			this.lbSSID.Size = new System.Drawing.Size(57, 23);
			this.lbSSID.TabIndex = 0;
			this.lbSSID.Text = "SSID:";
			this.lbSSID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lbKeyType
			// 
			this.lbKeyType.Location = new System.Drawing.Point(3, 102);
			this.lbKeyType.Name = "lbKeyType";
			this.lbKeyType.Size = new System.Drawing.Size(57, 23);
			this.lbKeyType.TabIndex = 1;
			this.lbKeyType.Text = "Key Type:";
			this.lbKeyType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lbKey
			// 
			this.lbKey.Location = new System.Drawing.Point(3, 152);
			this.lbKey.Name = "lbKey";
			this.lbKey.Size = new System.Drawing.Size(57, 23);
			this.lbKey.TabIndex = 2;
			this.lbKey.Text = "Key:";
			this.lbKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tbSSID
			// 
			this.tbSSID.Location = new System.Drawing.Point(3, 79);
			this.tbSSID.Name = "tbSSID";
			this.tbSSID.Size = new System.Drawing.Size(220, 20);
			this.tbSSID.TabIndex = 3;
			// 
			// tbKey
			// 
			this.tbKey.Location = new System.Drawing.Point(3, 178);
			this.tbKey.Name = "tbKey";
			this.tbKey.Size = new System.Drawing.Size(220, 20);
			this.tbKey.TabIndex = 4;
			// 
			// cbKeyType
			// 
			this.cbKeyType.FormattingEnabled = true;
			this.cbKeyType.Items.AddRange(new object[] {
									"None",
									"WEP",
									"WPA/WPA2"});
			this.cbKeyType.Location = new System.Drawing.Point(3, 128);
			this.cbKeyType.Name = "cbKeyType";
			this.cbKeyType.Size = new System.Drawing.Size(121, 21);
			this.cbKeyType.TabIndex = 5;
			// 
			// lbCarrierPowerState
			// 
			this.lbCarrierPowerState.Location = new System.Drawing.Point(3, 3);
			this.lbCarrierPowerState.Name = "lbCarrierPowerState";
			this.lbCarrierPowerState.Size = new System.Drawing.Size(121, 23);
			this.lbCarrierPowerState.TabIndex = 6;
			this.lbCarrierPowerState.Text = "Carrier Power State";
			this.lbCarrierPowerState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cbPowerState
			// 
			this.cbPowerState.FormattingEnabled = true;
			this.cbPowerState.Items.AddRange(new object[] {
									"Active",
									"Inactive",
									"Activating",
									"Unknown"});
			this.cbPowerState.Location = new System.Drawing.Point(3, 29);
			this.cbPowerState.Name = "cbPowerState";
			this.cbPowerState.Size = new System.Drawing.Size(94, 21);
			this.cbPowerState.TabIndex = 7;
			// 
			// RtdWifiHandoverControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.cbPowerState);
			this.Controls.Add(this.lbCarrierPowerState);
			this.Controls.Add(this.cbKeyType);
			this.Controls.Add(this.tbKey);
			this.Controls.Add(this.tbSSID);
			this.Controls.Add(this.lbKey);
			this.Controls.Add(this.lbKeyType);
			this.Controls.Add(this.lbSSID);
			this.Name = "RtdWifiHandoverControl";
			this.Size = new System.Drawing.Size(490, 294);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Label lbSSID;
		private System.Windows.Forms.Label lbKeyType;
		private System.Windows.Forms.Label lbKey;
		private System.Windows.Forms.TextBox tbSSID;
		private System.Windows.Forms.TextBox tbKey;
		private System.Windows.Forms.ComboBox cbPowerState;
		private System.Windows.Forms.Label lbCarrierPowerState;
		private System.Windows.Forms.ComboBox cbKeyType;
	}
}
