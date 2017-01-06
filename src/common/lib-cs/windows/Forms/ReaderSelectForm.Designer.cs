/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/03/2012
 * Heure: 12:29
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace SpringCardApplication
{
  partial class ReaderSelectForm
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
    	this.components = new System.ComponentModel.Container();
    	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReaderSelectForm));
    	this.imageList = new System.Windows.Forms.ImageList(this.components);
    	this.panel1 = new System.Windows.Forms.Panel();
    	this.label3 = new System.Windows.Forms.Label();
    	this.panel2 = new System.Windows.Forms.Panel();
    	this.imgRefresh = new System.Windows.Forms.PictureBox();
    	this.btnRefresh = new System.Windows.Forms.LinkLabel();
    	this.cbRemember = new System.Windows.Forms.CheckBox();
    	this.btnCancel = new System.Windows.Forms.Button();
    	this.btnOK = new System.Windows.Forms.Button();
    	this.pMain = new System.Windows.Forms.Panel();
    	this.lvReaders = new System.Windows.Forms.ListView();
    	this.colReaderName = new System.Windows.Forms.ColumnHeader();
    	this.panel1.SuspendLayout();
    	this.panel2.SuspendLayout();
    	((System.ComponentModel.ISupportInitialize)(this.imgRefresh)).BeginInit();
    	this.pMain.SuspendLayout();
    	this.SuspendLayout();
    	// 
    	// imageList
    	// 
    	this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
    	this.imageList.TransparentColor = System.Drawing.Color.Transparent;
    	this.imageList.Images.SetKeyName(0, "right_circular.png");
    	// 
    	// panel1
    	// 
    	this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
    	this.panel1.Controls.Add(this.label3);
    	this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
    	this.panel1.ForeColor = System.Drawing.Color.White;
    	this.panel1.Location = new System.Drawing.Point(0, 0);
    	this.panel1.Margin = new System.Windows.Forms.Padding(4);
    	this.panel1.Name = "panel1";
    	this.panel1.Size = new System.Drawing.Size(784, 64);
    	this.panel1.TabIndex = 1;
    	// 
    	// label3
    	// 
    	this.label3.AutoSize = true;
    	this.label3.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label3.Location = new System.Drawing.Point(13, 20);
    	this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
    	this.label3.Name = "label3";
    	this.label3.Size = new System.Drawing.Size(151, 23);
    	this.label3.TabIndex = 2;
    	this.label3.Text = "Select the Reader :";
    	// 
    	// panel2
    	// 
    	this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
    	this.panel2.Controls.Add(this.imgRefresh);
    	this.panel2.Controls.Add(this.btnRefresh);
    	this.panel2.Controls.Add(this.cbRemember);
    	this.panel2.Controls.Add(this.btnCancel);
    	this.panel2.Controls.Add(this.btnOK);
    	this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
    	this.panel2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.panel2.ForeColor = System.Drawing.Color.Black;
    	this.panel2.Location = new System.Drawing.Point(0, 369);
    	this.panel2.Margin = new System.Windows.Forms.Padding(4);
    	this.panel2.Name = "panel2";
    	this.panel2.Size = new System.Drawing.Size(784, 73);
    	this.panel2.TabIndex = 2;
    	// 
    	// imgRefresh
    	// 
    	this.imgRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
    	this.imgRefresh.Image = ((System.Drawing.Image)(resources.GetObject("imgRefresh.Image")));
    	this.imgRefresh.Location = new System.Drawing.Point(12, 15);
    	this.imgRefresh.Name = "imgRefresh";
    	this.imgRefresh.Size = new System.Drawing.Size(16, 16);
    	this.imgRefresh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
    	this.imgRefresh.TabIndex = 4;
    	this.imgRefresh.TabStop = false;
    	this.imgRefresh.Click += new System.EventHandler(this.ImgRefreshClick);
    	// 
    	// btnRefresh
    	// 
    	this.btnRefresh.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(6)))), ((int)(((byte)(18)))));
    	this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(6)))), ((int)(((byte)(18)))));
    	this.btnRefresh.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(6)))), ((int)(((byte)(18)))));
    	this.btnRefresh.Location = new System.Drawing.Point(29, 12);
    	this.btnRefresh.Name = "btnRefresh";
    	this.btnRefresh.Size = new System.Drawing.Size(408, 23);
    	this.btnRefresh.TabIndex = 3;
    	this.btnRefresh.TabStop = true;
    	this.btnRefresh.Text = "Refresh the list";
    	this.btnRefresh.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(6)))), ((int)(((byte)(18)))));
    	this.btnRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BtnRefreshLinkClicked);
    	// 
    	// cbRemember
    	// 
    	this.cbRemember.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.cbRemember.ForeColor = System.Drawing.Color.Black;
    	this.cbRemember.Location = new System.Drawing.Point(15, 29);
    	this.cbRemember.Margin = new System.Windows.Forms.Padding(4);
    	this.cbRemember.Name = "cbRemember";
    	this.cbRemember.Size = new System.Drawing.Size(439, 35);
    	this.cbRemember.TabIndex = 2;
    	this.cbRemember.Text = "Remember the settings and re-open the same reader next time";
    	this.cbRemember.UseVisualStyleBackColor = true;
    	// 
    	// btnCancel
    	// 
    	this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
    	this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
    	this.btnCancel.ForeColor = System.Drawing.Color.Black;
    	this.btnCancel.Location = new System.Drawing.Point(673, 15);
    	this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
    	this.btnCancel.Name = "btnCancel";
    	this.btnCancel.Size = new System.Drawing.Size(100, 49);
    	this.btnCancel.TabIndex = 1;
    	this.btnCancel.Text = "Cancel";
    	this.btnCancel.UseVisualStyleBackColor = false;
    	this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
    	// 
    	// btnOK
    	// 
    	this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
    	this.btnOK.ForeColor = System.Drawing.Color.Black;
    	this.btnOK.Location = new System.Drawing.Point(565, 15);
    	this.btnOK.Margin = new System.Windows.Forms.Padding(4);
    	this.btnOK.Name = "btnOK";
    	this.btnOK.Size = new System.Drawing.Size(100, 49);
    	this.btnOK.TabIndex = 0;
    	this.btnOK.Text = "OK";
    	this.btnOK.UseVisualStyleBackColor = false;
    	this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
    	// 
    	// pMain
    	// 
    	this.pMain.Controls.Add(this.lvReaders);
    	this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.pMain.Location = new System.Drawing.Point(0, 64);
    	this.pMain.Name = "pMain";
    	this.pMain.Padding = new System.Windows.Forms.Padding(9);
    	this.pMain.Size = new System.Drawing.Size(784, 305);
    	this.pMain.TabIndex = 3;
    	// 
    	// lvReaders
    	// 
    	this.lvReaders.BackColor = System.Drawing.Color.White;
    	this.lvReaders.BorderStyle = System.Windows.Forms.BorderStyle.None;
    	this.lvReaders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.colReaderName});
    	this.lvReaders.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.lvReaders.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.lvReaders.ForeColor = System.Drawing.Color.Black;
    	this.lvReaders.FullRowSelect = true;
    	this.lvReaders.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
    	this.lvReaders.HideSelection = false;
    	this.lvReaders.Location = new System.Drawing.Point(9, 9);
    	this.lvReaders.MultiSelect = false;
    	this.lvReaders.Name = "lvReaders";
    	this.lvReaders.Size = new System.Drawing.Size(766, 287);
    	this.lvReaders.TabIndex = 2;
    	this.lvReaders.UseCompatibleStateImageBehavior = false;
    	this.lvReaders.View = System.Windows.Forms.View.Details;
    	this.lvReaders.SelectedIndexChanged += new System.EventHandler(this.LvReadersSelectedIndexChanged);
    	this.lvReaders.DoubleClick += new System.EventHandler(this.LvReadersDoubleClick);
    	// 
    	// colReaderName
    	// 
    	this.colReaderName.Text = "Reader name";
    	this.colReaderName.Width = 564;
    	// 
    	// ReaderSelectForm
    	// 
    	this.AcceptButton = this.btnOK;
    	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.CancelButton = this.btnCancel;
    	this.ClientSize = new System.Drawing.Size(784, 442);
    	this.Controls.Add(this.pMain);
    	this.Controls.Add(this.panel2);
    	this.Controls.Add(this.panel1);
    	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
    	this.MaximizeBox = false;
    	this.MinimizeBox = false;
    	this.Name = "ReaderSelectForm";
    	this.ShowIcon = false;
    	this.ShowInTaskbar = false;
    	this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
    	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
    	this.Text = "Reader selection";
    	this.Shown += new System.EventHandler(this.ReaderSelectFormShown);
    	this.panel1.ResumeLayout(false);
    	this.panel1.PerformLayout();
    	this.panel2.ResumeLayout(false);
    	((System.ComponentModel.ISupportInitialize)(this.imgRefresh)).EndInit();
    	this.pMain.ResumeLayout(false);
    	this.ResumeLayout(false);

    }
    private System.Windows.Forms.ImageList imageList;
    private System.Windows.Forms.ColumnHeader colReaderName;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.ListView lvReaders;
    private System.Windows.Forms.Panel pMain;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.CheckBox cbRemember;
    private System.Windows.Forms.LinkLabel btnRefresh;
    private System.Windows.Forms.PictureBox imgRefresh;
  }
}
