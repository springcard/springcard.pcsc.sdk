/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 20/03/2012
 * Heure: 12:15
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace PcscDiag2
{
  partial class ReaderInfoForm
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Vendor name");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Product name");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Serial number");
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("USB ID");
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Version");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("NFC/RFID chipset");
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Smartcard chipset");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Slot name");
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lvReaderInfo = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lbDevice = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbDevice);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 135);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(432, 38);
            this.panel1.TabIndex = 13;
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(335, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 23);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lvReaderInfo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(6);
            this.panel2.Size = new System.Drawing.Size(432, 135);
            this.panel2.TabIndex = 14;
            // 
            // lvReaderInfo
            // 
            this.lvReaderInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lvReaderInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvReaderInfo.FullRowSelect = true;
            this.lvReaderInfo.GridLines = true;
            this.lvReaderInfo.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvReaderInfo.HideSelection = false;
            this.lvReaderInfo.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8});
            this.lvReaderInfo.Location = new System.Drawing.Point(6, 6);
            this.lvReaderInfo.MultiSelect = false;
            this.lvReaderInfo.Name = "lvReaderInfo";
            this.lvReaderInfo.Scrollable = false;
            this.lvReaderInfo.ShowGroups = false;
            this.lvReaderInfo.Size = new System.Drawing.Size(420, 123);
            this.lvReaderInfo.TabIndex = 0;
            this.lvReaderInfo.UseCompatibleStateImageBehavior = false;
            this.lvReaderInfo.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 300;
            // 
            // lbDevice
            // 
            this.lbDevice.AutoSize = true;
            this.lbDevice.Location = new System.Drawing.Point(12, 11);
            this.lbDevice.Name = "lbDevice";
            this.lbDevice.Size = new System.Drawing.Size(0, 13);
            this.lbDevice.TabIndex = 11;
            // 
            // ReaderInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(432, 173);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReaderInfoForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ReaderInfoForm";
            this.Shown += new System.EventHandler(this.ReaderInfoFormShown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

    }
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ListView lvReaderInfo;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbDevice;
    }
}
