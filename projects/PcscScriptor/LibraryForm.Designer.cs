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
            this.lvApdus = new System.Windows.Forms.ListView();
            this.columnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.eApdu = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(523, 412);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(100, 37);
            this.BtnCancel.TabIndex = 2;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOk
            // 
            this.BtnOk.Location = new System.Drawing.Point(415, 412);
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Size = new System.Drawing.Size(100, 37);
            this.BtnOk.TabIndex = 1;
            this.BtnOk.Text = "Ok";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select an APDU to import:";
            // 
            // lvApdus
            // 
            this.lvApdus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader});
            this.lvApdus.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvApdus.HideSelection = false;
            this.lvApdus.Location = new System.Drawing.Point(6, 28);
            this.lvApdus.Name = "lvApdus";
            this.lvApdus.Size = new System.Drawing.Size(572, 364);
            this.lvApdus.TabIndex = 3;
            this.lvApdus.UseCompatibleStateImageBehavior = false;
            this.lvApdus.View = System.Windows.Forms.View.Details;
            this.lvApdus.SelectedIndexChanged += new System.EventHandler(this.lvApdus_SelectedIndexChanged);
            // 
            // columnHeader
            // 
            this.columnHeader.Width = 260;
            // 
            // eApdu
            // 
            this.eApdu.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eApdu.Location = new System.Drawing.Point(6, 477);
            this.eApdu.Multiline = true;
            this.eApdu.Name = "eApdu";
            this.eApdu.ReadOnly = true;
            this.eApdu.Size = new System.Drawing.Size(572, 147);
            this.eApdu.TabIndex = 4;
            // 
            // LibraryForm
            // 
            this.AcceptButton = this.BtnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.ClientSize = new System.Drawing.Size(1441, 792);
            this.ControlBox = false;
            this.Controls.Add(this.eApdu);
            this.Controls.Add(this.lvApdus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.BtnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
        private System.Windows.Forms.ListView lvApdus;
        private System.Windows.Forms.ColumnHeader columnHeader;
        private System.Windows.Forms.TextBox eApdu;
    }
}