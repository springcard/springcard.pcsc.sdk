/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 07/08/2013
 * Heure: 09:19
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace SpringCard.LibCs
{
	partial class CommSelectAndSettingsForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.cbCommPort = new System.Windows.Forms.ComboBox();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.rbDefault = new System.Windows.Forms.RadioButton();
			this.rbCustom = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.cbBitRate = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cbDataBits = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cbParity = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cbStopBits = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.cbHandshake = new System.Windows.Forms.ComboBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(13, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Connect using:";
			// 
			// cbCommPort
			// 
			this.cbCommPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbCommPort.FormattingEnabled = true;
			this.cbCommPort.Location = new System.Drawing.Point(105, 22);
			this.cbCommPort.Name = "cbCommPort";
			this.cbCommPort.Size = new System.Drawing.Size(121, 21);
			this.cbCommPort.TabIndex = 1;
			this.cbCommPort.SelectedIndexChanged += new System.EventHandler(this.CbCommSelectedIndexChanged);
			// 
			// btnRefresh
			// 
			this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRefresh.Location = new System.Drawing.Point(232, 20);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(59, 23);
			this.btnRefresh.TabIndex = 2;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.BtnRefreshClick);
			// 
			// rbDefault
			// 
			this.rbDefault.Checked = true;
			this.rbDefault.Location = new System.Drawing.Point(105, 49);
			this.rbDefault.Name = "rbDefault";
			this.rbDefault.Size = new System.Drawing.Size(213, 24);
			this.rbDefault.TabIndex = 3;
			this.rbDefault.TabStop = true;
			this.rbDefault.Text = "Use default parameters";
			this.rbDefault.UseVisualStyleBackColor = true;
			this.rbDefault.CheckedChanged += new System.EventHandler(this.rbParamsChanged);
			// 
			// rbCustom
			// 
			this.rbCustom.Location = new System.Drawing.Point(105, 81);
			this.rbCustom.Name = "rbCustom";
			this.rbCustom.Size = new System.Drawing.Size(213, 24);
			this.rbCustom.TabIndex = 4;
			this.rbCustom.Text = "Custom parameters:";
			this.rbCustom.UseVisualStyleBackColor = true;
			this.rbCustom.CheckedChanged += new System.EventHandler(this.rbParamsChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(13, 108);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 23);
			this.label2.TabIndex = 5;
			this.label2.Text = "Bits per second:";
			// 
			// cbBitRate
			// 
			this.cbBitRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbBitRate.Enabled = false;
			this.cbBitRate.FormattingEnabled = true;
			this.cbBitRate.Items.AddRange(new object[] {
									"1200",
									"2400",
									"4800",
									"9600",
									"19200",
									"38400",
									"57600",
									"115200"});
			this.cbBitRate.Location = new System.Drawing.Point(105, 105);
			this.cbBitRate.Name = "cbBitRate";
			this.cbBitRate.Size = new System.Drawing.Size(121, 21);
			this.cbBitRate.TabIndex = 6;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(13, 137);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 23);
			this.label3.TabIndex = 7;
			this.label3.Text = "Data bits:";
			// 
			// cbDataBits
			// 
			this.cbDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbDataBits.Enabled = false;
			this.cbDataBits.FormattingEnabled = true;
			this.cbDataBits.Items.AddRange(new object[] {
									"7",
									"8"});
			this.cbDataBits.Location = new System.Drawing.Point(105, 135);
			this.cbDataBits.Name = "cbDataBits";
			this.cbDataBits.Size = new System.Drawing.Size(121, 21);
			this.cbDataBits.TabIndex = 8;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(13, 168);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 23);
			this.label4.TabIndex = 9;
			this.label4.Text = "Parity:";
			// 
			// cbParity
			// 
			this.cbParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbParity.Enabled = false;
			this.cbParity.FormattingEnabled = true;
			this.cbParity.Items.AddRange(new object[] {
									"None",
									"Odd",
									"Even"});
			this.cbParity.Location = new System.Drawing.Point(105, 165);
			this.cbParity.Name = "cbParity";
			this.cbParity.Size = new System.Drawing.Size(121, 21);
			this.cbParity.TabIndex = 10;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(13, 198);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 23);
			this.label5.TabIndex = 11;
			this.label5.Text = "Stop bits:";
			// 
			// cbStopBits
			// 
			this.cbStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbStopBits.Enabled = false;
			this.cbStopBits.FormattingEnabled = true;
			this.cbStopBits.Items.AddRange(new object[] {
									"1",
									"1.5",
									"2"});
			this.cbStopBits.Location = new System.Drawing.Point(105, 195);
			this.cbStopBits.Name = "cbStopBits";
			this.cbStopBits.Size = new System.Drawing.Size(121, 21);
			this.cbStopBits.TabIndex = 12;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(13, 228);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(100, 23);
			this.label6.TabIndex = 13;
			this.label6.Text = "Flow control:";
			// 
			// cbHandshake
			// 
			this.cbHandshake.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbHandshake.Enabled = false;
			this.cbHandshake.FormattingEnabled = true;
			this.cbHandshake.Items.AddRange(new object[] {
									"None",
									"XON/XOFF",
									"Hardware"});
			this.cbHandshake.Location = new System.Drawing.Point(105, 225);
			this.cbHandshake.Name = "cbHandshake";
			this.cbHandshake.Size = new System.Drawing.Size(121, 21);
			this.cbHandshake.TabIndex = 14;
			// 
			// btnOK
			// 
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOK.Location = new System.Drawing.Point(79, 298);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 15;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCancel.Location = new System.Drawing.Point(160, 298);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 16;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
			// 
			// CommSelectAndSettingsForm
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(316, 333);
			this.ControlBox = false;
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.cbHandshake);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.cbStopBits);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.cbParity);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cbDataBits);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cbBitRate);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.rbCustom);
			this.Controls.Add(this.rbDefault);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.cbCommPort);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CommSelectAndSettingsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select and Configure the Comm. Port";
			this.Shown += new System.EventHandler(this.CommSelectAndSettingsFormShown);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.ComboBox cbHandshake;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cbStopBits;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cbParity;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox cbDataBits;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cbBitRate;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton rbCustom;
		private System.Windows.Forms.RadioButton rbDefault;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.ComboBox cbCommPort;
		private System.Windows.Forms.Label label1;
	}
}
