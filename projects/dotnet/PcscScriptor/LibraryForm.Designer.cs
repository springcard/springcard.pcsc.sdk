namespace scscriptorxv
{
	partial class LibraryForm
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
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnOk = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.listBoxApdus = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// BtnCancel
			// 
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Location = new System.Drawing.Point(448, 357);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(86, 32);
			this.BtnCancel.TabIndex = 2;
			this.BtnCancel.Text = "Cancel";
			this.BtnCancel.UseVisualStyleBackColor = true;
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnOk
			// 
			this.BtnOk.Location = new System.Drawing.Point(356, 357);
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.Size = new System.Drawing.Size(86, 32);
			this.BtnOk.TabIndex = 1;
			this.BtnOk.Text = "Ok";
			this.BtnOk.UseVisualStyleBackColor = true;
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(131, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Select an APDU to import:";
			// 
			// listBoxApdus
			// 
			this.listBoxApdus.FormattingEnabled = true;
			this.listBoxApdus.Location = new System.Drawing.Point(13, 26);
			this.listBoxApdus.Name = "listBoxApdus";
			this.listBoxApdus.Size = new System.Drawing.Size(520, 316);
			this.listBoxApdus.TabIndex = 0;
			this.listBoxApdus.SelectedIndexChanged += new System.EventHandler(this.listBoxApdus_SelectedIndexChanged);
			this.listBoxApdus.DoubleClick += new System.EventHandler(this.BtnOk_Click);
			// 
			// LibraryForm
			// 
			this.AcceptButton = this.BtnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.BtnCancel;
			this.ClientSize = new System.Drawing.Size(545, 402);
			this.ControlBox = false;
			this.Controls.Add(this.listBoxApdus);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.BtnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LibraryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Library";
			this.Load += new System.EventHandler(this.LibraryForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox listBoxApdus;
	}
}